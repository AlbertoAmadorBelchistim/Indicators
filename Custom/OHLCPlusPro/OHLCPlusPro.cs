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

/// <summary>Where the color, the width and the style of a level come from.</summary>
public enum VisualScheme
{
    /// <summary>From the level itself, as the platform indicator does.</summary>
    [Display(ResourceType = typeof(Res), Name = nameof(Res.SchemeLevels))]
    Levels = 0,

    /// <summary>From the period: the day stands out and the older periods fade back.</summary>
    [Display(ResourceType = typeof(Res), Name = nameof(Res.SchemeByPeriod))]
    ByPeriod = 1,

    /// <summary>From the kind of level: the anchors stand out and the rest support them.</summary>
    [Display(ResourceType = typeof(Res), Name = nameof(Res.SchemeByLevelType))]
    ByLevelType = 2,
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
    private string _text = string.Empty;
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

    /// <summary>
    /// Text of this level, in place of the one the template builds. Empty means the template.
    /// </summary>
    [Display(ResourceType = typeof(Res), Name = nameof(Res.Text), Description = nameof(Res.OhlcPlusLevelTextDescription))]
    public string Text
    {
        get => _text;
        set => SetField(ref _text, value ?? string.Empty);
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
        CrossColor? textColor = null,
        string text = ""
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
        Text = text;
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

    /// <summary>
    /// One line and its label, measured and placed once the whole frame is known: a label can
    /// only dodge the others when all of them are on the table.
    /// </summary>
    private readonly struct LabelRequest
    {
        public LabelRequest(string text, int anchorX, int y, bool alignRight, bool isBar,
            LineType lineType, int lineX1, int lineX2, RenderPen pen, CrossColor? textColor, int priority, int sequence)
        {
            Text = text;
            AnchorX = anchorX;
            Y = y;
            AlignRight = alignRight;
            IsBar = isBar;
            LineType = lineType;
            LineX1 = lineX1;
            LineX2 = lineX2;
            Pen = pen;
            TextColor = textColor;
            Priority = priority;
            Sequence = sequence;
        }

        public string Text { get; }

        public int AnchorX { get; }

        public int Y { get; }

        public bool AlignRight { get; }

        public bool IsBar { get; }

        public LineType LineType { get; }

        public int LineX1 { get; }

        public int LineX2 { get; }

        public RenderPen Pen { get; }

        public CrossColor? TextColor { get; }

        /// <summary>Lower goes first, and the first one placed keeps the spot it asked for.</summary>
        public int Priority { get; }

        public int Sequence { get; }

        public bool HasText => Text.Length > 0;
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

    // Rebuilt on every frame: the labels of the frame and the rectangles already taken.
    private readonly List<LabelRequest> _labelQueue = new(64);
    private readonly List<Rectangle> _placedLabels = new(64);
    private readonly List<(int Left, int Right)> _lineHoles = new(16);

    /// <summary>Room left around a label before another one is considered to be on top of it.</summary>
    private const int LabelPadding = 2;

    private const int LabelProbeStep = 12;
    private const int LabelMaxShift = 72;
    private const int LabelVerticalSteps = 4;

    // Scheme tables, indexed like AllPeriods and like the nine levels.
    private static readonly int[] PeriodWidths = [2, 1, 1, 1, 1, 1, 1];

    private static readonly LineDashStyle[] PeriodDashes =
    [
        LineDashStyle.Solid, LineDashStyle.Solid, LineDashStyle.Dash, LineDashStyle.Dot,
        LineDashStyle.DashDot, LineDashStyle.DashDot, LineDashStyle.Dot,
    ];

    private static readonly int[] KindWidths = [1, 1, 1, 1, 1, 3, 2, 1, 1];

    private static readonly LineDashStyle[] KindDashes =
    [
        LineDashStyle.Dash, LineDashStyle.Solid, LineDashStyle.Solid, LineDashStyle.Dash, LineDashStyle.Dot,
        LineDashStyle.Solid, LineDashStyle.Solid, LineDashStyle.Dot, LineDashStyle.Dot,
    ];

    /// <summary>Which label claims its place first: anchors, then the value area, then the rest.</summary>
    private static readonly int[] KindPriority = [6, 4, 5, 7, 8, 0, 1, 2, 3];

    private readonly CrossColor[] _periodColors =
    [
        Rgb(0x29, 0xB6, 0xF6), Rgb(0x02, 0x88, 0xD1), Rgb(0x66, 0xBB, 0x6A), Rgb(0x2E, 0x7D, 0x32),
        Rgb(0xFF, 0xA7, 0x26), Rgb(0xEF, 0x6C, 0x00), Rgb(0xBD, 0xBD, 0xBD),
    ];

    private readonly CrossColor[] _levelColors =
    [
        Rgb(0x9E, 0x9E, 0x9E), Rgb(0xEF, 0x53, 0x50), Rgb(0x26, 0xA6, 0x9A), Rgb(0x60, 0x7D, 0x8B),
        Rgb(0x8D, 0x6E, 0x63), Rgb(0xFF, 0xB3, 0x00), Rgb(0x42, 0xA5, 0xF5), Rgb(0xAB, 0x47, 0xBC),
        Rgb(0x7E, 0x57, 0xC2),
    ];

    private readonly RenderPen[,] _schemePens = new RenderPen[AllPeriods.Length, LevelCount];

    private VisualScheme _visualScheme = VisualScheme.Levels;
    private bool _schemeDirty = true;

    private bool _avoidLabelOverlap = true;
    private bool _clipLinesAtLabels = true;

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

    #region Scheme

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Scheme), Name = nameof(Res.VisualScheme), Description = nameof(Res.OhlcPlusSchemeDescription), Order = 1300)]
    public VisualScheme Scheme
    {
        get => _visualScheme;
        set
        {
            _visualScheme = value;
            InvalidateScheme();
        }
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.CurrentDay), Order = 1310)]
    public CrossColor PeriodColorDay
    {
        get => _periodColors[0];
        set => SetSchemeColor(_periodColors, 0, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.PreviousDay), Order = 1320)]
    public CrossColor PeriodColorPrevDay
    {
        get => _periodColors[1];
        set => SetSchemeColor(_periodColors, 1, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.CurrentWeek), Order = 1330)]
    public CrossColor PeriodColorWeek
    {
        get => _periodColors[2];
        set => SetSchemeColor(_periodColors, 2, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.PreviousWeek), Order = 1340)]
    public CrossColor PeriodColorPrevWeek
    {
        get => _periodColors[3];
        set => SetSchemeColor(_periodColors, 3, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.CurrentMonth), Order = 1350)]
    public CrossColor PeriodColorMonth
    {
        get => _periodColors[4];
        set => SetSchemeColor(_periodColors, 4, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.PreviousMonth), Order = 1360)]
    public CrossColor PeriodColorPrevMonth
    {
        get => _periodColors[5];
        set => SetSchemeColor(_periodColors, 5, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.PeriodColors), Name = nameof(Res.Contract), Order = 1370)]
    public CrossColor PeriodColorContract
    {
        get => _periodColors[6];
        set => SetSchemeColor(_periodColors, 6, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.BarOpen), Order = 1410)]
    public CrossColor LevelColorOpen
    {
        get => _levelColors[0];
        set => SetSchemeColor(_levelColors, 0, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.BarHigh), Order = 1420)]
    public CrossColor LevelColorHigh
    {
        get => _levelColors[1];
        set => SetSchemeColor(_levelColors, 1, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.BarLow), Order = 1430)]
    public CrossColor LevelColorLow
    {
        get => _levelColors[2];
        set => SetSchemeColor(_levelColors, 2, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.BarClose), Order = 1440)]
    public CrossColor LevelColorClose
    {
        get => _levelColors[3];
        set => SetSchemeColor(_levelColors, 3, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.Equilibrium), Order = 1450)]
    public CrossColor LevelColorEquilibrium
    {
        get => _levelColors[4];
        set => SetSchemeColor(_levelColors, 4, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.POC), Order = 1460)]
    public CrossColor LevelColorPoc
    {
        get => _levelColors[5];
        set => SetSchemeColor(_levelColors, 5, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.VWAP), Order = 1470)]
    public CrossColor LevelColorVwap
    {
        get => _levelColors[6];
        set => SetSchemeColor(_levelColors, 6, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.VAH), Order = 1480)]
    public CrossColor LevelColorVah
    {
        get => _levelColors[7];
        set => SetSchemeColor(_levelColors, 7, value);
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.LevelColors), Name = nameof(Res.VAL), Order = 1490)]
    public CrossColor LevelColorVal
    {
        get => _levelColors[8];
        set => SetSchemeColor(_levelColors, 8, value);
    }

    #endregion

    #region Label layout

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.AvoidOverlap), Description = nameof(Res.OhlcPlusAvoidOverlapDescription), Order = 1195)]
    public bool AvoidLabelOverlap
    {
        get => _avoidLabelOverlap;
        set
        {
            _avoidLabelOverlap = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Res), GroupName = nameof(Res.Labels), Name = nameof(Res.ClipLines), Description = nameof(Res.OhlcPlusClipLinesDescription), Order = 1196)]
    public bool ClipLinesAtLabels
    {
        get => _clipLinesAtLabels;
        set
        {
            _clipLinesAtLabels = value;
            RedrawChart();
        }
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

        _labelQueue.Clear();
        _placedLabels.Clear();

        foreach (var period in AllPeriods)
        {
            var levels = LevelsOf(period);

            if (levels.Length != LevelCount)
                continue;

            for (var kind = 0; kind < LevelCount; kind++)
                RenderLevel(context, period, kind, levels[kind]);
        }

        FlushLabels(context);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Text of a level: the template with the prefix of its period and the name of its kind.
    /// </summary>
    private string BuildLabel(FixedProfilePeriods period, int kind, LevelSettings levelSettings)
    {
        // A text written on the level itself wins over the template.
        if (levelSettings.Text.Length > 0)
            return levelSettings.Text;

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

    private static CrossColor Rgb(int r, int g, int b) => Color.FromArgb(r, g, b).Convert();

    private void SetSchemeColor(CrossColor[] palette, int index, CrossColor value)
    {
        palette[index] = value;
        InvalidateScheme();
    }

    private void InvalidateScheme()
    {
        _schemeDirty = true;
        RedrawChart();
    }

    /// <summary>
    /// Pen of a level under the active scheme. With the Levels scheme it is the pen of the level
    /// itself; the other two build one per period and kind and keep it until the scheme changes.
    /// </summary>
    private RenderPen PenFor(FixedProfilePeriods period, int kind, LevelSettings levelSettings)
    {
        if (_visualScheme == VisualScheme.Levels)
            return levelSettings.RenderPen;

        var index = IndexOf(period);

        if (index < 0)
            return levelSettings.RenderPen;

        if (_schemeDirty)
        {
            Array.Clear(_schemePens, 0, _schemePens.Length);
            _schemeDirty = false;
        }

        return _schemePens[index, kind] ??= new PenSettings
        {
            Color = _visualScheme == VisualScheme.ByPeriod ? _periodColors[index] : _levelColors[kind],
            Width = _visualScheme == VisualScheme.ByPeriod ? PeriodWidths[index] : KindWidths[kind],
            LineDashStyle = _visualScheme == VisualScheme.ByPeriod ? PeriodDashes[index] : KindDashes[kind],
        }.RenderObject;
    }

    /// <summary>Order in which the labels of the frame claim their place.</summary>
    private int PriorityOf(FixedProfilePeriods period, int kind)
    {
        return _visualScheme switch
        {
            VisualScheme.ByPeriod => IndexOf(period) * LevelCount + KindPriority[kind],
            VisualScheme.ByLevelType => KindPriority[kind] * AllPeriods.Length + Math.Max(IndexOf(period), 0),
            _ => 0,
        };
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

        var renderPen = PenFor(period, kind, levelSettings);
        var labelText = BuildLabel(period, kind, levelSettings);

        // The price tag on the axis lives outside the chart area: nothing can collide with it.
        if (levelSettings.ShowPrice)
            DrawPriceLabel(context, level.Price, y, renderPen, levelSettings);

        var alignRight = levelSettings.LabelPosition == LabelPosition.Right;
        var isBar = levelSettings.LabelPosition == LabelPosition.Bar;

        var anchorX = levelSettings.LabelPosition switch
        {
            LabelPosition.Bar => currentBarRightX + 5,
            LabelPosition.Right => chartWidth - 5,
            LabelPosition.Left => 5,
            _ => 0,
        };

        var lineX1 = levelSettings.LineType == LineType.Bar ? currentBarRightX : 0;
        var text = levelSettings.LabelPosition == LabelPosition.None ? string.Empty : labelText;

        if (levelSettings.LineType == LineType.None && text.Length == 0)
            return;

        _labelQueue.Add(new LabelRequest(
            text,
            anchorX,
            y,
            alignRight,
            isBar,
            levelSettings.LineType,
            lineX1,
            chartWidth,
            renderPen,
            levelSettings.TextColor,
            PriorityOf(period, kind),
            sequence: _labelQueue.Count));
    }

    /// <summary>
    /// Places every label of the frame and then draws the lines around them. Labels go first so a
    /// line never crosses a text, and the ones that asked first keep their place.
    /// </summary>
    private void FlushLabels(RenderContext context)
    {
        if (_labelQueue.Count == 0)
            return;

        _labelQueue.Sort(static (a, b) =>
        {
            var byPriority = a.Priority.CompareTo(b.Priority);
            return byPriority != 0 ? byPriority : a.Sequence.CompareTo(b.Sequence);
        });

        var placed = new List<(LabelRequest Request, Rectangle Rect, Size Size)>(_labelQueue.Count);

        foreach (var request in _labelQueue)
        {
            if (!request.HasText)
                continue;

            var size = context.MeasureString(request.Text, _font);
            var rect = BuildLabelRect(request, size);

            if (_avoidLabelOverlap)
                rect = FindFreeSpot(rect, request);

            placed.Add((request, rect, size));
            _placedLabels.Add(rect);
        }

        foreach (var (request, rect, size) in placed)
            DrawTextLabel(context, request, rect, size);

        foreach (var request in _labelQueue)
        {
            if (request.LineType != LineType.None)
                DrawLineAroundLabels(context, request);
        }
    }

    private Rectangle BuildLabelRect(LabelRequest request, Size size)
    {
        var x = request.AlignRight ? request.AnchorX - size.Width : request.AnchorX;

        // A label at the bar starts exactly where the line does; the others get some air.
        var leftPadding = request.IsBar ? 0 : LabelPadding;

        return new Rectangle(x - leftPadding, request.Y - size.Height / 2 - 1, size.Width + 4, size.Height + 2);
    }

    /// <summary>
    /// Moves a label that landed on another one: first sideways, away from the price scale or
    /// towards it, and only then up and down, which is where it starts to lie about its price.
    /// </summary>
    private Rectangle FindFreeSpot(Rectangle desired, LabelRequest request)
    {
        if (!IntersectsPlaced(desired))
            return desired;

        var direction = request.AlignRight ? -1 : 1;

        for (var step = LabelProbeStep; step <= LabelMaxShift; step += LabelProbeStep)
        {
            var probe = desired;
            probe.X = desired.X + step * direction;

            if (probe.X >= 0 && probe.Right <= ChartInfo.PriceChartContainer.Region.Width && !IntersectsPlaced(probe))
                return probe;
        }

        var height = desired.Height;

        for (var step = 1; step <= LabelVerticalSteps; step++)
        {
            var offset = step * (height + LabelPadding);

            var up = desired;
            up.Y = desired.Y - offset;

            if (up.Y >= 0 && !IntersectsPlaced(up))
                return up;

            var down = desired;
            down.Y = desired.Y + offset;

            if (down.Bottom <= ChartInfo.PriceChartContainer.Region.Height && !IntersectsPlaced(down))
                return down;
        }

        return desired;
    }

    private bool IntersectsPlaced(Rectangle rect)
    {
        foreach (var placed in _placedLabels)
        {
            if (rect.IntersectsWith(placed))
                return true;
        }

        return false;
    }

    /// <summary>Draws the line of a level in the pieces the labels leave free.</summary>
    private void DrawLineAroundLabels(RenderContext context, LabelRequest request)
    {
        var x1 = request.LineX1;
        var x2 = request.LineX2;
        var y = request.Y;

        if (x2 <= x1)
            return;

        if (!_clipLinesAtLabels)
        {
            context.DrawLine(request.Pen, x1, y, x2, y);
            return;
        }

        _lineHoles.Clear();

        foreach (var rect in _placedLabels)
        {
            if (y < rect.Top || y >= rect.Bottom)
                continue;

            var left = rect.Left - LabelPadding;
            var right = rect.Right + LabelPadding;

            if (right <= x1 || left >= x2)
                continue;

            _lineHoles.Add((Math.Max(x1, left), Math.Min(x2, right)));
        }

        if (_lineHoles.Count == 0)
        {
            context.DrawLine(request.Pen, x1, y, x2, y);
            return;
        }

        _lineHoles.Sort(static (a, b) => a.Left.CompareTo(b.Left));

        // A piece shorter than this is a stub sticking out of a label, not a line.
        const int MinSegment = 4;

        var start = x1;

        foreach (var (left, right) in _lineHoles)
        {
            if (left - start >= MinSegment)
                context.DrawLine(request.Pen, start, y, left, y);

            if (right > start)
                start = right;
        }

        if (x2 - start >= MinSegment)
            context.DrawLine(request.Pen, start, y, x2, y);
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

    private void DrawTextLabel(RenderContext context, LabelRequest request, Rectangle rect, Size size)
    {
        var backgroundColor = ChartInfo.ColorsStore.BaseBackgroundColor;
        // An explicit text color is used as is; Auto contrasts with the chart background the label sits on
        var textColor = request.TextColor ?? GetContrastingColor(backgroundColor.Convert());

        context.FillRectangle(backgroundColor, rect);
        context.DrawRectangle(request.Pen, rect);

        var textRect = new Rectangle(rect.X + LabelPadding, rect.Y + 1, size.Width, size.Height);
        var format = request.AlignRight ? _stringRightFormat : _stringLeftFormat;
        context.DrawString(request.Text, _font, textColor.Convert(), textRect, format);
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