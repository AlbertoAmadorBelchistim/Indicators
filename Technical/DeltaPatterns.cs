using ATAS.Indicators.Drawing;
using System.Collections.Generic;
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

        #region Nested Types: Engine

        public sealed class RollingDeltaWindow
        {
            private struct TickRecord
            {
                public decimal Price;
                public decimal Volume;
                public int Direction;
                public long Seq;
                public decimal PrefixSum;
            }

            private readonly Queue<TickRecord> _ticks = new Queue<TickRecord>(4096);

            // Monotonic deques: front holds the current extreme.
            //   _maxRunDeque: prefix sums strictly DECREASE from front to back.
            //   _minRunDeque: prefix sums strictly INCREASE from front to back.
            //   _maxPriceDeque: prices strictly DECREASE from front to back.
            //   _minPriceDeque: prices strictly INCREASE from front to back.
            private readonly LinkedList<TickRecord> _maxRunDeque = new LinkedList<TickRecord>();
            private readonly LinkedList<TickRecord> _minRunDeque = new LinkedList<TickRecord>();
            private readonly LinkedList<TickRecord> _maxPriceDeque = new LinkedList<TickRecord>();
            private readonly LinkedList<TickRecord> _minPriceDeque = new LinkedList<TickRecord>();

            private long _nextSeq;
            private decimal _runningPrefixSum;
            private decimal _startPrefixSum;

            public int TickCount => _ticks.Count;

            public decimal CurrentVolume { get; private set; }

            public decimal CurrentDelta => _runningPrefixSum - _startPrefixSum;

            public decimal MaxRunningDelta
            {
                get
                {
                    if (_maxRunDeque.Count == 0) return 0m;
                    var candidate = _maxRunDeque.First.Value.PrefixSum - _startPrefixSum;
                    return candidate > 0m ? candidate : 0m;
                }
            }

            public decimal MinRunningDelta
            {
                get
                {
                    if (_minRunDeque.Count == 0) return 0m;
                    var candidate = _minRunDeque.First.Value.PrefixSum - _startPrefixSum;
                    return candidate < 0m ? candidate : 0m;
                }
            }

            public decimal StartPrice => _ticks.Count > 0 ? _ticks.Peek().Price : 0m;

            public decimal EndPrice { get; private set; }

            public decimal HighPrice => _maxPriceDeque.Count > 0 ? _maxPriceDeque.First.Value.Price : 0m;

            public decimal LowPrice => _minPriceDeque.Count > 0 ? _minPriceDeque.First.Value.Price : 0m;

            public void Push(decimal price, decimal volume, int direction)
            {
                var delta = volume * direction;
                _runningPrefixSum += delta;

                var record = new TickRecord
                {
                    Price = price,
                    Volume = volume,
                    Direction = direction,
                    Seq = _nextSeq++,
                    PrefixSum = _runningPrefixSum,
                };

                _ticks.Enqueue(record);
                CurrentVolume += volume;
                EndPrice = price;

                while (_maxRunDeque.Count > 0 && _maxRunDeque.Last.Value.PrefixSum <= record.PrefixSum)
                    _maxRunDeque.RemoveLast();
                _maxRunDeque.AddLast(record);

                while (_minRunDeque.Count > 0 && _minRunDeque.Last.Value.PrefixSum >= record.PrefixSum)
                    _minRunDeque.RemoveLast();
                _minRunDeque.AddLast(record);

                while (_maxPriceDeque.Count > 0 && _maxPriceDeque.Last.Value.Price <= price)
                    _maxPriceDeque.RemoveLast();
                _maxPriceDeque.AddLast(record);

                while (_minPriceDeque.Count > 0 && _minPriceDeque.Last.Value.Price >= price)
                    _minPriceDeque.RemoveLast();
                _minPriceDeque.AddLast(record);
            }

            public void Trim(decimal targetVolume)
            {
                while (_ticks.Count > 0)
                {
                    var oldest = _ticks.Peek();
                    if (CurrentVolume - oldest.Volume < targetVolume)
                        break;

                    _ticks.Dequeue();
                    CurrentVolume -= oldest.Volume;
                    _startPrefixSum = oldest.PrefixSum;

                    if (_maxRunDeque.Count > 0 && _maxRunDeque.First.Value.Seq == oldest.Seq)
                        _maxRunDeque.RemoveFirst();
                    if (_minRunDeque.Count > 0 && _minRunDeque.First.Value.Seq == oldest.Seq)
                        _minRunDeque.RemoveFirst();
                    if (_maxPriceDeque.Count > 0 && _maxPriceDeque.First.Value.Seq == oldest.Seq)
                        _maxPriceDeque.RemoveFirst();
                    if (_minPriceDeque.Count > 0 && _minPriceDeque.First.Value.Seq == oldest.Seq)
                        _minPriceDeque.RemoveFirst();
                }
            }

            public void Reset()
            {
                _ticks.Clear();
                _maxRunDeque.Clear();
                _minRunDeque.Clear();
                _maxPriceDeque.Clear();
                _minPriceDeque.Clear();
                CurrentVolume = 0m;
                EndPrice = 0m;
                _nextSeq = 0;
                _runningPrefixSum = 0m;
                _startPrefixSum = 0m;
            }
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
