# ERP Core 2 - One-Click Database Fix and Service Starter
# This script ensures everything works after deployment

Write-Host "ERP Core 2 - Complete Setup Verification" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$erpInstallPath = "C:\inetpub\ERPCore2"
$erpExePath = "$erpInstallPath\ERPCore2.exe"

# Function to test SQL Server connectivity
function Test-SqlConnection {
    param($server)
    try {
        $testCmd = "sqlcmd -S `"$server`" -E -Q `"SELECT 1`" -h -1"
        $result = Invoke-Expression $testCmd 2>$null
        return ($result -and $result.Trim() -eq "1")
    } catch {
        return $false
    }
}

# Function to get table count
function Get-TableCount {
    param($server)
    try {
        $tableCountCmd = "sqlcmd -S `"$server`" -E -d ERPCore2 -Q `"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'`" -h -1"
        $result = Invoke-Expression $tableCountCmd 2>$null
        if ($result) {
            return [int]$result.Trim()
        }
        return 0
    } catch {
        return 0
    }
}

# Step 1: Verify installation
Write-Host "Step 1: Verifying ERP Core 2 installation..." -ForegroundColor Cyan
if (-not (Test-Path $erpExePath)) {
    Write-Host "[-] ERPCore2.exe not found at: $erpExePath" -ForegroundColor Red
    Write-Host "Please run the main deployment first (INSTALL.bat)" -ForegroundColor Yellow
    exit 1
}
Write-Host "[+] ERP Core 2 installation found" -ForegroundColor Green

# Step 2: Find working SQL Server
Write-Host "`nStep 2: Testing SQL Server connectivity..." -ForegroundColor Cyan
$sqlServers = @("localhost\SQLEXPRESS", "localhost", ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB")
$workingServer = $null

foreach ($server in $sqlServers) {
    if (Test-SqlConnection -server $server) {
        Write-Host "[+] SQL Server connection successful: $server" -ForegroundColor Green
        $workingServer = $server
        break
    }
}

if (-not $workingServer) {
    Write-Host "[-] Cannot connect to any SQL Server instance" -ForegroundColor Red
    Write-Host "Please ensure SQL Server is running" -ForegroundColor Yellow
    exit 1
}

# Step 3: Ensure database exists
Write-Host "`nStep 3: Checking/creating ERPCore2 database..." -ForegroundColor Cyan
try {
    $dbCheckCmd = "sqlcmd -S `"$workingServer`" -E -Q `"SELECT name FROM sys.databases WHERE name = 'ERPCore2'`" -h -1"
    $dbExists = Invoke-Expression $dbCheckCmd 2>$null
    
    if (-not ($dbExists -and $dbExists.Trim() -eq "ERPCore2")) {
        Write-Host "[-] Creating ERPCore2 database..." -ForegroundColor Yellow
        $createDbCmd = "sqlcmd -S `"$workingServer`" -E -Q `"CREATE DATABASE ERPCore2`""
        Invoke-Expression $createDbCmd | Out-Null
    }
    Write-Host "[+] ERPCore2 database ready" -ForegroundColor Green
} catch {
    Write-Host "[-] Database creation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Update connection string
Write-Host "`nStep 4: Updating connection string..." -ForegroundColor Cyan
$appSettingsPath = "$erpInstallPath\appsettings.json"
if (Test-Path $appSettingsPath) {
    try {
        $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
        $connectionString = "Server=$workingServer;Database=ERPCore2;Integrated Security=true;TrustServerCertificate=true;"
        $appSettings.ConnectionStrings.DefaultConnection = $connectionString
        $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
        Write-Host "[+] Connection string updated for: $workingServer" -ForegroundColor Green
    } catch {
        Write-Host "[!] Failed to update connection string: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Step 5: Check tables and run migration if needed
Write-Host "`nStep 5: Checking database tables..." -ForegroundColor Cyan
$tableCount = Get-TableCount -server $workingServer

if ($tableCount -gt 0) {
    Write-Host "[+] Found $tableCount tables - database is ready" -ForegroundColor Green
} else {
    Write-Host "[!] No tables found - running database migration..." -ForegroundColor Yellow
    
    # Stop service if running
    try {
        Stop-Service -Name "ERPCore2" -Force -ErrorAction SilentlyContinue
    } catch { }
    
    # Run migration
    Write-Host "Starting migration process (this may take 2-3 minutes)..." -ForegroundColor Cyan
    
    try {
        $processStartInfo = New-Object System.Diagnostics.ProcessStartInfo
        $processStartInfo.FileName = $erpExePath
        $processStartInfo.WorkingDirectory = $erpInstallPath
        $processStartInfo.UseShellExecute = $false
        $processStartInfo.CreateNoWindow = $true
        $processStartInfo.RedirectStandardOutput = $true
        $processStartInfo.RedirectStandardError = $true
        
        $process = [System.Diagnostics.Process]::Start($processStartInfo)
        
        $timeout = 180  # 3 minutes
        $elapsed = 0
        $migrationCompleted = $false
        
        while (-not $process.HasExited -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 5
            $elapsed += 5
            
            if ($elapsed % 20 -eq 0) {
                Write-Host "... migration in progress ($elapsed/$timeout seconds)" -ForegroundColor Gray
            }
            
            # Check if tables have been created
            if ($elapsed -gt 30) {
                $newTableCount = Get-TableCount -server $workingServer
                if ($newTableCount -gt 0) {
                    Write-Host "[+] Migration completed! Found $newTableCount tables" -ForegroundColor Green
                    $migrationCompleted = $true
                    break
                }
            }
        }
        
        # Stop the process
        if (-not $process.HasExited) {
            $process.Kill()
            $process.WaitForExit(5000)
        }
        
        if (-not $migrationCompleted) {
            Write-Host "[!] Migration may have failed or timed out" -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "[-] Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 6: Start the service
Write-Host "`nStep 6: Starting ERP Core 2 service..." -ForegroundColor Cyan
try {
    $service = Get-Service -Name "ERPCore2" -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -ne 'Running') {
            Start-Service -Name "ERPCore2" -ErrorAction Stop
            Start-Sleep -Seconds 15
        }
        
        $serviceStatus = Get-Service -Name "ERPCore2"
        if ($serviceStatus.Status -eq 'Running') {
            Write-Host "[+] ERP Core 2 service is running!" -ForegroundColor Green
        } else {
            Write-Host "[!] Service status: $($serviceStatus.Status)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[!] ERPCore2 service not found" -ForegroundColor Yellow
        Write-Host "Please run the full deployment (INSTALL.bat) first" -ForegroundColor Gray
    }
} catch {
    Write-Host "[!] Service start failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 7: Final verification
Write-Host "`nStep 7: Final verification..." -ForegroundColor Cyan
$finalTableCount = Get-TableCount -server $workingServer
if ($finalTableCount -gt 0) {
    Write-Host "[+] Database has $finalTableCount tables ✓" -ForegroundColor Green
    
    # Test web response
    Write-Host "Testing web application..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:6011" -TimeoutSec 30 -UseBasicParsing -ErrorAction Stop
        Write-Host "[+] Web application is responding ✓" -ForegroundColor Green
    } catch {
        Write-Host "[!] Web application not responding yet" -ForegroundColor Yellow
        Write-Host "    Wait 1-2 minutes and try: http://localhost:6011" -ForegroundColor Gray
    }
    
    Write-Host "`n" + "="*50 -ForegroundColor Green
    Write-Host "SUCCESS! ERP Core 2 is ready for use" -ForegroundColor Green
    Write-Host "Access URL: http://localhost:6011" -ForegroundColor Cyan
    Write-Host "="*50 -ForegroundColor Green
    
} else {
    Write-Host "[-] Database setup incomplete" -ForegroundColor Red
    Write-Host "Please check the troubleshooting guide" -ForegroundColor Gray
}

Write-Host "`nSetup verification completed." -ForegroundColor White
