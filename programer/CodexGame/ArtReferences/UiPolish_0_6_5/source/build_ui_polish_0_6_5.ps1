[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$RepoRoot,
  [string]$ReviewRoot = ''
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot)
$assetsRoot = Join-Path $repoRootFull 'programer\CodexGame\Assets'
$artReferencesRoot = Join-Path $repoRootFull 'programer\CodexGame\ArtReferences'
$reviewRootFull = if ([string]::IsNullOrWhiteSpace($ReviewRoot)) { $null } else { [System.IO.Path]::GetFullPath($ReviewRoot) }
$metaGenerator = Join-Path $artReferencesRoot 'RuntimeBindingTools\generate_stable_unity_meta_0_6_0.ps1'
$templateMeta = Join-Path $assetsRoot 'Art\Prototype\UI\PokerPredictionClean_0_6_1\poker_prediction_player_idle_232x64_0_6_1.png.meta'
$utf8 = [System.Text.UTF8Encoding]::new($false)

if (-not (Test-Path -LiteralPath $metaGenerator)) { throw "Missing meta generator: $metaGenerator" }
if (-not (Test-Path -LiteralPath $templateMeta)) { throw "Missing template meta: $templateMeta" }

$C = @{
  Clear = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
  Ink = [System.Drawing.Color]::FromArgb(255, 8, 7, 6)
  Panel = [System.Drawing.Color]::FromArgb(255, 23, 17, 13)
  PanelHi = [System.Drawing.Color]::FromArgb(255, 48, 29, 18)
  BrassDark = [System.Drawing.Color]::FromArgb(255, 91, 51, 16)
  Brass = [System.Drawing.Color]::FromArgb(255, 180, 108, 27)
  BrassHi = [System.Drawing.Color]::FromArgb(255, 244, 188, 69)
  Cream = [System.Drawing.Color]::FromArgb(255, 239, 222, 181)
  Teal = [System.Drawing.Color]::FromArgb(255, 27, 199, 210)
  TealDark = [System.Drawing.Color]::FromArgb(255, 7, 62, 68)
  TealHover = [System.Drawing.Color]::FromArgb(255, 20, 92, 98)
  Red = [System.Drawing.Color]::FromArgb(255, 222, 57, 63)
  RedDark = [System.Drawing.Color]::FromArgb(255, 78, 22, 24)
  RedHover = [System.Drawing.Color]::FromArgb(255, 111, 31, 34)
  Disabled = [System.Drawing.Color]::FromArgb(255, 82, 79, 72)
  DisabledFill = [System.Drawing.Color]::FromArgb(255, 32, 30, 27)
}

function New-Canvas([int]$width, [int]$height) {
  $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $bitmap.SetResolution(96, 96)
  return $bitmap
}

function Get-Graphics([System.Drawing.Bitmap]$bitmap) {
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
  $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
  $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
  $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
  return $graphics
}

function Draw-Diamond([System.Drawing.Graphics]$graphics, [int]$cx, [int]$cy, [int]$radius, [System.Drawing.Color]$color) {
  $points = @(
    [System.Drawing.Point]::new($cx, $cy - $radius),
    [System.Drawing.Point]::new($cx + $radius, $cy),
    [System.Drawing.Point]::new($cx, $cy + $radius),
    [System.Drawing.Point]::new($cx - $radius, $cy)
  )
  $brush = [System.Drawing.SolidBrush]::new($color)
  try { $graphics.FillPolygon($brush, $points) } finally { $brush.Dispose() }
}

function Draw-Frame(
  [System.Drawing.Graphics]$graphics,
  [System.Drawing.Rectangle]$rect,
  [System.Drawing.Color]$accent,
  [System.Drawing.Color]$fill,
  [bool]$selected = $false
) {
  $outer = [System.Drawing.SolidBrush]::new($C.Ink)
  $border = [System.Drawing.SolidBrush]::new($C.BrassDark)
  $inner = [System.Drawing.SolidBrush]::new($fill)
  $accentPen = [System.Drawing.Pen]::new($accent, 2)
  try {
    $graphics.FillRectangle($outer, $rect)
    $graphics.FillRectangle($border, $rect.X + 2, $rect.Y + 2, $rect.Width - 4, $rect.Height - 4)
    $graphics.FillRectangle($inner, $rect.X + 5, $rect.Y + 5, $rect.Width - 10, $rect.Height - 10)
    $graphics.DrawLine($accentPen, $rect.X + 16, $rect.Y + 7, $rect.Right - 17, $rect.Y + 7)
    $graphics.DrawLine($accentPen, $rect.X + 8, $rect.Y + 16, $rect.X + 8, $rect.Bottom - 17)
    Draw-Diamond $graphics ($rect.X + 8) ($rect.Y + 8) 4 $accent
    Draw-Diamond $graphics ($rect.Right - 9) ($rect.Y + 8) 4 $C.Brass
    Draw-Diamond $graphics ($rect.X + 8) ($rect.Bottom - 9) 4 $C.Brass
    Draw-Diamond $graphics ($rect.Right - 9) ($rect.Bottom - 9) 4 $accent
    if ($selected) {
      $selectedPen = [System.Drawing.Pen]::new($C.BrassHi, 2)
      try { $graphics.DrawRectangle($selectedPen, $rect.X + 12, $rect.Y + 12, $rect.Width - 25, $rect.Height - 25) }
      finally { $selectedPen.Dispose() }
    }
  }
  finally { $accentPen.Dispose(); $inner.Dispose(); $border.Dispose(); $outer.Dispose() }
}

function Save-Png([System.Drawing.Bitmap]$bitmap, [string]$path) {
  $directory = Split-Path -Parent $path
  New-Item -ItemType Directory -Force -Path $directory | Out-Null
  $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
}

function New-FramedAsset(
  [int]$width,
  [int]$height,
  [System.Drawing.Color]$accent,
  [System.Drawing.Color]$fill,
  [bool]$selected = $false,
  [scriptblock]$decoration = $null
) {
  $bitmap = New-Canvas $width $height
  $graphics = Get-Graphics $bitmap
  try {
    $graphics.Clear($C.Clear)
    Draw-Frame $graphics ([System.Drawing.Rectangle]::new(0, 0, $width, $height)) $accent $fill $selected
    if ($null -ne $decoration) { & $decoration $graphics $width $height }
  }
  finally { $graphics.Dispose() }
  return $bitmap
}

function Copy-Versioned([string]$sourcePath, [string]$destinationPath) {
  if (-not (Test-Path -LiteralPath $sourcePath)) { throw "Missing baseline asset: $sourcePath" }
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destinationPath) | Out-Null
  Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
}

function Write-Json([object]$value, [string]$path) {
  [System.IO.File]::WriteAllText($path, ($value | ConvertTo-Json -Depth 12), $utf8)
}

function Write-ApprovalHashes([string]$referenceRoot, [string]$runtimeRoot, [string]$previewRoot) {
  $lines = [System.Collections.Generic.List[string]]::new()
  Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png' | Sort-Object Name | ForEach-Object {
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $relative = [System.IO.Path]::GetRelativePath($referenceRoot, $_.FullName).Replace('\', '/')
    $lines.Add("$hash  $relative")
  }
  Get-ChildItem -LiteralPath $previewRoot -Filter '*.png' | Sort-Object Name | ForEach-Object {
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $relative = [System.IO.Path]::GetRelativePath($referenceRoot, $_.FullName).Replace('\', '/')
    $lines.Add("$hash  $relative")
  }
  [System.IO.File]::WriteAllLines((Join-Path $referenceRoot 'APPROVED.sha256'), $lines, $utf8)
}

function Ensure-ApprovedPreview([string]$sourceName, [string]$destinationPath) {
  if ($null -ne $reviewRootFull) {
    $sourcePath = Join-Path $reviewRootFull $sourceName
    if (Test-Path -LiteralPath $sourcePath) {
      Copy-Versioned $sourcePath $destinationPath
      return
    }
  }
  if (-not (Test-Path -LiteralPath $destinationPath)) {
    throw "Approved preview is missing. Restore the tracked preview or pass -ReviewRoot: $destinationPath"
  }
}

function Normalize-UnityImportSettings([string]$runtimeRoot) {
  foreach ($metaFile in Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png.meta') {
    $text = [System.IO.File]::ReadAllText($metaFile.FullName)
    $text = [regex]::Replace($text, '(?m)^(\s*)enableMipMap:\s*\d+\s*$', '${1}enableMipMap: 0')
    $text = [regex]::Replace($text, '(?m)^(\s*)filterMode:\s*\d+\s*$', '${1}filterMode: 0')
    $text = [regex]::Replace($text, '(?m)^(\s*)alphaIsTransparency:\s*\d+\s*$', '${1}alphaIsTransparency: 1')
    $text = [regex]::Replace($text, '(?m)^(\s*)textureCompression:\s*\d+\s*$', '${1}textureCompression: 0')
    [System.IO.File]::WriteAllText($metaFile.FullName, $text, $utf8)
  }
}

function Assert-UnityImportSettings([string]$runtimeRoot) {
  $expectations = [ordered]@{
    enableMipMap = 0
    filterMode = 0
    alphaIsTransparency = 1
    textureCompression = 0
  }
  foreach ($metaFile in Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png.meta') {
    $text = [System.IO.File]::ReadAllText($metaFile.FullName)
    foreach ($field in $expectations.Keys) {
      $values = [regex]::Matches($text, "(?m)^\s*${field}:\s*(\d+)\s*$")
      if ($values.Count -eq 0) { throw "Missing $field in $($metaFile.FullName)" }
      foreach ($value in $values) {
        if ([int]$value.Groups[1].Value -ne [int]$expectations[$field]) {
          throw "Invalid $field in $($metaFile.FullName): $($value.Groups[1].Value)"
        }
      }
    }
  }
}

function Ensure-FolderMetaFinalNewline([string]$runtimeRoot) {
  $folderMeta = $runtimeRoot + '.meta'
  if (-not (Test-Path -LiteralPath $folderMeta)) {
    throw "Missing Unity folder meta: $folderMeta"
  }
  $bytes = [System.IO.File]::ReadAllBytes($folderMeta)
  if ($bytes.Length -eq 0) {
    throw "Empty Unity folder meta: $folderMeta"
  }
  $lastByte = $bytes[$bytes.Length - 1]
  if ($lastByte -ne 10 -and $lastByte -ne 13) {
    [System.IO.File]::AppendAllText($folderMeta, "`n", $utf8)
  }
}

function Assert-FolderMetaFinalNewline([string]$runtimeRoot) {
  $folderMeta = $runtimeRoot + '.meta'
  if (-not (Test-Path -LiteralPath $folderMeta)) {
    throw "Missing Unity folder meta: $folderMeta"
  }
  $bytes = [System.IO.File]::ReadAllBytes($folderMeta)
  if ($bytes.Length -eq 0 -or ($bytes[$bytes.Length - 1] -ne 10 -and $bytes[$bytes.Length - 1] -ne 13)) {
    throw "Unity folder meta must end with a newline: $folderMeta"
  }
}

function Write-SourceWrapper([string]$sourceRoot, [string]$packageName) {
  New-Item -ItemType Directory -Force -Path $sourceRoot | Out-Null
  $wrapper = @"
[CmdletBinding()]
param([string]`$RepoRoot = ([System.IO.Path]::GetFullPath((Join-Path `$PSScriptRoot '..\..\..\..\..'))))

# Regenerates the approved textless runtime package group.
& (Join-Path `$RepoRoot 'programer\CodexGame\ArtReferences\UiPolish_0_6_5\source\build_ui_polish_0_6_5.ps1') -RepoRoot `$RepoRoot
Write-Output '$packageName regenerated through UiPolish_0_6_5.'
"@
  [System.IO.File]::WriteAllText((Join-Path $sourceRoot ("build_{0}.ps1" -f $packageName.ToLowerInvariant())), $wrapper, $utf8)
}

function Generate-PokerPrediction {
  $version = '0_6_2'
  $runtimeRoot = Join-Path $assetsRoot "Art\Prototype\UI\PokerPredictionClean_$version"
  $referenceRoot = Join-Path $artReferencesRoot "PokerPredictionClean_$version"
  $previewRoot = Join-Path $referenceRoot 'preview'
  $sourceRoot = Join-Path $referenceRoot 'source'
  New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot, $sourceRoot | Out-Null

  $states = @(
    @{ Name = 'idle'; PlayerAccent = $C.Teal; PlayerFill = $C.TealDark; AiAccent = $C.Red; AiFill = $C.RedDark; Selected = $false },
    @{ Name = 'hover'; PlayerAccent = $C.BrassHi; PlayerFill = $C.TealHover; AiAccent = $C.BrassHi; AiFill = $C.RedHover; Selected = $false },
    @{ Name = 'selected'; PlayerAccent = $C.Teal; PlayerFill = $C.TealDark; AiAccent = $C.Red; AiFill = $C.RedDark; Selected = $true },
    @{ Name = 'disabled'; PlayerAccent = $C.Disabled; PlayerFill = $C.DisabledFill; AiAccent = $C.Disabled; AiFill = $C.DisabledFill; Selected = $false }
  )
  foreach ($state in $states) {
    foreach ($side in @('player', 'ai')) {
      $accent = if ($side -eq 'player') { $state.PlayerAccent } else { $state.AiAccent }
      $fill = if ($side -eq 'player') { $state.PlayerFill } else { $state.AiFill }
      $asset = New-FramedAsset 244 66 $accent $fill $state.Selected {
        param($g, $w, $h)
        $linePen = [System.Drawing.Pen]::new($accent, 1)
        try { $g.DrawLine($linePen, 28, 47, 216, 47) } finally { $linePen.Dispose() }
      }
      try { Save-Png $asset (Join-Path $runtimeRoot ("poker_prediction_{0}_{1}_244x66_{2}.png" -f $side, $state.Name, $version)) }
      finally { $asset.Dispose() }
    }
  }

  $title = New-FramedAsset 308 52 $C.BrassHi $C.PanelHi $false {
    param($g, $w, $h)
    Draw-Diamond $g 24 26 6 $C.BrassHi
    Draw-Diamond $g 284 26 4 $C.Brass
  }
  try { Save-Png $title (Join-Path $runtimeRoot "poker_prediction_title_plate_308x52_$version.png") } finally { $title.Dispose() }

  foreach ($entry in @(
    @{ Name = 'player'; Accent = $C.Teal },
    @{ Name = 'ai'; Accent = $C.Red },
    @{ Name = 'neutral'; Accent = $C.Brass }
  )) {
    $panel = New-FramedAsset 328 76 $entry.Accent $C.PanelHi $false {
      param($g, $w, $h)
      $pen = [System.Drawing.Pen]::new($C.BrassDark, 1)
      try { $g.DrawLine($pen, 34, 42, 294, 42) } finally { $pen.Dispose() }
    }
    try { Save-Png $panel (Join-Path $runtimeRoot ("poker_result_summary_{0}_328x76_{1}.png" -f $entry.Name, $version)) }
    finally { $panel.Dispose() }
  }

  foreach ($entry in @(
    @{ Name = 'idle'; Accent = $C.Brass; Fill = $C.PanelHi },
    @{ Name = 'hover'; Accent = $C.BrassHi; Fill = $C.PanelHi },
    @{ Name = 'pressed'; Accent = $C.Teal; Fill = $C.TealDark },
    @{ Name = 'disabled'; Accent = $C.Disabled; Fill = $C.DisabledFill }
  )) {
    $button = New-FramedAsset 164 44 $entry.Accent $entry.Fill $false
    try { Save-Png $button (Join-Path $runtimeRoot ("poker_result_continue_{0}_164x44_{1}.png" -f $entry.Name, $version)) }
    finally { $button.Dispose() }
  }

  $oldRoot = Join-Path $assetsRoot 'Art\Prototype\UI\PokerPredictionClean_0_6_1'
  foreach ($stem in @('poker_insurance_remaining_icon_28', 'poker_prediction_stage_emblem_40', 'poker_prediction_success_icon_28')) {
    Copy-Versioned (Join-Path $oldRoot ($stem + '_0_6_1.png')) (Join-Path $runtimeRoot ($stem + "_$version.png"))
  }

  Ensure-ApprovedPreview 'issue_73_prediction_controls_review_960x540_0_6_2.png' (Join-Path $previewRoot 'poker_prediction_controls_approved_960x540_0_6_2.png')
  Ensure-ApprovedPreview 'issue_68_73_result_hand_label_review_960x540_0_6_2.png' (Join-Path $previewRoot 'poker_result_hand_label_approved_960x540_0_6_2.png')

  & $metaGenerator -ProjectAssetsRoot $assetsRoot -RuntimeDirectory $runtimeRoot -TemplateMeta $templateMeta
  Normalize-UnityImportSettings $runtimeRoot
  Ensure-FolderMetaFinalNewline $runtimeRoot

  $catalog = [ordered]@{
    artSetId = 'poker_prediction_clean_0_6_2'
    issues = @(68, 73)
    artRevision = '0.6.2'
    integrationId = 'ART-RUNTIME-0.6.5-UI-POLISH'
    baseGitCommit = '28b0528fec8ce4b04e2e95619dc1c7c5416018bb'
    status = 'art_produced_user_approved_programmer_binding_required'
    runtimeRoot = 'Assets/Art/Prototype/UI/PokerPredictionClean_0_6_2/'
    sourceRoot = 'ArtReferences/PokerPredictionClean_0_6_2/source/'
    previewRoot = 'ArtReferences/PokerPredictionClean_0_6_2/preview/'
    supersedes = @('Assets/Art/Prototype/UI/PokerPredictionClean_0_6_1/')
    referenceResolution = @(960, 540)
    rules = [ordered]@{ communityCardMaximum = 2; bakedText = $false; largeResultModal = $false; winnerAndHandRows = 2 }
    runtimeAssets = @(
      [ordered]@{ pattern = 'poker_prediction_player_{idle|hover|selected|disabled}_244x66_0_6_2.png'; usage = 'player prediction button' },
      [ordered]@{ pattern = 'poker_prediction_ai_{idle|hover|selected|disabled}_244x66_0_6_2.png'; usage = 'AI prediction button' },
      [ordered]@{ file = 'poker_prediction_title_plate_308x52_0_6_2.png'; usage = 'prediction title' },
      [ordered]@{ pattern = 'poker_result_summary_{player|ai|neutral}_328x76_0_6_2.png'; usage = 'winner and winning-hand two-row plaque' },
      [ordered]@{ pattern = 'poker_result_continue_{idle|hover|pressed|disabled}_164x44_0_6_2.png'; usage = 'result continue action' }
    )
    layout960x540 = [ordered]@{
      titlePlateRect = @(326, 24, 308, 52)
      playerPredictionVisualRect = @(132, 454, 244, 66)
      playerPredictionTextRect = @(154, 466, 200, 40)
      playerPredictionHitRect = @(124, 446, 260, 82)
      aiPredictionVisualRect = @(584, 454, 244, 66)
      aiPredictionTextRect = @(606, 466, 200, 40)
      aiPredictionHitRect = @(576, 446, 260, 82)
      resultSummaryRect = @(316, 18, 328, 76)
      resultWinnerTextRect = @(344, 25, 272, 31)
      resultHandTextRect = @(344, 56, 272, 25)
      continueVisualRect = @(398, 492, 164, 44)
      continueTextRect = @(424, 500, 112, 28)
      communityCardMaximum = 2
    }
    localizedText = [ordered]@{
      predictionTitle = 'UI_POKER_PREDICTION_TITLE'
      playerWins = 'UI_POKER_PLAYER_WINS'
      aiWins = 'UI_POKER_AI_WINS'
      winningHand = 'runtime poker hand label and high-card detail'
      continue = 'UI_COMMON_CONTINUE'
    }
    guardrails = @(
      'Never render more than two community cards.',
      'Do not restore PokerPredictionClean_0_6_1 as an active runtime package after 0.6.2 is bound.',
      'Do not restore the central large grey result modal.',
      'Draw localized labels over text-safe rectangles; textures contain no text.',
      'Do not change prediction, insurance, timeout, HP, reward, or card rules.'
    )
  }
  Write-Json $catalog (Join-Path $referenceRoot 'poker_prediction_clean_art_catalog_0_6_2.json')
  Write-SourceWrapper $sourceRoot 'PokerPredictionClean_0_6_2'
  Write-ApprovalHashes $referenceRoot $runtimeRoot $previewRoot
}

function Generate-PrivateSelection {
  $version = '0_6_0'
  $runtimeRoot = Join-Path $assetsRoot "Art\Prototype\UI\PrivateSelection_$version"
  $referenceRoot = Join-Path $artReferencesRoot "PrivateSelection_$version"
  $previewRoot = Join-Path $referenceRoot 'preview'
  $sourceRoot = Join-Path $referenceRoot 'source'
  New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot, $sourceRoot | Out-Null

  $oldRoot = Join-Path $assetsRoot 'Art\Prototype\UI\PrivateSelection_0_5_5'
  foreach ($stem in @(
    'private_selection_modal_dim_960x540',
    'private_selection_modal_panel_860x456',
    'private_selection_public_frame_166x198',
    'private_selection_candidate_idle_112x150',
    'private_selection_candidate_hover_112x150',
    'private_selection_candidate_selected_112x150',
    'private_selection_candidate_confirmed_112x150',
    'private_selection_candidate_disabled_112x150'
  )) {
    Copy-Versioned (Join-Path $oldRoot ($stem + '_0_5_5.png')) (Join-Path $runtimeRoot ($stem + "_$version.png"))
  }

  $countPanel = New-FramedAsset 184 64 $C.Brass $C.Panel $false
  try { Save-Png $countPanel (Join-Path $runtimeRoot "private_selection_count_panel_184x64_$version.png") }
  finally { $countPanel.Dispose() }

  foreach ($entry in @(
    @{ Name = 'idle'; Accent = $C.Teal; Fill = $C.TealDark },
    @{ Name = 'hover'; Accent = $C.BrassHi; Fill = $C.TealHover },
    @{ Name = 'active'; Accent = $C.Teal; Fill = $C.TealDark },
    @{ Name = 'disabled'; Accent = $C.Disabled; Fill = $C.DisabledFill }
  )) {
    $button = New-FramedAsset 280 60 $entry.Accent $entry.Fill ($entry.Name -eq 'active') {
      param($g, $w, $h)
      $pen = [System.Drawing.Pen]::new($entry.Accent, 1)
      try { $g.DrawLine($pen, 52, 45, 228, 45) } finally { $pen.Dispose() }
    }
    try { Save-Png $button (Join-Path $runtimeRoot ("private_selection_confirm_{0}_280x60_{1}.png" -f $entry.Name, $version)) }
    finally { $button.Dispose() }
  }

  Ensure-ApprovedPreview 'issue_66_single_large_confirm_review_960x540_0_6_0.png' (Join-Path $previewRoot 'private_selection_single_confirm_approved_960x540_0_6_0.png')
  & $metaGenerator -ProjectAssetsRoot $assetsRoot -RuntimeDirectory $runtimeRoot -TemplateMeta $templateMeta
  Normalize-UnityImportSettings $runtimeRoot
  Ensure-FolderMetaFinalNewline $runtimeRoot

  $catalog = [ordered]@{
    artSetId = 'private_selection_0_6_0'
    issues = @(66)
    artRevision = '0.6.0'
    integrationId = 'ART-RUNTIME-0.6.5-UI-POLISH'
    baseGitCommit = '28b0528fec8ce4b04e2e95619dc1c7c5416018bb'
    status = 'art_produced_user_approved_programmer_binding_required'
    runtimeRoot = 'Assets/Art/Prototype/UI/PrivateSelection_0_6_0/'
    supersedes = @('Assets/Art/Prototype/UI/PrivateSelection_0_5_5/')
    referenceResolution = @(960, 540)
    layout = [ordered]@{
      panelRect = @(50, 42, 860, 456)
      selectionCountRect = @(72, 408, 184, 64)
      confirmVisualRect = @(580, 424, 280, 60)
      confirmTextRect = @(620, 438, 200, 32)
      confirmHitRect = @(568, 412, 304, 84)
      confirmButtonCount = 1
    }
    runtimeAssets = @(
      [ordered]@{ file = 'private_selection_count_panel_184x64_0_6_0.png'; usage = 'selection count only; never confirm input' },
      [ordered]@{ pattern = 'private_selection_confirm_{idle|hover|active|disabled}_280x60_0_6_0.png'; usage = 'single confirm button' }
    )
    binding = [ordered]@{ view = 'PrivateSelectionDevPanel'; onlyConfirmEvent = 'PrivateCardsConfirmRequested'; keyboard = 'Enter'; canConfirm = 'snapshot.CanConfirm' }
    guardrails = @(
      'Render exactly one confirm button.',
      'The 184x64 left panel is selection count information and must not receive confirm input.',
      'Do not restore the 180x52 confirm textures after 0.6.0 is bound.',
      'Keep localized words out of art textures.'
    )
  }
  Write-Json $catalog (Join-Path $referenceRoot 'private_selection_art_catalog_0_6_0.json')
  Write-SourceWrapper $sourceRoot 'PrivateSelection_0_6_0'
  Write-ApprovalHashes $referenceRoot $runtimeRoot $previewRoot
}

function Generate-StageReward {
  $version = '0_5_6'
  $runtimeRoot = Join-Path $assetsRoot "Art\Prototype\UI\StageReward_$version"
  $referenceRoot = Join-Path $artReferencesRoot "StageReward_$version"
  $previewRoot = Join-Path $referenceRoot 'preview'
  $sourceRoot = Join-Path $referenceRoot 'source'
  New-Item -ItemType Directory -Force -Path $runtimeRoot, $previewRoot, $sourceRoot | Out-Null

  $summary = New-Canvas 680 360
  $g = Get-Graphics $summary
  try {
    $g.Clear($C.Clear)
    Draw-Frame $g ([System.Drawing.Rectangle]::new(0, 0, 680, 360)) $C.BrassHi $C.Panel $false
    Draw-Frame $g ([System.Drawing.Rectangle]::new(20, 18, 640, 58)) $C.Brass $C.PanelHi $false
    Draw-Frame $g ([System.Drawing.Rectangle]::new(20, 86, 640, 162)) $C.Brass $C.Panel $false
    Draw-Frame $g ([System.Drawing.Rectangle]::new(220, 288, 240, 52)) $C.BrassHi $C.PanelHi $false
    Draw-Diamond $g 340 30 6 $C.BrassHi
    Draw-Diamond $g 340 30 3 $C.PanelHi
  }
  finally { $g.Dispose() }
  try { Save-Png $summary (Join-Path $runtimeRoot "stage_reward_summary_panel_680x360_$version.png") }
  finally { $summary.Dispose() }

  $content = New-Canvas 632 154
  $g = Get-Graphics $content
  try { $g.Clear($C.Panel) } finally { $g.Dispose() }
  try { Save-Png $content (Join-Path $runtimeRoot "stage_reward_content_opaque_632x154_$version.png") }
  finally { $content.Dispose() }

  foreach ($entry in @(
    @{ Name = 'base'; Accent = $C.BrassHi; Fill = $C.PanelHi },
    @{ Name = 'prediction'; Accent = $C.Teal; Fill = $C.TealDark },
    @{ Name = 'neutral'; Accent = $C.Brass; Fill = $C.Panel }
  )) {
    $row = New-FramedAsset 304 64 $entry.Accent $entry.Fill $false {
      param($g, $w, $h)
      $divider = [System.Drawing.Pen]::new($entry.Accent, 1)
      try { $g.DrawLine($divider, 204, 13, 204, 51) } finally { $divider.Dispose() }
    }
    try { Save-Png $row (Join-Path $runtimeRoot ("stage_reward_row_{0}_304x64_{1}.png" -f $entry.Name, $version)) }
    finally { $row.Dispose() }
  }

  $total = New-FramedAsset 324 48 $C.Brass $C.Panel $false
  try { Save-Png $total (Join-Path $runtimeRoot "stage_reward_total_row_324x48_$version.png") }
  finally { $total.Dispose() }

  foreach ($entry in @(
    @{ Name = 'idle'; Accent = $C.Brass; Fill = $C.PanelHi },
    @{ Name = 'hover'; Accent = $C.BrassHi; Fill = $C.PanelHi },
    @{ Name = 'pressed'; Accent = $C.Teal; Fill = $C.TealDark },
    @{ Name = 'disabled'; Accent = $C.Disabled; Fill = $C.DisabledFill }
  )) {
    $button = New-FramedAsset 240 52 $entry.Accent $entry.Fill $false
    try { Save-Png $button (Join-Path $runtimeRoot ("stage_reward_continue_{0}_240x52_{1}.png" -f $entry.Name, $version)) }
    finally { $button.Dispose() }
  }

  Ensure-ApprovedPreview 'issue_49_reward_readability_review_960x540_0_5_6.png' (Join-Path $previewRoot 'stage_reward_readability_approved_960x540_0_5_6.png')
  & $metaGenerator -ProjectAssetsRoot $assetsRoot -RuntimeDirectory $runtimeRoot -TemplateMeta $templateMeta
  Normalize-UnityImportSettings $runtimeRoot
  Ensure-FolderMetaFinalNewline $runtimeRoot

  $catalog = [ordered]@{
    package = 'StageReward_0_5_6'
    revision = '0.5.6'
    issues = @(49)
    integrationId = 'ART-RUNTIME-0.6.5-UI-POLISH'
    baseGitCommit = '28b0528fec8ce4b04e2e95619dc1c7c5416018bb'
    status = 'art_produced_user_approved_programmer_binding_required'
    runtimeRoot = 'Assets/Art/Prototype/UI/StageReward_0_5_6/'
    supersedes = @('Assets/Art/Prototype/UI/StageReward_0_5_5/')
    referenceResolution = @(960, 540)
    layout = [ordered]@{
      popupRect = @(140, 90, 680, 360)
      titleSafeRectLocal = @(24, 20, 632, 52)
      contentSafeRectLocal = @(24, 88, 632, 154)
      rewardGrid = [ordered]@{ columns = 2; rows = 2; gapX = 24; gapY = 12 }
      rowSize = @(304, 64)
      rowLabelSafeRectLocal = @(48, 14, 148, 36)
      rowValueSafeRectLocal = @(208, 14, 76, 36)
      totalRowRect = @(178, 170, 324, 48)
      continueButtonRect = @(220, 288, 240, 52)
      overflowRule = 'Hard clip to content safe rectangle; page or scroll after four rows.'
    }
    runtimeAssets = @(
      [ordered]@{ pattern = 'stage_reward_row_{base|prediction|neutral}_304x64_0_5_6.png'; usage = 'high contrast reward rows' },
      [ordered]@{ file = 'stage_reward_total_row_324x48_0_5_6.png'; usage = 'total gained summary' },
      [ordered]@{ pattern = 'stage_reward_continue_{idle|hover|pressed|disabled}_240x52_0_5_6.png'; usage = 'contained continue action' }
    )
    localizedText = [ordered]@{ title = 'reward settlement'; base = 'base reward'; prediction = 'prediction bonus'; total = 'total gained'; continue = 'UI_COMMON_CONTINUE' }
    guardrails = @(
      'Do not bake localized words into PNG files.',
      'Do not change currency calculation, reward values, or save order.',
      'Never draw reward rows outside the 632x154 content safe area.',
      'Do not restore StageReward_0_5_5 as active after 0.5.6 is bound.'
    )
  }
  Write-Json $catalog (Join-Path $referenceRoot 'stage_reward_art_catalog_0_5_6.json')
  Write-SourceWrapper $sourceRoot 'StageReward_0_5_6'
  Write-ApprovalHashes $referenceRoot $runtimeRoot $previewRoot
}

Generate-PokerPrediction
Generate-PrivateSelection
Generate-StageReward

$umbrellaSource = Join-Path $artReferencesRoot 'UiPolish_0_6_5\source'
New-Item -ItemType Directory -Force -Path $umbrellaSource | Out-Null
$destinationScript = Join-Path $umbrellaSource 'build_ui_polish_0_6_5.ps1'
if ([System.IO.Path]::GetFullPath($PSCommandPath) -ne [System.IO.Path]::GetFullPath($destinationScript)) {
  Copy-Item -LiteralPath $PSCommandPath -Destination $destinationScript -Force
}

foreach ($package in @('PokerPredictionClean_0_6_2', 'PrivateSelection_0_6_0', 'StageReward_0_5_6')) {
  $runtimeRoot = Join-Path $assetsRoot ("Art\Prototype\UI\$package")
  $referenceRoot = Join-Path $artReferencesRoot $package
  $pngCount = @(Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png').Count
  $metaCount = @(Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png.meta').Count
  $hashLines = @(Get-Content -LiteralPath (Join-Path $referenceRoot 'APPROVED.sha256')).Count
  if ($pngCount -ne $metaCount) { throw "$package png/meta mismatch: $pngCount/$metaCount" }
  Assert-UnityImportSettings $runtimeRoot
  Assert-FolderMetaFinalNewline $runtimeRoot
  Write-Output ("{0}: png={1} meta={2} approvedHashes={3}" -f $package, $pngCount, $metaCount, $hashLines)
}
