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
| B2 | Phase 0 gaps in existing ports | OHLCPlus Phase 0 (`fe9bb5c7`): missing es-ES. TradesOnChart Phase 0 (`2cdd4c82`): missing es-ES, fr-fr, hi-in, zh-cn | Add missing keys to 04-localization in a repair commit | **open** |
| B3 | `typeof(Resources)` scope rule | Correct: feat/ = hardcoded; i18n branch = typeof(Resources) | — | ✓ |
| B4 | Upstream locale count | Correct: upstream Develop only has en, de-de, ru-ru | — | ✓ |

## Section C — Port phases

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| C1 | 3-perspective evaluation | Correct | — | ✓ |
| C2 | Manifest before code | Correct | — | ✓ |
| C3 | Phase ordering | Correct overall; renumbered to 1-6 | See A4 | closed |
| C4 | Smoke tests are manual | Correct | — | ✓ |
| C5 | Docs updated after smoke test | Correct | — | ✓ |

## Section D — Memory / context gaps

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| D1 | ARCHITECTURE.md rules not in memory | Only a pointer existed | Pointer still in reference_docs.md; key rules in CONTRIBUTING.md | partial |
| D2 | Ultimate project goals not in memory | Missing | Created memory/user_goals.md | closed |
| D3 | CLAUDE.md branch-unaware | Structural limitation | Accepted; cross-reference docs/ in CLAUDE.md | open |
| D4 | publish_stable audit model not in memory | Missing | Documented in memory/user_goals.md (scores, categories, actions) | closed |

## Section E — Code patterns

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| E1 | Drawing.Color / CrossColor | Correct | — | ✓ |
| E2 | typeof(Strings) vs typeof(Resources) | Correct | — | ✓ |
| E3 | #if !ATAS_STABLE preference | Correct | — | ✓ |
| E4 | *Modif = controlled derivation | Correct | — | ✓ |

## Section F — Project understanding

| # | Item | Finding | Resolution | Status |
|---|------|---------|------------|--------|
| F1 | Three project objectives | Not in memory | Created memory/user_goals.md | closed |
| F2 | Sequencing: indicators → TradingManager API | Not in memory | In memory/user_goals.md | closed |
| F3 | prready/main is legacy | Clarified this session | Updated memory/project_port_workflow.md | closed |

## Open items

- **B2** — Phase 0 locale repair: add missing keys for OHLCPlus (es-ES) and TradesOnChart (es-ES, fr-fr, hi-in, zh-cn) to `local/build/04-localization`
- **D3** — CLAUDE.md does not reference docs/ or memory/ — add cross-references
