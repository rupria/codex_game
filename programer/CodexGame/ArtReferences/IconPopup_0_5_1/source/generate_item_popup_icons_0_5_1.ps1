param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\IconOverhaul_0_5_1'
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\IconPopup_0_5_1\preview'
$outputDir = Join-Path $repoRoot 'outputs\art\icon_popup_0_5_1\final'
$popupBase = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Halli_Item_0_3_7\preview\poker_item_select_stage_preview_960x540_0_3_7.png'

foreach ($dir in @($runtimeDir, $previewDir, $outputDir)) {
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

& $AsepriteExe --batch `
  --script-param "runtimeDir=$runtimeDir" `
  --script-param "sourceDir=$sourceDir" `
  --script-param "previewDir=$previewDir" `
  --script-param "outputDir=$outputDir" `
  --script-param "popupBase=$popupBase" `
  --script (Join-Path $sourceDir 'generate_item_popup_icons_0_5_1.lua')

Write-Output 'Item popup icon set 0.5.1 generated locally. No Git operation was performed.'
