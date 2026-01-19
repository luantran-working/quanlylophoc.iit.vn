@echo off
title Running Two .NET Projects

echo Starting two .NET projects simultaneously...
echo.

start "Project 1" cmd /k "cd /d "%~dp0" && dotnet run"
timeout /t 2 /nobreak > nul
start "Project 2" cmd /k "cd /d "%~dp0" && dotnet run"

echo Both projects have been started in separate windows.
pause