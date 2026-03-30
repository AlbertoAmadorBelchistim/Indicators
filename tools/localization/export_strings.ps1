<#
.SYNOPSIS
    Exports resources from ATAS Alpha and Stable installations.
 
.NOTES
    Each installation is processed in a SEPARATE PowerShell Job so that
    OFT.Localization.dll from Alpha and from Stable are loaded into
    independent AppDomains. Running both in the same process causes the
    CLR to return the first-loaded assembly for the second Load() call
    (same strong-name identity => cached), silently duplicating Alpha's
    resources into stable/neutral.json.
#>
param(
    [string]$AlphaRoot  = "C:\Program Files (x86)\ATAS Platform",
    [string]$StableRoot = "C:\Program Files (x86)\ATAS Platform Stable",
    [string]$OutputDir  = "$PSScriptRoot\..\..\.atas-exports"
)
 
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"
 
# ---------------------------------------------------------------------------
# Worker scriptblock: runs inside an isolated Job (fresh AppDomain).
# ---------------------------------------------------------------------------
$worker = {
    param([string]$InstallDir, [string]$TargetOutDir)
 
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Continue"
 
    function Read-StringsFromAssembly {
        param([string]$DllPath, [bool]$NeutralOnly)
 
        $bytes    = [System.IO.File]::ReadAllBytes($DllPath)
        $assembly = [System.Reflection.Assembly]::Load($bytes)
 
        $names = $assembly.GetManifestResourceNames()
        if ($NeutralOnly) {
            $resName = $names | Where-Object {
                $_ -like "OFT.Localization.Strings*.resources" -and
                $_ -notmatch "\.[a-z]{2}(-[A-Z]{2})?\.resources$"
            } | Select-Object -First 1
        } else {
            $resName = $names | Where-Object {
                $_ -like "OFT.Localization.Strings*.resources"
            } | Select-Object -First 1
        }
 
        if (-not $resName) { return $null }
 
        $stream = $assembly.GetManifestResourceStream($resName)
        if (-not $stream) { return $null }
 
        $reader = New-Object System.Resources.ResourceReader($stream)
        $dict = [ordered]@{}
        foreach ($entry in $reader) { $dict[$entry.Key] = [string]$entry.Value }
        $reader.Close(); $stream.Close()
        return $dict
    }
 
    if (-not (Test-Path $InstallDir)) {
        Write-Warning "Installation directory not found: $InstallDir"
        return
    }
    New-Item -ItemType Directory -Force -Path $TargetOutDir | Out-Null
    Write-Host "--- Exporting from: $InstallDir ---" -ForegroundColor Cyan
 
    # 1. Neutral
    $neutralDll = Join-Path $InstallDir "OFT.Localization.dll"
    if (Test-Path $neutralDll) {
        try {
            $dict = Read-StringsFromAssembly -DllPath $neutralDll -NeutralOnly $true
            if ($dict) {
                $jsonPath = Join-Path $TargetOutDir "neutral.json"
                $dict | ConvertTo-Json -Depth 3 | Set-Content -Path $jsonPath -Encoding UTF8
                Write-Host "  Exported $($dict.Count) neutral keys." -ForegroundColor Green
            } else {
                Write-Warning "  No neutral Strings resource found."
            }
        } catch { Write-Warning "  Neutral export failed: $_" }
    } else {
        Write-Warning "  Neutral OFT.Localization.dll not found."
    }
 
    # 2. Culture satellites
    Get-ChildItem -Path $InstallDir -Directory | ForEach-Object {
        $satellite = Join-Path $_.FullName "OFT.Localization.resources.dll"
        if (-not (Test-Path $satellite)) { return }
        Write-Host "  Processing culture: $($_.Name)"
        try {
            $dict = Read-StringsFromAssembly -DllPath $satellite -NeutralOnly $false
            if ($dict) {
                $jsonPath = Join-Path $TargetOutDir "$($_.Name).json"
                $dict | ConvertTo-Json -Depth 3 | Set-Content -Path $jsonPath -Encoding UTF8
            }
        } catch { Write-Warning "  Failed for $($_.Name): $_" }
    }
}
 
# ---------------------------------------------------------------------------
# Launch each installation in its own Job (isolated AppDomain).
# ---------------------------------------------------------------------------
function Invoke-IsolatedExport {
    param([string]$InstallDir, [string]$TargetOutDir)
 
    $job = Start-Job -ScriptBlock $worker -ArgumentList $InstallDir, $TargetOutDir
    Wait-Job $job | Out-Null
    Receive-Job $job
    Remove-Job  $job
}
 
$alphaOut  = Join-Path $OutputDir "alpha"
$stableOut = Join-Path $OutputDir "stable"
 
Invoke-IsolatedExport -InstallDir $AlphaRoot  -TargetOutDir $alphaOut
Invoke-IsolatedExport -InstallDir $StableRoot -TargetOutDir $stableOut
 
Write-Host "Process completed! JSON files are saved in $OutputDir" -ForegroundColor Yellow