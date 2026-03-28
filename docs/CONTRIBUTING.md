# Contributing - ATAS Indicators Repository

This document defines the contribution workflow for this repository.

Its purpose is to keep the repository:

- auditable
- reversible
- buildable
- maintainable under upstream change
- scalable as the number of local patches and stacks grows

This document focuses on:

- branch usage
- commit discipline
- patch promotion
- PR preparation
- interaction between local work and upstream alignment

For code architecture rules, see:

- `ARCHITECTURE.md`

For repository structure and active stacks, see:

- `docs/branch-stacks.md`
- `docs/patch-registry.md`

---

## 1. Core principles

All contributions should aim to preserve:

- atomicity
- reversibility
- traceability
- minimal hidden coupling
- compatibility with future upstream updates

A good contribution is not only correct in code:
it must also be easy to review, isolate, promote, revert, and document.

---

## 2. Branch model

### 2.1 Base branch

`develop` is the local branch aligned with upstream and acts as the default base branch.

Rules:
- do not treat `develop` as a dumping ground
- do not accumulate unrelated local work directly on `develop`
- create child branches from `develop` for individual fixes, features, and refactors

---

### 2.2 Individual work branches

Use dedicated branches for isolated code changes:

- `fix/*`
- `feat/*`
- `refactor/*`
- `chore/*`

Examples:
- `fix/domstrength-bid-sum-source`
- `feat/ohlcplus-label-system`
- `refactor/clusterstatistic-series-cleanup`
- `chore/delta-ui-polish`

One branch per **indivisible feature concept** — an indicator can have multiple `feat/` branches
if the features are independent (e.g. `feat/delta-audio-alerts` and `feat/delta-average-line`).

These branches should contain:
- one clear purpose
- minimal hidden dependencies
- commits that are understandable in isolation

New resource keys on these branches must use hardcoded English strings (no `typeof(Resources)`
for keys not present in the upstream `Strings` class). Use `typeof(Strings)` only where the key
already exists in the platform.

---

### 2.3 PR preparation branches

Use `prready/*` only when needed.

Typical use cases:
- regrouping several related branches into one upstream-friendly proposal
- cleaning commit history before a PR
- isolating a publishable subset from a broader local effort

Do not create `prready/*` by default for every small fix.

If a single `fix/*` branch is already clean and reviewable,
it can be used directly for PR preparation.

---

### 2.4 Local build stacks

Use `local/build/*` for compatibility and build-structure work.

Examples:
- `local/build/01-base`
- `local/build/02-multiversion`
- `local/build/03-version-shims`

These branches represent cohesive infrastructure layers such as:
- build flavor support
- compatibility guards
- version shims
- solution/configuration changes

They should be treated as **stacks**, not as arbitrary collections of independent patches.

---

### 2.5 Local integration branches

There are two distinct kinds of local integration branch:

**Per-indicator integration — `local/<indicator>-i18n`**

One branch per indicator, stacked on `local/build/04-localization`. Combines the indicator's
fix and feat standalone branches (in that order) and converts all new keys to `typeof(Resources)`.
This is the compilable, fully-localized version of a single indicator.

Examples:
- `local/delta-i18n`
- `local/tradesonchart-i18n`

**Meta-integration — `local/staging`**

A single branch that merges all completed `local/<indicator>-i18n` branches for pre-publication
verification. Created from `local/build/04-localization`; each i18n branch is merged in as it
completes.

- When an i18n branch gets new forward-only commits: re-merge into staging
- When an i18n branch is rebased (because 04-localization changed): rebuild staging from scratch

`local/staging` is the final step before publication. It is not where code is authored.

---

### 2.6 Repository documentation branch

Use `meta/docs` for repository engineering documentation when it should remain separate from code-oriented work.

Typical contents:
- `docs/branch-stacks.md`
- `docs/patch-registry.md`

---

## 3. Commit discipline

### 3.1 Atomicity

Each commit should represent one coherent change.

Good examples:
- one bugfix
- one isolated refactor
- one compatibility shim
- one resource-only update

Avoid commits that mix:
- logic and localization
- rendering and compatibility
- build changes and feature work
- refactor and behavior change

---

### 3.2 Reviewability

A commit should be reviewable in isolation.

Ask:
- what does this commit change?
- why does it exist?
- could it be reverted alone?
- could it be cherry-picked safely?

If the answer is unclear, the commit is likely too broad.

---

### 3.3 Buildability

As a rule, each commit should leave the repository in a buildable and understandable state.

Exceptions are acceptable only when:
- the branch intentionally represents an incomplete stack
- the relationship between commits is explicitly documented

Even in those cases:
- commit dependencies must be obvious
- hidden “follow-up required” commits should be avoided

---

### 3.4 Commit messages

Commit subjects should be short, precise, and structured.

Preferred style:
- `fix(domstrength): use bid depth values when summing bids`
- `compat(stable): guard newer CandleDataSeries visual API by build flavor`
- `docs(patch-registry): add initial domstrength fixes`

The subject must describe:
- the area
- the kind of change
- the intention

---

## 4. Patches vs stacks

This repository uses two different integration units.

### 4.1 Individual patches

An individual patch is a commit that:
- has clear scope
- can be promoted independently
- is a good candidate for cherry-pick
- may be suitable for upstream PRs

Typical examples:
- small fixes
- isolated logic corrections
- narrow refactors

These patches should be tracked in:
- `docs/patch-registry.md`

---

### 4.2 Cohesive stacks

A stack is a branch or branch segment that must be treated as a unit.

Typical examples:
- multi-version compatibility work
- build flavor support
- grouped shims for stable/beta/alpha differences

Do not force stack work into fake atomic commits if those commits are not independently meaningful.

Stacks should be documented as stacks, not misrepresented as unrelated standalone patches.

---

## 5. Cherry-pick, rebase, and merge

### 5.1 Cherry-pick

Use `cherry-pick` to promote **individual patches** into integration branches.

Typical workflow:
- author fix in `fix/*`
- validate it
- cherry-pick into `local/<indicator>-i18n`
- merge into `local/staging` when ready for publication

Cherry-pick should be preferred when:
- the patch is atomic
- the source branch contains useful work that should not bring its entire history
- promotion must remain selective

---

### 5.2 Rebase

Use `rebase` to keep work branches aligned and clean.

Typical use cases:
- rebasing `fix/*` on updated `develop`
- refreshing `prready/*` before review
- replaying a clean local branch on a newer base

Rebase rewrites commit history.
Do not use it casually on shared branches unless that is intentional and safe.

---

### 5.3 Merge

Use `merge` when preserving branch history matters more than a linear commit sequence.

Typical use cases:
- integrating a branch whose internal history is meaningful as history
- preserving the context of a local stack as a single integration event

Do not use merge by default where cherry-pick or rebase better expresses intent.

---

## 6. Upstream-friendly work vs local divergence

Contributors must distinguish between:

- upstream-friendly changes
- debatable but potentially upstreamable changes
- local-only divergence

### Upstream-friendly changes
Examples:
- bugfixes
- narrow null guards
- isolated resource cleanup
- small compatibility fixes with minimal behavioral impact

These should usually live in:
- `fix/*`
- `feat/*`
- `prready/*` if grouping is needed

---

### Local-only divergence
Examples:
- behavior intentionally tailored to a local workflow
- UI or semantics that do not match upstream expectations
- local overlays or workflow-specific features

These should not be disguised as upstream-friendly patches.

If long-term divergence is intentional, make it explicit and keep it documented.

---

## 7. Modified indicators (`*Modif`)

Do not create `*Modif` variants for small isolated fixes.

Use `*Modif` only when:
- divergence is intentional
- it is functionally meaningful
- the variant is expected to live beyond a temporary experiment

Rules:
- preserve traceability to the original indicator
- do not mix unrelated experiments into a derived variant
- document why the divergence exists

---

## 8. Localization workflow

Local branches use `typeof(Resources)` for display attributes; upstream-targeted branches use `typeof(Strings)`.

### Adding new resource keys

1. Add the key and English value to `Properties/Resources.resx`
2. Add the same key to **all 7 satellite `.resx` files**: `de-de`, `ru-ru`, `es-es`, `fr-fr`, `hi-in`, `zh-cn`
3. Regenerate `Properties/Resources.Designer.cs` (VS `PublicResXFileCodeGenerator` or the `scripts/gen-designer.py` helper)
4. Do this in a dedicated commit on `local/build/04-localization` **before** the feature branch that uses the key
5. Document the new key set in `docs/patch-registry.md` under the `04-localization` section

### Choosing between `typeof(Strings)` and `typeof(Resources)`

| Case | Use |
|------|-----|
| Key exists in upstream `Strings` (e.g. `Period`, `SMA`, `Volume`) | `typeof(Strings)` |
| Key is new, not present in any `Strings` version | `typeof(Resources)` |
| Key exists in `Strings` under a slightly different name (e.g. `Strings.AlertColor` vs your `Resources.AlertColorDescription`) | Use `typeof(Strings)` for the base key; add only the description variant to `Resources` if it is genuinely new |
| PR-targeted branch | `typeof(Strings)` — do not introduce `typeof(Resources)` into upstream PRs |

**Before adding any new key to `Resources.resx`**, run the diff tool to check whether the key (or a near-equivalent) already exists in the upstream platform:

```powershell
# On local/build/03-version-shims
tools/localization/diff_strings_vs_resources.ps1
# → outputs only_in_strings.json  (candidates for typeof(Strings))
# →         only_in_resources.json (confirmed local-only keys)
# →         in_both.json           (duplicate additions — should be removed from Resources)
```

If a key appears in `in_both.json`, remove it from `Resources.resx` and switch the reference to `typeof(Strings)`.

### Testing checklist

See `docs/testing-checklist.md` for the full per-indicator QA checklist.

Two-phase model:
- **Smoke test** when integrating a branch: does it load, do properties appear, does the panel render?
- **Full checklist** before PR submission or publication.

---

## 9. Documentation responsibilities

The repository uses different documents for different concerns.

### `ARCHITECTURE.md`
Defines code architecture principles:
- calculation
- state
- rendering
- compatibility layers
- localization boundaries

### `docs/branch-stacks.md`
Documents active branch topology:
- parent-child relationships
- stack structure
- branch purposes
- integration flow

### `docs/patch-registry.md`
Documents:
- individual patches
- promotion status
- patch sets for integration
- cohesive build stacks

This separation must be preserved.

---

## 10. Practical review checklist

Before considering a branch or commit ready, verify:

- is the scope clear?
- does it mix unrelated concerns?
- is it buildable on its intended base?
- is it cherry-pick safe?
- should it be a stack instead of a patch?
- is it a candidate for upstream?
- does it need documentation in `branch-stacks.md` or `patch-registry.md`?

---

## 11. Indicator port workflow (6 phases)

Use this workflow when porting an indicator from an upstream source or developing new indicator features.

### Phase 1 — Evaluation + Manifest

Evaluate the indicator from three perspectives:
- **Software engineer**: correctness bugs, data-race risks, O(n) hot paths, series indexing, edge cases at bar 0 / session boundary
- **Discretionary trader**: does the visualization communicate what traders need? Are defaults sensible?
- **Algo trader**: are calculated values stable and deterministic enough for signal use? Any recalculation or look-ahead issues?

Evaluate which changes to make. Agree on intentional divergences.
Create the port manifest in `docs/port-manifests/<indicator>.md` on `meta/docs`.
**Phase 1 ends when the manifest is committed. No code is written before this.**

### Phase 2 — Standalone branches (Develop-based)

Create all `feat/` and `fix/` branches rooted at `Develop` — one per indivisible concept.
- Use hardcoded English strings for new keys (no `typeof(Resources)` for keys not in upstream `Strings`)
- Must be C#-valid; Alpha build failures on Develop (MC1000 XAML error) are environmental, not a blocker
- These branches are candidates for upstream PRs

### Phase 3 — Resource keys

Add all new keys (not yet in upstream `Strings`) to `local/build/04-localization`.
All 7 locale files must be updated: `en`, `de-de`, `ru-ru`, `es-es`, `fr-fr`, `hi-in`, `zh-cn`.
Done before the integration branch is created.

### Phase 4 — Integration branch

Create `local/<indicator>-i18n` stacked on `local/build/04-localization`.
Apply fix branches first, then feat branches. Convert all new keys to `typeof(Resources)`.

### Phase 5 — Smoke test

Create the per-indicator verification document (upstream functionality preservation + each new feature).
User executes it manually in ATAS Platform. Nothing is marked complete until Phase 5 passes.

### Phase 6 — Documentation

Update indicator docs in `publish_stable_MyIndicators`. Mark port manifest `complete`.

---

### Upstream sync rules

| Target | Trigger | Command |
|--------|---------|---------|
| `Develop` | Every session (or daily when not actively porting) | `git fetch origin && git merge origin/Develop Develop` |
| `local/build/01→02→03→04` | After every `Develop` sync | Rebase each level in order |
| `local/<indicator>-i18n` | Only when `04-localization` itself changes | Rebase on updated 04 |
| `local/staging` | When an i18n branch is rebased | Rebuild from scratch; otherwise re-merge |

---

## 12. Final rule

Contribute in a way that makes future integration easier.

A change is not finished when it merely works:
it is finished when its place in the repository is clear.
