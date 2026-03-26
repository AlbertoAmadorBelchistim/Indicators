# Port manifest — [INDICATOR NAME]

This document is the authoritative record for porting `[Indicator].cs` from `prready/main`
into the local branch stack.

It defines **before starting**:
- which commits are in scope
- which branch each commit belongs to
- what keys are needed in `04-localization`

It records **as work progresses**:
- what has been done and verified
- what was intentionally skipped and why
- any divergence between local implementation and prready/main

**Source:** `prready/main`
**Integration target:** `local/[indicator]-i18n` (stacked on `local/build/04-localization`)
**Status:** `draft | in-progress | complete`

---

## How to use this document

1. **Before starting any branch:** fill Phases 0–3 from `git log prready/main -- Technical/[Indicator].cs`
2. **Before creating fix/* or feat/* branches:** confirm Phase 0 keys exist in `04-localization`
3. **After each branch is created:** update its Status cell
4. **After integrating into local/[indicator]-i18n:** run the Phase 4 verification checklist
5. **At any time:** record intentional divergences in Section 5

Status values:
- `pending` — not started
- `in-progress` — branch exists, work underway
- `done` — branch complete and verified
- `skipped` — intentionally excluded (reason required)
- `missing` — expected in local but confirmed absent (needs remediation)

---

## Phase -1 — Standalone branches (Develop-based)

Each fix and feat **must have its own standalone branch rooted at `Develop`**, independent of the integration branch and the localization infrastructure. This allows individual PRs to be reviewed without requiring the full localization stack.

**Rules:**
- Base: `git checkout -B <branch> Develop`
- `typeof(Resources)` **must not be used** for keys that don't exist in the platform `Strings` class on `Develop`. Use hardcoded English strings instead.
- Check which indicator-specific keys are NOT in `Strings` before creating branches; hardcode those, use `typeof(Strings)` only where both `Name` and `GroupName` keys exist in `Strings`.
- **Build note:** `dotnet build -c Alpha` on `Develop`-based branches may fail with MC1000 (DevExpress XAML assembly error). This is a pre-existing environmental issue on `Develop`, not a C# error in your changes.

### Fix branches (Develop-based)

| Branch | Develop-based commit | Status |
|--------|---------------------|--------|
| `fix/[indicator]-...` | | pending |

### Feat branches (stacked from Develop)

Feat branches are stacked when they share code dependencies. All must ultimately have `Develop` as ancestor.

| Branch | Develop-based commit | Stacked on | Status |
|--------|---------------------|------------|--------|
| `feat/[indicator]-...` | | `Develop` | pending |

---

## Phase 0 — Resource keys required in `local/build/04-localization`

Keys that do not exist in upstream `Strings` and must be added to `Resources.resx` (all locales)
**before** fix/* and feat/* branches are created.

| Key | Value (en) | Commit introducing it | Used by | Status |
|-----|-----------|----------------------|---------|--------|

---

## Phase 1 — Fix branches (port before feat)

Fix branches must be the base layer. Their content must be present in the local integration
branch before feat content is applied.

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|

---

## Phase 2 — Feat branches

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|

---

## Phase 3 — Chore / UI polish / compat

Commits that reorganize UI, rename labels, hide internal series, or adjust defaults.
These are not blocking for functionality but are required for publication.

| Commit (prready/main) | Description | Local branch | Status | Notes |
|----------------------|-------------|--------------|--------|-------|

---

## Phase 4 — Integration verification: `local/[indicator]-i18n`

Run this checklist after all phases above are complete and before marking the
manifest as `complete`.

### 4.1 Build
- [ ] `dotnet build -c Alpha` — 0 errors
- [ ] `dotnet build -c Stable` — 0 errors

### 4.2 Content completeness
- [ ] All fix/* branch content is present (verify via `git diff fix/[x] local/[ind]-i18n -- Technical/[Indicator].cs`)
- [ ] All feat/* branch content is present
- [ ] All Phase 3 chore changes are present

### 4.3 Resource completeness
- [ ] All Phase 0 keys exist in Resources.resx (en)
- [ ] All Phase 0 keys exist in all 6 satellite locales
- [ ] Resources.Designer.cs is up to date (all new keys have static properties)
- [ ] No `nameof(Resources.X)` reference in [Indicator].cs points to a missing key

### 4.4 Functional smoke test
- [ ] Indicator loads without crash
- [ ] All property groups visible in settings panel
- [ ] No resource key literal visible in UI (e.g. "DeltaLabelGroup" instead of "Delta label")

---

## Section 5 — Intentional divergences from prready/main

Record here any case where `local/[indicator]-i18n` deliberately differs from `prready/main`.

| Area | prready/main behavior | Local behavior | Reason | Revisit? |
|------|-----------------------|----------------|--------|----------|

---

## Section 6 — Pending / known gaps

Items identified but not yet remediated. Must be empty before manifest is marked `complete`.

| Gap | Severity | Planned fix | Target branch |
|-----|----------|-------------|---------------|
