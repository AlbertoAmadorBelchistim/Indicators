# ADR-018: AccountInfoDisplay — Per-account persistence with event log + daily snapshots + reset epochs

Status: Proposed  
Priority: P0  
Date: 2026-02-12  

## Context
`AccountInfoDisplay` is used for prop-firm risk management where correctness is critical.
The indicator must support:
- Multiple accounts used in the same ATAS session.
- Switching accounts without mixing configuration or runtime state.
- Persistence across ATAS restarts, preserving risk-critical state (drawdown, rails, streaks, etc.).
- Auditability: ability to explain "why" a risk status happened.

The current codebase already contains runtime per-account state for some parts (e.g., trailing DD),
but other modules (soft recommendations, daily rails/session metrics, etc.) are not fully isolated or persistently stored per account/day.

## Problem
We need a persistence model that is:
- Strictly isolated by account (no cross-account contamination).
- Deterministic across restarts.
- Auditable and recomputable to avoid "phantom states" or incorrect snapshots.
- Safe with respect to prop-firm resets: once an account is reset (manual or calendar-based), the indicator must not recompute or mix data from before the reset into the new tracking period.

Without a clear contract, the indicator risks producing incorrect risk rails/stop decisions,
which is unacceptable in prop trading.

## Options considered
1. Single thematic JSON file for all accounts (config + runtime + history).
2. Per-account JSON file(s) storing current runtime state only (no event history).
3. Per-account persistence with:
   - Per-account config file (JSON),
   - Per-account trade event log (append-only JSONL),
   - Per-account daily snapshots (JSON per day),
   - Account "epochs" delimiting resets, allowing snapshot recomputation within the active epoch.

## Decision
Adopt Option 3:
- Partition all data by `AccountKey` (primary partition key).
- Add an `AccountEpochId` per account to represent a reset boundary.
  - A reset creates a new epoch and becomes the "point of no return".
  - The indicator does not recompute or read prior epochs in normal operation.
- Persist:
  1) `AccountConfig` per account (JSON),
  2) Trade close events per account (JSONL, append-only),
  3) Daily snapshots per account-day (JSON).
- Allow recomputation of daily snapshots from the event log **within the active epoch**.

## Rationale
- Per-account files enforce isolation and reduce the blast radius of corruption/bugs.
- JSONL (append-only) supports auditability and avoids expensive rewrites.
- Daily snapshots provide fast startup and stable day-level state without minute granularity.
- Recomputing snapshots from events (within the epoch) avoids "silent drift" and enables integrity checks.
- Introducing epochs respects real prop-firm resets: after a reset, historical data is no longer relevant for live risk decisions and must not influence new tracking.
- This model balances correctness, maintainability, and performance.

Trade-offs:
- More files on disk (manageable; improves safety and clarity).
- Requires careful schema/versioning and atomic write strategy for snapshots/config.
- Requires strict definition of what constitutes a "trade event" and how to detect trade boundaries.

## Acceptance criteria
1) **Isolation**
- Switching accounts restores the correct runtime state and config for the target account.
- No UI parameter change in account A affects account B.

2) **Persistence**
- After restarting ATAS, account config and the latest daily snapshot load correctly for each account.
- The indicator can continue tracking without losing rails, streaks, or trailing DD state.

3) **Event log and recomputation**
- For a given account and day within the same epoch, recomputing the daily snapshot from the JSONL events
  yields the same snapshot as the one persisted at runtime (within defined tolerance if needed).
- Event log is append-only; no in-place edits are required in normal operation.

4) **Reset boundary (epochs)**
- Performing an account reset (manual or scheduled) increments `AccountEpochId`.
- After epoch change, the indicator does not apply events/snapshots from previous epochs to the active one.
- Files are stored with epoch in the name or metadata such that mixing is prevented.

5) **Safety**
- Config and snapshot writes use safe-write semantics (write temp + atomic replace).
- Corrupted or missing files fail gracefully with conservative defaults (no optimistic risk relaxation).

## Notes
- File naming suggestion (final to be decided during implementation):
  - Config: `AccountInfoDisplay.account.<AccountKey>.epoch.<EpochId>.config.v1.json`
  - Events: `AccountInfoDisplay.account.<AccountKey>.epoch.<EpochId>.trades.v1.jsonl`
  - Snapshot: `AccountInfoDisplay.account.<AccountKey>.epoch.<EpochId>.daily.<YYYYMMDD>.v1.json`
- "Day" is defined using local date derived from `InstrumentInfo.TimeZone` offset used in the indicator.
- "Trade event" is recorded on position close (open → flat), with sufficient metadata to rebuild daily metrics.
- Snapshot finalization must be deterministic (preferably boundary crossing, not "any time after").
