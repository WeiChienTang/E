# Create Deployment Package Script for ERP Core 2
# This script creates a complete deployment package for customers

Write-Host "Creating ERP Core 2 Deployment Package..." -ForegroundColor Green

# Set package name with timestamp
$packageName = "ERPCore2-Deployment-$(Get-Date -Format 'yyyyMMdd-HHmm')"
$packagePath = ".\Scripts\DeploymentPackages\$packageName"

# Create package directory
Write-Host "Creating package directory: $packagePath" -ForegroundColor Yellow
New-Item -Path $packagePath -ItemType Directory -Force | Out-Null

# Clean up old packages (keep only last 3)
Write-Host "Cleaning up old deployment packages..." -ForegroundColor Yellow
$allPackages = Get-ChildItem -Path ".\Scripts\DeploymentPackages" -Directory | Where-Object { $_.Name -like "ERPCore2-Deployment-*" } | Sort-Object CreationTime -Descending
if ($allPackages.Count -gt 3) {
    $packagesToDelete = $allPackages | Select-Object -Skip 3
    foreach ($package in $packagesToDelete) {
        Remove-Item -Path $package.FullName -Recurse -Force
        Write-Host "Removed old package: $($package.Name)" -ForegroundColor Gray
    }
}

# 1. Publish the application for Windows x64
Write-Host "Publishing application for Windows x64..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained -o "$packagePath\publish\win-x64"

if ($LASTEXITCODE -ne 0) {
    Write-Host "[-] Failed to publish application" -ForegroundColor Red
    exit 1
}

# 2. Copy deployment scripts
Write-Host "Copying deployment scripts..." -ForegroundColor Yellow
New-Item -Path "$packagePath\Scripts" -ItemType Directory -Force | Out-Null
Copy-Item ".\Scripts\deploy-windows.ps1" -Destination "$packagePath\Scripts\"
Copy-Item ".\Scripts\deploy-windows.bat" -Destination "$packagePath\Scripts\"
Copy-Item ".\Scripts\install-sqlserver-express.ps1" -Destination "$packagePath\Scripts\"
Copy-Item ".\Scripts\repair-database.ps1" -Destination "$packagePath\Scripts\"
Copy-Item ".\Scripts\complete-setup.ps1" -Destination "$packagePath\Scripts\"
Copy-Item ".\Scripts\diagnose-sql-server.ps1" -Destination "$packagePath\Scripts\"

# 3. Copy configuration files
Write-Host "Copying configuration files..." -ForegroundColor Yellow
Copy-Item ".\appsettings.Production.json" -Destination "$packagePath\appsettings.Production.json"

# Create a template appsettings.json for customer customization
$appSettingsTemplate = @{
    "Logging" = @{
        "LogLevel" = @{
            "Default" = "Information"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
    "AllowedHosts" = "*"
    "ConnectionStrings" = @{
        "DefaultConnection" = "Server=localhost;Database=ERPCore2;Integrated Security=true;TrustServerCertificate=true;"
    }
    "Kestrel" = @{
        "Endpoints" = @{
            "Http" = @{
                "Url" = "http://*:6011"
            }
        }
    }
    "urls" = "http://*:6011"
}

$appSettingsTemplate | ConvertTo-Json -Depth 5 | Set-Content "$packagePath\appsettings.json"

# 4. Copy documentation
Write-Host "Copying documentation..." -ForegroundColor Yellow
Copy-Item ".\DEPLOYMENT_GUIDE.md" -Destination "$packagePath\"
Copy-Item ".\DATABASE_SETUP.md" -Destination "$packagePath\"

# 5. Create README for the package
Write-Host "Creating package README..." -ForegroundColor Yellow
$readmeContent = @"
# ERP Core 2 - Deployment Package

## Quick Start

1. **Extract this package** to your Windows Server
2. **Run as Administrator**: `Scripts\deploy-windows.bat`
3. **Access the application**: http://[SERVER-IP]:6011

## Package Contents

- `publish/win-x64/` - Compiled application files
- `Scripts/` - Automated deployment scripts
- `appsettings.json` - Configuration template
- `DEPLOYMENT_GUIDE.md` - Complete deployment instructions
- `DATABASE_SETUP.md` - SQL Server configuration guide

## Requirements

- Windows Server 2019/2022 or Windows 10/11 Pro
- SQL Server 2019/2022 (Express edition supported)
- .NET 9+ Runtime (ASP.NET Core Runtime)

## Support

For technical support and configuration assistance, please contact your software provider.

## Important Notes

- Run deployment scripts as Administrator
- Port 6011 will be configured for the application
- SQL Server must be accessible for database operations
- Review DEPLOYMENT_GUIDE.md for complete instructions

---
Package created: $(Get-Date)
Version: ERP Core 2
Target Platform: Windows x64
"@

$readmeContent | Set-Content "$packagePath\README.txt"

# 6. Create a simple batch file for easy deployment
Write-Host "Creating quick deployment script..." -ForegroundColor Yellow
$quickDeployContent = @"
@echo off
echo ================================================
echo ERP Core 2 - Complete Automated Deployment
echo ================================================
echo.
echo This will fully deploy and configure ERP Core 2 on your Windows Server
echo.
echo Requirements:
echo - Administrator privileges
echo - Internet connection (for SQL Server download if needed)
echo.

REM Check for SQL Server
echo Step 1: Checking for SQL Server installation...
powershell -Command "Get-Service -Name 'MSSQL*' -ErrorAction SilentlyContinue | Select-Object -First 1" >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo [!] SQL Server not found. Installing SQL Server Express automatically...
    powershell -ExecutionPolicy Bypass -File "%~dp0Scripts\install-sqlserver-express.ps1"
    if %errorlevel% neq 0 (
        echo [-] Failed to install SQL Server Express automatically.
        echo Please install SQL Server Express manually and re-run this script.
        echo Download from: https://www.microsoft.com/sql-server/sql-server-downloads
        pause
        exit /b 1
    )
    echo [+] SQL Server setup completed successfully
) else (
    echo [+] SQL Server installation detected
)

echo.
echo Step 2: Deploying ERP Core 2 application...
cd /d "%~dp0"
cd Scripts
call deploy-windows.bat
if %errorlevel% neq 0 (
    echo [-] Deployment failed. Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo Step 3: Verifying and fixing database setup...
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "Scripts\complete-setup.ps1"

echo.
echo Step 4: Final verification...
timeout /t 3 >nul
echo Checking ERP Core 2 service status...
sc query ERPCore2 >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] ERP Core 2 service not found. Please check the deployment logs.
) else (
    sc query ERPCore2 | findstr "RUNNING" >nul 2>&1
    if %errorlevel% neq 0 (
        echo [!] ERP Core 2 service is not running. Attempting to start...
        net start ERPCore2
    )
    echo [+] Checking service status...
    sc query ERPCore2
)

echo.
echo ================================================
echo ERP Core 2 deployment process completed!
echo.
echo To access your ERP system:
echo http://localhost:6011
echo.
echo If you encounter any issues:
echo 1. Check TROUBLESHOOTING.md
echo 2. Run Scripts\complete-setup.ps1 manually
echo 3. Check Windows Event Viewer for detailed logs
echo ================================================
pause
"@

$quickDeployContent | Set-Content "$packagePath\INSTALL.bat"

# 7. Create a verification script
Write-Host "Creating verification script..." -ForegroundColor Yellow
$verifyContent = @"
@echo off
echo ================================================
echo ERP Core 2 - System Verification
echo ================================================

echo Checking .NET Runtime...
dotnet --version
if %errorlevel% neq 0 (
    echo [-] .NET Runtime not found
    echo Please install .NET 9+ Runtime from: https://dotnet.microsoft.com/download/dotnet
) else (
    echo [+] .NET Runtime found
)

echo.
echo Checking SQL Server...
sqlcmd -S localhost -E -Q "SELECT @@VERSION" -h -1
if %errorlevel% neq 0 (
    echo [-] SQL Server connection failed
    echo Please ensure SQL Server is running and accessible
) else (
    echo [+] SQL Server connection successful
)

echo.
echo Checking ERP Core 2 Service...
sc query ERPCore2 >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] ERP Core 2 service not found
    echo Run INSTALL.bat to deploy the application
) else (
    echo [+] ERP Core 2 service found
    sc query ERPCore2
)

echo.
echo Checking port 6011...
netstat -an | findstr :6011
if %errorlevel% neq 0 (
    echo [!] Port 6011 not in use
) else (
    echo [+] Port 6011 is active
)

echo.
echo ================================================
echo Verification completed
echo ================================================
pause
"@

$verifyContent | Set-Content "$packagePath\VERIFY.bat"

# 8. Create package info file
Write-Host "Creating package information..." -ForegroundColor Yellow
$packageInfo = @{
    "PackageName" = $packageName
    "CreatedDate" = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    "Version" = "ERP Core 2"
    "TargetPlatform" = "Windows x64"
    "DotNetVersion" = "9.0+"
    "DatabaseSupport" = "SQL Server"
    "DefaultPort" = 6011
    "Contents" = @(
        "Compiled application (win-x64)",
        "Deployment scripts",
        "Configuration templates",
        "Documentation",
        "Quick install tools"
    )
}

$packageInfo | ConvertTo-Json -Depth 3 | Set-Content "$packagePath\package-info.json"

# 9. Calculate package size
$packageSize = (Get-ChildItem -Path $packagePath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "[+] Deployment package created successfully!" -ForegroundColor Green
Write-Host "Location: $packagePath" -ForegroundColor Cyan
Write-Host "Size: $([math]::Round($packageSize, 2)) MB" -ForegroundColor Cyan

Write-Host "`nPackage Contents:" -ForegroundColor Yellow
Write-Host "   [DIR] publish/win-x64/     - Compiled application" -ForegroundColor White
Write-Host "   [DIR] Scripts/             - Deployment scripts" -ForegroundColor White
Write-Host "   [FILE] appsettings.json    - Configuration template" -ForegroundColor White
Write-Host "   [FILE] DEPLOYMENT_GUIDE.md - Complete instructions" -ForegroundColor White
Write-Host "   [FILE] README.txt          - Quick start guide" -ForegroundColor White
Write-Host "   [FILE] INSTALL.bat         - One-click installer" -ForegroundColor White
Write-Host "   [FILE] VERIFY.bat          - System verification" -ForegroundColor White

Write-Host "`n[SUCCESS] Ready to deliver to customer!" -ForegroundColor Green
Write-Host "Customer only needs to run: INSTALL.bat (as Administrator)" -ForegroundColor Cyan
