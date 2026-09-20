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
    [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Description = nameof(SpeedOfTapeV3Resources.SpeedOfTapeV3_Description))]
    public class SpeedOfTapeV3 : Indicator
    {
        #region Nested types

        /// <summary>
        /// Metric the engine computes over the sliding time window.
        /// </summary>
        public enum SpeedType
        {
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SpeedType_Ticks))] Ticks,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SpeedType_Volume))] Volume,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SpeedType_Delta))] Delta,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SpeedType_Buys))] Buys,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SpeedType_Sells))] Sells
        }

        /// <summary>
        /// Single trade snapshot kept inside the rolling time window.
        /// Direction: +1 buy, -1 sell, 0 no aggressor side.
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

        /// <summary>
        /// Corner anchor for the floating info panel. The panel always
        /// renders inside the price chart region with a small margin.
        /// </summary>
        public enum InfoPanelLocation
        {
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.InfoPanelLocation_TopRight))] TopRight,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.InfoPanelLocation_TopLeft))] TopLeft,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.InfoPanelLocation_BottomRight))] BottomRight,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.InfoPanelLocation_BottomLeft))] BottomLeft
        }

        /// <summary>
        /// Controls what the price panel shows. The histogram, threshold
        /// line and floating info panel are independent of this setting —
        /// only the zone overlays (rectangles and extension lines) are
        /// affected.
        /// </summary>
        public enum ZoneDisplayMode
        {
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ZoneDisplayMode_All))] All,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ZoneDisplayMode_PrimaryOnly))] PrimaryOnly,
            [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ZoneDisplayMode_Hidden))] Hidden
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

        // Invisible companion series whose only purpose is to push the
        // panel's auto-scale ceiling above both the histogram bars AND
        // the threshold line. Critical decisions:
        //   * VisualType = Histogram (not Hide). ATAS skips Hide-mode
        //     series for panel scaling because IsVisible returns false
        //     for them. Histogram mode with a fully transparent colour
        //     keeps the series counted for scaling but invisible to the
        //     eye.
        //   * Per-bar value = max(speed, threshold) * 1.15. Padding only
        //     against speed leaves the threshold spike-prone — when the
        //     percentile briefly exceeds the bar's HWM (quiet bar in
        //     a recently-active session), the threshold line clips.
        //     Taking the max of both anchors is what guarantees both
        //     fit.
        private readonly ValueDataSeries _headroomSeries = new ValueDataSeries("Headroom")
        {
            VisualType = VisualMode.Histogram,
            Color = System.Drawing.Color.FromArgb(0, 0, 0, 0).Convert(),
            IsHidden = true,
            ShowZeroValue = false,
            ScaleIt = true
        };

        // ─── Engine state ───────────────────────────────────────────────────
        // Sliding window of trades observed in the last TimeWindow seconds.
        // Populated by ProcessTick, for the history and realtime alike.
        private readonly Queue<TickSnapshot> _tickQueue = new Queue<TickSnapshot>();

        // Running aggregates of _tickQueue, updated on every enqueue and dequeue so a trade costs
        // O(1) instead of a pass over the whole window. The price extremes use monotonic deques:
        // _maxPrices holds decreasing prices from front to back and _minPrices increasing ones,
        // so the front is always the extreme of the window.
        private decimal _windowVolume;
        private decimal _windowBuys;
        private decimal _windowSells;
        private long _nextTickSeq;
        private readonly LinkedList<(long Seq, decimal Price)> _maxPrices = new LinkedList<(long Seq, decimal Price)>();
        private readonly LinkedList<(long Seq, decimal Price)> _minPrices = new LinkedList<(long Seq, decimal Price)>();
        private readonly Queue<long> _tickSeqs = new Queue<long>();

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

        // Snapshot at the moment the current bar's HWM was set. Used to
        // colour the histogram bar with the same scheme the price panel
        // uses for events: the bar reflects its peak moment's direction
        // and efficiency, not just its magnitude.
        private SpeedSnapshot _currentBarHwmSnapshot;

        // Per-bar HWM snapshot. Lets the histogram bars be recoloured
        // retroactively when BuyColor / SellColor / NeutralColor change —
        // without this, only the live bar would update because Colors[bar]
        // stores resolved colors that need to be re-resolved against the
        // new palette.
        private readonly Dictionary<int, SpeedSnapshot> _hwmByBar
            = new Dictionary<int, SpeedSnapshot>();

        // How often a new speed observation is sampled into the context
        // buffer. 1 Hz keeps the buffer at ~900 entries for a 15-min window
        // and the percentile compute trivial — cheap and robust enough for
        // session-rhythm tracking.
        private static readonly TimeSpan SamplePeriod = TimeSpan.FromSeconds(1);

        // Minimum number of samples required in the speed buffer before the
        // percentile threshold is considered statistically meaningful and
        // event detection is allowed. Below this count the buffer is too
        // thin and any small uptick crosses the percentile by definition.
        // At 1 Hz sampling this is ~30 seconds of warmup.
        private const int MinSamplesForDetection = 30;

        // Efficiency below which a burst is rendered with the neutral
        // colour regardless of side. Bursts under this threshold are
        // considered visually balanced — too much counterparty volume
        // for a clear directional read. Chosen empirically; works well
        // for the typical efficiency distribution of futures bursts.
        private const decimal MinEfficiencyForDirectionalColor = 0.3m;

        // How much to inflate the indicator panel's auto-scale ceiling
        // above the maximum visible value (histogram peak or threshold,
        // whichever is higher). Used by the transparent headroom series
        // to prevent both elements from touching the panel's top edge.
        private const decimal PanelHeadroomFactor = 1.15m;

        // Rolling buffer of speed observations over the last
        // ContextWindowMinutes minutes. Populated by MaybeSampleSpeed,
        // trimmed by TrimSpeedBuffer, read by ComputePercentile.
        private readonly Queue<SpeedObservation> _speedBuffer = new Queue<SpeedObservation>();

        // The speeds of _speedBuffer kept sorted, so the percentile is a lookup instead of a
        // sort of the whole buffer on every sample.
        private readonly List<decimal> _sortedSpeeds = new List<decimal>();

        // Trade-time of the last observation added; used to rate-limit
        // MaybeSampleSpeed to one entry per SamplePeriod.
        private DateTime _lastSampleTime = DateTime.MinValue;

        // Cached percentile value, recomputed every time a new observation
        // enters the buffer. Read by UpdateHistogram (writes the threshold
        // series) and by event detection.
        private decimal _currentThreshold;

        // Last observed speed. Used by DetectEvents for last-vs-current
        // upward-cross detection (event-based, not state-based — avoids
        // emitting duplicate events while speed lingers above threshold).
        private decimal _lastSpeed;

        // Multi-burst cache: each bar maps to the list of events detected
        // during it. Persistent (never pruned). The render layer iterates this
        // dictionary to draw rectangles and extension lines.
        private readonly Dictionary<int, List<EventRecord>> _eventsByBar
            = new Dictionary<int, List<EventRecord>>();

        // Color settings backing fields. The internal type is
        // System.Drawing.Color because RenderContext consumes that directly
        // during DrawZone; the panel-facing properties expose CrossColor and
        // bridge with .Convert() at the boundary.
        private System.Drawing.Color _buyColor = System.Drawing.Color.Lime;
        private System.Drawing.Color _sellColor = System.Drawing.Color.Red;
        private System.Drawing.Color _neutralColor = System.Drawing.Color.Gray;

        // Controls whether and how zone overlays are rendered in the
        // price panel. Default 'All' — every detected event shows.
        private ZoneDisplayMode _zoneDisplay = ZoneDisplayMode.All;

        // Info panel settings backing fields.
        private bool _showInfoPanel = true;
        private InfoPanelLocation _infoPanelPosition = InfoPanelLocation.TopRight;
        private int _maxEventsInPanel = 5;

        // Font for panel rows. Monospace for clean column alignment.
        private readonly RenderFont _panelFont = new RenderFont("Consolas", 9);

        // Extension line settings backing fields. ExtensionBars: number of
        // bars right of the primary event drawn at full opacity. FadeBars:
        // additional bars drawn with linearly decreasing alpha, ending
        // nearly transparent.
        private int _extensionBars = 3;
        private int _fadeBars = 3;

        // Recent events shown in the floating info panel. Capped at
        // _maxEventsInPanel — older entries are dequeued from the front
        // when capacity is exceeded.
        private readonly Queue<EventRecord> _recentEvents = new Queue<EventRecord>();

        // Single lock guarding ALL engine state — _tickQueue, _speedBuffer,
        // _eventsByBar, _recentEvents, _alertedBars and the per-frame
        // scalar fields like _currentSnapshot and _lastSpeed. Coarse but
        // simple: the data thread holds it for the duration of OnNewTrade
        // and OnCumulativeTradesResponse, the UI thread holds it briefly
        // at the top of OnRender to snapshot what it needs, and property
        // setters that mutate state hold it while doing so. Reentrant, so
        // nested calls within already-locked methods are fine.
        private readonly object _engineLock = new object();

        // True while OnCumulativeTradesResponse is iterating historical
        // trades. Used to suppress per-event log spam during replay; the
        // flag is cleared when replay finishes so live events log normally.
        private bool _isReplaying;

        // History request and realtime join. Until the history response has been processed,
        // realtime ticks are kept in _pendingTicks; afterwards they are replayed without the
        // ones the history already contains (same timestamp, counted from the end of the
        // history), so a recalculation gives the same result as the live run.
        private int _requestId;
        private bool _historyLoaded;
        private readonly List<MarketDataArg> _pendingTicks = new List<MarketDataArg>();
        private DateTime _historyEndTime;
        private int _boundaryTickCount;

        // Timestamp of the last processed tick and how many ticks carried it; copied when a
        // recalculation starts buffering, to know which boundary ticks the buffer lacks.
        private DateTime _lastTickTime;
        private int _lastTickCount;
        private DateTime _bufferStartTickTime;
        private int _bufferStartTickCount;

        // Last bar a tick was attributed to; the bar search starts from it.
        private int _barCursor;

        // Alert state. _alertedBars tracks which bars have already fired
        // an alert this session so a second burst in the same bar doesn't
        // fire again. _lastAlertTime gates the cooldown across bars.
        private readonly HashSet<int> _alertedBars = new HashSet<int>();
        private DateTime _lastAlertTime = DateTime.MinValue;

        // ─── Settings backing fields ────────────────────────────────────────
        // Kept private + exposed via Properties so the setters can trigger
        // RecalculateValues on parameter changes.
        private int _timeWindow = 5;
        private SpeedType _dataType = SpeedType.Ticks;
        private int _contextWindowMinutes = 15;
        private int _thresholdPercentile = 97;
        private int _sessionsToCalculate = 5;
        private decimal _manualThreshold;

        // Alerts settings backing fields.
        private bool _useAlerts;
        private string _alertFile = "alert1";
        private int _alertCooldownSeconds = 60;

        #endregion

        #region Properties

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.TimeWindow_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Calculation), Order = 10,
                 Description = nameof(SpeedOfTapeV3Resources.TimeWindow_Description))]
        [Range(1, 600)]
        public int TimeWindow
        {
            get => _timeWindow;
            set { _timeWindow = value; RecalculateValues(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.DataType_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Calculation), Order = 20,
                 Description = nameof(SpeedOfTapeV3Resources.DataType_Description))]
        public SpeedType DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;
                lock (_engineLock)
                {
                    ResetEngineState();
                }
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SessionsToCalculate_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Calculation), Order = 30,
                 Description = nameof(SpeedOfTapeV3Resources.SessionsToCalculate_Description))]
        [Range(0, 1000)]
        public int SessionsToCalculate
        {
            get => _sessionsToCalculate;
            set { _sessionsToCalculate = Math.Max(0, value); RecalculateValues(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ContextWindowMinutes_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Threshold), Order = 10,
                 Description = nameof(SpeedOfTapeV3Resources.ContextWindowMinutes_Description))]
        [Range(1, 120)]
        public int ContextWindowMinutes
        {
            get => _contextWindowMinutes;
            set { _contextWindowMinutes = value; RecalculateValues(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ThresholdPercentile_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Threshold), Order = 20,
                 Description = nameof(SpeedOfTapeV3Resources.ThresholdPercentile_Description))]
        [Range(50, 99)]
        public int ThresholdPercentile
        {
            get => _thresholdPercentile;
            set { _thresholdPercentile = value; RecalculateValues(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ManualThreshold_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Threshold), Order = 30,
                 Description = nameof(SpeedOfTapeV3Resources.ManualThreshold_Description))]
        [Range(0, 100000000)]
        public decimal ManualThreshold
        {
            get => _manualThreshold;
            set { _manualThreshold = Math.Max(0m, value); RecalculateValues(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ZoneDisplay_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Visuals), Order = 5,
                 Description = nameof(SpeedOfTapeV3Resources.ZoneDisplay_Description))]
        public ZoneDisplayMode ZoneDisplay
        {
            get => _zoneDisplay;
            set { _zoneDisplay = value; RedrawChart(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.BuyColor_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Visuals), Order = 10,
                 Description = nameof(SpeedOfTapeV3Resources.BuyColor_Description))]
        public CrossColor BuyColor
        {
            get => _buyColor.Convert();
            set 
            {   
                _buyColor = value.Convert();
                RecolorHistogram(); 
                RedrawChart(); 
            }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.SellColor_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Visuals), Order = 20,
                 Description = nameof(SpeedOfTapeV3Resources.SellColor_Description))]
        public CrossColor SellColor
        {
            get => _sellColor.Convert();
            set
            {
                _sellColor = value.Convert();
                RecolorHistogram();
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.NeutralColor_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Visuals), Order = 30,
                 Description = nameof(SpeedOfTapeV3Resources.NeutralColor_Description))]
        public CrossColor NeutralColor
        {
            get => _neutralColor.Convert();
            set
            {
                _neutralColor = value.Convert();
                RecolorHistogram();
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ExtensionBars_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Visuals), Order = 40,
                 Description = nameof(SpeedOfTapeV3Resources.ExtensionBars_Description))]
        [Range(0, 200)]
        public int ExtensionBars
        {
            get => _extensionBars;
            set { _extensionBars = value; RedrawChart(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.FadeBars_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Visuals), Order = 50,
                 Description = nameof(SpeedOfTapeV3Resources.FadeBars_Description))]
        [Range(0, 100)]
        public int FadeBars
        {
            get => _fadeBars;
            set { _fadeBars = value; RedrawChart(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.ShowInfoPanel_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_InfoPanel), Order = 10,
                 Description = nameof(SpeedOfTapeV3Resources.ShowInfoPanel_Description))]
        public bool ShowInfoPanel
        {
            get => _showInfoPanel;
            set { _showInfoPanel = value; RedrawChart(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.InfoPanelPosition_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_InfoPanel), Order = 20,
                 Description = nameof(SpeedOfTapeV3Resources.InfoPanelPosition_Description))]
        public InfoPanelLocation InfoPanelPosition
        {
            get => _infoPanelPosition;
            set { _infoPanelPosition = value; RedrawChart(); }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.MaxEventsInPanel_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_InfoPanel), Order = 30,
                 Description = nameof(SpeedOfTapeV3Resources.MaxEventsInPanel_Description))]
        [Range(1, 20)]
        public int MaxEventsInPanel
        {
            get => _maxEventsInPanel;
            set
            {
                _maxEventsInPanel = value;

                lock (_engineLock)
                {
                    while (_recentEvents.Count > value)
                        _recentEvents.Dequeue();
                }

                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.UseAlerts_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Alerts), Order = 10,
                 Description = nameof(SpeedOfTapeV3Resources.UseAlerts_Description))]
        public bool UseAlerts
        {
            get => _useAlerts;
            set => _useAlerts = value;
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.AlertFile_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Alerts), Order = 20,
                 Description = nameof(SpeedOfTapeV3Resources.AlertFile_Description))]
        public string AlertFile
        {
            get => _alertFile;
            set => _alertFile = value;
        }

        [Display(ResourceType = typeof(SpeedOfTapeV3Resources), Name = nameof(SpeedOfTapeV3Resources.AlertCooldownSeconds_DisplayName),
                 GroupName = nameof(SpeedOfTapeV3Resources.Group_Alerts), Order = 30,
                 Description = nameof(SpeedOfTapeV3Resources.AlertCooldownSeconds_Description))]
        [Range(0, 3600)]
        public int AlertCooldownSeconds
        {
            get => _alertCooldownSeconds;
            set => _alertCooldownSeconds = value;
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
            DataSeries.Add(_headroomSeries);

        }

        protected override void OnInitialize()
        {
            this.LogInfo($"SpeedOfTapeV3: initialized ({typeof(SpeedOfTapeV3).Assembly.GetName().Version}).");
        }

        #endregion

        #region Overrides: Lifecycle

        protected override void OnCalculate(int bar, decimal value)
        {
            // Engine lives in OnNewTrade.
            // OnCalculate must be overridden to satisfy BaseIndicator's
            // abstract contract; intentionally left empty.
        }

        /// <summary>
        /// Starts a recalculation: the engine is cleared and realtime ticks
        /// are buffered until the history response has been processed.
        /// </summary>
        protected override void OnRecalculate()
        {
            lock (_engineLock)
            {
                ResetEngineState();
                _historyLoaded = false;
                _requestId = 0;
                _pendingTicks.Clear();
                _bufferStartTickTime = _lastTickTime;
                _bufferStartTickCount = _lastTickCount;
            }
        }

        /// <summary>
        /// Fires once after every chart recalculation (initial load and
        /// every parameter change that triggers RecalculateValues).
        /// We use it to issue a CumulativeTradesRequest covering the
        /// chart's full historical time range; the response arrives in
        /// OnCumulativeTradesResponse where the engine replays them.
        /// </summary>
        protected override void OnFinishRecalculate()
        {
            if (CurrentBar < 1)
            {
                lock (_engineLock)
                    _historyLoaded = true;

                return;
            }

            var firstBar = FirstCalculatedBar();
            var startTime = GetCandle(firstBar).Time;
            var endTime = GetCandle(CurrentBar - 1).LastTime;
            var request = new CumulativeTradesRequest(startTime, endTime, 0, 0);

            this.LogInfo($"SpeedOfTapeV3: history request {startTime:yyyy-MM-dd HH:mm:ss}-{endTime:yyyy-MM-dd HH:mm:ss} (bars {firstBar}-{CurrentBar - 1}, sessions {(_sessionsToCalculate > 0 ? _sessionsToCalculate.ToString() : "all")})");

            lock (_engineLock)
                _requestId = request.RequestId;

            RequestForCumulativeTrades(request);
        }

        /// <summary>
        /// Called by ATAS when the indicator is removed from the chart, the
        /// chart is closed, or ATAS shuts down. Acquires _engineLock to
        /// ensure no trade is being processed concurrently, then clears
        /// all engine state to release the accumulated memory (events dict
        /// can grow to thousands of entries on long sessions).
        /// </summary>
        protected override void OnDispose()
        {
            lock (_engineLock)
            {
                ResetEngineState();
            }
            base.OnDispose();
        }

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
            lock (_engineLock)
            {
                if (!_historyLoaded)
                {
                    _pendingTicks.Add(trade);
                    return;
                }

                ProcessTick(trade);
            }
        }

        /// <summary>
        /// Receives the historical trades requested in OnFinishRecalculate.
        /// Resets engine state and replays the trades chronologically
        /// through ProcessTradeAt so historical bars get their events,
        /// histogram values and threshold buffer populated by the same
        /// pipeline as live trades. Each trade's bar is found by linear
        /// search resuming from the last hit — chronological ordering
        /// guarantees the search start advances monotonically.
        /// </summary>
        protected override void OnCumulativeTradesResponse(
                    CumulativeTradesRequest request,
                    IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            if (cumulativeTrades == null) return;

            lock (_engineLock)
            {
                if (request.RequestId != _requestId)
                    return;
            }

            // The history is replayed from the ticks of the cumulative trades, the same data the
            // realtime path receives. A cumulative trade groups several ticks into one trade with
            // one price, so replaying it as one trade gave the history fewer ticks and narrower
            // ranges than the live run.
            var ticks = cumulativeTrades
                .Where(t => t.Ticks != null)
                .SelectMany(t => t.Ticks)
                .OrderBy(t => t.Time)
                .ToList();

            lock (_engineLock)
            {
                ResetEngineState();

                _historyEndTime = ticks.Count > 0 ? ticks[ticks.Count - 1].Time : DateTime.MinValue;
                _boundaryTickCount = 0;

                for (var i = ticks.Count - 1; i >= 0 && ticks[i].Time == _historyEndTime; i--)
                    _boundaryTickCount++;

                _isReplaying = true;
                try
                {
                    foreach (var tick in ticks)
                        ProcessTick(tick);

                    int eventCount = _eventsByBar.Sum(kv => kv.Value.Count);
                    this.LogInfo($"SpeedOfTapeV3: history replay complete — ticks={ticks.Count} events={eventCount}");
                }
                finally
                {
                    _isReplaying = false;
                }
            }

            ReplayPendingTicks();
            RedrawChart();
        }

        /// <summary>
        /// Replays the realtime ticks buffered while the history was pending, skipping the ones
        /// the history already contains, then switches to realtime under the lock once the
        /// buffer is empty.
        /// </summary>
        private void ReplayPendingTicks()
        {
            int boundaryToSkip;
            var replayed = 0;
            var skipped = 0;

            lock (_engineLock)
            {
                boundaryToSkip = _boundaryTickCount -
                    (_bufferStartTickTime == _historyEndTime ? _bufferStartTickCount : 0);
            }

            while (true)
            {
                List<MarketDataArg> batch;

                lock (_engineLock)
                {
                    if (_pendingTicks.Count == 0)
                    {
                        _historyLoaded = true;
                        break;
                    }

                    batch = new List<MarketDataArg>(_pendingTicks);
                    _pendingTicks.Clear();
                }

                foreach (var tick in batch)
                {
                    lock (_engineLock)
                    {
                        if (tick.Time < _historyEndTime || (tick.Time == _historyEndTime && boundaryToSkip-- > 0))
                        {
                            skipped++;
                            continue;
                        }

                        ProcessTick(tick);
                        replayed++;
                    }
                }
            }

            this.LogInfo($"SpeedOfTapeV3: realtime started; buffered ticks: {replayed} replayed, {skipped} already in the history");
        }

        #endregion

        #region Overrides: Rendering

        /// <summary>
        /// Render entry point. Called by ATAS once per render frame.
        /// Snapshots the events dictionary and recent-events queue under
        /// _engineLock, then runs three render passes outside the lock:
        ///   Pass 1 — zone rectangles on the price panel (gated by
        ///            ZoneDisplay; PrimaryOnly suppresses secondary events).
        ///   Pass 2 — primary event extension lines on the price panel
        ///            (also gated by ZoneDisplay).
        ///   Pass 3 — floating info panel in the configured corner
        ///            (gated independently by ShowInfoPanel — the panel
        ///            persists even when zone overlays are hidden).
        /// </summary>
        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (layout != DrawingLayouts.Final || ChartInfo == null || InstrumentInfo == null)
                return;

            // Snapshot mutable engine state under the lock so the rest of
            // the render pass operates on a stable copy. The data thread
            // can resume processing trades immediately after the snapshot;
            // the snapshots are short-lived per-frame allocations.
            Dictionary<int, List<EventRecord>> eventsSnapshot;
            List<EventRecord> recentSnapshot;
            lock (_engineLock)
            {
                eventsSnapshot = new Dictionary<int, List<EventRecord>>(_eventsByBar.Count);
                foreach (var kv in _eventsByBar)
                    eventsSnapshot[kv.Key] = new List<EventRecord>(kv.Value);

                recentSnapshot = _recentEvents.ToList();
            }

            context.SetClip(ChartInfo.PriceChartContainer.Region);

            var buyColor = _buyColor;
            var sellColor = _sellColor;
            var neutralColor = _neutralColor;

            // Passes 1 and 2 (zone rectangles and extension lines) only run
            // when the user wants the price panel overlay visible. Histogram,
            // threshold line and floating panel are independent.
            if (_zoneDisplay != ZoneDisplayMode.Hidden)
            {
                // Pass 1: zone rectangles. In PrimaryOnly mode, secondary
                // events are skipped so each bar shows at most one rectangle.
                for (int bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
                {
                    if (!eventsSnapshot.TryGetValue(bar, out var events) || events.Count == 0)
                        continue;
                    if (!TryFindPrimaryEvent(events, out var primary)) continue;

                    foreach (var evt in events)
                    {
                        bool isPrimary =
                            evt.Time == primary.Time && evt.Snapshot.Speed == primary.Snapshot.Speed;
                        if (_zoneDisplay == ZoneDisplayMode.PrimaryOnly && !isPrimary)
                            continue;

                        var color = ResolveZoneColor(evt.Snapshot, buyColor, sellColor, neutralColor);
                        int penWidth = isPrimary ? 5 : 2;
                        DrawZone(context, bar, evt.Snapshot.High, evt.Snapshot.Low, color, penWidth);
                    }
                }

                // Pass 2: primary event extension lines. Same in All and
                // PrimaryOnly modes — only the primary draws its line.
                for (int bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
                {
                    if (!eventsSnapshot.TryGetValue(bar, out var events) || events.Count == 0)
                        continue;
                    if (!TryFindPrimaryEvent(events, out var primary)) continue;
                    var color = ResolveZoneColor(primary.Snapshot, buyColor, sellColor, neutralColor);
                    DrawExtensionLine(context, bar, primary.Snapshot, color);
                }
            }

            // Pass 3 (info panel) runs regardless of ZoneDisplay — the panel
            // is the persistent log of events even if the user has chosen to
            // keep the price chart clean.
            if (_showInfoPanel)
            {
                DrawInfoPanel(context, recentSnapshot, buyColor, sellColor, neutralColor);
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
            {
                var old = _tickQueue.Dequeue();
                var seq = _tickSeqs.Dequeue();

                _windowVolume -= old.Volume;

                if (old.Direction == 1) _windowBuys -= old.Volume;
                else if (old.Direction == -1) _windowSells -= old.Volume;

                if (_maxPrices.Count > 0 && _maxPrices.First.Value.Seq == seq) _maxPrices.RemoveFirst();
                if (_minPrices.Count > 0 && _minPrices.First.Value.Seq == seq) _minPrices.RemoveFirst();
            }
        }

        /// <summary>
        /// Metrics of the rolling tick queue, read from its running aggregates:
        /// buys and sells in parallel, plus the high/low price range observed
        /// during the window. The caller decides what to do with each field.
        /// </summary>
        private SpeedSnapshot ComputeInstantSnapshot()
        {
            if (_tickQueue.Count == 0)
                return new SpeedSnapshot(0, 0m, 0m, 0m, 0m, 0m, _dataType);

            return new SpeedSnapshot(_tickQueue.Count, _windowVolume, _windowBuys, _windowSells,
                _maxPrices.First.Value.Price, _minPrices.First.Value.Price, _dataType);
        }

        /// <summary>
        /// Adds a trade to the rolling window and to its running aggregates.
        /// </summary>
        private void EnqueueTick(TickSnapshot tick)
        {
            var seq = _nextTickSeq++;

            _tickQueue.Enqueue(tick);
            _tickSeqs.Enqueue(seq);
            _windowVolume += tick.Volume;

            if (tick.Direction == 1) _windowBuys += tick.Volume;
            else if (tick.Direction == -1) _windowSells += tick.Volume;

            while (_maxPrices.Count > 0 && _maxPrices.Last.Value.Price <= tick.Price) _maxPrices.RemoveLast();
            _maxPrices.AddLast((seq, tick.Price));

            while (_minPrices.Count > 0 && _minPrices.Last.Value.Price >= tick.Price) _minPrices.RemoveLast();
            _minPrices.AddLast((seq, tick.Price));
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
            InsertSorted(_currentSnapshot.Speed);
            TrimSpeedBuffer(now);
            _currentThreshold = _manualThreshold > 0m
                ? _manualThreshold
                : ComputePercentile(_thresholdPercentile);
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
                RemoveSorted(_speedBuffer.Dequeue().Speed);
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

            int idx = (int)(_sortedSpeeds.Count * percentile / 100m);
            if (idx >= _sortedSpeeds.Count) idx = _sortedSpeeds.Count - 1;
            return _sortedSpeeds[idx];
        }

        private void InsertSorted(decimal speed)
        {
            var idx = _sortedSpeeds.BinarySearch(speed);
            _sortedSpeeds.Insert(idx < 0 ? ~idx : idx, speed);
        }

        private void RemoveSorted(decimal speed)
        {
            var idx = _sortedSpeeds.BinarySearch(speed);

            if (idx >= 0)
                _sortedSpeeds.RemoveAt(idx);
        }

        /// <summary>
        /// Event-based upward-cross detector. Same logic as before but the
        /// bar index is supplied explicitly so historical replay can
        /// attribute events to their actual bars instead of CurrentBar - 1.
        /// The per-event LogInfo is suppressed during replay (otherwise a
        /// session with thousands of historical events would spam the log
        /// for several seconds at indicator load).
        /// </summary>
        private void DetectEvents(DateTime time, int bar)
        {
            if (bar < 0) return;

            decimal current = _currentSnapshot.Speed;
            decimal threshold = _currentThreshold;

            // The warm-up only applies to the percentile: a manual threshold is valid from the
            // first sample.
            if (threshold <= 0m || (_manualThreshold <= 0m && _speedBuffer.Count < MinSamplesForDetection))
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

                // Caller (OnNewTrade or OnCumulativeTradesResponse) holds _engineLock,
                // so direct mutation is safe.
                _recentEvents.Enqueue(evt);
                while (_recentEvents.Count > _maxEventsInPanel)
                    _recentEvents.Dequeue();

                MaybeFireAlert(time, bar, _currentSnapshot);

                if (!_isReplaying)
                {
                    var side = _currentSnapshot.IsBuyDominant ? "buy" : "sell";
                    this.LogInfo($"SpeedOfTapeV3: burst @ bar={bar} speed={current.ToString("0.00")} threshold={threshold.ToString("0.00")} side={side} eff={_currentSnapshot.Efficiency.ToString("0.00")} range={_currentSnapshot.Low}-{_currentSnapshot.High}");
                }
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
        private void UpdateHistogram(int bar)
        {
            if (bar < 0) return;

            if (bar != _lastBar)
            {
                if (_lastBar >= 0)
                    _renderSeries[_lastBar] = _currentBarHwm;

                _lastBar = bar;
                _currentBarHwm = _currentSnapshot.Speed;
                _currentBarHwmSnapshot = _currentSnapshot;
            }
            else if (_currentSnapshot.Speed > _currentBarHwm)
            {
                _currentBarHwm = _currentSnapshot.Speed;
                _currentBarHwmSnapshot = _currentSnapshot;
            }

            _hwmByBar[bar] = _currentBarHwmSnapshot;

            _renderSeries[bar] = _currentSnapshot.Speed;
            _renderSeries.Colors[bar] = ResolveZoneColor(
                _currentBarHwmSnapshot,
                _buyColor,
                _sellColor,
                _neutralColor);
            _thresholdSeries[bar] = _currentThreshold;
            decimal panelMax = Math.Max(_currentSnapshot.Speed, _currentThreshold);
            _headroomSeries[bar] = panelMax * PanelHeadroomFactor;
        }

        /// <summary>
        /// Unified per-trade entry point used by both live processing
        /// (OnNewTrade) and historical replay (OnCumulativeTradesResponse).
        /// The engine pipeline is identical in both cases — only the bar
        /// attribution differs (live: CurrentBar - 1; historical: the bar
        /// computed from the trade's timestamp).
        /// </summary>
        /// <summary>
        /// Attributes a tick to the bar that contains its time and runs it through the engine.
        /// Used for the history, the buffered ticks and realtime alike. Caller holds _engineLock.
        /// </summary>
        private void ProcessTick(MarketDataArg tick)
        {
            if (tick.Time == _lastTickTime)
                _lastTickCount++;
            else
            {
                _lastTickTime = tick.Time;
                _lastTickCount = 1;
            }

            var bar = BarOfTime(tick.Time);

            if (bar < 0)
                return;

            // Trades without an aggressor side (Between) count as ticks and volume but on neither
            // side; they used to be counted as sells.
            int direction = tick.Direction switch
            {
                TradeDirection.Buy => 1,
                TradeDirection.Sell => -1,
                _ => 0
            };
            ProcessTradeAt(tick.Time, tick.Volume, direction, tick.Price, bar);
        }

        /// <summary>
        /// First bar of the last SessionsToCalculate sessions, or 0 for the whole chart.
        /// </summary>
        private int FirstCalculatedBar()
        {
            if (_sessionsToCalculate <= 0)
                return 0;

            var sessions = 0;

            for (var bar = CurrentBar - 1; bar > 0; bar--)
            {
                if (IsNewSession(bar) && ++sessions == _sessionsToCalculate)
                    return bar;
            }

            return 0;
        }

        /// <summary>
        /// Bar that contains a time: the last bar opened at or before it, or -1 before the first
        /// bar. Ticks arrive in time order, so the search starts from the last bar found.
        /// </summary>
        private int BarOfTime(DateTime time)
        {
            if (CurrentBar == 0)
                return -1;

            var bar = Math.Min(Math.Max(0, _barCursor), CurrentBar - 1);

            while (bar > 0 && GetCandle(bar).Time > time)
                bar--;

            if (GetCandle(bar).Time > time)
                return -1;

            while (bar + 1 < CurrentBar && GetCandle(bar + 1).Time <= time)
                bar++;

            _barCursor = bar;
            return bar;
        }

        private void ProcessTradeAt(DateTime time, decimal volume, int direction, decimal price, int bar)
        {
            var snap = new TickSnapshot(time, volume, direction, price);
            EnqueueTick(snap);

            TrimToTimeWindow(time);
            _currentSnapshot = ComputeInstantSnapshot();
            MaybeSampleSpeed(time);
            DetectEvents(time, bar);
            UpdateHistogram(bar);
        }

        /// <summary>
        /// Clears all engine and panel state. Caller must hold _engineLock —
        /// every call site already does (DataType setter, OnDispose,
        /// OnCumulativeTradesResponse).
        /// </summary>
        private void ResetEngineState()
        {
            _tickQueue.Clear();
            _tickSeqs.Clear();
            _windowVolume = 0m;
            _windowBuys = 0m;
            _windowSells = 0m;
            _maxPrices.Clear();
            _minPrices.Clear();
            _currentSnapshot = default;
            _lastBar = -1;
            _currentBarHwm = 0m;
            _currentBarHwmSnapshot = default;
            _hwmByBar.Clear();
            _speedBuffer.Clear();
            _sortedSpeeds.Clear();
            _lastSampleTime = DateTime.MinValue;
            _currentThreshold = 0m;
            _lastSpeed = 0m;
            _eventsByBar.Clear();
            _alertedBars.Clear();
            _lastAlertTime = DateTime.MinValue;
            _recentEvents.Clear();
            _barCursor = 0;
        }

        #endregion

        #region Private methods: Alerts

        /// <summary>
        /// Decides whether to fire a popup + audio alert for a freshly
        /// detected event. Three gates in order:
        ///   1. Replay flag — never fire during historical replay.
        ///   2. Master toggle — UseAlerts must be on.
        ///   3. Per-bar dedup — first burst of each bar wins; subsequent
        ///      bursts in the same bar are silent.
        ///   4. Cooldown — at least AlertCooldownSeconds between alerts
        ///      across bars.
        ///
        /// The alert popup uses the burst's resolved zone color as the
        /// background so the visual identity matches the chart marker.
        /// Foreground is white for guaranteed contrast.
        /// </summary>
        private void MaybeFireAlert(DateTime time, int bar, SpeedSnapshot snap)
        {
            if (_isReplaying) return;
            if (!_useAlerts) return;
            if (_alertedBars.Contains(bar)) return;

            if (_alertCooldownSeconds > 0)
            {
                var cooldown = TimeSpan.FromSeconds(_alertCooldownSeconds);
                if (time - _lastAlertTime < cooldown) return;
            }

            _alertedBars.Add(bar);
            _lastAlertTime = time;

            string side = snap.IsBuyDominant ? "Buy" : "Sell";
            string message = $"{side} burst — speed={snap.Speed:0} Δ{snap.Delta:+0;-0;0} eff={(snap.Efficiency * 100m):0}%";

            var bg = (snap.IsBuyDominant ? _buyColor : _sellColor).Convert();
            var fg = CrossColor.FromArgb(255, 255, 255, 255);

            AddAlert(_alertFile, InstrumentInfo.Instrument, message, bg, fg);
            this.LogInfo($"SpeedOfTapeV3: alert fired @ bar={bar}: {message}");
        }

        #endregion

        #region Private methods: Rendering

        /// <summary>
        /// Decides which color to paint a zone with, based on the snapshot's
        /// dominant side and efficiency. Bursts under MinEfficiencyForDirectionalColor
        /// are considered balanced and painted neutral regardless of side. Above
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
            if (snap.Efficiency < MinEfficiencyForDirectionalColor) return neutral;

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
        /// Recomputes the colour of every histogram bar from its stored
        /// HWM snapshot. Called from the BuyColor / SellColor /
        /// NeutralColor setters so palette changes update closed bars
        /// immediately, matching the live behaviour of the price-panel
        /// rectangles whose colours are resolved fresh on every render.
        /// </summary>
        private void RecolorHistogram()
        {
            lock (_engineLock)
            {
                foreach (var kv in _hwmByBar)
                {
                    _renderSeries.Colors[kv.Key] = ResolveZoneColor(
                        kv.Value,
                        _buyColor,
                        _sellColor,
                        _neutralColor);
                }
            }
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
        /// Finds the highest-speed event in a list. Used by OnRender after
        /// snapshotting _eventsByBar — operates on the supplied list rather
        /// than reading the field directly so the caller controls
        /// concurrency.
        /// </summary>
        private static bool TryFindPrimaryEvent(List<EventRecord> events, out EventRecord primary)
        {
            primary = default;
            if (events == null || events.Count == 0) return false;

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

        /// <summary>
        /// Converts a UTC time to the chart time zone. Stable and Latest only offer
        /// whole hours; the others take the exact offset (half-hour zones included).
        /// </summary>
        private DateTime ChartTime(DateTime utc)
        {
            if (InstrumentInfo is null)
                return utc;

#if ATAS_STABLE || ATAS_LATEST
            return utc.AddHours(InstrumentInfo.TimeZone);
#else
            return utc.Add(InstrumentInfo.TimeZoneOffset);
#endif
        }

        /// <summary>
        /// Renders the floating info panel anchored in the configured
        /// corner of the price chart. The panel is sized to fit a title
        /// row plus one row per recent event; rows are coloured by the
        /// event's resolved zone colour (side + efficiency) so the same
        /// visual semantics apply across panel and chart.
        ///
        /// Iteration is over a snapshot of _recentEvents taken under
        /// _engineLock to avoid racing with DetectEvents on the
        /// data thread.
        /// </summary>
        private void DrawInfoPanel(RenderContext context, List<EventRecord> events, System.Drawing.Color buy, System.Drawing.Color sell, System.Drawing.Color neutral)
        {
            if (ChartInfo == null) return;
            if (events.Count == 0) return;

            // Newest first.
            events.Reverse();

            string title = $"SPEED OF TAPE — {events.Count} burst{(events.Count == 1 ? "" : "s")}";
            const string columnHeader = "TIME      D  SPD    DELTA   EFF";

            // Trade timestamps from the feed are in UTC; the rows use the chart time zone, so
            // they line up with the candles below.

            // Suffix shown next to the speed value depends on the active
            // DataType — "t" for tick counts, "c" for contract volume
            // (Volume, Delta, Buys and Sells all derive from contract counts).
            // Delta is shown as a signed number in its own column with no
            // suffix because the dedicated DELTA column already conveys it.
            string speedSuffix = _dataType == SpeedType.Ticks ? "t" : "c";

            var rows = new List<(string text, System.Drawing.Color color)>(events.Count);
            foreach (var evt in events)
            {
                var snap = evt.Snapshot;
                string side = snap.IsBuyDominant ? "▲" : "▼";
                DateTime chartTime = ChartTime(evt.Time);

                string row = string.Format(
                    "{0:HH:mm:ss}  {1}  {2,4:0}{5}  {3,5:+0;-0;0}  {4,3:0}%",
                    chartTime,
                    side,
                    snap.Speed,
                    snap.Delta,
                    snap.Efficiency * 100m,
                    speedSuffix);
                var rowColor = ResolveZoneColor(snap, buy, sell, neutral);
                rows.Add((row, rowColor));
            }

            // Measure dimensions.
            var titleSize = context.MeasureString(title, _panelFont);
            int rowHeight = (int)titleSize.Height;
            int maxWidth = (int)titleSize.Width;

            var headerSize = context.MeasureString(columnHeader, _panelFont);
            if (headerSize.Width > maxWidth) maxWidth = (int)headerSize.Width;
            if (headerSize.Height > rowHeight) rowHeight = (int)headerSize.Height;

            foreach (var (text, _) in rows)
            {
                var sz = context.MeasureString(text, _panelFont);
                if (sz.Width > maxWidth) maxWidth = (int)sz.Width;
                if (sz.Height > rowHeight) rowHeight = (int)sz.Height;
            }

            const int padX = 8;
            const int padY = 6;
            const int titleSpacing = 4;
            const int margin = 10;

            int panelWidth = maxWidth + 2 * padX;
            int panelHeight = padY * 2 + rowHeight + titleSpacing + rowHeight + (rowHeight * rows.Count);

            var region = ChartInfo.PriceChartContainer.Region;
            int panelX, panelY;
            switch (_infoPanelPosition)
            {
                case InfoPanelLocation.TopLeft:
                    panelX = region.Left + margin;
                    panelY = region.Top + margin;
                    break;
                case InfoPanelLocation.BottomLeft:
                    panelX = region.Left + margin;
                    panelY = region.Bottom - margin - panelHeight;
                    break;
                case InfoPanelLocation.BottomRight:
                    panelX = region.Right - margin - panelWidth;
                    panelY = region.Bottom - margin - panelHeight;
                    break;
                case InfoPanelLocation.TopRight:
                default:
                    panelX = region.Right - margin - panelWidth;
                    panelY = region.Top + margin;
                    break;
            }

            // Background: dark, semi-transparent.
            var bg = System.Drawing.Color.FromArgb(180, 15, 18, 24);
            context.FillRectangle(bg, new Rectangle(panelX, panelY, panelWidth, panelHeight));

            // Title row in muted gray.
            var titleColor = System.Drawing.Color.FromArgb(220, 200, 200, 200);
            context.DrawString(title, _panelFont, titleColor, panelX + padX, panelY + padY);

            // Column header row — slightly dimmer than rows, slightly brighter than title.
            var headerColor = System.Drawing.Color.FromArgb(180, 160, 160, 170);
            int headerY = panelY + padY + rowHeight + titleSpacing;
            context.DrawString(columnHeader, _panelFont, headerColor, panelX + padX, headerY);

            // Event rows.
            int rowY = headerY + rowHeight;
            foreach (var (text, color) in rows)
            {
                context.DrawString(text, _panelFont, color, panelX + padX, rowY);
                rowY += rowHeight;
            }
        }

        #endregion

    }
}
