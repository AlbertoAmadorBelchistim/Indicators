using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical;
using OFT.Attributes;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using static ATAS.Indicators.Technical.SpeedOfTape;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace MyIndicators
{

    [DisplayName("Speed of Tape (Lab)")]
    [Category("Custom")]
    [Description("Mide la velocidad de las órdenes en una ventana de tiempo y detecta aceleraciones inusuales.")]
    public class SpeedOfTapeLab : Indicator
    {
        #region Tipos y Clases Internas

        public enum SpeedType
        {
            Volume,
            Ticks,
            Buys,
            Sells,
            Delta
        }

        private class SignalLine
        {
            public int Bar { get; set; }
            public decimal Price { get; set; }
            public bool IsBullish { get; set; }
        }

        #endregion

        #region Fields

        // Almacenamiento de señales para dibujar líneas
        private readonly List<SignalLine> _signals = new List<SignalLine>();

        // Series visuales
        private readonly PaintbarsDataSeries _paintBars = new PaintbarsDataSeries("Paint Bars");
        private readonly ValueDataSeries _renderSeries = new ValueDataSeries("Speed Histogram");
        private readonly ValueDataSeries _thresholdSeries; // Línea azul del filtro automático

        // Indicador interno para calcular el promedio
        private readonly SMA _smaFilter = new SMA();

        // Variables privadas de configuración
        private int _timeWindowSeconds = 15;
        private int _fixedThreshold = 10;
        private Color _alertColor = Color.Yellow;
        private int _lastAlertBar = -1;

        #endregion

        #region Parameters

        [Display(Name = "AutoFiltro", GroupName = "Filtros", Order = 10, Description = "Si está activo, usa un promedio móvil dinámico. Si no, usa un valor fijo.")]
        public bool AutoFilter
        {
            get => _autoFilter;
            set
            {
                _autoFilter = value;
                RecalculateValues();
            }

        }
        private bool _autoFilter = true;

        [Display(Name = "Periodo de autofiltro", GroupName = "Filtros", Order = 20)]
        [Range(1, int.MaxValue)]
        public int AutoFilterPeriod
        {
            get => _smaFilter.Period;
            set
            {
                _smaFilter.Period = value;
                RecalculateValues();
            }
        }

        [Display(Name = "Filtro de tiempo (Seg)", GroupName = "Filtros", Order = 30, Description = "Segundos hacia atrás para calcular la velocidad.")]
        [Range(1, int.MaxValue)]
        public int TimeWindowSec
        {
            get => _timeWindowSeconds;
            set
            {
                _timeWindowSeconds = Math.Max(1, value);
                RecalculateValues();
            }
        }

        [Display(Name = "Filtro de transacciones", GroupName = "Filtros", Order = 40, Description = "Valor fijo de velocidad si el AutoFilter está desactivado.")]
        [Range(0, int.MaxValue)]
        public int FixedThreshold
        {
            get => _fixedThreshold;
            set
            {
                _fixedThreshold = Math.Max(1, value);
                RecalculateValues();
            }
        }

        [Display(Name = "Tipo de cálculo", GroupName = "Filtros", Order = 50)]
        public SpeedType CalculationType
        {
            get => _type;
            set
            {
                _type = value;
                RecalculateValues();
            }
        }
        private SpeedType _type = SpeedType.Ticks;

        [Display(Name = "Highlight Color", GroupName = "Visualization", Order = 60)]
        public Color MaxSpeedColor
        {
            get => _alertColor;
            set
            {
                _alertColor = value;
                RecalculateValues();
            }
        }

        [Display(Name = "Show Price Selection", GroupName = "Visualization", Order = 60)]
        public bool DrawLines
        {
            get => _drawLines;
            set
            {
                _drawLines = value;
                RedrawChart(); // <--- IMPORTANTE: Fuerza el repintado al marcar/desmarcar
            }
        }
        private bool _drawLines = true;

        [Display(Name = "Line Length (Bars)", GroupName = "Visualization", Order = 70)]
        [Range(1, int.MaxValue)]
        public int LineLength
        {
            get => _lineLength;
            set
            {
                _lineLength = value;
                RedrawChart();
            }
        }
        private int _lineLength = 10;

        [Display(Name = "Positive Pen", GroupName = "Visualization", Order = 80)]
        public PenSettings PosPen
        {
            get => _posPen;
            set
            {
                _posPen = value;
                // Si el usuario cambia el objeto entero, nos resuscribimos
                if (_posPen != null) _posPen.PropertyChanged += (s, e) => RedrawChart();
                RedrawChart();
            }
        }
        private PenSettings _posPen = new PenSettings { Color = Colors.Green };

        [Display(Name = "Negative Pen", GroupName = "Visualization", Order = 90)]
        public PenSettings NegPen
        {
            get => _negPen;
            set
            {
                _negPen = value;
                if (_negPen != null) _negPen.PropertyChanged += (s, e) => RedrawChart();
                RedrawChart();
            }
        }
        private PenSettings _negPen = new PenSettings { Color = Colors.Red };

        [Display(Name = "Use Alerts", GroupName = "Alerts", Order = 110)]
        public bool UseAlerts { get; set; }

        [Display(Name = "Alert File", GroupName = "Alerts", Order = 120)]
        public string AlertFile { get; set; } = "alert1";

        [Display(Name = "Alert Text Color", GroupName = "Alerts", Order = 130)]
        public Color AlertForeColor { get; set; } = Color.FromArgb(255, 247, 249, 249);

        [Display(Name = "Alert BG Color", GroupName = "Alerts", Order = 140)]
        public Color AlertBgColor { get; set; } = Color.FromArgb(255, 75, 72, 72);

        #endregion

        public SpeedOfTapeLab() : base(true)
        {
            // Configuración de series
            _paintBars.IsHidden = true;

            _renderSeries.VisualType = VisualMode.Histogram;
            _renderSeries.Color = System.Windows.Media.Color.FromRgb(0, 255, 255); // Cyan
            _renderSeries.ResetAlertsOnNewBar = true;

            _smaFilter.Period = 10;
            _smaFilter.ColoredDirection = false;
            _thresholdSeries = (ValueDataSeries)_smaFilter.DataSeries[0];
            _thresholdSeries.Name = "Filter line";
            _thresholdSeries.Color = System.Windows.Media.Color.FromRgb(173, 216, 230); // LightBlue
            _thresholdSeries.Width = 2;

            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
            DataSeries.Add(_renderSeries);
            DataSeries.Add(_thresholdSeries);
            DataSeries.Add(_paintBars);

            // Configuración por defecto
            Panel = "NewPanel"; // Se dibuja en panel separado por defecto

            if (PosPen != null) PosPen.PropertyChanged += (sender, args) => RedrawChart();
            if (NegPen != null) NegPen.PropertyChanged += (sender, args) => RedrawChart();
        }

        protected override void OnRecalculate()
        {
            _signals.Clear();
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // 1. Lógica de la ventana deslizante de tiempo
            decimal accumulatedSpeed = 0;
            int lookBackBar = bar;
            var currentCandle = GetCandle(bar);

            // Bucle hacia atrás
            while (lookBackBar >= 0)
            {
                var pastCandle = GetCandle(lookBackBar);
                TimeSpan timeDiff = currentCandle.Time - pastCandle.Time;

                // Si estamos DENTRO de la ventana de tiempo
                if (timeDiff.TotalSeconds < _timeWindowSeconds)
                {
                    switch (CalculationType)
                    {
                        case SpeedType.Volume: accumulatedSpeed += pastCandle.Volume; break;
                        case SpeedType.Ticks: accumulatedSpeed += pastCandle.Ticks; break;
                        case SpeedType.Buys: accumulatedSpeed += pastCandle.Ask; break;
                        case SpeedType.Sells: accumulatedSpeed += pastCandle.Bid; break;
                        case SpeedType.Delta: accumulatedSpeed += pastCandle.Delta; break;
                    }
                }
                else
                {
                    // Si la vela se sale del tiempo, interpolamos (regla de 3) y salimos del bucle
                    // Esto asegura precisión matemática exacta
                    if (timeDiff.TotalSeconds > 0)
                    {
                        accumulatedSpeed = accumulatedSpeed * (decimal)_timeWindowSeconds / (decimal)timeDiff.TotalSeconds;
                    }
                    break;
                }
                lookBackBar--;
            }

            // 2. Cálculo del Filtro (Threshold)
            // ATAS original alimenta la SMA con el valor multiplicado por 1.5 para crear un "techo" alto
            _smaFilter.Calculate(bar, accumulatedSpeed * 1.5m);

            // Si no usamos AutoFilter, sobreescribimos el valor de la SMA con el fijo del usuario
            if (!AutoFilter)
            {
                _thresholdSeries[bar] = (decimal)_fixedThreshold;
            }

            // Guardamos el valor actual para el histograma
            _renderSeries[bar] = accumulatedSpeed;

            // 3. Comprobación de disparo (Trigger)
            // Si la velocidad supera el umbral
            if (Math.Abs(accumulatedSpeed) > _thresholdSeries[bar])
            {
                // Pintar histograma y vela
                _renderSeries.Colors[bar] = _alertColor;
                _paintBars[bar] = System.Windows.Media.Color.FromRgb(_alertColor.R, _alertColor.G, _alertColor.B);

                // Guardar señal para dibujar línea
                var signal = _signals.LastOrDefault(s => s.Bar == bar);
                if (signal == null)
                {
                    signal = new SignalLine { Bar = bar };
                    _signals.Add(signal);
                }

                signal.Price = (currentCandle.High + currentCandle.Low) / 2m; // Precio medio
                signal.IsBullish = currentCandle.Delta >= 0;

                // Alerta sonora (simple)
                if (bar == CurrentBar - 1 && bar != _lastAlertBar)
                {
                    AddAlert(AlertFile, InstrumentInfo.Instrument, $"Tape Speed Alert: {accumulatedSpeed:0.##}", Colors.Black, Colors.White);
                    _lastAlertBar = bar;
                }
            }
            else
            {
                // Limpiar color si se recalcula y ya no cumple
                _paintBars[bar] = null;
            }
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Verificaciones básicas
            if (ChartInfo == null || ChartInfo.PriceChartContainer == null || !DrawLines) return;

            // Cacheamos la región del precio para verificar límites (igual que DeltaModif línea 507)
            var priceRegion = ChartInfo.PriceChartContainer.Region;

            foreach (var signal in _signals)
            {
                // 1. Visibilidad horizontal (Optimización estándar)
                if (signal.Bar > LastVisibleBarNumber || signal.Bar + LineLength < FirstVisibleBarNumber)
                    continue;

                // 2. Coordenadas X: El eje X es compartido, ChartInfo.GetXByBar es seguro
                int xStart = ChartInfo.GetXByBar(signal.Bar);
                int xEnd = Math.Min(ChartInfo.GetXByBar(signal.Bar + LineLength), ChartInfo.Region.Width);

                // 3. Coordenada Y: Calculada EXPLICITAMENTE sobre el contenedor de PRECIOS
                // Usamos 'GetYByPrice' del PriceChartContainer, no del ChartInfo genérico.
                int yPrice = ChartInfo.PriceChartContainer.GetYByPrice(signal.Price, false);

                // 4. Verificación de límites verticales (Igual que DeltaModif línea 514)
                // Solo dibujamos si la línea cae dentro del área visible del precio
                if (yPrice >= priceRegion.Top && yPrice <= priceRegion.Bottom)
                {
                    var pen = signal.IsBullish ? PosPen.RenderObject : NegPen.RenderObject;
                    context.DrawLine(pen, xStart, yPrice, xEnd, yPrice);
                }
            }
        }
    }
}
