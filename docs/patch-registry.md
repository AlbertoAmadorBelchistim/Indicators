# Patch registry

| Include | Commit | Date | Type | Area | Subject | Source branch | Cherry-pick safe | In testing | In release | PR status | Upstream status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| yes | `c62e44ae` | 2026-03-25 | fix | domstrength | fix(domstrength): use bid depth values when summing bids | fix/domstrength-bid-sum-source | yes | no | no | not-opened | pending | Copy-paste bug: was using `_mDepthAsk.Values` for bids |
| yes | `be5939ca` | 2026-03-25 | fix | domstrength | fix(domstrength): avoid off-by-one when iterating level depth | fix/domstrength-leveldepth-off-by-one | yes | no | no | not-opened | pending | Loop was `<= LevelDepth.Value` instead of `<` |
| yes | `0e610d33` | 2026-03-25 | fix | dompower | fix(dompower): reset state and rebind LevelDepth filter safely | fix/dompower-leveldepth-state-reset | yes | no | no | not-opened | pending | Event subscription leak + stale state on filter change |
| yes | `ef3f6513` | 2026-03-25 | fix | volume | fix(volume): make alert messages reflect selected input type | fix/volume-alert-label | yes | no | no | not-opened | pending | Upstream strings were hardcoded; uses `GetInputLabel()` |
| yes | `fdd1bbef` | 2026-03-25 | fix | volume | fix(volume): make MaximumVolume follow selected input | fix/volume-max-follow-input | yes | no | no | not-opened | pending | Lookback used `candle.Volume` instead of `val` |
| pending | `64ed7f58` | prready/main | fix | delta | fix(delta): clamp price-panel triangle markers to visible region | fix/delta-marker-clamp (not yet created) | yes | no | no | not-opened | pending | Markers outside chart bounds caused visual artifacts |

---

## Patch sets

### local/integration/testing

```bash
# DomStrength fixes (independent, any order)
git cherry-pick 8308d666 b3d7354f

# DomPower fix (independent)
git cherry-pick 70354534

# Volume fixes (ordered — alert-label must precede max-follow-input)
git cherry-pick 9760b860 f7fd5c82
```

### local/integration/release

```bash
git cherry-pick 8308d666 b3d7354f 70354534 9760b860 f7fd5c82
```

---

## Patch Notes

- `fix/domstrength-*` and `fix/dompower-*` are fully independent and atomic.
- `fix/volume-max-follow-input` depends on `fix/volume-alert-label` (adds `GetInputLabel()` used by both).
- All five original patches are safe for PR submission to upstream.
- `fix/delta-marker-clamp` is pending branch creation (cherry-pick from `prready/main` commit `64ed7f58`).

---

&nbsp;

# Build stacks

### local/build/03-version-shims

| Stack | Base | Scope | Integration mode | Notes |
|---|---|---|---|---|
| `local/build/03-version-shims` | `local/build/02-multiversion` | compatibility / multi-version support | Stack | Compatibility shims, TabAttribute stub, and build adaptations for all ATAS flavors |

#### Included changes (high-level)

**Compatibility**
- compat(stable): guard newer CandleDataSeries visual API by build flavor
- compat(beta): guard newer CandleDataSeries visual API by build flavor
- compat(candles): disable DrawCandleBorder on stable
- compat(candles): add VWAP shim for stable 7.09
- compat(stable): add PropertiesEditor compatibility shim
- compat(rollovers): exclude RolloverDates from stable build
- compat(clusterstatistic): add TabAttribute stub for pre-release OFT.Attributes builds

**Resources and tooling**
- compat(resources): sync local resource catalogs with extracted ATAS localization data
- tooling(localization): scripts to export and sync localization resources

**Build**
- build(solution): multi-flavor build configurations (Alpha / Beta / Latest / Stable / ATAS_X_Alpha / ATAS_X_Beta)

#### TabAttribute note

`Compat/TabAttributeCompat.cs` provides a stub `[Tab]` attribute for non-Alpha builds.
Remove this file (or set `TAB_ATTRIBUTE_AVAILABLE` for the relevant configs in csproj) once
all target ATAS versions ship `TabAttribute` in `OFT.Attributes.dll`.

### local/build/04-localization

| Stack | Base | Scope | Integration mode | Notes |
|---|---|---|---|---|
| `local/build/04-localization` | `local/build/03-version-shims` | resources | Stack | Adds new .resx keys needed by local indicator branches |

#### Included keys (2026-03-25)

27 keys for Volume.cs threshold feature + 40 keys for Delta.cs features, across all 7 locales
(en, de-de, ru-ru, es-es, fr-fr, hi-in, zh-cn):

**Volume keys (27):** `ThresholdsGroup`, `FixedThresholdGroup`, `DynamicThresholdGroup`, `ShowThresholdLines`,
`FixedMinorLevel`, `FixedMajorLevel`, `ThresholdSource`, `SessionWindowMode`, `RthStart`, `RthEnd`,
`SamplesForMeanStd`, `StdMultiplier`, `ThresholdSourceFixed`, `ThresholdSourceDynamicWelford`,
`SessionWindowModeRth`, `SessionWindowModeFull24h`, `VolumeThresholdMajor`, `VolumeThresholdMinor`
and their `*Description` variants.

**Delta keys (40):** `AudioAlerts`, `AudioAtBarCloseOnly`, `AudioDownThresholds`, `AudioUpThresholds`,
`BaseColor`, `ColorMode`, `Fixed`, `FixedNegMajorLevel`, `FixedNegMinorLevel`, `FixedPosMajorLevel`,
`FixedPosMinorLevel`, `MarkerSize`, `PriceSignalDownColor`, `PriceSignalUpColor`, `PriceSignalsGroup`,
`ShowVisualAlerts`, `Slope`, `SlopeDownColor`, `SlopeUpColor`, `ThresholdLevelMajor`, `ThresholdLevelMinor`,
`VisualDownThresholds`, `VisualUpThresholds`, `ZeroCross`
and their `*Description` variants.

### Stack notes

- Build stacks are treated as **cohesive units** — not suitable for cherry-picking individual commits.
- Each stack layer must be rebased on its parent after any upstream merge into Develop.
- `local/*` branches use `typeof(Resources)` for display attributes; `fix/*` branches targeting upstream use `typeof(Strings)`.
