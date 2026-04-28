using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
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

        /// <summary>
        /// A burst event detected when the engine's speed crossed the
        /// threshold upward. Carries the full SpeedSnapshot at the moment
        /// of crossing so downstream consumers (rectangles, extension line,
        /// info panel) read all metrics — direction, efficiency, volume,
        /// price range — from a single immutable record.
        /// </summary>
        private readonly struct EventRecord
        {
            public EventRecord(int bar, DateTime time, SpeedSnapshot snapshot)
            {
                Bar = bar;
                Time = time;
                Snapshot = snapshot;
            }
            public int Bar { get; }
            public DateTime Time { get; }
            public SpeedSnapshot Snapshot { get; }
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

        // Last observed speed. Used by DetectEvents for last-vs-current
        // upward-cross detection (event-based, not state-based — avoids
        // emitting duplicate events while speed lingers above threshold).
        private decimal _lastSpeed;

        // Multi-burst cache: each bar maps to the list of events detected
        // during it. Persistent (per spec — never pruned). The render layer
        // (C08+) iterates this dictionary to draw rectangles and extension
        // lines.
        private readonly Dictionary<int, List<EventRecord>> _eventsByBar
            = new Dictionary<int, List<EventRecord>>();

        // Color settings backing fields. The internal type is
        // System.Drawing.Color because RenderContext consumes that directly
        // during DrawZone; the panel-facing properties expose CrossColor and
        // bridge with .Convert() at the boundary.
        private System.Drawing.Color _buyColor = System.Drawing.Color.Lime;
        private System.Drawing.Color _sellColor = System.Drawing.Color.Red;
        private System.Drawing.Color _neutralColor = System.Drawing.Color.Gray;

        // Extension line settings backing fields. ExtensionBars: number of
        // bars right of the primary event drawn at full opacity. FadeBars:
        // additional bars drawn with linearly decreasing alpha, ending
        // nearly transparent.
        private int _extensionBars = 10;
        private int _fadeBars = 5;

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

                // All caches that depend on the speed metric must be cleared
                // when the metric (and thus the units) changes.
                _speedBuffer.Clear();
                _lastSampleTime = DateTime.MinValue;
                _currentThreshold = 0m;
                _lastSpeed = 0m;
                _eventsByBar.Clear();

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

        [Display(Name = "Buy color",
         GroupName = "Visuals",
         Description = "Color of zones whose dominant side is buying. Efficiency near 1 paints close to this color; lower efficiency blends toward the neutral color.",
         Order = 10)]
        public CrossColor BuyColor
        {
            get => _buyColor.Convert();
            set { _buyColor = value.Convert(); RedrawChart(); }
        }

        [Display(Name = "Sell color",
                 GroupName = "Visuals",
                 Description = "Color of zones whose dominant side is selling. Efficiency near 1 paints close to this color; lower efficiency blends toward the neutral color.",
                 Order = 20)]
        public CrossColor SellColor
        {
            get => _sellColor.Convert();
            set { _sellColor = value.Convert(); RedrawChart(); }
        }

        [Display(Name = "Neutral color",
                 GroupName = "Visuals",
                 Description = "Color used when a burst is balanced (efficiency below 0.3). Also acts as the blend origin for low-efficiency directional bursts.",
                 Order = 30)]
        public CrossColor NeutralColor
        {
            get => _neutralColor.Convert();
            set { _neutralColor = value.Convert(); RedrawChart(); }
        }

        [Display(Name = "Extension bars (full)",
         GroupName = "Visuals",
         Description = "Number of bars to the right of a primary event drawn at full opacity. The horizontal line marks the centre of the burst's price range as a transient support/resistance reference.",
         Order = 40)]
        [Range(0, 200)]
        public int ExtensionBars
        {
            get => _extensionBars;
            set { _extensionBars = value; RedrawChart(); }
        }

        [Display(Name = "Extension bars (fade)",
                 GroupName = "Visuals",
                 Description = "Number of bars after the full-opacity segment drawn with linearly decreasing alpha. Set to 0 for an abrupt cutoff; higher for a longer visual decay.",
                 Order = 50)]
        [Range(0, 100)]
        public int FadeBars
        {
            get => _fadeBars;
            set { _fadeBars = value; RedrawChart(); }
        }

        [Display(Name = "TEST: Inject burst",
                GroupName = "Diagnostics",
                Description = "Toggle to inject a synthetic burst event into the current bar. Used to smoke-test multi-burst detection without waiting for a natural cross. Removed in C15.",
                Order = 1000)]
        public bool TestInjectBurst
        {
            get => false;
            set { if (value) TestInjectBurstNow(); }
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
            DetectEvents(trade.Time);
            UpdateHistogram();
        }

        // Future market events:
        //   OnCumulativeTradesResponse — C12 (historical replay)

        #endregion

        #region Overrides: Rendering

        /// <summary>
        /// Render entry point. Called by ATAS once per render frame for
        /// every visible bar range. We iterate the events dictionary by
        /// visible bar number, look up events for each bar, and draw a
        /// zone rectangle per event. All bars and events are rendered on
        /// every pass — the dictionary is read-only here, all mutation
        /// happens in DetectEvents on the data thread.
        /// </summary>
        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (layout != DrawingLayouts.Final || ChartInfo == null || InstrumentInfo == null)
                return;

            context.SetClip(ChartInfo.PriceChartContainer.Region);

            var buyColor = _buyColor;
            var sellColor = _sellColor;
            var neutralColor = _neutralColor;

            // Pass 1: zone rectangles. The bar's primary event (highest speed)
            // gets a 3-pixel border; secondary events get 1-pixel borders. The
            // thickness difference creates a visual hierarchy at a glance —
            // event details live in the floating info panel (C11), not in the
            // chart itself.
            for (int bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
            {
                if (!_eventsByBar.TryGetValue(bar, out var events) || events.Count == 0)
                    continue;

                TryGetPrimaryEvent(bar, out var primary);

                foreach (var evt in events)
                {
                    var color = ResolveZoneColor(evt.Snapshot, buyColor, sellColor, neutralColor);
                    bool isPrimary =
                        evt.Time == primary.Time && evt.Snapshot.Speed == primary.Snapshot.Speed;
                    int penWidth = isPrimary ? 5 : 2;
                    DrawZone(context, bar, evt.Snapshot.High, evt.Snapshot.Low, color, penWidth);
                }
            }

            // Pass 2: primary event extension lines, one per bar that has
            // any events. Anchored at the primary event's centre price,
            // extending right with a fade-out tail.
            for (int bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
            {
                if (!TryGetPrimaryEvent(bar, out var primary)) continue;
                var color = ResolveZoneColor(primary.Snapshot, buyColor, sellColor, neutralColor);
                DrawExtensionLine(context, bar, primary.Snapshot, color);
            }

            context.ResetClip();
        }

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
        /// Event-based upward-cross detector. Compares the previous observed
        /// speed against the current one — when previous was at-or-below the
        /// threshold and current is strictly above, a new event is emitted
        /// for the current bar. The state-based alternative ("speed > threshold
        /// right now") would emit duplicate events on every trade while the
        /// burst persists; this version emits exactly one event per crossing.
        ///
        /// Skips emission while the threshold is still 0 (warmup, buffer
        /// hasn't sampled yet) so we don't spam events during the first
        /// second after the indicator loads.
        /// </summary>
        private void DetectEvents(DateTime time)
        {
            int bar = CurrentBar - 1;
            if (bar < 0) return;

            decimal current = _currentSnapshot.Speed;
            decimal threshold = _currentThreshold;

            if (threshold <= 0m)
            {
                _lastSpeed = current;
                return;
            }

            if (_lastSpeed <= threshold && current > threshold)
            {
                var evt = new EventRecord(bar, time, _currentSnapshot);

                if (!_eventsByBar.TryGetValue(bar, out var list))
                {
                    list = new List<EventRecord>();
                    _eventsByBar[bar] = list;
                }
                list.Add(evt);

                var side = _currentSnapshot.IsBuyDominant ? "buy" : "sell";
                this.LogInfo($"burst @ bar={bar} speed={current.ToString("0.00")} threshold={threshold.ToString("0.00")} side={side} eff={_currentSnapshot.Efficiency.ToString("0.00")} range={_currentSnapshot.Low}-{_currentSnapshot.High}");
            }

            _lastSpeed = current;
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

        #region Private methods: Rendering

        /// <summary>
        /// Decides which color to paint a zone with, based on the snapshot's
        /// dominant side and efficiency. Bursts under 30% efficiency are
        /// considered balanced and painted neutral regardless of side. Above
        /// that, the color blends from neutral toward the side color in
        /// proportion to efficiency: a 0.6-efficiency buy burst sits 60% of
        /// the way along the gradient from neutral to buy color.
        /// </summary>
        private System.Drawing.Color ResolveZoneColor(
            SpeedSnapshot snap,
            System.Drawing.Color buy,
            System.Drawing.Color sell,
            System.Drawing.Color neutral)
        {
            if (snap.Efficiency < 0.3m) return neutral;

            var target = snap.IsBuyDominant ? buy : sell;
            return Blend(neutral, target, (double)snap.Efficiency);
        }

        /// <summary>
        /// Linear RGB interpolation between two colors. ratio=0 returns
        /// `from`, ratio=1 returns `to`, intermediate values are mixed
        /// channel-wise. Clamped to [0, 1].
        /// </summary>
        private static System.Drawing.Color Blend(
            System.Drawing.Color from,
            System.Drawing.Color to,
            double ratio)
        {
            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;
            var r = (int)(from.R + (to.R - from.R) * ratio);
            var g = (int)(from.G + (to.G - from.G) * ratio);
            var b = (int)(from.B + (to.B - from.B) * ratio);
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Renders a zone as four corner brackets (camera-viewfinder style)
        /// rather than a full rectangle outline. The horizontal portions of
        /// each bracket are short enough to leave the centred footprint
        /// digits intact while still defining the zone's vertical extent
        /// unambiguously when multiple zones overlap.
        /// </summary>
        private void DrawZone(RenderContext context, int bar, decimal high, decimal low, System.Drawing.Color color, int penWidth)
        {
            if (high == low) high += InstrumentInfo.TickSize;

            int y1 = ChartInfo.GetYByPrice(high, true);
            int y2 = ChartInfo.GetYByPrice(low, false);
            int top = Math.Min(y1, y2);
            int bottom = Math.Max(y1, y2);
            int height = bottom - top;
            if (height < 1) height = 1;

            int x = ChartInfo.GetXByBar(bar, true);
            int width = (int)Math.Round((double)ChartInfo.PriceChartContainer.BarsWidth);
            if (width < 1) width = 1;
            int right = x + width;

            // Floor armLen at penWidth so the arm is always at least as long
            // as it is thick — otherwise corners degenerate into blobs at high
            // pen widths on small zones.
            int armLen = Math.Max(penWidth, Math.Min(8, Math.Min(width, height) / 3));
            int t = penWidth;

            // Each corner = two filled rectangles meeting at the corner pixel.
            // Top-left: horizontal bar going right + vertical bar going down.
            context.FillRectangle(color, new Rectangle(x, top, armLen, t));
            context.FillRectangle(color, new Rectangle(x, top, t, armLen));

            // Top-right: horizontal bar going left + vertical bar going down.
            context.FillRectangle(color, new Rectangle(right - armLen, top, armLen, t));
            context.FillRectangle(color, new Rectangle(right - t, top, t, armLen));

            // Bottom-left: horizontal bar going right + vertical bar going up.
            context.FillRectangle(color, new Rectangle(x, bottom - t, armLen, t));
            context.FillRectangle(color, new Rectangle(x, bottom - armLen, t, armLen));

            // Bottom-right: horizontal bar going left + vertical bar going up.
            context.FillRectangle(color, new Rectangle(right - armLen, bottom - t, armLen, t));
            context.FillRectangle(color, new Rectangle(right - t, bottom - armLen, t, armLen));
        }

        /// <summary>
        /// Returns the highest-speed event recorded for a bar, if any.
        /// Used to identify which event drives the persistent extension
        /// line (only one line per bar — the most aggressive burst).
        /// </summary>
        private bool TryGetPrimaryEvent(int bar, out EventRecord primary)
        {
            primary = default;
            if (!_eventsByBar.TryGetValue(bar, out var events) || events.Count == 0)
                return false;

            primary = events[0];
            var maxSpeed = primary.Snapshot.Speed;
            for (int i = 1; i < events.Count; i++)
            {
                if (events[i].Snapshot.Speed > maxSpeed)
                {
                    primary = events[i];
                    maxSpeed = events[i].Snapshot.Speed;
                }
            }
            return true;
        }

        /// <summary>
        /// Draws a horizontal line anchored at the snapshot's mid-price and
        /// extending to the right of the source bar. The first ExtensionBars
        /// bars are drawn at full opacity as a single segment; the next
        /// FadeBars are drawn one bar at a time with linearly decreasing
        /// alpha. Both use the same color as the zone rectangle for the
        /// primary event so the line and zone are visually unified.
        /// </summary>
        private void DrawExtensionLine(RenderContext context, int sourceBar, SpeedSnapshot snap, System.Drawing.Color color)
        {
            if (_extensionBars <= 0 && _fadeBars <= 0) return;

            decimal centerPrice = (snap.High + snap.Low) / 2m;
            int y = ChartInfo.GetYByPrice(centerPrice, false);

            // Full-opacity segment, drawn as a single line.
            if (_extensionBars > 0)
            {
                int x1 = ChartInfo.GetXByBar(sourceBar, true);
                int x2 = ChartInfo.GetXByBar(sourceBar + _extensionBars, true);
                var pen = new RenderPen(color, 2);
                context.DrawLine(pen, x1, y, x2, y);
            }

            // Fade segments: one bar each, alpha decreasing linearly.
            if (_fadeBars > 0)
            {
                for (int i = 0; i < _fadeBars; i++)
                {
                    double alphaRatio = 1.0 - (double)i / _fadeBars;
                    int alpha = (int)(255 * alphaRatio);
                    if (alpha <= 0) break;

                    var fadeColor = System.Drawing.Color.FromArgb(alpha, color.R, color.G, color.B);
                    int segStart = sourceBar + _extensionBars + i;
                    int x1 = ChartInfo.GetXByBar(segStart, true);
                    int x2 = ChartInfo.GetXByBar(segStart + 1, true);
                    var pen = new RenderPen(fadeColor, 2);
                    context.DrawLine(pen, x1, y, x2, y);
                }
            }
        }

        #endregion

        #region Private methods: Diagnostics

        /// <summary>
        /// TEMP: Forges a synthetic burst event and inserts it into the
        /// current bar's event list. Bypasses the cross-detection path
        /// (no threshold comparison, no _lastSpeed update) so the test is
        /// reproducible regardless of market conditions.
        ///
        /// Removed in C15 along with all TEST: properties.
        /// </summary>
        private void TestInjectBurstNow()
        {
            int bar = CurrentBar - 1;
            if (bar < 0) return;

            var candle = GetCandle(bar);
            decimal high = candle.High;
            decimal low = candle.Low;

            var forged = new SpeedSnapshot(
                ticks: 100,
                volume: 200m,
                buys: 150m,
                sells: 50m,
                high: high,
                low: low,
                dataType: _dataType);

            var evt = new EventRecord(bar, DateTime.UtcNow, forged);

            if (!_eventsByBar.TryGetValue(bar, out var list))
            {
                list = new List<EventRecord>();
                _eventsByBar[bar] = list;
            }
            list.Add(evt);

            this.LogInfo($"TEST: injected burst @ bar={bar} (events in bar = {list.Count})");
        }

        #endregion
    }
}
