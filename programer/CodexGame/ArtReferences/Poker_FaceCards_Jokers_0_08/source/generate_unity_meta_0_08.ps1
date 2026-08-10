param(
    [Parameter(Mandatory = $true)]
    [string]$UnityRoot,
    [Parameter(Mandatory = $true)]
    [string]$TemplateMeta
)

$ErrorActionPreference = 'Stop'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$template = [System.IO.File]::ReadAllText($TemplateMeta)

function Get-DeterministicGuid([string]$value) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($value.Replace('\', '/').ToLowerInvariant())
        $hash = $sha.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hash).Replace('-', '').ToLowerInvariant()).Substring(0, 32)
    }
    finally {
        $sha.Dispose()
    }
}

$root = [System.IO.Path]::GetFullPath($UnityRoot)
$files = Get-ChildItem -LiteralPath $root -Recurse -Filter '*.png'
foreach ($file in $files) {
    $relative = $file.FullName.Substring($root.Length).TrimStart('\')
    $guid = Get-DeterministicGuid("cards_0_08/$relative")
    $content = [System.Text.RegularExpressions.Regex]::Replace(
        $template,
        '(?m)^guid: .+$',
        "guid: $guid"
    )
    [System.IO.File]::WriteAllText($file.FullName + '.meta', $content, $utf8NoBom)
}

$folderTemplate = "fileFormatVersion: 2`nguid: {0}`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {{}}`n  userData:`n  assetBundleName:`n  assetBundleVariant:`n"
$folders = @($root) + @(Get-ChildItem -LiteralPath $root -Directory -Recurse | Select-Object -ExpandProperty FullName)
$parent = Split-Path $root -Parent
foreach ($folder in $folders) {
    $relative = $folder.Substring($parent.Length).TrimStart('\')
    $guid = Get-DeterministicGuid("cards_0_08/folder/$relative")
    [System.IO.File]::WriteAllText($folder + '.meta', ($folderTemplate -f $guid), $utf8NoBom)
}

Write-Output ("pngMeta=" + $files.Count)
