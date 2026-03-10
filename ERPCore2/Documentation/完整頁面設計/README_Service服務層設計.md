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

**已實作此方法的服務**（詳細遷移紀錄見 [Readme_資料載入_服務改善方式.md](Readme_資料載入_服務改善方式.md)）：

| 模組 | 服務 |
|------|------|
| 採購 | `PurchaseOrderService`、`PurchaseReceivingService`、`PurchaseReturnService`、`PurchaseReturnReasonService` |
| 銷售 | `QuotationService`、`SalesOrderService`、`SalesDeliveryService`、`SalesReturnService`、`SalesReturnReasonService` |
| 財務 | `JournalEntryService`、`SetoffDocumentService`、`AccountItemService`、`BankService`、`CurrencyService`、`PaymentMethodService` |
| 客戶 | `CustomerService`、`CustomerVisitService` |
| 廠商 | `SupplierService` |
| 員工 | `EmployeeService`、`DepartmentService`、`EmployeePositionService`、`RoleService`、`PermissionManagementService` |
| 商品 | `ProductService`、`ProductCategoryService`、`UnitService`、`SizeService` |
| 生產 | `MaterialService`、`ProductCompositionService`、`CompositionCategoryService` |
| 倉庫 | `InventoryStockService`、`InventoryTransactionService`、`StockTakingService`、`MaterialIssueService`、`WarehouseService`、`WarehouseLocationService` |
| 車輛 | `VehicleService`、`VehicleMaintenanceService`、`VehicleTypeService` |
| 文件 | `DocumentService`、`DocumentCategoryService` |
| 廢棄物 | `WasteRecordService`、`WasteTypeService` |
| 薪資 | `EmployeeSalaryService`、`EmployeeBankAccountService`、`PayrollPeriodService`、`PayrollItemService` |
| 系統 | `CompanyService`、`PrinterConfigurationService`、`PaperSettingService`、`ReportPrintConfigurationService`、`ErrorLogService` |

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
- [Readme_資料載入_服務改善方式.md](Readme_資料載入_服務改善方式.md) — 伺服器端分頁遷移紀錄
