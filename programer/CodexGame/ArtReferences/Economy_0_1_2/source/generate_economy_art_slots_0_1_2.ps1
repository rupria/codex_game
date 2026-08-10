param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Economy_0_1_2\preview'
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\Economy_0_1_2'

$arguments = @(
  '--batch',
  '--script-param', "sourceDir=$sourceDir",
  '--script-param', "previewDir=$previewDir",
  '--script-param', "runtimeDir=$runtimeDir",
  '--script', (Join-Path $sourceDir 'generate_economy_art_slots_0_1_2.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

& (Join-Path $sourceDir 'generate_economy_unity_meta_0_1_2.ps1') -RepoRoot $repoRoot
Write-Output 'Economy art hook assets 0.1.2 generated.'
