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

using ChartExtensions = ATAS.Indicators.Extensions;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("GammaLevels")]
    public sealed class GammaLevels : Indicator
    {
        #region Nested types: model

        internal enum LevelCategory
        {
            Combo = 0,
            LargeGamma = 1,
            VolTrigger = 2,
            CallWall = 3,
            PutWall = 4,
            ZeroGamma = 5,

            // MenthorQ-specific categories
            BlindLevel = 6,
            DayMin = 7,
            DayMax = 8,
            RiskTrigger = 9,
            LowerBand = 10,
            UpperBand = 11,

            Other = 100
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

        private sealed class LoloTextSource : ILevelsSource
        {
            private readonly GammaLevels _owner;

            public LoloTextSource(GammaLevels owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public string SourceId => "LoloText";

            public bool IsEnabled => _owner.EnableLoloText;

            public bool TryGetEntries(out ParsedEntry[] entries, out string[] warnings)
            {
                var raw = _owner.LoloTextRaw;

                if (!IsEnabled || string.IsNullOrWhiteSpace(raw))
                {
                    entries = Array.Empty<ParsedEntry>();
                    warnings = Array.Empty<string>();
                    return true;
                }

                var result = ParseLoloText(raw);

                entries = result.Entries;
                warnings = result.Warnings;
                return true;
            }
        }

        private sealed class MenthorQTextSource : ILevelsSource
        {
            private readonly GammaLevels _owner;

            public MenthorQTextSource(GammaLevels owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public string SourceId => "MenthorQText";

            public bool IsEnabled => _owner.EnableMenthorQText;

            public bool TryGetEntries(out ParsedEntry[] entries, out string[] warnings)
            {
                var raw = _owner.MenthorQTextRaw;

                if (!IsEnabled || string.IsNullOrWhiteSpace(raw))
                {
                    entries = Array.Empty<ParsedEntry>();
                    warnings = Array.Empty<string>();
                    return true;
                }

                var result = ParseMenthorQText(raw, _owner.MenthorQOffset);

                entries = result.Entries;
                warnings = result.Warnings;
                return true;
            }
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
        // Sources
        // -----------------------------
        private readonly List<ILevelsSource> _sources = new List<ILevelsSource>(2);
        private bool _sourcesInitialized;

        // Collector output (union of sources)
        private ParsedEntry[] _collectedEntries = Array.Empty<ParsedEntry>();

        // -----------------------------
        // Rendering (shell only)
        // -----------------------------
        private RenderFont _font;

        // NOTE: Keep halo behavior aligned with legacy LevelsLolo (red halo style).
        // The actual drawing will be implemented in later commits.

        // -----------------------------
        // Text measurement cache (avoid MeasureString per-frame)
        // -----------------------------
        private readonly Dictionary<string, (int W, int H)> _textSizeCache = new Dictionary<string, (int W, int H)>(StringComparer.Ordinal);

        // -----------------------------
        // Base pens by category (legacy-like defaults)
        // -----------------------------
        private readonly PenSettings _penPutWall = new PenSettings { Color = CrossColor.FromArgb(255, 0, 255, 128), Width = 2 };   // PW #00FF80
        private readonly PenSettings _penCallWall = new PenSettings { Color = CrossColor.FromArgb(255, 255, 64, 64), Width = 2 };  // CW #FF4040
        private readonly PenSettings _penVolTrigger = new PenSettings { Color = CrossColor.FromArgb(255, 255, 215, 0), Width = 3 }; // VT #FFD700
        private readonly PenSettings _penLargeGamma = new PenSettings { Color = CrossColor.FromArgb(255, 255, 136, 0), Width = 2 }; // LG #FF8800
        private readonly PenSettings _penCombo = new PenSettings { Color = CrossColor.FromArgb(255, 255, 192, 77), Width = 1 };     // CO #FFC04D
        private readonly PenSettings _penZeroGamma = new PenSettings { Color = CrossColor.FromArgb(255, 170, 170, 170), Width = 1 }; // ZG #AAAAAA

        // Optional overlay: thin dotted accent ON TOP for high-priority 0DTE LG/PW/CW.
        private readonly PenSettings _pen0DteAccent = new PenSettings
        {
            Color = CrossColor.FromArgb(220, 255, 64, 64), // brighter red
            Width = 1,
            LineDashStyle = LineDashStyle.Dot
        };

        // MenthorQ palette (defaults)
        // Notes:
        // - DayMax/DayMin use resistance/support semantics (red/green) for fast scanning.
        // - Bands are informational -> cooler hue, thinner by default.
        // - RiskTrigger is distinct (purple) to avoid confusion with CW/PW/LG/VT.
        // - BlindLevel uses a neutral blue (visibility without implying support/resistance).
        private readonly PenSettings _penBlindLevel = new PenSettings { Color = CrossColor.FromArgb(255, 90, 165, 255), Width = 1 };   // BL #5AA5FF
        private readonly PenSettings _penDayMin = new PenSettings { Color = CrossColor.FromArgb(255, 80, 200, 120), Width = 1 };       // 1D Min #50C878
        private readonly PenSettings _penDayMax = new PenSettings { Color = CrossColor.FromArgb(255, 235, 90, 90), Width = 1 };         // 1D Max #EB5A5A
        private readonly PenSettings _penRiskTrigger = new PenSettings { Color = CrossColor.FromArgb(255, 175, 110, 255), Width = 1 };  // RT #AF6EFF
        private readonly PenSettings _penLowerBand = new PenSettings { Color = CrossColor.FromArgb(255, 70, 200, 210), Width = 1 };     // LB #46C8D2
        private readonly PenSettings _penUpperBand = new PenSettings { Color = CrossColor.FromArgb(255, 70, 200, 210), Width = 1 };     // UB #46C8D2

        private readonly PenSettings _penOther = new PenSettings { Color = CrossColor.FromArgb(255, 160, 160, 160), Width = 1 };     // Other

        // -----------------------------
        // Render cache (no allocations in OnRender)
        // -----------------------------
        // [category, tier, dash(0=solid,1=dash)]
        private PenSettings[,,] _linePens;

        // [tier] halo pen (solid)
        private PenSettings[] _haloPens;

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

        private string _lastWarningsKey;
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
        // UI: MenthorQ text source
        // -----------------------------
        private bool _enableMenthorQText;
        private string _menthorQTextRaw = string.Empty;
        private decimal _menthorQOffset;

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
        private int _fontSize = 12;

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

                // Ensure UI textbox refreshes immediately (legacy LevelsLolo behavior).
                RaisePropertyChanged(nameof(LoloTextRaw));
                _textSizeCache.Clear();

                _dataDirty = true;
                RecalculateValues();
            }
        }
        #endregion

        #region Properties: Sources (MenthorQText)
        [Display(ResourceType = typeof(Resources),
    Name = nameof(Resources.MenthorQText),
    GroupName = nameof(Resources.MenthorQText),
    Description = nameof(Resources.MenthorQTextDesc),
    Order = 40)]
        public bool EnableMenthorQText
        {
            get => _enableMenthorQText;
            set
            {
                if (_enableMenthorQText == value)
                    return;

                _enableMenthorQText = value;
                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Text),
            GroupName = nameof(Resources.MenthorQText),
            Description = nameof(Resources.MenthorQTextDesc),
            Order = 50)]
        public string MenthorQTextRaw
        {
            get => _menthorQTextRaw;
            set
            {
                value ??= string.Empty;

                if (_menthorQTextRaw == value)
                    return;

                _menthorQTextRaw = value;
                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Clear),
            GroupName = nameof(Resources.MenthorQText),
            Description = nameof(Resources.ClearDesc),
            Order = 60)]
        public bool ClearMenthorQTextNow
        {
            get => false;
            set
            {
                if (!value)
                    return;

                if (_menthorQTextRaw.Length == 0)
                    return;

                _menthorQTextRaw = string.Empty;
                RaisePropertyChanged(nameof(MenthorQTextRaw));
                _textSizeCache.Clear();

                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MenthorQOffset),
            GroupName = nameof(Resources.MenthorQText),
            Description = nameof(Resources.MenthorQOffsetDesc),
            Order = 70)]
        public decimal MenthorQOffset
        {
            get => _menthorQOffset;
            set
            {
                if (_menthorQOffset == value)
                    return;

                _menthorQOffset = value;
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

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.FontSize),
            GroupName = nameof(Resources.Labels),
            Description = nameof(Resources.FontSizeDescription),
            Order = 230)]
        [Range(6, 48)]
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value)
                    return;

                _fontSize = value;

                RebuildFont();

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

            _font = new RenderFont("Arial", _fontSize);
        }
        #endregion

        #region Overrides
        protected override void OnCalculate(int bar, decimal value)
        {
            RebuildParsedEntriesIfNeeded();
            RebuildRenderCacheIfNeeded();

            // Shell: no calculations yet.
            // Future commits will parse sources and rebuild render packets when _dataDirty/_visualDirty.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (ChartInfo is null)
                return;

            if (LastVisibleBarNumber > CurrentBar - 1)
                return;

            if (_levels == null || _levels.Length == 0)
                return;

            // Ensure cache exists even if chart render happens before a calculate pass.
            RebuildRenderCacheIfNeeded();

            var chart = ChartInfo;

            int firstX = ChartExtensions.GetXByBar(chart, FirstVisibleBarNumber, false);
            int lastX = ChartExtensions.GetXByBar(chart, LastVisibleBarNumber, false);
            int rightX = LastBarOnly ? lastX : Container.Region.Right;

            int topY = Container.Region.Top;
            int botY = Container.Region.Bottom;

            for (int i = 0; i < _levels.Length; i++)
            {
                var level = _levels[i];

                // Defensive: should not happen if engine is correct, but keep render safe.
                if (level.Labels == null || level.Labels.Length == 0)
                    continue;

                int y = ChartExtensions.GetYByPrice(chart, level.Price, false);
                
                // Defensive guard: some builds return extreme values when price is far outside visible scale.
                if (y <= -1000000 || y >= 1000000)
                    continue;

                if (OnlyVisiblePriceRange)
                {
                    int margin = 20; // px
                    if (y < topY - margin || y > botY + margin)
                        continue;
                }

                var winner = level.Winner;

                var tier = GetTierForWinner(winner);
                int tierIdx = (int)tier;

                // ZeroGamma is informational: always thin tier visuals (legacy behavior).
                if (winner.Category == LevelCategory.ZeroGamma)
                    tierIdx = (int)LineTier.Thin;

                bool dash = winner.Is0Dte && Dash0Dte;
                int dashIdx = dash ? 1 : 0;

                int catIdx = (int)winner.Category;

                // Defensive guard: enum values can be sparse (e.g., Other=100). Keep render safe even if cache is smaller for any reason.
                if (_linePens == null || catIdx < 0 || catIdx >= _linePens.GetLength(0))
                    catIdx = (int)LevelCategory.Other;

                // 0DTE halo pass (draw BEFORE main line) — fixed red halo, legacy-like
                if (winner.Is0Dte && Show0DteHalo)
                {
                    var haloPen = _haloPens[tierIdx];
                    if (haloPen != null)
                        context.DrawLine(haloPen.RenderObject, firstX, y, rightX, y);
                }

                var pen = _linePens[catIdx, tierIdx, dashIdx];
                context.DrawLine(pen.RenderObject, firstX, y, rightX, y);

                // Optional thin dotted accent ON TOP for high-priority 0DTE LG/PW/CW (legacy behavior).
                if (ShouldDraw0DteAccent(winner))
                    context.DrawLine(_pen0DteAccent.RenderObject, firstX, y, rightX, y);

                // Draw label (commit 9)
                var text = level.DisplayText;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var (tw, th) = GetTextSizeCached(context, text);

                    // Position: prefer above line; if clipped, draw below
                    int textY = y - th - OffsetY;
                    if (textY < topY)
                        textY = y + OffsetY;

                    int textX = LabelAlignment == LabelSide.Right
                        ? (rightX - tw - OffsetX)
                        : (Container.Region.Left + OffsetX);

                    // Use same effective color as the line (already includes tier transparency)
                    context.DrawString(text, _font, pen.RenderObject.Color, textX, textY);
                }
            }
        }

        #endregion

        #region Private methods: logging

        private void LogParseWarningsIfNeeded(string[] warnings, string input)
        {
            if (warnings == null || warnings.Length == 0)
                return;

            var nowMs = Environment.TickCount64;

            // Avoid repeated spam: only log for new input, or if enough time has passed.
            if (string.Equals(input, _lastWarningsKey, StringComparison.Ordinal) &&
                (nowMs - _lastWarningLogMs) < _warningThrottleMs)
                return;

            _lastWarningsKey = input;
            _lastWarningLogMs = nowMs;

            var count = Math.Min(warnings.Length, _maxWarningsPerUpdate);

            for (int i = 0; i < count; i++)
                this.LogWarn($"[GammaLevels] {warnings[i]}");

            if (warnings.Length > count)
                this.LogWarn($"[GammaLevels] ... {warnings.Length - count} more warning(s).");
        }

        #endregion

        #region Private methods: sources

        private void EnsureSources()
        {
            if (_sourcesInitialized)
                return;

            _sourcesInitialized = true;

            _sources.Clear();
            _sources.Add(new LoloTextSource(this));
            _sources.Add(new MenthorQTextSource(this));
        }

        private void CollectEntriesFromSources()
        {
            EnsureSources();

            // Fast path: no enabled sources or no data
            // We'll build into lists and then materialize arrays once.
            var entries = new List<ParsedEntry>(128);
            var warnings = new List<string>(16);

            for (int i = 0; i < _sources.Count; i++)
            {
                var src = _sources[i];
                if (src == null || !src.IsEnabled)
                    continue;

                if (!src.TryGetEntries(out var srcEntries, out var srcWarnings))
                    continue;

                if (srcEntries != null && srcEntries.Length > 0)
                    entries.AddRange(srcEntries);

                if (srcWarnings != null && srcWarnings.Length > 0)
                {
                    // Prefix warnings with the source id for clarity.
                    for (int w = 0; w < srcWarnings.Length; w++)
                    {
                        var msg = srcWarnings[w];
                        if (string.IsNullOrWhiteSpace(msg))
                            continue;

                        warnings.Add($"[{src.SourceId}] {msg}");
                    }
                }
            }

            _collectedEntries = entries.Count == 0 ? Array.Empty<ParsedEntry>() : entries.ToArray();

            var warningsKey = $"{LoloTextRaw}\n---\n{MenthorQTextRaw}";
            LogParseWarningsIfNeeded(
                warnings.Count == 0 ? Array.Empty<string>() : warnings.ToArray(),
                warningsKey);
        }

        #endregion

        #region Private methods: parsing (common)

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

        #endregion

        #region Private methods: parsing (LoloText)

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

        #region Private methods: parsing (MenthorQText)

        private static ParseResult ParseMenthorQText(string raw, decimal offset)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new ParseResult(Array.Empty<ParsedEntry>(), Array.Empty<string>());

            raw = raw.Trim();

            // Optional prefix: "$TICKER:"
            // We don't strictly need the ticker for MVP, but we parse it for logging/debug.
            string ticker = string.Empty;
            var colon = raw.IndexOf(':');
            if (colon > 0 && raw[0] == '$')
            {
                ticker = raw.Substring(1, colon - 1).Trim();
                raw = raw.Substring(colon + 1);
            }

            var parts = raw.Split(',');
            if (parts.Length < 2)
                return new ParseResult(Array.Empty<ParsedEntry>(), new[] { "Expected comma-separated label/price pairs." });

            var warnings = new List<string>(8);
            var entries = new List<ParsedEntry>(parts.Length / 2);

            // Parse as label/price pairs: (0,1), (2,3), ...
            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                var labelRaw = (parts[i] ?? string.Empty).Trim();
                var priceRaw = (parts[i + 1] ?? string.Empty).Trim();

                if (labelRaw.Length == 0)
                {
                    warnings.Add($"Empty label at pair index {i / 2 + 1}.");
                    continue;
                }

                if (!TryParseDecimalInvariant(priceRaw, out var price))
                {
                    warnings.Add($"Invalid price '{priceRaw}' for label '{labelRaw}'.");
                    continue;
                }

                price += offset;

                if (!TryMapMenthorQLabel(labelRaw, out var mapped))
                {
                    // Unknown label: keep it as Other, but still plot it (user pasted something new).
                    mapped = new MenthorQMappedLabel(
                        displayLabel: NormalizeWhitespace(labelRaw),
                        category: LevelCategory.Other,
                        rank: 999,
                        is0Dte: Contains0Dte(labelRaw));
                }

                // Build a single-label entry at this price.
                var sourceId = string.IsNullOrEmpty(ticker) ? "MenthorQText" : $"MenthorQText:{ticker}";
                var lvlLabel = new LevelLabel(
                    mapped.Category,
                    mapped.Rank,
                    mapped.Is0Dte,
                    rawLabel: labelRaw,
                    displayLabel: mapped.DisplayLabel);

                entries.Add(new ParsedEntry(price, new[] { lvlLabel }, sourceId));
            }

            if (entries.Count == 0 && warnings.Count == 0)
                warnings.Add("No valid label/price pairs found.");

            return new ParseResult(
                entries.Count == 0 ? Array.Empty<ParsedEntry>() : entries.ToArray(),
                warnings.Count == 0 ? Array.Empty<string>() : warnings.ToArray());
        }

        private readonly struct MenthorQMappedLabel
        {
            public MenthorQMappedLabel(string displayLabel, LevelCategory category, int rank, bool is0Dte)
            {
                DisplayLabel = displayLabel;
                Category = category;
                Rank = rank;
                Is0Dte = is0Dte;
            }

            public string DisplayLabel { get; }
            public LevelCategory Category { get; }
            public int Rank { get; }
            public bool Is0Dte { get; }
        }

        private static bool TryMapMenthorQLabel(string labelRaw, out MenthorQMappedLabel mapped)
        {
            mapped = default;

            var s = NormalizeWhitespace(labelRaw);
            if (s.Length == 0)
                return false;

            bool is0Dte = Contains0Dte(s);

            // Strip trailing "0DTE" for display normalization if present.
            var sNo0 = Remove0DteSuffix(s);

            // Exact common labels
            if (EqualsIgnoreCase(sNo0, "Call Resistance"))
            {
                mapped = new MenthorQMappedLabel("CW" + (is0Dte ? " 0DTE" : string.Empty), LevelCategory.CallWall, 0, is0Dte);
                return true;
            }

            if (EqualsIgnoreCase(sNo0, "Put Support"))
            {
                mapped = new MenthorQMappedLabel("PW" + (is0Dte ? " 0DTE" : string.Empty), LevelCategory.PutWall, 0, is0Dte);
                return true;
            }

            if (EqualsIgnoreCase(sNo0, "HVL"))
            {
                mapped = new MenthorQMappedLabel("VT" + (is0Dte ? " 0DTE" : string.Empty), LevelCategory.VolTrigger, 0, is0Dte);
                return true;
            }

            if (EqualsIgnoreCase(sNo0, "Gamma Wall"))
            {
                // Strongest LargeGamma -> rank 0
                mapped = new MenthorQMappedLabel("LG0" + (is0Dte ? " 0DTE" : string.Empty), LevelCategory.LargeGamma, 0, is0Dte);
                return true;
            }

            if (EqualsIgnoreCase(sNo0, "1D Min"))
            {
                mapped = new MenthorQMappedLabel("1D Min", LevelCategory.DayMin, 0, is0Dte);
                return true;
            }

            if (EqualsIgnoreCase(sNo0, "1D Max"))
            {
                mapped = new MenthorQMappedLabel("1D Max", LevelCategory.DayMax, 0, is0Dte);
                return true;
            }

            // Patterns:
            // - "GEX 1" .. "GEX 10"
            // - "BL 1" .. "BL 10"
            // - "LB 01-13", "RT 01-14", "UB 01-20"
            if (TryParsePrefixNumber(sNo0, "GEX", out var gexN))
            {
                // Smaller number = stronger
                mapped = new MenthorQMappedLabel($"LG{gexN}" + (is0Dte ? " 0DTE" : string.Empty), LevelCategory.LargeGamma, gexN, is0Dte);
                return true;
            }

            if (TryParsePrefixNumber(sNo0, "BL", out var blN))
            {
                mapped = new MenthorQMappedLabel($"BL {blN}" + (is0Dte ? " 0DTE" : string.Empty), LevelCategory.BlindLevel, blN, is0Dte);
                return true;
            }

            if (TryParseBandOrTrigger(sNo0, out var bandType, out var mmdd))
            {
                var cat = bandType switch
                {
                    "LB" => LevelCategory.LowerBand,
                    "UB" => LevelCategory.UpperBand,
                    "RT" => LevelCategory.RiskTrigger,
                    _ => LevelCategory.Other
                };

                mapped = new MenthorQMappedLabel($"{bandType} {mmdd}" + (is0Dte ? " 0DTE" : string.Empty), cat, 0, is0Dte);
                return true;
            }

            return false;
        }


        private static string NormalizeWhitespace(string s)
        {
            if (s == null)
                return string.Empty;

            s = s.Trim();
            if (s.Length == 0)
                return string.Empty;

            // Collapse multiple spaces to single space.
            var sb = new System.Text.StringBuilder(s.Length);
            bool prevSpace = false;

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (!prevSpace)
                        sb.Append(' ');
                    prevSpace = true;
                }
                else
                {
                    sb.Append(ch);
                    prevSpace = false;
                }
            }

            return sb.ToString();
        }

        private static bool Contains0Dte(string s)
        {
            return s.IndexOf("0DTE", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Remove0DteSuffix(string s)
        {
            // Remove trailing "... 0DTE" if present (case-insensitive).
            // Keep internal "0DTE" occurrences (rare).
            var idx = s.LastIndexOf("0DTE", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return s;

            // Only treat as suffix if after it there's only whitespace.
            for (int i = idx + 4; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                    return s;
            }

            var before = s.Substring(0, idx).TrimEnd();
            return before;
        }

        private static bool EqualsIgnoreCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParsePrefixNumber(string s, string prefix, out int n)
        {
            n = 0;

            // Accept "PREFIX 1" or "PREFIX    1"
            if (!s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            var rest = s.Substring(prefix.Length).TrimStart();
            if (rest.Length == 0)
                return false;

            // If rest starts with '-' it's not a number we want.
            int i = 0;
            while (i < rest.Length && char.IsDigit(rest[i]))
                i++;

            if (i == 0)
                return false;

            return int.TryParse(rest.Substring(0, i), NumberStyles.Integer, CultureInfo.InvariantCulture, out n);
        }

        private static bool TryParseBandOrTrigger(string s, out string bandType, out string mmdd)
        {
            bandType = null;
            mmdd = null;

            // Expect: "LB 01-13" / "RT 01-14" / "UB 01-20"
            if (s.Length < 3)
                return false;

            var prefix = s.Substring(0, 2).ToUpperInvariant();
            if (prefix != "LB" && prefix != "RT" && prefix != "UB")
                return false;

            var rest = s.Substring(2).TrimStart();
            if (rest.Length < 5)
                return false;

            // Very lightweight validation: NN-NN
            // We keep as label string; no date parsing in MVP.
            if (!char.IsDigit(rest[0]) || !char.IsDigit(rest[1]) || rest[2] != '-' || !char.IsDigit(rest[3]) || !char.IsDigit(rest[4]))
                return false;

            bandType = prefix;
            mmdd = rest.Substring(0, 5);
            return true;
        }

        #endregion

        #region Private methods: engine

        private static int GetCategoryPriority(LevelCategory category)
        {
            // Lower number = higher priority.
            // Priority (as agreed):
            // Volatility Trigger > PutWall/CallWall > Blind Levels > DayMin/DayMax
            // > Zero Gamma > LargeGamma > Combo > RiskTrigger > Bands > Other
            return category switch
            {
                LevelCategory.VolTrigger => 0,

                LevelCategory.PutWall => 10,
                LevelCategory.CallWall => 11,

                LevelCategory.BlindLevel => 20,

                LevelCategory.DayMin => 30,
                LevelCategory.DayMax => 31,

                LevelCategory.ZeroGamma => 40,

                LevelCategory.LargeGamma => 50,

                LevelCategory.Combo => 60,

                LevelCategory.RiskTrigger => 70,

                LevelCategory.LowerBand => 80,
                LevelCategory.UpperBand => 81,

                _ => 1000
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

            try
            {
                CollectEntriesFromSources();

                _parsedEntries = _collectedEntries;

                if (_parsedEntries.Length == 0)
                {
                    _levels = Array.Empty<Level>();
                    _visualDirty = true;
                    _textSizeCache.Clear();
                    return;
                }

                _levels = BuildLevels(_parsedEntries);
                _visualDirty = true;
                _textSizeCache.Clear();
            }
            catch (Exception ex)
            {
                this.LogError("[GammaLevels] Unexpected error while collecting/parsing sources.", ex);

                _parsedEntries = Array.Empty<ParsedEntry>();
                _collectedEntries = Array.Empty<ParsedEntry>();
                _levels = Array.Empty<Level>();
                _visualDirty = true;
                _textSizeCache.Clear();
            }
        }

        #endregion

        #region Private methods: rendering

        private void RedrawChart()
        {
            // RecalculateValues triggers a render refresh in ATAS indicators.
            // Use it for visual-only changes too to keep behavior consistent.
            RecalculateValues();
        }

        private LineTier GetTierForWinner(LevelLabel winner)
        {
            // Rank 0 or missing behaves as "best" (thick) in legacy.
            int r = winner.Rank <= 0 ? 0 : winner.Rank;

            if (r == 0 || r <= ThickMaxRank)
                return LineTier.Thick;

            if (r <= MediumMaxRank)
                return LineTier.Medium;

            return LineTier.Thin;
        }

        private void RebuildRenderCacheIfNeeded()
        {
            if (!_visualDirty && _linePens != null && _haloPens != null)
                return;

            _visualDirty = false;

            // Build cache arrays if needed
            if (_linePens == null)
            {
                int maxCat = GetMaxLevelCategoryValue();
                _linePens = new PenSettings[maxCat + 1, 3, 2];
            }

            if (_haloPens == null)
                _haloPens = new PenSettings[3];

            // Tier params
            int wThick = Math.Clamp(ThickLineWidth, 1, 20);
            int wMed = Math.Clamp(MediumLineWidth, 1, 20);
            int wThin = Math.Clamp(ThinLineWidth, 1, 20);

            byte aThick = TransparencyToAlpha(ThickLineTransparency);
            byte aMed = TransparencyToAlpha(MediumLineTransparency);
            byte aThin = TransparencyToAlpha(ThinLineTransparency);

            // Halo params (legacy-like): fixed red, width = baseWidth + HaloWidth (extra)
            int haloExtra = Math.Clamp(HaloWidth, 0, 50);
            byte haloA = TransparencyToAlpha(HaloTransparency);

            // Build pens per category/tier/dash
            int catCount2 = _linePens.GetLength(0);

            for (int c = 0; c < catCount2; c++)
            {
                var basePen = GetBasePen((LevelCategory)c);
                var bc = basePen.Color;

                for (int tier = 0; tier < 3; tier++)
                {
                    int width = tier switch
                    {
                        0 => wThick,
                        1 => wMed,
                        _ => wThin
                    };

                    byte alpha = tier switch
                    {
                        0 => aThick,
                        1 => aMed,
                        _ => aThin
                    };

                    // ZeroGamma override (always thin)
                    if ((LevelCategory)c == LevelCategory.ZeroGamma)
                    {
                        width = wThin;
                        alpha = aThin;
                    }

                    var effColor = CrossColor.FromArgb(alpha, bc.R, bc.G, bc.B);

                    // solid
                    _linePens[c, tier, 0] = new PenSettings
                    {
                        Color = effColor,
                        Width = width,
                        LineDashStyle = LineDashStyle.Solid
                    };

                    // dash (only used for 0DTE when Dash0Dte == true)
                    _linePens[c, tier, 1] = new PenSettings
                    {
                        Color = effColor,
                        Width = width,
                        LineDashStyle = LineDashStyle.Dash
                    };
                }
            }

            // Halo pens per tier (solid, fixed red)
            // Note: if Show0DteHalo is false, we still build cache; it's cheap and avoids branching complexity.
            _haloPens[0] = new PenSettings
            {
                Color = CrossColor.FromArgb(haloA, 255, 64, 64),
                Width = Math.Max(1, wThick + haloExtra),
                LineDashStyle = LineDashStyle.Solid
            };

            _haloPens[1] = new PenSettings
            {
                Color = CrossColor.FromArgb(haloA, 255, 64, 64),
                Width = Math.Max(1, wMed + haloExtra),
                LineDashStyle = LineDashStyle.Solid
            };

            _haloPens[2] = new PenSettings
            {
                Color = CrossColor.FromArgb(haloA, 255, 64, 64),
                Width = Math.Max(1, wThin + haloExtra),
                LineDashStyle = LineDashStyle.Solid
            };
        }

        private static byte TransparencyToAlpha(int transparency0To100)
        {
            // UI is "transparency" (0 = opaque, 100 = fully transparent).
            int t = Math.Clamp(transparency0To100, 0, 100);
            int a = (255 * (100 - t)) / 100;
            return (byte)Math.Clamp(a, 0, 255);
        }

        private PenSettings GetBasePen(LevelCategory c)
        {
            return c switch
            {
                LevelCategory.Combo => _penCombo,
                LevelCategory.LargeGamma => _penLargeGamma,
                LevelCategory.VolTrigger => _penVolTrigger,
                LevelCategory.CallWall => _penCallWall,
                LevelCategory.PutWall => _penPutWall,
                LevelCategory.ZeroGamma => _penZeroGamma,

                // MenthorQ palette
                LevelCategory.BlindLevel => _penBlindLevel,
                LevelCategory.DayMin => _penDayMin,
                LevelCategory.DayMax => _penDayMax,
                LevelCategory.RiskTrigger => _penRiskTrigger,
                LevelCategory.LowerBand => _penLowerBand,
                LevelCategory.UpperBand => _penUpperBand,

                _ => _penOther
            };
        }

        private (int W, int H) GetTextSizeCached(RenderContext context, string text)
        {
            if (string.IsNullOrEmpty(text))
                return (0, 0);

            if (_textSizeCache.TryGetValue(text, out var size))
                return size;

            var s = context.MeasureString(text, _font);
            size = ((int)s.Width, (int)s.Height);
            _textSizeCache[text] = size;
            return size;
        }

        private void RebuildFont()
        {
            // Rebuild font when size changes (text metrics depend on it).
            _font = new RenderFont("Arial", _fontSize);

            // Invalidate cached measurements because they depend on font size.
            _textSizeCache.Clear();
        }

        private bool ShouldDraw0DteAccent(LevelLabel winner)
        {
            // Legacy rule: 0DTE + (LG/PW/CW) + rank <= ThickMaxRank.
            // Rank==0 is considered strongest (also used by MenthorQ mappings).
            if (!winner.Is0Dte)
                return false;

            if (winner.Category != LevelCategory.LargeGamma &&
                winner.Category != LevelCategory.PutWall &&
                winner.Category != LevelCategory.CallWall)
                return false;

            return winner.Rank <= ThickMaxRank;
        }

        #endregion

        #region Private methods: misc

        private static int GetMaxLevelCategoryValue()
        {
            // Avoid LINQ allocations; keep this deterministic and cheap.
            int max = 0;

            var values = (LevelCategory[])Enum.GetValues(typeof(LevelCategory));
            for (int i = 0; i < values.Length; i++)
            {
                int v = (int)values[i];
                if (v > max)
                    max = v;
            }

            return max;
        }

        private static bool TryParseDecimalInvariant(string s, out decimal value)
        {
            return decimal.TryParse(
                s,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }

        #endregion
    }
}
