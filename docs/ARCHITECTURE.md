# Architecture - ATAS Indicators Repository

This document defines the **code architecture principles and recurring design patterns**
used across this repository for developing and modifying ATAS indicators.

It is intentionally based on **real, existing indicators**
(e.g. `ClusterStatistic`, `Volume`, `OHLCPlus`)
and not on abstract or idealized designs.

This document covers:

- indicator code architecture
- separation of responsibilities
- state, rendering and calculation patterns
- compatibility and adaptation rules
- localization as a presentation concern

This document does **not** define:

- branch strategy
- patch promotion workflow
- PR preparation rules
- repository integration flows

Those concerns belong in repository engineering documentation such as:

- `docs/branch-stacks.md`
- `docs/patch-registry.md`
- `CONTRIBUTING.md`

---

## 1. Architectural goals

All indicators in this repository should aim to satisfy:

- Clear separation between **calculation**, **state**, and **rendering**
- Deterministic behavior across:
  - historical bars
  - realtime bars
  - session boundaries
- Minimal implicit coupling between components
- Visual output that accurately reflects the calculated data
- Compatibility with upstream ATAS evolution
- Local compatibility adaptations that remain isolated and auditable

---

## 2. Separation of responsibilities

### 2.1 Calculation layer

Responsibilities:
- Compute values from market data
- Handle historical vs realtime logic explicitly
- Avoid direct interaction with rendering or UI state

Rules:
- No drawing logic in calculation paths
- No dependency on screen coordinates or visual properties
- Guard all divisions and derived metrics against zero or missing data

Observed patterns:
- `ClusterStatistic`: heavy calculation with multiple derived series
- `Volume`: minimal calculation, but strict dependency on bar lifecycle
- `OHLCPlus`: calculation tightly coupled to bar indexing and session logic

---

### 2.2 State management

Indicators must manage **explicit state**, not implicit assumptions.

Key distinctions:
- Historical vs realtime bars
- Session resets vs continuous accumulation
- Per-bar cached values vs recomputed values

Rules:
- State resets must be explicit and intentional
- Avoid relying on implicit ATAS behavior unless documented
- Series must be indexed consistently and predictably

Anti-patterns to avoid:
- Mixing session reset logic into rendering
- Hidden state changes triggered by visual flags
- Relying on `CurrentBar` side effects without guarding logic

---

### 2.3 Rendering layer

Responsibilities:
- Visualize already-calculated data
- Reflect sign, magnitude and hierarchy correctly
- Remain stable across zoom, scale and refresh events

Rules:
- Rendering must never change calculation results
- Rendering must tolerate missing or zero values
- Color, thickness and style must be derived from **meaning**, not convenience

Observed patterns:
- `ClusterStatistic`: color logic tied to semantic meaning
- `Volume`: minimal rendering but strict alignment with bars
- `OHLCPlus`: rendering directly mirrors OHLC semantics

---

## 3. Historical vs realtime behavior

Indicators must behave correctly in all phases:

- full historical load
- realtime bar updates
- session transitions

Rules:
- Realtime updates must not invalidate historical values
- Historical calculations must not depend on realtime-only data
- Any difference in behavior must be **explicitly documented**

Common risk:
- Indicators that look correct historically but drift or repaint in realtime

---

## 4. UI, parameters and resources

UI elements must be treated as **interfaces**, not logic drivers.

Rules:
- Parameters affect calculation configuration, not flow control
- UI changes must not alter indicator semantics unless explicitly intended
- Localization and resources (`Display`, `.resx`) must be isolated from logic

Commit discipline:
- UI/resource changes must not be mixed with calculation changes
- UI divergence from upstream must be intentional and documented

---

## 5. Compatibility and build-flavor adaptations

Compatibility adaptations must be treated as **isolated technical layers**, not as silent changes to indicator semantics.

Examples include:
- version shims
- API guards
- build-flavor conditionals
- stable/beta/alpha compatibility workarounds

Rules:
- Compatibility code must preserve functional intent whenever possible
- Version-specific behavior must be explicit and easy to locate
- Shims should isolate platform or API differences, not hide architectural problems
- Compatibility work must not leak unnecessarily into core calculation paths

Preferred pattern:
- keep core indicator logic stable
- isolate version or flavor differences behind narrow compatibility points
- document non-trivial behavior changes when exact parity is impossible

Anti-patterns to avoid:
- scattering flavor-specific conditionals throughout unrelated logic
- mixing compatibility code with unrelated refactors
- using shims to justify unclear semantics or broken abstractions

---

## 6. Modified indicators (`*Modif`)

Modified indicators are **derived variants**, not uncontrolled forks.

Rules:
- The original indicator logic should remain the conceptual reference unless an intentional divergence is documented
- Variants must preserve traceability between:
  - original implementation
  - renamed/derived artifact
  - local behavior changes
- Namespace, class name and display metadata may change as part of derivation
- Logic changes should be made in a controlled and auditable way before or during derivation, depending on the workflow in use

Guidelines:
- Avoid ad hoc manual divergence with no traceable rationale
- Keep variant-specific behavior explicit
- Do not treat `*Modif` files as a dumping ground for unrelated local experiments

The goal is controlled derivation, not permanent fork drift.

---

## 7. Acceptable patterns vs anti-patterns

### Acceptable
- Explicit state transitions
- Redundant guards for safety
- Clear naming of derived metrics
- Slight duplication to preserve clarity
- Narrow compatibility layers when required by ATAS version differences

### Anti-patterns
- Hidden coupling between render and calculation
- Implicit session assumptions
- Refactors that obscure trading meaning
- Visual tweaks that distort interpretation
- Compatibility code scattered across unrelated logic

---

## 8. Localization and resources

Localization in this repository follows ATAS conventions and is treated as a
presentation concern, not a logic driver.

### Core rules

- Indicator logic must never depend on language or culture
- All functional semantics must be invariant across locales
- The neutral resource value (`Resources.resx`) is mandatory and acts as the authoritative fallback

### Resource coverage

- Additional languages are provided on a best-effort basis, but:
  - any **new** key introduced in `Resources.resx` **must** be added to all satellite `.resx` files shipped by this repository
- Missing translations must not affect:
  - calculation
  - rendering
  - indicator behavior
- Resource completeness for existing keys is not required for architectural correctness, but PRs introducing new UI strings must not ship partial coverage

### Naming, reuse, and structure

- Prefer reusing existing keys when the meaning matches exactly
- Create new keys only when reuse would change meaning, cause ambiguity, or degrade UX clarity
- Resource keys must be stable, descriptive, and follow the existing naming style (PascalCase)
- Use `...Description` suffix for tooltip/description strings
- Do **not** include indicator names in resource key names when keys are intended to be reusable
- Technical identifiers (class names, enums, internal fields) must not be localized
- UI-facing strings must be isolated in resource files

### Comments and traceability

- Every `<data>` entry should include a `<comment>` node
- Prefer comments that identify the indicator or UI context where the key is used

### UX constraints

- Resource values should be short, scannable, and interpretable quickly under pressure
- Avoid long phrases in names shown in settings panels; prefer compact labels with unambiguous meaning

### Change isolation

Localization changes must not be mixed with:
- calculation logic
- rendering refactors
- architectural changes

If a feature requires both localization and logic:
- do localization first
- then implement the feature in separate commits

---

## 9. Architecture as a living document

This document is:
- authoritative
- versioned
- intentionally conservative

It should be updated when:
- a new recurring pattern emerges
- an existing rule is proven insufficient
- a compatibility pattern becomes common enough to deserve explicit guidance

Architecture changes must be justified by **real code**, not preference.

---

## 10. Architecture Decision Records (ADR)

Non-trivial technical or UX decisions should be documented using an ADR,
especially when they affect:

- multiple indicators
- compatibility strategy
- rendering semantics
- long-term repository conventions

See:

`docs/decisions/ADR-TEMPLATE.md`

