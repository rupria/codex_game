[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\codes\codex_game',
    [string]$DriveSharePath = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($DriveSharePath)) {
    $driveFolder = -join @(
        [char]0xB0B4,
        ' ',
        [char]0xB4DC,
        [char]0xB77C,
        [char]0xC774,
        [char]0xBE0C
    )
    $DriveSharePath = Join-Path (Join-Path 'G:\' $driveFolder) 'codex_game\shared'
}

$repository = (Resolve-Path -LiteralPath $RepositoryPath).Path

if (-not (Test-Path -LiteralPath (Join-Path $repository '.git'))) {
    throw "Not a Git repository: $repository"
}

$dirty = @(git -C $repository status --porcelain=v1)
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to read Git status.'
}
if ($dirty.Count -gt 0) {
    throw 'Only a clean, committed working tree can be published.'
}

$branch = (git -C $repository branch --show-current).Trim()
$sha = (git -C $repository rev-parse HEAD).Trim()
$shortSha = (git -C $repository rev-parse --short=12 HEAD).Trim()
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$releaseName = "$timestamp-$branch-$shortSha"
$releaseRoot = Join-Path (Join-Path $DriveSharePath 'releases') $releaseName

if (Test-Path -LiteralPath $releaseRoot) {
    throw "Release already exists: $releaseRoot"
}

New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null

$zipPath = Join-Path $releaseRoot 'codex_game-source.zip'
$bundlePath = Join-Path $releaseRoot 'codex_game.bundle'
$manifestPath = Join-Path $releaseRoot 'SHARE_MANIFEST.md'

git -C $repository archive --format=zip --output=$zipPath HEAD
if ($LASTEXITCODE -ne 0) {
    throw 'git archive failed.'
}

git -C $repository bundle create $bundlePath --all
if ($LASTEXITCODE -ne 0) {
    throw 'git bundle creation failed.'
}

$fileCount = @(git -C $repository ls-tree -r --name-only HEAD).Count
$zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
$bundleHash = (Get-FileHash -LiteralPath $bundlePath -Algorithm SHA256).Hash
$created = Get-Date -Format 'yyyy-MM-dd HH:mm:ss K'

$manifest = @(
    '# codex_game shared release',
    '',
    "- Release: $releaseName",
    "- Created: $created",
    "- Branch: $branch",
    "- Commit: $sha",
    "- Tracked files: $fileCount",
    "- Source ZIP SHA256: $zipHash",
    "- Git bundle SHA256: $bundleHash",
    '',
    'This release contains committed files only. Clone the bundle or GitHub repository to a local disk before coding.'
)
$manifest | Set-Content -LiteralPath $manifestPath -Encoding UTF8

New-Item -ItemType Directory -Path $DriveSharePath -Force | Out-Null
$releaseName | Set-Content -LiteralPath (Join-Path $DriveSharePath 'LATEST.txt') -Encoding ASCII

Write-Output "Drive release created: $releaseRoot"
Write-Output "Commit: $sha"
Write-Output "Source ZIP SHA256: $zipHash"
Write-Output "Git bundle SHA256: $bundleHash"
