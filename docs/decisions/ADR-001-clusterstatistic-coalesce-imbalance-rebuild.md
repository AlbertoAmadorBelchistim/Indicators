# ADR-001: Coalesce imbalance rebuild in ClusterStatistic

Status: Postponed  
Priority: P2  
Date: 2026-01-14  

## Context
ClusterStatistic rebuilds historical imbalances to compute background gradients
based on maximum imbalance values across the loaded range or session.

Imbalance rebuilds are computationally expensive and involve a full historical
pass over the data series.

## Problem
Multiple UI property changes can independently trigger full imbalance rebuilds
from individual setters.

This may lead to:
- repeated full-history recalculations,
- unnecessary CPU usage,
- and visible UI stutter during configuration changes.

## Options considered
1. Debounce rebuilds using a time-based delay.
2. Coalesce rebuilds and execute once per recalculation cycle.
3. Introduce an explicit Apply action (not supported by the documented ATAS API).

## Decision
Postpone implementation.

## Rationale
The change affects the indicator calculation lifecycle and requires careful
validation to avoid regressions in:
- imbalance correctness,
- gradient maxima consistency,
- and visual stability.

Current focus is on stabilizing upstream rebase changes and ensuring deterministic
behavior before introducing lifecycle-level optimizations.

## Acceptance criteria
- A single full-history imbalance rebuild per UI Apply or recalculation cycle.
- Correct gradient maxima for the full session or loaded historical range.
- No intermediate visual states or flickering.
- Performance validated on large historical datasets.

## Notes
Revisit once upstream changes are fully stabilized and the calculation lifecycle
is considered frozen.




