# ADR-2026-01-23-06: Snapshot persistence (storage model)

Status: Proposed
Date: 2026-01-23

## Context
GammaLevels currently works with live/pasted inputs only. Users need the ability to store and replay daily level sets (auditable snapshots), and later render levels only for their intended validity window.

## Decision
Introduce a snapshot persistence layer:
- Serialize "daily snapshot" objects:
  - timestamp
  - instrument/ticker
  - source payload(s) or parsed results
  - derived levels (optional)
- Store locally (file-based) with deterministic naming.

## Rationale
- Enables historical review and backtesting-like workflows.
- Provides auditability of levels used on a given day.
- Decouples fetching/parsing from rendering.

## Scope / Non-goals
- Not building a full database system.
- Not syncing across machines.

## Implementation Notes
- Define snapshot schema (versioned).
- Decide storage location and retention policy.
- Provide UI actions:
  - Save snapshot
  - Load snapshot (by date)
  - Clear snapshots (optional)

## Consequences
- Adds complexity and IO concerns.
- Needs careful versioning for forward compatibility.

## Alternatives Considered
- No persistence: limits product value for serious users.

## Follow-ups
- Level lifetime model and render filtering rely on snapshots.
