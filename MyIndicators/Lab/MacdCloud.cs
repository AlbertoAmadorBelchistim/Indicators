using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using ATAS.Indicators;
using ATAS.Indicators.Technical;

namespace MyIndicators
{
    [DisplayName("Macd Cloud modif")]
    public class MacdCloudClean : Indicator
    {
        #region Fields & Series

        // Series de datos para pintar en el gráfico
        private readonly RangeDataSeries _cloudSeriesGreen = new RangeDataSeries("Cloud Up");
        private readonly RangeDataSeries _cloudSeriesRed = new RangeDataSeries("Cloud Down");
        private readonly ValueDataSeries _fastLineSeries = new ValueDataSeries("Fast Line");
        private readonly ValueDataSeries _slowLineSeries = new ValueDataSeries("Slow Line");
        private readonly ValueDataSeries _emaSeries = new ValueDataSeries("EMA Line");

        // Indicadores internos para el cálculo
        private readonly WMA _fastWma = new WMA();
        private readonly WMA _slowWma = new WMA();
        private readonly EMA _ema = new EMA();

        // Variables de estado
        private bool _isFastAboveSlow;

        #endregion

        #region Parameters

        [Display(Name = "Fast Period", GroupName = "Settings", Order = 10)]
        public int FastPeriod
        {
            get => _fastWma.Period;
            set
            {
                if (value <= 0) return;
                _fastWma.Period = value;
                RecalculateValues();
            }
        }

        [Display(Name = "Slow Period", GroupName = "Settings", Order = 20)]
        public int SlowPeriod
        {
            get => _slowWma.Period;
            set
            {
                if (value <= 0) return;
                _slowWma.Period = value;
                RecalculateValues();
            }
        }

        [Display(Name = "EMA Period", GroupName = "Settings", Order = 30)]
        public int EmaPeriod
        {
            get => _ema.Period;
            set
            {
                if (value <= 0) return;
                _ema.Period = value;
                RecalculateValues();
            }
        }

        [Display(Name = "Cloud Color Up", GroupName = "Colors", Order = 40)]
        public Color CloudColorAbove { get; set; } = Color.FromArgb(90, 0, 255, 0);

        [Display(Name = "Cloud Color Down", GroupName = "Colors", Order = 50)]
        public Color CloudColorBelow { get; set; } = Color.FromArgb(90, 255, 0, 0);

        #endregion

        #region Constructor

        public MacdCloudClean()
        {
            // Configuración de las líneas (WMA Rápida y Lenta)
            _fastLineSeries.VisualType = VisualMode.Line;
            _fastLineSeries.Width = 2;
            _fastLineSeries.Color = Colors.White;
            _fastLineSeries.IgnoredByAlerts = true;

            _slowLineSeries.VisualType = VisualMode.Line;
            _slowLineSeries.Width = 1;
            _slowLineSeries.Color = Colors.Gray; // Color por defecto para diferenciar
            _slowLineSeries.IgnoredByAlerts = true;

            // Configuración de la línea EMA
            _emaSeries.VisualType = VisualMode.Line;
            _emaSeries.Color = Colors.Yellow;
            _emaSeries.Width = 2;

            // Configuración de las Nubes (Clouds)
            _cloudSeriesGreen.DrawAbovePrice = false;
            _cloudSeriesRed.DrawAbovePrice = false;

            // Inicialmente ocultas hasta que se calculen
            _cloudSeriesGreen.IsHidden = true;
            _cloudSeriesRed.IsHidden = true;

            // Añadir series al gráfico
            DataSeries.Add(_fastLineSeries);
            DataSeries.Add(_slowLineSeries);
            DataSeries.Add(_cloudSeriesGreen);
            DataSeries.Add(_cloudSeriesRed);
            DataSeries.Add(_emaSeries);

            // Valores por defecto
            FastPeriod = 9;
            SlowPeriod = 18;
            EmaPeriod = 8;
        }

        #endregion

        #region Calculation

        protected override void OnCalculate(int bar, decimal value)
        {
            // 1. Calcular los valores de los indicadores
            var fastVal = _fastWma.Calculate(bar, value);
            var slowVal = _slowWma.Calculate(bar, value);
            var emaVal = _ema.Calculate(bar, value);

            // 2. Asignar valores a las series visibles
            _fastLineSeries[bar] = fastVal;
            _slowLineSeries[bar] = slowVal;
            _emaSeries[bar] = emaVal;

            // Si no hay suficientes barras para el cálculo, salimos
            if (bar < Math.Max(FastPeriod, SlowPeriod))
                return;

            // 3. Lógica de la Nube (Cloud)
            bool currentFastAboveSlow = fastVal > slowVal;

            // Detectar cambio de estado o primera ejecución válida
            // Esto actualiza los colores dinámicamente
            UpdateCloudColors();

            if (currentFastAboveSlow)
            {
                _cloudSeriesGreen.IsHidden = false;
                _cloudSeriesRed.IsHidden = true;

                // Definir el rango de la nube verde
                _cloudSeriesGreen[bar].Upper = Math.Max(fastVal, slowVal);
                _cloudSeriesGreen[bar].Lower = Math.Min(fastVal, slowVal);
            }
            else
            {
                _cloudSeriesGreen.IsHidden = true;
                _cloudSeriesRed.IsHidden = false;

                // Definir el rango de la nube roja
                _cloudSeriesRed[bar].Upper = Math.Max(fastVal, slowVal);
                _cloudSeriesRed[bar].Lower = Math.Min(fastVal, slowVal);
            }

            _isFastAboveSlow = currentFastAboveSlow;
        }

        private void UpdateCloudColors()
        {
            // Aplicamos los colores seleccionados por el usuario a las series
            // Se hace aquí para asegurar que si el usuario cambia el color, se actualice
            if (_cloudSeriesGreen.RangeColor != CloudColorAbove)
                _cloudSeriesGreen.RangeColor = CloudColorAbove;

            if (_cloudSeriesRed.RangeColor != CloudColorBelow)
                _cloudSeriesRed.RangeColor = CloudColorBelow;
        }

        #endregion
    }
}
