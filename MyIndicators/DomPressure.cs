using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using ATAS.Indicators;
using ATAS.Indicators.Technical;
using OFT.Attributes;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;

// Alias para evitar colisiones
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace MyIndicators
{
    [Category("Order Flow")]
    [DisplayName("DOM Pressure")]
    [Description("Indicador Híbrido: Combina la presión pasiva (DOM Power) con la agresión real (Delta) para detectar absorción.")]
    public class DomPressure : Indicator
    {
        #region Series de Datos (UI y Escala)

        private readonly ValueDataSeries _powerSeries = new("DomPower", "DOM Power (Passive)")
        {
            // FIX COLOR: Usamos el tipo correcto para WPF
            Color = System.Windows.Media.Colors.Transparent,
            VisualType = VisualMode.Histogram,
            ScaleIt = true,
            ShowZeroValue = false
        };

        private readonly ValueDataSeries _strengthSeries = new("DomStrength", "Trade Strength (Aggressive)")
        {
            Color = System.Windows.Media.Colors.Transparent,
            VisualType = VisualMode.Histogram,
            ScaleIt = true,
            ShowZeroValue = false
        };

        #endregion

        #region Fields Internos

        private readonly SortedList<decimal, decimal> _mDepthAsk = new();
        private readonly SortedList<decimal, decimal> _mDepthBid = new();
        private readonly object _locker = new();

        private decimal _barBuyVolume;
        private decimal _barSellVolume;
        private int _lastBar = -1;

        #endregion

        #region Parámetros

        #region 1. Calculation

        [Display(Name = "DOM Depth Limit", GroupName = "1. Calculation", Order = 10, Description = "Niveles del DOM a sumar. 0 = Todo.")]
        [Range(0, 1000)]
        public int DomDepthLimit { get; set; } = 0;

        #endregion

        #region 2. Visuals

        [Display(Name = "Power Width %", GroupName = "2. Visuals", Order = 30)]
        [Range(10, 100)]
        public int PowerWidth { get; set; } = 80;

        [Display(Name = "Strength Width %", GroupName = "2. Visuals", Order = 40)]
        [Range(5, 90)]
        public int StrengthWidth { get; set; } = 25;

        [Display(Name = "Power Opacity", GroupName = "2. Visuals", Order = 50)]
        [Range(10, 255)]
        public int PowerOpacity { get; set; } = 100;

        [Display(Name = "Strength Opacity", GroupName = "2. Visuals", Order = 60)]
        [Range(10, 255)]
        public int StrengthOpacity { get; set; } = 255;

        #endregion

        #region 3. Colors

        // Usamos DrawingColor para los parámetros de dibujo manual (GDI+)
        [Display(Name = "Buy Color", GroupName = "3. Colors", Order = 70)]
        public DrawingColor BuyColor { get; set; } = DrawingColor.LimeGreen;

        [Display(Name = "Sell Color", GroupName = "3. Colors", Order = 80)]
        public DrawingColor SellColor { get; set; } = DrawingColor.Red;

        [Display(Name = "Absorption Marker", GroupName = "3. Colors", Order = 90)]
        public DrawingColor AbsorptionColor { get; set; } = DrawingColor.Yellow;

        #endregion

        #endregion

        public DomPressure() : base(true)
        {
            Panel = IndicatorDataProvider.NewPanel;
            DenyToChangePanel = true;

            DataSeries[0] = _powerSeries;
            DataSeries.Add(_strengthSeries);

            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);
        }

        #region Core Logic

        protected override void OnInitialize()
        {
            RequestForCumulativeTrades(new CumulativeTradesRequest(GetCandle(Math.Max(0, CurrentBar - 300)).Time));
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            if (bar == 0)
            {
                _mDepthAsk.Clear();
                _mDepthBid.Clear();
                var snapshot = MarketDepthInfo.GetMarketDepthSnapshot();
                foreach (var d in snapshot) ProcessDepthUpdate(d);
            }

            if (bar != _lastBar)
            {
                _barBuyVolume = 0;
                _barSellVolume = 0;
                _lastBar = bar;
            }
        }

        protected override void MarketDepthChanged(MarketDataArg depth)
        {
            ProcessDepthUpdate(depth);
            CalculateDomPower();
        }

        protected override void OnNewTrade(MarketDataArg trade)
        {
            if (trade.Direction == TradeDirection.Buy) _barBuyVolume += trade.Volume;
            else if (trade.Direction == TradeDirection.Sell) _barSellVolume += trade.Volume;

            decimal delta = _barBuyVolume - _barSellVolume;
            _strengthSeries[CurrentBar - 1] = delta;
        }

        protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            var trades = cumulativeTrades.ToList();
            if (trades.Count == 0) return;

            int currentProcessingBar = 0;
            foreach (var tradeBar in trades)
            {
                while (currentProcessingBar < CurrentBar && GetCandle(currentProcessingBar).LastTime < tradeBar.Time)
                    currentProcessingBar++;

                if (currentProcessingBar >= CurrentBar) break;

                decimal delta = 0;
                foreach (var tick in tradeBar.Ticks)
                {
                    if (tick.Direction == TradeDirection.Buy) delta += tick.Volume;
                    else if (tick.Direction == TradeDirection.Sell) delta -= tick.Volume;
                }
                _strengthSeries[currentProcessingBar] += delta;
            }
        }

        #endregion

        #region Helpers

        private void ProcessDepthUpdate(MarketDataArg depth)
        {
            lock (_locker)
            {
                var list = depth.DataType == MarketDataType.Ask ? _mDepthAsk : _mDepthBid;
                if (depth.Volume == 0) list.Remove(depth.Price);
                else list[depth.Price] = depth.Volume;
            }
        }

        private void CalculateDomPower()
        {
            decimal asksSum = 0;
            decimal bidsSum = 0;

            lock (_locker)
            {
                if (DomDepthLimit <= 0 || _mDepthAsk.Count <= DomDepthLimit)
                    asksSum = _mDepthAsk.Values.Sum();
                else
                    for (int i = 0; i < DomDepthLimit; i++) asksSum += _mDepthAsk.Values[i];

                if (DomDepthLimit <= 0 || _mDepthBid.Count <= DomDepthLimit)
                    bidsSum = _mDepthBid.Values.Sum();
                else
                {
                    var count = _mDepthBid.Count;
                    for (int i = 0; i < DomDepthLimit; i++) bidsSum += _mDepthBid.Values[count - 1 - i];
                }
            }
            _powerSeries[CurrentBar - 1] = bidsSum - asksSum;
        }

        #endregion

        #region Rendering (FIX MANUAL SCALING)

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (ChartInfo == null || Container == null) return;

            // 1. Calculamos el rango visible para Auto-Escala Manual
            decimal maxAbsValue = 1; // Evitar div/0
            for (int i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
            {
                maxAbsValue = Math.Max(maxAbsValue, Math.Abs(_powerSeries[i]));
                maxAbsValue = Math.Max(maxAbsValue, Math.Abs(_strengthSeries[i]));
            }

            // 2. Definimos el centro y el factor de escala (Pixeles por Unidad de Volumen)
            // Dejamos un margen del 5% arriba y abajo
            float panelHeight = Container.Region.Height;
            float zeroY = Container.Region.Y + (panelHeight / 2f);
            float availableHeight = (panelHeight / 2f) * 0.95f;
            float pixelsPerUnit = availableHeight / (float)maxAbsValue;

            // 3. Dibujamos la línea cero
            context.DrawLine(new RenderPen(DrawingColor.Gray), Container.Region.X, (int)zeroY, Container.Region.Right, (int)zeroY);

            // 4. Dibujamos las barras
            for (int i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
            {
                int x = ChartInfo.GetXByBar(i);
                int w = (int)ChartInfo.PriceChartContainer.BarsWidth;

                // POWER (Fondo)
                decimal powerVal = _powerSeries[i];
                DrawBarManual(context, x, w, zeroY, powerVal, pixelsPerUnit, PowerWidth, PowerOpacity, false);

                // STRENGTH (Frente)
                decimal strengthVal = _strengthSeries[i];
                DrawBarManual(context, x, w, zeroY, strengthVal, pixelsPerUnit, StrengthWidth, StrengthOpacity, true);

                // ABSORPTION
                if (Math.Sign(powerVal) != Math.Sign(strengthVal) && powerVal != 0 && strengthVal != 0)
                {
                    DrawAbsorptionMarker(context, x, w, (int)zeroY);
                }
            }
        }

        private void DrawBarManual(RenderContext ctx, int x, int w, float zeroY, decimal val, float scale, int widthPct, int alpha, bool isFront)
        {
            if (val == 0) return;

            float height = Math.Abs((float)val) * scale;
            // Si es positivo, subimos (restamos Y). Si es negativo, bajamos (sumamos Y).
            float topY = val > 0 ? (zeroY - height) : zeroY;

            int drawW = Math.Max(1, (w * widthPct) / 100);
            int drawX = x + (w - drawW) / 2;

            // Asegurar rectángulo válido
            if (height < 1) height = 1;

            DrawingColor c = val > 0 ? BuyColor : SellColor;
            DrawingColor finalColor = DrawingColor.FromArgb(alpha, c);

            var rect = new Rectangle(drawX, (int)topY, drawW, (int)height);
            ctx.FillRectangle(finalColor, rect);

            if (isFront)
            {
                ctx.DrawRectangle(new RenderPen(DrawingColor.FromArgb(150, DrawingColor.White)), rect);
            }
        }

        private void DrawAbsorptionMarker(RenderContext ctx, int x, int w, int y0)
        {
            int size = 5;
            int centerX = x + w / 2;

            Point[] diamond = {
                new Point(centerX, y0 - size),
                new Point(centerX + size, y0),
                new Point(centerX, y0 + size),
                new Point(centerX - size, y0)
            };

            ctx.FillPolygon(AbsorptionColor, diamond);
        }

        #endregion
    }
}