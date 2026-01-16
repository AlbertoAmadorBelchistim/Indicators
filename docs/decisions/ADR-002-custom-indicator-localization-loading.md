# ADR-002: Custom indicator localization loading in ATAS

Status: Accepted  
Priority: P2  
Date: 2026-01-15  

## Context
ATAS built-in indicators load localized resources from satellite assemblies
located in culture-specific subfolders (e.g. de-DE, fr-FR) under the ATAS
installation directory.

For custom indicators deployed under:
%AppData%\ATAS\Indicators

the standard .NET satellite probing mechanism does not appear to work reliably.
Placing `<IndicatorAssembly>.resources.dll` under culture subfolders does not
result in localized UI strings being loaded at runtime.

## Problem
Custom indicators cannot currently rely on standard satellite resource loading
for localization, leading to inconsistent or missing translations depending on
runtime loading behavior.

## Options considered
1. Accept limited localization for custom indicators.
2. Implement a custom `AssemblyResolve` hook for satellite assemblies.
3. Defer the problem until official ATAS guidance or API support exists.

## Decision
Option 1 — accept limited localization for now.

## Rationale
Implementing a custom assembly resolution mechanism increases complexity and
maintenance cost, and risks subtle loading issues.

Current priority is indicator stability and feature parity, not full localization
in all custom deployment scenarios.

## Acceptance criteria
- Neutral resources (`Resources.resx`) are always available.
- Additional language resources may exist without breaking runtime behavior.
- No custom assembly loading logic is required.

## Notes
Revisit if ATAS documents or exposes a supported localization mechanism for
custom indicators.
