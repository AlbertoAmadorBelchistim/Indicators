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
- `feature/*`
- `refactor/*`

Examples:
- `fix/domstrength-bid-sum-source`
- `feature/ohlcplus-layout-option`
- `refactor/clusterstatistic-series-cleanup`

These branches should contain:
- one clear purpose
- minimal hidden dependencies
- commits that are understandable in isolation

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

Use `local/integration/*` for patch promotion and local validation.

Examples:
- `local/integration/testing`
- `local/integration/release`

These branches are intended to:
- collect approved individual patches
- test patch combinations
- assemble reproducible local builds

They are not the default place where fixes are authored.

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
- cherry-pick into `local/integration/testing`
- promote later to `local/integration/release`

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
- `feature/*`
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

## 8. Documentation responsibilities

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

## 9. Practical review checklist

Before considering a branch or commit ready, verify:

- is the scope clear?
- does it mix unrelated concerns?
- is it buildable on its intended base?
- is it cherry-pick safe?
- should it be a stack instead of a patch?
- is it a candidate for upstream?
- does it need documentation in `branch-stacks.md` or `patch-registry.md`?

---

## 10. Final rule

Contribute in a way that makes future integration easier.

A change is not finished when it merely works:
it is finished when its place in the repository is clear.
