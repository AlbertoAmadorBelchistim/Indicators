namespace MyIndicators
{
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
    using System.Text.RegularExpressions;

    [Category("Custom")]
    [DisplayName("LevelsLolo")]
    public class LevelsLolo : Indicator
    {
        // -----------------------------
        // Rendering
        // -----------------------------
        private RenderFont _font;

        // === SpotGamma semantic palette (black background) ===
        // PW  = support (green)          | CW  = resistance (red)
        // VT  = regime trigger (yellow)  | LG  = institutional absorption (orange deep)
        // CO  = price magnet (amber)     | ZG  = regime context (neutral gray)
        // 0DTE halo accent (cyan) is used as a secondary overlay stroke in OnRender.

        private PenSettings _penPutWall = new PenSettings { Color = CrossColor.FromArgb(255, 0, 255, 128), Width = 2 }; // PW  #00FF80
        private PenSettings _penCallWall = new PenSettings { Color = CrossColor.FromArgb(255, 255, 64, 64), Width = 2 }; // CW  #FF4040
        private PenSettings _penVolTrigger = new PenSettings { Color = CrossColor.FromArgb(255, 255, 215, 0), Width = 3 }; // VT  #FFD700
        private PenSettings _penLargeGamma = new PenSettings { Color = CrossColor.FromArgb(255, 255, 136, 0), Width = 2 }; // LG  #FF8800
        private PenSettings _penCombo = new PenSettings { Color = CrossColor.FromArgb(255, 255, 192, 77), Width = 1 }; // CO  #FFC04D
        private PenSettings _penZeroGamma = new PenSettings { Color = CrossColor.FromArgb(255, 170, 170, 170), Width = 1 }; // ZG  #AAAAAA
        private PenSettings _penOther = new PenSettings { Color = CrossColor.FromArgb(255, 160, 160, 160), Width = 1 }; // Fallback gray

        // 0DTE halo accent (secondary overlay stroke). Use as:
        // context.DrawLine(_pen0DTEHalo.RenderObject, firstX, y, rightX, y) BEFORE the main dashed line.
        private PenSettings _pen0DTEHalo = new PenSettings { Color = CrossColor.FromArgb(120, 0, 255, 255), Width = 1 }; // cyan glow (#00FFFF, alpha ~120)


        // Price -> merged labels at that price
        private readonly Dictionary<decimal, MergedLevel> _levelsByPrice = new Dictionary<decimal, MergedLevel>();

        // -----------------------------
        // Parameters - Text
        // -----------------------------
        [Display(Name = "Font size", GroupName = "Text", Order = 1)]
        [Range(6, 48)]
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                _font = new RenderFont("Arial", _fontSize);
                RecalculateValues();
            }
        }
        private int _fontSize = 10;

        [Display(Name = "Right-aligned text", GroupName = "Text", Order = 2)]
        public bool RightAligned
        {
            get => _rightAligned;
            set { _rightAligned = value; RecalculateValues(); }
        }
        private bool _rightAligned = true;

        [Display(Name = "Last bar only", GroupName = "Text", Order = 3,
            Description = "Extend to last visible bar instead of full right edge.")]
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

        // -----------------------------
        // Parameters - Tiers & alpha
        // -----------------------------
        [Display(Name = "Thick up to rank", GroupName = "Width tiers", Order = 1)]
        [Range(1, 20)]
        public int ThickMaxRank { get; set; } = 3;

        [Display(Name = "Medium up to rank", GroupName = "Width tiers", Order = 2)]
        [Range(1, 50)]
        public int MediumMaxRank { get; set; } = 10;

        [Display(Name = "Width (thick)", GroupName = "Width tiers", Order = 3)]
        [Range(1, 8)]
        public int ThickWidth { get; set; } = 3;

        [Display(Name = "Width (medium)", GroupName = "Width tiers", Order = 4)]
        [Range(1, 8)]
        public int MediumWidth { get; set; } = 2;

        [Display(Name = "Width (thin)", GroupName = "Width tiers", Order = 5)]
        [Range(1, 8)]
        public int ThinWidth { get; set; } = 1;

        [Display(Name = "Alpha (thick tier)", GroupName = "Alpha tiers", Order = 1)]
        [Range(0, 255)]
        public int ThickAlpha { get; set; } = 255;

        [Display(Name = "Alpha (medium tier)", GroupName = "Alpha tiers", Order = 2)]
        [Range(0, 255)]
        public int MediumAlpha { get; set; } = 210;

        [Display(Name = "Alpha (thin tier)", GroupName = "Alpha tiers", Order = 3)]
        [Range(0, 255)]
        public int ThinAlpha { get; set; } = 160;

        // -----------------------------
        // Parameters - Pens
        // -----------------------------
        [Display(Name = "Combo (CO)", GroupName = "Pens", Order = 1)]
        public PenSettings PenCombo { get => _penCombo; set => _penCombo = value; }

        [Display(Name = "Large Gamma (LG)", GroupName = "Pens", Order = 2)]
        public PenSettings PenLargeGamma { get => _penLargeGamma; set => _penLargeGamma = value; }

        [Display(Name = "Volatility Trigger (VT)", GroupName = "Pens", Order = 3)]
        public PenSettings PenVolTrigger { get => _penVolTrigger; set => _penVolTrigger = value; }

        [Display(Name = "Call Wall (CW)", GroupName = "Pens", Order = 4)]
        public PenSettings PenCallWall { get => _penCallWall; set => _penCallWall = value; }

        [Display(Name = "Put Wall (PW)", GroupName = "Pens", Order = 5)]
        public PenSettings PenPutWall { get => _penPutWall; set => _penPutWall = value; }

        [Display(Name = "Zero Gamma (ZG)", GroupName = "Pens", Order = 6)]
        public PenSettings PenZeroGamma { get => _penZeroGamma; set => _penZeroGamma = value; }

        [Display(Name = "Other/Unknown", GroupName = "Pens", Order = 7)]
        public PenSettings PenOther { get => _penOther; set => _penOther = value; }

        [Display(Name = "Enable 0DTE halo", GroupName = "Accents", Order = 1)]
        public bool Enable0DTEHalo { get; set; } = true;

        [Display(Name = "0DTE halo alpha (0-255)", GroupName = "Accents", Order = 2)]
        [Range(0, 255)]
        public int HaloAlpha { get; set; } = 120;

        [Display(Name = "0DTE halo extra width (px)", GroupName = "Accents", Order = 3)]
        [Range(0, 10)]
        public int HaloExtraWidth { get; set; } = 2;

        // -----------------------------
        // Parameters - Data & visibility
        // -----------------------------
        [Display(Name = "Raw text", GroupName = "Data", Order = 1,
            Description = "Example: $SP: CO44, 7073, LG07, 7048, CO05 & LG14, 6898, VT 0DTE, 6743, LG1 0DTE, 6720 ...")]
        public string RawText
        {
            get => _rawText;
            set
            {
                _rawText = value ?? string.Empty;
                ParseRawText();
                RecalculateValues();
            }
        }
        private string _rawText = string.Empty;

        [Display(Name = "Clear text now", GroupName = "Data", Order = 2,
            Description = "Toggle to clear the input box.")]
        public bool ClearTextNow
        {
            get => false;
            set
            {
                if (value)
                {
                    // 1) Clear internal state
                    _rawText = string.Empty;
                    RawText = string.Empty;
                    _levelsByPrice.Clear();

                    // 2) Notify UI bindings so the textbox clears instantly
                    RaisePropertyChanged(nameof(RawText));

                    // 3) Recalculate
                    RecalculateValues();
                }
            }
        }

        [Display(Name = "Only visible price range", GroupName = "Visibility", Order = 1,
            Description = "Skip drawing levels whose price is outside the visible Y-range.")]
        public bool OnlyVisiblePriceRange { get; set; } = true;

        public LevelsLolo() : base(false)
        {
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);
            _font = new RenderFont("Arial", _fontSize);
        }

        // -----------------------------
        // Model
        // -----------------------------
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
            public string Raw;   // original fragment, e.g. "LG1 0DTE"
            public int? Rank;    // null => max priority (treated as 0)
            public bool Is0DTE;

            public int EffectiveRank => Rank.HasValue ? Rank.Value : 0; // lower is better
        }

        private sealed class MergedLevel
        {
            public decimal Price;
            public List<LabelToken> Labels = new List<LabelToken>();
            public LabelToken Winner;
            public string LabelText;
        }

        // -----------------------------
        // Parsing
        // -----------------------------
        private static readonly Regex NumberOnly = new Regex(@"^\s*\d+(\.\d+)?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // e.g.: "CO44", "LG1 0DTE", "VT 0DTE", "Zero Gamma", "CW", "PW"
        private static readonly Regex LabelPattern = new Regex(
            @"^\s*(?<name>[A-Za-z ]+?)(?<rank>\d{1,3})?(?<suffix>\s+0DTE)?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static string NormalizeInvisible(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var arr = s.ToCharArray();
            var res = new List<char>(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(arr[i]);
                if (cat != UnicodeCategory.Format)
                    res.Add(arr[i]);
            }
            return new string(res.ToArray()).Trim();
        }

        private static Category MapCategory(string name)
        {
            var u = (name ?? string.Empty).ToUpperInvariant().Replace("  ", " ").Trim();

            if (u == "CO" || u == "COMBO") return Category.Combo;
            if (u == "LG" || u == "LARGE GAMMA" || u == "LARGEGAMMA") return Category.LargeGamma;
            if (u == "VT" || u == "VOLATILITY TRIGGER" || u == "VOLATILITYTRIGGER") return Category.VolTrigger;
            if (u == "CW" || u == "CALL WALL" || u == "CALLWALL") return Category.CallWall;
            if (u == "PW" || u == "PUT WALL" || u == "PUTWALL") return Category.PutWall;
            if (u == "ZG" || u == "ZERO GAMMA" || u == "ZEROGAMMA") return Category.ZeroGamma;

            return Category.Other;
        }

        private void ParseRawText()
        {
            _levelsByPrice.Clear();

            var text = NormalizeInvisible(_rawText);
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Optional prefix like "$SP:"
            int colon = text.IndexOf(':');
            if (colon >= 0 && colon + 1 < text.Length)
                text = text.Substring(colon + 1);

            var tokens = text.Split(',');
            var cleaned = new List<string>();
            foreach (var t0 in tokens)
            {
                var t = NormalizeInvisible(t0);
                if (!string.IsNullOrEmpty(t))
                    cleaned.Add(t);
            }
            if (cleaned.Count == 0)
                return;

            var pending = new List<LabelToken>();

            foreach (var token in cleaned)
            {
                // Split by '&' (multiple labels before one price)
                var parts = token.Split('&');
                bool isSingleNumber = (parts.Length == 1) && NumberOnly.IsMatch(parts[0]);

                if (isSingleNumber)
                {
                    if (pending.Count == 0)
                        continue;

                    // Parse price with invariant first then current culture
                    if (!TryParseDecimal(parts[0], out var price))
                        continue;

                    if (!_levelsByPrice.TryGetValue(price, out var merged))
                    {
                        merged = new MergedLevel { Price = price };
                        _levelsByPrice[price] = merged;
                    }
                    merged.Labels.AddRange(pending);
                    pending.Clear();
                }
                else
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        var part = NormalizeInvisible(parts[i]);
                        if (string.IsNullOrEmpty(part))
                            continue;

                        var m = LabelPattern.Match(part);
                        if (!m.Success)
                            continue;

                        string rawName = NormalizeInvisible(m.Groups["name"].Value);

                        // Normalize "Zero Gamma" to "ZG" to reduce label clutter
                        if (string.Equals(rawName, "Zero Gamma", StringComparison.OrdinalIgnoreCase))
                            rawName = "ZG";

                        int? rank = null;
                        if (m.Groups["rank"].Success && int.TryParse(m.Groups["rank"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
                            rank = r; // lower is stronger (1 > 5 > 15)

                        bool is0dte = m.Groups["suffix"].Success;

                        var cat = MapCategory(rawName);

                        pending.Add(new LabelToken
                        {
                            Cat = cat,
                            Raw = part,
                            Rank = rank,
                            Is0DTE = is0dte
                        });
                    }
                }
            }

            // Decide winners & label text per price
            foreach (var kv in _levelsByPrice)
            {
                var ml = kv.Value;
                if (ml.Labels.Count == 0)
                    continue;

                // Winner rule:
                // 1) Lowest EffectiveRank wins (null => 0)
                // 2) Tie: higher category priority wins (VT > LG > PW/CW > CO > ZG > Other)
                LabelToken winner = ml.Labels[0];
                for (int i = 1; i < ml.Labels.Count; i++)
                {
                    var cand = ml.Labels[i];

                    var wr = winner.EffectiveRank;
                    var cr = cand.EffectiveRank;

                    if (cr < wr) { winner = cand; continue; }
                    if (cr > wr) { continue; }

                    // Same rank => category priority
                    var wp = CategoryPriority(winner.Cat);
                    var cp = CategoryPriority(cand.Cat);
                    if (cp < wp) // lower priority number = higher priority
                        winner = cand;
                }
                ml.Winner = winner;

                // Build merged label text "LG01 0DTE & PW10 & CO2"
                var pieces = new List<string>(ml.Labels.Count);
                foreach (var l in ml.Labels)
                    pieces.Add(FormatLabel(l));
                ml.LabelText = string.Join(" & ", pieces);
            }
        }

        private static int CategoryPriority(Category c)
        {
            // Lower number = higher priority
            // VT > LG > PW/CW > CO > ZG > Other
            switch (c)
            {
                case Category.VolTrigger: return 0;
                case Category.LargeGamma: return 1;
                case Category.PutWall:    // tie with CallWall
                case Category.CallWall: return 2;
                case Category.Combo: return 3;
                case Category.ZeroGamma: return 4;
                default: return 5;
            }
        }

        private static string FormatLabel(LabelToken l)
        {
            string baseName = l.Cat switch
            {
                Category.Combo => "CO",
                Category.LargeGamma => "LG",
                Category.VolTrigger => "VT",
                Category.CallWall => "CW",
                Category.PutWall => "PW",
                Category.ZeroGamma => "ZG",
                _ => l.Raw.ToUpperInvariant()
            };

            // ZG is informational; keep concise tag
            string rankTxt = l.Rank.HasValue ? l.Rank.Value.ToString() : "";
            string suffix = l.Is0DTE ? " 0DTE" : "";

            return (baseName + rankTxt + suffix).Trim();
        }

        private static bool TryParseDecimal(string s, out decimal value)
        {
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;
            return false;
        }

        // -----------------------------
        // ATAS
        // -----------------------------
        protected override void OnCalculate(int bar, decimal value)
        {
            // No per-bar calculation; everything is drawn from parsed state.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (_levelsByPrice.Count == 0)
                return;

            var chart = ChartInfo;

            int firstX = Extensions.GetXByBar(chart, FirstVisibleBarNumber, false);
            int lastX = Extensions.GetXByBar(chart, LastVisibleBarNumber, false);
            int rightX = LastBarOnly ? lastX : Container.Region.Right;

            int topY = Container.Region.Top;
            int botY = Container.Region.Bottom;

            foreach (var kv in _levelsByPrice.OrderBy(k => k.Key))
            {
                decimal price = kv.Key;
                var merged = kv.Value;
                if (merged.Winner == null)
                    continue;

                int y = Extensions.GetYByPrice(chart, price, false);

                if (OnlyVisiblePriceRange)
                {
                    if (y < topY || y > botY)
                        continue;
                }

                var winner = merged.Winner;
                var basePen = GetBasePen(winner.Cat);

                // Width by rank (null/0 => thick; 1..ThickMaxRank => thick; <= MediumMaxRank => medium; else thin)
                var widthTier = GetWidthTier(winner);
                int width = widthTier switch
                {
                    0 => ThickWidth,
                    1 => MediumWidth,
                    _ => ThinWidth
                };

                // Zero Gamma is informational: always thin width & thin alpha
                if (winner.Cat == Category.ZeroGamma)
                    width = ThinWidth;

                // Alpha tier
                byte alpha = widthTier switch
                {
                    0 => (byte)ThickAlpha,
                    1 => (byte)MediumAlpha,
                    _ => (byte)ThinAlpha
                };

                if (winner.Cat == Category.ZeroGamma)
                    alpha = (byte)ThinAlpha;

                // Build effective pen color with tier alpha (keep RGB from base)
                var bc = basePen.Color;
                var effColor = CrossColor.FromArgb(alpha, bc.R, bc.G, bc.B);

                var effPen = new PenSettings
                {
                    Color = effColor,
                    Width = width,
                    LineDashStyle = (winner.Is0DTE ? LineDashStyle.Dash : basePen.LineDashStyle)
                };

                // 0DTE halo pass (draw BEFORE main line) — FIXED RED HALO
                if (winner.Is0DTE && Enable0DTEHalo)
                {
                    var halo = new PenSettings
                    {
                        // Rojo brillante con alpha configurable
                        Color = CrossColor.FromArgb((byte)Math.Clamp(HaloAlpha, 0, 255), 255, 64, 64), // #FF4040 con alpha
                        Width = Math.Max(1, width + HaloExtraWidth),
                        LineDashStyle = LineDashStyle.Solid
                    };

                    context.DrawLine(halo.RenderObject, firstX, y, rightX, y);
                }

                // Draw line
                context.DrawLine(effPen.RenderObject, firstX, y, rightX, y);

                // (optional) thin dotted accent ON TOP for high-priority 0DTE LG/PW/CW
                bool highPriority0DTE =
                    winner.Is0DTE &&
                    (winner.Cat == Category.LargeGamma || winner.Cat == Category.PutWall || winner.Cat == Category.CallWall) &&
                    winner.EffectiveRank <= ThickMaxRank;

                if (highPriority0DTE)
                {
                    var accent = new PenSettings
                    {
                        Color = CrossColor.FromArgb(220, 255, 64, 64), // brighter red
                        Width = 1,
                        LineDashStyle = LineDashStyle.Dot
                    };
                    context.DrawLine(accent.RenderObject, firstX, y, rightX, y);
                }

                // Draw label
                string label = merged.LabelText;
                var size = context.MeasureString(label, _font);
                int textY = y - size.Height - OffsetY;
                int textX = RightAligned ? (rightX - size.Width - OffsetX) : (Container.Region.Left + OffsetX);

                // Clip text if needed (simple guard)
                if (textY < topY) textY = y + OffsetY;

                context.DrawString(label, _font, effPen.RenderObject.Color, textX, textY);
            }
        }

        private int GetWidthTier(LabelToken winner)
        {
            var r = winner.EffectiveRank; // 0 if null
            if (r <= ThickMaxRank) return 0;     // thick
            if (r <= MediumMaxRank) return 1;    // medium
            return 2;                             // thin
        }

        private PenSettings GetBasePen(Category c)
        {
            switch (c)
            {
                case Category.Combo: return _penCombo;
                case Category.LargeGamma: return _penLargeGamma;
                case Category.VolTrigger: return _penVolTrigger;
                case Category.CallWall: return _penCallWall;
                case Category.PutWall: return _penPutWall;
                case Category.ZeroGamma: return _penZeroGamma;
                default: return _penOther;
            }
        }
    }
}
