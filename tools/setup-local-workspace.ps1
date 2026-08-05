[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\codes\codex_game',
    [string]$ObsidianVaultPath = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ObsidianVaultPath)) {
    $driveFolder = -join @(
        [char]0xB0B4,
        ' ',
        [char]0xB4DC,
        [char]0xB77C,
        [char]0xC774,
        [char]0xBE0C
    )
    $ObsidianVaultPath = Join-Path (Join-Path 'G:\' $driveFolder) 'codex_game\obsidian'
}

$repository = (Resolve-Path -LiteralPath $RepositoryPath).Path
$vault = (Resolve-Path -LiteralPath $ObsidianVaultPath).Path

if (-not (Test-Path -LiteralPath (Join-Path $repository '.git'))) {
    throw "Not a Git repository: $repository"
}

if (-not (Test-Path -LiteralPath (Join-Path $vault 'rules'))) {
    throw "Obsidian rules directory was not found: $vault"
}

$linkPath = Join-Path $repository 'obsidian'

if (Test-Path -LiteralPath $linkPath) {
    $existing = Get-Item -LiteralPath $linkPath -Force
    $targets = @($existing.Target)
    if ($existing.LinkType -eq 'Junction' -and $targets -contains $vault) {
        Write-Output "Workspace link already exists: $linkPath -> $vault"
    }
    else {
        throw "Existing path will not be overwritten: $linkPath"
    }
}
else {
    New-Item -ItemType Junction -Path $linkPath -Target $vault | Out-Null
    Write-Output "Workspace link created: $linkPath -> $vault"
}

$obsidianConfig = Join-Path $repository '.obsidian'
if (-not (Test-Path -LiteralPath $obsidianConfig)) {
    New-Item -ItemType Directory -Path $obsidianConfig | Out-Null
}

git -C $repository status --short --branch
