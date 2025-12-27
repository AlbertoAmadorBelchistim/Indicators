namespace MyIndicators
{
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

    using DrawingColor = System.Drawing.Color;

    [Category("Order Flow")]
    [DisplayName("DOM Pressure")]
    [Description("Indicador Híbrido: Combina la presión pasiva (DOM Power) con la agresión real (Delta). Incluye filtro inteligente de absorción.")]
    public class DomPressure : Indicator
    {
        #region Fields

        private readonly ValueDataSeries _powerSeries = new("DomPower", "DOM Power (Passive)")
        {
            Color = CrossColor.FromArgb(0, 0, 0, 0),
            VisualType = VisualMode.Hide,
            ScaleIt = false,
            ShowZeroValue = false
        };

        private readonly ValueDataSeries _strengthSeries = new("DomStrength", "Trade Strength (Aggressive)")
        {
            Color = CrossColor.FromArgb(0, 0, 0, 0),
            VisualType = VisualMode.Hide,
            ScaleIt = false,
            ShowZeroValue = false
        };

        private readonly SortedList<decimal, decimal> _mDepthAsk = new();
        private readonly SortedList<decimal, decimal> _mDepthBid = new();
        private readonly object _locker = new();

        private decimal _barBuyVolume;
        private decimal _barSellVolume;
        private int _lastBar = -1;

        // Fuentes para etiquetas
        private readonly RenderFont _axisFont = new("Arial", 8);
        private readonly RenderStringFormat _axisFormat = new()
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Far
        };

        #endregion

        #region Parameters

        #region 1. Calculation

        [Display(Name = "DOM Depth Limit", GroupName = "1. Calculation", Order = 10, Description = "Niveles por lado (Bid/Ask) a sumar. 20 = 20 Bid + 20 Ask.")]
        [Range(0, 1000)]
        public int DomDepthLimit { get; set; } = 20;

        [Display(Name = "Absorption Threshold %", GroupName = "1. Calculation", Order = 20, Description = "Porcentaje mínimo de agresión respecto al muro para marcar absorción (Filtro de ruido).")]
        [Range(1, 100)]
        public int AbsorptionThreshold { get; set; } = 15;

        #endregion

        #region 2. Visuals

        [Display(Name = "Power Width %", GroupName = "2. Visuals", Order = 30)]
        [Range(10, 100)]
        public int PowerWidth { get; set; } = 95;

        [Display(Name = "Strength Width %", GroupName = "2. Visuals", Order = 40)]
        [Range(5, 90)]
        public int StrengthWidth { get; set; } = 30;

        [Display(Name = "Power Opacity", GroupName = "2. Visuals", Order = 50)]
        [Range(10, 255)]
        public int PowerOpacity { get; set; } = 60;

        [Display(Name = "Strength Opacity", GroupName = "2. Visuals", Order = 60)]
        [Range(10, 255)]
        public int StrengthOpacity { get; set; } = 255;

        #endregion

        #region 3. Colors

        [Display(Name = "Buy Color", GroupName = "3. Colors", Order = 70)]
        public DrawingColor BuyColor { get; set; } = DrawingColor.LimeGreen;

        [Display(Name = "Sell Color", GroupName = "3. Colors", Order = 80)]
        public DrawingColor SellColor { get; set; } = DrawingColor.Red;

        [Display(Name = "Absorption Marker", GroupName = "3. Colors", Order = 90)]
        public DrawingColor AbsorptionColor { get; set; } = DrawingColor.Yellow;

        [Display(Name = "Axis Color", GroupName = "3. Colors", Order = 100)]
        public DrawingColor AxisColor { get; set; } = DrawingColor.Gray;

        #endregion

        #endregion

        public DomPressure() : base(true)
        {
            Panel = IndicatorDataProvider.NewPanel;
            DenyToChangePanel = true;

            DataSeries[0] = _powerSeries;
            DataSeries.Add(_strengthSeries);

            // Forzar ocultación en UI
            _powerSeries.IsHidden = true;
            _strengthSeries.IsHidden = true;

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
            if (CurrentBar - 1 >= 0) RedrawChart();
        }

        protected override void OnNewTrade(MarketDataArg trade)
        {
            if (trade.Direction == TradeDirection.Buy) _barBuyVolume += trade.Volume;
            else if (trade.Direction == TradeDirection.Sell) _barSellVolume += trade.Volume;

            decimal delta = _barBuyVolume - _barSellVolume;
            _strengthSeries[CurrentBar - 1] = delta;

            RedrawChart();
        }

        protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            // FIX HISTÓRICO: Resetear serie antes de rellenar para evitar duplicados
            // Nota: No podemos limpiar _strengthSeries aquí fácilmente porque ATAS maneja el buffer,
            // pero sobreescribiremos los valores.

            var trades = cumulativeTrades.ToList();
            if (trades.Count == 0) return;

            // Diccionario temporal para acumular deltas por barra
            var deltaPorBarra = new Dictionary<int, decimal>();

            foreach (var tradeBar in trades)
            {
                // Buscar la barra exacta por tiempo (Más robusto)
                // Usamos GetCandle(i).Time <= tradeBar.Time < GetCandle(i+1).Time
                // Pero como CumulativeTrade ya agrupa por ticks, asumimos que pertenece a una vela.
                // Método eficiente: Buscar índice por fecha

                // Aproximación: Asumimos que tradeBar.Time coincide con el inicio de la vela o está dentro.
                // ATAS suele devolver CumulativeTrades alineados si la petición es correcta.
                // Si no, buscamos:

                for (int i = 0; i < CurrentBar; i++)
                {
                    var candle = GetCandle(i);
                    if (tradeBar.Time >= candle.Time && tradeBar.Time <= candle.LastTime)
                    {
                        decimal delta = 0;
                        foreach (var tick in tradeBar.Ticks)
                        {
                            if (tick.Direction == TradeDirection.Buy) delta += tick.Volume;
                            else if (tick.Direction == TradeDirection.Sell) delta -= tick.Volume;
                        }

                        if (!deltaPorBarra.ContainsKey(i)) deltaPorBarra[i] = 0;
                        deltaPorBarra[i] += delta;
                        break; // Trade asignado, siguiente
                    }
                }
            }

            // Volcar al DataSeries
            foreach (var kvp in deltaPorBarra)
            {
                _strengthSeries[kvp.Key] = kvp.Value;
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

        private string FormatK(decimal val) => Math.Abs(val) >= 1000 ? $"{val / 1000m:0.#}k" : $"{val:0}";

        #endregion

        #region Rendering (Escala Dual Independiente)

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (ChartInfo == null || Container == null) return;

            Rectangle region = Container.Region;
            context.SetClip(region);
            int middleY = region.Y + (region.Height / 2);

            // 1. AUTO-ESCALA SIMÉTRICA (Basada en lo visible)
            decimal maxPower = 0;   // Empezamos en 0 para evitar errores si no hay datos
            decimal maxStrength = 0;

            for (int i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
            {
                if (i >= _powerSeries.Count) continue; // Protección de índice

                // Buscamos el pico máximo absoluto (sea positivo o negativo) para mantener el 0 centrado
                maxPower = Math.Max(maxPower, Math.Abs(_powerSeries[i]));
                maxStrength = Math.Max(maxStrength, Math.Abs(_strengthSeries[i]));
            }

            // Evitar división por cero si el gráfico está plano
            if (maxPower == 0) maxPower = 1;
            if (maxStrength == 0) maxStrength = 1;

            // 2. FACTORES DE ESCALA (Con Margen del 10%)
            // Usamos el 90% de la "mitad" de la altura (del centro al borde)
            float availableHalfHeight = (region.Height / 2f) * 0.90f;

            float scalePower = availableHalfHeight / (float)maxPower;
            float scaleStrength = availableHalfHeight / (float)maxStrength;

            // 3. DIBUJAR REJILLA DE PARETO (Basada en Power/Contexto)
            // Dibujamos las líneas sobre la escala del POWER, que es el "Muro" de referencia.
            // Línea Cero
            context.DrawLine(new RenderPen(AxisColor, 1), region.X, middleY, region.Right, middleY);

            // Líneas Pareto (80% y 50%)
            DrawParetoLine(context, region, middleY, availableHalfHeight, 0.8f, true);  // +80%
            DrawParetoLine(context, region, middleY, availableHalfHeight, 0.5f, true);  // +50%
            DrawParetoLine(context, region, middleY, availableHalfHeight, 0.5f, false); // -50%
            DrawParetoLine(context, region, middleY, availableHalfHeight, 0.8f, false); // -80%

            // 4. BUCLE DE DIBUJO (Barras)
            for (int i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
            {
                if (i < 0 || i >= _powerSeries.Count) continue;

                int x = ChartInfo.GetXByBar(i);
                int w = (int)ChartInfo.PriceChartContainer.BarsWidth;

                // A) POWER (Fondo)
                DrawBarVisual(context, x, w, middleY, _powerSeries[i], scalePower,
                              PowerWidth, PowerOpacity, false, out _);

                // B) STRENGTH (Frente)
                DrawBarVisual(context, x, w, middleY, _strengthSeries[i], scaleStrength,
                              StrengthWidth, StrengthOpacity, true, out int strengthTipY);

                // C) ABSORCIÓN
                decimal pVal = _powerSeries[i];
                decimal sVal = _strengthSeries[i];

                // Ratio relativo: ¿Es la agresión al menos el 15% del muro?
                decimal ratio = Math.Abs(pVal) == 0 ? 0 : Math.Abs(sVal) / Math.Abs(pVal);
                decimal threshold = (decimal)AbsorptionThreshold / 100m;

                if (Math.Sign(pVal) != Math.Sign(sVal) && ratio > threshold)
                {
                    DrawAbsorptionMarker(context, x, w, strengthTipY);
                }
            }

            // 5. ETIQUETAS DE ESCALA (Dinámicas)
            DrawLabels(context, region, maxPower, maxStrength);
            context.ResetClip();
        }

        // Helper para dibujar líneas de rejilla punteadas
        private void DrawParetoLine(RenderContext ctx, Rectangle region, int zeroY, float maxPx, float percent, bool isUp)
        {
            int yOffset = (int)(maxPx * percent);
            int lineY = isUp ? zeroY - yOffset : zeroY + yOffset;

            // Usamos un color más tenue para el grid
            var pen = new RenderPen(DrawingColor.FromArgb(180, AxisColor), 1, System.Drawing.Drawing2D.DashStyle.Dot);
            ctx.DrawLine(pen, region.X, lineY, region.Right, lineY);
        }

        private void DrawBarVisual(RenderContext ctx, int x, int w, int yZero, decimal val, float scale, int widthPct, int alpha, bool isFront, out int tipY)
        {
            tipY = yZero;
            if (val == 0) return;

            int heightPx = (int)(Math.Abs((float)val) * scale);
            if (heightPx < 1) heightPx = 1;

            int rectY = val > 0 ? yZero - heightPx : yZero;
            tipY = val > 0 ? rectY : rectY + heightPx;

            int drawW = Math.Max(1, (w * widthPct) / 100);
            int drawX = x + (w - drawW) / 2;

            DrawingColor c = val > 0 ? BuyColor : SellColor;
            DrawingColor finalColor = DrawingColor.FromArgb(alpha, c);
            Rectangle rect = new Rectangle(drawX, rectY, drawW, heightPx);

            ctx.FillRectangle(finalColor, rect);

            if (isFront)
            {
                ctx.DrawRectangle(new RenderPen(DrawingColor.Black), rect);
            }
        }

        private void DrawAbsorptionMarker(RenderContext ctx, int x, int w, int yTip)
        {
            int size = 4;
            int centerX = x + w / 2;

            Point[] diamond = {
                new Point(centerX, yTip - size),
                new Point(centerX + size, yTip),
                new Point(centerX, yTip + size),
                new Point(centerX - size, yTip)
            };

            ctx.FillPolygon(AbsorptionColor, diamond);
            ctx.DrawPolygon(new RenderPen(DrawingColor.Black), diamond);
        }

        private void DrawLabels(RenderContext ctx, Rectangle region, decimal maxP, decimal maxS)
        {
            string text = $"Scale Max\nPower: {FormatK(maxP)}\nStrength: {FormatK(maxS)}";

            // Usamos la versión simple de DrawString que no limita el tamaño.
            // Lo colocamos 85 pixeles a la izquierda del borde derecho, y 5 pixeles hacia abajo desde el borde superior.
            ctx.DrawString(text, _axisFont, AxisColor, region.Right - 85, region.Y + 5);
        }

        #endregion
    }
}