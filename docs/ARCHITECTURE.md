# Architecture — ATAS Indicators Repository

This document defines the **architectural principles and recurring patterns**
used across this repository for developing and modifying ATAS indicators.

It is intentionally based on **real, existing indicators**
(e.g. `ClusterStatistic`, `Volume`, `OHLCPlus`)
and not on abstract or idealized designs.

The goal is to ensure:
- conceptual correctness,
- auditability (including upstream ATAS review),
- and long-term maintainability.

---

## 1. Architectural goals

All indicators in this repository should aim to satisfy:

- Clear separation between **calculation**, **state**, and **rendering**
- Deterministic behavior across:
  - historical bars,
  - realtime bars,
  - and session boundaries
- Minimal implicit coupling between components
- Visual output that accurately reflects the calculated data
- Compatibility with upstream ATAS evolution

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
- `ClusterStatistic`: color logic tied to semantic meaning (delta sign)
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
- UI/resource changes must not be mixed with calculation or refactors
- Any UI divergence from upstream must be intentional and documented

---

## 5. Modified indicators (`*Modif`)

Modified indicators are **derived artifacts**, not independent forks.

Rules:
- The source of truth is always the original indicator in `prready/main`
- `*Modif` versions:
  - change namespace
  - change public class name
  - adjust display metadata
- Logic changes must happen **before derivation**

Manual editing of `*Modif` files is forbidden.

---

## 6. Acceptable patterns vs anti-patterns

### Acceptable
- Explicit state transitions
- Redundant guards for safety
- Clear naming of derived metrics
- Slight duplication to preserve clarity

### Anti-patterns
- Hidden coupling between render and calculation
- Implicit session assumptions
- Refactors that obscure trading meaning
- Visual tweaks that distort interpretation

---

## 7. Architecture as a living document

This document is:
- authoritative,
- versioned,
- and intentionally conservative.

It should be updated only when:
- a new recurring pattern emerges,
- or an existing rule is proven insufficient.

Architecture changes must be justified by **real code**, not preference.

---

## 8. Localization and resources

Localization in this repository follows ATAS conventions and is treated as a
presentation concern, not a logic driver.

### Core rules

- Indicator logic must never depend on language or culture.
- All functional semantics must be invariant across locales.
- The neutral resource value is mandatory and acts as the authoritative fallback.

### Resource coverage

- Additional languages are provided on a best-effort basis.
- Missing translations must not affect:
  - calculation,
  - rendering,
  - or indicator behavior.
- Resource completeness is not required for architectural correctness.

### Naming and structure

- Resource keys must be stable and descriptive.
- Technical identifiers (class names, enums, internal fields) must not be localized.
- UI-facing strings must be isolated in resource files.

Localization changes must not be mixed with:
- calculation logic,
- rendering refactors,
- or architectural changes.
