using ATAS.Indicators.Drawing;
using OFT.Attributes.Editors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Utils.Common.Logging;

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

        public struct BarMetrics
        {
            public decimal Delta;
            public decimal Volume;
            public decimal MaxRunningDelta;
            public decimal MinRunningDelta;
            public decimal OpenPrice;
            public decimal ClosePrice;
            public decimal HighPrice;
            public decimal LowPrice;
        }

        public sealed class BarMetricsCache
        {
            private BarMetrics[] _metrics = Array.Empty<BarMetrics>();
            private bool[] _populated = Array.Empty<bool>();

            public int BarCount { get; private set; }

            public void EnsureCapacity(int requiredCount)
            {
                if (requiredCount <= _metrics.Length) return;
                int newCapacity = Math.Max(requiredCount, Math.Max(_metrics.Length * 2, 1024));
                Array.Resize(ref _metrics, newCapacity);
                Array.Resize(ref _populated, newCapacity);
            }

            public void Set(int bar, BarMetrics value)
            {
                if (bar < 0) return;
                EnsureCapacity(bar + 1);
                _metrics[bar] = value;
                _populated[bar] = true;
                if (bar + 1 > BarCount) BarCount = bar + 1;
            }

            public bool TryGet(int bar, out BarMetrics value)
            {
                if (bar < 0 || bar >= _populated.Length || !_populated[bar])
                {
                    value = default;
                    return false;
                }
                value = _metrics[bar];
                return true;
            }

            public bool IsPopulated(int bar)
            {
                return bar >= 0 && bar < _populated.Length && _populated[bar];
            }

            public void Clear()
            {
                BarCount = 0;
                if (_populated.Length > 0)
                    Array.Clear(_populated, 0, _populated.Length);
            }
        }

        public sealed class BarPatternCache
        {
            private DeltaPattern[] _patterns = Array.Empty<DeltaPattern>();

            public int BarCount { get; private set; }

            public void EnsureCapacity(int requiredCount)
            {
                if (requiredCount <= _patterns.Length) return;
                int newCapacity = Math.Max(requiredCount, Math.Max(_patterns.Length * 2, 1024));
                Array.Resize(ref _patterns, newCapacity);
            }

            public void Set(int bar, DeltaPattern pattern)
            {
                if (bar < 0) return;
                EnsureCapacity(bar + 1);
                _patterns[bar] = pattern;
                if (bar + 1 > BarCount) BarCount = bar + 1;
            }

            public DeltaPattern Get(int bar)
            {
                if (bar < 0 || bar >= BarCount) return DeltaPattern.None;
                return _patterns[bar];
            }

            public void Clear()
            {
                BarCount = 0;
                if (_patterns.Length > 0)
                    Array.Clear(_patterns, 0, _patterns.Length);
            }
        }

        #endregion

        #region Fields

        private readonly object _stateLock = new object();
        private readonly RollingDeltaWindow _window = new RollingDeltaWindow();
        private readonly BarMetricsCache _metrics = new BarMetricsCache();
        private readonly BarPatternCache _patterns = new BarPatternCache();
        private bool _historyLoaded;

        private int _targetVolume = 2500;

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

        #region Properties: Calculation

        [Display(Name = "Target Volume", GroupName = "Calculation", Order = 1,
            Description = "Rolling window size in contracts. Each snapshot summarises the last N traded contracts.")]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public int TargetVolume
        {
            get => _targetVolume;
            set
            {
                var clamped = Math.Max(1, value);
                if (_targetVolume == clamped) return;
                _targetVolume = clamped;
                RecalculateValues();
            }
        }

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

        protected override void OnRecalculate()
        {
            lock (_stateLock)
            {
                _window.Reset();
                _metrics.Clear();
                _patterns.Clear();
                _historyLoaded = false;
            }
        }

        protected override void OnFinishRecalculate()
        {
            bool needsFetch;
            lock (_stateLock)
            {
                needsFetch = !_historyLoaded;
            }

            if (!needsFetch) return;
            if (CurrentBar < 1) return;

            var firstCandle = GetCandle(0);
            var lastCandle = GetCandle(CurrentBar - 1);
            var sessionStart = firstCandle.Time;
            var sessionEnd = lastCandle?.LastTime ?? firstCandle.LastTime;

            this.LogInfo($"DeltaPatterns: fetch started range={sessionStart:yyyy-MM-dd HH:mm:ss}-{sessionEnd:yyyy-MM-dd HH:mm:ss}, target {TargetVolume} contracts");

            var request = new CumulativeTradesRequest(sessionStart, sessionEnd, 0, 0);
            RequestForCumulativeTrades(request);
        }

        protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            if (cumulativeTrades == null) return;

            int totalBars = CurrentBar - 1;
            if (totalBars < 0) return;

            int tradeCount = 0;
            int barsCached = 0;

            try
            {
                var target = (decimal)TargetVolume;
                int currentBarIndex = 0;

                lock (_stateLock)
                {
                    _window.Reset();
                    _metrics.Clear();
                    _patterns.Clear();

                    foreach (var trade in cumulativeTrades)
                    {
                        tradeCount++;

                        while (currentBarIndex <= totalBars)
                        {
                            var candle = GetCandle(currentBarIndex);
                            if (candle != null && candle.LastTime < trade.Time)
                            {
                                SnapshotBar(currentBarIndex);
                                currentBarIndex++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        int direction = trade.Direction == TradeDirection.Buy ? 1 : -1;
                        _window.Push(trade.FirstPrice, trade.Volume, direction);
                        _window.Trim(target);
                    }

                    while (currentBarIndex <= totalBars)
                    {
                        SnapshotBar(currentBarIndex);
                        currentBarIndex++;
                    }

                    barsCached = _metrics.BarCount;
                    _historyLoaded = true;
                }

                this.LogInfo($"DeltaPatterns: fetch completed {tradeCount} trades, {barsCached} bars cached");
            }
            catch (Exception ex)
            {
                this.LogError($"DeltaPatterns: fetch failed - {ex.Message}");
            }
        }

        protected override void OnNewTrade(MarketDataArg trade)
        {
            lock (_stateLock)
            {
                if (!_historyLoaded) return;

                int direction = trade.Direction == TradeDirection.Buy ? 1 : -1;
                _window.Push(trade.Price, trade.Volume, direction);
                _window.Trim(TargetVolume);

                int currentBar = CurrentBar - 1;
                if (currentBar >= 0)
                    SnapshotBar(currentBar);
            }
        }

        #endregion

        #region Private Methods

        // Caller must hold _stateLock.
        private void SnapshotBar(int bar)
        {
            var snapshot = new BarMetrics
            {
                Delta = _window.CurrentDelta,
                Volume = _window.CurrentVolume,
                MaxRunningDelta = _window.MaxRunningDelta,
                MinRunningDelta = _window.MinRunningDelta,
                OpenPrice = _window.StartPrice,
                ClosePrice = _window.EndPrice,
                HighPrice = _window.HighPrice,
                LowPrice = _window.LowPrice,
            };
            _metrics.Set(bar, snapshot);
            _patterns.Set(bar, Classify(snapshot));
        }

        public DeltaPattern Classify(BarMetrics snapshot)
        {
            if (snapshot.Volume <= 0m) return DeltaPattern.None;

            return TryDetectDivergence(snapshot)
                ?? TryDetectReversal(snapshot)
                ?? TryDetectDominance(snapshot)
                ?? TryDetectAggressive(snapshot)
                ?? TryDetectNeutralStruggle(snapshot)
                ?? DeltaPattern.None;
        }

        private DeltaPattern? TryDetectDivergence(BarMetrics m)
        {
            if (!Divergence.Enabled) return null;

            decimal threshold = (decimal)TargetVolume * (Divergence.MinDeltaPercent / 100m);
            if (Math.Abs(m.Delta) <= threshold) return null;

            bool priceUp = m.ClosePrice > m.OpenPrice;
            bool deltaUp = m.Delta > 0m;
            if (priceUp == deltaUp) return null;

            return deltaUp ? DeltaPattern.DivergenceBullish : DeltaPattern.DivergenceBearish;
        }

        private DeltaPattern? TryDetectReversal(BarMetrics m)
        {
            if (!Reversal.Enabled) return null;

            decimal target = (decimal)TargetVolume;
            decimal extreme = target * (Reversal.MinDeltaPercent / 100m);
            decimal close = target * (Reversal.ClosePercent / 100m);

            if (m.MaxRunningDelta > extreme && m.Delta < -close)
                return DeltaPattern.ReversalSell;
            if (m.MinRunningDelta < -extreme && m.Delta > close)
                return DeltaPattern.ReversalBuy;
            return null;
        }

        private DeltaPattern? TryDetectDominance(BarMetrics m)
        {
            if (!Dominance.Enabled) return null;

            decimal target = (decimal)TargetVolume;
            decimal threshold = target * (Dominance.MinDeltaPercent / 100m);
            if (Math.Abs(m.Delta) <= threshold) return null;

            decimal wick = target * (Dominance.WickTolerancePercent / 100m);

            if (m.Delta > 0m && m.MinRunningDelta >= -wick)
                return DeltaPattern.DominanceBuy;
            if (m.Delta < 0m && m.MaxRunningDelta <= wick)
                return DeltaPattern.DominanceSell;
            return null;
        }

        private DeltaPattern? TryDetectAggressive(BarMetrics m)
        {
            if (!Aggressive.Enabled) return null;

            decimal threshold = (decimal)TargetVolume * (Aggressive.MinDeltaPercent / 100m);
            if (Math.Abs(m.Delta) <= threshold) return null;

            return m.Delta > 0m ? DeltaPattern.AggressiveBuy : DeltaPattern.AggressiveSell;
        }

        private DeltaPattern? TryDetectNeutralStruggle(BarMetrics m)
        {
            if (!Neutral.Enabled) return null;
            if (m.Volume <= 0m) return null;

            decimal deltaPercent = Math.Abs(m.Delta / m.Volume * 100m);
            if (deltaPercent > Neutral.MinDeltaPercent) return null;

            decimal struggle = (decimal)TargetVolume * (Neutral.StrugglePercent / 100m);
            if (m.MaxRunningDelta > struggle || m.MinRunningDelta < -struggle)
                return DeltaPattern.NeutralStruggle;
            return null;
        }

        #endregion
    }
}
