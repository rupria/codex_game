[CmdletBinding()]
param(
    [string]$SourceRoot = 'G:\내 드라이브\codex_game\art_source\02_working',
    [string]$ExportRoot = 'G:\내 드라이브\codex_game\art_source\03_exports',
    [string]$UnityRoot = 'C:\sk-encoa\codex_game\programer\CodexGame\Assets\Art\Prototype',
    [string]$AsepriteExe = 'D:\Program Files (x86)\steamapps\common\Aseprite\Aseprite.exe',
    [switch]$Force,
    [switch]$Watch,
    [switch]$DryRun,
    [switch]$SkipManifestCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-NormalizedPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    return [System.IO.Path]::GetFullPath($Path).TrimEnd([char[]]@('\', '/'))
}

function Assert-ChildPath {
    param(
        [Parameter(Mandatory = $true)][string]$Parent,
        [Parameter(Mandatory = $true)][string]$Child
    )

    $parentPrefix = (Get-NormalizedPath $Parent) + [System.IO.Path]::DirectorySeparatorChar
    $childPath = Get-NormalizedPath $Child
    if (-not $childPath.StartsWith($parentPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the expected root: $childPath"
    }

    return $childPath
}

function Get-RelativeSourcePath {
    param([Parameter(Mandatory = $true)][string]$SourceFile)

    $fullSource = Assert-ChildPath -Parent $script:SourceRoot -Child $SourceFile
    return $fullSource.Substring($script:SourceRoot.Length).TrimStart([char[]]@('\', '/'))
}

function Copy-IfChanged {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    if ($script:DryRun) {
        Write-Output "[dry-run] copy: $Source -> $Destination"
        return
    }

    $destinationDirectory = Split-Path -Parent $Destination
    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null

    $copyRequired = -not (Test-Path -LiteralPath $Destination -PathType Leaf)
    if (-not $copyRequired) {
        $sourceHash = (Get-FileHash -LiteralPath $Source -Algorithm SHA256).Hash
        $destinationHash = (Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash
        $copyRequired = $sourceHash -ne $destinationHash
    }

    if ($copyRequired) {
        Copy-Item -LiteralPath $Source -Destination $Destination -Force
        Write-Output "[copied] $Destination"
    }
    else {
        Write-Output "[unchanged] $Destination"
    }
}

function Test-AssetManifest {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeSource,
        [Parameter(Mandatory = $true)][string]$OutputFileName
    )

    if ($script:SkipManifestCheck) {
        return
    }

    $artSourceRoot = Split-Path -Parent $script:SourceRoot
    $manifestPath = Join-Path $artSourceRoot 'ASSET_MANIFEST.csv'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        Write-Warning "Asset manifest not found: $manifestPath"
        return
    }

    $normalizedSource = ('02_working/' + $RelativeSource.Replace('\', '/'))
    $rows = @(Import-Csv -LiteralPath $manifestPath -Encoding UTF8)
    $registered = $rows | Where-Object {
        ($_.source_file -and $_.source_file.Replace('\', '/').TrimStart('/') -eq $normalizedSource) -or
        ($_.filename -and $_.filename -eq $OutputFileName)
    }

    if (-not $registered) {
        Write-Warning "Manifest entry is missing for $normalizedSource. Register license and public_build_allowed before a public build."
    }
}

function Export-AsepriteFile {
    param(
        [Parameter(Mandatory = $true)][string]$SourceFile,
        [switch]$ForceExport
    )

    if (-not (Test-Path -LiteralPath $SourceFile -PathType Leaf)) {
        return
    }

    if ([System.IO.Path]::GetExtension($SourceFile) -ne '.aseprite') {
        return
    }

    $relativeSource = Get-RelativeSourcePath $SourceFile
    $relativeDirectory = Split-Path -Parent $relativeSource
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($relativeSource)

    $exportDirectory = if ($relativeDirectory) { Join-Path $script:ExportRoot $relativeDirectory } else { $script:ExportRoot }
    $unityDirectory = if ($relativeDirectory) { Join-Path $script:UnityRoot $relativeDirectory } else { $script:UnityRoot }
    $exportPng = Join-Path $exportDirectory ($baseName + '.png')
    $exportJson = Join-Path $exportDirectory ($baseName + '.json')
    $unityPng = Join-Path $unityDirectory ($baseName + '.png')
    $unityJson = Join-Path $unityDirectory ($baseName + '.json')

    Test-AssetManifest -RelativeSource $relativeSource -OutputFileName ($baseName + '.png')

    $sourceInfo = Get-Item -LiteralPath $SourceFile
    $needsExport = $ForceExport -or
        -not (Test-Path -LiteralPath $exportPng -PathType Leaf) -or
        -not (Test-Path -LiteralPath $exportJson -PathType Leaf)

    if (-not $needsExport) {
        $needsExport = (Get-Item -LiteralPath $exportPng).LastWriteTimeUtc -lt $sourceInfo.LastWriteTimeUtc -or
            (Get-Item -LiteralPath $exportJson).LastWriteTimeUtc -lt $sourceInfo.LastWriteTimeUtc
    }

    if ($needsExport) {
        if ($script:DryRun) {
            Write-Output "[dry-run] export: $SourceFile -> $exportPng"
        }
        else {
            New-Item -ItemType Directory -Force -Path $exportDirectory | Out-Null
            $temporaryPng = Join-Path $exportDirectory ($baseName + '.syncing.png')
            $temporaryJson = Join-Path $exportDirectory ($baseName + '.syncing.json')

            Remove-Item -LiteralPath $temporaryPng, $temporaryJson -Force -ErrorAction SilentlyContinue
            try {
                $quotedSource = '"' + $SourceFile.Replace('"', '\"') + '"'
                $quotedPng = '"' + $temporaryPng.Replace('"', '\"') + '"'
                $quotedJson = '"' + $temporaryJson.Replace('"', '\"') + '"'
                $asepriteArguments = @(
                    '--batch', $quotedSource,
                    '--sheet', $quotedPng,
                    '--data', $quotedJson,
                    '--format', 'json-array',
                    '--sheet-type', 'horizontal'
                ) -join ' '
                $asepriteProcess = Start-Process `
                    -FilePath $script:AsepriteExe `
                    -ArgumentList $asepriteArguments `
                    -WindowStyle Hidden `
                    -Wait `
                    -PassThru

                if ($asepriteProcess.ExitCode -ne 0) {
                    throw "Aseprite export failed with exit code $($asepriteProcess.ExitCode)`: $SourceFile"
                }
                if (-not (Test-Path -LiteralPath $temporaryPng -PathType Leaf) -or
                    -not (Test-Path -LiteralPath $temporaryJson -PathType Leaf)) {
                    throw "Aseprite did not create the expected PNG/JSON output: $SourceFile"
                }

                Move-Item -LiteralPath $temporaryJson -Destination $exportJson -Force
                Move-Item -LiteralPath $temporaryPng -Destination $exportPng -Force
                Write-Output "[exported] $relativeSource"
            }
            finally {
                Remove-Item -LiteralPath $temporaryPng, $temporaryJson -Force -ErrorAction SilentlyContinue
            }
        }
    }
    else {
        Write-Output "[up-to-date] $relativeSource"
    }

    if (-not $script:DryRun -or ((Test-Path -LiteralPath $exportPng) -and (Test-Path -LiteralPath $exportJson))) {
        # JSON is copied first so Unity can read frame metadata when the PNG import event arrives.
        Copy-IfChanged -Source $exportJson -Destination $unityJson
        Copy-IfChanged -Source $exportPng -Destination $unityPng
    }
}

function Sync-AllAsepriteFiles {
    $sources = @(Get-ChildItem -LiteralPath $script:SourceRoot -Filter '*.aseprite' -File -Recurse | Sort-Object FullName)
    if ($sources.Count -eq 0) {
        Write-Output "No .aseprite sources found under $script:SourceRoot"
        return
    }

    foreach ($source in $sources) {
        Export-AsepriteFile -SourceFile $source.FullName -ForceExport:$script:Force
    }
}

$script:SourceRoot = Get-NormalizedPath $SourceRoot
$script:ExportRoot = Get-NormalizedPath $ExportRoot
$script:UnityRoot = Get-NormalizedPath $UnityRoot
$script:AsepriteExe = Get-NormalizedPath $AsepriteExe
$script:DryRun = [bool]$DryRun
$script:Force = [bool]$Force
$script:SkipManifestCheck = [bool]$SkipManifestCheck

if (-not (Test-Path -LiteralPath $script:SourceRoot -PathType Container)) {
    throw "Aseprite source root was not found: $script:SourceRoot"
}
if (-not (Test-Path -LiteralPath $script:AsepriteExe -PathType Leaf)) {
    throw "Aseprite executable was not found: $script:AsepriteExe"
}

if (-not $script:DryRun) {
    New-Item -ItemType Directory -Force -Path $script:ExportRoot, $script:UnityRoot | Out-Null
}

Write-Output "Aseprite=$script:AsepriteExe"
Write-Output "Source=$script:SourceRoot"
Write-Output "Export=$script:ExportRoot"
Write-Output "Unity=$script:UnityRoot"

Sync-AllAsepriteFiles

if (-not $Watch) {
    Write-Output 'Aseprite art sync complete.'
    return
}

$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $script:SourceRoot
$watcher.Filter = '*.aseprite'
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::FileName -bor
    [System.IO.NotifyFilters]::LastWrite -bor
    [System.IO.NotifyFilters]::Size

$watchId = 'CodexGameAseprite.' + [Guid]::NewGuid().ToString('N')
$subscriptions = @(
    Register-ObjectEvent -InputObject $watcher -EventName Changed -SourceIdentifier ($watchId + '.Changed')
    Register-ObjectEvent -InputObject $watcher -EventName Created -SourceIdentifier ($watchId + '.Created')
    Register-ObjectEvent -InputObject $watcher -EventName Renamed -SourceIdentifier ($watchId + '.Renamed')
    Register-ObjectEvent -InputObject $watcher -EventName Deleted -SourceIdentifier ($watchId + '.Deleted')
)

$watcher.EnableRaisingEvents = $true
Write-Output 'Watching .aseprite saves. Press Ctrl+C to stop.'

try {
    while ($true) {
        $event = Wait-Event -Timeout 2
        if (-not $event) {
            continue
        }
        if (-not $event.SourceIdentifier.StartsWith($watchId, [System.StringComparison]::Ordinal)) {
            continue
        }

        $path = $event.SourceEventArgs.FullPath
        $changeType = $event.SourceEventArgs.ChangeType
        Remove-Event -EventIdentifier $event.EventIdentifier -ErrorAction SilentlyContinue

        if ($changeType -eq [System.IO.WatcherChangeTypes]::Deleted) {
            Write-Warning "Source deleted; existing exports were preserved: $path"
            continue
        }

        Start-Sleep -Milliseconds 350
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            try {
                Export-AsepriteFile -SourceFile $path -ForceExport
            }
            catch {
                Write-Error $_
            }
        }
    }
}
finally {
    $watcher.EnableRaisingEvents = $false
    foreach ($subscription in $subscriptions) {
        Unregister-Event -SourceIdentifier $subscription.Name -ErrorAction SilentlyContinue
        Remove-Job -Id $subscription.Id -Force -ErrorAction SilentlyContinue
    }
    $watcher.Dispose()
}
