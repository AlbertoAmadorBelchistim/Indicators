# DailyLines — Production Issues Log

> Scope: DailyLines (multi-scope + ETH)  
> Baseline: prready-DailyLines-multiscope-eth-v1  
> Upstream base: <rellenar>  
> Status legend:  
> P0 = crash / data corruption  
> P1 = wrong calculation  
> P2 = functional/UX inconsistency  
> P3 = cosmetic / polish  

| ID | Sev | Area | Symptom | Repro | Expected | Actual | Classification | Fix Plan | Retest Scope |
|----|-----|------|---------|-------|----------|--------|---------------|-----------|---------------|
| DL-001 | P1 | Data completeness | Week open incorrect when only 4 days loaded | Load only 4 days of data, enable CurrentWeek | Should either show real week open or warn "incomplete" | Shows first loaded bar open | Upstream design gap (no history completeness guard) | Add "Insufficient history" detection OR optional strict mode | C, D, J |
| DL-002 | P2 | UX | Period vs MultiScope confusing | Open settings | Clear mode separation | Redundant fields | Product design debt | MS-5: collapse Period or auto-enable multi | D, UI |
| DL-003 | P1 | MultiScope | Toggled scopes sometimes not rendering | Enable multi-scope and activate toggles | Scope renders immediately | Nothing renders | Possibly GetActiveScopes / state.OpenBar guard | Investigate state init and fallback logic | D, I |
| DL-004 | P2 | UX | "Custom Session" naming unclear (should be RTH) | View settings | Clear RTH terminology | Ambiguous | UX naming issue | Rename display strings only (keep property name) | UI only |
| DL-005 | P2 | Feature gap | No independent toggles for RTH vs full day | Enable CurrentDay + CustomSession | Ability to separate full-day from RTH | Not possible | New feature (not bug) | Consider adding RTH scope separately | D |
| DL-006 | P1 | TradingDayStart | TradingDayStart not modifiable | Change TradingDayStart in UI | Recalculate & redraw | No effect | Event hook issue (was missing _tradingDayStart handler) | Confirm handler working | F |
| DL-007 | P2 | UX text | "First bar" term unclear | Inspect UI | Clear terminology | Ambiguous | Copy issue | Replace with clearer description | UI |
| DL-008 | P1 | Render | HalfGap does not repaint when enabled | Toggle ShowHalfGap | Immediate redraw | No repaint | Missing redraw trigger | Ensure RecalculateValues + RedrawChart | H |
| DL-009 | P3 | Styling | Line color/name change applies to all scopes | Modify style | Per-scope control | Global control only | Feature gap | Per-scope LevelMask & Style (large feature) | D, H |
