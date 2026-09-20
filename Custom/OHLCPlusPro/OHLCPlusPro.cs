#nullable enable annotations
namespace ATAS.Indicators.Technical.OhlcPlusPro;

using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

using Res = ATAS.Indicators.Technical.OHLCPlusProResources;

public enum LabelPosition
{
    [Display(ResourceType = typeof(Res), Name = nameof(Res.None))]
    None = 0,
    
    [Display(ResourceType = typeof(Res), Name = nameof(Res.Bar))]
    Bar = 1,
    
    [Display(ResourceType = typeof(Res), Name = nameof(Res.Right))]
    Right = 2,
    
    [Display(ResourceType = typeof(Res), Name = nameof(Res.Left))]
    Left = 3
}

public enum LineType
{
    [Display(ResourceType = typeof(Res), Name = nameof(Res.None))]
    None = 0,
    
    [Display(ResourceType = typeof(Res), Name = nameof(Res.TillBar))]
    Bar = 1,
    
    [Display(ResourceType = typeof(Res), Name = nameof(Res.FullWidth))]
    Full = 2
}

public abstract class NotifiableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

[Editor(typeof(Editors.LevelSettingsEditor), typeof(Editors.LevelSettingsEditor))]
public class LevelSettings : NotifiableObject
{
    #region Fields

    private bool _enabled;
    private CrossColor _color;
    private CrossColor? _textColor;
    private bool _showPrice;
    private LineType _lineType;
    private int _width;
    private LineDashStyle _lineStyle;
    private LabelPosition _labelPosition;
    private RenderPen? _renderPen;

    #endregion
      
    #region Properties

    [Display(ResourceType = typeof(Res), Name = nameof(Res.Enabled))]
    public bool Enabled 
    {
        get => _enabled;
        set => SetField(ref _enabled, value);
    }

    [Display(ResourceType = typeof(Res), Name = nameof(Res.Color))]
    public CrossColor Color
    {
        get => _color;
        set => SetField(ref _color, value);
    }

    /// <summary>
    /// Color of the level text: the label on the chart, the price on the price scale and the
    /// level badge in the heatmap. <see langword="null"/> means "Auto": the color is picked for
    /// contrast with the background behind the text. Templates and workspaces saved before the
    /// setting existed carry no value, so they read as Auto.
    /// </summary>
    [Display(ResourceType = typeof(Res), Name = nameof(Res.TextColor), Description = nameof(Res.OhlcPlusTextColorDescription))]
    public CrossColor? TextColor
    {
        get => _textColor;
        set => SetField(ref _textColor, value);
    }

    [Display(ResourceType = typeof(Res), Name = nameof(Res.ShowPrice))]
    public bool ShowPrice 
    {
        get => _showPrice;
        set => SetField(ref _showPrice, value);
    }

    [Display(ResourceType = typeof(Res), Name = nameof(Res.Line))]
    public LineType LineType 
    {
        get => _lineType;
        set => SetField(ref _lineType, value);
    }

    [Display(ResourceType = typeof(Res), Name = nameof(Res.Width))]
    [Range(1, 10)]
    public int Width 
    { 
        get => _width;
        set => SetField(ref _width, value);
    }

    [Display(ResourceType = typeof(Res), Name = nameof(Res.LineStyle))]
    public LineDashStyle LineStyle 
    { 
        get => _lineStyle;
        set => SetField(ref _lineStyle, value);
    }

    [Display(ResourceType = typeof(Res), Name = nameof(Res.Label))]
    public LabelPosition LabelPosition 
    {
        get => _labelPosition;
        set => SetField(ref _labelPosition, value);
    }

    /// <summary>
    /// Pen of the level, built once and kept until one of the settings behind it changes. The
    /// chart asks for it on every frame and for every level.
    /// </summary>
    [Browsable(false)]
    public RenderPen RenderPen => _renderPen ??= new PenSettings { Color = Color, Width = Width, LineDashStyle = LineStyle }.RenderObject;

    #endregion

    #region Methods

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        // The color, the width and the style are baked into the pen: drop it and the next frame
        // builds it again.
        _renderPen = null;
        base.OnPropertyChanged(propertyName);
    }

    #endregion

    #region ctor

    public LevelSettings
    (
        bool enabled = false,
        CrossColor color = default,
        int width = 1,
        LineDashStyle lineStyle = LineDashStyle.Solid,
        bool showPrice = true,
        LabelPosition labelPosition = LabelPosition.Bar,
        LineType lineType = LineType.Bar,
        CrossColor? textColor = null
    )
    {
        Enabled = enabled;
        Color = color == default ? System.Drawing.Color.Blue.Convert() : color;
        TextColor = textColor;
        Width = width;
        LineStyle = lineStyle;
        ShowPrice = showPrice;
        LabelPosition = labelPosition;
        LineType = lineType;
    }

    #endregion
}

[DisplayName("OHLC Plus Pro")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Res), Description = nameof(Res.OHLCPlusDescription))]
public class OHLCPlusPro : Indicator
{
    #region Nested types

    /// <summary>
    /// One level, immutable on purpose: the profile responses arrive on a thread of the pool
    /// while the chart is rendering, and a level is replaced as a whole rather than written into.
    /// </summary>
    private sealed class LevelData
    {
        public LevelData(decimal price, bool isValid)
        {
            Price = price;
            IsValid = isValid;
        }

        public decimal Price { get; }

        public bool IsValid { get; }
    }

    private sealed class RefEqComparer : IEqualityComparer<object>
    {
        public static readonly RefEqComparer Instance = new();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    #endregion

    #region Fields

    private readonly HashSet<LevelSettings> _subscribedLevels = new(RefEqComparer.Instance);
    private readonly Dictionary<LevelSettings, FixedProfilePeriods> _periodByLevel = new(RefEqComparer.Instance);
    private readonly Dictionary<FixedProfilePeriods, string[]> _keys = new()
    {
        [FixedProfilePeriods.CurrentDay] = ["dOpen", "dHigh", "dLow", "dClose", "dEQ", "dPOC", "dVWAP", "dVAH", "dVAL"],
        [FixedProfilePeriods.LastDay] = ["pOpen", "pHigh", "pLow", "pClose", "pEQ", "pPOC", "pVWAP", "pVAH", "pVAL"],
        [FixedProfilePeriods.CurrentWeek] = ["wOpen", "wHigh", "wLow", "wClose", "wEQ", "wPOC", "wVWAP", "wVAH", "wVAL"],
        [FixedProfilePeriods.LastWeek] = ["pwOpen", "pwHigh", "pwLow", "pwClose", "pwEQ", "pwPOC", "pwVWAP", "pwVAH", "pwVAL"],
        [FixedProfilePeriods.CurrentMonth] = ["mOpen", "mHigh", "mLow", "mClose", "mEQ", "mPOC", "mVWAP", "mVAH", "mVAL"],
        [FixedProfilePeriods.LastMonth] = ["pmOpen", "pmHigh", "pmLow", "pmClose", "pmEQ", "pmPOC", "pmVWAP", "pmVAH", "pmVAL"],
        [FixedProfilePeriods.Contract] = ["cOpen", "cHigh", "cLow", "cClose", "cEQ", "cPOC", "cVWAP", "cVAH", "cVAL"],
    };


    // Written from profile-response callbacks (thread pool) and read/cleared from the
    // calculation thread — same concurrency story as _levels below.
    private readonly ConcurrentDictionary<FixedProfilePeriods, IndicatorCandle> _profileCandles = [];
    private readonly ConcurrentDictionary<FixedProfilePeriods, IndicatorCandle> _originProfileCandles = [];
    private readonly ConcurrentDictionary<string, LevelData> _levels = [];
    private readonly RenderFont _font = new("Arial", 10);
    private readonly RenderFont _axisFont = new("Arial", 11);
    private readonly RenderStringFormat _stringRightFormat = new()
    {
        Alignment = StringAlignment.Far,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoWrap
    };
    
    private readonly RenderStringFormat _stringLeftFormat = new()
    {
        Alignment = StringAlignment.Near,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoWrap
    };

    /// <summary>Periods the indicator draws, in the order of the settings.</summary>
    private static readonly FixedProfilePeriods[] AllPeriods =
    [
        FixedProfilePeriods.CurrentDay,
        FixedProfilePeriods.LastDay,
        FixedProfilePeriods.CurrentWeek,
        FixedProfilePeriods.LastWeek,
        FixedProfilePeriods.CurrentMonth,
        FixedProfilePeriods.LastMonth,
        FixedProfilePeriods.Contract,
    ];

    // Per period, indexed like AllPeriods. Both are written from the calculation thread and read
    // from the property handlers of the levels, so each entry is replaced as a whole and never
    // edited in place.
    private readonly LevelSettings[][] _levelsByPeriod = new LevelSettings[AllPeriods.Length][];
    private readonly bool[] _needed = new bool[AllPeriods.Length];

    /// <summary>Order of the nine levels of every period, and of the texts that name them.</summary>
    private const int LevelCount = 9;

    private readonly string[] _levelTexts = ["Open", "High", "Low", "Close", "EQ", "POC", "VWAP", "VAH", "VAL"];
    private readonly string[] _prefixes = ["D", "PD", "W", "PW", "M", "PM", "C"];

    private string _labelTemplate = "{prefix} {level}";

    private int _lastBar = -1;
    private bool _candleRequested;
    private bool _useAbsolutePrices;
    private bool _autoUpdateLevels = true;

    private bool _allLevelsVisible = true;

    #endregion

    #region Properties

    #region Day Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings DayOpenLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings DayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings DayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings DayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings DayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings DayPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.CurrentDay), Order = 65)]
    public LevelSettings DayVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings DayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentDay), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings DayVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Prev.Day Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings PrevDayOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings PrevDayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings PrevDayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings PrevDayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings PrevDayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings PrevDayPOCLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.PreviousDay), Order = 65)]
    public LevelSettings PrevDayVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings PrevDayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousDay), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings PrevDayVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Week Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings WeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings WeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings WeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings WeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings WeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings WeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.CurrentWeek), Order = 65)]
    public LevelSettings WeekVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings WeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentWeek), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings WeekVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Prev.Week Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings PrevWeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings PrevWeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings PrevWeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings PrevWeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings PrevWeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings PrevWeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.PreviousWeek), Order = 65)]
    public LevelSettings PrevWeekVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings PrevWeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousWeek), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings PrevWeekVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Month Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings MonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings MonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings MonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings MonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings MonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings MonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.CurrentMonth), Order = 65)]
    public LevelSettings MonthVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings MonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.CurrentMonth), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings MonthVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Prev.Month Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings PrevMonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings PrevMonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings PrevMonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings PrevMonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings PrevMonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings PrevMonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.PreviousMonth), Order = 65)]
    public LevelSettings PrevMonthVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings PrevMonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PreviousMonth), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings PrevMonthVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Contract Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.BarOpen), Order = 10)]
    public LevelSettings ContractOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.BarHigh), Order = 20)]
    public LevelSettings ContractHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.BarLow), Order = 30)]
    public LevelSettings ContractLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.BarClose), Order = 40)]
    public LevelSettings ContractCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.Equilibrium), Order = 50)]
    public LevelSettings ContractEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.POC), Order = 60)]
    public LevelSettings ContractPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), Name = nameof(Res.VWAP), GroupName = nameof(Res.Contract), Order = 65)]
    public LevelSettings ContractVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.VAH), Order = 70)]
    public LevelSettings ContractVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Contract), Name = nameof(Res.VAL), Order = 80)]
    public LevelSettings ContractVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Calculation Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Calculation), Name = nameof(Res.AbsolutePrice), Description = nameof(Res.OhlcPlusAbsolutePriceDescription), Order = 900)]
    public bool UseAbsolutePrices
    {
        get => _useAbsolutePrices;
        set
        {
            _useAbsolutePrices = value;

            // The levels of the other mode would survive in the periods that have no profile yet.
            _levels.Clear();
            UpdateAllNeededLevelsFromCache();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Calculation), Name = nameof(Res.AutoUpdate), Description = nameof(Res.OhlcPlusAutoUpdateDescription), Order = 910)]
    public bool AutoUpdateLevels
    {
        get => _autoUpdateLevels;
        set => _autoUpdateLevels = value;
    }

    #endregion

    #region Labels

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.LabelTemplate), Description = nameof(Res.LabelTemplateDescription), Order = 1100)]
    public string LabelTemplate
    {
        get => _labelTemplate;
        set => SetText(ref _labelTemplate, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.BarOpen), Order = 1110)]
    public string OpenText
    {
        get => _levelTexts[0];
        set => SetLevelText(0, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.BarHigh), Order = 1120)]
    public string HighText
    {
        get => _levelTexts[1];
        set => SetLevelText(1, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.BarLow), Order = 1130)]
    public string LowText
    {
        get => _levelTexts[2];
        set => SetLevelText(2, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.BarClose), Order = 1140)]
    public string CloseText
    {
        get => _levelTexts[3];
        set => SetLevelText(3, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.Equilibrium), Order = 1150)]
    public string EquilibriumText
    {
        get => _levelTexts[4];
        set => SetLevelText(4, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.POC), Order = 1160)]
    public string PocText
    {
        get => _levelTexts[5];
        set => SetLevelText(5, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.VWAP), Order = 1170)]
    public string VwapText
    {
        get => _levelTexts[6];
        set => SetLevelText(6, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.VAH), Order = 1180)]
    public string VahText
    {
        get => _levelTexts[7];
        set => SetLevelText(7, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.VAL), Order = 1190)]
    public string ValText
    {
        get => _levelTexts[8];
        set => SetLevelText(8, value);
    }

    #endregion

    #region Prefixes

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.CurrentDay), Order = 1200)]
    public string DayPrefix
    {
        get => _prefixes[0];
        set => SetPrefix(0, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.PreviousDay), Order = 1210)]
    public string PrevDayPrefix
    {
        get => _prefixes[1];
        set => SetPrefix(1, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.CurrentWeek), Order = 1220)]
    public string WeekPrefix
    {
        get => _prefixes[2];
        set => SetPrefix(2, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.PreviousWeek), Order = 1230)]
    public string PrevWeekPrefix
    {
        get => _prefixes[3];
        set => SetPrefix(3, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.CurrentMonth), Order = 1240)]
    public string MonthPrefix
    {
        get => _prefixes[4];
        set => SetPrefix(4, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.PreviousMonth), Order = 1250)]
    public string PrevMonthPrefix
    {
        get => _prefixes[5];
        set => SetPrefix(5, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Prefixes), Name = nameof(Res.Contract), Order = 1260)]
    public string ContractPrefix
    {
        get => _prefixes[6];
        set => SetPrefix(6, value);
    }

    #endregion

    #region Visibility Settings

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Visibility), Name = nameof(Res.ToggleLevelsVisibilityHotKey), Order = 1000)]
    public CrossKey[] ToggleVisibilityHotKey { get; set; } = { CrossKey.Q };

    #endregion

    #endregion

    #region Constructor

    public OHLCPlusPro()
        : base(true)
    {
        DataSeries[0].IsHidden = true;
        ((ValueDataSeries)DataSeries[0]).ShowZeroValue = false;
        DenyToChangePanel = true;
        EnableCustomDrawing = true;
        SubscribeToDrawingEvents(DrawingLayouts.Final);
        DrawAbovePrice = true;
    }

    #endregion

    #region Protected methods

    protected override void OnApplyDefaultColors()
    {
        if (ChartInfo is null)
            return;

        base.OnApplyDefaultColors();
    }

    protected override void OnInitialize()
    {
        SubscribeAllLevels();
        RecalcAllNeeds();
    }

    protected override void OnDispose()
    {
        foreach (var ls in _subscribedLevels)
            ls.PropertyChanged -= OnLevelSettingsChanged;

        _subscribedLevels.Clear();
        _periodByLevel.Clear();
        base.OnDispose();
    }

    public override bool ProcessKeyDown(CrossKeyEventArgs e)
    {
        if (ToggleVisibilityHotKey != null && ToggleVisibilityHotKey.Contains(e.Key))
        {
            ToggleAllLevelsVisibility();
            return true;
        }

        return base.ProcessKeyDown(e);
    }

    protected override void OnCalculate(int bar, decimal value)
    {
        if (bar == 0)
        {
            _profileCandles.Clear();
            _originProfileCandles.Clear();
            _levels.Clear();

            // A template restored after OnInitialize replaces the level objects: the new ones are
            // not subscribed and their period is never requested. Both calls are idempotent.
            SubscribeAllLevels();
            RecalcAllNeeds();
        }

        if (bar == 0 || IsNewSession(bar) && _lastBar != bar)
            _candleRequested = false;

        if (bar != CurrentBar - 1)
            return;

        if (!_candleRequested)
        {
            _candleRequested = true;
            RequestProfiles();
            _lastBar = bar;
        }

        // Both profile candles read through to the live tick-updated profile,
        // so re-reading them here is enough to keep the levels current.
        if (_autoUpdateLevels)
            UpdateAllNeededLevelsFromCache();
    }

#pragma warning disable CS0672
    protected override void OnFixedProfilesResponse(IndicatorCandle fixedProfileScaled, IndicatorCandle fixedProfileOriginScale, FixedProfilePeriods period)
#pragma warning restore CS0672
    {
        // The scaled profile is binned to the current chart scale, so all levels (OHLC, POC,
        // VWAP and the value area) line up with what the Profile indicator draws. The origin
        // scale profile keeps exact tick prices and reads through to the live profile.
        _profileCandles[period] = fixedProfileScaled;
        _originProfileCandles[period] = fixedProfileOriginScale;

        if (UpdateLevels(period, _useAbsolutePrices ? fixedProfileOriginScale : fixedProfileScaled))
            RedrawChart();
    }

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo is null || InstrumentInfo is null)
            return;

        foreach (var period in AllPeriods)
        {
            var levels = LevelsOf(period);

            if (levels.Length != LevelCount)
                continue;

            for (var kind = 0; kind < LevelCount; kind++)
                RenderLevel(context, period, kind, levels[kind]);
        }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Text of a level: the template with the prefix of its period and the name of its kind.
    /// </summary>
    private string BuildLabel(FixedProfilePeriods period, int kind)
    {
        var index = IndexOf(period);
        var prefix = index >= 0 ? _prefixes[index] : string.Empty;
        var text = kind >= 0 && kind < LevelCount ? _levelTexts[kind] : string.Empty;

        if (_labelTemplate.Length == 0)
            return text;

        return _labelTemplate
            .Replace("{prefix}", prefix, StringComparison.Ordinal)
            .Replace("{level}", text, StringComparison.Ordinal)
            .Trim();
    }

    private void SetText(ref string field, string value)
    {
        field = value ?? string.Empty;
        RedrawChart();
    }

    private void SetLevelText(int kind, string value)
    {
        _levelTexts[kind] = value ?? string.Empty;
        RedrawChart();
    }

    private void SetPrefix(int index, string value)
    {
        _prefixes[index] = value ?? string.Empty;
        RedrawChart();
    }

    private void ToggleAllLevelsVisibility()
    {
        _allLevelsVisible = !_allLevelsVisible;
        RedrawChart();
    }

    #region OnCalculate

    private void UpdateAllNeededLevelsFromCache()
    {
        var candles = _useAbsolutePrices ? _originProfileCandles : _profileCandles;
        var changed = false;

        void UpdateIf(FixedProfilePeriods p)
        {
            if (IsNeeded(p) && candles.TryGetValue(p, out var candle) && candle is not null)
                changed |= UpdateLevels(p, candle);
        }

        UpdateIf(FixedProfilePeriods.CurrentDay);
        UpdateIf(FixedProfilePeriods.LastDay);
        UpdateIf(FixedProfilePeriods.CurrentWeek);
        UpdateIf(FixedProfilePeriods.LastWeek);
        UpdateIf(FixedProfilePeriods.CurrentMonth);
        UpdateIf(FixedProfilePeriods.LastMonth);
        UpdateIf(FixedProfilePeriods.Contract);

        // Every tick lands here through OnCalculate: redrawing the chart when no level moved is
        // the most expensive thing this indicator could do for nothing.
        if (changed)
            RedrawChart();
    }

    private void RequestProfiles()
    {
        foreach (var period in AllPeriods)
        {
            if (IsNeeded(period))
                RequestProfileForPeriod(period);
        }
    }

    private void RequestProfileForPeriod(FixedProfilePeriods period, bool force = true)
    {
        if (!force && _profileCandles.TryGetValue(period, out var candle) && candle is not null)
        {
            RedrawChart();
            return;
        }

#pragma warning disable CS0618
        GetFixedProfile(new FixedProfileRequest(period));
#pragma warning restore CS0618
    }

    private void RecalcAllNeeds()
    {
        foreach (var period in AllPeriods)
            RecalcNeedFor(period);
    }

    private void RecalcNeedFor(FixedProfilePeriods period)
    {
        var index = IndexOf(period);

        if (index >= 0)
            _needed[index] = NeedsData(period);
    }

    /// <summary>A period is needed while any of its nine levels is enabled.</summary>
    private bool NeedsData(FixedProfilePeriods period)
    {
        foreach (var level in LevelsOf(period))
        {
            if (level.Enabled)
                return true;
        }

        return false;
    }

    private LevelSettings[] LevelsOf(FixedProfilePeriods period)
    {
        var index = IndexOf(period);

        return index < 0 ? [] : _levelsByPeriod[index] ?? [];
    }

    private static int IndexOf(FixedProfilePeriods period) => Array.IndexOf(AllPeriods, period);

    private bool UpdateLevels(FixedProfilePeriods period, IndicatorCandle candle)
    {
        if (candle == null)
            return false;

        var keys = _keys[period];
        var changed = false;

        // OHLC + EQ
        changed |= UpdateLevel(keys[0], candle.Open);                          // Open
        changed |= UpdateLevel(keys[1], candle.High);                          // High
        changed |= UpdateLevel(keys[2], candle.Low);                           // Low
        changed |= UpdateLevel(keys[3], candle.Close);                         // Close
        changed |= UpdateLevel(keys[4], (candle.High + candle.Low) / 2);       // EQ

        // POC
        if (candle.MaxVolumePriceInfo != null && candle.MaxVolumePriceInfo.Price > 0)
            changed |= UpdateLevel(keys[5], candle.MaxVolumePriceInfo.Price);

        // VWAP
        var vwap = candle.GetVwapCompat();

        if (vwap > 0)
            changed |= UpdateLevel(keys[6], vwap);

        // VAH/VAL
        if (candle.ValueArea != null &&
            candle.ValueArea.ValueAreaHigh > 0 &&
            candle.ValueArea.ValueAreaLow > 0 &&
            candle.ValueArea.ValueAreaHigh >= candle.ValueArea.ValueAreaLow)
        {
            changed |= UpdateLevel(keys[7], candle.ValueArea.ValueAreaHigh);
            changed |= UpdateLevel(keys[8], candle.ValueArea.ValueAreaLow);
        }

        return changed;
    }

    private bool UpdateLevel(string key, decimal price)
    {
        if (_levels.TryGetValue(key, out var previous) && previous.IsValid && previous.Price == price)
            return false;

        _levels[key] = new LevelData(price, true);
        return true;
    }

    #endregion

    #region OnRender

    private void RenderLevel(RenderContext context, FixedProfilePeriods period, int kind, LevelSettings levelSettings)
    {
        if (!_allLevelsVisible || !levelSettings.Enabled || !_levels.TryGetValue(_keys[period][kind], out var level) || !level.IsValid)
            return;
            
        // Validate price is reasonable
        if (level.Price <= 0)
            return;

        var y = ChartInfo.GetYByPrice(level.Price, false);
        
        // Check if price is visible on chart
        if (y < 0 || y > ChartInfo.PriceChartContainer.Region.Height)
            return;
            
        var chartWidth = ChartInfo.PriceChartContainer.Region.Width;
        var currentBarX = ChartInfo.GetXByBar(CurrentBar - 1);
        var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
        var currentBarRightX = currentBarX + barWidth;

        // Get pen from LevelSettings
        var renderPen = levelSettings.RenderPen;
        var labelText = BuildLabel(period, kind);

        // Draw line first (if LineType != None)
        switch (levelSettings.LineType)
        {
            case LineType.Bar:
                // If label is at bar position, start line after the label to avoid overlap
                if (levelSettings.LabelPosition == LabelPosition.Bar)
                {
                    // Calculate actual label width for better positioning
                    var labelSize = context.MeasureString(labelText, _font);
                    var labelStartX = currentBarRightX + 5;
                    var lineStartX = labelStartX + labelSize.Width + 4; // 4px padding
                    context.DrawLine(renderPen, lineStartX, y, chartWidth, y);
                }
                else
                {
                    // Normal bar line from right edge of bar to price axis
                    context.DrawLine(renderPen, currentBarRightX, y, chartWidth, y);
                }
                break;
            case LineType.Full:
                context.DrawLine(renderPen, 0, y, chartWidth, y);
                break;
            case LineType.None:
                // No line to draw
                break;
        }

        // Draw price label (if ShowPrice == true)
        if (levelSettings.ShowPrice)
        {
            DrawPriceLabel(context, level.Price, y, renderPen, levelSettings);
        }

        // Draw text label (if LabelPosition != None)
        switch (levelSettings.LabelPosition)
        {
            case LabelPosition.Bar:
                var barLabelX = currentBarRightX + 5;
                DrawTextLabel(context, labelText, barLabelX, y, renderPen, levelSettings.TextColor, false);
                break;
            case LabelPosition.Right:
                var rightLabelX = chartWidth - 5;
                DrawTextLabel(context, labelText, rightLabelX, y, renderPen, levelSettings.TextColor, true);
                break;
            case LabelPosition.Left:
                var leftLabelX = 5;
                DrawTextLabel(context, labelText, leftLabelX, y, renderPen, levelSettings.TextColor, false);
                break;
            case LabelPosition.None:
                // No text label to draw
                break;
        }
    }

    private void DrawPriceLabel(RenderContext context, decimal price, int y, RenderPen pen, LevelSettings levelSettings)
    {
        var priceText = string.Format(ChartInfo.StringFormat, price);
        
        // An explicit text color is used as is; Auto contrasts with the label background, i.e. the line color
        var backgroundColor = levelSettings.Color;
        var textColor = levelSettings.TextColor ?? GetContrastingColor(backgroundColor);
        
        this.DrawLabelOnPriceAxis(context, priceText, y, _axisFont, backgroundColor.Convert(), textColor.Convert());
    }
    
    private CrossColor GetContrastingColor(CrossColor backgroundColor)
    {
        // Calculate luminance using relative luminance formula
        // See: https://www.w3.org/TR/WCAG20/#relativeluminancedef
        double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
        
        // If background is dark, use white text; if light, use black text
        if (luminance > 0.5)
        {
            // Dark text for light backgrounds
            return CrossColors.Black;
        }
        else
        {
            // Light text for dark backgrounds
            return CrossColors.White;
        }
    }

    private void DrawTextLabel(RenderContext context, string text, int x, int y, RenderPen pen, CrossColor? explicitTextColor, bool alignRight)
    {
        var size = context.MeasureString(text, _font);
        var backgroundColor = ChartInfo.ColorsStore.BaseBackgroundColor;
        // An explicit text color is used as is; Auto contrasts with the chart background the label sits on
        var textColor = explicitTextColor ?? GetContrastingColor(backgroundColor.Convert());

        // Calculate rectangle position based on alignment
        var rectX = alignRight ? x - size.Width : x;
        var rect = new Rectangle(rectX - 2, y - size.Height / 2 - 1, size.Width + 4, size.Height + 2);

        // Draw background with border
        context.FillRectangle(backgroundColor, rect);
        context.DrawRectangle(pen, rect);

        // Draw text
        var textRect = new Rectangle(rectX, y - size.Height / 2, size.Width, size.Height);
        var format = alignRight ? _stringRightFormat : _stringLeftFormat;
        context.DrawString(text, _font, textColor.Convert(), textRect, format);
    }

    #endregion

    #region SubscribeAllLevels

    /// <summary>
    /// The nine levels of a period, in the order of <see cref="LevelCount"/>: open, high, low,
    /// close, equilibrium, POC, VWAP, VAH, VAL. Written out rather than found by reflection,
    /// because the order is what names every level and places every band.
    /// </summary>
    private LevelSettings[] BuildLevelsOf(FixedProfilePeriods period) => period switch
    {
        FixedProfilePeriods.CurrentDay =>
        [
            DayOpenLevel, DayHighLevel, DayLowLevel, DayCloseLevel, DayEquilibriumLevel,
            DayPOCLevel, DayVWAPLevel, DayVAHLevel, DayVALLevel,
        ],
        FixedProfilePeriods.LastDay =>
        [
            PrevDayOpenLevel, PrevDayHighLevel, PrevDayLowLevel, PrevDayCloseLevel, PrevDayEquilibriumLevel,
            PrevDayPOCLevel, PrevDayVWAPLevel, PrevDayVAHLevel, PrevDayVALLevel,
        ],
        FixedProfilePeriods.CurrentWeek =>
        [
            WeekOpenLevel, WeekHighLevel, WeekLowLevel, WeekCloseLevel, WeekEquilibriumLevel,
            WeekPOCLevel, WeekVWAPLevel, WeekVAHLevel, WeekVALLevel,
        ],
        FixedProfilePeriods.LastWeek =>
        [
            PrevWeekOpenLevel, PrevWeekHighLevel, PrevWeekLowLevel, PrevWeekCloseLevel, PrevWeekEquilibriumLevel,
            PrevWeekPOCLevel, PrevWeekVWAPLevel, PrevWeekVAHLevel, PrevWeekVALLevel,
        ],
        FixedProfilePeriods.CurrentMonth =>
        [
            MonthOpenLevel, MonthHighLevel, MonthLowLevel, MonthCloseLevel, MonthEquilibriumLevel,
            MonthPOCLevel, MonthVWAPLevel, MonthVAHLevel, MonthVALLevel,
        ],
        FixedProfilePeriods.LastMonth =>
        [
            PrevMonthOpenLevel, PrevMonthHighLevel, PrevMonthLowLevel, PrevMonthCloseLevel, PrevMonthEquilibriumLevel,
            PrevMonthPOCLevel, PrevMonthVWAPLevel, PrevMonthVAHLevel, PrevMonthVALLevel,
        ],
        FixedProfilePeriods.Contract =>
        [
            ContractOpenLevel, ContractHighLevel, ContractLowLevel, ContractCloseLevel, ContractEquilibriumLevel,
            ContractPOCLevel, ContractVWAPLevel, ContractVAHLevel, ContractVALLevel,
        ],
        _ => [],
    };

    /// <summary>
    /// Rebuilds the table of levels per period and keeps the handlers in step with it. A restored
    /// template replaces the level objects, so the ones that are no longer reachable are dropped.
    /// </summary>
    private void SubscribeAllLevels()
    {
        var current = new HashSet<LevelSettings>(RefEqComparer.Instance);

        for (var i = 0; i < AllPeriods.Length; i++)
        {
            var period = AllPeriods[i];
            var levels = BuildLevelsOf(period);

            foreach (var ls in levels)
            {
                current.Add(ls);
                TrySubscribe(ls, period);
            }

            _levelsByPeriod[i] = levels;
        }

        foreach (var ls in _subscribedLevels.ToArray())
        {
            if (current.Contains(ls))
                continue;

            ls.PropertyChanged -= OnLevelSettingsChanged;
            _subscribedLevels.Remove(ls);
            _periodByLevel.Remove(ls);
        }
    }

    private void TrySubscribe(LevelSettings? ls, FixedProfilePeriods period)
    {
        if (ls is null) return;

        if (_subscribedLevels.Add(ls))
        {
            _periodByLevel[ls] = period;
            ls.PropertyChanged += OnLevelSettingsChanged;
        }
    }

    private void OnLevelSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not LevelSettings ls)
            return;

        if (e.PropertyName != nameof(LevelSettings.Enabled))
        {
            // Color, width, style, label position, price on the axis: nothing to recalculate,
            // but the chart keeps the old frame until it is told to draw again.
            RedrawChart();
            return;
        }

        if (_periodByLevel.TryGetValue(ls, out var period))
        {
            RecalcNeedFor(period);

            if (ls.Enabled && IsNeeded(period))
                RequestProfileForPeriod(period, force: false);
            else
                RedrawChart();
        }
    }

    private bool IsNeeded(FixedProfilePeriods period)
    {
        var index = IndexOf(period);

        return index >= 0 && _needed[index];
    }

    #endregion

    #endregion
}