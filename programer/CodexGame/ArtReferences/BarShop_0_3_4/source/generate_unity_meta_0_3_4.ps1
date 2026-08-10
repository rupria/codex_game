param(
    [Parameter(Mandatory = $true)] [string[]]$Roots,
    [Parameter(Mandatory = $true)] [string]$TemplateMeta
)

$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
$template = [System.IO.File]::ReadAllText($TemplateMeta)

function Get-ArtGuid([string]$value) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($value.Replace('\', '/').ToLowerInvariant())
        $hash = $sha.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hash).Replace('-', '').ToLowerInvariant()).Substring(0, 32)
    }
    finally { $sha.Dispose() }
}

$folderTemplate = "fileFormatVersion: 2`nguid: {0}`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {{}}`n  userData:`n  assetBundleName:`n  assetBundleVariant:`n"
$count = 0
foreach ($rootInput in $Roots) {
    $root = [System.IO.Path]::GetFullPath($rootInput)
    $salt = Split-Path $root -Leaf
    $files = @(Get-ChildItem -LiteralPath $root -Recurse -Filter '*.png')
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($root.Length).TrimStart('\')
        $guid = Get-ArtGuid("ui_0_3_4/$salt/$relative")
        $content = [System.Text.RegularExpressions.Regex]::Replace($template, '(?m)^guid: .+$', "guid: $guid")
        [System.IO.File]::WriteAllText($file.FullName + '.meta', $content, $utf8)
        $count++
    }
    $folders = @($root) + @(Get-ChildItem -LiteralPath $root -Directory -Recurse | Select-Object -ExpandProperty FullName)
    $parent = Split-Path $root -Parent
    foreach ($folder in $folders) {
        $relative = $folder.Substring($parent.Length).TrimStart('\')
        $guid = Get-ArtGuid("ui_0_3_4/folder/$relative")
        [System.IO.File]::WriteAllText($folder + '.meta', ($folderTemplate -f $guid), $utf8)
    }
}
Write-Output "pngMeta=$count"
