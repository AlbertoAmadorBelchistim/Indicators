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
            Color = MediaColor.FromArgb(0, 0, 0, 0), // Transparente total
            VisualType = VisualMode.Hide, // <--- CAMBIO: Oculto para que no pinte nada automático
            ScaleIt = false, // <--- CAMBIO CRÍTICO: Evita que ATAS deforme la escala
            ShowZeroValue = false
        };

        private readonly ValueDataSeries _strengthSeries = new("DomStrength", "Trade Strength (Aggressive)")
        {
            Color = MediaColor.FromArgb(0, 0, 0, 0),
            VisualType = VisualMode.Hide, // <--- CAMBIO
            ScaleIt = false, // <--- CAMBIO CRÍTICO
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
        public int DomDepthLimit { get; set; } = 20;

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

            DataSeries[0].IsHidden = true;
            DataSeries[1].IsHidden = true;

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

            // 1. COORDENADAS DEL PANEL (SISTEMA MANUAL)
            // Container.Region nos da el rectángulo exacto de este indicador
            int h = Container.Region.Height;
            int y = Container.Region.Y;
            int w = Container.Region.Width;

            // El CERO siempre en el centro del panel
            int middleY = y + (h / 2);

            // 2. AUTO-ESCALA MANUAL
            // Buscamos el valor más alto visible para que quepa en el panel
            decimal maxVal = 1;
            for (int i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
            {
                maxVal = Math.Max(maxVal, Math.Abs(_powerSeries[i]));
                maxVal = Math.Max(maxVal, Math.Abs(_strengthSeries[i]));
            }

            // Factor: (Altura media - Margen) / Valor Máximo
            float scaleFactor = (h / 2f - 5) / (float)maxVal;

            // 3. DIBUJAR LÍNEA CERO
            context.DrawLine(new RenderPen(DrawingColor.Gray), 0, middleY, w, middleY);

            // 4. BUCLE DE DIBUJO
            for (int i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
            {
                int barX = ChartInfo.GetXByBar(i);
                int barW = (int)ChartInfo.PriceChartContainer.BarsWidth;

                // DIBUJAR POWER (Fondo)
                DrawBarManual(context, barX, barW, middleY, _powerSeries[i], scaleFactor, PowerWidth, PowerOpacity, false);

                // DIBUJAR STRENGTH (Frente)
                DrawBarManual(context, barX, barW, middleY, _strengthSeries[i], scaleFactor, StrengthWidth, StrengthOpacity, true);

                // DIBUJAR ABSORCIÓN
                if (Math.Sign(_powerSeries[i]) != Math.Sign(_strengthSeries[i])
                    && _powerSeries[i] != 0 && _strengthSeries[i] != 0)
                {
                    DrawAbsorptionMarker(context, barX, barW, middleY);
                }
            }
        }

        private void DrawBarManual(RenderContext ctx, int x, int w, int yZero, decimal val, float scale, int widthPct, int alpha, bool isFront)
        {
            if (val == 0) return;

            // Altura en píxeles = Valor * Escala
            int heightPx = (int)(Math.Abs((float)val) * scale);
            if (heightPx < 1) heightPx = 1;

            // Si es positivo (>0), dibujamos hacia arriba (Y - altura)
            // Si es negativo (<0), dibujamos hacia abajo (Y)
            int yPos = val > 0 ? yZero - heightPx : yZero;

            int drawW = Math.Max(1, (w * widthPct) / 100);
            int drawX = x + (w - drawW) / 2;

            DrawingColor c = val > 0 ? BuyColor : SellColor;
            DrawingColor finalColor = DrawingColor.FromArgb(alpha, c);

            var rect = new Rectangle(drawX, yPos, drawW, heightPx);
            ctx.FillRectangle(finalColor, rect);

            if (isFront)
                ctx.DrawRectangle(new RenderPen(DrawingColor.FromArgb(150, DrawingColor.White)), rect);
        }

        private void DrawAbsorptionMarker(RenderContext ctx, int x, int w, int y0)
        {
            int size = 4;
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