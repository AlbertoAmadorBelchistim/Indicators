# Port manifest — ClusterSearch

**Source:** `prready/main`
**Integration target:** `local/clustersearch-i18n` (stacked on `local/build/04-localization`)
**Status:** `smoke-test-pending`

---

## Phase -1 — Standalone branches (Develop-based)

**Rules:**
- Base: `git checkout -B <branch> Develop`
- `typeof(Resources)` must NOT be used for keys that don't exist in the platform `Strings` class on `Develop`. Hardcode English strings instead.
- Keys available in `Strings` on `Develop`: all keys used in existing `ClusterSearch.cs` properties (Bid, Ask, Delta, Volume, Ticks, PocLevel, etc.).
- Keys that must be hardcoded: all DiagonalImbalance-specific keys (see Phase 0 table).
- **`using ATAS.Indicators.Technical.Properties`** must NOT appear in Develop-based branches.
- **Build note:** `dotnet build -c Alpha` may fail on `Develop`-based branches with MC1000. Environmental issue — verify by review.

### Fix branches (Develop-based)

| Branch | Develop-based commit | Base | Status |
|--------|---------------------|------|--------|
| `fix/cs-dead-fields` | `7a3599f3` | `Develop` | done |

### Feat branches (stacked from Develop)

| Branch | HEAD commit | Stacked on | Status |
|--------|------------|------------|--------|
| `feat/cs-diagonal-imbalance` | `4e2c492d` | `Develop` | done |

Commits in `feat/cs-diagonal-imbalance`:

| Local SHA | Source (prready/main) | Description |
|-----------|----------------------|-------------|
| `bceb5cec` | `81b6ffde` | Pin CalcMode values + add DiagonalImbalance enum + color override field |
| `b883fc60` | `6ad637b91` | Support DiagonalImbalance in SeriesHandling |
| `746d4d52` | `4313ce4a` | Add diagonal imbalance filters and detection logic |
| `45e16993` | `255d8e77` | Wire diagonal imbalance descriptions into UI attributes |
| `4e2c492d` | — (intentional addition) | CalcType setter restores filters on DI exit; hardcode DI strings (no typeof(Resources)) |

---

## Phase 0 — Resource keys required in `local/build/04-localization`

17 new keys needed. All from commits `b0d115ff` (UI labels) and `2f1d5de8` (descriptions).

### UI labels (`b0d115ff`)

| Key | Value (en) | Used by | Status |
|-----|-----------|---------|--------|
| `DiagonalImbalance` | Diagonal Imbalance | CalcMode enum + tooltip | done |
| `DiagonalImbalancesFilters` | Diagonal Imbalance Filters | group name | done |
| `ImbalancesVisualization` | Imbalances Visualization | group name | done |
| `ImbalanceRatio` | Imbalance Ratio | ImbalanceRatio property | done |
| `MinVolumeDifference` | Minimum Volume Difference | MinVolumeDifference property | done |
| `MinDominantVolume` | Minimum Dominant Volume | MinDominantVolume property | done |
| `ImbalanceStackedRange` | Imbalance Stacked Range | ImbalanceStackedRange property | done |
| `UseSeparateColors` | Use Separate Colors | UseSeparateColors property | done |
| `BuyImbalanceColor` | Buy Imbalance Color | BuyImbalanceColor property | done |
| `SellImbalanceColor` | Sell Imbalance Color | SellImbalanceColor property | done |

### Descriptions (`2f1d5de8`)

| Key | Value (en) | Used by | Status |
|-----|-----------|---------|--------|
| `ImbalanceRatioDescription` | Minimum dominance ratio between aggressor and passive side required to validate a diagonal imbalance. | ImbalanceRatio Description | done |
| `MinVolumeDifferenceDescription` | Minimum absolute volume difference between aggressor and passive side required for confirmation. | MinVolumeDifference Description | done |
| `MinDominantVolumeDescription` | Minimum traded volume on the dominant side required to mark an imbalance. | MinDominantVolume Description | done |
| `ImbalanceStackedRangeDescription` | Number of consecutive imbalance windows required to confirm a stacked imbalance. | ImbalanceStackedRange Description | done |
| `UseSeparateColorsDescription` | Color diagonal imbalances by direction (buy vs sell). | UseSeparateColors Description | done |
| `BuyImbalanceColorDescription` | Color used for buy-side diagonal imbalances. | BuyImbalanceColor Description | done |
| `SellImbalanceColorDescription` | Color used for sell-side diagonal imbalances. | SellImbalanceColor Description | done |

**Note:** `ImbalanceRatio` key already existed in `Resources.Designer.cs` (from OHLCPlus ~line 23482); duplicate Designer.cs property was not added. resx key was added normally.

Resource commit on `local/build/04-localization`: `7c9e008d` (manually inserted to avoid conflict with pre-existing MMP/Delta keys).

---

## Phase 1 — Fix branches (port before feat)

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `9867dfd5` | Correct MaxAverageTrade filter comparison | — | skipped | Already absorbed by PLAT-3831 (`983fc8dc`) in Develop |

### Phase 1 — local-only fix (not in prready/main)

| Description | Local branch | Status | Notes |
|-------------|--------------|--------|-------|
| Remove dead fields (`_pocPrice`, `_pocVolume`, `Transparency`) | `fix/cs-dead-fields` | done | Upstream-submittable cleanup; `7a3599f3` |

---

## Phase 2 — Feat branches

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `81b6ffde` | Pin CalcMode values + add DiagonalImbalance enum + PriceSelectionColor field | `feat/cs-diagonal-imbalance` | done (`bceb5cec`) | base commit of feature |
| `6ad637b91` | Support DiagonalImbalance in SeriesHandling | `feat/cs-diagonal-imbalance` | done (`b883fc60`) | commit 2 of 4 |
| `4313ce4a` | Add diagonal imbalance filters and detection logic | `feat/cs-diagonal-imbalance` | done (`746d4d52`) | commit 3 of 4; adds CalcType setter DI guard |
| `255d8e77` | Wire diagonal imbalance descriptions into UI attributes | `feat/cs-diagonal-imbalance` | done (`45e16993`) | commit 4 of 4 |

### Intentional addition not in prready/main

| Description | Applied in | Notes |
|-------------|-----------|-------|
| Restore AutoFilter/MinimumFilter/MaximumFilter when switching away from DiagonalImbalance | `feat/cs-diagonal-imbalance` | CalcType setter disables them on DI but never restores; UX fix |

---

## Phase 2b — Dependent fixes/refactors

None identified.

---

## Phase 3 — Chore / UI polish

None identified.

---

## Phase 4 — Integration verification: `local/clustersearch-i18n`

### 4.1 Build
- [x] `dotnet build` — 0 C# errors (MC3074 XAML errors are environment-only, expected on all branches)

### 4.2 Content completeness
- [x] Phase 1 fix applied (cherry-pick `86c3ca78`)
- [x] Phase 2 feat branch applied (cherry-pick `72848945`)

### 4.3 Resource completeness
- [x] 17 new keys in Resources.resx (en) — commit `7c9e008d`
- [x] 17 keys in all 6 satellite locales (de-de, ru-ru, fr-fr, hi-in, zh-cn, es-ES)
- [x] Resources.Designer.cs up to date (16 new properties; ImbalanceRatio already existed)

### 4.4 Functional smoke test (manual, ATAS Platform)
- [ ] Indicator loads without crash
- [ ] CalcType = DiagonalImbalance shows DI filter group in UI
- [ ] Stacked imbalances detected and highlighted
- [ ] Buy/Sell separate colors work when UseSeparateColors = true
- [ ] Switching CalcType away from DI restores AutoFilter/MinFilter controls
- [ ] fix/cs-dead-fields: removed properties not visible in UI

---

## Section 5 — Intentional divergences from prready/main

| Area | prready/main behavior | Local behavior | Reason | Revisit? |
|------|-----------------------|----------------|--------|----------|
| CalcType setter | Disables AutoFilter/Min/Max on DI; never restores | Restores on switch away from DI | Bug fix / UX improvement | No |

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

| Gap | Severity | Planned fix | Target branch |
|-----|----------|-------------|---------------|
| Smoke test not yet run | medium | Manual test in ATAS Platform | — |
