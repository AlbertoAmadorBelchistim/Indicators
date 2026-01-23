# ADR-2026-01-23-01: Level filters by type and count (Top-N)

Status: Proposed
Date: 2026-01-23

## Context
GammaLevels can aggregate many levels across sources. In high-volatility sessions, the chart may become cluttered and reduce readability for scalping workflows.

## Decision
Introduce optional filtering controls:
- Filter by LevelCategory (per-category toggles or a compact selection).
- Limit the number of rendered levels (global cap).
- Add an optional "Top-N Gamma" filter for LargeGamma / key gamma categories.

Filtering will be applied after parsing and conflict resolution, but before rendering.

## Rationale
- Improves chart readability and reduces cognitive load.
- Keeps engine deterministic by applying filters at a single stage.
- Allows quick product-level control without modifying parsers or sources.

## Scope / Non-goals
- Not changing parsing or label mapping.
- Not implementing per-price clustering; only selection filters.

## Implementation Notes
- Add UI:
  - `MaxLevels` (int, 0 = unlimited)
  - Optional category toggles (or a bitmask-style setting)
  - `TopNGamma` (int, 0 = disabled)
- Apply filters on the final `Level[]` list, preserving priority ordering rules.

## Consequences
- Less clutter, more stable UX.
- Some levels may be hidden; must be clearly documented in UI descriptions.

## Alternatives Considered
- Render everything and rely on zoom: insufficient for scalpers.
- Only Top-N overall: loses category-specific control.

## Follow-ups
- Consider a small on-chart indicator of "X levels hidden" (optional).
