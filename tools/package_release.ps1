<#
.SYNOPSIS
    Packages the built mod (dll + pck + manifest) into a zip for a GitHub release.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/package_release.ps1
    powershell -ExecutionPolicy Bypass -File tools/package_release.ps1 -ModsDir "D:\steam\steamapps\common\Slay the Spire 2\mods\DongniDefense"
#>
param(
    [string]$ModsDir = "D:\steam\steamapps\common\Slay the Spire 2\mods\DongniDefense",
    [string]$OutDir = "dist"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $root "DongniDefense.json"

if (-not (Test-Path $ModsDir)) {
    throw "Built mod folder not found: $ModsDir`nRun 'dotnet publish' first, or pass -ModsDir."
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$id = $manifest.id
$version = $manifest.version

$staging = Join-Path ([System.IO.Path]::GetTempPath()) "$id-$([guid]::NewGuid().ToString('N'))"
$target = Join-Path $staging $id
New-Item -ItemType Directory -Force -Path $target | Out-Null

foreach ($name in @("$id.dll", "$id.pck", "$id.json")) {
    $file = Join-Path $ModsDir $name
    if (-not (Test-Path $file)) {
        throw "Missing build output: $file"
    }
    Copy-Item $file $target
}

$outPath = Join-Path $root $OutDir
New-Item -ItemType Directory -Force -Path $outPath | Out-Null
$zip = Join-Path $outPath "$id-$version.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }

Compress-Archive -Path $target -DestinationPath $zip -CompressionLevel Optimal
Remove-Item $staging -Recurse -Force

$size = [math]::Round((Get-Item $zip).Length / 1MB, 2)
Write-Host "Packaged $id $version -> $zip ($size MB)"
Write-Host "Contents: $id/$id.dll, $id/$id.pck, $id/$id.json"
