#!/usr/bin/env pwsh

# 一次性修正所有剩餘 Service 的建構子注入腳本

$servicesToProcess = @(
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierContactService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerAddressService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerContactService.cs", 
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeAddressService.cs"
)

Write-Host "開始批量修正 Service 建構子注入..." -ForegroundColor Green

foreach ($filePath in $servicesToProcess) {
    if (Test-Path $filePath) {
        $serviceName = (Split-Path $filePath -Leaf) -replace "\.cs$", ""
        Write-Host "正在修正 $serviceName..." -ForegroundColor Yellow
        
        $content = Get-Content $filePath -Raw
        
        # 檢查是否已經有 IErrorLogService 欄位
        if ($content -notmatch "_errorLogService") {
            # 找到 ILogger 欄位行，並在其後添加 IErrorLogService 欄位
            $content = $content -replace "(private readonly ILogger<$serviceName> _logger;)", "`$1`n        private readonly IErrorLogService _errorLogService;"
            
            # 修正建構子參數和賦值
            $content = $content -replace "public $serviceName\(AppDbContext context, ILogger<$serviceName> logger\)", "public $serviceName(AppDbContext context, ILogger<$serviceName> logger, IErrorLogService errorLogService)"
            $content = $content -replace "(\s*_logger = logger;)", "`$1`n            _errorLogService = errorLogService;"
            
            Set-Content -Path $filePath -Value $content -Encoding UTF8
            Write-Host "  - 已完成修正 $serviceName" -ForegroundColor Green
        } else {
            Write-Host "  - $serviceName 已經有 IErrorLogService 欄位" -ForegroundColor Blue
        }
    } else {
        Write-Host "檔案不存在: $filePath" -ForegroundColor Red
    }
}

Write-Host "批量修正完成!" -ForegroundColor Green
