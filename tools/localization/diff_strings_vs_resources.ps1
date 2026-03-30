<#
.SYNOPSIS
    Compares the current ATAS Alpha Strings inventory against the local
    Resources.resx to classify every key into one of three buckets.

.DESCRIPTION
    Run this script:
      - Before adding a new resource key (to check it is not already in Strings)
      - After an ATAS Platform update (to find keys that graduated into Strings
        and can now use typeof(Strings) instead of typeof(Resources))
      - Periodically to keep the typeof(Strings) vs typeof(Resources) decision
        table accurate

    The script writes four output files to $OutputDir:
        only_in_strings.json    Keys available via typeof(Strings) for all flavors
                                that carry them; check whether the key is also in
                                Stable/Beta before removing the Resources fallback.
        only_in_resources.json  Our custom keys — confirmed typeof(Resources).
        in_both.json            Keys duplicated in both; decide per flavor which
                                class to use and remove the Resources copy when safe.
        summary.txt             Human-readable summary.

.PARAMETER AtasRoot
    Path to the ATAS Platform installation directory.
    Default: C:\Program Files (x86)\ATAS Platform

.PARAMETER ExportsDir
    Directory where export_strings_from_satellites.ps1 wrote its JSON files.
    Default: .\exports

.PARAMETER ResxFile
    Path to the neutral Resources.resx to compare against.
    Default: ..\..\Technical\Properties\Resources.resx

.PARAMETER OutputDir
    Directory where output files are written.
    Default: .\diff-output

.EXAMPLE
    # Run after exporting current Alpha Strings:
    .\export_strings_from_satellites.ps1
    .\diff_strings_vs_resources.ps1

.EXAMPLE
    # Run before adding a new key to Resources to check if it is already in Strings:
    .\export_strings_from_satellites.ps1 -AtasRoot "C:\Program Files (x86)\ATAS Platform"
    .\diff_strings_vs_resources.ps1 | Select-String "MyNewKey"
#>

param(
    [string]$AtasRoot   = "C:\Program Files (x86)\ATAS Platform",
    [string]$ExportsDir = ".\exports",
    [string]$ResxFile   = "..\..\Technical\Properties\Resources.resx",
    [string]$OutputDir  = ".\diff-output"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# 1. Resolve paths
# ---------------------------------------------------------------------------

$ExportsDir = Resolve-Path $ExportsDir -ErrorAction SilentlyContinue
if (-not $ExportsDir) {
    Write-Error "Exports directory not found. Run export_strings_from_satellites.ps1 first."
    exit 1
}

$neutralJson = Join-Path $ExportsDir "neutral.json"
if (-not (Test-Path $neutralJson)) {
    Write-Error "neutral.json not found in $ExportsDir. Run export_strings_from_satellites.ps1 first."
    exit 1
}

if (-not (Test-Path $ResxFile)) {
    Write-Error "Resources.resx not found at $ResxFile"
    exit 1
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# ---------------------------------------------------------------------------
# 2. Load Strings keys from neutral.json
# ---------------------------------------------------------------------------

Write-Host "Loading Alpha Strings from $neutralJson ..."
$stringsRaw = Get-Content $neutralJson -Raw -Encoding UTF8 | ConvertFrom-Json
$stringsKeys = @{}
foreach ($prop in $stringsRaw.PSObject.Properties) {
    $stringsKeys[$prop.Name] = $prop.Value
}
Write-Host "  $($stringsKeys.Count) keys in Alpha Strings (neutral)"

# ---------------------------------------------------------------------------
# 3. Load Resources keys from Resources.resx
# ---------------------------------------------------------------------------

Write-Host "Loading Resources from $ResxFile ..."
[xml]$resx = Get-Content $ResxFile -Encoding UTF8
$resourceKeys = @{}
foreach ($node in $resx.root.data) {
    if ($node.name) {
        $val = if ($node.value) { $node.value } else { "" }
        $resourceKeys[$node.name] = $val
    }
}
Write-Host "  $($resourceKeys.Count) keys in Resources.resx"

# ---------------------------------------------------------------------------
# 4. Classify keys into three buckets
# ---------------------------------------------------------------------------

$onlyInStrings   = @{}   # in Strings, not in Resources     -> typeof(Strings) only
$onlyInResources = @{}   # in Resources, not in Strings     -> typeof(Resources) confirmed
$inBoth          = @{}   # in both                          -> duplication; review per flavor

foreach ($k in $stringsKeys.Keys) {
    if ($resourceKeys.ContainsKey($k)) {
        $inBoth[$k] = @{ Strings = $stringsKeys[$k]; Resources = $resourceKeys[$k] }
    } else {
        $onlyInStrings[$k] = $stringsKeys[$k]
    }
}
foreach ($k in $resourceKeys.Keys) {
    if (-not $stringsKeys.ContainsKey($k)) {
        $onlyInResources[$k] = $resourceKeys[$k]
    }
}

Write-Host ""
Write-Host "Results:"
Write-Host "  Only in Alpha Strings  : $($onlyInStrings.Count)   -> typeof(Strings) safe (check Stable/Beta coverage)"
Write-Host "  Only in Resources      : $($onlyInResources.Count)   -> typeof(Resources) confirmed (our custom keys)"
Write-Host "  In both                : $($inBoth.Count)   -> duplicated; review flavor coverage before removing from Resources"

# ---------------------------------------------------------------------------
# 5. Write output files
# ---------------------------------------------------------------------------

$onlyInStrings   | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $OutputDir "only_in_strings.json")   -Encoding UTF8
$onlyInResources | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $OutputDir "only_in_resources.json") -Encoding UTF8
$inBoth          | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $OutputDir "in_both.json")           -Encoding UTF8

$summary = @"
diff_strings_vs_resources — $(Get-Date -Format 'yyyy-MM-dd HH:mm')
ATAS root   : $AtasRoot
Exports dir : $ExportsDir
Resources   : $ResxFile

--- SUMMARY ---
Only in Alpha Strings (neutral)  : $($onlyInStrings.Count)
  -> Use typeof(Strings) for these keys.
  -> Verify Stable/Beta coverage before removing any #if guard.

Only in Resources                : $($onlyInResources.Count)
  -> Our custom keys. Always typeof(Resources) on i18n branches.
  -> Check this list before adding a new Phase 0 key to confirm
     it is not already in Strings.

In both (duplicated)             : $($inBoth.Count)
  -> These keys exist in both classes.
  -> For Alpha builds: typeof(Strings) is sufficient.
  -> For Stable/Beta: keep typeof(Resources) fallback until the key
     ships in that flavor's Strings.
  -> When a key is in ALL shipped flavors, remove from Resources.

--- GUIDANCE: typeof(Strings) vs typeof(Resources) decision table ---

| Situation                                          | Use                        |
|----------------------------------------------------|----------------------------|
| Key in Alpha Strings only                          | typeof(Strings) on feat/   |
|                                                    | typeof(Resources) on i18n  |
|                                                    | + #if !ATAS_STABLE guard   |
| Key in all flavors' Strings (Alpha+Beta+Stable)    | typeof(Strings) everywhere |
|                                                    | Remove from Resources.resx |
| Key in our custom Resources only (not in Strings)  | typeof(Resources) always   |
| Key is new (Phase 0) — not yet in any Strings      | typeof(Resources) always   |
|                                                    | Hardcode on feat/ branches |
"@

$summary | Set-Content (Join-Path $OutputDir "summary.txt") -Encoding UTF8

Write-Host ""
Write-Host "Output written to $OutputDir"
Write-Host "  only_in_strings.json   — typeof(Strings) candidates"
Write-Host "  only_in_resources.json — confirmed typeof(Resources) keys (our custom set)"
Write-Host "  in_both.json           — duplicates to review"
Write-Host "  summary.txt            — human-readable summary + decision table"
