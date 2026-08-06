[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'programer\CodexGame'
$editorPath = Join-Path $repoRoot '.local-tools\Unity\6000.3.18f1\Editor\Unity.exe'

if (-not (Test-Path -LiteralPath $editorPath -PathType Leaf)) {
  throw "저장소 전용 Unity Editor가 없습니다: $editorPath"
}

if (-not (Test-Path -LiteralPath $projectPath -PathType Container)) {
  throw "Unity 프로젝트가 없습니다: $projectPath"
}

Start-Process `
  -FilePath $editorPath `
  -ArgumentList @('-projectPath', $projectPath) `
  -WorkingDirectory $repoRoot

Write-Output "UnityEditor=$editorPath"
Write-Output "Project=$projectPath"
