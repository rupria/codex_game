param(
  [Parameter(Mandatory = $true)][string]$ProjectAssetsRoot,
  [Parameter(Mandatory = $true)][string]$RuntimeDirectory,
  [Parameter(Mandatory = $true)][string]$TemplateMeta
)

$utf8 = [System.Text.UTF8Encoding]::new($false)
$md5 = [System.Security.Cryptography.MD5]::Create()
$assetsRoot = [System.IO.Path]::GetFullPath($ProjectAssetsRoot)
$runtimeRoot = [System.IO.Path]::GetFullPath($RuntimeDirectory)

if (!$runtimeRoot.StartsWith($assetsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
  throw "Runtime directory must be inside the supplied Unity Assets root: $runtimeRoot"
}

function Get-StableId([string]$value) {
  $normalized = $value.Replace('\', '/').ToLowerInvariant()
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($normalized)
  return ([System.BitConverter]::ToString($md5.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
}

function Get-ProjectKey([string]$path) {
  $relative = [System.IO.Path]::GetRelativePath($assetsRoot, [System.IO.Path]::GetFullPath($path))
  return ('Assets/' + $relative.Replace('\', '/'))
}

$folderGuid = Get-StableId ((Get-ProjectKey $runtimeRoot) + '.folder')
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
[System.IO.File]::WriteAllText($runtimeRoot + '.meta', $folderMeta, $utf8)

$template = [System.IO.File]::ReadAllText($TemplateMeta)
Get-ChildItem -LiteralPath $runtimeRoot -Filter '*.png' | ForEach-Object {
  $projectKey = Get-ProjectKey $_.FullName
  $guid = Get-StableId $projectKey
  $spriteId = Get-StableId ($projectKey + '.sprite')
  $meta = $template -replace '(?m)^guid: [0-9a-f]+$', ('guid: ' + $guid)
  $meta = $meta -replace '(?m)^    spriteID: [0-9a-f]+$', ('    spriteID: ' + $spriteId)
  $meta = $meta -replace '(?m)[ \t]+$', ''
  [System.IO.File]::WriteAllText($_.FullName + '.meta', $meta, $utf8)
}

$md5.Dispose()
