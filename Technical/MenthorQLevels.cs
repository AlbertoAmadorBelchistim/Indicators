using ATAS.Indicators;
using OFT.Rendering.Context;
using System;
using System.ComponentModel;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("MenthorQLevels")]
    public sealed class MenthorQLevels : Indicator
    {
        #region Nested types: model

        internal enum LevelCategory
        {
            // Dense enum (no gaps) so it can be used as an index into pen caches
            // later. `Other` stays at 100 as a fallback bucket — renderer guards
            // bounds when it sees it.
            CallResistance = 0,   // CR
            PutSupport = 1,   // PS
            HighVolatilityLevel = 2,   // HVL
            GammaExposure = 3,   // GEX (ranked: rank 1 strongest, higher = weaker)
            GammaWall = 4,   // GW  — pivot / magnet (max total gamma, single per context)
            BlindSpot = 5,   // BL (ranked)
            DayMin = 6,   // 1D Min
            DayMax = 7,   // 1D Max
            RiskTrigger = 8,   // RT MM-DD
            LowerBand = 9,   // LB MM-DD
            UpperBand = 10,  // UB MM-DD
            Swing = 11,  // generic swing levels not matching UB/LB/RT

            Other = 100
        }

        internal readonly struct LevelLabel
        {
            public readonly LevelCategory Category;

            // Rank is 0 for flagship levels (CallWall, PutWall, HVL, Gamma Wall,
            // 1D Min/Max). For numbered families like GEX N / BL N, a lower
            // number means a stronger level (rank 1 is the strongest). Used by
            // the renderer to map to tiers (Thick/Medium/Thin).
            public readonly int Rank;

            public readonly bool Is0Dte;

            // Raw text exactly as it came from the source (API response or paste).
            // Kept for logging and debug overlay.
            public readonly string RawLabel;

            // Canonical display text the renderer draws on chart
            // (e.g., "CR", "PS", "GEX 3", "BL 4", "UB 01-13", "GW").
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

            // One entry may carry several labels when the same price is tagged
            // by more than one concept within the same source (rare, but e.g.
            // "Call Resistance 0DTE / Gamma Wall 0DTE" at the same price).
            public readonly LevelLabel[] Labels;

            // Stable identifier used for logging and debug (e.g. "MenthorQ:Api",
            // "MenthorQ:Text:Index", "MenthorQ:Text:Futures").
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

            // All labels that resolved to this price after dedup / conflict
            // resolution within the active source.
            public readonly LevelLabel[] Labels;

            // The label chosen to drive the visual style (category / tier / 0DTE).
            public readonly LevelLabel Winner;

            // Pre-built display text (e.g. "CW / LG2"). Computed once at build
            // time so OnRender stays allocation-free.
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

        #region Ctor

        public MenthorQLevels()
            : base(useCandles: true)
        {
            // Custom drawing is enabled from the first commit so that, as
            // later commits add the renderer, there's no behavioural diff
            // coming from the indicator constructor itself.
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);
        }

        #endregion

        #region Overrides

        protected override void OnCalculate(int bar, decimal value)
        {
            // Shell: no calculations yet.
            // - Source collection (API + MenthorQText paste) is introduced in commits 3-6.
            // - Engine (conflict resolution, ConceptKey dedup) in commits 7-8.
            // - Alerts in commit 12.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Shell: no drawing yet.
            // Modern tier + 0DTE halo pipeline is ported from GammaLevels in commits 9-10.
        }

        #endregion
    }
}
