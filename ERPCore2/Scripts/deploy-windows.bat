@echo off
REM Windows Deployment Batch File for ERP Core 2
echo Starting ERP Core 2 deployment...

REM Check if running as Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo This script requires Administrator privileges.
    echo Please run as Administrator.
    pause
    exit /b 1
)

REM Execute PowerShell deployment script
powershell -ExecutionPolicy Bypass -File "%~dp0deploy-windows.ps1"

pause
