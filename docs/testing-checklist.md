# Testing checklist

This document contains functional verification checklists for all local and PR-candidate branches.

## When to use this document

**Two-phase testing model:**

1. **Smoke test** — run when integrating a new branch into a local integration branch.
   Goal: catch crashes, missing series, broken property panels. Takes 1-2 minutes per indicator.

2. **Full functional check** — run before PR submission or publication.
   Use the full checklist for the relevant indicator section below.

---

## Build verification

Before any functional testing, verify the build is clean in all relevant flavors:

```bash
dotnet build Technical/Indicators.csproj -c Alpha
dotnet build Technical/Indicators.csproj -c Beta
dotnet build Technical/Indicators.csproj -c Stable
dotnet build Technical/Indicators.csproj -c ATAS_X_Alpha --platform Cross
```

Expected: 0 errors. Warnings from `NU1510` (System.Drawing.Common) are expected and non-blocking.

---

## fix/domstrength-bid-sum-source

**Scope:** `DomStrength.cs` — bid aggregation reads from `_mDepthBid.Values` instead of `_mDepthAsk.Values`

**Smoke test:**
- [ ] DomStrength loads on chart without crash

**Full check:**
- [ ] With a live or replay feed: bid depth line visually differs from ask depth line (they should not mirror each other)
- [ ] Previous builds showed both lines nearly identical — confirm they diverge after fix
- [ ] No NaN or zero values in bid series when depth data is present

---

## fix/domstrength-leveldepth-off-by-one

**Scope:** `DomStrength.cs` — level iteration was `<= LevelDepth.Value` (one extra level)

**Smoke test:**
- [ ] DomStrength loads and renders

**Full check:**
- [ ] Change `LevelDepth` to 5 — verify exactly 5 levels are aggregated (not 6)
- [ ] Change `LevelDepth` to 1 — single level only, values match top-of-book bid/ask
- [ ] Values are consistent between historical and realtime bars

---

## fix/dompower-leveldepth-state-reset

**Scope:** `DomPower.cs` — event leak and stale state on `LevelDepth` setter change

**Smoke test:**
- [ ] DomPower loads and renders

**Full check:**
- [ ] Change `LevelDepth` while indicator is running — series clears and recalculates without leftover stale values
- [ ] No duplicate event subscriptions (no doubled or amplified values after repeated changes)
- [ ] Reloading the indicator from scratch produces identical values to a post-reset state

---

## fix/volume-alert-label

**Scope:** `Volume.cs` — alert messages reflect selected input type via `GetInputLabel()`

**Smoke test:**
- [ ] Volume indicator loads

**Full check:**
- [ ] Set `Input` to `Volume` — alert fires with message referencing "volume"
- [ ] Set `Input` to `Ticks` — alert fires with message referencing "ticks"
- [ ] Set `Input` to `Bid` / `Ask` — alert messages reflect the selected side
- [ ] No hardcoded "volume" text in alerts when a different input is selected

---

## fix/volume-max-follow-input

**Scope:** `Volume.cs` — MaximumVolume lookback now uses `val` (current input) not `candle.Volume`

**Smoke test:**
- [ ] Volume indicator loads

**Full check:**
- [ ] Set `Input` to `Bid` — MaximumVolume correctly tracks maximum bid volume, not candle volume
- [ ] Set `Input` to `Ask` — same for ask
- [ ] Set `Input` to `Volume` — behavior identical to before (both code paths converge)
- [ ] MaxFollowInput toggle respected with non-volume inputs

---

## Delta indicator — full feature set

### Pre-flight
- [ ] Indicator loads on chart without crash
- [ ] All property groups visible: Settings, Thresholds, Fixed Thresholds, Alerts, Average Line, Drawing
- [ ] No missing resource key warnings in ATAS log
- [ ] Panel renders in separate panel below chart

### Fixed threshold lines (feat/delta-threshold-lines)
- [ ] `ShowThresholdLines` toggle shows/hides 4 lines
- [ ] `UpMajorLevel`, `UpMinorLevel`, `DownMinorLevel`, `DownMajorLevel` values move lines to correct prices
- [ ] Lines are continuous across all historical bars
- [ ] Line colors/styles visible and distinguishable (major solid, minor dotted by default)

### Price signals — visual alerts (feat/delta-price-signals)
- [ ] `ShowVisualAlerts` toggle shows/hides triangle markers
- [ ] Markers appear only on bars where delta crosses the selected threshold
- [ ] `VerticalOffset` shifts markers away from bar by the specified number of ticks
- [ ] `MarkerSize` changes arrow size visually
- [ ] `PriceSignalUpColor` and `PriceSignalDownColor` change arrow colors
- [ ] No markers on bars with delta inside the threshold band

### Threshold level selection (feat/delta-threshold-selection)
- [ ] `VisualUpThresholds` = Minor — up markers fire at minor level, not major
- [ ] `VisualUpThresholds` = Major — up markers fire at major level
- [ ] `VisualDownThresholds` behaves symmetrically for down markers
- [ ] Changing level while indicator is running updates markers on historical bars after recalculate

### Dynamic thresholds — Welford (feat/delta-dynamic-thresholds)
- [ ] `Thresholds` = Fixed — behavior identical to fixed threshold tests above
- [ ] `Thresholds` = DynamicWelford — threshold lines adapt to recent delta distribution
- [ ] `SamplesForMeanStd` = 1 — band adapts immediately (high sensitivity)
- [ ] `SamplesForMeanStd` = 50 — band is stable (low sensitivity)
- [ ] `StdMultiplier` = 1.0 vs 2.0 — visibly different band width
- [ ] `SessionMode` = Full24h — full session data feeds Welford accumulators
- [ ] `SessionMode` = RTH — only bars inside the RTH window contribute
- [ ] `RthStart` / `RthEnd` correctly gate session-aware accumulation (confirm with instrument timezone)
- [ ] No look-ahead: band values at historical bars are consistent with a forward-only calculation

### Audio alerts (feat/delta-audio-alerts)
- [ ] `AudioEnabled` = on — alert sound fires when delta crosses threshold
- [ ] `AudioAtBarCloseOnly` = on — alert fires only at bar close confirmation (no mid-bar sound)
- [ ] `AudioAtBarCloseOnly` = off — alert fires immediately when crossing during bar build
- [ ] `AlertCooldownBars` = 3 — no repeated alert within 3 bars after last trigger
- [ ] `AudioUpLevel` / `AudioDownLevel` control which threshold triggers audio independently from visual
- [ ] No crash when `InstrumentInfo` is null (e.g., indicator attached before data is loaded)

### Average delta line (feat/delta-average-line)
- [ ] `ShowAverage` toggle shows/hides average line
- [ ] `AveragePeriod` change recalculates and line smoothness changes visibly
- [ ] `CalculationMode` = SMA — flat smoothing visible
- [ ] `CalculationMode` = EMA — faster response to recent values
- [ ] `ColorMode` = Fixed — single color (`AverageColor`) used for entire line
- [ ] `ColorMode` = ZeroCross — line turns green above zero, red below zero
- [ ] `ColorMode` = Slope — line turns green when rising, red when falling
- [ ] `SlopeUpColor` / `SlopeDownColor` are respected in Slope and ZeroCross modes
- [ ] `Width` (line thickness) changes visually

### Localization
- [ ] Switch ATAS UI to German (de-de) — all Delta property names/descriptions appear in German
- [ ] Switch to Russian (ru-ru) — same
- [ ] (Optional) Spanish, French, Hindi, Chinese if your ATAS build supports these locales

---

## General regression check

After integration of any Delta change:
- [ ] Existing Delta.cs behavior unchanged on a chart with default settings
- [ ] No exception in ATAS log at indicator attach or detach
- [ ] Recalculate (via settings change) completes without hang or crash

---

## Pending — not yet extracted as individual branches

The following changes from `prready/main` are not yet in dedicated local branches.
They must be tested after extraction and before publication:

| Commit | Area | Description |
|--------|------|-------------|
| `64ed7f58` | delta | fix: clamp price-panel triangle markers to visible region |
| `3e420e5c` | delta | chore: hide internal series from Drawing group |
| `811000d6` | delta | chore: reorganize UI groups and ordering |
| `5fe49e01` | delta | chore: LineWidth for average thickness + UI order change |
| `5f78e83f` | delta | chore: specific divergence descriptions + localization fixes |
| `5e336b28` | delta | chore: delta-specific labels for fixed thresholds |
| `eff7ee3f` | delta | chore: hide and disable legacy fixed-value alerts |
| `0d087ebd` | delta | chore: delta-specific color labels in Drawing group |
