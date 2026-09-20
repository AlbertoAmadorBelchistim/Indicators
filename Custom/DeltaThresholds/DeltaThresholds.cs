namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

using OFT.Attributes.Editors;
using OFT.Rendering.Context;

using ATAS.Indicators.Drawing;

using Utils.Common.Logging;

[DisplayName("Delta Thresholds")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Description("Bar delta with fixed or session-based dynamic thresholds, price-chart signals and alerts.")]
public class DeltaThresholds : Indicator
{
	#region Nested Types

	public enum ThresholdSource
	{
		// The four levels set in the settings.
		[Display(Name = "Fixed")]
		Fixed,

		// Mean and standard deviation of the bar extremes of the session so far.
		[Display(Name = "Dynamic (session statistics)")]
		Dynamic
	}

	public enum WindowMode
	{
		// Every bar of the chart's default session.
		[Display(Name = "Full session")]
		FullSession,

		// Only the bars that open between a start and an end time, in chart time.
		[Display(Name = "Time window")]
		TimeWindow
	}

	public enum ThresholdLevel
	{
		[Display(Name = "Major")]
		Major,

		[Display(Name = "Minor")]
		Minor
	}

	public enum SignalTrigger
	{
		// The delta of the bar reached the level at some point (MaxDelta or MinDelta).
		[Display(Name = "Level reached")]
		Reached,

		// The delta of the closed bar is beyond the level.
		[Display(Name = "Bar close")]
		BarClose
	}

	public enum AverageType
	{
		[Display(Name = "SMA")]
		Sma,

		[Display(Name = "EMA")]
		Ema
	}

	public enum AverageColoring
	{
		// One color.
		[Display(Name = "Fixed")]
		Fixed,

		// Up color at or above zero, down color below.
		[Display(Name = "Zero cross")]
		ZeroCross,

		// Up color while the average rises or stays, down color while it falls.
		[Display(Name = "Slope")]
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

	// Delta of each bar (ask volume - bid volume), drawn as a histogram.
	private readonly ValueDataSeries _deltaSeries = new("DeltaSeries", "Delta")
	{
		VisualType = VisualMode.Histogram,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	// Threshold lines, drawn as one dash per bar so a level that changes between sessions or
	// bars never draws a slanted connector. Zero values are not drawn.
	private readonly ValueDataSeries _upMajorSeries = ThresholdSeries("UpMajor", "Up major", CrossColor.FromArgb(255, 0, 200, 0), 2);
	private readonly ValueDataSeries _upMinorSeries = ThresholdSeries("UpMinor", "Up minor", CrossColor.FromArgb(255, 144, 238, 144), 1);
	private readonly ValueDataSeries _downMinorSeries = ThresholdSeries("DownMinor", "Down minor", CrossColor.FromArgb(255, 255, 160, 160), 1);
	private readonly ValueDataSeries _downMajorSeries = ThresholdSeries("DownMajor", "Down major", CrossColor.FromArgb(255, 220, 0, 0), 2);

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
	private readonly ValueDataSeries _averageSeries = new("AverageSeries", "Average")
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

	private bool _showHistogram = true;
	private CrossColor _upColor = CrossColor.FromArgb(255, 0, 170, 0);
	private CrossColor _downColor = CrossColor.FromArgb(255, 205, 0, 0);

	#endregion

	#region Properties

	[Display(Name = "Show histogram", GroupName = "Histogram",
		Description = "Draws the delta of each bar. Turn it off to show only the thresholds, for example over the Delta indicator's panel.",
		Order = 10)]
	public bool ShowHistogram
	{
		get => _showHistogram;
		set
		{
			_showHistogram = value;
			_deltaSeries.VisualType = value ? VisualMode.Histogram : VisualMode.Hide;
		}
	}

	[Display(Name = "Positive color", GroupName = "Histogram", Description = "Color of the bars with a positive delta.", Order = 20)]
	public CrossColor UpColor
	{
		get => _upColor;
		set
		{
			_upColor = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Negative color", GroupName = "Histogram", Description = "Color of the bars with a negative delta.", Order = 30)]
	public CrossColor DownColor
	{
		get => _downColor;
		set
		{
			_downColor = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Threshold source", GroupName = "Thresholds",
		Description = "Fixed: the four levels below. Dynamic: from the bars of the session so far; minor = mean of the bar extremes, major = mean + multiplier x standard deviation.",
		Order = 90)]
	public ThresholdSource Source
	{
		get => _source;
		set
		{
			_source = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Standard deviation multiplier", GroupName = "Thresholds", Description = "Dynamic source: major level = mean + this x standard deviation.", Order = 92)]
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

	[Display(Name = "Minimum bars", GroupName = "Thresholds", Description = "Dynamic source: closed bars of the session needed before the levels are drawn and used.", Order = 94)]
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

	[Display(Name = "Statistics window", GroupName = "Thresholds",
		Description = "Dynamic source: bars used for the statistics. Full session: every bar, restarting at each default session. Time window: only bars that open between the start and end times; the statistics restart at each window start and there are no levels outside the window.",
		Order = 95)]
	public WindowMode Window
	{
		get => _windowMode;
		set
		{
			_windowMode = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Window start", GroupName = "Thresholds", Description = "Time window start, in chart time (included). A start later than the end crosses midnight.", Order = 96)]
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

	[Display(Name = "Window end", GroupName = "Thresholds", Description = "Time window end, in chart time (not included).", Order = 97)]
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

	[Display(Name = "Show threshold lines", GroupName = "Thresholds", Description = "Draws the four threshold levels.", Order = 100)]
	public bool ShowThresholdLines
	{
		get => _showThresholdLines;
		set
		{
			_showThresholdLines = value;
			UpdateThresholdVisibility();
		}
	}

	[Display(Name = "Up major level", GroupName = "Thresholds", Description = "Fixed level: strong positive delta.", Order = 110)]
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

	[Display(Name = "Up minor level", GroupName = "Thresholds", Description = "Fixed level: moderate positive delta.", Order = 120)]
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

	[Display(Name = "Down minor level", GroupName = "Thresholds", Description = "Fixed level: moderate negative delta (0 or below).", Order = 130)]
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

	[Display(Name = "Down major level", GroupName = "Thresholds", Description = "Fixed level: strong negative delta (0 or below).", Order = 140)]
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

	[Display(Name = "Up major color", GroupName = "Thresholds", Description = "Color of the up major line.", Order = 150)]
	public CrossColor UpMajorColor
	{
		get => _upMajorSeries.Color;
		set => _upMajorSeries.Color = value;
	}

	[Display(Name = "Up minor color", GroupName = "Thresholds", Description = "Color of the up minor line.", Order = 160)]
	public CrossColor UpMinorColor
	{
		get => _upMinorSeries.Color;
		set => _upMinorSeries.Color = value;
	}

	[Display(Name = "Down minor color", GroupName = "Thresholds", Description = "Color of the down minor line.", Order = 170)]
	public CrossColor DownMinorColor
	{
		get => _downMinorSeries.Color;
		set => _downMinorSeries.Color = value;
	}

	[Display(Name = "Down major color", GroupName = "Thresholds", Description = "Color of the down major line.", Order = 180)]
	public CrossColor DownMajorColor
	{
		get => _downMajorSeries.Color;
		set => _downMajorSeries.Color = value;
	}

	[Display(Name = "Show signals", GroupName = "Signals", Description = "Draws a triangle on the price chart when the delta of a bar reaches the selected level: below the bar for the up side, above it for the down side.", Order = 300)]
	public bool ShowSignals
	{
		get => _showSignals;
		set
		{
			_showSignals = value;
			RedrawChart();
		}
	}

	[Display(Name = "Trigger", GroupName = "Signals", Description = "Level reached: the delta of the bar reached the level at any time (its maximum or minimum), so the signal appears as soon as it happens and stays. Bar close: the delta of the closed bar is beyond the level.", Order = 310)]
	public SignalTrigger Trigger
	{
		get => _signalTrigger;
		set
		{
			_signalTrigger = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Up level", GroupName = "Signals", Description = "Level used for the up signals.", Order = 320)]
	public ThresholdLevel SignalUpLevel
	{
		get => _signalUpLevel;
		set
		{
			_signalUpLevel = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Down level", GroupName = "Signals", Description = "Level used for the down signals.", Order = 330)]
	public ThresholdLevel SignalDownLevel
	{
		get => _signalDownLevel;
		set
		{
			_signalDownLevel = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Offset (ticks)", GroupName = "Signals", Description = "Distance between the bar and the triangle.", Order = 340)]
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

	[Display(Name = "Size", GroupName = "Signals", Description = "Size of the triangles in pixels.", Order = 350)]
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

	[Display(Name = "Up color", GroupName = "Signals", Description = "Color of the up triangles.", Order = 360)]
	public CrossColor SignalUpColor
	{
		get => _signalUpColor;
		set
		{
			_signalUpColor = value;
			RedrawChart();
		}
	}

	[Display(Name = "Down color", GroupName = "Signals", Description = "Color of the down triangles.", Order = 370)]
	public CrossColor SignalDownColor
	{
		get => _signalDownColor;
		set
		{
			_signalDownColor = value;
			RedrawChart();
		}
	}

	[Display(Name = "Enabled", GroupName = "Alerts", Description = "Alerts when the delta of a bar reaches the selected level (only in realtime).", Order = 400)]
	public bool UseAlerts
	{
		get => _alertsEnabled;
		set => _alertsEnabled = value;
	}

	[Display(Name = "Up level", GroupName = "Alerts", Description = "Level used for the up alerts.", Order = 410)]
	public ThresholdLevel AlertUpLevel
	{
		get => _alertUpLevel;
		set => _alertUpLevel = value;
	}

	[Display(Name = "Down level", GroupName = "Alerts", Description = "Level used for the down alerts.", Order = 420)]
	public ThresholdLevel AlertDownLevel
	{
		get => _alertDownLevel;
		set => _alertDownLevel = value;
	}

	[Display(Name = "Only at bar close", GroupName = "Alerts", Description = "On: alerts when a bar closes with its delta beyond the level. Off: alerts as soon as the delta of the forming bar reaches the level.", Order = 430)]
	public bool AlertAtBarClose
	{
		get => _alertAtBarClose;
		set => _alertAtBarClose = value;
	}

	[Display(Name = "Minimum bars between alerts", GroupName = "Alerts", Description = "Minimum number of bars between two alerts of the same side.", Order = 440)]
	[Range(0, 1000)]
	public int AlertCooldownBars
	{
		get => _alertCooldownBars;
		set => _alertCooldownBars = value;
	}

	[Display(Name = "Alert sound", GroupName = "Alerts", Description = "Sound file of the alerts.", Order = 450)]
	public string AlertFile
	{
		get => _alertFile;
		set => _alertFile = value;
	}

	[Display(Name = "Show average", GroupName = "Average", Description = "Draws a moving average of the bar delta.", Order = 500)]
	public bool ShowAverage
	{
		get => _showAverage;
		set
		{
			_showAverage = value;
			_averageSeries.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(Name = "Period", GroupName = "Average", Description = "Number of bars of the average.", Order = 510)]
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

	[Display(Name = "Type", GroupName = "Average", Description = "Simple (SMA) or exponential (EMA) moving average.", Order = 520)]
	public AverageType AverageKind
	{
		get => _averageType;
		set
		{
			_averageType = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Coloring", GroupName = "Average", Description = "Fixed: the line color. Zero cross: up color at or above zero, down color below. Slope: up color while the average rises or stays, down color while it falls.", Order = 530)]
	public AverageColoring AverageColorMode
	{
		get => _averageColoring;
		set
		{
			_averageColoring = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Color", GroupName = "Average", Description = "Line color with fixed coloring.", Order = 540)]
	public CrossColor AverageColor
	{
		get => _averageSeries.Color;
		set
		{
			_averageSeries.Color = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Up color", GroupName = "Average", Description = "Zero cross and slope coloring: color above zero or while rising.", Order = 550)]
	public CrossColor AverageUpColor
	{
		get => _averageUpColor;
		set
		{
			_averageUpColor = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Down color", GroupName = "Average", Description = "Zero cross and slope coloring: color below zero or while falling.", Order = 560)]
	public CrossColor AverageDownColor
	{
		get => _averageDownColor;
		set
		{
			_averageDownColor = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Line width", GroupName = "Average", Description = "Width of the average line.", Order = 570)]
	[Range(1, 20)]
	public int AverageWidth
	{
		get => _averageSeries.Width;
		set => _averageSeries.Width = value;
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

			if (InWindow(bar - 1))
				AddSamples(GetCandle(bar - 1));
		}

		if (IsSessionStart(bar))
		{
			_positive.Reset();
			_negative.Reset();
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
