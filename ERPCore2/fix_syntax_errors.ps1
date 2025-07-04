# 修正因自動添加錯誤記錄而產生的語法錯誤
$servicesToFix = @(
    ".\Services\Customers\CustomerAddressService.cs",
    ".\Services\Customers\CustomerContactService.cs",
    ".\Services\Employees\EmployeeAddressService.cs",
    ".\Services\Suppliers\SupplierContactService.cs"
)

Write-Host "開始修正語法錯誤..." -ForegroundColor Green

foreach ($serviceFile in $servicesToFix) {
    if (Test-Path $serviceFile) {
        $serviceName = Split-Path $serviceFile -Leaf
        Write-Host "正在修正 $serviceName..." -ForegroundColor Yellow
        
        $content = Get-Content $serviceFile -Raw
        
        # 修正多行字串斷行問題
        $content = $content -replace '("Error in [^"]*)\s*\n\s*([^"]*")', '$1 $2'
        
        # 修正字串中的換行問題
        $content = $content -replace '(".*?)\n\s*([^"]*?")', '$1 $2'
        
        # 修正 LogErrorAsync 調用的格式問題
        $content = $content -replace 'await _errorLogService\.LogErrorAsync\(ex,\s*"([^"]*)\s*\n\s*([^"]*)"', 'await _errorLogService.LogErrorAsync(ex, "$1 $2"'
        
        # 修正字串插值中的括號問題
        $content = $content -replace '\$"([^"]*)\{([^}]*)\}\s*\n\s*([^"]*)"', '$"$1{$2} $3"'
        
        # 修正未閉合的字串
        $content = $content -replace '(".*?)\n\s*([^"]*)\s*\);', '$1 $2");'
        
        # 修正 catch 區塊的格式
        $content = $content -replace 'catch \(Exception ex\)\s*\n\s*\{([^}]*)\}', 'catch (Exception ex) { $1 }'
        
        Set-Content -Path $serviceFile -Value $content -Encoding UTF8
        Write-Host "  - 已修正 $serviceName" -ForegroundColor Green
    }
}

Write-Host "語法錯誤修正完成!" -ForegroundColor Green
