# Contributing Guidelines

This repository contains professional development work on ATAS indicators.
All contributions must prioritize auditability, functional integrity and upstream viability.

---

## Core principles

- Changes must be **technically correct**, **auditable**, and **trading-aware**.
- The commit history is part of the product.
- Each commit should be reviewable in isolation by ATAS developers without external context.

---

## Branch model (source of truth vs distribution)

This repo follows a user-first publication model while keeping development audit-ready.

### Upstream reference branches (ATAS)
These branches are upstream snapshots used as references for rebases/comparisons:
- `Develop`
- `publish_alpha`, `publish_beta`, `publish_latest`, `publish_stable`

They are not used for development.

### Development (source of truth)
- `prready/main`
  - Primary development branch and **source of truth**.
  - Contains:
    - patch-queue style modifications to upstream indicators, and
    - custom indicators owned by this repository.
  - All commits here must be audit-ready and cherry-pick friendly.

### Build / packaging (artifacts)
- `compile/myIndicators`
  - Assembly branch for distributable code (custom + `*Modif` artifacts).
  - No direct development should happen here.
  - `*Modif` files are derived outputs from `prready/main` (mechanical/reproducible).

### User-facing publication
- `publish_stable_myindicators`
  - Documentation, media and distributed DLLs for non-technical users.
  - Code review should be done in `prready/main`.

---

## Auditability (non-negotiable)

Commits must be written to facilitate **technical audit and cherry-picking** by ATAS developers.

Each commit must:
- Have a **single, narrow technical purpose**.
- Be understandable **without chat history or external explanations**.
- Avoid mixing unrelated concerns.

Explicitly avoid mixing in the same commit:
- calculation logic with rendering changes,
- UI / resources (`Display`, `GroupName`, `.resx`) with internal refactors,
- refactors with new visible features.

If multiple changes are required, they must be split into **separate, ordered commits**.

---

## Functional integrity per commit

Every commit must leave the codebase in a **functional and compilable state**.

Rules:
- No intermediate broken states.
- No partial refactors that require a later commit to “fix” them.
- Feature development must be split into **functional steps**, not conceptual placeholders.

At any commit:
- the indicator must compile,
- calculations must be coherent,
- rendering must reflect the calculated state.

This is mandatory to allow:
- reliable bisection,
- safe rollback,
- selective cherry-picking.

---

## Commit messages

- Language: **English**
- Style: concise and technical
- Prefixes:
  - `fix`: bug fixes or correctness issues
  - `feat`: new features or user-visible behavior
  - `refactor`: structural changes without behavior change
  - `chore`: tooling, docs, non-functional changes

Examples:
- `fix(clusterstatistic): correct delta accumulation on live bar`
- `refactor(dailylines): separate calculation from rendering`
- `feat(volumeprofile): add optional VWAP overlay`

---

## Commit ordering (patch-queue friendly)

Commits must be ordered from **lowest to highest divergence** from upstream.

Preferred order:
1. Localization / resources
2. UI / Display metadata
3. Internal refactors (no behavior change)
4. Calculation logic changes
5. New features

This ordering facilitates:
- auditing,
- rebasing,
- upstream cherry-picking.

---

## Scope discipline

- Do not introduce helpers, abstractions or plumbing unless they are:
  - used in the same commit, or
  - immediately consumed in the next commit with a clear dependency.
- Avoid cosmetic churn (formatting, renaming) without functional justification.
- Large changes must be explicitly justified in the commit message.

---

## Responsibility

If a change:
- increases future maintenance cost,
- introduces divergence from upstream,
- affects rendering semantics or trader interpretation,

it must be **explicitly stated** in the commit message or accompanying documentation.

Silence is not acceptable for impactful decisions.
