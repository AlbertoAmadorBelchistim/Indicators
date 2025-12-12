namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using ATAS.DataFeedsCore;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Color = System.Drawing.Color;
using DashStyle = System.Drawing.Drawing2D.DashStyle;
using Pen = OFT.Rendering.Tools.RenderPen;

[HelpLink("https://help.atas.net/support/solutions/articles/72000633119")]
[Category(IndicatorCategories.Trading)]
[DisplayName("Trades On Chart Modif v4")]
[Display(Description = "Shows historical and recent trades. Use 'Manual Time Offset' to align trades.")]
public class TradesOnChartModifV4 : Indicator
{
    #region Nested Types

    internal class TradeObj
    {
        internal int OpenBar { get; set; }
        internal decimal OpenPrice { get; set; }
        internal int CloseBar { get; set; }
        internal decimal ClosePrice { get; set; }
        internal OrderDirections Direction { get; set; }
        internal decimal PnL { get; set; }
        internal decimal PnLTicks { get; set; }
        internal DateTime OpenTime { get; set; }
        internal DateTime CloseTime { get; set; }
        internal decimal Volume { get; set; }
        internal string Security { get; set; }

        public TradeObj(HistoryMyTrade trade)
        {
            OpenPrice = trade.OpenPrice;
            ClosePrice = trade.ClosePrice;
            Direction = trade.OpenVolume > 0 ? OrderDirections.Buy : OrderDirections.Sell;
            PnL = trade.PnL;
            PnLTicks = trade.TicksPnL;
            OpenTime = trade.OpenTime;
            CloseTime = trade.CloseTime;
            Volume = Math.Abs(trade.OpenVolume);
            Security = trade.Security.Code;
        }

        public TradeObj() { }
    }

    public enum LabelDisplayMode
    {
        [Display(Name = "Hide")] Hide,
        [Display(Name = "Short")] Short,
        [Display(Name = "Full")] Full
    }

    #endregion

    #region Fields

    private RenderFont _font = new RenderFont("Arial", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
    private RenderFont _labelFont = new RenderFont("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);

    private readonly List<TradeObj> _tradesHistorical = new();
    private readonly List<TradeObj> _tooltipTrades = new();

    private Pen _buyPen;
    private Pen _sellPen;
    private Color _buyColor;
    private Color _sellColor;
    private Color _profitColor;
    private Color _lossColor;
    private float _lineWidth = 2f;
    private DashStyle _lineStyle = DashStyle.Dash;
    private readonly List<Rectangle> _labelsAbove = new();
    private readonly List<Rectangle> _labelsBelow = new();
    private Pen _borderPen = new Pen(Color.Black, 1);

    private bool _historyLoaded;
    private int _historyTradesCount;
    private int _recentTradesCount;
    private int _processedMyTradesCount = 0;

    #endregion

    #region Properties

    [Display(Name = "Manual Time Offset (Hours)", GroupName = "Visualization", Description = "Adjust this if trades appear in wrong place. Example: -1, 0, 1, 5...")]
    public int ManualTimeOffset { get; set; } = 0;

    [Display(Name = "Show Lines", GroupName = "Visualization")]
    public bool ShowLine { get; set; } = true;

    [Display(Name = "Show Tooltip", GroupName = "Visualization")]
    public bool ShowTooltip { get; set; } = true;

    [Display(Name = "Label Display", GroupName = "Visualization")]
    public LabelDisplayMode LabelDisplay { get; set; } = LabelDisplayMode.Hide;

    [Display(Name = "Buy Color", GroupName = "Visualization")]
    public Color BuyColor
    {
        get => _buyColor;
        set { _buyColor = value; _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle); }
    }

    [Display(Name = "Sell Color", GroupName = "Visualization")]
    public Color SellColor
    {
        get => _sellColor;
        set { _sellColor = value; _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle); }
    }

    [Display(Name = "Profit Color", GroupName = "Visualization")]
    public Color ProfitColor { get => _profitColor; set => _profitColor = value; }

    [Display(Name = "Loss Color", GroupName = "Visualization")]
    public Color LossColor { get => _lossColor; set => _lossColor = value; }

    [Range(1, 20)]
    [Display(Name = "Line Width", GroupName = "Visualization")]
    public float LineWidth
    {
        get => _lineWidth;
        set { _lineWidth = value; _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle); _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle); }
    }

    [Display(Name = "Dash Style", GroupName = "Visualization")]
    public DashStyle LineStyle
    {
        get => _lineStyle;
        set { _lineStyle = value; _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle); _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle); }
    }

    [Range(1, 10)]
    [Display(Name = "Marker Size", GroupName = "Visualization")]
    public int MarkerSize { get; set; } = 4;

    [Display(Name = "Debug Overlay", GroupName = "Visualization")]
    public bool ShowDebug { get; set; } = true;

    #endregion

    #region ctor

    public TradesOnChartModifV4() : base(true)
    {
        DenyToChangePanel = true;
        DataSeries[0].IsHidden = true;
        ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

        SubscribeToDrawingEvents(DrawingLayouts.Final);
        EnableCustomDrawing = true;
    }

    #endregion

    #region Protected Methods

    protected override void OnInitialize()
    {
        TradingStatisticsProvider.Realtime.HistoryMyTrades.Added += OnHistoryTradeAdded;
        TradingManager.PortfolioSelected += TradingManager_PortfolioChanged;
        TradingManager.SecuritySelected += TradingManager_SecurityChanged;

        OnApplyDefaultColors();
        OnRecalculate();
    }

    protected override void OnDispose()
    {
        TradingStatisticsProvider.Realtime.HistoryMyTrades.Added -= OnHistoryTradeAdded;
        if (TradingManager != null)
        {
            TradingManager.PortfolioSelected -= TradingManager_PortfolioChanged;
            TradingManager.SecuritySelected -= TradingManager_SecurityChanged;
        }
        base.OnDispose();
    }

    protected override void OnApplyDefaultColors()
    {
        BuyColor = Color.FromArgb(255, 0, 180, 0);
        SellColor = Color.FromArgb(255, 200, 0, 0);
        ProfitColor = Color.FromArgb(255, 100, 200, 100);
        LossColor = Color.FromArgb(255, 200, 100, 100);
    }

    private void TradingManager_SecurityChanged(Security obj) => OnRecalculate();
    private void TradingManager_PortfolioChanged(Portfolio obj) => OnRecalculate();

    protected override void OnRecalculate()
    {
        _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle);
        _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle);

        _tradesHistorical.Clear();
        _tradesSynthetic.Clear();
        _openLots.Clear();
        _processedMyTradesCount = 0;

        if (CurrentBar > 0 && TradingManager?.Portfolio != null && TradingManager?.Security != null)
        {
            AddHistoryMyTradesSnapshot();
            CheckForNewRealtimeTrades();
        }
    }

    protected override void OnCalculate(int bar, decimal value)
    {
        if (!_historyLoaded && bar == CurrentBar - 1)
        {
            _historyLoaded = true;
            OnRecalculate();
            RedrawChart();
        }

        // Polling constante
        if (bar == CurrentBar - 1)
        {
            CheckForNewRealtimeTrades();
        }
    }

    #region Rendering

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ShowDebug) DrawDebugOverlay(context);

        if (ChartInfo is null) return;

        // Histórico
        foreach (var trade in _tradesHistorical)
            RenderSingleTrade(context, trade.OpenBar, trade.OpenPrice, trade.CloseBar, trade.ClosePrice, trade.Direction, trade);

        // Sintético (RealTime)
        foreach (var synth in _tradesSynthetic)
        {
            decimal tickCost = 0;
            if (TradingManager?.Security != null)
                tickCost = TradingManager.Security.TickCost;

            var tempObj = new TradeObj
            {
                OpenBar = synth.OpenBar,
                CloseBar = synth.CloseBar,
                OpenPrice = synth.OpenPrice,
                ClosePrice = synth.ClosePrice,
                Direction = synth.Direction,
                Volume = synth.Volume,
                OpenTime = synth.OpenTime,
                CloseTime = synth.CloseTime,
                Security = TradingManager?.Security?.Code ?? "",
                PnLTicks = synth.PnLTicks,
                PnL = synth.PnLTicks * tickCost
            };
            RenderSingleTrade(context, synth.OpenBar, synth.OpenPrice, synth.CloseBar, synth.ClosePrice, synth.Direction, tempObj);
        }

        DrawTooltips(context);
    }

    private void RenderSingleTrade(RenderContext context, int openBar, decimal openPrice, int closeBar, decimal closePrice, OrderDirections dir, TradeObj tradeDataForLabel)
    {
        if (closeBar < FirstVisibleBarNumber - 1 && openBar < FirstVisibleBarNumber - 1) return;
        if (openBar > LastVisibleBarNumber + 1) return;

        var x1 = ChartInfo.GetXByBar(openBar, false);
        var y1 = ChartInfo.GetYByPrice(openPrice, false);
        var x2 = ChartInfo.GetXByBar(closeBar, false);
        var y2 = ChartInfo.GetYByPrice(closePrice, false);
        var pen = GetPenByDirection(dir);

        if (ShowLine) context.DrawLine(pen, x1, y1, x2, y2);

        var mo1 = DrawMarker(context, new Point(x1, y1), dir, true);
        var mo2 = DrawMarker(context, new Point(x2, y2), dir, false);

        bool moLabel = false;
        if (LabelDisplay != LabelDisplayMode.Hide && closeBar >= 0)
        {
            var candle = GetCandle(closeBar);
            var isAbove = dir == OrderDirections.Buy;
            var (labelRect, labelHover) = DrawTradeLabel(context, tradeDataForLabel, closeBar, candle, isAbove);
            moLabel = labelHover;
            if (isAbove) _labelsAbove.Add(labelRect); else _labelsBelow.Add(labelRect);
        }

        if (ShowTooltip && (mo1 || mo2 || moLabel)) _tooltipTrades.Add(tradeDataForLabel);
    }

    private void DrawTooltips(RenderContext context)
    {
        if (!_tooltipTrades.Any()) return;
        var y = MouseLocationInfo.LastPosition.Y + 15;
        var x = MouseLocationInfo.LastPosition.X + 15;
        foreach (var trade in _tooltipTrades)
        {
            DrawTooltip(context, trade, x, ref y);
            y += 5;
        }
        _tooltipTrades.Clear();
        _labelsAbove.Clear();
        _labelsBelow.Clear();
    }

    private void DrawTooltip(RenderContext context, TradeObj trade, int x, ref int y)
    {
        var directionColor = trade.Direction == OrderDirections.Buy ? _buyColor : _sellColor;
        var resultColor = trade.PnLTicks >= 0 ? _profitColor : _lossColor;
        var cornerRadius = 3;

        var direction = trade.Direction == OrderDirections.Buy ? "Long" : "Short";
        var openTime = ToChartTime(trade.OpenTime);
        var closeTime = ToChartTime(trade.CloseTime);

        var topText = $"{direction} {trade.Volume} {trade.Security}{Environment.NewLine}{Environment.NewLine}" +
                      $"Entry\t:  {ChartInfo.GetPriceString(trade.OpenPrice)}  {openTime:dd MMM HH:mm:ss}{Environment.NewLine}" +
                      $"Exit\t:  {ChartInfo.GetPriceString(trade.ClosePrice)}  {closeTime:dd MMM HH:mm:ss}";

        string bottomText;
        if (trade.PnL != 0)
            bottomText = $"Result:  {(trade.PnL > 0 ? "+" : "")}{trade.PnL:N2}  ({trade.PnLTicks} ticks)";
        else
            bottomText = $"Result:  {trade.PnLTicks} ticks";

        var topSize = context.MeasureString(topText, _font);
        var bottomSize = context.MeasureString(bottomText, _font);

        var padding = 10;
        var width = (int)Math.Max(topSize.Width, bottomSize.Width) + padding * 2;
        var topHeight = (int)topSize.Height + padding * 2;
        var bottomHeight = (int)bottomSize.Height + padding * 2;

        if (x + width > ChartInfo.PriceChartContainer.Region.Width)
            x -= width + 30;

        var topRect = new Rectangle(x, y, width, topHeight + cornerRadius * 2);
        var bottomRect = new Rectangle(x, y + topHeight, width, bottomHeight);

        context.FillRectangle(directionColor, topRect, cornerRadius);
        context.FillRectangle(resultColor, bottomRect, cornerRadius);
        context.FillRectangle(resultColor, new Rectangle(x, y + topHeight, width, cornerRadius * 2));

        var textFormat = new RenderStringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        context.DrawString(topText, _font, Color.White, new Rectangle(x + padding, y + padding, width - padding * 2, topHeight - padding * 2), textFormat);
        context.DrawString(bottomText, _font, Color.White, new Rectangle(x + padding, y + topHeight + padding, width - padding * 2, bottomHeight - padding * 2), textFormat);

        y += topHeight + bottomHeight;
    }

    private bool DrawMarker(RenderContext context, Point point, OrderDirections direction, bool isOpen)
    {
        var shift = MarkerSize * 3;
        var dir = direction == OrderDirections.Buy ? 1 : -1;
        Point p1, p2, p3;

        if (isOpen)
        {
            p1 = point;
            p2 = new Point(point.X - shift, point.Y + (shift * 2 * dir));
            p3 = new Point(point.X + shift, point.Y + (shift * 2 * dir));
        }
        else
        {
            p1 = point;
            p2 = new Point(point.X - shift, point.Y - (shift * 2 * dir));
            p3 = new Point(point.X + shift, point.Y - (shift * 2 * dir));
        }

        var color = GetMarkerColor(direction, isOpen);
        var points = new Point[] { p1, p2, p3 };
        context.FillPolygon(color, points);
        context.DrawPolygon(_borderPen, points);
        return IsPointInTriangle(MouseLocationInfo.LastPosition, p1, p2, p3);
    }

    private (Rectangle Rect, bool MouseOver) DrawTradeLabel(RenderContext context, TradeObj trade, int bar, IndicatorCandle candle, bool isAbove)
    {
        var direction = trade.Direction == OrderDirections.Buy ? "L" : "S";
        var pnlSign = trade.PnLTicks > 0 ? "+" : "";

        string leftText = $"{direction} {trade.Volume}";
        string rightText = $" {pnlSign}{trade.PnLTicks}t";

        if (LabelDisplay == LabelDisplayMode.Full)
        {
            var entryPrice = ChartInfo.GetPriceString(trade.OpenPrice);
            var exitPrice = ChartInfo.GetPriceString(trade.ClosePrice);
            leftText += $" | {entryPrice}->{exitPrice}";
        }

        var leftSize = context.MeasureString(leftText, _labelFont);
        var rightSize = context.MeasureString(rightText, _labelFont);
        var padding = 2;
        var leftWidth = leftSize.Width + padding * 2;
        var rightWidth = rightSize.Width + padding * 2;
        var rectWidth = leftWidth + rightWidth;
        var rectHeight = Math.Max(leftSize.Height, rightSize.Height) + padding;

        var candleX = ChartInfo.GetXByBar(bar, false);
        var labelX = candleX - rectWidth / 2;
        var markerOffset = MarkerSize * 6;
        var baseY = isAbove
            ? ChartInfo.GetYByPrice(candle.High, false) - markerOffset - rectHeight
            : ChartInfo.GetYByPrice(candle.Low, false) + markerOffset;

        var testRect = new Rectangle(labelX, baseY, rectWidth, rectHeight);

        var allLabels = _labelsAbove.Concat(_labelsBelow).ToList();
        int attempts = 0;
        while (allLabels.Any(r => r.IntersectsWith(testRect)) && attempts < 10)
        {
            var intersecting = allLabels.Where(r => r.IntersectsWith(testRect)).ToList();
            if (isAbove) testRect.Y = intersecting.Min(r => r.Y) - rectHeight - 2;
            else testRect.Y = intersecting.Max(r => r.Bottom) + 2;
            attempts++;
        }

        var directionColor = trade.Direction == OrderDirections.Buy ? _buyColor : _sellColor;
        var resultColor = trade.PnLTicks >= 0 ? _profitColor : _lossColor;

        context.FillRectangle(directionColor, new Rectangle(testRect.X, testRect.Y, leftWidth, testRect.Height));
        context.FillRectangle(resultColor, new Rectangle(testRect.X + leftWidth, testRect.Y, rightWidth, testRect.Height));
        context.DrawRectangle(_borderPen, testRect);

        var centerFormat = new RenderStringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        context.DrawString(leftText, _labelFont, Color.White, new Rectangle(testRect.X, testRect.Y, leftWidth, testRect.Height), centerFormat);
        context.DrawString(rightText, _labelFont, Color.White, new Rectangle(testRect.X + leftWidth, testRect.Y, rightWidth, testRect.Height), centerFormat);

        return (testRect, testRect.Contains(MouseLocationInfo.LastPosition));
    }

    #endregion

    #endregion

    #region Private Methods & Helpers

    // CORRECCIÓN CLAVE: Usamos el Offset manual
    private DateTime ToChartTime(DateTime tradeTime)
    {
        return tradeTime.AddHours(ManualTimeOffset);
    }

    private int GetBarByDate(DateTime time)
    {
        if (CurrentBar <= 0) return -1;
        if (time <= GetCandle(0).Time) return 0;

        // Si el tiempo está en el futuro, devolvemos la última barra
        // Esto es lo que causaba la "pila" cuando el offset era incorrecto
        if (time >= GetCandle(CurrentBar - 1).Time) return CurrentBar - 1;

        int left = 0, right = CurrentBar - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            var midTime = GetCandle(mid).Time;
            if (midTime == time) return mid;
            if (midTime < time) left = mid + 1;
            else right = mid - 1;
        }
        return Math.Max(0, right);
    }

    private void AddHistoryMyTradesSnapshot()
    {
        if (TradingManager?.Portfolio == null || TradingManager?.Security == null) return;
        var trades = TradingStatisticsProvider.Realtime.HistoryMyTrades
            .Where(t => t.AccountID == TradingManager.Portfolio.AccountID && t.Security.Code == TradingManager.Security.Code);

        foreach (var trade in trades) CreateHistoricalTradeObj(trade);
        _historyTradesCount = _tradesHistorical.Count;
    }

    private void OnHistoryTradeAdded(HistoryMyTrade trade)
    {
        if (TradingManager?.Portfolio == null || TradingManager?.Security == null) return;
        if (trade.AccountID == TradingManager.Portfolio.AccountID && trade.Security.Code == TradingManager.Security.Code)
        {
            CreateHistoricalTradeObj(trade);
            _historyTradesCount++;
            RedrawChart();
        }
    }

    private void CreateHistoricalTradeObj(HistoryMyTrade trade)
    {
        var enterBar = GetBarByDate(ToChartTime(trade.OpenTime));
        var exitBar = GetBarByDate(ToChartTime(trade.CloseTime));
        if (enterBar < 0) return;
        if (exitBar < 0) exitBar = enterBar;
        _tradesHistorical.Add(new TradeObj(trade) { OpenBar = enterBar, CloseBar = exitBar });
    }

    private Color GetMarkerColor(OrderDirections direction, bool isOpen) => direction == OrderDirections.Buy ? _buyColor : _sellColor;
    private Pen GetPenByDirection(OrderDirections directions) => directions == OrderDirections.Buy ? _buyPen : _sellPen;
    private Pen GetNewPen(Color color, float lineWidth, DashStyle lineStyle) => new Pen(color, lineWidth) { DashStyle = lineStyle };
    private bool IsPointInTriangle(Point p, Point p0, Point p1, Point p2) => Math.Abs(TriangleArea(p0, p1, p2) - (TriangleArea(p, p0, p1) + TriangleArea(p, p1, p2) + TriangleArea(p, p2, p0))) < 1.0;
    private double TriangleArea(Point p0, Point p1, Point p2) => Math.Abs((p0.X * (p1.Y - p2.Y) + p1.X * (p2.Y - p0.Y) + p2.X * (p0.Y - p1.Y)) / 2.0);

    private void DrawDebugOverlay(RenderContext context)
    {
        // Debug con info del offset
        var text = $"V4: Hist={_historyTradesCount} | RealTime={_recentTradesCount} | Synth={_tradesSynthetic.Count} | TimeOffset={ManualTimeOffset}h";
        var rect = new Rectangle(10, 10, 400, 25);
        context.FillRectangle(Color.FromArgb(150, 0, 0, 0), rect);
        context.DrawString(text, _font, Color.White, rect, new RenderStringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
    }

    #endregion

    #region MYTRADE MATCHING ENGINE (FIFO)

    private class OpenLot
    {
        public OrderDirections Dir;
        public decimal Price;
        public DateTime Time;
        public decimal RemainingVolume;
    }

    private class TradeObjSynthetic
    {
        public int OpenBar, CloseBar;
        public decimal OpenPrice, ClosePrice, Volume, PnLTicks;
        public DateTime OpenTime, CloseTime;
        public OrderDirections Direction;
    }

    private readonly List<OpenLot> _openLots = new();
    private readonly List<TradeObjSynthetic> _tradesSynthetic = new();

    private void CheckForNewRealtimeTrades()
    {
        if (TradingManager?.MyTrades == null) return;
        var allMyTrades = TradingManager.MyTrades.ToList();

        if (allMyTrades.Count > _processedMyTradesCount)
        {
            if (allMyTrades.Count < _processedMyTradesCount)
            {
                _processedMyTradesCount = 0;
                _openLots.Clear();
                _tradesSynthetic.Clear();
            }

            for (int i = _processedMyTradesCount; i < allMyTrades.Count; i++)
            {
                var mt = allMyTrades[i];
                if (TradingManager.Portfolio != null && TradingManager.Security != null &&
                    mt.AccountID == TradingManager.Portfolio.AccountID &&
                    mt.Security.Code == TradingManager.Security.Code)
                {
                    ProcessSingleMyTrade(mt);
                    _recentTradesCount++;
                }
            }

            _processedMyTradesCount = allMyTrades.Count;
            RedrawChart();
        }
    }

    private void ProcessSingleMyTrade(MyTrade mt)
    {
        var dir = mt.OrderDirection;
        var vol = Math.Abs(mt.Volume);
        var price = mt.Price;
        var time = mt.Time;

        if (vol <= 0) return;

        OrderDirections oppositeDir = dir == OrderDirections.Buy ? OrderDirections.Sell : OrderDirections.Buy;
        decimal volumeToClose = vol;

        for (int i = 0; i < _openLots.Count && volumeToClose > 0; i++)
        {
            var lot = _openLots[i];
            if (lot.Dir != oppositeDir) continue;

            decimal matchedVol = Math.Min(lot.RemainingVolume, volumeToClose);
            CreateSyntheticTrade(lot, price, time, matchedVol);

            lot.RemainingVolume -= matchedVol;
            volumeToClose -= matchedVol;

            if (lot.RemainingVolume <= 0.00001m)
            {
                _openLots.RemoveAt(i);
                i--;
            }
        }

        if (volumeToClose > 0.00001m)
        {
            _openLots.Add(new OpenLot { Dir = dir, Price = price, Time = time, RemainingVolume = volumeToClose });
        }
    }

    private void CreateSyntheticTrade(OpenLot openLot, decimal closePrice, DateTime closeTime, decimal volume)
    {
        var chartOpenTime = ToChartTime(openLot.Time);
        var chartCloseTime = ToChartTime(closeTime);
        var openBar = GetBarByDate(chartOpenTime);
        var closeBar = GetBarByDate(chartCloseTime);

        decimal sign = openLot.Dir == OrderDirections.Buy ? 1 : -1;
        decimal tickSize = InstrumentInfo.TickSize == 0 ? 1 : InstrumentInfo.TickSize;
        decimal pnlTicks = (closePrice - openLot.Price) * sign / tickSize;

        _tradesSynthetic.Add(new TradeObjSynthetic
        {
            OpenBar = openBar,
            CloseBar = closeBar,
            OpenPrice = openLot.Price,
            ClosePrice = closePrice,
            Direction = openLot.Dir,
            Volume = volume,
            OpenTime = openLot.Time,
            CloseTime = closeTime,
            PnLTicks = pnlTicks
        });
    }

    #endregion
}
