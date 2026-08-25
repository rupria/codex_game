[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$RepoRoot,
  [string]$AsepritePath = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
$repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot)
$assetsRoot = Join-Path $repoRootFull 'programer\CodexGame\Assets'
$referenceRoot = Join-Path $repoRootFull 'programer\CodexGame\ArtReferences\PrivateSelection_0_6_1'
$sourceRoot = Join-Path $referenceRoot 'source'
$previewRoot = Join-Path $referenceRoot 'preview'
$runtimeRoot = Join-Path $assetsRoot 'Art\Prototype\UI\PrivateSelection_0_6_1'
$baselineRoot = Join-Path $assetsRoot 'Art\Prototype\UI\PrivateSelection_0_6_0'
$baseline = Join-Path $baselineRoot 'private_selection_modal_panel_860x456_0_6_0.png'
$output = Join-Path $runtimeRoot 'private_selection_modal_panel_clean_860x456_0_6_1.png'
$preview = Join-Path $previewRoot 'private_selection_modal_panel_clean_preview_860x456_0_6_1.png'
$lua = Join-Path $sourceRoot 'remove_legacy_lower_left_frame.lua'
$metaGenerator = Join-Path $repoRootFull 'programer\CodexGame\ArtReferences\RuntimeBindingTools\generate_stable_unity_meta_0_6_0.ps1'
$templateMeta = Join-Path $baselineRoot 'private_selection_modal_panel_860x456_0_6_0.png.meta'

foreach ($required in @($AsepritePath, $baseline, $lua, $metaGenerator, $templateMeta)) {
  if (-not (Test-Path -LiteralPath $required)) { throw "Missing required file: $required" }
}

New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot | Out-Null

& $AsepritePath -b `
  --script-param "source=$baseline" `
  --script-param "output=$output" `
  --script $lua
if ($LASTEXITCODE -ne 0) { throw "Aseprite failed with exit code $LASTEXITCODE" }
for ($attempt = 0; $attempt -lt 20 -and -not (Test-Path -LiteralPath $output); $attempt++) {
  Start-Sleep -Milliseconds 100
}
if (-not (Test-Path -LiteralPath $output)) { throw "Aseprite output is missing: $output" }

Add-Type -AssemblyName System.Drawing
$bitmap = [System.Drawing.Bitmap]::new($output)
try {
  if ($bitmap.Width -ne 860 -or $bitmap.Height -ne 456) {
    throw "Unexpected output dimensions: $($bitmap.Width)x$($bitmap.Height)"
  }
  for ($y = 318; $y -le 363; $y++) {
    for ($x = 18; $x -le 207; $x++) {
      $pixel = $bitmap.GetPixel($x, $y)
      if ($pixel.A -ne 255 -or $pixel.R -ne 14 -or $pixel.G -ne 13 -or $pixel.B -ne 11) {
        throw "Legacy frame cleanup verification failed at ($x,$y)."
      }
    }
  }
}
finally {
  $bitmap.Dispose()
}

Copy-Item -LiteralPath $output -Destination $preview -Force
& $metaGenerator `
  -ProjectAssetsRoot $assetsRoot `
  -RuntimeDirectory $runtimeRoot `
  -TemplateMeta $templateMeta

$hashLines = [System.Collections.Generic.List[string]]::new()
foreach ($path in @($output, $preview)) {
  $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
  $relative = [System.IO.Path]::GetRelativePath($referenceRoot, $path).Replace('\', '/')
  $hashLines.Add("$hash  $relative")
}
[System.IO.File]::WriteAllLines((Join-Path $referenceRoot 'APPROVED.sha256'), $hashLines, $utf8)

Write-Output "PrivateSelection_0_6_1 generated with Aseprite: $output"
