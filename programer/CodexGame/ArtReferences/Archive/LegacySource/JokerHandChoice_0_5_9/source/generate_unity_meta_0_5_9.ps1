param(
  [Parameter(Mandatory = $true)][string]$RuntimeDirectory,
  [Parameter(Mandatory = $true)][string]$TemplateMeta
)

$utf8 = [System.Text.UTF8Encoding]::new($false)
$md5 = [System.Security.Cryptography.MD5]::Create()

function Get-StableId([string]$value) {
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($value.Replace('\\', '/').ToLowerInvariant())
  return ([System.BitConverter]::ToString($md5.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
}

$packageName = Split-Path -Leaf $RuntimeDirectory
$folderGuid = Get-StableId ($packageName + '.folder')
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
[System.IO.File]::WriteAllText($RuntimeDirectory + '.meta', $folderMeta, $utf8)

$template = [System.IO.File]::ReadAllText($TemplateMeta)
Get-ChildItem -LiteralPath $RuntimeDirectory -Filter '*.png' | ForEach-Object {
  $assetKey = $packageName + '/' + $_.Name
  $guid = Get-StableId $assetKey
  $spriteId = Get-StableId ($assetKey + '.sprite')
  $meta = $template -replace '(?m)^guid: [0-9a-f]+$', ('guid: ' + $guid)
  $meta = $meta -replace '(?m)^    spriteID: [0-9a-f]+$', ('    spriteID: ' + $spriteId)
  [System.IO.File]::WriteAllText($_.FullName + '.meta', $meta, $utf8)
}

$md5.Dispose()
