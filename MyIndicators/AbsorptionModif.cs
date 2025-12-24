using ATAS.DataFeedsCore;
using ATAS.Indicators.Drawing;
using OFT.Attributes;
using OFT.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

#nullable disable

namespace ATAS.Indicators.Technical
{
    [Category("Volume & OrderFlow")]
    [DisplayName("Absorption Modif")]
    [Display(ResourceType = typeof(Strings), Description = "AbsorptionIndDescription")]
    [HelpLink("https://help.atas.net/support/solutions/articles/72000641183-absorption")]
    public class AbsorptionModif : Indicator
    {
        private readonly Pen _bearPen;
        private readonly Pen _bullPen;

        private int _lastProcessedBar = -1;      
        private int _startBar = 0;              
        private bool _allowAlertsThisTick = true; 
        private int _lastNewLevelAlertBar = -1;  
        private int _lastCrossAlertBar = -1;     
        private decimal _prevClose = 0m;         
        private readonly HashSet<decimal> _crossAlertedPrices = new();

        private int _absorptionRange = 1;        
        private int _absorptionRatio = 300;      // Ratio (300 => 3:1)
        private int _absorptionVolume = 10;      // Min dominant volume
        private int _days = 90;                  // Days lookback (sessions)
        private bool _calculateLastBar = false;

        private Color _bearColor = Color.FromArgb(90, 255, 152, 0);
        private Color _bullColor = Color.FromArgb(90, 0, 255, 255);
        private int _lineWidth = 5;

        private bool _useAlerts = false;
        private bool _useCrossAlerts = false;
        private string _alertFile = "alert1";

        public AbsorptionModif()
            : base(true)
        {
            DenyToChangePanel = true;

            _bullPen = new Pen(_bullColor, _lineWidth);
            _bearPen = new Pen(_bearColor, _lineWidth);

            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
        }

        // ----------------------------
        // Visualization
        // ----------------------------

        [Display(ResourceType = typeof(Strings), GroupName = "Visualization", Name = "BullishColor", Description = "BullishColorDescription", Order = 110)]
        public Color BullAbsorptionColor
        {
            get => _bullColor;
            set
            {
                _bullColor = value;
                _bullPen.Color = value;

                // Update already drawn lines (only Buy context lines).
                for (int i = 0; i < HorizontalLinesTillTouch.Count; i++)
                {
                    var line = HorizontalLinesTillTouch[i];
                    if (line?.Context is OrderDirections dir && dir == OrderDirections.Buy)
                        line.Pen = _bullPen;
                }

                OnChangeProperty(nameof(BullAbsorptionColor));
            }
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Visualization", Name = "BearlishColor", Description = "BearishColorDescription", Order = 120)]
        public Color BearAbsorptionColor
        {
            get => _bearColor;
            set
            {
                _bearColor = value;
                _bearPen.Color = value;

                // Update already drawn lines (only Sell context lines).
                for (int i = 0; i < HorizontalLinesTillTouch.Count; i++)
                {
                    var line = HorizontalLinesTillTouch[i];
                    if (line?.Context is OrderDirections dir && dir == OrderDirections.Sell)
                        line.Pen = _bearPen;
                }

                OnChangeProperty(nameof(BearAbsorptionColor));
            }
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Visualization", Name = "LineWidth", Description = "LineWidthDescription", Order = 125)]
        [Range(1, 100)]
        public int LineWidth
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;

                // Line thickness controlled by pen widths.
                _bearPen.Width = value;
                _bullPen.Width = value;

                for (int i = 0; i < HorizontalLinesTillTouch.Count; i++)
                {
                    if (HorizontalLinesTillTouch[i]?.Pen != null)
                        HorizontalLinesTillTouch[i].Pen.Width = value;
                }

                OnChangeProperty(nameof(LineWidth));
            }
        }

        // ----------------------------
        // Calculation
        // ----------------------------

        [Display(ResourceType = typeof(Strings), GroupName = "Calculation", Name = "DaysLookBack", Description = "DaysLookBackDescription", Order = 200)]
        [Range(0, 2000)]
        public int Days
        {
            get => _days;
            set
            {
                _days = value;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Calculation", Name = "Ratio", Description = "MinRatioDescription", Order = 210)]
        [Range(0, 100000)]
        public int AbsorptionRatio
        {
            get => _absorptionRatio;
            set
            {
                _absorptionRatio = value;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Calculation", Name = "StackedLevels", Description = "StackedLevelsDescription", Order = 220)]
        [Range(0, 100000)]
        public int AbsorptionRange
        {
            get => _absorptionRange;
            set
            {
                _absorptionRange = value;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Calculation", Name = "MinVolume", Description = "MinVolumeFilterCommonDescription", Order = 230)]
        [Range(0, 100000)]
        public int AbsorptionVolume
        {
            get => _absorptionVolume;
            set
            {
                _absorptionVolume = value;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Calculation", Name = "LastBar", Description = "CalculateLastBarDescription", Order = 240)]
        public bool CalculateLastBar
        {
            get => _calculateLastBar;
            set
            {
                _calculateLastBar = value;
                RecalculateValues();
            }
        }

        // ----------------------------
        // Alerts
        // ----------------------------

        [Display(ResourceType = typeof(Strings), GroupName = "Alerts", Name = "UseAlert", Description = "UseAlertsDescription", Order = 300)]
        public bool UseAlerts
        {
            get => _useAlerts;
            set => _useAlerts = value;
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Alerts", Name = "ApproximationAlert", Description = "IsApproximationAlertDescription", Order = 310)]
        public bool UseCrossAlerts
        {
            get => _useCrossAlerts;
            set => _useCrossAlerts = value;
        }

        [Display(ResourceType = typeof(Strings), GroupName = "Alerts", Name = "AlertFile", Description = "AlertFileDescription", Order = 320)]
        public string AlertFile
        {
            get => _alertFile;
            set => _alertFile = value;
        }

        // ----------------------------
        // Core logic
        // ----------------------------

        protected override void OnCalculate(int bar, decimal value)
        {
            // 1) Initialization on first bar call.
            if (bar == 0)
            {
                _crossAlertedPrices.Clear();
                _prevClose = 0m;
                _allowAlertsThisTick = false;
                _startBar = 0;

                // Compute start bar based on session count (Days).
                if (_days > 0)
                {
                    int sessions = 0;
                    _startBar = 0;

                    for (int b = CurrentBar - 1; b >= 0; b--)
                    {
                        _startBar = b;

                        if (IsNewSession(b))
                        {
                            sessions++;
                            if (sessions == _days)
                                break;
                        }
                    }
                }

                return;
            }

            // 2) Ignore anything older than the start bar (lookback by sessions).
            if (bar < _startBar)
                return;

            // 3) Determine the target bar to process.
            //    If CalculateLastBar=false, we process only closed bars => bar-1.
            //    If CalculateLastBar=true, we process current bar (including forming bar).
            int targetBar = _calculateLastBar ? bar : bar - 1;
            if (targetBar < 0)
                return;

            if (targetBar < _startBar)
                return;

            // 4) Rebuild logic: if we are on the same bar as last time and CalculateLastBar is false, do nothing.
            //    If CalculateLastBar is true, we allow refresh on the forming bar.
            if (!_calculateLastBar && bar == _lastProcessedBar)
                return;

            // When target bar changes (or last bar is enabled), redraw levels for targetBar.
            if (bar != _lastProcessedBar || _calculateLastBar)
            {
                // Remove previously drawn levels created for targetBar to avoid duplicates.
                for (int i = HorizontalLinesTillTouch.Count - 1; i >= 0; i--)
                {
                    if (HorizontalLinesTillTouch[i].FirstBar == targetBar)
                        HorizontalLinesTillTouch.RemoveAt(i);
                }

                _lastProcessedBar = bar;
                _crossAlertedPrices.Clear();

                int linesBefore = HorizontalLinesTillTouch.Count;

                var ladder = BuildPriceLadder(targetBar);

                DrawSellAbsorptionLevels(targetBar, ladder); // resistance (Sell context)
                DrawBuyAbsorptionLevels(targetBar, ladder);  // support (Buy context)

                int linesAfter = HorizontalLinesTillTouch.Count;

                // New-level alert: trigger once, only on the most recent closed bar.
                if (_useAlerts && _allowAlertsThisTick && _lastNewLevelAlertBar != bar && bar == CurrentBar - 1)
                {
                    if (linesAfter > linesBefore)
                    {
                        _lastNewLevelAlertBar = bar;
                        AddAlert(_alertFile, "Absorption level(s) detected");
                        _allowAlertsThisTick = false;
                    }
                }
            }

            _allowAlertsThisTick = true;

            // 5) Cross alerts: detect if close crosses any existing level between previous close and current close.
            decimal close = GetCandle(bar).Close;

            if (_useCrossAlerts && _prevClose != 0m && bar == CurrentBar - 1 && _lastCrossAlertBar != bar)
            {

                for (int i = 0; i < HorizontalLinesTillTouch.Count; i++)
                {
                    var line = HorizontalLinesTillTouch[i];

                    if (line == null)
                        continue;

                    if (line.SecondBar < bar)
                        continue;

                    decimal price = line.FirstPrice;

                    if (_crossAlertedPrices.Contains(price))
                        continue;

                    bool crossed =
                        (_prevClose < price && close >= price) ||
                        (_prevClose > price && close <= price);

                    if (crossed)
                    {
                        _lastCrossAlertBar = bar;
                        AddAlert(_alertFile, $"Absorption level crossed: {price}");
                        _crossAlertedPrices.Add(price);
                    }
                }
            }

            _prevClose = close;
        }

        /// <summary>
        /// Builds a ladder of [price, bid, ask] for the candle.
        /// </summary>
        private List<decimal[]> BuildPriceLadder(int bar)
        {
            var candle = GetCandle(bar);
            var list = new List<decimal[]>(128);

            for (decimal price = candle.Low; price <= candle.High; price += InstrumentInfo.TickSize)
            {
                var info = candle.GetPriceVolumeInfo(price);
                if (info == null)
                    continue;

                list.Add(new[] { price, info.Bid, info.Ask });
            }

            return list;
        }

        /// <summary>
        /// SELL absorption: detects diagonal sell-imbalance and draws levels above Close with Sell context.
        /// Condition (diagonal): Ask(i+1) > Bid(i) * ratio AND Ask(i+1) > minVol
        /// Close-hold filter: price > Close
        /// </summary>
        private void DrawSellAbsorptionLevels(int bar, List<decimal[]> ladder)
        {
            if (ladder == null || ladder.Count < 2 || _absorptionRange <= 0)
                return;

            // Flag array refers to index i (lower level), derived from pair (i, i+1).
            bool[] flags = new bool[ladder.Count];

            for (int i = 0; i < ladder.Count - 1; i++)
            {
                decimal bidLower = ladder[i][1];
                decimal askUpper = ladder[i + 1][2];

                decimal threshold = bidLower * _absorptionRatio / 100m;

                // Dominance must exceed ratio threshold and min volume.
                if (askUpper > threshold && askUpper > _absorptionVolume)
                    flags[i] = true;
            }

            decimal close = GetCandle(bar).Close;

            // Extract runs of consecutive flags and draw.
            int runLen = 0;
            int runStart = -1;

            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    if (runLen == 0) runStart = i;
                    runLen++;
                }
                else
                {
                    FlushSellRun(bar, ladder, close, runStart, runLen);
                    runLen = 0;
                    runStart = -1;
                }
            }

            // Fix: flush last run if it reaches end.
            FlushSellRun(bar, ladder, close, runStart, runLen);
        }

        private void FlushSellRun(int bar, List<decimal[]> ladder, decimal close, int runStart, int runLen)
        {
            if (runLen < _absorptionRange || runStart < 0)
                return;

            int runEnd = runStart + runLen - 1;

            for (int idx = runStart; idx <= runEnd; idx++)
            {
                decimal price = ladder[idx][0];

                // Close-hold filter: only keep levels ABOVE the close => "not crossed at close".
                if (price <= close)
                    continue;

                HorizontalLinesTillTouch.Add(new LineTillTouch(bar, price, _bearPen)
                {
                    Context = OrderDirections.Sell
                });
            }
        }

        /// <summary>
        /// BUY absorption: detects diagonal buy-imbalance and draws levels at/below Close with Buy context.
        /// Condition (diagonal): Bid(i) > Ask(i+1) * ratio AND Bid(i) > minVol
        /// Close-hold filter: price <= Close
        /// </summary>
        private void DrawBuyAbsorptionLevels(int bar, List<decimal[]> ladder)
        {
            if (ladder == null || ladder.Count < 2 || _absorptionRange <= 0)
                return;

            bool[] flags = new bool[ladder.Count];

            for (int i = 0; i < ladder.Count - 1; i++)
            {
                decimal bidLower = ladder[i][1];
                decimal askUpper = ladder[i + 1][2];

                decimal threshold = askUpper * _absorptionRatio / 100m;

                if (bidLower > threshold && bidLower > _absorptionVolume)
                    flags[i] = true;
            }

            decimal close = GetCandle(bar).Close;

            int runLen = 0;
            int runStart = -1;

            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    if (runLen == 0) runStart = i;
                    runLen++;
                }
                else
                {
                    FlushBuyRun(bar, ladder, close, runStart, runLen);
                    runLen = 0;
                    runStart = -1;
                }
            }

            FlushBuyRun(bar, ladder, close, runStart, runLen);
        }

        private void FlushBuyRun(int bar, List<decimal[]> ladder, decimal close, int runStart, int runLen)
        {
            if (runLen < _absorptionRange || runStart < 0)
                return;

            int runEnd = runStart + runLen - 1;

            for (int idx = runStart; idx <= runEnd; idx++)
            {
                decimal price = ladder[idx][0];

                // Close-hold filter: only keep levels AT/BELOW the close => "not crossed at close".
                if (price > close)
                    continue;

                HorizontalLinesTillTouch.Add(new LineTillTouch(bar, price, _bullPen)
                {
                    Context = OrderDirections.Buy
                });
            }
        }
    }
}
