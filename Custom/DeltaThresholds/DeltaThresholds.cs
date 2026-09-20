namespace ATAS.Indicators.Technical;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using ATAS.Indicators.Drawing;

using Utils.Common.Logging;

[DisplayName("Delta Thresholds")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Description("Bar delta with fixed or session-based dynamic thresholds, price-chart signals and alerts.")]
public class DeltaThresholds : Indicator
{
	#region Fields

	// Delta of each bar (ask volume - bid volume), drawn as a histogram.
	private readonly ValueDataSeries _deltaSeries = new("DeltaSeries", "Delta")
	{
		VisualType = VisualMode.Histogram,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

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

	#endregion

	#region Ctor

	public DeltaThresholds()
		: base(true)
	{
		// Its own panel by default; it can be moved to another panel, for example the Delta indicator's.
		Panel = IndicatorDataProvider.NewPanel;

		DataSeries[0] = _deltaSeries;
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
	protected override void OnCalculate(int bar, decimal value)
	{
		var delta = GetCandle(bar).Delta;

		_deltaSeries[bar] = delta;
		_deltaSeries.Colors[bar] = (delta >= 0 ? _upColor : _downColor).Convert();
	}

	#endregion
}
