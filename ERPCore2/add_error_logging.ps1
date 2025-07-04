#!/usr/bin/env pwsh

# 為 80% 合規的 Service 添加錯誤記錄調用

$servicesToProcess = @(
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Products\ProductSupplierService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerAddressService.cs", 
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerContactService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierAddressService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierContactService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeAddressService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeContactService.cs"
)

Write-Host "開始為 80% 合規的 Service 添加錯誤記錄..." -ForegroundColor Green

foreach ($filePath in $servicesToProcess) {
    if (Test-Path $filePath) {
        $serviceName = (Split-Path $filePath -Leaf) -replace "\.cs$", ""
        Write-Host "正在處理 $serviceName..." -ForegroundColor Yellow
        
        $content = Get-Content $filePath -Raw
        
        # 計算原始的錯誤記錄調用數量
        $originalCount = ($content | Select-String "await _errorLogService\.LogErrorAsync" -AllMatches).Matches.Count
        
        # 查找所有沒有錯誤記錄的 catch 塊
        $catchBlocks = [regex]::Matches($content, 'catch\s*\(\s*Exception\s+ex\s*\)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}')
        
        $replacements = 0
        foreach ($match in $catchBlocks) {
            $catchContent = $match.Groups[1].Value
            
            # 如果這個 catch 塊沒有錯誤記錄調用
            if ($catchContent -notmatch "_errorLogService\.LogErrorAsync") {
                # 查找方法名稱
                $beforeCatch = $content.Substring(0, $match.Index)
                $methodMatch = [regex]::Matches($beforeCatch, 'public\s+(?:async\s+)?(?:Task<?[^>]*>?\s+|ServiceResult[^>]*>\s+|override\s+async\s+Task<?[^>]*>?\s+)?(\w+)\s*\([^)]*\)\s*$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
                
                if ($methodMatch.Count -gt 0) {
                    $methodName = $methodMatch[$methodMatch.Count - 1].Groups[1].Value
                    
                    # 準備錯誤記錄代碼
                    $errorLogCode = @"
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof($methodName),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error in $methodName");
"@
                    
                    # 在 catch 塊開始後插入錯誤記錄
                    $newCatchContent = $catchContent -replace '^(\s*)', "`$1$errorLogCode`n`$1"
                    $newMatch = $match.Value -replace [regex]::Escape($catchContent), $newCatchContent
                    $content = $content -replace [regex]::Escape($match.Value), $newMatch
                    $replacements++
                }
            }
        }
        
        if ($replacements -gt 0) {
            Set-Content -Path $filePath -Value $content -Encoding UTF8
            Write-Host "  - 已添加 $replacements 個錯誤記錄調用" -ForegroundColor Green
        } else {
            Write-Host "  - 沒有需要修正的 catch 塊" -ForegroundColor Blue
        }
    } else {
        Write-Host "檔案不存在: $filePath" -ForegroundColor Red
    }
}

Write-Host "批量添加錯誤記錄完成!" -ForegroundColor Green
