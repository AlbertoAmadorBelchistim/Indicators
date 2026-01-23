# ADR-2026-01-23-10: Lifetime UI modes and snapshot migration

Status: Proposed
Date: 2026-01-23

## Context
Users need simple controls for how long a snapshot's levels remain valid (session, 30 minutes, daily). Older snapshots (or text-only inputs) need a migration/default rule.

## Decision
Add UI lifetime controls:
- `LifetimeMode`:
  - Session (RTH day)
  - Fixed duration (e.g., 30 minutes)
  - Daily
- Apply lifetime to new snapshots at creation time.
Define migration behavior:
- Existing snapshots without lifetime default to Daily (or Session) based on configuration.

## Rationale
- Keeps UX simple while enabling correct temporal behavior.
- Provides safe defaults for existing data.

## Scope / Non-goals
- Not adding custom per-level lifetimes in MVP.
- Not building complex schedule editors.

## Implementation Notes
- UI strings localized.
- When saving snapshots, compute ValidFrom/ValidTo according to selected mode.
- Provide an explicit migration step or auto-upgrade on load.

## Consequences
- Requires stable session boundary logic (time zone).
- Migration must be deterministic to avoid user confusion.

## Alternatives Considered
- Require user to re-save all snapshots: unacceptable friction.

## Follow-ups
- Potential advanced mode: per-category lifetimes (future ADR).
