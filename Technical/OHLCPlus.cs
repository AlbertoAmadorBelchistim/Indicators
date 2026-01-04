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

#if STABLE

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.TillBar))]

#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.TillBar))]
#endif
    Bar = 1,

#if STABLE

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.FullWidth))]

#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FullWidth))]
#endif
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
    private bool _overrideColorInSchemes;
    private bool _overrideWidthInSchemes;
    private bool _overrideStyleInSchemes;

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

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.OverrideLabel))]
    public string OverrideLabel
    {
        get => _overrideLabel;
        set => SetField(ref _overrideLabel, value);
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.OverrideColorInSchemes),
        Description = nameof(Resources.OverrideColorInSchemesDescription))]
    public bool OverrideColorInSchemes
    {
        get => _overrideColorInSchemes;
        set => SetField(ref _overrideColorInSchemes, value);
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.OverrideWidthInSchemes),
        Description = nameof(Resources.OverrideWidthInSchemesDescription))]
    public bool OverrideWidthInSchemes
    {
        get => _overrideWidthInSchemes;
        set => SetField(ref _overrideWidthInSchemes, value);
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.OverrideStyleInSchemes),
        Description = nameof(Resources.OverrideStyleInSchemesDescription))]
    public bool OverrideStyleInSchemes
    {
        get => _overrideStyleInSchemes;
        set => SetField(ref _overrideStyleInSchemes, value);
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
        LineType lineType = LineType.Bar,
        bool overrideColorInSchemes = false
    )
    {
        Enabled = enabled;
        Color = color == default ? System.Drawing.Color.Blue.Convert() : color;
        Width = width;
        LineStyle = lineStyle;
        ShowPrice = showPrice;
        LabelPosition = labelPosition;
        LineType = lineType;
        OverrideColorInSchemes = overrideColorInSchemes;
    }

    #endregion
}

[DisplayName("OHLC Plus")]
[Category(IndicatorCategories.VolumeOrderFlow)]
#if STABLE

[Display(ResourceType = typeof(Resources), Description = nameof(Resources.OHLCPlusDescription))]

#else

[Display(ResourceType = typeof(Strings), Description = nameof(Strings.OHLCPlusDescription))]

#endif
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

        // Lower = drawn first (more “important” in pq01 = preserve current order)
        public int Priority { get; }

        public int Sequence { get; }

        public bool IsBar { get; }
    }

    #region Visual Semantic (pq02)

    public enum VisualSemanticMode
    {
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.VisualMode_Legacy), Description = nameof(Resources.VisualMode_Legacy_Description))]
        Legacy = 0,
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.VisualMode_Ruleset), Description = nameof(Resources.VisualMode_Ruleset_Description))]
        RuleSet = 1
    }

    public enum VisualSemanticPresetKind
    {
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ByPeriod))]
        ByPeriod = 0,

        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ByLevelType))]
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

    #region HVN / LVN
    private readonly Dictionary<FixedProfilePeriods, List<Band>> _hvnBands = new();
    private readonly FixedProfilePeriods[] _vnPriorityOrder =
    {
    FixedProfilePeriods.CurrentDay,
    FixedProfilePeriods.LastDay,
    FixedProfilePeriods.CurrentWeek,
    FixedProfilePeriods.LastWeek,
    FixedProfilePeriods.CurrentMonth,
    FixedProfilePeriods.LastMonth,
    FixedProfilePeriods.Contract
    };

    private sealed class Band
    {
        public decimal Low { get; init; }
        public decimal High { get; init; }
    }

    private readonly Dictionary<FixedProfilePeriods, List<Band>> _lvnBands = new();

    // HVN/LVN toggles (per fixed-profile period)
    private bool _dayHVNEnabled;
    private bool _dayLVNEnabled;

    private bool _prevDayHVNEnabled;
    private bool _prevDayLVNEnabled;

    private bool _weekHVNEnabled;
    private bool _weekLVNEnabled;

    private bool _prevWeekHVNEnabled;
    private bool _prevWeekLVNEnabled;

    private bool _monthHVNEnabled;
    private bool _monthLVNEnabled;

    private bool _prevMonthHVNEnabled;
    private bool _prevMonthLVNEnabled;

    private bool _contractHVNEnabled;
    private bool _contractLVNEnabled;

    // Per-period colors
    private CrossColor _dayHVNColor = Color.FromArgb(50, 245, 245, 245).Convert();
    private CrossColor _prevDayHVNColor = Color.FromArgb(55, 120, 120, 120).Convert();
    private CrossColor _weekHVNColor = Color.FromArgb(55, 0, 191, 255).Convert();
    private CrossColor _prevWeekHVNColor = Color.FromArgb(45, 70, 130, 180).Convert();
    private CrossColor _monthHVNColor = Color.FromArgb(45, 60, 179, 113).Convert();
    private CrossColor _prevMonthHVNColor = Color.FromArgb(45, 85, 107, 47).Convert();
    private CrossColor _contractHVNColor = Color.FromArgb(45, 85, 107, 47).Convert();

    private CrossColor _dayLVNColor = Color.FromArgb(18, 245, 245, 245).Convert();
    private CrossColor _prevDayLVNColor = Color.FromArgb(16, 120, 120, 120).Convert();
    private CrossColor _weekLVNColor = Color.FromArgb(18, 0, 191, 255).Convert();
    private CrossColor _prevWeekLVNColor = Color.FromArgb(14, 70, 130, 180).Convert();
    private CrossColor _monthLVNColor = Color.FromArgb(14, 60, 179, 113).Convert();
    private CrossColor _prevMonthLVNColor = Color.FromArgb(12, 85, 107, 47).Convert();
    private CrossColor _contractLVNColor = Color.FromArgb(12, 85, 107, 47).Convert();

    // LVN: very transparent fill + emphasized borders
    private int _lvnFillAlphaOverride = -1;     // -1 = use LVN color alpha; otherwise force this alpha (0..255)
    private int _lvnBorderAlpha = 130;          // border opacity (higher = more visible)
    private int _lvnBorderWidth = 2;       // emphasized boundaries

    // HVN: keep as-is or slightly emphasize
    private int _hvnBorderAlpha = 80;
    private int _hvnBorderWidth = 1;

    // HVN calculation parameters (you already declared these fields but you are NOT using them)
    private decimal _hvnThresholdPct = 60m;
    private int _hvnGapToleranceTicks = 1;
    private int _hvnOcclusionTicks = 2;

    // LVN calculation parameters
    private decimal _lvnThresholdPct = 20m;
    private int _lvnGapToleranceTicks = 1;
    private int _lvnOcclusionTicks = 2;

    private decimal _minPocVolForLVN = 500m;
    private int _lvnTailFilterMinTicks = 3;

    #endregion

    #endregion

    #region Properties

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic), Name = nameof(Resources.VisualSemanticMode), Order = 1)]
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

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic), Name = nameof(Resources.VisualSemanticPreset), Order = 2)]
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

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.BarOpen), Order = 10)]

#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarOpen), Order = 10)]

#endif


    public LevelSettings DayOpenLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.BarHigh), Order = 20)]

#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarHigh), Order = 20)]

#endif
    public LevelSettings DayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );
#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.BarLow), Order = 30)]

#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarLow), Order = 30)]

#endif
    public LevelSettings DayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings DayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings DayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );
#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings DayPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentDay), Order = 65)]
#endif
    public LevelSettings DayVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.VAH), Order = 70)]
#endif

    public LevelSettings DayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings DayVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool DayHVNEnabled
    {
        get => _dayHVNEnabled;
        set
        {
            if (_dayHVNEnabled == value) return;
            _dayHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor DayHVNColor
    {
        get => _dayHVNColor;
        set
        {
            if (_dayHVNColor.Equals(value)) return;
            _dayHVNColor = value;
            RedrawChart(); // color does not require profile recompute, only repaint
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool DayLVNEnabled
    {
        get => _dayLVNEnabled;
        set
        {
            if (_dayLVNEnabled == value) return;
            _dayLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentDay), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor DayLVNColor
    {
        get => _dayLVNColor;
        set
        {
            if (_dayLVNColor.Equals(value)) return;
            _dayLVNColor = value;
            RedrawChart();
        }
    }

    #endregion

    #region Prev.Day Settings

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.BarOpen), Order = 10)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarOpen), Order = 10)]
#endif
    public LevelSettings PrevDayOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.BarHigh), Order = 20)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarHigh), Order = 20)]
#endif
    public LevelSettings PrevDayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.BarLow), Order = 30)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarLow), Order = 30)]
#endif
    public LevelSettings PrevDayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings PrevDayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings PrevDayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings PrevDayPOCLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousDay), Order = 65)]
#endif
    public LevelSettings PrevDayVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.VAH), Order = 70)]
#endif
    public LevelSettings PrevDayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings PrevDayVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool PrevDayHVNEnabled
    {
        get => _prevDayHVNEnabled;
        set
        {
            if (_prevDayHVNEnabled == value) return;
            _prevDayHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor PrevDayHVNColor
    {
        get => _prevDayHVNColor;
        set
        {
            if (_prevDayHVNColor.Equals(value)) return;
            _prevDayHVNColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool PrevDayLVNEnabled
    {
        get => _prevDayLVNEnabled;
        set
        {
            if (_prevDayLVNEnabled == value) return;
            _prevDayLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousDay), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor PrevDayLVNColor
    {
        get => _prevDayLVNColor;
        set
        {
            if (_prevDayLVNColor.Equals(value)) return;
            _prevDayLVNColor = value;
            RedrawChart();
        }
    }

    #endregion

    #region Week Settings

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.BarOpen), Order = 10)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarOpen), Order = 10)]
#endif
    public LevelSettings WeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.BarHigh), Order = 20)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarHigh), Order = 20)]
#endif
    public LevelSettings WeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.BarLow), Order = 30)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarLow), Order = 30)]
#endif
    public LevelSettings WeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings WeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings WeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings WeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentWeek), Order = 65)]
#endif

    public LevelSettings WeekVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.VAH), Order = 70)]
#endif
    public LevelSettings WeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings WeekVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool WeekHVNEnabled
    {
        get => _weekHVNEnabled;
        set
        {
            if (_weekHVNEnabled == value) return;
            _weekHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor WeekHVNColor
    {
        get => _weekHVNColor;
        set
        {
            if (_weekHVNColor.Equals(value)) return;
            _weekHVNColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool WeekLVNEnabled
    {
        get => _weekLVNEnabled;
        set
        {
            if (_weekLVNEnabled == value) return;
            _weekLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentWeek), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor WeekLVNColor
    {
        get => _weekLVNColor;
        set
        {
            if (_weekLVNColor.Equals(value)) return;
            _weekLVNColor = value;
            RedrawChart();
        }
    }

    #endregion

    #region Prev.Week Settings

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.BarOpen), Order = 10)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarOpen), Order = 10)]
#endif
    public LevelSettings PrevWeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.BarHigh), Order = 20)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarHigh), Order = 20)]
#endif
    public LevelSettings PrevWeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.BarLow), Order = 30)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarLow), Order = 30)]
#endif
    public LevelSettings PrevWeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings PrevWeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings PrevWeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings PrevWeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousWeek), Order = 65)]
#endif
    public LevelSettings PrevWeekVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.VAH), Order = 70)]
#endif
    public LevelSettings PrevWeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings PrevWeekVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool PrevWeekHVNEnabled
    {
        get => _prevWeekHVNEnabled;
        set
        {
            if (_prevWeekHVNEnabled == value) return;
            _prevWeekHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor PrevWeekHVNColor
    {
        get => _prevWeekHVNColor;
        set
        {
            if (_prevWeekHVNColor.Equals(value)) return;
            _prevWeekHVNColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool PrevWeekLVNEnabled
    {
        get => _prevWeekLVNEnabled;
        set
        {
            if (_prevWeekLVNEnabled == value) return;
            _prevWeekLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousWeek), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor PrevWeekLVNColor
    {
        get => _prevWeekLVNColor;
        set
        {
            if (_prevWeekLVNColor.Equals(value)) return;
            _prevWeekLVNColor = value;
            RedrawChart();
        }
    }

    #endregion

    #region Month Settings

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.BarOpen), Order = 10)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarOpen), Order = 10)]
#endif
    public LevelSettings MonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.BarHigh), Order = 20)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarHigh), Order = 20)]
#endif
    public LevelSettings MonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.BarLow), Order = 30)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarLow), Order = 30)]
#endif
    public LevelSettings MonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings MonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings MonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings MonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentMonth), Order = 65)]
#endif
    public LevelSettings MonthVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.VAH), Order = 70)]
#endif
    public LevelSettings MonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings MonthVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool MonthHVNEnabled
    {
        get => _monthHVNEnabled;
        set
        {
            if (_monthHVNEnabled == value) return;
            _monthHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor MonthHVNColor
    {
        get => _monthHVNColor;
        set
        {
            if (_monthHVNColor.Equals(value)) return;
            _monthHVNColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool MonthLVNEnabled
    {
        get => _monthLVNEnabled;
        set
        {
            if (_monthLVNEnabled == value) return;
            _monthLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.CurrentMonth), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor MonthLVNColor
    {
        get => _monthLVNColor;
        set
        {
            if (_monthLVNColor.Equals(value)) return;
            _monthLVNColor = value;
            RedrawChart();
        }
    }

    #endregion

    #region Prev.Month Settings

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.BarOpen), Order = 10)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarOpen), Order = 10)]
#endif
    public LevelSettings PrevMonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.BarHigh), Order = 20)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarHigh), Order = 20)]
#endif
    public LevelSettings PrevMonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.BarLow), Order = 30)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarLow), Order = 30)]
#endif
    public LevelSettings PrevMonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings PrevMonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings PrevMonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings PrevMonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousMonth), Order = 65)]
#endif
    public LevelSettings PrevMonthVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.VAH), Order = 70)]
#endif
    public LevelSettings PrevMonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings PrevMonthVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool PrevMonthHVNEnabled
    {
        get => _prevMonthHVNEnabled;
        set
        {
            if (_prevMonthHVNEnabled == value) return;
            _prevMonthHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor PrevMonthHVNColor
    {
        get => _prevMonthHVNColor;
        set
        {
            if (_prevMonthHVNColor.Equals(value)) return;
            _prevMonthHVNColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool PrevMonthLVNEnabled
    {
        get => _prevMonthLVNEnabled;
        set
        {
            if (_prevMonthLVNEnabled == value) return;
            _prevMonthLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.PreviousMonth), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor PrevMonthLVNColor
    {
        get => _prevMonthLVNColor;
        set
        {
            if (_prevMonthLVNColor.Equals(value)) return;
            _prevMonthLVNColor = value;
            RedrawChart();
        }
    }

    #endregion

    #region Contract Settings

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.BarOpen), Order = 10)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarOpen), Order = 10)]
#endif
    public LevelSettings ContractOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.BarHigh), Order = 20)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarHigh), Order = 20)]
#endif
    public LevelSettings ContractHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.BarLow), Order = 30)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarLow), Order = 30)]
#endif
    public LevelSettings ContractLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.BarClose), Order = 40)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarClose), Order = 40)]
#endif
    public LevelSettings ContractCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.Equilibrium), Order = 50)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.Equilibrium), Order = 50)]
#endif
    public LevelSettings ContractEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.POC), Order = 60)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.POC), Order = 60)]
#endif
    public LevelSettings ContractPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.VWAP), Order = 65)]
#else

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.Contract), Order = 65)]
#endif
    public LevelSettings ContractVWAPLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.SteelBlue.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.VAH), Order = 70)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.VAH), Order = 70)]
#endif
    public LevelSettings ContractVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

#if STABLE

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.VAL), Order = 80)]
#else

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.VAL), Order = 80)]
#endif
    public LevelSettings ContractVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.HVN_Enabled), Order = 90)]
    public bool ContractHVNEnabled
    {
        get => _contractHVNEnabled;
        set
        {
            if (_contractHVNEnabled == value) return;
            _contractHVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.HVN_Color), Order = 95)]
    public CrossColor ContractHVNColor
    {
        get => _contractHVNColor;
        set
        {
            if (_contractHVNColor.Equals(value)) return;
            _contractHVNColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.LVN_Enabled), Order = 96)]
    public bool ContractLVNEnabled
    {
        get => _contractLVNEnabled;
        set
        {
            if (_contractLVNEnabled == value) return;
            _contractLVNEnabled = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.Contract), Name = nameof(Resources.LVN_Color), Order = 97)]
    public CrossColor ContractLVNColor
    {
        get => _contractLVNColor;
        set
        {
            if (_contractLVNColor.Equals(value)) return;
            _contractLVNColor = value;
            RedrawChart();
        }
    }


    #endregion

    #region HVN

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.HVN_Group), Name = nameof(Resources.HVN_ThresholdPct), Description = nameof(Resources.HVN_ThresholdPct_Description), Order = 10)]
    [Range(0, 100)]
    public decimal HVNThresholdPct
    {
        get => _hvnThresholdPct;
        set
        {
            if (_hvnThresholdPct == value) return;
            _hvnThresholdPct = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.HVN_Group), Name = nameof(Resources.HVN_GapToleranceTicks), Description = nameof(Resources.HVN_GapToleranceTicks_Description), Order = 20)]
    [Range(0, 100)]
    public int HVNGapToleranceTicks
    {
        get => _hvnGapToleranceTicks;
        set
        {
            if (_hvnGapToleranceTicks == value) return;
            _hvnGapToleranceTicks = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.HVN_Group), Name = nameof(Resources.HVN_OcclusionTicks), Description = nameof(Resources.HVN_OcclusionTicks_Description), Order = 30)]
    [Range(0, 100)]
    public int HVNOcclusionTicks
    {
        get => _hvnOcclusionTicks;
        set
        {
            if (_hvnOcclusionTicks == value) return;
            _hvnOcclusionTicks = value;
            RedrawChart(); // occlusion affects rendering only; no need to recompute bands
        }
    }

    #endregion

    #region LVN

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.LVN_Group), Name = nameof(Resources.LVN_ThresholdPct), Description = nameof(Resources.LVN_ThresholdPct_Description), Order = 10)]
    [Range(0, 100)]
    public decimal LVNThresholdPct
    {
        get => _lvnThresholdPct;
        set
        {
            if (_lvnThresholdPct == value) return;
            _lvnThresholdPct = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.LVN_Group), Name = nameof(Resources.LVN_GapToleranceTicks), Description = nameof(Resources.LVN_GapToleranceTicks_Description), Order = 20)]
    [Range(0, 100)]
    public int LVNGapToleranceTicks
    {
        get => _lvnGapToleranceTicks;
        set
        {
            if (_lvnGapToleranceTicks == value) return;
            _lvnGapToleranceTicks = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.LVN_Group), Name = nameof(Resources.LVN_OcclusionTicks), Description = nameof(Resources.LVN_OcclusionTicks_Description), Order = 30)]
    [Range(0, 100)]
    public int LVNOcclusionTicks
    {
        get => _lvnOcclusionTicks;
        set
        {
            if (_lvnOcclusionTicks == value) return;
            _lvnOcclusionTicks = value;
            RedrawChart(); // occlusion affects rendering only
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.LVN_Group), Name = nameof(Resources.LVN_MinPocVol), Description = nameof(Resources.LVN_MinPocVol_Description), Order = 40)]
    [Range(1, 100000)]
    public decimal MinPocVolForLVN
    {
        get => _minPocVolForLVN;
        set
        {
            if (_minPocVolForLVN == value) return;
            _minPocVolForLVN = value;
            RefreshData();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.LVN_Group), Name = nameof(Resources.LVN_TailFilterMinTicks), Description = nameof(Resources.LVN_TailFilterMinTicks_Description), Order = 50)]
    [Range(0, 50)]
    public int LVNTailFilterMinTicks
    {
        get => _lvnTailFilterMinTicks;
        set
        {
            if (_lvnTailFilterMinTicks == value) return;
            _lvnTailFilterMinTicks = value;
            RefreshData();
        }
    }

    #endregion

    #region Visibility Settings

#if ATAS_ALPHA || ATAS_X
    [Display(
        ResourceType = typeof(Strings),
        GroupName = nameof(Strings.Visibility),
        Name = nameof(Strings.ToggleLevelsVisibilityHotKey),
        Order = 1000)]
#else
[Display(
    ResourceType = typeof(Resources),
    GroupName = nameof(Resources.Visibility),
    Name = nameof(Resources.ToggleLevelsVisibilityHotKey),
    Order = 1000)]
#endif
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
    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.CurrentDay), Order = 10)]
    public CrossColor PeriodColorCurrentDay { get; set; } = System.Drawing.Color.WhiteSmoke.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.PreviousDay), Order = 20)]
    public CrossColor PeriodColorPreviousDay { get; set; } = System.Drawing.Color.DarkGray.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.CurrentWeek), Order = 30)]
    public CrossColor PeriodColorCurrentWeek { get; set; } = System.Drawing.Color.DeepSkyBlue.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.PreviousWeek), Order = 40)]
    public CrossColor PeriodColorPreviousWeek { get; set; } = System.Drawing.Color.SteelBlue.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.CurrentMonth), Order = 50)]
    public CrossColor PeriodColorCurrentMonth { get; set; } = System.Drawing.Color.MediumSeaGreen.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.PreviousMonth), Order = 60)]
    public CrossColor PeriodColorPreviousMonth { get; set; } = System.Drawing.Color.DarkOliveGreen.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_PeriodPalette), Name = nameof(Resources.Contract), Order = 70)]
    public CrossColor PeriodColorContract { get; set; } = System.Drawing.Color.SaddleBrown.Convert();


    // Level-type palette (ByLevelType)
    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.BarOpen), Order = 10)]
    public CrossColor LevelColorOpen { get; set; } = System.Drawing.Color.DarkOrange.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.BarHigh), Order = 20)]
    public CrossColor LevelColorHigh { get; set; } = System.Drawing.Color.ForestGreen.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.BarLow), Order = 30)]
    public CrossColor LevelColorLow { get; set; } = System.Drawing.Color.Firebrick.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.BarClose), Order = 40)]
    public CrossColor LevelColorClose { get; set; } = System.Drawing.Color.DimGray.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.Equilibrium), Order = 50)]
    public CrossColor LevelColorEQ { get; set; } = System.Drawing.Color.Gray.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.POC), Order = 60)]
    public CrossColor LevelColorPOC { get; set; } = System.Drawing.Color.Goldenrod.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.VWAP), Order = 70)]
    public CrossColor LevelColorVWAP { get; set; } = System.Drawing.Color.DodgerBlue.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.VAH), Order = 80)]
    public CrossColor LevelColorVAH { get; set; } = System.Drawing.Color.Teal.Convert();

    [Display(ResourceType = typeof(Resources), GroupName = nameof(Resources.VisualSemantic_LevelPalette), Name = nameof(Resources.VAL), Order = 90)]
    public CrossColor LevelColorVAL { get; set; } = System.Drawing.Color.Teal.Convert();

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

        var levelsChanged = UpdateLevels(period, fixedProfileOriginScale);
        var hvnChanged = UpdateHVNs(period, fixedProfileOriginScale); // make UpdateHVNs return bool
        var lvnChanged = UpdateLVNs(period, fixedProfileOriginScale);

        if (levelsChanged || hvnChanged || lvnChanged)
            RedrawChart();
    }

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo is null || InstrumentInfo is null)
            return;

        BeginLabelLayoutFrame();

        // HVN and LVN overlay should be rendered as background (below levels/labels)
        RenderAllHVNsWithPriority(context);
        RenderAllLVNsWithPriority(context);

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

            var changed = false;
            changed |= UpdateLevels(p, candle);
            changed |= UpdateHVNs(p, candle);
            changed |= UpdateLVNs(p, candle);
            return changed;
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

    private static readonly FixedProfilePeriods[] _allPeriods =
{
    FixedProfilePeriods.CurrentDay,
    FixedProfilePeriods.LastDay,
    FixedProfilePeriods.CurrentWeek,
    FixedProfilePeriods.LastWeek,
    FixedProfilePeriods.CurrentMonth,
    FixedProfilePeriods.LastMonth,
    FixedProfilePeriods.Contract
};

    private void RequestProfiles()
    {
        foreach (var period in _allPeriods)
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

        GetFixedProfile(new FixedProfileRequest(period));
    }

    private void RecalcAllNeeds()
    {
        _needDay = NeedsData(FixedProfilePeriods.CurrentDay);
        _needPrevDay = NeedsData(FixedProfilePeriods.LastDay);
        _needWeek = NeedsData(FixedProfilePeriods.CurrentWeek);
        _needPrevWeek = NeedsData(FixedProfilePeriods.LastWeek);
        _needMonth = NeedsData(FixedProfilePeriods.CurrentMonth);
        _needPrevMonth = NeedsData(FixedProfilePeriods.LastMonth);
        _needContract = NeedsData(FixedProfilePeriods.Contract);
    }

    private void RefreshData()
    {
        RecalcAllNeeds();

        // Request profiles for periods that are now needed
        RequestProfiles();

        // If we already have cached profiles, recompute immediately
        UpdateAllNeededLevelsFromCache();

        RedrawChart();
    }

    private void RecalcNeedFor(FixedProfilePeriods period)
    {
        switch (period)
        {
            case FixedProfilePeriods.CurrentDay:
                _needDay = NeedsData(FixedProfilePeriods.CurrentDay);
                break;
            case FixedProfilePeriods.LastDay:
                _needPrevDay = NeedsData(FixedProfilePeriods.LastDay);
                break;
            case FixedProfilePeriods.CurrentWeek:
                _needWeek = NeedsData(FixedProfilePeriods.CurrentWeek);
                break;
            case FixedProfilePeriods.LastWeek:
                _needPrevWeek = NeedsData(FixedProfilePeriods.LastWeek);
                break;
            case FixedProfilePeriods.CurrentMonth:
                _needMonth = NeedsData(FixedProfilePeriods.CurrentMonth);
                break;
            case FixedProfilePeriods.LastMonth:
                _needPrevMonth = NeedsData(FixedProfilePeriods.LastMonth);
                break;
            case FixedProfilePeriods.Contract:
                _needContract = NeedsData(FixedProfilePeriods.Contract);
                break;
        }
    }

    private bool NeedsData(FixedProfilePeriods period)
    => AnyEnabled(EnumerateLevelsForPeriod(period))
       || IsHvnEnabled(period)
       || IsLvnEnabled(period);

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

    private bool UpdateHVNs(FixedProfilePeriods period, IndicatorCandle candle)
    {
        if (InstrumentInfo?.TickSize is null || InstrumentInfo.TickSize <= 0m)
            return false;

        // If HVN is disabled for this period, clear cached bands (if any) and skip work.
        if (!IsHvnEnabled(period))
        {
            if (_hvnBands.TryGetValue(period, out var cached) && cached.Count > 0)
            {
                cached.Clear();
                return true; // state changed -> redraw
            }

            return false;
        }

        if (candle == null)
            return false;

        int oldHash;
        if (!_hvnBands.TryGetValue(period, out var bands))
        {
            bands = new List<Band>();
            _hvnBands[period] = bands;
            oldHash = 0;
        }
        else
        {
            oldHash = ComputeBandsHash(bands);
            bands.Clear();
        }

        var poc = candle.MaxVolumePriceInfo;
        if (poc == null || poc.Volume <= 0)
            return oldHash != 0;

        var cutoff = poc.Volume * (HVNThresholdPct / 100m);

        var levelsEnum = candle.GetAllPriceLevels();
        if (levelsEnum == null)
            return oldHash != 0;

        var levels = levelsEnum.OrderBy(l => l.Price).ToList();
        if (levels.Count == 0)
            return oldHash != 0;

        var tick = InstrumentInfo.TickSize;

        decimal? runStart = null;
        decimal lastPriceInRun = 0m;
        int gapLeft = 0;

        bool IsNextTick(decimal prev, decimal next)
            => Math.Abs(next - prev) <= tick * 1.0000001m;

        for (int i = 0; i < levels.Count; i++)
        {
            var p = levels[i].Price;
            var v = (decimal)levels[i].Volume;

            bool isHigh = v >= cutoff;

            if (runStart == null)
            {
                if (!isHigh)
                    continue;

                runStart = p;
                lastPriceInRun = p;
                gapLeft = HVNGapToleranceTicks;
                continue;
            }

            bool contiguous = IsNextTick(lastPriceInRun, p);

            if (!contiguous)
            {
                bands.Add(new Band { Low = runStart.Value, High = lastPriceInRun });
                runStart = null;

                if (isHigh)
                {
                    runStart = p;
                    lastPriceInRun = p;
                    gapLeft = HVNGapToleranceTicks;
                }

                continue;
            }

            if (isHigh)
            {
                lastPriceInRun = p;
                gapLeft = HVNGapToleranceTicks;
            }
            else
            {
                if (gapLeft > 0)
                {
                    gapLeft--;
                    lastPriceInRun = p;
                }
                else
                {
                    bands.Add(new Band { Low = runStart.Value, High = lastPriceInRun });
                    runStart = null;
                }
            }
        }

        if (runStart != null && lastPriceInRun >= runStart.Value)
            bands.Add(new Band { Low = runStart.Value, High = lastPriceInRun });

        var newHash = ComputeBandsHash(bands);
        return newHash != oldHash;
    }

    private bool UpdateLVNs(FixedProfilePeriods period, IndicatorCandle candle)
    {
        if (InstrumentInfo?.TickSize is null || InstrumentInfo.TickSize <= 0m)
            return false;

        // If LVN is disabled for this period, clear cached bands (if any) and skip work.
        if (!IsLvnEnabled(period))
        {
            if (_lvnBands.TryGetValue(period, out var cached) && cached.Count > 0)
            {
                cached.Clear();
                return true; // state changed -> redraw
            }

            return false;
        }

        if (candle == null)
            return false;

        int oldHash;

        if (!_lvnBands.TryGetValue(period, out var bands))
        {
            bands = new List<Band>();
            _lvnBands[period] = bands;
            oldHash = 0;
        }
        else
        {
            oldHash = ComputeBandsHash(bands);
            bands.Clear();
        }

        var poc = candle.MaxVolumePriceInfo;
        if (poc == null || poc.Volume <= 0)
            return oldHash != 0;

        // LVN needs a "mature" profile; otherwise it will label almost the whole range as a void.
        if (poc.Volume < MinPocVolForLVN)
            return oldHash != 0;

        var cutoff = poc.Volume * (LVNThresholdPct / 100m);

        var levelsEnum = candle.GetAllPriceLevels();
        if (levelsEnum == null)
            return oldHash != 0; // cleared

        var levels = levelsEnum.OrderBy(l => l.Price).ToList();
        if (levels.Count == 0)
            return oldHash != 0; // cleared

        var tick = InstrumentInfo.TickSize;

        decimal? runStart = null;
        decimal lastPriceInRun = 0m;
        int gapLeft = 0;

        bool IsNextTick(decimal prev, decimal next)
            => Math.Abs(next - prev) <= tick * 1.0000001m;

        var tailFilterAmount = LVNTailFilterMinTicks * tick;
        var highLimit = candle.High - tailFilterAmount;
        var lowLimit = candle.Low + tailFilterAmount;

        for (int i = 0; i < levels.Count; i++)
        {
            var p = levels[i].Price;
            var v = (decimal)levels[i].Volume;

            // LVN condition: low-volume levels (below cutoff)
            bool isLow = (v < cutoff) && (p < highLimit) && (p > lowLimit);

            if (runStart == null)
            {
                if (!isLow)
                    continue;

                runStart = p;
                lastPriceInRun = p;
                gapLeft = LVNGapToleranceTicks;
                continue;
            }

            bool contiguous = IsNextTick(lastPriceInRun, p);

            if (!contiguous)
            {
                bands.Add(new Band { Low = runStart.Value, High = lastPriceInRun });
                runStart = null;

                if (isLow)
                {
                    runStart = p;
                    lastPriceInRun = p;
                    gapLeft = LVNGapToleranceTicks;
                }

                continue;
            }

            if (isLow)
            {
                lastPriceInRun = p;
                gapLeft = LVNGapToleranceTicks;
            }
            else
            {
                if (gapLeft > 0)
                {
                    gapLeft--;
                    lastPriceInRun = p;
                }
                else
                {
                    bands.Add(new Band { Low = runStart.Value, High = lastPriceInRun });
                    runStart = null;
                }
            }
        }

        if (runStart != null && lastPriceInRun >= runStart.Value)
            bands.Add(new Band { Low = runStart.Value, High = lastPriceInRun });

        // Change detection (cheap, consistent across all exit paths)
        var newHash = ComputeBandsHash(bands);
        return newHash != oldHash;
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
        // Precedence: per-line override > semantic matrix > base LevelSettings
        var effColor = levelSettings.OverrideColorInSchemes
            ? levelSettings.Color
            : (sem.Style.Color ?? levelSettings.Color);

        var effWidth = levelSettings.OverrideWidthInSchemes
            ? levelSettings.Width
            : (sem.Style.Width ?? levelSettings.Width);

        var effDash = levelSettings.OverrideStyleInSchemes
            ? levelSettings.LineStyle
            : (sem.Style.Dash ?? levelSettings.LineStyle);

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
            // 2) level weight (pq01 semantic ordering within period)
            // 3) deterministic tie-breaker inside period
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
        var textColor = ChartInfo.ColorsStore.MouseTextColor;

        // Keep existing look & feel: background + border
        var backgroundColor = ChartInfo.ColorsStore.BaseBackgroundColor;
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

        // Safety padding: include pen width + 1px to avoid “overpassing” artifacts
        var pad = _labelPadding + 1;

        // Collect “holes” on the line caused by labels
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

        // Keep the “nice border breathing room” for Left/Right,
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

    #region HVN / LVN

    private void RenderAllHVNsWithPriority(RenderContext context)
    {
        if (ChartInfo is null || InstrumentInfo is null)
            return;

        var region = ChartInfo.PriceChartContainer.Region;
        if (region.Width <= 0 || region.Height <= 0)
            return;

        if (InstrumentInfo.TickSize <= 0m)
            return;

        var tick = InstrumentInfo.TickSize;
        var buffer = HVNOcclusionTicks > 0 ? HVNOcclusionTicks * tick : 0m;

        // Claimed ranges in PRICE space (expanded with occlusion buffer)
        var claimed = new List<(decimal Low, decimal High)>(64);

        foreach (var period in _vnPriorityOrder)
        {
            if (!IsHvnEnabled(period))
                continue;

            if (!_hvnBands.TryGetValue(period, out var bands) || bands is null || bands.Count == 0)
                continue;

            var fillColor = GetHvnColor(period);
            // HVN borders: keep styling but ensure they remain visible even with low-alpha fills.
            var borderColor = Color.FromArgb(
                Math.Min(255, Math.Max(0, _hvnBorderAlpha)),
                fillColor.R, fillColor.G, fillColor.B).Convert();

            var borderPen = new PenSettings
            {
                Color = borderColor,
                Width = _hvnBorderWidth,
                LineDashStyle = LineDashStyle.Solid
            }.RenderObject;

            // Render each band with occlusion handling
            foreach (var b in bands)
            {
                var low = Math.Min(b.Low, b.High);
                var high = Math.Max(b.Low, b.High);

                // Subtract already claimed ranges (with buffer)
                var visibleSegments = SubtractClaimedRanges(low, high, claimed, buffer);
                if (visibleSegments.Count == 0)
                    continue;

                foreach (var seg in visibleSegments)
                    RenderBandSegment(context, region, seg.Low, seg.High, fillColor, borderPen);

                // Mark this band as claimed (expanded by buffer)
                var cLow = low - buffer;
                var cHigh = high + buffer;
                AddClaimedRange(claimed, cLow, cHigh);
            }
        }
    }

    private void RenderAllLVNsWithPriority(RenderContext context)
    {
        if (ChartInfo is null || InstrumentInfo is null)
            return;

        var region = ChartInfo.PriceChartContainer.Region;
        if (region.Width <= 0 || region.Height <= 0)
            return;

        if (InstrumentInfo.TickSize <= 0m)
            return;

        var tick = InstrumentInfo.TickSize;
        var lvnBuffer = LVNOcclusionTicks > 0 ? LVNOcclusionTicks * tick : 0m;

        // Claimed ranges start with ALL rendered HVN bands (cut-by-HVN)
        var claimed = new List<(decimal Low, decimal High)>(128);

        SeedClaimedRangesFromHVN(claimed, tick);

        foreach (var period in _vnPriorityOrder)
        {
            if (!IsLvnEnabled(period))
                continue;

            if (!_lvnBands.TryGetValue(period, out var bands) || bands is null || bands.Count == 0)
                continue;

            var fillColor = GetLvnColor(period);

            // LVN fill can be ultra-transparent; allow overriding alpha to keep bands visible while emphasizing borders.
            if (_lvnFillAlphaOverride >= 0 && _lvnFillAlphaOverride <= 255)
                fillColor = Color.FromArgb(_lvnFillAlphaOverride, fillColor.R, fillColor.G, fillColor.B).Convert();

            // LVN borders: more visible than fill (alpha/width overrides).
            var borderColor = Color.FromArgb(
                Math.Min(255, Math.Max(0, _lvnBorderAlpha)),
                fillColor.R, fillColor.G, fillColor.B).Convert();

            var borderPen = new PenSettings
            {
                Color = borderColor,
                Width = _lvnBorderWidth,
                LineDashStyle = LineDashStyle.Dot
            }.RenderObject;

            foreach (var b in bands)
            {
                var low = Math.Min(b.Low, b.High);
                var high = Math.Max(b.Low, b.High);

                // Subtract already claimed ranges (including HVN + previous LVN with buffer)
                var visibleSegments = SubtractClaimedRanges(low, high, claimed, lvnBuffer);
                if (visibleSegments.Count == 0)
                    continue;

                foreach (var seg in visibleSegments)
                    RenderBandSegment(context, region, seg.Low, seg.High, fillColor, borderPen);

                // Mark this LVN segment as claimed (expanded by LVN buffer)
                AddClaimedRange(claimed, low - lvnBuffer, high + lvnBuffer);
            }
        }
    }

    private void RenderBandSegment(
    RenderContext context,
    Rectangle region,
    decimal low,
    decimal high,
    CrossColor fillColor,
    RenderPen borderPen)
    {
        // Convert PRICE -> Y
        var y1 = ChartInfo.GetYByPrice(high, false);
        var y2 = ChartInfo.GetYByPrice(low, false);

        var top = Math.Min(y1, y2);
        var bottom = Math.Max(y1, y2);

        // Clip to visible region
        if (bottom < region.Top || top > region.Bottom)
            return;

        var clippedTop = Math.Max(top, region.Top);
        var clippedBottom = Math.Min(bottom, region.Bottom);

        // Ensure at least 1 px height
        var height = Math.Max(1, clippedBottom - clippedTop);

        var rect = new Rectangle(region.Left, clippedTop, region.Width, height);

        context.FillRectangle(ToDrawingColor(fillColor), rect);
        context.DrawRectangle(borderPen, rect);
    }


    private List<(decimal Low, decimal High)> SubtractClaimedRanges(
        decimal low,
        decimal high,
        List<(decimal Low, decimal High)> claimed,
        decimal buffer)
    {
        // Work list initially contains the full band
        var result = new List<(decimal Low, decimal High)>(4) { (low, high) };
        if (claimed.Count == 0)
            return result;

        // Subtract each claimed (expanded) range from current segments
        for (int i = 0; i < claimed.Count; i++)
        {
            var c = claimed[i];
            var cLow = c.Low;
            var cHigh = c.High;

            var next = new List<(decimal Low, decimal High)>(result.Count);

            for (int j = 0; j < result.Count; j++)
            {
                var seg = result[j];
                var sLow = seg.Low;
                var sHigh = seg.High;

                // No overlap
                if (sHigh < cLow || sLow > cHigh)
                {
                    next.Add(seg);
                    continue;
                }

                // Left remainder
                if (sLow < cLow)
                    next.Add((sLow, cLow));

                // Right remainder
                if (sHigh > cHigh)
                    next.Add((cHigh, sHigh));
            }

            result = next;
            if (result.Count == 0)
                break;
        }

        // Normalize & remove degenerate segments
        for (int k = result.Count - 1; k >= 0; k--)
        {
            var seg = result[k];
            var a = Math.Min(seg.Low, seg.High);
            var b = Math.Max(seg.Low, seg.High);
            if (b <= a)
                result.RemoveAt(k);
            else
                result[k] = (a, b);
        }

        return result;
    }

    private void AddClaimedRange(List<(decimal Low, decimal High)> claimed, decimal low, decimal high)
    {
        if (high <= low)
            return;

        // Simple merge into claimed list (kept compact; order not required)
        for (int i = 0; i < claimed.Count; i++)
        {
            var c = claimed[i];

            // Overlap or touch: merge
            if (!(high < c.Low || low > c.High))
            {
                var nLow = Math.Min(low, c.Low);
                var nHigh = Math.Max(high, c.High);
                claimed[i] = (nLow, nHigh);
                return;
            }
        }

        claimed.Add((low, high));
    }
    private void SeedClaimedRangesFromHVN(List<(decimal Low, decimal High)> claimed, decimal tick)
    {
        // Use HVN occlusion buffer (so LVN does not bleed into HVN zones)
        var hvnBuffer = HVNOcclusionTicks > 0 ? HVNOcclusionTicks * tick : 0m;

        foreach (var period in _vnPriorityOrder)
        {
            if (!IsHvnEnabled(period))
                continue;

            if (!_hvnBands.TryGetValue(period, out var bands) || bands is null || bands.Count == 0)
                continue;

            foreach (var b in bands)
            {
                var low = Math.Min(b.Low, b.High);
                var high = Math.Max(b.Low, b.High);

                AddClaimedRange(claimed, low - hvnBuffer, high + hvnBuffer);
            }
        }
    }

    private bool IsHvnEnabled(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => DayHVNEnabled,
            FixedProfilePeriods.LastDay => PrevDayHVNEnabled,
            FixedProfilePeriods.CurrentWeek => WeekHVNEnabled,
            FixedProfilePeriods.LastWeek => PrevWeekHVNEnabled,
            FixedProfilePeriods.CurrentMonth => MonthHVNEnabled,
            FixedProfilePeriods.LastMonth => PrevMonthHVNEnabled,
            FixedProfilePeriods.Contract => ContractHVNEnabled,
            _ => false
        };
    }

    private CrossColor GetHvnColor(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => DayHVNColor,
            FixedProfilePeriods.LastDay => PrevDayHVNColor,
            FixedProfilePeriods.CurrentWeek => WeekHVNColor,
            FixedProfilePeriods.LastWeek => PrevWeekHVNColor,
            FixedProfilePeriods.CurrentMonth => MonthHVNColor,
            FixedProfilePeriods.LastMonth => PrevMonthHVNColor,
            FixedProfilePeriods.Contract => ContractHVNColor,
            _ => System.Drawing.Color.Transparent.Convert()
        };
    }

    private bool IsLvnEnabled(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => DayLVNEnabled,
            FixedProfilePeriods.LastDay => PrevDayLVNEnabled,
            FixedProfilePeriods.CurrentWeek => WeekLVNEnabled,
            FixedProfilePeriods.LastWeek => PrevWeekLVNEnabled,
            FixedProfilePeriods.CurrentMonth => MonthLVNEnabled,
            FixedProfilePeriods.LastMonth => PrevMonthLVNEnabled,
            FixedProfilePeriods.Contract => ContractLVNEnabled,
            _ => false
        };
    }

    private CrossColor GetLvnColor(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => DayLVNColor,
            FixedProfilePeriods.LastDay => PrevDayLVNColor,
            FixedProfilePeriods.CurrentWeek => WeekLVNColor,
            FixedProfilePeriods.LastWeek => PrevWeekLVNColor,
            FixedProfilePeriods.CurrentMonth => MonthLVNColor,
            FixedProfilePeriods.LastMonth => PrevMonthLVNColor,
            FixedProfilePeriods.Contract => ContractLVNColor,
            _ => Color.Transparent.Convert()
        };
    }

    private static Color ToDrawingColor(CrossColor cc)
    {
        return Color.FromArgb(cc.A, cc.R, cc.G, cc.B);
    }

    private static int ComputeBandsHash(List<Band> bands)
    {
        unchecked
        {
            int h = 17;
            h = (h * 31) + bands.Count;

            for (int i = 0; i < bands.Count; i++)
            {
                // decimal.GetHashCode() is stable within the process and cheap enough here.
                h = (h * 31) + bands[i].Low.GetHashCode();
                h = (h * 31) + bands[i].High.GetHashCode();
            }

            return h;
        }
    }

    #endregion



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

    private IEnumerable<LevelSettings> EnumerateLevelsForPeriod(FixedProfilePeriods period)
    {
        foreach (var (ls, p) in EnumerateAllLevelSettingsWithPeriods())
        {
            if (p == period)
                yield return ls;
        }
    }

    private static bool AnyEnabled(IEnumerable<LevelSettings> levels)
    {
        foreach (var ls in levels)
        {
            if (ls.Enabled)
                return true;
        }

        return false;
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

        if (e.PropertyName == nameof(LevelSettings.Color)
            || e.PropertyName == nameof(LevelSettings.OverrideColorInSchemes)
            || e.PropertyName == nameof(LevelSettings.LineStyle)
            || e.PropertyName == nameof(LevelSettings.Width)
            || e.PropertyName == nameof(LevelSettings.LabelPosition)
            || e.PropertyName == nameof(LevelSettings.ShowPrice)
            || e.PropertyName == nameof(LevelSettings.OverrideLabel)
            || e.PropertyName == nameof(LevelSettings.OverrideWidthInSchemes)
            || e.PropertyName == nameof(LevelSettings.OverrideStyleInSchemes))
        {
            RedrawChart();
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