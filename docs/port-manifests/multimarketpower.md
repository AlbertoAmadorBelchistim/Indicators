# Port manifest — MultiMarketPower

**Source:** `prready/main`
**Integration target:** `local/multimarketpower-i18n` (stacked on `local/build/04-localization`)
**Status:** `smoke-test-pending`

---

## Phase 1 — Evaluation + Manifest

Evaluation complete. Manifest created as part of port planning.

---

## Phase 2 — Standalone branches (Develop-based)

Each fix and feat **must have its own standalone branch rooted at `Develop`**, independent of the integration branch and the localization infrastructure branch. This allows individual PRs to be reviewed without requiring the full localization stack.

**Rules:**
- Base: `git checkout -B <branch> Develop`
- `typeof(Resources)` **must not be used** for keys that don't exist in the platform `Strings` class on `Develop`. Use hardcoded English strings instead.
- Keys available in `Strings` on `Develop`: `Visualization`, `SignalPeriod`, `Session`, `CustomSessionStart` (and all keys used by other existing indicators).
- Keys that must be hardcoded: `ViewMode`, `SessionMode`, `SessionsBack`, `DefaultSession`, `SmartMoneySpread`, `Use4ColorSystem`, `Use4ColorSystemDescription`, `ShowSignalLine`, `SimplePositiveColor`, `SimpleNegativeColor`, `SimpleColorDescription`, `ColorPosSmaUp`, `ColorPosSmaDown`, `ColorNegSmaUp`, `ColorNegSmaDown`.
- **Build note:** `dotnet build -c Alpha` on `Develop`-based branches may fail with MC1000 (DevExpress XAML assembly error). This is an environmental issue on `Develop` unrelated to C# changes. Verify logic review instead.

### Fix branches (Develop-based)

| Branch | Develop-based commit | Base | Status |
|--------|---------------------|------|--------|
| `fix/mmp-history-tick-cursor` | `2ddf2f9e` | `Develop` | done |
| `fix/mmp-history-request-window` | `db0a9615` | `Develop` | done |
| `fix/mmp-bar0-guard` | `b2473291` | `Develop` | done |
| `fix/mmp-realtime-replay-buffer` | `c4797b17` | `feat/mmp-session-controls` | done |
| `fix/mmp-view-mode-refresh` | `7d33332e` | `feat/mmp-signal-sma` | done |
| `fix/mmp-filter-visibility` | `b522f594` | `feat/mmp-signal-sma` | done |
| `fix/mmp-session-timezone` | `a3feed58` | `feat/mmp-session-controls` | done |
| `fix/mmp-enum-i18n` | `19e7b0dc` | `feat/mmp-signal-sma` | done |
| `fix/mmp-sma-tick-guard` | `ecc82c12` | `feat/mmp-signal-sma` + `refactor/mmp-rolling-sma` | done |
| `fix/mmp-realtime-overlap` | `c991604a` | `feat/mmp-session-controls` + `refactor/mmp-rolling-sma` | done |
| `fix/mmp-sma-cumulative-update` | `2e8201f4` | `refactor/mmp-rolling-sma` | done |

### Feat branches (stacked from Develop)

Feat branches are stacked because they share code dependencies. All ultimately have `Develop` as ancestor.

| Branch | Develop-based commit | Stacked on | Status |
|--------|---------------------|------------|--------|
| `feat/mmp-spread-series` | `8b56d6d7` | `Develop` | done |
| `feat/mmp-view-mode` | `d0ac758a` | `feat/mmp-spread-series` | done |
| `feat/mmp-session-controls` | `7d74b6ef` | `feat/mmp-view-mode` | done |
| `feat/mmp-signal-sma` | `c5efe05a` | `feat/mmp-session-controls` | done |

---

## Phase 3 — Resource keys required in `local/build/04-localization`

15 new keys needed. All others already exist (`Filters`, `General`, `Session`, `CustomSession`, `CustomSessionStart`, `SignalPeriod`).

| Key | Value (en) | Comment | Used by | Status |
|-----|-----------|---------|---------|--------|
| `ViewMode` | View Mode | UI | ViewMode enum display + property | done (b0b56ba1) |
| `SessionMode` | Session Mode | UI | SessionMode property | done (b0b56ba1) |
| `SessionsBack` | Sessions Back | UI | SessionsBack property | done (b0b56ba1) |
| `DefaultSession` | Default Session | UI | SessionMode enum value | done (b0b56ba1) |
| `SmartMoneySpread` | Smart Money Spread | MultiMarketPower | ViewMode enum value + group header | done (b0b56ba1) |
| `Use4ColorSystem` | Use 4-Color System | MultiMarketPower | spread property | done (b0b56ba1) |
| `Use4ColorSystemDescription` | If true, uses slope-based colors. If false, uses simple zero-line colors. | MultiMarketPower | tooltip | done (b0b56ba1) |
| `ShowSignalLine` | Show Signal Line (SMA) | MultiMarketPower | spread property | done (b0b56ba1) |
| `SimplePositiveColor` | Simple Positive Color | MultiMarketPower | spread property | done (b0b56ba1) |
| `SimpleNegativeColor` | Simple Negative Color | MultiMarketPower | spread property | done (b0b56ba1) |
| `SimpleColorDescription` | Used only when the 4-Color System is disabled. | MultiMarketPower | tooltip | done (b0b56ba1) |
| `ColorPosSmaUp` | Positive · SMA ↑ | MultiMarketPower | 4-color property | done (b0b56ba1) |
| `ColorPosSmaDown` | Positive · SMA ↓ | MultiMarketPower | 4-color property | done (b0b56ba1) |
| `ColorNegSmaUp` | Negative · SMA ↑ | MultiMarketPower | 4-color property | done (b0b56ba1) |
| `ColorNegSmaDown` | Negative · SMA ↓ | MultiMarketPower | 4-color property | done (b0b56ba1) |

---

## Phase 4 — Integration branch: `local/multimarketpower-i18n`

### Fix commits

Fix commits must be applied to `local/multimarketpower-i18n` **before** feat content.
Fix-only commits must be applied first; fixes that build on feat state come after their feat.

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `94a06c9b` | Stabilize historical tick cursor iteration | `fix/mmp-history-tick-cursor` | done (98ca4ebb) | |
| `e393cc28` | Make historical request window deterministic | `fix/mmp-history-request-window` | done (1272f030) | |
| `31cc814e` | Guard OnCalculate against bar 0 | `fix/mmp-bar0-guard` | done (72804d95) | simple null/bounds guard |
| `86ea4948` | Replay buffered realtime after history load | `fix/mmp-realtime-replay-buffer` | done (3ea4cb6b) | base: `feat/mmp-session-controls`; `_pendingRealtimeReplay`/`ReplayBufferedRealtimeAfterHistory` already in `f3ba2b8d`, conflicts resolved |
| `a8cf811d` | Refresh chart correctly when switching view mode | `fix/mmp-view-mode-refresh` | done (45725b77) | base: `feat/mmp-signal-sma` |
| `0aa076bc` | Respect filter visibility toggles in Filters view | `fix/mmp-filter-visibility` | done (b2cdf11d) | base: `feat/mmp-signal-sma` |
| `08cdc973` | Avoid treating every tick as new bar for signal SMA | `fix/mmp-sma-tick-guard` | done (88c9381b) | depends on `dd28d2f7` (Phase 2b rolling SMA refactor) |
| `f4c76d71` | Update rolling SMA on in-bar cumulative trade updates | `fix/mmp-sma-cumulative-update` | done (3e933cc0) | depends on `dd28d2f7` (refactor: rolling SMA) |
| `b199694b` | Normalize custom session boundary using instrument timezone | `fix/mmp-session-timezone` | done (13c7f380) | base: `feat/mmp-session-controls`; spaces→tabs conflict in `IsSessionStart` resolved |
| `8422d948` | Filter realtime replay to avoid history overlap | `fix/mmp-realtime-overlap` | done (3b80c3ad) | depends on `86ea4948` + `dd28d2f7` |
| `63c69d84` | Localize view and session mode enum values | `fix/mmp-enum-i18n` | done (bab715b6) | base: `feat/mmp-signal-sma`; `Filters`+`CustomSession`→`typeof(Strings)`, others hardcoded |

---

### Feat commits

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `0e309fa5` | Add Smart Money Spread series | `feat/mmp-spread-series` | done (dd87823e) | new histogram series + spread calculation |
| `d77aa581` | Add view mode toggle for Filters vs Spread | `feat/mmp-view-mode` | done (3ece00fe) | new ViewMode enum + property |
| `ffbfbe08` | Add session controls and session-based resets | `feat/mmp-session-controls` | done (1c66f347) | fixed tab indentation in CalculateTick |
| `994c03a2` | Add signal SMA and 4-color spread shading | `feat/mmp-signal-sma` | done (f3ba2b8d) | added `_currentSessionBegin` to fix SMA session boundary |

---

### Fixes and refactors (depend on feat/Phase 1 state)

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `dd28d2f7` | Compute signal SMA using rolling window | `refactor/mmp-rolling-sma` | done (fc89cc5d) | genuine perf refactor; enables `08cdc973`, `8422d948`, `f4c76d71` |
| `f07f1eca` | Remove double-replay in history calculation | `fix/mmp-history-realtime-duplicate` | done (232589ae) | was `refactor/` — actually a bug fix; requires `86ea4948` applied first |

---

### Chore / UI polish

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `3e56472a` | Align default filter line colors/widths with volume semantics | `local/multimarketpower-i18n` | done (3c748ea3) | color defaults only |
| `025467ee` | Align default spread colors with market state semantics | `local/multimarketpower-i18n` | done (6047bb14) | color defaults only |
| `38ed1595` | Reorganize UI groups for clearer workflow | `local/multimarketpower-i18n` | done (ffb726f4) | Order values + group assignments |

---

## Phase 5 — Smoke test

### 4.1 Build
- [x] `dotnet build -c Alpha` — 0 errors (verified after Phase 2b)

### 4.2 Content completeness
- [x] Phase 1 fixes applied
- [x] Phase 2 feat branches ported
- [x] Phase 2b fixes/refactors applied
- [x] Phase 3 chore commits applied

### 4.3 Resource completeness
- [x] 15 new keys in Resources.resx (en)
- [x] 15 keys in all 6 satellite locales
- [x] Resources.Designer.cs up to date

### 4.4 Functional smoke test (manual, ATAS Platform)
- [ ] Indicator loads without crash
- [ ] View mode toggle works (Filters ↔ Smart Money Spread)
- [ ] Smart Money Spread histogram renders
- [ ] Signal SMA line renders when enabled
- [ ] 4-color system works (colors change by SMA slope)
- [ ] Session controls reset state at session boundary
- [ ] No resource key literals visible in UI

---

## Phase 6 — Documentation

- [ ] `publish_stable_MyIndicators` docs updated to reflect final behavior
- [ ] Manifest marked `complete`

---

## Section 5 — Intentional divergences from prready/main

| Area | prready/main behavior | Local behavior | Reason | Revisit? |
|------|-----------------------|----------------|--------|----------|
| SMA session boundary | `Math.Max(0, bar - period + 1)` — SMA includes bars from previous sessions after reset | Added `_currentSessionBegin` field; `UpdateSpreadVisuals` clamps start to `_currentSessionBegin`; `ResetSessionState` updates it | Bug fix: cross-session contamination of signal SMA | No — correct behavior |

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

| Gap | Severity | Planned fix | Target branch |
|-----|----------|-------------|---------------|
| ~~All 20 commits not yet ported~~ | ~~high~~ | ~~Execute Phases 1-3 in order~~ | ~~done~~ |
| ~~15 resource keys missing from `04-localization`~~ | ~~high~~ | ~~Add keys before Phase 2 work~~ | ~~done b0b56ba1~~ |
