namespace MyIndicators
{
    using ATAS.DataFeedsCore;
    using ATAS.Indicators;
    using OFT.Rendering.Settings;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Windows.Media;

    [Category("Custom")]
    [DisplayName("RiskRails Minimal")]
    public class RiskRailsMinimal : Indicator
    {
        // ====== STAGE 3 ======
        // - Equity: live (scale-on)
        // - DailyTarget / DailyLoss: fixed per day from the first valid tick (scale-on)
        // - AccountGoal / AccountFloor: rails that DO NOT open the scale (scale-off)

        // --- INPUTS ---
        [Display(Name = "Account ID (empty = selected)")]
        public string AccountId { get; set; } = string.Empty;

        [Display(Name = "Starting Balance")]
        public decimal StartingBalance { get; set; } = 50000m;

        [Display(Name = "Daily Target Offset (+)")]
        public decimal DailyTargetOffset { get; set; } = 300m;

        [Display(Name = "Daily Loss Offset (−)")]
        public decimal DailyLossOffset { get; set; } = 300m;

        [Display(Name = "Account Goal Offset (+)")]
        public decimal AccountGoalOffset { get; set; } = 3000m;

        [Display(Name = "Max Loss (absolute)")]
        public decimal MaxLoss { get; set; } = 2500m;

        // --- VISIBILITY TOGGLES ---
        [Display(Name = "Show Equity")]
        public bool ShowEquity { get; set; } = true;

        [Display(Name = "Show Daily Target")]
        public bool ShowDailyTarget { get; set; } = true;

        [Display(Name = "Show Daily Loss")]
        public bool ShowDailyLoss { get; set; } = true;

        [Display(Name = "Show Account Goal")]
        public bool ShowAccountGoal { get; set; } = true;

        [Display(Name = "Show Account Floor")]
        public bool ShowAccountFloor { get; set; } = true;

        [Display(Name = "Manual Floor ($)")]
        public decimal ManualFloor { get; set; } = 0m;

        [Display(Name = "Apply Manual Floor NOW")]
        public bool ApplyManualFloorNow { get; set; } = false;

        // --- SERIES ---
        private ValueDataSeries _equity;        // scale-on
        private ValueDataSeries _dailyTarget;   // scale-on (fixed for the day)
        private ValueDataSeries _dailyLoss;     // scale-on (fixed for the day)
        private ValueDataSeries _accountGoal;   // scale-off
        private ValueDataSeries _accountFloor;  // scale-off

        // --- PERSISTENCE (per account) ---
        [Display(Name = "State path (JSON)")]
        public string StatePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ATAS", "Exports", "riskrails_state.json");

        private sealed class State
        {
            public decimal SavedMaxEquity { get; set; }     // only increases
            public string DailyAnchorDate { get; set; }     // "yyyy-MM-dd"
            public decimal DailyAnchorEquity { get; set; }  // anchor for daily rails
        }

        private readonly Dictionary<string, State> _perAccount =
            new(StringComparer.OrdinalIgnoreCase);

        // --- RUNTIME GUARDS ---
        private int _startBar = -1;           // draw only from here forward
        private string _lastAccountId = "";   // detect account switch
        private int _warmupBars = 0;          // small cooldown after account change
        private const int WarmupOnSwitch = 2; // #bars to wait to avoid writing zeros
        private bool _needBackfill = false;  // necesitamos rellenar todo [0..bar] al primer tick válido tras el switch

        private static string GetAccountKey(Portfolio p) =>
            string.IsNullOrWhiteSpace(p.AccountID) ? "default" : p.AccountID.Trim();

        public RiskRailsMinimal()
        {
            Panel = IndicatorDataProvider.NewPanel;
            DenyToChangePanel = true;

            _equity = new ValueDataSeries("Equity")
            {
                VisualType = VisualMode.Line,
                Color = Colors.White,
                Width = 2,
                Digits = 2,
                ScaleIt = true
            };

            _dailyTarget = new ValueDataSeries("DailyTarget")
            {
                VisualType = VisualMode.Line,
                Color = Colors.DeepSkyBlue,
                Width = 1,
                Digits = 2,
                ScaleIt = true
            };

            _dailyLoss = new ValueDataSeries("DailyLoss")
            {
                VisualType = VisualMode.Line,
                Color = Colors.HotPink,
                Width = 1,
                Digits = 2,
                ScaleIt = true
            };

            _accountGoal = new ValueDataSeries("AccountGoal")
            {
                VisualType = VisualMode.Line,
                Color = Colors.Orange,
                Width = 1,
                Digits = 2,
                ScaleIt = false // do not open the scale
            };

            _accountFloor = new ValueDataSeries("AccountFloor")
            {
                VisualType = VisualMode.Line,
                Color = Colors.MediumPurple,
                Width = 1,
                Digits = 2,
                ScaleIt = false // do not open the scale
            };

            DataSeries[0] = _equity;
            DataSeries.Add(_dailyTarget);
            DataSeries.Add(_dailyLoss);
            DataSeries.Add(_accountGoal);
            DataSeries.Add(_accountFloor);
        }

        protected override void OnInitialize()
        {
            LoadState();

            var (p0, k0) = ResolvePortfolio();
            if (p0 != null)
            {
                var equity0 = p0.Balance + p0.OpenPnL;
                EnsureStateExists(k0, equity0);
                DebugLog($"[RiskRails] (Init) Account='{k0}' SavedMaxEquity={_perAccount[k0].SavedMaxEquity} Anchor={_perAccount[k0].DailyAnchorDate}={_perAccount[k0].DailyAnchorEquity}");
                _lastAccountId = p0.AccountID ?? "";
            }

            _startBar = -1;
            _warmupBars = 0;
        }

        protected override void OnDispose()
        {
            SaveState();
            base.OnDispose();
        }

        protected override void OnPortfolioChanged(Portfolio portfolio)
        {
            var newId = portfolio?.AccountID ?? "";
            if (string.Equals(newId, _lastAccountId, StringComparison.Ordinal))
                return;

            _lastAccountId = newId;

            // refresco limpio: mismas 5 series, sin acumular
            RecreateSeriesAndAttach();

            // gating para evitar 0s y pedir el back-fill al primer tick válido
            _startBar = -1;
            _warmupBars = WarmupOnSwitch;
            _needBackfill = true;
        }

        protected override void OnCalculate(int bar, decimal value)
        {

            var (p, key) = ResolvePortfolio();
            if (p is null || string.IsNullOrEmpty(key))
                return;

            var equityNow = p.Balance + p.OpenPnL;
            EnsureStateExists(key, equityNow);

            // portfolio ready?
            var looksUninitialized = (p.Balance == 0m && p.OpenPnL == 0m && equityNow == 0m);
            if (_warmupBars > 0) { _warmupBars--; return; }
            if (looksUninitialized) return;

            var st = _perAccount[key];

            // --- One-shot manual floor application ---
            if (ApplyManualFloorNow)
            {
                var absLoss = Math.Abs(MaxLoss);
                var targetFloor = ManualFloor;

                // Floor = max(StartingBalance - MaxLoss, SavedMaxEquity - MaxLoss)
                // Si definimos Floor manual, el SavedMaxEquity implícito es:
                var impliedMaxEquity = targetFloor + absLoss;

                // Dos casos:
                // 1) Subir el Floor → subir SavedMaxEquity (monotónico)
                if (impliedMaxEquity >= st.SavedMaxEquity)
                {
                    // raise max (monotonic)
                    IncreaseSavedMaxEquity(key, impliedMaxEquity);
                    // refresh local 'st' after helper updates dictionary
                    st = _perAccount[key];
                    DebugLog($"[RiskRails] Manual floor raised: Floor={targetFloor} => SavedMaxEquity={impliedMaxEquity}");
                }
                // 2) Bajar el Floor (reset mensual / cuenta reseteada)
                else
                {
                    st.SavedMaxEquity = impliedMaxEquity; // explicit decrease allowed
                    _perAccount[key] = st;
                    SaveState();
                    DebugLog($"[RiskRails] Manual floor decreased: Floor={targetFloor} => SavedMaxEquity={impliedMaxEquity}");
                }

                ApplyManualFloorNow = false; // consume el botón
            }

            // daily anchor (fixed rails per day)
            var today = GetCandle(bar).Time.Date;
            var todayStr = today.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(st.DailyAnchorDate)
                || !string.Equals(st.DailyAnchorDate, todayStr, StringComparison.Ordinal)
                || st.DailyAnchorEquity <= 0m)
            {
                st.DailyAnchorDate = todayStr;
                st.DailyAnchorEquity = equityNow;
                _perAccount[key] = st;
                SaveState();
                DebugLog($"[RiskRails] Daily anchor set: {key} {todayStr} = {st.DailyAnchorEquity}");
            }

            // ========= BACK-FILL PLANO TRAS CAMBIO DE CUENTA =========
            if (_needBackfill)
            {
                // valor constante para todo el histórico visible
                var target = equityNow;
                var tgt = st.DailyAnchorEquity + Math.Abs(DailyTargetOffset);
                var loss = st.DailyAnchorEquity - Math.Abs(DailyLossOffset);

                // rails que no abren escala
                var accountGoalVal = StartingBalance + Math.Abs(AccountGoalOffset);
                var initialFloor = StartingBalance - Math.Abs(MaxLoss);
                var dynamicFloor = Math.Max(initialFloor, st.SavedMaxEquity - Math.Abs(MaxLoss));

                // Rellena desde 0 hasta la barra actual con los mismos valores
                for (int i = 0; i <= bar; i++)
                {
                    if (ShowEquity) _equity[i] = target;
                    if (ShowDailyTarget) _dailyTarget[i] = tgt;
                    if (ShowDailyLoss) _dailyLoss[i] = loss;

                    if (ShowAccountGoal) _accountGoal[i] = accountGoalVal;
                    if (ShowAccountFloor) _accountFloor[i] = dynamicFloor;
                }

                _needBackfill = false;
                // A partir de ahora seguimos escribiendo normalmente desde esta barra
                _startBar = bar;
            }
            // =========================================================

            // Si no estamos back-filleando, escribe el punto normal de esta barra

            if (_startBar < 0) _startBar = bar;
            if (bar < _startBar) return;

            // ===== scale-on =====
            if (ShowEquity)
                _equity[bar] = equityNow;

            if (ShowDailyTarget)
                _dailyTarget[bar] = st.DailyAnchorEquity + Math.Abs(DailyTargetOffset);

            if (ShowDailyLoss)
                _dailyLoss[bar] = st.DailyAnchorEquity - Math.Abs(DailyLossOffset);

            // ===== scale-off =====
            var goalVal = StartingBalance + Math.Abs(AccountGoalOffset);
            var initFl = StartingBalance - Math.Abs(MaxLoss);
            var dynFl = Math.Max(initFl, st.SavedMaxEquity - Math.Abs(MaxLoss));

            if (ShowAccountGoal)
                _accountGoal[bar] = goalVal;

            if (ShowAccountFloor)
                _accountFloor[bar] = dynFl;

            // SavedMaxEquity: monotonic
            IncreaseSavedMaxEquity(key, equityNow);
        }

        // ---------- helpers ----------
        private (Portfolio? portfolio, string key) ResolvePortfolio()
        {
            var p = TradingManager?.Portfolio;
            if (p is null) return (null, "");

            if (!string.IsNullOrWhiteSpace(AccountId) &&
                !string.Equals(p.AccountID, AccountId, StringComparison.OrdinalIgnoreCase))
                return (null, "");

            var key = GetAccountKey(p);
            return (p, key);
        }

        private void EnsureStateExists(string key, decimal equityNow)
        {
            if (_perAccount.TryGetValue(key, out var st))
            {
                bool changed = false;
                if (st.SavedMaxEquity <= 0m && equityNow > 0m)
                {
                    st.SavedMaxEquity = equityNow;
                    changed = true;
                }
                if (string.IsNullOrWhiteSpace(st.DailyAnchorDate))
                {
                    st.DailyAnchorDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                    changed = true;
                }
                if (changed)
                {
                    _perAccount[key] = st;
                    SaveState();
                }
                return;
            }

            var init = Math.Max(equityNow, Math.Max(StartingBalance, 1m));
            _perAccount[key] = new State
            {
                SavedMaxEquity = init,
                DailyAnchorDate = "",
                DailyAnchorEquity = 0m
            };
            SaveState();
        }

        private void LoadState()
        {
            try
            {
                if (!File.Exists(StatePath)) return;
                var json = File.ReadAllText(StatePath, Encoding.UTF8);
                var dict = JsonSerializer.Deserialize<Dictionary<string, State>>(json);
                if (dict is null) return;

                _perAccount.Clear();
                foreach (var kv in dict)
                    _perAccount[kv.Key] = kv.Value;
            }
            catch { /* ignore read errors */ }
        }

        private void SaveState()
        {
            try
            {
                var dir = Path.GetDirectoryName(StatePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(
                    _perAccount,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(StatePath, json, Encoding.UTF8);
            }
            catch { /* ignore write errors */ }
        }

        private void DebugLog(string msg)
        {
            try { AddAlert("", msg); } catch { /* ignore in production */ }
        }

        // Max-only writer
        private void IncreaseSavedMaxEquity(string key, decimal candidate)
        {
            if (!_perAccount.TryGetValue(key, out var st))
                return;

            var newMax = candidate > st.SavedMaxEquity ? candidate : st.SavedMaxEquity;
            if (newMax != st.SavedMaxEquity)
            {
                st.SavedMaxEquity = newMax;
                _perAccount[key] = st;
                SaveState();
                DebugLog($"[RiskRails] New SavedMaxEquity for '{key}': {st.SavedMaxEquity}");
            }
        }

        private void RecreateSeriesAndAttach()
        {
            var eq = new ValueDataSeries("Equity")
            {
                VisualType = _equity.VisualType,
                Color = _equity.Color,
                Width = _equity.Width,
                Digits = _equity.Digits,
                ScaleIt = true
            };
            var dt = new ValueDataSeries("DailyTarget")
            {
                VisualType = _dailyTarget.VisualType,
                Color = _dailyTarget.Color,
                Width = _dailyTarget.Width,
                Digits = _dailyTarget.Digits,
                ScaleIt = true
            };
            var dl = new ValueDataSeries("DailyLoss")
            {
                VisualType = _dailyLoss.VisualType,
                Color = _dailyLoss.Color,
                Width = _dailyLoss.Width,
                Digits = _dailyLoss.Digits,
                ScaleIt = true
            };
            var ag = new ValueDataSeries("AccountGoal")
            {
                VisualType = _accountGoal.VisualType,
                Color = _accountGoal.Color,
                Width = _accountGoal.Width,
                Digits = _accountGoal.Digits,
                ScaleIt = false
            };
            var af = new ValueDataSeries("AccountFloor")
            {
                VisualType = _accountFloor.VisualType,
                Color = _accountFloor.Color,
                Width = _accountFloor.Width,
                Digits = _accountFloor.Digits,
                ScaleIt = false
            };

            // Reemplaza el contenido del indicador por EXACTAMENTE estas 5 series
            DataSeries.Clear();
            DataSeries.Add(eq);
            DataSeries.Add(dt);
            DataSeries.Add(dl);
            DataSeries.Add(ag);
            DataSeries.Add(af);

            // Actualiza las referencias
            _equity = eq;
            _dailyTarget = dt;
            _dailyLoss = dl;
            _accountGoal = ag;
            _accountFloor = af;
        }
    }
}
