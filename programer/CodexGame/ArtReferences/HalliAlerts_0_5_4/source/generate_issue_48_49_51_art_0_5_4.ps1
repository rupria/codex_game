param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$halliRuntimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\HalliAlerts_0_5_4'
$rewardRuntimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\StageReward_0_5_4'
$halliPreviewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\HalliAlerts_0_5_4\preview'
$rewardPreviewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\StageReward_0_5_4\preview'
$rewardSourceDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\StageReward_0_5_4\source'
$background = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\BarShop_0_3_0\bar_shop_background_unlit_960x540_0_3_0.png'

$directories = @($halliRuntimeDir, $rewardRuntimeDir, $halliPreviewDir, $rewardPreviewDir, $rewardSourceDir)
foreach ($directory in $directories) {
  New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

$arguments = @(
  '--batch',
  '--script-param', "halliRuntimeDir=$halliRuntimeDir",
  '--script-param', "rewardRuntimeDir=$rewardRuntimeDir",
  '--script-param', "halliPreviewDir=$halliPreviewDir",
  '--script-param', "rewardPreviewDir=$rewardPreviewDir",
  '--script-param', "halliSourceDir=$sourceDir",
  '--script-param', "rewardSourceDir=$rewardSourceDir",
  '--script-param', "background=$background",
  '--script', (Join-Path $sourceDir 'generate_issue_48_49_51_art_0_5_4.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

Write-Output 'GitHub issue 48, 49 and 51 art 0.5.4 generated.'
