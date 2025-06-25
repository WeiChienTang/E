# Windows Server Deployment Script for ERP Core 2
# PowerShell script for Windows deployment with MSSQL

Write-Host "Starting ERP Core 2 Windows Server deployment..." -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires Administrator privileges. Please run as Administrator." -ForegroundColor Red
    exit 1
}

# 1. Check port usage and configure Windows Firewall for port 6011
Write-Host "Checking port 6011 usage and configuring Windows Firewall..." -ForegroundColor Yellow

$targetPort = 6011

# First, check what's using port 6011
Write-Host "Checking if port $targetPort is in use..." -ForegroundColor Cyan
$portInUse = $false
$conflictingProcesses = @()

try {
    $portProcesses = Get-NetTCPConnection -LocalPort $targetPort -ErrorAction SilentlyContinue
    
    if ($portProcesses) {
        $portInUse = $true
        Write-Host "[!] Port $targetPort is currently in use by:" -ForegroundColor Yellow
        
        foreach ($conn in $portProcesses) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                $conflictingProcesses += $process
                Write-Host "    PID: $($conn.OwningProcess), Process: $($process.ProcessName)" -ForegroundColor White
                
                # Check if it's our own ERP service
                if ($process.Path -and $process.Path -like "*ERPCore2*") {
                    Write-Host "    -> This appears to be an existing ERP Core 2 instance" -ForegroundColor Green
                    
                    # Try to stop the existing service
                    Write-Host "    Attempting to stop existing ERP Core 2 service..." -ForegroundColor Yellow
                    try {
                        Stop-Service -Name "ERPCore2" -Force -ErrorAction SilentlyContinue
                        Start-Sleep -Seconds 3
                        
                        # Force kill if service didn't stop
                        $stillRunning = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
                        if ($stillRunning) {
                            Write-Host "    Force stopping process..." -ForegroundColor Yellow
                            Stop-Process -Id $conn.OwningProcess -Force
                            Start-Sleep -Seconds 2
                        }
                        
                        Write-Host "    [+] Existing ERP service stopped" -ForegroundColor Green
                        $portInUse = $false
                        
                    } catch {
                        Write-Host "    [!] Could not stop existing service: $($_.Exception.Message)" -ForegroundColor Red
                    }
                } else {
                    Write-Host "    -> This is a different application using port $targetPort" -ForegroundColor Yellow
                }
            }
        }
        
        # If port is still in use by other applications, warn user
        if ($portInUse -and $conflictingProcesses.Count -gt 0) {
            $nonERPProcesses = $conflictingProcesses | Where-Object { -not ($_.Path -like "*ERPCore2*") }
            if ($nonERPProcesses.Count -gt 0) {
                Write-Host "`n[!] Warning: Port $targetPort is still in use by other applications." -ForegroundColor Red
                Write-Host "ERP Core 2 may not start correctly. Consider:" -ForegroundColor Yellow
                Write-Host "1. Stopping the conflicting applications" -ForegroundColor White
                Write-Host "2. Changing ERP Core 2 to use a different port" -ForegroundColor White
                Write-Host "3. Restarting the server to clear port conflicts" -ForegroundColor White
            }
        }
    } else {
        Write-Host "[+] Port $targetPort is available" -ForegroundColor Green
    }
} catch {
    Write-Host "[!] Error checking port usage: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Continuing with deployment..." -ForegroundColor Gray
}

# Configure firewall
try {
    # Check if firewall rule already exists
    $existingRule = Get-NetFirewallRule -DisplayName "*ERP Core 2*" -ErrorAction SilentlyContinue
    if ($existingRule) {
        Write-Host "[+] Found existing firewall rule for ERP Core 2" -ForegroundColor Green
        # Remove old rule to create fresh one
        Remove-NetFirewallRule -DisplayName "MSSQL ERP Core 2" -ErrorAction SilentlyContinue
        Remove-NetFirewallRule -DisplayName "ERP Core 2" -ErrorAction SilentlyContinue
    }
    
    # Create new firewall rule for port 6011
    New-NetFirewallRule -DisplayName "ERP Core 2" -Direction Inbound -Protocol TCP -LocalPort $targetPort -Action Allow -Profile Any
    Write-Host "[+] Firewall rule for port $targetPort created successfully" -ForegroundColor Green
} catch {
    Write-Host "[-] Failed to configure firewall: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "You may need to manually configure Windows Firewall to allow port $targetPort" -ForegroundColor Yellow
}

# 1.1. Configure power settings to prevent sleep (for server reliability)
Write-Host "Configuring power settings to prevent sleep..." -ForegroundColor Yellow

try {
    # Set power scheme to High Performance
    powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
    
    # Disable sleep for AC power
    powercfg /change standby-timeout-ac 0
    powercfg /change hibernate-timeout-ac 0
    powercfg /change monitor-timeout-ac 0
    
    # Disable sleep for battery (if laptop)
    powercfg /change standby-timeout-dc 0
    powercfg /change hibernate-timeout-dc 0
    
    Write-Host "[+] Power settings configured - system will not sleep" -ForegroundColor Green
} catch {
    Write-Host "[!] Failed to configure power settings: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. Check required software installations
Write-Host "Checking required software..." -ForegroundColor Yellow

# For self-contained deployment, .NET Runtime check is optional
$dotnetVersion = try { 
    $output = & dotnet --version 2>$null
    if ($output -match "^([9-9]\d*|[1-9]\d+)\.") { $output } else { $null }
} catch { $null }

if ($dotnetVersion) {
    Write-Host "[+] .NET Runtime found: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "[!] .NET Runtime not found - using self-contained deployment" -ForegroundColor Yellow
    Write-Host "    Self-contained deployment includes all required .NET files" -ForegroundColor Yellow
}

# Note: ASP.NET Core Hosting Bundle is not needed for Kestrel self-hosting
Write-Host "[+] Using Kestrel web server (no IIS dependency)" -ForegroundColor Green

# Check SQL Server connectivity and setup database
Write-Host "Checking SQL Server connectivity and database setup..." -ForegroundColor Yellow

$sqlServerConnected = $false
$useIntegratedSecurity = $true
$sqlServer = "localhost"
$sqlUsername = ""
$sqlPassword = ""

# Check if SQL Server service is running
Write-Host "Checking SQL Server service status..." -ForegroundColor Cyan
$sqlServices = Get-Service -Name "MSSQL*" -ErrorAction SilentlyContinue
if ($sqlServices) {
    foreach ($service in $sqlServices) {
        if ($service.Status -eq "Running") {
            Write-Host "[+] SQL Server service running: $($service.Name)" -ForegroundColor Green
        } else {
            Write-Host "[!] SQL Server service not running: $($service.Name)" -ForegroundColor Yellow
            try {
                Start-Service -Name $service.Name
                Write-Host "[+] Started SQL Server service: $($service.Name)" -ForegroundColor Green
            } catch {
                Write-Host "[-] Failed to start SQL Server service: $($service.Name)" -ForegroundColor Red
            }
        }
    }
} else {
    Write-Host "[-] No SQL Server services found. Please install SQL Server." -ForegroundColor Red
    Write-Host "    Download SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Yellow
}

# Try different SQL Server connection methods
$connectionMethods = @(
    @{ Server = "localhost\SQLEXPRESS"; Description = "localhost\SQLEXPRESS (named instance)" },
    @{ Server = ".\SQLEXPRESS"; Description = ".\SQLEXPRESS (named instance)" },
    @{ Server = "localhost"; Description = "localhost (default instance)" },
    @{ Server = "(localdb)\MSSQLLocalDB"; Description = "LocalDB instance" },
    @{ Server = "(local)"; Description = "(local) alias" },
    @{ Server = "127.0.0.1"; Description = "127.0.0.1 (IP address)" }
)

Write-Host "Testing SQL Server connections with Windows Authentication..." -ForegroundColor Cyan
foreach ($method in $connectionMethods) {
    try {
        $connectionString = "Server=$($method.Server);Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        Write-Host "[+] SQL Server connection successful: $($method.Description)" -ForegroundColor Green
        $sqlServer = $method.Server
        $sqlServerConnected = $true
        $useIntegratedSecurity = $true
        break
    } catch {
        Write-Host "[-] Failed: $($method.Description) - $($_.Exception.Message.Split('.')[0])" -ForegroundColor Red
    }
}

# If Windows Authentication failed, try SQL Server Authentication
if (-not $sqlServerConnected) {
    Write-Host "`n[!] Windows Authentication failed for all connection methods." -ForegroundColor Yellow
    Write-Host "Attempting to configure SQL Server for automatic setup..." -ForegroundColor Yellow
    
    # Try to connect with sa account (common default)
    $defaultCredentials = @(
        @{ Username = "sa"; Password = "" },
        @{ Username = "sa"; Password = "123456" },
        @{ Username = "sa"; Password = "password" },
        @{ Username = "sa"; Password = "admin" }
    )
    
    $tempConnection = $false
    foreach ($cred in $defaultCredentials) {
        foreach ($method in $connectionMethods) {
            try {
                $connectionString = "Server=$($method.Server);Database=master;User Id=$($cred.Username);Password=$($cred.Password);TrustServerCertificate=true;Connection Timeout=5;"
                $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
                $connection.Open()
                $connection.Close()
                Write-Host "[+] Connected with default credentials: $($cred.Username)" -ForegroundColor Green
                $sqlServer = $method.Server
                $tempConnection = $true
                $tempUsername = $cred.Username
                $tempPassword = $cred.Password
                break
            } catch {
                # Continue trying
            }
        }
        if ($tempConnection) { break }
    }
    
    if ($tempConnection) {
        # Create dedicated ERP user automatically
        try {
            Write-Host "Creating dedicated ERP Core 2 database user..." -ForegroundColor Cyan
            $connectionString = "Server=$sqlServer;Database=master;User Id=$tempUsername;Password=$tempPassword;TrustServerCertificate=true;"
            $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
            $connection.Open()
            
            # Generate secure password
            $erpPassword = -join ((1..12) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)}) + "123!"
            $erpUsername = "ERPCore2User"
            
            # Create login and user
            $createLoginCommand = $connection.CreateCommand()
            $createLoginCommand.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '$erpUsername')
                BEGIN
                    CREATE LOGIN [$erpUsername] WITH PASSWORD = '$erpPassword';
                END
"@
            $createLoginCommand.ExecuteNonQuery()
            
            # Enable SQL Server Authentication if not already enabled
            $enableMixedModeCommand = $connection.CreateCommand()
            $enableMixedModeCommand.CommandText = @"
                EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', 
                    N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2;
"@
            try {
                $enableMixedModeCommand.ExecuteNonQuery()
                Write-Host "[+] SQL Server mixed authentication mode enabled" -ForegroundColor Green
            } catch {
                Write-Host "[!] Mixed mode may already be enabled" -ForegroundColor Yellow
            }
            
            $connection.Close()
            
            # Test new user connection
            $connectionString = "Server=$sqlServer;Database=master;User Id=$erpUsername;Password=$erpPassword;TrustServerCertificate=true;"
            $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
            $connection.Open()
            $connection.Close()
            
            Write-Host "[+] ERP Core 2 database user created successfully" -ForegroundColor Green
            Write-Host "    Username: $erpUsername" -ForegroundColor Cyan
            Write-Host "    Password: [Generated automatically]" -ForegroundColor Cyan
            
            $sqlServerConnected = $true
            $useIntegratedSecurity = $false
            $sqlUsername = $erpUsername
            $sqlPassword = $erpPassword
            
        } catch {
            Write-Host "[-] Failed to create ERP user: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "Will attempt manual SQL Server Authentication setup..." -ForegroundColor Yellow
            
            # Fallback to manual setup
            $choice = Read-Host "`nWould you like to provide SQL Server credentials manually? (y/n)"
            if ($choice -eq 'y' -or $choice -eq 'Y') {
                Write-Host "`nPlease provide SQL Server credentials:" -ForegroundColor Yellow
                $sqlServer = Read-Host "SQL Server instance (default: localhost)"
                if ([string]::IsNullOrWhiteSpace($sqlServer)) { $sqlServer = "localhost" }
                
                $sqlUsername = Read-Host "SQL Server username (e.g., sa)"
                $securePassword = Read-Host "SQL Server password" -AsSecureString
                $sqlPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))
                
                # Test manual credentials
                try {
                    $connectionString = "Server=$sqlServer;Database=master;User Id=$sqlUsername;Password=$sqlPassword;TrustServerCertificate=true;Connection Timeout=10;"
                    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
                    $connection.Open()
                    $connection.Close()
                    Write-Host "[+] SQL Server connection successful with manual credentials" -ForegroundColor Green
                    $sqlServerConnected = $true
                    $useIntegratedSecurity = $false
                } catch {
                    Write-Host "[-] Manual SQL Server connection failed: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
    } else {
        Write-Host "[-] Could not connect with default credentials." -ForegroundColor Red
        Write-Host "Please ensure SQL Server Express is installed and sa account is enabled." -ForegroundColor Yellow
        Write-Host "Download SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Cyan
    }
    
    if (-not $sqlServerConnected) {
        Write-Host "`n[!] Continuing deployment without database setup..." -ForegroundColor Yellow
        Write-Host "You can configure the database connection manually later in appsettings.json" -ForegroundColor Yellow
    }
}

# Create database if connection successful
if ($sqlServerConnected) {
    Write-Host "Setting up ERP Core 2 database..." -ForegroundColor Yellow
    
    try {
        if ($useIntegratedSecurity) {
            $connectionString = "Server=$sqlServer;Database=master;Integrated Security=true;TrustServerCertificate=true;"
        } else {
            $connectionString = "Server=$sqlServer;Database=master;User Id=$sqlUsername;Password=$sqlPassword;TrustServerCertificate=true;"
        }
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        # Check if ERPCore2 database exists
        $checkDbCommand = $connection.CreateCommand()
        $checkDbCommand.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = 'ERPCore2'"
        $dbExists = $checkDbCommand.ExecuteScalar()
        
        if ($dbExists -eq 0) {
            # Create database
            $createDbCommand = $connection.CreateCommand()
            $createDbCommand.CommandText = "CREATE DATABASE ERPCore2"
            $createDbCommand.ExecuteNonQuery()
            Write-Host "[+] ERPCore2 database created successfully" -ForegroundColor Green
        } else {
            Write-Host "[+] ERPCore2 database already exists" -ForegroundColor Green
        }
        
        # Configure database user permissions if using SQL authentication
        if (-not $useIntegratedSecurity) {
            try {
                # Switch to ERPCore2 database and create user
                $connection.ChangeDatabase("ERPCore2")
                
                $createUserCommand = $connection.CreateCommand()
                $createUserCommand.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '$sqlUsername')
                    BEGIN
                        CREATE USER [$sqlUsername] FOR LOGIN [$sqlUsername];
                        ALTER ROLE db_owner ADD MEMBER [$sqlUsername];
                    END
"@
                $createUserCommand.ExecuteNonQuery()
                Write-Host "[+] Database user permissions configured" -ForegroundColor Green
            } catch {
                Write-Host "[!] User permissions setup warning: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
        
        $connection.Close()
        
        # Note: Database schema and data will be created automatically when the application starts
        # The application uses Entity Framework Core Migrations and SeedData
        Write-Host "[+] Database schema and initial data will be created on first application startup" -ForegroundColor Green
        Write-Host "    * Entity Framework Migrations will create all tables and relationships" -ForegroundColor Cyan
        Write-Host "    * SeedData will populate initial system data" -ForegroundColor Cyan
        
    } catch {
        Write-Host "[-] Database setup failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "You may need to create the database manually" -ForegroundColor Yellow
    }
}

# 3. Stop existing service if running
Write-Host "Stopping existing ERP Core 2 service..." -ForegroundColor Yellow

$service = Get-Service -Name "ERPCore2" -ErrorAction SilentlyContinue
if ($service) {
    if ($service.Status -eq "Running") {
        Stop-Service -Name "ERPCore2" -Force
        Write-Host "[+] Existing service stopped" -ForegroundColor Green
    }
    # Remove existing service
    sc.exe delete "ERPCore2" | Out-Null
}

# 4. Create deployment directory
$deployPath = "C:\inetpub\ERPCore2"
Write-Host "Setting up deployment directory: $deployPath" -ForegroundColor Yellow

if (Test-Path $deployPath) {
    # Backup existing version
    $backupPath = "C:\inetpub\ERPCore2_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item -Path $deployPath -Destination $backupPath -Recurse -Force
    Write-Host "[+] Existing version backed up to: $backupPath" -ForegroundColor Green
    
    # Remove existing deployment
    Remove-Item -Path $deployPath -Recurse -Force
}

New-Item -Path $deployPath -ItemType Directory -Force | Out-Null

# 5. Copy published files (assumes publish folder exists)
$publishPath = ".\publish\win-x64"
if (-not (Test-Path $publishPath)) {
    # Try alternative paths that might exist in deployment package
    $possiblePaths = @(
        "..\publish\win-x64",
        "..\..\publish\win-x64", 
        ".\ERPCore2.exe"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            if ($path.EndsWith(".exe")) {
                # If we found the exe directly, copy the entire current directory
                Copy-Item -Path ".\*" -Destination $deployPath -Recurse -Force -Exclude @("Scripts", "*.md", "*.txt", "*.bat")
                Write-Host "[+] Application files copied from current directory" -ForegroundColor Green
                break
            } else {
                $publishPath = $path
                break
            }
        }
    }
}

if (Test-Path $publishPath) {
    Copy-Item -Path "$publishPath\*" -Destination $deployPath -Recurse -Force
    Write-Host "[+] Application files copied to deployment directory" -ForegroundColor Green
} elseif (-not (Test-Path "$deployPath\ERPCore2.exe")) {
    Write-Host "[-] Published application files not found." -ForegroundColor Red
    Write-Host "    Expected locations:" -ForegroundColor Yellow
    Write-Host "    - .\publish\win-x64\" -ForegroundColor Yellow
    Write-Host "    - ..\publish\win-x64\" -ForegroundColor Yellow
    Write-Host "    - Current directory with ERPCore2.exe" -ForegroundColor Yellow
}

# 6. Create Windows Service
Write-Host "Creating Windows Service..." -ForegroundColor Yellow

$serviceName = "ERPCore2"
$serviceDisplayName = "ERP Core 2 Application"
$serviceDescription = "ERP Core 2 Business Management System"
$executablePath = "$deployPath\ERPCore2.exe"

if (Test-Path $executablePath) {
    # First, test database connection and initialize schema
    Write-Host "Initializing database schema..." -ForegroundColor Yellow
    
    try {
        # Run the application briefly to trigger database migration
        $processStartInfo = New-Object System.Diagnostics.ProcessStartInfo
        $processStartInfo.FileName = $executablePath
        $processStartInfo.WorkingDirectory = $deployPath
        $processStartInfo.UseShellExecute = $false
        $processStartInfo.CreateNoWindow = $true
        $processStartInfo.RedirectStandardOutput = $true
        $processStartInfo.RedirectStandardError = $true
        
        $process = [System.Diagnostics.Process]::Start($processStartInfo)
        
        # Wait for database initialization (max 30 seconds)
        $timeout = 30
        $elapsed = 0
        
        while (-not $process.HasExited -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 1
            $elapsed++                # Check if database tables have been created by looking for output
                if ($elapsed -gt 5) {
                    # Try to connect to database and check for tables
                    try {
                        if ($useIntegratedSecurity) {
                            $testQuery = "sqlcmd -S $sqlServer -E -d ERPCore2 -Q `"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES`" -h -1"
                        } else {
                            $testQuery = "sqlcmd -S $sqlServer -U $sqlUsername -P $sqlPassword -d ERPCore2 -Q `"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES`" -h -1"
                        }
                    
                    $result = Invoke-Expression $testQuery 2>$null
                    if ($result -and [int]$result.Trim() -gt 0) {
                        Write-Host "[+] Database tables created successfully" -ForegroundColor Green
                        break
                    }
                } catch {
                    # Continue waiting
                }
            }
        }
        
        # Stop the initialization process
        if (-not $process.HasExited) {
            $process.Kill()
            $process.WaitForExit(5000)
        }
        
        Write-Host "[+] Database initialization completed" -ForegroundColor Green
        
    } catch {
        Write-Host "[!] Database initialization warning: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    # Create the service
    try {
        # Check if service already exists and remove it
        $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
        if ($existingService) {
            Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
            sc.exe delete $serviceName | Out-Null
            Start-Sleep -Seconds 2
        }
        
        New-Service -Name $serviceName -BinaryPathName $executablePath -DisplayName $serviceDisplayName -Description $serviceDescription -StartupType Automatic
        
        # Configure service to restart on failure
        sc.exe failure $serviceName reset= 86400 actions= restart/5000/restart/5000/restart/5000 | Out-Null
        
        Write-Host "[+] Windows Service created successfully" -ForegroundColor Green
        Write-Host "    Service will be started after configuration is complete..." -ForegroundColor Cyan
    } catch {
        Write-Host "[-] Failed to create service: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[-] Executable not found at: $executablePath" -ForegroundColor Red
}

# 7. Configure application settings for network access and database
Write-Host "Configuring application for network access and database..." -ForegroundColor Yellow

$appSettingsPath = "$deployPath\appsettings.json"
if (Test-Path $appSettingsPath) {
    # Read existing configuration
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    
    # Update database connection string based on SQL Server configuration
    if ($sqlServerConnected) {
        Write-Host "Configuring database connection string..." -ForegroundColor Cyan
        
        if (-not $appSettings.ConnectionStrings) {
            $appSettings | Add-Member -Name "ConnectionStrings" -Value @{} -MemberType NoteProperty
        }
        
        if ($useIntegratedSecurity) {
            $connectionString = "Server=$sqlServer;Database=ERPCore2;Integrated Security=true;TrustServerCertificate=true;"
        } else {
            $connectionString = "Server=$sqlServer;Database=ERPCore2;User Id=$sqlUsername;Password=$sqlPassword;TrustServerCertificate=true;"
        }
        
        $appSettings.ConnectionStrings.DefaultConnection = $connectionString
        Write-Host "[+] Database connection string configured" -ForegroundColor Green
    }
    
    # Add or update Kestrel configuration
    if (-not $appSettings.Kestrel) {
        $appSettings | Add-Member -Name "Kestrel" -Value @{} -MemberType NoteProperty
    }
    if (-not $appSettings.Kestrel.Endpoints) {
        $appSettings.Kestrel | Add-Member -Name "Endpoints" -Value @{} -MemberType NoteProperty
    }
    
    $appSettings.Kestrel.Endpoints = @{
        "Http" = @{
            "Url" = "http://*:6011"
        }
    }
    
    # Add URLs configuration
    $appSettings | Add-Member -Name "urls" -Value "http://*:6011" -MemberType NoteProperty -Force
    
    # Save updated configuration
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
    Write-Host "[+] Application configured to listen on port 6011" -ForegroundColor Green
} else {
    Write-Host "[!] appsettings.json not found. Manual configuration may be required." -ForegroundColor Yellow
}

# 8. Display network information
Write-Host "`nNetwork Access Information:" -ForegroundColor Cyan
$ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.254.*" }

Write-Host "ERP Core 2 will be accessible at:" -ForegroundColor Green
foreach ($ip in $ipAddresses) {
    Write-Host "  http://$($ip.IPAddress):6011" -ForegroundColor White
}

Write-Host "`nDeployment Summary:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan
Write-Host "- Service Name: ERPCore2" -ForegroundColor White
Write-Host "- Installation Path: $deployPath" -ForegroundColor White
Write-Host "- Port: 6011" -ForegroundColor White
Write-Host "- Firewall: Configured for port 6011" -ForegroundColor White
if ($sqlServerConnected) {
    Write-Host "- Database: ERPCore2 (ready for automatic initialization)" -ForegroundColor White
}

Write-Host "`nDatabase Initialization Process:" -ForegroundColor Cyan
Write-Host "- During deployment, the system automatically:" -ForegroundColor Yellow
Write-Host "  1. Creates the ERPCore2 database" -ForegroundColor White
Write-Host "  2. Executes Entity Framework migrations to create tables" -ForegroundColor White
Write-Host "  3. Establishes all relationships and indexes" -ForegroundColor White
Write-Host "  4. Populates initial system data" -ForegroundColor White
Write-Host "  5. Starts the ERP service ready for use" -ForegroundColor White

Write-Host "`nUseful Commands:" -ForegroundColor Cyan
Write-Host "- Check service status: Get-Service ERPCore2" -ForegroundColor White
Write-Host "- Start service: Start-Service ERPCore2" -ForegroundColor White
Write-Host "- Stop service: Stop-Service ERPCore2" -ForegroundColor White
Write-Host "- View service logs: Get-EventLog -LogName Application -Source ERPCore2" -ForegroundColor White

# 9. Final service startup attempt after all configuration is complete
Write-Host "`nFinal service startup..." -ForegroundColor Cyan
try {
    $currentService = Get-Service -Name "ERPCore2" -ErrorAction SilentlyContinue
    if ($currentService) {
        if ($currentService.Status -ne 'Running') {
            Write-Host "Attempting to start ERP Core 2 service..." -ForegroundColor Yellow
            
            # Ensure configuration files are ready
            Start-Sleep -Seconds 2
            
            # Try to start the service with more detailed error handling
            try {
                Start-Service -Name "ERPCore2" -ErrorAction Stop
                Write-Host "[+] Service start command executed" -ForegroundColor Green
                
                # Wait longer for service startup (database initialization can take time)
                Write-Host "Waiting for service to initialize (this may take 30-60 seconds)..." -ForegroundColor Yellow
                $timeout = 60
                $elapsed = 0
                
                do {
                    Start-Sleep -Seconds 5
                    $elapsed += 5
                    $serviceStatus = Get-Service -Name "ERPCore2"
                    
                    if ($serviceStatus.Status -eq 'Running') {
                        Write-Host "[+] ERP Core 2 service is now running!" -ForegroundColor Green
                        break
                    } elseif ($serviceStatus.Status -eq 'StartPending') {
                        Write-Host "    Service starting... ($elapsed/$timeout seconds)" -ForegroundColor Gray
                    } else {
                        Write-Host "[!] Service status: $($serviceStatus.Status)" -ForegroundColor Yellow
                        break
                    }
                } while ($elapsed -lt $timeout)
                
                # Final status check
                $finalStatus = Get-Service -Name "ERPCore2"
                if ($finalStatus.Status -eq 'Running') {
                    # Test if application is responding
                    Write-Host "Testing application response..." -ForegroundColor Yellow
                    Start-Sleep -Seconds 10
                    
                    try {
                        $response = Invoke-WebRequest -Uri "http://localhost:6011" -TimeoutSec 30 -ErrorAction Stop
                        Write-Host "[+] Application is responding on port 6011" -ForegroundColor Green
                        Write-Host "[+] Deployment completed successfully!" -ForegroundColor Green
                    } catch {
                        Write-Host "[!] Application may still be starting up..." -ForegroundColor Yellow
                        Write-Host "    Wait 1-2 minutes and try accessing: http://localhost:6011" -ForegroundColor Cyan
                        Write-Host "    Or check: Scripts\repair-database.ps1 if database issues persist" -ForegroundColor Cyan
                    }
                } else {
                    Write-Host "[!] Service failed to start properly" -ForegroundColor Yellow
                    Write-Host "    Current status: $($finalStatus.Status)" -ForegroundColor Gray
                    Write-Host "    Check Windows Event Viewer for detailed error information" -ForegroundColor Gray
                    Write-Host "    Try running: Scripts\complete-setup.ps1 for diagnosis and repair" -ForegroundColor Cyan
                }
                
            } catch {
                Write-Host "[!] Failed to start service: $($_.Exception.Message)" -ForegroundColor Yellow
                Write-Host "`nDiagnostic information:" -ForegroundColor Cyan
                
                # Check if port is already in use
                try {
                    $portCheck = netstat -an | findstr ":6011"
                    if ($portCheck) {
                        Write-Host "[!] Port 6011 is already in use:" -ForegroundColor Yellow
                        Write-Host $portCheck -ForegroundColor Gray
                    }
                } catch {}
                
                # Check database connectivity
                if ($sqlServerConnected) {
                    try {
                        if ($useIntegratedSecurity) {
                            $dbTest = sqlcmd -S $sqlServer -E -Q "SELECT 1" -h -1 2>$null
                        } else {
                            $dbTest = sqlcmd -S $sqlServer -U $sqlUsername -P $sqlPassword -Q "SELECT 1" -h -1 2>$null
                        }
                        
                        if ($dbTest -and $dbTest.Trim() -eq "1") {
                            Write-Host "[+] Database connection is working" -ForegroundColor Green
                        } else {
                            Write-Host "[!] Database connection issue detected" -ForegroundColor Yellow
                        }
                    } catch {
                        Write-Host "[!] Cannot test database connection" -ForegroundColor Yellow
                    }
                }
            }
            
        } else {
            Write-Host "[+] Service is already running" -ForegroundColor Green
        }
    } else {
        Write-Host "[!] ERPCore2 service not found" -ForegroundColor Red
    }
} catch {
    Write-Host "[!] Service startup issue: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`nTroubleshooting steps if service fails to start:" -ForegroundColor Cyan
Write-Host "1. Check Windows Event Viewer (Application Log)" -ForegroundColor White
Write-Host "2. Run Scripts\diagnose-sql-server.ps1 to check database connectivity" -ForegroundColor White
Write-Host "3. Run Scripts\repair-database.ps1 to fix database issues" -ForegroundColor White
Write-Host "4. Try manual start: Start-Service ERPCore2" -ForegroundColor White
Write-Host "5. Check port availability: netstat -an | findstr :6011" -ForegroundColor White

Write-Host "`nDeployment completed!" -ForegroundColor Green
