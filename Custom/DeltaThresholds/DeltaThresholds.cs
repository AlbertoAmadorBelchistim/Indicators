namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using OFT.Attributes.Editors;

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

	#endregion

	#region Ctor

	public DeltaThresholds()
		: base(true)
	{
		// Its own panel by default; it can be moved to another panel, for example the Delta indicator's.
		Panel = IndicatorDataProvider.NewPanel;

		DataSeries[0] = _deltaSeries;
		DataSeries.Add(_upMajorSeries);
		DataSeries.Add(_upMinorSeries);
		DataSeries.Add(_downMinorSeries);
		DataSeries.Add(_downMajorSeries);

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
		_levels.Clear();
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

		if (bar > 0 && InWindow(bar - 1))
			AddSamples(GetCandle(bar - 1));

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
