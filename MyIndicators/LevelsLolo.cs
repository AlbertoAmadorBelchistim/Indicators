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
public class LevelsLolo : Indicator
{
    // -------- Rendering --------
    private RenderFont _font;

    // Base pens (0DTE inherits color & uses dashed line)
    private PenSettings _penCombo = new PenSettings { Color = Colors.White, Width = 1 };
    private PenSettings _penLargeGamma = new PenSettings { Color = Colors.Yellow, Width = 1 };
    private PenSettings _penVolTrigger = new PenSettings { Color = Colors.DeepSkyBlue, Width = 1 };
    private PenSettings _penCallWall = new PenSettings { Color = Colors.Orange, Width = 1 };
    private PenSettings _penPutWall = new PenSettings { Color = Colors.HotPink, Width = 1 };
    private PenSettings _penZeroGamma = new PenSettings { Color = Colors.MediumPurple, Width = 1 };
    private PenSettings _penOther = new PenSettings { Color = Colors.Gray, Width = 1 };

    // Merged levels: price -> entry
    private readonly Dictionary<decimal, MergedLevel> _levelsByPrice = new Dictionary<decimal, MergedLevel>();

    // -------- Parameters --------

    [Display(Name = "Font size", GroupName = "Text", Order = 1)]
    [Range(6, 48)]
    public int FontSize
    {
        get { return _fontSize; }
        set { _fontSize = value; _font = new RenderFont("Arial", _fontSize); RecalculateValues(); }
    }
    private int _fontSize = 10;

    [Display(Name = "Right-aligned text", GroupName = "Text", Order = 2)]
    public bool RightAligned
    {
        get { return _rightAligned; }
        set { _rightAligned = value; RecalculateValues(); }
    }
    private bool _rightAligned = true;

    [Display(Name = "Last bar only", GroupName = "Text", Order = 3,
        Description = "Extend to last visible bar instead of full right border.")]
    public bool LastBarOnly
    {
        get { return _lastBarOnly; }
        set { _lastBarOnly = value; RecalculateValues(); }
    }
    private bool _lastBarOnly = true;

    [Display(Name = "Offset X", GroupName = "Text", Order = 4)]
    [Range(0, 500)]
    public int OffsetX { get; set; } = 6;

    [Display(Name = "Offset Y", GroupName = "Text", Order = 5)]
    [Range(-500, 500)]
    public int OffsetY { get; set; } = 6;

    // Width tiers
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

    // Editable pens
    [Display(Name = "Combo (CO)", GroupName = "Pens", Order = 1)] public PenSettings PenCombo { get { return _penCombo; } set { _penCombo = value; } }
    [Display(Name = "Large Gamma (LG)", GroupName = "Pens", Order = 2)] public PenSettings PenLargeGamma { get { return _penLargeGamma; } set { _penLargeGamma = value; } }
    [Display(Name = "Volatility Trigger (VT)", GroupName = "Pens", Order = 3)] public PenSettings PenVolTrigger { get { return _penVolTrigger; } set { _penVolTrigger = value; } }
    [Display(Name = "Call Wall (CW)", GroupName = "Pens", Order = 4)] public PenSettings PenCallWall { get { return _penCallWall; } set { _penCallWall = value; } }
    [Display(Name = "Put Wall (PW)", GroupName = "Pens", Order = 5)] public PenSettings PenPutWall { get { return _penPutWall; } set { _penPutWall = value; } }
    [Display(Name = "Zero Gamma (ZG)", GroupName = "Pens", Order = 6)] public PenSettings PenZeroGamma { get { return _penZeroGamma; } set { _penZeroGamma = value; } }
    [Display(Name = "Other/Unknown", GroupName = "Pens", Order = 7)] public PenSettings PenOther { get { return _penOther; } set { _penOther = value; } }

    // Free text input
    [Display(Name = "Raw text", GroupName = "Data", Order = 1,
        Description = "Example: $SP: CO44, 7073, LG07, 7048, CO05 & LG14, 6898, VT 0DTE, 6743, LG1 0DTE, 6720 ...")]
    public string RawText
    {
        get { return _rawText; }
        set { _rawText = value ?? string.Empty; ParseRawText(); RecalculateValues(); }
    }
    private string _rawText = string.Empty;

    // UI trigger to clear text
    [Display(Name = "Clear text now", GroupName = "Data", Order = 2, Description = "Toggle to clear the input box.")]
    public bool ClearTextNow
    {
        get { return false; }
        set
        {
            if (value)
            {
                _rawText = string.Empty;
                _levelsByPrice.Clear();
                RecalculateValues();
            }
        }
    }

    public LevelsLolo() : base(false)
    {
        EnableCustomDrawing = true;
        SubscribeToDrawingEvents(DrawingLayouts.Final);
        _font = new RenderFont("Arial", _fontSize);
    }

    // -------- Model --------

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
        public int? Rank;    // null => max priority
        public bool Is0DTE;

        public int EffectiveRank { get { return Rank.HasValue ? Rank.Value : 0; } } // lower is better
    }

    private sealed class MergedLevel
    {
        public decimal Price;
        public List<LabelToken> Labels = new List<LabelToken>();
        public LabelToken Winner;
        public string LabelText;
    }

    // -------- Parsing --------

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

        // remove Unicode format chars and trim
        var arr = s.ToCharArray();
        var res = new List<char>(arr.Length);
        for (int i = 0; i < arr.Length; i++)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(arr[i]);
            if (cat != System.Globalization.UnicodeCategory.Format)
                res.Add(arr[i]);
        }
        return new string(res.ToArray()).Trim();
    }

    private static Category MapCategory(string name)
    {
        var u = (name ?? string.Empty).ToUpperInvariant();
        if (u == "CO" || u == "COMBO") return Category.Combo;
        if (u == "LG" || u == "LARGE GAMMA" || u == "LARGEGAMMA") return Category.LargeGamma;
        if (u == "VT" || u == "VOLATILITY TRIGGER" || u == "VOLATILITYTRIGGER") return Category.VolTrigger;
        if (u == "CW" || u == "CALL WALL" || u == "CALLWALL") return Category.CallWall;
        if (u == "PW" || u == "PUT WALL" || u == "PUTWALL") return Category.PutWall;
        if (u == "ZG" || u == "ZERO GAMMA" || u == "ZEROGAMMA" || u == "ZERO GAMMA") return Category.ZeroGamma;
        return Category.Other;
    }

    private void ParseRawText()
    {
        _levelsByPrice.Clear();

        var text = NormalizeInvisible(_rawText);
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Optional prefix "$SP:"
        int colon = text.IndexOf(':');
        if (colon >= 0 && colon + 1 < text.Length)
            text = text.Substring(colon + 1);

        var tokens = text.Split(',');
        var cleaned = new List<string>();
        for (int i = 0; i < tokens.Length; i++)
        {
            var t = NormalizeInvisible(tokens[i]);
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
                decimal price;
                var ptxt = parts[0];
                if (!decimal.TryParse(ptxt, NumberStyles.Any, CultureInfo.InvariantCulture, out price) &&
                    !decimal.TryParse(ptxt, NumberStyles.Any, CultureInfo.CurrentCulture, out price))
                    continue;

                if (pending.Count == 0)
                    continue;

                MergedLevel merged;
                if (!_levelsByPrice.TryGetValue(price, out merged))
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
                    if (string.Equals(rawName, "Zero Gamma", StringComparison.OrdinalIgnoreCase))
                        rawName = "ZG";

                    int r;
                    int? rank = null;
                    if (m.Groups["rank"].Success && int.TryParse(m.Groups["rank"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out r))
                        rank = r;

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

            // winner: lowest EffectiveRank; tie-breaker by category ordinal for stability
            LabelToken winner = ml.Labels[0];
            for (int i = 1; i < ml.Labels.Count; i++)
            {
                var cand = ml.Labels[i];
                if (cand.EffectiveRank < winner.EffectiveRank)
                    winner = cand;
                else if (cand.EffectiveRank == winner.EffectiveRank)
                {
                    if ((int)cand.Cat < (int)winner.Cat)
                        winner = cand;
                }
            }
            ml.Winner = winner;

            // Build label text (CO19 & LG11 0DTE …)
            var pieces = new List<string>(ml.Labels.Count);
            for (int i = 0; i < ml.Labels.Count; i++)
                pieces.Add(FormatLabel(ml.Labels[i]));
            ml.LabelText = string.Join(" & ", pieces.ToArray());
        }
    }

    private static string FormatLabel(LabelToken l)
    {
        string baseName;
        switch (l.Cat)
        {
            case Category.Combo: baseName = "CO"; break;
            case Category.LargeGamma: baseName = "LG"; break;
            case Category.VolTrigger: baseName = "VT"; break;
            case Category.CallWall: baseName = "CW"; break;
            case Category.PutWall: baseName = "PW"; break;
            case Category.ZeroGamma: baseName = "Zero Gamma"; break;
            default: baseName = l.Raw.ToUpperInvariant(); break;
        }

        string rankTxt = (l.Rank.HasValue ? (baseName == "Zero Gamma" ? (" " + l.Rank.Value.ToString()) : l.Rank.Value.ToString()) : "");
        string suffix = l.Is0DTE ? " 0DTE" : "";

        return (baseName + rankTxt + suffix).Trim();
    }

    // -------- ATAS --------

    protected override void OnCalculate(int bar, decimal value)
    {
        // No per-bar calc; all is drawn from parsed state.
    }

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (_levelsByPrice.Count == 0)
            return;

        var chart = ChartInfo;
        int firstX = Extensions.GetXByBar(chart, FirstVisibleBarNumber, false);
        int lastX = Extensions.GetXByBar(chart, LastVisibleBarNumber, false);
        int rightX = LastBarOnly ? lastX : Container.Region.Right;

        foreach (var kv in _levelsByPrice.OrderBy(k => k.Key))
        {
            decimal price = kv.Key;
            var merged = kv.Value;
            if (merged.Winner == null)
                continue;

            var winner = merged.Winner;
            var basePen = GetBasePen(winner.Cat);

            // Width tier by rank (null => thick; 1..3 => thick; 4..10 => medium; >10 => thin)
            int width;
            if (!winner.Rank.HasValue || winner.Rank.Value <= ThickMaxRank)
                width = ThickWidth;
            else if (winner.Rank.Value <= MediumMaxRank)
                width = MediumWidth;
            else
                width = ThinWidth;

            // Effective pen (same color, tiered width, dashed if 0DTE)
            var effPen = new PenSettings
            {
                Color = basePen.Color,
                Width = width,
                LineDashStyle = (winner.Is0DTE ? LineDashStyle.Dash : basePen.LineDashStyle)
            };

            int y = Extensions.GetYByPrice(chart, price, false);
            context.DrawLine(effPen.RenderObject, firstX, y, rightX, y);

            // Text
            string label = merged.LabelText;
            var size = context.MeasureString(label, _font);
            int textY = y - size.Height - OffsetY;
            int textX = RightAligned ? (rightX - size.Width - OffsetX) : (Container.Region.Left + OffsetX);

            context.DrawString(label, _font, effPen.RenderObject.Color, textX, textY);
        }
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