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
$manifestPath = Join-Path $PSScriptRoot "current_art_runtime_manifest_0_6_4.json"
$builderPath = Join-Path $assetsRoot "Editor\PlayableDevSceneBuilder.cs"
$scenePath = Join-Path $assetsRoot "Scenes\PlayableDev.unity"
$pokerPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerDevPanel.cs"
$pokerLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerTableLayout.cs"
$halliLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliPileOverlapLayout.cs"
$halliPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliDevPanel.cs"

foreach ($required in @(
  $manifestPath,
  $builderPath,
  $scenePath,
  $pokerPanelPath,
  $pokerLayoutPath,
  $halliLayoutPath,
  $halliPanelPath
)) {
  if (-not (Test-Path -LiteralPath $required)) {
    throw "Required release-gate input is missing: $required"
  }
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$builderText = Get-Content -LiteralPath $builderPath -Raw
$sceneText = Get-Content -LiteralPath $scenePath -Raw
$pokerPanelText = Get-Content -LiteralPath $pokerPanelPath -Raw
$pokerLayoutText = Get-Content -LiteralPath $pokerLayoutPath -Raw
$halliLayoutText = Get-Content -LiteralPath $halliLayoutPath -Raw
$halliPanelText = Get-Content -LiteralPath $halliPanelPath -Raw
$failures = [System.Collections.Generic.List[string]]::new()

function Add-GateFailure {
  param([string]$Message)

  $failures.Add($Message)
  Write-Output "FAIL $Message"
}

function Get-PackageSceneReferenceCount {
  param(
    [string]$RuntimeRoot,
    [string]$Package
  )

  if (-not (Test-Path -LiteralPath $RuntimeRoot)) {
    return 0
  }

  $count = 0
  Get-ChildItem -LiteralPath $RuntimeRoot -Recurse -Filter "*.meta" -File | ForEach-Object {
    $guidMatch = Select-String -LiteralPath $_.FullName -Pattern "^guid: (.+)$"
    if (-not $guidMatch) {
      return
    }

    $guid = $guidMatch.Matches[0].Groups[1].Value
    if ($sceneText.Contains($guid)) {
      $count++
    }
  }

  return $count
}

function Get-UiPackageSceneReferenceCount {
  param([string]$Package)

  $packageRoot = Join-Path $assetsRoot ("Art\Prototype\UI\" + $Package)
  return Get-PackageSceneReferenceCount -RuntimeRoot $packageRoot -Package $Package
}

Write-Output "ART_RUNTIME_RELEASE_GATE revision=$($manifest.manifestRevision)"
Write-Output "BASELINE_DEV $($manifest.baselineDev)"

if ($manifest.manifestRevision -ne "0.6.4") {
  Add-GateFailure "manifest revision is not 0.6.4"
}

Write-Output "REQUIRED_RUNTIME_BINDINGS"
foreach ($entry in $manifest.requiredRuntimeBindings) {
  $runtimeRoot = Join-Path $CodexGameRoot ([string]$entry.runtimeRoot)
  $assetsExist = Test-Path -LiteralPath $runtimeRoot
  $builderBound = $builderText.Contains([string]$entry.package)
  $sceneReferences = Get-PackageSceneReferenceCount `
    -RuntimeRoot $runtimeRoot `
    -Package ([string]$entry.package)

  Write-Output ("issue={0} package={1} assets={2} builderBound={3} sceneReferences={4}" -f `
    $entry.issue,
    $entry.package,
    $assetsExist,
    $builderBound,
    $sceneReferences)

  if (-not $assetsExist) {
    Add-GateFailure "issue $($entry.issue): runtime assets missing for $($entry.package)"
  }
  if ([bool]$entry.requireBuilderBinding -and -not $builderBound) {
    Add-GateFailure "issue $($entry.issue): builder does not bind $($entry.package)"
  }
  if ([bool]$entry.requireSceneBinding -and $sceneReferences -le 0) {
    Add-GateFailure "issue $($entry.issue): saved scene does not bind $($entry.package)"
  }
}

Write-Output "FORBIDDEN_LEGACY_BINDINGS"
foreach ($entry in $manifest.forbiddenLegacyBindings) {
  $package = [string]$entry.package
  $builderBound = $builderText.Contains($package)
  $knownGuids = if ($entry.PSObject.Properties.Name -contains "knownSceneGuids") {
    @($entry.knownSceneGuids | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })
  } else {
    @()
  }
  $sceneReferences = if ($knownGuids.Count -gt 0) {
    @($knownGuids | Where-Object { $sceneText.Contains([string]$_) }).Count
  } else {
    Get-UiPackageSceneReferenceCount -Package $package
  }
  Write-Output ("package={0} builderBound={1} sceneReferences={2}" -f `
    $package,
    $builderBound,
    $sceneReferences)

  if ($builderBound) {
    Add-GateFailure "legacy package remains in builder: $package"
  }
  if ($sceneReferences -gt 0) {
    Add-GateFailure "legacy package remains in saved scene: $package ($sceneReferences refs)"
  }
}

Write-Output "CANONICAL_LAYOUT_CONTRACTS"
foreach ($entry in $manifest.canonicalLayoutContracts) {
  $maximumMatch = [regex]::Match($halliLayoutText, "MaximumPileCards\s*=\s*(\d+)")
  $actualMaximum = if ($maximumMatch.Success) {
    [int]$maximumMatch.Groups[1].Value
  } else {
    -1
  }
  $widthMatch = [regex]::Match($halliLayoutText, "CardWidth\s*=\s*([0-9.]+)f")
  $stepMatch = [regex]::Match($halliLayoutText, "CardStepX\s*=\s*([0-9.]+)f")
  $actualOverlapRatio = if ($widthMatch.Success -and $stepMatch.Success) {
    $width = [double]$widthMatch.Groups[1].Value
    $step = [double]$stepMatch.Groups[1].Value
    ($width - $step) / $width
  } else {
    1.0
  }
  $newestDrawnLast = $halliLayoutText.Contains("return drawIndex;") `
    -and $halliPanelText.Contains("history.RemoveAt(0);") `
    -and $halliPanelText.Contains("history.Add(card);")

  Write-Output ("issue={0} package={1} expectedMax={2} currentMax={3} overlapRatio={4:N3} newestTop={5}" -f `
    $entry.issue,
    $entry.package,
    $entry.expectedMaximumVisiblePerPile,
    $actualMaximum,
    $actualOverlapRatio,
    $newestDrawnLast)

  if ($actualMaximum -ne [int]$entry.expectedMaximumVisiblePerPile) {
    Add-GateFailure "issue $($entry.issue): maximum visible cards changed from $($entry.expectedMaximumVisiblePerPile) to $actualMaximum"
  }
  if ($actualOverlapRatio -gt [double]$entry.maximumHorizontalOverlapRatio) {
    Add-GateFailure "issue $($entry.issue): overlap ratio $actualOverlapRatio exceeds $($entry.maximumHorizontalOverlapRatio)"
  }
  if (-not $newestDrawnLast) {
    Add-GateFailure "issue $($entry.issue): latest revealed card is not guaranteed on top"
  }
}

Write-Output "ARCHIVE_GUARDS"
foreach ($entry in $manifest.archivedConflictingContracts) {
  $restoredContract = Join-Path $CodexGameRoot ("ArtReferences\" + [string]$entry.package)
  $restored = Test-Path -LiteralPath $restoredContract
  Write-Output ("contract={0} restoredAsCurrent={1}" -f $entry.package, $restored)
  if ($restored) {
    Add-GateFailure "archived conflicting contract was restored: $($entry.package)"
  }
}
foreach ($entry in $manifest.archivedSupersededRuntime) {
  $restoredRuntime = Join-Path $assetsRoot ("Art\Prototype\UI\" + [string]$entry.package)
  $restored = Test-Path -LiteralPath $restoredRuntime
  Write-Output ("runtime={0} restoredUnderAssets={1}" -f $entry.package, $restored)
  if ($restored) {
    Add-GateFailure "archived runtime was restored under Assets: $($entry.package)"
  }
}

Write-Output "POKER_ACCEPTANCE_CONTRACTS"
$predictionPackageBound = $builderText.Contains("PokerPredictionClean_0_6_1")
$jokerPackageBound = $builderText.Contains("JokerHandChoice_0_6_0")
$predictionLabelsDrawn = $pokerPanelText.Contains("UI_POKER_PREDICTION_TITLE") `
  -and -not $pokerPanelText.Contains("new GUIContent(string.Empty, label)")
$largeResultModalRemoved = -not $pokerPanelText.Contains("PokerResultOverlayRenderer.Draw(")
$predictionLayoutUpdated = $pokerLayoutText.Contains("new Rect(139f, 456f, 232f, 64f)") `
  -and $pokerLayoutText.Contains("new Rect(589f, 456f, 232f, 64f)")
$jokerTwoColumnFallbackRemoved = -not $pokerPanelText.Contains("var column = index % 2;")

Write-Output ("predictionPackageBound={0} labelsDrawn={1} layout232x64={2} largeResultModalRemoved={3}" -f `
  $predictionPackageBound,
  $predictionLabelsDrawn,
  $predictionLayoutUpdated,
  $largeResultModalRemoved)
Write-Output ("jokerPackageBound={0} twoColumnFallbackRemoved={1}" -f `
  $jokerPackageBound,
  $jokerTwoColumnFallbackRemoved)

if (-not $predictionPackageBound) {
  Add-GateFailure "issues 67/68/73: prediction package is not bound"
}
if (-not $predictionLabelsDrawn) {
  Add-GateFailure "issues 67/73: prediction button labels are not drawn"
}
if (-not $predictionLayoutUpdated) {
  Add-GateFailure "issue 73: prediction layout is not 232x64"
}
if (-not $largeResultModalRemoved) {
  Add-GateFailure "issue 68: obsolete large result modal remains"
}
if (-not $jokerPackageBound) {
  Add-GateFailure "issue 74: joker hand-choice package is not bound"
}
if (-not $jokerTwoColumnFallbackRemoved) {
  Add-GateFailure "issue 74: obsolete two-column joker fallback remains"
}

if ($failures.Count -gt 0) {
  Write-Output "ART_RUNTIME_RELEASE_GATE_FAILED count=$($failures.Count)"
  foreach ($failure in $failures) {
    Write-Output "BLOCKER $failure"
  }
  exit 1
}

Write-Output "ART_RUNTIME_RELEASE_GATE_OK"
