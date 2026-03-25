# Port manifest — MultiMarketPower

**Source:** `prready/main`
**Integration target:** `local/multimarketpower-i18n` (stacked on `local/build/04-localization`)
**Status:** `pending`

---

## Phase 0 — Resource keys required in `local/build/04-localization`

15 new keys needed. All others already exist (`Filters`, `General`, `Session`, `CustomSession`, `CustomSessionStart`, `SignalPeriod`).

| Key | Value (en) | Comment | Used by | Status |
|-----|-----------|---------|---------|--------|
| `ViewMode` | View Mode | UI | ViewMode enum display + property | pending |
| `SessionMode` | Session Mode | UI | SessionMode property | pending |
| `SessionsBack` | Sessions Back | UI | SessionsBack property | pending |
| `DefaultSession` | Default Session | UI | SessionMode enum value | pending |
| `SmartMoneySpread` | Smart Money Spread | MultiMarketPower | ViewMode enum value + group header | pending |
| `Use4ColorSystem` | Use 4-Color System | MultiMarketPower | spread property | pending |
| `Use4ColorSystemDescription` | If true, uses slope-based colors. If false, uses simple zero-line colors. | MultiMarketPower | tooltip | pending |
| `ShowSignalLine` | Show Signal Line (SMA) | MultiMarketPower | spread property | pending |
| `SimplePositiveColor` | Simple Positive Color | MultiMarketPower | spread property | pending |
| `SimpleNegativeColor` | Simple Negative Color | MultiMarketPower | spread property | pending |
| `SimpleColorDescription` | Used only when the 4-Color System is disabled. | MultiMarketPower | tooltip | pending |
| `ColorPosSmaUp` | Positive · SMA ↑ | MultiMarketPower | 4-color property | pending |
| `ColorPosSmaDown` | Positive · SMA ↓ | MultiMarketPower | 4-color property | pending |
| `ColorNegSmaUp` | Negative · SMA ↑ | MultiMarketPower | 4-color property | pending |
| `ColorNegSmaDown` | Negative · SMA ↓ | MultiMarketPower | 4-color property | pending |

---

## Phase 1 — Fix branches (port before feat)

Fix commits must be applied to `local/multimarketpower-i18n` **before** feat content.
Fix-only commits must be applied first; fixes that build on feat state come after their feat.

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `94a06c9b` | Stabilize historical tick cursor iteration | `fix/mmp-history-tick-cursor` | pending | |
| `e393cc28` | Make historical request window deterministic | `fix/mmp-history-request-window` | pending | |
| `31cc814e` | Guard OnCalculate against bar 0 | `fix/mmp-bar0-guard` | pending | simple null/bounds guard |
| `86ea4948` | Replay buffered realtime after history load | `fix/mmp-realtime-replay-buffer` | pending | depends on `0e309fa5` (feat: spread series) |
| `a8cf811d` | Refresh chart correctly when switching view mode | `fix/mmp-view-mode-refresh` | pending | depends on `d77aa581` (feat: view mode) |
| `0aa076bc` | Respect filter visibility toggles in Filters view | `fix/mmp-filter-visibility` | pending | depends on `d77aa581` (feat: view mode) |
| `08cdc973` | Avoid treating every tick as new bar for signal SMA | `fix/mmp-sma-tick-guard` | pending | depends on `994c03a2` (feat: signal SMA) |
| `f4c76d71` | Update rolling SMA on in-bar cumulative trade updates | `fix/mmp-sma-cumulative-update` | pending | depends on `dd28d2f7` (refactor: rolling SMA) |
| `b199694b` | Normalize custom session boundary using instrument timezone | `fix/mmp-session-timezone` | pending | depends on `ffbfbe08` (feat: session controls) |
| `8422d948` | Filter realtime replay to avoid history overlap | `fix/mmp-realtime-overlap` | pending | depends on `ffbfbe08` (feat: session controls) |
| `63c69d84` | Localize view and session mode enum values | `fix/mmp-enum-i18n` | pending | depends on feat: view mode + session controls |

---

## Phase 2 — Feat branches

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `0e309fa5` | Add Smart Money Spread series | `feat/mmp-spread-series` | pending | new histogram series + spread calculation |
| `d77aa581` | Add view mode toggle for Filters vs Spread | `feat/mmp-view-mode` | pending | new ViewMode enum + property |
| `ffbfbe08` | Add session controls and session-based resets | `feat/mmp-session-controls` | pending | new SessionMode enum + CustomSessionStart + SessionsBack |
| `994c03a2` | Add signal SMA and 4-color spread shading | `feat/mmp-signal-sma` | pending | ShowSignalLine + Use4ColorSystem + 8 color properties |

---

## Phase 2b — Refactors (depend on feat state, apply after feat)

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `f07f1eca` | Remove redundant realtime replay from history calculation | `refactor/mmp-history-realtime` | pending | skippable if high integration cost |
| `dd28d2f7` | Compute signal SMA using rolling window | `refactor/mmp-rolling-sma` | pending | needed before `f4c76d71` fix |

---

## Phase 3 — Chore / UI polish

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `3e56472a` | Align default filter line colors/widths with volume semantics | `local/multimarketpower-i18n` | pending | color defaults only |
| `025467ee` | Align default spread colors with market state semantics | `local/multimarketpower-i18n` | pending | color defaults only |
| `38ed1595` | Reorganize UI groups for clearer workflow | `local/multimarketpower-i18n` | pending | Order values + group assignments |

---

## Phase 4 — Integration verification: `local/multimarketpower-i18n`

### 4.1 Build
- [ ] `dotnet build -c Alpha` — 0 errors
- [ ] `dotnet build -c Stable` — 0 errors

### 4.2 Content completeness
- [ ] Phase 1 fixes applied
- [ ] Phase 2 feat branches ported
- [ ] Phase 2b refactors applied
- [ ] Phase 3 chore commits applied

### 4.3 Resource completeness
- [ ] 15 new keys in Resources.resx (en)
- [ ] 15 keys in all 6 satellite locales
- [ ] Resources.Designer.cs up to date

### 4.4 Functional smoke test
- [ ] Indicator loads without crash
- [ ] View mode toggle works (Filters ↔ Smart Money Spread)
- [ ] Smart Money Spread histogram renders
- [ ] Signal SMA line renders when enabled
- [ ] 4-color system works (colors change by SMA slope)
- [ ] Session controls reset state at session boundary
- [ ] No resource key literals visible in UI

---

## Section 5 — Intentional divergences from prready/main

| Area | prready/main behavior | Local behavior | Reason | Revisit? |
|------|-----------------------|----------------|--------|----------|
| _(none identified yet)_ | | | | |

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

| Gap | Severity | Planned fix | Target branch |
|-----|----------|-------------|---------------|
| All 20 commits not yet ported | high | Execute Phases 1-3 in order | `local/multimarketpower-i18n` |
| 15 resource keys missing from `04-localization` | high | Add keys before Phase 2 work | `local/build/04-localization` |
