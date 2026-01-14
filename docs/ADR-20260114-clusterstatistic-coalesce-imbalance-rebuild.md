\# ADR: Coalesce imbalance rebuild in ClusterStatistic



Status: Postponed  

Priority: P2  

Date: 2026-01-14  



\## Context

ClusterStatistic rebuilds historical imbalances to compute background gradients based on maximum values.

Multiple UI setting changes can trigger repeated full-history rebuilds.



\## Problem

Rebuilding imbalances from individual property setters may cause unnecessary repeated recalculations and UI stutter.



\## Options considered

1\. Debounce rebuilds by time (e.g. delay execution).

2\. Coalesce rebuilds and execute once per recalculation cycle.

3\. Add an explicit Apply hook (not available in documented ATAS API).



\## Decision

Postpone.



\## Rationale

The change affects the calculation lifecycle and requires careful QA to avoid regressions in gradient correctness and performance.

Current focus is stabilizing upstream rebase changes.



\## Acceptance criteria

\- Single full-history imbalance rebuild per UI Apply / recalculation cycle.

\- Gradient maxima remain correct for the full session or loaded range.

\- No visual flicker or intermediate states.

\- Performance validated on large historical data.



\## Notes

Revisit once upstream changes are fully stabilized.



