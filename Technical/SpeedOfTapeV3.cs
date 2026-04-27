using ATAS.Indicators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utils.Common.Logging;

namespace ATAS.Indicators.Technical
{
    [DisplayName("Speed of Tape V3")]
    [Category(IndicatorCategories.VolumeOrderFlow)]
    [Description("Event-driven tape speed detector with multi-burst tracking, symmetric Buy/Sell context, percentile-based threshold over a rolling time window, persistent zones in the price panel, fading extension lines for primary events, and a floating info panel with the most recent bursts.")]
    public class SpeedOfTapeV3 : Indicator
    {
        #region Nested types

        /// <summary>
        /// Metric the engine computes over the sliding time window.
        /// </summary>
        public enum SpeedType
        {
            [Display(Name = "Ticks (HFT)")] Ticks,
            [Display(Name = "Volume (Blocks)")] Volume,
            [Display(Name = "Delta (Aggression)")] Delta,
            [Display(Name = "Buy Volume")] Buys,
            [Display(Name = "Sell Volume")] Sells
        }

        /// <summary>
        /// Single trade snapshot kept inside the rolling time window.
        /// Direction: +1 buy, -1 sell.
        /// </summary>
        private readonly struct TickSnapshot
        {
            public TickSnapshot(System.DateTime time, decimal volume, int direction, decimal price)
            {
                Time = time;
                Volume = volume;
                Direction = direction;
                Price = price;
            }

            public System.DateTime Time { get; }
            public decimal Volume { get; }
            public int Direction { get; }
            public decimal Price { get; }
        }

        #endregion

        #region Fields

        // Sliding window of trades observed in the last TimeWindow seconds.
        // Populated by OnNewTrade (C03) and OnCumulativeTradesResponse (C12).
        private readonly Queue<TickSnapshot> _tickQueue = new Queue<TickSnapshot>();

        // Settings backing fields. Kept private + exposed via Properties so
        // the setters can trigger RecalculateValues on parameter changes.
        private int _timeWindow = 5;
        private SpeedType _dataType = SpeedType.Ticks;
        private int _contextWindowMinutes = 15;
        private int _thresholdPercentile = 95;

        // Latest instantaneous speed computed over the rolling tick queue.
        // Updated on every OnNewTrade. Read by the histogram (C04), event
        // detection (C07) and the diagnostic tracepoints used in smoke tests.
        private decimal _currentInstantSpeed;

        #endregion

        #region Properties

        [Display(Name = "Time window (seconds)",
                 GroupName = "Calculation",
                 Description = "Length of the sliding window over which the speed metric is computed. Smaller values react faster but are noisier; larger values smooth out micro-bursts.",
                 Order = 10)]
        [Range(1, 600)]
        public int TimeWindow
        {
            get => _timeWindow;
            set { _timeWindow = value; RecalculateValues(); }
        }

        [Display(Name = "Data type",
                 GroupName = "Calculation",
                 Description = "Which metric the engine accumulates inside the time window. Ticks counts trades; Volume sums lots; Delta is signed buy-minus-sell volume; Buy/Sell Volume isolate one side.",
                 Order = 20)]
        public SpeedType DataType
        {
            get => _dataType;
            set { _dataType = value; RecalculateValues(); }
        }

        [Display(Name = "Context window (minutes)",
                 GroupName = "Threshold",
                 Description = "How far back the engine looks to compute the percentile threshold. Adapts to time-of-day rhythm: short enough to track session phases, long enough to be statistically stable.",
                 Order = 10)]
        [Range(1, 120)]
        public int ContextWindowMinutes
        {
            get => _contextWindowMinutes;
            set { _contextWindowMinutes = value; RecalculateValues(); }
        }

        [Display(Name = "Threshold percentile",
                 GroupName = "Threshold",
                 Description = "Percentile of recent speed observations above which a burst is detected. P95 means 'speeds that occur only 5% of the time in the recent context'. Higher values are more selective.",
                 Order = 20)]
        [Range(50, 99)]
        public int ThresholdPercentile
        {
            get => _thresholdPercentile;
            set { _thresholdPercentile = value; RecalculateValues(); }
        }

        #endregion

        #region ctor

        public SpeedOfTapeV3() : base(true)
        {
            Panel = IndicatorDataProvider.NewPanel;

            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

            this.LogInfo("SpeedOfTapeV3 scaffold loaded");
        }

        #endregion

        #region Overrides: Lifecycle

        protected override void OnCalculate(int bar, decimal value)
        {
            // Engine lives in OnNewTrade.
            // OnCalculate must be overridden to satisfy BaseIndicator's
            // abstract contract; intentionally left empty.
        }

        // Future lifecycle overrides:
        //   OnFinishRecalculate — C12 (historical replay trigger)
        //   OnDispose           — C14 (cleanup + thread-safety pass)

        #endregion

        #region Overrides: Market events

        /// <summary>
        /// Live-only entry point. Called by ATAS once per real-time trade.
        /// Adds the trade to the rolling window keyed on the trade's own
        /// timestamp (NOT DateTime.UtcNow — that was the V2 reproducibility
        /// bug between live and historical paths). Then trims the window and
        /// recomputes the instantaneous speed.
        /// </summary>
        protected override void OnNewTrade(MarketDataArg trade)
        {
            var direction = trade.Direction == TradeDirection.Buy ? 1 : -1;
            var snap = new TickSnapshot(trade.Time, trade.Volume, direction, trade.Price);
            _tickQueue.Enqueue(snap);

            TrimToTimeWindow(trade.Time);
            _currentInstantSpeed = ComputeInstantSpeed();
        }

        // Future market events:
        //   OnCumulativeTradesResponse — C12 (historical replay)

        #endregion

        #region Private methods: Engine

        /// <summary>
        /// Drops trades older than now - TimeWindow seconds from the front of
        /// the queue. Called after every enqueue so the queue length is always
        /// bounded by the configured time window.
        /// </summary>
        private void TrimToTimeWindow(DateTime now)
        {
            var cutoff = now.AddSeconds(-_timeWindow);
            while (_tickQueue.Count > 0 && _tickQueue.Peek().Time <= cutoff)
                _tickQueue.Dequeue();
        }

        /// <summary>
        /// Computes the instantaneous speed value for the currently selected
        /// DataType, by aggregating over the trades inside the rolling window.
        /// Returns 0 when the queue is empty.
        ///
        /// In C05 this method will be refactored to return a richer snapshot
        /// carrying buy/sell metrics in parallel, to support symmetric
        /// burst tracking.
        /// </summary>
        private decimal ComputeInstantSpeed()
        {
            if (_tickQueue.Count == 0) return 0;

            int ticks = _tickQueue.Count;
            decimal vol = 0, delta = 0, buys = 0, sells = 0;

            foreach (var t in _tickQueue)
            {
                vol += t.Volume;
                if (t.Direction == 1) { buys += t.Volume; delta += t.Volume; }
                else { sells += t.Volume; delta -= t.Volume; }
            }

            return _dataType switch
            {
                SpeedType.Ticks => ticks,
                SpeedType.Volume => vol,
                SpeedType.Delta => Math.Abs(delta),
                SpeedType.Buys => buys,
                SpeedType.Sells => sells,
                _ => 0
            };
        }

        #endregion
    }
}
