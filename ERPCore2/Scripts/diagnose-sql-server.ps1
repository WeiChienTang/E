# SQL Server Diagnostics and Connection Test Script
# This script helps diagnose SQL Server connection issues

Write-Host "SQL Server Diagnostics and Connection Test" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Green

# Step 1: Check SQL Server services
Write-Host "`n1. Checking SQL Server services..." -ForegroundColor Cyan
$sqlServices = Get-Service -Name "*SQL*" -ErrorAction SilentlyContinue
if ($sqlServices) {
    Write-Host "[+] SQL Server services found:" -ForegroundColor Green
    foreach ($service in $sqlServices) {
        $status = if ($service.Status -eq 'Running') { "[OK]" } else { "[!]" }
        Write-Host "    $status $($service.Name) - $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') { 'Green' } else { 'Yellow' })
    }
} else {
    Write-Host "[-] No SQL Server services found" -ForegroundColor Red
    Write-Host "    SQL Server may not be installed" -ForegroundColor Yellow
    exit 1
}

# Step 2: Check SQL Server instances
Write-Host "`n2. Checking SQL Server instances..." -ForegroundColor Cyan
try {
    $instances = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server" -Name InstalledInstances -ErrorAction SilentlyContinue
    if ($instances -and $instances.InstalledInstances) {
        Write-Host "[+] SQL Server instances found:" -ForegroundColor Green
        foreach ($instance in $instances.InstalledInstances) {
            Write-Host "    - $instance" -ForegroundColor White
        }
    } else {
        Write-Host "[!] No instances found in registry" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[!] Could not read registry: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 3: Test sqlcmd availability
Write-Host "`n3. Testing sqlcmd availability..." -ForegroundColor Cyan
try {
    $sqlcmdTest = sqlcmd -? 2>$null
    Write-Host "[+] sqlcmd is available" -ForegroundColor Green
} catch {
    Write-Host "[-] sqlcmd not found" -ForegroundColor Red
    Write-Host "    This may cause connection test failures" -ForegroundColor Yellow
}

# Step 4: Test connection methods
Write-Host "`n4. Testing SQL Server connections..." -ForegroundColor Cyan
$connectionMethods = @(
    @{ Server = "localhost\SQLEXPRESS"; Method = "Windows Auth"; Type = "sqlcmd" },
    @{ Server = ".\SQLEXPRESS"; Method = "Windows Auth"; Type = "sqlcmd" },
    @{ Server = "localhost"; Method = "Windows Auth"; Type = "sqlcmd" },
    @{ Server = "(localdb)\MSSQLLocalDB"; Method = "Windows Auth"; Type = "sqlcmd" },
    @{ Server = "localhost\SQLEXPRESS"; Method = "Windows Auth"; Type = "dotnet" },
    @{ Server = ".\SQLEXPRESS"; Method = "Windows Auth"; Type = "dotnet" }
)

$workingConnections = @()

foreach ($conn in $connectionMethods) {
    Write-Host "`nTesting: $($conn.Server) ($($conn.Type))..." -ForegroundColor Gray
    
    $success = $false
    try {
        if ($conn.Type -eq "sqlcmd") {
            $testCmd = "sqlcmd -S `"$($conn.Server)`" -E -Q `"SELECT 'Connected' as Status`" -h -1"
            $result = Invoke-Expression $testCmd 2>$null
            if ($result -and $result.Trim() -eq "Connected") {
                $success = $true
            }
        } elseif ($conn.Type -eq "dotnet") {
            $connectionString = "Server=$($conn.Server);Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5;"
            $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
            $connection.Open()
            $connection.Close()
            $success = $true
        }
    } catch {
        # Connection failed
    }
    
    if ($success) {
        Write-Host "[+] SUCCESS: $($conn.Server)" -ForegroundColor Green
        $workingConnections += $conn
    } else {
        Write-Host "[-] FAILED: $($conn.Server)" -ForegroundColor Red
    }
}

# Step 5: Protocol and configuration checks
Write-Host "`n5. Checking SQL Server configuration..." -ForegroundColor Cyan

# Check if TCP/IP is enabled
try {
    $tcpEnabled = $false
    $namedPipesEnabled = $false
    
    # Try to check via registry (SQL Express typical path)
    $sqlExpressPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL*\MSSQLServer\SuperSocketNetLib\Tcp"
    $regPaths = Get-ItemProperty -Path $sqlExpressPath -ErrorAction SilentlyContinue 2>$null
    
    Write-Host "    Network protocols status:" -ForegroundColor White
    Write-Host "    - TCP/IP: Unknown (requires SQL Server Configuration Manager)" -ForegroundColor Gray
    Write-Host "    - Named Pipes: Unknown (requires SQL Server Configuration Manager)" -ForegroundColor Gray
} catch {
    Write-Host "    - Could not determine protocol status" -ForegroundColor Yellow
}

# Step 6: Firewall check
Write-Host "`n6. Checking firewall rules..." -ForegroundColor Cyan
try {
    $firewallRules = Get-NetFirewallRule -DisplayName "*SQL*" -ErrorAction SilentlyContinue
    if ($firewallRules) {
        Write-Host "[+] SQL Server firewall rules found:" -ForegroundColor Green
        foreach ($rule in $firewallRules) {
            Write-Host "    - $($rule.DisplayName) ($($rule.Enabled))" -ForegroundColor White
        }
    } else {
        Write-Host "[!] No SQL Server firewall rules found" -ForegroundColor Yellow
        Write-Host "    This may block remote connections" -ForegroundColor Gray
    }
} catch {
    Write-Host "[!] Could not check firewall rules: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 7: Summary and recommendations
Write-Host "`n7. Summary and Recommendations" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

if ($workingConnections.Count -gt 0) {
    Write-Host "`n[+] WORKING CONNECTIONS FOUND:" -ForegroundColor Green
    foreach ($conn in $workingConnections) {
        Write-Host "    [OK] $($conn.Server) ($($conn.Type))" -ForegroundColor Green
    }
    
    $recommended = $workingConnections[0]
    Write-Host "`n[RECOMMENDED] Use this connection string:" -ForegroundColor Cyan
    Write-Host "Server=$($recommended.Server);Database=ERPCore2;Integrated Security=true;TrustServerCertificate=true;" -ForegroundColor Yellow
} else {
    Write-Host "`n[-] NO WORKING CONNECTIONS FOUND" -ForegroundColor Red
    Write-Host "`nTroubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Ensure SQL Server services are running:" -ForegroundColor White
    Write-Host "   - Start-Service 'MSSQL`$SQLEXPRESS'" -ForegroundColor Gray
    Write-Host "   - Start-Service 'SQLBrowser'" -ForegroundColor Gray
    Write-Host "2. Enable TCP/IP protocol in SQL Server Configuration Manager" -ForegroundColor White
    Write-Host "3. Enable Named Pipes protocol in SQL Server Configuration Manager" -ForegroundColor White
    Write-Host "4. Restart SQL Server services after protocol changes" -ForegroundColor White
    Write-Host "5. Check Windows Firewall settings" -ForegroundColor White
    Write-Host "6. Verify SQL Server is configured for Windows Authentication" -ForegroundColor White
}

Write-Host "`nDiagnostics completed." -ForegroundColor Green
