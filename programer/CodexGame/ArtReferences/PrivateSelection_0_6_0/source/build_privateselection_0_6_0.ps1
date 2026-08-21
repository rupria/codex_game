[CmdletBinding()]
param([string]$RepoRoot = ([System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..\..'))))

# Regenerates the approved textless runtime package group.
& (Join-Path $RepoRoot 'programer\CodexGame\ArtReferences\UiPolish_0_6_5\source\build_ui_polish_0_6_5.ps1') -RepoRoot $RepoRoot
Write-Output 'PrivateSelection_0_6_0 regenerated through UiPolish_0_6_5.'