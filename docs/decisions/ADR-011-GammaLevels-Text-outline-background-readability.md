# ADR-2026-01-23-05: Text outline/background for readability

Status: Proposed
Date: 2026-01-23

## Context
On busy charts, labels can be hard to read due to overlapping candles, heatmaps, and other indicators. GammaLevels currently draws text without a strong readability layer.

## Decision
Add optional label readability enhancements:
- Background (solid/transparent rectangle) behind text
- Or outline/stroke around glyphs (if feasible with ATAS rendering)

Provide a UI toggle and minimal configuration (opacity/padding).

## Rationale
- Improves readability for scalping.
- Reduces cognitive load when multiple levels are present.

## Scope / Non-goals
- Not redesigning label layout/placement algorithm.
- Not adding complex typography settings.

## Implementation Notes
- Introduce render option:
  - `LabelBackgroundEnabled` (bool)
  - Optional `LabelBackgroundOpacity`, `LabelPadding`
- Ensure caches invalidate with setting changes (`_visualDirty`).
- Keep performance acceptable (avoid per-frame allocations).

## Consequences
- Slight render cost increase.
- Must ensure correct behavior across zoom/scale.

## Alternatives Considered
- Rely on font size only: insufficient on complex charts.

## Follow-ups
- Combine with merge-close and filters to further reduce clutter.
