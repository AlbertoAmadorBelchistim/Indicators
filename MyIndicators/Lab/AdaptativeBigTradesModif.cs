using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using OFT.Attributes;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;

namespace MyIndicators
{
    [Category("Order Flow")]
    [Description("Identifies and visualizes trades that are large relative to the current market regime, reducing the need for fixed volume thresholds.")]
    [DisplayName("Adaptive Big Trades Modif")]
    public class AdaptiveBigTradesModif : Indicator
    {
        #region Fields

        private readonly List<TradeBubble> _bubbles = new();
        private readonly object _locker = new();

        // Threshold (signal core)
        private decimal _calculatedThreshold;
        private bool _isCalculated;

        // Rendering
        private RenderFont _font;
        private System.Drawing.Color _buyColorNative;
        private System.Drawing.Color _sellColorNative;
        private RenderPen _outlinePen;

        // Scheduler / scope
        private DateTime _lastRequestedFrom = DateTime.MinValue;
        private DateTime _lastRequestedTo = DateTime.MinValue;
        private int _lastSeenCurrentBar = -1;

        // Dedupe / update support (best-effort)
        private readonly Dictionary<string, int> _bubbleIndexByKey = new(); // key -> index in _bubbles

        #endregion

        #region Parameters

        public enum DataScope
        {
            VisibleRange,
            LookbackBars
        }

        public enum DirectionFilter
        {
            Both,
            OnlyBuys,
            OnlySells
        }

        public enum SizeMode
        {
            Fixed,
            Scaled
        }

        // =====================
        // Data
        // =====================

        [Display(Name = "Data Scope", GroupName = "Data", Order = 10)]
        public DataScope Scope { get; set; } = DataScope.VisibleRange;

        [Display(Name = "Lookback Bars", GroupName = "Data", Order = 20)]
        [Range(100, 20000)]
        public int LookbackBars { get; set; } = 4000;

        // =====================
        // Calculation
        // =====================

        [Display(
            Name = "Percentile Filter",
            GroupName = "Calculation",
            Order = 10,
            Description = "0.80 means the largest ~20% of trades are highlighted (signal definition remains: trade.Volume > threshold).")]
        [Range(0.10, 0.99)]
        public decimal Percentile { get; set; } = 0.80m;

        [Display(
            Name = "Min Trades For Threshold",
            GroupName = "Calculation",
            Order = 20,
            Description = "Prevents unstable thresholds when there are too few trades in the current scope.")]
        [Range(50, 5000)]
        public int MinTradesForThreshold { get; set; } = 300;

        // =====================
        // Filters
        // =====================

        [Display(Name = "Direction", GroupName = "Filters", Order = 10)]
        public DirectionFilter Direction { get; set; } = DirectionFilter.Both;

        [Display(
            Name = "Min Price Distance (ticks)",
            GroupName = "Filters",
            Order = 20,
            Description = "Optional visual de-clutter. 0 disables.")]
        [Range(0, 100)]
        public int MinPriceDistanceTicks { get; set; } = 0;

        // =====================
        // Visualization
        // =====================

        [Display(Name = "Size Mode", GroupName = "Visualization", Order = 10)]
        public SizeMode BubbleSizeMode { get; set; } = SizeMode.Fixed;

        [Display(Name = "Size", GroupName = "Visualization", Order = 20)]
        [Range(1, 100)]
        public int ObjectSize { get; set; } = 40;

        [Display(Name = "Max Radius", GroupName = "Visualization", Order = 30)]
        [Range(5, 200)]
        public int MaxRadius { get; set; } = 60;

        [Display(Name = "Show Text", GroupName = "Visualization", Order = 40)]
        public bool ShowText { get; set; } = false;

        [Display(Name = "Font Size", GroupName = "Visualization", Order = 50)]
        [Range(8, 18)]
        public int FontSize { get; set; } = 10;

        [Display(Name = "Opacity (0-255)", GroupName = "Visualization", Order = 60)]
        [Range(10, 255)]
        public int Opacity { get; set; } = 150;

        [Display(Name = "Outline Width", GroupName = "Visualization", Order = 70)]
        [Range(0, 5)]
        public int OutlineWidth { get; set; } = 1;

        // =====================
        // Colors (ATAS)
        // =====================

        [Display(Name = "Buy Color", GroupName = "Colors", Order = 10)]
        public CrossColor BuyColor { get; set; } = CrossColor.FromArgb(150, 50, 205, 50);

        [Display(Name = "Sell Color", GroupName = "Colors", Order = 20)]
        public CrossColor SellColor { get; set; } = CrossColor.FromArgb(150, 220, 20, 60);

        #endregion


        #region Internal Class for Drawing
        class TradeBubble
        {
            public int Bar { get; set; }
            public long TimeTicks { get; set; }   // Stable identity for dedupe/update
            public decimal Price { get; set; }
            public decimal Volume { get; set; }
            public TradeDirection Direction { get; set; }
            public Rectangle Region { get; set; } // Mouse hover hitbox
        }
        #endregion

        #region ctor
        public AdaptiveBigTradesModif()
        {
            DataSeries[0].IsHidden = true;
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            DrawAbovePrice = true;

            SubscribeToDrawingEvents(DrawingLayouts.Historical | DrawingLayouts.LatestBar | DrawingLayouts.Final);
        }
        #endregion

        #region protected methods

        protected override void OnInitialize()
        {
            UpdateNativeResources();

            // React to UI changes without requiring indicator reload
            PropertyChanged += OnIndicatorPropertyChanged;
        }

        protected override void OnDispose()
        {
            PropertyChanged -= OnIndicatorPropertyChanged;
            base.OnDispose();
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // Trigger once per full calculation / when range changes
            if (bar != 0)
                return;

            if (CurrentBar <= 1)
                return;

            // Detect chart changes to allow re-scope
            if (_lastSeenCurrentBar != CurrentBar)
            {
                _lastSeenCurrentBar = CurrentBar;
            }

            DateTime from;
            DateTime to;

            if (Scope == DataScope.VisibleRange && ChartInfo != null)
            {
                var first = Math.Max(0, ChartInfo.PriceChartContainer.FirstVisibleBarNumber);
                var last = Math.Min(CurrentBar - 1, ChartInfo.PriceChartContainer.LastVisibleBarNumber);

                from = GetCandle(first).Time;
                to = GetCandle(last).LastTime;
            }
            else
            {
                var last = CurrentBar - 1;
                var first = Math.Max(0, last - LookbackBars);

                from = GetCandle(first).Time;
                to = GetCandle(last).LastTime;
            }

            // Avoid spamming identical requests
            if (from == _lastRequestedFrom && to == _lastRequestedTo && _isCalculated)
                return;

            _lastRequestedFrom = from;
            _lastRequestedTo = to;

            // Reset computed threshold when scope changes
            _isCalculated = false;
            _calculatedThreshold = 0;

            lock (_locker)
            {
                _bubbles.Clear();
                _bubbleIndexByKey.Clear();
            }

            RequestForCumulativeTrades(new CumulativeTradesRequest(
                from,
                to,
                minVolume: 0,
                maxVolume: 0
            ));
        }


        // Raw cumulative trades snapshot arrives here
        protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            var trades = cumulativeTrades.ToList();
            if (trades.Count == 0)
                return;

            if (trades.Count < MinTradesForThreshold)
            {
                // Not enough data to compute a stable threshold
                _isCalculated = false;
                _calculatedThreshold = 0;
                RedrawChart();
                return;
            }

            // Count volumes
            var volumeCounts = new SortedDictionary<decimal, int>();
            foreach (var t in trades)
            {
                if (!volumeCounts.TryGetValue(t.Volume, out var c))
                    volumeCounts[t.Volume] = 1;
                else
                    volumeCounts[t.Volume] = c + 1;
            }

            // Percentile cut by trade-count
            var total = trades.Count;
            var cutoff = (int)Math.Ceiling(total * Percentile);

            int acc = 0;
            _calculatedThreshold = 0;
            foreach (var kvp in volumeCounts)
            {
                acc += kvp.Value;
                _calculatedThreshold = kvp.Key;

                if (acc >= cutoff)
                    break;
            }

            _isCalculated = true;

            lock (_locker)
            {
                _bubbles.Clear();
                _bubbleIndexByKey.Clear();

                // Trades should already be time-ordered, but enforce to be safe
                foreach (var t in trades.OrderBy(x => x.Time))
                {
                    if (!PassDirectionFilter(t.Direction))
                        continue;

                    if (t.Volume <= _calculatedThreshold) // STRICT: official-like (>)
                        continue;

                    var barIndex = ResolveBarIndex(t.Time);
                    if (barIndex < 0)
                        continue;

                    var price = t.FirstPrice;

                    if (MinPriceDistanceTicks > 0 && IsTooCloseToExisting(barIndex, price))
                        continue;

                    var key = MakeKey(barIndex, t.Time.Ticks, t.FirstPrice, t.Volume, t.Direction);
                    if (_bubbleIndexByKey.ContainsKey(key))
                        continue;

                    _bubbleIndexByKey[key] = _bubbles.Count;
                    _bubbles.Add(new TradeBubble
                    {
                        Bar = barIndex,
                        TimeTicks = t.Time.Ticks,
                        Price = t.FirstPrice,
                        Volume = t.Volume,
                        Direction = t.Direction
                    });
                }

                PruneIfNeeded();
            }

            RedrawChart();
        }

        protected override void OnCumulativeTrade(CumulativeTrade trade)
        {
            if (!_isCalculated)
                return;

            if (!PassDirectionFilter(trade.Direction))
                return;

            if (trade.Volume <= _calculatedThreshold) // STRICT (>)
                return;

            var barIndex = ResolveBarIndex(trade.Time);
            if (barIndex < 0)
                return;

            lock (_locker)
            {
                var key = MakeKey(barIndex, trade.Time.Ticks, trade.FirstPrice, trade.Volume, trade.Direction);
                if (_bubbleIndexByKey.ContainsKey(key))
                    return;

                if (MinPriceDistanceTicks > 0 && IsTooCloseToExisting(barIndex, trade.FirstPrice))
                    return;

                _bubbleIndexByKey[key] = _bubbles.Count;
                _bubbles.Add(new TradeBubble
                {
                    Bar = barIndex,
                    TimeTicks = trade.Time.Ticks,
                    Price = trade.FirstPrice,
                    Volume = trade.Volume,
                    Direction = trade.Direction
                });


                PruneIfNeeded();
            }

            RedrawChart();
        }


        protected override void OnUpdateCumulativeTrade(CumulativeTrade trade)
        {
            if (!_isCalculated)
                return;

            if (!PassDirectionFilter(trade.Direction))
                return;

            var barIndex = ResolveBarIndex(trade.Time);
            if (barIndex < 0)
                return;

            lock (_locker)
            {
                // Remove outdated entries that might represent the same evolving trade (best-effort)
                // Strategy: remove any bubble on same bar, same direction, same first price, within small time window
                var updated = false;

                for (int i = _bubbles.Count - 1; i >= 0; i--)
                {
                    var b = _bubbles[i];
                    if (b.Bar != barIndex)
                        continue;

                    if (b.Direction != trade.Direction)
                        continue;

                    if (b.Price != trade.FirstPrice)
                        continue;

                    // If trade no longer qualifies, remove bubble
                    if (trade.Volume <= _calculatedThreshold)
                    {
                        _bubbles.RemoveAt(i);
                        updated = true;
                        continue;
                    }

                    // Update volume (and keep)
                    b.Volume = trade.Volume;
                    updated = true;
                }

                if (!updated && trade.Volume > _calculatedThreshold)
                {
                    if (MinPriceDistanceTicks == 0 || !IsTooCloseToExisting(barIndex, trade.FirstPrice))
                    {
                        _bubbles.Add(new TradeBubble
                        {
                            Bar = barIndex,
                            TimeTicks = trade.Time.Ticks,
                            Price = trade.FirstPrice,
                            Volume = trade.Volume,
                            Direction = trade.Direction
                        });
                    }
                }

                // Rebuild index (safe; optimize later)
                _bubbleIndexByKey.Clear();
                for (int i = 0; i < _bubbles.Count; i++)
                {
                    var b = _bubbles[i];
                    var key = MakeKey(b.Bar, b.TimeTicks, b.Price, b.Volume, b.Direction);
                    if (!_bubbleIndexByKey.ContainsKey(key))
                        _bubbleIndexByKey[key] = i;
                }

                PruneIfNeeded();
            }

            RedrawChart();
        }


        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (context == null || ChartInfo == null)
                return;

            lock (_locker)
            {
                foreach (var bubble in _bubbles)
                {
                    // Optimization: draw only when visible
                    if (bubble.Bar < FirstVisibleBarNumber || bubble.Bar > LastVisibleBarNumber)
                        continue;

                    // Coordinates
                    int x = ChartInfo.GetXByBar(bubble.Bar);
                    int y = ChartInfo.GetYByPrice(bubble.Price, false);

                    int radius;

                    if (BubbleSizeMode == SizeMode.Fixed)
                    {
                        radius = Math.Min(ObjectSize, MaxRadius);
                    }
                    else
                    {
                        // Scaled: proportional but bounded
                        var factor = (double)(bubble.Volume / Math.Max(1m, _calculatedThreshold));
                        radius = (int)Math.Round(ObjectSize * Math.Min(3.0, factor));
                        radius = Math.Max(3, Math.Min(radius, MaxRadius));
                    }

                    Rectangle rect = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
                    bubble.Region = rect;

                    var color = bubble.Direction == TradeDirection.Buy ? _buyColorNative : _sellColorNative;

                    context.FillEllipse(color, rect);

                    if (_outlinePen != null)
                        context.DrawEllipse(_outlinePen, rect);

                    if (ShowText)
                    {
                        var txt = $"{bubble.Volume:0.##}";
                        var sz = context.MeasureString(txt, _font);
                        context.DrawString(txt, _font, System.Drawing.Color.White, x - (sz.Width / 2), y - (sz.Height / 2));
                    }
                }
            }

            DrawHoverTooltip(context);
        }

        #endregion

        #region Private Methods

        private void DrawHoverTooltip(RenderContext context)
        {
            if (ChartInfo == null || _font == null)
                return;

            var mouse = ChartInfo.MouseLocationInfo.LastPosition;
            TradeBubble hoveredBubble = null;

            lock (_locker)
            {
                for (int i = _bubbles.Count - 1; i >= 0; i--)
                {
                    if (_bubbles[i].Region.Contains(mouse))
                    {
                        hoveredBubble = _bubbles[i];
                        break;
                    }
                }
            }

            if (hoveredBubble != null)
            {
                string text = $"{hoveredBubble.Direction}\nVol: {hoveredBubble.Volume:0.##}\nPrice: {hoveredBubble.Price}";
                var size = context.MeasureString(text, _font);

                Rectangle bg = new Rectangle(mouse.X + 10, mouse.Y, size.Width + 10, size.Height + 5);
                context.FillRectangle(System.Drawing.Color.FromArgb(200, 0, 0, 0), bg);

                context.DrawString(text, _font, System.Drawing.Color.White, bg.X + 5, bg.Y + 2);
            }
        }

        private void UpdateNativeResources()
        {
            _buyColorNative = System.Drawing.Color.FromArgb(Opacity, BuyColor.R, BuyColor.G, BuyColor.B);
            _sellColorNative = System.Drawing.Color.FromArgb(Opacity, SellColor.R, SellColor.G, SellColor.B);

            _font = new RenderFont("Arial", FontSize);
            _outlinePen = OutlineWidth <= 0
                ? null
                : new RenderPen(System.Drawing.Color.Black, OutlineWidth);
        }

        private void OnIndicatorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BuyColor)
                || e.PropertyName == nameof(SellColor)
                || e.PropertyName == nameof(FontSize)
                || e.PropertyName == nameof(Opacity)
                || e.PropertyName == nameof(OutlineWidth)
                || e.PropertyName == nameof(ObjectSize)
                || e.PropertyName == nameof(MaxRadius)
                || e.PropertyName == nameof(ShowText)
                || e.PropertyName == nameof(BubbleSizeMode))
            {
                UpdateNativeResources();
                RedrawChart();
            }

        }

        private bool PassDirectionFilter(TradeDirection dir)
        {
            return Direction switch
            {
                DirectionFilter.Both => true,
                DirectionFilter.OnlyBuys => dir == TradeDirection.Buy,
                DirectionFilter.OnlySells => dir == TradeDirection.Sell,
                _ => true
            };
        }

        private string MakeKey(int bar, long timeTicks, decimal price, decimal vol, TradeDirection dir)
        {
            return $"{bar}|{timeTicks}|{price:0.########}|{vol:0.########}|{(int)dir}";
        }

        private int ResolveBarIndex(DateTime tradeTime)
        {
            // Fast-path: if trade time is within last bar, map there
            var last = CurrentBar - 1;
            if (last < 0)
                return -1;

            for (int i = last; i >= 0; i--)
            {
                var c = GetCandle(i);
                if (c.Time <= tradeTime && c.LastTime >= tradeTime)
                    return i;

                if (c.Time < tradeTime && c.LastTime < tradeTime)
                    break;
            }

            // Fallback: linear scan backwards is safe; optimize later if needed
            for (int i = last; i >= 0; i--)
            {
                var c = GetCandle(i);
                if (c.Time <= tradeTime && c.LastTime >= tradeTime)
                    return i;
            }

            return -1;
        }

        private void PruneIfNeeded()
        {
            // Hard safety cap (system indicator)
            const int hardCap = 20000;
            if (_bubbles.Count <= hardCap)
                return;

            // Drop oldest first
            _bubbles.RemoveRange(0, _bubbles.Count - hardCap);

            // Keep dedupe index coherent
            _bubbleIndexByKey.Clear();
            for (int i = 0; i < _bubbles.Count; i++)
            {
                var b = _bubbles[i];
                var key = MakeKey(b.Bar, b.TimeTicks, b.Price, b.Volume, b.Direction);
                if (!_bubbleIndexByKey.ContainsKey(key))
                    _bubbleIndexByKey[key] = i;
            }
        }


        private bool IsTooCloseToExisting(int bar, decimal price)
        {
            if (MinPriceDistanceTicks <= 0)
                return false;

            var tickSize = InstrumentInfo.TickSize;
            var minDist = MinPriceDistanceTicks * tickSize;

            foreach (var b in _bubbles)
            {
                if (b.Bar != bar)
                    continue;

                if (Math.Abs((double)(b.Price - price)) < (double)minDist)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
