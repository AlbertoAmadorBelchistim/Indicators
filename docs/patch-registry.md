# Patch registry

| Include | Commit | Date | Type | Area | Subject | Source branch | Cherry-pick safe | In testing | In release | PR status | Upstream status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| yes | `c62e44ae` | 2026-03-25 | fix | domstrength | fix(domstrength): use bid depth values when summing bids | fix/domstrength-bid-sum-source | yes | no | no | not-opened | pending | Copy-paste bug: was using `_mDepthAsk.Values` for bids |
| yes | `be5939ca` | 2026-03-25 | fix | domstrength | fix(domstrength): avoid off-by-one when iterating level depth | fix/domstrength-leveldepth-off-by-one | yes | no | no | not-opened | pending | Loop was `<= LevelDepth.Value` instead of `<` |
| yes | `0e610d33` | 2026-03-25 | fix | dompower | fix(dompower): reset state and rebind LevelDepth filter safely | fix/dompower-leveldepth-state-reset | yes | no | no | not-opened | pending | Event subscription leak + stale state on filter change |
| yes | `ef3f6513` | 2026-03-25 | fix | volume | fix(volume): make alert messages reflect selected input type | fix/volume-alert-label | yes | no | no | not-opened | pending | Upstream strings were hardcoded; uses `GetInputLabel()` |
| yes | `fdd1bbef` | 2026-03-25 | fix | volume | fix(volume): make MaximumVolume follow selected input | fix/volume-max-follow-input | yes | no | no | not-opened | pending | Lookback used `candle.Volume` instead of `val` |
| pending | `64ed7f58` | prready/main | fix | delta | fix(delta): clamp price-panel triangle markers to visible region | fix/delta-marker-clamp (not yet created) | yes | no | no | not-opened | pending | Markers outside chart bounds caused visual artifacts |
| yes | `2ddf2f9e` | 2026-01-15 | fix | multimarketpower | fix(multimarketpower): stabilize historical tick cursor iteration | fix/mmp-history-tick-cursor | yes | no | no | not-opened | pending | Cursor index now scoped per request; avoids stale position across calls |
| yes | `db0a9615` | 2026-01-15 | fix | multimarketpower | fix(multimarketpower): make historical request window deterministic | fix/mmp-history-request-window | yes | no | no | not-opened | pending | Use last fully-formed bar time as window end to avoid moving-end race |
| yes | `b2473291` | 2026-01-15 | fix | multimarketpower | fix(multimarketpower): guard OnCalculate against bar 0 | fix/mmp-bar0-guard | yes | no | no | not-opened | pending | Skip calculation on bar 0 to avoid index underrun |
| yes | `7a3599f3` | 2026-03-26 | fix | clustersearch | fix(ClusterSearch): remove unused _pocPrice/_pocVolume fields and dead Transparency property | fix/cs-dead-fields | yes | no | no | not-opened | pending | Dead fields set but never read; property was [Browsable(false)] and never referenced |
| yes | `e6b262f3` | 2026-03-26 | fix | clusterstatistic | fix(clusterstatistic): correct max bid and delta/vol maxima tracking | fix/cs-statistic-maxbid | yes | no | no | not-opened | pending | maxBid was reading candle.Ask (copy-paste bug); deltaPerVol Math.Abs removed (ratio is signed) |

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
- All five original Dom/Volume patches are safe for PR submission to upstream.
- `fix/delta-marker-clamp` is pending branch creation (cherry-pick from `prready/main` commit `64ed7f58`).
- `fix/mmp-history-*` and `fix/mmp-bar0-guard` are fully independent and atomic — safe for PR submission. All other MMP branches depend on local feat branches not yet submitted upstream.
- `fix/cs-dead-fields` is fully independent and atomic — safe for PR submission. All other ClusterSearch branches depend on local feat branches not yet submitted upstream.
- `fix/cs-statistic-maxbid` is fully independent and atomic — safe for PR submission. All other ClusterStatistic branches depend on local feat branches not yet submitted upstream.

---

&nbsp;

# Build stacks

Build stacks are **cohesive units** — not suitable for cherry-picking individual commits.
Each layer must be rebased on its parent after any upstream merge into `Develop`.
`local/*` branches use `typeof(Resources)`; `fix/*` PR-candidate branches use `typeof(Strings)`.
For the full key inventory per indicator use `git log --oneline local/build/04-localization`.

| Branch | Base | Purpose | Key contents |
|---|---|---|---|
| `local/build/03-version-shims` | `02-multiversion` | Compatibility shims + build configs | CandleDataSeries guards, VWAP shim (Stable 7.09), PropertiesEditor shim, RolloverDates exclusion, TabAttribute stub (remove when OFT.Attributes ships it in all flavors), multi-flavor build configs (Alpha/Beta/Latest/Stable/ATAS_X_*), localization sync tooling |
| `local/build/04-localization` | `03-version-shims` | New `.resx` keys (7 locales) | Volume (27 keys), Delta (40), MultiMarketPower (15), ClusterSearch (17), ClusterStatistic (74), OHLCPlus (Phase 0), TradesOnChart (33). All locale gaps repaired 2026-03-29. |
