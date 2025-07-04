#!/usr/bin/env pwsh

# 批量修正 40% 合規 Service 的建構子注入腳本

# 需要修正的 Service 列表
$servicesToFix = @(
    @{
        Path = "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierAddressService.cs"
        ServiceName = "SupplierAddressService"
    },
    @{
        Path = "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeContactService.cs"
        ServiceName = "EmployeeContactService"
    },
    @{
        Path = "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierContactService.cs"
        ServiceName = "SupplierContactService"
    },
    @{
        Path = "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerAddressService.cs"
        ServiceName = "CustomerAddressService"
    },
    @{
        Path = "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerContactService.cs"
        ServiceName = "CustomerContactService"
    },
    @{
        Path = "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeAddressService.cs"
        ServiceName = "EmployeeAddressService"
    }
)

Write-Host "開始批量修正 Service 建構子注入..." -ForegroundColor Green

foreach ($service in $servicesToFix) {
    $filePath = $service.Path
    $serviceName = $service.ServiceName
    
    if (Test-Path $filePath) {
        Write-Host "正在修正 $serviceName..." -ForegroundColor Yellow
        
        # 讀取檔案內容
        $content = Get-Content $filePath -Raw
        
        # 修正建構子 - 添加 IErrorLogService 參數和欄位
        $oldConstructorPattern = "public\s+$serviceName\s*\(\s*AppDbContext\s+context\s*,\s*ILogger<$serviceName>\s+logger\s*\)\s*:\s*base\s*\(\s*context\s*\)\s*\{\s*_logger\s*=\s*logger;\s*\}"
        
        $newConstructor = @"
private readonly IErrorLogService _errorLogService;

        public $serviceName(AppDbContext context, ILogger<$serviceName> logger, IErrorLogService errorLogService) : base(context)
        {
            _logger = logger;
            _errorLogService = errorLogService;
        }
"@
        
        $content = $content -replace $oldConstructorPattern, $newConstructor
        
        # 將修正後的內容寫回檔案
        Set-Content -Path $filePath -Value $content -Encoding UTF8
        
        Write-Host "  - 已完成修正 $serviceName" -ForegroundColor Green
    } else {
        Write-Host "檔案不存在: $filePath" -ForegroundColor Red
    }
}

Write-Host "批量修正完成!" -ForegroundColor Green
