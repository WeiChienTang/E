# 修正 ProductService.cs 的建構子和錯誤處理機制
param(
    [Parameter(Mandatory=$false)]
    [string]$ServicePath = ".\Services\Products\ProductService.cs"
)

function Update-ProductService {
    param([string]$FilePath)
    
    if (!(Test-Path $FilePath)) {
        Write-Host "檔案不存在: $FilePath" -ForegroundColor Red
        return
    }
    
    $content = Get-Content $FilePath -Raw
    
    # 修正建構子 - 移除 IErrorLogService 的可選參數
    $content = $content -replace 'IErrorLogService\?\s+_errorLogService;', 'IErrorLogService _errorLogService;'
    $content = $content -replace 'IErrorLogService\?\s+errorLogService\s*=\s*null', 'IErrorLogService errorLogService'
    
    # 修正建構子參數
    $constructorPattern = 'public ProductService\(AppDbContext context, ILogger<ProductService> logger, IErrorLogService\? errorLogService = null\)'
    $newConstructor = 'public ProductService(AppDbContext context, ILogger<ProductService> logger, IErrorLogService errorLogService)'
    $content = $content -replace [regex]::Escape($constructorPattern), $newConstructor
    
    # 新增方法的 try-catch 包裝
    $methods = @(
        'GetAllAsync',
        'GetByIdAsync', 
        'SearchAsync',
        'ValidateAsync',
        'CreateAsync',
        'UpdateAsync',
        'DeleteAsync',
        'GetByProductCodeAsync',
        'IsProductCodeExistsAsync',
        'GetProductsBySupplierAsync',
        'GetProductsByCategoryAsync',
        'GetProductsWithLowStockAsync',
        'GetProductsWithHighStockAsync',
        'GetProductInventoryAsync',
        'UpdateInventoryAsync',
        'GetProductSupplierMappingAsync',
        'AddProductSupplierMappingAsync',
        'RemoveProductSupplierMappingAsync',
        'GetProductSizesAsync',
        'GetProductColorsAsync',
        'GetProductMaterialsAsync'
    )
    
    foreach ($method in $methods) {
        # 找到方法開始位置
        $methodPattern = "public\s+(override\s+)?async\s+Task<[^>]+>\s+$method\s*\("
        if ($content -match $methodPattern) {
            Write-Host "處理方法: $method" -ForegroundColor Green
            
            # 為每個方法添加 try-catch 包裝
            $content = Add-TryCatchToMethod -Content $content -MethodName $method
        }
    }
    
    # 寫入檔案
    Set-Content -Path $FilePath -Value $content -Encoding UTF8
    Write-Host "已修正 ProductService.cs" -ForegroundColor Green
}

function Add-TryCatchToMethod {
    param([string]$Content, [string]$MethodName)
    
    # 找到方法的開始和結束位置
    $lines = $Content -split "`n"
    $methodStart = -1
    $methodEnd = -1
    $braceCount = 0
    $inMethod = $false
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        
        # 尋找方法開始
        if ($line -match "public\s+(override\s+)?async\s+Task<[^>]+>\s+$MethodName\s*\(") {
            $methodStart = $i
            $inMethod = $true
            continue
        }
        
        if ($inMethod) {
            # 計算大括號
            $openBraces = ($line -split '\{').Count - 1
            $closeBraces = ($line -split '\}').Count - 1
            $braceCount += $openBraces - $closeBraces
            
            # 如果大括號平衡，找到方法結束
            if ($braceCount -eq 0 -and $line -match '\}') {
                $methodEnd = $i
                break
            }
        }
    }
    
    if ($methodStart -ne -1 -and $methodEnd -ne -1) {
        # 檢查是否已經有 try-catch
        $methodContent = $lines[$methodStart..$methodEnd] -join "`n"
        if ($methodContent -notmatch 'try\s*\{' -or $methodContent -notmatch '_errorLogService\.LogErrorAsync') {
            # 添加 try-catch 包裝
            $newMethodContent = Add-TryCatchWrapper -MethodContent $methodContent -MethodName $MethodName
            $lines[$methodStart..$methodEnd] = $newMethodContent -split "`n"
        }
    }
    
    return $lines -join "`n"
}

function Add-TryCatchWrapper {
    param([string]$MethodContent, [string]$MethodName)
    
    # 找到方法體的開始
    $lines = $MethodContent -split "`n"
    $methodBodyStart = -1
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '\{') {
            $methodBodyStart = $i
            break
        }
    }
    
    if ($methodBodyStart -eq -1) { return $MethodContent }
    
    # 構建新的方法內容
    $newLines = @()
    
    # 保留方法簽名和開始大括號
    for ($i = 0; $i -le $methodBodyStart; $i++) {
        $newLines += $lines[$i]
    }
    
    # 添加 try 區塊
    $newLines += "            try"
    $newLines += "            {"
    
    # 添加原方法體內容（縮排）
    for ($i = $methodBodyStart + 1; $i -lt $lines.Count - 1; $i++) {
        $newLines += "    " + $lines[$i]
    }
    
    # 添加 catch 區塊
    $newLines += "            }"
    $newLines += "            catch (Exception ex)"
    $newLines += "            {"
    $newLines += "                await _errorLogService.LogErrorAsync(ex, `"Error in $MethodName`", new { });"
    $newLines += "                _logger.LogError(ex, `"Error in $MethodName`");"
    
    # 根據回傳類型決定回傳值
    if ($MethodContent -match 'Task<List<[^>]+>>') {
        $newLines += "                return new List<Product>();"
    } elseif ($MethodContent -match 'Task<Product\?>') {
        $newLines += "                return null;"
    } elseif ($MethodContent -match 'Task<bool>') {
        $newLines += "                return false;"
    } elseif ($MethodContent -match 'Task<ServiceResult>') {
        $newLines += "                return ServiceResult.Failure(`"操作失敗`");"
    } elseif ($MethodContent -match 'Task<int>') {
        $newLines += "                return 0;"
    } else {
        $newLines += "                throw;"
    }
    
    $newLines += "            }"
    
    # 保留方法結束大括號
    $newLines += $lines[$lines.Count - 1]
    
    return $newLines -join "`n"
}

# 執行修正
Update-ProductService -FilePath $ServicePath
Write-Host "ProductService.cs 修正完成!" -ForegroundColor Green
