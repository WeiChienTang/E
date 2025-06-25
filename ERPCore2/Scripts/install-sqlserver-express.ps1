# SQL Server Express Auto-Installer for ERP Core 2
# This script automatically downloads and installs SQL Server Express with optimal settings

Write-Host "SQL Server Express Auto-Installer for ERP Core 2" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan

# Check if SQL Server is already installed
$sqlServices = Get-Service -Name "MSSQL*" -ErrorAction SilentlyContinue
if ($sqlServices) {
    Write-Host "[+] SQL Server already installed. Services found:" -ForegroundColor Green
    foreach ($service in $sqlServices) {
        Write-Host "    - $($service.Name) ($($service.Status))" -ForegroundColor White
    }
    Write-Host "[+] SQL Server installation check completed successfully" -ForegroundColor Green
    exit 0
}

Write-Host "No SQL Server installation found. Starting automatic installation..." -ForegroundColor Yellow

# Create temporary directory
$tempDir = "$env:TEMP\SQLServerExpress"
New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

try {
    # Download SQL Server Express
    $sqlExpressUrl = "https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SQLEXPR_x64_ENU.exe"
    $sqlExpressPath = "$tempDir\SQLEXPR_x64_ENU.exe"
    
    Write-Host "Downloading SQL Server Express..." -ForegroundColor Yellow
    Write-Host "URL: $sqlExpressUrl" -ForegroundColor Cyan
    
    # Download with progress
    $webClient = New-Object System.Net.WebClient
    $webClient.DownloadFile($sqlExpressUrl, $sqlExpressPath)
    
    Write-Host "[+] SQL Server Express downloaded successfully" -ForegroundColor Green
    
    # Extract installation files
    Write-Host "Extracting SQL Server Express..." -ForegroundColor Yellow
    $extractPath = "$tempDir\SQLExpressExtracted"
    Start-Process -FilePath $sqlExpressPath -ArgumentList "/x:$extractPath", "/u" -Wait -NoNewWindow
    
    # Install SQL Server Express with optimal settings for ERP
    Write-Host "Installing SQL Server Express with ERP-optimized settings..." -ForegroundColor Yellow
    
    $setupPath = "$extractPath\setup.exe"
    $installArgs = @(
        "/QUIETSIMPLE",
        "/IACCEPTSQLSERVERLICENSETERMS",
        "/ACTION=Install",
        "/FEATURES=SQLEngine,Tools",
        "/INSTANCENAME=SQLEXPRESS",
        "/SQLSVCACCOUNT=""NT AUTHORITY\SYSTEM""",
        "/SQLSYSADMINACCOUNTS=""$env:COMPUTERNAME\$env:USERNAME""",
        "/SECURITYMODE=Mixed",
        "/SAPWD=""ERPCore2Admin123!""",
        "/TCPENABLED=1",
        "/NPENABLED=1",
        "/BROWSERSVCSTARTUPTYPE=Automatic"
    )
    
    Write-Host "Installation command: $setupPath $($installArgs -join ' ')" -ForegroundColor Cyan
    $installProcess = Start-Process -FilePath $setupPath -ArgumentList $installArgs -Wait -PassThru -NoNewWindow
    
    if ($installProcess.ExitCode -eq 0) {
        Write-Host "[+] SQL Server Express installed successfully!" -ForegroundColor Green
        
        # Configure SQL Server for remote connections
        Write-Host "Configuring SQL Server for remote connections..." -ForegroundColor Yellow
        
        # Start SQL Server services
        Start-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
        Start-Service -Name "SQLBrowser" -ErrorAction SilentlyContinue
        
        # Enable TCP/IP protocol
        $sqlWmiNamespace = "root\Microsoft\SqlServer\ComputerManagement15"
        try {
            $sqlWmi = Get-WmiObject -Namespace $sqlWmiNamespace -Class ServerNetworkProtocol | Where-Object {$_.ProtocolName -eq 'Tcp'}
            $sqlWmi.SetEnable()
            Write-Host "[+] TCP/IP protocol enabled" -ForegroundColor Green
        } catch {
            Write-Host "[!] Could not configure TCP/IP via WMI: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        # Configure Windows Firewall for SQL Server
        try {
            New-NetFirewallRule -DisplayName "SQL Server Express" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow -ErrorAction SilentlyContinue
            Write-Host "[+] Windows Firewall configured for SQL Server" -ForegroundColor Green
        } catch {
            Write-Host "[!] Firewall configuration warning: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        Write-Host "`n[SUCCESS] SQL Server Express installation completed!" -ForegroundColor Green
        Write-Host "Default credentials created:" -ForegroundColor Cyan
        Write-Host "  Server: localhost\SQLEXPRESS" -ForegroundColor White
        Write-Host "  Username: sa" -ForegroundColor White
        Write-Host "  Password: ERPCore2Admin123!" -ForegroundColor White
        Write-Host "`nYou can now run the ERP Core 2 deployment script." -ForegroundColor Yellow
        exit 0
        
    } else {
        Write-Host "[-] SQL Server Express installation failed with exit code: $($installProcess.ExitCode)" -ForegroundColor Red
        Write-Host "Please install SQL Server Express manually from:" -ForegroundColor Yellow
        Write-Host "https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Cyan
        exit 1
    }
    
} catch {
    Write-Host "[-] Installation error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please install SQL Server Express manually from:" -ForegroundColor Yellow
    Write-Host "https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Cyan
    exit 1
} finally {
    # Cleanup
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
