# Alignment audit — 2026-03-28

This document tracks the alignment audit between the documented workflow and the actual
working model. Items are closed once the corresponding fix is committed.

## Section A — Branch model

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| A1 | `feat/` vs `feature/` prefix | CONTRIBUTING.md used `feature/*`; memory and practice use `feat/` | Fix CONTRIBUTING.md | closed |
| A2 | Integration naming distinction | CONTRIBUTING.md conflated `local/integration/*` (meta-integration) with `local/<indicator>-i18n` (per-indicator). Clarified: per-indicator = `local/<indicator>-i18n`; meta-integration = `local/staging` | Rewrite CONTRIBUTING.md §2.5; add `local/staging` to branch-stacks.md | closed |
| A3 | `feat/` branch granularity | Was: one branch per indicator. Correct: one branch per **indivisible feature concept** — one indicator can have multiple `feat/` branches | Updated memory/project_port_workflow.md | closed |
| A4 | Phase numbering | Old: −1, 0, 1, 2, 3, 4 (awkward). New: 1–6 (natural reading order) | Renumber TEMPLATE.md + all 6 port manifests; update CONTRIBUTING.md | closed |
| A5 | Build stack purposes in memory | Not documented in memory | Created memory/build_stack.md | closed |
| A6 | `feature/` branch renames | `feature/volume-thresholds`, `feature/SpeedOfTapeV2`, `feature/TradesOnChart-realtime-engine` need rename to `feat/` on local + origin | git branch rename + push | closed |

## Section B — Localization

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| B1 | 7 locale files locally | Was: de-de + ru-ru only. Correct: en, de-de, ru-ru, es-ES, fr-fr, hi-in, zh-cn | Updated memory; fix CONTRIBUTING.md locale list | closed |
| B2 | Phase 0 gaps in existing ports | OHLCPlus Phase 0 (`fe9bb5c7`): missing es-ES. TradesOnChart Phase 0 (`2cdd4c82`): missing es-ES, fr-fr, hi-in, zh-cn | Repair commits: es-es rebuilt (`0688067e`); fr-fr/hi-in/zh-cn added (`9dd3e31f`) | closed |
| B3 | `typeof(Resources)` scope rule | Correct: feat/ = hardcoded; i18n branch = typeof(Resources). Added missing: near-duplicate key case + diff tool reference + in_both.json guidance | Expanded CONTRIBUTING.md §8 (`d001c05b`) | closed |
| B4 | Upstream locale count | Correct: upstream Develop only has en, de-de, ru-ru | — | ✓ |

## Section C — Port phases

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| C1 | 3-perspective evaluation | Correct | — | ✓ |
| C2 | Manifest before code | Correct | — | ✓ |
| C3 | Phase ordering | Correct overall; renumbered to 1-6 | See A4 | closed |
| C4 | Smoke tests are manual | Correct | — | ✓ |
| C5 | Docs updated after smoke test | Correct | — | ✓ |
| C6 | Per-indicator verification document | Phase 5 in CONTRIBUTING.md mentions creating the document, but `docs/testing-checklist.md` is a general skeleton with no per-indicator sections. Format and location not defined. | Decide: append to testing-checklist.md vs separate `docs/smoke-tests/<indicator>.md` files. Defer to user. | **open** |

## Section D — Memory / context gaps

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| D1 | ARCHITECTURE.md rules not in memory | Only a pointer existed | Created `memory/architecture_rules.md` with all fundamental rules | closed |
| D5 | `patch-registry.md` not in workflow | Pointer existed in memory but column semantics undocumented; no workflow step required updating it | Added Phase 2 step to CONTRIBUTING.md; updated `memory/reference_docs.md` with column semantics | closed |
| D6 | `patch-registry.md` structure — build stacks section | Lower half of file mixes exhaustive key lists (git log duplicate, goes stale) with useful build stack purpose docs. Cherry-pick command sets are hash-dependent and fragile post-rebase. | Keep patch table + patch notes as-is. Trim build stacks section to purpose + high-level categories only; remove key enumerations. Remove static cherry-pick command sets. | open |
| D2 | Ultimate project goals not in memory | Missing | Created memory/user_goals.md | closed |
| D3 | CLAUDE.md branch-unaware | Structural limitation | Added: key branches table, docs/ cross-references, memory/ path, fixed 7-locale count. Also: `reference_docs.md` updated with key branch roles + prready/main tracking note. | closed |
| D4 | publish_stable audit model not in memory | Missing | Documented in memory/user_goals.md (scores, categories, actions) | closed |

## Section E — Code patterns

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| E1 | Drawing.Color / CrossColor | Correct | — | ✓ |
| E2 | typeof(Strings) vs typeof(Resources) | Correct | — | ✓ |
| E3 | #if !ATAS_STABLE preference | Correct | — | ✓ |
| E4 | *Modif = controlled derivation | Correct | — | ✓ |
| E5 | DataSeries types not documented | `ValueDataSeries` properties (`VisualMode`, `IsHidden`, `ShowCurrentValue`, `IgnoredByAlerts`, `ResetAlertsOnNewBar`, `UseMinimizedModeIfEnabled`, `ScaleIt`), `RangeDataSeries`, `CandleDataSeries`, `PaintbarsDataSeries` all used in 20+ indicators but absent from ARCHITECTURE.md | Add §9.5 to ARCHITECTURE.md | **open** |
| E6 | Indicator lifecycle methods not documented | `OnInitialize`, `OnApplyDefaultColors`, `OnRecalculate`, `OnDispose`, `SubscribeToDrawingEvents(DrawingLayouts)` — used across 30+ indicators, none documented in ARCHITECTURE.md | Add §9.6 | **open** |
| E7 | OnRender / custom drawing pattern not documented | `EnableCustomDrawing`, `OnRender(RenderContext, DrawingLayouts)`, coordinate conversion (`GetXByBar`, `GetYByPrice`), `FirstVisibleBarNumber`/`LastVisibleBarNumber` guards, render resource fields — used in TradesOnChart, DOM, ClusterStatistic, AccountInfoDisplay. Also: `ChartInfo.ColorsStore` for platform color theming. | Add §9.7 | **open** |
| E8 | Parameter attribute pattern not documented | `[Parameter]`, `[Range]`, `[Display(..., Description=..., Order=...)]` full pattern; `Order` numbering conventions; `RaisePropertyChanged` vs `RecalculateValues`; `Filter`/`FilterInt`/`FilterString` types | Add §9.8 | **open** |
| E9 | Session/bar lifecycle pattern not documented | `bar == 0` full reset (`DataSeries.ForEach(x => x.Clear())`), `bar == CurrentBar - 1` realtime guard, `IsNewSession(bar)`, `SetPointOfEndLine`, `_lastBar`/`_lastAlert` dedup guards | Add §9.9 | **open** |
| E10 | Alerts and trading statistics not documented | `AddAlert(file, instrument, message, bgColor, fontColor)`, `IgnoredByAlerts`, `ResetAlertsOnNewBar`, `InstrumentInfo.TickSize`, `TradingStatisticsProvider`, `TradingManager.PortfolioSelected` — subscribe in `OnInitialize`, unsubscribe in `OnDispose` | Add §9.10 | **open** |
| E11 | ATAS X compatibility gap | Custom WPF editors (`Editors/` folder) are **incompatible** with ATAS X. `System.Windows.Media` base types auto-mapped (no code change needed). Indicators using custom `UserControl` editors will fail on ATAS X. | Audit `Editors/` folder; add §5.x to ARCHITECTURE.md | **open** |

## Section F — Project understanding

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| F1 | Three project objectives | Not in memory | Created memory/user_goals.md | closed |
| F2 | Sequencing: indicators → TradingManager API | Not in memory | In memory/user_goals.md | closed |
| F3 | prready/main is legacy | Clarified this session | Updated memory/project_port_workflow.md | closed |

## Open items

Items to be addressed when we reach the corresponding audit section:

| # | Item | Deferred to | Notes |
|---|------|-------------|-------|
| B2 | Phase 0 locale repair | Section B | Fully closed: es-es (`0688067e`) + fr-fr/hi-in/zh-cn (`9dd3e31f`). All 7 locales complete for TradesOnChart Phase 0. |
| D3 | CLAUDE.md cross-references | Section D | Closed: key branches table + docs/ + memory/ added to CLAUDE.md. |

## Additional findings (post-A review)

| # | Item | Finding | Action |
|---|------|---------|--------|
| A7 | `es-ES` vs `es-es` casing | ATAS Platform uses all-lowercase folder names (`de-de`, `es-es`, `fr-fr`, `hi-in`, `ru-ru`, `zh-cn`). BCP-47 uppercase rename would break the sync tool. `es-ES` was a mistake from `c9e31e23`. | **Repaired**: `es-es.resx` restored in `0688067e` (6451 keys: March-25 base + 128 custom + 36 new). `diff_strings_vs_resources.ps1` added to `03-version-shims`. | closed |
| A8 | `feat/tradesonchart` branch granularity | Single branch with 23 commits covering multiple independent features — does not meet current "per indivisible concept" rule | Audit and split after context audit is complete |
| A9 | `feat/TradesOnChart-realtime-engine` | Possibly legacy (content may already be in `local/tradesonchart-i18n`). Was mistakenly lowercased during rename — corrected to CamelCase. | Review content vs tradesonchart-i18n before deleting |
| A10 | `publish_platformx_beta` | Appeared from upstream during `git fetch`. Not created locally. | Monitor — upstream-only branch, no action needed |
| A11 | `local-only` label meaning | In branch-stacks.md: means "not yet submitted as PR to upstream ATAS", not "absent from origin". All active branches should exist on origin. | Clarify label definition in branch-stacks.md |
| G1 | Branch history audit + key integration workflow | Need to audit every active branch one by one: review commit history, verify Phase correctness, and define how new Resource keys are added going forward (automated by Claude or via explicit user command). | New Section G — schedule as dedicated session after Section C-F review complete |
