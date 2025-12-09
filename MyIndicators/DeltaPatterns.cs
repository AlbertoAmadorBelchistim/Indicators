using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical;
using OFT.Attributes;
using OFT.Attributes.Editors;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace MyIndicators
{
    [DisplayName("Delta Patterns")]
    [Category("Order Flow")]
    internal class DeltaPatterns : Indicator
    {
        #region Settings

        // Backing fields para detectar cambios y recalcular
        private int _targetVolume = 10000;
        private bool _showChartSignals = true;
        private int _signalSize = 10;

        // Porcentajes (Basados en la lógica "200 de 2500 = 8%")
        private decimal _aggPercent = 8.0m;         // Antes 200 (8%)
        private decimal _aggClosePercent = 12.0m;   // % de cierre (ya era %)
        private decimal _domPercent = 8.0m;         // Antes 200 (8%)
        private decimal _domWickPercent = 0.2m;     // Antes 5 (0.2%)
        private decimal _divPercent = 10.0m;        // Antes 250 (10%)
        private decimal _revExtremePercent = 8.0m;  // Antes 200 (8%)
        private decimal _revClosePercent = 2.0m;    // Antes 50 (2%)
        private decimal _neuMaxPercent = 1.0m;      // Rango neutral
        private decimal _neuStrugglePercent = 12.0m;// Antes 300 (12%)

        private decimal _maxScaleAbs = 5000m;
        private decimal _scaleSoftFactor = 1.0m;

        // --- Configuración Principal ---

        [Display(GroupName = "Config", Name = "Target Volume", Order = 1, Description = "Volumen base para calcular los %")]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public int TargetVolume
        {
            get => _targetVolume;
            set => SetProperty(ref _targetVolume, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "Config", Name = "Show Signals on Chart", Order = 2)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public bool ShowChartSignals
        {
            get => _showChartSignals;
            set => SetProperty(ref _showChartSignals, value, () => RedrawChart());
        }

        [Display(GroupName = "Config", Name = "Signal Size", Order = 3)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public int SignalSize
        {
            get => _signalSize;
            set => SetProperty(ref _signalSize, value, () => RedrawChart());
        }

        // --- Lógica en Porcentajes (Relativo al TargetVolume) ---

        [Display(GroupName = "1. Aggressive", Name = "Min Delta %", Order = 10, Description = "Delta mínimo como % del Target Volume")]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal AggressivePercent
        {
            get => _aggPercent;
            set => SetProperty(ref _aggPercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "1. Aggressive", Name = "Bar Close Delta %", Order = 11, Description = "% del volumen que debe tener el cuerpo")]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal AggressiveClosePercent
        {
            get => _aggClosePercent;
            set => SetProperty(ref _aggClosePercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "2. Dominance", Name = "Min Delta %", Order = 20)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal DominancePercent
        {
            get => _domPercent;
            set => SetProperty(ref _domPercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "2. Dominance", Name = "Wick Tolerance %", Order = 21)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal DominanceWickPercent
        {
            get => _domWickPercent;
            set => SetProperty(ref _domWickPercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "3. Divergence", Name = "Min Delta %", Order = 30)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal DivPercent
        {
            get => _divPercent;
            set => SetProperty(ref _divPercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "4. Reversal", Name = "Extreme Min %", Order = 40)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal RevExtremePercent
        {
            get => _revExtremePercent;
            set => SetProperty(ref _revExtremePercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "4. Reversal", Name = "Close Min % (Opposite)", Order = 41)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal RevClosePercent
        {
            get => _revClosePercent;
            set => SetProperty(ref _revClosePercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "5. Neutral", Name = "Max Close %", Order = 50)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal NeutralMaxPercent
        {
            get => _neuMaxPercent;
            set => SetProperty(ref _neuMaxPercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "5. Neutral", Name = "Struggle % (Max/Min)", Order = 51)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal NeutralStrugglePercent
        {
            get => _neuStrugglePercent;
            set => SetProperty(ref _neuStrugglePercent, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        // --- Configuración de Escala ---

        [Display(GroupName = "Config", Name = "Max scale abs", Order = 90)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal MaxScaleAbs
        {
            get => _maxScaleAbs;
            set => SetProperty(ref _maxScaleAbs, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        [Display(GroupName = "Config", Name = "Scale softness", Order = 91)]
        [PostValueMode(PostValueModes.OnLostFocus)]
        public decimal ScaleSoftFactor
        {
            get => _scaleSoftFactor;
            set => SetProperty(ref _scaleSoftFactor, value, () =>
            {
                _historyLoaded = false;
                RecalculateValues();
            });
        }

        #endregion


        #region Data Series

        // 1. Aggressive
        private readonly CandleDataSeries _cAggBuy = new CandleDataSeries("Agg Buy") { IsHidden = true, ShowCurrentValue = false };
        private readonly CandleDataSeries _cAggSell = new CandleDataSeries("Agg Sell") { IsHidden = true, ShowCurrentValue = false };

        // 2. Dominance
        private readonly CandleDataSeries _cDomBuy = new CandleDataSeries("Dom Buy") { IsHidden = true, ShowCurrentValue = false };
        private readonly CandleDataSeries _cDomSell = new CandleDataSeries("Dom Sell") { IsHidden = true, ShowCurrentValue = false };

        // 3. Divergence
        private readonly CandleDataSeries _cDiv = new CandleDataSeries("Divergence") { IsHidden = true, ShowCurrentValue = false };

        // 4. Reversal
        private readonly CandleDataSeries _cRevBuy = new CandleDataSeries("Rev Buy") { IsHidden = true, ShowCurrentValue = false };
        private readonly CandleDataSeries _cRevSell = new CandleDataSeries("Rev Sell") { IsHidden = true, ShowCurrentValue = false };

        // 5. Neutral
        private readonly CandleDataSeries _cNeu = new CandleDataSeries("Neutral") { IsHidden = true, ShowCurrentValue = false };

        private readonly CandleDataSeries _cNormal = new CandleDataSeries("Normal") { IsHidden = true, ShowCurrentValue = false };

        // SOLUCIÓN ESCALA: Series de línea VISIBLES (para el motor) pero TRANSPARENTES (para el ojo)
        // Esto obliga a ATAS a escalar el panel hasta estos valores.
        private readonly ValueDataSeries _scaleHigh = new ValueDataSeries("Scale High")
        {
            VisualType = VisualMode.Line,
            Color = CrossColor.FromArgb(0, 0, 0, 0), // Transparente total
            Width = 1,
            IsHidden = false, // CRÍTICO: Tiene que ser false para que la escala funcione
            ShowCurrentValue = false,
            ScaleIt = true
        };

        private readonly ValueDataSeries _scaleLow = new ValueDataSeries("Scale Low")
        {
            VisualType = VisualMode.Line,
            Color = CrossColor.FromArgb(0, 0, 0, 0), // Transparente total
            Width = 1,
            IsHidden = false,
            ShowCurrentValue = false,
            ScaleIt = true
        };

        // Cache interno de señales
        private Dictionary<int, int> _signalTypeCache = new Dictionary<int, int>();
        private Dictionary<int, decimal> _signalPriceCache = new Dictionary<int, decimal>();

        #endregion

        #region Color Fields (Backing Fields)

        // --- PALETA SEMÁNTICA (Por defecto: Lógica de Intensidad) ---
        // Compras: Gama Verde/Azul
        private CrossColor _semAggPos = System.Drawing.Color.Lime.Convert();         // Compra Agresiva (Brillante)
        private CrossColor _semDomPos = System.Drawing.Color.ForestGreen.Convert();  // Compra Sólida (Oscuro)
        private CrossColor _semRevPos = System.Drawing.Color.Cyan.Convert();         // Giro Alcista (Destaca)

        // Ventas: Gama Roja/Naranja
        private CrossColor _semAggNeg = System.Drawing.Color.Red.Convert();          // Venta Agresiva (Brillante)
        private CrossColor _semDomNeg = System.Drawing.Color.DarkRed.Convert();      // Venta Sólida (Oscuro)
        private CrossColor _semRevNeg = System.Drawing.Color.Orange.Convert();       // Giro Bajista (Alerta)

        // Contexto
        private CrossColor _semDiv = System.Drawing.Color.Yellow.Convert();          // Divergencia (Warning)
        private CrossColor _semNeutral = System.Drawing.Color.Silver.Convert();      // Indecisión
        private CrossColor _semNormal = System.Drawing.Color.DimGray.Convert();      // Ruido de fondo

        // 1) Sin Señal -> Gris
        private CrossColor _orgNormal = System.Drawing.Color.Gray.Convert();

        // 2) Agresivo Positivo -> Verde (Lime)
        private CrossColor _orgAggPos = System.Drawing.Color.Lime.Convert();
        // 3) Agresivo Negativo -> Rojo
        private CrossColor _orgAggNeg = System.Drawing.Color.Red.Convert();

        // 4) Dominance Positivo -> Cyan
        private CrossColor _orgDomPos = System.Drawing.Color.Cyan.Convert();
        // 5) Dominance Negativo -> Fucsia (Magenta)
        private CrossColor _orgDomNeg = System.Drawing.Color.Magenta.Convert();

        // 6) Divergencia -> Blanca
        private CrossColor _orgDiv = System.Drawing.Color.White.Convert();

        // 7) Reversal Positivo -> Celeste (DeepSkyBlue)
        private CrossColor _orgRevPos = System.Drawing.Color.DeepSkyBlue.Convert();
        // 8) Reversal Negativo -> Azul Oscuro
        private CrossColor _orgRevNeg = System.Drawing.Color.DarkBlue.Convert();

        // 9) Neutral -> Amarilla
        private CrossColor _orgNeutral = System.Drawing.Color.Yellow.Convert();

        private ColorScheme _colorMode = ColorScheme.Original;

        #endregion

        // --- PROPIEDADES DE COLOR ---

        public enum ColorScheme { Original, Semantic }

        [Display(GroupName = "Colores", Name = "Modo de Color", Order = 60)]
        public ColorScheme ColorMode
        {
            get => _colorMode;
            set { _colorMode = value; UpdateSeriesColors(); RedrawChart(); }
        }


        // --- SEMÁNTICOS (9 Colores Configurables) ---

        [Display(GroupName = "Colores - Semántico", Name = "Agresivo Alcista", Order = 70)]
        public CrossColor SemAggPos
        {
            get => _semAggPos;
            set { _semAggPos = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Agresivo Bajista", Order = 71)]
        public CrossColor SemAggNeg
        {
            get => _semAggNeg;
            set { _semAggNeg = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Dominancia Alcista", Order = 72)]
        public CrossColor SemDomPos
        {
            get => _semDomPos;
            set { _semDomPos = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Dominancia Bajista", Order = 73)]
        public CrossColor SemDomNeg
        {
            get => _semDomNeg;
            set { _semDomNeg = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Reversal Alcista", Order = 74)]
        public CrossColor SemRevPos
        {
            get => _semRevPos;
            set { _semRevPos = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Reversal Bajista", Order = 75)]
        public CrossColor SemRevNeg
        {
            get => _semRevNeg;
            set { _semRevNeg = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Divergencia (Warning)", Order = 76)]
        public CrossColor SemDiv
        {
            get => _semDiv;
            set { _semDiv = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Neutral (Indecisión)", Order = 77)]
        public CrossColor SemNeutral
        {
            get => _semNeutral;
            set { _semNeutral = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Semántico", Name = "Normal (Sin Señal)", Order = 78)]
        public CrossColor SemNormal
        {
            get => _semNormal;
            set { _semNormal = value; UpdateSeriesColors(); RedrawChart(); }
        }

        // ORIGINALES
        [Display(GroupName = "Colores - Original", Name = "Aggressive Buy (+)", Order = 80)]
        public CrossColor OrgAggPos
        {
            get => _orgAggPos;
            set { _orgAggPos = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Aggressive Sell (-)", Order = 81)]
        public CrossColor OrgAggNeg
        {
            get => _orgAggNeg;
            set { _orgAggNeg = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Dominance Buy (+)", Order = 82)]
        public CrossColor OrgDomPos
        {
            get => _orgDomPos;
            set { _orgDomPos = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Dominance Sell (-)", Order = 83)]
        public CrossColor OrgDomNeg
        {
            get => _orgDomNeg;
            set { _orgDomNeg = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Divergencia", Order = 84)]
        public CrossColor OrgDiv
        {
            get => _orgDiv;
            set { _orgDiv = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Reversal Buy", Order = 85)]
        public CrossColor OrgRevPos
        {
            get => _orgRevPos;
            set { _orgRevPos = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Reversal Sell", Order = 86)]
        public CrossColor OrgRevNeg
        {
            get => _orgRevNeg;
            set { _orgRevNeg = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "Neutral", Order = 87)]
        public CrossColor OrgNeutral
        {
            get => _orgNeutral;
            set { _orgNeutral = value; UpdateSeriesColors(); RedrawChart(); }
        }

        [Display(GroupName = "Colores - Original", Name = "No Signal / Normal", Order = 88)]
        public CrossColor OrgNormal
        {
            get => _orgNormal;
            set { _orgNormal = value; UpdateSeriesColors(); RedrawChart(); }
        }

        #region Engine Variables (The Queue)

        // Structure to hold individual ticks in memory
        private struct TickData
        {
            public decimal Price;
            public decimal Volume;
            public int Direction; // 1 = Buy, -1 = Sell
        }

        private readonly Queue<TickData> _tickQueue = new Queue<TickData>(20000); // Pre-allocate capacity
        private decimal _currentQueueVolume;
        private decimal _currentQueueDelta;

        // Rolling extremes tracking is complex in a simple queue removal. 
        // For performance, we recalculate Max/Min delta only when needed or maintain a deque.
        // To be 100% exact on "Max Delta during the formation of these specific 10k contracts",
        // we iterate the queue. It is fast enough for 10k items.

        private bool _historyLoaded = false;
        private int _lastCalculatedBar = -1;

        #endregion

        #region ctor
        public DeltaPatterns() : base(true)
        {
            Panel = IndicatorDataProvider.NewPanel;
            DenyToChangePanel = true;
            IgnoreHistoryScale = true;

            // 1. Ocultamos la serie por defecto
            //DataSeries[0].IsHidden = true;
            //((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

            // Limpiamos series por defecto
            DataSeries.Clear();

            // 1. Series de Escala (Transparentes)
            DataSeries.Add(_scaleHigh);
            DataSeries.Add(_scaleLow);

            // Añadimos las 8 series
            DataSeries.Add(_cAggBuy);
            DataSeries.Add(_cAggSell);
            DataSeries.Add(_cDomBuy);
            DataSeries.Add(_cDomSell);
            DataSeries.Add(_cDiv);
            DataSeries.Add(_cRevBuy);
            DataSeries.Add(_cRevSell);
            DataSeries.Add(_cNeu);
            DataSeries.Add(_cNormal);

            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            // Aplicar colores iniciales
            UpdateSeriesColors();
        }

        #endregion

        protected override void OnCalculate(int bar, decimal value)
        {
            if (bar == 0 && !_historyLoaded)
            {
                // Reset on first load
                DataSeries.ForEach(x => x.Clear());
                _tickQueue.Clear();
                _currentQueueVolume = 0;
                _currentQueueDelta = 0;
                _lastCalculatedBar = -1;
                _signalTypeCache.Clear();
                _signalPriceCache.Clear();
                return;
            }

            // PROTECCIÓN DE ESCALA PROPORCIONAL
            // Si no hay datos de escala, usamos el 10% del TargetVolume como mínimo
            if (_scaleHigh[bar] == 0m && _scaleLow[bar] == 0m)
            {
                if (_historyLoaded && bar > 0)
                {
                    _scaleHigh[bar] = _scaleHigh[bar - 1];
                    _scaleLow[bar] = _scaleLow[bar - 1];
                }
                else
                {
                    // Primer bar o vacío: Usamos el 10% del TargetVolume para abrir el panel
                    decimal minScale = TargetVolume * 0.1m;
                    _scaleHigh[bar] = minScale;
                    _scaleLow[bar] = -minScale;
                }
            }
        }

        protected override void OnRecalculate()
        {
            // Este método se ejecuta cuando cambias cualquier [Parameter] en la UI.
            // Aquí forzamos la recarga del histórico.
            _historyLoaded = false;

            // Limpiamos colas por seguridad
            _tickQueue.Clear();
            _currentQueueVolume = 0;
            _currentQueueDelta = 0;
        }

        // Request historical ticks when calculation finishes (standard ATAS pattern for tick-based indicators)
        protected override void OnFinishRecalculate()
        {
            if (_historyLoaded) return;

            // Si no hay velas, no podemos pedir historia aún
            if (CurrentBar < 1)
                return;

            var firstCandle = GetCandle(0);
            if (firstCandle == null)
                return;

            // Mejor usar la última vela real en vez de DateTime.UtcNow
            var lastCandle = GetCandle(CurrentBar - 1);
            var sessionStart = firstCandle.Time;
            var sessionEnd = lastCandle?.LastTime ?? firstCandle.LastTime;

            // We request data to fill the history
            var request = new CumulativeTradesRequest(sessionStart, sessionEnd, 0, 0);
            RequestForCumulativeTrades(request);
        }

        // Handle the incoming historical ticks
        protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
        {
            if (cumulativeTrades == null) return;

            // Reset state
            _tickQueue.Clear();
            _currentQueueVolume = 0;
            _currentQueueDelta = 0;
            DataSeries.ForEach(x => x.Clear());

            // Process all historical trades
            // We need to map trades to bars to paint the histogram at the correct time

            var trades = cumulativeTrades.ToList();
            int currentBarIndex = 0;
            int totalBars = CurrentBar - 1;

            foreach (var trade in trades)
            {
                // 1. Advance Bar Index if needed to match trade time
                while (currentBarIndex <= totalBars && GetCandle(currentBarIndex).LastTime < trade.Time)
                {
                    // Finalize the state of the previous bar (Snapshots the rolling window state at bar close)
                    PaintBarState(currentBarIndex);
                    currentBarIndex++;
                }

                // 2. Process Tick
                ProcessTick(trade.Volume, trade.Direction, trade.FirstPrice); // Using FirstPrice as approx for the block if aggregated
            }

            // Paint remaining bars if any
            while (currentBarIndex <= totalBars)
            {
                PaintBarState(currentBarIndex);
                currentBarIndex++;
            }

            _historyLoaded = true;
            _lastCalculatedBar = totalBars;
        }

        // Handle Real-Time Ticks
        protected override void OnNewTrade(MarketDataArg trade)
        {
            if (!_historyLoaded) return;

            ProcessTick(trade.Volume, trade.Direction, trade.Price);

            // In real-time, we repaint the current bar with the latest state of the queue
            PaintBarState(CurrentBar - 1);
        }

        private void ProcessTick(decimal volume, TradeDirection dir, decimal price)
        {
            int direction = (dir == TradeDirection.Buy) ? 1 : -1;
            decimal delta = volume * direction;

            // Add new tick
            var newTick = new TickData { Price = price, Volume = volume, Direction = direction };
            _tickQueue.Enqueue(newTick);

            _currentQueueVolume += volume;
            _currentQueueDelta += delta;

            // Remove old ticks to maintain Target Volume (Rolling Window)
            while (_currentQueueVolume > TargetVolume && _tickQueue.Count > 0)
            {
                var oldTick = _tickQueue.Peek();

                // Exact removal logic:
                // If removing the whole tick keeps us >= Target, remove it all.
                // If removing it drops us below Target, we theoretically keep a fraction, 
                // BUT for exact "Price at Start" logic, usually FIFO drops whole ticks or we accept slight volume jitter.
                // For this implementation, we will act as a strict FIFO on trades to ensure price precision.
                // If the oldest trade was 100 lots and we are 50 over, we remove the whole trade to shift the "Start Price" correctly to the next trade.
                // This implies the window might be slightly < TargetVolume for a split second until next tick fills it, 
                // OR slightly > TargetVolume.
                // Strategy: Keep Queue >= TargetVolume. Only remove if (Volume - OldTick.Volume) >= TargetVolume.

                if ((_currentQueueVolume - oldTick.Volume) >= TargetVolume)
                {
                    _tickQueue.Dequeue();
                    _currentQueueVolume -= oldTick.Volume;
                    _currentQueueDelta -= (oldTick.Volume * oldTick.Direction);
                }
                else
                {
                    break; // Cannot remove more without going under target
                }
            }
        }

        private void PaintBarState(int bar)
        {
            if (bar < 0 || _tickQueue.Count == 0) return;

            // 1. Calculate Exact Metrics from the Queue
            // We iterate the queue to find Max/Min Delta Running Sum and Start/End Prices

            decimal runningDelta = 0;
            decimal maxD = 0; // Max Delta reached DURING this 10k window
            decimal minD = 0; // Min Delta reached DURING this 10k window

            decimal startPrice = _tickQueue.Peek().Price;
            decimal endPrice = 0;
            decimal highPrice = decimal.MinValue;
            decimal lowPrice = decimal.MaxValue;

            foreach (var t in _tickQueue)
            {
                // Price Stats
                if (t.Price > highPrice) highPrice = t.Price;
                if (t.Price < lowPrice) lowPrice = t.Price;
                endPrice = t.Price; // The last one will be the end price

                // Delta Stats
                decimal tickDelta = t.Volume * t.Direction;
                runningDelta += tickDelta;

                if (runningDelta > maxD) maxD = runningDelta;
                if (runningDelta < minD) minD = runningDelta;
            }

            // 2. Logic Analysis
            AnalyzeLogic(bar, _currentQueueDelta, _currentQueueVolume, maxD, minD, startPrice, endPrice, highPrice, lowPrice);
        }

        private void AnalyzeLogic(int bar, decimal delta, decimal vol, decimal maxD, decimal minD, decimal openPrice, decimal closePrice, decimal highPrice, decimal lowPrice)
        {
            if (vol == 0) return;

            // --- CÁLCULO DINÁMICO DE UMBRALES ---
            // Usamos las propiedades públicas (TargetVolume, AggressivePercent, etc.)
            decimal baseVol = (decimal)TargetVolume;

            decimal thAggMin = baseVol * (AggressivePercent / 100m);
            decimal thDomMin = baseVol * (DominancePercent / 100m);
            decimal thDomWick = baseVol * (DominanceWickPercent / 100m);
            decimal thDivMin = baseVol * (DivPercent / 100m);
            decimal thRevExt = baseVol * (RevExtremePercent / 100m);
            decimal thRevClose = baseVol * (RevClosePercent / 100m);
            decimal thNeuStruggle = baseVol * (NeutralStrugglePercent / 100m);

            // Variables de estado
            decimal deltaPercent = (delta / vol) * 100m;
            decimal absDelta = Math.Abs(delta);

            int signalType = 0;
            decimal signalPrice = 0;

            // --- A. DIVERGENCE ---
            bool isDiv = false;
            if (absDelta > thDivMin)
            {
                bool priceUp = closePrice > openPrice;
                bool deltaUp = delta > 0;

                if (priceUp != deltaUp)
                {
                    isDiv = true;
                    signalType = deltaUp ? 6 : -6;
                    signalPrice = deltaUp ? highPrice : lowPrice;
                }
            }

            // --- B. REVERSAL ---
            if (!isDiv)
            {
                if (maxD > thRevExt && delta < -thRevClose)
                {
                    signalType = 8; // Sell
                    signalPrice = highPrice;
                }
                else if (minD < -thRevExt && delta > thRevClose)
                {
                    signalType = 7; // Buy
                    signalPrice = lowPrice;
                }
            }

            // --- C. NEUTRAL ---
            if (signalType == 0 && Math.Abs(deltaPercent) <= NeutralMaxPercent)
            {
                if (maxD > thNeuStruggle || minD < -thNeuStruggle)
                {
                    signalType = 9;
                    signalPrice = closePrice;
                }
            }

            // --- D. AGGRESSIVE ---
            if (signalType == 0 && Math.Abs(deltaPercent) > AggressiveClosePercent && absDelta > thAggMin)
            {
                if (delta > 0)
                {
                    signalType = 2; // Agg Buy
                    signalPrice = lowPrice;
                }
                else
                {
                    signalType = 3; // Agg Sell
                    signalPrice = highPrice;
                }
            }

            // --- E. DOMINANCE ---
            if (signalType == 0 && absDelta > thDomMin)
            {
                if (delta > 0 && minD >= -thDomWick)
                {
                    signalType = 4; // Dom Buy
                    signalPrice = lowPrice;
                }
                else if (delta < 0 && maxD <= thDomWick)
                {
                    signalType = 5; // Dom Sell
                    signalPrice = highPrice;
                }
            }

            // 2. CREAR LA VELA
            var candle = new ATAS.Indicators.Candle();
            candle.Open = 0;
            candle.Close = delta;
            candle.High = maxD;
            candle.Low = minD;

            // 3. SOLUCIÓN ESCALA: usar amplitud simétrica alrededor de 0
            // Tomamos el extremo mayor en valor absoluto dentro de la ventana
            var rawAmp = Math.Max(Math.Abs(maxD), Math.Abs(minD));

            // Aplicamos un “soft factor” para no irnos directamente al extremo
            if (ScaleSoftFactor > 0 && ScaleSoftFactor < 1)
                rawAmp *= ScaleSoftFactor;

            // Límite duro para que una barra puntual no destruya la escala
            var amp = Math.Min(rawAmp, MaxScaleAbs);

            // NUEVO: Escala Mínima Dinámica = 10% del TargetVolume
            decimal minScaleDynamic = TargetVolume * 0.1m;

            // Seguridad: si no hay rango, dale un mínimo
            if (amp <= minScaleDynamic)
                amp = minScaleDynamic;

            // La escala que ve ATAS es SIEMPRE [-amp, +amp]
            _scaleHigh[bar] = amp;
            _scaleLow[bar] = -amp;

            // 4. LIMPIEZA DE SERIES
            var empty = new ATAS.Indicators.Candle();
            _cAggBuy[bar] = empty; _cAggSell[bar] = empty;
            _cDomBuy[bar] = empty; _cDomSell[bar] = empty;
            _cDiv[bar] = empty;
            _cRevBuy[bar] = empty; _cRevSell[bar] = empty;
            _cNeu[bar] = empty;
            _cNormal[bar] = empty; // Limpiamos normal también

            // 5. ASIGNACIÓN A LA SERIE CORRECTA
            if (signalType == 2) _cAggBuy[bar] = candle;
            else if (signalType == 3) _cAggSell[bar] = candle;
            else if (signalType == 4) _cDomBuy[bar] = candle;
            else if (signalType == 5) _cDomSell[bar] = candle;
            else if (Math.Abs(signalType) == 6) _cDiv[bar] = candle;
            else if (signalType == 7) _cRevBuy[bar] = candle;
            else if (signalType == 8) _cRevSell[bar] = candle;
            else if (signalType == 9) _cNeu[bar] = candle;
            else
            {
                // Si signalType es 0 (Sin señal), va a la serie Normal
                _cNormal[bar] = candle;
            }

            // 6. CACHÉ PARA CHART
            _signalTypeCache[bar] = signalType;
            if (signalType != 0) _signalPriceCache[bar] = signalPrice;
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (ChartInfo == null || layout != DrawingLayouts.Final) return;

            if (ShowChartSignals)
            {
                var priceContainer = ChartInfo.PriceChartContainer;
                if (priceContainer != null)
                {
                    context.SetClip(priceContainer.Region);

                    // Elegimos la paleta activa
                    bool isSem = ColorMode == ColorScheme.Semantic;

                    // Cache local de colores (GDI)
                    var cAggPos = (isSem ? SemAggPos : OrgAggPos).Convert();
                    var cAggNeg = (isSem ? SemAggNeg : OrgAggNeg).Convert();
                    var cDomPos = (isSem ? SemDomPos : OrgDomPos).Convert();
                    var cDomNeg = (isSem ? SemDomNeg : OrgDomNeg).Convert();
                    var cDiv = (isSem ? SemDiv : OrgDiv).Convert();
                    var cRevPos = (isSem ? SemRevPos : OrgRevPos).Convert();
                    var cRevNeg = (isSem ? SemRevNeg : OrgRevNeg).Convert();
                    var cNeu = (isSem ? SemNeutral : OrgNeutral).Convert();

                    for (int bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
                    {
                        if (!_signalTypeCache.TryGetValue(bar, out int type) || type == 0) continue;
                        if (!_signalPriceCache.TryGetValue(bar, out decimal price)) continue;

                        int x = ChartInfo.GetXByBar(bar);
                        int y = priceContainer.GetYByPrice(price, false);

                        if (type == 2) DrawDot(context, x, y, cAggPos, false);
                        else if (type == 3) DrawDot(context, x, y, cAggNeg, true);
                        else if (type == 4) DrawDot(context, x, y, cDomPos, false);
                        else if (type == 5) DrawDot(context, x, y, cDomNeg, true);
                        else if (Math.Abs(type) == 6) DrawDiamond(context, x, y, cDiv);
                        else if (type == 7) DrawSquare(context, x, y, cRevPos);
                        else if (type == 8) DrawSquare(context, x, y, cRevNeg);
                        else if (type == 9) DrawDot(context, x, y, cNeu, true);
                    }
                    context.ResetClip();
                }
            }
        }

        #region Draw Helpers

        private void DrawDot(RenderContext ctx, int x, int y, Color c, bool top)
        {
            int offset = top ? -SignalSize - 5 : 5;
            var rect = new Rectangle(x - SignalSize / 2, y + offset, SignalSize, SignalSize);
            ctx.FillEllipse(c, rect);
        }

        private void DrawSquare(RenderContext ctx, int x, int y, Color c)
        {
            var rect = new Rectangle(x - SignalSize / 2, y - SignalSize / 2, SignalSize, SignalSize);
            ctx.FillRectangle(c, rect);
            // Optional: ctx.DrawRectangle(Color.Black, rect);
        }

        private void DrawDiamond(RenderContext ctx, int x, int y, Color c)
        {
            int s = SignalSize / 2 + 2;
            var p1 = new Point(x, y - s);
            var p2 = new Point(x + s, y);
            var p3 = new Point(x, y + s);
            var p4 = new Point(x - s, y);
            ctx.FillPolygon(c, new[] { p1, p2, p3, p4 });
        }

        private void UpdateSeriesColors()
        {
            bool isSemantic = ColorMode == ColorScheme.Semantic;

            void SetColor(CandleDataSeries series, CrossColor color)
            {
                series.UpCandleColor = color;
                series.DownCandleColor = color;
                series.BorderColor = color;
            }

            // Mapeo directo: Si es semántico usa _semX, si es original usa _orgX
            // Esto mantiene la distinción de las 9 señales en ambos modos.

            // 1. Aggressive
            SetColor(_cAggBuy, isSemantic ? SemAggPos : OrgAggPos);
            SetColor(_cAggSell, isSemantic ? SemAggNeg : OrgAggNeg);

            // 2. Dominance
            SetColor(_cDomBuy, isSemantic ? SemDomPos : OrgDomPos);
            SetColor(_cDomSell, isSemantic ? SemDomNeg : OrgDomNeg);

            // 3. Divergence
            SetColor(_cDiv, isSemantic ? SemDiv : OrgDiv);

            // 4. Reversal
            SetColor(_cRevBuy, isSemantic ? SemRevPos : OrgRevPos);
            SetColor(_cRevSell, isSemantic ? SemRevNeg : OrgRevNeg);

            // 5. Neutral
            SetColor(_cNeu, isSemantic ? SemNeutral : OrgNeutral);

            // 6. Normal
            SetColor(_cNormal, isSemantic ? SemNormal : OrgNormal);
        }

        #endregion
    }
}
