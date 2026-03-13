param(
    [string]$ExportsDir = ".\exports",
    [string]$ResxDir = "..\..\Technical\Properties",
    [string]$BaseName = "Resources"
)

Add-Type -AssemblyName System.Windows.Forms

function Write-Resx {
    param(
        [string]$File,
        [hashtable]$Data
    )

    $writer = New-Object System.Resources.ResXResourceWriter $File

    foreach ($key in $Data.Keys | Sort-Object) {
        $writer.AddResource($key, $Data[$key])
    }

    $writer.Generate()
    $writer.Close()
}

$files = Get-ChildItem "$ExportsDir\*.json" | Where-Object { $_.BaseName -ne "" }

foreach ($file in $files)
{
    $culture = [System.IO.Path]::GetFileNameWithoutExtension($file)

    Write-Host "Processing $culture"

    $json = Get-Content $file.FullName -Raw | ConvertFrom-Json

    $table = @{}

    foreach ($prop in $json.PSObject.Properties)
    {
        $table[$prop.Name] = $prop.Value
    }

    if ($culture -eq "neutral")
    {
        $resxFile = Join-Path $ResxDir "$BaseName.resx"
    }
    else
    {
        $resxFile = Join-Path $ResxDir "$BaseName.$culture.resx"
    }

    Write-Resx -File $resxFile -Data $table
}

Write-Host "Done."