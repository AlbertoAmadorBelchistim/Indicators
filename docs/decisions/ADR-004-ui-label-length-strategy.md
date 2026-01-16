# ADR-004: UI label length strategy across languages

Status: Accepted  
Priority: P3  
Date: 2026-01-15  

## Context
Some UI labels in MultiMarketPower, particularly those related to Smart Money
Spread color configuration, were long enough to cause line wrapping in the
settings panel.

This negatively affected readability during active trading.

## Problem
Long, descriptive labels reduce scan speed and increase cognitive load,
especially when multiple color configuration entries are displayed together.

## Options considered
1. Keep verbose labels for clarity across all languages.
2. Shorten labels uniformly across all languages.
3. Shorten labels selectively where visual validation is possible.

## Decision
Option 3 — shorten labels selectively.

## Rationale
Visual validation was performed only for languages using the Latin alphabet
(e.g. English and Spanish).

Shortening labels in other languages without visual confirmation risks degrading
readability or introducing ambiguity.

## Acceptance criteria
- Reduced label wrapping in validated languages.
- Clear semantic meaning preserved in shortened labels.
- No regression in languages not explicitly reviewed.

## Notes
Perform a dedicated localization and UX review across all supported languages
before harmonizing label length globally.
