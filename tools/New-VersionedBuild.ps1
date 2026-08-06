[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$ArtifactPath,

    [string]$RepositoryPath = 'C:\sk-encoa\codex_game',
    [string]$DriveWorkspacePath = 'G:\내 드라이브\codex_game',
    [string]$OutputRoot,
    [string[]]$SpecificationPaths
)

$ErrorActionPreference = 'Stop'

function Get-ResolvedDirectory {
    param([string]$Path, [string]$Label)

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Label 폴더를 찾을 수 없습니다: $Path"
    }
    return (Resolve-Path -LiteralPath $Path).Path
}

function Assert-ChildPath {
    param([string]$ChildPath, [string]$ParentPath)

    $parent = [System.IO.Path]::GetFullPath($ParentPath).TrimEnd('\')
    $child = [System.IO.Path]::GetFullPath($ChildPath)
    if (-not $child.StartsWith($parent + '\', [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "허용된 폴더 밖의 경로입니다: $child"
    }
}

if ($Version -notmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
    throw "버전은 선행 0이 없는 Major.Minor.Patch 형식이어야 합니다: $Version"
}

$repository = Get-ResolvedDirectory -Path $RepositoryPath -Label 'Git 저장소'
$driveWorkspace = Get-ResolvedDirectory -Path $DriveWorkspacePath -Label 'Drive 작업공간'

if (-not (Test-Path -LiteralPath (Join-Path $repository '.git'))) {
    throw "Git 저장소가 아닙니다: $repository"
}

if (-not (Test-Path -LiteralPath $ArtifactPath)) {
    throw "빌드 산출물을 찾을 수 없습니다: $ArtifactPath"
}
$artifact = (Resolve-Path -LiteralPath $ArtifactPath).Path

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $driveWorkspace 'multi_pc\versioned_builds'
}
$outputRootFull = [System.IO.Path]::GetFullPath($OutputRoot)

if (-not $SpecificationPaths -or $SpecificationPaths.Count -eq 0) {
    $SpecificationPaths = @(
        'obsidian\rules',
        'obsidian\core\designer',
        'obsidian\core\programer',
        'obsidian\core\30_Decisions'
    )
}

$dirty = @(& git -C $repository status --porcelain=v1 --untracked-files=all)
if ($LASTEXITCODE -ne 0) {
    throw 'Git 상태를 확인하지 못했습니다.'
}
if ($dirty.Count -gt 0) {
    throw "공식 빌드는 깨끗한 Git 작업 트리에서만 만들 수 있습니다.`n$($dirty -join "`n")"
}

$gitCommit = (& git -C $repository rev-parse --verify 'HEAD^{commit}').Trim()
if ($LASTEXITCODE -ne 0 -or $gitCommit -notmatch '^[0-9a-fA-F]{40}$') {
    throw '전체 Git commit SHA를 확인하지 못했습니다.'
}
$gitShortCommit = (& git -C $repository rev-parse --short=12 HEAD).Trim()
$gitBranch = (& git -C $repository branch --show-current).Trim()
$gitTag = "v$Version"

& git -C $repository rev-parse --verify --quiet "refs/tags/$gitTag" *> $null
if ($LASTEXITCODE -eq 0) {
    throw "이미 존재하는 Git 태그입니다: $gitTag"
}

New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null
$existingVersions = @(
    Get-ChildItem -LiteralPath $outputRootFull -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$' } |
        ForEach-Object { [System.Version]$_.Name.Substring(1) }
)
$requestedVersion = [System.Version]$Version

if ($existingVersions.Count -gt 0) {
    $latestVersion = $existingVersions | Sort-Object -Descending | Select-Object -First 1
    if ($requestedVersion -le $latestVersion) {
        throw "새 버전은 현재 최고 버전 $latestVersion 보다 커야 합니다: $Version"
    }
    if ($requestedVersion.Major -gt $latestVersion.Major -and ($requestedVersion.Minor -ne 0 -or $requestedVersion.Build -ne 0)) {
        throw 'Major 증가 시 Minor와 Patch는 0이어야 합니다.'
    }
    if ($requestedVersion.Major -eq $latestVersion.Major -and $requestedVersion.Minor -gt $latestVersion.Minor -and $requestedVersion.Build -ne 0) {
        throw 'Minor 증가 시 Patch는 0이어야 합니다.'
    }
}

$finalDirectory = Join-Path $outputRootFull $gitTag
if (Test-Path -LiteralPath $finalDirectory) {
    throw "공식 배포 폴더는 덮어쓸 수 없습니다: $finalDirectory"
}

$stagingDirectory = Join-Path $outputRootFull ('.staging-{0}-{1}' -f $gitTag, [guid]::NewGuid().ToString('N'))
Assert-ChildPath -ChildPath $stagingDirectory -ParentPath $outputRootFull

try {
    New-Item -ItemType Directory -Path $stagingDirectory | Out-Null
    $artifactDirectory = Join-Path $stagingDirectory 'artifact'
    $specSnapshotDirectory = Join-Path $stagingDirectory 'spec_snapshot'
    New-Item -ItemType Directory -Path $artifactDirectory, $specSnapshotDirectory | Out-Null

    if (Test-Path -LiteralPath $artifact -PathType Container) {
        $packagedArtifact = Join-Path $artifactDirectory ("codex_game-$gitTag-g$gitShortCommit.zip")
        $artifactChildren = @(Get-ChildItem -LiteralPath $artifact -Force)
        if ($artifactChildren.Count -eq 0) {
            throw "빌드 산출물 폴더가 비어 있습니다: $artifact"
        }
        Compress-Archive -LiteralPath $artifactChildren.FullName -DestinationPath $packagedArtifact -CompressionLevel Optimal
    }
    else {
        $packagedArtifact = Join-Path $artifactDirectory ([System.IO.Path]::GetFileName($artifact))
        Copy-Item -LiteralPath $artifact -Destination $packagedArtifact
    }

    $copiedRoots = [System.Collections.Generic.List[string]]::new()
    foreach ($relativeSpecPath in $SpecificationPaths) {
        $source = Join-Path $driveWorkspace $relativeSpecPath
        if (-not (Test-Path -LiteralPath $source)) {
            throw "명세 경로를 찾을 수 없습니다: $source"
        }
        $destination = Join-Path $specSnapshotDirectory $relativeSpecPath
        $destinationParent = Split-Path -Parent $destination
        New-Item -ItemType Directory -Path $destinationParent -Force | Out-Null
        Copy-Item -LiteralPath $source -Destination $destination -Recurse
        $copiedRoots.Add($relativeSpecPath.Replace('\', '/'))
    }

    $specManifestPath = Join-Path $stagingDirectory 'SPEC_FILES_SHA256.txt'
    $specLines = Get-ChildItem -LiteralPath $specSnapshotDirectory -Recurse -File |
        ForEach-Object {
            $relative = $_.FullName.Substring($specSnapshotDirectory.Length + 1).Replace('\', '/')
            '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $relative
        } |
        Sort-Object
    [System.IO.File]::WriteAllLines($specManifestPath, [string[]]$specLines, [System.Text.UTF8Encoding]::new($false))

    $specRevision = (Get-FileHash -LiteralPath $specManifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $artifactSha256 = (Get-FileHash -LiteralPath $packagedArtifact -Algorithm SHA256).Hash.ToLowerInvariant()
    $artifactRelativePath = $packagedArtifact.Substring($stagingDirectory.Length + 1).Replace('\', '/')

    $manifest = [ordered]@{
        SchemaVersion = 1
        ProductVersion = $Version
        GitTag = $gitTag
        GitBranch = $gitBranch
        GitCommit = $gitCommit.ToLowerInvariant()
        GitShortCommit = $gitShortCommit.ToLowerInvariant()
        BuildRevision = "g$($gitShortCommit.ToLowerInvariant())"
        SpecRevision = $specRevision
        SpecRevisionDisplay = "spec-$($specRevision.Substring(0, 12))"
        SpecificationRoots = @($copiedRoots)
        ArtifactPath = $artifactRelativePath
        ArtifactSHA256 = $artifactSha256
        CreatedAt = [DateTimeOffset]::Now.ToString('o')
        Immutable = $true
    }
    $manifestJson = $manifest | ConvertTo-Json -Depth 5
    [System.IO.File]::WriteAllText((Join-Path $stagingDirectory 'RELEASE_MANIFEST.json'), $manifestJson, [System.Text.UTF8Encoding]::new($false))

    $immutableNotice = @(
        "OFFICIAL BUILD: $gitTag"
        'This directory is immutable. Do not edit or overwrite it.'
        'Any code, specification, configuration, or artifact change requires a new ProductVersion.'
    )
    [System.IO.File]::WriteAllLines((Join-Path $stagingDirectory 'IMMUTABLE_RELEASE.txt'), $immutableNotice, [System.Text.UTF8Encoding]::new($false))

    Move-Item -LiteralPath $stagingDirectory -Destination $finalDirectory
}
catch {
    if (Test-Path -LiteralPath $stagingDirectory) {
        Assert-ChildPath -ChildPath $stagingDirectory -ParentPath $outputRootFull
        if ([System.IO.Path]::GetFileName($stagingDirectory) -like '.staging-*') {
            Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
        }
    }
    throw
}

Write-Output "ReleaseDirectory=$finalDirectory"
Write-Output "ProductVersion=$Version"
Write-Output "GitCommit=$($gitCommit.ToLowerInvariant())"
Write-Output "GitShortCommit=$($gitShortCommit.ToLowerInvariant())"
Write-Output "SpecRevision=$specRevision"
Write-Output "ArtifactSHA256=$artifactSha256"
Write-Output "SuggestedTag=$gitTag"
