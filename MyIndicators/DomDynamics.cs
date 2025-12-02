namespace MyIndicators
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Windows.Media;
    using ATAS.Indicators;
    using ATAS.Indicators.Technical;
    using OFT.Attributes;

    [DisplayName("DOM Dynamics")]
    [Category("Order Flow")]
    [Description("Monitor de Dinámica del DOM. Muestra velas de flujo NETO de liquidez.")]
    public class DomDynamics : Indicator
    {
        #region Fields

        // Usamos CandleDataSeries nativa
        private readonly CandleDataSeries _flowCandles = new("LiquidityFlow", "DOM Flow Candles")
        {
            UpCandleColor = Colors.LimeGreen,
            DownCandleColor = Colors.Red,
            IsHidden = false, // Visible en la lista
            ScaleIt = true    // Escala automática perfecta
        };

        private readonly Dictionary<decimal, decimal> _bidSnapshot = new();
        private readonly Dictionary<decimal, decimal> _askSnapshot = new();

        private decimal _currentNetChange = 0;
        private decimal _currentHigh = 0;
        private decimal _currentLow = 0;
        private int _lastCalculatedBar = -1;

        #endregion

        #region Parameters

        [Display(Name = "DOM Depth Limit", GroupName = "Filters", Order = 10, Description = "Niveles del libro a monitorizar. 0 = Todo.")]
        [Range(0, 1000)]
        public int DepthLimit { get; set; } = 10;

        [Display(Name = "Min Change Filter", GroupName = "Filters", Order = 20, Description = "Filtro de ruido (lotes).")]
        [Range(0, 1000)]
        public int MinVolumeFilter { get; set; } = 0;

        #endregion

        public DomDynamics() : base(true)
        {
            Panel = IndicatorDataProvider.NewPanel;

            // Añadimos la serie de velas
            DataSeries[0] = _flowCandles;

            DenyToChangePanel = false;
            EnableCustomDrawing = false; // No necesitamos dibujar nada a mano
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // Reset en carga inicial
            if (bar == 0)
            {
                _flowCandles.Clear();
                _bidSnapshot.Clear();
                _askSnapshot.Clear();

                // Snapshot inicial (solo realtime)
                var depth = MarketDepthInfo.GetMarketDepthSnapshot();
                foreach (var d in depth) UpdateSnapshot(d);
            }

            // Cambio de barra: Reset contadores intra-vela
            if (bar != _lastCalculatedBar)
            {
                _currentNetChange = 0;
                _currentHigh = 0;
                _currentLow = 0;
                _lastCalculatedBar = bar;
            }

            // Construimos/Actualizamos la vela actual
            // Open = 0 siempre (punto neutral)
            var candle = _flowCandles[bar];

            candle.Open = 0;
            candle.Close = _currentNetChange;
            candle.High = _currentHigh;
            candle.Low = _currentLow;
        }

        protected override void MarketDepthChanged(MarketDataArg depth)
        {
            // 1. Cálculo del Delta
            decimal previousVol = 0;
            bool isBid = depth.DataType == MarketDataType.Bid;
            var snapshot = isBid ? _bidSnapshot : _askSnapshot;

            if (snapshot.ContainsKey(depth.Price)) previousVol = snapshot[depth.Price];

            decimal change = depth.Volume - previousVol;

            // Actualizar Snapshot
            if (depth.Volume == 0) snapshot.Remove(depth.Price);
            else snapshot[depth.Price] = depth.Volume;

            // Filtro de Ruido
            if (Math.Abs(change) < MinVolumeFilter) return;

            // 2. Acumular Presión
            if (isBid) _currentNetChange += change;
            else _currentNetChange -= change;

            // 3. Actualizar High/Low de la sesión actual
            if (_currentNetChange > _currentHigh) _currentHigh = _currentNetChange;
            if (_currentNetChange < _currentLow) _currentLow = _currentNetChange;

            // 4. Actualizar la vela visual en tiempo real
            int bar = CurrentBar - 1;
            if (bar >= 0)
            {
                var candle = _flowCandles[bar];
                candle.Open = 0;
                candle.Close = _currentNetChange;
                candle.High = _currentHigh;
                candle.Low = _currentLow;

                // ATAS repinta automáticamente al modificar DataSeries
            }
        }

        private void UpdateSnapshot(MarketDataArg depth)
        {
            var targetDict = depth.DataType == MarketDataType.Bid ? _bidSnapshot : _askSnapshot;
            if (depth.Volume == 0) targetDict.Remove(depth.Price);
            else targetDict[depth.Price] = depth.Volume;
        }
    }
}
