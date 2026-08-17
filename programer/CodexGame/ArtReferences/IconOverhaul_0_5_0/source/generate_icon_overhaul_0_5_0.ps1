param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\IconOverhaul_0_5_0\preview'
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\IconOverhaul_0_5_0'
$outputDir = Join-Path $repoRoot 'outputs\art\icon_overhaul_0_5_0\final'
$uiRoot = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI'

foreach ($dir in @($previewDir, $runtimeDir, $outputDir)) {
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

& $AsepriteExe --batch `
  --script-param "runtimeDir=$runtimeDir" `
  --script-param "sourceDir=$sourceDir" `
  --script-param "previewDir=$previewDir" `
  --script-param "outputDir=$outputDir" `
  --script (Join-Path $sourceDir 'generate_icon_overhaul_0_5_0.lua')

Start-Sleep -Milliseconds 800

& $AsepriteExe --batch `
  --script-param "uiRoot=$uiRoot" `
  --script-param "newRoot=$runtimeDir" `
  --script-param "previewDir=$previewDir" `
  --script-param "outputDir=$outputDir" `
  --script (Join-Path $sourceDir 'generate_icon_overhaul_comparison_0_5_0.lua')

Start-Sleep -Milliseconds 800
Write-Output 'Icon overhaul 0.5.0 generated locally. No Git operation was performed.'
