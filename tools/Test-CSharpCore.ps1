[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$testProject = Join-Path $repoRoot 'programer\dotnet\CodexGame.SmokeTests\CodexGame.SmokeTests.csproj'

if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
  throw ".NET SDK 실행 파일이 없습니다: $dotnet"
}

& $dotnet run `
  --project $testProject `
  --configuration Release

if ($LASTEXITCODE -ne 0) {
  throw "C# core smoke tests failed with exit code $LASTEXITCODE"
}
