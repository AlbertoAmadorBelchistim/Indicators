using ATAS.Indicators;
using ATAS.Indicators.Technical.Properties;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using Utils.Common.Logging;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("GammaLevels")]
    public sealed class GammaLevels : Indicator
    {
        #region Nested types: model

        internal enum LevelCategory
        {
            VolTrigger = 0,   // VT
            LargeGamma = 1,   // LG
            PutWall = 2,      // PW
            CallWall = 3,     // CW
            Combo = 4,        // CO
            ZeroGamma = 5,    // ZG / Zero Gamma
            Other = 6
        }

        internal enum LineTier
        {
            Thick = 0,
            Medium = 1,
            Thin = 2
        }

        public enum LabelSide
        {
            [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Left))]
            Left = 0,

            [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Right))]
            Right = 1
        }

        internal readonly struct LevelLabel
        {
            public readonly LevelCategory Category;
            public readonly int Rank;
            public readonly bool Is0Dte;
            public readonly string RawLabel;
            public readonly string DisplayLabel;

            public LevelLabel(LevelCategory category, int rank, bool is0Dte, string rawLabel, string displayLabel)
            {
                Category = category;
                Rank = rank;
                Is0Dte = is0Dte;
                RawLabel = rawLabel ?? string.Empty;
                DisplayLabel = displayLabel ?? string.Empty;
            }
        }

        internal readonly struct ParsedEntry
        {
            public readonly decimal Price;
            public readonly LevelLabel[] Labels;
            public readonly string SourceId;

            public ParsedEntry(decimal price, LevelLabel[] labels, string sourceId)
            {
                Price = price;
                Labels = labels ?? Array.Empty<LevelLabel>();
                SourceId = sourceId ?? string.Empty;
            }
        }

        internal readonly struct Level
        {
            public readonly decimal Price;
            public readonly LevelLabel[] Labels;
            public readonly LevelLabel Winner;
            public readonly string DisplayText;

            public Level(decimal price, LevelLabel[] labels, LevelLabel winner, string displayText)
            {
                Price = price;
                Labels = labels ?? Array.Empty<LevelLabel>();
                Winner = winner;
                DisplayText = displayText ?? string.Empty;
            }
        }

        internal readonly struct LevelRenderItem
        {
            public readonly decimal Price;
            public readonly string Text;

            public readonly LevelCategory Category;
            public readonly bool Is0Dte;

            public readonly LineTier Tier;
            public readonly int LineWidth;
            public readonly int Transparency; // 0..100 (UI semantics)
            public readonly bool Dash;
            public readonly bool DrawHalo;

            public LevelRenderItem(
                decimal price,
                string text,
                LevelCategory category,
                bool is0Dte,
                LineTier tier,
                int lineWidth,
                int transparency,
                bool dash,
                bool drawHalo)
            {
                Price = price;
                Text = text ?? string.Empty;
                Category = category;
                Is0Dte = is0Dte;
                Tier = tier;
                LineWidth = lineWidth;
                Transparency = transparency;
                Dash = dash;
                DrawHalo = drawHalo;
            }
        }

        #endregion

        #region Nested types: sources

        internal interface ILevelsSource
        {
            // A stable id for logging/tracing (e.g., "LoloText", "Api", "File", ...).
            string SourceId { get; }

            // Whether the source contributes entries.
            bool IsEnabled { get; }

            // Fetch entries produced by the source. Parsing warnings should be returned so the indicator can log them
            // with throttling (to avoid spamming the platform log).
            bool TryGetEntries(out ParsedEntry[] entries, out string[] warnings);
        }

        #endregion

        #region Nested types: parsing

        internal readonly struct ParseResult
        {
            public readonly ParsedEntry[] Entries;
            public readonly string[] Warnings;

            public ParseResult(ParsedEntry[] entries, string[] warnings)
            {
                Entries = entries ?? Array.Empty<ParsedEntry>();
                Warnings = warnings ?? Array.Empty<string>();
            }
        }

        #endregion

        #region Fields

        // -----------------------------
        // Rendering (shell only)
        // -----------------------------
        private RenderFont _font;

        // NOTE: Keep halo behavior aligned with legacy LevelsLolo (red halo style).
        // The actual drawing will be implemented in later commits.

        // -----------------------------
        // Dirty flags
        // -----------------------------
        private bool _dataDirty = true;
        private bool _visualDirty = true;

        // -----------------------------
        // Logging throttle
        // -----------------------------
        private const int _maxWarningsPerUpdate = 10;
        private const long _warningThrottleMs = 800;

        private int _lastInputHash;
        private long _lastWarningLogMs;

        // -----------------------------
        // Runtime: parsed entries (sources output)
        // -----------------------------
        private ParsedEntry[] _parsedEntries = Array.Empty<ParsedEntry>();

        // -----------------------------
        // Runtime: engine output
        // -----------------------------
        private Level[] _levels = Array.Empty<Level>();

        // -----------------------------
        // UI: Source - LoloText
        // -----------------------------
        private bool _enableLoloText = true;
        private string _loloTextRaw = string.Empty;

        // -----------------------------
        // UI: Visibility
        // -----------------------------
        private bool _onlyVisiblePriceRange = true;
        private bool _lastBarOnly = false;

        // -----------------------------
        // UI: Labels
        // -----------------------------
        private LabelSide _labelAlignment = LabelSide.Right;
        private int _offsetX = 6;
        private int _offsetY = 0;

        // -----------------------------
        // UI: Lines / Tiers (shell only - used later)
        // -----------------------------
        private int _thickMaxRank = 3;
        private int _mediumMaxRank = 10;

        private int _thickLineWidth = 3;
        private int _mediumLineWidth = 2;
        private int _thinLineWidth = 1;

        private int _thickLineTransparency = 0;
        private int _mediumLineTransparency = 40;
        private int _thinLineTransparency = 70;

        // -----------------------------
        // UI: 0DTE Halo (shell only)
        // -----------------------------
        private bool _show0DteHalo = true;
        private int _haloWidth = 6;
        private int _haloTransparency = 60;
        private bool _dash0Dte = true;

        #endregion

        #region Properties: Sources (LoloText)
        // -----------------------------
        // UI: Source - LoloText
        // -----------------------------

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Enabled),
            GroupName = nameof(Resources.LoloText),
            Description = nameof(Resources.LoloTextDesc),
            Order = 10)]
        public bool EnableLoloText
        {
            get => _enableLoloText;
            set
            {
                if (_enableLoloText == value)
                    return;

                _enableLoloText = value;
                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Text),
            GroupName = nameof(Resources.LoloText),
            Description = nameof(Resources.LoloTextDesc),
            Order = 20)]
        public string LoloTextRaw
        {
            get => _loloTextRaw;
            set
            {
                value ??= string.Empty;

                if (string.Equals(_loloTextRaw, value, StringComparison.Ordinal))
                    return;

                _loloTextRaw = value;
                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Clear),
            GroupName = nameof(Resources.LoloText),
            Description = nameof(Resources.ClearDesc),
            Order = 30)]
        public bool ClearLoloTextNow
        {
            get => false;
            set
            {
                if (!value)
                    return;

                if (_loloTextRaw.Length == 0)
                    return;

                _loloTextRaw = string.Empty;
                _dataDirty = true;
                RecalculateValues();
            }
        }
        #endregion
        // -----------------------------
        // UI: Visibility
        // -----------------------------
        #region Properties: Visibility
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.OnlyVisiblePriceRange),
            GroupName = nameof(Resources.Visibility),
            Description = nameof(Resources.OnlyVisiblePriceRangeDesc),
            Order = 100)]
        public bool OnlyVisiblePriceRange
        {
            get => _onlyVisiblePriceRange;
            set
            {
                if (_onlyVisiblePriceRange == value)
                    return;

                _onlyVisiblePriceRange = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.LastBarOnly),
            GroupName = nameof(Resources.Visibility),
            Description = nameof(Resources.LastBarOnlyDesc),
            Order = 110)]
        public bool LastBarOnly
        {
            get => _lastBarOnly;
            set
            {
                if (_lastBarOnly == value)
                    return;

                _lastBarOnly = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        #region Properties: Labels
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.LabelAlignment),
            GroupName = nameof(Resources.Labels),
            Description = nameof(Resources.LabelAlignmentDesc),
            Order = 200)]
        public LabelSide LabelAlignment
        {
            get => _labelAlignment;
            set
            {
                if (_labelAlignment == value)
                    return;

                _labelAlignment = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.OffsetX),
            GroupName = nameof(Resources.Labels),
            Order = 210)]
        public int OffsetX
        {
            get => _offsetX;
            set
            {
                if (_offsetX == value)
                    return;

                _offsetX = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.OffsetY),
            GroupName = nameof(Resources.Labels),
            Order = 220)]
        public int OffsetY
        {
            get => _offsetY;
            set
            {
                if (_offsetY == value)
                    return;

                _offsetY = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        // -----------------------------
        // UI: Lines / Tiers (shell only - used later)
        // -----------------------------
        #region Properties: Tiers
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThickMaxRank),
            GroupName = nameof(Resources.Tiers),
            Description = nameof(Resources.ThickMaxRankDesc),
            Order = 300)]
        [Range(0, 999)]
        public int ThickMaxRank
        {
            get => _thickMaxRank;
            set
            {
                if (_thickMaxRank == value)
                    return;

                _thickMaxRank = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MediumMaxRank),
            GroupName = nameof(Resources.Tiers),
            Description = nameof(Resources.MediumMaxRankDesc),
            Order = 310)]
        [Range(0, 999)]
        public int MediumMaxRank
        {
            get => _mediumMaxRank;
            set
            {
                if (_mediumMaxRank == value)
                    return;

                _mediumMaxRank = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThickLineWidth),
            GroupName = nameof(Resources.Tiers),
            Order = 320)]
        [Range(1, 10)]
        public int ThickLineWidth
        {
            get => _thickLineWidth;
            set
            {
                if (_thickLineWidth == value)
                    return;

                _thickLineWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MediumLineWidth),
            GroupName = nameof(Resources.Tiers),
            Order = 330)]
        [Range(1, 10)]
        public int MediumLineWidth
        {
            get => _mediumLineWidth;
            set
            {
                if (_mediumLineWidth == value)
                    return;

                _mediumLineWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThinLineWidth),
            GroupName = nameof(Resources.Tiers),
            Order = 340)]
        [Range(1, 10)]
        public int ThinLineWidth
        {
            get => _thinLineWidth;
            set
            {
                if (_thinLineWidth == value)
                    return;

                _thinLineWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThickLineTransparency),
            GroupName = nameof(Resources.Tiers),
            Order = 350)]
        [Range(0, 100)]
        public int ThickLineTransparency
        {
            get => _thickLineTransparency;
            set
            {
                if (_thickLineTransparency == value)
                    return;

                _thickLineTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MediumLineTransparency),
            GroupName = nameof(Resources.Tiers),
            Order = 360)]
        [Range(0, 100)]
        public int MediumLineTransparency
        {
            get => _mediumLineTransparency;
            set
            {
                if (_mediumLineTransparency == value)
                    return;

                _mediumLineTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThinLineTransparency),
            GroupName = nameof(Resources.Tiers),
            Order = 370)]
        [Range(0, 100)]
        public int ThinLineTransparency
        {
            get => _thinLineTransparency;
            set
            {
                if (_thinLineTransparency == value)
                    return;

                _thinLineTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        // -----------------------------
        // UI: 0DTE Halo (shell only)
        // -----------------------------
        #region Properties: 0DTE halo
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Show0DteHalo),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.Show0DteHaloDesc),
            Order = 400)]
        public bool Show0DteHalo
        {
            get => _show0DteHalo;
            set
            {
                if (_show0DteHalo == value)
                    return;

                _show0DteHalo = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.HaloWidth),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.HaloWidthDesc),
            Order = 410)]
        [Range(1, 20)]
        public int HaloWidth
        {
            get => _haloWidth;
            set
            {
                if (_haloWidth == value)
                    return;

                _haloWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.HaloTransparency),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.HaloTransparencyDesc),
            Order = 420)]
        [Range(0, 100)]
        public int HaloTransparency
        {
            get => _haloTransparency;
            set
            {
                if (_haloTransparency == value)
                    return;

                _haloTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Dash0Dte),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.Dash0DteDesc),
            Order = 430)]
        public bool Dash0Dte
        {
            get => _dash0Dte;
            set
            {
                if (_dash0Dte == value)
                    return;

                _dash0Dte = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        // -----------------------------
        // Lifecycle
        // -----------------------------
        #region Ctor
        public GammaLevels()
        {
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            // Keep a simple default font; will be finalized when text rendering is implemented.
            _font = new RenderFont("Arial", 12);
        }
        #endregion

        #region Overrides
        protected override void OnCalculate(int bar, decimal value)
        {
            RebuildParsedEntriesIfNeeded();

            // Shell: no calculations yet.
            // Future commits will parse sources and rebuild render packets when _dataDirty/_visualDirty.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Shell: do nothing yet, but keep safe early-exits aligned with legacy indicator patterns.
            if (ChartInfo is not { PriceChartContainer.BarsWidth: > 2 })
                return;

            if (LastVisibleBarNumber > CurrentBar - 1)
                return;

            // Future commits will draw levels here.
        }
        #endregion

        #region Private methods

        private const string _loloSourceId = "LoloText";

        private static readonly Regex _numberOnly = new Regex(@"^\s*\d+(\.\d+)?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // e.g.: "CO44", "LG1 0DTE", "VT 0DTE", "Zero Gamma", "CW", "PW"
        private static readonly Regex _labelPattern = new Regex(
            @"^\s*(?<name>[A-Za-z ]+?)(?<rank>\d{1,3})?(?<suffix>\s+0DTE)?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static bool TryParseDecimal(string s, out decimal value)
        {
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;

            return false;
        }

        private static string NormalizeInvisible(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            // Remove Unicode "Format" chars (e.g. zero-width joiners), then trim.
            var arr = s.ToCharArray();
            var res = new char[arr.Length];
            var count = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(arr[i]);
                if (cat != UnicodeCategory.Format)
                    res[count++] = arr[i];
            }

            return new string(res, 0, count).Trim();
        }

        private static LevelCategory MapCategory(string name)
        {
            var u = (name ?? string.Empty).ToUpperInvariant().Replace("  ", " ").Trim();

            if (u == "CO" || u == "COMBO") return LevelCategory.Combo;
            if (u == "LG" || u == "LARGE GAMMA" || u == "LARGEGAMMA") return LevelCategory.LargeGamma;
            if (u == "VT" || u == "VOLATILITY TRIGGER" || u == "VOLATILITYTRIGGER") return LevelCategory.VolTrigger;
            if (u == "CW" || u == "CALL WALL" || u == "CALLWALL") return LevelCategory.CallWall;
            if (u == "PW" || u == "PUT WALL" || u == "PUTWALL") return LevelCategory.PutWall;
            if (u == "ZG" || u == "ZERO GAMMA" || u == "ZEROGAMMA") return LevelCategory.ZeroGamma;

            return LevelCategory.Other;
        }

        private static string FormatDisplayLabel(LevelCategory category, int rank, bool is0Dte, string rawName)
        {
            // Keep labels compact, aligned with legacy LevelsLolo behavior.
            string baseName = category switch
            {
                LevelCategory.Combo => "CO",
                LevelCategory.LargeGamma => "LG",
                LevelCategory.VolTrigger => "VT",
                LevelCategory.CallWall => "CW",
                LevelCategory.PutWall => "PW",
                LevelCategory.ZeroGamma => "ZG",
                _ => (rawName ?? string.Empty).ToUpperInvariant().Trim()
            };

            string rankTxt = rank > 0 ? rank.ToString(CultureInfo.InvariantCulture) : string.Empty;
            string suffix = is0Dte ? " 0DTE" : string.Empty;

            return (baseName + rankTxt + suffix).Trim();
        }

        private void RedrawChart()
        {
            // RecalculateValues triggers a render refresh in ATAS indicators.
            // Use it for visual-only changes too to keep behavior consistent.
            RecalculateValues();
        }
        private static int GetCategoryPriority(LevelCategory category)
        {
            // Lower value = higher priority.
            return category switch
            {
                LevelCategory.VolTrigger => 0, // VT
                LevelCategory.LargeGamma => 1, // LG
                LevelCategory.PutWall => 2,    // PW
                LevelCategory.CallWall => 2,   // CW (same tier as PW)
                LevelCategory.Combo => 3,      // CO
                LevelCategory.ZeroGamma => 4,  // ZG
                _ => 5
            };
        }

        private static Level[] BuildLevels(ParsedEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
                return Array.Empty<Level>();

            // Key: price, Value: merged labels
            var map = new System.Collections.Generic.Dictionary<decimal, System.Collections.Generic.List<LevelLabel>>(entries.Length);

            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e.Labels == null || e.Labels.Length == 0)
                    continue;

                if (!map.TryGetValue(e.Price, out var list))
                {
                    list = new System.Collections.Generic.List<LevelLabel>(e.Labels.Length);
                    map.Add(e.Price, list);
                }

                for (int j = 0; j < e.Labels.Length; j++)
                    list.Add(e.Labels[j]);
            }

            if (map.Count == 0)
                return Array.Empty<Level>();

            var levels = new Level[map.Count];
            int k = 0;

            foreach (var kvp in map)
            {
                var price = kvp.Key;
                var labels = kvp.Value;

                var labelsArray = labels.Count == 0 ? Array.Empty<LevelLabel>() : labels.ToArray();
                var winner = SelectWinner(labelsArray);
                var display = BuildDisplayText(labelsArray);

                levels[k++] = new Level(price, labelsArray, winner, display);
            }

            Array.Sort(levels, (a, b) => a.Price.CompareTo(b.Price));
            return levels;
        }

        private static LevelLabel SelectWinner(LevelLabel[] labels)
        {
            // Assumption: labels.Length > 0
            var best = labels[0];

            for (int i = 1; i < labels.Length; i++)
            {
                var cur = labels[i];

                // 1) lower rank wins
                if (cur.Rank < best.Rank)
                {
                    best = cur;
                    continue;
                }

                if (cur.Rank > best.Rank)
                    continue;

                // 2) tie-breaker: category priority (VT > LG > PW/CW > CO > ZG > Other)
                var pCur = GetCategoryPriority(cur.Category);
                var pBest = GetCategoryPriority(best.Category);

                if (pCur < pBest)
                {
                    best = cur;
                    continue;
                }

                if (pCur > pBest)
                    continue;

                // 3) deterministic tie-breaker: prefer 0DTE
                if (cur.Is0Dte && !best.Is0Dte)
                {
                    best = cur;
                    continue;
                }

                // 4) final tie-breaker: lexical DisplayLabel to make output stable
                if (string.Compare(cur.DisplayLabel, best.DisplayLabel, StringComparison.Ordinal) < 0)
                    best = cur;
            }

            return best;
        }

        private static string BuildDisplayText(LevelLabel[] labels)
        {
            if (labels == null || labels.Length == 0)
                return string.Empty;

            // Keep a stable order: by rank, then category priority, then 0DTE first, then label.
            Array.Sort(labels, (a, b) =>
            {
                var c = a.Rank.CompareTo(b.Rank);
                if (c != 0) return c;

                c = GetCategoryPriority(a.Category).CompareTo(GetCategoryPriority(b.Category));
                if (c != 0) return c;

                // 0DTE first
                c = b.Is0Dte.CompareTo(a.Is0Dte);
                if (c != 0) return c;

                return string.Compare(a.DisplayLabel, b.DisplayLabel, StringComparison.Ordinal);
            });

            // Join unique display labels (avoid duplicates)
            var sb = new StringBuilder(64);

            string last = null;

            for (int i = 0; i < labels.Length; i++)
            {
                var t = labels[i].DisplayLabel;
                if (string.IsNullOrWhiteSpace(t))
                    continue;

                if (string.Equals(t, last, StringComparison.Ordinal))
                    continue;

                if (sb.Length > 0)
                    sb.Append(" & ");

                sb.Append(t);
                last = t;
            }

            return sb.ToString();
        }

        private void RebuildParsedEntriesIfNeeded()
        {
            if (!_dataDirty)
                return;

            _dataDirty = false;

            // If source disabled or empty, clear.
            if (!EnableLoloText || string.IsNullOrWhiteSpace(LoloTextRaw))
            {
                _parsedEntries = Array.Empty<ParsedEntry>();
                _levels = Array.Empty<Level>();
                _visualDirty = true;
                return;
            }

            var result = ParseLoloText(LoloTextRaw);

            _parsedEntries = result.Entries;

            LogParseWarningsIfNeeded(result.Warnings, LoloTextRaw);

            _levels = BuildLevels(_parsedEntries);
            _visualDirty = true;
        }

        private void LogParseWarningsIfNeeded(string[] warnings, string input)
        {
            if (warnings == null || warnings.Length == 0)
                return;

            var nowMs = Environment.TickCount64;
            var hash = input.GetHashCode();

            // Avoid repeated spam: only log for new input, or if enough time has passed.
            if (hash == _lastInputHash && (nowMs - _lastWarningLogMs) < _warningThrottleMs)
                return;

            _lastInputHash = hash;
            _lastWarningLogMs = nowMs;

            var count = Math.Min(warnings.Length, _maxWarningsPerUpdate);

            for (int i = 0; i < count; i++)
                this.LogWarn($"[GammaLevels] {warnings[i]}");

            if (warnings.Length > count)
                this.LogWarn($"[GammaLevels] ... {warnings.Length - count} more warning(s).");
        }

        private static ParseResult ParseLoloText(string raw)
        {
            var warnings = new List<string>(8);
            var entries = new List<ParsedEntry>(64);

            var text = NormalizeInvisible(raw);
            if (string.IsNullOrWhiteSpace(text))
                return new ParseResult(
                    Array.Empty<ParsedEntry>(),
                    Array.Empty<string>());

            // Optional prefix like "$SP:" (or any "$TICKER:")
            int colon = text.IndexOf(':');
            if (colon >= 0 && colon + 1 < text.Length)
                text = text.Substring(colon + 1);

            var tokens = text.Split(',');
            var cleaned = new List<string>(tokens.Length);

            for (int i = 0; i < tokens.Length; i++)
            {
                var t = NormalizeInvisible(tokens[i]);
                if (!string.IsNullOrEmpty(t))
                    cleaned.Add(t);
            }

            if (cleaned.Count == 0)
                return new ParseResult(
                    Array.Empty<ParsedEntry>(),
                    Array.Empty<string>());

            var pending = new List<LevelLabel>(8);

            for (int i = 0; i < cleaned.Count; i++)
            {
                var token = cleaned[i];

                // Split by '&' (multiple labels before one price)
                var parts = token.Split('&');

                bool isSingleNumber = (parts.Length == 1) && _numberOnly.IsMatch(parts[0]);

                if (isSingleNumber)
                {
                    // Price token
                    if (pending.Count == 0)
                        continue;

                    if (!TryParseDecimal(parts[0], out var price))
                    {
                        warnings.Add($"Invalid price token '{parts[0]}' (ignored).");
                        continue;
                    }

                    entries.Add(new ParsedEntry(price, pending.ToArray(), _loloSourceId));
                    pending.Clear();
                    continue;
                }

                // Label token(s)
                for (int p = 0; p < parts.Length; p++)
                {
                    var part = NormalizeInvisible(parts[p]);
                    if (string.IsNullOrEmpty(part))
                        continue;

                    var m = _labelPattern.Match(part);
                    if (!m.Success)
                    {
                        warnings.Add($"Unrecognized label '{part}' (ignored).");
                        continue;
                    }

                    var rawName = NormalizeInvisible(m.Groups["name"].Value);

                    // Normalize "Zero Gamma" to "ZG" to reduce label clutter
                    if (string.Equals(rawName, "Zero Gamma", StringComparison.OrdinalIgnoreCase))
                        rawName = "ZG";

                    int rank = 0;
                    if (m.Groups["rank"].Success &&
                        int.TryParse(m.Groups["rank"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
                    {
                        rank = r;
                    }

                    bool is0dte = m.Groups["suffix"].Success;

                    var cat = MapCategory(rawName);

                    var display = FormatDisplayLabel(cat, rank, is0dte, rawName);

                    pending.Add(new LevelLabel(
                        category: cat,
                        rank: rank,
                        is0Dte: is0dte,
                        rawLabel: part,
                        displayLabel: display));
                }
            }

            // NOTE: If input ends with labels and no price, we intentionally ignore pending labels (legacy behavior).
            // We do not warn to avoid noise for partial pastes.

            return new ParseResult(
                entries.Count == 0 ? Array.Empty<ParsedEntry>() : entries.ToArray(),
                warnings.Count == 0 ? Array.Empty<string>() : warnings.ToArray());
        }


        #endregion
    }
}
