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
$latestContractsPath = Join-Path $PSScriptRoot "latest_approved_art_contracts.json"
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $CodexGameRoot "..\.."))
$builderPath = Join-Path $assetsRoot "Editor\PlayableDevSceneBuilder.cs"
$scenePath = Join-Path $assetsRoot "Scenes\PlayableDev.unity"
$pokerPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerDevPanel.cs"
$pokerLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerTableLayout.cs"
$pokerResultPanelLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerResultPanelLayout.cs"
$pokerActionStatePath = Join-Path $assetsRoot "Scripts\Presentation\Views\PokerResultOverlayState.cs"
$playableDevViewPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PlayableDevView.cs"
$halliLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliPileOverlapLayout.cs"
$halliPanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\HalliDevPanel.cs"
$privatePanelPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PrivateSelectionDevPanel.cs"
$privateLayoutPath = Join-Path $assetsRoot "Scripts\Presentation\Views\PrivateSelectionPanelLayout.cs"
$privateArtPath = Join-Path $assetsRoot "Scripts\Presentation\Art\PrivateSelectionUiArtSet.cs"
$economyRendererPath = Join-Path $assetsRoot "Scripts\Presentation\Views\EconomyUiRenderer.cs"
$economyArtPath = Join-Path $assetsRoot "Scripts\Presentation\Art\EconomyUiArtSet.cs"
$pokerArtPath = Join-Path $assetsRoot "Scripts\Presentation\Art\PokerUiArtSet.cs"

foreach ($required in @(
  $manifestPath,
  $latestContractsPath,
  $builderPath,
  $scenePath,
  $pokerPanelPath,
  $pokerLayoutPath,
  $pokerResultPanelLayoutPath,
  $pokerActionStatePath,
  $playableDevViewPath,
  $halliLayoutPath,
  $halliPanelPath,
  $privatePanelPath,
  $privateLayoutPath,
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
$latestContracts = Get-Content -LiteralPath $latestContractsPath -Raw | ConvertFrom-Json
$builderText = Get-Content -LiteralPath $builderPath -Raw
$sceneText = Get-Content -LiteralPath $scenePath -Raw
$pokerPanelText = Get-Content -LiteralPath $pokerPanelPath -Raw
$pokerLayoutText = Get-Content -LiteralPath $pokerLayoutPath -Raw
$pokerResultPanelLayoutText = Get-Content -LiteralPath $pokerResultPanelLayoutPath -Raw
$pokerActionStateText = Get-Content -LiteralPath $pokerActionStatePath -Raw
$playableDevViewText = Get-Content -LiteralPath $playableDevViewPath -Raw
$halliLayoutText = Get-Content -LiteralPath $halliLayoutPath -Raw
$halliPanelText = Get-Content -LiteralPath $halliPanelPath -Raw
$privatePanelText = Get-Content -LiteralPath $privatePanelPath -Raw
$privateLayoutText = Get-Content -LiteralPath $privateLayoutPath -Raw
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

Write-Output "LATEST_APPROVED_ART_CONTRACTS"
if ($latestContracts.schemaVersion -ne 1 -or $latestContracts.policy -ne "latest-human-approval-wins") {
  Add-GateFailure "latest art contract registry has an unsupported schema or policy"
}

$registeredIssues = [System.Collections.Generic.HashSet[int]]::new()
foreach ($contract in $latestContracts.contracts) {
  $issue = [int]$contract.issue
  if (-not $registeredIssues.Add($issue)) {
    Add-GateFailure "latest art contract registry contains more than one active contract for issue $issue"
    continue
  }

  $approvalTime = [System.DateTimeOffset]::MinValue
  $approvalTimeValid = [System.DateTimeOffset]::TryParse(
    [string]$contract.approvedAtUtc,
    [ref]$approvalTime)
  $allowedApprovalKinds = @(
    "human-issue-and-image",
    "human-issue-comment-and-image"
  )
  $approvalSourceValid = $allowedApprovalKinds.Contains([string]$contract.approvalKind) `
    -and ([string]$contract.approvalSourceUrl).StartsWith("https://github.com/rupria/codex_game/issues/$issue")
  $activeDocumentPath = Join-Path $repoRoot ([string]$contract.activeDocument)
  if (-not $approvalTimeValid -or -not $approvalSourceValid -or -not (Test-Path -LiteralPath $activeDocumentPath)) {
    Add-GateFailure "issue ${issue}: latest approval metadata or active contract document is invalid"
    continue
  }

  foreach ($runtimeCheck in $contract.runtimeChecks) {
    $runtimePath = Join-Path $repoRoot ([string]$runtimeCheck.path)
    if (-not (Test-Path -LiteralPath $runtimePath)) {
      Add-GateFailure "issue ${issue}: registered runtime path is missing: $($runtimeCheck.path)"
      continue
    }

    $runtimeText = Get-Content -LiteralPath $runtimePath -Raw
    foreach ($requiredToken in $runtimeCheck.requiredTokens) {
      if (-not $runtimeText.Contains([string]$requiredToken)) {
        Add-GateFailure "issue ${issue}: latest required token is missing from $($runtimeCheck.path): $requiredToken"
      }
    }
    foreach ($forbiddenToken in $runtimeCheck.forbiddenTokens) {
      if ($runtimeText.Contains([string]$forbiddenToken)) {
        Add-GateFailure "issue ${issue}: superseded token returned in $($runtimeCheck.path): $forbiddenToken"
      }
    }
  }

  Write-Output ("issue={0} approvedAtUtc={1} source={2}" -f `
    $issue,
    $contract.approvedAtUtc,
    $contract.approvalSourceUrl)
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
  "PrivateSelection_0_6_1",
  "PrivateSelection_0_6_3",
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
    $folderMetaText = Get-Content -LiteralPath $folderMeta -Raw
    $validFolderMeta = $folderMetaText -match '(?m)^fileFormatVersion: 2$' `
      -and $folderMetaText -match '(?m)^guid: [0-9a-f]{32}$' `
      -and $folderMetaText -match '(?m)^folderAsset: yes$' `
      -and $folderMetaText -match '(?m)^DefaultImporter:$'
  }
  Write-Output ("package={0} approvedHashFailures={1} unityFolderMetaValid={2}" -f `
    $package,
    $hashFailures,
    $validFolderMeta)
  if (-not $validFolderMeta) {
    Add-GateFailure "${package}: Unity folder .meta is missing or structurally malformed"
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
$legacyResultBackdropRemoved = $playableDevViewText -match '(?s)if \(_snapshot\.Phase == PlayableGamePhase\.PokerPrediction\)\s*\{\s*_presentation0124Panel\.DrawShowdownFrame\(\s*false,\s*_presentationUiArtSet\);' `
  -and $playableDevViewText -notmatch '(?s)DrawShowdownFrame\(\s*_snapshot\.Phase == PlayableGamePhase\.PokerResult'
$predictionLayoutUpdated = $pokerLayoutText.Contains("new Rect(132f, 454f, 244f, 66f)") `
  -and $pokerLayoutText.Contains("new Rect(584f, 454f, 244f, 66f)")
$resultSummaryBound = $pokerArtText.Contains("ResultSummaryPlayer") `
  -and $pokerPanelText.Contains("DrawResultSummary(") `
  -and $pokerLayoutText.Contains("new Rect(316f, 18f, 328f, 76f)")
$issue77ResultActionsAligned = $pokerResultPanelLayoutText.Contains("ContinueVisualX = 398f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueVisualY = 465f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueVisualWidth = 164f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueVisualHeight = 44f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueHitX = 390f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueHitY = 455f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueHitWidth = 180f;") `
  -and $pokerResultPanelLayoutText.Contains("ContinueHitHeight = 64f;") `
  -and $pokerLayoutText.Contains("PredictionSuccessPlate = new Rect(716f, 366f, 224f, 44f)") `
  -and $pokerPanelText.Contains("PokerTableLayout.PredictionSuccessPlate") `
  -and $pokerPanelText.Contains("actionVisibility.ShowPredictionActions") `
  -and $pokerPanelText.Contains("actionVisibility.ShowContinueAction") `
  -and $pokerActionStateText.Contains("phase == PokerRoundPhase.Resolved") `
  -and $pokerPanelText.Contains("fontSize = 20") `
  -and $pokerPanelText.Contains("fontSize = 15")
$jokerTwoColumnFallbackRemoved = -not $pokerPanelText.Contains("var column = index % 2;")

Write-Output ("predictionPackageBound={0} labelsDrawn={1} layout244x66={2} resultSummaryBound={3} largeResultModalRemoved={4} legacyResultBackdropRemoved={5}" -f `
  $predictionPackageBound,
  $predictionLabelsDrawn,
  $predictionLayoutUpdated,
  $resultSummaryBound,
  $largeResultModalRemoved,
  $legacyResultBackdropRemoved)
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
if (-not $legacyResultBackdropRemoved) {
  Add-GateFailure "issues 68/73: legacy Presentation_0_1_2_4 result backdrop remains connected"
}
if (-not $issue77ResultActionsAligned) {
  Add-GateFailure "issue 77: poker result actions and prediction-success metric are not aligned"
}
if (-not $jokerPackageBound) {
  Add-GateFailure "issue 74: joker hand-choice package is not bound"
}
if (-not $jokerTwoColumnFallbackRemoved) {
  Add-GateFailure "issue 74: obsolete two-column joker fallback remains"
}

Write-Output "UI_POLISH_ACCEPTANCE_CONTRACTS"
$halliLockedPublicSlotRemoved = -not $halliPanelText.Contains("DrawLockedPublicSlot") `
  -and -not $halliPanelText.Contains("HalliBoardLayout.LockedPublicCard")
$privatePackageBound = $builderText.Contains("PrivateSelection_0_6_0") `
  -and $builderText.Contains("PrivateSelection_0_6_1") `
  -and $builderText.Contains("PrivateSelection_0_6_3")
$singleConfirmHit = $privateLayoutText.Contains("ConfirmHitX = 64f;") `
  -and $privateLayoutText.Contains("ConfirmHitY = 358f;") `
  -and $privateLayoutText.Contains("ConfirmHitWidth = 200f;") `
  -and $privateLayoutText.Contains("ConfirmHitHeight = 136f;") `
  -and $privateLayoutText.Contains("ConfirmVisualX = 72f;") `
  -and $privateLayoutText.Contains("ConfirmVisualY = 366f;") `
  -and $privateLayoutText.Contains("ConfirmVisualWidth = 184f;") `
  -and $privateLayoutText.Contains("ConfirmVisualHeight = 120f;") `
  -and ([regex]::Matches($privatePanelText, "GUI\.Button\(ConfirmHitRect")).Count -eq 1
$confirmProgressIntegrated = $privatePanelText.Contains("ConfirmProgressRect") `
  -and $privatePanelText.Contains('"UI_PRIVATE_CONFIRM_ACTION"') `
  -and $privatePanelText.Contains('"UI_PRIVATE_CONFIRM_PROGRESS"') `
  -and -not $privatePanelText.Contains("SelectionCountRect = new Rect(") `
  -and -not $privatePanelText.Contains("DrawSelectionCount(") `
  -and -not $privatePanelText.Contains("art?.SelectionCountPanel") `
  -and -not $privateArtText.Contains("SelectionCountPanel")
$candidateGridBound = $privateLayoutText.Contains("CandidateColumns = 4;") `
  -and $privateLayoutText.Contains("MaximumCandidateCount = 5;") `
  -and $privateLayoutText.Contains("CandidateTopY = 140f;") `
  -and $privateLayoutText.Contains("CandidateBottomY = 286f;") `
  -and $privateLayoutText.Contains("return 326f + column * (CandidateWidth + CandidateGapX);") `
  -and $privatePanelText.Contains("PrivateSelectionPanelLayout.CandidateX(index)") `
  -and -not $privateLayoutText.Contains("MaximumCandidateCount = 8;")
$stageRewardPackageBound = $builderText.Contains("StageReward_0_5_6")
$stageRewardStatesBound = $economyArtText.Contains("StageRewardBaseRow") `
  -and $economyArtText.Contains("StageRewardPredictionRow") `
  -and $economyArtText.Contains("StageRewardTotalRow") `
  -and $economyRendererText.Contains("DrawStageRewardContinue(")
$communityMaximumTwo = $pokerPanelText.Contains("DrawFaceCards(snapshot.PublicCards, PokerTableLayout.CommunityCard, 2, cards);")

Write-Output ("halliLockedPublicSlotRemoved={0}" -f $halliLockedPublicSlotRemoved)
Write-Output ("privatePackageBound={0} singleConfirmHit={1} confirmProgressIntegrated={2} candidateGridBound={3}" -f `
  $privatePackageBound,
  $singleConfirmHit,
  $confirmProgressIntegrated,
  $candidateGridBound)
Write-Output ("stageRewardPackageBound={0} rewardStatesBound={1} communityMaximumTwo={2}" -f `
  $stageRewardPackageBound,
  $stageRewardStatesBound,
  $communityMaximumTwo)

if (-not $halliLockedPublicSlotRemoved) {
  Add-GateFailure "Halli presentation: obsolete second-community-card lock slot returned"
}
if (-not $privatePackageBound -or -not $singleConfirmHit -or -not $confirmProgressIntegrated -or -not $candidateGridBound) {
  Add-GateFailure "issue 66: private-selection must keep one tall lower-left confirm, no separate count panel, and the maximum-five four-plus-one candidate layout"
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
