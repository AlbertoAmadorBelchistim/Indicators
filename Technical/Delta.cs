namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

using ATAS.Indicators.Technical.Properties;

using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

using CrossColor = CrossColor;

[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.DeltaDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602362")]
public class Delta : Indicator
{
	#region Nested types

	[Serializable]
	public enum BarDirection
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Any))]
		Any = 0,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bullish))]
		Bullish = 1,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bearlish))]
		Bearlish = 2
	}

	[Serializable]
	public enum DeltaType
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Any))]
		Any = 0,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Positive))]
		Positive = 1,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Negative))]
		Negative = 2
	}

	[Serializable]
	public enum DeltaVisualMode
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Candles))]
		Candles = 0,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HighLow))]
		HighLow = 1,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Histogram))]
		Histogram = 2,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bars))]
		Bars = 3
	}

	public enum Location
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Up))]
		Up,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Middle))]
		Middle,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Down))]
		Down
	}

	[Serializable]
	public enum AverageMode
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.SMA))]
		Sma = 0,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.EMA))]
		Ema = 1
	}

	[Serializable]
	public enum AverageColorMode
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.Fixed))]
		Fixed = 0,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ZeroCross))]
		ZeroCross = 1,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.Slope))]
		Slope = 2
	}

	[Serializable]
	public enum ThresholdSource
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.Fixed))]
		Fixed = 0,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSourceDynamicWelford))]
		DynamicWelford = 1
	}

	[Serializable]
	public enum SessionWindowMode
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowModeFull24h))]
		Full24h = 0,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowModeRth))]
		RTH = 1
	}

	[Serializable]
	public enum ThresholdLevel
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdLevelMajor))]
		Major = 0,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdLevelMinor))]
		Minor = 1
	}

	private struct WelfordAcc
	{
		public int Count;
		public decimal Mean;
		public decimal M2;

		public void Add(decimal x)
		{
			Count++;
			var delta = x - Mean;
			Mean += delta / Count;
			var delta2 = x - Mean;
			M2 += delta * delta2;
		}

		public decimal Std()
		{
			if (Count <= 1) return 0m;
			var variance = M2 / (Count - 1);
			return (decimal)Math.Sqrt((double)variance);
		}

		public void Reset() { Count = 0; Mean = 0m; M2 = 0m; }
	}

	#endregion

	#region Fields

	private readonly CandleDataSeries _candles = new("Candles", "Delta candles")
	{
		DownCandleColor = Color.Red.Convert(),
		UpCandleColor = Color.Green.Convert(),
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		ResetAlertsOnNewBar = true
	};

	private readonly ValueDataSeries _currentValues = new("CurrentValues", "Current Values")
	{
		IsHidden = true,
		VisualType = VisualMode.OnlyValueOnAxis,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

	private readonly ValueDataSeries _delta = new("DeltaId", "Delta")
	{
		Color = Color.Red.Convert(),
		VisualType = VisualMode.Hide,
		ShowZeroValue = false,
		ShowCurrentValue = false,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		ResetAlertsOnNewBar = true
	};

	private readonly ValueDataSeries _diapasonHigh = new("DiapasonHigh", "Delta range high")
	{
		Color = CrossColor.FromArgb(128, 128, 128, 128),
		ShowZeroValue = false,
		ShowCurrentValue = false,
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

	private readonly ValueDataSeries _diapasonLow = new("DiapasonLow", "Delta range low")
	{
		Color = CrossColor.FromArgb(128, 128, 128, 128),
		ShowZeroValue = false,
		ShowCurrentValue = false,
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

	private readonly Dictionary<int, bool> _divergenceBars = new();

	private readonly CandleDataSeries _divergenceCandles = new("DivergenceCandles", "Divergence candles")
	{
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		Visible = false,
		#if !ATAS_STABLE
		DrawCandleBorder = false
		#endif
    };

	private readonly CandleDataSeries _divergenceDownCandles = new("DivergenceDownCandles", "Divergence down candles")
	{
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		Visible = false,
		#if !ATAS_STABLE
		DrawCandleBorder = false
		#endif
    };

	private readonly CandleDataSeries _downCandles = new("DownCandles", "Delta candles")
	{
		DownCandleColor = Color.Green.Convert(),
		UpCandleColor = Color.Red.Convert(),
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		ResetAlertsOnNewBar = true
	};

	private BarDirection _barDirection;
	private DeltaType _deltaType;
	private Color _downColor = Color.Red;

	private ValueDataSeries _downSeries = new("DownSeries", Strings.Down)
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

	private decimal _filter;

	private Color _fontColor;

	private RenderStringFormat _format = new()
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	private int _lastBar;
	private int _lastBarAlert;
	private int _lastBarNegativeAlert;
	private bool _minimizedMode;
	private DeltaVisualMode _mode = DeltaVisualMode.Candles;

	private Color _neutralColor = Color.Gray;
	private decimal _prevDeltaValue;
	private bool _showCurrentValues = true;

	private Color _upColor = Color.Green;

	private ValueDataSeries _upSeries = new("UpSeries", Strings.Up)
	{
		Color = Color.Green.Convert(),
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

	#region Fields (fixed threshold lines)

	private readonly ValueDataSeries _upMajor = new("UpMajor", "Up Major")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		Color = CrossColor.FromArgb(255, 169, 169, 169),
		Width = 1,
		LineDashStyle = LineDashStyle.Solid
	};

	private readonly ValueDataSeries _upMinor = new("UpMinor", "Up Minor")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		Color = CrossColor.FromArgb(255, 105, 105, 105),
		Width = 1,
		LineDashStyle = LineDashStyle.Dot
	};

	private readonly ValueDataSeries _dnMinor = new("DnMinor", "Down Minor")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		Color = CrossColor.FromArgb(255, 105, 105, 105),
		Width = 1,
		LineDashStyle = LineDashStyle.Dot
	};

	private readonly ValueDataSeries _dnMajor = new("DnMajor", "Down Major")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		Color = CrossColor.FromArgb(255, 169, 169, 169),
		Width = 1,
		LineDashStyle = LineDashStyle.Solid
	};

	private bool _showThresholdLines = true;
	private int _upMajorLevel = 300;
	private int _upMinorLevel = 200;
	private int _downMinorLevel = -200;
	private int _downMajorLevel = -300;

	private ThresholdLevel _visualUpLevel = ThresholdLevel.Major;
	private ThresholdLevel _visualDownLevel = ThresholdLevel.Major;

	#endregion

	#region Fields (dynamic thresholds)

	private int _samplesForMeanStd = 1;

	private bool _posReady;
	private bool _negReady;

	private readonly List<bool> _posReadyByBar = new();
	private readonly List<bool> _negReadyByBar = new();

	private WelfordAcc _posAcc;
	private WelfordAcc _negAcc;

	private decimal _dynPosMinor, _dynPosMajor;
	private decimal _dynNegMinor, _dynNegMajor;

	private SessionWindowMode _sessionMode = SessionWindowMode.RTH;
	private TimeSpan _rthStart = new(9, 30, 0);
	private TimeSpan _rthEnd = new(16, 0, 0);
	private decimal _stdMultiplier = 1.0m;

	private ThresholdSource _thresholds = ThresholdSource.Fixed;

	#endregion

	#region Fields (audio alerts)

	private bool _audioEnabled = false;
	private bool _audioAtBarCloseOnly = true;
	private int _alertCooldownBars = 3;

	private ThresholdLevel _audioUpLevel = ThresholdLevel.Major;
	private ThresholdLevel _audioDownLevel = ThresholdLevel.Major;

	private int _lastBarAudioUpAlert;
	private int _lastBarAudioDownAlert;

	#endregion

	#region Fields (price signals)

	private readonly ValueDataSeries _priceSignalUp = new("PriceSignalUp", "Price Signal Up")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		ShowCurrentValue = false
	};

	private readonly ValueDataSeries _priceSignalDown = new("PriceSignalDown", "Price Signal Down")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		ShowCurrentValue = false
	};

	#region Fields (average line)

	private readonly ValueDataSeries _avgSeries = new("AverageDelta", "Average")
	{
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true,
		Width = 2,
		Color = CrossColor.FromArgb(255, 60, 120, 240),
		ShowCurrentValue = false
	};

	private bool _showAverage;
	private int _averagePeriod = 20;
	private AverageMode _avgMode = AverageMode.Sma;
	private AverageColorMode _avgColorMode = AverageColorMode.Fixed;
	private Color _avgSlopeUpColor = Color.Green;
	private Color _avgSlopeDownColor = Color.Red;

	private readonly Queue<decimal> _smaWindow = new();
	private decimal _smaSum;
	private decimal _emaValue;
	private bool _emaInitialized;

	#endregion

	private bool _visualEnabled = true;
	private int _priceSignalOffsetTicks = 2;
	private int _priceSignalSize = 10;
	private Color _priceSignalUpColor = Color.Lime;
	private Color _priceSignalDownColor = Color.Fuchsia;

	#endregion

#endregion

    #region Properties

    #region Visualization

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VisualMode), GroupName = nameof(Strings.Visualization),
        Description = nameof(Strings.VisualModeDescription), Order = 10)]
    public DeltaVisualMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;

            if (_mode == DeltaVisualMode.Histogram)
            {
                _delta.VisualType = VisualMode.Histogram;
                _diapasonHigh.VisualType = VisualMode.Hide;
                _diapasonLow.VisualType = VisualMode.Hide;
                _candles.Visible = _downCandles.Visible = false;
                _divergenceCandles.Visible = _divergenceDownCandles.Visible = false;
            }
            else if (_mode == DeltaVisualMode.HighLow)
            {
                _delta.VisualType = VisualMode.Histogram;
                _diapasonHigh.VisualType = VisualMode.Histogram;
                _diapasonLow.VisualType = VisualMode.Histogram;
                _candles.Visible = _downCandles.Visible = false;
                _divergenceCandles.Visible = _divergenceDownCandles.Visible = false;
            }
            else if (_mode == DeltaVisualMode.Candles)
            {
                _delta.VisualType = VisualMode.Hide;
                _diapasonHigh.VisualType = VisualMode.Hide;
                _diapasonLow.VisualType = VisualMode.Hide;
                _candles.Visible = _downCandles.Visible = true;
                _candles.Mode = _downCandles.Mode = CandleVisualMode.Candles;
                _divergenceCandles.Mode = _divergenceDownCandles.Mode = CandleVisualMode.Candles;
            }
            else
            {
                _delta.VisualType = VisualMode.Hide;
                _diapasonHigh.VisualType = VisualMode.Hide;
                _diapasonLow.VisualType = VisualMode.Hide;
                _candles.Visible = _downCandles.Visible = true;
                _candles.Mode = _downCandles.Mode = CandleVisualMode.Bars;
                _divergenceCandles.Mode = _divergenceDownCandles.Mode = CandleVisualMode.Bars;
            }

            RaisePropertyChanged("Mode");
            RecalculateValues();

			ApplyDivergenceColorsToCurrentMode();
            UpdateDivergenceCandlesVisibility();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimizedMode), GroupName = nameof(Strings.Visualization),
        Description = nameof(Strings.HistogramMinimizedModeDescription), Order = 20)]

    public bool MinimizedMode
    {
        get => _minimizedMode;
        set
        {
            _minimizedMode = value;
            RaisePropertyChanged("MinimizedMode");
            RecalculateValues();
			UpdateDivergenceCandlesVisibility();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowCurrentValue), GroupName = nameof(Strings.Visualization),
        Description = nameof(Strings.ShowCurrentValueDescription), Order = 30)]
    public bool ShowCurrentValues
    {
        get => _showCurrentValues;
        set
        {
            _showCurrentValues = value;
            _currentValues.ShowCurrentValue = value;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DeltaPositiveColor), GroupName = nameof(Resources.Drawing),
        Description = nameof(Resources.PositiveValueColorDescription), Order = 1000)]
    public CrossColor UpColor
    {
        get => _upColor.Convert();
        set
        {
            _upColor = value.Convert();
            _candles.UpCandleColor = value;
            _upSeries.Color = value;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DeltaNegativeColor), GroupName = nameof(Resources.Drawing),
        Description = nameof(Resources.NegativeValueColorDescription), Order = 1010)]
    public CrossColor DownColor
    {
        get => _downColor.Convert();
        set
        {
            _downColor = value.Convert();
            _candles.DownCandleColor = value;
            _downCandles.UpCandleColor = value;
            _downSeries.Color = value;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.NeutralBorderColor), GroupName = nameof(Resources.Drawing),
        Description = nameof(Resources.NeutralValueDescription), Order = 1020)]
    public CrossColor NeutralColor
    {
        get => _neutralColor.Convert();
        set
        {
            _neutralColor = value.Convert();
            _candles.BorderColor = _downCandles.BorderColor = value;
            _diapasonHigh.Color = _diapasonLow.Color = value;
        }
    }

    #endregion

    #region Filters

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BarsDirection), GroupName = nameof(Strings.Filters),
        Description = nameof(Strings.BarDirectionDescription), Order = 100)]
    public BarDirection BarsDirection
    {
        get => _barDirection;
        set
        {
            _barDirection = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.DeltaType), GroupName = nameof(Strings.Filters),
        Description = nameof(Strings.DeltaTypeDescription), Order = 110)]
    public DeltaType DeltaTypes
    {
        get => _deltaType;
        set
        {
            _deltaType = value;
            RecalculateValues();
        }
    }

    [Parameter]
    [Range(0, int.MaxValue)]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Filter), GroupName = nameof(Strings.Filters),
        Description = nameof(Strings.MinDeltaVolumeFilterCommonDescription), Order = 120)]
    public decimal Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            RecalculateValues();
        }
    }

    #endregion

    #region Divergence

    private Indicators.FilterColor _divergenceBarsFilter = new(true) { Enabled = false, Value = CrossColor.FromArgb(255, 255, 165, 0) };

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DivergenceDots), GroupName = nameof(Resources.Divergence),
        Description = nameof(Resources.DivergenceDotsDescription), Order = 130)]
    public bool ShowDivergence { get; set; }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DivergenceBars), GroupName = nameof(Resources.Divergence),
        Description = nameof(Resources.DivergenceBarsDescription), Order = 135)]
    public Indicators.FilterColor DivergenceBarsFilter
    {
        get => _divergenceBarsFilter;
        set
        {
            if (_divergenceBarsFilter == value)
                return;

            if (_divergenceBarsFilter != null)
                _divergenceBarsFilter.PropertyChanged -= OnDivergenceFilterChanged;

            _divergenceBarsFilter = value;

            if (_divergenceBarsFilter != null)
                _divergenceBarsFilter.PropertyChanged += OnDivergenceFilterChanged;

            RaisePropertyChanged(nameof(DivergenceBarsFilter));
        }
    }

    #endregion

    #region Absorption

    private readonly CandleDataSeries _absorptionCandles = new("AbsorptionDotsCandles", "Absorption Dots")
    {
        UpCandleColor = Color.Green.Convert(),
        DownCandleColor = Color.Red.Convert(),
        BorderColor = CrossColor.FromArgb(0, 0, 0, 0),
        IsHidden = true,
        UseMinimizedModeIfEnabled = true,
        ShowCurrentValue = false
    };

    private FilterInt _absorption = new(true) { Enabled = false, Value = 250 };

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Absorption), GroupName = nameof(Resources.Absorption),
        Description = "AbsorptionThresholdDesc", Order = 140)]
    [Range(0, int.MaxValue)]
    public FilterInt Absorption
    {
        get => _absorption;
        set
        {
            if (_absorption == value)
                return;

            if (_absorption != null)
                _absorption.PropertyChanged -= OnAbsorptionFilterChanged;

            _absorption = value;

            if (_absorption != null)
                _absorption.PropertyChanged += OnAbsorptionFilterChanged;

            RaisePropertyChanged(nameof(Absorption));
        }
    }

    // Backward compatibility properties (hidden from UI)
    [Browsable(false)]
    public bool ShowAbsorptionDots
    {
        get => Absorption.Enabled;
        set => Absorption.Enabled = value;
    }

    [Browsable(false)]
    public int AbsorptionDeltaThreshold
    {
        get => Absorption.Value;
        set => Absorption.Value = value;
    }

    #endregion

    #region Average Delta

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowAverage),
        GroupName = nameof(Resources.Average), Order = 400)]
    public bool ShowAverage
    {
        get => _showAverage;
        set
        {
            if (_showAverage == value)
                return;

            _showAverage = value;
            _avgSeries.VisualType = value ? VisualMode.Line : VisualMode.Hide;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.AveragePeriod),
        GroupName = nameof(Resources.Average), Order = 410)]
    [Range(1, 1000)]
    public int AveragePeriod
    {
        get => _averagePeriod;
        set
        {
            if (value < 1) value = 1;
            if (_averagePeriod == value)
                return;

            _averagePeriod = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.CalculationMode),
        GroupName = nameof(Resources.Average), Order = 420)]
    public AverageMode AvgMode
    {
        get => _avgMode;
        set
        {
            if (_avgMode == value)
                return;

            _avgMode = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ColorMode),
        GroupName = nameof(Resources.Average), Order = 425)]
    public AverageColorMode AvgColorMode
    {
        get => _avgColorMode;
        set
        {
            if (_avgColorMode == value)
                return;

            _avgColorMode = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.BaseColor),
        GroupName = nameof(Resources.Average), Order = 430)]
    public CrossColor AverageColor
    {
        get => _avgSeries.Color;
        set
        {
            _avgSeries.Color = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SlopeUpColor),
        GroupName = nameof(Resources.Average), Order = 431)]
    public CrossColor AvgSlopeUpColor
    {
        get => _avgSlopeUpColor.Convert();
        set
        {
            _avgSlopeUpColor = value.Convert();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SlopeDownColor),
        GroupName = nameof(Resources.Average), Order = 432)]
    public CrossColor AvgSlopeDownColor
    {
        get => _avgSlopeDownColor.Convert();
        set
        {
            _avgSlopeDownColor = value.Convert();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.LineWidth),
        GroupName = nameof(Resources.Average), Order = 440)]
    [Range(1, 10)]
    public int AverageWidth
    {
        get => _avgSeries.Width;
        set
        {
            if (_avgSeries.Width == value)
                return;

            _avgSeries.Width = value;
            RedrawChart();
        }
    }

    #endregion

    #region Dynamic thresholds

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSource),
        Description = nameof(Resources.ThresholdSourceDescription),
        GroupName = nameof(Resources.ThresholdsGroup), Order = 140)]
    public ThresholdSource Thresholds
    {
        get => _thresholds;
        set
        {
            if (_thresholds == value)
                return;

            _thresholds = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowMode),
        Description = nameof(Resources.SessionWindowModeDescription),
        GroupName = nameof(Resources.DynamicThresholdGroup), Order = 141)]
    public SessionWindowMode SessionMode
    {
        get => _sessionMode;
        set
        {
            if (_sessionMode == value)
                return;

            _sessionMode = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthStart),
        Description = nameof(Resources.RthStartDescription),
        GroupName = nameof(Resources.DynamicThresholdGroup), Order = 142)]
    public TimeSpan RthStart
    {
        get => _rthStart;
        set
        {
            if (_rthStart == value)
                return;

            _rthStart = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthEnd),
        Description = nameof(Resources.RthEndDescription),
        GroupName = nameof(Resources.DynamicThresholdGroup), Order = 143)]
    public TimeSpan RthEnd
    {
        get => _rthEnd;
        set
        {
            if (_rthEnd == value)
                return;

            _rthEnd = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.StdMultiplier),
        Description = nameof(Resources.StdMultiplierDescription),
        GroupName = nameof(Resources.DynamicThresholdGroup), Order = 144)]
    [Range(typeof(decimal), "0", "10")]
    [DisplayFormat(DataFormatString = "F2")]
    public decimal StdMultiplier
    {
        get => _stdMultiplier;
        set
        {
            if (value < 0) value = 0;
            if (_stdMultiplier == value)
                return;

            _stdMultiplier = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SamplesForMeanStd),
        Description = nameof(Resources.SamplesForMeanStdDescription),
        GroupName = nameof(Resources.DynamicThresholdGroup), Order = 145)]
    [Range(1, 5000)]
    public int SamplesForMeanStd
    {
        get => _samplesForMeanStd;
        set
        {
            if (value < 1) value = 1;
            if (_samplesForMeanStd == value)
                return;

            _samplesForMeanStd = value;
            RecalculateValues();
            RedrawChart();
        }
    }

    #endregion

    #region Threshold lines (fixed)

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowThresholdLines),
        Description = nameof(Resources.ShowThresholdLinesDescription),
        GroupName = nameof(Resources.ThresholdsGroup), Order = 145)]
    public bool ShowThresholdLines
    {
        get => _showThresholdLines;
        set
        {
            if (_showThresholdLines == value)
                return;

            _showThresholdLines = value;
            UpdateThresholdLinesVisibility(repaint: true);
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedPosMajorLevel),
        Description = nameof(Resources.FixedPosMajorLevelDescription),
        GroupName = nameof(Resources.FixedThresholdGroup), Order = 150)]
    [Range(0, int.MaxValue)]
    public int UpMajorLevel
    {
        get => _upMajorLevel;
        set
        {
            if (_upMajorLevel == value)
                return;

            _upMajorLevel = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedPosMinorLevel),
        Description = nameof(Resources.FixedPosMinorLevelDescription),
        GroupName = nameof(Resources.FixedThresholdGroup), Order = 160)]
    [Range(0, int.MaxValue)]
    public int UpMinorLevel
    {
        get => _upMinorLevel;
        set
        {
            if (_upMinorLevel == value)
                return;

            _upMinorLevel = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedNegMinorLevel),
        Description = nameof(Resources.FixedNegMinorLevelDescription),
        GroupName = nameof(Resources.FixedThresholdGroup), Order = 170)]
    [Range(int.MinValue, 0)]
    public int DownMinorLevel
    {
        get => _downMinorLevel;
        set
        {
            if (_downMinorLevel == value)
                return;

            _downMinorLevel = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedNegMajorLevel),
        Description = nameof(Resources.FixedNegMajorLevelDescription),
        GroupName = nameof(Resources.FixedThresholdGroup), Order = 180)]
    [Range(int.MinValue, 0)]
    public int DownMajorLevel
    {
        get => _downMajorLevel;
        set
        {
            if (_downMajorLevel == value)
                return;

            _downMajorLevel = value;
            RecalculateValues();
        }
    }

    #endregion

    #region Volume

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Show), GroupName = nameof(Resources.DeltaLabelGroup), Order = 900,
        Description = nameof(Resources.VolumeLabelDescription))]
    public bool ShowVolume { get; set; }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Color), GroupName = nameof(Resources.DeltaLabelGroup),
        Description = nameof(Resources.LabelTextColorDescription), Order = 910)]
    public CrossColor FontColor
    {
        get => _fontColor.Convert();
        set => _fontColor = value.Convert();
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Location), GroupName = nameof(Resources.DeltaLabelGroup),
        Description = nameof(Resources.LabelLocationDescription), Order = 920)]
    public Location VolLocation { get; set; } = Location.Middle;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Font), GroupName = nameof(Resources.DeltaLabelGroup),
        Description = nameof(Resources.FontSettingDescription), Order = 930)]
    public FontSetting Font { get; set; } = new("Arial", 10);

    #endregion

    #region Audio alerts

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioAlerts),
        Description = nameof(Resources.AudioAlertsDescription),
        GroupName = nameof(Resources.Alerts), Order = 301)]
    public bool AudioEnabled
    {
        get => _audioEnabled;
        set
        {
            if (_audioEnabled == value)
                return;

            _audioEnabled = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioUpThresholds),
        Description = nameof(Resources.AudioUpThresholdsDescription),
        GroupName = nameof(Resources.Alerts), Order = 302)]
    public ThresholdLevel AudioUpLevel
    {
        get => _audioUpLevel;
        set
        {
            if (_audioUpLevel == value)
                return;

            _audioUpLevel = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioDownThresholds),
        Description = nameof(Resources.AudioDownThresholdsDescription),
        GroupName = nameof(Resources.Alerts), Order = 303)]
    public ThresholdLevel AudioDownLevel
    {
        get => _audioDownLevel;
        set
        {
            if (_audioDownLevel == value)
                return;

            _audioDownLevel = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioAtBarCloseOnly),
        Description = nameof(Resources.AudioAtBarCloseOnlyDescription),
        GroupName = nameof(Resources.Alerts), Order = 304)]
    public bool AudioAtBarCloseOnly
    {
        get => _audioAtBarCloseOnly;
        set
        {
            if (_audioAtBarCloseOnly == value)
                return;

            _audioAtBarCloseOnly = value;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.CoolDownPeriod),
        GroupName = nameof(Resources.Alerts), Order = 305)]
    [Range(0, 100)]
    public int AlertCooldownBars
    {
        get => _alertCooldownBars;
        set
        {
            if (_alertCooldownBars == value)
                return;

            _alertCooldownBars = value;
        }
    }

    #endregion

    #region Threshold level selection

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.VisualUpThresholds),
        Description = nameof(Resources.VisualUpThresholdsDescription),
        GroupName = nameof(Resources.Alerts), Order = 293)]
    public ThresholdLevel VisualUpLevel
    {
        get => _visualUpLevel;
        set
        {
            if (_visualUpLevel == value)
                return;

            _visualUpLevel = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.VisualDownThresholds),
        Description = nameof(Resources.VisualDownThresholdsDescription),
        GroupName = nameof(Resources.Alerts), Order = 294)]
    public ThresholdLevel VisualDownLevel
    {
        get => _visualDownLevel;
        set
        {
            if (_visualDownLevel == value)
                return;

            _visualDownLevel = value;
            RedrawChart();
        }
    }

    #endregion

    #region Visual alerts (price panel)

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowVisualAlerts),
        Description = nameof(Resources.ShowVisualAlertsDescription),
        GroupName = nameof(Resources.Alerts), Order = 295)]
    public bool VisualEnabled
    {
        get => _visualEnabled;
        set
        {
            if (_visualEnabled == value)
                return;

            _visualEnabled = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.VerticalOffset),
        Description = nameof(Resources.PriceSignalOffsetTicksDescription),
        GroupName = nameof(Resources.Alerts), Order = 296)]
    [Range(0, 50)]
    public int PriceSignalOffsetTicks
    {
        get => _priceSignalOffsetTicks;
        set
        {
            if (_priceSignalOffsetTicks == value)
                return;

            _priceSignalOffsetTicks = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.MarkerSize),
        Description = nameof(Resources.MarkerSizeDescription),
        GroupName = nameof(Resources.Alerts), Order = 297)]
    [Range(6, 24)]
    public int PriceSignalSize
    {
        get => _priceSignalSize;
        set
        {
            if (_priceSignalSize == value)
                return;

            _priceSignalSize = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.PriceSignalUpColor),
        Description = nameof(Resources.PriceSignalUpColorDescription),
        GroupName = nameof(Resources.Alerts), Order = 298)]
    public CrossColor PriceSignalUpColor
    {
        get => _priceSignalUpColor.Convert();
        set
        {
            _priceSignalUpColor = value.Convert();
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.PriceSignalDownColor),
        Description = nameof(Resources.PriceSignalDownColorDescription),
        GroupName = nameof(Resources.Alerts), Order = 299)]
    public CrossColor PriceSignalDownColor
    {
        get => _priceSignalDownColor.Convert();
        set
        {
            _priceSignalDownColor = value.Convert();
            RedrawChart();
        }
    }

    #endregion

    #region Alerts

    [Browsable(false)]
    [Range(0, int.MaxValue)]
    [DisplayFormat(DataFormatString = "F0")]
    public Filter UpAlert { get; set; } = new()
    { Enabled = false, Value = 0 };

    [Browsable(false)]
    [Range(int.MinValue, 0)]
    [DisplayFormat(DataFormatString = "F0")]
    public Filter DownAlert { get; set; } = new()
    { Enabled = false, Value = 0 };

    [Browsable(false)]
    public bool UseAlerts
    {
        get => UpAlert.Enabled;
        set => UpAlert.Enabled = value;
    }

    [Browsable(false)]
    public decimal AlertFilter
    {
        get => UpAlert.Value;
        set => UpAlert.Value = value;
    }

    [Browsable(false)]
    public bool UseNegativeAlerts
    {
        get => DownAlert.Enabled;
        set => DownAlert.Enabled = value;
    }

    [Browsable(false)]
    public decimal NegativeAlertFilter
    {
        get => DownAlert.Value;
        set => DownAlert.Value = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.Alerts),
        Description = nameof(Strings.AlertFileDescription), Order = 320)]
    public string AlertFile { get; set; } = "alert1";

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FontColor), GroupName = nameof(Strings.Alerts),
        Description = nameof(Strings.AlertTextColorDescription), Order = 330)]
    public CrossColor AlertForeColor { get; set; } = CrossColor.FromArgb(255, 247, 249, 249);

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BackGround), GroupName = nameof(Strings.Alerts),
        Description = nameof(Strings.AlertFillColorDescription), Order = 340)]
    public CrossColor AlertBGColor { get; set; } = CrossColor.FromArgb(255, 75, 72, 72);

    #endregion

    #endregion

    #region ctor

    public Delta()
		: base(true)
	{
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
		FontColor = Color.Blue.Convert();

		Panel = IndicatorDataProvider.NewPanel;
		DataSeries[0] = _delta;

		DataSeries.Insert(0, _diapasonHigh);
		DataSeries.Insert(1, _diapasonLow);
		DataSeries.Add(_candles);

		DataSeries.Add(_upSeries);
		DataSeries.Add(_downSeries);
		DataSeries.Add(_currentValues);
		DataSeries.Add(_downCandles);
		DataSeries.Add(_divergenceCandles);
		DataSeries.Add(_divergenceDownCandles);

		DataSeries.Add(_absorptionCandles);

		DataSeries.Add(_upMajor);
		DataSeries.Add(_upMinor);
		DataSeries.Add(_dnMinor);
		DataSeries.Add(_dnMajor);

		DataSeries.Add(_avgSeries);

		DataSeries.Add(_priceSignalUp);
		DataSeries.Add(_priceSignalDown);

		UpdateThresholdLinesVisibility(repaint: false);

		UpAlert.PropertyChanged += (sender, e) => _lastBarAlert = 0;
		DownAlert.PropertyChanged += (sender, e) => _lastBarNegativeAlert = 0;
		_divergenceBarsFilter.PropertyChanged += OnDivergenceFilterChanged;
		_absorption.PropertyChanged += OnAbsorptionFilterChanged;

		UpdateDivergenceCandlesVisibility();

		if (DivergenceBarsFilter?.Enabled == true)
			RecalculateValues();
	}

    #endregion

    #region Protected methods

	protected override void OnApplyDefaultColors()
	{
		if (ChartInfo is null)
			return;

		UpColor = ChartInfo.ColorsStore.UpCandleColor.Convert();
		DownColor = ChartInfo.ColorsStore.DownCandleColor.Convert();
		NeutralColor = ChartInfo.ColorsStore.BarBorderPen.Color.Convert();
		FontColor = ChartInfo.ColorsStore.FootprintMaximumVolumeTextColor.Convert();

		UpdateDivergenceCandlesVisibility();
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo is null || InstrumentInfo is null)
			return;

	
		if (ShowDivergence)
		{
			for (var i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
			{
				try
				{
					if (_upSeries[i] == 0 && _downSeries[i] == 0)
						continue;

					var candle = GetCandle(i);
					var x = ChartInfo.PriceChartContainer.GetXByBar(i, false);

					if (_upSeries[i] != 0)
					{
						var yPrice = ChartInfo.PriceChartContainer.GetYByPrice(candle.Low, false) + 10;
						var region = ChartInfo.PriceChartContainer.Region;

						if (yPrice >= region.Top && yPrice <= region.Bottom)
						{
							var rect = new Rectangle(x - 5, yPrice - 4, 8, 8);
							context.FillEllipse(_upColor, rect);
						}
					}

					if (_downSeries[i] != 0)
					{
						var yPrice = ChartInfo.PriceChartContainer.GetYByPrice(candle.High, false) - 10;
						var region = ChartInfo.PriceChartContainer.Region;

						if (yPrice >= region.Top && yPrice <= region.Bottom)
						{
							var rect = new Rectangle(x - 5, yPrice - 4, 8, 8);
							context.FillEllipse(_downColor, rect);
						}
					}
				}
				catch (OverflowException)
				{
					return;
				}
			}
		}

		// Price signals (triangles) - only on price panel
		if (_visualEnabled)
		{
			var priceRc = ChartInfo.PriceChartContainer.Region;
			var half = _priceSignalSize / 2;
			// Clamp drawing area so triangles are fully visible inside the region.
			// We clamp the triangle "center" so its vertices don't exceed bounds.
			var yMin = priceRc.Top + half;
			var yMax = priceRc.Bottom - half;

			for (var i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
			{
				var x = ChartInfo.PriceChartContainer.GetXByBar(i, false);

				// Up triangle
				var upPrice = _priceSignalUp[i];
				if (upPrice > 0)
				{
					var yPx = ChartInfo.PriceChartContainer.GetYByPrice(upPrice, false);
					yPx = ClampInt(yPx, yMin, yMax);
					var p1 = new Point(x, yPx - half);
					var p2 = new Point(x - half, yPx + half);
					var p3 = new Point(x + half, yPx + half);
					context.FillPolygon(_priceSignalUpColor, new[] { p1, p2, p3 });
				}

				// Down triangle
				var dnPrice = _priceSignalDown[i];
				if (dnPrice > 0)
				{
					var yPx = ChartInfo.PriceChartContainer.GetYByPrice(dnPrice, false);
					yPx = ClampInt(yPx, yMin, yMax);
					var p1 = new Point(x, yPx + half);
					var p2 = new Point(x - half, yPx - half);
					var p3 = new Point(x + half, yPx - half);
					context.FillPolygon(_priceSignalDownColor, new[] { p1, p2, p3 });
				}
			}
		}

		if (!ShowVolume || ChartInfo.ChartVisualMode != ChartVisualModes.Clusters || Panel == IndicatorDataProvider.CandlesPanel)
			return;

		var minWidth = GetMinWidth(context, FirstVisibleBarNumber, LastVisibleBarNumber);
		var barWidth = ChartInfo.GetXByBar(1) - ChartInfo.GetXByBar(0);

		if (minWidth > barWidth)
			return;

		var strHeight = context.MeasureString("0", Font.RenderObject).Height;

		var y = VolLocation switch
		{
			Location.Up => Container.Region.Y,
			Location.Down => Container.Region.Bottom - strHeight,
			_ => Container.Region.Y + (Container.Region.Bottom - Container.Region.Y) / 2
		};

		for (var i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
		{
			decimal value;

			if (MinimizedMode)
			{
				value = _candles[i].Close > 0
					? _candles[i].Close
					: -_downCandles[i].Close;
			}
			else
				value = _candles[i].Close;

			var renderText = ChartInfo.TryGetMinimizedVolumeString(value);

			var strRect = new Rectangle(ChartInfo.GetXByBar(i),
				y,
				barWidth,
				strHeight);

			context.DrawString(renderText, Font.RenderObject, _fontColor, strRect, _format);
		}
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		if (bar == 0)
		{
			DataSeries.ForEach(x => x.Clear());
			_upSeries.Clear();
			_downSeries.Clear();
			_divergenceBars.Clear();
		}

		// Always clear per bar to avoid stale markers
		_priceSignalUp[bar] = 0m;
		_priceSignalDown[bar] = 0m;

		var candle = GetCandle(bar);
		var deltaValue = candle.Delta;
		var absDelta = Math.Abs(deltaValue);
		var maxDelta = candle.MaxDelta;
		var minDelta = candle.MinDelta;

		if (maxDelta == minDelta)
		{
			if(maxDelta > 0)
				minDelta = 0;
			else
				maxDelta = 0;
        }

		var isUnderFilter = absDelta < _filter;

		if (_barDirection == BarDirection.Bullish)
		{
			if (candle.Close < candle.Open)
				isUnderFilter = true;
		}
		else if (_barDirection == BarDirection.Bearlish)
		{
			if (candle.Close > candle.Open)
				isUnderFilter = true;
		}

		if (_deltaType == DeltaType.Negative && deltaValue > 0)
			isUnderFilter = true;

		if (_deltaType == DeltaType.Positive && deltaValue < 0)
			isUnderFilter = true;

		if (isUnderFilter)
		{
			deltaValue = 0;
			absDelta = 0;
			minDelta = maxDelta = 0;
		}

		_delta[bar] = MinimizedMode ? absDelta : deltaValue;

		if (MinimizedMode)
		{
			var high = Math.Abs(maxDelta);
			var low = Math.Abs(minDelta);
			_diapasonLow[bar] = Math.Min(Math.Min(high, low), absDelta);
			_diapasonHigh[bar] = Math.Max(high, low);

			if (deltaValue >= 0)
			{
				var currentCandle = _candles[bar];
				currentCandle.Open = deltaValue > 0 ? 0 : absDelta;
				currentCandle.Close = deltaValue > 0 ? absDelta : 0;
				currentCandle.High = _diapasonHigh[bar];
				currentCandle.Low = _diapasonLow[bar];
				_downCandles[bar] = new Candle();
			}
			else
			{
				var currentCandle = _downCandles[bar];
				currentCandle.Open = 0;
				currentCandle.Close = absDelta;
				currentCandle.High = _diapasonHigh[bar];
				currentCandle.Low = _diapasonLow[bar];
				_candles[bar] = new Candle();
			}
		}
		else
		{
			_diapasonLow[bar] = minDelta;
			_diapasonHigh[bar] = maxDelta;

			_candles[bar].Open = 0;
			_candles[bar].Close = deltaValue;
			_candles[bar].High = maxDelta;
			_candles[bar].Low = minDelta;
		}

		var hasDivergence = false;

		if (MinimizedMode)
		{
			if (candle.Close > candle.Open && deltaValue < 0)
			{
				_downSeries[bar] = _downCandles[bar].High;
				hasDivergence = true;
			}
			else if (candle.Close < candle.Open && deltaValue > 0)
			{
				_upSeries[bar] = _candles[bar].High;
				hasDivergence = true;
			}
			else
			{
				_upSeries[bar] = 0;
				_downSeries[bar] = 0;
			}
		}
		else
		{
			if (candle.Close > candle.Open && _candles[bar].Close < _candles[bar].Open)
			{
				_downSeries[bar] = _candles[bar].High;
				hasDivergence = true;
			}
			else
				_downSeries[bar] = 0;

			if (candle.Close < candle.Open && _candles[bar].Close > _candles[bar].Open)
			{
				_upSeries[bar] = _candles[bar].High;
				hasDivergence = true;
			}
			else
				_upSeries[bar] = 0;
		}

		_divergenceBars[bar] = hasDivergence;

		if (hasDivergence && DivergenceBarsFilter != null && DivergenceBarsFilter.Enabled &&
		    (_mode == DeltaVisualMode.Candles || _mode == DeltaVisualMode.Bars))
		{
			if (MinimizedMode)
			{
				if (deltaValue >= 0)
				{
					_divergenceCandles[bar] = _candles[bar];
					_divergenceDownCandles[bar] = new Candle();
				}
				else
				{
					_divergenceDownCandles[bar] = _downCandles[bar];
					_divergenceCandles[bar] = new Candle();
				}
			}
			else
			{
				_divergenceCandles[bar] = _candles[bar];
				_divergenceDownCandles[bar] = new Candle();
			}
		}
		else
		{
			_divergenceCandles[bar] = new Candle();
			_divergenceDownCandles[bar] = new Candle();
		}

		_delta.Colors[bar] = deltaValue > 0 ? _upColor : _downColor;

		if (DivergenceBarsFilter != null && DivergenceBarsFilter.Enabled &&
		    (_mode == DeltaVisualMode.Histogram || _mode == DeltaVisualMode.HighLow))
		{
			if (hasDivergence)
			{
				var divergenceColor = DivergenceBarsFilter.Value.Convert();
				_delta.Colors[bar] = divergenceColor;
			}
		}

		if (_lastBar != bar)
		{
			_prevDeltaValue = deltaValue;
			_lastBar = bar;
		}

		if (UpAlert.Enabled && CurrentBar - 1 == bar && _lastBarAlert != bar)
		{
			var alertValue = UpAlert.Value;

			if ((deltaValue >= alertValue && _prevDeltaValue < alertValue) || (deltaValue <= alertValue && _prevDeltaValue > alertValue))
			{
				_lastBarAlert = bar;
				AddAlert(AlertFile, InstrumentInfo.Instrument, $"Delta reached {alertValue} filter", AlertBGColor, AlertForeColor);
			}
		}

		if (DownAlert.Enabled && CurrentBar - 1 == bar && _lastBarNegativeAlert != bar)
		{
			var negativeAlertValue = DownAlert.Value;

			if ((deltaValue >= negativeAlertValue && _prevDeltaValue < negativeAlertValue) ||
			    (deltaValue <= negativeAlertValue && _prevDeltaValue > negativeAlertValue))
			{
				_lastBarNegativeAlert = bar;
				AddAlert(AlertFile, InstrumentInfo.Instrument, $"Delta reached {negativeAlertValue} filter", AlertBGColor, AlertForeColor);
			}
		}

		if (_visualEnabled && CurrentBar - 1 == bar)
		{
			var visualUpTh = PickUpThreshold(bar, _visualUpLevel);
			var visualDownTh = PickDownThreshold(bar, _visualDownLevel);
			var offset = _priceSignalOffsetTicks * InstrumentInfo.TickSize;

			if (deltaValue >= visualUpTh && _prevDeltaValue < visualUpTh)
				_priceSignalUp[bar] = candle.Low - offset;

			if (deltaValue <= visualDownTh && _prevDeltaValue > visualDownTh)
				_priceSignalDown[bar] = candle.High + offset;
		}

		// --- Average delta (SMA/EMA) ---
		if (bar == 0)
		{
			_smaWindow.Clear();
			_smaSum = 0m;
			_emaInitialized = false;
			_emaValue = 0m;
		}

		if (_showAverage)
		{
			decimal avgVal;

			if (_avgMode == AverageMode.Sma)
			{
				_smaWindow.Enqueue(deltaValue);
				_smaSum += deltaValue;

				while (_smaWindow.Count > _averagePeriod)
					_smaSum -= _smaWindow.Dequeue();

				avgVal = _smaSum / Math.Max(1, _smaWindow.Count);
			}
			else // EMA
			{
				if (!_emaInitialized)
				{
					_emaValue = deltaValue;
					_emaInitialized = true;
				}
				else
				{
					var k = 2m / (_averagePeriod + 1m);
					_emaValue = (deltaValue - _emaValue) * k + _emaValue;
				}

				avgVal = _emaValue;
			}

			_avgSeries[bar] = avgVal;

			if (_avgColorMode == AverageColorMode.Fixed)
			{
				_avgSeries.Colors[bar] = _avgSeries.Color.Convert();
			}
			else if (_avgColorMode == AverageColorMode.ZeroCross)
			{
				_avgSeries.Colors[bar] = avgVal >= 0 ? _avgSlopeUpColor : _avgSlopeDownColor;
			}
			else // Slope
			{
				var prevAvg = bar > 0 ? _avgSeries[bar - 1] : avgVal;
				_avgSeries.Colors[bar] = avgVal >= prevAvg ? _avgSlopeUpColor : _avgSlopeDownColor;
			}
		}
		else
		{
			_avgSeries[bar] = 0m;
		}

		// --- Audio alerts (edge or bar-close confirmation) ---
		if (_audioEnabled && InstrumentInfo is not null)
		{
			var audioUpTh = PickUpThreshold(bar, _audioUpLevel);
			var audioDownTh = PickDownThreshold(bar, _audioDownLevel);

			if (!_audioAtBarCloseOnly)
			{
				if (_prevDeltaValue < audioUpTh && deltaValue >= audioUpTh
					&& bar - _lastBarAudioUpAlert >= _alertCooldownBars)
				{
					_lastBarAudioUpAlert = bar;
					TryAddAudioAlert($"Delta >= {audioUpTh} (UP)");
				}

				if (_prevDeltaValue > audioDownTh && deltaValue <= audioDownTh
					&& bar - _lastBarAudioDownAlert >= _alertCooldownBars)
				{
					_lastBarAudioDownAlert = bar;
					TryAddAudioAlert($"Delta <= {audioDownTh} (DOWN)");
				}
			}
			else if (bar > 0 && bar == CurrentBar - 1)
			{
				var closedBar = bar - 1;
				var closedDelta = _delta[closedBar];

				if (closedDelta >= audioUpTh && closedBar - _lastBarAudioUpAlert >= _alertCooldownBars)
				{
					_lastBarAudioUpAlert = closedBar;
					TryAddAudioAlert($"Delta CLOSE >= {audioUpTh} (UP)");
				}
				else if (closedDelta <= audioDownTh && closedBar - _lastBarAudioDownAlert >= _alertCooldownBars)
				{
					_lastBarAudioDownAlert = closedBar;
					TryAddAudioAlert($"Delta CLOSE <= {audioDownTh} (DOWN)");
				}
			}
		}

		// --- Threshold lines (fixed or dynamic) ---
		UpdateDynamicThresholdState(bar, candle);

		_prevDeltaValue = deltaValue;

		if (Absorption.Enabled)
		{
			decimal deltaOpen, deltaClose, deltaHigh, deltaLow;

			if (MinimizedMode)
			{
				deltaClose = _delta[bar];
				deltaHigh = _diapasonHigh[bar];
				deltaLow = _diapasonLow[bar];
				deltaOpen = 0;
			}
			else
			{
				deltaOpen = _candles[bar].Open;
				deltaClose = _candles[bar].Close;
				deltaHigh = _candles[bar].High;
				deltaLow = _candles[bar].Low;
			}

			decimal upperTail, lowerTail;

			if (deltaClose > deltaOpen)
			{
				upperTail = deltaHigh - deltaClose;
				lowerTail = deltaOpen - deltaLow;
			}
			else
			{
				upperTail = deltaHigh - deltaOpen;
				lowerTail = deltaClose - deltaLow;
			}

			if (upperTail > lowerTail && upperTail > Absorption.Value)
			{
				var c = new Candle();
				var center = deltaHigh - upperTail / 2;
				var halfSize = upperTail / 4;
				var halfSizeOuter = halfSize * 1.2m;
				c.Open = center + halfSize;
				c.Close = center - halfSize;
				c.High = center + halfSizeOuter;
				c.Low = center - halfSizeOuter;
				_absorptionCandles[bar] = c;
			}
			else if (lowerTail > upperTail && lowerTail > Absorption.Value)
			{
				var c = new Candle();
				var center = deltaLow + lowerTail / 2;
				var halfSize = lowerTail / 4;
				var halfSizeOuter = halfSize * 1.2m;
				c.Open = center - halfSize;
				c.Close = center + halfSize;
				c.High = center + halfSizeOuter;
				c.Low = center - halfSizeOuter;
				_absorptionCandles[bar] = c;
			}
			else
				_absorptionCandles[bar] = new Candle();
		}
		else
			_absorptionCandles[bar] = new Candle();

		if (!ShowCurrentValues)
			return;

		_currentValues[bar] = MinimizedMode ? absDelta : deltaValue;

		_currentValues.Colors[bar] = deltaValue > 0
			? _upColor
			: deltaValue < 0
				? _downColor
				: _neutralColor;
	}

    #endregion

    #region Private methods

	private void EnsureReadyCapacity(int bar)
	{
		while (_posReadyByBar.Count <= bar) _posReadyByBar.Add(false);
		while (_negReadyByBar.Count <= bar) _negReadyByBar.Add(false);
	}

	private void ResetDynamicState()
	{
		_posAcc.Reset();
		_negAcc.Reset();

		_posReady = false;
		_negReady = false;

		_dynPosMinor = _dynPosMajor = 0;
		_dynNegMinor = _dynNegMajor = 0;
	}

	private bool InSession(DateTime exchangeTimeUtc)
	{
		if (SessionMode == SessionWindowMode.Full24h)
			return true;

		var tLocal = exchangeTimeUtc.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
		return tLocal >= RthStart && tLocal <= RthEnd;
	}

	private bool IsSessionStart(int bar)
	{
		if (bar == 0)
			return true;

		var prevUtc = GetCandle(bar - 1).Time;
		var currUtc = GetCandle(bar).Time;

		var prevIn = InSession(prevUtc);
		var currIn = InSession(currUtc);

		if (!prevIn && currIn)
			return true;

		if (currIn && prevUtc.Date != currUtc.Date)
		{
			var tLocal = currUtc.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
			if (tLocal >= RthStart && tLocal <= RthEnd)
				return true;
		}

		return false;
	}

	// No look-ahead: thresholds for bar b computed from state up to b-1; bar b fed after writing.
	private void UpdateDynamicThresholdState(int bar, IndicatorCandle candle)
	{
		if (Thresholds != ThresholdSource.DynamicWelford)
		{
			// Fixed: write constant levels every bar
			_upMajor[bar] = _upMajorLevel;
			_upMinor[bar] = _upMinorLevel;
			_dnMinor[bar] = _downMinorLevel;
			_dnMajor[bar] = _downMajorLevel;
			return;
		}

		if (IsSessionStart(bar))
		{
			ResetDynamicState();
			CutAllThresholdsAt(bar - 1);
		}

		var inside = InSession(candle.Time);
		EnsureReadyCapacity(bar);

		if (inside)
		{
			_posReady = _posAcc.Count >= SamplesForMeanStd;
			_negReady = _negAcc.Count >= SamplesForMeanStd;

			_posReadyByBar[bar] = _posReady;
			_negReadyByBar[bar] = _negReady;

			if (_posReady)
			{
				var k = StdMultiplier;
				_dynPosMinor = _posAcc.Mean;
				_dynPosMajor = _posAcc.Mean + k * _posAcc.Std();
			}
			else
				_dynPosMinor = _dynPosMajor = 0m;

			if (_negReady)
			{
				var k = StdMultiplier;
				_dynNegMinor = -_negAcc.Mean;
				_dynNegMajor = -(_negAcc.Mean + k * _negAcc.Std());
			}
			else
				_dynNegMinor = _dynNegMajor = 0m;

			// Write to series (enables PickUpThreshold to read per-bar history)
			_upMinor[bar] = _dynPosMinor;
			_upMajor[bar] = _dynPosMajor;
			_dnMinor[bar] = _dynNegMinor;
			_dnMajor[bar] = _dynNegMajor;

			// Feed current bar extremes AFTER writing (no look-ahead)
			if (TryGetPositiveExtremeSample(candle, out var posSample))
				_posAcc.Add(posSample);

			if (TryGetNegativeMagnitudeExtremeSample(candle, out var negAbsSample))
				_negAcc.Add(negAbsSample);

			if (!_posReady) CutUpThresholdsAt(bar - 1);
			if (!_negReady) CutDownThresholdsAt(bar - 1);
		}
		else
		{
			_posReadyByBar[bar] = false;
			_negReadyByBar[bar] = false;
			CutAllThresholdsAt(bar - 1);
		}
	}

	private decimal PickUpThreshold(int bar, ThresholdLevel level)
	{
		var t = GetCandle(bar).Time;

		if (Thresholds == ThresholdSource.DynamicWelford && !InSession(t))
			return decimal.MaxValue;

		if (Thresholds == ThresholdSource.Fixed)
			return (level == ThresholdLevel.Major) ? UpMajorLevel : UpMinorLevel;

		if (bar < 0 || bar >= _posReadyByBar.Count || !_posReadyByBar[bar])
			return decimal.MaxValue;

		return (level == ThresholdLevel.Major) ? _upMajor[bar] : _upMinor[bar];
	}

	private decimal PickDownThreshold(int bar, ThresholdLevel level)
	{
		var t = GetCandle(bar).Time;

		if (Thresholds == ThresholdSource.DynamicWelford && !InSession(t))
			return decimal.MinValue;

		if (Thresholds == ThresholdSource.Fixed)
			return (level == ThresholdLevel.Major) ? DownMajorLevel : DownMinorLevel;

		if (bar < 0 || bar >= _negReadyByBar.Count || !_negReadyByBar[bar])
			return decimal.MinValue;

		return (level == ThresholdLevel.Major) ? _dnMajor[bar] : _dnMinor[bar];
	}

	private void CutAllThresholdsAt(int bar)
	{
		CutUpThresholdsAt(bar);
		CutDownThresholdsAt(bar);
	}

	private void CutUpThresholdsAt(int bar)
	{
		if (bar >= 0)
		{
			_upMajor[bar] = 0m;
			_upMinor[bar] = 0m;
		}
	}

	private void CutDownThresholdsAt(int bar)
	{
		if (bar >= 0)
		{
			_dnMinor[bar] = 0m;
			_dnMajor[bar] = 0m;
		}
	}

	private int GetMinWidth(RenderContext context, int startBar, int endBar)
	{
		var maxLength = 0;

		for (var i = startBar; i <= endBar; i++)
		{
			decimal value;

			if (MinimizedMode)
			{
				value = _candles[i].Close > _candles[i].Open
					? _candles[i].Close
					: -_candles[i].Open;
			}
			else
				value = _candles[i].Close;

			var length = $"{value:0.#####}".Length;

			if (length > maxLength)
				maxLength = length;
		}

		var sampleStr = "";

		for (var i = 0; i < maxLength; i++)
			sampleStr += '0';

		return context.MeasureString(sampleStr, Font.RenderObject).Width;
	}

	private static int ClampInt(int value, int min, int max)
	{
		if (value < min) return min;
		if (value > max) return max;
		return value;
	}

    #endregion

    #region Event handlers

	private void TryAddAudioAlert(string message)
	{
		try
		{
			var symbol = InstrumentInfo?.Instrument ?? "Delta";
			AddAlert(AlertFile, symbol, message, AlertBGColor, AlertForeColor);
		}
		catch
		{
			// Intentionally swallow: alert infrastructure must not break indicator calculation.
		}
	}

	private void UpdateThresholdLinesVisibility(bool repaint)
	{
		var vis = _showThresholdLines ? VisualMode.Line : VisualMode.Hide;

		_upMajor.VisualType = vis;
		_upMinor.VisualType = vis;
		_dnMinor.VisualType = vis;
		_dnMajor.VisualType = vis;

		if (repaint)
			RedrawChart();
	}

	private void OnDivergenceFilterChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(Indicators.FilterColor.Enabled))
			ApplyDivergenceColorsToCurrentMode();

		UpdateDivergenceCandlesVisibility();

		RecalculateValues();

		RedrawChart();
	}

	private void OnAbsorptionFilterChanged(object sender, PropertyChangedEventArgs e)
	{
		RecalculateValues();
		RedrawChart();
	}

	private void ApplyDivergenceColorsToCurrentMode()
	{
		if (_divergenceBarsFilter?.Enabled != true)
		{
			for (var i = 0; i < CurrentBar; i++)
			{
				var actualDelta = GetCandle(i).Delta;
				var defaultColor = actualDelta > 0 ? _upColor : _downColor;
				_delta.Colors[i] = defaultColor;
			}

			return;
		}

		var divergenceColor = _divergenceBarsFilter.Value.Convert();

		for (var i = 0; i < CurrentBar; i++)
		{
			if (_divergenceBars.TryGetValue(i, out var hasDivergence) && hasDivergence)
			{
				if (_mode == DeltaVisualMode.Histogram || _mode == DeltaVisualMode.HighLow)
					_delta.Colors[i] = divergenceColor;
			}
			else
			{
				var actualDelta = GetCandle(i).Delta;
				var defaultColor = actualDelta > 0 ? _upColor : _downColor;
				_delta.Colors[i] = defaultColor;
			}
		}
	}

	private void UpdateDivergenceCandlesVisibility()
	{
		if (DivergenceBarsFilter != null && (_mode == DeltaVisualMode.Candles || _mode == DeltaVisualMode.Bars))
		{
			var divergenceColor = DivergenceBarsFilter.Value;
			_divergenceCandles.Visible = DivergenceBarsFilter.Enabled;
			_divergenceCandles.UpCandleColor = divergenceColor;
			_divergenceCandles.DownCandleColor = divergenceColor;
			_divergenceCandles.BorderColor = divergenceColor;
			_divergenceCandles.Mode = _mode == DeltaVisualMode.Candles ? CandleVisualMode.Candles : CandleVisualMode.Bars;

			_divergenceDownCandles.Visible = DivergenceBarsFilter.Enabled && MinimizedMode;
			_divergenceDownCandles.UpCandleColor = divergenceColor;
			_divergenceDownCandles.DownCandleColor = divergenceColor;
			_divergenceDownCandles.BorderColor = divergenceColor;
			_divergenceDownCandles.Mode = _mode == DeltaVisualMode.Candles ? CandleVisualMode.Candles : CandleVisualMode.Bars;
		}
		else
		{
			_divergenceCandles.Visible = false;
			_divergenceDownCandles.Visible = false;
		}
	}

    #endregion
}