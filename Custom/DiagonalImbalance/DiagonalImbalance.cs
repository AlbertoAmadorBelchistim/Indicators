namespace ATAS.Indicators.Technical;

using System.ComponentModel;

using Utils.Common.Logging;

[DisplayName("Diagonal Imbalance")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Description("Detects diagonal bid/ask imbalances in the footprint and stacked imbalance zones.")]
public class DiagonalImbalance : Indicator
{
	#region Fields

	#endregion

	#region Properties

	#endregion

	#region Ctor

	public DiagonalImbalance()
		: base(true)
	{
		DenyToChangePanel = true;

		// The indicator draws on the price panel; the default series stays hidden
		// so it does not show up as an empty line in the chart or the Drawing panel.
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"DiagonalImbalance: initialized ({typeof(DiagonalImbalance).Assembly.GetName().Version}).");
	}

	protected override void OnCalculate(int bar, decimal value)
	{
	}

	#endregion

	#region Private Methods

	#endregion
}
