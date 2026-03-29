# Smoke test — {{IndicatorName}}

**Status:** pending | pass | fail
**Tested on:** ATAS Platform vX.X.X / branch `local/{{indicator}}-i18n` / date YYYY-MM-DD
**Tester:** manual (user in ATAS Platform)

---

## 0. Pre-test

- [ ] Build passes with 0 errors: `dotnet build Technical/Indicators.csproj -c Alpha -v quiet`
- [ ] DLL copied to `%APPDATA%\ATAS\Indicators`
- [ ] ATAS Platform restarted

---

## 1. Load and initialize

- [ ] Indicator appears in the indicator list under the expected category
- [ ] Indicator can be added to a chart without error
- [ ] Indicator panel renders (chart does not crash or blank out)
- [ ] No errors in ATAS log on load

---

## 2. Property panel

- [ ] All parameter groups appear with correct labels (no missing or empty group names)
- [ ] All parameters display their localized names (no raw key names visible)
- [ ] Tooltips/descriptions visible on hover for parameters that have them
- [ ] Numeric inputs respect their min/max ranges (try entering out-of-range values)
- [ ] Color pickers work correctly
- [ ] Enum dropdowns show all expected options with correct labels

---

## 3. Upstream functionality preservation

Verify that all functionality present in the upstream version is intact:

- [ ] {{upstream_feature_1}} — describe expected behavior
- [ ] {{upstream_feature_2}}
- [ ] Session boundary behavior (indicator resets / continues correctly at session open)
- [ ] Historical load (full backfill renders without gaps or crashes)
- [ ] Realtime update (indicator updates on each new tick without repainting history)

---

## 4. New features (per feat/ branch)

### feat/{{feature-1}}

- [ ] {{test_case_1}}
- [ ] {{test_case_2}}
- [ ] Edge case: {{edge_case}}

### feat/{{feature-2}}

- [ ] {{test_case_1}}
- [ ] {{test_case_2}}

---

## 5. Localization

- [ ] Switch ATAS language to German (de-de) — all new strings appear (no fallback to English key names)
- [ ] Switch to Russian (ru-ru) — same
- [ ] Switch to Spanish (es-es) — same
- [ ] Switch back to English — no regression

---

## 6. Edge cases and stability

- [ ] Indicator survives chart timeframe change
- [ ] Indicator survives instrument change
- [ ] Indicator survives ATAS workspace save/reload
- [ ] No memory leak or performance degradation after 10+ minutes of realtime data

---

## Notes

<!-- Document any unexpected behavior, deferred issues, or observations here -->
