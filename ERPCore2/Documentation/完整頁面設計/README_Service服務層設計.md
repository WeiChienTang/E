# Service 服務層設計

## 更新日期
2026-03-10

---

## 概述

所有服務繼承 `GenericManagementService<T>`（`T` 必須繼承 `BaseEntity`），透過 `ServiceResult` 統一回傳操作結果。個別服務視需要額外實作 `GetPagedWithFiltersAsync` 供 Index 頁面伺服器端分頁使用。

---

## BaseEntity 共用屬性

所有實體均繼承以下欄位：

| 屬性 | 型別 | 說明 |
|------|------|------|
| `Id` | `int` | 主鍵 |
| `Code` | `string?` | 編號（最長 50） |
| `Status` | `EntityStatus` | Active / Inactive（預設 Active） |
| `IsDraft` | `bool` | 草稿模式（預設 false） |
| `Remarks` | `string?` | 備註（最長 500） |
| `CreatedAt` | `DateTime` | 建立時間 |
| `UpdatedAt` | `DateTime?` | 最後更新時間 |
| `CreatedBy` | `string?` | 建立者 |
| `UpdatedBy` | `string?` | 修改者 |

---

## GenericManagementService\<T\> — Protected 成員

| 成員 | 型別 | 說明 |
|------|------|------|
| `_contextFactory` | `IDbContextFactory<AppDbContext>` | 資料庫上下文工廠 |
| `_logger` | `ILogger<GenericManagementService<T>>?` | 日誌記錄器（可為 null） |

---

## GenericManagementService\<T\> — 可覆寫的 Protected 方法

| 方法 | 預設行為 | 子類別覆寫目的 |
|------|----------|---------------|
| `BuildGetAllQuery(AppDbContext context)` | `context.Set<T>().OrderByDescending(x => x.CreatedAt)` | 加入 Include、自訂排序 |
| `PostProcessGetAllResultsAsync(AppDbContext, List<T>)` | 無動作 | 非同步後處理（如填充關聯名稱） |
| `CanDeleteAsync(T entity)` | 呼叫 `CheckForeignKeyReferencesAsync` | 自訂刪除前置檢查 |
| `CheckForeignKeyReferencesAsync(int entityId)` | 回傳 null（允許刪除） | 自訂外鍵關聯檢查 |
| `GetEntityDisplayName()` | 回傳 `typeof(T).Name` | 自訂錯誤訊息顯示名稱 |

> **注意**：`GetAllAsync()` 與 `GetAllIncludingDraftsAsync()` **不可被子類別覆寫**，透過覆寫 `BuildGetAllQuery()` 自訂查詢。

---

## IGenericManagementService\<T\> — 介面方法

### 基本 CRUD

| 方法簽章 | 回傳值 | 說明 |
|---------|--------|------|
| `GetAllAsync()` | `Task<List<T>>` | 取得所有正式資料（自動排除 IsDraft=true） |
| `GetAllIncludingDraftsAsync()` | `Task<List<T>>` | 取得所有資料（含草稿） |
| `GetActiveAsync()` | `Task<List<T>>` | 取得所有 Status=Active 的資料 |
| `GetByIdAsync(int id)` | `Task<T?>` | 依 ID 取得單筆（AsNoTracking） |
| `CreateAsync(T entity)` | `Task<ServiceResult<T>>` | 新增（草稿模式跳過驗證） |
| `UpdateAsync(T entity)` | `Task<ServiceResult<T>>` | 更新（草稿模式跳過驗證，保留建立資訊） |
| `DeleteAsync(int id)` | `Task<ServiceResult>` | 刪除（內部呼叫 PermanentDeleteAsync） |
| `PermanentDeleteAsync(int id)` | `Task<ServiceResult>` | 硬刪除（先呼叫 CanDeleteAsync） |

### 批次操作

| 方法簽章 | 回傳值 | 說明 |
|---------|--------|------|
| `CreateBatchAsync(List<T> entities)` | `Task<ServiceResult<List<T>>>` | 批次新增 |
| `UpdateBatchAsync(List<T> entities)` | `Task<ServiceResult<List<T>>>` | 批次更新 |
| `DeleteBatchAsync(List<int> ids)` | `Task<ServiceResult>` | 批次硬刪除（逐一呼叫 CanDeleteAsync） |

### 查詢

| 方法簽章 | 回傳值 | 說明 |
|---------|--------|------|
| `GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)` | `Task<(List<T> Items, int TotalCount)>` | 基本分頁（依 CreatedAt 排序，無 Include） |
| `SearchAsync(string searchTerm)` | `Task<List<T>>` | 搜尋（**抽象，子類別必須實作**） |
| `ExistsAsync(int id)` | `Task<bool>` | 檢查 ID 是否存在 |
| `GetCountAsync()` | `Task<int>` | 取得總筆數 |

### 狀態管理

| 方法簽章 | 回傳值 | 說明 |
|---------|--------|------|
| `SetStatusAsync(int id, EntityStatus status)` | `Task<ServiceResult>` | 設定指定狀態 |
| `ToggleStatusAsync(int id)` | `Task<ServiceResult>` | 切換 Active ↔ Inactive |
| `SetStatusBatchAsync(List<int> ids, EntityStatus status)` | `Task<ServiceResult>` | 批次設定狀態 |

### 驗證

| 方法簽章 | 回傳值 | 說明 |
|---------|--------|------|
| `ValidateAsync(T entity)` | `Task<ServiceResult>` | 驗證（**抽象，子類別必須實作**） |
| `IsNameExistsAsync(string name, int? excludeId)` | `Task<bool>` | 名稱重複檢查（預設回傳 false，子類別覆寫） |

### 記錄導航

| 方法簽章 | 回傳值 | 說明 |
|---------|--------|------|
| `GetPreviousIdAsync(int currentId)` | `Task<int?>` | 上一筆 ID（按 Id 遞增排序） |
| `GetNextIdAsync(int currentId)` | `Task<int?>` | 下一筆 ID（按 Id 遞增排序） |
| `GetFirstIdAsync()` | `Task<int?>` | 第一筆 ID |
| `GetLastIdAsync()` | `Task<int?>` | 最後一筆 ID |

---

## GetPagedWithFiltersAsync — 個別服務方法

此方法**不在基底類別**，由每個服務個別實作，供 Index 頁面伺服器端分頁使用。

**方法簽章**（所有已實作的服務均相同模式）：

```
Task<(List<T> Items, int TotalCount)> GetPagedWithFiltersAsync(
    Func<IQueryable<T>, IQueryable<T>>? filterFunc,
    int pageNumber,
    int pageSize)
```

| 參數 | 說明 |
|------|------|
| `filterFunc` | 由 Index 頁面傳入的篩選函式（含 IsDraft 過濾、FieldConfiguration.ApplyFilters 等）；可為 null |
| `pageNumber` | 頁碼（從 1 開始） |
| `pageSize` | 每頁筆數 |

**回傳**：`(List<T> Items, int TotalCount)` — 本頁資料 + 符合條件總筆數。

---

### 新增步驟（以 `SalesReturnReason` 為例）

**步驟 1：Service Interface（`ISalesReturnReasonService.cs`）**

```csharp
Task<(List<SalesReturnReason> Items, int TotalCount)> GetPagedWithFiltersAsync(
    Func<IQueryable<SalesReturnReason>, IQueryable<SalesReturnReason>>? filterFunc,
    int pageNumber,
    int pageSize);
```

**步驟 2：Service 實作（`SalesReturnReasonService.cs`）**

```csharp
public async Task<(List<SalesReturnReason> Items, int TotalCount)> GetPagedWithFiltersAsync(
    Func<IQueryable<SalesReturnReason>, IQueryable<SalesReturnReason>>? filterFunc,
    int pageNumber,
    int pageSize)
{
    try
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        // 基礎查詢：不加 IsDraft 過濾（由 filterFunc 統一處理）
        // 視 Index 欄位是否顯示關聯資料決定是否加 Include
        IQueryable<SalesReturnReason> query = context.SalesReturnReasons;

        if (filterFunc != null)
            query = filterFunc(query);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)   // 維持與原 GetAll 相同的排序
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
        return (new List<SalesReturnReason>(), 0);
    }
}
```

**步驟 3：Index 頁面（`SalesReturnReasonIndex.razor`）**

移除 `DataLoader="@..."` 與 `FilterApplier="@..."`，改為 `ServerDataLoader="@ServerLoadDataAsync"`，並新增對應方法：

```csharp
private Task<(List<SalesReturnReason> Items, int TotalCount)> ServerLoadDataAsync(
    SearchFilterModel filter, int page, int pageSize)
{
    return SalesReturnReasonService.GetPagedWithFiltersAsync(
        q =>
        {
            // IsDraft 統一在 filterFunc 內處理
            // 無 ShowDraftTab 時 ShowDrafts 永遠是 null → ?? false → 只顯示正式資料
            q = q.Where(x => x.IsDraft == (filter.ShowDrafts ?? false));
            // ⚠️ null 檢查必要：ServerDataLoader 可能在 OnInitializedAsync 完成前被呼叫
            if (fieldConfiguration != null)
                q = fieldConfiguration.ApplyFilters(filter, q, nameof(ServerLoadDataAsync), GetType());
            return q;
        },
        page, pageSize);
}
```

> **說明**：不論頁面是否設定 `ShowDraftTab`，IsDraft 一律在 filterFunc 內以 `filter.ShowDrafts ?? false` 處理。Service 基礎查詢不加 IsDraft 過濾，方便未來隨時新增 ShowDraftTab 而無需修改 Service。

---

**已實作此方法的服務**：

| 模組 | 服務 |
|------|------|
| 採購 | `PurchaseOrderService`、`PurchaseReceivingService`、`PurchaseReturnService`、`PurchaseReturnReasonService` |
| 銷售 | `QuotationService`、`SalesOrderService`、`SalesDeliveryService`、`SalesReturnService`、`SalesReturnReasonService` |
| 財務 | `JournalEntryService`、`SetoffDocumentService`、`AccountItemService`、`BankService`、`CurrencyService`、`PaymentMethodService` |
| 客戶 | `CustomerService`、`CustomerVisitService` |
| 廠商 | `SupplierService` |
| 員工 | `EmployeeService`、`DepartmentService`、`EmployeePositionService`、`RoleService`、`PermissionManagementService` |
| 品項 | `ProductService`、`ProductCategoryService`、`UnitService`、`SizeService` |
| 生產 | `MaterialService`、`ProductCompositionService`、`CompositionCategoryService` |
| 倉庫 | `InventoryStockService`、`InventoryTransactionService`、`StockTakingService`、`MaterialIssueService`、`WarehouseService`、`WarehouseLocationService` |
| 車輛 | `VehicleService`、`VehicleMaintenanceService`、`VehicleTypeService` |
| 文件 | `DocumentService`、`DocumentCategoryService` |
| 磅秤 | `WasteRecordService`、`WasteTypeService` |
| 薪資 | `EmployeeSalaryService`、`EmployeeBankAccountService`、`PayrollPeriodService`、`PayrollItemService` |
| 系統 | `CompanyService`、`PrinterConfigurationService`、`PaperSettingService`、`ReportPrintConfigurationService`、`ErrorLogService` |

---

### 已遷移的 Index 頁面完整清單（截至 2026-03-10）

| 頁面 | 模組 | ShowDraftTab | Include |
|------|------|:------------:|---------|
| `AccountItemIndex.razor` | 會計 | | 無 |
| `JournalEntryIndex.razor` | 財務 | | Include Company |
| `SetoffDocumentIndex.razor` | 財務 | | Include Company |
| `PurchaseOrderIndex.razor` | 採購 | ✓ | Include Company、Supplier |
| `PurchaseReceivingIndex.razor` | 採購 | ✓ | Include Supplier |
| `PurchaseReturnIndex.razor` | 採購 | ✓ | Include Supplier |
| `PurchaseReturnReasonIndex.razor` | 採購 | ✓ | 無 |
| `QuotationIndex.razor` | 銷售 | ✓ | Include Customer |
| `SalesOrderIndex.razor` | 銷售 | ✓ | Include Customer |
| `SalesDeliveryIndex.razor` | 銷售 | ✓ | Include Customer |
| `SalesReturnIndex.razor` | 銷售 | ✓ | Include Customer、ReturnReason |
| `SalesReturnReasonIndex.razor` | 銷售 | ✓ | 無 |
| `CustomerIndex.razor` | 客戶 | ✓ | 無 |
| `CustomerVisitIndex.razor` | 客戶 | | Include Customer |
| `SupplierIndex.razor` | 廠商 | ✓ | 無 |
| `EmployeeIndex.razor` | 員工 | ✓ | Include Dept、Position、Role |
| `DepartmentIndex.razor` | 員工 | ✓ | 無 |
| `EmployeePositionIndex.razor` | 員工 | ✓ | 無 |
| `RoleIndex.razor` | 員工 | | 無 |
| `PermissionIndex.razor` | 員工 | | 無 |
| `ProductIndex.razor` | 品項 | ✓ | Include ProductCategory、Unit、ProductionUnit、Size |
| `ProductCategoryIndex.razor` | 品項 | ✓ | 無 |
| `UnitIndex.razor` | 品項 | ✓ | 無 |
| `SizeIndex.razor` | 品項 | ✓ | 無 |
| `MaterialIndex.razor` | 生產 | | 無 |
| `ProductCompositionIndex.razor` | 生產 | ✓ | Include ParentProduct、CompositionCategory |
| `CompositionCategoryIndex.razor` | 生產 | ✓ | 無 |
| `InventoryStockIndex.razor` | 倉庫 | ✓ | Include Product.ProductCategory |
| `InventoryTransactionIndex.razor` | 倉庫 | | Include Warehouse |
| `StockTakingIndex.razor` | 倉庫 | ✓ | Include Warehouse |
| `MaterialIssueIndex.razor` | 倉庫 | ✓ | 無 |
| `WarehouseIndex.razor` | 倉庫 | ✓ | 無 |
| `WarehouseLocationIndex.razor` | 倉庫 | ✓ | Include Warehouse |
| `VehicleIndex.razor` | 車輛 | ✓ | 無 |
| `VehicleMaintenanceIndex.razor` | 車輛 | ✓ | Include Vehicle |
| `VehicleTypeIndex.razor` | 車輛 | ✓ | 無 |
| `DocumentIndex.razor` | 文件 | ✓ | Include DocumentCategory |
| `DocumentCategoryIndex.razor` | 文件 | ✓ | 無 |
| `WasteRecordIndex.razor` | 磅秤 | ✓ | Include WasteType |
| `WasteTypeIndex.razor` | 磅秤 | ✓ | 無 |
| `BankIndex.razor` | 財務 | ✓ | 無 |
| `CurrencyIndex.razor` | 財務 | ✓ | 無 |
| `PaymentMethodIndex.razor` | 財務 | ✓ | 無 |
| `EmployeeSalaryIndex.razor` | 薪資 | | Include Employee |
| `EmployeeBankAccountIndex.razor` | 薪資 | | Include Employee |
| `PayrollPeriodIndex.razor` | 薪資 | | 無 |
| `PayrollItemIndex.razor` | 薪資 | | 無 |
| `CompanyIndex.razor` | 系統 | | 無 |
| `PrinterConfigurationIndex.razor` | 系統 | | 無 |
| `PaperSettingIndex.razor` | 系統 | | 無 |
| `ReportPrintConfigurationIndex.razor` | 系統 | | 無 |
| `ErrorLogIndex.razor` | 系統 | | 無 |

> **備註**：`PayrollIndex.razor` 為完全自訂頁面（非 GenericIndexPageComponent），無需遷移。

---

## ServiceResult 結構

### ServiceResult（無資料）

| 屬性 / 靜態方法 | 型別 / 說明 |
|----------------|------------|
| `IsSuccess` | `bool` |
| `ErrorMessage` | `string` |
| `ValidationErrors` | `List<string>` |
| `Success()` | 建立成功結果 |
| `Failure(string errorMessage)` | 建立失敗結果 |
| `ValidationFailure(List<string>)` | 建立驗證失敗結果 |

### ServiceResult\<T\>（含資料，繼承 ServiceResult）

| 屬性 / 靜態方法 | 型別 / 說明 |
|----------------|------------|
| `Data` | `T?` |
| `Success(T data)` | 建立含資料的成功結果 |
| `Failure(string errorMessage)` | 建立失敗結果 |
| `ValidationFailure(List<string>)` | 建立驗證失敗結果 |

---

## 錯誤處理規範

| 使用場景 | 呼叫方法 |
|---------|---------|
| Service 層 catch | `ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(Method), GetType(), _logger)` |
| Page 層 catch | `ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(Method), GetType(), additionalData: "描述")` |

| 回傳型別 | 安全預設值 |
|---------|-----------|
| `List<T>` | `new List<T>()` |
| `T?` | `null` |
| `bool` | `false` |
| `int` | `0` |
| `ServiceResult` | `ServiceResult.Failure("...")` |
| `ServiceResult<T>` | `ServiceResult<T>.Failure("...")` |
| `(List<T> Items, int TotalCount)` | `(new List<T>(), 0)` |

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md)
