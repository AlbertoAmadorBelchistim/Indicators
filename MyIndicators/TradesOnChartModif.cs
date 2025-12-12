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
[DisplayName("Trades On Chart Modif v6")]
[Display(Description = "Visual update: Fixed labels near candles, distinct arrows.")]
public class TradesOnChartModifV6 : Indicator
{
    #region Nested Types

    internal class TradeObj
    {
        internal int OpenBar { get; set; }
        internal decimal OpenPrice { get; set; }
        internal int CloseBar { get; set; }
        internal decimal ClosePrice { get; set; }
        internal OrderDirections Direction { get; set; } // Dirección GLOBAL del trade
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
        [Display(Name = "Result Only")] Short,
        [Display(Name = "Full Info")] Full
    }

    #endregion

    #region Fields

    // FUENTES MÁS GRANDES Y CLARAS
    private RenderFont _fontTooltip = new RenderFont("Arial", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
    private RenderFont _labelFont = new RenderFont("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);

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

    // Pen para el borde de las etiquetas
    private Pen _borderPen = new Pen(Color.FromArgb(200, 0, 0, 0), 1);

    private bool _historyLoaded;
    private int _historyTradesCount;
    private int _recentTradesCount;

    #endregion

    #region Properties

    [Display(Name = "Manual Time Offset (Hours)", GroupName = "Visualization")]
    public int ManualTimeOffset { get; set; } = 0;

    [Display(Name = "Label Mode", GroupName = "Visualization")]
    public LabelDisplayMode LabelDisplay { get; set; } = LabelDisplayMode.Short;

    [Display(Name = "Label Distance (Pixels)", GroupName = "Visualization", Description = "Distance from candle wick")]
    [Range(5, 100)]
    public int LabelDistance { get; set; } = 20;

    [Display(Name = "Show Lines", GroupName = "Visualization")]
    public bool ShowLine { get; set; } = true;

    [Display(Name = "Show Tooltip", GroupName = "Visualization")]
    public bool ShowTooltip { get; set; } = true;

    [Display(Name = "Buy Color", GroupName = "Colors")]
    public Color BuyColor
    {
        get => _buyColor;
        set { _buyColor = value; _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle); }
    }

    [Display(Name = "Sell Color", GroupName = "Colors")]
    public Color SellColor
    {
        get => _sellColor;
        set { _sellColor = value; _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle); }
    }

    [Display(Name = "Profit Color", GroupName = "Colors")]
    public Color ProfitColor { get => _profitColor; set => _profitColor = value; }

    [Display(Name = "Loss Color", GroupName = "Colors")]
    public Color LossColor { get => _lossColor; set => _lossColor = value; }

    [Range(1, 20)]
    [Display(Name = "Line Width", GroupName = "Visual Style")]
    public float LineWidth
    {
        get => _lineWidth;
        set { _lineWidth = value; _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle); _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle); }
    }

    [Display(Name = "Dash Style", GroupName = "Visual Style")]
    public DashStyle LineStyle
    {
        get => _lineStyle;
        set { _lineStyle = value; _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle); _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle); }
    }

    [Range(1, 10)]
    [Display(Name = "Marker Size", GroupName = "Visual Style")]
    public int MarkerSize { get; set; } = 6; // Ligeramente más grandes por defecto

    [Display(Name = "Debug Overlay", GroupName = "System")]
    public bool ShowDebug { get; set; } = true;

    #endregion

    #region ctor

    public TradesOnChartModifV6() : base(true)
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
        BuyColor = Color.FromArgb(255, 0, 200, 0);       // Verde brillante
        SellColor = Color.FromArgb(255, 220, 0, 0);     // Rojo brillante
        ProfitColor = Color.FromArgb(255, 34, 139, 34); // ForestGreen
        LossColor = Color.FromArgb(255, 178, 34, 34);   // Firebrick
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
        _recentTradesCount = 0;

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

        // Render Histórico
        foreach (var trade in _tradesHistorical)
            RenderSingleTrade(context, trade);

        // Render Sintético (RealTime)
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
            RenderSingleTrade(context, tempObj);
        }

        DrawTooltips(context);
    }

    private void RenderSingleTrade(RenderContext context, TradeObj trade)
    {
        // Verificar visibilidad
        if (trade.CloseBar < FirstVisibleBarNumber - 1 && trade.OpenBar < FirstVisibleBarNumber - 1) return;
        if (trade.OpenBar > LastVisibleBarNumber + 1) return;

        var x1 = ChartInfo.GetXByBar(trade.OpenBar, false);
        var y1 = ChartInfo.GetYByPrice(trade.OpenPrice, false);
        var x2 = ChartInfo.GetXByBar(trade.CloseBar, false);
        var y2 = ChartInfo.GetYByPrice(trade.ClosePrice, false);

        // Línea de conexión: Color basado en el resultado (Ganancia/Pérdida) o Dirección
        // Usaremos la dirección del trade para la línea para mantener coherencia
        var pen = trade.Direction == OrderDirections.Buy ? _buyPen : _sellPen;

        if (ShowLine) context.DrawLine(pen, x1, y1, x2, y2);

        // --- DIBUJAR MARCADORES (Lógica de flechas mejorada) ---
        // Entrada (Open): Si es Buy -> Flecha Arriba. Si es Sell -> Flecha Abajo.
        var mo1 = DrawMarker(context, new Point(x1, y1), trade.Direction, true);

        // Salida (Close): La acción opuesta. 
        // Si el trade era Buy, la salida es una Venta (Flecha Abajo).
        // Si el trade era Sell, la salida es una Compra (Flecha Arriba).
        var exitAction = trade.Direction == OrderDirections.Buy ? OrderDirections.Sell : OrderDirections.Buy;
        var mo2 = DrawMarker(context, new Point(x2, y2), exitAction, false);

        // --- ETIQUETAS MEJORADAS ---
        bool moLabel = false;
        if (LabelDisplay != LabelDisplayMode.Hide && trade.CloseBar >= FirstVisibleBarNumber && trade.CloseBar <= LastVisibleBarNumber)
        {
            var candle = GetCandle(trade.CloseBar);

            // Decidir posición: Arriba del High o Abajo del Low
            // Si el trade fue Long, cerramos vendiendo (flecha roja abajo), la etiqueta se ve mejor ARRIBA.
            // Si el trade fue Short, cerramos comprando (flecha verde arriba), la etiqueta se ve mejor ABAJO.
            // O simplemente: Si Ganancia -> Verde, Si Pérdida -> Rojo.

            // Lógica simple: Poner etiqueta donde moleste menos.
            // Normalmente encima del High para Longs, debajo del Low para Shorts.
            bool placeAbove = trade.Direction == OrderDirections.Buy;

            var (labelRect, labelHover) = DrawFixedLabel(context, trade, trade.CloseBar, candle, placeAbove);
            moLabel = labelHover;
        }

        if (ShowTooltip && (mo1 || mo2 || moLabel)) _tooltipTrades.Add(trade);
    }

    // --- NUEVO MÉTODO PARA ETIQUETAS FIJAS ---
    private (Rectangle Rect, bool MouseOver) DrawFixedLabel(RenderContext context, TradeObj trade, int bar, IndicatorCandle candle, bool isAbove)
    {
        // 1. Contenido del texto
        string textContent = "";
        string pnlString = trade.PnL != 0
            ? $"{trade.PnL:N2} ({trade.PnLTicks}t)"
            : $"{trade.PnLTicks} ticks";

        if (LabelDisplay == LabelDisplayMode.Full)
        {
            var dirStr = trade.Direction == OrderDirections.Buy ? "LONG" : "SHORT";
            var entry = ChartInfo.GetPriceString(trade.OpenPrice);
            var exit = ChartInfo.GetPriceString(trade.ClosePrice);
            var time = trade.CloseTime.ToString("HH:mm");

            // Formato multilínea
            textContent = $"{dirStr} {trade.Volume}\n{entry} -> {exit}\n{pnlString}";
        }
        else // Short Mode
        {
            textContent = pnlString;
        }

        // 2. Medir y posicionar
        var textSize = context.MeasureString(textContent, _labelFont);
        int padding = 4;
        int width = textSize.Width + (padding * 2);
        int height = textSize.Height + (padding * 2);

        int centerX = ChartInfo.GetXByBar(bar, false);
        int yPos;

        // Usamos el High/Low de la vela para anclar, no el precio del trade.
        // Esto evita que la etiqueta tape la vela.
        if (isAbove)
        {
            int candleHighY = ChartInfo.GetYByPrice(candle.High, false);
            yPos = candleHighY - LabelDistance - height;
        }
        else
        {
            int candleLowY = ChartInfo.GetYByPrice(candle.Low, false);
            yPos = candleLowY + LabelDistance;
        }

        var rect = new Rectangle(centerX - (width / 2), yPos, width, height);

        // 3. Colores
        // Fondo: Verde si PnL positivo, Rojo si negativo.
        Color bgColor = trade.PnLTicks >= 0 ? _profitColor : _lossColor;
        // Hacemos el fondo un poco sólido para tapar la rejilla
        Color bgSolid = Color.FromArgb(240, bgColor.R, bgColor.G, bgColor.B);

        // 4. Dibujar
        context.FillRectangle(bgSolid, rect);
        context.DrawRectangle(_borderPen, rect);

        // Texto centrado
        var format = new RenderStringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        context.DrawString(textContent, _labelFont, Color.White, rect, format);

        // Conector opcional (pequeña línea desde la etiqueta a la vela)
        if (isAbove)
            context.DrawLine(_borderPen, centerX, rect.Bottom, centerX, rect.Bottom + (LabelDistance / 2));
        else
            context.DrawLine(_borderPen, centerX, rect.Top, centerX, rect.Top - (LabelDistance / 2));

        return (rect, rect.Contains(MouseLocationInfo.LastPosition));
    }

    private bool DrawMarker(RenderContext context, Point point, OrderDirections action, bool isEntry)
    {
        var shift = MarkerSize * 2; // Tamaño del marcador
        Point p1, p2, p3;

        // LÓGICA VISUAL:
        // Compra (Buy) = Flecha hacia ARRIBA (triángulo base abajo, punta arriba)
        // Venta (Sell) = Flecha hacia ABAJO (triángulo base arriba, punta abajo)

        bool pointUp = action == OrderDirections.Buy;

        if (pointUp)
        {
            // Punta arriba (Price location), Base abajo
            p1 = point;
            p2 = new Point(point.X - shift, point.Y + (shift * 2));
            p3 = new Point(point.X + shift, point.Y + (shift * 2));
        }
        else
        {
            // Punta abajo (Price location), Base arriba
            p1 = point;
            p2 = new Point(point.X - shift, point.Y - (shift * 2));
            p3 = new Point(point.X + shift, point.Y - (shift * 2));
        }

        // Color de la flecha: Verde para compras, Rojo para ventas (Independiente del PnL)
        Color arrowColor = action == OrderDirections.Buy ? _buyColor : _sellColor;

        // Si es salida, podemos oscurecerlo un poco o cambiar el borde, pero el color base ayuda a identificar la acción.
        // Opcional: Si es salida, usar el color del PnL? 
        // Para simplicidad didáctica: Acción pura. Verde=Compré, Rojo=Vendí.

        var points = new Point[] { p1, p2, p3 };
        context.FillPolygon(arrowColor, points);
        context.DrawPolygon(_borderPen, points);

        return IsPointInTriangle(MouseLocationInfo.LastPosition, p1, p2, p3);
    }

    private void DrawTooltips(RenderContext context)
    {
        if (!_tooltipTrades.Any()) return;
        var y = MouseLocationInfo.LastPosition.Y + 20;
        var x = MouseLocationInfo.LastPosition.X + 20;

        foreach (var trade in _tooltipTrades)
        {
            // Usamos el render original del tooltip pero con la fuente nueva
            DrawTooltipBox(context, trade, x, ref y);
            y += 5;
        }
        _tooltipTrades.Clear();
    }

    private void DrawTooltipBox(RenderContext context, TradeObj trade, int x, ref int y)
    {
        // Tooltip flotante (solo al pasar el ratón)
        var bg = Color.FromArgb(230, 40, 40, 40); // Gris oscuro casi negro
        var fg = Color.White;

        var openTime = ToChartTime(trade.OpenTime);
        var closeTime = ToChartTime(trade.CloseTime);
        var dirStr = trade.Direction == OrderDirections.Buy ? "LONG" : "SHORT";

        var text = $"{dirStr} {trade.Volume} | {trade.Security}\n" +
                   $"In: {ChartInfo.GetPriceString(trade.OpenPrice)} @ {openTime:HH:mm:ss}\n" +
                   $"Out: {ChartInfo.GetPriceString(trade.ClosePrice)} @ {closeTime:HH:mm:ss}\n" +
                   $"PnL: {trade.PnL:N2} ({trade.PnLTicks} pts)";

        var size = context.MeasureString(text, _fontTooltip);
        var rect = new Rectangle(x, y, size.Width + 10, size.Height + 10);

        // Ajuste si se sale de pantalla
        if (rect.Right > ChartInfo.PriceChartContainer.Region.Width) rect.X -= (rect.Width + 40);

        context.FillRectangle(bg, rect);
        context.DrawRectangle(new Pen(Color.Gray, 1), rect);
        context.DrawString(text, _fontTooltip, fg, new Rectangle(rect.X + 5, rect.Y + 5, rect.Width, rect.Height));

        y += rect.Height;
    }

    #endregion

    #endregion

    #region Private Methods & Helpers

    private DateTime ToChartTime(DateTime tradeTime)
    {
        return tradeTime.AddHours(ManualTimeOffset);
    }

    private int GetBarByDate(DateTime time)
    {
        if (CurrentBar <= 0) return -1;
        if (time <= GetCandle(0).Time) return 0;
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

    private Pen GetNewPen(Color color, float lineWidth, DashStyle lineStyle) => new Pen(color, lineWidth) { DashStyle = lineStyle };
    private bool IsPointInTriangle(Point p, Point p0, Point p1, Point p2) => Math.Abs(TriangleArea(p0, p1, p2) - (TriangleArea(p, p0, p1) + TriangleArea(p, p1, p2) + TriangleArea(p, p2, p0))) < 1.0;
    private double TriangleArea(Point p0, Point p1, Point p2) => Math.Abs((p0.X * (p1.Y - p2.Y) + p1.X * (p2.Y - p0.Y) + p2.X * (p0.Y - p1.Y)) / 2.0);

    private void DrawDebugOverlay(RenderContext context)
    {
        var text = $"V6: Hist={_historyTradesCount} | RealTime={_recentTradesCount} | Offset={ManualTimeOffset}h";
        var rect = new Rectangle(10, 10, 250, 20);
        context.FillRectangle(Color.FromArgb(100, 0, 0, 0), rect);
        context.DrawString(text, _fontTooltip, Color.White, rect, new RenderStringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
    }

    #endregion

    #region MYTRADE MATCHING ENGINE (FIFO - Corrected)

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

        if (allMyTrades.Count != _recentTradesCount)
        {
            var sortedTrades = allMyTrades.OrderBy(t => t.Time).ToList();
            _openLots.Clear();
            _tradesSynthetic.Clear();

            foreach (var mt in sortedTrades)
            {
                if (TradingManager.Portfolio != null && TradingManager.Security != null &&
                    mt.AccountID == TradingManager.Portfolio.AccountID &&
                    mt.Security.Code == TradingManager.Security.Code)
                {
                    ProcessSingleMyTrade(mt);
                }
            }
            _recentTradesCount = allMyTrades.Count;
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
            _openLots.Add(new OpenLot { Dir = dir, Price = price, Time = time, RemainingVolume = volumeToClose });
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
