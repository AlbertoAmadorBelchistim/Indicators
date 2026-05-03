using ATAS.Indicators.Drawing;
﻿using System.ComponentModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace ATAS.Indicators.Technical
{
    [DisplayName("Delta Patterns")]
    [Category(IndicatorCategories.VolumeOrderFlow)]
    public class DeltaPatterns : Indicator
    {
		#region Nested Types: Pattern Model
		public enum DeltaPattern
		{
			None,
			AggressiveBuy,
			AggressiveSell,
			DominanceBuy,
			DominanceSell,
			DivergenceBullish,
			DivergenceBearish,
			ReversalBuy,
			ReversalSell,
			NeutralStruggle,
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class PatternCategory
		{
			[DisplayName("Enabled")]
			public bool Enabled { get; set; } = true;

			[DisplayName("Visible")]
			public bool Visible { get; set; } = true;

			[DisplayName("Color")]
			public CrossColor Color { get; set; }

			[DisplayName("Min Delta %")]
			public decimal MinDeltaPercent { get; set; }

			[DisplayName("Enable Alert")]
			public bool EnableAlert { get; set; }

			public override string ToString()
			{
				if (!Enabled)
					return "Disabled";

				var visibility = Visible ? "Visible" : "Hidden";
				var alert = EnableAlert ? " · Alert" : string.Empty;

				return MinDeltaPercent > 0m
					? $"{visibility} · {MinDeltaPercent:0.#}%{alert}"
					: $"{visibility}{alert}";
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class DominancePatternCategory : PatternCategory
		{
			[DisplayName("Wick Tolerance %")]
			public decimal WickTolerancePercent { get; set; }
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class ReversalPatternCategory : PatternCategory
		{
			[DisplayName("Close Min %")]
			public decimal ClosePercent { get; set; }
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class NeutralPatternCategory : PatternCategory
		{
			[DisplayName("Struggle %")]
			public decimal StrugglePercent { get; set; }
		}

        #endregion
        #region Properties: Patterns

        [Display(Name = "Aggressive", GroupName = "Patterns", Order = 10,
			Description = "Raw absolute delta of the rolling window over the threshold percentage.")]
		public PatternCategory Aggressive { get; set; } = new PatternCategory
		{
			Color = System.Drawing.Color.Lime.Convert(),
			MinDeltaPercent = 15m,
		};

		[Display(Name = "Dominance", GroupName = "Patterns", Order = 20,
			Description = "Sustained one-sided pressure with negligible counter-excursion in the rolling window.")]
		public DominancePatternCategory Dominance { get; set; } = new DominancePatternCategory
		{
			Color = System.Drawing.Color.ForestGreen.Convert(),
			MinDeltaPercent = 12m,
			WickTolerancePercent = 0.1m,
		};

		[Display(Name = "Divergence", GroupName = "Patterns", Order = 30,
			Description = "The rolling window's price direction contradicts the net delta direction.")]
		public PatternCategory Divergence { get; set; } = new PatternCategory
		{
			Color = System.Drawing.Color.Yellow.Convert(),
			MinDeltaPercent = 10m,
		};

		[Display(Name = "Reversal", GroupName = "Patterns", Order = 40,
			Description = "Delta extreme was reached and the window then closed past the threshold in the opposite direction. MinDeltaPercent is the extreme reached; ClosePercent is the close-side confirmation.")]
		public ReversalPatternCategory Reversal { get; set; } = new ReversalPatternCategory
		{
			Color = System.Drawing.Color.Cyan.Convert(),
			MinDeltaPercent = 10m,
			ClosePercent = 2m,
		};

		[Display(Name = "Neutral", GroupName = "Patterns", Order = 50,
			Description = "Small net delta combined with high internal struggle range. MinDeltaPercent is the maximum close range; StrugglePercent is the internal volatility floor.")]
		public NeutralPatternCategory Neutral { get; set; } = new NeutralPatternCategory
		{
			Color = System.Drawing.Color.Silver.Convert(),
			MinDeltaPercent = 1m,
			StrugglePercent = 12m,
		};

		[Display(Name = "Normal", GroupName = "Patterns", Order = 60,
			Description = "Background coloring for windows that did not match any other pattern.")]
		public PatternCategory Normal { get; set; } = new PatternCategory
		{
			Color = System.Drawing.Color.DimGray.Convert(),
			Visible = false,
		};

        #endregion
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
