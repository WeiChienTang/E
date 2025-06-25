# Database Repair and Verification Script for ERP Core 2
# This script checks database status and fixes common issues

Write-Host "ERP Core 2 - Database Repair and Verification" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Step 1: Check if we're in the right location
$erpExePath = "C:\inetpub\ERPCore2\ERPCore2.exe"
if (-not (Test-Path $erpExePath)) {
    Write-Host "[-] ERPCore2.exe not found at: $erpExePath" -ForegroundColor Red
    Write-Host "Please ensure ERP Core 2 is properly deployed" -ForegroundColor Yellow
    exit 1
}

Write-Host "[+] Found ERP Core 2 installation" -ForegroundColor Green

# Step 2: Check SQL Server connectivity
Write-Host "`nTesting SQL Server connectivity..." -ForegroundColor Cyan
$sqlServers = @("localhost\SQLEXPRESS", "localhost", ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB")
$workingServer = $null

# Only use Windows Authentication
foreach ($server in $sqlServers) {
    try {
        $testCmd = "sqlcmd -S `"$server`" -E -Q `"SELECT 1`" -h -1"
        $result = Invoke-Expression $testCmd 2>$null
        if ($result -and $result.Trim() -eq "1") {
            Write-Host "[+] SQL Server connection successful with Windows Auth: $server" -ForegroundColor Green
            $workingServer = $server
            break
        }
    } catch {
        # Continue to next server
    }
}

if (-not $workingServer) {
    Write-Host "[-] Cannot connect to SQL Server" -ForegroundColor Red
    Write-Host "Please ensure SQL Server is running" -ForegroundColor Yellow
    exit 1
}

# Step 3: Check if ERPCore2 database exists
Write-Host "`nChecking ERPCore2 database..." -ForegroundColor Cyan
try {
    $dbCheckCmd = "sqlcmd -S `"$workingServer`" -E -Q `"SELECT name FROM sys.databases WHERE name = 'ERPCore2'`" -h -1"
    $dbExists = Invoke-Expression $dbCheckCmd 2>$null
    
    if ($dbExists -and $dbExists.Trim() -eq "ERPCore2") {
        Write-Host "[+] ERPCore2 database exists" -ForegroundColor Green
    } else {
        Write-Host "[-] ERPCore2 database not found, creating..." -ForegroundColor Yellow
        $createDbCmd = "sqlcmd -S `"$workingServer`" -E -Q `"CREATE DATABASE ERPCore2`""
        Invoke-Expression $createDbCmd
        Write-Host "[+] ERPCore2 database created" -ForegroundColor Green
    }
} catch {
    Write-Host "[-] Database check failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Check database tables
Write-Host "`nChecking database tables..." -ForegroundColor Cyan
try {
    $tableCountCmd = "sqlcmd -S `"$workingServer`" -E -d ERPCore2 -Q `"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'`" -h -1"
    $tableCount = Invoke-Expression $tableCountCmd 2>$null
    
    if ($tableCount -and [int]$tableCount.Trim() -gt 0) {
        Write-Host "[+] Found $($tableCount.Trim()) tables in database" -ForegroundColor Green
        
        # List tables
        $listTablesCmd = "sqlcmd -S `"$workingServer`" -E -d ERPCore2 -Q `"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME`" -h -1"
        $tables = Invoke-Expression $listTablesCmd 2>$null
        
        Write-Host "`nExisting tables:" -ForegroundColor White
        $tables.Split("`n") | Where-Object { $_.Trim() -ne "" } | ForEach-Object {
            Write-Host "  - $($_.Trim())" -ForegroundColor Gray
        }
    } else {
        Write-Host "[!] No tables found in database - need to run migration" -ForegroundColor Yellow
        
        # Step 5: Update connection string and run migration
        Write-Host "`nUpdating connection string..." -ForegroundColor Cyan
        $appSettingsPath = "C:\inetpub\ERPCore2\appsettings.json"
        
        if (Test-Path $appSettingsPath) {
            $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
            
            # Create Windows Authentication connection string
            $connectionString = "Server=$workingServer;Database=ERPCore2;Integrated Security=true;TrustServerCertificate=true;"
            Write-Host "[+] Using Windows Authentication connection string" -ForegroundColor Green
            
            $appSettings.ConnectionStrings.DefaultConnection = $connectionString
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
            Write-Host "[+] Connection string updated" -ForegroundColor Green
        }
        
        # Step 6: Run application to trigger migration
        Write-Host "`nRunning database migration..." -ForegroundColor Cyan
        Write-Host "This may take 1-2 minutes..." -ForegroundColor Yellow
        
        try {
            $processStartInfo = New-Object System.Diagnostics.ProcessStartInfo
            $processStartInfo.FileName = $erpExePath
            $processStartInfo.WorkingDirectory = "C:\inetpub\ERPCore2"
            $processStartInfo.UseShellExecute = $false
            $processStartInfo.CreateNoWindow = $true
            $processStartInfo.RedirectStandardOutput = $true
            $processStartInfo.RedirectStandardError = $true
            
            $process = [System.Diagnostics.Process]::Start($processStartInfo)
            
            $timeout = 120  # 2 minutes
            $elapsed = 0
            $migrationCompleted = $false
            $outputText = ""
            $errorText = ""
            
            while (-not $process.HasExited -and $elapsed -lt $timeout) {
                Start-Sleep -Seconds 5
                $elapsed += 5
                
                # Capture output for debugging
                try {
                    if (-not $process.StandardOutput.EndOfStream) {
                        $outputText += $process.StandardOutput.ReadToEnd()
                    }
                    if (-not $process.StandardError.EndOfStream) {
                        $errorText += $process.StandardError.ReadToEnd()
                    }
                } catch {
                    # Continue if output reading fails
                }
                
                # Check progress
                if ($elapsed % 15 -eq 0) {
                    Write-Host "... migration in progress ($elapsed/$timeout seconds)" -ForegroundColor Gray
                }
                
                # Check if tables have been created
                if ($elapsed -gt 20) {
                    try {
                        $checkCmd = "sqlcmd -S `"$workingServer`" -E -d ERPCore2 -Q `"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'`" -h -1"
                        $newTableCount = Invoke-Expression $checkCmd 2>$null
                        
                        if ($newTableCount -and [int]$newTableCount.Trim() -gt 0) {
                            Write-Host "[+] Migration completed! Found $($newTableCount.Trim()) tables" -ForegroundColor Green
                            $migrationCompleted = $true
                            break
                        }
                    } catch {
                        # Continue waiting
                    }
                }
            }
            
            # Stop the process
            if (-not $process.HasExited) {
                $process.Kill()
                $process.WaitForExit(5000)
            }
            
            # Capture any remaining output
            try {
                if (-not $process.StandardOutput.EndOfStream) {
                    $outputText += $process.StandardOutput.ReadToEnd()
                }
                if (-not $process.StandardError.EndOfStream) {
                    $errorText += $process.StandardError.ReadToEnd()
                }
            } catch {
                # Continue if output reading fails
            }
            
            if ($migrationCompleted) {
                Write-Host "[+] Database migration successful!" -ForegroundColor Green
            } else {
                Write-Host "[!] Migration may have failed or timed out" -ForegroundColor Yellow
                
                # Show detailed error information
                if ($outputText.Trim() -ne "") {
                    Write-Host "`nApplication Output:" -ForegroundColor Cyan
                    Write-Host $outputText -ForegroundColor Gray
                }
                
                if ($errorText.Trim() -ne "") {
                    Write-Host "`nApplication Errors:" -ForegroundColor Red
                    Write-Host $errorText -ForegroundColor Gray
                }
                
                # Additional diagnostic information
                Write-Host "`nDiagnostic Information:" -ForegroundColor Cyan
                Write-Host "- Check if the connection string is correct" -ForegroundColor White
                Write-Host "- Verify SQL Server permissions for the ERP user" -ForegroundColor White
                Write-Host "- Check Windows Event Viewer for detailed error logs" -ForegroundColor White
                Write-Host "- Ensure Entity Framework migrations are included in the deployment" -ForegroundColor White
            }
            
        } catch {
            Write-Host "[-] Migration failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "[-] Table check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 7: Final verification
Write-Host "`nFinal verification..." -ForegroundColor Cyan
try {
    $finalTableCountCmd = "sqlcmd -S `"$workingServer`" -E -d ERPCore2 -Q `"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'`" -h -1"
    $finalTableCount = Invoke-Expression $finalTableCountCmd 2>$null
    
    if ($finalTableCount -and [int]$finalTableCount.Trim() -gt 0) {
        Write-Host "[+] Database verification successful!" -ForegroundColor Green
        Write-Host "    Total tables: $($finalTableCount.Trim())" -ForegroundColor White
        
        # Try to start the service
        Write-Host "`nStarting ERP Core 2 service..." -ForegroundColor Cyan
        try {
            Start-Service -Name "ERPCore2" -ErrorAction Stop
            Start-Sleep -Seconds 10
            
            $serviceStatus = Get-Service -Name "ERPCore2"
            if ($serviceStatus.Status -eq 'Running') {
                Write-Host "[+] ERP Core 2 service is running!" -ForegroundColor Green
                Write-Host "`nYou can now access ERP Core 2 at:" -ForegroundColor Green
                Write-Host "http://localhost:6011" -ForegroundColor Cyan
            } else {
                Write-Host "[!] Service is not running. Status: $($serviceStatus.Status)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "[!] Could not start service: $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host "Try starting manually: Start-Service ERPCore2" -ForegroundColor Gray
        }
    } else {
        Write-Host "[-] Database still has no tables" -ForegroundColor Red
        Write-Host "Manual intervention may be required" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[-] Final verification failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nScript completed." -ForegroundColor Green
