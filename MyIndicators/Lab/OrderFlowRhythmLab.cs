using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Windows.Media;
using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical;
using OFT.Attributes;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Color = System.Drawing.Color;
using Brush = System.Drawing.Brush;


namespace ATAS.Indicators.Technical

{

    [DisplayName("Order Flow Rhythm (Lab)")]
    [Category("Custom")]
    [Description("Entorno de pruebas para simular el funcionamiento de Order FLow Rhythm.")]

    public class OrderFlowRhythmLab : Indicator

    {

        #region Enums

        public enum RhythmMode { Volume, BidAsk }
        public enum RhythmPalette { ColdToHot, RedToGreen, Grayscale }

        #endregion

        #region Fields

        // Motores de cálculo internos

        private readonly SpeedOfTape _speedVol = new SpeedOfTape();
        private readonly SpeedOfTape _speedBuy = new SpeedOfTape();
        private readonly SpeedOfTape _speedSell = new SpeedOfTape();


        // SMA interna para replicar el lag del original

        private readonly SMA _smaVol = new SMA();
        private readonly SMA _smaBuy = new SMA();
        private readonly SMA _smaSell = new SMA();



        // Series de datos para renderizado (Ocultas de la UI)

        private readonly ValueDataSeries _volValues = new ValueDataSeries("VolSpeed");
        private readonly ValueDataSeries _buyValues = new ValueDataSeries("BuySpeed");
        private readonly ValueDataSeries _sellValues = new ValueDataSeries("SellSpeed");



        // Optimización de render

        private Color[] _paletteCache;
        private RhythmPalette _lastPalette;
        private int _lastContrast = -1;

        #endregion

        #region Parameters

        [Display(Name = "Display Mode", GroupName = "Engine Core", Order = 10)]

        public RhythmMode Mode { get; set; } = RhythmMode.Volume;


        [Display(Name = "Period (Seconds/Bars)", GroupName = "Engine Core", Order = 20, Description = "Define la ventana de arrastre.")]
        [Range(1, 10000)]

        public int Period

        {

            get => _speedVol.AutoFilterPeriod;
            
            set
            {

                // Propagamos el periodo a los motores internos
                _speedVol.AutoFilterPeriod = value;
                _speedBuy.AutoFilterPeriod = value;
                _speedSell.AutoFilterPeriod = value;

                // También ajustamos el suavizado para replicar el comportamiento de ventana

                _smaVol.Period = value;
                _smaBuy.Period = value;
                _smaSell.Period = value;

                RecalculateValues();

            }

        }


        [Display(Name = "Heatmap Palette", GroupName = "Visuals", Order = 30)]
        public RhythmPalette PaletteType { get; set; } = RhythmPalette.ColdToHot;


        [Display(Name = "Upper Cutoff %", GroupName = "Visuals", Order = 40)]
        [Range(0, 90)]

        public int UpperCutoff { get; set; } = 0;


        [Display(Name = "Contrast", GroupName = "Visuals", Order = 50)]
        [Range(0, 100)]

        public int Contrast { get; set; } = 0;


        #endregion



        public OrderFlowRhythmLab() : base(true)

        {

            // 1. UI: Forzamos panel nuevo
            Panel = IndicatorDataProvider.NewPanel;


            // 2. Configuración Motores
            _speedVol.Type = SpeedOfTape.SpeedOfTapeType.Volume;
            _speedBuy.Type = SpeedOfTape.SpeedOfTapeType.Buys;
            _speedSell.Type = SpeedOfTape.SpeedOfTapeType.Sells;


            // 3. UI: Limpieza radical de series expuestas
            // Ocultamos las series de los indicadores hijos

            ((ValueDataSeries)_speedVol.DataSeries[0]).VisualType = VisualMode.Hide;
            ((ValueDataSeries)_speedBuy.DataSeries[0]).VisualType = VisualMode.Hide;
            ((ValueDataSeries)_speedSell.DataSeries[0]).VisualType = VisualMode.Hide;



            // Ocultamos nuestras propias series de la lista de indicadores y del gráfico

            _volValues.VisualType = VisualMode.Hide;
            _buyValues.VisualType = VisualMode.Hide;
            _sellValues.VisualType = VisualMode.Hide;
            



            // Nota: ATAS muestra en la UI las DataSeries agregadas. 
            // Al ponerlas en VisualMode.Hide no se pintan, pero aparecen en la lista.
            // Para que no molesten, las definimos sin nombre o controlamos su visibilidad.


            Add(_speedVol); Add(_speedBuy); Add(_speedSell);
            DataSeries.Add(_volValues);
            DataSeries.Add(_buyValues);
            DataSeries.Add(_sellValues);
            DataSeries[0].IsHidden = true;
            DataSeries[1].IsHidden = true;
            DataSeries[2].IsHidden = true;
            DataSeries[3].IsHidden = true;



            EnableCustomDrawing = true;

            SubscribeToDrawingEvents(DrawingLayouts.Final);

            Period = 10; // Valor por defecto

        }



        protected override void OnInitialize()

        {

            UpdatePalette();

        }



        protected override void OnCalculate(int bar, decimal value)

        {

            // 1. Obtener Velocidad Base (Instantánea)

            var rawVol = ((ValueDataSeries)_speedVol.DataSeries[0])[bar];
            var rawBuy = ((ValueDataSeries)_speedBuy.DataSeries[0])[bar];
            var rawSell = ((ValueDataSeries)_speedSell.DataSeries[0])[bar];



            // El SMA de ATAS requiere calcularse secuencialmente

            _volValues[bar] = _smaVol.Calculate(bar, rawVol);
            _buyValues[bar] = _smaBuy.Calculate(bar, rawBuy);
            _sellValues[bar] = _smaSell.Calculate(bar, rawSell);

        }



        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (ChartInfo == null || DataSeries == null) return;

            Rectangle rectContainer = Container.Region;
            if (rectContainer.Height <= 0) return;

            int start = FirstVisibleBarNumber;
            int end = LastVisibleBarNumber;

            if (start < 0 || end >= _volValues.Count) return;

            // 1. Normalización Dinámica (Auto-Scale)
            decimal maxVal = 0;

            // Optimizamos el bucle de búsqueda de máximos
            var searchSeries = (Mode == RhythmMode.Volume) ? _volValues : null;

            if (Mode == RhythmMode.Volume)
            {
                for (int i = start; i <= end; i++)
                    if (_volValues[i] > maxVal) maxVal = _volValues[i];
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    if (_buyValues[i] > maxVal) maxVal = _buyValues[i];
                    if (_sellValues[i] > maxVal) maxVal = _sellValues[i];
                }
            }

            decimal effectiveMax = maxVal;
            if (UpperCutoff > 0 && maxVal > 0)
                effectiveMax = maxVal * (1.0m - (UpperCutoff / 100.0m));

            if (effectiveMax <= 0) effectiveMax = 1;

            // Convertimos a double una sola vez fuera del bucle
            double maxValDouble = (double)effectiveMax;

            // 2. Dibujado Pixel-Perfect
            int halfHeight = rectContainer.Height / 2;
            int yTop = rectContainer.Y;
            int yMid = rectContainer.Y + halfHeight;

            // Actualizamos la paleta (solo si cambió algo)
            UpdatePalette();

            for (int i = start; i <= end; i++)
            {
                int x = ChartInfo.GetXByBar(i);
                int w = (int)ChartInfo.PriceChartContainer.BarsWidth + 1;

                if (x + w < 0 || x > rectContainer.Right) continue;

                if (Mode == RhythmMode.Volume)
                {
                    // GetHeatmapColor ahora es instantáneo (lectura de array)
                    Color c = GetHeatmapColor(_volValues[i], maxValDouble);
                    context.FillRectangle(c, new Rectangle(x, yTop, w, rectContainer.Height));
                }
                else
                {
                    Color cBuy = GetHeatmapColor(_buyValues[i], maxValDouble);
                    Color cSell = GetHeatmapColor(_sellValues[i], maxValDouble);

                    context.FillRectangle(cBuy, new Rectangle(x, yTop, w, halfHeight));
                    context.FillRectangle(cSell, new Rectangle(x, yMid, w, halfHeight));
                }
            }
        }



        #region Color Engine Optimizado

        private void UpdatePalette()
        {
            // Solo recalculamos si cambia el tipo de paleta o el valor de contraste
            if (_paletteCache != null && _lastPalette == PaletteType && _lastContrast == Contrast) return;

            int steps = 256;
            _paletteCache = new Color[steps];
            _lastPalette = PaletteType;
            _lastContrast = Contrast;

            // Calculamos el factor Gamma una sola vez
            double gamma = 1.0;
            if (Contrast > 0) gamma = 1.0 - (Contrast / 120.0);

            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / (steps - 1);

                // Aplicamos el contraste AQUÍ (Pre-cálculo)
                double t_corrected = (Contrast > 0) ? Math.Pow(t, gamma) : t;

                _paletteCache[i] = InterpolateColor(t_corrected, PaletteType);
            }
        }

        private Color GetHeatmapColor(decimal value, double max)
        {
            if (value <= 0) return _paletteCache[0];

            double ratio = (double)value / max;

            // Clamp simple
            if (ratio >= 1.0) return _paletteCache[255];
            if (ratio <= 0.0) return _paletteCache[0];

            // Mapeo directo al array pre-calculado (Velocidad extrema)
            int index = (int)(ratio * 255);
            return _paletteCache[index];
        }

        private Color InterpolateColor(double t, RhythmPalette type)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;

            if (type == RhythmPalette.ColdToHot)
            {
                if (t < 0.33) return Blend(Color.FromArgb(255, 10, 10, 40), Color.Blue, t / 0.33);
                else if (t < 0.66) return Blend(Color.Blue, Color.Cyan, (t - 0.33) / 0.33);
                else if (t < 0.9) return Blend(Color.Cyan, Color.Yellow, (t - 0.66) / 0.24);
                else return Blend(Color.Yellow, Color.White, (t - 0.9) / 0.1);
            }
            else if (type == RhythmPalette.Grayscale)
            {
                int v = (int)(t * 255);
                return Color.FromArgb(255, v, v, v);
            }
            else // RedToGreen
            {
                if (t < 0.5) return Blend(Color.Red, Color.Black, t * 2);
                return Blend(Color.Black, Color.Lime, (t - 0.5) * 2);
            }
        }

        private Color Blend(Color c1, Color c2, double t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return Color.FromArgb(255,
                (byte)(c1.R + (c2.R - c1.R) * t),
                (byte)(c1.G + (c2.G - c1.G) * t),
                (byte)(c1.B + (c2.B - c1.B) * t));
        }

        #endregion

    }

}
