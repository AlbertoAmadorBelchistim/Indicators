# ADR-2026-01-23-04: Add JSON / API source

Status: Proposed
Date: 2026-01-23

## Context
Some providers (e.g., MenthorQ-like services) deliver levels as JSON with periodic updates and rate limits.
GammaLevels currently supports text inputs but not network-based sources.

## Decision
Introduce a new optional source type:
- `JsonApiSource` implementing ILevelsSource
- Fetches JSON on a controlled schedule
- Parses into ParsedEntry with stable SourceId
- Supports throttled warnings and resiliency (fallback / last-known snapshot)

## Rationale
- Enables near-real-time updates.
- Reduces manual copy/paste workflows.
- Aligns with multi-source pipeline design.

## Scope / Non-goals
- Not implementing a specific provider contract unless documented.
- Not embedding secrets in indicator settings (must follow secure patterns).

## Implementation Notes
- UI:
  - Enable toggle
  - Endpoint / token (if allowed) and refresh interval
- Caching:
  - Store last successful payload and parse results
- Rate limiting:
  - Conservative defaults
- Error handling:
  - Throttled warnings, no render crashes

## Consequences
- Requires careful threading/model integration (ATAS constraints).
- Needs clear documentation about refresh and reliability.

## Alternatives Considered
- External tooling that converts JSON to text: less integrated, more friction.

## Follow-ups
- Add provider-specific adapters (separate commits/ADRs).
