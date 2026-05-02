using System.ComponentModel;

namespace ATAS.Indicators.Technical
{
    [DisplayName("Delta Patterns")]
    [Category(IndicatorCategories.VolumeOrderFlow)]
    public class DeltaPatterns : Indicator
    {
        #region Ctor

        public DeltaPatterns()
            : base(useCandles: true)
        {
            Panel = IndicatorDataProvider.NewPanel;
            DenyToChangePanel = true;

            // Hide the implicit DataSeries[0] so the Drawing panel
            // does not list a phantom 1px line for the indicator.
            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
        }

        #endregion

        #region Protected Methods

        protected override void OnCalculate(int bar, decimal value)
        {
            // Engine, classification, panel rendering, chart overlay
            // and alerts all arrive in follow-up commits.
        }

        #endregion
    }
}
