[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\sk-encoa\codex_game'
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
Write-Output 'Drive documents: https://drive.google.com/drive/folders/1XvoJlNKxS_BgFIEHBQxTwNC5ujtJHBHm'
Write-Output 'Code is managed in Git. Planning, rules, QA, PM, and submission documents are managed in Drive.'

git -C $repository status --short --branch
