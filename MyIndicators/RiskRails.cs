namespace MyIndicators
{
    using ATAS.DataFeedsCore;              // Portfolio
    using ATAS.Indicators;                 // Indicator, ValueDataSeries, IndicatorDataProvider
    using OFT.Rendering.Settings;          // VisualMode
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Windows.Media;            // Colors

    [Category("Custom")]
    [DisplayName("RiskRails")]
    public class RiskRails : Indicator
    {
        // === VISUAL TOGGLES ===
        [Display(Name = "Show Equity")] public bool ShowEquity { get; set; } = true;
        [Display(Name = "Show Account Goal")] public bool ShowAccountGoal { get; set; } = true;
        [Display(Name = "Show Daily Target")] public bool ShowDailyTarget { get; set; } = true;
        [Display(Name = "Show Daily Loss")] public bool ShowDailyLoss { get; set; } = true;
        [Display(Name = "Show Account Floor")] public bool ShowAccountFloor { get; set; } = true;

        // === INPUTS ===
        [Display(Name = "Account ID (empty = selected)")] public string AccountId { get; set; } = string.Empty;

        [Display(Name = "Starting Balance")] public decimal StartingBalance { get; set; } = 50000m;
        [Display(Name = "Initial MaxLoss")] public decimal MaxLoss { get; set; } = 2500m;

        [Display(Name = "Account Goal Offset (+)")] public decimal AccountGoalOffset { get; set; } = 3000m;
        [Display(Name = "Daily Target Offset (+)")] public decimal DailyTargetOffset { get; set; } = 300m;
        [Display(Name = "Daily Loss Offset (−)")] public decimal DailyLossOffset { get; set; } = 300m;

        // Seed externo (Bulenox)
        [Display(Name = "Seed AccountFloor ($)")]
        public decimal SeedAccountFloor { get; set; } = 0m;

        [Display(Name = "Apply floor seed NOW")]
        public bool ApplyFloorSeedNow { get; set; } = false;

        // Reset del máximo guardado (si reinician cuenta)
        [Display(Name = "Reset saved Max Equity NOW")]
        public bool ResetMaxEquityNow { get; set; } = false;

        // === SERIES ===
        private ValueDataSeries _equity;        // scale-on
        private ValueDataSeries _dailyTarget;   // scale-on
        private ValueDataSeries _dailyLoss;     // scale-on
        private ValueDataSeries _accountGoal;   // scale-off
        private ValueDataSeries _accountFloor;  // scale-off

        // === PERSISTENCE ===
        [Display(Name = "State path (JSON)")]
        public string StatePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ATAS", "Exports", "riskrails_state.json");

        private sealed class State
        {
            public decimal SavedMaxEquity; // máximo observado/sembrado (solo sube)
        }

        private readonly Dictionary<string, State> _perAccount = new(StringComparer.OrdinalIgnoreCase);
        private int _startBar = -1;
        private static string GetAccountKey(ATAS.DataFeedsCore.Portfolio p)
    => string.IsNullOrWhiteSpace(p.AccountID) ? "default" : p.AccountID.Trim();

        private string _lastAccountId = "";

        public RiskRails()
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
                Digits = 2,
                ScaleIt = true
            };

            _dailyLoss = new ValueDataSeries("DailyLoss")
            {
                VisualType = VisualMode.Line,
                Color = Colors.HotPink,
                Digits = 2,
                ScaleIt = true
            };

            _accountGoal = new ValueDataSeries("AccountGoal")
            {
                VisualType = VisualMode.Line,
                Color = Colors.Orange,
                Digits = 2,
                ScaleIt = false // no abre la escala
            };

            _accountFloor = new ValueDataSeries("AccountFloor")
            {
                VisualType = VisualMode.Line,
                Color = Colors.MediumPurple,
                Digits = 2,
                ScaleIt = false // no abre la escala
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
            if (p0 != null && _perAccount.TryGetValue(k0, out var st0))
                DebugLog($"[RiskRails] Loaded SavedMaxEquity for '{k0}': {st0.SavedMaxEquity}");

            // migración: si hay “default” y tenemos AccountID real, clónalo
            var p = TradingManager?.Portfolio;
            if (p != null)
            {
                var id = GetAccountKey(p);
                if (_perAccount.ContainsKey("default") && !_perAccount.ContainsKey(id))
                {
                    _perAccount[id] = _perAccount["default"];
                    // Persist migration to ensure the value survives restarts.
                    SaveState();
                }

                _lastAccountId = p.AccountID ?? ""; // ← guarda la cuenta actual
            }

            _startBar = -1;
        }

        protected override void OnDispose()
        {
            SaveState();
            base.OnDispose();
        }

        protected override void OnPortfolioChanged(Portfolio portfolio)
        {
            _lastAccountId = portfolio?.AccountID ?? "";
            SoftResetSeries();
        }

        private void SoftResetSeries()
        {
            RecreateSeriesAndAttach();   // series nuevas y vacías
            _startBar = CurrentBar;      // empezamos a pintar desde aquí
            RedrawChart();
        }
        protected override void OnCalculate(int bar, decimal value)
        {
            var (p, key) = ResolvePortfolio();
            if (p is null || string.IsNullOrEmpty(key))
                return;

            // detectar cambio de cuenta con p garantizado
            var curId = p.AccountID ?? "";

            if (!string.Equals(curId, _lastAccountId, StringComparison.Ordinal))
            {
                _lastAccountId = curId;
                SoftResetSeries();
                return; // esta barra no se pinta; la siguiente ya arranca limpio
            }

            var equity = p.Balance + p.OpenPnL;
            var st = _perAccount[key];

            // 1) Seed manual del floor (Bulenox) -> implica SavedMaxEquity
            if (ApplyFloorSeedNow && SeedAccountFloor > 0m)
            {
                var seedInitialFloor = StartingBalance - Math.Abs(MaxLoss);
                var impliedMaxEquity = (SeedAccountFloor > seedInitialFloor)
                    ? SeedAccountFloor + Math.Abs(MaxLoss)
                    : StartingBalance;

                if (impliedMaxEquity > st.SavedMaxEquity)
                    st.SavedMaxEquity = impliedMaxEquity; // solo sube

                _perAccount[key] = st;
                SaveState();                 // <<< guarda ya, no esperes al Dispose
                DebugLog($"[RiskRails] Seed applied. SavedMaxEquity={st.SavedMaxEquity} | Floor={Math.Max(StartingBalance - Math.Abs(MaxLoss), st.SavedMaxEquity - Math.Abs(MaxLoss))}");

                SoftResetSeries();    // corta pasado y recentra rango
                ApplyFloorSeedNow = false;
                return;
            }

            // 2) Reset del máximo guardado (reinicio de cuenta)
            if (ResetMaxEquityNow)
            {
                st.SavedMaxEquity = StartingBalance;
                SaveState();
                ResetMaxEquityNow = false;
                _perAccount[key] = st;
                SoftResetSeries();
                return;
            }

            // 3) Max en vivo (solo sube)
            if (equity > st.SavedMaxEquity)
            {
                st.SavedMaxEquity = equity;
                SaveState();
            }
            _perAccount[key] = st;

            // 4) Pintar solo desde la vela actual
            if (_startBar < 0) _startBar = bar;
            if (bar < _startBar) return;

            // === series que fijan la escala ===
            if (ShowEquity) _equity[bar] = equity;
            if (ShowDailyTarget) _dailyTarget[bar] = equity + Math.Abs(DailyTargetOffset);
            if (ShowDailyLoss) _dailyLoss[bar] = equity - Math.Abs(DailyLossOffset);

            // === rails que NO abren la escala ===
            var accountGoalVal = StartingBalance + Math.Abs(AccountGoalOffset);
            var initialFloor = StartingBalance - Math.Abs(MaxLoss);
            var dynamicFloor = Math.Max(initialFloor, st.SavedMaxEquity - Math.Abs(MaxLoss));

            if (ShowAccountGoal) _accountGoal[bar] = accountGoalVal;
            if (ShowAccountFloor) _accountFloor[bar] = dynamicFloor;
        }

        // ---- helpers ----
        private (Portfolio? portfolio, string key) ResolvePortfolio()
        {
            var p = TradingManager?.Portfolio;
            if (p is null) return (null, "");

            if (!string.IsNullOrWhiteSpace(AccountId) &&
                !string.Equals(p.AccountID, AccountId, StringComparison.OrdinalIgnoreCase))
                return (null, "");

            var key = GetAccountKey(p);
            if (!_perAccount.ContainsKey(key))
                _perAccount[key] = new State { SavedMaxEquity = StartingBalance };

            return (p, key);
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
                foreach (var kv in dict) _perAccount[kv.Key] = kv.Value;
            }
            catch { /* ignore */ }
        }

        private void SaveState()
        {
            try
            {
                var dir = Path.GetDirectoryName(StatePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(_perAccount, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(StatePath, json, Encoding.UTF8);
            }
            catch { /* ignore */ }
        }

        private void RecreateSeriesAndAttach()
        {
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
                Digits = 2,
                ScaleIt = true
            };
            _dailyLoss = new ValueDataSeries("DailyLoss")
            {
                VisualType = VisualMode.Line,
                Color = Colors.HotPink,
                Digits = 2,
                ScaleIt = true
            };
            _accountGoal = new ValueDataSeries("AccountGoal")
            {
                VisualType = VisualMode.Line,
                Color = Colors.Orange,
                Digits = 2,
                ScaleIt = false
            };
            _accountFloor = new ValueDataSeries("AccountFloor")
            {
                VisualType = VisualMode.Line,
                Color = Colors.MediumPurple,
                Digits = 2,
                ScaleIt = false
            };

            DataSeries.Clear();
            DataSeries.Add(_equity);
            DataSeries.Add(_dailyTarget);
            DataSeries.Add(_dailyLoss);
            DataSeries.Add(_accountGoal);
            DataSeries.Add(_accountFloor);
        }



        private void DebugLog(string msg)
        {
            try { AddAlert("", msg); } catch { /* ignore in production */ }
        }

    }
}









