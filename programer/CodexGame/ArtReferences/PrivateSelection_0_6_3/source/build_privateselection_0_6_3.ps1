[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$RepoRoot,
  [string]$AsepritePath = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe'
)

$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
$repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot)
$assetsRoot = Join-Path $repoRootFull 'programer\CodexGame\Assets'
$referenceRoot = Join-Path $repoRootFull 'programer\CodexGame\ArtReferences\PrivateSelection_0_6_3'
$sourceRoot = Join-Path $referenceRoot 'source'
$previewRoot = Join-Path $referenceRoot 'preview'
$runtimeRoot = Join-Path $assetsRoot 'Art\Prototype\UI\PrivateSelection_0_6_3'
$baseline060 = Join-Path $assetsRoot 'Art\Prototype\UI\PrivateSelection_0_6_0'
$baseline061 = Join-Path $assetsRoot 'Art\Prototype\UI\PrivateSelection_0_6_1'
$buttonSource = Join-Path $baseline060 'private_selection_count_panel_184x64_0_6_0.png'
$modalSource = Join-Path $baseline061 'private_selection_modal_panel_clean_860x456_0_6_1.png'
$lua = Join-Path $sourceRoot 'build_private_selection_hand_lock_0_6_3.lua'
$idle = Join-Path $runtimeRoot 'private_selection_hand_lock_idle_184x120_0_6_3.png'
$hover = Join-Path $runtimeRoot 'private_selection_hand_lock_hover_184x120_0_6_3.png'
$active = Join-Path $runtimeRoot 'private_selection_hand_lock_active_184x120_0_6_3.png'
$disabled = Join-Path $runtimeRoot 'private_selection_hand_lock_disabled_184x120_0_6_3.png'
$preview = Join-Path $previewRoot 'private_selection_hand_lock_pool_inset_preview_860x456_0_6_3.png'
$approvedReference = Join-Path $previewRoot 'approved_private_selection_reference_1680x945_0_6_3.png'
$metaGenerator = Join-Path $repoRootFull 'programer\CodexGame\ArtReferences\RuntimeBindingTools\generate_stable_unity_meta_0_6_0.ps1'
$templateMeta = Join-Path $baseline060 'private_selection_count_panel_184x64_0_6_0.png.meta'

foreach ($required in @($AsepritePath, $buttonSource, $modalSource, $lua, $metaGenerator, $templateMeta)) {
  if (-not (Test-Path -LiteralPath $required)) { throw "Missing required file: $required" }
}
if (-not (Test-Path -LiteralPath $approvedReference)) {
  throw "Missing approved layout reference: $approvedReference"
}
New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot | Out-Null

& $AsepritePath -b `
  --script-param "modal=$modalSource" `
  --script-param "button=$buttonSource" `
  --script-param "idle=$idle" `
  --script-param "hover=$hover" `
  --script-param "active=$active" `
  --script-param "disabled=$disabled" `
  --script-param "preview=$preview" `
  --script $lua

$outputs = @($idle, $hover, $active, $disabled, $preview)
for ($attempt = 0; $attempt -lt 20; $attempt++) {
  $missing = @($outputs | Where-Object { -not (Test-Path -LiteralPath $_) })
  if ($missing.Count -eq 0) { break }
  Start-Sleep -Milliseconds 100
}
$missing = @($outputs | Where-Object { -not (Test-Path -LiteralPath $_) })
if ($missing.Count -gt 0) { throw "Aseprite output is missing: $($missing -join ', ')" }

Add-Type -AssemblyName System.Drawing
foreach ($path in @($idle, $hover, $active, $disabled)) {
  $bitmap = [System.Drawing.Bitmap]::new($path)
  try {
    if ($bitmap.Width -ne 184 -or $bitmap.Height -ne 120) {
      throw "Unexpected button dimensions: $path"
    }
  }
  finally { $bitmap.Dispose() }
}

& $metaGenerator `
  -ProjectAssetsRoot $assetsRoot `
  -RuntimeDirectory $runtimeRoot `
  -TemplateMeta $templateMeta

$hashLines = [System.Collections.Generic.List[string]]::new()
foreach ($path in @($outputs + $approvedReference)) {
  $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
  $relative = [System.IO.Path]::GetRelativePath($referenceRoot, $path).Replace('\', '/')
  $hashLines.Add("$hash  $relative")
}
[System.IO.File]::WriteAllLines((Join-Path $referenceRoot 'APPROVED.sha256'), $hashLines, $utf8)
Write-Output 'PrivateSelection_0_6_3 generated with Aseprite.'
