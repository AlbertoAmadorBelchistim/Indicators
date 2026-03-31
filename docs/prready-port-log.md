# prready/main port log

Tracks every commit in `prready/main` that is not in `Develop`, grouped by indicator/area.
Each entry records what was done and where, or why it was skipped.

**Last updated:** 2026-03-31
**Scope:** `git log --oneline Develop..prready/main -- Technical/ docs/`

---

## Status legend

| Symbol | Meaning |
|--------|---------|
| ✅ | Ported — commit content present in local branch or layer |
| ⚠️ | Ported partially — branch exists, integration incomplete |
| ⏭️ | Skipped intentionally — reason documented |
| 🔲 | Pending — not yet evaluated or ported |
| 📋 | Pending evaluation — docs/ADR commit, needs review before porting |

---

## Summary

| Indicator / Area | Total commits | ✅ Done | ⚠️ Partial | ⏭️ Skipped | 🔲 Pending |
|------------------|--------------|--------|-----------|-----------|-----------|
| Delta | 24 | 21 | 1 | 2 | 0 |
| Resources (Delta-specific) | 8 | 8 | 0 | 0 | 0 |
| OHLCPlus | 44 | 0 | 0 | 0 | 44 |
| ClusterStatistic | 37 | 0 | 0 | 0 | 37 |
| MultiMarketPower | 22 | 0 | 0 | 0 | 22 |
| DailyLines | 21 | 0 | 0 | 0 | 21 |
| Volume | 6 | 0 | 0 | 0 | 6 |
| TradesOnChart | 3 | 0 | 0 | 0 | 3 |
| ClusterSearch | 6 | 0 | 0 | 0 | 6 |
| InitialBalance | 10 | 0 | 0 | 0 | 10 |
| AccountInfoDisplay | 14 | 0 | 0 | 0 | 14 |
| GammaLevels | 30 | 0 | 0 | 0 | 30 |
| DOM / DOMStrength | 2 | 0 | 0 | 0 | 2 |
| Compat / Build | 2 | 2 | 0 | 0 | 0 |
| Docs / ADRs | 10 | 0 | 0 | 0 | 10 |
| **Total** | **239** | **31** | **1** | **2** | **205** |

---

## Delta — Technical/Delta.cs

All 24 code commits in prready/main that are not in Develop. Ports live under `local/delta-i18n`
(stacked on `local/build/04-localization`). See also `docs/port-manifests/delta.md` for full detail.

| Hash | Description | Status | Local branch / commit |
|------|-------------|--------|-----------------------|
| `19fc5e7a` | feat: add fixed threshold line series (up/down major/minor) | ✅ | `feat/delta-fixed-thresholds` |
| `f60173c2` | feat: add price-panel triangle signals (UI + rendering) | ✅ | `feat/delta-price-signals` |
| `e403bc22` | feat: add threshold level selection for visual signals (per side) | ✅ | `feat/delta-threshold-selection` |
| `e2890e31` | feat: add session-anchored dynamic signed thresholds (no look-ahead) | ⚠️ | `feat/delta-dynamic-thresholds` — infra ported, OnCalculate integration was missing; fixed 2026-03-31 |
| `a539ce4b` | feat: add audio alerts with threshold level and bar-close policy | ✅ | `feat/delta-audio-alerts` |
| `72af4410` | feat: add average delta line with SMA/EMA and color modes | ✅ | `feat/delta-average-line` |
| `29f60e6d` | refactor: extract OnCalculate helpers and unify state reset (no behavior change) | ⏭️ | High integration cost, zero external value; deferred indefinitely |
| `64ed7f58` | fix: clamp price-panel triangle markers to visible region | ✅ | `fix/delta-triangle-clamp` |
| `3e420e5c` | chore: hide internal series from Drawing group and enforce single UI source | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `811000d6` | chore: reorganize UI groups and ordering (no behavior change) | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `5fe49e01` | chore: use LineWidth for average thickness and changed UI order | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `5f78e83f` | chore: assign specific divergence descriptions to UI controls and fix localization errors | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `5e336b28` | chore: use delta-specific labels for fixed thresholds | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `eff7ee3f` | chore: hide and disable legacy fixed-value alerts | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `0d087ebd` | chore: use delta-specific color labels in drawing settings | ✅ | `local/delta-i18n` commit `6c63cac1` |
| `9da4aa2a` | fix: add Major/Minor threshold level labels | ✅ | `local/delta-i18n` commit `29a9b0d7` |
| `1ef5799d` | chore: add STABLE compatibility guards | ⏭️ | Uses `#if STABLE` (never true); local `typeof(Resources)` unconditional is correct for all flavors |

## Resources — Delta-specific (Technical/Properties/)

| Hash | Description | Status | Notes |
|------|-------------|--------|-------|
| `f4d9c60c` | chore: add localized strings for delta thresholds, alerts, and average modes | ✅ | Handled via Phase 3 / `local/build/04-localization` |
| `b071cbf0` | chore: refine threshold group labels and add price signal / delta label groups | ✅ | Keys in layer 04; Display attrs in `local/delta-i18n` |
| `1de5e2c8` | chore: refine delta volume group labels | ✅ | Keys in layer 04 |
| `7b764bad` | fix: correct average labels and SMA translations | ✅ | Keys in layer 04 |
| `9053fcd7` | fix: clarify divergence labels and descriptions | ✅ | Keys in layer 04 |
| `ea6d3c7c` | fix: add Divergence GroupName localization | ✅ | Keys in layer 04 |
| `338068b1` | fix: clarify fixed delta threshold labels and descriptions | ✅ | Keys in layer 04 |
| `9c59238b` | fix: clarify audio and visual alerts localization | ✅ | Keys in layer 04 |
| `241ce6cd` | chore: use delta-specific color labels in drawing settings | ✅ | Keys in layer 04 |
| `25f0ac37` | fix: add absorption strings for stable compatibility | ✅ | Keys in layer 04 |

---

## OHLCPlus — Technical/OHLCPlus.cs (44 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `cfaba2f9` | fix(build): stabilize multi-platform builds (AnyCPU vs Cross) and prevent obj collisions | 🔲 |
| `0f816958` | chore(csproj): fix signing semantics and simplify SDK imports | 🔲 |
| `6e502c67` | refactor: route Display attributes to Resources under STABLE builds | 🔲 |
| `c5ca65ed` | fix: add missing localization resource files for main ATAS languages | 🔲 |
| `81a5e616` | refactor: streamline profile requests loop (no behavior change) | 🔲 |
| `14c68410` | refactor: compute period needs via generic helper (no behavior change) | 🔲 |
| `8447b822` | refactor: centralize level enumeration by period (no behavior change) | 🔲 |
| `45e21dfa` | chore: add label template and label parameters (no behavior change) | 🔲 |
| `5c74a65f` | feat: render labels using template and per-level overrides | 🔲 |
| `9acbbbd5` | feat: add configurable period prefixes for labels | 🔲 |
| `247c1247` | refactor: resolve label text via split-key suffix mapping | 🔲 |
| `05f12175` | chore: add localized labels for UI settings | 🔲 |
| `9d4652fa` | feat: add label and prefix settings UI | 🔲 |
| `9ffd20fc` | chore: normalize label template tokens to lowercase | 🔲 |
| `d22a882c` | feat: introduce batched, collision-aware label layout | 🔲 |
| `ad20febc` | feat: improve label placement with horizontal corridor and stable ordering | 🔲 |
| `b5ccac8a` | fix: correct line clipping bounds and trim bar line start | 🔲 |
| `c2f18e18` | feat: introduce explicit semantic label priority | 🔲 |
| `a12abc02` | fix: align UI resources for stable vs alpha/atas_x builds | 🔲 |
| `9664a58f` | fix: avoid redundant redraws when levels are unchanged | 🔲 |
| `b425f198` | chore: scaffold pq02 visual semantic descriptors | 🔲 |
| `545a78a7` | feat: add pq02 visual semantic UI and palettes | 🔲 |
| `981e13c0` | feat: implement pq02 visual semantic rulesets (by period / by level type) | 🔲 |
| `eb51f389` | feat: apply pq02 visual semantic styles during rendering | 🔲 |
| `004e89e9` | chore: add OverrideLabel resource key | 🔲 |
| `2bed76ee` | feat: add per-level label override | 🔲 |
| `1a21cb4c` | fix: make per-level OverrideLabel replace the full label | 🔲 |
| `2176efbd` | chore: add OverrideColorInSchemes resource keys | 🔲 |
| `69fcdc69` | feat: allow per-level color override when semantic schemes are active | 🔲 |
| `8000c260` | chore: localize visual semantic mode and preset labels | 🔲 |
| `4ece1376` | chore: localize visual semantic preset and mode displays | 🔲 |
| `8ce33b3e` | chore: add semantic scheme width and line style override strings | 🔲 |
| `aa7bbc18` | feat: allow per-line width and style overrides in scheme modes | 🔲 |
| `ef458870` | chore: add visual semantic mode and palettes strings | 🔲 |
| `9edf68b3` | chore: localize visual semantic mode and palettes strings | 🔲 |
| `045d2b72` | chore: add HVN labels and descriptions for all languages | 🔲 |
| `68d773d0` | feat: compute HVN bands from fixed profiles (overlay plumbing) | 🔲 |
| `15b2ca42` | feat: render HVN bands with priority and occlusion | 🔲 |
| `9709fc85` | fix: skip HVN recomputation when disabled; convert WPF colors for HVN fill | 🔲 |
| `75b6b904` | refactor: unify profile band model for HVN/LVN overlays | 🔲 |
| `8ce614b9` | chore: add LVN labels and descriptions for all languages; align HVN | 🔲 |
| `10cd6ce9` | chore: add LVN UI group for all supported languages | 🔲 |
| `079b863d` | feat: add LVN bands settings and cache scaffolding | 🔲 |
| `9d85b2a8` | feat: compute LVN bands from fixed profiles | 🔲 |

---

## ClusterStatistic — Technical/ClusterStatistic.cs (37 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `9ad699ae` | refactor: reorganize settings and row toggles for scalping workflow | 🔲 |
| `3659ed92` | fix: correct net imbalance alert crossing (closed candle) | 🔲 |
| `e4ed3f20` | perf: gate imbalance computation and rebuild on enable/params change | 🔲 |
| `d32ea792` | chore: add localization keys | 🔲 |
| `ca44800c` | chore: add more localization keys | 🔲 |
| `9e656e71` | refactor: replace hardcoded UI strings with Resources | 🔲 |
| `3c5546d3` | fix: correct, complete and shorten localization strings | 🔲 |
| `47457e7c` | fix: add row subgroup labels and fix missing translations | 🔲 |
| `eee746cb` | fix: correct DeltaAlert localization | 🔲 |
| `36658d0b` | refactor: reorganize Rows into logical UI subgroups | 🔲 |
| `b18066ff` | fix: rebuild imbalances immediately on parameter changes | 🔲 |
| `18add1c8` | fix: use full-range MAX for non-visible scaling | 🔲 |
| `14a9e105` | fix: make closed net-imbalance alert deterministic | 🔲 |
| `e5bf859f` | refactor: centralize imbalance compute gating | 🔲 |
| `4c5a8b06` | fix: reset SoT runtime window on bar change | 🔲 |
| `0b645e2f` | chore: align UI order comments with actual property order | 🔲 |
| `3e040bed` | refactor: reuse imbalance series writer in historical rebuild | 🔲 |
| `876edebb` | fix: use absolute max for net imbalance scaling | 🔲 |
| `b0b372300` | chore: normalize indentation and remove duplicate comments | 🔲 |
| `e0046c0b` | fix: use auto-filter mean fallback for live bar scaling | 🔲 |
| `419d2b86` | feat: add peak Vol/sec and paired Delta rows (plumbing) | 🔲 |
| `33af74b7` | feat: compute SoT peak Vol/sec and paired Delta from cumulative trades | 🔲 |
| `b9ad6779` | fix: seed SoT live window in chronological order | 🔲 |
| `a305147a` | feat: add SoT auto-filter scaling (EMA/SMA) for peak metrics | 🔲 |
| `7130799f` | feat: add Peak Delta/Vol row derived from SoT peaks | 🔲 |
| `876d89ab` | feat: add Delta/sec row with scaling and formatting | 🔲 |
| `7e78c702` | fix: correct max bid and delta/vol maxima tracking | 🔲 |
| `b677ec05` | refactor: add ratio formatting helper | 🔲 |
| `4e100638` | feat: unify ratio formatting and optional percent display | 🔲 |
| `8dad75ce` | feat: add buy/sell/net imbalance rows | 🔲 |
| `ac74892a` | feat: add net imbalance threshold alert (crossing, no-spam) | 🔲 |
| `991b0380` | feat: add stacked imbalances rows (consecutive levels) | 🔲 |
| `87cefe24` | refactor: improve table readability with contrast text and high-rate outlines | 🔲 |
| `de4b05e3` | refactor: reorder default rows for faster scalping reads | 🔲 |
| `dc1a4587` | chore: add ClusterStatistic alert taxonomy groups and message templates | 🔲 |
| `16cd5796` | refactor: place Alerts after imbalance settings (2000–2690) | 🔲 |
| `2fa0f4a4` | refactor: reorganize alert UI ordering and align group names | 🔲 |
| `ff76f97c` | fix: localize alert messages using Resources templates | 🔲 |

---

## MultiMarketPower — Technical/MultiMarketPower.cs (22 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `94a06c9b` | fix: stabilize historical tick cursor iteration | 🔲 |
| `e393cc28` | fix: make historical request window deterministic | 🔲 |
| `9d56b8e7` | chore: add localization keys for upcoming UI settings | 🔲 |
| `0e309fa5` | feat: add Smart Money Spread series | 🔲 |
| `f7316bba` | chore: sync ResourceDesigner with updated .resx files | 🔲 |
| `d77aa581` | feat: add view mode toggle for filters vs spread | 🔲 |
| `ffbfbe08` | feat: add session controls and session-based resets | 🔲 |
| `86ea4948` | fix: replay buffered realtime after history load | 🔲 |
| `31cc814e` | fix: guard OnCalculate against bar 0 | 🔲 |
| `994c03a2` | feat: add signal SMA and 4-color spread shading | 🔲 |
| `a8cf811d` | fix: refresh chart correctly when switching view mode | 🔲 |
| `dd28d2f7` | refactor: compute signal SMA using rolling window | 🔲 |
| `f4c76d71` | fix: update rolling SMA on in-bar cumulative trade updates | 🔲 |
| `f07f1eca` | refactor: remove redundant realtime replay from history calculation | 🔲 |
| `08cdc973` | fix: avoid treating every tick as new bar for signal SMA | 🔲 |
| `0aa076bc` | fix: respect filter visibility toggles in Filters view | 🔲 |
| `b199694b` | fix: normalize custom session boundary using instrument timezone | 🔲 |
| `8422d948` | fix: filter realtime replay to avoid history overlap | 🔲 |
| `c6c0d034` | chore: add localized entries for view and session mode values | 🔲 |
| `63c69d84` | fix: localize view and session mode values | 🔲 |
| `3e56472a` | chore: align default filter line colors and widths with volume semantics | 🔲 |
| `025467ee` | chore: align default spread colors with market state semantics | 🔲 |
| `38ed1595` | chore: reorganize UI groups for clearer workflow | 🔲 |
| `add88635` | chore: refine UI labels for improved readability | 🔲 |

---

## DailyLines — Technical/DailyLines.cs (21 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `eddf2199` | feat: add TradingDayStart setting (backward-compatible defaults) | 🔲 |
| `164dcc6d` | refactor: add TradingDayStart anchor helpers for day bucketing | 🔲 |
| `74843eb1` | fix: anchor Day rollover to TradingDayStart and compute day range only from session window | 🔲 |
| `46036b44` | fix: make PreviousDay use last completed trading-day bucket with in-window data | 🔲 |
| `b73b52b4` | feat: add optional Half Gap line for CurrentDay custom session | 🔲 |
| `a3916ce1` | refactor: introduce per-scope state container (legacy Period behavior unchanged) | 🔲 |
| `7f83fc86` | feat: add multi-scope toggles (legacy Period mode preserved) | 🔲 |
| `d481e9bc` | feat: compute ranges per active scope (multi-scope calc) | 🔲 |
| `dd030dcf` | feat: add ETH scope (TradingDayStart → RTH start) | 🔲 |
| `f9af8ee2` | feat: add RTH scopes (current/previous) for multi-scope mode | 🔲 |
| `43899a13` | feat: add per-scope style overrides and scope-prefixed labels for multi-scope | 🔲 |
| `a49a7748` | feat: add per-scope level visibility for day-family scopes | 🔲 |
| `207b0b64` | fix: guard DrawPrice against negative y | 🔲 |
| `41663971` | fix: make "custom session inactive" check viewport-based for custom sessions | 🔲 |
| `70687b1d` | feat: add overlay warnings for missing/incomplete history (multi-scope) | 🔲 |
| `5b5f7151` | fix: do not block non-session scopes when custom session is inactive | 🔲 |
| `84bc4cdd` | fix: recalc/redraw when multi-scope and HalfGap settings change | 🔲 |
| `4298d412` | refactor: render levels via per-line settings model | 🔲 |
| `5e553c92` | feat: add per-line UI settings grouped by scope (OHLCPlus-style) | 🔲 |
| `edf31aba` | chore: reorder settings UI and avoid redundant scope prefix in labels | 🔲 |
| `234bdc08` | feat: default per-line UI disabled with single-period auto-enable | 🔲 |
| `c3f6940d` | chore: reorder show settings UI | 🔲 |
| `fc6fceb4` | fix(DailyLines): wip fixes for production | 🔲 |

---

## Volume — Technical/Volume.cs (6 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `016b4690` | fix: make MaximumVolume follow selected input | 🔲 |
| `023a3c4d` | feat: add fixed threshold lines (minor/major) | 🔲 |
| `8870e9ce` | feat: add dynamic volume thresholds (mean + std) with session reset | 🔲 |
| `d0e989f4` | fix: reset Full24h dynamic thresholds on new session | 🔲 |
| `d58a0c32` | chore: normalize indentation to tabs | 🔲 |
| `2fc7396e` | chore: remove redundant dynamic reset and tidy whitespace | 🔲 |
| `17a2fd33` | chore: localize threshold UI (properties and drawing series) | 🔲 |

---

## TradesOnChart — Technical/TradesOnChart.cs (3 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `3749d4f6` | feat: load trade history by chart range | 🔲 |
| `7c558b15` | fix: update TradingStatisticsProvider event and property names after upstream API change | 🔲 |
| `48ab4e05` | chore: localize Display metadata for label mode and PnL colors | 🔲 |

---

## ClusterSearch — Technical/ClusterSearch.cs (6 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `81b6ffde` | chore: pin CalcMode values and add DiagonalImbalance enum + color override field | 🔲 |
| `b0d115ff` | chore: add localized resources for diagonal imbalance UI | 🔲 |
| `4313ce4a` | feat: add diagonal imbalance filters and detection logic | 🔲 |
| `6ad637b91` | feat: support DiagonalImbalance in SeriesHandling | 🔲 |
| `255d8e77` | chore: wire diagonal imbalance descriptions into UI attributes | 🔲 |
| `2f1d5de8` | chore: add localized descriptions for diagonal imbalance settings | 🔲 |
| `9867dfd5` | fix: correct MaxAverageTrade filter comparison | 🔲 |

---

## InitialBalance — Technical/InitialBalance.cs (10 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `7802d34b` | chore: add resources for label position, overlay type and formation flag | 🔲 |
| `67bc7e4a` | refactor: migrate labels to OnRender and use localized resource strings | 🔲 |
| `2e7edc85` | feat: add FontSize parameter for label rendering | 🔲 |
| `d7ce7205` | feat: add LabelPosition parameter with localized enum values | 🔲 |
| `ec544f5f` | feat: add overlay connectors under labels | 🔲 |
| `9ad23d11` | fix: align overlay connectors with measured label boxes | 🔲 |
| `8f02714e` | fix: use label caption resources | 🔲 |
| `263c8252` | feat: freeze labels after custom session and snapshot IB levels at completion | 🔲 |
| `d051ee90` | fix: restore Mid label and complete overlay connectors | 🔲 |
| `766d6b28` | feat: add ShowDuringFormation option | 🔲 |
| `180fcb28` | fix: snapshot IB completion prices after bar values are computed | 🔲 |
| `ca57d23a` | fix: keep label anchor fixed after custom session ends | 🔲 |

---

## AccountInfoDisplay — Technical/AccountInfoDisplay.cs (14 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `3eb703f9` | feat: keep last trade max open pnl visible after close | 🔲 |
| `4ca7f1ae` | feat: complete soft risk engine with full CAUTION thresholds | 🔲 |
| `9ad899b6` | feat: add per-row visibility toggles for risk panels | 🔲 |
| `d9f4a2d7` | refactor: stabilize UI property order ranges | 🔲 |
| `38deb9b8` | fix: merge loaded per-account config with defaults | 🔲 |
| `b3014078` | fix: stabilize monthly reset marker and EOD peak when EOD time changes | 🔲 |
| `f3ea628b` | feat: add daily price rails config and persistence | 🔲 |
| `91bc1f68` | feat: render daily price rails on chart | 🔲 |

---

## GammaLevels — Technical/GammaLevels.cs (30 commits — pending, new indicator)

| Hash | Description | Status |
|------|-------------|--------|
| `7cef71fc` | chore: scaffold GammaLevels indicator shell with localized UI | 🔲 |
| `811cd6ae` | refactor: align file layout with standard regions | 🔲 |
| `66794c01` | refactor: add normalized model and multi-source contracts | 🔲 |
| `61a809d6` | feat: add LoloText parsing pipeline with throttled warnings logging | 🔲 |
| `db9fd89c` | feat: implement LoloText parser | 🔲 |
| `9655efb5` | feat: add engine to merge levels and select winner by rank and category | 🔲 |
| `fc1b6d31` | feat: render level lines with tiers and 0DTE halo | 🔲 |
| `bb4ca3a6` | feat: render labels with cached text measurement | 🔲 |
| `7fd9ee8e` | refactor: collect entries from multiple sources via ILevelsSource | 🔲 |
| `235f16ff` | fix: refresh text input after clear actions | 🔲 |
| `2e584052` | feat: add configurable label font size | 🔲 |
| `ea0abb57` | fix: invalidate label cache on font changes and stabilize warning throttle | 🔲 |
| `26fbe47f` | fix: make rendering robust under extreme zoom and thin bars | 🔲 |
| `f1a99042` | feat: add MenthorQ text source UI and wire source pipeline | 🔲 |
| `7b413f42` | feat: add MenthorQ categories and update winner priority order | 🔲 |
| `4fba3eb0` | feat: parse MenthorQ text levels and apply offset | 🔲 |
| `cbb2478b` | feat: add default pens for MenthorQ categories | 🔲 |
| `4d919905` | fix: make pen cache resilient to sparse LevelCategory values | 🔲 |
| `a2fbaf64` | refactor: split private methods into parsing/engine/rendering/logging regions | 🔲 |
| `1f8c95c1` | refactor: remove unused LevelRenderItem model | 🔲 |
| `90334951` | feat: add thin dotted accent for high-priority 0DTE levels | 🔲 |
| `8c9f2d68` | chore: sync pens UI resources across all locales | 🔲 |
| `0ccf8e6e` | feat: expose pen settings in UI with localized descriptions | 🔲 |
| `a721397c` | fix: support MenthorQ index/futures text inputs in source pipeline | 🔲 |
| `a404331a` | fix: dedupe same concepts across sources with deterministic precedence | 🔲 |
| `1413590e` | feat: add source truth preset for conflict resolution | 🔲 |
| `4fd9f4e2` | fix: make source truth preset robust to missing optional sources | 🔲 |

---

## DOM / DOMStrength (2 commits — pending)

| Hash | Description | Status |
|------|-------------|--------|
| `3a365962` | fix(dompower): reset state safely when depth filter changes | 🔲 |
| `cee95603` | fix(domstrength): correct bid cumulative depth source and depth loop size | 🔲 |

---

## Compat / Build (2 commits — done)

| Hash | Description | Status | Notes |
|------|-------------|--------|-------|
| `cfaba2f9` | fix(build): stabilize multi-platform builds (AnyCPU vs Cross) and prevent obj collisions | ✅ | Absorbed into `local/build/02-multiversion` (superseded by full rewrite) |
| `0f816958` | chore(csproj): fix signing semantics and simplify SDK imports | ✅ | Absorbed into `local/build/02-multiversion` |

---

## Docs / ADRs (10 commits — pending evaluation)

These are documentation and architecture decision records in `prready/main:docs/`. They do not
exist yet in `meta/docs`. Each needs review before porting — some may be superseded, merged,
or kept as-is.

| Hash | Description | Status |
|------|-------------|--------|
| `185087e8` | docs(decisions): add ADR-005 (alert API compat wrapper) and ADR-006 (session timezone policy for dynamic thresholds) | 📋 |
| `0219309e` | docs(architecture): refine localization rules and formalize ADR section | 📋 |
| `a797cb7b` | docs(decisions): add ADR template, normalize ADRs, and reference process in architecture | 📋 |
| `423d284f` | chore(docs): document rebase and retag workflow | 📋 |
| `a8594d5f` | chore(docs): add ADR for ClusterStatistic imbalance rebuild coalescing | 📋 |
| `7faf4dc8` | docs(atas): add normative process docs for Phase 2/3 and Gold Standard v7 | 📋 |
| `1228db5f` | chore(docs): add ADRs for GammaLevels post-tag roadmap | 📋 |
| `78d4efe6` | docs(tradesonchart): add ADR to postpone synthetic FIFO trade reconstruction | 📋 |
| `fe917f34` | docs(AccountInfoDisplay): add ADR for per-account persistence model | 📋 |
| `e37d81e3` | chore(docs): add DailyLines production checklist and issue log templates | 📋 |
