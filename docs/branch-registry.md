# Branch registry

Inventory of all branches across the three locations: **upstream** (AtasPlatform/Indicators),
**origin** (AlbertoAmadorBelchistim/Indicators — combined backup of upstream + local work),
**local** (working machine).

Last full sync: **2026-03-29**

Column guide:
- **L** = exists in local | **O** = exists in origin | **U** = exists in upstream
- **Last U/O/L** = date of most recent commit in that location (`—` = not present)
- **Action** = recommended next action

---

## 1. Shared reference branches

Branches that exist in upstream AND are tracked locally. Must be kept in sync.

| Branch | Purpose | L | O | U | Last U | Last O | Last L | Sync status | Action |
|--------|---------|---|---|---|--------|--------|--------|-------------|--------|
| `Develop` | Upstream main dev branch. Base for all feat/fix branches. | ✓ | ✓ | ✓ | 2026-03-26 | 2026-03-26 | 2026-03-26 | ✓ synced | Sync at session start |
| `publish_stable` | Last stable release from upstream | ✓ | ✓ | ✓ | 2025-06-17 | 2025-06-17 | 2025-06-17 | ✓ synced | Update after each stable release |
| `publish_alpha` | Alpha release (most current published) | ✓ | ✓ | ✓ | 2026-03-27 | 2026-03-24 | 2026-03-24 | O/L behind upstream 1 commit | Pull from upstream |
| `publish_beta` | Beta release | ✓ | ✓ | ✓ | 2026-03-23 | 2026-03-23 | 2026-03-23 | ✓ synced | Update after each beta release |
| `publish_latest` | Latest release | ✓ | ✓ | ✓ | 2026-02-26 | 2026-02-26 | 2026-02-26 | ✓ synced | — |
| `publish_platformx_beta` | ATAS X beta (cross-platform) | — | ✓ | ✓ | 2026-03-23 | 2026-03-23 | n/a | O mirrors U | Bring to local if ATAS X testing needed |
| `develop_hotfixes` | Hotfixes on top of Develop (PLAT-3829, 3831, 3826) | ✓ | ✓ | ✓ | 2026-03-21 | 2026-03-21 | 2026-03-21 | ✓ synced | Monitor — may merge to Develop |

---

## 2. Upstream active branches (2025–2026)

Upstream feature branches with recent activity. Worth tracking in origin; evaluate for local.

| Branch | Purpose | L | O | U | Last U | Notes | Action |
|--------|---------|---|---|---|--------|-------|--------|
| `session_color_list` | New SessionColors feature (PLAT-3929): multiple color objects per session | — | ✓ | ✓ | 2026-03-23 | Very recent, likely merges to Develop soon | Keep in origin, monitor |
| `develop-avalonia2` | ATAS X (Avalonia UI) cross-platform port | — | ✓ | ✓ | 2026-02-04 | Active. Affects custom editor compatibility (E11). | Keep in origin, monitor |
| `net10` | .NET 10 migration (last: Nov 2025, stalled?) | — | ✓ | ✓ | 2025-11-28 | May revive. Our build stack would need adaptation. | Keep in origin, monitor |
| `cluster_search_revert` | Rollback of ClusterSearch change (PLAT-3550 revert) | — | ✓ | ✓ | 2026-02-13 | Relevant: may affect our ClusterSearch port | Keep in origin |
| `OHLCPlus` | ATAS own OHLCPlus work (PLAT-3096, partial localization) | — | ✓ | ✓ | 2025-09-10 | **Critical**: overlaps with our OHLCPlus port. Review before Phase 6. | Keep in origin, review at OHLCPlus Phase 5 |
| `js-dom-indicator` | DOM indicator perf/rendering improvements | — | ✓ | ✓ | 2026-01-21 | DOM render optimizations; relevant if we port DOM variants | Keep in origin |

---

## 3. Upstream legacy branches (pre-2025)

Stale upstream branches. Most represent completed or abandoned work already merged or superseded.

| Branch | Last U | What it was | In O | In L | Action |
|--------|--------|------------|------|------|--------|
| `cluster_search_refactoring` | 2025-02-13 | ClusterSearch alert + description update | ✓ | — | Keep in origin (recent enough) |
| `mainwindow_compact` | 2025-07-15 | RolloverLines / RolloverDates indicator | ✓ | — | Keep in origin |
| `smartdom_contextmenu` | 2025-07-15 | SmartDOM context menu | ✓ | — | Keep in origin |
| `revert-60-patch-6` | 2025-04-14 | **Reverts PR #60 (AlbertoAmadorBelchistim/patch-6)**. ATAS reverted a contribution from this repo. | ✓ | — | Keep — understand what was reverted and why |
| `cross-platform` | 2024-07-25 | Earlier cross-platform attempt (superseded by develop-avalonia2) | ✓ | — | Origin only, no local needed |
| `custom-view` | 2023-12-06 | Custom view / Properties indicator simplification | ✓ | — | Origin only |
| `HideChartProperty` / `main` | 2023-08-07 | Same tip. OrderFlowRhythm indicator added. `main` is an alias. | ✓ | — | `main` is confusing naming — origin only, no local |
| `domlevels_heatmap` | 2023-03-30 | DOM levels heatmap (ClusterSearch MaxVolume mode) | ✓ | — | Origin only |
| `dom` | 2022-10-07 | DOM cumulative render | ✓ | — | Origin only |
| `heatmap_volume` | 2022-06-02 | HeatMap volume coloring | ✓ | — | Origin only |
| `iceberg` | 2022-11-03 | Iceberg indicator prototype | ✓ | — | Origin only |
| `DomStrength` | 2022-03-31 | DomStrength indicator addition | ✓ | — | Likely merged to Develop. Origin only. |
| `OSMA` | 2022-03-31 | OSMA indicator | ✓ | — | Likely merged. Origin only. |
| `Logo` | 2021-05-13 | Logo indicator | ✓ | — | Origin only |
| `average_candle_range` | 2022-03-31 | Average Candle Range indicator | ✓ | — | Origin only |
| `tma_bands` | 2022-07-06 | TMA Bands indicator | ✓ | — | Origin only |
| `vwap_filter_properies` | 2023-05-24 | VWAP filter properties | ✓ | — | Origin only |
| `avalonia` | 2023-05-04 | Early Avalonia attempt (superseded) | ✓ | — | Origin only |
| `resource_refactor` | 2021-09-15 | Localization resource refactor (superseded by current resource structure) | ✓ | — | Origin only |
| `cumulative_dom` | 2022-01-21 | DOM cumulative render prototype | ✓ | — | Origin only |
| `supply_demand_zones` | 2022-03-28 | Supply/demand zones prototype | ✓ | — | Origin only |
| `option_niked` | 2022-05-30 | OptionNiked indicator prototype | ✓ | — | Origin only |
| `indicators-editors` | 2020-11-11 | UI editors (WPF) — predates current Editors/ structure | ✓ | — | Origin only |

---

## 4. Origin-only candidates for deletion

In origin but **deleted from upstream** (deleted during fetch 2026-03-29) and **no local counterpart**.

| Branch | Was in upstream | Last O | Content | Action |
|--------|----------------|--------|---------|--------|
| `auto-chart-reload` | Yes, deleted 2026-03-29 | unknown | Auto chart reload feature (merged/abandoned by ATAS) | **Delete from origin** |
| `mn_trades-on-chart` | Yes, deleted 2026-03-29 | unknown | PLAT-3736: TradesOnChart fixes by ATAS (shows all data regardless stats filters) | **Review vs our port**, then delete |
| `vp_history-of-expiries` | Yes, deleted 2026-03-29 | unknown | History of expiries feature | **Delete from origin** |
| `vp_DataSeriesPropVizible2` | Never in upstream (or deleted earlier) | unknown | DataSeries visibility properties work | **Delete from origin** |
| `local/build-base` | — | — | Older name for `local/build/01-base`, superseded | **Delete from origin** |

---

## 5. Local-only work branches (user's own — need origin push)

User's active work branches. Should exist in origin as backup. Currently missing from origin:

| Branch | Last L | Status | Missing from origin | Action |
|--------|--------|--------|---------------------|--------|
| `feat/ohlcplus` | 2026-03-27 | Active (41 commits) | **Yes** | `git push origin feat/ohlcplus` |
| `feat/tradesonchart` | 2026-03-27 | Active (23 commits) | **Yes** | `git push origin feat/tradesonchart` |
| `local/ohlcplus-i18n` | 2026-03-27 | Complete, pending smoke test | **Yes** | `git push origin local/ohlcplus-i18n` |
| `local/tradesonchart-i18n` | 2026-03-27 | Complete, pending smoke test | **Yes** | `git push origin local/tradesonchart-i18n` |
| `refactor/ohlcplus-period-helpers` | 2026-03-27 | Active refactor | **Yes** | `git push origin refactor/ohlcplus-period-helpers` |
| `backup/prready-main-pre-rebase` | 2026-02-19 | Safety backup | **Yes** | `git push origin backup/prready-main-pre-rebase` |

---

## 6. Local work branches (in origin — active)

| Branch | Last L | Last O | Status | Notes |
|--------|--------|--------|--------|-------|
| `local/build/01-base` | 2026-03-25 | 2026-03-25 | ✓ synced | PR open to upstream |
| `local/build/02-multiversion` | 2026-03-25 | 2026-03-25 | ✓ synced | local-only |
| `local/build/03-version-shims` | 2026-03-28 | — | **Not in origin** | `git push origin local/build/03-version-shims` |
| `local/build/04-localization` | 2026-03-28 | — | **Not in origin** | `git push origin local/build/04-localization` |
| `meta/docs` | 2026-03-29 | 2026-03-26 | O behind L by 10 commits | `git push origin meta/docs` |
| `prready/main` | 2026-03-05 | 2026-03-05 | ✓ synced | Legacy migration source |
| `compile/myindicators` | 2025-12-29 | 2025-12-29 | ✓ synced | Last published build reference |
| `compile/local-testing` | 2026-03-20 | 2026-03-20 | ✓ synced | Local test build |
| `publish_stable_MyIndicators` | 2026-01-11 | 2026-01-11 | ✓ synced | User-facing GitHub main branch |
| `refactor/mmp-rolling-sma` | 2026-03-26 | 2026-03-26 | ✓ synced | MMP rolling SMA refactor |
| `refactor/ohlcplus-period-helpers` | 2026-03-27 | — | **Not in origin** | Push needed |

---

## 7. Feat and fix branches (all in origin)

All active feat/ and fix/ branches are in both local and origin. No sync issues detected.

| Branch | Last L | Last O | Notes |
|--------|--------|--------|-------|
| `feat/delta-*` (5 branches) | 2026-03-25 | 2026-03-25 | All synced |
| `feat/mmp-*` (4 branches) | 2026-03-26 | 2026-03-26 | All synced |
| `feat/cs-*` (2 branches) | 2026-03-26 | 2026-03-26 | All synced |
| `feat/volume-thresholds` | 2026-03-25 | 2026-03-25 | Synced |
| `feat/SpeedOfTapeV2` | — | ✓ | Local missing — check if still needed |
| `feat/TradesOnChart-realtime-engine` | — | ✓ | Origin only — local deleted after rename to CamelCase. Review A9. |
| `fix/delta-*` (3 branches) | 2026-03-25–26 | same | All synced |
| `fix/mmp-*` (11 branches) | 2026-03-26 | 2026-03-26 | All synced |
| `fix/domstrength-*` (2 branches) | 2026-03-25 | 2026-03-25 | All synced |
| `fix/dompower-*` (1 branch) | 2026-03-25 | 2026-03-25 | Synced |
| `fix/volume-*` (2 branches) | 2026-03-25 | 2026-03-25 | Synced |
| `fix/cs-*` (2 branches) | 2026-03-26 | 2026-03-26 | Synced |

---

## 8. Immediate actions required

| Priority | Action | Branch(es) |
|----------|--------|-----------|
| 🔴 High | Push missing active branches to origin | `feat/ohlcplus`, `feat/tradesonchart`, `local/ohlcplus-i18n`, `local/tradesonchart-i18n`, `local/build/03-version-shims`, `local/build/04-localization`, `refactor/ohlcplus-period-helpers`, `backup/prready-main-pre-rebase` |
| 🔴 High | Push meta/docs (10 commits behind origin) | `meta/docs` |
| 🟡 Medium | Review `mn_trades-on-chart` vs our TradesOnChart port (PLAT-3736) | `origin/mn_trades-on-chart` |
| 🟡 Medium | Review `revert-60-patch-6` — understand what ATAS reverted and why | `upstream/revert-60-patch-6` |
| 🟡 Medium | Review `upstream/OHLCPlus` vs our OHLCPlus port | `upstream/OHLCPlus` |
| 🟡 Medium | Pull `upstream/publish_alpha` (1 commit ahead of local) | `publish_alpha` |
| 🟠 Low | Delete from origin: `auto-chart-reload`, `vp_history-of-expiries`, `vp_DataSeriesPropVizible2`, `local/build-base` | origin cleanup |
| 🟠 Low | Evaluate `feat/SpeedOfTapeV2` — still needed in local? | — |
| 🟠 Low | All other upstream legacy branches — origin only is correct, no local needed | — |
