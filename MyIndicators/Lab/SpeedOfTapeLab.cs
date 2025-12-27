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

        public enum MarkerShape
        {
            Triangle,
            Circle,
            Diamond,
            Square
        }

        private class SignalLine
        {
            public int Bar { get; set; }
            public bool IsBullish { get; set; }
        }

        #endregion

        #region Fields

        // Almacenamiento de señales para dibujar
        private readonly List<SignalLine> _signals = new List<SignalLine>();

        // Series visuales
        private readonly PaintbarsDataSeries _paintBars = new PaintbarsDataSeries("Paint Bars");
        private readonly ValueDataSeries _renderSeries = new ValueDataSeries("Speed Histogram");
        private readonly ValueDataSeries _thresholdSeries;

        // Indicador interno para calcular el promedio
        private readonly SMA _smaFilter = new SMA();

        // Variables privadas de configuración
        private int _timeWindowSeconds = 15;
        private int _fixedThreshold = 10;
        private Color _alertColor = Color.Yellow;
        private int _lastAlertBar = -1;

        #endregion

        #region Parameters

        // --- FILTROS ---

        [Display(Name = "AutoFiltro", GroupName = "Filtros", Order = 10, Description = "Si está activo, usa un promedio móvil dinámico.")]
        public bool AutoFilter
        {
            get => _autoFilter;
            set { _autoFilter = value; RecalculateValues(); }
        }
        private bool _autoFilter = true;

        [Display(Name = "Periodo de autofiltro", GroupName = "Filtros", Order = 20)]
        [Range(1, int.MaxValue)]
        public int AutoFilterPeriod
        {
            get => _smaFilter.Period;
            set { _smaFilter.Period = value; RecalculateValues(); }
        }

        [Display(Name = "Filtro de tiempo (Seg)", GroupName = "Filtros", Order = 30, Description = "Segundos hacia atrás para calcular la velocidad.")]
        [Range(1, int.MaxValue)]
        public int TimeWindowSec
        {
            get => _timeWindowSeconds;
            set { _timeWindowSeconds = Math.Max(1, value); RecalculateValues(); }
        }

        [Display(Name = "Filtro de transacciones", GroupName = "Filtros", Order = 40)]
        [Range(0, int.MaxValue)]
        public int FixedThreshold
        {
            get => _fixedThreshold;
            set { _fixedThreshold = Math.Max(1, value); RecalculateValues(); }
        }

        [Display(Name = "Tipo de cálculo", GroupName = "Filtros", Order = 50)]
        public SpeedType CalculationType
        {
            get => _type;
            set { _type = value; RecalculateValues(); }
        }
        private SpeedType _type = SpeedType.Ticks;


        // --- VISUALIZACIÓN ---

        [Display(Name = "Pintar Velas (Highlight)", GroupName = "Visualization", Order = 60)]
        public bool ShowPaintBars
        {
            get => _paintBars.Visible;
            set { _paintBars.Visible = value; RedrawChart(); }
        }

        [Display(Name = "Color de Highlight", GroupName = "Visualization", Order = 65)]
        public Color MaxSpeedColor
        {
            get => _alertColor;
            set { _alertColor = value; RecalculateValues(); }
        }

        [Display(Name = "Marker Shape", GroupName = "Visualization", Order = 70)]
        public MarkerShape Shape { get; set; } = MarkerShape.Triangle;

        [Display(Name = "Marker Size", GroupName = "Visualization", Order = 71)]
        [Range(2, 50)]
        public int MarkerSize { get; set; } = 8;

        [Display(Name = "Vertical Offset (Px)", GroupName = "Visualization", Order = 72)]
        [Range(0, 100)]
        public int MarkerOffset { get; set; } = 15;

        [Display(Name = "Buy Signal Color", GroupName = "Visualization", Order = 80)]
        public Color BuyColor { get; set; } = Color.Lime;

        [Display(Name = "Sell Signal Color", GroupName = "Visualization", Order = 81)]
        public Color SellColor { get; set; } = Color.Red;


        // --- ALERTS ---

        [Display(Name = "Use Alerts", GroupName = "Alerts", Order = 110)]
        public bool UseAlerts { get; set; }

        [Display(Name = "Alert File", GroupName = "Alerts", Order = 120)]
        public string AlertFile { get; set; } = "alert1";

        #endregion

        public SpeedOfTapeLab() : base(true)
        {
            SubscribeToDrawingEvents(DrawingLayouts.Final);
            EnableCustomDrawing = true;

            // Configuración de series
            _paintBars.IsHidden = true;

            _renderSeries.VisualType = VisualMode.Histogram;
            _renderSeries.Color = System.Windows.Media.Color.FromRgb(0, 255, 255);
            _renderSeries.ResetAlertsOnNewBar = true;

            _smaFilter.Period = 10;
            _smaFilter.ColoredDirection = false;
            _thresholdSeries = (ValueDataSeries)_smaFilter.DataSeries[0];
            _thresholdSeries.Name = "Filter line";
            _thresholdSeries.Color = System.Windows.Media.Color.FromRgb(173, 216, 230);
            _thresholdSeries.Width = 2;

            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
            DataSeries.Add(_renderSeries);
            DataSeries.Add(_thresholdSeries);
            DataSeries.Add(_paintBars);

            Panel = IndicatorDataProvider.NewPanel;
        }

        protected override void OnRecalculate()
        {
            _signals.Clear();
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // 1. Lógica de la ventana deslizante (INTERPOLACIÓN)
            decimal accumulatedSpeed = 0;
            int lookBackBar = bar;
            var currentCandle = GetCandle(bar);

            while (lookBackBar >= 0)
            {
                var pastCandle = GetCandle(lookBackBar);
                TimeSpan timeDiff = currentCandle.Time - pastCandle.Time;

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
                    // Interpolación lineal si la ventana corta una vela
                    if (timeDiff.TotalSeconds > 0)
                    {
                        accumulatedSpeed = accumulatedSpeed * (decimal)_timeWindowSeconds / (decimal)timeDiff.TotalSeconds;
                    }
                    break;
                }
                lookBackBar--;
            }

            // 2. Cálculo del Filtro
            var filterInput = Math.Abs(accumulatedSpeed);
            _smaFilter.Calculate(bar, filterInput * 1.5m);

            if (!AutoFilter)
            {
                _thresholdSeries[bar] = (decimal)_fixedThreshold;
            }

            _renderSeries[bar] = accumulatedSpeed;

            // 3. Comprobación de disparo
            if (Math.Abs(accumulatedSpeed) > _thresholdSeries[bar])
            {
                _renderSeries.Colors[bar] = _alertColor;

                if (ShowPaintBars)
                    _paintBars[bar] = System.Windows.Media.Color.FromRgb(_alertColor.R, _alertColor.G, _alertColor.B);
                else
                    _paintBars[bar] = null;

                // Guardar señal para dibujo
                var signal = _signals.LastOrDefault(s => s.Bar == bar);
                if (signal == null)
                {
                    signal = new SignalLine { Bar = bar };
                    _signals.Add(signal);
                }

                // Definir dirección (Bullish/Bearish) según el Delta de la vela
                signal.IsBullish = currentCandle.Delta >= 0;

                // Alertas
                if (UseAlerts && bar == CurrentBar - 1 && bar != _lastAlertBar)
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

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (layout != DrawingLayouts.Final || ChartInfo == null || ChartInfo.PriceChartContainer == null) return;

            var priceContainer = ChartInfo.PriceChartContainer;
            var signals = _signals.ToArray();
            int size = MarkerSize;
            int offset = MarkerOffset;

            foreach (var signal in signals)
            {
                if (signal.Bar > LastVisibleBarNumber || signal.Bar < FirstVisibleBarNumber) continue;

                var candle = GetCandle(signal.Bar);
                int x = priceContainer.GetXByBar(signal.Bar, false);
                Color drawColor = signal.IsBullish ? BuyColor : SellColor;

                // Lógica de posición:
                // Bullish (Delta +) -> Debajo del Low, apuntando arriba
                // Bearish (Delta -) -> Encima del High, apuntando abajo

                int yPos;
                if (signal.IsBullish)
                    yPos = priceContainer.GetYByPrice(candle.Low, false) + offset;
                else
                    yPos = priceContainer.GetYByPrice(candle.High, false) - offset;

                DrawMarker(context, x, yPos, size, signal.IsBullish, drawColor);
            }
        }

        private void DrawMarker(RenderContext context, int x, int y, int size, bool isBullish, Color color)
        {
            switch (Shape)
            {
                case MarkerShape.Triangle:
                    if (isBullish) // Apunta arriba
                    {
                        System.Drawing.Point[] p = {
                            new System.Drawing.Point(x, y - size),
                            new System.Drawing.Point(x - size, y + size),
                            new System.Drawing.Point(x + size, y + size)
                        };
                        context.FillPolygon(color, p);
                    }
                    else // Apunta abajo
                    {
                        System.Drawing.Point[] p = {
                            new System.Drawing.Point(x, y + size),
                            new System.Drawing.Point(x - size, y - size),
                            new System.Drawing.Point(x + size, y - size)
                        };
                        context.FillPolygon(color, p);
                    }
                    break;

                case MarkerShape.Circle:
                    // FillEllipse toma un rectangulo que envuelve el circulo
                    var rectC = new Rectangle(x - size, y - size, size * 2, size * 2);
                    context.FillEllipse(color, rectC);
                    break;

                case MarkerShape.Square:
                    var rectS = new Rectangle(x - size, y - size, size * 2, size * 2);
                    context.FillRectangle(color, rectS);
                    break;

                case MarkerShape.Diamond:
                    System.Drawing.Point[] d = {
                        new System.Drawing.Point(x, y - size),      // Top
                        new System.Drawing.Point(x + size, y),      // Right
                        new System.Drawing.Point(x, y + size),      // Bottom
                        new System.Drawing.Point(x - size, y)       // Left
                    };
                    context.FillPolygon(color, d);
                    break;
            }
        }
    }
}
