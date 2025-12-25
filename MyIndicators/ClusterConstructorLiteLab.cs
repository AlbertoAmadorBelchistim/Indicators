using ATAS.Indicators;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace MyIndicators
{
    internal class ClusterInfo
    {
        internal int BarIndex { get; set; }
        internal decimal TopPrice { get; set; }
        internal decimal BottomPrice { get; set; }
    }

    [DisplayName("Cluster Constructor Lite (Lab)")]
    [Category("Order Flow")]
    [Description("Detecta patrones de doble clúster de volumen máximo.")]
    public class ClusterConstructorLab : Indicator
    {
        #region Enums
        public enum CandleRegion
        {
            FullCandle, // Busca en toda la vela
            WicksOnly   // Busca solo en las mechas (fuera del cuerpo)
        }

        public enum ClusterLocation
        {
            All,      // Cualquier ubicación
            Nearby,   // Los dos clústers deben estar pegados
            Separate  // Los dos clústers deben estar separados
        }
        #endregion

        #region Fields

        private readonly PaintbarsDataSeries _signalSeries = new PaintbarsDataSeries("Signal Bars");
        private readonly List<ClusterInfo> _signalClusters = new();

        // Cache de alerta
        private int _lastProcessedBar;
        private bool _lastBarProcessed;
        private int _targetBarIndex;
        private decimal _priceTickSize;

        #endregion

        #region Parameters

        private bool _searchDoubleMax = true;

        [Display(Name = "Search Pattern", GroupName = "Filters", Order = 10)]
        public bool SearchDoubleMax
        {
            get => _searchDoubleMax;
            set
            {
                _searchDoubleMax = value;
                RecalculateValues();
            }
        }


        private CandleRegion _region = CandleRegion.FullCandle;

        [Display(Name = "Search Area", GroupName = "Filters", Order = 20)]
        public CandleRegion Region
        {
            get => _region;
            set
            {
                _region = value;
                RecalculateValues();
            }
        }

        private ClusterLocation _position = ClusterLocation.All;

        [Display(Name = "Cluster Position", GroupName = "Filters", Order = 30)]
        public ClusterLocation Position
        {
            get => _position;
            set
            {
                _position = value;
                RecalculateValues();
            }
        }


        private int _minVolume = 0;

        [Display(Name = "Min Volume", GroupName = "Filters", Order = 40)]
        public int MinVolume
        {
            get => _minVolume;
            set
            {
                if (value < 0) return;
                _minVolume = value;
                RecalculateValues();
            }
        }

        private int _maxVolume = 100000;

        [Display(Name = "Max Volume", GroupName = "Filters", Order = 50)]
        public int MaxVolume
        {
            get => _maxVolume;
            set
            {
                if (value < 0) return;
                _maxVolume = value;
                RecalculateValues();
            }
        }


        private System.Windows.Media.Color _signalColor = Colors.Blue;

        [Display(Name = "Signal Color", GroupName = "Visuals", Order = 60)]
        public System.Windows.Media.Color SignalColor
        {
            get => _signalColor;
            set
            {
                _signalColor = value;
                RecalculateValues();
            }
        }


        [Display(Name = "Use Alerts", GroupName = "Alerts", Order = 70)]
        public bool UseAlerts { get; set; }

        [Display(Name = "Alert File", GroupName = "Alerts", Order = 80)]
        public string AlertFile { get; set; } = "alert1";

        #endregion

        public ClusterConstructorLab()
            : base(true)
        {
            _lastProcessedBar = 0;

            DataSeries[0] = _signalSeries;
            _signalSeries.IsHidden = true; // match decompiled
            DenyToChangePanel = true;

            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);
        }


        protected override void OnCalculate(int barIndex, decimal value)
        {
            if (barIndex == 0)
            {
                _signalClusters.Clear();

                _priceTickSize = InstrumentInfo.TickSize;
                _lastBarProcessed = false;
                _targetBarIndex = 0;
                _lastProcessedBar = 0;
            }
            else
            {
                if (barIndex < _targetBarIndex)
                    return;

                var candle = GetCandle(barIndex);

                if (_lastProcessedBar == barIndex)
                {
                    _lastBarProcessed = true;
                    _signalSeries[barIndex] = null;
                }
                else
                {
                    _lastProcessedBar = barIndex;

                    if (_lastBarProcessed && UseAlerts && _signalSeries[barIndex - 1].HasValue)
                        AddAlert(AlertFile, "Appropriate bar detected");
                }

                if (SearchDoubleMax && !CheckDoubleMaxVolumeClusters(candle))
                    return;

                _signalSeries[barIndex] = SignalColor;
            }
        }



        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (layout != DrawingLayouts.Final)
                return;

            foreach (var cluster in _signalClusters)
            {
                int x = ChartInfo.GetXByBar(cluster.BarIndex);
                int y = ChartInfo.GetYByPrice(cluster.TopPrice);
                int width = (int)ChartInfo.PriceChartContainer.BarsWidth;
                int height = (int)(ChartInfo.GetYByPrice(cluster.BottomPrice) + ChartInfo.PriceChartContainer.PriceRowHeight - y);
                context.DrawRectangle(new RenderPen(SignalColor.Convert(), 2f), new Rectangle(x, y, width, height));
            }
        }


        private bool CheckDoubleMaxVolumeClusters(IndicatorCandle candle)
        {
            var levels = candle.GetAllPriceLevels().ToList();
            if (levels.Count == 0)
                return false;

            decimal maxVolume = levels.Max(p => p.Volume);

            if (maxVolume < MinVolume || maxVolume > MaxVolume)
                return false;

            decimal tickSize = _priceTickSize;

            decimal low = candle.Low;
            decimal high = candle.High;

            // WicksOnly ranges (decompiled splits lower/upper wick search)
            decimal bodyBottom = Math.Min(candle.Open, candle.Close);
            decimal bodyTop = Math.Max(candle.Open, candle.Close);

            decimal lowerWickStart = low;
            decimal lowerWickEnd = bodyBottom - tickSize;

            decimal upperWickStart = bodyTop + tickSize;
            decimal upperWickEnd = high;

            switch (Region)
            {
                case CandleRegion.FullCandle:
                    return FindDoubleMaxVolumeClusters(candle, maxVolume, low, high, tickSize);

                case CandleRegion.WicksOnly:
                    // In decompiled: the "Nearby" check is evaluated separately for lower/upper wick.
                    // "All" still requires exactly 2 max-volume levels in the filtered region.
                    {
                        // Count max-vol levels in wicks only
                        int wickMaxCount = levels.Count(l =>
                        {
                            if (l.Volume != maxVolume)
                                return false;
                            return l.Price < bodyBottom || l.Price > bodyTop;
                        });

                        if (Position == ClusterLocation.All)
                            return wickMaxCount == 2;

                        bool nearbyLower = lowerWickStart <= lowerWickEnd
                            && FindNearbyMaxVolumeClusters(candle, maxVolume, lowerWickStart, lowerWickEnd, tickSize);

                        bool nearbyUpper = upperWickStart <= upperWickEnd
                            && FindNearbyMaxVolumeClusters(candle, maxVolume, upperWickStart, upperWickEnd, tickSize);

                        if (Position == ClusterLocation.Nearby)
                            return nearbyLower || nearbyUpper;

                        if (Position == ClusterLocation.Separate)
                            return wickMaxCount == 2 && !nearbyLower && !nearbyUpper;

                        return false;
                    }

                default:
                    return false;
            }
        }

        private bool FindDoubleMaxVolumeClusters(
    IndicatorCandle candle,
    decimal maxVolume,
    decimal start,
    decimal end,
    decimal tickSize)
        {
            // Decompiled: source = all price levels where Volume == maxVolume (whole candle)
            var source = candle.GetAllPriceLevels().Where(pvi => pvi.Volume == maxVolume);

            switch (Position)
            {
                case ClusterLocation.All:
                    return source.Count() == 2;

                case ClusterLocation.Nearby:
                    return source.Count() == 2 && FindNearbyMaxVolumeClusters(candle, maxVolume, start, end, tickSize);

                case ClusterLocation.Separate:
                    return source.Count() == 2 && !FindNearbyMaxVolumeClusters(candle, maxVolume, start, end, tickSize);

                default:
                    return false;
            }
        }


        private bool FindNearbyMaxVolumeClusters(
            IndicatorCandle candle,
            decimal maxVolume,
            decimal start,
            decimal end,
            decimal tickSize)
        {
            // Decompiled logic: scan each price level in the range and track longest streak of maxVolume
            int streak = 0;
            int maxStreak = 0;

            decimal price = start;

            while (price <= end)
            {
                var pvi = candle.GetPriceVolumeInfo(price);

                if (pvi == null)
                {
                    streak = 0;
                }
                else if (pvi.Volume == maxVolume)
                {
                    streak++;
                    maxStreak = Math.Max(streak, maxStreak);
                }
                else
                {
                    streak = 0;
                }

                price += tickSize;
            }

            // Decompiled requirement: EXACTLY 2 consecutive max-volume levels
            return maxStreak == 2;
        }


    }
}
