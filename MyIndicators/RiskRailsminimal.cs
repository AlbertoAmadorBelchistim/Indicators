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
    [DisplayName("RiskRails Minimal")]
    public class RiskRailsMinimal : Indicator
    {
        // ====== STAGE 1: ONLY EQUITY IN A DEDICATED PANEL ======
        // Goal:
        //  - Draw Equity (Balance + OpenPnL) from the CURRENT BAR only.
        //  - Never write zeros on account switch / reconnect (skip until portfolio is ready).
        //  - Persist per-account SavedMaxEquity (we won’t USE it yet, just prove persistence).
        //  - No offsets, no floor/goal lines, no API calls not present in 7.0.11.144.

        // --- INPUTS (keep only what we need in Stage 1) ---
        [Display(Name = "Account ID (empty = selected)")]
        public string AccountId { get; set; } = string.Empty;

        [Display(Name = "Starting Balance")]
        public decimal StartingBalance { get; set; } = 50000m;

        // --- SERIES ---
        private ValueDataSeries _equity; // scale-on

        // --- PERSISTENCE (per account) ---
        [Display(Name = "State path (JSON)")]
        public string StatePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ATAS", "Exports", "riskrails_state.json");

        // Only field we need now; we’ll prove this survives restarts per AccountID.
        private sealed class State
        {
            public decimal SavedMaxEquity; // “max equity observed/seeded” (only increases)
        }

        private readonly Dictionary<string, State> _perAccount =
            new(StringComparer.OrdinalIgnoreCase);

        // --- RUNTIME GUARDS ---
        private int _startBar = -1;           // draw only from here forward
        private string _lastAccountId = "";   // detect account switch
        private int _warmupBars = 0;          // small cooldown after account change
        private const int WarmupOnSwitch = 2; // #bars to wait to avoid writing zeros

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
                ScaleIt = true // equity opens the scale
            };

            DataSeries[0] = _equity;
        }

        protected override void OnInitialize()
        {
            LoadState();

            // Log/load current account state (if available)
            var (p0, k0) = ResolvePortfolio();
            if (p0 != null)
            {
                EnsureStateExists(k0);
                DebugLog($"[RiskRails] (Init) Account='{k0}' SavedMaxEquity={_perAccount[k0].SavedMaxEquity}");
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
            // Account changed → do NOT recreate series, do NOT write zeros.
            // Just re-anchor drawing to current bar and wait a couple bars
            // until the new portfolio reports non-zero values.
            _lastAccountId = portfolio?.AccountID ?? "";
            _startBar = -1;               // re-anchor on next valid write
            _warmupBars = WarmupOnSwitch; // small cooldown to avoid 0 writes
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            var (p, key) = ResolvePortfolio();
            if (p is null || string.IsNullOrEmpty(key))
                return;

            EnsureStateExists(key);

            // Detect “portfolio not ready”: some builds report 0 for a few bars after switching.
            // If everything is exactly zero, SKIP writing this bar.
            var equityNow = p.Balance + p.OpenPnL;
            var looksUninitialized = (p.Balance == 0m && p.OpenPnL == 0m && equityNow == 0m);

            // Warmup: after an account switch, wait N bars to reduce the chance of a 0 write.
            if (_warmupBars > 0)
            {
                _warmupBars--;
                return; // do not write
            }

            // If portfolio still looks uninitialized, do not write yet.
            if (looksUninitialized)
                return;

            // First effective write anchor
            if (_startBar < 0)
                _startBar = bar;

            // Write only from the start bar forward
            if (bar < _startBar)
                return;

            // Stage 1: Only the equity line
            _equity[bar] = equityNow;

            // Stage 1 also keeps SavedMaxEquity in sync (prove persistence across restarts).
            var st = _perAccount[key];
            if (equityNow > st.SavedMaxEquity)
            {
                st.SavedMaxEquity = equityNow;
                _perAccount[key] = st;
                SaveState(); // persist immediately to survive ATAS restarts
            }
        }

        // ---------- helpers ----------
        private (Portfolio? portfolio, string key) ResolvePortfolio()
        {
            var p = TradingManager?.Portfolio;
            if (p is null) return (null, "");

            // Optional filter: if user typed a specific AccountId, enforce it
            if (!string.IsNullOrWhiteSpace(AccountId) &&
                !string.Equals(p.AccountID, AccountId, StringComparison.OrdinalIgnoreCase))
                return (null, "");

            var key = GetAccountKey(p);
            return (p, key);
        }

        private void EnsureStateExists(string key)
        {
            if (_perAccount.ContainsKey(key))
            {
                // If a legacy entry had zero (e.g., previous bad save), normalize to StartingBalance ONCE.
                if (_perAccount[key].SavedMaxEquity == 0m && StartingBalance > 0m)
                {
                    _perAccount[key].SavedMaxEquity = StartingBalance;
                    SaveState();
                }
                return;
            }

            // New account → start from StartingBalance (non-zero baseline)
            _perAccount[key] = new State { SavedMaxEquity = StartingBalance > 0m ? StartingBalance : 1m };
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

                // Normalize accidental zero states (from previous broken saves)
                bool changed = false;
                foreach (var k in new List<string>(_perAccount.Keys))
                {
                    if (_perAccount[k] == null)
                    {
                        _perAccount[k] = new State { SavedMaxEquity = StartingBalance > 0m ? StartingBalance : 1m };
                        changed = true;
                    }
                    else if (_perAccount[k].SavedMaxEquity == 0m && StartingBalance > 0m)
                    {
                        _perAccount[k].SavedMaxEquity = StartingBalance;
                        changed = true;
                    }
                }
                if (changed) SaveState();
            }
            catch
            {
                // ignore read errors → start fresh in memory
            }
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
            catch
            {
                // ignore write errors (no crash in production)
            }
        }

        private void DebugLog(string msg)
        {
            try { AddAlert("", msg); } catch { /* ignore in production */ }
        }
    }
}