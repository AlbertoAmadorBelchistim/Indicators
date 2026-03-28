# Port manifest — [INDICATOR NAME]

This document is the authoritative record for porting or developing `[Indicator].cs`.

It defines **before starting**:
- which changes are in scope (ported from upstream or developed locally)
- which branch each change belongs to
- what keys are needed in `04-localization`

It records **as work progresses**:
- what has been done and verified
- what was intentionally skipped and why
- any divergence between local implementation and upstream

**Source:** `prready/main` *(or: original development)*
**Integration target:** `local/[indicator]-i18n` (stacked on `local/build/04-localization`)
**Status:** `draft | in-progress | complete`

---

## How to use this document

1. **Phase 1 — before any branch:** complete the evaluation and commit this manifest
2. **Phase 2 — before creating branches:** confirm Phase 3 keys will be ready in `04-localization`
3. **After each branch is created:** update its Status cell
4. **After integrating into `local/[indicator]-i18n`:** run the Phase 5 verification checklist
5. **At any time:** record intentional divergences in Section 5

Status values:
- `pending` — not started
- `in-progress` — branch exists, work underway
- `done` — branch complete and verified
- `skipped` — intentionally excluded (reason required)
- `missing` — expected in local but confirmed absent (needs remediation)

---

## Phase 1 — Evaluation + Manifest

*Record evaluation decisions here. This section is complete when this file is committed.*

**Software engineer assessment:** [bugs, perf, edge cases, dead fields]

**Discretionary trader assessment:** [UX, defaults, visualization quality]

**Algo trader assessment:** [signal stability, determinism, look-ahead risks]

**Changes in scope:** [summary of what will be ported/developed]

**Intentional divergences decided upfront:** [if any]

---

## Phase 2 — Standalone branches (Develop-based)

Each fix and feat **must have its own branch rooted at `Develop`**, independent of the integration
branch and the localization infrastructure.

**Rules:**
- Base: `git checkout -B <branch> Develop`
- Use hardcoded English strings — no `typeof(Resources)` for keys absent from upstream `Strings`
- Use `typeof(Strings)` only where both `Name` and `GroupName` keys already exist in platform `Strings`
- **Build note:** `dotnet build -c Alpha` on Develop may fail with MC1000 (DevExpress XAML). Environmental, not a C# error.

### Fix branches

| Branch | Description | Status |
|--------|-------------|--------|
| `fix/[indicator]-...` | | pending |

### Feat branches

Feat branches are stacked when they share code dependencies. All must have `Develop` as ultimate ancestor.

| Branch | Description | Stacked on | Status |
|--------|-------------|------------|--------|
| `feat/[indicator]-...` | | `Develop` | pending |

---

## Phase 3 — Resource keys (`local/build/04-localization`)

Keys not yet in upstream `Strings`. Add to **all 7 locale files** before creating the integration branch.

| Key | Value (en) | Introducing commit | Status |
|-----|-----------|-------------------|--------|

**Note:** Keys already in upstream `Strings` do not need to be added here.

---

## Phase 4 — Integration branch: `local/[indicator]-i18n`

Stacked on `local/build/04-localization`. Apply fix branches first, then feat branches.
All new keys use `typeof(Resources)` here (keys are available via 04-localization).

### Commits applied

| Commit / Branch | Description | Needs adaptation | Status | Notes |
|----------------|-------------|-----------------|--------|-------|

---

## Phase 5 — Smoke test

Run this checklist after Phase 4 is complete and before marking the manifest `complete`.

### 5.1 Build
- [ ] `dotnet build -c Alpha` — 0 errors
- [ ] `dotnet build -c Stable` — 0 errors

### 5.2 Content completeness
- [ ] All fix/* branch content present (verify via `git diff`)
- [ ] All feat/* branch content present
- [ ] All Phase 4 notes resolved

### 5.3 Resource completeness
- [ ] All Phase 3 keys exist in `Resources.resx` (en)
- [ ] All Phase 3 keys exist in all 6 satellite locales (de-de, ru-ru, es-ES, fr-fr, hi-in, zh-cn)
- [ ] `Resources.Designer.cs` up to date
- [ ] No `nameof(Resources.X)` in `[Indicator].cs` points to a missing key

### 5.4 Functional smoke test
- [ ] Indicator loads without crash
- [ ] All property groups visible in settings panel
- [ ] No resource key literal visible in UI (e.g. "DeltaLabelGroup" instead of "Delta label")
- [ ] *[Add indicator-specific checks here]*

---

## Phase 6 — Documentation

- [ ] `publish_stable_MyIndicators` docs updated to reflect final behavior
- [ ] Manifest marked `complete`

---

## Section 5 — Intentional divergences

Record any case where `local/[indicator]-i18n` deliberately differs from upstream.

| Area | Upstream behavior | Local behavior | Reason | Revisit? |
|------|-------------------|----------------|--------|----------|

---

## Section 6 — Pending / known gaps

Must be empty before manifest is marked `complete`.

| Gap | Severity | Planned fix | Target branch |
|-----|----------|-------------|---------------|
