# ADR-2026-01-23-03: Optimize MenthorQ parser (no regex/split, low allocations)

Status: Proposed
Date: 2026-01-23

## Context
MenthorQText parsing currently uses string splits and potentially regex, which may allocate heavily for large inputs. Typical use is small, but product robustness requires handling larger pasted texts without UI lag.

## Decision
Refactor MenthorQ parser to a streaming approach:
- Avoid regex in the hot path.
- Replace Split-based parsing with manual scanning (span-like) to reduce allocations.
- Keep behavior identical (mapping, ticker prefix, 0DTE markers, warnings).

## Rationale
- Lower allocations and improved responsiveness.
- Deterministic parsing with fewer intermediate strings.
- Easier to add validation and better error reporting.

## Scope / Non-goals
- Not changing level semantics or mapping.
- Not changing logging/warnings format unless explicitly documented.

## Implementation Notes
- Implement tokenization via index scanning.
- Normalize whitespace and separators consistently.
- Add targeted unit-like test vectors (inputs -> parsed entries count + warnings count).

## Consequences
- More complex parser code; must be well-commented (why, not what).
- Requires careful regression testing.

## Alternatives Considered
- Keep current parser: acceptable for MVP, but risks UI freezes on large pastes.

## Follow-ups
- Similar optimization can be applied to LoloText if needed.
