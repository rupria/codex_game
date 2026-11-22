[CmdletBinding()]
param(
    [string]$RepositoryPath = (Split-Path -Parent $PSScriptRoot),
    [string]$ProjectDrivePath = $env:CODEX_GAME_DRIVE_ROOT,
    [string]$DocumentVaultPath
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($DocumentVaultPath) -and -not [string]::IsNullOrWhiteSpace($ProjectDrivePath)) {
    $DocumentVaultPath = Join-Path $ProjectDrivePath 'obsidian'
}

$repository = (Resolve-Path -LiteralPath $RepositoryPath).Path

if (-not (Test-Path -LiteralPath (Join-Path $repository '.git'))) {
    throw "Not a Git repository: $repository"
}

$remote = (git -C $repository remote get-url origin).Trim()
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to read the origin remote.'
}

Write-Output "Code workspace: $repository"
Write-Output "Origin: $remote"

if ([string]::IsNullOrWhiteSpace($DocumentVaultPath)) {
    Write-Warning 'Drive document vault is not configured. Set CODEX_GAME_DRIVE_ROOT or pass -DocumentVaultPath.'
}
else {
    Write-Output "Drive document vault: $DocumentVaultPath"
    if (Test-Path -LiteralPath (Join-Path $DocumentVaultPath 'rules')) {
        Write-Output 'Drive rules: available'
    }
    else {
        Write-Warning 'Drive document vault is not connected. Code work is available, but check Drive before making document-dependent decisions.'
    }
}

Write-Output 'Code is managed in Git. Planning, rules, QA, PM, and submission documents are managed in Drive.'
git -C $repository status --short --branch
