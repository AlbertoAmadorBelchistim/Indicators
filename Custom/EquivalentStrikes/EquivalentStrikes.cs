using ATAS.Indicators;
using OFT.Rendering.Context;
using System.ComponentModel;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("EquivalentStrikes")]
    public sealed class EquivalentStrikes : Indicator
    {
        #region Ctor

        public EquivalentStrikes()
            : base(useCandles: true)
        {
            // Overlay on the main price panel. Custom drawing is enabled from
            // the first commit so later render commits introduce no
            // behavioural diff coming from the constructor itself.
            DenyToChangePanel = true;
            DrawAbovePrice = true;
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            // The indicator renders entirely through OnRender; the base-class
            // series has nothing to plot and would otherwise show up in the
            // Drawing panel as an empty 1px solid line entry.
            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
        }

        #endregion

        #region Protected Methods

        protected override void OnCalculate(int bar, decimal value)
        {
            // Shell: the strike grid is fully derived from the anchor and the
            // visible price range, so nothing is computed per bar yet.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Shell: grid rendering lands in a later commit.
        }

        #endregion
    }
}
