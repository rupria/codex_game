[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\sk-encoa\codex_game',
    [string]$DocumentVaultPath = 'G:\내 드라이브\codex_game\obsidian'
)

$ErrorActionPreference = 'Stop'

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
Write-Output "Drive document vault: $DocumentVaultPath"

if (Test-Path -LiteralPath (Join-Path $DocumentVaultPath 'rules')) {
    Write-Output 'Drive rules: available'
}
else {
    Write-Warning 'Drive document vault is not connected. Code work is available, but check Drive before making document-dependent decisions.'
}

Write-Output 'Code is managed in Git. Planning, rules, QA, PM, and submission documents are managed in Drive.'
git -C $repository status --short --branch
