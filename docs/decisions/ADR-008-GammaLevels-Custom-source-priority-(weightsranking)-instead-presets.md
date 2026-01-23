# ADR-2026-01-23-02: Custom source priority (weights/ranking) instead of presets

Status: Proposed
Date: 2026-01-23

## Context
GammaLevels resolves semantic conflicts ("same concept, different price") using a SourceTruthPreset.
As the number of sources grows, fixed presets do not scale and cannot express all user preferences.

## Decision
Add an optional "Custom source priority" mode where users can set ranking/weights per source group:
- Lolo
- MenthorQ:Futures
- MenthorQ:Index
- Others (future sources)

When enabled, conflict resolution uses these weights instead of presets.

## Rationale
- Scales to multiple sources without multiplying presets.
- Enables fine-grained control (e.g., MenthorQ:Futures > Lolo > MenthorQ:Index).

## Scope / Non-goals
- Not redesigning source model or adding new sources here.
- Not providing per-category source truth (global only).

## Implementation Notes
- UI:
  - `UseCustomSourcePriority` (bool)
  - `PriorityLolo`, `PriorityMenthorQFutures`, `PriorityMenthorQIndex`, `PriorityOther` (int)
- Lower number = higher priority (consistent with current logic).
- Keep deterministic tie-breaking (do not replace if equal).

## Consequences
- More UI options; must keep defaults safe.
- Needs clear tooltip/docs to avoid misconfiguration.

## Alternatives Considered
- Add more presets: does not scale.
- Auto-weight by recency: introduces nondeterminism and hidden behavior.

## Follow-ups
- If additional sources appear, extend the priority set (backward compatible).
