# DailyLines — Production Test Report v1

Version tested: prready-DailyLines-multiscope-eth-v1  
Upstream base: <commit hash>  
Date: <date>  
Environment: ATAS <version>  
Instrument: ES  
Timeframes tested: M1, M5, M15  
Data provider: Rithmic

---

## 1. Build & Load

- Clean build (Debug): PASS
- Clean build (Release): PASS
- Indicator loads without runtime errors: PASS

---

## 2. Legacy Mode (UseMultiScope = false)

- CurrentDay parity: PASS
- PreviousDay parity: PASS
- CurrentWeek parity: PASS (see DL-001)
- PreviousWeek parity: PASS
- CurrentMonth parity: PASS
- PreviousMonth parity: PASS

Notes:
- No visible regression from upstream baseline.

---

## 3. Multi-Scope Mode

- Independent scope toggles working: <PASS/FAIL>
- Fixed render order respected: <PASS/FAIL>
- No duplicate labels: <PASS/FAIL>
- Fallback when all toggles off: <PASS/FAIL>

---

## 4. RTH (CustomSession)

- Window filtering correct: <PASS/FAIL>
- TradingDayStart anchor correct: <PASS/FAIL>
- PreviousDay skip-empty works: <PASS/FAIL>

---

## 5. ETH Scope

- ETH window = TradingDayStart → RTH start: <PASS/FAIL>
- ETH requires CustomSession: <PASS/FAIL>
- ETH does not include 16:00–18:00: <PASS/FAIL>

---

## 6. HalfGap (Option A)

- Only appears for CurrentDay + CustomSession: <PASS/FAIL>
- Does not duplicate in multi-scope: <PASS/FAIL>
- Repaints immediately on toggle: <PASS/FAIL>

---

## 7. Render Stability

- Scroll stability: <PASS/FAIL>
- Zoom stability: <PASS/FAIL>
- No out-of-bounds errors: <PASS/FAIL>

---

## 8. Realtime (if tested)

- Live bar updates correct: <PASS/FAIL>
- Trading day transition stable: <PASS/FAIL>

---

## 9. Known Issues

- DL-001 (Incomplete week open)
- DL-002 (Period vs MultiScope UX)
- DL-009 (Per-scope styling)

---

## Final Status

☐ Not production ready  
☐ Production ready with minor UX debt  
☐ Production ready  

Decision: <fill>