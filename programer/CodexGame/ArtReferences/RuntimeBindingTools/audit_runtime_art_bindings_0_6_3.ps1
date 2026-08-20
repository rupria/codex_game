[CmdletBinding()]
param(
  [string]$CodexGameRoot = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CodexGameRoot)) {
  $CodexGameRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
} else {
  $CodexGameRoot = [System.IO.Path]::GetFullPath($CodexGameRoot)
}

$assetsRoot = Join-Path $CodexGameRoot "Assets"
$manifestPath = Join-Path $PSScriptRoot "current_art_runtime_manifest_0_6_3.json"
$builderPath = Join-Path $assetsRoot "Editor\PlayableDevSceneBuilder.cs"
$scenePath = Join-Path $assetsRoot "Scenes\PlayableDev.unity"

foreach ($required in @($manifestPath, $builderPath, $scenePath)) {
  if (-not (Test-Path -LiteralPath $required)) {
    throw "Required audit input is missing: $required"
  }
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$builderText = Get-Content -LiteralPath $builderPath -Raw
$sceneText = Get-Content -LiteralPath $scenePath -Raw
$pokerPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerDevPanel.cs"
$pokerLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerTableLayout.cs"
$pokerPanelText = Get-Content -LiteralPath $pokerPanelPath -Raw
$pokerLayoutText = Get-Content -LiteralPath $pokerLayoutPath -Raw
$failed = $false

Write-Output "ART_RUNTIME_AUDIT revision=$($manifest.manifestRevision)"
Write-Output "PENDING_PROGRAMMER_BINDINGS"

foreach ($entry in $manifest.pendingProgrammerBindings) {
  $runtimeRoot = Join-Path $CodexGameRoot ([string]$entry.runtimeRoot)
  $exists = Test-Path -LiteralPath $runtimeRoot
  $builderMentionsPackage = $builderText.Contains([string]$entry.package)
  Write-Output ("issue={0} package={1} assets={2} builderBound={3}" -f `
    $entry.issue, $entry.package, $exists, $builderMentionsPackage)
  if (-not $exists) {
    $failed = $true
  }
}

Write-Output "ISSUE_67_68_73_74_BINDING_STATUS"
$predictionPackageBound = $builderText.Contains('PokerPredictionClean_0_6_1')
$jokerPackageBound = $builderText.Contains('JokerHandChoice_0_6_0')
$predictionLabelsDrawn = $pokerPanelText.Contains('UI_POKER_PREDICTION_TITLE') `
  -and -not $pokerPanelText.Contains('new GUIContent(string.Empty, label)')
$largeResultModalRemoved = -not $pokerPanelText.Contains('PokerResultOverlayRenderer.Draw(')
$predictionLayoutUpdated = $pokerLayoutText.Contains('new Rect(139f, 456f, 232f, 64f)') `
  -and $pokerLayoutText.Contains('new Rect(589f, 456f, 232f, 64f)')
$jokerTwoColumnFallbackRemoved = -not $pokerPanelText.Contains('var column = index % 2;')
Write-Output ("predictionPackageBound={0} labelsDrawn={1} layout232x64={2} largeResultModalRemoved={3}" -f `
  $predictionPackageBound, $predictionLabelsDrawn, $predictionLayoutUpdated, $largeResultModalRemoved)
Write-Output ("jokerPackageBound={0} twoColumnFallbackRemoved={1}" -f `
  $jokerPackageBound, $jokerTwoColumnFallbackRemoved)

Write-Output "CANONICAL_LAYOUT_CONTRACTS"
foreach ($entry in $manifest.canonicalLayoutContracts) {
  $layoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliPileOverlapLayout.cs"
  $layoutText = Get-Content -LiteralPath $layoutPath -Raw
  $panelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliDevPanel.cs"
  $panelText = Get-Content -LiteralPath $panelPath -Raw
  $maximumMatch = [regex]::Match(
    $layoutText,
    "MaximumPileCards\s*=\s*(\d+)")
  $actualMaximum = if ($maximumMatch.Success) {
    [int]$maximumMatch.Groups[1].Value
  } else {
    -1
  }
  $widthMatch = [regex]::Match($layoutText, "CardWidth\s*=\s*([0-9.]+)f")
  $stepMatch = [regex]::Match($layoutText, "CardStepX\s*=\s*([0-9.]+)f")
  $actualOverlapRatio = if ($widthMatch.Success -and $stepMatch.Success) {
    $width = [double]$widthMatch.Groups[1].Value
    $step = [double]$stepMatch.Groups[1].Value
    ($width - $step) / $width
  } else {
    1.0
  }
  $newestDrawnLast = $layoutText.Contains("return drawIndex;") `
    -and $panelText.Contains("history.RemoveAt(0);") `
    -and $panelText.Contains("history.Add(card);")
  Write-Output ("issue={0} package={1} expectedMax={2} currentMax={3} overlapRatio={4:N3} newestTop={5}" -f `
    $entry.issue,
    $entry.package,
    $entry.expectedMaximumVisiblePerPile,
    $actualMaximum,
    $actualOverlapRatio,
    $newestDrawnLast)
  if ($actualMaximum -ne [int]$entry.expectedMaximumVisiblePerPile `
    -or $actualOverlapRatio -gt [double]$entry.maximumHorizontalOverlapRatio `
    -or -not $newestDrawnLast) {
    $failed = $true
  }
}

Write-Output "ARCHIVED_CONFLICTING_CONTRACTS"
foreach ($entry in $manifest.archivedConflictingContracts) {
  $restoredContract = Join-Path $CodexGameRoot ("ArtReferences\" + [string]$entry.package)
  $restored = Test-Path -LiteralPath $restoredContract
  Write-Output ("package={0} restoredAsCurrent={1}" -f $entry.package, $restored)
  if ($restored) {
    $failed = $true
  }
}

Write-Output "ARCHIVED_RUNTIME_GUARD"
foreach ($entry in $manifest.archivedSupersededRuntime) {
  $restoredRuntime = Join-Path $assetsRoot ("Art\Prototype\UI\" + [string]$entry.package)
  $restored = Test-Path -LiteralPath $restoredRuntime
  Write-Output ("package={0} restoredUnderAssets={1}" -f $entry.package, $restored)
  if ($restored) {
    $failed = $true
  }
}

$guidToPackage = @{}
$uiRoot = Join-Path $assetsRoot "Art\Prototype\UI"
Get-ChildItem -LiteralPath $uiRoot -Recurse -Filter "*.meta" -File | ForEach-Object {
  $guidMatch = Select-String -LiteralPath $_.FullName -Pattern "^guid: (.+)$"
  if (-not $guidMatch) {
    return
  }

  $relative = $_.FullName.Substring($uiRoot.Length + 1)
  $separator = $relative.IndexOf([char]92)
  if ($separator -le 0) {
    return
  }

  $guid = $guidMatch.Matches[0].Groups[1].Value
  $guidToPackage[$guid] = $relative.Substring(0, $separator)
}

$scenePackageCounts = @{}
foreach ($guid in $guidToPackage.Keys) {
  if (-not $sceneText.Contains($guid)) {
    continue
  }

  $package = $guidToPackage[$guid]
  if (-not $scenePackageCounts.ContainsKey($package)) {
    $scenePackageCounts[$package] = 0
  }
  $scenePackageCounts[$package]++
}

Write-Output "SERIALIZED_LEGACY_BINDINGS"
foreach ($entry in $manifest.serializedLegacyBindings) {
  $actual = if ($scenePackageCounts.ContainsKey([string]$entry.package)) {
    $scenePackageCounts[[string]$entry.package]
  } else {
    0
  }
  Write-Output ("package={0} sceneReferences={1}" -f $entry.package, $actual)
}

if ($failed) {
  throw "ART_RUNTIME_AUDIT_FAILED"
}

Write-Output "ART_RUNTIME_AUDIT_OK"
