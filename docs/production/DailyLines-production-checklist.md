# DailyLines — Production Checklist

> Baseline: upstream/<branch>...HEAD  
> Scope: DailyLines + resources + integration smoke  
> Goal: confirm legacy parity + multi-scope correctness + UX sanity + no regressions

## A. Build & Packaging

- [ ] A1. `git status` clean (no local changes)
- [ ] A2. `git fetch upstream --prune` done
- [ ] A3. Clean + Rebuild solution (Debug)
- [ ] A4. Clean + Rebuild solution (Release)
- [ ] A5. ATAS loads indicator assembly without errors (TypeLoad/MissingMethod)
- [ ] A6. No new critical warnings (nullable/resources)

## B. Resources / i18n integrity

- [ ] B1. `Resources.resx` contains all DailyLines keys added/used
- [ ] B2. es-ES contains the same keys
- [ ] B3. fr-FR contains the same keys
- [ ] B4. de-DE contains the same keys
- [ ] B5. ru-RU contains the same keys
- [ ] B6. hi-IN contains the same keys
- [ ] B7. zh-CN contains the same keys
- [ ] B8. `Resources.Designer.cs` exposes properties for new keys (no duplicates)

## C. Legacy parity (UseMultiScope = false)

> Run on: M1 + M5 (recommended also M15)
> Instrument: ES (or your main), regular data source

- [ ] C1. Period=CurrentDay renders exactly as baseline
- [ ] C2. Period=PreviousDay renders exactly as baseline
- [ ] C3. Period=CurrentWeek renders exactly as baseline
- [ ] C4. Period=PreviousWeek renders exactly as baseline
- [ ] C5. Period=CurrentMonth renders exactly as baseline
- [ ] C6. Period=PreviousMonth renders exactly as baseline
- [ ] C7. No ETH-related visuals exist in legacy mode
- [ ] C8. No unexpected "custom session inactive" messages

## D. Multi-scope correctness (UseMultiScope = true)

- [ ] D1. Only CurrentDay ON → renders
- [ ] D2. Only PreviousDay ON → renders
- [ ] D3. Only CurrentWeek ON → renders
- [ ] D4. Only PreviousWeek ON → renders
- [ ] D5. Only CurrentMonth ON → renders
- [ ] D6. Only PreviousMonth ON → renders
- [ ] D7. Day+Week+Month all ON → all render in fixed order
- [ ] D8. Toggle OFF all scopes → safety fallback shows legacy scope (not empty)
- [ ] D9. No duplicated labels when enabling multiple scopes

## E. RTH / CustomSession window (09:30–16:00)

- [ ] E1. CustomSession=true + Start=09:30 + End=16:00 filters the day range correctly
- [ ] E2. CustomSession=false disables the filter and day becomes full (baseline behavior)
- [ ] E3. "Custom session inactive" message appears only when appropriate (does not block unrelated scopes)

## F. TradingDayStart bucketing (18:00 anchor)

- [ ] F1. TradingDayStart=18:00 rotates the day bucket as expected
- [ ] F2. Changing TradingDayStart triggers recalc + redraw immediately
- [ ] F3. PreviousDay skip-empty works (no empty bucket shown when there is no in-window data)
- [ ] F4. Week/Month unaffected by TradingDayStart logic

## G. ETH scope (TradingDayStart → RTH start)

- [ ] G1. ShowEth=true + CustomSession=true renders ETH levels (18:00→09:30)
- [ ] G2. ShowEth=true + CustomSession=false renders no ETH (by design) but does not break
- [ ] G3. ETH does not include 16:00–18:00 (covered only by full-day scope)
- [ ] G4. ETH does not change CurrentDay/PreviousDay results

## H. HalfGap (Option A)

- [ ] H1. ShowHalfGap=true + CustomSession=true shows HalfGap for CurrentDay
- [ ] H2. ShowHalfGap=true + CustomSession=false does not show HalfGap
- [ ] H3. HalfGap never appears on ETH / Week / Month
- [ ] H4. Enabling ShowHalfGap triggers redraw immediately (no restart needed)
- [ ] H5. HalfGap aligns to the rendered range start bar (no offset)

## I. Render stability & bounds

- [ ] I1. Horizontal scroll does not throw exceptions
- [ ] I2. Zoom in/out does not throw exceptions
- [ ] I3. Vertical scale change does not throw exceptions
- [ ] I4. Text stays within chart bounds (DrawPrice guard)
- [ ] I5. No flicker / no label duplication after repeated parameter changes

## J. Realtime behavior (if applicable)

- [ ] J1. Live bar updates keep OHLC correct
- [ ] J2. New trading day transition (18:00) archives current and shifts previous correctly
- [ ] J3. New RTH start handling does not mis-archive ETH
- [ ] J4. No performance degradation when all scopes enabled

## K. Repo smoke test (not DailyLines)

- [ ] K1. Load 2–3 unrelated indicators successfully
- [ ] K2. No global resource load errors on startup
- [ ] K3. No runtime exceptions in log during normal use