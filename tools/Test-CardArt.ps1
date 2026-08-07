[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\sk-encoa\codex_game',
    [string]$SourceCardsPath = 'G:\내 드라이브\codex_game\art_source\02_working\Cards'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$unityCardsPath = Join-Path $RepositoryPath 'programer\CodexGame\Assets\Art\Prototype\Cards'
$catalogPath = Join-Path $unityCardsPath 'card_art_catalog.json'
$variantPath = Join-Path $unityCardsPath 'deck_variants'
$componentPath = Join-Path $unityCardsPath 'components'

if (-not (Test-Path -LiteralPath $catalogPath -PathType Leaf)) {
    throw "Card art catalog not found: $catalogPath"
}

$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($catalog.specRevision -ne 'gameplay_flow_0.05') {
    throw "Unexpected spec revision: $($catalog.specRevision)"
}
if ($catalog.cardWidth -ne 64 -or $catalog.cardHeight -ne 90) {
    throw "Unexpected card dimensions in catalog: $($catalog.cardWidth)x$($catalog.cardHeight)"
}

$cards = @($catalog.cards)
if ($cards.Count -ne 156) {
    throw "Expected 156 suit/rank/skull variants, found $($cards.Count)."
}

$duplicateIds = $cards | Group-Object assetId | Where-Object Count -gt 1
if ($duplicateIds) {
    throw "Duplicate card asset IDs: $($duplicateIds.Name -join ', ')"
}

$missing = @()
$invalidDimensions = @()
foreach ($card in $cards) {
    $absolutePath = Join-Path $RepositoryPath ('programer\CodexGame\' + $card.assetPath.Replace('/', '\'))
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        $missing += $card.assetId
    }
    else {
        $image = [System.Drawing.Image]::FromFile($absolutePath)
        try {
            if ($image.Width -ne $catalog.cardWidth -or $image.Height -ne $catalog.cardHeight) {
                $invalidDimensions += "$($card.assetId)=$($image.Width)x$($image.Height)"
            }
        }
        finally {
            $image.Dispose()
        }
    }
    if ($card.skullCount -lt 1 -or $card.skullCount -gt 3) {
        throw "Invalid skullCount for $($card.assetId): $($card.skullCount)"
    }
}
if ($missing.Count -gt 0) {
    throw "Missing card PNG files: $($missing -join ', ')"
}
if ($invalidDimensions.Count -gt 0) {
    throw "Card PNG dimensions do not match $($catalog.cardWidth)x$($catalog.cardHeight): $($invalidDimensions -join ', ')"
}

$pngCount = @(Get-ChildItem -LiteralPath $variantPath -Filter 'card_*.png' -File).Count
if ($pngCount -ne 156) {
    throw "Expected 156 Unity card PNG files, found $pngCount."
}

$sourceCount = @(Get-ChildItem -LiteralPath (Join-Path $SourceCardsPath 'deck_variants') -Filter 'card_*.aseprite' -File).Count
if ($sourceCount -ne 156) {
    throw "Expected 156 Aseprite card sources, found $sourceCount."
}

$requiredComponents = @(
    'card_front_base.png', 'card_back.png',
    'suit_spades.png', 'suit_diamonds.png', 'suit_hearts.png', 'suit_clubs.png',
    'rank_a.png', 'rank_k.png', 'rank_q.png', 'rank_j.png', 'rank_10.png',
    'rank_9.png', 'rank_8.png', 'rank_7.png', 'rank_6.png', 'rank_5.png',
    'rank_4.png', 'rank_3.png', 'rank_2.png',
    'skull_01.png', 'skull_02.png', 'skull_03.png'
)
foreach ($component in $requiredComponents) {
    $componentFile = Join-Path $componentPath $component
    if (-not (Test-Path -LiteralPath $componentFile -PathType Leaf)) {
        throw "Missing card component: $componentFile"
    }
}

$testArtifacts = @(Get-ChildItem -LiteralPath $unityCardsPath, $SourceCardsPath -File -Recurse | Where-Object Name -Match 'test|probe')
if ($testArtifacts.Count -gt 0) {
    throw "Test artifacts remain in card paths: $($testArtifacts.FullName -join ', ')"
}

Write-Output 'Card art validation passed.'
Write-Output "SpecRevision=$($catalog.specRevision)"
Write-Output "Variants=$($cards.Count)"
Write-Output "AsepriteSources=$sourceCount"
Write-Output "UnityPNGs=$pngCount"
Write-Output "Components=$($requiredComponents.Count)"
