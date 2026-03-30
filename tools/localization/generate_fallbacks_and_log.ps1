<#
.SYNOPSIS
    Computes required Fallback resources by combining:
      (a) alpha-only keys (genuine diff alpha vs stable), AND
      (b) any stable key that is co-referenced inside the same [Display]/[Tab]/...
          attribute block as an alpha-only key.

.NOTES
    We need (b) because [Display] admits only ONE ResourceType. If an attribute
    block mixes alpha-only keys (e.g. FitToPriceRange) with stable-only keys
    (e.g. Settings), the whole block has to be repointed to FallbackResources,
    which then needs BOTH keys present — Settings is duplicated.

    This script only computes the expanded set and emits the fallback JSONs.
    Use the companion refactor_strings_to_fallback.ps1 to rewrite the .cs files.
#>
param(
    [string]$AlphaExportsDir     = "$PSScriptRoot\..\..\.atas-exports\alpha",
    [string]$StableExportsDir    = "$PSScriptRoot\..\..\.atas-exports\stable",
    [string]$CodeRoot            = "$PSScriptRoot\..\..\Technical",
    [string]$OutputDir           = "$PSScriptRoot\..\..\fallbacks-data",
    [string]$CurrentFallbackResx = "$PSScriptRoot\..\..\Technical\Properties\FallbackResources.resx"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# ---------------------------------------------------------------------------
# 1. Load alpha + stable neutral dictionaries
# ---------------------------------------------------------------------------
$alphaNeutralPath  = Join-Path $AlphaExportsDir  "neutral.json"
$stableNeutralPath = Join-Path $StableExportsDir "neutral.json"

if (-not (Test-Path $alphaNeutralPath) -or -not (Test-Path $stableNeutralPath)) {
    Write-Error "neutral.json files are missing in the export folders."
    exit 1
}

$alphaJson  = Get-Content $alphaNeutralPath  -Raw -Encoding UTF8 | ConvertFrom-Json
$stableJson = Get-Content $stableNeutralPath -Raw -Encoding UTF8 | ConvertFrom-Json

$alphaSet  = @{}
foreach ($p in $alphaJson.PSObject.Properties)  { $alphaSet[$p.Name]  = $p.Value }
$stableSet = @{}
foreach ($p in $stableJson.PSObject.Properties) { $stableSet[$p.Name] = $true }

# alpha-only (genuine diff)
$alphaOnlySet = @{}
foreach ($k in $alphaSet.Keys) {
    if (-not $stableSet.ContainsKey($k)) { $alphaOnlySet[$k] = $true }
}

Write-Host "Alpha neutral keys:  $($alphaSet.Count)"
Write-Host "Stable neutral keys: $($stableSet.Count)"
Write-Host "Alpha-only (pure diff): $($alphaOnlySet.Count)"

# ---------------------------------------------------------------------------
# 2. Scan .cs files; for each attribute block collect Strings.X refs
# ---------------------------------------------------------------------------
if (-not (Test-Path $CodeRoot)) {
    Write-Error "Code root not found: $CodeRoot"
    exit 1
}
Write-Host "`nScanning C# files under $CodeRoot..." -ForegroundColor Cyan

# Attribute block: [...] with balanced brackets, multiline.
$attrBlockRx = [regex]::new(
    '\[(?>[^\[\]]+|\[(?<d>)|\](?<-d>))*(?(d)(?!))\]',
    'Singleline'
)
# Strings.Identifier (also captures nameof(Strings.X) — the Strings.X is inside).
$stringsRefRx = [regex]'\bStrings\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)'

# Co-referenced keys promoted by at least one mixed attribute block.
$promotedFromBlocks = @{}
$promotedByFile     = @{}

# Bare Strings.X outside attribute blocks pointing to alpha-only keys.
# They MUST also be rewritten, so the refactor script needs them too.
$bareAlphaOnlyRefs = @{}
$bareRefByFile     = @{}

# Track every attribute block that needs promotion (for the refactor script).
$pendingPromotions = New-Object System.Collections.Generic.List[object]

$csFiles = Get-ChildItem -Path $CodeRoot -Recurse -Filter *.cs -File
foreach ($file in $csFiles) {
    try {
        $text = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    } catch {
        Write-Warning "Cannot read $($file.FullName): $_"
        continue
    }

    # Spans occupied by attribute blocks (needed to classify bare refs later)
    $blockSpans = New-Object System.Collections.Generic.List[object]

    foreach ($m in $attrBlockRx.Matches($text)) {
        $span = @{
            Start = $m.Index
            End   = $m.Index + $m.Length
            Text  = $m.Value
        }
        $blockSpans.Add($span)

        # Collect Strings refs inside this block
        $refsInBlock = @{}
        foreach ($rm in $stringsRefRx.Matches($span.Text)) {
            $refsInBlock[$rm.Groups[1].Value] = $true
        }
        if ($refsInBlock.Count -eq 0) { continue }

        # Any alpha-only? Promote the whole block.
        $hasAlphaOnly = $false
        foreach ($k in $refsInBlock.Keys) {
            if ($alphaOnlySet.ContainsKey($k)) { $hasAlphaOnly = $true; break }
        }
        if (-not $hasAlphaOnly) { continue }

        foreach ($k in $refsInBlock.Keys) {
            $promotedFromBlocks[$k] = $true
            if (-not $promotedByFile.ContainsKey($k)) { $promotedByFile[$k] = @{} }
            $promotedByFile[$k][$file.Name] = $true
        }

        $pendingPromotions.Add([pscustomobject]@{
            File  = $file.FullName
            Start = $span.Start
            End   = $span.End
            Keys  = @($refsInBlock.Keys)
        })
    }

    # Bare refs: Strings.X outside any attribute block
    foreach ($rm in $stringsRefRx.Matches($text)) {
        $pos = $rm.Index
        $inBlock = $false
        foreach ($b in $blockSpans) {
            if ($pos -ge $b.Start -and $pos -lt $b.End) { $inBlock = $true; break }
        }
        if ($inBlock) { continue }

        $k = $rm.Groups[1].Value
        if ($alphaOnlySet.ContainsKey($k)) {
            $bareAlphaOnlyRefs[$k] = $true
            if (-not $bareRefByFile.ContainsKey($k)) { $bareRefByFile[$k] = @{} }
            $bareRefByFile[$k][$file.Name] = $true
        }
    }
}

# Split promoted keys into "alpha-only" vs "co-referenced from stable" for the log.
$promotedAlphaOnly = @{}
$promotedStable    = @{}
foreach ($k in $promotedFromBlocks.Keys) {
    if ($alphaOnlySet.ContainsKey($k))   { $promotedAlphaOnly[$k] = $true }
    elseif ($stableSet.ContainsKey($k))  { $promotedStable[$k]    = $true }
    # else -> unknown, reported separately below
}

# Unknown keys used in code but missing in alpha (typos or removed).
$unknownInCode = @()
foreach ($k in $promotedFromBlocks.Keys) {
    if (-not $alphaSet.ContainsKey($k)) { $unknownInCode += $k }
}

# ---------------------------------------------------------------------------
# 3. requiredFallbackKeys = (alpha-only set) ∪ (all promoted co-referenced)
#    Also include bare alpha-only refs (already in alpha-only, redundant but
#    makes the intent explicit).
# ---------------------------------------------------------------------------
$requiredFallbackKeys = @{}
foreach ($k in $alphaOnlySet.Keys)       { $requiredFallbackKeys[$k] = $true }
foreach ($k in $promotedFromBlocks.Keys) {
    if ($alphaSet.ContainsKey($k)) { $requiredFallbackKeys[$k] = $true }
}

Write-Host "`nPromotion summary:"
Write-Host "  Attribute blocks flagged for promotion: $($pendingPromotions.Count)"
Write-Host "  Promoted keys (alpha-only co-refs):     $($promotedAlphaOnly.Count)"
Write-Host "  Promoted keys (stable co-refs, dupd):   $($promotedStable.Count)"
Write-Host "  Bare alpha-only refs outside blocks:    $($bareAlphaOnlyRefs.Count)"
Write-Host "  Unknown keys referenced in code:        $($unknownInCode.Count)"
Write-Host "  Total keys -> FallbackResources.resx:   $($requiredFallbackKeys.Count)"

# ---------------------------------------------------------------------------
# 4. Graduation (keys in current resx that are no longer alpha-only)
# ---------------------------------------------------------------------------
$graduatedKeys = @()
if (Test-Path $CurrentFallbackResx) {
    [xml]$resx = Get-Content $CurrentFallbackResx -Encoding UTF8
    foreach ($node in $resx.root.data) {
        if (-not $requiredFallbackKeys.ContainsKey($node.name)) {
            $graduatedKeys += $node.name
        }
    }
}

# ---------------------------------------------------------------------------
# 5. Persist pending-promotions metadata for the refactor script
# ---------------------------------------------------------------------------
$pendingPath = Join-Path $OutputDir "PENDING_PROMOTIONS.json"
$pendingPromotions | ConvertTo-Json -Depth 4 | Set-Content $pendingPath -Encoding UTF8

$bareListPath = Join-Path $OutputDir "BARE_ALPHA_ONLY_REFS.json"
$bareMap = @{}
foreach ($k in $bareAlphaOnlyRefs.Keys) {
    $bareMap[$k] = @($bareRefByFile[$k].Keys | Sort-Object)
}
($bareMap | ConvertTo-Json -Depth 3) | Set-Content $bareListPath -Encoding UTF8

# ---------------------------------------------------------------------------
# 6. Human-readable report
# ---------------------------------------------------------------------------
$logPath = Join-Path $OutputDir "GRADUATION_LOG.txt"
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("=== RESOURCE FALLBACK REPORT ($(Get-Date -Format 'yyyy-MM-dd HH:mm')) ===")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("Alpha neutral keys:  $($alphaSet.Count)")
[void]$sb.AppendLine("Stable neutral keys: $($stableSet.Count)")
[void]$sb.AppendLine("Alpha-only (diff):   $($alphaOnlySet.Count)")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("Attribute blocks to promote (Strings -> FallbackResources): $($pendingPromotions.Count)")
[void]$sb.AppendLine("  Alpha-only keys inside promoted blocks:     $($promotedAlphaOnly.Count)")
[void]$sb.AppendLine("  Stable keys co-referenced (duplicated):     $($promotedStable.Count)")
[void]$sb.AppendLine("Bare alpha-only refs (outside attributes):    $($bareAlphaOnlyRefs.Count)")
[void]$sb.AppendLine("Unknown keys (in code, not in alpha):         $($unknownInCode.Count)")
[void]$sb.AppendLine("Total keys -> FallbackResources.resx:         $($requiredFallbackKeys.Count)")
[void]$sb.AppendLine("")

if ($unknownInCode.Count -gt 0) {
    [void]$sb.AppendLine("### UNKNOWN KEYS (not in alpha) ###")
    [void]$sb.AppendLine("Typos or removed keys. Fix these in code before the refactor.")
    foreach ($k in ($unknownInCode | Sort-Object -Unique)) {
        [void]$sb.AppendLine("  - $k")
    }
    [void]$sb.AppendLine("")
}

if ($promotedStable.Count -gt 0) {
    [void]$sb.AppendLine("### STABLE KEYS DUPLICATED INTO FALLBACK (informational) ###")
    [void]$sb.AppendLine("These keys exist in Stable; we ship them in FallbackResources because")
    [void]$sb.AppendLine("they share [Display]/[Tab]/... attributes with alpha-only keys.")
    foreach ($k in ($promotedStable.Keys | Sort-Object)) {
        [void]$sb.AppendLine("  -> $k")
    }
    [void]$sb.AppendLine("")
}

if ($graduatedKeys.Count -gt 0) {
    [void]$sb.AppendLine("### GRADUATED KEYS ###")
    [void]$sb.AppendLine("These keys were in the previous FallbackResources.resx but are no longer needed.")
    [void]$sb.AppendLine("sync_fallbacks_to_resx.ps1 will drop them. Revert their code usages to Strings.X.")
    foreach ($k in ($graduatedKeys | Sort-Object)) {
        [void]$sb.AppendLine("  -> $k")
    }
    [void]$sb.AppendLine("")
}

$sb.ToString() | Set-Content $logPath -Encoding UTF8
Write-Host "`nReport:              $logPath"
Write-Host "Pending promotions:  $pendingPath"
Write-Host "Bare alpha-only map: $bareListPath"

# ---------------------------------------------------------------------------
# 7. Emit fallback_<culture>.json with the required keys, using Alpha values
# ---------------------------------------------------------------------------
$alphaFiles = Get-ChildItem "$AlphaExportsDir\*.json"
foreach ($file in $alphaFiles) {
    $cultureName = $file.BaseName
    $alphaDict = Get-Content $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json

    $fallbackDict = @{}
    foreach ($p in $alphaDict.PSObject.Properties) {
        if ($requiredFallbackKeys.ContainsKey($p.Name)) {
            $fallbackDict[$p.Name] = $p.Value
        }
    }

    $outJson = Join-Path $OutputDir "fallback_$cultureName.json"
    $json = ConvertTo-Json -InputObject $fallbackDict -Depth 3
    Set-Content -Path $outJson -Value $json -Encoding UTF8
}

Write-Host "Fallback JSONs generated in $OutputDir (total keys: $($requiredFallbackKeys.Count))." -ForegroundColor Green

if ($unknownInCode.Count -gt 0) {
    Write-Warning "Unknown keys detected. See GRADUATION_LOG.txt."
}