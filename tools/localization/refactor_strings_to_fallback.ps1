<#
.SYNOPSIS
    Rewrites .cs files based on the analysis produced by
    generate_fallbacks_and_log.ps1. For every attribute block flagged for
    promotion, replaces 'Strings' with 'FallbackResources' inside the block
    (covers typeof(Strings), Strings.X and nameof(Strings.X) in one pass).
    For bare alpha-only references outside attribute blocks, replaces
    'Strings.<Key>' with 'FallbackResources.<Key>' per-key.

.PARAMETER DryRun
    If set, reports what would change without modifying files.

.NOTES
    Idempotent: running it twice is a no-op because the second pass finds
    no more 'Strings.<alpha-only>' references.
#>
param(
    [string]$DataDir = "$PSScriptRoot\..\..\fallbacks-data",
    [switch]$DryRun
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$pendingPath  = Join-Path $DataDir "PENDING_PROMOTIONS.json"
$bareListPath = Join-Path $DataDir "BARE_ALPHA_ONLY_REFS.json"

if (-not (Test-Path $pendingPath)) {
    Write-Error "Missing $pendingPath. Run generate_fallbacks_and_log.ps1 first."
    exit 1
}

$pending = Get-Content $pendingPath -Raw -Encoding UTF8 | ConvertFrom-Json
# Normalize to array even when only one element
if ($pending -isnot [System.Array]) { $pending = @($pending) }

$bareMap = @{}
if (Test-Path $bareListPath) {
    $bareJson = Get-Content $bareListPath -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($p in $bareJson.PSObject.Properties) { $bareMap[$p.Name] = $p.Value }
}

# Group pending promotions per file
$byFile = @{}
foreach ($p in $pending) {
    if (-not $byFile.ContainsKey($p.File)) { $byFile[$p.File] = @() }
    $byFile[$p.File] += $p
}

$stringsWordRx = [regex]'\bStrings\b'

$totalBlocks = 0
$totalBareRewrites = 0
$filesChanged = 0

foreach ($file in $byFile.Keys) {
    $entries = $byFile[$file]

    if (-not (Test-Path $file)) {
        Write-Warning "File not found, skipping: $file"
        continue
    }

    $original = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
    $text = $original

    # 1. Rewrite each promoted attribute block (end -> start to preserve offsets)
    $sorted = $entries | Sort-Object -Property Start -Descending
    foreach ($e in $sorted) {
        $before  = $text.Substring(0, $e.Start)
        $block   = $text.Substring($e.Start, $e.End - $e.Start)
        $after   = $text.Substring($e.End)
        $newBlock = $stringsWordRx.Replace($block, 'FallbackResources')
        if ($newBlock -ne $block) {
            $text = $before + $newBlock + $after
            $totalBlocks++
        }
    }

    # 2. Rewrite bare Strings.<Key> for alpha-only Keys referenced in this file.
    #    We use a per-key regex so we never rewrite non-alpha-only refs.
    $shortName = Split-Path -Leaf $file
    foreach ($key in $bareMap.Keys) {
        $filesForKey = $bareMap[$key]
        if ($filesForKey -notcontains $shortName) { continue }
        $rx = [regex]("\bStrings\s*\.\s*" + [regex]::Escape($key) + "\b")
        $newText = $rx.Replace($text, "FallbackResources.$key")
        if ($newText -ne $text) {
            $totalBareRewrites += ($rx.Matches($text)).Count
            $text = $newText
        }
    }

    if ($text -ne $original) {
        $filesChanged++
        if ($DryRun) {
            Write-Host "[DRY-RUN] would rewrite: $file" -ForegroundColor Yellow
        } else {
            [System.IO.File]::WriteAllText($file, $text, [System.Text.UTF8Encoding]::new($false))
            Write-Host "rewritten: $file" -ForegroundColor Green
        }
    }
}

Write-Host "`nSummary:"
Write-Host "  Files touched:              $filesChanged"
Write-Host "  Attribute blocks rewritten: $totalBlocks"
Write-Host "  Bare Strings.X rewrites:    $totalBareRewrites"
if ($DryRun) { Write-Host "  (dry-run: no files modified)" -ForegroundColor Yellow }