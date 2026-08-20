param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\MainMenu_0_5_8'
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\MainMenu_0_5_8\preview'
$base = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\Board\halli_western_saloon_background.png'
$crest = Join-Path $sourceDir 'main_menu_duel_crest_imagegen_source_1536x1024_0_5_8.png'

New-Item -ItemType Directory -Force -Path $runtimeDir,$previewDir | Out-Null

$arguments = @(
  '--batch',
  '--script-param', "base=$base",
  '--script-param', "crest=$crest",
  '--script-param', "runtimeRoot=$runtimeDir",
  '--script-param', "sourceRoot=$sourceDir",
  '--script-param', "previewRoot=$previewDir",
  '--script', (Join-Path $sourceDir 'generate_main_menu_0_5_8.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

$templateMeta = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\MainMenu_0_5_6\main_menu_start_idle_336x78_0_5_6.png.meta'
& (Join-Path $sourceDir 'generate_unity_meta_0_5_8.ps1') -RuntimeDirectory $runtimeDir -TemplateMeta $templateMeta

Write-Output 'MainMenu 0.5.8 generated.'
