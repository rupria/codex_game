[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceRoot '..\..\..\..\..'))
$assetsRoot = Join-Path $repoRoot 'programer\CodexGame\Assets'
$runtimeRoot = Join-Path $assetsRoot 'Art\Prototype\UI\JokerHandChoice_0_6_0'
$previewRoot = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\JokerHandChoice_0_6_0\preview'
$baselineRoot = Join-Path $sourceRoot 'baseline_0_5_9'
$approvedPanelPath = Join-Path $sourceRoot 'joker_hand_choice_panel_approved_no_vertical_600x420_0_6_0.png'

New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot | Out-Null

foreach ($stem in @(
  'joker_hand_choice_dim_960x540',
  'joker_hand_option_disabled_440x44',
  'joker_hand_option_hover_440x44',
  'joker_hand_option_idle_440x44',
  'joker_hand_option_selected_440x44'
)) {
  Copy-Item -LiteralPath (Join-Path $baselineRoot ($stem + '_0_5_9.png')) `
    -Destination (Join-Path $runtimeRoot ($stem + '_0_6_0.png')) -Force
}
Copy-Item -LiteralPath $approvedPanelPath `
  -Destination (Join-Path $runtimeRoot 'joker_hand_choice_panel_compact_600x420_0_6_0.png') -Force

$oldPanel = [System.Drawing.Bitmap]::FromFile((Join-Path $baselineRoot 'joker_hand_choice_panel_compact_600x420_0_5_9.png'))
$newPanel = [System.Drawing.Bitmap]::FromFile($approvedPanelPath)
$preview = [System.Drawing.Bitmap]::FromFile((Join-Path $baselineRoot 'joker_hand_choice_vertical_4_preview_960x540_0_5_9.png'))
try {
  $offsetX = 180
  $offsetY = 60
  for ($y = 0; $y -lt $oldPanel.Height; $y++) {
    for ($x = 0; $x -lt $oldPanel.Width; $x++) {
      $oldColor = $oldPanel.GetPixel($x, $y)
      $newColor = $newPanel.GetPixel($x, $y)
      if ($oldColor.ToArgb() -eq $newColor.ToArgb()) { continue }
      $previewColor = $preview.GetPixel($offsetX + $x, $offsetY + $y)
      if ($previewColor.ToArgb() -eq $oldColor.ToArgb()) {
        $preview.SetPixel($offsetX + $x, $offsetY + $y, $newColor)
      }
    }
  }
  $preview.Save(
    (Join-Path $previewRoot 'joker_hand_choice_vertical_4_preview_960x540_0_6_0.png'),
    [System.Drawing.Imaging.ImageFormat]::Png)
} finally {
  $oldPanel.Dispose()
  $newPanel.Dispose()
  $preview.Dispose()
}

Copy-Item -LiteralPath (Join-Path $baselineRoot 'joker_hand_option_states_960x260_0_5_9.png') `
  -Destination (Join-Path $previewRoot 'joker_hand_option_states_960x260_0_6_0.png') -Force

$templateMeta = Join-Path $assetsRoot 'Art\Prototype\UI\JokerHandChoice_0_5_9\joker_hand_option_idle_440x44_0_5_9.png.meta'
if (-not (Test-Path -LiteralPath $templateMeta)) {
  $templateMeta = Join-Path $assetsRoot 'Art\Prototype\UI\MainMenu_0_5_6\main_menu_start_idle_336x78_0_5_6.png.meta'
}
$metaGenerator = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\RuntimeBindingTools\generate_stable_unity_meta_0_6_0.ps1'
& $metaGenerator -ProjectAssetsRoot $assetsRoot -RuntimeDirectory $runtimeRoot -TemplateMeta $templateMeta

$pngCount = @(Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png').Count
$metaCount = @(Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png.meta').Count
if ($pngCount -ne 6 -or $metaCount -ne 6) {
  throw "JokerHandChoice 0.6.0 count mismatch: png=$pngCount meta=$metaCount"
}

Write-Output 'JokerHandChoice 0.6.0 generated.'
