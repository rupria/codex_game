[CmdletBinding()]
param(
  [string]$CodexGameRoot = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CodexGameRoot)) {
  $CodexGameRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
} else {
  $CodexGameRoot = [System.IO.Path]::GetFullPath($CodexGameRoot)
}

$assetsRoot = Join-Path $CodexGameRoot "Assets"
$manifestPath = Join-Path $PSScriptRoot "current_art_runtime_manifest_0_6_1.json"
$builderPath = Join-Path $assetsRoot "Editor\PlayableDevSceneBuilder.cs"
$scenePath = Join-Path $assetsRoot "Scenes\PlayableDev.unity"

foreach ($required in @($manifestPath, $builderPath, $scenePath)) {
  if (-not (Test-Path -LiteralPath $required)) {
    throw "Required audit input is missing: $required"
  }
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$builderText = Get-Content -LiteralPath $builderPath -Raw
$sceneText = Get-Content -LiteralPath $scenePath -Raw
$failed = $false

Write-Output "ART_RUNTIME_AUDIT revision=$($manifest.manifestRevision)"
Write-Output "PENDING_PROGRAMMER_BINDINGS"

foreach ($entry in $manifest.pendingProgrammerBindings) {
  $runtimeRoot = Join-Path $CodexGameRoot ([string]$entry.runtimeRoot)
  $exists = Test-Path -LiteralPath $runtimeRoot
  $builderMentionsPackage = $builderText.Contains([string]$entry.package)
  Write-Output ("issue={0} package={1} assets={2} builderBound={3}" -f `
    $entry.issue, $entry.package, $exists, $builderMentionsPackage)
  if (-not $exists) {
    $failed = $true
  }
}

Write-Output "ARCHIVED_RUNTIME_GUARD"
foreach ($entry in $manifest.archivedSupersededRuntime) {
  $restoredRuntime = Join-Path $assetsRoot ("Art\Prototype\UI\" + [string]$entry.package)
  $restored = Test-Path -LiteralPath $restoredRuntime
  Write-Output ("package={0} restoredUnderAssets={1}" -f $entry.package, $restored)
  if ($restored) {
    $failed = $true
  }
}

$guidToPackage = @{}
$uiRoot = Join-Path $assetsRoot "Art\Prototype\UI"
Get-ChildItem -LiteralPath $uiRoot -Recurse -Filter "*.meta" -File | ForEach-Object {
  $guidMatch = Select-String -LiteralPath $_.FullName -Pattern "^guid: (.+)$"
  if (-not $guidMatch) {
    return
  }

  $relative = $_.FullName.Substring($uiRoot.Length + 1)
  $separator = $relative.IndexOf([char]92)
  if ($separator -le 0) {
    return
  }

  $guid = $guidMatch.Matches[0].Groups[1].Value
  $guidToPackage[$guid] = $relative.Substring(0, $separator)
}

$scenePackageCounts = @{}
foreach ($guid in $guidToPackage.Keys) {
  if (-not $sceneText.Contains($guid)) {
    continue
  }

  $package = $guidToPackage[$guid]
  if (-not $scenePackageCounts.ContainsKey($package)) {
    $scenePackageCounts[$package] = 0
  }
  $scenePackageCounts[$package]++
}

Write-Output "SERIALIZED_LEGACY_BINDINGS"
foreach ($entry in $manifest.serializedLegacyBindings) {
  $actual = if ($scenePackageCounts.ContainsKey([string]$entry.package)) {
    $scenePackageCounts[[string]$entry.package]
  } else {
    0
  }
  Write-Output ("package={0} sceneReferences={1}" -f $entry.package, $actual)
}

if ($failed) {
  throw "ART_RUNTIME_AUDIT_FAILED"
}

Write-Output "ART_RUNTIME_AUDIT_OK"
