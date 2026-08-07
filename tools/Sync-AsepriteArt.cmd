@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Sync-AsepriteArt.ps1" %*
set "result=%ERRORLEVEL%"
if not "%result%"=="0" pause
exit /b %result%
