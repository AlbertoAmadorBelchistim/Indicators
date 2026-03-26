# Port manifest — ClusterSearch

**Source:** `prready/main`
**Integration target:** `local/clustersearch-i18n` (stacked on `local/build/04-localization`)
**Status:** `in-progress`

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
| `fix/cs-dead-fields` | TBD | `Develop` | pending |

### Feat branches (stacked from Develop)

| Branch | Develop-based commit | Stacked on | Status |
|--------|---------------------|------------|--------|
| `feat/cs-diagonal-imbalance` | TBD | `Develop` | pending |

---

## Phase 0 — Resource keys required in `local/build/04-localization`

17 new keys needed. All from commits `b0d115ff` (UI labels) and `2f1d5de8` (descriptions).

### UI labels (`b0d115ff`)

| Key | Value (en) | Used by | Status |
|-----|-----------|---------|--------|
| `DiagonalImbalance` | Diagonal Imbalance | CalcMode enum + tooltip | pending |
| `DiagonalImbalancesFilters` | Diagonal Imbalance Filters | group name | pending |
| `ImbalancesVisualization` | Imbalances Visualization | group name | pending |
| `ImbalanceRatio` | Imbalance Ratio | ImbalanceRatio property | pending |
| `MinVolumeDifference` | Minimum Volume Difference | MinVolumeDifference property | pending |
| `MinDominantVolume` | Minimum Dominant Volume | MinDominantVolume property | pending |
| `ImbalanceStackedRange` | Imbalance Stacked Range | ImbalanceStackedRange property | pending |
| `UseSeparateColors` | Use Separate Colors | UseSeparateColors property | pending |
| `BuyImbalanceColor` | Buy Imbalance Color | BuyImbalanceColor property | pending |
| `SellImbalanceColor` | Sell Imbalance Color | SellImbalanceColor property | pending |

### Descriptions (`2f1d5de8`)

| Key | Value (en) | Used by | Status |
|-----|-----------|---------|--------|
| `ImbalanceRatioDescription` | Minimum dominance ratio between aggressor and passive side required to validate a diagonal imbalance. | ImbalanceRatio Description | pending |
| `MinVolumeDifferenceDescription` | Minimum absolute volume difference between aggressor and passive side required for confirmation. | MinVolumeDifference Description | pending |
| `MinDominantVolumeDescription` | Minimum traded volume on the dominant side required to mark an imbalance. | MinDominantVolume Description | pending |
| `ImbalanceStackedRangeDescription` | Number of consecutive imbalance windows required to confirm a stacked imbalance. | ImbalanceStackedRange Description | pending |
| `UseSeparateColorsDescription` | Color diagonal imbalances by direction (buy vs sell). | UseSeparateColors Description | pending |
| `BuyImbalanceColorDescription` | Color used for buy-side diagonal imbalances. | BuyImbalanceColor Description | pending |
| `SellImbalanceColorDescription` | Color used for sell-side diagonal imbalances. | SellImbalanceColor Description | pending |

---

## Phase 1 — Fix branches (port before feat)

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `9867dfd5` | Correct MaxAverageTrade filter comparison | — | skipped | Already absorbed by PLAT-3831 (`983fc8dc`) in Develop |

### Phase 1 — local-only fix (not in prready/main)

| Description | Local branch | Status | Notes |
|-------------|--------------|--------|-------|
| Remove dead fields (`_pocPrice`, `_pocVolume`, `Transparency`) and obsolete `CalcMode.Time` | `fix/cs-dead-fields` | pending | Upstream-submittable cleanup |

---

## Phase 2 — Feat branches

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|
| `81b6ffde` | Pin CalcMode values + add DiagonalImbalance enum + PriceSelectionColor field | `feat/cs-diagonal-imbalance` | pending | base commit of feature |
| `6ad637b91` | Support DiagonalImbalance in SeriesHandling | `feat/cs-diagonal-imbalance` | pending | commit 2 of 4 |
| `4313ce4a` | Add diagonal imbalance filters and detection logic | `feat/cs-diagonal-imbalance` | pending | commit 3 of 4; adds CalcType setter DI guard |
| `255d8e77` | Wire diagonal imbalance descriptions into UI attributes | `feat/cs-diagonal-imbalance` | pending | commit 4 of 4 |

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
- [ ] `dotnet build -c Alpha` — 0 errors

### 4.2 Content completeness
- [ ] Phase 1 fix applied
- [ ] Phase 2 feat branch applied

### 4.3 Resource completeness
- [ ] 17 new keys in Resources.resx (en)
- [ ] 17 keys in all 6 satellite locales
- [ ] Resources.Designer.cs up to date

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
| All 6 commits not yet ported | high | Execute Phases 0–2 | `local/clustersearch-i18n` |
