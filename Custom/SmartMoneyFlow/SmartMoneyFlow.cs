namespace ATAS.Indicators.Technical;

using System.ComponentModel;

using ATAS.Indicators.Drawing;

using Utils.Common.Logging;

[DisplayName("Smart Money Flow")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Description("Cumulative delta of five trade-size filters, with the smart money spread and its signal line.")]
public class SmartMoneyFlow : Indicator
{
	#region Fields

	// Number of trade-size filters.
	private const int FilterCount = 5;

	// One cumulative delta line per trade-size filter. Filter 1 replaces the default series.
	private readonly ValueDataSeries[] _filterSeries = new ValueDataSeries[FilterCount];

	// Smart money minus dumb money, drawn as a histogram.
	private readonly ValueDataSeries _spreadSeries = new("SpreadSeries", "Smart Money Spread")
	{
		VisualType = VisualMode.Hide,
		Width = 3,
		ShowZeroValue = true,
		UseMinimizedModeIfEnabled = true
	};

	// Simple moving average of the spread.
	private readonly ValueDataSeries _signalSeries = new("SignalSeries", "Signal")
	{
		Color = CrossColor.FromArgb(255, 255, 255, 255),
		VisualType = VisualMode.Hide,
		Width = 2,
		UseMinimizedModeIfEnabled = true
	};

	#endregion

	#region Properties

	#endregion

	#region Ctor

	public SmartMoneyFlow()
		: base(true)
	{
		Panel = IndicatorDataProvider.NewPanel;
		DenyToChangePanel = true;

		// Default line colors and widths grow with the trade size: gray, cyan, royal blue, orange, firebrick.
		var colors = new[]
		{
			CrossColor.FromArgb(255, 128, 128, 128),
			CrossColor.FromArgb(255, 0, 255, 255),
			CrossColor.FromArgb(255, 65, 105, 225),
			CrossColor.FromArgb(255, 255, 165, 0),
			CrossColor.FromArgb(255, 178, 34, 34)
		};
		var widths = new[] { 1, 2, 2, 3, 4 };

		for (var i = 0; i < FilterCount; i++)
		{
			_filterSeries[i] = new ValueDataSeries($"Filter{i + 1}Series", $"Filter {i + 1}")
			{
				Color = colors[i],
				Width = widths[i],
				IsHidden = true,
				ShowZeroValue = false,
				UseMinimizedModeIfEnabled = true
			};
		}

		DataSeries[0] = _filterSeries[0];

		for (var i = 1; i < FilterCount; i++)
			DataSeries.Add(_filterSeries[i]);

		DataSeries.Add(_spreadSeries);
		DataSeries.Add(_signalSeries);
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"SmartMoneyFlow: initialized ({typeof(SmartMoneyFlow).Assembly.GetName().Version}).");
	}

	protected override void OnCalculate(int bar, decimal value)
	{
	}

	#endregion

	#region Private Methods

	#endregion
}
