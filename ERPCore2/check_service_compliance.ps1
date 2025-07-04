#!/usr/bin/env pwsh

# 自動化檢查 Service 錯誤處理合規性腳本
# 此腳本將檢查所有 Service 檔案，並生成合規性報告

# 定義要檢查的 Service 檔案列表
$serviceFiles = @(
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Products\ProductService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Products\ProductCategoryService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Products\ProductSupplierService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerTypeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerContactService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Customers\CustomerAddressService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierTypeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierContactService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Suppliers\SupplierAddressService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\RoleService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\PermissionManagementService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeContactService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Employees\EmployeeAddressService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\WarehouseService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\UnitService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\UnitConversionService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\WarehouseLocationService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Inventory\InventoryTransactionTypeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\BOMFoundations\WeatherService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\BOMFoundations\ColorService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\BOMFoundations\MaterialService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Industries\IndustryTypeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Commons\ContactTypeService.cs",
    "c:\Users\cses3\source\repos\ERPCore2\ERPCore2\Services\Commons\AddressTypeService.cs"
)

# 合規性檢查結果
$complianceReport = @()

foreach ($file in $serviceFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # 檢查項目
        $hasCorrectConstructor = $content -match "public\s+\w+Service\s*\([^)]*ILogger<[^>]+>\s+logger[^)]*IErrorLogService\s+errorLogService[^)]*\)"
        $hasErrorLogField = $content -match "private\s+readonly\s+IErrorLogService\s+_errorLogService"
        $hasLoggerField = $content -match "private\s+readonly\s+ILogger<[^>]+>\s+_logger"
        $hasErrorHandling = $content -match "await\s+_errorLogService\.LogErrorAsync"
        $hasTryCatchBlocks = $content -match "try\s*{[^}]*}\s*catch\s*\([^)]*Exception[^)]*\)"
        
        # 計算方法數量
        $publicMethodCount = ($content | Select-String "public\s+(async\s+)?Task" -AllMatches).Matches.Count
        $tryCatchMethodCount = ($content | Select-String "try\s*{[^}]*}\s*catch\s*\([^)]*Exception[^)]*\)" -AllMatches).Matches.Count
        $errorLogCallCount = ($content | Select-String "await\s+_errorLogService\.LogErrorAsync" -AllMatches).Matches.Count
        
        $complianceReport += [PSCustomObject]@{
            ServiceFile = Split-Path $file -Leaf
            HasCorrectConstructor = $hasCorrectConstructor
            HasErrorLogField = $hasErrorLogField
            HasLoggerField = $hasLoggerField
            HasErrorHandling = $hasErrorHandling
            HasTryCatchBlocks = $hasTryCatchBlocks
            PublicMethodCount = $publicMethodCount
            TryCatchMethodCount = $tryCatchMethodCount
            ErrorLogCallCount = $errorLogCallCount
            ComplianceScore = [Math]::Round(($hasCorrectConstructor + $hasErrorLogField + $hasLoggerField + $hasErrorHandling + $hasTryCatchBlocks) / 5 * 100, 2)
        }
    }
}

# 輸出報告
$complianceReport | Sort-Object ComplianceScore | Format-Table -AutoSize
