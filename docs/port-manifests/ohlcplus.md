# Port manifest — OHLCPlus

**Source:** `prready/main`
**Integration target:** `local/ohlcplus-i18n` (stacked on `local/build/04-localization`)
**Status:** `not-started`

---

## Pre-port evaluation

### SE perspective

- 1477-line indicator on Develop; 4341 lines on prready/main — significant net additions (~2900 lines).
- All new work falls into four orthogonal but sequentially-dependent feature areas:
  1. **Label system** — template-driven level labels with per-level overrides and collision-aware layout
  2. **Visual Semantic Schemes** — pq02 ruleset engine (by period / by level type), palette and per-level/per-line overrides
  3. **HVN Bands** — High Volume Node overlay computed from fixed profiles
  4. **LVN Bands** — Low Volume Node overlay sharing the same band model
- 43 feat/fix commits form a single tightly-coupled stack (each area builds on prior ones).
- 3 refactor commits (`8447b822`, `14c68410`, `81a5e616`) are standalone (no behavior change) — candidate for `refactor/ohlcplus-period-helpers`.
- `635a5706` (Feb 7) is a one-line fix restoring `Color.Convert()` lost in a rebase conflict — belongs at the end of the feat stack.
- **typeof(Resources) pattern:** `6e502c67` uses `#if STABLE / typeof(Resources) / #else / typeof(Strings) / #endif` guards — **simplify**: strip `#if STABLE` blocks, keep only the `#else` content (`typeof(Strings)`) uniformly. `a12abc02` only modifies the `#if STABLE` branch of `6e502c67` — **skip** entirely (no-op once STABLE blocks are removed). Ten other commits (`4ece1376`, `9edf68b3`, `9d4652fa`, `68d773d0`, `079b863d`, `2bed76ee`, `69fcdc69`, `aa7bbc18`, `30850ada`, `a9d7e36e`) use `typeof(Resources)` directly — **adaptation required** on the Develop-based feat branch.
- **`ToggleLevelsVisibilityHotKey` `#if ATAS_ALPHA || ATAS_X` guard** — present in `c5ca65ed`. This is a build-flavor concern handled at the `local/build/` layer, not in `feat/ohlcplus` or `local/ohlcplus-i18n`.
- **Three upstream bugs found** during review — to be fixed as additional commits in `feat/ohlcplus` (see Phase 2f).

### Discretionary trader / algo trader perspective

- Semantic schemes add visual clarity by coloring levels according to their market context (period + level type).
- HVN/LVN overlays immediately surface high-activity and low-activity clusters across fixed profiles.
- The label template system with prefixes allows fast at-a-glance reads of which period a level belongs to.
- Together these add meaningful depth to profile-based analysis directly on the chart.

### Intentional additions/deviations

Three bug fixes added locally (not in prready/main) — see Phase 2f.

---

## Skip (docs-only, no code impact)

None identified.

---

## Phase -1 — Standalone branches (Develop-based)

**Rules:**
- Base: `git checkout -B <branch> Develop`
- No `typeof(Resources)` / no `using ATAS.Indicators.Technical.Properties`.
- **Build note:** MC3074 XAML errors are environmental — verify C# logic by review.

### Refactor branch

| Branch | prready/main commits | Status |
|--------|---------------------|--------|
| `refactor/ohlcplus-period-helpers` | `8447b822`, `14c68410`, `81a5e616` | pending |

Three sequential "no behavior change" refactors (Dec 30):
- `8447b822` — centralize level enumeration by period (adds helper method)
- `14c68410` — compute period needs via generic helper (replaces repeated loops)
- `81a5e616` — streamline profile requests loop (minor cleanup)

These apply cleanly to Develop's OHLCPlus.cs and have no impact on platform keys or resources.

### Fix branches from prready/main

None (all prready/main fixes are coupled to features not present in Develop).

---

## Phase 0 — Resource keys required in `local/build/04-localization`

**60 net new keys** across 13 commits (all 7 locales):

| Commit | Keys (+/-) | Files | Notes | Status |
|--------|-----------|-------|-------|--------|
| `c5ca65ed` | +13 | All 7 locales + Designer.cs + csproj | Core display keys: ToggleLevelsVisibilityHotKey, OHLCPlusDescription, TillBar, FullWidth, BarOpen-Close, Equilibrium, POC, VWAP, VAH, VAL | pending |
| `05f12175` | +4 | All 7 locales | Labels, LabelTemplate, Prefixes, Contract | pending |
| `004e89e9` | +1 | All 7 locales | OverrideLabel | pending |
| `2176efbd` | +2 | All 7 locales | OverrideColorInSchemes, OverrideColorInSchemesDescription | pending |
| `8000c260` | +5 | All 7 locales | ByPeriod, ByLevelType, VisualSemantic, VisualSemanticMode, VisualSemanticPreset | pending |
| `8ce33b3e` | +4 | All 7 locales | OverrideWidthInSchemes/Description, OverrideStyleInSchemes/Description | pending |
| `ef458870` | +6 | All 7 locales | VisualSemantic_LevelPalette, VisualSemantic_PeriodPalette, VisualMode_Legacy/Ruleset + descriptions | pending |
| `045d2b72` | +9 | All 7 locales | HVN_Group, HVN_Enable, HVN_Color, HVN_ThresholdPct, HVN_GapToleranceTicks, HVN_OcclusionTicks + 3 descriptions | pending |
| `8ce614b9` | +8 | All 7 locales | HVN_Enabled, LVN_Group, LVN_ThresholdPct/GapToleranceTicks/OcclusionTicks + descriptions | pending |
| `10cd6ce9` | +2 | All 7 locales | LVN_Enabled, LVN_Color | pending |
| `435cc30a` | +4 | All 7 locales | LVN_MinPocVol/Description, LVN_TailFilterMinTicks/Description | pending |
| `7997d0fd` | +2 | All 7 locales | LVN_TailFilterRangePct, LVN_TailFilterRangePct_Description | pending |
| `0047162c` | 0 new (value fixes) | de-de, es-ES, fr-fr, hi-in | Fix LVN label values in satellite locales only | pending |

### New keys (60 total)

**Core display (13):** `ToggleLevelsVisibilityHotKey`, `OHLCPlusDescription`, `TillBar`, `FullWidth`,
`BarOpen`, `BarHigh`, `BarLow`, `BarClose`, `Equilibrium`, `POC`, `VWAP`, `VAH`, `VAL`

**Label system (5):** `Labels`, `LabelTemplate`, `Prefixes`, `Contract`, `OverrideLabel`

**Visual semantic scheme (19):** `OverrideColorInSchemes`, `OverrideColorInSchemesDescription`,
`ByPeriod`, `ByLevelType`, `VisualSemantic`, `VisualSemanticMode`, `VisualSemanticPreset`,
`OverrideWidthInSchemes`, `OverrideWidthInSchemesDescription`,
`OverrideStyleInSchemes`, `OverrideStyleInSchemesDescription`,
`VisualSemantic_LevelPalette`, `VisualSemantic_PeriodPalette`,
`VisualMode_Legacy`, `VisualMode_Ruleset`,
`VisualMode_Legacy_Description`, `VisualMode_Ruleset_Description`,
`OverrideColorInSchemes`, `OverrideColorInSchemesDescription`

**HVN bands (10):** `HVN_Group`, `HVN_Enable`, `HVN_Color`, `HVN_Enabled`,
`HVN_ThresholdPct`, `HVN_GapToleranceTicks`, `HVN_OcclusionTicks`,
`HVN_ThresholdPct_Description`, `HVN_GapToleranceTicks_Description`, `HVN_OcclusionTicks_Description`

**LVN bands (13):** `LVN_Group`, `LVN_Enabled`, `LVN_Color`,
`LVN_ThresholdPct`, `LVN_ThresholdPct_Description`,
`LVN_GapToleranceTicks`, `LVN_GapToleranceTicks_Description`,
`LVN_OcclusionTicks`, `LVN_OcclusionTicks_Description`,
`LVN_MinPocVol`, `LVN_MinPocVol_Description`,
`LVN_TailFilterMinTicks`, `LVN_TailFilterMinTicks_Description`,
`LVN_TailFilterRangePct`, `LVN_TailFilterRangePct_Description`

Note: `ImbalanceRatio` key already exists in Designer.cs (ClusterSearch). All other keys are new.

---

## Phase 2 — Feat branch

All 40 commits (excluding the 3 period-helper refactors) form a single tightly-coupled stack: `feat/ohlcplus`.

The optional 3 period-helper refactors (`8447b822`, `14c68410`, `81a5e616`) may be either cherry-picked first from `refactor/ohlcplus-period-helpers` or included directly in `feat/ohlcplus`.

### Phase 2a — Label system (11 commits, Dec 30–Jan 2)

| Commit (prready/main) | Description | Adaptation | Status |
|----------------------|-------------|-----------|--------|
| `45e21dfa` | Scaffold label template and parameters (no behavior change) | None | pending |
| `5c74a65f` | Render labels using template and per-level overrides | None | pending |
| `247c1247` | Resolve label text via split-key suffix mapping | None | pending |
| `9acbbbd5` | Add configurable period prefixes for labels | None | pending |
| `9ffd20fc` | Normalize label template tokens to lowercase | None | pending |
| `d22a882c` | Batched, collision-aware label layout | None | pending |
| `ad20febc` | Improve label placement with horizontal corridor + stable ordering | None | pending |
| `b5ccac8a` | Correct line clipping bounds and trim bar line start | None | pending |
| `9d4652fa` | Add label and prefix settings UI | **Adapt: 17 typeof(Resources) → hardcode/typeof(Strings)** | pending |
| `2bed76ee` | Add per-level label override | **Adapt: 1 typeof(Resources) → hardcode** | pending |
| `1a21cb4c` | Fix: per-level OverrideLabel replaces full label | None | pending |

### Phase 2b — Visual Semantic Schemes (12 commits, Jan 1–3)

| Commit (prready/main) | Description | Adaptation | Status |
|----------------------|-------------|-----------|--------|
| `b425f198` | Scaffold pq02 visual semantic descriptors | None | pending |
| `9664a58f` | Fix: avoid redundant redraws when levels unchanged | None | pending |
| ~~`a12abc02`~~ | ~~Fix: align UI resources for stable vs alpha~~  | **Skip — only edits #if STABLE blocks which are removed in 6e502c67 adaptation** | skip |
| `c2f18e18` | Introduce explicit semantic label priority | None | pending |
| `6e502c67` | Route Display attributes to Resources under STABLE | **Simplify: strip all `#if STABLE / #else / #endif` blocks, keep `#else` content (`typeof(Strings)`) as the unconditional form** | pending |
| `545a78a7` | Add pq02 visual semantic UI and palettes | None | pending |
| `981e13c0` | Implement pq02 visual semantic rulesets (by period / by level type) | None | pending |
| `eb51f389` | Apply pq02 visual semantic styles during rendering | None | pending |
| `4ece1376` | Localize visual semantic preset and mode displays | **Adapt: 4 typeof(Resources) → hardcode** | pending |
| `9edf68b3` | Localize visual semantic mode and palettes strings | **Adapt: 18 typeof(Resources) → hardcode** | pending |
| `aa7bbc18` | Allow per-line width and style overrides in scheme modes | **Adapt: 2 typeof(Resources) → hardcode** | pending |
| `69fcdc69` | Allow per-level color override when semantic schemes active | **Adapt: 1 typeof(Resources) → hardcode** | pending |

### Phase 2c — HVN Bands (3 commits, Jan 3)

| Commit (prready/main) | Description | Adaptation | Status |
|----------------------|-------------|-----------|--------|
| `68d773d0` | Compute HVN bands from fixed profiles (overlay plumbing) | **Adapt: 17 typeof(Resources) → hardcode** | pending |
| `15b2ca42` | Render HVN bands with priority and occlusion | None | pending |
| `9709fc85` | Fix: skip HVN recomputation when disabled; convert WPF → System.Drawing colors | None | pending |

### Phase 2d — LVN Bands + Unification (13 commits, Jan 4–5)

| Commit (prready/main) | Description | Adaptation | Status |
|----------------------|-------------|-----------|--------|
| `75b6b904` | Refactor: unify profile band model for HVN/LVN overlays | None | pending |
| `079b863d` | Add LVN bands settings and cache scaffolding | **Adapt: 24 typeof(Resources) → hardcode** | pending |
| `9d85b2a8` | Compute LVN bands from fixed profiles | None | pending |
| `a9d7e36e` | Add HVN/LVN settings surface (toggles/colors/params) | **Adapt: 36 typeof(Resources) → hardcode** | pending |
| `1f3ae47b` | Compute HVN/LVN bands (cache + refresh triggers) | None | pending |
| `3f776004` | Render HVN/LVN overlays with occlusion | None | pending |
| `3dd8f336` | Fix: emphasize VN borders | None | pending |
| `36a83021` | Fix: stabilize HVN/LVN band updates and change detection | None | pending |
| `30850ada` | Add hybrid LVN tail filter (min ticks + % of range) | **Adapt: 1 typeof(Resources) → hardcode** | pending |
| `1aba0c0b` | Perf: reuse ordered profile levels for HVN and LVN | None | pending |
| `886cc0df` | Perf: remove dynamic and cache typed price levels for HVN/LVN | None | pending |
| `b6801850` | Refactor: make fixed-profile request helper side-effect free | None | pending |
| `da472879` | Chore: remove outdated comment about unused HVN parameters | None | pending |

### Phase 2e — Bug fix (1 commit, Feb 7)

| Commit (prready/main) | Description | Adaptation | Status |
|----------------------|-------------|-----------|--------|
| `635a5706` | Fix: restore Color.Convert() lost during rebase conflict resolution | None | pending |

### Phase 2f — Local fixes (not in prready/main)

Three bugs found during pre-port review. These are new commits authored locally in `feat/ohlcplus`.

#### Fix 1 — Stale ordered-levels cache in `OnFixedProfilesResponse`

**Impact:** Medium. When a fixed profile is refreshed mid-session (new data arrives for a period already in `_profileCandles`), `UpdateHVNs`/`UpdateLVNs` call `GetOrderedLevels(period, newCandle)`, which returns the sorted price-level list cached by the previous `UpdateAllNeededLevelsFromCache()` call — computed against the old candle. HVN/LVN bands are then computed against stale data until the next cache clear.

**Fix:** Add `_orderedLevelsCache.Remove(period);` at the top of `OnFixedProfilesResponse`, before `_profileCandles[period] = fixedProfileOriginScale`.

#### Fix 2 — Label and prefix property setters don't redraw

**Impact:** Medium / UX. All 16 label-text and period-prefix properties are bare pass-throughs (`set => _field = value`). Changing any of them in the settings panel has no visible effect until the next natural redraw (new tick, chart resize). Each setter must call `RedrawChart()`.

**Affected properties (16):** `LabelTemplate`, `OpenLabel`, `HighLabel`, `LowLabel`, `CloseLabel`, `EquilibriumLabel`, `PocLabel`, `VwapLabel`, `VahLabel`, `ValLabel`, `DayPrefix`, `PrevDayPrefix`, `WeekPrefix`, `PrevWeekPrefix`, `MonthPrefix`, `PrevMonthPrefix`, `ContractPrefix`.

#### Fix 3 — Palette color changes don't invalidate the Visual Semantic ruleset

**Impact:** High / correctness. All 16 palette-color properties (`PeriodColorCurrentDay`, …, `LevelColorVAL`) are auto-properties. When the user changes a palette color while in RuleSet mode, two things are missing: `_visualRuleSetDirty = true` (the cached ruleset was built with the old color and won't rebuild) and `RedrawChart()` (the chart doesn't repaint at all). **Changing palette colors in RuleSet mode is completely silent.**

**Fix:** Back each palette property with a private field; setter sets `_visualRuleSetDirty = true` and calls `RedrawChart()`.

### Adaptation summary

Commits requiring `typeof(Resources)` → hardcode/`typeof(Strings)` adaptation on the Develop-based feat branch:

| Commit | Refs | Area |
|--------|------|------|
| `9d4652fa` | 17 | Labels UI |
| `2bed76ee` | 1 | Per-level label override |
| `4ece1376` | 4 | Semantic preset/mode display |
| `9edf68b3` | 18 | Semantic mode + palettes |
| `aa7bbc18` | 2 | Per-line width/style overrides |
| `69fcdc69` | 1 | Per-level color override |
| `68d773d0` | 17 | HVN bands plumbing |
| `079b863d` | 24 | LVN bands scaffold |
| `a9d7e36e` | 36 | HVN/LVN settings surface |
| `30850ada` | 1 | LVN tail filter |

Total adapted refs: **121**

`6e502c67` — strip `#if STABLE / #else / #endif` wrappers; the `#else` body (`typeof(Strings)`) becomes the unconditional form. Net change: removes ~60 lines of preprocessor noise, no logic change.
`a12abc02` — **skip** (its only changes are inside `#if STABLE` blocks that no longer exist).

### Strategy for Develop-based `feat/ohlcplus`

- All `typeof(Resources)` references in adapted commits: replace with hardcoded English strings for new OHLCPlus-specific keys; use `typeof(Strings)` for keys already in the platform `Strings` class.
- Do NOT add `using ATAS.Indicators.Technical.Properties`.
- `ToggleLevelsVisibilityHotKey` `#if ATAS_ALPHA || ATAS_X` guard: this is a build-flavor concern owned by the `local/build/` stack, not by `feat/ohlcplus`. On `feat/ohlcplus` use `typeof(Resources)` (the `#else` branch) unconditionally — the guard is restored by the integration branch layer if needed.

### Strategy for `local/ohlcplus-i18n` (integration)

- Cherry-pick from `feat/ohlcplus` and additionally re-apply the adapted commits using `typeof(Resources)` instead of hardcoded strings.
- Alternatively: cherry-pick directly from prready/main SHAs (preserves typeof(Resources) without inverse adaptation).

---

## Phase 3 — Chore / UI polish

No separate phase — all polish commits are included in the feat stack above.

---

## Phase 4 — Integration verification: `local/ohlcplus-i18n`

### 4.1 Build
- [ ] `dotnet build` — 0 C# errors

### 4.2 Content completeness
- [ ] refactor/ohlcplus-period-helpers (or inline): 3 commits applied
- [ ] feat/ohlcplus: 40 commits applied, HEAD TBD

### 4.3 Resource completeness
- [ ] 60 new keys in Resources.resx (en)
- [ ] 60 keys in all 6 satellite locales
- [ ] Resources.Designer.cs up to date

### 4.4 Functional smoke test (manual, ATAS Platform)
- [ ] Indicator loads without crash
- [ ] Level labels render using template (e.g., period prefix + level name)
- [ ] Changing a label text or period prefix immediately redraws (Fix 2)
- [ ] Per-level label override replaces the full label
- [ ] Visual Semantic Mode selector visible and switches rendering
- [ ] pq02 rulesets (ByPeriod / ByLevelType) color levels correctly
- [ ] Changing a palette color in RuleSet mode immediately redraws with new color (Fix 3)
- [ ] HVN bands overlay appears when enabled
- [ ] LVN bands overlay appears when enabled
- [ ] Hybrid LVN tail filter (min ticks + % of range) gates LVN detection
- [ ] HVN/LVN occlusion and priority ordering work correctly
- [ ] Collision-aware label layout prevents overlap on dense charts

---

## Section 5 — Intentional divergences from prready/main

| Area | Divergence | Reason |
|------|-----------|--------|
| `6e502c67` | `#if STABLE` blocks removed; `#else` (`typeof(Strings)`) kept unconditionally | STABLE builds not targeted; preprocessor noise eliminated |
| `a12abc02` | Skipped entirely | No-op once STABLE blocks are gone |
| Fix 1 | `_orderedLevelsCache.Remove(period)` added to `OnFixedProfilesResponse` | Bug fix not in prready/main |
| Fix 2 | `RedrawChart()` added to 16 label/prefix setters | Bug fix not in prready/main |
| Fix 3 | Palette properties converted from auto to backed-field with `_visualRuleSetDirty = true; RedrawChart()` | Bug fix not in prready/main |

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

- Port not yet started.
