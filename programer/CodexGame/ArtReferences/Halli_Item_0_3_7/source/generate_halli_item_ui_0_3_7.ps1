param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Halli_Item_0_3_7\preview'
$outputDir = Join-Path $repoRoot 'outputs\art\halli_item_0_3_7\final'
$halliDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\Halli_0_3_7'
$pokerDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\Poker_0_3_7'
$cardRoot = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\Cards_0_06'
$uiRoot = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI'

$arguments = @(
  '--batch',
  '--script-param', "sourceDir=$sourceDir",
  '--script-param', "previewDir=$previewDir",
  '--script-param', "outputDir=$outputDir",
  '--script-param', "halliDir=$halliDir",
  '--script-param', "pokerDir=$pokerDir",
  '--script-param', "halliBase=$(Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Halli_StatusTray_0_3_4\preview\halli_bottom_ui_application_preview_960x540_0_3_4.png')",
  '--script-param', "pokerBase=$(Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Gameplay_0_1_2\preview\saloon_lighting_visibility_reference_960x540_0_3_6.png')",
  '--script-param', "crateEmpty=$(Join-Path $uiRoot 'Poker_0_3_4\poker_item_crate_open_empty_160x160_0_3_4.png')",
  '--script-param', "crateFilled=$(Join-Path $uiRoot 'Poker_0_3_4\poker_item_crate_open_filled_160x160_0_3_4.png')",
  '--script-param', "card1=$(Join-Path $cardRoot 'halli_variants\card_halli_hearts_8_skull_03.png')",
  '--script-param', "card2=$(Join-Path $cardRoot 'halli_variants\card_halli_spades_10_skull_02.png')",
  '--script-param', "card3=$(Join-Path $cardRoot 'halli_variants\card_halli_diamonds_6_skull_02.png')",
  '--script-param', "card4=$(Join-Path $cardRoot 'halli_variants\card_halli_clubs_4_skull_01.png')",
  '--script-param', "card5=$(Join-Path $cardRoot 'halli_variants\card_halli_hearts_7_skull_02.png')",
  '--script-param', "card6=$(Join-Path $cardRoot 'halli_variants\card_halli_spades_3_skull_01.png')",
  '--script-param', "card7=$(Join-Path $cardRoot 'halli_variants\card_halli_diamonds_9_skull_03.png')",
  '--script-param', "card8=$(Join-Path $cardRoot 'halli_variants\card_halli_clubs_5_skull_02.png')",
  '--script-param', "icon1=$(Join-Path $uiRoot 'Gameplay_0_1_2\item_reload_64_0_1_2.png')",
  '--script-param', "icon2=$(Join-Path $uiRoot 'Gameplay_0_1_2\item_bottom_deal_64_0_1_2.png')",
  '--script-param', "icon3=$(Join-Path $uiRoot 'Gameplay_0_1_2\item_hype_man_64_0_1_2.png')",
  '--script-param', "icon4=$(Join-Path $uiRoot 'Gameplay_0_1_2\item_heal_tonic_64_0_1_2.png')",
  '--script-param', "targetCard1=$(Join-Path $cardRoot 'poker_variants\card_poker_hearts_q.png')",
  '--script-param', "targetCard2=$(Join-Path $cardRoot 'poker_variants\card_poker_spades_10.png')",
  '--script-param', "targetCard3=$(Join-Path $cardRoot 'poker_variants\card_poker_diamonds_8.png')",
  '--script', (Join-Path $sourceDir 'generate_halli_item_ui_0_3_7.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

Write-Output 'Halli and Poker item UI 0.3.7 generated.'
