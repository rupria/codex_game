param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Bullet_Rope_0_3_9\preview'
$outputDir = Join-Path $repoRoot 'outputs\art\bullet_rope_0_3_9\final'
$barRuntimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\BarShop_0_3_9'
$halliRuntimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\Halli_0_3_9'

$arguments = @(
  '--batch',
  '--script-param', "sourceDir=$sourceDir",
  '--script-param', "previewDir=$previewDir",
  '--script-param', "outputDir=$outputDir",
  '--script-param', "barRuntimeDir=$barRuntimeDir",
  '--script-param', "halliRuntimeDir=$halliRuntimeDir",
  '--script-param', "oldBullet=$(Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\BarShop_0_3_3\bar_shop_bullet_realistic_24x40_0_3_3.png')",
  '--script-param', "pouch=$(Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\BarShop_0_3_8\bar_shop_ammo_pouch_empty_180x150_0_3_8.png')",
  '--script-param', "halliBackground=$(Join-Path $repoRoot 'programer\CodexGame\ArtReferences\Halli_Item_0_3_7\preview\halli_two_card_fan_application_preview_960x540_0_3_7.png')",
  '--script', (Join-Path $sourceDir 'generate_bullet_rope_0_3_9.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

Write-Output 'Bullet and rope art 0.3.9 generated.'
