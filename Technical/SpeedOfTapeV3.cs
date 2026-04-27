using ATAS.Indicators;
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

        #region Overrides

        protected override void OnCalculate(int bar, decimal value)
        {
            // Engine lives in OnNewTrade (added in C03).
            // OnCalculate must be overridden to satisfy BaseIndicator's
            // abstract contract; intentionally left empty.
        }

        // Future overrides: OnNewTrade (C03), OnCumulativeTradesResponse + OnFinishRecalculate (C12),
        // OnRender (C08), OnDispose (C14).

        #endregion

        #region Private methods

        // Engine, rendering helpers and alert dispatch.
        // Filled across C03–C13.

        #endregion
    }
}
