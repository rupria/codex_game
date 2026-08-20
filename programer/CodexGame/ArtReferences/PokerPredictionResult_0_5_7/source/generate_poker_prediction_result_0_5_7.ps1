param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\PokerPredictionResult_0_5_7\preview'
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\PokerPredictionResult_0_5_7'
$cardRoot = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\Cards_0_06\poker_variants'
$uiRoot = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI'

$arguments = @(
  '--batch',
  '--script-param', "outDir=$runtimeDir",
  '--script-param', "previewDir=$previewDir",
  '--script-param', "sourceDir=$sourceDir",
  '--script-param', "background=$(Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Halli_0_2_0\halli_western_round_table_unlit_960x540_0_2_0.png')",
  '--script-param', "cardRoot=$cardRoot",
  '--script-param', "cardBack=$(Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\Cards_0_06\card_back.png')",
  '--script-param', "crate=$(Join-Path $uiRoot 'Poker_0_3_4\poker_item_crate_closed_160x160_0_3_4.png')",
  '--script', (Join-Path $sourceDir 'generate_poker_prediction_result_0_5_7.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

$templateMeta = Join-Path $uiRoot 'Poker_0_3_4\poker_item_crate_closed_160x160_0_3_4.png.meta'
& (Join-Path $sourceDir 'generate_unity_meta_0_5_7.ps1') `
  -RuntimeDirectory $runtimeDir `
  -TemplateMeta $templateMeta

Write-Output 'PokerPredictionResult 0.5.7 generated.'
