# ADR-2026-01-23-07: MergeCloseLevelsTicks (merge nearby levels, global)

Status: Proposed
Date: 2026-01-23

## Context
When multiple sources provide similar levels (often within a few ticks), lines and labels can become visually stacked and unreadable. Even if concepts differ, close proximity can harm chart usability.

## Decision
Add `MergeCloseLevelsTicks` (int, default 0).
When > 0, levels with prices within the tick threshold are merged into one rendered level:
- Combine labels (union)
- Winner selection uses existing priority rules
- Price chosen by precedence rules (SourceTruth / highest-priority label), then normalized to tick

## Rationale
- Improves readability in dense zones.
- Reduces label collisions and "hairball" effect near key levels.

## Scope / Non-goals
- Not a clustering algorithm for all market structure.
- Not guaranteed to preserve every micro-level; this is a UX feature.

## Implementation Notes
- Apply after semantic conflict resolution and filtering.
- Sort by price; sweep merge window.
- Use instrument TickSize to convert ticks -> price delta.
- Ensure deterministic behavior.

## Consequences
- Potential loss of granularity (intentional).
- Must document that merge is a visualization aid.

## Alternatives Considered
- Merge only within same concept: safer but may still leave unreadable stacks.

## Follow-ups
- Add optional "merge only labels, keep strongest line" style.
