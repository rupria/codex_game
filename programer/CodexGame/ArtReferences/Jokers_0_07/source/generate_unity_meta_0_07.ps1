param(
    [Parameter(Mandatory = $true)]
    [string]$RepoRoot
)

$cardsRelative = 'programer/CodexGame/Assets/Art/Prototype/Cards_0_07'
$variantsRelative = $cardsRelative + '/joker_variants'
$cardsDir = Join-Path $RepoRoot $cardsRelative
$variantsDir = Join-Path $RepoRoot $variantsRelative
$templatePath = Join-Path $RepoRoot 'programer/CodexGame/Assets/Art/Prototype/Cards_0_06/poker_variants/card_poker_clubs_j.png.meta'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Get-DeterministicGuid([string]$value) {
    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($value.Replace('\', '/').ToLowerInvariant())
        return ([System.BitConverter]::ToString($md5.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally { $md5.Dispose() }
}

function Write-FolderMeta([string]$path, [string]$relative) {
    $guid = Get-DeterministicGuid $relative
    $content = @"
fileFormatVersion: 2
guid: $guid
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"@
    [System.IO.File]::WriteAllText($path + '.meta', $content.Replace("`r`n", "`n"), $utf8NoBom)
}

Write-FolderMeta $cardsDir $cardsRelative
Write-FolderMeta $variantsDir $variantsRelative

$template = [System.IO.File]::ReadAllText($templatePath)
$template = [System.Text.RegularExpressions.Regex]::Replace($template, '(?m)[ \t]+$', '')
Get-ChildItem -LiteralPath $variantsDir -Filter '*.png' | Sort-Object Name | ForEach-Object {
    $relative = $variantsRelative + '/' + $_.Name
    $guid = Get-DeterministicGuid $relative
    $meta = [System.Text.RegularExpressions.Regex]::Replace($template, '(?m)^guid: [0-9a-f]+$', 'guid: ' + $guid)
    [System.IO.File]::WriteAllText($_.FullName + '.meta', $meta.Replace("`r`n", "`n"), $utf8NoBom)
}

Write-Output 'Unity meta generated for Jokers_0_07'
