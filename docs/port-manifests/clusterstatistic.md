# Port manifest — ClusterStatistic

**Source:** `prready/main`
**Integration target:** `local/clusterstatistic-i18n` (stacked on `local/build/04-localization`)
**Status:** `smoke-test-pending`

---

## Pre-port evaluation

### SE perspective
- 1937-line indicator; well-structured with `#region` blocks.
- Commits add two orthogonal feature areas: Speed-of-Tape (SoT) metrics and cluster imbalances (buy/sell/net/stacked).
- `7e78c702` is a standalone, independently correct fix (copy-paste bug: `maxBid` was reading `candle.Ask`; also removes unnecessary `Math.Abs` on a signed ratio).
- `9e656e71` converts hardcoded/typeof(Strings) display attributes to `typeof(Resources)`. Three subsequent commits (`36658d0b`, `0b645e2f`, `e0046c0b`) also introduce or carry over `typeof(Resources)` references. **This is the main adaptation challenge** for the Develop-based feat branch.
- `d6da874c` and `7faf4dc8` are docs-only commits (ARCHITECTURE.md, process docs) — no code changes.

### Discretionary trader / algo trader perspective
Both feature areas add high-value scalping signals:
- SoT Delta/sec and peak metrics reveal order flow acceleration in real time.
- Imbalance rows (buy/sell/net + stacked) expose aggressive one-sided activity at price levels.
- Net imbalance threshold alert with closed-candle mode is immediately useful for systematic signals.

### Intentional additions/deviations
None identified — all commits ported as-is, with Develop-branch adaptation for Resources.

---

## Skip (docs-only, no code impact)

| Commit | Files | Reason |
|--------|-------|--------|
| `d6da874c` | ARCHITECTURE.md, CONTRIBUTING.md | Repo-root docs only |
| `7faf4dc8` | docs/*.md | Process docs only |

---

## Phase 1 — Evaluation + Manifest

Evaluation complete. Manifest created as part of port planning.

---

## Phase 2 — Standalone branches (Develop-based)

**Rules:**
- Base: `git checkout -B <branch> Develop`
- No `typeof(Resources)` / no `using ATAS.Indicators.Technical.Properties`.
- Platform keys (already in `Strings`) use `typeof(Strings)`. New CS-specific keys: hardcode English string.
- **Build note:** MC3074 XAML errors are environmental — verify C# logic by review.

### Fix branches

| Branch | Develop-based commit | Base | Status |
|--------|---------------------|------|--------|
| `fix/cs-statistic-maxbid` | `e6b262f3` | `Develop` | done |

`fix/cs-statistic-maxbid` ports **`7e78c702`**:
- `_maxBid = Math.Max(candle.Bid, _maxBid)` — was erroneously using `candle.Ask` (copy-paste bug)
- `maxBid = Math.Max(candle.Bid, maxBid)` inside the full rebuild loop — same bug
- `_deltaPerVol[bar]` ratio: removes unnecessary `Math.Abs` (ratio is signed by definition)

### Feat branches

| Branch | HEAD commit | Stacked on | Status |
|--------|------------|------------|--------|
| `feat/cs-statistic` | `b618cc0a` | `Develop` | done |

`feat/cs-statistic` contains **27 commits** (all prready/main feat+refactor+fix commits except `9e656e71` which is split — see Phase 2b).

---

## Phase 3 — Resource keys required in `local/build/04-localization`

**74 net new keys** from 5 commits:

| Commit | Action | Files | Notes | Status |
|--------|--------|-------|-------|--------|
| `d32ea792` | +49 keys | All 7 locales (no Designer.cs) | Main batch: SoT + imbalance keys | done (`7868c4a1`) |
| `ca44800c` | +3 keys | All 7 locales + Designer.cs | ShowDeltaPerSecond, NetStkShort, LabelFillColorDescription | done (`bfdb684c`) |
| `3c5546d3` | value fixes + removes 3 keys | All 7 locales | Removes SotUseAutoFilterName, SotAutoFilterPeriodName, SotAutoFilterUseEmaName (superseded by 47457e7c) | done (`50e306b0`) |
| `47457e7c` | +25 keys | All 7 locales + Designer.cs | Re-adds the 3 Sot keys with corrected values + 22 description keys | done (`c4346781`) |
| `eee746cb` | value corrections only | All 7 locales + Designer.cs | Corrects DeltaAlert templates (no new keys) | done (`b8207ba7`) |

### New keys (74 total after all 5 commits applied)

**SoT / Pace rows (10):** `ShowDeltaPerSecond`, `ShowDeltaPerSecondDescription`, `ShowPeakVolPerSec`, `ShowPeakDeltaPerSec`,
`ShowPeakDeltaPerVol`, `MaxVolPerSecGroup`, `SotTimeWindowSecName`, `SotMinVolumeName`,
`SotUseAutoFilterName`, `SotAutoFilterPeriodName`, `SotAutoFilterUseEmaName`

**Imbalance rows (10):** `ShowBuyImbalances`, `ShowSellImbalances`, `ShowNetImbalances`,
`ShowStackedBuyImbalances`, `ShowStackedSellImbalances`, `ShowStackedNetImbalances`,
`ImbalanceGroup`, `ImbalanceThresholdPercentName`, `ImbalanceMinDominantVolumeName`, `ImbalanceMinDifferenceName`,
`StackedImbalanceMinLevelsName`

**Net imbalance alert (5):** `NetImbalanceAlertGroup`, `NetImbalanceAlertThresholdAbs`,
`NetImbalanceAlertUseClosedCandle`, `AlertNetImbalanceTemplate`, `AlertDeltaTemplate`, `AlertVolumeTemplate`

**Row subgroups (6):** `RowsPaceGroup`, `RowsPressureGroup`, `RowsImbalanceRowsGroup`,
`RowsCandleContextGroup`, `RowsSessionContextGroup`, `RowsRawPrintsGroup`

**Display extras (7):** `RatiosAsPercent`, `NetStkShort`, `LabelFillColorDescription`,
`DeltaVolume`, `DeltaVolumePercent`, `AlertPaceVolPerSecGroup`, `DefaultSession`

**Description keys (25):** `ShowVolumePerSecondDescription`, `ShowDeltaDescription`, `ShowDeltaPerVolumeDescription`,
`ShowDeltaChangeDescription`, `ShowMaximumDeltaDescription`, `ShowMinimumDeltaDescription`,
`ShowVolumesDescription`, `ShowTradesCountDescription`, `ShowCandleHeightDescription`,
`ShowCandleDurationDescription`, `ShowCandleTimeDescription`, `ShowSessionVolumeDescription`,
`ShowSessionDeltaDescription`, `ShowSessionDeltaPerVolumeDescription`, `ShowAsksDescription`,
`ShowBidsDescription` + `AlertFileDescription`, `AlertFilterDescription`, `UseAlertDescription`,
`CustomSessionStartDescription`, `SessionCumModeDescription`, `AlertCandleHeightGroup`,
`AlertCandleTradesGroup`, `AlertCandleVolumeGroup`, `AlertPressure*Group`, etc.

---

## Phase 4 — Integration branch: `local/clusterstatistic-i18n`

### Fix commits

No prready/main fixes are standalone (all depend on new features added in Phase 2).

### Phase 1 — local-only fix (not in prready/main)

| Description | Local branch | Status | Notes |
|-------------|--------------|--------|-------|
| Correct maxBid copy-paste bug + delta/vol signed ratio | `fix/cs-statistic-maxbid` | pending | Upstream-submittable; ports `7e78c702` |

---

### Feat commits

All 28 remaining commits form a single tightly-coupled stack: `feat/cs-statistic`.

### Phase 2a — SoT metrics (commits 1–6)

| Commit (prready/main) | Description | Status |
|----------------------|-------------|--------|
| `876d89ab` | Add Delta/sec row with scaling and formatting | done |
| `40b7e7d0` | Add peak Vol/sec and paired Delta rows (plumbing) | done |
| `96bf0433` | Compute SoT peak Vol/sec and paired Delta from cumulative trades | done |
| `d702f6b1` | Fix: seed SoT live window in chronological order | done |
| `64bbfb69` | Add SoT auto-filter scaling (EMA/SMA) for peak metrics | done |
| `a5eeea28` | Add Peak Delta/Vol row derived from SoT peaks | done |

### Phase 2b — Ratio display + table readability (commits 7–9)

| Commit (prready/main) | Description | Status |
|----------------------|-------------|--------|
| `72927be0` | Refactor: add ratio formatting helper | done |
| `ccd84094` | Unify ratio formatting and optional percent display | done |
| `669956e3` | Refactor: improve table readability with contrast text and high-rate outlines | done |

### Phase 2c — Imbalances (commits 10–12)

| Commit (prready/main) | Description | Status |
|----------------------|-------------|--------|
| `4903d8e2` | Add buy/sell/net imbalance rows | done |
| `634055f0` | Add net imbalance threshold alert (crossing, no-spam) | done |
| `6151b7c2` | Add stacked imbalances rows (consecutive levels) | done |

### Phase 2d — Settings refactor + fixes (commits 13–15, plus pre-existing Revert)

| Commit (prready/main) | Description | Status |
|----------------------|-------------|--------|
| `8ff73a47` | Revert "CumulativeDalta lining refactoring" | done |
| `69b6cadd` | Reorder default rows for faster scalping reads | done |
| `8185c11a` | Reorganize settings and row toggles for scalping workflow | done |
| `7e4a5932` | Fix: correct net imbalance alert crossing (closed candle) | done |
| `e9708c76` | Perf: gate imbalance computation and rebuild on enable/params change | done |

### Phase 2e — i18n refactor (adaptation required on Develop branch)

⚠️ **Adaptation note:** `9e656e71` converts all Display attributes to `typeof(Resources)`. Three later commits
(`36658d0b`, `0b645e2f`, `e0046c0b`) also introduce `typeof(Resources)` references.

**Strategy for Develop-based `feat/cs-statistic`:**
- `9e656e71`: apply **adapted** — keep `typeof(Strings)` for 21 existing platform keys; keep hardcoded English strings for the 53+ new CS-specific keys. Do NOT add `using ATAS.Indicators.Technical.Properties`.
- `36658d0b`, `0b645e2f`, `e0046c0b`: apply with same adaptation (replace `typeof(Resources)` inline with hardcoded/`typeof(Strings)` as appropriate).

**Strategy for `local/cs-statistic-i18n` (integration):**
- Cherry-pick from `feat/cs-statistic` and additionally apply `9e656e71` (and the Resource references in `36658d0b`, `0b645e2f`, `e0046c0b`) using `typeof(Resources)`.

| Commit (prready/main) | Description | Adaptation | Status |
|----------------------|-------------|-----------|--------|
| `9e656e71` | Replace hardcoded UI strings with Resources | Adapted on feat branch (`f8e76772` + `40cc7917`) | done |
| `36658d0b` | Reorganize Rows into logical UI subgroups | Adapted on feat branch (`459d1cb3` + `b618cc0a`) | done |
| `b18066ff` | Fix: rebuild imbalances immediately on parameter changes | None | done |
| `18add1c8` | Fix: use full-range MAX for non-visible scaling | None | done |
| `14a9e105` | Fix: make closed net-imbalance alert deterministic | None | done |
| `e5bf859f` | Refactor: centralize imbalance compute gating | None | done |
| `4c5a8b06` | Fix: reset SoT runtime window on bar change | None | done |
| `0b645e2f` | Chore: align UI order comments with actual property order | Adapted on feat branch | done |
| `3e040bed` | Refactor: reuse imbalance series writer in historical rebuild | None | done |
| `876edebb` | Fix: use absolute max for net imbalance scaling | None | done |
| `b372300b` | Chore: normalize indentation and remove duplicate comments | None | done |
| `e0046c0b` | Fix: use auto-filter mean fallback for live bar scaling | Adapted on feat branch | done |

---

### Dependent fixes/refactors

Already included above in Phase 2d and 2e.

---

### Chore / UI polish

Already included in Phase 2e (`b372300b`, `0b645e2f`).

---

## Phase 5 — Smoke test

### 4.1 Build
- [x] `dotnet build` — 0 C# errors (MC1000 XAML error is environmental DevExpress only)

### 4.2 Content completeness
- [x] Phase 1 fix applied: `fix/cs-statistic-maxbid` → `2446c814`
- [x] Phase 2 feat branch applied: 29 commits cherry-picked, HEAD `71d1f29c`

### 4.3 Resource completeness
- [x] 74 new keys in Resources.resx (en)
- [x] 74 keys in all 6 satellite locales
- [x] Resources.Designer.cs up to date

### 4.4 Functional smoke test (manual, ATAS Platform)
- [ ] Indicator loads without crash
- [ ] Delta/sec row visible when enabled
- [ ] Peak Vol/sec row displays and scales correctly
- [ ] Buy/Sell/Net imbalance rows work
- [ ] Stacked imbalance rows appear on relevant bars
- [ ] Net imbalance alert fires when threshold crossed
- [ ] Closed-candle alert mode is deterministic
- [ ] Percent display mode (RatiosAsPercent) toggles correctly

---

## Phase 6 — Documentation

- [ ] `publish_stable_MyIndicators` docs updated to reflect final behavior
- [ ] Manifest marked `complete`

---

## Section 5 — Intentional divergences from prready/main

None. All logic ported as-is; only the typeof(Resources) → hardcoded adaptation differs on Develop-based branches (by design).

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

All gaps resolved. Smoke test pending (4.4).
