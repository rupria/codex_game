param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\BarShop_Payment_0_3_8\preview'
$outputDir = Join-Path $repoRoot 'outputs\art\bar_shop_payment_0_3_8\final'
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\BarShop_0_3_8'
$uiRoot = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI'

$arguments = @(
  '--batch',
  '--script-param', "sourceDir=$sourceDir",
  '--script-param', "previewDir=$previewDir",
  '--script-param', "outputDir=$outputDir",
  '--script-param', "runtimeDir=$runtimeDir",
  '--script-param', "background=$(Join-Path $repoRoot 'programer\CodexGame\ArtReferences\BarShop_0_3_4\preview\bar_shop_lighting_visibility_reference_960x540_0_3_6.png')",
  '--script-param', "bullet=$(Join-Path $uiRoot 'BarShop_0_3_3\bar_shop_bullet_realistic_24x40_0_3_3.png')",
  '--script-param', "bulletShine=$(Join-Path $uiRoot 'BarShop_0_3_3\bar_shop_bullet_realistic_shine_24x40_0_3_3.png')",
  '--script-param', "pouch=$(Join-Path $uiRoot 'BarShop_0_3_4\bar_shop_ammo_pouch_180x150_0_3_4.png')",
  '--script', (Join-Path $sourceDir 'generate_bar_shop_payment_0_3_8.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

Write-Output 'Bar shop payment art 0.3.8 generated.'
