using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical;
using OFT.Attributes;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Windows.Media;

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

        // --- NUEVO: Interruptor para las Barras Amarillas ---
        [Display(Name = "Pintar Velas (Highlight)", GroupName = "Visualization", Order = 55, Description = "Pinta la vela del precio cuando se detecta alta velocidad.")]
        public bool ShowPaintBars
        {
            get => _paintBars.Visible;
            set
            {
                _paintBars.Visible = value;
                // Al cambiar la visibilidad de una serie, ATAS suele refrescar solo.
                // Si no lo hace, RedrawChart() fuerza el cambio visual.
                RedrawChart();
            }
        }

        [Display(Name = "Color de Highlight", GroupName = "Visualization", Order = 60)]
        public Color MaxSpeedColor
        {
            get => _alertColor;
            set
            {
                _alertColor = value;
                RecalculateValues();
            }
        }

        // --- RENOMBRADO: Interruptor para las Líneas ---
        [Display(Name = "Dibujar Líneas de Señal", GroupName = "Visualization", Order = 65, Description = "Dibuja líneas horizontales en el gráfico de precios.")]
        public bool DrawLines
        {
            get => _drawLines;
            set
            {
                _drawLines = value;
                RedrawChart(); // Fuerza el repintado al marcar/desmarcar
            }
        }
        private bool _drawLines = true;

        [Display(Name = "Longitud de Línea (Barras)", GroupName = "Visualization", Order = 70)]
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

        [Display(Name = "Lápiz Positivo", GroupName = "Visualization", Order = 80)]
        public PenSettings PosPen
        {
            get => _posPen;
            set
            {
                _posPen = value;
                if (_posPen != null) _posPen.PropertyChanged += (s, e) => RedrawChart();
                RedrawChart();
            }
        }
        private PenSettings _posPen = new PenSettings { Color = Colors.Green };

        [Display(Name = "Lápiz Negativo", GroupName = "Visualization", Order = 90)]
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
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            EnableCustomDrawing = true;

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
            Panel = IndicatorDataProvider.NewPanel;

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
            // Usamos Math.Abs para que el filtro mida la MAGNITUD, no la dirección.
            // Esto evita que el filtro se vuelva negativo en el delta.
            var filterInput = Math.Abs(accumulatedSpeed);

            _smaFilter.Calculate(bar, filterInput * 1.5m);

            // Si no usamos AutoFilter, sobreescribimos el valor de la SMA con el fijo del usuario
            if (!AutoFilter)
            {
                _thresholdSeries[bar] = (decimal)_fixedThreshold;
            }

            // Guardamos el valor actual para el histograma
            _renderSeries[bar] = accumulatedSpeed;

            // 3. Comprobación de disparo
            if (Math.Abs(accumulatedSpeed) > _thresholdSeries[bar])
            {
                _renderSeries.Colors[bar] = _alertColor;

                // Gestión de barras amarillas
                if (ShowPaintBars)
                    _paintBars[bar] = System.Windows.Media.Color.FromRgb(_alertColor.R, _alertColor.G, _alertColor.B);
                else
                    _paintBars[bar] = null;

                // Gestión de señales
                var signal = _signals.LastOrDefault(s => s.Bar == bar);
                if (signal == null)
                {
                    signal = new SignalLine { Bar = bar };
                    _signals.Add(signal);
                }

                // --- LÓGICA ORIGINAL EXACTA ---
                // Se usa el punto medio de la vela.
                signal.Price = (currentCandle.High + currentCandle.Low) / 2m;
                signal.IsBullish = currentCandle.Delta >= 0;

                // Alertas
                if (bar == CurrentBar - 1 && bar != _lastAlertBar)
                {
                    AddAlert(AlertFile, InstrumentInfo.Instrument, $"Tape Speed Alert: {accumulatedSpeed:0.##}", Colors.Black, Colors.White);
                    _lastAlertBar = bar;
                }
            }
            else
            {
                _paintBars[bar] = null;
            }

        }

        private System.Drawing.Color ToDrawingColor(System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Solo capa Final para dibujar sobre el gráfico sin recortes
            if (layout != DrawingLayouts.Final) return;

            if (ChartInfo == null || ChartInfo.PriceChartContainer == null) return;

            var priceContainer = ChartInfo.PriceChartContainer;
            var signals = _signals.ToArray();

            // Definir pinceles sólidos para los marcadores
            var greenBrush = System.Drawing.Color.Lime;
            var redBrush = System.Drawing.Color.Red;

            // Distancia en píxeles desde la mecha hasta el marcador
            int offset = 10;
            // Tamaño del marcador
            int size = 8;

            foreach (var signal in signals)
            {
                // Optimización de visibilidad
                if (signal.Bar > LastVisibleBarNumber || signal.Bar < FirstVisibleBarNumber)
                    continue;

                // Recuperamos la vela real para saber sus extremos exactos
                var candle = GetCandle(signal.Bar);

                // Coordenada X (Centro de la vela)
                int x = priceContainer.GetXByBar(signal.Bar, false);

                // LÓGICA DE DIBUJO: MARCADORES EXTERNOS
                // No dibujamos líneas de precio falso. Marcamos la zona de actividad.

                if (signal.IsBullish)
                {
                    // SEÑAL ALCISTA (Delta +): Dibujar DEBAJO del Low
                    // Esto indica que el impulso viene de abajo y el Low es soporte.
                    int yLow = priceContainer.GetYByPrice(candle.Low, false);
                    int yPos = yLow + offset; // + Y es hacia abajo en pantalla

                    // Dibujar Triángulo apuntando hacia arriba (▲)
                    System.Drawing.Point[] triangle = {
                new System.Drawing.Point(x, yPos - size),       // Punta superior
                new System.Drawing.Point(x - size, yPos + size), // Base izq
                new System.Drawing.Point(x + size, yPos + size)  // Base der
            };
                    context.FillPolygon(greenBrush, triangle);
                }
                else
                {
                    // SEÑAL BAJISTA (Delta -): Dibujar ENCIMA del High
                    // Esto indica que el impulso es bajista y el High es resistencia.
                    int yHigh = priceContainer.GetYByPrice(candle.High, false);
                    int yPos = yHigh - offset; // - Y es hacia arriba en pantalla

                    // Dibujar Triángulo apuntando hacia abajo (▼)
                    System.Drawing.Point[] triangle = {
                new System.Drawing.Point(x, yPos + size),       // Punta inferior
                new System.Drawing.Point(x - size, yPos - size), // Base izq
                new System.Drawing.Point(x + size, yPos - size)  // Base der
            };
                    context.FillPolygon(redBrush, triangle);
                }
            }
        }
    }
}
