# ADR-006: Session window and timezone policy for dynamic threshold anchoring

Status: Proposed  
Priority: P0  
Date: 2026-01-16  

## Context
Delta dynamic thresholds (Welford-based signed thresholds) are anchored to a session window (e.g., RTH vs full 24h) and explicitly enforce “no look-ahead” by using b-1 state to compute thresholds for bar b.

Session checks currently depend on:
- candle timestamps (often exchange/UTC in ATAS),
- `InstrumentInfo.TimeZone` offsets,
- user-configured RTH start/end times,
- session start detection and threshold line cutting outside session.

This impacts scalping trust: thresholds and signals must be stable, deterministic, and non-repainting under real-time conditions.

## Problem
Session/timezone handling is subtle:
- DST transitions can shift perceived local times.
- Instruments can have different exchange sessions or non-equity schedules.
- If time interpretation differs across builds/symbols, the dynamic threshold window can drift, causing unexpected triggers or gaps.
- Session boundaries affect accumulator resets and line cutting; mistakes here undermine trust.

Without a documented policy, future refactors or ports can unintentionally change behavior.

## Options considered
1. Option A — Define a strict policy: treat candle time as exchange UTC; convert using `InstrumentInfo.TimeZone`; apply RTH window in that local time; reset on session entry; cut lines outside session.
2. Option B — Use ATAS-provided session/calendar utilities (if available and stable across builds) to resolve sessions and DST robustly.
3. Option C — Treat session as “Full24h only” for dynamic thresholds to avoid TZ complexity (reduces feature value).

## Decision
Adopt **Option A as the documented baseline policy**, with an explicit note that Option B may replace it if a stable, documented ATAS utility exists across target builds.

Implementation is already aligned with Option A; this ADR formalizes it as “source of truth” to prevent accidental drift.

## Rationale
Option A is:
- deterministic,
- locally testable (bar timestamps + configured window),
- compatible with patch-queue/compat branches (no dependency on undocumented utilities).

Option B can be superior for DST correctness, but it requires a confirmed, documented API surface across builds; otherwise it introduces fragility.

Option C avoids complexity but removes a key feature used for intraday scalping discipline (session-anchored statistics).

## Acceptance criteria
- Dynamic thresholds are computed using **b-1** state (no look-ahead).
- Outside session window: thresholds do not trigger signals/alerts and threshold lines are cut (no misleading flatlines).
- Session start resets accumulators exactly once per session entry (no double resets).
- Real-time behavior matches historical replay (deterministic given the same data).
- DST transition behavior is documented (expected behavior) and does not produce crashes; any known limitations are stated.

## Notes
- If ATAS session utilities are later adopted (Option B), create a follow-up ADR: “Replace manual session window with ATAS session calendar API” and include migration/compat plan.
- Consider adding a minimal automated test harness (where feasible) using recorded timestamps around session boundaries (RTH open/close, DST day).
