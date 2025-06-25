# Port Management and Service Conflict Resolution Script
# This script helps identify and resolve port conflicts for ERP Core 2

Write-Host "ERP Core 2 - Port Conflict Resolution" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

$targetPort = 6011

# Check what's using port 6011
Write-Host "`nStep 1: Checking port $targetPort usage..." -ForegroundColor Cyan
try {
    $portProcesses = Get-NetTCPConnection -LocalPort $targetPort -ErrorAction SilentlyContinue
    
    if ($portProcesses) {
        Write-Host "Port $targetPort is currently in use by:" -ForegroundColor Yellow
        
        foreach ($conn in $portProcesses) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  PID: $($conn.OwningProcess), Process: $($process.ProcessName), Path: $($process.Path)" -ForegroundColor White
                
                # Check if it's our own ERP service
                if ($process.Path -and $process.Path -like "*ERPCore2*") {
                    Write-Host "  -> This appears to be an existing ERP Core 2 instance" -ForegroundColor Green
                    
                    # Try to stop the existing service
                    Write-Host "Attempting to stop existing ERP Core 2 service..." -ForegroundColor Yellow
                    try {
                        Stop-Service -Name "ERPCore2" -Force -ErrorAction SilentlyContinue
                        Start-Sleep -Seconds 5
                        
                        # Force kill if service didn't stop
                        $stillRunning = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
                        if ($stillRunning) {
                            Write-Host "Force stopping process..." -ForegroundColor Yellow
                            Stop-Process -Id $conn.OwningProcess -Force
                            Start-Sleep -Seconds 3
                        }
                        
                        Write-Host "[+] Existing ERP service stopped" -ForegroundColor Green
                        
                    } catch {
                        Write-Host "[!] Could not stop existing service: $($_.Exception.Message)" -ForegroundColor Red
                    }
                } else {
                    Write-Host "  -> This is a different application using port $targetPort" -ForegroundColor Yellow
                    Write-Host "     You may need to:" -ForegroundColor Cyan
                    Write-Host "     1. Stop this process manually" -ForegroundColor White
                    Write-Host "     2. Change ERP Core 2 to use a different port" -ForegroundColor White
                    Write-Host "     3. Configure the other application to use a different port" -ForegroundColor White
                }
            }
        }
    } else {
        Write-Host "[+] Port $targetPort is available" -ForegroundColor Green
    }
} catch {
    Write-Host "[!] Error checking port usage: $($_.Exception.Message)" -ForegroundColor Red
}

# Check ERP Core 2 service status
Write-Host "`nStep 2: Checking ERP Core 2 service..." -ForegroundColor Cyan
try {
    $erpService = Get-Service -Name "ERPCore2" -ErrorAction SilentlyContinue
    if ($erpService) {
        Write-Host "Service Status: $($erpService.Status)" -ForegroundColor White
        
        if ($erpService.Status -eq 'Running') {
            Write-Host "Service is running but may be using the port from a previous deployment" -ForegroundColor Yellow
        } elseif ($erpService.Status -eq 'Stopped') {
            Write-Host "Service is stopped - ready for restart" -ForegroundColor Green
        }
    } else {
        Write-Host "ERP Core 2 service not found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[!] Error checking service: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Check firewall rules and ask user about configuration
Write-Host "`nStep 3: Checking firewall configuration..." -ForegroundColor Cyan
$existingRule = Get-NetFirewallRule -DisplayName "*ERP Core 2*" -ErrorAction SilentlyContinue
if ($existingRule) {
    Write-Host "[+] Found existing firewall rule for ERP Core 2" -ForegroundColor Green
    foreach ($rule in $existingRule) {
        $portFilter = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $rule -ErrorAction SilentlyContinue
        if ($portFilter) {
            Write-Host "    Rule: $($rule.DisplayName), Port: $($portFilter.LocalPort), Direction: $($rule.Direction)" -ForegroundColor White
        }
    }
} else {
    Write-Host "[!] No existing firewall rule found for ERP Core 2" -ForegroundColor Yellow
    
    # Ask user if they want to create firewall rule
    Write-Host "`nDo you want to create a firewall rule for port $targetPort?" -ForegroundColor Cyan
    Write-Host "This will allow external access to the ERP Core 2 application." -ForegroundColor Yellow
    $createRule = Read-Host "Create firewall rule? (Y/N)"
    
    if ($createRule -eq 'Y' -or $createRule -eq 'y' -or $createRule -eq 'Yes' -or $createRule -eq 'yes') {
        try {
            # Remove any existing rule with the same name
            Remove-NetFirewallRule -DisplayName "ERP Core 2" -ErrorAction SilentlyContinue
            
            # Create new firewall rule
            New-NetFirewallRule -DisplayName "ERP Core 2" -Direction Inbound -Protocol TCP -LocalPort $targetPort -Action Allow -Profile Any
            Write-Host "[+] Firewall rule created successfully for port $targetPort" -ForegroundColor Green
        } catch {
            Write-Host "[-] Failed to create firewall rule: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "[!] Firewall rule not created. ERP Core 2 may only be accessible locally." -ForegroundColor Yellow
    }
}

# Provide recommendations
Write-Host "`nStep 4: Recommendations..." -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

$portStillInUse = Get-NetTCPConnection -LocalPort $targetPort -ErrorAction SilentlyContinue
if (-not $portStillInUse) {
    Write-Host "[+] Port is now available - you can restart the ERP service" -ForegroundColor Green
    Write-Host "Run: Start-Service ERPCore2" -ForegroundColor Cyan
} else {
    Write-Host "[!] Port is still in use. Options:" -ForegroundColor Yellow
    Write-Host "1. Reboot the server (will clear all port conflicts)" -ForegroundColor White
    Write-Host "2. Manually stop the conflicting process" -ForegroundColor White
    Write-Host "3. Change ERP Core 2 to use a different port (edit appsettings.json)" -ForegroundColor White
}

Write-Host "`nScript completed." -ForegroundColor Green
