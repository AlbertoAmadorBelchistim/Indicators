# Build Stack Audit Guide

This document defines the ongoing maintenance protocol for the `local/build/01-04` stack.
Run sections A–D every time ATAS releases a new platform version.

Last audited: **2026-03-30** (ATAS alpha 8.0.14.105)

---

## 1. Stack overview

```
Develop (upstream)
└── local/build/01-base          — csproj alignment: Cross/Windows, ATAS_BASE, DLL refs
    └── local/build/02-multiversion  — build matrix: Alpha/Beta/Latest/Stable/ATAS_X_Alpha/ATAS_X_Beta
        └── local/build/03-version-shims  — compat shims + resource sync + tooling
            └── local/build/04-localization  — custom .resx keys (all 7 locales)
                └── (indicator integration branches)
```

**Install paths per flavor** (as of 2026-03-30):

| Configuration | Platform | ATAS_BASE | Define constants | Assembly output |
|---------------|----------|-----------|-----------------|-----------------|
| `Alpha` | AnyCPU | `C:\Program Files (x86)\ATAS Platform` | `ATAS_ALPHA` | `Indicators_Alpha.dll` |
| `Beta` | AnyCPU | `C:\Program Files (x86)\ATAS Platform beta` | `ATAS_BETA` | `Indicators_Beta.dll` |
| `Latest` | AnyCPU | `C:\Program Files (x86)\ATAS Platform latest` | `ATAS_LATEST` | `Indicators_Latest.dll` |
| `Stable` | AnyCPU | `C:\Program Files (x86)\ATAS Platform stable` | `ATAS_STABLE` | `Indicators_Stable.dll` |
| `ATAS_X_Alpha` | Cross | `C:\Program Files\ATAS X` | `ATAS_X_ALPHA` | `Indicators_X_Alpha.dll` |
| `ATAS_X_Beta` | Cross | `C:\Program Files\ATAS X beta` | `ATAS_X_BETA` | `Indicators_X_Beta.dll` |

**Framework targets:**

- Windows (AnyCPU): `net10.0-windows` — verify this matches ATAS Platform's runtime on each update
- Cross: `net10.0`

---

## 2. Checklist: after each ATAS platform update

### A — Build verification (all active flavors)

Run from repo root on `local/build/04-localization` (or your working branch stacked on it):

```bash
dotnet build Technical/Indicators.csproj -c Alpha
dotnet build Technical/Indicators.csproj -c Beta
dotnet build Technical/Indicators.csproj -c Stable
# Only if ATAS X is installed:
dotnet build Technical/Indicators.csproj -c ATAS_X_Alpha -p:Platform=Cross
dotnet build Technical/Indicators.csproj -c ATAS_X_Beta -p:Platform=Cross
```

**Expected:** Alpha must always pass. Beta should pass. Stable may produce known errors (see §4).
Any NEW error on Alpha or Beta means an API was added/changed and needs a compat shim in `03-version-shims`.

### B — New strings check

```powershell
cd tools/localization
.\export_strings_from_satellites.ps1   # reads OFT.Localization.dll from Alpha install path
.\diff_strings_vs_resources.ps1        # compares against Technical/Properties/Resources.resx
```

Review the three output files:

| File | Meaning | Action |
|------|---------|--------|
| `only_in_strings.json` | New ATAS keys not in our Resources | Evaluate: are any relevant to our custom indicators? Import selectively. |
| `only_in_resources.json` | Our custom keys (confirmed unique) | No action needed — these ARE our local keys. |
| `in_both.json` | Keys duplicated in both | Remove from our Resources.resx (use `typeof(Strings)` instead). |

**When to add to Resources.resx from `only_in_strings.json`:** Never — if the key already exists in Strings, use `typeof(Strings)`. Only add to Resources.resx keys that do not exist in Strings at all.

### C — Runtime framework verification

```powershell
# Check what .NET runtime ATAS Platform is actually running on
Get-Process "ATASPlatform" | Select-Object Id, ProcessName
# Then in Process Explorer or Task Manager: check .NET version column
# Alternatively, read from ATAS install dir:
Get-Item "C:\Program Files (x86)\ATAS Platform\ATASPlatform.exe" |
    Select-Object -ExpandProperty VersionInfo
```

If ATAS Platform runtime changes (e.g., from net8 to net10), update the `<TargetFramework>` in
`Technical/Indicators.csproj` on `local/build/01-base` and rebase the stack.

### D — TabAttribute availability check

`Technical/Compat/TabAttributeCompat.cs` provides a stub for `OFT.Attributes.TabAttribute`
when it is not yet available in the installed DLL. Check if it can be removed:

```bash
# Temporarily add TAB_ATTRIBUTE_AVAILABLE to Alpha defines in csproj, then:
dotnet build Technical/Indicators.csproj -c Alpha
# If it compiles: TabAttribute is now in OFT.Attributes → remove the stub and the define guard.
# If it fails with "TabAttribute not found": keep the stub.
```

Once TabAttribute is confirmed available in ALL flavors (Alpha, Beta, Latest, Stable), remove
`Technical/Compat/TabAttributeCompat.cs` from `local/build/03-version-shims`.

### E — VWAP shim check (Stable)

`Technical/Compat/IndicatorCandleCompat.cs` provides a manual VWAP fallback for `ATAS_STABLE`
(7.0.9 lacked `candle.VWAP`). If Stable is updated past 7.0.9, verify:

```bash
dotnet build Technical/Indicators.csproj -c Stable
```

If VWAP compiles without the shim, remove the `#if ATAS_STABLE` guard from affected indicators
and delete `IndicatorCandleCompat.cs`.

---

## 3. How to add a new compat shim

When a build error appears on a specific flavor after an ATAS update:

1. Identify the API: note the exact error message (method/property not found, type not found, etc.)
2. Create or update a file in `Technical/Compat/` with a `#if ATAS_STABLE` (or appropriate define) guard
3. Name the file `<Concept>Compat.cs` (e.g., `IndicatorCandleCompat.cs`)
4. Commit to `local/build/03-version-shims` with message: `compat(<scope>): <what and why>`
5. Rebase downstream: `03` → `04` → each integration branch (`local/<indicator>-i18n`)

**Convention for shim files:**

```csharp
// If the API is missing only on Stable:
#if ATAS_STABLE
// fallback implementation
#endif

// If the API is missing on all older flavors except Alpha:
#if !ATAS_ALPHA && !ATAS_BETA
// fallback implementation
#endif
```

---

## 4. Known stable incompatibilities (as of 2026-03-30)

| API | Indicator(s) | Guard | Notes |
|-----|-------------|-------|-------|
| `candle.VWAP` | VWMA, others | `#if !ATAS_STABLE` → manual calc in `IndicatorCandleCompat.cs` | Stable 7.0.9 lacks this property |
| `CandleDataSeries` visual properties | Multiple | `#if !ATAS_STABLE`, `#if !ATAS_BETA` | Newer API not in older builds |
| `DrawCandleBorder` | Delta | `#if !ATAS_STABLE` | Property added post-stable |
| `IgnoreHistoryScale` | Multiple | `#if !ATAS_STABLE` | Missing in stable |
| `FixedProfileRequest(baseTime)` overload | MaxLevels | `#if !ATAS_STABLE` | Overload added post-stable |
| Trading statistics initialization | TradesOnChart | `#if ATAS_STABLE` → fallback | Different init path in stable |
| `RolloverDates` class | — | `#if !ATAS_STABLE` exclude from build | PLAT-2716 not in stable |
| `PropertiesEditor` | SampleProperties | `#if !ATAS_STABLE` | Editor not available in stable |
| `OFT.Attributes.TabAttribute` | ClusterStatistic | `#if !TAB_ATTRIBUTE_AVAILABLE` stub | Not yet in any flavor as of 2026-03-30 |
| `typeof(Resources)` display strings | Multiple | `#if ATAS_STABLE`, `#if ATAS_BETA` | Some Strings keys missing in older builds |

---

## 5. Known issues and deferred fixes

### 5.1 `Alpha|Cross` etc. in solution file

`Indicators.sln` lists `Alpha|Cross`, `Beta|Cross`, `Latest|Cross`, `Stable|Cross` as solution
configurations. These will error immediately at build time (the `ValidateConfigurationPlatform`
target rejects Cross for non-ATAS_X configs). They are dead entries — harmless but noisy in VS.

**Fix:** Remove all `|Cross` entries except `ATAS_X_Alpha|Cross` and `ATAS_X_Beta|Cross` from
`Indicators.sln`. Requires rebase of `03-version-shims`, `04-localization`, and all integration
branches. Schedule for next major build stack maintenance window.

### 5.2 net10.0 for Windows build

`local/build/01-base` targets `net10.0-windows` for the Windows (AnyCPU) build. This assumes
ATAS Platform runs on .NET 10. Confirm on each major ATAS version update (§2-C). If ATAS
reverts to .NET 8, the output DLL will fail to load.

### 5.3 04-localization history has a delete+restore pair

`c9e31e23` (deleted `es-es.resx`) + `0688067e` (restored it) should be a single clean commit.
Squash when next doing a full stack rebase (e.g., when syncing with a major Develop update).

---

## 6. How to add new .resx keys (Phase 3 of port workflow)

1. Switch to `local/build/04-localization`
2. Add the key to **all 7 files** in a single commit:
   - `Resources.resx` (en)
   - `Resources.de-de.resx`
   - `Resources.ru-ru.resx`
   - `Resources.es-es.resx`
   - `Resources.fr-fr.resx`
   - `Resources.hi-in.resx`
   - `Resources.zh-cn.resx`
3. Regenerate `Resources.Designer.cs` (VS does this automatically on build)
4. Commit: `chore(i18n): add <IndicatorName> Phase 0 keys to all locales`
5. Push to origin
6. Rebase integration branches that use the new keys:
   ```bash
   git rebase local/build/04-localization local/<indicator>-i18n
   ```

**Never** add keys to only some locales and follow up with another commit for the rest.
Use machine-translated placeholders for locales you cannot translate manually.

---

## 7. PR for `local/build/01-base`

A PR has been submitted to `AtasPlatform/Indicators`. Before it can be accepted:

- Upstream uses `Indicators.Technical.csproj` with **ProjectReferences** to their internal GitLab OFT repos.
  Our `Indicators.csproj` uses **DLL HintPaths**. These are structurally different.
- The Cross-platform and net10.0 changes are valid in principle, but the PR may need to be
  reformulated to match upstream's project structure (no HintPaths, no ATAS_BASE variable).
- If upstream rejects or closes the PR, the branch remains local-only (same status as 02-04).

**Recommendation:** If the PR is rejected, keep `01-base` as a permanent local-only branch and
document it as such in `branch-stacks.md`.
