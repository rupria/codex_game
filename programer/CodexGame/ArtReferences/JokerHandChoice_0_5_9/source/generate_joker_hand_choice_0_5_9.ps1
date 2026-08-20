param(
  [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceDir '..\..\..\..\..'))
$runtimeDir = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\JokerHandChoice_0_5_9'
$previewDir = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\JokerHandChoice_0_5_9\preview'
$panel = Join-Path $sourceDir 'joker_hand_choice_panel_base_760x420_0_5_9.png'
$before = Join-Path $sourceDir 'joker_hand_choice_before_852x599_0_5_9.png'

New-Item -ItemType Directory -Force -Path $runtimeDir,$previewDir | Out-Null

$arguments = @(
  '--batch',
  '--script-param', "panel=$panel",
  '--script-param', "before=$before",
  '--script-param', "runtimeRoot=$runtimeDir",
  '--script-param', "sourceRoot=$sourceDir",
  '--script-param', "previewRoot=$previewDir",
  '--script', (Join-Path $sourceDir 'generate_joker_hand_choice_0_5_9.lua')
)

& $AsepriteExe @arguments
if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
  throw "Aseprite generation failed with exit code $LASTEXITCODE"
}

$templateMeta = Join-Path $repoRoot 'programer\CodexGame\Assets\Art\Prototype\UI\MainMenu_0_5_6\main_menu_start_idle_336x78_0_5_6.png.meta'
$metaGenerator = Join-Path $sourceDir 'generate_unity_meta_0_5_9.ps1'
& $metaGenerator -RuntimeDirectory $runtimeDir -TemplateMeta $templateMeta

$pngCount = @(Get-ChildItem -LiteralPath $runtimeDir -Filter '*.png').Count
$metaCount = @(Get-ChildItem -LiteralPath $runtimeDir | Where-Object Name -Like '*.png.meta').Count
if ($pngCount -ne $metaCount) {
  Start-Sleep -Milliseconds 100
  & $metaGenerator -RuntimeDirectory $runtimeDir -TemplateMeta $templateMeta
  $metaCount = @(Get-ChildItem -LiteralPath $runtimeDir | Where-Object Name -Like '*.png.meta').Count
}
if ($pngCount -ne 6 -or $metaCount -ne 6) {
  throw "JokerHandChoice meta generation mismatch: png=$pngCount meta=$metaCount"
}

Write-Output 'JokerHandChoice 0.5.9 generated.'
