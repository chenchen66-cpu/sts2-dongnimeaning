<#
.SYNOPSIS
    Fills the workshop workspace with the freshly built mod files and (re)generates the images.

.DESCRIPTION
    Copy the built mod into workshop/content/DongniDefense, then run tools/make_workshop_images.py.
    After that, upload with the official uploader:

      cd <ModUploader folder>
      .\ModUploader.exe upload -w "<repo>\workshop"

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/prepare_workshop.ps1
#>
param(
    [string]$ModsDir = "D:\steam\steamapps\common\Slay the Spire 2\mods\DongniDefense"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$content = Join-Path $root "workshop\content\DongniDefense"

if (-not (Test-Path $ModsDir)) {
    throw "Built mod folder not found: $ModsDir`nRun 'dotnet publish' first, or pass -ModsDir."
}

New-Item -ItemType Directory -Force -Path $content | Out-Null

foreach ($name in @("DongniDefense.dll", "DongniDefense.pck", "DongniDefense.json")) {
    $file = Join-Path $ModsDir $name
    if (-not (Test-Path $file)) { throw "Missing build output: $file" }
    Copy-Item $file $content -Force
    Write-Host ("copied {0} ({1:N2} MB)" -f $name, ((Get-Item $file).Length / 1MB))
}

python (Join-Path $PSScriptRoot "make_workshop_images.py")

Write-Host ""
Write-Host "Workspace ready: $root\workshop"
Write-Host "Upload with:"
Write-Host "  cd `"<folder with ModUploader.exe>`""
Write-Host "  .\ModUploader.exe upload -w `"$root\workshop`""
