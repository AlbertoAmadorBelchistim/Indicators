namespace ATAS.Indicators.Technical;

using ATAS.Indicators.Technical.Properties;
using OFT.Localization;
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

public enum LabelPosition
{
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
    None = 0,
    
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bar))]
    Bar = 1,
    
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Right))]
    Right = 2,
    
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Left))]
    Left = 3
}

public enum LineType
{
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
    None = 0,


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.TillBar))]
    Bar = 1,


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FullWidth))]
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

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

[Editor(typeof(ATAS.Indicators.Technical.Editors.LevelSettingsEditor), typeof(ATAS.Indicators.Technical.Editors.LevelSettingsEditor))]
public class LevelSettings : NotifiableObject
{
    #region Fields

    private bool _enabled;
    private CrossColor _color;
    private bool _showPrice;
    private LineType _lineType;
    private int _width;
    private LineDashStyle _lineStyle;
    private LabelPosition _labelPosition;
    private string _overrideLabel = string.Empty;

    #endregion

    #region Properties

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled))]
    public bool Enabled 
    {
        get => _enabled;
        set => SetField(ref _enabled, value);
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color))]
    public CrossColor Color
    {
        get => _color;
        set => SetField(ref _color, value);
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowPrice))]
    public bool ShowPrice 
    {
        get => _showPrice;
        set => SetField(ref _showPrice, value);
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Line))]
    public LineType LineType 
    {
        get => _lineType;
        set => SetField(ref _lineType, value);
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Width))]
    [Range(1, 10)]
    public int Width 
    { 
        get => _width;
        set => SetField(ref _width, value);
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineStyle))]
    public LineDashStyle LineStyle 
    { 
        get => _lineStyle;
        set => SetField(ref _lineStyle, value);
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Label))]
    public LabelPosition LabelPosition 
    {
        get => _labelPosition;
        set => SetField(ref _labelPosition, value);
    }

    [Display(Name = "Override label")]
    public string OverrideLabel
    {
        get => _overrideLabel;
        set => SetField(ref _overrideLabel, value);
    }

    [Browsable(false)]
    public RenderPen RenderPen => new PenSettings { Color = Color, Width = Width, LineDashStyle = LineStyle }.RenderObject;

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
        LineType lineType = LineType.Bar
    )
    {
        Enabled = enabled;
        Color = color == default ? System.Drawing.Color.Blue.Convert() : color;
        Width = width;
        LineStyle = lineStyle;
        ShowPrice = showPrice;
        LabelPosition = labelPosition;
        LineType = lineType;
    }

    #endregion
}

[DisplayName("OHLC Plus")]
[Category(IndicatorCategories.VolumeOrderFlow)]

[Display(ResourceType = typeof(Strings), Description = nameof(Strings.OHLCPlusDescription))]

public class OHLCPlus : Indicator
{
    #region Nested types

    private class LevelData
    {
        public decimal Price { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }

    private sealed class RefEqComparer : IEqualityComparer<object>
    {
        public static readonly RefEqComparer Instance = new();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private readonly struct LabelDrawRequest
    {
        public LabelDrawRequest(
            string text,
            int xAnchor,
            int yAnchor,
            RenderPen borderPen,
            bool alignRight,
            LineType lineType,
            int lineX1,
            int lineX2,
            int lineY,
            int priority,
            int sequence,
            bool isBar)
        {
            Text = text;
            XAnchor = xAnchor;
            YAnchor = yAnchor;
            BorderPen = borderPen;
            AlignRight = alignRight;

            LineType = lineType;
            LineX1 = lineX1;
            LineX2 = lineX2;
            LineY = lineY;

            Priority = priority;
            Sequence = sequence;
            IsBar = isBar;
        }

        public string Text { get; }
        public int XAnchor { get; }
        public int YAnchor { get; }
        public RenderPen BorderPen { get; }
        public bool AlignRight { get; }

        // Deferred line info (only used if LineType != None)
        public LineType LineType { get; }
        public int LineX1 { get; }
        public int LineX2 { get; }
        public int LineY { get; }

        // Lower = drawn first (more �important� in pq01 = preserve current order)
        public int Priority { get; }

        public int Sequence { get; }

        public bool IsBar { get; }
    }

    #region Visual Semantic (pq02)

    public enum VisualSemanticMode
    {
        Legacy = 0,
        RuleSet = 1
    }

    public enum VisualSemanticPresetKind
    {
        ByPeriod = 0,
        ByLevelType = 1
    }

    private enum LevelType
    {
        Unknown = 0,
        Open,
        High,
        Low,
        Close,
        EQ,
        POC,
        VWAP,
        VAH,
        VAL
    }

    [Flags]
    private enum SemanticFamily
    {
        None = 0,
        Reference = 1 << 0,  // e.g., Open/Close (optional)
        Extreme = 1 << 1,  // High/Low (optional)
        ValueArea = 1 << 2,  // POC/VAH/VAL/VWAP/EQ (optional)
        Custom = 1 << 3
    }

    private readonly struct LevelDescriptor
    {
        public LevelDescriptor(LevelType type, FixedProfilePeriods period, SemanticFamily family)
        {
            Type = type;
            Period = period;
            Family = family;
        }

        public LevelType Type { get; }
        public FixedProfilePeriods Period { get; }
        public SemanticFamily Family { get; }
    }

    private sealed class VisualStyleDelta
    {
        public CrossColor? Color { get; init; }
        public int? Width { get; init; }
        public LineDashStyle? Dash { get; init; }
        public byte? Opacity { get; init; } // optional; can be ignored initially
    }

    private readonly struct ResolvedVisualSemantic
    {
        public ResolvedVisualSemantic(VisualStyleDelta style, int semanticWeight)
        {
            Style = style;
            SemanticWeight = semanticWeight;
        }

        public VisualStyleDelta Style { get; }
        public int SemanticWeight { get; } // used for pq01 tie-breaking later
    }

    private readonly struct VisualRule
    {
        public VisualRule(FixedProfilePeriods? period, LevelType? levelType, VisualStyleDelta style)
        {
            Period = period;
            LevelType = levelType;
            Style = style;
        }

        public FixedProfilePeriods? Period { get; }
        public LevelType? LevelType { get; }
        public VisualStyleDelta Style { get; }
    }

    private sealed class VisualRuleSet
    {
        public static readonly VisualRuleSet Empty = new([]);

        public VisualRuleSet(List<VisualRule> rules)
        {
            Rules = rules;
        }

        public List<VisualRule> Rules { get; }
    }


    #region Mapping

    private static readonly Dictionary<string, LevelType> _suffixToLevelType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Open"] = LevelType.Open,
        ["High"] = LevelType.High,
        ["Low"] = LevelType.Low,
        ["Close"] = LevelType.Close,
        ["EQ"] = LevelType.EQ,
        ["POC"] = LevelType.POC,
        ["VWAP"] = LevelType.VWAP,
        ["VAH"] = LevelType.VAH,
        ["VAL"] = LevelType.VAL
    };


    private static LevelType LevelTypeFromSuffix(string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
            return LevelType.Unknown;

        return _suffixToLevelType.TryGetValue(suffix, out var t)
            ? t
            : LevelType.Unknown;
    }

    // Family is OPTIONAL and user-driven later.
    // For pq02.1 we keep a conservative default classification.
    private static SemanticFamily DefaultFamily(LevelType t) => t switch
    {
        LevelType.Open or LevelType.Close or LevelType.EQ => SemanticFamily.Reference,
        LevelType.High or LevelType.Low => SemanticFamily.Extreme,
        LevelType.POC or LevelType.VAH or LevelType.VAL or LevelType.VWAP => SemanticFamily.ValueArea,
        _ => SemanticFamily.None
    };

    #endregion

    #endregion



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


    private readonly Dictionary<FixedProfilePeriods, IndicatorCandle> _profileCandles = [];
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

    // --- Label layout (per-frame) ---
    private readonly List<LabelDrawRequest> _labelQueue = new(128);
    private readonly List<Rectangle> _occupiedLabelRects = new(128);
    private const int _labelPadding = 4;
    private const int _labelProbeStep = 2;
    private const int _labelProbeMaxSteps = 30;

    private const int _labelHProbeStep = 10;
    private const int _labelHMaxShift = 120;

    // Vertical fallback (used only if X corridor is full)
    private const int _labelVProbeStep = 8;
    private const int _labelVProbeMaxSteps = 24;

    private int _lastBar = -1;
    private bool _candleRequested;

    private bool _needDay;
    private bool _needPrevDay;
    private bool _needWeek;
    private bool _needPrevWeek;
    private bool _needMonth;
    private bool _needPrevMonth;
    private bool _needContract;

    private bool _allLevelsVisible = true;

    //Label override
    private string _labelTemplate = "{prefix}{level}";

    // Level suffixes (level-level)
    private string _openLabel = "Open";
    private string _highLabel = "High";
    private string _lowLabel = "Low";
    private string _closeLabel = "Close";
    private string _equilibriumLabel = "EQ";
    private string _pocLabel = "POC";
    private string _vwapLabel = "VWAP";
    private string _vahLabel = "VAH";
    private string _valLabel = "VAL";

    private static readonly string[] _knownLevelSuffixes =
{
    "Close",
    "Open",
    "High",
    "Low",
    "VWAP",
    "POC",
    "VAH",
    "VAL",
    "EQ"
};

    // Label prefixes (period-level)
    private string _dayPrefix = "D";
    private string _prevDayPrefix = "PD";
    private string _weekPrefix = "W";
    private string _prevWeekPrefix = "PW";
    private string _monthPrefix = "M";
    private string _prevMonthPrefix = "PM";
    private string _contractPrefix = "C";

    #region Visual Semantic - Resolver (disabled by default)

    private VisualSemanticMode _visualSemanticMode = VisualSemanticMode.Legacy;

    private VisualSemanticPresetKind _visualSemanticPreset = VisualSemanticPresetKind.ByPeriod;

    // Cache (rebuild only when preset changes)
    private VisualRuleSet _visualRuleSet = VisualRuleSet.Empty;
    private bool _visualRuleSetDirty = true;

    // Legacy mode intentionally bypasses pq02 semantics and relies exclusively
    // on per-level LevelSettings (upstream-compatible behavior).
    private static readonly VisualStyleDelta _emptyVisualStyle = new();
    private static readonly ResolvedVisualSemantic _emptySemantic = new(_emptyVisualStyle, semanticWeight: 0);

    #endregion

    #endregion

    #region Properties

    [Display(GroupName = "Visual Semantic (pq02)", Name = "Mode", Order = 1)]
    public VisualSemanticMode VisualSemantic
    {
        get => _visualSemanticMode;
        set
        {
            if (_visualSemanticMode == value)
                return;

            _visualSemanticMode = value;
            RedrawChart();
        }
    }

    [Display(GroupName = "Visual Semantic (pq02)", Name = "Preset", Order = 2)]
    public VisualSemanticPresetKind VisualSemanticPreset
    {
        get => _visualSemanticPreset;
        set
        {
            if (_visualSemanticPreset == value)
                return;

            _visualSemanticPreset = value;
            _visualRuleSetDirty = true;
            RedrawChart();
        }
    }


    #region Day Settings


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarOpen), Order = 10)]



    public LevelSettings DayOpenLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarHigh), Order = 20)]

    public LevelSettings DayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarLow), Order = 30)]

    public LevelSettings DayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings DayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings DayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings DayPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentDay), Order = 65)]
    public LevelSettings DayVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.VAH), Order = 70)]

    public LevelSettings DayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.VAL), Order = 80)]
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


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarOpen), Order = 10)]
    public LevelSettings PrevDayOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarHigh), Order = 20)]
    public LevelSettings PrevDayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarLow), Order = 30)]
    public LevelSettings PrevDayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings PrevDayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings PrevDayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings PrevDayPOCLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousDay), Order = 65)]
    public LevelSettings PrevDayVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.VAH), Order = 70)]
    public LevelSettings PrevDayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.VAL), Order = 80)]
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


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarOpen), Order = 10)]
    public LevelSettings WeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarHigh), Order = 20)]
    public LevelSettings WeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarLow), Order = 30)]
    public LevelSettings WeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings WeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings WeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings WeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentWeek), Order = 65)]

    public LevelSettings WeekVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.VAH), Order = 70)]
    public LevelSettings WeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.VAL), Order = 80)]
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


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarOpen), Order = 10)]
    public LevelSettings PrevWeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarHigh), Order = 20)]
    public LevelSettings PrevWeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarLow), Order = 30)]
    public LevelSettings PrevWeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings PrevWeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings PrevWeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings PrevWeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousWeek), Order = 65)]
    public LevelSettings PrevWeekVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.VAH), Order = 70)]
    public LevelSettings PrevWeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.VAL), Order = 80)]
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


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarOpen), Order = 10)]
    public LevelSettings MonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarHigh), Order = 20)]
    public LevelSettings MonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarLow), Order = 30)]
    public LevelSettings MonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings MonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings MonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings MonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentMonth), Order = 65)]
    public LevelSettings MonthVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.VAH), Order = 70)]
    public LevelSettings MonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.VAL), Order = 80)]
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


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarOpen), Order = 10)]
    public LevelSettings PrevMonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarHigh), Order = 20)]
    public LevelSettings PrevMonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarLow), Order = 30)]
    public LevelSettings PrevMonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings PrevMonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings PrevMonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings PrevMonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousMonth), Order = 65)]
    public LevelSettings PrevMonthVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.VAH), Order = 70)]
    public LevelSettings PrevMonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.VAL), Order = 80)]
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


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarOpen), Order = 10)]
    public LevelSettings ContractOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarHigh), Order = 20)]
    public LevelSettings ContractHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarLow), Order = 30)]
    public LevelSettings ContractLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarClose), Order = 40)]
    public LevelSettings ContractCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.Equilibrium), Order = 50)]
    public LevelSettings ContractEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.POC), Order = 60)]
    public LevelSettings ContractPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.Contract), Order = 65)]
    public LevelSettings ContractVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.VAH), Order = 70)]
    public LevelSettings ContractVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );


    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.VAL), Order = 80)]
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

    #region Visibility Settings

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visibility), Name = nameof(Strings.ToggleLevelsVisibilityHotKey), Order = 1000)]
    public CrossKey[] ToggleVisibilityHotKey { get; set; } = { CrossKey.Q };

    #endregion

    #region Labels
    // --- Labels group ---
    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.LabelTemplate), Order = 10)]
    public string LabelTemplate
    {
        get => _labelTemplate;
        set => _labelTemplate = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.BarOpen), Order = 20)]
    public string OpenLabel
    {
        get => _openLabel;
        set => _openLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.BarHigh), Order = 30)]
    public string HighLabel
    {
        get => _highLabel;
        set => _highLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.BarLow), Order = 40)]
    public string LowLabel
    {
        get => _lowLabel;
        set => _lowLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.BarClose), Order = 50)]
    public string CloseLabel
    {
        get => _closeLabel;
        set => _closeLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.Equilibrium), Order = 60)]
    public string EquilibriumLabel
    {
        get => _equilibriumLabel;
        set => _equilibriumLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.POC), Order = 70)]
    public string PocLabel
    {
        get => _pocLabel;
        set => _pocLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.VWAP), Order = 80)]
    public string VwapLabel
    {
        get => _vwapLabel;
        set => _vwapLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.VAH), Order = 90)]
    public string VahLabel
    {
        get => _vahLabel;
        set => _vahLabel = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Labels), Name = nameof(Resources.VAL), Order = 100)]
    public string ValLabel
    {
        get => _valLabel;
        set => _valLabel = value;
    }

    // --- Prefixes group ---
    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.CurrentDay), Order = 10)]
    public string DayPrefix
    {
        get => _dayPrefix;
        set => _dayPrefix = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.PreviousDay), Order = 20)]
    public string PrevDayPrefix
    {
        get => _prevDayPrefix;
        set => _prevDayPrefix = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.CurrentWeek), Order = 30)]
    public string WeekPrefix
    {
        get => _weekPrefix;
        set => _weekPrefix = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.PreviousWeek), Order = 40)]
    public string PrevWeekPrefix
    {
        get => _prevWeekPrefix;
        set => _prevWeekPrefix = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.CurrentMonth), Order = 50)]
    public string MonthPrefix
    {
        get => _monthPrefix;
        set => _monthPrefix = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.PreviousMonth), Order = 60)]
    public string PrevMonthPrefix
    {
        get => _prevMonthPrefix;
        set => _prevMonthPrefix = value;
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Prefixes), Name = nameof(Resources.Contract), Order = 70)]
    public string ContractPrefix
    {
        get => _contractPrefix;
        set => _contractPrefix = value;
    }


    #endregion

    #region Visual Semantic Palettes (pq02)

    // Period palette (ByPeriod)
    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Current Day", Order = 10)]
    public CrossColor PeriodColorCurrentDay { get; set; } = CrossColors.WhiteSmoke;

    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Previous Day", Order = 20)]
    public CrossColor PeriodColorPreviousDay { get; set; } = CrossColors.DarkGray;

    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Current Week", Order = 30)]
    public CrossColor PeriodColorCurrentWeek { get; set; } = CrossColors.DeepSkyBlue;

    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Previous Week", Order = 40)]
    public CrossColor PeriodColorPreviousWeek { get; set; } = CrossColors.SteelBlue;

    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Current Month", Order = 50)]
    public CrossColor PeriodColorCurrentMonth { get; set; } = CrossColors.MediumSeaGreen;

    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Previous Month", Order = 60)]
    public CrossColor PeriodColorPreviousMonth { get; set; } = CrossColors.DarkOliveGreen;

    [Display(GroupName = "Visual Semantic (pq02) - Period Palette", Name = "Contract", Order = 70)]
    public CrossColor PeriodColorContract { get; set; } = CrossColors.SaddleBrown;


    // Level-type palette (ByLevelType)
    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "Open", Order = 10)]
    public CrossColor LevelColorOpen { get; set; } = CrossColors.DarkOrange;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "High", Order = 20)]
    public CrossColor LevelColorHigh { get; set; } = CrossColors.ForestGreen;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "Low", Order = 30)]
    public CrossColor LevelColorLow { get; set; } = CrossColors.Firebrick;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "Close", Order = 40)]
    public CrossColor LevelColorClose { get; set; } = CrossColors.DimGray;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "EQ", Order = 50)]
    public CrossColor LevelColorEQ { get; set; } = CrossColors.Gray;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "POC", Order = 60)]
    public CrossColor LevelColorPOC { get; set; } = CrossColors.Goldenrod;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "VWAP", Order = 70)]
    public CrossColor LevelColorVWAP { get; set; } = CrossColors.DodgerBlue;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "VAH", Order = 80)]
    public CrossColor LevelColorVAH { get; set; } = CrossColors.Teal;

    [Display(GroupName = "Visual Semantic (pq02) - Level Palette", Name = "VAL", Order = 90)]
    public CrossColor LevelColorVAL { get; set; } = CrossColors.Teal;

    #endregion


    #endregion

    #region Constructor

    public OHLCPlus()
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
        RecalcAllNeeds();
        SubscribeAllLevels();
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
            _levels.Clear();
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

        UpdateAllNeededLevelsFromCache();
    }

    protected override void OnFixedProfilesResponse(IndicatorCandle fixedProfileScaled, IndicatorCandle fixedProfileOriginScale, FixedProfilePeriods period)
    {
        _profileCandles[period] = fixedProfileOriginScale;
        if (UpdateLevels(period, fixedProfileOriginScale))
            RedrawChart();
    }

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo is null || InstrumentInfo is null)
            return;

        BeginLabelLayoutFrame();

        // Render all levels in groups for better organization
        RenderLevelGroup(context, "d", DayOpenLevel, DayHighLevel, DayLowLevel, DayCloseLevel, DayEquilibriumLevel, DayPOCLevel, DayVWAPLevel, DayVAHLevel, DayVALLevel);
        RenderLevelGroup(context, "p", PrevDayOpenLevel, PrevDayHighLevel, PrevDayLowLevel, PrevDayCloseLevel, PrevDayEquilibriumLevel, PrevDayPOCLevel, PrevDayVWAPLevel, PrevDayVAHLevel, PrevDayVALLevel);
        RenderLevelGroup(context, "w", WeekOpenLevel, WeekHighLevel, WeekLowLevel, WeekCloseLevel, WeekEquilibriumLevel, WeekPOCLevel, WeekVWAPLevel, WeekVAHLevel, WeekVALLevel);
        RenderLevelGroup(context, "pw", PrevWeekOpenLevel, PrevWeekHighLevel, PrevWeekLowLevel, PrevWeekCloseLevel, PrevWeekEquilibriumLevel, PrevWeekPOCLevel, PrevWeekVWAPLevel, PrevWeekVAHLevel, PrevWeekVALLevel);
        RenderLevelGroup(context, "m", MonthOpenLevel, MonthHighLevel, MonthLowLevel, MonthCloseLevel, MonthEquilibriumLevel, MonthPOCLevel, MonthVWAPLevel, MonthVAHLevel, MonthVALLevel);
        RenderLevelGroup(context, "pm", PrevMonthOpenLevel, PrevMonthHighLevel, PrevMonthLowLevel, PrevMonthCloseLevel, PrevMonthEquilibriumLevel, PrevMonthPOCLevel, PrevMonthVWAPLevel, PrevMonthVAHLevel, PrevMonthVALLevel);
        RenderLevelGroup(context, "c", ContractOpenLevel, ContractHighLevel, ContractLowLevel, ContractCloseLevel, ContractEquilibriumLevel, ContractPOCLevel, ContractVWAPLevel, ContractVAHLevel, ContractVALLevel);

        FlushLabelQueue(context);
    }

    #endregion

    #region Private methods

    private void ToggleAllLevelsVisibility()
    {
        _allLevelsVisible = !_allLevelsVisible;
        RedrawChart();
    }

    #region label overrides
    private string BuildLabelText(string prefix, string levelStorageKey, LevelSettings levelSettings)
    {
        // If the user overrides the label for this specific level, it should override everything.
        if (!string.IsNullOrWhiteSpace(levelSettings?.OverrideLabel))
            return levelSettings.OverrideLabel;

        var (displayPrefix, suffix) = SplitKey(levelStorageKey);

        // Prefer the provided prefix (period prefix), otherwise use the parsed one
        var effectivePrefix = prefix ?? displayPrefix ?? string.Empty;

        var levelText = ResolveLevelText(suffix);

        var template = string.IsNullOrEmpty(LabelTemplate) ? "{prefix}{level}" : LabelTemplate;

        var result = template.Replace("{prefix}", effectivePrefix, StringComparison.Ordinal)
                             .Replace("{level}", levelText ?? string.Empty, StringComparison.Ordinal);

        return string.IsNullOrWhiteSpace(result) ? levelText : result;
    }

    private static (string Prefix, string Suffix) SplitKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return (string.Empty, string.Empty);

        foreach (var suffix in _knownLevelSuffixes)
        {
            if (key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                var prefix = key.Substring(0, key.Length - suffix.Length);
                return (prefix, suffix);
            }
        }

        return (key, string.Empty);
    }

    private string ResolveLevelText(string suffix)
    {
        return suffix.ToUpperInvariant() switch
        {
            "OPEN" => OpenLabel,
            "HIGH" => HighLabel,
            "LOW" => LowLabel,
            "CLOSE" => CloseLabel,
            "EQ" => EquilibriumLabel,
            "POC" => PocLabel,
            "VWAP" => VwapLabel,
            "VAH" => VahLabel,
            "VAL" => ValLabel,
            _ => suffix
        };
    }

    private string GetDisplayPrefix(FixedProfilePeriods period) => period switch
    {
        FixedProfilePeriods.CurrentDay => DayPrefix,
        FixedProfilePeriods.LastDay => PrevDayPrefix,
        FixedProfilePeriods.CurrentWeek => WeekPrefix,
        FixedProfilePeriods.LastWeek => PrevWeekPrefix,
        FixedProfilePeriods.CurrentMonth => MonthPrefix,
        FixedProfilePeriods.LastMonth => PrevMonthPrefix,
        FixedProfilePeriods.Contract => ContractPrefix,
        _ => DayPrefix
    };

    #endregion


    #region OnCalculate

    private void UpdateAllNeededLevelsFromCache()
    {
        var dirty = false;

        bool UpdateIf(FixedProfilePeriods p)
        {
            if (!IsNeeded(p))
                return false;

            if (!_profileCandles.TryGetValue(p, out var candle))
                return false;

            if (candle is null)
                return false;

            return UpdateLevels(p, candle);
        }

        dirty |= UpdateIf(FixedProfilePeriods.CurrentDay);
        dirty |= UpdateIf(FixedProfilePeriods.LastDay);
        dirty |= UpdateIf(FixedProfilePeriods.CurrentWeek);
        dirty |= UpdateIf(FixedProfilePeriods.LastWeek);
        dirty |= UpdateIf(FixedProfilePeriods.CurrentMonth);
        dirty |= UpdateIf(FixedProfilePeriods.LastMonth);
        dirty |= UpdateIf(FixedProfilePeriods.Contract);

        if (dirty)
            RedrawChart();
    }

    private void RequestProfiles()
    {
        if (_needDay) RequestProfileForPeriod(FixedProfilePeriods.CurrentDay);
        if (_needPrevDay) RequestProfileForPeriod(FixedProfilePeriods.LastDay);
        if (_needWeek) RequestProfileForPeriod(FixedProfilePeriods.CurrentWeek);
        if (_needPrevWeek) RequestProfileForPeriod(FixedProfilePeriods.LastWeek);
        if (_needMonth) RequestProfileForPeriod(FixedProfilePeriods.CurrentMonth);
        if (_needPrevMonth) RequestProfileForPeriod(FixedProfilePeriods.LastMonth);
        if (_needContract) RequestProfileForPeriod(FixedProfilePeriods.Contract);
    }

    private void RequestProfileForPeriod(FixedProfilePeriods period, bool force = true)
    {
        if (!force && _profileCandles.TryGetValue(period, out var candle) && candle is not null)
        {
            RedrawChart();
            return;
        }

        GetFixedProfile(new FixedProfileRequest(period));
    }

    private void RecalcAllNeeds()
    {
        _needDay = NeedsDayData();
        _needPrevDay = NeedsPrevDayData();
        _needWeek = NeedsWeekData();
        _needPrevWeek = NeedsPrevWeekData();
        _needMonth = NeedsMonthData();
        _needPrevMonth = NeedsPrevMonthData();
        _needContract = NeedsContractData();
    }

    private void RecalcNeedFor(FixedProfilePeriods period)
    {
        switch (period)
        {
            case FixedProfilePeriods.CurrentDay:
                _needDay = NeedsDayData();
                break;
            case FixedProfilePeriods.LastDay:
                _needPrevDay = NeedsPrevDayData();
                break;
            case FixedProfilePeriods.CurrentWeek:
                _needWeek = NeedsWeekData();
                break;
            case FixedProfilePeriods.LastWeek:
                _needPrevWeek = NeedsPrevWeekData();
                break;
            case FixedProfilePeriods.CurrentMonth:
                _needMonth = NeedsMonthData();
                break;
            case FixedProfilePeriods.LastMonth:
                _needPrevMonth = NeedsPrevMonthData();
                break;
            case FixedProfilePeriods.Contract:
                _needContract = NeedsContractData();
                break;
        }
    }

    private bool NeedsDayData()
    {
        return DayOpenLevel.Enabled || DayHighLevel.Enabled || DayLowLevel.Enabled || DayCloseLevel.Enabled ||
               DayEquilibriumLevel.Enabled || DayPOCLevel.Enabled || DayVWAPLevel.Enabled || DayVAHLevel.Enabled || DayVALLevel.Enabled;
    }

    private bool NeedsPrevDayData()
    {
        return PrevDayOpenLevel.Enabled || PrevDayHighLevel.Enabled || PrevDayLowLevel.Enabled || PrevDayCloseLevel.Enabled ||
               PrevDayEquilibriumLevel.Enabled || PrevDayPOCLevel.Enabled || PrevDayVWAPLevel.Enabled || PrevDayVAHLevel.Enabled || PrevDayVALLevel.Enabled;
    }

    private bool NeedsWeekData()
    {
        return WeekOpenLevel.Enabled || WeekHighLevel.Enabled || WeekLowLevel.Enabled || WeekCloseLevel.Enabled ||
               WeekEquilibriumLevel.Enabled || WeekPOCLevel.Enabled || WeekVWAPLevel.Enabled || WeekVAHLevel.Enabled || WeekVALLevel.Enabled;
    }

    private bool NeedsPrevWeekData()
    {
        return PrevWeekOpenLevel.Enabled || PrevWeekHighLevel.Enabled || PrevWeekLowLevel.Enabled || PrevWeekCloseLevel.Enabled ||
               PrevWeekEquilibriumLevel.Enabled || PrevWeekPOCLevel.Enabled || PrevWeekVWAPLevel.Enabled || PrevWeekVAHLevel.Enabled || PrevWeekVALLevel.Enabled;
    }

    private bool NeedsMonthData()
    {
        return MonthOpenLevel.Enabled || MonthHighLevel.Enabled || MonthLowLevel.Enabled || MonthCloseLevel.Enabled ||
               MonthEquilibriumLevel.Enabled || MonthPOCLevel.Enabled || MonthVWAPLevel.Enabled || MonthVAHLevel.Enabled || MonthVALLevel.Enabled;
    }

    private bool NeedsPrevMonthData()
    {
        return PrevMonthOpenLevel.Enabled || PrevMonthHighLevel.Enabled || PrevMonthLowLevel.Enabled || PrevMonthCloseLevel.Enabled ||
               PrevMonthEquilibriumLevel.Enabled || PrevMonthPOCLevel.Enabled || PrevMonthVWAPLevel.Enabled || PrevMonthVAHLevel.Enabled || PrevMonthVALLevel.Enabled;
    }

    private bool NeedsContractData()
    {
        return ContractOpenLevel.Enabled || ContractHighLevel.Enabled || ContractLowLevel.Enabled || ContractCloseLevel.Enabled ||
               ContractEquilibriumLevel.Enabled || ContractPOCLevel.Enabled || ContractVWAPLevel.Enabled || ContractVAHLevel.Enabled || ContractVALLevel.Enabled;
    }

    private bool UpdateLevels(FixedProfilePeriods period, IndicatorCandle candle)
    {
        if (candle == null) 
            return false;

        var dirty = false;
        var keys = _keys[period];

        // OHLC + EQ
        dirty |= UpdateLevel(keys[0], candle.Open);                          // Open
        dirty |= UpdateLevel(keys[1], candle.High);                          // High
        dirty |= UpdateLevel(keys[2], candle.Low);                           // Low
        dirty |= UpdateLevel(keys[3], candle.Close);                         // Close
        dirty |= UpdateLevel(keys[4], (candle.High + candle.Low) / 2);       // EQ

        // POC
        if (candle.MaxVolumePriceInfo != null && candle.MaxVolumePriceInfo.Price > 0)
            dirty |= UpdateLevel(keys[5], candle.MaxVolumePriceInfo.Price);

        // VWAP
        if (candle.VWAP > 0)
            dirty |= UpdateLevel(keys[6], candle.VWAP);

        // VAH/VAL
        if (candle.ValueArea != null &&
            candle.ValueArea.ValueAreaHigh > 0 &&
            candle.ValueArea.ValueAreaLow > 0 &&
            candle.ValueArea.ValueAreaHigh >= candle.ValueArea.ValueAreaLow)
        {
            dirty |= UpdateLevel(keys[7], candle.ValueArea.ValueAreaHigh);
            dirty |= UpdateLevel(keys[8], candle.ValueArea.ValueAreaLow);
        }

        return dirty;
    }

    private bool UpdateLevel(string key, decimal price)
    {
        // Get previous state if it exists
        if (!_levels.TryGetValue(key, out var ld))
        {
            // New level -> always dirty
            ld = new LevelData { Label = key };
            _levels[key] = ld;

            ld.Price = price;
            ld.IsValid = true;
            return true;
        }

        // Capture previous values
        var prevPrice = ld.Price;
        var prevValid = ld.IsValid;

        // Apply new values
        ld.Price = price;
        ld.IsValid = true;

        // Detect meaningful change
        // NOTE: price is decimal, so exact compare is safe here
        var priceChanged = prevPrice != ld.Price;
        var validChanged = prevValid != ld.IsValid;

        return priceChanged || validChanged;
    }

    #endregion

    #region OnRender

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FixedProfilePeriods PeriodFromPrefix(string prefix) => prefix switch
    {
        "d" => FixedProfilePeriods.CurrentDay,
        "p" => FixedProfilePeriods.LastDay,
        "w" => FixedProfilePeriods.CurrentWeek,
        "pw" => FixedProfilePeriods.LastWeek,
        "m" => FixedProfilePeriods.CurrentMonth,
        "pm" => FixedProfilePeriods.LastMonth,
        "c" => FixedProfilePeriods.Contract,
        _ => FixedProfilePeriods.CurrentDay
    };

    private void RenderLevel(RenderContext context, string prefix, string levelKey, LevelSettings levelSettings)
    {
        if (!_allLevelsVisible || !levelSettings.Enabled || !_levels.TryGetValue(levelKey, out var level) || !level.IsValid)
            return;
            
        // Validate price is reasonable
        if (level.Price <= 0)
            return;

        var y = ChartInfo.GetYByPrice(level.Price, false);

        // Check if price is visible on chart
        var region = ChartInfo.PriceChartContainer.Region;
        if (y < region.Top || y >= region.Bottom)
            return;

        var chartWidth = ChartInfo.PriceChartContainer.Region.Width;
        var currentBarX = ChartInfo.GetXByBar(CurrentBar - 1);
        var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
        var currentBarRightX = currentBarX + barWidth;

        // Get pen from semantic
        var period = PeriodFromPrefix(prefix);          // already canonical: d/p/w/pw/m/pm/c
        var (_, suffix) = SplitKey(levelKey);

        var levelType = LevelTypeFromSuffix(suffix);
        var descriptor = new LevelDescriptor(levelType, period, DefaultFamily(levelType));

        var sem = _visualSemanticMode == VisualSemanticMode.RuleSet
            ? ResolveVisualSemantic(descriptor, levelSettings)
            : _emptySemantic;

        // Apply semantic style delta (pq02.2: color/width/dash only; no priority changes yet)
        var effColor = sem.Style.Color ?? levelSettings.Color;
        var effWidth = sem.Style.Width ?? levelSettings.Width;
        var effDash = sem.Style.Dash ?? levelSettings.LineStyle;

        var renderPen = new PenSettings
        {
            Color = effColor,
            Width = effWidth,
            LineDashStyle = effDash
        }.RenderObject;

        // Build label text
        var displayPrefix = GetDisplayPrefix(period);
        var labelText = BuildLabelText(displayPrefix, levelKey, levelSettings);

        // Decide if we should defer line drawing (only when we have a text label)
        var hasTextLabel = levelSettings.LabelPosition != LabelPosition.None;

        // Precompute line endpoints (used for immediate draw or deferred draw)
        var x1 = levelSettings.LineType switch
        {
            LineType.Full => region.Left,
            LineType.Bar => currentBarRightX,
            _ => region.Left
        };

        var x2 = region.Right;

        // Draw line immediately only when there is no label (keeps current behavior stable)
        if (!hasTextLabel)
        {
            switch (levelSettings.LineType)
            {
                case LineType.Bar:
                    context.DrawLine(renderPen, x1, y, x2, y);
                    break;
                case LineType.Full:
                    context.DrawLine(renderPen, x1, y, x2, y);
                    break;
                case LineType.None:
                    break;
            }
        }

        // Draw price label (if ShowPrice == true)
        if (levelSettings.ShowPrice)
        {
            DrawPriceLabel(context, level.Price, y, renderPen, levelSettings, effColor);
        }

        // Enqueue text label (collision-safe draw happens in FlushLabelQueue)
        if (hasTextLabel)
        {
            int xAnchor;
            bool alignRight;

            switch (levelSettings.LabelPosition)
            {
                case LabelPosition.Right:
                    xAnchor = region.Right - 5;
                    alignRight = true;
                    break;

                case LabelPosition.Left:
                    xAnchor = region.Left + 5;
                    alignRight = false;
                    break;

                case LabelPosition.Bar:
                default:
                    xAnchor = currentBarRightX + 8;
                    alignRight = false;
                    break;
            }

            // Stable priority: keep deterministic ordering across frames and enable/disable toggles.
            // Lower = more important (draw earlier).
            var idxInPeriod = Array.IndexOf(_keys[period], levelKey);
            if (idxInPeriod < 0) idxInPeriod = 99;


            // Priority composition:
            // 1) period bucket (stable)
            // 2) semantic importance (dominant inside period)
            // 3) deterministic tie-breaker inside semantic bucket
            var priority = (ResolvePeriodWeight(period) * 1000) + (ResolveLevelWeight(suffix) * 10) + idxInPeriod;
            var isBarLabel = levelSettings.LabelPosition == LabelPosition.Bar;

            _labelQueue.Add(new LabelDrawRequest(
                text: labelText,
                xAnchor: xAnchor,
                yAnchor: y,
                borderPen: renderPen,
                alignRight: alignRight,

                // Deferred line: we skipped drawing it when hasTextLabel == true,
                // so FlushLabelQueue must draw it with cropping against the final label rect.
                lineType: levelSettings.LineType,
                lineX1: x1,
                lineX2: x2,
                lineY: y,

                priority: priority,
                sequence: _labelQueue.Count,
                isBar: isBarLabel
            ));
        }

    }

    private void DrawPriceLabel(RenderContext context, decimal price, int y, RenderPen pen, LevelSettings levelSettings, CrossColor effectiveColor)
    {
        var priceText = string.Format(ChartInfo.StringFormat, price);

        // Calculate contrasting text color based on background color
        var backgroundColor = effectiveColor;
        var textColor = GetContrastingColor(backgroundColor);
        
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

    private void DrawTextLabelAtRect(RenderContext context, LabelDrawRequest req, Rectangle rect, Size size)
    {
        var backgroundColor = ChartInfo.ColorsStore.BaseBackgroundColor;
        var textColor = GetContrastingColor(backgroundColor.Convert());

        // Draw background with border
        context.FillRectangle(backgroundColor, rect);
        context.DrawRectangle(req.BorderPen, rect);

        // Text rect inside the padded rect
        var textRect = new Rectangle(rect.X + 2, rect.Y + 1, size.Width, size.Height);
        var format = req.AlignRight ? _stringRightFormat : _stringLeftFormat;
        context.DrawString(req.Text, _font, textColor, textRect, format);
    }


    private void RenderLevelGroup(RenderContext context, string prefix,
      LevelSettings openLevel, LevelSettings highLevel, LevelSettings lowLevel, LevelSettings closeLevel,
      LevelSettings eqLevel, LevelSettings pocLevel, LevelSettings vwapLevel, LevelSettings vahLevel, LevelSettings valLevel)
    {
        var keys = _keys[PeriodFromPrefix(prefix)];
        // 0 Open, 1 High, 2 Low, 3 Close, 4 EQ, 5 POC, 6 VWAP, 7 VAH, 8 VAL

        RenderLevel(context, prefix, keys[0], openLevel);
        RenderLevel(context, prefix, keys[1], highLevel);
        RenderLevel(context, prefix, keys[2], lowLevel);
        RenderLevel(context, prefix, keys[3], closeLevel);
        RenderLevel(context, prefix, keys[4], eqLevel);
        RenderLevel(context, prefix, keys[5], pocLevel);
        RenderLevel(context, prefix, keys[6], vwapLevel);
        RenderLevel(context, prefix, keys[7], vahLevel);
        RenderLevel(context, prefix, keys[8], valLevel);
    }

    private void BeginLabelLayoutFrame()
    {
        _labelQueue.Clear();
        _occupiedLabelRects.Clear();
    }

    private void FlushLabelQueue(RenderContext context)
    {
        if (_labelQueue.Count == 0)
            return;

        // Stable order: priority then insertion order (keeps behavior predictable in pq01)
        _labelQueue.Sort(static (a, b) =>
        {
            var cmp = a.Priority.CompareTo(b.Priority);
            return cmp != 0 ? cmp : a.Sequence.CompareTo(b.Sequence);
        });

        var placed = new List<(LabelDrawRequest Req, Rectangle Rect, Size Size)>(_labelQueue.Count);

        foreach (var req in _labelQueue)
        {
            var size = context.MeasureString(req.Text, _font);
            var rect = BuildLabelRect(req.XAnchor, req.YAnchor, size, req.AlignRight, req.IsBar);
            rect = ResolveLabelPlacement(req, rect);

            placed.Add((req, rect, size));
            _occupiedLabelRects.Add(rect);
        }

        foreach (var item in placed)
            DrawTextLabelAtRect(context, item.Req, item.Rect, item.Size);

        foreach (var item in placed)
        {
            if (item.Req.LineType != LineType.None)
                DrawDeferredLineClippedAgainstAll(context, item.Req, item.Rect, _occupiedLabelRects);
        }
    }

    private void DrawDeferredLineClippedAgainstAll(
    RenderContext context,
    LabelDrawRequest req,
    Rectangle ownRect,
    List<Rectangle> allRects)
    {
        var x1 = req.LineX1;
        var x2 = req.LineX2;
        var y = req.LineY;

        // For Bar lines, trim the start so the line doesn't "stick out" under the label.
        // We prefer trimming the line rather than moving the label rect.
        if (req.LineType == LineType.Bar)
        {
            // Ensure the line starts at (or after) the label's left edge (minus padding),
            // removing the small "stub" between the bar edge and the label.
            var startTrim = ownRect.Left - (_labelPadding + 1);

            if (startTrim > x1)
                x1 = startTrim;
        }

        if (x2 <= x1)
            return;

        // Safety padding: include pen width + 1px to avoid �overpassing� artifacts
        var pad = _labelPadding + 1;

        // Collect �holes� on the line caused by labels
        var holes = new List<(int L, int R)>(8);

        for (var i = 0; i < allRects.Count; i++)
        {
            var r = allRects[i];

            // Does this label cover the line y?
            if (y < r.Top || y >= r.Bottom)
                continue;

            var l = r.Left - pad;
            var rr = r.Right + pad;

            // Intersect with line span
            if (rr <= x1 || l >= x2)
                continue;

            holes.Add((Math.Max(x1, l), Math.Min(x2, rr)));
        }

        if (holes.Count == 0)
        {
            context.DrawLine(req.BorderPen, x1, y, x2, y);
            return;
        }

        // Merge overlapping holes
        holes.Sort((a, b) => a.L.CompareTo(b.L));

        var merged = new List<(int L, int R)>(holes.Count);
        var cur = holes[0];

        for (var i = 1; i < holes.Count; i++)
        {
            var h = holes[i];
            if (h.L <= cur.R)
                cur = (cur.L, Math.Max(cur.R, h.R));
            else
            {
                merged.Add(cur);
                cur = h;
            }
        }
        merged.Add(cur);

        // Draw remaining segments between holes
        var start = x1;

        foreach (var h in merged)
        {
            if (h.L > start)
                context.DrawLine(req.BorderPen, start, y, h.L, y);

            start = Math.Max(start, h.R);
        }

        if (start < x2)
            context.DrawLine(req.BorderPen, start, y, x2, y);
    }


    private Rectangle BuildLabelRect(int xAnchor, int yAnchor, Size size, bool alignRight, bool isBar)
    {
        var rectX = alignRight ? xAnchor - size.Width : xAnchor;

        // Keep the �nice border breathing room� for Left/Right,
        // but do not shift Bar labels to the left (it looks misaligned with the line start).
        var leftPad = isBar ? 0 : 2;

        return new Rectangle(
            rectX - leftPad,
            yAnchor - size.Height / 2 - 1,
            size.Width + 4,
            size.Height + 2);
    }

    private Rectangle ResolveLabelPlacement(LabelDrawRequest req, Rectangle desired)
    {
        var container = ChartInfo.PriceChartContainer.Region;

        // First try: desired position (ideal)
        desired = ClampToContainer(desired, container);
        if (!IntersectsAny(desired))
            return desired;

        // Horizontal corridor bounds depend on label alignment/anchor and chart width
        var sizeW = desired.Width;
        int minX, maxX, dir;
        var safeLeft = container.Left + 4;
        var safeRight = container.Right - 4;

        if (req.AlignRight)
        {
            // Right anchored: push left
            var rightEdge = req.XAnchor;
            maxX = rightEdge - sizeW;                 // closest to axis
            minX = maxX - _labelHMaxShift;            // allow shift left
            dir = -1;
        }
        else
        {
            // Left / Bar: push right
            minX = desired.X;
            maxX = minX + _labelHMaxShift;
            dir = +1;
        }

        // Clamp corridor within container
        minX = Math.Max(safeLeft, minX);
        maxX = Math.Min(safeRight - sizeW, maxX);
        if (minX > maxX) minX = maxX;

        // Probe horizontally
        for (var step = 1; step * _labelHProbeStep <= _labelHMaxShift; step++)
        {
            var dx = step * _labelHProbeStep * dir;

            var probe = desired;
            probe.X = Clamp(probe.X + dx, minX, maxX);

            if (!IntersectsAny(probe))
                return probe;
        }

        // Fallback: vertical probing around the original Y (keep X = best effort)
        var original = desired;

        for (var step = 1; step <= _labelVProbeMaxSteps; step++)
        {
            var dy = step * _labelVProbeStep;

            var up = original;
            up.Y = ClampToContainerY(up.Y - dy, up.Height, container);
            if (!IntersectsAny(up))
                return up;

            var down = original;
            down.Y = ClampToContainerY(down.Y + dy, down.Height, container);
            if (!IntersectsAny(down))
                return down;
        }

        return original;
    }

    private static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);

    private static int ClampToContainerY(int y, int h, Rectangle container)
    {
        if (y < container.Top) return container.Top;
        if (y + h > container.Bottom) return container.Bottom - h;
        return y;
    }


    private bool IntersectsAny(Rectangle rect)
    {
        for (var i = 0; i < _occupiedLabelRects.Count; i++)
        {
            if (rect.IntersectsWith(_occupiedLabelRects[i]))
                return true;
        }
        return false;
    }

    private static Rectangle ClampToContainer(Rectangle rect, Rectangle container)
    {
        if (rect.Top < container.Top)
            rect.Y = container.Top;

        if (rect.Bottom > container.Bottom)
            rect.Y = container.Bottom - rect.Height;

        if (rect.Left < container.Left)
            rect.X = container.Left;

        if (rect.Right > container.Right)
            rect.X = container.Right - rect.Width;

        return rect;
    }

    // Semantic priority: lower number = higher importance (draw first, wins collisions).
    private static int ResolveLevelWeight(string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
            return 99;

        // Key levels first, then OHLC, then secondary (EQ).
        return suffix.ToUpperInvariant() switch
        {
            "POC" => 0,
            "VWAP" => 1,
            "VAH" => 2,
            "VAL" => 3,


            "HIGH" => 4,
            "LOW" => 5,
            "OPEN" => 6,
            "CLOSE" => 7,

            "EQ" => 8,

            _ => 50
        };
    }

    private static int ResolvePeriodWeight(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => 0,
            FixedProfilePeriods.LastDay => 10,
            FixedProfilePeriods.CurrentWeek => 20,
            FixedProfilePeriods.LastWeek => 30,
            FixedProfilePeriods.CurrentMonth => 40,
            FixedProfilePeriods.LastMonth => 50,
            FixedProfilePeriods.Contract => 60,
            _ => 100
        };
    }

    private ResolvedVisualSemantic ResolveVisualSemantic(LevelDescriptor d, LevelSettings baseSettings)
    {
        EnsureVisualRuleSet();

        // No semantic priority yet in pq02.2
        const int weight = 0;

        if (_visualRuleSet.Rules.Count == 0)
            return new ResolvedVisualSemantic(new VisualStyleDelta(), weight);

        // First match wins (rules are already ordered by preset builder).
        foreach (var r in _visualRuleSet.Rules)
        {
            if (r.Period.HasValue && r.Period.Value != d.Period)
                continue;

            if (r.LevelType.HasValue && r.LevelType.Value != d.Type)
                continue;

            return new ResolvedVisualSemantic(r.Style, weight);
        }

        return new ResolvedVisualSemantic(new VisualStyleDelta(), weight);
    }

    private void EnsureVisualRuleSet()
    {
        if (!_visualRuleSetDirty)
            return;

        _visualRuleSet = _visualSemanticPreset switch
        {
            VisualSemanticPresetKind.ByPeriod => BuildRuleSetByPeriod(),
            VisualSemanticPresetKind.ByLevelType => BuildRuleSetByLevelType(),
            _ => VisualRuleSet.Empty
        };

        _visualRuleSetDirty = false;
    }

    private CrossColor GetPeriodPaletteColor(FixedProfilePeriods period) => period switch
    {
        FixedProfilePeriods.CurrentDay => PeriodColorCurrentDay,
        FixedProfilePeriods.LastDay => PeriodColorPreviousDay,
        FixedProfilePeriods.CurrentWeek => PeriodColorCurrentWeek,
        FixedProfilePeriods.LastWeek => PeriodColorPreviousWeek,
        FixedProfilePeriods.CurrentMonth => PeriodColorCurrentMonth,
        FixedProfilePeriods.LastMonth => PeriodColorPreviousMonth,
        FixedProfilePeriods.Contract => PeriodColorContract,
        _ => PeriodColorCurrentDay
    };

    private CrossColor GetLevelPaletteColor(LevelType levelType) => levelType switch
    {
        LevelType.Open => LevelColorOpen,
        LevelType.High => LevelColorHigh,
        LevelType.Low => LevelColorLow,
        LevelType.Close => LevelColorClose,
        LevelType.EQ => LevelColorEQ,
        LevelType.POC => LevelColorPOC,
        LevelType.VWAP => LevelColorVWAP,
        LevelType.VAH => LevelColorVAH,
        LevelType.VAL => LevelColorVAL,
        _ => LevelColorOpen
    };


    private VisualRuleSet BuildRuleSetByPeriod()
        {
            // Period semantics: temporal hierarchy (today > prev day > week > month > contract).
            // Color comes from the period palette; width/dash encodes "age" for fast reading.
            var rules = new List<VisualRule>
            {
                new (FixedProfilePeriods.CurrentDay, null,
                    new VisualStyleDelta
                    {
                        Color = GetPeriodPaletteColor(FixedProfilePeriods.CurrentDay),
                        Width = 2,
                        Dash = LineDashStyle.Solid
                    }),

                new (FixedProfilePeriods.LastDay, null,
                    new VisualStyleDelta
                    {
                        Color = GetPeriodPaletteColor(FixedProfilePeriods.LastDay),
                        Width = 1,
                        Dash = LineDashStyle.Solid
                    }),

                new(FixedProfilePeriods.CurrentWeek, null,
                new VisualStyleDelta
                    {
                    Color = GetPeriodPaletteColor(FixedProfilePeriods.CurrentWeek),
                    Width = 1,
                    Dash = LineDashStyle.Dash
                    }),

                new(FixedProfilePeriods.LastWeek, null,
                new VisualStyleDelta
                    {
                    Color = GetPeriodPaletteColor(FixedProfilePeriods.LastWeek),
                    Width = 1,
                    Dash = LineDashStyle.Dot
                    }),

                new(FixedProfilePeriods.CurrentMonth, null,
                new VisualStyleDelta
                    {
                    Color = GetPeriodPaletteColor(FixedProfilePeriods.CurrentMonth),
                    Width = 1,
                    Dash = LineDashStyle.DashDot
                    }),

                new(FixedProfilePeriods.LastMonth, null,
                new VisualStyleDelta
                    {
                    Color = GetPeriodPaletteColor(FixedProfilePeriods.LastMonth),
                    Width = 1,
                    Dash = LineDashStyle.DashDot
                    }),

                new(FixedProfilePeriods.Contract, null,
                new VisualStyleDelta
                    {
                    Color = GetPeriodPaletteColor(FixedProfilePeriods.Contract),
                    Width = 1,
                    Dash = LineDashStyle.Dot
                    })
            };

            return new VisualRuleSet(rules);
        }


    private VisualRuleSet BuildRuleSetByLevelType()
    {
        // Level hierarchy for intraday scalping:
        // Anchors dominate (POC/VWAP), Value Area is secondary (VAH/VAL), extremes are simple (High/Low),
        // session references are tertiary (Open/Close/EQ).
        var rules = new List<VisualRule>
    {
        // Anchors
        new(null, LevelType.POC,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.POC),
                Width = 3,
                Dash = LineDashStyle.Solid
            }),

        new(null, LevelType.VWAP,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.VWAP),
                Width = 2,
                Dash = LineDashStyle.Solid
            }),

        // Value Area (dotted to avoid competing with anchors)
        new(null, LevelType.VAH,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.VAH),
                Width = 1,
                Dash = LineDashStyle.Dot
            }),

        new(null, LevelType.VAL,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.VAL),
                Width = 1,
                Dash = LineDashStyle.Dot
            }),

        // Extremes (solid, thin)
        new(null, LevelType.High,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.High),
                Width = 1,
                Dash = LineDashStyle.Solid
            }),

        new(null, LevelType.Low,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.Low),
                Width = 1,
                Dash = LineDashStyle.Solid
            }),

        // Session references (dashed, subtle)
        new(null, LevelType.Open,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.Open),
                Width = 1,
                Dash = LineDashStyle.Dash
            }),

        new(null, LevelType.Close,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.Close),
                Width = 1,
                Dash = LineDashStyle.Dash
            }),

        // Equilibrium (informational)
        new(null, LevelType.EQ,
            new VisualStyleDelta
            {
                Color = GetLevelPaletteColor(LevelType.EQ),
                Width = 1,
                Dash = LineDashStyle.Dash
            })
    };

        return new VisualRuleSet(rules);
    }



    #endregion

    #region SubscribeAllLevels

    private static bool TryParsePeriodFromPropertyName(string propertyName, out FixedProfilePeriods period)
    {
        if (propertyName.StartsWith("Day")) { period = FixedProfilePeriods.CurrentDay; return true; }
        if (propertyName.StartsWith("PrevDay")) { period = FixedProfilePeriods.LastDay; return true; }
        if (propertyName.StartsWith("Week")) { period = FixedProfilePeriods.CurrentWeek; return true; }
        if (propertyName.StartsWith("PrevWeek")) { period = FixedProfilePeriods.LastWeek; return true; }
        if (propertyName.StartsWith("Month")) { period = FixedProfilePeriods.CurrentMonth; return true; }
        if (propertyName.StartsWith("PrevMonth")) { period = FixedProfilePeriods.LastMonth; return true; }
        if (propertyName.StartsWith("Contract")) { period = FixedProfilePeriods.Contract; return true; }

        period = default;
        return false;
    }

    private void SubscribeAllLevels()
    {
        foreach (var (ls, period) in EnumerateAllLevelSettingsWithPeriods())
            TrySubscribe(ls, period);
    }

    private IEnumerable<(LevelSettings ls, FixedProfilePeriods period)> EnumerateAllLevelSettingsWithPeriods()
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
        foreach (var pi in GetType().GetProperties(flags))
        {
            if (pi.PropertyType != typeof(LevelSettings) || !pi.CanRead)
                continue;

            if (pi.GetValue(this) is LevelSettings ls && TryParsePeriodFromPropertyName(pi.Name, out var period))
                yield return (ls, period);
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

        if (e.PropertyName == nameof(LevelSettings.Enabled))
        {
            if (_periodByLevel.TryGetValue(ls, out var period))
            {
                RecalcNeedFor(period);

                if (ls.Enabled && IsNeeded(period))
                    RequestProfileForPeriod(period, force: false);
                else
                    RedrawChart();
            }
        }
    }

    private bool IsNeeded(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => _needDay,
            FixedProfilePeriods.LastDay => _needPrevDay,
            FixedProfilePeriods.CurrentWeek => _needWeek,
            FixedProfilePeriods.LastWeek => _needPrevWeek,
            FixedProfilePeriods.CurrentMonth => _needMonth,
            FixedProfilePeriods.LastMonth => _needPrevMonth,
            FixedProfilePeriods.Contract => _needContract,
            _ => false
        };
    }

    #endregion

    #endregion
}