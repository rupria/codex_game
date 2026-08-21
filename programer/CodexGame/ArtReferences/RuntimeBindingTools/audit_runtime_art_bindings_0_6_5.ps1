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
$manifestPath = Join-Path $PSScriptRoot "current_art_runtime_manifest_0_6_5.json"
$builderPath = Join-Path $assetsRoot "Editor\PlayableDevSceneBuilder.cs"
$scenePath = Join-Path $assetsRoot "Scenes\PlayableDev.unity"
$pokerPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerDevPanel.cs"
$pokerLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerTableLayout.cs"
$halliLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliPileOverlapLayout.cs"
$halliPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliDevPanel.cs"
$privatePanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PrivateSelectionDevPanel.cs"
$privateArtPath = Join-Path $assetsRoot "Scripts\Presentation\Art\PrivateSelectionUiArtSet.cs"
$economyRendererPath = Join-Path $assetsRoot "Scripts\Presentation\Views\EconomyUiRenderer.cs"
$economyArtPath = Join-Path $assetsRoot "Scripts\Presentation\Art\EconomyUiArtSet.cs"
$pokerArtPath = Join-Path $assetsRoot "Scripts\Presentation\Art\PokerUiArtSet.cs"

foreach ($required in @(
  $manifestPath,
  $builderPath,
  $scenePath,
  $pokerPanelPath,
  $pokerLayoutPath,
  $halliLayoutPath,
  $halliPanelPath,
  $privatePanelPath,
  $privateArtPath,
  $economyRendererPath,
  $economyArtPath,
  $pokerArtPath
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
$privatePanelText = Get-Content -LiteralPath $privatePanelPath -Raw
$privateArtText = Get-Content -LiteralPath $privateArtPath -Raw
$economyRendererText = Get-Content -LiteralPath $economyRendererPath -Raw
$economyArtText = Get-Content -LiteralPath $economyArtPath -Raw
$pokerArtText = Get-Content -LiteralPath $pokerArtPath -Raw
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

if ($manifest.manifestRevision -ne "0.6.5") {
  Add-GateFailure "manifest revision is not 0.6.5"
}

Write-Output "ART_PACKAGE_INTEGRITY"
foreach ($package in @(
  "PokerPredictionClean_0_6_2",
  "PrivateSelection_0_6_0",
  "StageReward_0_5_6"
)) {
  $referenceRoot = Join-Path $CodexGameRoot ("ArtReferences\" + $package)
  $approvedPath = Join-Path $referenceRoot "APPROVED.sha256"
  $folderMeta = Join-Path $assetsRoot ("Art\Prototype\UI\" + $package + ".meta")
  if (-not (Test-Path -LiteralPath $approvedPath)) {
    Add-GateFailure "${package}: APPROVED.sha256 is missing"
    continue
  }

  $hashFailures = 0
  foreach ($line in Get-Content -LiteralPath $approvedPath) {
    if ($line -notmatch '^([0-9a-fA-F]{64})\s+(.+)$') {
      Add-GateFailure "${package}: malformed APPROVED.sha256 line"
      $hashFailures++
      continue
    }
    $expected = $Matches[1].ToLowerInvariant()
    $target = [System.IO.Path]::GetFullPath((Join-Path $referenceRoot $Matches[2]))
    if (-not (Test-Path -LiteralPath $target)) {
      Add-GateFailure "${package}: approved file is missing: $target"
      $hashFailures++
      continue
    }
    $actual = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $expected) {
      Add-GateFailure "${package}: approved hash mismatch: $target"
      $hashFailures++
    }
  }

  $validFolderMeta = Test-Path -LiteralPath $folderMeta
  if ($validFolderMeta) {
    $bytes = [System.IO.File]::ReadAllBytes($folderMeta)
    $validFolderMeta = $bytes.Length -gt 0 -and ($bytes[$bytes.Length - 1] -eq 10 -or $bytes[$bytes.Length - 1] -eq 13)
  }
  Write-Output ("package={0} approvedHashFailures={1} unityFolderMetaValid={2}" -f `
    $package,
    $hashFailures,
    $validFolderMeta)
  if (-not $validFolderMeta) {
    Add-GateFailure "${package}: Unity folder .meta is malformed or missing its final newline"
  }
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
$predictionPackageBound = $builderText.Contains("PokerPredictionClean_0_6_2")
$jokerPackageBound = $builderText.Contains("JokerHandChoice_0_6_0")
$predictionLabelsDrawn = $pokerPanelText.Contains("UI_POKER_PREDICTION_TITLE") `
  -and -not $pokerPanelText.Contains("new GUIContent(string.Empty, label)")
$largeResultModalRemoved = -not $pokerPanelText.Contains("PokerResultOverlayRenderer.Draw(")
$predictionLayoutUpdated = $pokerLayoutText.Contains("new Rect(132f, 454f, 244f, 66f)") `
  -and $pokerLayoutText.Contains("new Rect(584f, 454f, 244f, 66f)")
$resultSummaryBound = $pokerArtText.Contains("ResultSummaryPlayer") `
  -and $pokerPanelText.Contains("DrawResultSummary(") `
  -and $pokerLayoutText.Contains("new Rect(316f, 18f, 328f, 76f)")
$jokerTwoColumnFallbackRemoved = -not $pokerPanelText.Contains("var column = index % 2;")

Write-Output ("predictionPackageBound={0} labelsDrawn={1} layout244x66={2} resultSummaryBound={3} largeResultModalRemoved={4}" -f `
  $predictionPackageBound,
  $predictionLabelsDrawn,
  $predictionLayoutUpdated,
  $resultSummaryBound,
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
  Add-GateFailure "issue 73: prediction layout is not 244x66"
}
if (-not $resultSummaryBound) {
  Add-GateFailure "issue 68: winner and winning-hand result summary is not bound"
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

Write-Output "UI_POLISH_ACCEPTANCE_CONTRACTS"
$privatePackageBound = $builderText.Contains("PrivateSelection_0_6_0")
$singleConfirmHit = $privatePanelText.Contains("ConfirmHitRect = new Rect(568f, 412f, 304f, 84f)") `
  -and $privatePanelText.Contains("ConfirmVisualRect = new Rect(580f, 424f, 280f, 60f)") `
  -and ([regex]::Matches($privatePanelText, "GUI\.Button\(ConfirmHitRect")).Count -eq 1 `
  -and -not $privatePanelText.Contains("ConfirmRect = new Rect(73f, 418f, 180f, 52f)")
$selectionCountBound = $privateArtText.Contains("SelectionCountPanel") `
  -and $privatePanelText.Contains("SelectionCountRect = new Rect(72f, 408f, 184f, 64f)")
$stageRewardPackageBound = $builderText.Contains("StageReward_0_5_6")
$stageRewardStatesBound = $economyArtText.Contains("StageRewardBaseRow") `
  -and $economyArtText.Contains("StageRewardPredictionRow") `
  -and $economyArtText.Contains("StageRewardTotalRow") `
  -and $economyRendererText.Contains("DrawStageRewardContinue(")
$communityMaximumTwo = $pokerPanelText.Contains("DrawFaceCards(snapshot.PublicCards, PokerTableLayout.CommunityCard, 2, cards);")

Write-Output ("privatePackageBound={0} singleConfirmHit={1} selectionCountBound={2}" -f `
  $privatePackageBound,
  $singleConfirmHit,
  $selectionCountBound)
Write-Output ("stageRewardPackageBound={0} rewardStatesBound={1} communityMaximumTwo={2}" -f `
  $stageRewardPackageBound,
  $stageRewardStatesBound,
  $communityMaximumTwo)

if (-not $privatePackageBound -or -not $singleConfirmHit -or -not $selectionCountBound) {
  Add-GateFailure "issue 66: private-selection 0.6.0 must have one confirm hit and a non-interactive count panel"
}
if (-not $stageRewardPackageBound -or -not $stageRewardStatesBound) {
  Add-GateFailure "issue 49: stage-reward 0.5.6 rows, total and continue states are not fully bound"
}
if (-not $communityMaximumTwo) {
  Add-GateFailure "issues 54/68: community-card rendering no longer enforces maximum two"
}

if ($failures.Count -gt 0) {
  Write-Output "ART_RUNTIME_RELEASE_GATE_FAILED count=$($failures.Count)"
  foreach ($failure in $failures) {
    Write-Output "BLOCKER $failure"
  }
  exit 1
}

Write-Output "ART_RUNTIME_RELEASE_GATE_OK"
