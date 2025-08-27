# 關聯依賴刪除檢查功能使用指南

## 概述
本系統提供了完整的關聯依賴檢查功能，可以在刪除資料前自動檢查是否有其他資料正在使用該筆資料，避免因外鍵約束而導致的刪除失敗。

## 架構設計

### 1. 基底服務層 (GenericManagementService)
所有的管理服務都繼承自 `GenericManagementService<T>`，該基底類別提供了：
- `CanDeleteAsync(T entity)` - 可被子類別覆寫的刪除檢查方法
- `CheckForeignKeyReferencesAsync(int entityId)` - 可被子類別覆寫的關聯檢查方法

### 2. 前端組件層 (GenericIndexPageComponent)
泛型 Index 頁面組件在執行刪除操作時會：
1. 先調用服務的 `CanDeleteAsync` 方法檢查是否可以刪除
2. 如果不能刪除，顯示具體的錯誤訊息
3. 如果可以刪除，顯示確認對話框
4. 確認後執行實際刪除操作

### 3. 輔助工具類別 (DependencyCheckHelper)
提供了常見實體的關聯檢查方法，包括：
- 部門依賴檢查 (`CheckDepartmentDependenciesAsync`)
- 角色依賴檢查 (`CheckRoleDependenciesAsync`)
- 商品依賴檢查 (`CheckProductDependenciesAsync`)
- 供應商依賴檢查 (`CheckSupplierDependenciesAsync`)
- 客戶依賴檢查 (`CheckCustomerDependenciesAsync`)
- 倉庫依賴檢查 (`CheckWarehouseDependenciesAsync`)
- 單位依賴檢查 (`CheckUnitDependenciesAsync`)
- 職位依賴檢查 (`CheckEmployeePositionDependenciesAsync`)

## 使用方法

### 1. 在服務層實作刪除檢查

以部門服務為例：

```csharp
/// <summary>
/// 覆寫基底類別的 CanDeleteAsync 方法，實作部門特定的刪除檢查
/// </summary>
protected override async Task<ServiceResult> CanDeleteAsync(Department entity)
{
    try
    {
        var dependencyCheck = await DependencyCheckHelper.CheckDepartmentDependenciesAsync(_contextFactory, entity.Id);
        if (!dependencyCheck.CanDelete)
        {
            return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("部門"));
        }
        
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
            Method = nameof(CanDeleteAsync),
            ServiceType = GetType().Name,
            DepartmentId = entity.Id 
        });
        return ServiceResult.Failure("檢查部門刪除條件時發生錯誤");
    }
}
```

### 2. 在前端頁面使用

前端頁面使用 `GenericIndexPageComponent` 時，刪除功能會自動調用服務的檢查方法：

```razor
<GenericIndexPageComponent TEntity="Department"
                          TService="IDepartmentService"
                          Service="DepartmentService"
                          EntityName="部門"
                          ShowDeleteButton="true" />
```

### 3. 自訂關聯檢查邏輯

如果需要檢查的關聯比較特殊，可以直接在服務中覆寫 `CanDeleteAsync` 方法：

```csharp
protected override async Task<ServiceResult> CanDeleteAsync(CustomEntity entity)
{
    try
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // 自訂的檢查邏輯
        var hasSpecialRelation = await context.SpecialTable
            .AnyAsync(s => s.CustomEntityId == entity.Id && !s.IsDeleted);
            
        if (hasSpecialRelation)
        {
            return ServiceResult.Failure("無法刪除此資料，因為有特殊關聯正在使用");
        }
        
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        // 錯誤處理
        return ServiceResult.Failure("檢查刪除條件時發生錯誤");
    }
}
```

## 擴充 DependencyCheckHelper

如果要為新的實體類型添加關聯檢查，可以在 `DependencyCheckHelper` 中添加新的方法：

```csharp
/// <summary>
/// 檢查新實體是否可以刪除
/// </summary>
public static async Task<DependencyCheckResult> CheckNewEntityDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int entityId)
{
    try
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var result = new DependencyCheckResult { CanDelete = true };
        
        // 檢查相關表格
        var relatedCount = await context.RelatedTable
            .CountAsync(r => r.NewEntityId == entityId && !r.IsDeleted);
            
        if (relatedCount > 0)
        {
            result.CanDelete = false;
            result.DependentEntities.Add($"相關資料({relatedCount}筆)");
        }
        
        return result;
    }
    catch (Exception)
    {
        return new DependencyCheckResult 
        { 
            CanDelete = false, 
            ErrorMessage = "檢查新實體依賴關係時發生錯誤" 
        };
    }
}
```

## 錯誤訊息自訂

`DependencyCheckResult.GetFormattedErrorMessage()` 方法會根據檢查結果產生適當的錯誤訊息：

- 如果有具體的依賴項目，會列出所有依賴的資料類型和數量
- 如果有自訂錯誤訊息，會使用自訂訊息
- 否則使用預設的通用錯誤訊息

## 注意事項

1. **效能考量**：關聯檢查可能涉及多個資料表的查詢，在大量資料的情況下需要注意效能
2. **一致性**：確保所有相關的服務都實作了適當的關聯檢查
3. **錯誤處理**：關聯檢查失敗時應該記錄適當的錯誤日誌
4. **測試**：為每個實體的關聯檢查編寫單元測試

## 故障排除

### 1. 編譯錯誤：找不到 DependencyCheckHelper
確保已經添加了正確的 using 語句：
```csharp
using ERPCore2.Helpers;
```

### 2. 運行時錯誤：找不到特定的資料表
檢查 `DependencyCheckHelper` 中的資料表名稱是否與 `AppDbContext` 中定義的 `DbSet` 名稱一致。

### 3. 刪除檢查沒有生效
確保：
- 服務正確覆寫了 `CanDeleteAsync` 方法
- `GenericIndexPageComponent` 使用了最新版本的刪除邏輯
- 前端頁面正確設定了 `EntityName` 和 `Service` 參數

## 已實作的服務

目前已經實作了關聯檢查的服務包括：
- `DepartmentService` - 檢查是否有員工隸屬於該部門
- `RoleService` - 檢查是否有員工使用該角色，以及是否為系統角色
- `UnitService` - 檢查是否有商品或訂單明細使用該單位
- `EmployeePositionService` - 檢查是否有員工使用該職位
- `ProductCategoryService` - 檢查是否有商品使用該分類（已存在）
- `SupplierTypeService` - 檢查是否有供應商使用該類型（已存在）
- `WarehouseService` - 檢查是否有庫存記錄使用該倉庫
- `ProductService` - 檢查是否有訂單明細、庫存記錄或供應商關聯使用該商品
- `SupplierService` - 檢查是否有採購訂單或商品供應商關聯使用該供應商
- `CustomerService` - 檢查是否有銷貨訂單使用該客戶
- `PurchaseOrderService` - 檢查是否有進貨明細記錄使用該採購單

## 待實作的服務

其他可考慮實作關聯檢查的服務：
- `CustomerTypeService` - 檢查是否有客戶使用該類型（部分已實作，可以整合到新架構）
- 其他特殊業務邏輯的實體服務
