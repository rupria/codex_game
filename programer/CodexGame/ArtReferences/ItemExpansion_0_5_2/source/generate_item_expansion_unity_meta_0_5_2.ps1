param(
  [Parameter(Mandatory=$true)][string]$RuntimeDir,
  [Parameter(Mandatory=$true)][string]$TemplateMeta
)

$runtimeFull = [System.IO.Path]::GetFullPath($RuntimeDir)
$templateFull = [System.IO.Path]::GetFullPath($TemplateMeta)
if (!(Test-Path -LiteralPath $runtimeFull)) { throw "Runtime directory not found: $runtimeFull" }
if (!(Test-Path -LiteralPath $templateFull)) { throw "Template meta not found: $templateFull" }
$template = Get-Content -LiteralPath $templateFull -Raw -Encoding UTF8

function Get-StableHex([string]$value, [int]$length) {
  $sha = [System.Security.Cryptography.SHA256]::Create()
  try {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($value)
    $hash = $sha.ComputeHash($bytes)
    $hex = -join ($hash | ForEach-Object { $_.ToString('x2') })
    return $hex.Substring(0, $length)
  } finally { $sha.Dispose() }
}

Get-ChildItem -LiteralPath $runtimeFull -Filter '*.png' -File | Sort-Object Name | ForEach-Object {
  $key = 'codex-game/item-expansion-0.5.2/' + $_.Name.ToLowerInvariant()
  $guid = Get-StableHex $key 32
  $spriteId = Get-StableHex ($key + '/sprite') 32
  $meta = $template -replace '(?m)^guid: [0-9a-f]+$', ('guid: ' + $guid)
  $meta = $meta -replace '(?m)^    spriteID: [0-9a-f]+$', ('    spriteID: ' + $spriteId)
  [System.IO.File]::WriteAllText($_.FullName + '.meta', $meta, (New-Object System.Text.UTF8Encoding($false)))
}

Write-Output ('Generated texture metas: ' + (Get-ChildItem -LiteralPath $runtimeFull -Filter '*.png.meta' -File).Count)
