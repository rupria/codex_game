param(
  [Parameter(Mandatory = $true)][string[]]$RuntimeDirectories,
  [Parameter(Mandatory = $true)][string]$TemplateMeta
)

$utf8 = [System.Text.UTF8Encoding]::new($false)
$md5 = [System.Security.Cryptography.MD5]::Create()

function Get-StableId([string]$value) {
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($value.Replace('\\', '/').ToLowerInvariant())
  return ([System.BitConverter]::ToString($md5.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
}

$template = [System.IO.File]::ReadAllText($TemplateMeta)
foreach ($runtimeDirectory in $RuntimeDirectories) {
  $packageName = Split-Path -Leaf $runtimeDirectory
  $folderGuid = Get-StableId ('codex-game-art/' + $packageName + '/folder')
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
  [System.IO.File]::WriteAllText($runtimeDirectory + '.meta', $folderMeta, $utf8)

  Get-ChildItem -LiteralPath $runtimeDirectory -Filter '*.png' | ForEach-Object {
    $key = 'codex-game-art/' + $packageName + '/' + $_.Name
    $guid = Get-StableId $key
    $spriteId = Get-StableId ($key + '/sprite')
    $meta = $template -replace '(?m)^guid: [0-9a-f]+$', ('guid: ' + $guid)
    $meta = $meta -replace '(?m)^    spriteID: [0-9a-f]+$', ('    spriteID: ' + $spriteId)
    [System.IO.File]::WriteAllText($_.FullName + '.meta', $meta, $utf8)
  }
}

$md5.Dispose()
