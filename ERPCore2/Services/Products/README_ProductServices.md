# Products Services 功能說明

## 🏗️ 架構概述

所有商品服務都繼承自 `GenericManagementService<T>`，提供統一的基礎 CRUD 操作，並根據業務需求擴展特定功能。

### 服務繼承結構
```
GenericManagementService<T>
├── ProductService
├── ProductCategoryService
└── ProductSupplierService
```

## 📁 檔案結構
```
Services/
└── Products/
    ├── Interfaces/
    │   ├── IProductService.cs
    │   ├── IProductCategoryService.cs
    │   └── IProductSupplierService.cs
    ├── ProductService.cs
    ├── ProductCategoryService.cs
    └── ProductSupplierService.cs
```

## 🔧 核心服務功能

### 1. ProductService - 商品主要服務
**繼承**：`GenericManagementService<Product>` → `IProductService`

**主要功能**：
- ✅ 基本 CRUD 操作（繼承自基底類別）
- ✅ 商品代碼唯一性驗證
- ✅ 價格與成本管理
- ✅ 庫存管理（庫存調整、警戒值設定）
- ✅ 啟用狀態管理
- ✅ 關聯資料查詢（商品分類、主要供應商）
- ✅ 供應商關聯管理

**特殊查詢方法**：
- `GetByProductCodeAsync()` - 根據商品代碼查詢
- `GetByProductCategoryAsync()` - 根據商品分類查詢
- `GetByPrimarySupplierAsync()` - 根據主要供應商查詢
- `GetLowStockProductsAsync()` - 取得庫存不足商品
- `GetOverStockProductsAsync()` - 取得庫存過量商品

**庫存管理功能**：
- `UpdateStockAsync()` - 更新庫存
- `AdjustStockAsync()` - 調整庫存（含調整原因）
- `SetStockLevelsAsync()` - 設定庫存警戒值

**價格管理功能**：
- `UpdatePricesAsync()` - 更新商品價格
- `BatchUpdatePricesAsync()` - 批次更新價格（支援百分比調整）

### 2. ProductCategoryService - 商品分類服務
**繼承**：`GenericManagementService<ProductCategory>` → `IProductCategoryService`

**主要功能**：
- ✅ 商品分類階層管理（支援父子關係）
- ✅ 分類名稱與代碼唯一性驗證
- ✅ 循環參考檢查
- ✅ 刪除前檢查（是否有商品或子分類使用）
- ✅ 分類樹狀結構查詢

**階層管理功能**：
- `GetTopLevelCategoriesAsync()` - 取得頂層分類
- `GetChildCategoriesAsync()` - 取得子分類
- `GetCategoryTreeAsync()` - 取得完整分類樹
- `CanSetAsParentAsync()` - 檢查是否可設為父分類

### 3. ProductSupplierService - 商品供應商關聯服務
**繼承**：`GenericManagementService<ProductSupplier>` → `IProductSupplierService`

**主要功能**：
- ✅ 商品與供應商多對多關聯管理
- ✅ 主要供應商設定
- ✅ 供應商價格資訊管理
- ✅ 交期和最小訂購量管理
- ✅ 批次關聯設定
- ✅ 價格分析和統計

**關聯管理功能**：
- `SetPrimarySupplierAsync()` - 設定主要供應商
- `BatchSetProductSuppliersAsync()` - 批次設定商品供應商
- `BatchSetSupplierProductsAsync()` - 批次設定供應商商品

**價格分析功能**：
- `GetBestPriceProductsAsync()` - 取得供應商最佳價格商品
- `GetBestPriceSuppliersAsync()` - 取得商品最佳價格供應商
- `GetPriceRangeAsync()` - 取得價格範圍
- `GetAverageLeadTimeAsync()` - 取得平均交期

## 🎯 設計模式與最佳實踐

### 通用服務模式
```csharp
// 所有服務都遵循相同的模式
public class ProductService : GenericManagementService<Product>, IProductService
{
    // 1. 覆寫基底方法（如需要）
    public override async Task<List<Product>> GetAllAsync() { }
    
    // 2. 實作業務特定方法
    public async Task<bool> IsProductCodeExistsAsync(string code) { }
    
    // 3. 輔助方法
    public void InitializeNewProduct(Product product) { }
}
```

### 依賴注入模式
所有服務都已註冊在 `ServiceRegistration.cs` 中：
```csharp
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductCategoryService, ProductCategoryService>();
services.AddScoped<IProductSupplierService, ProductSupplierService>();
```

### 錯誤處理模式
```csharp
// 統一使用 ServiceResult 封裝結果
public async Task<ServiceResult> UpdateStockAsync(int productId, int newStock)
{
    try
    {
        // 業務邏輯
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "錯誤訊息");
        return ServiceResult.Failure($"操作失敗: {ex.Message}");
    }
}
```

## 🔄 與其他服務的關聯

### 商品相關服務依賴
- **SupplierService** - 供應商管理
- **ProductCategoryService** - 商品分類管理
- **ProductSupplierService** - 商品供應商關聯

### 資料流向
```
ProductService (主服務)
├── ProductCategoryService (商品分類)
├── ProductSupplierService (供應商關聯)
└── SupplierService (供應商資料)
```

## 📊 重要特性

### 🔒 資料驗證
- **必填欄位檢查**：商品代碼、商品名稱
- **唯一性檢查**：商品代碼、分類名稱/代碼
- **格式驗證**：價格範圍、庫存數量
- **關聯性檢查**：主要供應商、商品分類階層

### 🏃‍♂️ 效能優化
- **Include() 關聯載入**：避免 N+1 查詢問題
- **AsNoTracking()**：只讀查詢效能優化
- **批次操作**：支援批次建立、更新、刪除

### 📦 庫存管理特性
- **實時庫存追蹤**：CurrentStock 即時更新
- **庫存警戒**：MinStockLevel、MaxStockLevel 設定
- **庫存狀態**：自動計算庫存狀態（正常/不足/過量）
- **庫存調整**：支援調整原因記錄

### 💰 價格管理特性
- **多價格支援**：UnitPrice（售價）、CostPrice（成本價）
- **供應商報價**：每個供應商可設定不同報價
- **批次價格調整**：支援固定金額或百分比調整
- **價格分析**：價格範圍、最佳價格供應商分析

### 🔗 關聯管理特性
- **多對多關聯**：商品與供應商的彈性關聯
- **主要供應商**：每個商品可設定主要供應商
- **供應商資訊**：交期、最小訂購量、供應商商品代碼

## 🎮 使用範例

### 基本商品管理
```csharp
// 注入服務
private readonly IProductService _productService;

// 取得所有商品（包含關聯資料）
var products = await _productService.GetAllAsync();

// 根據商品代碼查詢
var product = await _productService.GetByProductCodeAsync("PRD001");

// 建立新商品
var newProduct = new Product();
_productService.InitializeNewProduct(newProduct);
var result = await _productService.CreateAsync(newProduct);
```

### 庫存管理
```csharp
// 更新庫存
await _productService.UpdateStockAsync(productId, 100);

// 調整庫存（含原因）
await _productService.AdjustStockAsync(productId, -10, "銷售出貨");

// 設定庫存警戒值
await _productService.SetStockLevelsAsync(productId, 10, 1000);

// 取得庫存不足商品
var lowStockProducts = await _productService.GetLowStockProductsAsync();
```

### 價格管理
```csharp
// 更新商品價格
await _productService.UpdatePricesAsync(productId, 150.00m, 120.00m);

// 批次調整價格（上漲10%）
var productIds = new List<int> { 1, 2, 3 };
await _productService.BatchUpdatePricesAsync(productIds, 10, true);
```

### 商品分類管理
```csharp
// 取得分類樹狀結構
var categoryTree = await _productCategoryService.GetCategoryTreeAsync();

// 取得頂層分類
var topCategories = await _productCategoryService.GetTopLevelCategoriesAsync();

// 取得子分類
var childCategories = await _productCategoryService.GetChildCategoriesAsync(parentId);
```

### 供應商關聯管理
```csharp
// 取得商品的所有供應商
var suppliers = await _productSupplierService.GetByProductIdAsync(productId);

// 設定主要供應商
await _productSupplierService.SetPrimarySupplierAsync(productSupplierId);

// 取得最佳價格供應商
var bestPriceSuppliers = await _productSupplierService.GetBestPriceSuppliersAsync(productId);

// 批次設定商品供應商
var supplierIds = new List<int> { 1, 2, 3 };
await _productSupplierService.BatchSetProductSuppliersAsync(productId, supplierIds);
```

## 📈 商業邏輯亮點

### 庫存狀態智能判斷
```csharp
public string GetStockStatus(Product product)
{
    if (product.MinStockLevel.HasValue && product.CurrentStock <= product.MinStockLevel.Value)
        return "庫存不足";
    
    if (product.MaxStockLevel.HasValue && product.CurrentStock >= product.MaxStockLevel.Value)
        return "庫存過量";
        
    return "正常";
}
```

### 供應商價格分析
```csharp
// 取得價格範圍分析
var (minPrice, maxPrice) = await _productSupplierService.GetPriceRangeAsync(productId);

// 取得平均交期
var avgLeadTime = await _productSupplierService.GetAverageLeadTimeAsync(productId);
```

---

*此架構遵循 ERPCore2 系統的統一設計原則，提供完整的商品管理功能，支援庫存管理、價格管理、供應商關聯等核心業務需求。*
