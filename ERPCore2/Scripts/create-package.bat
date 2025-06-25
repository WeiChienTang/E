@echo off
REM Create Deployment Package for ERP Core 2
echo ================================================
echo ERP Core 2 - Create Deployment Package
echo ================================================
echo.
echo This will create a complete deployment package for your customer
echo.

REM Check if running from project directory
if not exist "ERPCore2.csproj" (
    echo Error: Please run this script from the project root directory
    echo Current directory: %CD%
    pause
    exit /b 1
)

echo Creating deployment package...
powershell -ExecutionPolicy Bypass -File "%~dp0create-deployment-package.ps1"

echo.
echo ================================================
echo Package creation completed!
echo Check the DeploymentPackages folder
echo ================================================
pause
