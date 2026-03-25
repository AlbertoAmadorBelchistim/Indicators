# Port manifest — Delta

**Source:** `prready/main`
**Integration target:** `local/delta-i18n` (stacked on `local/build/04-localization`)
**Status:** `in-progress`

---

## Phase 0 — Resource keys required in `local/build/04-localization`

Keys added in commit `67e122dc` on `local/build/04-localization`.

| Key | Value (en) | Introducing commit | Used by | Status |
|-----|-----------|-------------------|---------|--------|
| `AudioAlerts` | Audio alerts | `9c59238b` | audio alerts group header | done |
| `AudioAlerts`Description | ... | `9c59238b` | tooltip | done |
| `AudioAtBarCloseOnly` | At bar close only | `9c59238b` | audio property | done |
| `AudioAtBarCloseOnlyDescription` | ... | `9c59238b` | tooltip | done |
| `AudioDownThresholds` | Down threshold | `9c59238b` | audio property | done |
| `AudioDownThresholdsDescription` | ... | `9c59238b` | tooltip | done |
| `AudioUpThresholds` | Up threshold | `9c59238b` | audio property | done |
| `AudioUpThresholdsDescription` | ... | `9c59238b` | tooltip | done |
| `BaseColor` | Base color | `241ce6cd` | average line property | done |
| `ColorMode` | Color mode | `241ce6cd` | average line property | done |
| `Fixed` | Fixed | `338068b1` | ThresholdSource enum / ThresholdLevel | done |
| `FixedNegMajorLevel` | Negative major | `338068b1` | fixed threshold property | done |
| `FixedNegMajorLevelDescription` | ... | `338068b1` | tooltip | done |
| `FixedNegMinorLevel` | Negative minor | `338068b1` | fixed threshold property | done |
| `FixedNegMinorLevelDescription` | ... | `338068b1` | tooltip | done |
| `FixedPosMajorLevel` | Positive major | `338068b1` | fixed threshold property | done |
| `FixedPosMajorLevelDescription` | ... | `338068b1` | tooltip | done |
| `FixedPosMinorLevel` | Positive minor | `338068b1` | fixed threshold property | done |
| `FixedPosMinorLevelDescription` | ... | `338068b1` | tooltip | done |
| `MarkerSize` | Marker size | `9c59238b` | price signal property | done |
| `MarkerSizeDescription` | ... | `9c59238b` | tooltip | done |
| `PriceSignalDownColor` | Down signal color | `9c59238b` | price signal property | done |
| `PriceSignalDownColorDescription` | ... | `9c59238b` | tooltip | done |
| `PriceSignalOffsetTicksDescription` | ... | `9c59238b` | tooltip | done |
| `PriceSignalSizeDescription` | ... | `9c59238b` | tooltip | done |
| `PriceSignalUpColor` | Up signal color | `9c59238b` | price signal property | done |
| `PriceSignalUpColorDescription` | ... | `9c59238b` | tooltip | done |
| `PriceSignalsGroup` | Price signals | `b071cbf0` | property group header | done |
| `ShowVisualAlerts` | Show visual alerts | `9c59238b` | price signal property | done |
| `ShowVisualAlertsDescription` | ... | `9c59238b` | tooltip | done |
| `Slope` | Slope | `241ce6cd` | AverageColorMode enum | done |
| `SlopeDownColor` | Slope down color | `241ce6cd` | average line property | done |
| `SlopeUpColor` | Slope up color | `241ce6cd` | average line property | done |
| `ThresholdLevelMajor` | Major | `9c59238b` | ThresholdLevel enum | done |
| `ThresholdLevelMinor` | Minor | `9c59238b` | ThresholdLevel enum | done |
| `VisualDownThresholds` | Down threshold | `9c59238b` | visual alert property | done |
| `VisualDownThresholdsDescription` | ... | `9c59238b` | tooltip | done |
| `VisualUpThresholds` | Up threshold | `9c59238b` | visual alert property | done |
| `VisualUpThresholdsDescription` | ... | `9c59238b` | tooltip | done |
| `ZeroCross` | Zero cross | `241ce6cd` | AverageColorMode enum | done |
| `DeltaLabelGroup` | Delta label | `b071cbf0` | label group header in Delta.cs | done (`d20c6846`) |
| `DivergenceDotsDescription` | Shows divergence markers... | `9053fcd7` | DivergenceDots tooltip | done (`d20c6846`) |
| `DivergenceBarsDescription` | Highlights divergence bars... | `9053fcd7` | DivergenceBars tooltip | done (`d20c6846`) |
| `DeltaPositiveColor` | Positive color | `241ce6cd` | needed by `0d087ebd` (Phase 3) | pending (Phase 3) |
| `DeltaNegativeColor` | Negative color | `241ce6cd` | needed by `0d087ebd` (Phase 3) | pending (Phase 3) |
| `UpperMajorLevel` | Upper major | `b071cbf0` | not referenced in current Delta.cs | skipped — unused |

---

## Phase 1 — Fix branches (port before feat)

Fix branches must have their content applied to `local/delta-i18n` **before** feat content.

| Commit (prready/main) | Description | Local branch | Status | Gap in local/delta-i18n |
|----------------------|-------------|--------------|--------|--------------------------|
| `e806415e` (local) / matches `prready/main` | Remove dead field `_absorptionThreshold` | `fix/delta-deadfield-absorptionthreshold` | **missing** | `_absorptionThreshold = 250` still present |
| `183adb44` (local) / matches `prready/main` | Clamp divergence dots to visible price region (add `>= region.Top` check) | `fix/delta-divergence-dot-bounds` | **missing** | only `<= region.Bottom` check present |
| `64ed7f58` (prready/main) / `7b3a8405` (local) | Clamp triangle markers to visible region | `fix/delta-triangle-clamp` | **partial** — see Section 5 |

---

## Phase 2 — Feat branches

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `19fc5e7a` | Fixed threshold line series (up/down major/minor) | `feat/delta-fixed-thresholds` | done | portado en commit `93a9c698` |
| `f60173c2` | Price-panel triangle signals (visual alerts) | `feat/delta-price-signals` | done | portado en commit `8711535a` |
| `e403bc22` | ThresholdLevel enum + per-side visual alert selection | `feat/delta-threshold-selection` | done | portado en commit `29a9b0d7` |
| `e2890e31` | Session-anchored dynamic thresholds (Welford) | `feat/delta-dynamic-thresholds` | done | portado en commit `b1332a2e` |
| `a539ce4b` | Audio alerts with cooldown and bar-close policy | `feat/delta-audio-alerts` | done | portado en commit `df6bfd07` |
| `72af4410` | Average delta line: SMA/EMA + three color modes | `feat/delta-average-line` | done | portado en commit `9e7bd98c` |
| `29f60e6d` | Refactor: extract OnCalculate helpers + unify state reset | none | **skipped** | high integration cost, low external value; deferred indefinitely |

---

## Phase 3 — Chore / UI polish / compat

These commits must be applied before publication. They do not add features but affect UX quality.

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `3e420e5c` | Hide internal series from Drawing group (`IsHidden = true`) | `chore/delta-ui-polish` (planned) | **pending** | affects _priceSignalUp/Down + _avgSeries visibility |
| `811000d6` | Reorganize UI groups and ordering (no behavior change) | `chore/delta-ui-polish` (planned) | **pending** | property Order values differ from prready/main |
| `5fe49e01` | Use `Width` (LineWidth) for average thickness + UI order | `chore/delta-ui-polish` (planned) | **pending** | average uses different thickness property |
| `5f78e83f` | Assign specific divergence descriptions + fix i18n errors | `chore/delta-ui-polish` (planned) | **pending** | DivergenceDots Name is hardcoded string, not `nameof` |
| `5e336b28` | Delta-specific labels for fixed threshold properties | `chore/delta-ui-polish` (planned) | **pending** | uses generic labels |
| `eff7ee3f` | Hide and disable legacy fixed-value alert fields | `chore/delta-ui-polish` (planned) | **pending** | legacy fields still visible |
| `9da4aa2a` | Add Display attrs to ThresholdLevel enum | `local/delta-i18n` | done | applied in commit `29a9b0d7` |
| `0d087ebd` | Use `DeltaPositiveColor`/`DeltaNegativeColor` in Drawing group | `chore/delta-ui-polish` (planned) | **pending** | requires Phase 0 keys added first |
| `1ef5799d` | Add STABLE compatibility guards | none | **skipped** | bug: uses `#if STABLE` (never true) instead of `#if ATAS_STABLE`; unconditional `typeof(Resources)` is acceptable for all flavors |

**Resource-only commits (refine keys, all covered by Phase 0 keys above):**

| Commit | Description | Status |
|--------|-------------|--------|
| `b071cbf0` | Refine threshold group labels + PriceSignalsGroup + DeltaLabelGroup | partially done — DeltaLabelGroup missing |
| `1de5e2c8` | Refine delta volume group labels (LabelTextColor/Location/Font) | done — keys exist |
| `7b764bad` | Correct average labels and SMA translations | done — keys exist |
| `9053fcd7` | Clarify divergence labels (DivergenceDots/Bars + descriptions) | partially done — descriptions missing |
| `ea6d3c7c` | Add Divergence GroupName localization | done |
| `338068b1` | Clarify fixed threshold labels/descriptions | done |
| `9c59238b` | Clarify audio/visual alerts localization | done |
| `241ce6cd` | Delta-specific color labels (resources side) | done |
| `25f0ac37` | Absorption strings for stable compat | done |

---

## Phase 4 — Integration verification: `local/delta-i18n`

### 4.1 Build
- [x] `dotnet build -c Alpha` — 0 errors (verified 2026-03-25)
- [ ] `dotnet build -c Stable` — pending

### 4.2 Content completeness
- [ ] fix/delta-deadfield content present — **NO** (`_absorptionThreshold` still present)
- [ ] fix/delta-divergence-dot-bounds content present — **NO** (missing `>= region.Top`)
- [~] fix/delta-triangle-clamp content present — **PARTIAL** (skip vs clamp — see Section 5)
- [x] feat/delta-fixed-thresholds ported
- [x] feat/delta-price-signals ported
- [x] feat/delta-threshold-selection ported
- [x] feat/delta-dynamic-thresholds ported
- [x] feat/delta-audio-alerts ported
- [x] feat/delta-average-line ported
- [ ] Phase 3 chore changes applied — **NO**

### 4.3 Resource completeness
- [x] 40 new keys in Resources.resx (en) — verified
- [x] All 40 keys in 6 satellite locales — verified
- [x] Resources.Designer.cs up to date for 40 keys
- [x] `DeltaLabelGroup` in Resources.resx — done (`d20c6846`)
- [x] `DivergenceDotsDescription` in Resources.resx — done (`d20c6846`)
- [x] `DivergenceBarsDescription` in Resources.resx — done (`d20c6846`)
- [ ] DivergenceDots `Name` uses `nameof(Resources.DivergenceDots)` not hardcoded string — pending (Phase 3)
- [ ] DivergenceBars has `Description = nameof(Resources.DivergenceBarsDescription)` — pending (Phase 3)

### 4.4 Functional smoke test
- [ ] Indicator loads without crash — pending full integration test
- [ ] "Delta label" group visible (not "DeltaLabelGroup" literal) — pending key fix
- [ ] No resource key literal visible in UI — pending

---

## Section 5 — Intentional divergences from prready/main

| Area | prready/main behavior | Local behavior | Reason | Revisit? |
|------|-----------------------|----------------|--------|----------|
| `typeof(Resources)` unconditional | prready/main wraps some Display attrs in `#if !ATAS_STABLE` guards via `1ef5799d` (buggy: uses `#if STABLE`) | Local uses `typeof(Resources)` unconditionally | `1ef5799d` is dead code (wrong define constant). Resources.resx is synced from platform so all flavors have the keys. Functionally equivalent. | Only if upstream fixes the define constant and ships real STABLE-specific behavior |
| Triangle rendering | prready/main clamps triangles to screen edge (`ClampInt`) — triangle always drawn at boundary | Local skips triangle if price is outside visible region | Different UX: prready/main always shows a triangle at the edge; local hides it if price is offscreen | Yes — prefer prready/main behavior; needs fix in Phase 1 remediation |
| `AverageMode` ResourceType | prready/main uses `typeof(Resources)` with `Resources.SMA`/`Resources.EMA` | Local uses `typeof(Strings)` with `Strings.SMA`/`Strings.EMA` | `SMA`/`EMA` keys exist in upstream `Strings` — using `Strings` is more correct for upstream-compatible keys | No — local approach is correct |
| `_averagePeriod` default | prready/main: `20` | Local: `14` | Ported from earlier feat branch before default was revised | Yes — update to `20` during Phase 3 |
| `_samplesForMeanStd` default | prready/main: `1` | Local: `10` | Same | Yes — update to `1` during Phase 3 |

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

| Gap | Severity | Planned fix | Target branch |
|-----|----------|-------------|---------------|
| `_absorptionThreshold` dead field still present | low — compiles, no behavior impact | Cherry-pick `e806415e` content | Add commit to `local/delta-i18n` |
| Divergence dots missing `>= region.Top` check | medium — dots can render above chart top | Cherry-pick `183adb44` content | Add commit to `local/delta-i18n` |
| Triangle rendering: skip vs clamp | medium — different UX for off-screen prices | Apply `ClampInt` approach from `7b3a8405` | Replace skip logic in `local/delta-i18n` |
| ~~`DeltaLabelGroup` key missing~~ | ~~high~~ | ~~done `d20c6846`~~ | ~~`local/build/04-localization`~~ |
| ~~`DivergenceDotsDescription` key missing~~ | ~~medium~~ | ~~done `d20c6846`~~ | ~~`local/build/04-localization`~~ |
| ~~`DivergenceBarsDescription` key missing~~ | ~~medium~~ | ~~done `d20c6846`~~ | ~~`local/build/04-localization`~~ |
| `DivergenceDots` Name is hardcoded string `"DivergenceDots"` | low — works but not refactoring-safe | Change to `nameof(Resources.DivergenceDots)` | `local/delta-i18n` |
| 8 Phase 3 chore commits not applied | medium — affects UX quality, not functionality | Create `chore/delta-ui-polish` branch + port to `local/delta-i18n` | `chore/delta-ui-polish` → `local/delta-i18n` |
| `_averagePeriod` default is `14` (should be `20`) | low | Update field initializer | `local/delta-i18n` |
| `_samplesForMeanStd` default is `10` (should be `1`) | low | Update field initializer | `local/delta-i18n` |
