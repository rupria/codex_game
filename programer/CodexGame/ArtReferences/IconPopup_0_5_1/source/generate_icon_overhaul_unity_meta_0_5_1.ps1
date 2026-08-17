$ErrorActionPreference = 'Stop'

$codexGameRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$templatePath = Join-Path $codexGameRoot 'Assets\Art\Prototype\UI\Gameplay_0_1_2\item_reload_64_0_1_2.png.meta'
$template = Get-Content -LiteralPath $templatePath -Raw

function Get-StableGuid([string]$relativePath) {
    $normalized = $relativePath.Replace('\', '/').ToLowerInvariant()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($normalized)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').Substring(0, 32).ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

$runtimeFolders = @(
    'Assets\Art\Prototype\UI\IconOverhaul_0_5_0',
    'Assets\Art\Prototype\UI\IconOverhaul_0_5_1'
)

foreach ($relativeFolder in $runtimeFolders) {
    $folderPath = Join-Path $codexGameRoot $relativeFolder
    if (-not (Test-Path -LiteralPath $folderPath)) {
        throw "Missing runtime folder: $folderPath"
    }

    $folderGuid = Get-StableGuid $relativeFolder
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
    [System.IO.File]::WriteAllText("$folderPath.meta", $folderMeta.Replace("`r`n", "`n"), [System.Text.UTF8Encoding]::new($false))

    foreach ($png in Get-ChildItem -LiteralPath $folderPath -Filter '*.png' -File) {
        $relativePng = "$relativeFolder\$($png.Name)"
        $guid = Get-StableGuid $relativePng
        $meta = [System.Text.RegularExpressions.Regex]::Replace($template, '(?m)^guid: [0-9a-f]+$', "guid: $guid")
        [System.IO.File]::WriteAllText("$($png.FullName).meta", $meta.Replace("`r`n", "`n"), [System.Text.UTF8Encoding]::new($false))
    }
}

Write-Output 'Unity metadata generated for IconOverhaul 0.5.0 and IconPopup 0.5.1.'
