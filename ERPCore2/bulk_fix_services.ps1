#!/usr/bin/env pwsh

# 批量修正 Service 錯誤處理腳本
# 此腳本將自動為低合規性的 Service 添加錯誤處理

# 需要修正的 Service 列表
$servicesToFix = @(
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Commons\AddressTypeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\UnitConversionService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\WarehouseLocationService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\PermissionManagementService.cs"
)

# 創建修正報告
Write-Host "開始批量修正 Service 錯誤處理..." -ForegroundColor Green

foreach ($serviceFile in $servicesToFix) {
    if (Test-Path $serviceFile) {
        $serviceName = Split-Path $serviceFile -Leaf
        Write-Host "正在修正 $serviceName..." -ForegroundColor Yellow
        
        # 讀取檔案內容
        $content = Get-Content $serviceFile -Raw
        
        # 檢查是否需要添加 using Microsoft.Extensions.Logging
        if ($content -notmatch "using Microsoft\.Extensions\.Logging;") {
            $content = $content -replace "(using Microsoft\.EntityFrameworkCore;)", "`$1`nusing Microsoft.Extensions.Logging;"
            Write-Host "  - 已添加 Microsoft.Extensions.Logging using" -ForegroundColor Cyan
        }
        
        # 檢查是否需要修正建構子
        if ($content -notmatch "ILogger<[^>]+>\s+logger[^)]*IErrorLogService\s+errorLogService") {
            Write-Host "  - 需要修正建構子..." -ForegroundColor Cyan
            
            # 找到建構子並修正
            $constructorPattern = "public\s+(\w+Service)\s*\(\s*AppDbContext\s+context\s*\)\s*:\s*base\s*\(\s*context\s*\)\s*\{\s*\}"
            $replacement = @"
private readonly ILogger<`$1> _logger;
        private readonly IErrorLogService _errorLogService;

        public `$1(AppDbContext context, ILogger<`$1> logger, IErrorLogService errorLogService) : base(context)
        {
            _logger = logger;
            _errorLogService = errorLogService;
        }
"@
            
            $content = $content -replace $constructorPattern, $replacement
        }
        
        # 將修正後的內容寫回檔案
        Set-Content -Path $serviceFile -Value $content -Encoding UTF8
        
        Write-Host "  - 已完成修正 $serviceName" -ForegroundColor Green
    } else {
        Write-Host "檔案不存在: $serviceFile" -ForegroundColor Red
    }
}

Write-Host "批量修正完成!" -ForegroundColor Green
