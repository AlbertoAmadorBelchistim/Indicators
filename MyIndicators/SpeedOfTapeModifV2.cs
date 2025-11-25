using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical;
using OFT.Attributes;
using OFT.Rendering.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace MyIndicators
{
    [DisplayName("Speed of Tape Modif V2")]
    [Category("Order Flow")]
    [Description("Mide la velocidad real de la cinta (High Water Mark) y marca la zona de precio de la aceleración.")]
    [HelpLink("https://support.atas.net/")]
    public class SpeedOfTapeModif_V2 : Indicator
    {
        #region Enums & Structs

        public enum SpeedType
        {
            [Display(Name = "Ticks (HFT)")] Ticks,
            [Display(Name = "Volume (Blocks)")] Volume,
            [Display(Name = "Delta (Aggression)")] Delta,
            [Display(Name = "Buy Volume")] Buys,
            [Display(Name = "Sell Volume")] Sells
        }

        private struct TickSnapshot
        {
            public DateTime Time;
            public decimal Volume;
            public int Direction; // 1 Buy, -1 Sell
            public decimal Price;
        }

        private struct SpeedState
        {
            public decimal Speed;
            public decimal Context;
            public decimal TotalVolume;
            public decimal HighRange;
            public decimal LowRange;
        }

        #endregion

        #region Fields

        // Panel Inferior (Histograma)
        private readonly ValueDataSeries _renderSeries = new("Speed Histogram")
        {
            VisualType = VisualMode.Histogram,
            ShowZeroValue = false,
            UseMinimizedModeIfEnabled = true,
            ResetAlertsOnNewBar = true
        };

        private readonly ValueDataSeries _thresholdSeries = new("Filter Line")
        {
            VisualType = VisualMode.Line,
            Color = System.Drawing.Color.Aqua.Convert(),
            Width = 2
        };

        // Panel Principal (Semáforo de Zonas)
        private readonly RangeDataSeries _rangeBuy = new("Zone Buy")
        {
            DrawAbovePrice = true,
            RangeColor = System.Drawing.Color.FromArgb(100, 0, 255, 0).Convert(),
            IsHidden = false
        };

        private readonly RangeDataSeries _rangeSell = new("Zone Sell")
        {
            DrawAbovePrice = true,
            RangeColor = System.Drawing.Color.FromArgb(100, 255, 0, 0).Convert(),
            IsHidden = false
        };

        private readonly RangeDataSeries _rangeNeutral = new("Zone Neutral")
        {
            DrawAbovePrice = true,
            RangeColor = System.Drawing.Color.FromArgb(100, 128, 128, 128).Convert(),
            IsHidden = false
        };

        // Motor
        private readonly Queue<TickSnapshot> _tickQueue = new();
        private readonly Queue<decimal> _historyMaxSpeeds = new(); // Cola para guardar los picos históricos
        private decimal _currentThreshold = 100; // Valor actual del filtro

        // Estado
        private int _lastBar = -1;
        private bool _historyLoaded = false;
        private SpeedState _currentBarMaxState;
        private int _lastAlertBar = -1;

        // Settings
        private int _timeWindow = 5;
        private int _filterPeriod = 10;
        private bool _autoFilter = true;
        private decimal _manualThreshold = 100;
        private System.Drawing.Color _buyColor = System.Drawing.Color.Lime;
        private System.Drawing.Color _sellColor = System.Drawing.Color.Red;
        private System.Drawing.Color _neutralColor = System.Drawing.Color.Gray;

        // Caché de historial para cambios rápidos de parámetros
        private List<CumulativeTrade> _cachedTrades = new List<CumulativeTrade>();

        #endregion

        #region Parameters

        [Display(Name = "Time Window (sec)", GroupName = "Calculation", Order = 10)]
        [Range(1, 600)]
        public int TimeWindow
        {
            get => _timeWindow;
            set { _timeWindow = value; RecalculateValues(); }
        }

        [Display(Name = "Data Type", GroupName = "Calculation", Order = 20)]
        public SpeedType DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;
                // Al cambiar el tipo, no necesitamos descargar datos, solo reprocesar lo que tenemos en memoria
                if (_historyLoaded) RebuildHistoryFromCache();
                RecalculateValues();
            }
        }
        private SpeedType _dataType = SpeedType.Ticks;

        [Display(Name = "Use AutoFilter", GroupName = "Filter", Order = 40)]
        public bool UseAutoFilter
        {
            get => _autoFilter;
            set { _autoFilter = value; RecalculateValues(); }
        }

        [Display(Name = "AutoFilter Period", GroupName = "Filter", Order = 50)]
        [Range(1, 1000)]
        public int FilterPeriod
        {
            get => _filterPeriod;
            set
            {
                _filterPeriod = value;
                RecalculateValues();
            }
        }

        [Display(Name = "Manual Threshold", GroupName = "Filter", Order = 60)]
        public decimal ManualThreshold
        {
            get => _manualThreshold;
            set { _manualThreshold = value; RecalculateValues(); }
        }

        [Display(Name = "Show Price Marker", GroupName = "Visuals", Order = 65)]
        public bool ShowPriceMarker
        {
            get => _showPriceMarker;
            set { _showPriceMarker = value; RedrawChart(); } // Solo Redraw
        }
        private bool _showPriceMarker = true;

        [Display(Name = "Use Smart Colors", GroupName = "Visuals", Order = 70)]
        public bool UseSmartColors
        {
            get => _useSmartColors;
            set { _useSmartColors = value; RedrawChart(); } // Solo Redraw
        }
        private bool _useSmartColors = true;

        [Display(Name = "Buy Color", GroupName = "Visuals", Order = 80)]
        public CrossColor BuyColor
        {
            get => _buyColor.Convert();
            set { _buyColor = value.Convert(); RedrawChart(); }
        }

        [Display(Name = "Sell Color", GroupName = "Visuals", Order = 90)]
        public CrossColor SellColor
        {
            get => _sellColor.Convert();
            set { _sellColor = value.Convert(); RedrawChart(); }
        }

        [Display(Name = "Neutral Color", GroupName = "Visuals", Order = 100)]
        public CrossColor NeutralColor
        {
            get => _neutralColor.Convert();
            set { _neutralColor = value.Convert(); RedrawChart(); }
        }

        [Display(Name = "Use Alerts", GroupName = "Alerts", Order = 110)]
        public bool UseAlerts { get; set; }

        [Display(Name = "Alert File", GroupName = "Alerts", Order = 120)]
        public string AlertFile { get; set; } = "alert1";

        #endregion

        public SpeedOfTapeModif_V2() : base(true)
        {
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            Panel = IndicatorDataProvider.NewPanel;

            // 2. OCULTAR LAS SERIES DE RANGO (Las usaremos solo para guardar datos)
            _rangeBuy.IsHidden = true;
            _rangeSell.IsHidden = true;
            _rangeNeutral.IsHidden = true;

            // Aseguramos que la serie del filtro sea visible en el panel inferior
            DataSeries[0] = _renderSeries;
            DataSeries.Add(_thresholdSeries);

            // Añadimos las series ocultas para que guarden los valores internamente
            DataSeries.Add(_rangeBuy);
            DataSeries.Add(_rangeSell);
            DataSeries.Add(_rangeNeutral);

        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // Gestión de estado inicial
            if (bar == 0 && !_historyLoaded)
            {
                DataSeries.ForEach(x => x.Clear());
                _tickQueue.Clear();
                _lastBar = -1;
            }
        }

        protected override void OnFinishRecalculate()
        {
            // PROTECCIÓN 1: No pedir datos si no hay barras suficientes
            if (CurrentBar < 2) return;

            var startTime = GetCandle(0).Time;

            // PROTECCIÓN 2: Pedir SOLO hasta la penúltima vela cerrada.
            // Esto evita conflictos con los datos en tiempo real que entran por OnNewTrade.
            // ClusterStatistic hace exactamente esto.
            var endTime = GetCandle(CurrentBar - 2).LastTime;

            // Petición acotada en el tiempo
            var request = new CumulativeTradesRequest(startTime, endTime, 0, 0);
            RequestForCumulativeTrades(request);
        }

        #region Core Logic

        private bool ProcessTick(DateTime time, decimal volume, int direction, decimal price, int barIndex)
        {
            // Reset de estado si cambiamos de barra
            if (barIndex != _lastBar)
            {
                _lastBar = barIndex;
                _currentBarMaxState = new SpeedState();

                // Limpiar las zonas visuales de la nueva barra
                _rangeBuy[barIndex].Upper = 0; _rangeBuy[barIndex].Lower = 0;
                _rangeSell[barIndex].Upper = 0; _rangeSell[barIndex].Lower = 0;
                _rangeNeutral[barIndex].Upper = 0; _rangeNeutral[barIndex].Lower = 0;
            }

            _tickQueue.Enqueue(new TickSnapshot { Time = time, Volume = volume, Direction = direction, Price = price });

            var cutoff = time.AddSeconds(-TimeWindow);
            while (_tickQueue.Count > 0 && _tickQueue.Peek().Time <= cutoff)
                _tickQueue.Dequeue();

            decimal winVol = 0;
            decimal winDelta = 0;
            decimal winBuys = 0;
            decimal winSells = 0;
            int winTicks = _tickQueue.Count;

            decimal winHigh = decimal.MinValue;
            decimal winLow = decimal.MaxValue;

            foreach (var t in _tickQueue)
            {
                winVol += t.Volume;
                var d = (t.Direction == 1 ? t.Volume : -t.Volume);
                winDelta += d;

                if (t.Direction == 1) winBuys += t.Volume;
                else winSells += t.Volume;

                // Rango de la ráfaga
                if (t.Price > winHigh) winHigh = t.Price;
                if (t.Price < winLow) winLow = t.Price;
            }

            if (winTicks == 0) { winHigh = price; winLow = price; }

            decimal currentSpeed = 0;
            decimal contextDelta = winDelta;

            switch (DataType)
            {
                case SpeedType.Ticks: currentSpeed = winTicks; break; // Aquí contamos "Eventos de Cinta", no contratos. Como procesamos CumulativeTrades, cada llamada es 1 evento.
                case SpeedType.Volume: currentSpeed = winVol; break;
                case SpeedType.Delta: currentSpeed = Math.Abs(winDelta); break;
                case SpeedType.Buys: currentSpeed = winBuys; contextDelta = winBuys; break;
                case SpeedType.Sells: currentSpeed = winSells; contextDelta = -winSells; break;
            }

            // 4. High Water Mark
            if (currentSpeed > _currentBarMaxState.Speed)
            {
                _currentBarMaxState = new SpeedState
                {
                    Speed = currentSpeed,
                    Context = contextDelta,
                    TotalVolume = winVol,
                    HighRange = winHigh,
                    LowRange = winLow
                };

                // Asignamos valor preliminar para que la SMA tenga datos
                _renderSeries[barIndex] = currentSpeed;
                return true;
            }

            return false;
        }

        private void UpdateVisuals(int bar)
        {
            var speed = _renderSeries[bar];

            // 1. Filtro
            // Usamos la variable pre-calculada o el manual
            var threshold = UseAutoFilter
                ? _currentThreshold
                : ManualThreshold;

            _thresholdSeries[bar] = threshold;

            // 2. Color
            var finalColor = CalculateSmartColor(_currentBarMaxState);
            _renderSeries.Colors[bar] = finalColor;

            // 3. Semáforo de Zonas
            _rangeBuy[bar].Upper = 0; _rangeBuy[bar].Lower = 0;
            _rangeSell[bar].Upper = 0; _rangeSell[bar].Lower = 0;
            _rangeNeutral[bar].Upper = 0; _rangeNeutral[bar].Lower = 0;

            if (ShowPriceMarker && speed > threshold)
            {
                decimal totalVol = _currentBarMaxState.TotalVolume;
                double efficiency = totalVol == 0 ? 0 : (double)(Math.Abs(_currentBarMaxState.Context) / totalVol);
                bool isBuy = _currentBarMaxState.Context > 0;

                if (UseSmartColors && efficiency < 0.3)
                {
                    _rangeNeutral[bar].Upper = _currentBarMaxState.HighRange;
                    _rangeNeutral[bar].Lower = _currentBarMaxState.LowRange;
                }
                else if (isBuy)
                {
                    _rangeBuy[bar].Upper = _currentBarMaxState.HighRange;
                    _rangeBuy[bar].Lower = _currentBarMaxState.LowRange;
                }
                else
                {
                    _rangeSell[bar].Upper = _currentBarMaxState.HighRange;
                    _rangeSell[bar].Lower = _currentBarMaxState.LowRange;
                }
            }

            // 4. Alertas
            if (UseAlerts && bar == CurrentBar - 1 && speed > threshold && _lastAlertBar != bar)
            {
                AddAlert(AlertFile, $"Speed Alert: {speed:0}");
                _lastAlertBar = bar;
            }
        }

        private System.Drawing.Color CalculateSmartColor(SpeedState state)
        {
            bool isBuy = state.Context > 0;
            var baseColor = isBuy ? BuyColor : SellColor;

            if (!UseSmartColors) return baseColor.Convert();
            if (state.TotalVolume == 0) return NeutralColor.Convert();

            double efficiency = (double)(Math.Abs(state.Context) / state.TotalVolume);
            return Blend(NeutralColor.Convert(), baseColor.Convert(), efficiency);
        }

        private System.Drawing.Color Blend(System.Drawing.Color background, System.Drawing.Color foreground, double ratio)
        {
            if (ratio > 1) ratio = 1;
            if (ratio < 0) ratio = 0;

            var r = (int)(background.R + (foreground.R - background.R) * ratio);
            var g = (int)(background.G + (foreground.G - background.G) * ratio);
            var b = (int)(background.B + (foreground.B - background.B) * ratio);

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        #endregion

        #region Event Handlers

        protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            // 1. Guardamos en caché para el futuro
            _cachedTrades = cumulativeTrades?.ToList() ?? new List<CumulativeTrade>();

            // 2. Procesamos
            RebuildHistoryFromCache();
        }

        private void RebuildHistoryFromCache()
        {
            if (_cachedTrades.Count == 0) return;

            // 1. Limpieza inicial
            _tickQueue.Clear();
            _historyMaxSpeeds.Clear();
            _currentThreshold = 100; // Valor inicial por defecto
            _lastBar = -1;
            DataSeries.ForEach(x => x.Clear());

            // 2. Proceso
            foreach (var trade in _cachedTrades)
            {
                // Buscar a qué barra pertenece el trade
                int tradeBar = -1;
                int searchStart = _lastBar < 0 ? 0 : _lastBar;

                // Búsqueda optimizada (asumiendo orden cronológico)
                for (int i = searchStart; i < CurrentBar; i++)
                {
                    var c = GetCandle(i);
                    if (trade.Time >= c.Time && trade.Time <= c.LastTime)
                    {
                        tradeBar = i;
                        break;
                    }
                }

                if (tradeBar < 0) continue;

                // -----------------------------------------------------------
                // CAMBIO DE VELA: FINALIZAR ANTERIOR E INICIAR NUEVA
                // -----------------------------------------------------------
                if (tradeBar != _lastBar)
                {
                    // SI HABÍA UNA BARRA PREVIA, "CERRAMOS" SU VISUALIZACIÓN
                    if (_lastBar >= 0)
                    {
                        // A) PINTAR LA LÍNEA DEL FILTRO (Threshold usado en esa barra)
                        _thresholdSeries[_lastBar] = _currentThreshold;

                        // B) PINTAR EL COLOR DE LA BARRA (Histograma)
                        // Usamos el estado final antes de borrarlo
                        _renderSeries.Colors[_lastBar] = CalculateSmartColor(_currentBarMaxState);

                        // C) DIBUJAR LAS ZONAS DE PRECIO (Rectángulos)
                        // Solo si superó el filtro que estaba vigente
                        if (_currentBarMaxState.Speed > _currentThreshold && ShowPriceMarker)
                        {
                            bool isBuy = _currentBarMaxState.Context > 0;
                            decimal high = _currentBarMaxState.HighRange;
                            decimal low = _currentBarMaxState.LowRange;

                            // Lógica copiada de UpdateVisuals para zonas
                            if (UseSmartColors && _currentBarMaxState.TotalVolume > 0 &&
                                (Math.Abs(_currentBarMaxState.Context) / _currentBarMaxState.TotalVolume) < 0.3m)
                            {
                                _rangeNeutral[_lastBar].Upper = high;
                                _rangeNeutral[_lastBar].Lower = low;
                            }
                            else if (isBuy)
                            {
                                _rangeBuy[_lastBar].Upper = high;
                                _rangeBuy[_lastBar].Lower = low;
                            }
                            else
                            {
                                _rangeSell[_lastBar].Upper = high;
                                _rangeSell[_lastBar].Lower = low;
                            }
                        }

                        // D) CALCULAR EL NUEVO PROMEDIO PARA EL FUTURO
                        // Añadimos el pico de la barra cerrada a la historia
                        _historyMaxSpeeds.Enqueue(_currentBarMaxState.Speed);

                        while (_historyMaxSpeeds.Count > FilterPeriod)
                            _historyMaxSpeeds.Dequeue();

                        // Recalculamos el umbral que se usará para la SIGUIENTE barra
                        if (_historyMaxSpeeds.Count > 0)
                            _currentThreshold = _historyMaxSpeeds.Average();
                    }

                    // RESET DE ESTADO (Ahora sí es seguro borrar)
                    _lastBar = tradeBar;
                    _currentBarMaxState = new SpeedState();

                    // Asignamos el valor inicial visual de la nueva barra
                    _renderSeries[tradeBar] = 0;
                    _thresholdSeries[tradeBar] = _currentThreshold; // Pintar inicio de línea
                }

                // -----------------------------------------------------------
                // LÓGICA DE COLA (Sliding Window) - IGUAL QUE ANTES
                // -----------------------------------------------------------
                _tickQueue.Enqueue(new TickSnapshot
                {
                    Time = trade.Time,
                    Volume = trade.Volume,
                    Direction = trade.Direction == TradeDirection.Buy ? 1 : -1,
                    Price = trade.FirstPrice // FirstPrice es más seguro en histórico
                });

                var cutoff = trade.Time.AddSeconds(-TimeWindow);
                while (_tickQueue.Count > 0 && _tickQueue.Peek().Time <= cutoff)
                    _tickQueue.Dequeue();

                // -----------------------------------------------------------
                // CÁLCULOS
                // -----------------------------------------------------------
                decimal winVol = 0;
                decimal winDelta = 0;
                decimal winBuys = 0;
                decimal winSells = 0;
                int winTicks = _tickQueue.Count;

                decimal winHigh = decimal.MinValue;
                decimal winLow = decimal.MaxValue;

                foreach (var t in _tickQueue)
                {
                    winVol += t.Volume;
                    var d = (t.Direction == 1 ? t.Volume : -t.Volume);
                    winDelta += d;
                    if (t.Direction == 1) winBuys += t.Volume; else winSells += t.Volume;

                    if (t.Price > winHigh) winHigh = t.Price;
                    if (t.Price < winLow) winLow = t.Price;
                }

                if (winTicks == 0) { winHigh = trade.FirstPrice; winLow = trade.FirstPrice; }

                decimal currentSpeed = 0;
                decimal contextDelta = winDelta;

                switch (DataType)
                {
                    case SpeedType.Ticks: currentSpeed = winTicks; break;
                    case SpeedType.Volume: currentSpeed = winVol; break;
                    case SpeedType.Delta: currentSpeed = Math.Abs(winDelta); break;
                    case SpeedType.Buys: currentSpeed = winBuys; contextDelta = winBuys; break;
                    case SpeedType.Sells: currentSpeed = winSells; contextDelta = -winSells; break;
                }

                // High Water Mark
                if (currentSpeed > _currentBarMaxState.Speed)
                {
                    _currentBarMaxState = new SpeedState
                    {
                        Speed = currentSpeed,
                        Context = contextDelta,
                        TotalVolume = winVol,
                        HighRange = winHigh,
                        LowRange = winLow
                    };
                }

                // Asignación continua para que la barra crezca visualmente si redibujamos
                _renderSeries[tradeBar] = _currentBarMaxState.Speed;
            }

            // 3. PINTAR LA ÚLTIMA BARRA PROCESADA
            // (El bucle termina y la última barra queda sin "cerrar" visualmente en el bloque if)
            if (_lastBar >= 0)
            {
                _thresholdSeries[_lastBar] = _currentThreshold;
                _renderSeries.Colors[_lastBar] = CalculateSmartColor(_currentBarMaxState);

                if (_currentBarMaxState.Speed > _currentThreshold && ShowPriceMarker)
                {
                    // Aplicar lógica de zonas una última vez para la barra actual/final
                    bool isBuy = _currentBarMaxState.Context > 0;
                    decimal high = _currentBarMaxState.HighRange;
                    decimal low = _currentBarMaxState.LowRange;

                    if (UseSmartColors && _currentBarMaxState.TotalVolume > 0 &&
                       (Math.Abs(_currentBarMaxState.Context) / _currentBarMaxState.TotalVolume) < 0.3m)
                    {
                        _rangeNeutral[_lastBar].Upper = high; _rangeNeutral[_lastBar].Lower = low;
                    }
                    else if (isBuy)
                    {
                        _rangeBuy[_lastBar].Upper = high; _rangeBuy[_lastBar].Lower = low;
                    }
                    else
                    {
                        _rangeSell[_lastBar].Upper = high; _rangeSell[_lastBar].Lower = low;
                    }
                }
            }

            // Forzamos repintado del chart completo
            RedrawChart();
        }

        // Definimos la cola como variable de clase, pero la trataremos solo para RT
        private readonly Queue<TickSnapshot> _rtWindow = new();
        private int _rtBar = -1; // Control específico para RT

        protected override void OnNewTrade(MarketDataArg trade)
        {
            int bar = CurrentBar - 1;
            if (bar < 0) return;

            // ------------------------------------------------------------------
            // PASO 1: GESTIÓN DE DATOS (CONTINUA)
            // ------------------------------------------------------------------
            // Añadimos el dato siempre, sin importar si la vela es nueva o vieja.
            // Usamos DateTime.UtcNow para que el RT sea fluido.
            var now = DateTime.UtcNow;

            _tickQueue.Enqueue(new TickSnapshot
            {
                Time = now,
                Volume = trade.Volume,
                Direction = trade.Direction == TradeDirection.Buy ? 1 : -1,
                Price = trade.Price
            });

            // Limpiamos datos antiguos por TIEMPO.
            // Esto asegura que la cola cruce fronteras de velas suavemente.
            var cutoff = now.AddSeconds(-TimeWindow);
            while (_tickQueue.Count > 0 && _tickQueue.Peek().Time <= cutoff)
            {
                _tickQueue.Dequeue();
            }

            // ------------------------------------------------------------------
            // PASO 2: GESTIÓN DE CAMBIO DE VELA (RESET DE RÉCORD)
            // ------------------------------------------------------------------
            if (bar != _lastBar)
            {
                // A) LOGICA DE FILTRO (Hacer esto ANTES de resetear el estado)
                // Si la barra anterior (_lastBar) era válida, guardamos su velocidad máxima en el historial
                if (_lastBar >= 0)
                {
                    _historyMaxSpeeds.Enqueue(_currentBarMaxState.Speed);

                    // Mantenemos la cola del tamaño del periodo elegido
                    while (_historyMaxSpeeds.Count > FilterPeriod)
                        _historyMaxSpeeds.Dequeue();

                    // Calculamos el nuevo promedio (Threshold)
                    if (_historyMaxSpeeds.Count > 0)
                        _currentThreshold = _historyMaxSpeeds.Average();
                }

                // B) RESET DE VARIABLES
                _lastBar = bar;

                // Aquí está la clave: Reseteamos el "Campeón" de la vela a 0...
                _currentBarMaxState = new SpeedState();

                // Forzamos 0 visual al inicio (opcional, pero limpio)
                _renderSeries[bar] = 0;

                // ...PERO NO BORRAMOS _tickQueue. La velocidad actual se mantiene.
            }

            // ------------------------------------------------------------------
            // PASO 3: CÁLCULO DE LA VELOCIDAD ACTUAL
            // ------------------------------------------------------------------
            // Recorremos la cola para sumar valores (Volumen, Delta, etc.)
            decimal winVol = 0;
            decimal winDelta = 0;
            decimal winBuys = 0;
            decimal winSells = 0;
            int winTicks = _tickQueue.Count;

            // Variables para el rango de precio de la ráfaga actual
            decimal winHigh = decimal.MinValue;
            decimal winLow = decimal.MaxValue;

            foreach (var t in _tickQueue)
            {
                winVol += t.Volume;
                var d = (t.Direction == 1 ? t.Volume : -t.Volume);
                winDelta += d;

                if (t.Direction == 1) winBuys += t.Volume;
                else winSells += t.Volume;

                if (t.Price > winHigh) winHigh = t.Price;
                if (t.Price < winLow) winLow = t.Price;
            }

            // Protección por si la cola se vació (raro en RT activo, pero posible)
            if (winTicks == 0) { winHigh = trade.Price; winLow = trade.Price; }

            // Elegimos qué métrica estamos midiendo
            decimal currentSpeed = 0;
            decimal contextDelta = winDelta;

            switch (DataType)
            {
                case SpeedType.Ticks: currentSpeed = winTicks; break;
                case SpeedType.Volume: currentSpeed = winVol; break;
                case SpeedType.Delta: currentSpeed = Math.Abs(winDelta); break;
                case SpeedType.Buys: currentSpeed = winBuys; contextDelta = winBuys; break;
                case SpeedType.Sells: currentSpeed = winSells; contextDelta = -winSells; break;
            }

            // ------------------------------------------------------------------
            // PASO 4: HIGH WATER MARK (MÁXIMO DE LA VELA)
            // ------------------------------------------------------------------
            // Si acabamos de cambiar de vela (Paso 2), _currentBarMaxState.Speed es 0.
            // Por tanto, la currentSpeed actual (ej. 300) será inmediatamente el nuevo máximo.
            if (currentSpeed > _currentBarMaxState.Speed)
            {
                _currentBarMaxState = new SpeedState
                {
                    Speed = currentSpeed,
                    Context = contextDelta,
                    TotalVolume = winVol,
                    HighRange = winHigh,
                    LowRange = winLow
                };
            }

            // ------------------------------------------------------------------
            // PASO 5: ACTUALIZACIÓN VISUAL
            // ------------------------------------------------------------------
            // Asignamos siempre para que el indicador esté vivo
            _renderSeries[bar] = _currentBarMaxState.Speed;

            // Recalculamos colores y filtros
            UpdateVisuals(bar);
        
        }

        #endregion

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (layout != DrawingLayouts.Final) return;
            if (ChartInfo == null || InstrumentInfo == null) return;

            // CORRECCIÓN 1: LIMITAR EL DIBUJO (CLIPPING)
            // Esto obliga a que los rectángulos solo existan dentro del cuadro del gráfico de precios.
            // Si salen de ahí (por arriba o abajo), se cortan y no manchan los ejes.
            context.SetClip(ChartInfo.PriceChartContainer.Region);

            for (int bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
            {
                // Pasamos las variables privadas de color (_buyColor, etc.)
                DrawZone(context, bar, _rangeBuy, _buyColor);
                DrawZone(context, bar, _rangeSell, _sellColor);
                DrawZone(context, bar, _rangeNeutral, _neutralColor);
            }

            // Restauramos el clip para no afectar a otros indicadores (buena práctica)
            context.ResetClip();
        }

        private void DrawZone(RenderContext context, int bar, RangeDataSeries series, System.Drawing.Color color)
        {
            if (series[bar].Upper == 0) return;

            decimal highPrice = series[bar].Upper;
            decimal lowPrice = series[bar].Lower;

            if (highPrice == lowPrice)
                highPrice += InstrumentInfo.TickSize;

            int y1 = ChartInfo.GetYByPrice(highPrice, false);
            int y2 = ChartInfo.GetYByPrice(lowPrice, false);

            int height = Math.Abs(y2 - y1);
            if (height < 1) height = 1;

            // --- CÁLCULO DE PRECISIÓN HORIZONTAL ---

            // 1. CORRECCIÓN: Añadimos (double) para convertir el decimal de ATAS
            double preciseWidth = (double)ChartInfo.PriceChartContainer.BarsWidth;

            // 2. Ancho redondeado
            int width = (int)Math.Round(preciseWidth);
            if (width < 1) width = 1;

            // 3. Centro
            int xCenter = ChartInfo.GetXByBar(bar, false);

            // 4. Cálculo del borde izquierdo
            int xLeft = (int)Math.Round(xCenter - (preciseWidth / 2.0));

            // ----------------------------------------

            var rect = new Rectangle(xLeft, Math.Min(y1, y2), width, height);

            // Dibujamos
            var finalColor = System.Drawing.Color.FromArgb(150, color.R, color.G, color.B);
            context.FillRectangle(finalColor, rect);
        }
    }
}
