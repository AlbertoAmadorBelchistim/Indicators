# Feature Branch Dependency Guide

**Purpose:** Reusable decision framework for structuring `feat/` branches when porting or developing
new indicator features. Answers: *should this branch stand alone on `Develop`, or should it stack
on another `feat/` branch?*

---

## 1. The core question

> "Could upstream accept this feature **without** accepting any other new feature first?"

If yes → branch from **`Develop`** (fully standalone).
If no → branch from the **minimal prerequisite** `feat/` branch (stacked).

---

## 2. Decision checklist (run per feature)

For each new feature extracted from a `*Modif` file or from `prready/main`:

### 2.1 Identify what the feature needs at runtime

List every field, method, or series the feature reads or writes that **does not exist on `Develop`**.

Example — `feat/delta-price-signals`:
- Reads `PickUpThreshold(bar, level)` → not on Develop
- Reads `UpMajorLevel` / `UpMinorLevel` → not on Develop (introduced by fixed thresholds)
- Conclusion: **depends on** `feat/delta-threshold-selection`

### 2.2 Could the feature work with a degraded fallback?

Sometimes a feature can be standalone if it falls back to a simpler value (e.g., comparing against
`UpAlert.Value` instead of `PickUpThreshold`). Ask:

- Does the fallback preserve the feature's core purpose?
- Does the fallback degrade the reviewer's understanding of the feature?
- Would the fallback need to be **removed** before merging the dependency?

If a fallback would require a follow-up cleanup commit in the same PR, it is not worth using.
If the feature genuinely works without the dependency (different behavior, not degraded), it can
stand alone.

### 2.3 Classify the dependency type

| Type | Description | Branch topology |
|------|-------------|----------------|
| **Data dependency** | Feature reads fields/series introduced by another feature | Stack on that feature |
| **Semantic dependency** | Feature's meaning changes completely without prior feature | Stack on that feature |
| **Cosmetic dependency** | Feature looks better with another feature but works independently | Standalone — document the relationship |
| **No dependency** | Feature uses only what exists on Develop | Standalone from Develop |

---

## 3. Resulting branch topologies (valid patterns)

### Pattern A — All standalone
```
Develop
  ├── feat/indicator-feature-1
  ├── feat/indicator-feature-2
  └── feat/indicator-feature-3
```
Best for: features that are truly orthogonal. Reviewer can pick any subset.

### Pattern B — Partial stack
```
Develop
  ├── feat/indicator-base          ← introduces shared primitives
  │     ├── feat/indicator-feat-x  ← depends on base
  │     └── feat/indicator-feat-y  ← depends on base, independent of feat-x
  └── feat/indicator-feat-z        ← fully standalone
```
Best for: one "infrastructure" feature (types, fields, pickers) plus several consumers of it.
Each consumer is still independently evaluable if the base is accepted.

### Pattern C — Linear stack
```
Develop
  └── feat/indicator-base
        └── feat/indicator-mid
              └── feat/indicator-top
```
Avoid unless each level genuinely adds value independently. Deep stacks make PRs harder to review.

---

## 4. PR submission strategy for stacked branches

Stacked PRs are legitimate. The pattern:

1. Submit the base branch as PR #1.
2. Submit each dependent branch as a subsequent PR, with the description stating:
   *"Depends on #1. Can be reviewed independently once #1 is accepted."*
3. Upstream can evaluate features in parallel, but will merge in order.

This is preferable to:
- Bundling all features into one large PR (loses evaluability)
- Waiting for PR #1 to merge before submitting PR #2 (wastes time)

---

## 5. Integration branch (`local/<indicator>-i18n`) responsibility

The integration branch is **not** simply a merge of all feat branches. It is responsible for:

1. **Wiring** — connecting features that are independent at the feat level but interact at runtime.
   Example: `PickUpThreshold` is defined in `feat/delta-threshold-selection` (fixed-only), but
   the dynamic case (reading `_upMajor[bar]`) is only wired in `local/delta-i18n` once both
   `feat/delta-fixed-thresholds` and `feat/delta-dynamic-thresholds` are merged.

2. **Localization** — converting all hardcoded English strings to `typeof(Resources)`.

3. **Session cuts** — `CutAllThresholdsAt`, `CutUpThresholdsAt`, `CutDownThresholdsAt` that depend
   on the full series set being present.

4. **Any behavior that requires simultaneous presence of multiple feat branches.**

The integration branch must reach functional parity with the published `*Modif` file.
Document any remaining gaps in `Section 6` of the port manifest before marking complete.

---

## 6. Delta — reference implementation (confirmed 2026-03-31)

### Feature inventory and branch topology

```
Develop
  ├── feat/delta-average-line              (standalone — uses only Develop APIs)
  ├── feat/delta-fixed-thresholds          (standalone — introduces _upMajor/_upMinor/_dnMinor/_dnMajor + UpMajorLevel etc.)
  │     └── feat/delta-threshold-selection  (needs UpMajorLevel etc. → stack on fixed-thresholds)
  │           ├── feat/delta-price-signals   (needs PickUpThreshold → stack on threshold-selection)
  │           └── feat/delta-audio-alerts    (needs PickUpThreshold → stack on threshold-selection, independent of price-signals)
  └── feat/delta-dynamic-thresholds        (standalone — adds WelfordAcc machinery; scalar only, no _upMajor writes)

local/delta-i18n  (stacked on local/build/04-localization)
  Merges all feat/* above, then adds:
  - i18n conversion (typeof(Resources) for all new keys)
  - Dynamic case in PickUpThreshold: reads _upMajor[bar] / _upMinor[bar] etc. (series exist after merge)
  - CutAllThresholdsAt / CutUpThresholdsAt / CutDownThresholdsAt
  - UpdateDynamicThresholdState writes _upMajor[bar] / _upMinor[bar] etc. per bar
  - Full parity with compile/myindicators:MyIndicators/DeltaModif.cs
```

### Why `feat/delta-dynamic-thresholds` is standalone

Dynamic thresholds only add `WelfordAcc`, `_posAcc`/`_negAcc`, accumulation logic, and scalar
`_dynPosMinor`/`_dynPosMajor`/`_dynNegMinor`/`_dynNegMajor` fields. It does **not** write to
`_upMajor[bar]` etc. (those series don't exist on Develop). The writing to series and the
`PickUpThreshold` dynamic case are integration-layer concerns (`local/delta-i18n`).

### Why `feat/delta-audio-alerts` and `feat/delta-price-signals` are siblings

Both consume `PickUpThreshold(bar, level)` from `feat/delta-threshold-selection`. Neither requires
the other. Upstream can accept visual signals without audio, or audio without visual signals.

---

## 7. Applying this model to future indicators

For each new indicator port (OHLCPlus, ClusterStatistic, etc.):

### Step 1 — Feature extraction
List all features new vs upstream Develop. Source: `prready/main` diff + `*Modif` inventory.

### Step 2 — Dependency matrix
For each feature pair (A, B): does A require B to exist? Build a directed dependency graph.

### Step 3 — Topology selection
- No deps → all standalone (Pattern A)
- One infrastructure feature + N consumers → Pattern B
- True sequential chain → Pattern C (accept the PR submission ordering cost)

### Step 4 — Document in port manifest
Add a "Branch topology" section (§7) to the indicator's port manifest on `meta/docs`.

### Step 5 — Integration branch design
List explicitly what the integration branch adds beyond the feat branches:
wiring, localization, session cuts, cross-feature interactions.

---

*Last updated: 2026-03-31*
