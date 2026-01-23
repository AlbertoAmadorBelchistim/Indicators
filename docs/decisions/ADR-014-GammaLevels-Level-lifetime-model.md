# ADR-2026-01-23-08: Level lifetime model (ValidFrom/ValidTo)

Status: Proposed
Date: 2026-01-23

## Context
Once snapshots exist, levels should not live forever. Each snapshot/day may have levels valid for a session, 30 minutes, or other durations. Rendering must respect intended validity.

## Decision
Extend the level model to include lifetime metadata:
- `ValidFrom` (DateTime)
- `ValidTo` (DateTime)
Optionally:
- `SessionId` or `TradingDay` marker

Rendering filters levels by whether their lifetime intersects the visible time range.

## Rationale
- Prevents stale levels from accumulating indefinitely.
- Enables correct historical replay.

## Scope / Non-goals
- Not computing lifetimes automatically from market behavior (rule-based only).

## Implementation Notes
- Store lifetime in snapshot entries or derived Level objects.
- Provide deterministic time zone handling.
- Ensure lifetime survives serialization.

## Consequences
- Requires careful date/session conventions.
- Changes internal model and render pipeline.

## Alternatives Considered
- Keep everything forever: not acceptable for serious workflow.

## Follow-ups
- Implement render filtering (ADR-09) and UI (ADR-10).
