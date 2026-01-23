# ADR-2026-01-23-09: Implement ValidFrom/ValidTo + render filtering

Status: Proposed
Date: 2026-01-23

## Context
Level lifetime metadata is only useful if rendering enforces it. GammaLevels render currently draws all available levels regardless of time.

## Decision
Implement render-time filtering:
- Compute visible chart time range (first/last visible bar time).
- Only draw levels where:
  - `ValidFrom <= visibleEnd` AND `ValidTo >= visibleStart`

## Rationale
- Ensures correct temporal accuracy.
- Reduces clutter for multi-day charts.

## Scope / Non-goals
- Not altering label priority logic.
- Not adding complex partial visibility logic beyond intersection.

## Implementation Notes
- Determine bar timestamps safely via ATAS APIs.
- Cache computed visible range per render call.
- Ensure the filter does not allocate heavily.

## Consequences
- Additional logic in render pipeline.
- Must handle missing lifetime (fallback: treat as always valid or session-only).

## Alternatives Considered
- Filter only on current bar time: incorrect for historical viewing.

## Follow-ups
- UI lifetime modes and snapshot migration.
