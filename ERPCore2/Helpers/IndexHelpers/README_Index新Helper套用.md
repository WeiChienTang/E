# Index 頁面 Helper 遷移指南

## 概述

本文檔記錄將現有 Index 頁面遷移至使用新的 Helper 類別（BreadcrumbHelper 和 DataLoaderHelper）的進度。

## 新的 Helper 類別

### 1. BreadcrumbHelper
位置：`Helpers/IndexHelpers/BreadcrumbHelper.cs`

**功能**：
- 簡化麵包屑導航的初始化
- 內建完整的錯誤處理機制
- 提供三種便捷方法

**使用方式**：
```csharp
// 兩層麵包屑
breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync("客戶管理", NotificationService, GetType());

// 三層麵包屑
breadcrumbItems = await BreadcrumbHelper.CreateThreeLevelAsync("庫存管理", "倉庫維護", NotificationService, GetType());

// 自訂麵包屑
breadcrumbItems = await BreadcrumbHelper.InitializeAsync(new[] { new BreadcrumbItem("採購管理", "#"), new BreadcrumbItem("進貨退出管理") }, NotificationService, GetType());
```

### 2. DataLoaderHelper
位置：`Helpers/IndexHelpers/DataLoaderHelper.cs`

**功能**：
- 統一資料載入的錯誤處理
- 自動記錄錯誤和通知使用者
- 提供安全的後備值

**使用方式**：
```csharp
private Task<List<Customer>> LoadCustomersAsync() => DataLoaderHelper.LoadAsync(() => CustomerService.GetAllAsync(), "客戶", NotificationService, GetType());
```

## 遷移檢查清單

### 需要修改的項目

每個 Index 頁面需要檢查以下兩個方法：

- [ ] `InitializeBreadcrumbsAsync()` - 使用 BreadcrumbHelper
- [ ] `LoadXXXAsync()` - 使用 DataLoaderHelper

### 遷移步驟

1. **更新 InitializeBreadcrumbsAsync**
   - 移除 try-catch 區塊
   - 使用 BreadcrumbHelper 的對應方法
   - 從 18 行簡化為 1 行

2. **更新 LoadXXXAsync**
   - 移除 try-catch 區塊
   - 使用 DataLoaderHelper.LoadAsync
   - 從 15 行簡化為 1 行

3. **驗證編譯**
   - 確保沒有編譯錯誤
   - 測試頁面功能正常

## 遷移進度追蹤

### ✅ 已完成遷移

| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 遷移日期 | 備註 |
|------|---------|------------------|------------------|----------|------|
| CustomerIndex | `Components/Pages/Customers/CustomerIndex.razor` | ✅ | ✅ | 2025-11-08 | 首個範例頁面 |
| SupplierIndex | `Components/Pages/Suppliers/SupplierIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| ProductIndex | `Components/Pages/Products/ProductIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| ProductCategoryIndex | `Components/Pages/Products/ProductCategoryIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| UnitIndex | `Components/Pages/Products/UnitIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| SizeIndex | `Components/Pages/Products/SizeIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| WarehouseIndex | `Components/Pages/Warehouse/WarehouseIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| WarehouseLocationIndex | `Components/Pages/Warehouse/WarehouseLocationIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| MaterialIssueIndex | `Components/Pages/Warehouse/MaterialIssueIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| InventoryStockIndex | `Components/Pages/Warehouse/InventoryStockIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| InventoryTransactionIndex | `Components/Pages/Warehouse/InventoryTransactionIndex.razor` | ✅ | ✅ | 2025-11-08 | 低優先級 |
| PurchaseOrderIndex | `Components/Pages/Purchase/PurchaseOrderIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| PurchaseReceivingIndex | `Components/Pages/Purchase/PurchaseReceivingIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| PurchaseReturnIndex | `Components/Pages/Purchase/PurchaseReturnIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| QuotationIndex | `Components/Pages/Sales/QuotationIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| SalesOrderIndex | `Components/Pages/Sales/SalesOrderIndex.razor` | ✅ | ✅ | 2025-11-08 | 高優先級 |
| SalesReturnIndex | `Components/Pages/Sales/SalesReturnIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| SalesReturnReasonIndex | `Components/Pages/Sales/SalesReturnReasonIndex.razor` | ✅ | ✅ | 2025-11-08 | 低優先級 |
| EmployeeIndex | `Components/Pages/Employees/EmployeeIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| DepartmentIndex | `Components/Pages/Employees/DepartmentIndex.razor` | ✅ | ✅ | 2025-11-08 | 中優先級 |
| EmployeePositionIndex | `Components/Pages/Employees/EmployeePositionIndex.razor` | ✅ | ✅ | 2025-11-08 | 低優先級 |
| RoleIndex | `Components/Pages/Employees/RoleIndex.razor` | ✅ | ✅ | 2025-11-08 | 低優先級 |
| PermissionIndex | `Components/Pages/Employees/PermissionIndex.razor` | ✅ | ✅ | 2025-11-08 | 低優先級 |

### 📋 待遷移頁面

#### Customers 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| - | - | - | - | - |

#### Suppliers 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| SupplierIndex | `Components/Pages/Suppliers/SupplierIndex.razor` | ✅ | ✅ | 高 |

#### Products 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| ProductIndex | `Components/Pages/Products/ProductIndex.razor` | ✅ | ✅ | 高 |
| ProductCategoryIndex | `Components/Pages/Products/ProductCategoryIndex.razor` | ✅ | ✅ | 中 |
| UnitIndex | `Components/Pages/Products/UnitIndex.razor` | ✅ | ✅ | 中 |
| SizeIndex | `Components/Pages/Products/SizeIndex.razor` | ✅ | ✅ | 中 |

#### Warehouse 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| WarehouseIndex | `Components/Pages/Warehouse/WarehouseIndex.razor` | ✅ | ✅ | 高 |
| WarehouseLocationIndex | `Components/Pages/Warehouse/WarehouseLocationIndex.razor` | ✅ | ✅ | 中 |
| MaterialIssueIndex | `Components/Pages/Warehouse/MaterialIssueIndex.razor` | ✅ | ✅ | 中 |
| InventoryStockIndex | `Components/Pages/Warehouse/InventoryStockIndex.razor` | ✅ | ✅ | 中 |
| InventoryTransactionIndex | `Components/Pages/Warehouse/InventoryTransactionIndex.razor` | ✅ | ✅ | 低 |

#### Purchase 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| PurchaseOrderIndex | `Components/Pages/Purchase/PurchaseOrderIndex.razor` | ✅ | ✅ | 高 |
| PurchaseReceivingIndex | `Components/Pages/Purchase/PurchaseReceivingIndex.razor` | ✅ | ✅ | 高 |
| PurchaseReturnIndex | `Components/Pages/Purchase/PurchaseReturnIndex.razor` | ✅ | ✅ | 中 |

#### Sales 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| QuotationIndex | `Components/Pages/Sales/QuotationIndex.razor` | ✅ | ✅ | 高 |
| SalesOrderIndex | `Components/Pages/Sales/SalesOrderIndex.razor` | ✅ | ✅ | 高 |
| SalesReturnIndex | `Components/Pages/Sales/SalesReturnIndex.razor` | ✅ | ✅ | 中 |
| SalesReturnReasonIndex | `Components/Pages/Sales/SalesReturnReasonIndex.razor` | ✅ | ✅ | 低 |

#### Employees 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| EmployeeIndex | `Components/Pages/Employees/EmployeeIndex.razor` | ✅ | ✅ | 中 |
| DepartmentIndex | `Components/Pages/Employees/DepartmentIndex.razor` | ✅ | ✅ | 中 |
| EmployeePositionIndex | `Components/Pages/Employees/EmployeePositionIndex.razor` | ✅ | ✅ | 低 |
| RoleIndex | `Components/Pages/Employees/RoleIndex.razor` | ✅ | ✅ | 低 |
| PermissionIndex | `Components/Pages/Employees/PermissionIndex.razor` | ✅ | ✅ | 低 |
| RolePermissionManagement | `Components/Pages/Employees/RolePermissionManagement.razor` | ⏳ | ⏳ | 低 |

#### FinancialManagement 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| SetoffDocumentIndex | `Components/Pages/FinancialManagement/SetoffDocumentIndex.razor` | ⏳ | ⏳ | 中 |
| BankIndex | `Components/Pages/FinancialManagement/BankIndex.razor` | ⏳ | ⏳ | 低 |
| CurrencyIndex | `Components/Pages/FinancialManagement/CurrencyIndex.razor` | ⏳ | ⏳ | 低 |
| PaymentMethodIndex | `Components/Pages/FinancialManagement/PaymentMethodIndex.razor` | ⏳ | ⏳ | 低 |

#### ProductionManagement 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| ProductionScheduleIndex | `Components/Pages/ProductionManagement/ProductionScheduleIndex.razor` | ⏳ | ⏳ | 中 |
| ProductCompositionIndex | `Components/Pages/ProductionManagement/ProductCompositionIndex.razor` | ⏳ | ⏳ | 低 |
| ColorIndex | `Components/Pages/ProductionManagement/ColorIndex.razor` | ⏳ | ⏳ | 低 |
| MaterialIndex | `Components/Pages/ProductionManagement/MaterialIndex.razor` | ⏳ | ⏳ | 低 |
| WeatherIndex | `Components/Pages/ProductionManagement/WeatherIndex.razor` | ⏳ | ⏳ | 低 |

#### Systems 模組
| 頁面 | 檔案路徑 | BreadcrumbHelper | DataLoaderHelper | 優先級 |
|------|---------|------------------|------------------|--------|
| CompanyIndex | `Components/Pages/Systems/CompanyIndex.razor` | ⏳ | ⏳ | 中 |
| ErrorLogIndex | `Components/Pages/Systems/ErrorLogIndex.razor` | ⏳ | ⏳ | 低 |
| PaperSettingIndex | `Components/Pages/Systems/PaperSettingIndex.razor` | ⏳ | ⏳ | 低 |
| PrinterConfigurationIndex | `Components/Pages/Systems/PrinterConfigurationIndex.razor` | ⏳ | ⏳ | 低 |
| ReportPrintConfigurationIndex | `Components/Pages/Systems/ReportPrintConfigurationIndex.razor` | ⏳ | ⏳ | 低 |
| SystemParameterSettings | `Components/Pages/Systems/SystemParameterSettings/SystemParameterSettings.razor` | ⏳ | ⏳ | 低 |

## 統計資訊

### 總體進度
- **總頁面數**: 41
- **已完成**: 23 (56.1%)
- **待遷移**: 18 (43.9%)

### 按優先級統計
- **高優先級**: 7/7 已完成 (100%)
- **中優先級**: 10/15 已完成 (66.7%)
- **低優先級**: 6/18 已完成 (33.3%)

### 預估效益
以每個頁面平均簡化 31 行程式碼計算：
- **已減少**: ~713 行程式碼 (23 個頁面)
- **剩餘可減少**: ~558 行程式碼 (18 個頁面)
- **總計可減少**: ~1,271 行程式碼
- **維護性提升**: 錯誤處理邏輯集中管理
- **一致性提升**: 所有頁面使用統一的模式

## 遷移範例

### 修改前（CustomerIndex）
```csharp
private async Task InitializeBreadcrumbsAsync()
{
    try
    {
        breadcrumbItems = new List<BreadcrumbItem>
        {
            new("首頁", "/"),
            new("客戶管理")
        };
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(InitializeBreadcrumbsAsync), GetType(), additionalData: "初始化麵包屑導航失敗");
        await NotificationService.ShowErrorAsync("初始化麵包屑導航失敗");
        breadcrumbItems = new List<BreadcrumbItem>();
    }
}

private async Task<List<Customer>> LoadCustomersAsync()
{
    try
    {
        return await CustomerService.GetAllAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadCustomersAsync), GetType(), additionalData: "載入客戶資料失敗");
        await NotificationService.ShowErrorAsync("載入客戶資料失敗");
        return new List<Customer>();
    }
}
```

### 修改後（CustomerIndex）
```csharp
private async Task InitializeBreadcrumbsAsync() => breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync("客戶管理", NotificationService, GetType());

private Task<List<Customer>> LoadCustomersAsync() => DataLoaderHelper.LoadAsync(() => CustomerService.GetAllAsync(), "客戶", NotificationService, GetType());
```

### 程式碼減少
- **InitializeBreadcrumbsAsync**: 18 行 → 1 行 (減少 94%)
- **LoadCustomersAsync**: 15 行 → 1 行 (減少 93%)
- **總計**: 33 行 → 2 行 (減少 94%)

## 注意事項

### 遷移前檢查
1. ✅ 確保已引用 `@using ERPCore2.Models`
2. ✅ 確保已注入 `INotificationService`
3. ✅ 變數類型已從 `GenericHeaderComponent.BreadcrumbItem` 改為 `BreadcrumbItem`

### 常見問題

**Q: 如果需要自訂錯誤訊息怎麼辦？**
A: DataLoaderHelper 的第二個參數就是實體名稱，會自動組合成「載入XX資料失敗」的訊息。

**Q: 如果載入方法需要參數怎麼辦？**
A: 使用 lambda 表達式傳遞參數：
```csharp
() => ProductService.GetByCategoryAsync(categoryId)
```

**Q: 三層麵包屑的中間層需要連結嗎？**
A: 可選。不需要連結時省略 `moduleUrl` 參數，需要時傳入 URL 或 `"#"`。

## 相關文檔

- [README_Index_Design.md](./README_Index_Design.md) - Index 頁面設計規範
- [BreadcrumbHelper 原始碼](../Helpers/IndexHelpers/BreadcrumbHelper.cs)
- [DataLoaderHelper 原始碼](../Helpers/IndexHelpers/DataLoaderHelper.cs)

## 更新記錄

| 日期 | 更新內容 | 更新人 |
|------|---------|--------|
| 2025-11-08 | 建立文檔，完成 CustomerIndex 遷移 | - |

---

**圖例說明**
- ✅ 已完成
- ⏳ 待處理
- 🔄 進行中
- ❌ 暫時跳過
