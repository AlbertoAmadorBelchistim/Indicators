# ADR-05: Alert API compatibility wrapper for cross-build stability

Status: Proposed  
Priority: P1  
Date: 2026-01-16  

## Context
Delta indicators (and other core indicators) rely on ATAS alert infrastructure (`AddAlert` / alert file + colors).  
Across ATAS builds (alpha/beta/stable, legacy branches), the available `AddAlert` overloads and runtime behavior can differ. Delta currently contains a dedicated wrapper for audio alerts, while other alert paths may call `AddAlert` directly.

Alerts are not part of the computational core, but they are user-facing and must not break indicator execution or cause runtime exceptions during real-time trading.

## Problem
Direct `AddAlert(...)` calls can fail to compile or throw at runtime depending on the ATAS build/API surface (overload differences, missing parameters, changed signature).  
This creates:
- cross-branch maintenance friction (compat branches),
- risk of broken builds when backporting,
- potential runtime instability (alerts should never stop calculation/render).

## Options considered
1. Option A — Standardize a single `TryAddAlert(...)` wrapper for all alerts (audio + legacy + future channels).
2. Option B — Keep direct `AddAlert(...)` calls and adjust per-branch with `#if` or local patches.
3. Option C — Remove legacy alert paths and rely only on new audio channel (not always acceptable).

## Decision
Postpone implementation and document the standardization path: **Option A** is the preferred direction, but will be implemented only if real-time testing reveals compatibility issues or if the indicator becomes a “core” cross-build artifact.

## Rationale
Option A minimizes cross-build breakage and keeps alert code robust and isolated. It also aligns with patch-queue workflows: one stable abstraction reduces repeated conflict resolution across branches.

Option B is workable short-term but causes recurring churn (especially in patch queues) and increases maintenance cost.

Option C reduces surface area but may regress user workflows that rely on legacy alert configuration. It also conflates “feature evolution” with “compat hardening”.

Given Delta is now feature-complete and entering real-time validation, we postpone implementation to avoid introducing changes that could affect perceived behavior.

## Acceptance criteria
- The wrapper compiles on all supported ATAS branches (alpha/beta/stable/legacy targets used by this repo).
- All alert calls are routed through a single method (`TryAddAlert` / `TryAddAudioAlert` unified or layered).
- Any exception in alert infrastructure is caught and does not stop calculation/render.
- Behavior parity: alerts fire at the same bars/conditions as before the refactor.

## Notes
- If implemented, keep message formatting stable and avoid changing alert semantics in the same commit.
- Consider a small helper that selects the best available overload at compile-time, and fall back to the simplest overload where required.
