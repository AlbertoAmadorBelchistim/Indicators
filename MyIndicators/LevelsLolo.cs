namespace MyIndicators;

using ATAS.Indicators;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;


[Category("Custom")]
[DisplayName("LevelsLolo")]
    public class LevelsLolo:Indicator
    {
        // ---- Rendering ----
        private RenderFont _font;
        private PenSettings _penCombo = new() { Color = Colors.White, Width = 1 };
        private PenSettings _penLargeGamma = new() { Color = Colors.Yellow, Width = 1 };
        private PenSettings _penVolTrigger = new() { Color = Colors.DeepSkyBlue, Width = 1 };
        private PenSettings _penCallWall = new() { Color = Colors.Orange, Width = 1 };
        private PenSettings _penPutWall = new() { Color = Colors.HotPink, Width = 1 };
        private PenSettings _penZeroGamma = new() { Color = Colors.MediumPurple, Width = 1 };
        private PenSettings _penOther = new() { Color = Colors.Gray, Width = 1 };

        // Final computed levels for rendering (price -> merged entry)
        private readonly Dictionary<decimal, MergedLevel> _levelsByPrice = new();

        // Cache for top-N membership per category (recomputed on parse)
        private readonly HashSet<(Category cat, decimal price)> _isTop = new();

        // ---- Parameters (UI) ----

        [Display(Name = "Font size", GroupName = "Text", Order = 1)]
        [Range(6, 48)]
        public int FontSize
        {
            get => _fontSize;
            set { _fontSize = value; _font = new RenderFont("Arial", _fontSize); RecalculateValues(); }
        }
        private int _fontSize = 10;

        [Display(Name = "Right-aligned text", GroupName = "Text", Order = 2)]
        public bool RightAligned
        {
            get => _rightAligned;
            set { _rightAligned = value; RecalculateValues(); }
        }
        private bool _rightAligned = true;

        [Display(Name = "Last bar only", GroupName = "Text", Order = 3, Description = "Extend line to last visible bar instead of full right border.")]
        public bool LastBarOnly
        {
            get => _lastBarOnly;
            set { _lastBarOnly = value; RecalculateValues(); }
        }
        private bool _lastBarOnly = true;

        [Display(Name = "Offset X", GroupName = "Text", Order = 4)]
        [Range(0, 500)]
        public int OffsetX { get; set; } = 6;

        [Display(Name = "Offset Y", GroupName = "Text", Order = 5)]
        [Range(-500, 500)]
        public int OffsetY { get; set; } = 6;

        [Display(Name = "Top-N per type (thick)", GroupName = "Ranking", Order = 1)]
        [Range(1, 50)]
        public int TopNPerCategory { get; set; } = 5;

        [Display(Name = "Thick width", GroupName = "Ranking", Order = 2)]
        [Range(1, 8)]
        public int ThickWidth { get; set; } = 3;

        [Display(Name = "Thin width", GroupName = "Ranking", Order = 3)]
        [Range(1, 8)]
        public int ThinWidth { get; set; } = 1;

        // Pens per category (editable)
        [Display(Name = "Combo (CO)", GroupName = "Pens", Order = 1)] public PenSettings PenCombo { get => _penCombo; set => _penCombo = value; }
        [Display(Name = "Large Gamma (LG)", GroupName = "Pens", Order = 2)] public PenSettings PenLargeGamma { get => _penLargeGamma; set => _penLargeGamma = value; }
        [Display(Name = "Volatility Trigger (VT)", GroupName = "Pens", Order = 3)] public PenSettings PenVolTrigger { get => _penVolTrigger; set => _penVolTrigger = value; }
        [Display(Name = "Call Wall (CW)", GroupName = "Pens", Order = 4)] public PenSettings PenCallWall { get => _penCallWall; set => _penCallWall = value; }
        [Display(Name = "Put Wall (PW)", GroupName = "Pens", Order = 5)] public PenSettings PenPutWall { get => _penPutWall; set => _penPutWall = value; }
        [Display(Name = "Zero Gamma (ZG)", GroupName = "Pens", Order = 6)] public PenSettings PenZeroGamma { get => _penZeroGamma; set => _penZeroGamma = value; }
        [Display(Name = "Other/Unknown", GroupName = "Pens", Order = 7)] public PenSettings PenOther { get => _penOther; set => _penOther = value; }

        // Main text input
        [Display(Name = "Raw text", GroupName = "Data", Order = 1, Description = "Free text like: $SP: CO44, 7073, LG07, 7048, ...\nSupports '&' for multiple labels before the price.")]
        public string RawText
        {
            get => _rawText;
            set { _rawText = value ?? string.Empty; ParseRawText(); RecalculateValues(); }
        }
        private string _rawText = string.Empty;

        public LevelsLolo() : base(false)
        {
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);
            _font = new RenderFont("Arial", _fontSize);
        }

        // ---- Core types ----

        private enum Category
        {
            Combo,
            LargeGamma,
            VolTrigger,
            CallWall,
            PutWall,
            ZeroGamma,
            Other
        }

        private sealed class LabelToken
        {
            public Category Cat;
            public string Raw;       // e.g., "CO19" or "Zero Gamma" or "CW"
            public int? Rank;        // numeric part; null => “max priority”
            public int EffectiveRank // lower is better; null -> 0
                => Rank.HasValue ? Rank.Value : 0;
        }

        private sealed class MergedLevel
        {
            public decimal Price;
            public List<LabelToken> Labels = new();
            public LabelToken Winner;       // decides style
            public string LabelText;        // concatenated for drawing
        }

        // ---- Parsing ----

        private static readonly Regex NumberOnly = new(@"^\s*\d+(\.\d+)?\s*$", RegexOptions.Compiled);
        // Matches "CO44", "LG07", "VT", "CW", "PW", "ZG13", "Zero Gamma"
        private static readonly Regex LabelPattern = new(@"^\s*(?<name>[A-Za-z ]+?)(?<rank>\d{1,3})?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static string NormalizeInvisible(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            // Remove format characters (zero-width, etc.)
            var cleaned = new string(s.Where(ch => !char.GetUnicodeCategory(ch).HasFlag(UnicodeCategory.Format)).ToArray());
            return cleaned.Trim();
        }

        private static Category MapCategory(string name)
        {
            name = name.ToUpperInvariant();
            // Short aliases
            if (name == "CO" || name == "COMBO") return Category.Combo;
            if (name == "LG" || name == "LARGE GAMMA" || name == "LARGEGAMMA") return Category.LargeGamma;
            if (name == "VT" || name == "VOLATILITY TRIGGER" || name == "VOLATILITYTRIGGER") return Category.VolTrigger;
            if (name == "CW" || name == "CALL WALL" || name == "CALLWALL") return Category.CallWall;
            if (name == "PW" || name == "PUT WALL" || name == "PUTWALL") return Category.PutWall;
            if (name == "ZG" || name == "ZERO GAMMA" || name == "ZEROGAMMA") return Category.ZeroGamma;
            // Fallback
            return Category.Other;
        }

        private void ParseRawText()
        {
            _levelsByPrice.Clear();
            _isTop.Clear();

            var text = NormalizeInvisible(_rawText);
            if (string.IsNullOrWhiteSpace(text)) return;

            // Optional prefix "$SP:" or similar
            var colonIdx = text.IndexOf(':');
            if (colonIdx >= 0)
                text = text[(colonIdx + 1)..];

            // Split by commas, keep order
            var tokens = text.Split(',').Select(t => NormalizeInvisible(t)).Where(t => t.Length > 0).ToList();
            if (tokens.Count == 0) return;

            var pendingLabels = new List<LabelToken>();
            foreach (var token in tokens)
            {
                // A token may contain multiple labels separated by "&"
                var parts = token.Split('&').Select(p => NormalizeInvisible(p)).Where(p => p.Length > 0).ToArray();

                // If single numeric => price for pending labels
                if (parts.Length == 1 && NumberOnly.IsMatch(parts[0]))
                {
                    if (!decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                    {
                        // Try current culture as fallback
                        if (!decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.CurrentCulture, out price))
                            continue; // skip malformed number
                    }
                    if (pendingLabels.Count == 0)
                        continue; // no label for this number; ignore

                    // Commit pending labels to this price
                    if (!_levelsByPrice.TryGetValue(price, out var merged))
                    {
                        merged = new MergedLevel { Price = price };
                        _levelsByPrice[price] = merged;
                    }

                    merged.Labels.AddRange(pendingLabels);
                    pendingLabels.Clear();
                }
                else
                {
                    // One or more labels in the same token (with '&')
                    foreach (var part in parts)
                    {
                        var m = LabelPattern.Match(part);
                        if (!m.Success) continue;

                        var name = NormalizeInvisible(m.Groups["name"].Value);
                        var rankStr = m.Groups["rank"].Success ? m.Groups["rank"].Value : null;

                        int? rank = null;
                        if (!string.IsNullOrEmpty(rankStr) && int.TryParse(rankStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
                            rank = r;

                        // Normalize special literal "Zero Gamma"
                        if (name.Equals("Zero Gamma", StringComparison.OrdinalIgnoreCase))
                            name = "ZG";

                        var cat = MapCategory(name);
                        pendingLabels.Add(new LabelToken
                        {
                            Cat = cat,
                            Raw = (rank.HasValue ? $"{name.ToUpperInvariant()}{rank.Value:D2}" : name),
                            Rank = rank // null => max priority
                        });
                    }
                }
            }

            // Build winners + label text; compute top-N per category
            ComputeWinnersAndTopSets();
        }

        private void ComputeWinnersAndTopSets()
        {
            // Per-category sorted lists (lowest rank first; null rank = 0 => highest)
            var perCat = new Dictionary<Category, List<(decimal price, int effRank)>>();

            foreach (var kv in _levelsByPrice)
            {
                var price = kv.Key;
                var labels = kv.Value.Labels;

                // Winner: lowest EffectiveRank; tie-breaker by category order to stabilize
                var winner = labels.OrderBy(l => l.EffectiveRank)
                                   .ThenBy(l => (int)l.Cat)
                                   .First();

                kv.Value.Winner = winner;
                kv.Value.LabelText = string.Join(" & ", labels.Select(l => FormatLabelForText(l)));

                // Feed per-category for Top-N computation (only labels of that category count)
                foreach (var l in labels)
                {
                    if (!perCat.TryGetValue(l.Cat, out var list))
                        perCat[l.Cat] = list = new List<(decimal, int)>();
                    list.Add((price, l.EffectiveRank));
                }
            }

            // Compute TopN sets per category
            _isTop.Clear();
            foreach (var kv in perCat)
            {
                var cat = kv.Key;
                var list = kv.Value.OrderBy(t => t.effRank)
                                   .ThenBy(t => t.price) // stabilize
                                   .Take(TopNPerCategory)
                                   .ToArray();
                foreach (var t in list)
                    _isTop.Add((cat, t.price));
            }
        }

        private static string FormatLabelForText(LabelToken l)
        {
            // Human-friendly label for drawing
            // Example: CO19, LG11, VT, CW, Zero Gamma
            return l.Cat switch
            {
                Category.Combo => l.Rank.HasValue ? $"CO{l.Rank}" : "CO",
                Category.LargeGamma => l.Rank.HasValue ? $"LG{l.Rank}" : "LG",
                Category.VolTrigger => "VT",
                Category.CallWall => "CW",
                Category.PutWall => "PW",
                Category.ZeroGamma => l.Rank.HasValue ? $"ZG{l.Rank}" : "Zero Gamma",
                _ => l.Raw.ToUpperInvariant()
            };
        }

        // ---- ATAS cycle ----

        protected override void OnCalculate(int bar, decimal value)
        {
            // Nothing to calculate per-bar; drawing uses current dictionary
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (_levelsByPrice.Count == 0)
                return;

            var chart = ChartInfo;
            var firstX = Extensions.GetXByBar(chart, FirstVisibleBarNumber, false);
            var lastX = Extensions.GetXByBar(chart, LastVisibleBarNumber, false);
            var rightX = LastBarOnly ? lastX : Container.Region.Right;

            foreach (var kv in _levelsByPrice.OrderBy(k => k.Key)) // sorted by price asc
            {
                var price = kv.Key;
                var merged = kv.Value;
                var cat = merged.Winner.Cat;

                var pen = GetPen(cat);
                var y = Extensions.GetYByPrice(chart, price, false);

                // Pick width depending on Top-N membership for the *winning* category at this price
                var width = _isTop.Contains((cat, price)) ? ThickWidth : ThinWidth;

                // Draw line
                var linePen = new PenSettings { Color = pen.Color, Width = width, LineDashStyle = pen.LineDashStyle };
                context.DrawLine(linePen.RenderObject, firstX, y, rightX, y);

                // Draw label at y, aligned
                var label = merged.LabelText;
                var size = context.MeasureString(label, _font);
                var textY = y - size.Height - OffsetY;
                int textX = RightAligned ? rightX - size.Width - OffsetX
                                         : Container.Region.Left + OffsetX;

                context.DrawString(label, _font, pen.RenderObject.Color, textX, textY);
            }
        }

        private PenSettings GetPen(Category c) =>
            c switch
            {
                Category.Combo => _penCombo,
                Category.LargeGamma => _penLargeGamma,
                Category.VolTrigger => _penVolTrigger,
                Category.CallWall => _penCallWall,
                Category.PutWall => _penPutWall,
                Category.ZeroGamma => _penZeroGamma,
                _ => _penOther
            };
    
    }

