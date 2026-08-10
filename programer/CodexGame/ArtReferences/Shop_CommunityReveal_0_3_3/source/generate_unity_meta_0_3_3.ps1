param(
    [Parameter(Mandatory = $true)]
    [string]$RepoRoot
)

$runtimeRelatives = @(
    'programer/CodexGame/Assets/Art/Prototype/UI/BarShop_0_3_3',
    'programer/CodexGame/Assets/Art/Prototype/UI/Halli_0_3_3',
    'programer/CodexGame/Assets/Art/Prototype/UI/Poker_0_3_3'
)
$templatePath = Join-Path $RepoRoot 'programer/CodexGame/Assets/Art/Prototype/UI/Poker_0_2_2/hp_heart_player_filled_24_0_2_2.png.meta'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Get-DeterministicGuid([string]$value) {
    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($value.Replace('\', '/').ToLowerInvariant())
        return ([System.BitConverter]::ToString($md5.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally { $md5.Dispose() }
}

$template = [System.IO.File]::ReadAllText($templatePath)
$template = [System.Text.RegularExpressions.Regex]::Replace($template, '(?m)[ \t]+$', '')

foreach ($runtimeRelative in $runtimeRelatives) {
    $runtimeDir = Join-Path $RepoRoot $runtimeRelative
    $folderGuid = Get-DeterministicGuid $runtimeRelative
    $folderMeta = @"
fileFormatVersion: 2
guid: $folderGuid
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"@
    [System.IO.File]::WriteAllText($runtimeDir + '.meta', $folderMeta.Replace("`r`n", "`n"), $utf8NoBom)

    Get-ChildItem -LiteralPath $runtimeDir -Filter '*.png' | Sort-Object Name | ForEach-Object {
        $relative = $runtimeRelative + '/' + $_.Name
        $guid = Get-DeterministicGuid $relative
        $meta = [System.Text.RegularExpressions.Regex]::Replace($template, '(?m)^guid: [0-9a-f]+$', 'guid: ' + $guid)
        [System.IO.File]::WriteAllText($_.FullName + '.meta', $meta.Replace("`r`n", "`n"), $utf8NoBom)
    }
}

Write-Output 'Unity meta generated for Shop_CommunityReveal_0_3_3'
