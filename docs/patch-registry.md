# Patch registry

| Include | Commit | Date | Type | Area | Subject | Source branch | Cherry-pick safe | In testing | In release | PR status | Upstream status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| yes | `939641e7` | 2026-03-20 | fix | domstrength | fix(domstrength): use bid depth values when summing bids | fix/domstrength-bid-sum-source | yes | no | no | not-opened | pending | Atomic bugfix |
| yes | `988f065a` | 2026-03-20 | fix | domstrength | fix(domstrength): avoid off-by-one when iterating level depth | fix/domstrength-leveldepth-off-by-one | yes | no | no | not-opened | pending | Bounds fix |

---

## Patch sets

### local/integration/testing

```bash
git cherry-pick 939641e7 988f065a
```

### local/integration/release

```bash
git cherry-pick 939641e7 988f065a
```

---

## Patch Notes

- Both commits are atomic and independent.
- Safe for cherry-pick into integration branches.
- No dependency between patches detected.
- Suitable for PR submission as separate fixes.


---

&nbsp;

# Build stacks

### local/build/03-version-shims

| Stack | Base | Scope | Integration mode | Notes |
|---|---|---|---|---|
| `local/build/03-version-shims` | `local/build/02-multiversion` | compatibility / multi-version support | Stack | Stack of compatibility shims and build adaptations for multiple ATAS versions |

### Included changes (high-level)
#### Compatibility
- compat(stable): guard newer CandleDataSeries visual API by build flavor
- compat(beta): guard newer CandleDataSeries visual API by build flavor
- compat(candles): disable DrawCandleBorder on stable
- compat(candles): add VWAP shim for stable 7.09
- compat(stable): add PropertiesEditor compatibility shim
- compat(rollovers): exclude RolloverDates from stable build
#### Resources and tooling
- compat(resources): sync local resource catalogs with extracted ATAS localization data
- tooling(localization): scripts to export and sync localization resources
#### Build
- build(solution): multi-flavor build configurations (ATAS_X, Alpha, Beta, Latest, Stable)

### Stack Notes

- This stack is treated as a **cohesive unit**, not as independent patches.
- Not suitable for cherry-picking individual commits.
- Changes are tightly coupled to build system and version compatibility.