using OFT.Rendering.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("LevelsLolo")]
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

        protected override void OnRender(RenderContext context, DrawingLayouts layout) { }

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

            return result;
        }

        #endregion
    }
}
