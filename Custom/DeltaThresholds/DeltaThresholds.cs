namespace ATAS.Indicators.Technical;

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

	// Sets the levels of a bar that has just appeared.
	private void OpenBar(int bar)
	{
		while (_levels.Count < bar)
			_levels.Add(default);

		var levels = new Levels(_upMajorLevel, _upMinorLevel, _downMinorLevel, _downMajorLevel);
		_levels.Add(levels);

		_upMajorSeries[bar] = levels.UpMajor;
		_upMinorSeries[bar] = levels.UpMinor;
		_downMinorSeries[bar] = levels.DownMinor;
		_downMajorSeries[bar] = levels.DownMajor;
	}

	#endregion
}
