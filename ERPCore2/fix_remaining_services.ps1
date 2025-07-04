# 批量為 SupplierContactService 和 EmployeeAddressService 添加錯誤處理

$services = @(
    ".\Services\Suppliers\SupplierContactService.cs",
    ".\Services\Employees\EmployeeAddressService.cs"
)

foreach ($servicePath in $services) {
    if (Test-Path $servicePath) {
        $serviceName = (Split-Path $servicePath -Leaf) -replace "\.cs$", ""
        Write-Host "修正 $serviceName..." -ForegroundColor Green
        
        $content = Get-Content $servicePath -Raw
        
        # 為所有沒有 try-catch 的方法添加錯誤處理
        $lines = $content -split "`n"
        $newLines = @()
        
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            $newLines += $line
            
            # 檢查是否為方法開始且沒有 try-catch
            if ($line -match "^\s*public\s+(override\s+)?async\s+Task" -and 
                $i + 20 -lt $lines.Count -and 
                ($lines[($i+1)..($i+20)] -join "`n") -notmatch "try\s*\{") {
                
                # 找到方法體開始的大括號
                $braceIndex = -1
                for ($j = $i + 1; $j -lt [Math]::Min($i + 10, $lines.Count); $j++) {
                    if ($lines[$j] -match "^\s*\{") {
                        $braceIndex = $j
                        break
                    }
                }
                
                if ($braceIndex -ne -1) {
                    # 在方法體開始後插入 try
                    $newLines[$braceIndex] = $lines[$braceIndex] + "`n            try`n            {"
                    
                    # 找到方法結束的大括號
                    $braceCount = 1
                    $endBraceIndex = -1
                    for ($k = $braceIndex + 1; $k -lt $lines.Count; $k++) {
                        $openBraces = ($lines[$k] -split '\{').Count - 1
                        $closeBraces = ($lines[$k] -split '\}').Count - 1
                        $braceCount += $openBraces - $closeBraces
                        
                        if ($braceCount -eq 0) {
                            $endBraceIndex = $k
                            break
                        }
                    }
                    
                    if ($endBraceIndex -ne -1) {
                        # 在方法結束前插入 catch
                        $returnType = ""
                        if ($line -match "Task<List<") {
                            $returnType = "return new List<" + ($line -replace ".*Task<List<([^>]+)>.*", '$1') + ">();"
                        } elseif ($line -match "Task<ServiceResult>") {
                            $returnType = "return ServiceResult.Failure(`"操作失敗`");"
                        } elseif ($line -match "Task<[^>]+\?>") {
                            $returnType = "return null;"
                        } elseif ($line -match "Task<bool>") {
                            $returnType = "return false;"
                        } elseif ($line -match "Task<int>") {
                            $returnType = "return 0;"
                        } else {
                            $returnType = "throw;"
                        }
                        
                        $catchBlock = @"
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "Error in method");
                $returnType
            }
"@
                        
                        $newLines[$endBraceIndex] = $catchBlock + "`n" + $lines[$endBraceIndex]
                    }
                }
            }
        }
        
        $newContent = $newLines -join "`n"
        Set-Content -Path $servicePath -Value $newContent -Encoding UTF8
        Write-Host "已完成 $serviceName" -ForegroundColor Yellow
    }
}

Write-Host "批量修正完成!" -ForegroundColor Green
