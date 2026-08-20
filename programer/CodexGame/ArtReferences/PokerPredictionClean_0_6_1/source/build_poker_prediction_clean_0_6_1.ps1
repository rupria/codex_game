[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $sourceRoot '..\..\..\..\..'))
$assetsRoot = Join-Path $repoRoot 'programer\CodexGame\Assets'
$runtimeRoot = Join-Path $assetsRoot 'Art\Prototype\UI\PokerPredictionClean_0_6_1'
$previewRoot = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\PokerPredictionClean_0_6_1\preview'
$baselineRoot = Join-Path $sourceRoot 'baseline_0_6_0'

New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot | Out-Null

function Save-Png([System.Drawing.Bitmap]$bitmap, [string]$path) {
  $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
}

function Remove-Legacy-Pictogram([string]$sourcePath, [string]$destinationPath) {
  $bitmap = [System.Drawing.Bitmap]::FromFile($sourcePath)
  try {
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
      $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
      $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
      $sourceRect = [System.Drawing.Rectangle]::new(68, 16, 36, 32)
      $destinationRect = [System.Drawing.Rectangle]::new(25, 16, 36, 32)
      $graphics.DrawImage($bitmap, $destinationRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
    } finally {
      $graphics.Dispose()
    }
    Save-Png $bitmap $destinationPath
  } finally {
    $bitmap.Dispose()
  }
}

$states = @('idle', 'hover', 'selected', 'disabled')
foreach ($side in @('player', 'ai')) {
  foreach ($state in $states) {
    $oldName = "poker_prediction_${side}_${state}_232x64_0_6_0.png"
    $newName = "poker_prediction_${side}_${state}_232x64_0_6_1.png"
    Remove-Legacy-Pictogram (Join-Path $baselineRoot $oldName) (Join-Path $runtimeRoot $newName)
  }
}

$copiedAssets = @(
  'poker_insurance_remaining_icon_28',
  'poker_prediction_stage_emblem_40',
  'poker_prediction_success_icon_28',
  'poker_prediction_title_plate_320x48',
  'poker_result_continue_hover_164x44',
  'poker_result_continue_idle_164x44'
)
foreach ($stem in $copiedAssets) {
  Copy-Item -LiteralPath (Join-Path $baselineRoot ($stem + '_0_6_0.png')) `
    -Destination (Join-Path $runtimeRoot ($stem + '_0_6_1.png')) -Force
}

$sheet = [System.Drawing.Bitmap]::new(960, 360)
try {
  $graphics = [System.Drawing.Graphics]::FromImage($sheet)
  try {
    $graphics.Clear([System.Drawing.Color]::FromArgb(255, 9, 7, 6))
    for ($index = 0; $index -lt $states.Count; $index++) {
      $state = $states[$index]
      foreach ($entry in @(
        @{ Side = 'player'; X = 20 },
        @{ Side = 'ai'; X = 280 }
      )) {
        $path = Join-Path $runtimeRoot ("poker_prediction_{0}_{1}_232x64_0_6_1.png" -f $entry.Side, $state)
        $image = [System.Drawing.Image]::FromFile($path)
        try {
          $graphics.DrawImageUnscaled($image, [int]$entry.X, 20 + $index * 80)
        } finally {
          $image.Dispose()
        }
      }
    }
    foreach ($entry in @(
      @{ File = 'poker_prediction_title_plate_320x48_0_6_1.png'; X = 560; Y = 20 },
      @{ File = 'poker_prediction_stage_emblem_40_0_6_1.png'; X = 572; Y = 24 },
      @{ File = 'poker_insurance_remaining_icon_28_0_6_1.png'; X = 568; Y = 102 },
      @{ File = 'poker_prediction_success_icon_28_0_6_1.png'; X = 568; Y = 174 },
      @{ File = 'poker_result_continue_idle_164x44_0_6_1.png'; X = 640; Y = 102 },
      @{ File = 'poker_result_continue_hover_164x44_0_6_1.png'; X = 640; Y = 174 }
    )) {
      $image = [System.Drawing.Image]::FromFile((Join-Path $runtimeRoot $entry.File))
      try {
        $graphics.DrawImageUnscaled($image, [int]$entry.X, [int]$entry.Y)
      } finally {
        $image.Dispose()
      }
    }
  } finally {
    $graphics.Dispose()
  }
  Save-Png $sheet (Join-Path $previewRoot 'poker_prediction_clean_asset_states_960x360_0_6_1.png')
} finally {
  $sheet.Dispose()
}

$templateMeta = Join-Path $assetsRoot 'Art\Prototype\UI\PokerPredictionClean_0_6_0\poker_prediction_player_idle_232x64_0_6_0.png.meta'
if (-not (Test-Path -LiteralPath $templateMeta)) {
  $templateMeta = Join-Path $assetsRoot 'Art\Prototype\UI\MainMenu_0_5_6\main_menu_start_idle_336x78_0_5_6.png.meta'
}
$metaGenerator = Join-Path $repoRoot 'programer\CodexGame\ArtReferences\RuntimeBindingTools\generate_stable_unity_meta_0_6_0.ps1'
& $metaGenerator -ProjectAssetsRoot $assetsRoot -RuntimeDirectory $runtimeRoot -TemplateMeta $templateMeta

$pngCount = @(Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png').Count
$metaCount = @(Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png.meta').Count
if ($pngCount -ne 14 -or $metaCount -ne 14) {
  throw "PokerPredictionClean 0.6.1 count mismatch: png=$pngCount meta=$metaCount"
}

Write-Output 'PokerPredictionClean 0.6.1 generated.'
