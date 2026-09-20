namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

using OFT.Attributes.Editors;
using OFT.Rendering.Context;

using ATAS.Indicators.Drawing;

using Utils.Common.Logging;

[DisplayName("Delta Thresholds")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(DeltaThresholdsResources), Description = nameof(DeltaThresholdsResources.DeltaThresholds_Description))]
public class DeltaThresholds : Indicator
{
	#region Nested Types

	public enum ThresholdSource
	{
		// The four levels set in the settings.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ThresholdSource_Fixed))]
		Fixed,

		// Mean and standard deviation of the bar extremes of the session so far.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ThresholdSource_Dynamic))]
		Dynamic
	}

	public enum WindowMode
	{
		// Every bar of the chart's default session.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.WindowMode_FullSession))]
		FullSession,

		// Only the bars that open between a start and an end time, in chart time.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.WindowMode_TimeWindow))]
		TimeWindow
	}

	public enum ThresholdLevel
	{
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ThresholdLevel_Major))]
		Major,

		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ThresholdLevel_Minor))]
		Minor
	}

	public enum SignalTrigger
	{
		// The delta of the bar reached the level at some point (MaxDelta or MinDelta).
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalTrigger_Reached))]
		Reached,

		// The delta of the closed bar is beyond the level.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalTrigger_BarClose))]
		BarClose
	}

	public enum AverageType
	{
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageType_Sma))]
		Sma,

		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageType_Ema))]
		Ema
	}

	public enum AverageColoring
	{
		// One color.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageColoring_Fixed))]
		Fixed,

		// Up color at or above zero, down color below.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageColoring_ZeroCross))]
		ZeroCross,

		// Up color while the average rises or stays, down color while it falls.
		[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageColoring_Slope))]
		Slope
	}

	// Running mean and variance (Welford). One sample per closed bar.
	internal struct RunningStats
	{
		public int Count { get; private set; }

		public decimal Mean { get; private set; }

		private decimal _m2;

		public void Add(decimal x)
		{
			Count++;
			var delta = x - Mean;
			Mean += delta / Count;
			_m2 += delta * (x - Mean);
		}

		// Sample standard deviation; 0 with fewer than two samples.
		public decimal StdDev()
		{
			return Count > 1 ? (decimal)Math.Sqrt((double)(_m2 / (Count - 1))) : 0;
		}

		public void Reset()
		{
			Count = 0;
			Mean = 0;
			_m2 = 0;
		}
	}

	// Threshold levels of one bar. A level of 0 is not drawn and never triggers.
	internal readonly struct Levels
	{
		public Levels(decimal upMajor, decimal upMinor, decimal downMinor, decimal downMajor)
		{
			UpMajor = upMajor;
			UpMinor = upMinor;
			DownMinor = downMinor;
			DownMajor = downMajor;
		}

		public decimal UpMajor { get; }

		public decimal UpMinor { get; }

		public decimal DownMinor { get; }

		public decimal DownMajor { get; }
	}

	#endregion

	#region Fields

	// Number of most recent bars written to the log after the history (detailed log).
	private const int LoggedHistoryBars = 20;

	// Delta of each bar (ask volume - bid volume), drawn as a histogram.
	private readonly ValueDataSeries _deltaSeries = new("DeltaSeries", DeltaThresholdsResources.Series_Delta)
	{
		VisualType = VisualMode.Histogram,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	private bool _showHistogram = true;
	private CrossColor _upColor = CrossColor.FromArgb(255, 0, 170, 0);
	private CrossColor _downColor = CrossColor.FromArgb(255, 205, 0, 0);

	// Threshold lines, drawn as one dash per bar so a level that changes between sessions or
	// bars never draws a slanted connector. Zero values are not drawn.
	private readonly ValueDataSeries _upMajorSeries = ThresholdSeries("UpMajor", DeltaThresholdsResources.Series_UpMajor, CrossColor.FromArgb(255, 0, 200, 0), 2);
	private readonly ValueDataSeries _upMinorSeries = ThresholdSeries("UpMinor", DeltaThresholdsResources.Series_UpMinor, CrossColor.FromArgb(255, 144, 238, 144), 1);
	private readonly ValueDataSeries _downMinorSeries = ThresholdSeries("DownMinor", DeltaThresholdsResources.Series_DownMinor, CrossColor.FromArgb(255, 255, 160, 160), 1);
	private readonly ValueDataSeries _downMajorSeries = ThresholdSeries("DownMajor", DeltaThresholdsResources.Series_DownMajor, CrossColor.FromArgb(255, 220, 0, 0), 2);

	// Levels of each calculated bar.
	private readonly List<Levels> _levels = new();

	private ThresholdSource _source = ThresholdSource.Fixed;
	private decimal _stdMultiplier = 1m;
	private int _minSamples = 1;
	private WindowMode _windowMode = WindowMode.TimeWindow;
	private TimeSpan _windowStart = new(9, 30, 0);
	private TimeSpan _windowEnd = new(16, 0, 0);

	// Dynamic source: statistics of the positive extreme (MaxDelta > 0) and of the size of the
	// negative extreme (|MinDelta|, MinDelta < 0) of the closed bars of the current session.
	private RunningStats _positive;
	private RunningStats _negative;

	private bool _showThresholdLines = true;
	private decimal _upMajorLevel = 300;
	private decimal _upMinorLevel = 200;
	private decimal _downMinorLevel = -200;
	private decimal _downMajorLevel = -300;

	// Signals of each calculated bar, read by the render thread under _signalsLock.
	private readonly List<(bool Up, bool Down)> _signals = new();
	private readonly object _signalsLock = new();

	private bool _showSignals = true;
	private ThresholdLevel _signalUpLevel = ThresholdLevel.Major;
	private ThresholdLevel _signalDownLevel = ThresholdLevel.Major;
	private SignalTrigger _signalTrigger = SignalTrigger.Reached;
	private int _signalOffsetTicks = 2;
	private int _signalSize = 10;
	private CrossColor _signalUpColor = CrossColor.FromArgb(255, 0, 255, 0);
	private CrossColor _signalDownColor = CrossColor.FromArgb(255, 255, 0, 255);

	// Visible signals copied under the lock by OnRender, reused between frames.
	private readonly List<(int Bar, bool Up, bool Down)> _renderSignals = new();

	private bool _alertsEnabled;
	private ThresholdLevel _alertUpLevel = ThresholdLevel.Major;
	private ThresholdLevel _alertDownLevel = ThresholdLevel.Major;
	private bool _alertAtBarClose = true;
	private int _alertCooldownBars = 3;
	private string _alertFile = "alert2";

	// Realtime state of the alerts: bars from _firstLiveBar on opened after the history; the last
	// bar of each side that alerted; and, for the forming bar, whether its level was already
	// reached at the previous update.
	private bool _historyLoaded;
	private int _firstLiveBar;
	private int _lastUpAlertBar = int.MinValue / 2;
	private int _lastDownAlertBar = int.MinValue / 2;
	private int _observedBar = -1;
	private bool _upReachedBefore;
	private bool _downReachedBefore;

	// Moving average of the bar delta.
	private readonly ValueDataSeries _averageSeries = new("AverageSeries", DeltaThresholdsResources.Series_Average)
	{
		Color = CrossColor.FromArgb(255, 255, 215, 0),
		Width = 2,
		VisualType = VisualMode.Hide,
		IsHidden = true,
		UseMinimizedModeIfEnabled = true
	};

	private bool _showAverage;
	private int _averagePeriod = 20;
	private AverageType _averageType = AverageType.Sma;
	private AverageColoring _averageColoring = AverageColoring.Fixed;
	private CrossColor _averageUpColor = CrossColor.FromArgb(255, 0, 200, 0);
	private CrossColor _averageDownColor = CrossColor.FromArgb(255, 220, 0, 0);

	// Running sum of the bar delta up to each bar: the SMA of any bar comes from two sums, so
	// recalculating the forming bar on every update gives the same value as the history.
	private readonly List<decimal> _deltaSums = new();

	private bool _detailedLog;

	#endregion

	#region Properties

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ShowHistogram_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Histogram), Order = 10,
		Description = nameof(DeltaThresholdsResources.ShowHistogram_Description))]
	public bool ShowHistogram
	{
		get => _showHistogram;
		set
		{
			_showHistogram = value;
			_deltaSeries.VisualType = value ? VisualMode.Histogram : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.UpColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Histogram), Order = 20,
		Description = nameof(DeltaThresholdsResources.UpColor_Description))]
	public CrossColor UpColor
	{
		get => _upColor;
		set
		{
			_upColor = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.DownColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Histogram), Order = 30,
		Description = nameof(DeltaThresholdsResources.DownColor_Description))]
	public CrossColor DownColor
	{
		get => _downColor;
		set
		{
			_downColor = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Thresholds

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.Source_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 90,
		Description = nameof(DeltaThresholdsResources.Source_Description))]
	public ThresholdSource Source
	{
		get => _source;
		set
		{
			_source = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.StdMultiplier_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 92,
		Description = nameof(DeltaThresholdsResources.StdMultiplier_Description))]
	[Range(0, 10)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal StdMultiplier
	{
		get => _stdMultiplier;
		set
		{
			_stdMultiplier = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.MinSamples_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 94,
		Description = nameof(DeltaThresholdsResources.MinSamples_Description))]
	[Range(1, 5000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int MinSamples
	{
		get => _minSamples;
		set
		{
			_minSamples = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.Window_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 95,
		Description = nameof(DeltaThresholdsResources.Window_Description))]
	public WindowMode Window
	{
		get => _windowMode;
		set
		{
			_windowMode = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.WindowStart_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 96,
		Description = nameof(DeltaThresholdsResources.WindowStart_Description))]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public TimeSpan WindowStart
	{
		get => _windowStart;
		set
		{
			_windowStart = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.WindowEnd_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 97,
		Description = nameof(DeltaThresholdsResources.WindowEnd_Description))]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public TimeSpan WindowEnd
	{
		get => _windowEnd;
		set
		{
			_windowEnd = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ShowThresholdLines_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 100,
		Description = nameof(DeltaThresholdsResources.ShowThresholdLines_Description))]
	public bool ShowThresholdLines
	{
		get => _showThresholdLines;
		set
		{
			_showThresholdLines = value;
			UpdateThresholdVisibility();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.UpMajorLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 110,
		Description = nameof(DeltaThresholdsResources.UpMajorLevel_Description))]
	[Range(0, 1000000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal UpMajorLevel
	{
		get => _upMajorLevel;
		set
		{
			_upMajorLevel = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.UpMinorLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 120,
		Description = nameof(DeltaThresholdsResources.UpMinorLevel_Description))]
	[Range(0, 1000000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal UpMinorLevel
	{
		get => _upMinorLevel;
		set
		{
			_upMinorLevel = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.DownMinorLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 130,
		Description = nameof(DeltaThresholdsResources.DownMinorLevel_Description))]
	[Range(-1000000000, 0)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal DownMinorLevel
	{
		get => _downMinorLevel;
		set
		{
			_downMinorLevel = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.DownMajorLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 140,
		Description = nameof(DeltaThresholdsResources.DownMajorLevel_Description))]
	[Range(-1000000000, 0)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal DownMajorLevel
	{
		get => _downMajorLevel;
		set
		{
			_downMajorLevel = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.UpMajorColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 150,
		Description = nameof(DeltaThresholdsResources.UpMajorColor_Description))]
	public CrossColor UpMajorColor
	{
		get => _upMajorSeries.Color;
		set => _upMajorSeries.Color = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.UpMinorColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 160,
		Description = nameof(DeltaThresholdsResources.UpMinorColor_Description))]
	public CrossColor UpMinorColor
	{
		get => _upMinorSeries.Color;
		set => _upMinorSeries.Color = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.DownMinorColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 170,
		Description = nameof(DeltaThresholdsResources.DownMinorColor_Description))]
	public CrossColor DownMinorColor
	{
		get => _downMinorSeries.Color;
		set => _downMinorSeries.Color = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.DownMajorColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Thresholds), Order = 180,
		Description = nameof(DeltaThresholdsResources.DownMajorColor_Description))]
	public CrossColor DownMajorColor
	{
		get => _downMajorSeries.Color;
		set => _downMajorSeries.Color = value;
	}

	#endregion

	#region Properties: Signals

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ShowSignals_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 300,
		Description = nameof(DeltaThresholdsResources.ShowSignals_Description))]
	public bool ShowSignals
	{
		get => _showSignals;
		set
		{
			_showSignals = value;
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.Trigger_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 310,
		Description = nameof(DeltaThresholdsResources.Trigger_Description))]
	public SignalTrigger Trigger
	{
		get => _signalTrigger;
		set
		{
			_signalTrigger = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalUpLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 320,
		Description = nameof(DeltaThresholdsResources.SignalUpLevel_Description))]
	public ThresholdLevel SignalUpLevel
	{
		get => _signalUpLevel;
		set
		{
			_signalUpLevel = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalDownLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 330,
		Description = nameof(DeltaThresholdsResources.SignalDownLevel_Description))]
	public ThresholdLevel SignalDownLevel
	{
		get => _signalDownLevel;
		set
		{
			_signalDownLevel = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalOffsetTicks_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 340,
		Description = nameof(DeltaThresholdsResources.SignalOffsetTicks_Description))]
	[Range(0, 1000)]
	public int SignalOffsetTicks
	{
		get => _signalOffsetTicks;
		set
		{
			_signalOffsetTicks = value;
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalSize_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 350,
		Description = nameof(DeltaThresholdsResources.SignalSize_Description))]
	[Range(4, 50)]
	public int SignalSize
	{
		get => _signalSize;
		set
		{
			_signalSize = value;
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalUpColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 360,
		Description = nameof(DeltaThresholdsResources.SignalUpColor_Description))]
	public CrossColor SignalUpColor
	{
		get => _signalUpColor;
		set
		{
			_signalUpColor = value;
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.SignalDownColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Signals), Order = 370,
		Description = nameof(DeltaThresholdsResources.SignalDownColor_Description))]
	public CrossColor SignalDownColor
	{
		get => _signalDownColor;
		set
		{
			_signalDownColor = value;
			RedrawChart();
		}
	}

	#endregion

	#region Properties: Alerts

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.UseAlerts_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Alerts), Order = 400,
		Description = nameof(DeltaThresholdsResources.UseAlerts_Description))]
	public bool UseAlerts
	{
		get => _alertsEnabled;
		set => _alertsEnabled = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AlertUpLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Alerts), Order = 410,
		Description = nameof(DeltaThresholdsResources.AlertUpLevel_Description))]
	public ThresholdLevel AlertUpLevel
	{
		get => _alertUpLevel;
		set => _alertUpLevel = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AlertDownLevel_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Alerts), Order = 420,
		Description = nameof(DeltaThresholdsResources.AlertDownLevel_Description))]
	public ThresholdLevel AlertDownLevel
	{
		get => _alertDownLevel;
		set => _alertDownLevel = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AlertAtBarClose_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Alerts), Order = 430,
		Description = nameof(DeltaThresholdsResources.AlertAtBarClose_Description))]
	public bool AlertAtBarClose
	{
		get => _alertAtBarClose;
		set => _alertAtBarClose = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AlertCooldownBars_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Alerts), Order = 440,
		Description = nameof(DeltaThresholdsResources.AlertCooldownBars_Description))]
	[Range(0, 1000)]
	public int AlertCooldownBars
	{
		get => _alertCooldownBars;
		set => _alertCooldownBars = value;
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AlertFile_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Alerts), Order = 450,
		Description = nameof(DeltaThresholdsResources.AlertFile_Description))]
	public string AlertFile
	{
		get => _alertFile;
		set => _alertFile = value;
	}

	#endregion

	#region Properties: Average

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.ShowAverage_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 500,
		Description = nameof(DeltaThresholdsResources.ShowAverage_Description))]
	public bool ShowAverage
	{
		get => _showAverage;
		set
		{
			_showAverage = value;
			_averageSeries.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AveragePeriod_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 510,
		Description = nameof(DeltaThresholdsResources.AveragePeriod_Description))]
	[Range(1, 1000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int AveragePeriod
	{
		get => _averagePeriod;
		set
		{
			_averagePeriod = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageKind_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 520,
		Description = nameof(DeltaThresholdsResources.AverageKind_Description))]
	public AverageType AverageKind
	{
		get => _averageType;
		set
		{
			_averageType = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageColorMode_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 530,
		Description = nameof(DeltaThresholdsResources.AverageColorMode_Description))]
	public AverageColoring AverageColorMode
	{
		get => _averageColoring;
		set
		{
			_averageColoring = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 540,
		Description = nameof(DeltaThresholdsResources.AverageColor_Description))]
	public CrossColor AverageColor
	{
		get => _averageSeries.Color;
		set
		{
			_averageSeries.Color = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageUpColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 550,
		Description = nameof(DeltaThresholdsResources.AverageUpColor_Description))]
	public CrossColor AverageUpColor
	{
		get => _averageUpColor;
		set
		{
			_averageUpColor = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageDownColor_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 560,
		Description = nameof(DeltaThresholdsResources.AverageDownColor_Description))]
	public CrossColor AverageDownColor
	{
		get => _averageDownColor;
		set
		{
			_averageDownColor = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.AverageWidth_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Average), Order = 570,
		Description = nameof(DeltaThresholdsResources.AverageWidth_Description))]
	[Range(1, 20)]
	public int AverageWidth
	{
		get => _averageSeries.Width;
		set => _averageSeries.Width = value;
	}

	#endregion

	#region Properties: Diagnostics

	[Display(ResourceType = typeof(DeltaThresholdsResources), Name = nameof(DeltaThresholdsResources.DetailedLog_DisplayName),
		GroupName = nameof(DeltaThresholdsResources.Group_Diagnostics), Order = 900,
		Description = nameof(DeltaThresholdsResources.DetailedLog_Description))]
	public bool DetailedLog
	{
		get => _detailedLog;
		set => _detailedLog = value;
	}

	#endregion

	#region Ctor

	public DeltaThresholds()
		: base(true)
	{
		// Its own panel by default; it can be moved to another panel, for example the Delta indicator's.
		Panel = IndicatorDataProvider.NewPanel;

		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);

		DataSeries[0] = _deltaSeries;
		DataSeries.Add(_upMajorSeries);
		DataSeries.Add(_upMinorSeries);
		DataSeries.Add(_downMinorSeries);
		DataSeries.Add(_downMajorSeries);
		DataSeries.Add(_averageSeries);

		UpdateThresholdVisibility();
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"DeltaThresholds: initialized ({typeof(DeltaThresholds).Assembly.GetName().Version}).");
	}

	// Candle data only: Delta, MaxDelta and MinDelta are fields of the bar, so there is no trade
	// request and no realtime buffer. OnCalculate runs once per closed bar during the history and
	// on every update of the forming bar.
	protected override void OnRecalculate()
	{
		_historyLoaded = false;
		_observedBar = -1;
		_lastUpAlertBar = _lastDownAlertBar = int.MinValue / 2;

		lock (_signalsLock)
			_signals.Clear();

		_levels.Clear();
		_deltaSums.Clear();
		_positive.Reset();
		_negative.Reset();
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		var delta = GetCandle(bar).Delta;

		_deltaSeries[bar] = delta;
		_deltaSeries.Colors[bar] = (delta >= 0 ? _upColor : _downColor).Convert();

		// The levels of a bar are set when the bar is first calculated and stay fixed while it forms.
		if (bar >= _levels.Count)
			OpenBar(bar);

		UpdateSignals(bar, false);
		UpdateAverage(bar, delta);

		if (_historyLoaded && bar == CurrentBar - 1 && !_alertAtBarClose)
			CheckReachedAlerts(bar);
	}

	protected override void OnFinishRecalculate()
	{
		_historyLoaded = true;
		_firstLiveBar = CurrentBar;

		int upSignals, downSignals;

		lock (_signalsLock)
		{
			upSignals = _signals.Count(s => s.Up);
			downSignals = _signals.Count(s => s.Down);
		}

		var last = CurrentBar - 1;
		var levels = last >= 0 && last < _levels.Count ? _levels[last] : default;

		this.LogInfo($"DeltaThresholds: history calculated, {CurrentBar} bars; source {_source}" +
			(_source == ThresholdSource.Dynamic
				? $" (multiplier {_stdMultiplier}, minimum bars {_minSamples}, window {WindowDescription()}; " +
				$"session samples {_positive.Count} up / {_negative.Count} down)"
				: "") +
			$"; levels of the last bar {FormatLevels(levels)}; signals {_signalTrigger} " +
			$"(up {_signalUpLevel}, down {_signalDownLevel}): {upSignals} up / {downSignals} down; " +
			$"alerts {(_alertsEnabled ? (_alertAtBarClose ? "at bar close" : "when reached") : "off")}.");

		if (!_detailedLog)
			return;

		for (var bar = Math.Max(0, last - LoggedHistoryBars + 1); bar <= last; bar++)
			LogBar(bar);
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (!_showSignals || ChartInfo is null || InstrumentInfo is null)
			return;

		_renderSignals.Clear();

		lock (_signalsLock)
		{
			var last = Math.Min(LastVisibleBarNumber, _signals.Count - 1);

			for (var bar = Math.Max(0, FirstVisibleBarNumber); bar <= last; bar++)
			{
				var (up, down) = _signals[bar];

				if (up || down)
					_renderSignals.Add((bar, up, down));
			}
		}

		// The indicator may sit in its own panel: the triangles are drawn in the price panel's
		// coordinates and kept inside it.
		var container = ChartInfo.PriceChartContainer;
		var region = container.Region;
		var half = _signalSize / 2;
		var offset = _signalOffsetTicks * InstrumentInfo.TickSize;

		foreach (var (bar, up, down) in _renderSignals)
		{
			var candle = GetCandle(bar);
			var x = container.GetXByBar(bar, false) + (int)container.BarsWidth / 2;

			if (up)
			{
				var y = Clamp(container.GetYByPrice(candle.Low - offset, false) + half, region.Top + half, region.Bottom - half);
				context.FillPolygon(_signalUpColor.Convert(), new[] { new Point(x, y - half), new Point(x - half, y + half), new Point(x + half, y + half) });
			}

			if (down)
			{
				var y = Clamp(container.GetYByPrice(candle.High + offset, false) - half, region.Top + half, region.Bottom - half);
				context.FillPolygon(_signalDownColor.Convert(), new[] { new Point(x, y + half), new Point(x - half, y - half), new Point(x + half, y - half) });
			}
		}
	}

	#endregion

	#region Private Methods

	private static ValueDataSeries ThresholdSeries(string id, string name, CrossColor color, int width)
	{
		return new ValueDataSeries(id, name)
		{
			Color = color,
			Width = width,
			VisualType = VisualMode.Hash,
			ShowZeroValue = false,
			IsHidden = true,
			UseMinimizedModeIfEnabled = true
		};
	}

	private void UpdateThresholdVisibility()
	{
		var mode = _showThresholdLines ? VisualMode.Hash : VisualMode.Hide;
		_upMajorSeries.VisualType = mode;
		_upMinorSeries.VisualType = mode;
		_downMinorSeries.VisualType = mode;
		_downMajorSeries.VisualType = mode;
	}

	// Sets the levels of a bar that has just appeared. The previous bar has closed: its extremes
	// are final and become one sample of the session statistics, before a new session restarts
	// them. The levels of a bar therefore only use the bars before it (no look-ahead), and the
	// forming bar, calculated on every update, never adds samples: live values and a
	// recalculation give the same levels.
	private void OpenBar(int bar)
	{
		while (_levels.Count < bar)
			_levels.Add(default);

		if (bar > 0)
		{
			// Bar-close signals of the bar that has just closed.
			UpdateSignals(bar - 1, true);

			if (_historyLoaded && _alertAtBarClose)
				CheckCloseAlerts(bar - 1);

			if (_historyLoaded && _detailedLog)
				LogBar(bar - 1);

			if (InWindow(bar - 1))
				AddSamples(GetCandle(bar - 1));
		}

		if (IsSessionStart(bar))
		{
			_positive.Reset();
			_negative.Reset();

			if (_historyLoaded && _source == ThresholdSource.Dynamic)
				this.LogInfo($"DeltaThresholds: statistics restart at bar {bar} ({GetCandle(bar).Time:yyyy-MM-dd HH:mm:ss}).");
		}

		var levels = _source != ThresholdSource.Dynamic
			? new Levels(_upMajorLevel, _upMinorLevel, _downMinorLevel, _downMajorLevel)
			: InWindow(bar)
				? DynamicLevels()
				: default;

		_levels.Add(levels);

		_upMajorSeries[bar] = levels.UpMajor;
		_upMinorSeries[bar] = levels.UpMinor;
		_downMinorSeries[bar] = levels.DownMinor;
		_downMajorSeries[bar] = levels.DownMajor;
	}

	// Start of the statistics: each default session, or each time window (its first bar inside
	// the window, or the first bar of a new window day for a window that covers the whole day).
	// Signals of a bar. Level reached: the bar's maximum or minimum delta against its levels, which
	// only grows while the bar forms, so a signal appears as soon as it happens, stays, and is the
	// same after a recalculation. Bar close: the final delta, evaluated once the bar has closed.
	private void UpdateSignals(int bar, bool closed)
	{
		if (bar >= _levels.Count)
			return;

		var levels = _levels[bar];
		var upLevel = _signalUpLevel == ThresholdLevel.Major ? levels.UpMajor : levels.UpMinor;
		var downLevel = _signalDownLevel == ThresholdLevel.Major ? levels.DownMajor : levels.DownMinor;
		var candle = GetCandle(bar);
		bool up, down;

		if (_signalTrigger == SignalTrigger.Reached)
		{
			up = upLevel > 0 && candle.MaxDelta >= upLevel;
			down = downLevel < 0 && candle.MinDelta <= downLevel;
		}
		else if (closed)
		{
			up = upLevel > 0 && candle.Delta >= upLevel;
			down = downLevel < 0 && candle.Delta <= downLevel;
		}
		else
			return;

		lock (_signalsLock)
		{
			while (_signals.Count <= bar)
				_signals.Add(default);

			_signals[bar] = (up, down);
		}
	}

	// Idempotent per bar: the value of a bar only depends on the stored values of the bars before
	// it and its own delta, so updating the forming bar any number of times gives the same result
	// as calculating it once in the history.
	private void UpdateAverage(int bar, decimal delta)
	{
		while (_deltaSums.Count < bar)
			_deltaSums.Add(_deltaSums.Count > 0 ? _deltaSums[^1] : 0);

		var previousSum = bar > 0 ? _deltaSums[bar - 1] : 0;

		if (_deltaSums.Count == bar)
			_deltaSums.Add(previousSum + delta);
		else
			_deltaSums[bar] = previousSum + delta;

		decimal average;

		if (_averageType == AverageType.Sma)
		{
			var count = Math.Min(_averagePeriod, bar + 1);
			var startSum = bar - count >= 0 ? _deltaSums[bar - count] : 0;
			average = (_deltaSums[bar] - startSum) / count;
		}
		else
		{
			var alpha = 2m / (_averagePeriod + 1);
			average = bar == 0 ? delta : _averageSeries[bar - 1] + alpha * (delta - _averageSeries[bar - 1]);
		}

		_averageSeries[bar] = average;

		var color = _averageColoring switch
		{
			AverageColoring.ZeroCross => average >= 0 ? _averageUpColor : _averageDownColor,
			AverageColoring.Slope => bar == 0 || average >= _averageSeries[bar - 1] ? _averageUpColor : _averageDownColor,
			_ => _averageSeries.Color
		};

		_averageSeries.Colors[bar] = color.Convert();
	}

	private decimal AlertLevel(int bar, bool up)
	{
		var levels = _levels[bar];

		return up
			? _alertUpLevel == ThresholdLevel.Major ? levels.UpMajor : levels.UpMinor
			: _alertDownLevel == ThresholdLevel.Major ? levels.DownMajor : levels.DownMinor;
	}

	// Event-based: an alert fires when the forming bar goes from not having reached the level to
	// having reached it. A bar that was already forming when the history finished and had
	// already reached the level does not alert: that happened before.
	private void CheckReachedAlerts(int bar)
	{
		if (!_alertsEnabled || bar >= _levels.Count)
			return;

		var candle = GetCandle(bar);
		var upLevel = AlertLevel(bar, true);
		var downLevel = AlertLevel(bar, false);
		var upReached = upLevel > 0 && candle.MaxDelta >= upLevel;
		var downReached = downLevel < 0 && candle.MinDelta <= downLevel;

		if (_observedBar != bar)
		{
			// First observation of this bar: a bar that opened after the history starts from
			// "not reached"; the bar forming at load starts from what it has already reached.
			var preexisting = bar < _firstLiveBar;
			_upReachedBefore = preexisting && upReached;
			_downReachedBefore = preexisting && downReached;
			_observedBar = bar;
		}

		if (upReached && !_upReachedBefore)
			RaiseAlert(bar, true, $"Delta Thresholds: delta reached {upLevel:0.##} ({_alertUpLevel.ToString().ToLowerInvariant()} up level), bar delta {candle.Delta:0.##}");

		if (downReached && !_downReachedBefore)
			RaiseAlert(bar, false, $"Delta Thresholds: delta reached {downLevel:0.##} ({_alertDownLevel.ToString().ToLowerInvariant()} down level), bar delta {candle.Delta:0.##}");

		_upReachedBefore = upReached;
		_downReachedBefore = downReached;
	}

	// A bar that has just closed in realtime with its final delta beyond the level.
	private void CheckCloseAlerts(int bar)
	{
		if (!_alertsEnabled || bar >= _levels.Count || bar < _firstLiveBar - 1)
			return;

		var delta = GetCandle(bar).Delta;
		var upLevel = AlertLevel(bar, true);
		var downLevel = AlertLevel(bar, false);

		if (upLevel > 0 && delta >= upLevel)
			RaiseAlert(bar, true, $"Delta Thresholds: bar closed with delta {delta:0.##} above {upLevel:0.##} ({_alertUpLevel.ToString().ToLowerInvariant()} up level)");

		if (downLevel < 0 && delta <= downLevel)
			RaiseAlert(bar, false, $"Delta Thresholds: bar closed with delta {delta:0.##} below {downLevel:0.##} ({_alertDownLevel.ToString().ToLowerInvariant()} down level)");
	}

	private void RaiseAlert(int bar, bool up, string message)
	{
		var last = up ? _lastUpAlertBar : _lastDownAlertBar;

		if (bar - last < Math.Max(1, _alertCooldownBars))
			return;

		if (up)
			_lastUpAlertBar = bar;
		else
			_lastDownAlertBar = bar;

		this.LogInfo($"DeltaThresholds: alert: {message}");
		AddAlert(_alertFile, InstrumentInfo?.Instrument ?? string.Empty, message,
			up ? _signalUpColor : _signalDownColor, CrossColor.FromArgb(255, 0, 0, 0));
	}

	private void LogBar(int bar)
	{
		if (bar < 0 || bar >= _levels.Count)
			return;

		var candle = GetCandle(bar);
		(bool Up, bool Down) signals;

		lock (_signalsLock)
			signals = bar < _signals.Count ? _signals[bar] : default;

		this.LogInfo($"DeltaThresholds: bar {bar} {candle.Time:yyyy-MM-dd HH:mm:ss}: delta {candle.Delta:0.##} " +
			$"(max {candle.MaxDelta:0.##}, min {candle.MinDelta:0.##}); levels {FormatLevels(_levels[bar])}" +
			$"{(signals.Up ? "; UP signal" : "")}{(signals.Down ? "; DOWN signal" : "")}" +
			$"{(_showAverage ? $"; average {_averageSeries[bar]:0.##}" : "")}");
	}

	private static string FormatLevels(Levels levels)
	{
		return $"up {levels.UpMajor:0.##}/{levels.UpMinor:0.##}, down {levels.DownMinor:0.##}/{levels.DownMajor:0.##}";
	}

	private string WindowDescription()
	{
		return _windowMode == WindowMode.FullSession
			? "full session"
			: $"{_windowStart:hh\\:mm}-{_windowEnd:hh\\:mm}";
	}

	private static int Clamp(int value, int min, int max)
	{
		return value < min ? min : value > max ? max : value;
	}

	private bool IsSessionStart(int bar)
	{
		if (bar == 0)
			return true;

		if (_windowMode == WindowMode.FullSession)
			return IsNewSession(bar);

		return InWindow(bar) && (!InWindow(bar - 1) || WindowDay(bar) != WindowDay(bar - 1));
	}

	private bool InWindow(int bar)
	{
		if (_windowMode == WindowMode.FullSession || _windowStart == _windowEnd)
			return true;

		var time = ChartTime(GetCandle(bar).Time).TimeOfDay;

		return _windowStart < _windowEnd
			? time >= _windowStart && time < _windowEnd
			: time >= _windowStart || time < _windowEnd;
	}

	// Day of the window a bar opens in: its chart date shifted back by the window start, so a
	// window that crosses midnight belongs to the day it started.
	private DateTime WindowDay(int bar)
	{
		return (ChartTime(GetCandle(bar).Time) - _windowStart).Date;
	}

	private DateTime ChartTime(DateTime utc)
	{
		if (InstrumentInfo is null)
			return utc;

#if ATAS_STABLE || ATAS_LATEST
		return utc.AddHours(InstrumentInfo.TimeZone);
#else
		return utc.Add(InstrumentInfo.TimeZoneOffset);
#endif
	}

	private void AddSamples(IndicatorCandle candle)
	{
		if (candle.MaxDelta > 0)
			_positive.Add(candle.MaxDelta);

		if (candle.MinDelta < 0)
			_negative.Add(-candle.MinDelta);
	}

	// Levels from the statistics so far; a side without enough samples has no levels (0).
	private Levels DynamicLevels()
	{
		decimal upMajor = 0, upMinor = 0, downMinor = 0, downMajor = 0;

		if (_positive.Count >= _minSamples)
		{
			upMinor = _positive.Mean;
			upMajor = _positive.Mean + _stdMultiplier * _positive.StdDev();
		}

		if (_negative.Count >= _minSamples)
		{
			downMinor = -_negative.Mean;
			downMajor = -(_negative.Mean + _stdMultiplier * _negative.StdDev());
		}

		return new Levels(upMajor, upMinor, downMinor, downMajor);
	}

	#endregion
}
