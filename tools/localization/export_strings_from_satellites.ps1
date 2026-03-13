param(
    [string]$AtasRoot = "C:\Program Files (x86)\ATAS Platform",
    [string]$OutputDir = ".\exports"
)

# Create output folder
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Export neutral/base strings from OFT.Localization.dll
$neutralDll = Join-Path $AtasRoot "OFT.Localization.dll"

if (Test-Path $neutralDll) {
    Write-Host "Processing neutral resources from OFT.Localization.dll"

    try {
        $assembly = [System.Reflection.Assembly]::LoadFrom($neutralDll)
        $resourceNames = $assembly.GetManifestResourceNames()

        $stringsResource = $resourceNames |
            Where-Object {
                $_ -like "OFT.Localization.Strings*.resources" -and
                $_ -notmatch "\.[a-z]{2}(-[A-Z]{2})?\.resources$"
            } |
            Select-Object -First 1

        if (-not $stringsResource) {
            Write-Warning "No neutral Strings resource found in OFT.Localization.dll"
        }
        else {
            $stream = $assembly.GetManifestResourceStream($stringsResource)
            if (-not $stream) {
                Write-Warning "Could not open neutral resource stream"
            }
            else {
                $reader = New-Object System.Resources.ResourceReader($stream)
                $dict = @{}

                foreach ($entry in $reader) {
                    $dict[$entry.Key] = [string]$entry.Value
                }

                $reader.Close()
                $stream.Close()

                $jsonPath = Join-Path $OutputDir "neutral.json"
                $dict | ConvertTo-Json -Depth 3 | Set-Content -Path $jsonPath -Encoding UTF8

                Write-Host "Exported $($dict.Count) neutral keys to $jsonPath"
            }
        }
    }
    catch {
        Write-Warning "Failed for neutral resources: $_"
    }
}
else {
    Write-Warning "Neutral OFT.Localization.dll not found at $neutralDll"
}

# Find culture folders containing OFT.Localization.resources.dll
Get-ChildItem -Path $AtasRoot -Directory | ForEach-Object {
    $cultureDir = $_.FullName
    $cultureName = $_.Name
    $satelliteDll = Join-Path $cultureDir "OFT.Localization.resources.dll"

    if (-not (Test-Path $satelliteDll)) {
        return
    }

    Write-Host "Processing culture: $cultureName"

    try {
        $assembly = [System.Reflection.Assembly]::LoadFrom($satelliteDll)
        $resourceNames = $assembly.GetManifestResourceNames()

        $stringsResource = $resourceNames | Where-Object { $_ -like "OFT.Localization.Strings*.resources" } | Select-Object -First 1

        if (-not $stringsResource) {
            Write-Warning "No Strings resource found for $cultureName"
            return
        }

        $stream = $assembly.GetManifestResourceStream($stringsResource)
        if (-not $stream) {
            Write-Warning "Could not open resource stream for $cultureName"
            return
        }

        $reader = New-Object System.Resources.ResourceReader($stream)
        $dict = @{}

        foreach ($entry in $reader) {
            $dict[$entry.Key] = [string]$entry.Value
        }

        $reader.Close()
        $stream.Close()

        $jsonPath = Join-Path $OutputDir "$cultureName.json"
        $dict | ConvertTo-Json -Depth 3 | Set-Content -Path $jsonPath -Encoding UTF8

        Write-Host "Exported $($dict.Count) keys to $jsonPath"
    }
    catch {
        Write-Warning "Failed for $cultureName : $_"
    }
}