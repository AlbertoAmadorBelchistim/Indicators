using OFT.Rendering.Context;
using System.Collections.Generic;
using System.ComponentModel;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("LevelsLolo")]
    public class LevelsLolo : Indicator
    {
        #region Nested types

        private enum LabelCategory { Combo, LargeGamma, VolTrigger, CallWall, PutWall, ZeroGamma, Other }

        private sealed class LabelToken
        {
            public LabelCategory Cat;
            public string Raw = "";
            public int? Rank;
            public bool Is0DTE;
            public int EffectiveRank => Rank ?? 0;
        }

        private sealed class MergedLevel
        {
            public decimal Price;
            public List<LabelToken> Labels = new();
            public LabelToken Winner;
            public string LabelText = "";
        }

        #endregion

        #region Constructor

        public LevelsLolo() : base(false)
        {
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            DrawAbovePrice = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
        }

        #endregion

        #region Overrides

        protected override void OnCalculate(int bar, decimal value) { }

        protected override void OnRender(RenderContext context, DrawingLayouts layout) { }

        #endregion

        #region Static helpers

        private static int CategoryPriority(LabelCategory c) => c switch
        {
            LabelCategory.VolTrigger => 0,
            LabelCategory.LargeGamma => 1,
            LabelCategory.CallWall or LabelCategory.PutWall => 2,
            LabelCategory.Combo => 3,
            LabelCategory.ZeroGamma => 4,
            _ => 5,
        };

        private static string FormatLabel(LabelToken l)
        {
            string baseName = l.Cat switch
            {
                LabelCategory.Combo => "CO",
                LabelCategory.LargeGamma => "LG",
                LabelCategory.VolTrigger => "VT",
                LabelCategory.CallWall => "CW",
                LabelCategory.PutWall => "PW",
                LabelCategory.ZeroGamma => "ZG",
                _ => l.Raw.ToUpperInvariant()
            };

            // ZG is informational; keep concise tag
            string rankTxt = l.Rank.HasValue ? l.Rank.Value.ToString() : "";
            string suffix = l.Is0DTE ? " 0DTE" : "";

            return (baseName + rankTxt + suffix).Trim();
        }

        #endregion
    }
}
