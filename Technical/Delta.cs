namespace ATAS.Indicators.Technical;

using ATAS.Indicators.Technical.Properties;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

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
	public enum ThresholdLevel
	{
		Major = 0,
		Minor = 1
	}

	[Serializable]
	public enum ThresholdSource
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSourceFixed))]
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
	public enum AverageMode
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SMA))]
		Sma = 0,
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.EMA))]
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
		DrawCandleBorder = false
	};

	private readonly CandleDataSeries _divergenceDownCandles = new("DivergenceDownCandles", "Divergence down candles")
	{
		IsHidden = true,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		Visible = false,
		DrawCandleBorder = false
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

		// Default style (Major = solid)
		Color = CrossColor.FromArgb(255, 169, 169, 169), // DarkGray
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

		// Default style (Minor = dotted)
		Color = CrossColor.FromArgb(255, 105, 105, 105), // DimGray
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

		// Default style (Minor = dotted)
		Color = CrossColor.FromArgb(255, 105, 105, 105), // DimGray
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

		// Default style (Major = solid)
		Color = CrossColor.FromArgb(255, 169, 169, 169), // DarkGray
		Width = 1,
		LineDashStyle = LineDashStyle.Solid
	};

	private bool _showThresholdLines = true;

	// Defaults
	private int _upMajorLevel = 300;
	private int _upMinorLevel = 200;
	private int _downMinorLevel = -200;
	private int _downMajorLevel = -300;

	#endregion

	#region Fields (dynamic thresholds)

	// Gate: how many useful samples are needed to compute mean/std reliably (Welford)
	private int _samplesForMeanStd = 1;

	// Per-side readiness flags (updated each bar)
	private bool _posReady;
	private bool _negReady;

	// Readiness by bar (needed by pickers to avoid repaint / out-of-session triggers)
	private readonly List<bool> _posReadyByBar = new();
	private readonly List<bool> _negReadyByBar = new();

	// Welford accumulators (positive extremes and negative magnitude extremes)
	private int _posN;
	private decimal _posMean;
	private decimal _posM2;

	private int _negN;
	private decimal _negMean;
	private decimal _negM2;

	// Last computed dynamic thresholds (cached for the current bar)
	private decimal _dynPosMinor, _dynPosMajor;
	private decimal _dynNegMinor, _dynNegMajor;

	// Session config
	private SessionWindowMode _sessionMode = SessionWindowMode.RTH;
	private TimeSpan _rthStart = new(9, 30, 0);
	private TimeSpan _rthEnd = new(16, 0, 0);
	private decimal _stdMultiplier = 1.0m;

	// Threshold source
	private ThresholdSource _thresholds = ThresholdSource.Fixed;

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

	private bool _visualEnabled = true;
	private int _priceSignalOffsetTicks = 2;
	private int _priceSignalSize = 10;
	private Color _priceSignalUpColor = Color.Lime;
	private Color _priceSignalDownColor = Color.Fuchsia;

	private ThresholdLevel _visualUpLevel = ThresholdLevel.Major;
	private ThresholdLevel _visualDownLevel = ThresholdLevel.Major;

	#endregion

	#region Fields (audio alerts)

	private bool _audioEnabled = false;
	private bool _audioAtBarCloseOnly = true;

	private ThresholdLevel _audioUpLevel = ThresholdLevel.Major;
	private ThresholdLevel _audioDownLevel = ThresholdLevel.Major;

	private int _lastBarAudioUpAlert;
	private int _lastBarAudioDownAlert;

	#endregion

	#region Fields (average delta)

	private readonly ValueDataSeries _avgSeries = new("AverageDelta", "Average")
	{
		VisualType = VisualMode.Hide,
		Width = 2,
		Color = CrossColor.FromArgb(255, 60, 120, 240),
		ShowCurrentValue = false,
		IsHidden = true
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

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BullishColor), GroupName = nameof(Strings.Drawing),
		Description = nameof(Strings.PositiveValueColorDescription), Order = 40)]
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

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BearlishColor), GroupName = nameof(Strings.Drawing),
		Description = nameof(Strings.NegativeValueColorDescription), Order = 50)]
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

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.NeutralBorderColor), GroupName = nameof(Strings.Drawing),
		Description = nameof(Strings.NeutralValueDescription), Order = 60)]
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

	[Display(ResourceType = typeof(Strings), Name = "DivergenceDots", GroupName = nameof(Strings.Divergence),
		Description = nameof(Strings.BarDirVsDeltaDivergenceDescription), Order = 130)]
	public bool ShowDivergence { get; set; }

	[Display(ResourceType = typeof(Strings), Name = "DivergenceBars", GroupName = nameof(Strings.Divergence), Order = 135)]
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSource), Description = nameof(Resources.ThresholdSourceDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowMode), Description = nameof(Resources.SessionWindowModeDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthStart), Description = nameof(Resources.RthStartDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthEnd), Description = nameof(Resources.RthEndDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.StdMultiplier), Description = nameof(Resources.StdMultiplierDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SamplesForMeanStd), Description = nameof(Resources.SamplesForMeanStdDescription),
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


	#region Threshold lines (fixed)

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowThresholdLines), Description = nameof(Resources.ShowThresholdLinesDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedMajorLevel), Description = nameof(Resources.FixedMajorLevelDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedMinorLevel), Description = nameof(Resources.FixedMinorLevelDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedMinorLevel), Description = nameof(Resources.FixedMinorLevelDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedMajorLevel), Description = nameof(Resources.FixedMajorLevelDescription),
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

	#region Absorption

	private readonly CandleDataSeries _absorptionCandles = new("AbsorptionDotsCandles", "Absorption Dots")
	{
		UpCandleColor = Color.Green.Convert(),
		DownCandleColor = Color.Red.Convert(),
		BorderColor = CrossColor.FromArgb(0, 0, 0, 0),
		IsHidden = false,
		UseMinimizedModeIfEnabled = true,
		ShowCurrentValue = false
	};

	private int _absorptionThreshold = 250;

	private FilterInt _absorption = new(true) { Enabled = false, Value = 250 };

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Absorption), GroupName = nameof(Strings.Absorption),
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

	#region Volume

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Show), GroupName = nameof(Strings.VolumeLabel), Order = 200,
		Description = nameof(Strings.VolumeLabelDescription))]
	public bool ShowVolume { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.VolumeLabel),
		Description = nameof(Strings.LabelTextColorDescription), Order = 210)]
	public CrossColor FontColor
	{
		get => _fontColor.Convert();
		set => _fontColor = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Location), GroupName = nameof(Strings.VolumeLabel),
		Description = nameof(Strings.LabelLocationDescription), Order = 220)]
	public Location VolLocation { get; set; } = Location.Middle;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Font), GroupName = nameof(Strings.VolumeLabel),
		Description = nameof(Strings.FontSettingDescription), Order = 230)]
	public FontSetting Font { get; set; } = new("Arial", 10);

	#endregion

	#region Visual alerts (price panel)

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.VisualUpThresholds), Description = nameof(Resources.VisualUpThresholdsDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.VisualDownThresholds), Description = nameof(Resources.VisualDownThresholdsDescription),
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


	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowVisualAlerts), Description = nameof(Resources.ShowVisualAlertsDescription), 
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.VerticalOffset), Description = nameof(Resources.PriceSignalOffsetTicksDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.Size), Description = nameof(Resources.PriceSignalSizeDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.PriceSignalUpColor), Description = nameof(Resources.PriceSignalUpColorDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.PriceSignalDownColor), Description = nameof(Resources.PriceSignalDownColorDescription),
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

	#region Audio alerts

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioAlerts), Description = nameof(Resources.AudioAlertsDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioUpThresholds), Description = nameof(Resources.AudioUpThresholdsDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioDownThresholds), Description = nameof(Resources.AudioDownThresholdsDescription),
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.AudioAtBarCloseOnly), Description = nameof(Resources.AudioAtBarCloseOnlyDescription),
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

	#endregion

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.UpAlert), GroupName = nameof(Strings.Alerts),
		Description = nameof(Strings.UpAlertFileFilterDescription), Order = 300)]
	[Range(0, int.MaxValue)]
	[DisplayFormat(DataFormatString = "F0")]
	public Filter UpAlert { get; set; } = new()
	{ Enabled = false, Value = 0 };

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.DownAlert), GroupName = nameof(Strings.Alerts),
		Description = nameof(Strings.DownAlertFileFilterDescription), Order = 310)]
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

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.Width),
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
		DataSeries.Add(_avgSeries);

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

		// fixed threshold lines
		DataSeries.Add(_upMajor);
		DataSeries.Add(_upMinor);
		DataSeries.Add(_dnMinor);
		DataSeries.Add(_dnMajor);

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

						if (yPrice <= ChartInfo.PriceChartContainer.Region.Bottom)
						{
							var rect = new Rectangle(x - 5, yPrice - 4, 8, 8);
							context.FillEllipse(_upColor, rect);
						}
					}

					if (_downSeries[i] != 0)
					{
						var yPrice = ChartInfo.PriceChartContainer.GetYByPrice(candle.High, false) - 10;

						if (yPrice <= ChartInfo.PriceChartContainer.Region.Bottom)
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
			ResetStateOnFirstBar();

		ClearPriceSignals(bar);

		var candle = GetCandle(bar);

		var deltaValue = candle.Delta;
		var absDelta = Math.Abs(deltaValue);
		var maxDelta = candle.MaxDelta;
		var minDelta = candle.MinDelta;

		NormalizeDiapason(ref maxDelta, ref minDelta);

		if (ApplyFilters(ref deltaValue, ref absDelta, ref maxDelta, ref minDelta, candle))
		{
			// filtered to zero inside ApplyFilters
		}

		WriteMainSeries(bar, candle, deltaValue, absDelta, maxDelta, minDelta);

		var hasDivergence = UpdateDivergence(bar, candle, deltaValue);
		UpdateDivergenceCandles(bar, candle, deltaValue, hasDivergence);

		UpdateDeltaColors(bar, deltaValue, hasDivergence);

		SyncPrevDeltaOnNewBar(bar, deltaValue);

		ProcessLegacyAlerts(bar, deltaValue);

		UpdateVisualSignals(bar, candle, deltaValue);

		UpdateAudioAlerts(bar, deltaValue);

		// Update _prevDeltaValue ONCE at the end (used by edge/cross logic)
		_prevDeltaValue = deltaValue;

		UpdateThresholdLines(bar, candle);

		UpdateAverage(bar, deltaValue);

		UpdateAbsorption(bar, absDelta, deltaValue);

		UpdateCurrentValues(bar, absDelta, deltaValue);
	}


	#endregion

	#region Private methods

	#region OnCalculate helpers (no behavior change)

	private void ResetStateOnFirstBar()
	{
		DataSeries.ForEach(x => x.Clear());
		_upSeries.Clear();
		_downSeries.Clear();
		_divergenceBars.Clear();

		// Keep auxiliary state consistent with recalculations
		_posReadyByBar.Clear();
		_negReadyByBar.Clear();
		ResetDynamicState();

		_smaWindow.Clear();
		_smaSum = 0m;
		_emaInitialized = false;
		_emaValue = 0m;
	}

	private void ClearPriceSignals(int bar)
	{
		// Always clear per bar to avoid stale markers ("ghosts")
		_priceSignalUp[bar] = 0m;
		_priceSignalDown[bar] = 0m;
	}

	private static void NormalizeDiapason(ref decimal maxDelta, ref decimal minDelta)
	{
		if (maxDelta == minDelta)
		{
			if (maxDelta > 0)
				minDelta = 0;
			else
				maxDelta = 0;
		}
	}

	private bool ApplyFilters(
		ref decimal deltaValue,
		ref decimal absDelta,
		ref decimal maxDelta,
		ref decimal minDelta,
		IndicatorCandle candle)
	{
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

		if (!isUnderFilter)
			return false;

		deltaValue = 0;
		absDelta = 0;
		minDelta = maxDelta = 0;
		return true;
	}

	private void WriteMainSeries(
		int bar,
		IndicatorCandle candle,
		decimal deltaValue,
		decimal absDelta,
		decimal maxDelta,
		decimal minDelta)
	{
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
	}

	private bool UpdateDivergence(int bar, IndicatorCandle candle, decimal deltaValue)
	{
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
		return hasDivergence;
	}

	private void UpdateDivergenceCandles(int bar, IndicatorCandle candle, decimal deltaValue, bool hasDivergence)
	{
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
	}

	private void UpdateDeltaColors(int bar, decimal deltaValue, bool hasDivergence)
	{
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
	}

	private void SyncPrevDeltaOnNewBar(int bar, decimal deltaValue)
	{
		if (_lastBar != bar)
		{
			_prevDeltaValue = deltaValue;
			_lastBar = bar;
		}
	}

	private void ProcessLegacyAlerts(int bar, decimal deltaValue)
	{
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
	}

	private void UpdateVisualSignals(int bar, IndicatorCandle candle, decimal deltaValue)
	{
		// --- Visual signals (price panel): threshold selection per side ---
		if (_visualEnabled && InstrumentInfo is not null)
		{
			var upTh = PickUpThreshold(bar, _visualUpLevel);
			var dnTh = PickDownThreshold(bar, _visualDownLevel);

			var offset = _priceSignalOffsetTicks * InstrumentInfo.TickSize;

			// Match DeltaModif semantics: up marker above High, down marker below Low.
			if (deltaValue >= upTh)
				_priceSignalUp[bar] = candle.High + offset;

			if (deltaValue <= dnTh)
				_priceSignalDown[bar] = candle.Low - offset;
		}
	}

	private void UpdateAudioAlerts(int bar, decimal deltaValue)
	{
		// --- Audio alerts (edge or bar-close confirmation) ---
		if (_audioEnabled && InstrumentInfo is not null)
		{
			var audioUpTh = PickUpThreshold(bar, _audioUpLevel);
			var audioDownTh = PickDownThreshold(bar, _audioDownLevel);

			// Case 1: Instant audio (intra-bar cross)
			if (!_audioAtBarCloseOnly)
			{
				if (_prevDeltaValue < audioUpTh && deltaValue >= audioUpTh && _lastBarAudioUpAlert != bar)
				{
					_lastBarAudioUpAlert = bar;
					TryAddAudioAlert($"Delta >= {audioUpTh} (UP)");
				}

				if (_prevDeltaValue > audioDownTh && deltaValue <= audioDownTh && _lastBarAudioDownAlert != bar)
				{
					_lastBarAudioDownAlert = bar;
					TryAddAudioAlert($"Delta <= {audioDownTh} (DOWN)");
				}
			}
			// Case 2: Audio only at confirmed bar close
			else
			{
				// Run when we are calculating the previous bar (already closed)
				if (bar == CurrentBar - 2)
				{
					var prevBarDelta = _delta[bar];

					if (prevBarDelta >= audioUpTh && _lastBarAudioUpAlert != bar)
					{
						_lastBarAudioUpAlert = bar;
						TryAddAudioAlert($"Delta CLOSE >= {audioUpTh} (UP)");
					}
					else if (prevBarDelta <= audioDownTh && _lastBarAudioDownAlert != bar)
					{
						_lastBarAudioDownAlert = bar;
						TryAddAudioAlert($"Delta CLOSE <= {audioDownTh} (DOWN)");
					}
				}
			}
		}
	}

	private void UpdateThresholdLines(int bar, IndicatorCandle candle)
	{
		// --- Threshold lines (Fixed / DynamicWelford, session-anchored, no look-ahead) ---
		if (Thresholds == ThresholdSource.DynamicWelford && IsSessionStart(bar))
		{
			// Reset accumulators AND cut all threshold polylines at the bar before the start
			ResetDynamicState();
			CutAllThresholdsAt(bar - 1);
		}

		var inside = (Thresholds == ThresholdSource.DynamicWelford)
			? InSession(candle.Time)
			: true;

		if (Thresholds == ThresholdSource.Fixed)
		{
			_upMajor[bar] = _upMajorLevel;
			_upMinor[bar] = _upMinorLevel;
			_dnMinor[bar] = _downMinorLevel;
			_dnMajor[bar] = _downMajorLevel;
		}
		else // DynamicWelford
		{
			EnsureReadyCapacity(bar);

			if (inside)
			{
				// POS side thresholds from previous state
				if (_posN >= SamplesForMeanStd)
				{
					var m = _posMean;
					var s = WelfordStd(_posN, _posM2);
					var k = StdMultiplier;

					_dynPosMinor = m;         // mean
					_dynPosMajor = m + k * s; // mean + k*std
					_posReady = true;
				}
				else
				{
					_dynPosMinor = _dynPosMajor = 0;
					_posReady = false;
				}

				// NEG side thresholds from previous state (signed negatives based on magnitude stats)
				if (_negN >= SamplesForMeanStd)
				{
					var mAbs = _negMean;
					var sAbs = WelfordStd(_negN, _negM2);
					var k = StdMultiplier;

					_dynNegMinor = -mAbs;              // -mean
					_dynNegMajor = -(mAbs + k * sAbs); // -(mean + k*std)
					_negReady = true;
				}
				else
				{
					_dynNegMinor = _dynNegMajor = 0;
					_negReady = false;
				}

				_posReadyByBar[bar] = _posReady;
				_negReadyByBar[bar] = _negReady;

				// Write series values for this bar (used by pickers/history)
				_upMinor[bar] = _dynPosMinor;
				_upMajor[bar] = _dynPosMajor;
				_dnMinor[bar] = _dynNegMinor;
				_dnMajor[bar] = _dynNegMajor;

				// Now feed current bar extremes (AFTER thresholds were computed)
				if (TryGetPositiveExtremeSample(GetCandle(bar), out var posSample))
					WelfordPush(ref _posN, ref _posMean, ref _posM2, posSample);

				if (TryGetNegativeMagnitudeExtremeSample(GetCandle(bar), out var negAbsSample))
					WelfordPush(ref _negN, ref _negMean, ref _negM2, negAbsSample);

				// Hide portions of the polyline until readiness is achieved
				if (!_posReady) CutUpThresholdsAt(bar - 1);
				if (!_negReady) CutDownThresholdsAt(bar - 1);
			}
			else
			{
				// Outside session: cut the four lines and skip writing values this bar
				_posReadyByBar[bar] = false;
				_negReadyByBar[bar] = false;
				CutAllThresholdsAt(bar - 1);
			}
		}
	}

	private void UpdateAverage(int bar, decimal deltaValue)
	{
		// --- Average delta (SMA/EMA) ---
		if (_showAverage)
		{
			var sample = deltaValue; // signed, already filtered (no repaint semantics)

			decimal smaVal;
			decimal emaVal;

			// SMA
			_smaWindow.Enqueue(sample);
			_smaSum += sample;

			while (_smaWindow.Count > _averagePeriod)
				_smaSum -= _smaWindow.Dequeue();

			smaVal = _smaSum / Math.Max(1, _smaWindow.Count);

			// EMA
			if (!_emaInitialized)
			{
				_emaValue = sample;
				_emaInitialized = true;
			}
			else
			{
				var k = 2m / (_averagePeriod + 1m);
				_emaValue = (sample - _emaValue) * k + _emaValue;
			}

			emaVal = _emaValue;

			var avgVal = (_avgMode == AverageMode.Sma) ? smaVal : emaVal;

			_avgSeries[bar] = avgVal;

			if (_avgColorMode == AverageColorMode.Fixed)
			{
				_avgSeries.Colors[bar] = _avgSeries.Color.Convert();
			}
			else if (_avgColorMode == AverageColorMode.ZeroCross)
			{
				_avgSeries.Colors[bar] = (avgVal >= 0m) ? _avgSlopeUpColor : _avgSlopeDownColor;
			}
			else // Slope
			{
				if (bar > 0)
				{
					var prevAvg = _avgSeries[bar - 1];
					_avgSeries.Colors[bar] = (avgVal >= prevAvg) ? _avgSlopeUpColor : _avgSlopeDownColor;
				}
				else
				{
					_avgSeries.Colors[bar] = _avgSeries.Color.Convert();
				}
			}
		}
		else
		{
			_avgSeries[bar] = 0m;
		}
	}

	private void UpdateAbsorption(int bar, decimal absDelta, decimal deltaValue)
	{
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
				c.Open = center + 10;
				c.Close = center - 10;
				c.High = center + 12;
				c.Low = center - 12;
				_absorptionCandles[bar] = c;
			}
			else if (lowerTail > upperTail && lowerTail > Absorption.Value)
			{
				var c = new Candle();
				var center = deltaLow + lowerTail / 2;
				c.Open = center - 10;
				c.Close = center + 10;
				c.High = center + 12m;
				c.Low = center - 12m;
				_absorptionCandles[bar] = c;
			}
			else
				_absorptionCandles[bar] = new Candle();
		}
		else
			_absorptionCandles[bar] = new Candle();
	}

	private void UpdateCurrentValues(int bar, decimal absDelta, decimal deltaValue)
	{
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

	private static int ClampInt(int value, int min, int max)
	{
		if (value < min) return min;
		if (value > max) return max;
		return value;
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

	private decimal PickUpThreshold(int bar, ThresholdLevel level)
	{
		var t = GetCandle(bar).Time;

		if (Thresholds == ThresholdSource.DynamicWelford && !InSession(t))
			return decimal.MaxValue; // disable outside session

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
			return decimal.MinValue; // disable outside session

		if (Thresholds == ThresholdSource.Fixed)
			return (level == ThresholdLevel.Major) ? DownMajorLevel : DownMinorLevel;

		if (bar < 0 || bar >= _negReadyByBar.Count || !_negReadyByBar[bar])
			return decimal.MinValue;

		return (level == ThresholdLevel.Major) ? _dnMajor[bar] : _dnMinor[bar];
	}

	private void TryAddAudioAlert(string message)
	{
		try
		{
			// Reuse the existing alert sound channel configured by AlertFile.
			// If InstrumentInfo is null (shouldn't happen here), fall back to a safe label.
			var symbol = InstrumentInfo?.Instrument ?? "Delta";
			AddAlert(AlertFile, symbol, message, AlertBGColor, AlertForeColor);
		}
		catch
		{
			// Intentionally swallow: alert infrastructure must not break indicator calculation.
		}
	}



	#region Dynamic threshold private methods

	private static bool TryGetPositiveExtremeSample(IndicatorCandle candle, out decimal sample)
	{
		var x = candle.MaxDelta;
		if (x > 0) { sample = x; return true; }

		sample = 0;
		return false;
	}

	private static bool TryGetNegativeMagnitudeExtremeSample(IndicatorCandle candle, out decimal sampleAbs)
	{
		var x = candle.MinDelta;
		if (x < 0) { sampleAbs = Math.Abs(x); return true; }

		sampleAbs = 0;
		return false;
	}

	private void EnsureReadyCapacity(int bar)
	{
		while (_posReadyByBar.Count <= bar) _posReadyByBar.Add(false);
		while (_negReadyByBar.Count <= bar) _negReadyByBar.Add(false);
	}

	private void ResetDynamicState()
	{
		_posN = 0;
		_posMean = 0;
		_posM2 = 0;

		_negN = 0;
		_negMean = 0;
		_negM2 = 0;

		_posReady = false;
		_negReady = false;

		_dynPosMinor = _dynPosMajor = 0;
		_dynNegMinor = _dynNegMajor = 0;
	}

	// Returns true if bar timestamp (adjusted to instrument TZ) is inside the session window
	private bool InSession(DateTime exchangeTimeUtc)
	{
		if (SessionMode == SessionWindowMode.Full24h)
			return true;

		// Same idea as Initial Balance: candle.Time is exchange UTC and we shift by InstrumentInfo.TimeZone
		var tLocal = exchangeTimeUtc.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
		return tLocal >= RthStart && tLocal <= RthEnd;
	}

	// Detects the first bar inside a new session to reset accumulators
	private bool IsSessionStart(int bar)
	{
		if (bar == 0)
			return true;

		var prevUtc = GetCandle(bar - 1).Time;
		var currUtc = GetCandle(bar).Time;

		var prevIn = InSession(prevUtc);
		var currIn = InSession(currUtc);

		// Start when we enter the session window from outside
		if (!prevIn && currIn)
			return true;

		// Day change while still in window: re-anchor at first RTH bar of the new day
		if (currIn && prevUtc.Date != currUtc.Date)
		{
			var tLocal = currUtc.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
			if (tLocal >= RthStart && tLocal <= RthEnd)
				return true;
		}

		return false;
	}

	private void CutAllThresholdsAt(int bar /* inclusive end */)
	{
		var b = Math.Max(0, bar);
		_upMajor.SetPointOfEndLine(b);
		_upMinor.SetPointOfEndLine(b);
		_dnMinor.SetPointOfEndLine(b);
		_dnMajor.SetPointOfEndLine(b);
	}

	private void CutUpThresholdsAt(int bar)
	{
		var b = Math.Max(0, bar);
		_upMajor.SetPointOfEndLine(b);
		_upMinor.SetPointOfEndLine(b);
	}

	private void CutDownThresholdsAt(int bar)
	{
		var b = Math.Max(0, bar);
		_dnMajor.SetPointOfEndLine(b);
		_dnMinor.SetPointOfEndLine(b);
	}

	private static void WelfordPush(ref int n, ref decimal mean, ref decimal m2, decimal x)
	{
		n++;
		var delta = x - mean;
		mean += delta / n;
		var delta2 = x - mean;
		m2 += delta * delta2;
	}

	private static decimal WelfordStd(int n, decimal m2)
	{
		if (n <= 1)
			return 0;

		// sample std (n-1)
		var variance = m2 / (n - 1);
		return (decimal)Math.Sqrt((double)variance);
	}

	#endregion

	#endregion


	#region Event handlers

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