using ATAS.Indicators.Technical.Properties;
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

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("LevelsLolo")]
    [Display(ResourceType = typeof(CustomResources),
             Description = nameof(CustomResources.LevelsLolo_Description))]
    public class LevelsLolo : Indicator
    {
        #region Nested types

        private enum LabelCategory { Combo, LargeGamma, VolTrigger, CallWall, PutWall, ZeroGamma, Other }

        private sealed class LabelToken
        {
            public LabelCategory Cat;
            public string Raw = "";
            public int? Rank;
            public bool Is0DTE;
            public int EffectiveRank => Rank ?? 0;
        }

        private sealed class MergedLevel
        {
            public decimal Price;
            public List<LabelToken> Labels = new();
            public LabelToken Winner;
            public string LabelText = "";
        }

        #endregion

        #region Fields

        // Immutable snapshot (replaced wholesale on each parse)
        private volatile IReadOnlyList<MergedLevel> _snapshot = Array.Empty<MergedLevel>();
        private string _rawText = string.Empty;

        private PenSettings _penPutWall = new() { Color = CrossColor.FromArgb(255, 0, 255, 128), Width = 2 };
        private PenSettings _penCallWall = new() { Color = CrossColor.FromArgb(255, 255, 64, 64), Width = 2 };
        private PenSettings _penVolTrigger = new() { Color = CrossColor.FromArgb(255, 255, 215, 0), Width = 3 };
        private PenSettings _penLargeGamma = new() { Color = CrossColor.FromArgb(255, 255, 136, 0), Width = 2 };
        private PenSettings _penCombo = new() { Color = CrossColor.FromArgb(255, 255, 192, 77), Width = 1 };
        private PenSettings _penZeroGamma = new() { Color = CrossColor.FromArgb(255, 170, 170, 170), Width = 1 };
        private PenSettings _penOther = new() { Color = CrossColor.FromArgb(255, 160, 160, 160), Width = 1 };

        private RenderFont _font = new("Arial", 10);
        private int _fontSize = 10;

        private PenSettings _pen0DTEHalo = new()
        {
            Color = CrossColor.FromArgb(120, 0, 255, 255),   // cyan
            Width = 1
        };

        // Render pen cache.
        // Keyed by effective (argb, width, style). PenSettings instances are created on
        // demand and reused across frames with identical attributes. Orphaned entries
        // (after the user edits a Pen* property) are harmless: the key space stays
        // small in practice (< 64 distinct combinations).
        private readonly Dictionary<(uint argb, int width, LineDashStyle style), PenSettings> _penCache = new();

        #endregion

        #region Properties

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_RawText_DisplayName),
                 GroupName = nameof(CustomResources.Group_Data),
                 Description = nameof(CustomResources.LevelsLolo_RawText_Description),
                 Order = 1)]
        public string RawText
        {
            get => _rawText;
            set
            {
                _rawText = value ?? string.Empty;
                var parsed = Parse(_rawText);
                _snapshot = parsed.Values
                                  .Where(ml => ml.Winner != null)
                                  .OrderBy(ml => ml.Price)
                                  .ToArray();
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_ClearTextNow_DisplayName),
                 GroupName = nameof(CustomResources.Group_Data),
                 Order = 2)]
        public bool ClearTextNow
        {
            get => false;
            set { if (value) { RawText = string.Empty; RaisePropertyChanged(nameof(RawText)); } }
        }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_RightAligned_DisplayName),
                 GroupName = nameof(CustomResources.Group_Text),
                 Order = 1)]
        public bool RightAligned { get; set; } = true;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_LastBarOnly_DisplayName),
                 GroupName = nameof(CustomResources.Group_Text),
                 Description = nameof(CustomResources.LevelsLolo_LastBarOnly_Description),
                 Order = 2)]
        public bool LastBarOnly { get; set; } = true;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_OffsetX_DisplayName),
                 GroupName = nameof(CustomResources.Group_Text),
                 Order = 3)]
        [Range(0, 500)]
        public int OffsetX { get; set; } = 6;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_OffsetY_DisplayName),
                 GroupName = nameof(CustomResources.Group_Text),
                 Order = 4)]
        [Range(-500, 500)]
        public int OffsetY { get; set; } = 6;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_FontSize_DisplayName),
                 GroupName = nameof(CustomResources.Group_Text),
                 Order = 5)]
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

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_ThickMaxRank_DisplayName),
                 GroupName = nameof(CustomResources.Group_WidthTiers),
                 Order = 1)]
        [Range(1, 20)]
        public int ThickMaxRank { get; set; } = 3;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_MediumMaxRank_DisplayName),
                 GroupName = nameof(CustomResources.Group_WidthTiers),
                 Order = 2)]
        [Range(1, 50)]
        public int MediumMaxRank { get; set; } = 10;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_ThickWidth_DisplayName),
                 GroupName = nameof(CustomResources.Group_WidthTiers),
                 Order = 3)]
        [Range(1, 8)]
        public int ThickWidth { get; set; } = 3;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_MediumWidth_DisplayName),
                 GroupName = nameof(CustomResources.Group_WidthTiers),
                 Order = 4)]
        [Range(1, 8)]
        public int MediumWidth { get; set; } = 2;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_ThinWidth_DisplayName),
                 GroupName = nameof(CustomResources.Group_WidthTiers),
                 Order = 5)]
        [Range(1, 8)]
        public int ThinWidth { get; set; } = 1;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_ThickAlpha_DisplayName),
                 GroupName = nameof(CustomResources.Group_AlphaTiers),
                 Order = 1)]
        [Range(0, 255)]
        public int ThickAlpha { get; set; } = 255;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_MediumAlpha_DisplayName),
                 GroupName = nameof(CustomResources.Group_AlphaTiers),
                 Order = 2)]
        [Range(0, 255)]
        public int MediumAlpha { get; set; } = 210;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_ThinAlpha_DisplayName),
                 GroupName = nameof(CustomResources.Group_AlphaTiers),
                 Order = 3)]
        [Range(0, 255)]
        public int ThinAlpha { get; set; } = 160;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenCombo_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 1)]
        public PenSettings PenCombo { get => _penCombo; set => _penCombo = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenLargeGamma_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 2)]
        public PenSettings PenLargeGamma { get => _penLargeGamma; set => _penLargeGamma = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenVolTrigger_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 3)]
        public PenSettings PenVolTrigger { get => _penVolTrigger; set => _penVolTrigger = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenCallWall_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 4)]
        public PenSettings PenCallWall { get => _penCallWall; set => _penCallWall = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenPutWall_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 5)]
        public PenSettings PenPutWall { get => _penPutWall; set => _penPutWall = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenZeroGamma_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 6)]
        public PenSettings PenZeroGamma { get => _penZeroGamma; set => _penZeroGamma = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_PenOther_DisplayName),
                 GroupName = nameof(CustomResources.Group_Pens),
                 Order = 7)]
        public PenSettings PenOther { get => _penOther; set => _penOther = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_Enable0DTEHalo_DisplayName),
                 GroupName = nameof(CustomResources.Group_Accents),
                 Order = 1)]
        public bool Enable0DTEHalo { get; set; } = true;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_Pen0DTEHalo_DisplayName),
                 GroupName = nameof(CustomResources.Group_Accents),
                 Order = 2)]
        public PenSettings Pen0DTEHalo { get => _pen0DTEHalo; set => _pen0DTEHalo = value; }

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_HaloAlpha_DisplayName),
                 GroupName = nameof(CustomResources.Group_Accents),
                 Order = 3)]
        [Range(0, 255)]
        public int HaloAlpha { get; set; } = 120;

        [Display(ResourceType = typeof(CustomResources),
                 Name = nameof(CustomResources.LevelsLolo_HaloExtraWidth_DisplayName),
                 GroupName = nameof(CustomResources.Group_Accents),
                 Order = 4)]
        [Range(0, 10)]
        public int HaloExtraWidth { get; set; } = 2;

        #endregion

        #region Constructor

        public LevelsLolo() : base(false)
        {
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            DrawAbovePrice = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            DataSeries[0].IsHidden = true;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
        }

        #endregion

        #region Overrides

        protected override void OnCalculate(int bar, decimal value) { }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            var snapshot = _snapshot;
            if (snapshot.Count == 0) return;

            var region = Container.Region;
            int firstX = ChartInfo.GetXByBar(FirstVisibleBarNumber, false);
            int rightX = LastBarOnly
                ? ChartInfo.GetXByBar(LastVisibleBarNumber, false)
                : region.Right;

            foreach (var ml in snapshot)
            {
                int y = ChartInfo.GetYByPrice(ml.Price, false);
                if (y < region.Top || y > region.Bottom) continue;

                var basePen = GetBasePen(ml.Winner.Cat);
                int tier = GetWidthTier(ml.Winner);
                int width = tier switch { 0 => ThickWidth, 1 => MediumWidth, _ => ThinWidth };
                byte alpha = tier switch { 0 => (byte)ThickAlpha, 1 => (byte)MediumAlpha, _ => (byte)ThinAlpha };
                if (ml.Winner.Cat == LabelCategory.ZeroGamma) { width = ThinWidth; alpha = (byte)ThinAlpha; }

                var c = basePen.Color;
                var effColor = CrossColor.FromArgb(alpha, c.R, c.G, c.B);
                var dashStyle = ml.Winner.Is0DTE ? LineDashStyle.Dash : basePen.LineDashStyle;
                var effPen = GetCachedPen(effColor, width, dashStyle);

                if (ml.Winner.Is0DTE)
                {
                    if (Enable0DTEHalo)
                    {
                        var haloC = _pen0DTEHalo.Color;
                        var haloColor = CrossColor.FromArgb((byte)Math.Clamp(HaloAlpha, 0, 255), haloC.R, haloC.G, haloC.B);
                        var haloWidth = Math.Max(1, width + HaloExtraWidth);
                        var halo = GetCachedPen(haloColor, haloWidth, LineDashStyle.Solid);
                        context.DrawLine(halo.RenderObject, firstX, y, rightX, y);
                    }
                }

                context.DrawLine(effPen.RenderObject, firstX, y, rightX, y);

                bool highPriority = ml.Winner.Is0DTE
                    && (ml.Winner.Cat is LabelCategory.LargeGamma or LabelCategory.PutWall or LabelCategory.CallWall)
                    && ml.Winner.EffectiveRank <= ThickMaxRank;
                if (highPriority)
                {
                    var accent = GetCachedPen(HighPriorityAccentColor, 1, LineDashStyle.Dot);
                    context.DrawLine(accent.RenderObject, firstX, y, rightX, y);
                }

                var size = context.MeasureString(ml.LabelText, _font);
                int textX = RightAligned ? rightX - size.Width - OffsetX : firstX + OffsetX;
                int textY = y - size.Height - OffsetY;
                if (textY < region.Top) textY = y + OffsetY;
                if (textY + size.Height > region.Bottom) textY = region.Bottom - size.Height;
                if (textY < region.Top) textY = region.Top;
                context.DrawString(ml.LabelText, _font, effPen.RenderObject.Color, textX, textY);
            }
        }

        protected override void OnDispose()
        {
            _penCache.Clear();
            base.OnDispose();
        }

        #endregion

        #region Static helpers

        private static int CategoryPriority(LabelCategory c) => c switch
        {
            LabelCategory.VolTrigger => 0,
            LabelCategory.LargeGamma => 1,
            LabelCategory.CallWall or LabelCategory.PutWall => 2,
            LabelCategory.Combo => 3,
            LabelCategory.ZeroGamma => 4,
            _ => 5,
        };

        private static string FormatLabel(LabelToken l)
        {
            string baseName = l.Cat switch
            {
                LabelCategory.Combo => "CO",
                LabelCategory.LargeGamma => "LG",
                LabelCategory.VolTrigger => "VT",
                LabelCategory.CallWall => "CW",
                LabelCategory.PutWall => "PW",
                LabelCategory.ZeroGamma => "ZG",
                _ => l.Raw.ToUpperInvariant()
            };

            // ZG is informational; keep concise tag
            string rankTxt = l.Rank.HasValue ? l.Rank.Value.ToString() : "";
            string suffix = l.Is0DTE ? " 0DTE" : "";

            return (baseName + rankTxt + suffix).Trim();
        }

        private static readonly Regex NumberOnly = new(
            @"^\s*\d+(\.\d+)?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex LabelPattern = new(
            @"^\s*(?<name>[A-Za-z ]+?)(?<rank>\d{1,3})?(?<suffix>\s+0DTE)?\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly CrossColor HighPriorityAccentColor = CrossColor.FromArgb(220, 255, 64, 64);

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

        private static LabelCategory MapCategory(string name)
        {
            var u = (name ?? string.Empty).ToUpperInvariant().Replace("  ", " ").Trim();

            if (u == "CO" || u == "COMBO") return LabelCategory.Combo;
            if (u == "LG" || u == "LARGE GAMMA" || u == "LARGEGAMMA") return LabelCategory.LargeGamma;
            if (u == "VT" || u == "VOLATILITY TRIGGER" || u == "VOLATILITYTRIGGER") return LabelCategory.VolTrigger;
            if (u == "CW" || u == "CALL WALL" || u == "CALLWALL") return LabelCategory.CallWall;
            if (u == "PW" || u == "PUT WALL" || u == "PUTWALL") return LabelCategory.PutWall;
            if (u == "ZG" || u == "ZERO GAMMA" || u == "ZEROGAMMA") return LabelCategory.ZeroGamma;

            return LabelCategory.Other;
        }

        private static bool TryParseDecimal(string s, out decimal value)
        {
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;
            return false;
        }

        private static IDictionary<decimal, MergedLevel> Parse(string raw)
        {
            var result = new Dictionary<decimal, MergedLevel>();

            var text = NormalizeInvisible(raw);
            if (string.IsNullOrWhiteSpace(text))
                return result;

            // Optional prefix like "$SP:"
            int colon = text.IndexOf(':');
            if (colon >= 0 && colon + 1 < text.Length)
                text = text.Substring(colon + 1);

            var tokens = text.Split(',');
            var cleaned = new List<string>(tokens.Length);
            foreach (var t0 in tokens)
            {
                var t = NormalizeInvisible(t0);
                if (!string.IsNullOrEmpty(t))
                    cleaned.Add(t);
            }
            if (cleaned.Count == 0)
                return result;

            var pending = new List<LabelToken>();

            foreach (var token in cleaned)
            {
                // '&' = multiple labels sharing a single price
                var parts = token.Split('&');
                bool isSingleNumber = (parts.Length == 1) && NumberOnly.IsMatch(parts[0]);

                if (isSingleNumber)
                {
                    if (pending.Count == 0)
                        continue;

                    if (!TryParseDecimal(parts[0], out var price))
                        continue;

                    if (!result.TryGetValue(price, out var merged))
                    {
                        merged = new MergedLevel { Price = price };
                        result[price] = merged;
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

                        // Normalize "Zero Gamma" to "ZG" to keep the merged label compact
                        if (string.Equals(rawName, "Zero Gamma", StringComparison.OrdinalIgnoreCase))
                            rawName = "ZG";

                        int? rank = null;
                        if (m.Groups["rank"].Success
                            && int.TryParse(m.Groups["rank"].Value, NumberStyles.Integer,
                                            CultureInfo.InvariantCulture, out var r))
                            rank = r; // lower is stronger

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

            // Winner resolution and label text per price
            foreach (var ml in result.Values)
            {
                if (ml.Labels.Count == 0) continue;

                var winner = ml.Labels[0];
                for (int i = 1; i < ml.Labels.Count; i++)
                {
                    var cand = ml.Labels[i];
                    if (cand.EffectiveRank < winner.EffectiveRank) { winner = cand; continue; }
                    if (cand.EffectiveRank > winner.EffectiveRank) continue;
                    if (CategoryPriority(cand.Cat) < CategoryPriority(winner.Cat)) winner = cand;
                }
                ml.Winner = winner;
                ml.LabelText = string.Join(" & ", ml.Labels.Select(FormatLabel));
            }

            return result;
        }

        #endregion

        #region Private methods

        private PenSettings GetBasePen(LabelCategory c) => c switch
        {
            LabelCategory.Combo => _penCombo,
            LabelCategory.LargeGamma => _penLargeGamma,
            LabelCategory.VolTrigger => _penVolTrigger,
            LabelCategory.CallWall => _penCallWall,
            LabelCategory.PutWall => _penPutWall,
            LabelCategory.ZeroGamma => _penZeroGamma,
            _ => _penOther
        };

        private int GetWidthTier(LabelToken w) =>
            w.EffectiveRank <= ThickMaxRank ? 0 :
            w.EffectiveRank <= MediumMaxRank ? 1 : 2;

        private PenSettings GetCachedPen(CrossColor color, int width, LineDashStyle style)
        {
            uint argb = ((uint)color.A << 24)
                      | ((uint)color.R << 16)
                      | ((uint)color.G << 8)
                      | (uint)color.B;

            var key = (argb, width, style);
            if (!_penCache.TryGetValue(key, out var pen))
            {
                pen = new PenSettings { Color = color, Width = width, LineDashStyle = style };
                _penCache[key] = pen;
            }
            return pen;
        }

        #endregion
    }
}