param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Textless_Currency_0_4_0\preview'
$outputDir = Join-Path $repoRoot 'outputs\art\textless_currency_0_4_0\final'
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\Textless_Currency_0_4_0'

$arguments = @(
  '--batch',
  '--script-param', "sourceDir=$sourceDir",
  '--script-param', "previewDir=$previewDir",
  '--script-param', "outputDir=$outputDir",
  '--script-param', "runtimeDir=$runtimeDir",
  '--script-param', "bullet=$(Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\BarShop_0_3_9\bar_shop_bullet_western_brass_24x40_0_3_9.png')",
  '--script-param', "player=$(Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\Characters\player_silhouette.png')",
  '--script-param', "ai=$(Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\Characters\ai_silhouette.png')",
  '--script', (Join-Path $sourceDir 'generate_textless_currency_0_4_0.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

& (Join-Path $sourceDir 'generate_unity_meta_0_4_0.ps1') -RepoRoot $repoRoot
Write-Output 'Textless currency and portrait UI art 0.4.0 generated.'
