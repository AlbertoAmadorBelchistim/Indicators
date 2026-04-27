using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
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

        /// <summary>
        /// Aggregated metrics over the rolling window at a single instant.
        /// Buy and Sell volumes are tracked in parallel — the user-selected
        /// DataType only chooses which scalar gets compared to the threshold,
        /// while the rest of the system (event color, efficiency, primary
        /// direction, zone bounds) reads the rest of the snapshot directly.
        /// </summary>
        private readonly struct SpeedSnapshot
        {
            public SpeedSnapshot(int ticks, decimal volume, decimal buys, decimal sells,
                                 decimal high, decimal low, SpeedType dataType)
            {
                Ticks = ticks;
                Volume = volume;
                Buys = buys;
                Sells = sells;
                Delta = buys - sells;
                High = high;
                Low = low;
                Efficiency = volume == 0 ? 0m : Math.Abs(Delta) / volume;
                IsBuyDominant = Delta >= 0;
                Speed = dataType switch
                {
                    SpeedType.Ticks => ticks,
                    SpeedType.Volume => volume,
                    SpeedType.Delta => Math.Abs(Delta),
                    SpeedType.Buys => buys,
                    SpeedType.Sells => sells,
                    _ => 0m
                };
            }

            public int Ticks { get; }
            public decimal Volume { get; }
            public decimal Buys { get; }
            public decimal Sells { get; }
            public decimal Delta { get; }
            public decimal High { get; }
            public decimal Low { get; }
            public decimal Efficiency { get; }
            public bool IsBuyDominant { get; }
            public decimal Speed { get; }
        }

        /// <summary>
        /// Single (timestamp, speed) datapoint kept inside the rolling
        /// context buffer used to compute the percentile threshold.
        /// </summary>
        private readonly struct SpeedObservation
        {
            public SpeedObservation(DateTime time, decimal speed)
            {
                Time = time;
                Speed = speed;
            }
            public DateTime Time { get; }
            public decimal Speed { get; }
        }

        #endregion

        #region Fields

        // ─── DataSeries displayed in the panel ──────────────────────────────
        // Speed histogram: live bar pulses with the current instantaneous
        // speed; closed bars are frozen at their high water mark in
        // UpdateHistogram() when a bar transition is detected.
        private readonly ValueDataSeries _renderSeries = new ValueDataSeries("Speed")
        {
            VisualType = VisualMode.Histogram,
            ShowZeroValue = false,
            UseMinimizedModeIfEnabled = false,
            ResetAlertsOnNewBar = true,
            ScaleIt = true
        };

        // Threshold line: aqua horizontal level showing the current
        // percentile-derived burst boundary. Updated once per second from
        // the rolling speed buffer in MaybeSampleSpeed.
        private readonly ValueDataSeries _thresholdSeries = new ValueDataSeries("Threshold")
        {
            VisualType = VisualMode.Line,
            Color = System.Drawing.Color.Aqua.Convert(),
            Width = 2,
            ScaleIt = true,
            ShowZeroValue = false
        };

        // ─── Engine state ───────────────────────────────────────────────────
        // Sliding window of trades observed in the last TimeWindow seconds.
        // Populated by OnNewTrade (C03) and OnCumulativeTradesResponse (C12).
        private readonly Queue<TickSnapshot> _tickQueue = new Queue<TickSnapshot>();

        // Latest engine snapshot computed over the rolling tick queue.
        // Updated on every OnNewTrade. The histogram reads .Speed; downstream
        // consumers (event detection, coloring, info panel) read the rest.
        private SpeedSnapshot _currentSnapshot;

        // Bar transition tracking. _lastBar is -1 until the first trade
        // arrives. _currentBarHwm tracks the highest instantaneous speed
        // reached while the live bar is in progress; when a new bar starts
        // the HWM is committed to _renderSeries[oldBar].
        private int _lastBar = -1;
        private decimal _currentBarHwm;

        // How often a new speed observation is sampled into the context
        // buffer. 1 Hz keeps the buffer at ~900 entries for a 15-min window
        // and the percentile compute trivial — cheap and robust enough for
        // session-rhythm tracking.
        private static readonly TimeSpan SamplePeriod = TimeSpan.FromSeconds(1);

        // Rolling buffer of speed observations over the last
        // ContextWindowMinutes minutes. Populated by MaybeSampleSpeed,
        // trimmed by TrimSpeedBuffer, read by ComputePercentile.
        private readonly Queue<SpeedObservation> _speedBuffer = new Queue<SpeedObservation>();

        // Trade-time of the last observation added; used to rate-limit
        // MaybeSampleSpeed to one entry per SamplePeriod.
        private DateTime _lastSampleTime = DateTime.MinValue;

        // Cached percentile value, recomputed every time a new observation
        // enters the buffer. Read by UpdateHistogram (writes the threshold
        // series) and by event detection in C07.
        private decimal _currentThreshold;

        // ─── Settings backing fields ────────────────────────────────────────
        // Kept private + exposed via Properties so the setters can trigger
        // RecalculateValues on parameter changes.
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
            set
            {
                _dataType = value;

                // Buffer holds observations in the previous metric's units;
                // clear so the threshold doesn't compare apples to oranges.
                _speedBuffer.Clear();
                _lastSampleTime = DateTime.MinValue;
                _currentThreshold = 0m;

                RecalculateValues();
            }
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

            DataSeries.Add(_renderSeries);
            DataSeries.Add(_thresholdSeries);

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
            _currentSnapshot = ComputeInstantSnapshot();
            MaybeSampleSpeed(trade.Time);
            UpdateHistogram();
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
        /// Single-pass aggregation over the rolling tick queue. Returns all
        /// metrics in parallel — buys and sells are accumulated independently
        /// and the snapshot exposes both, plus the high/low price range
        /// observed during the window. The caller decides what to do with
        /// each field.
        /// </summary>
        private SpeedSnapshot ComputeInstantSnapshot()
        {
            if (_tickQueue.Count == 0)
                return new SpeedSnapshot(0, 0m, 0m, 0m, 0m, 0m, _dataType);

            int ticks = _tickQueue.Count;
            decimal vol = 0m, buys = 0m, sells = 0m;
            decimal high = decimal.MinValue, low = decimal.MaxValue;

            foreach (var t in _tickQueue)
            {
                vol += t.Volume;
                if (t.Direction == 1) buys += t.Volume;
                else sells += t.Volume;

                if (t.Price > high) high = t.Price;
                if (t.Price < low) low = t.Price;
            }

            return new SpeedSnapshot(ticks, vol, buys, sells, high, low, _dataType);
        }

        /// <summary>
        /// Adds the current snapshot's Speed to the rolling context buffer
        /// at most once per SamplePeriod. Called on every trade; the
        /// rate-limit prevents the buffer from blowing up in active markets
        /// while still capturing enough observations for a stable percentile
        /// over the configured ContextWindowMinutes.
        /// </summary>
        private void MaybeSampleSpeed(DateTime now)
        {
            if (now - _lastSampleTime < SamplePeriod) return;
            _lastSampleTime = now;

            _speedBuffer.Enqueue(new SpeedObservation(now, _currentSnapshot.Speed));
            TrimSpeedBuffer(now);
            _currentThreshold = ComputePercentile(_thresholdPercentile);
        }

        /// <summary>
        /// Drops observations older than now - ContextWindowMinutes from the
        /// front of the buffer. Mirrors TrimToTimeWindow but on minutes
        /// instead of seconds.
        /// </summary>
        private void TrimSpeedBuffer(DateTime now)
        {
            var cutoff = now.AddMinutes(-_contextWindowMinutes);
            while (_speedBuffer.Count > 0 && _speedBuffer.Peek().Time <= cutoff)
                _speedBuffer.Dequeue();
        }

        /// <summary>
        /// Returns the value at the requested percentile of the current
        /// buffer using the nearest-rank method. P95 of N samples returns
        /// the element at sorted index floor(0.95 * N). Returns 0 when the
        /// buffer is empty (e.g. fresh start before the first sample).
        /// </summary>
        private decimal ComputePercentile(int percentile)
        {
            if (_speedBuffer.Count == 0) return 0m;

            var sorted = _speedBuffer.Select(o => o.Speed).OrderBy(s => s).ToArray();
            int idx = (int)(sorted.Length * percentile / 100m);
            if (idx >= sorted.Length) idx = sorted.Length - 1;
            return sorted[idx];
        }

        /// <summary>
        /// Bridges engine state to the histogram series. While the live bar
        /// is in progress, _renderSeries[bar] pulses with the current
        /// instantaneous speed. On bar transition, the just-closed bar is
        /// frozen at its HWM so closed history reflects peak intensity, not
        /// the value that happened to be live at the closing tick.
        /// </summary>
        private void UpdateHistogram()
        {
            int bar = CurrentBar - 1;
            if (bar < 0) return;

            if (bar != _lastBar)
            {
                if (_lastBar >= 0)
                    _renderSeries[_lastBar] = _currentBarHwm;

                _lastBar = bar;
                _currentBarHwm = _currentSnapshot.Speed;
            }
            else if (_currentSnapshot.Speed > _currentBarHwm)
            {
                _currentBarHwm = _currentSnapshot.Speed;
            }

            _renderSeries[bar] = _currentSnapshot.Speed;
            _thresholdSeries[bar] = _currentThreshold;
        }

        #endregion
    }
}
