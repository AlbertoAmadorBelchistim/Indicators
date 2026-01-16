# ADR-003: UI refresh throttling for high-frequency updates

Status: Accepted  
Priority: P2  
Date: 2026-01-15  

## Context
MultiMarketPower performs frequent updates in response to cumulative trades,
tick-level data, and in-bar recalculations.

These updates may trigger repeated calls to UI refresh mechanisms such as
`RaiseBarValueChanged`.

## Problem
In high-activity markets (e.g. ES/NQ), repeated redraw requests could introduce
unnecessary CPU usage or UI stutter.

## Options considered
1. Throttle UI refreshes using time-based debouncing.
2. Coalesce refreshes based on state changes.
3. Keep the current behavior without throttling.

## Decision
Option 3 — keep the current behavior.

## Rationale
The current implementation favors correctness, determinism, and immediate visual
feedback.

No consistent performance issues have been observed that justify introducing
additional timing or state complexity at this stage.

## Acceptance criteria
- UI updates remain visually stable and deterministic.
- No delayed or skipped visual states occur.
- Performance remains acceptable under normal and high-activity conditions.

## Notes
Revisit if users report CPU pressure or observable UI lag during sustained
high-frequency updates.
