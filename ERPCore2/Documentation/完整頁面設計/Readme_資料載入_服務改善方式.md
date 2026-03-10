# Index 頁面資料載入改善方式（伺服器端分頁）

## 更新日期
2026-03-10

---

## 概述

原本所有 Index 頁面都使用「**全量載入 + 客戶端分頁**」模式：

```
DB → 全部資料載入記憶體 → client-side filter → client-side Skip/Take → 顯示 N 筆
```

當資料量大或 JOIN 複雜時會造成查詢逾時（`Execution Timeout`）。
本文記錄改善為「**伺服器端分頁**」的方式與各頁面狀態。

---

## 架構設計

### 兩種模式共存（向下相容）

`GenericIndexPageComponent` 同時支援兩種模式，用參數決定走哪條路：

| 參數 | 模式 | 說明 |
|------|------|------|
| `DataLoader` | 客戶端全量 | 原有模式，全部資料載入記憶體，不需修改 |
| `ServerDataLoader` | 伺服器端分頁 | 新模式，DB 層過濾與分頁，優先使用 |

**設定 `ServerDataLoader` 時，`DataLoader` 會被忽略（不需移除，留作備用）。**

### 資料流比較

**客戶端模式（舊）：**
```
DB: SELECT * FROM Table + N個JOIN  →  List<全量>  →  Skip/Take = 顯示頁面資料
```

**伺服器端模式（新）：**
```
DB: SELECT COUNT(*) WHERE [條件]   →  totalCount
DB: SELECT TOP N WHERE [條件] SKIP →  pagedItems（只有本頁資料）
```

### filterFunc 設計原理

篩選邏輯由 `FieldConfiguration.ApplyFilters()` 提供，以 `Func<IQueryable<T>, IQueryable<T>>` 傳入 Service，確保過濾條件由 EF Core 翻譯成 SQL，全部在 DB 內執行：

```csharp
// Index 頁面的 ServerDataLoader 方法
private Task<(List<T> Items, int TotalCount)> ServerLoadDataAsync(
    SearchFilterModel filter, int page, int pageSize)
{
    return XxxService.GetPagedWithFiltersAsync(
        q => {
            q = fieldConfiguration.ApplyFilters(filter, q, nameof(ServerLoadDataAsync), GetType());
            // 備註篩選（GenericIndexPageComponent 標準篩選欄位）
            var remarks = filter.GetFilterValue("Remarks")?.ToString();
            if (!string.IsNullOrWhiteSpace(remarks))
                q = q.Where(e => e.Remarks != null && e.Remarks.Contains(remarks));
            return q;
        },
        page, pageSize);
}
```

### Debug Badge

SuperAdmin 在頁面左下角 Debug Badge 看到量測資訊：

```
🔧 SuperAdmin | AccountItemIndex | server 20/847 筆 | 12 ms
```

- `server`（綠色）/ `client`（黃色）— 當前使用的模式
- `20/847 筆` — 本頁載入筆數 / 符合條件總筆數
- `12 ms`（綠 < 500ms、黃 < 2000ms、紅 > 2000ms）— 查詢耗時

---

## 每個 Service 的修改步驟

每個頁面需修改三個地方，順序如下：

### 步驟 1：Service Interface（`IXxxService.cs`）

新增方法宣告：

```csharp
/// <summary>
/// 伺服器端分頁查詢（不載入 Include，僅取列表所需欄位）。
/// </summary>
Task<(List<Xxx> Items, int TotalCount)> GetPagedWithFiltersAsync(
    Func<IQueryable<Xxx>, IQueryable<Xxx>>? filterFunc,
    int pageNumber,
    int pageSize);
```

### 步驟 2：Service 實作（`XxxService.cs`）

```csharp
public async Task<(List<Xxx> Items, int TotalCount)> GetPagedWithFiltersAsync(
    Func<IQueryable<Xxx>, IQueryable<Xxx>>? filterFunc,
    int pageNumber,
    int pageSize)
{
    try
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // 基礎查詢：
        // - 不加 IsDraft 過濾（IsDraft 由 filterFunc 統一處理，不論有無 ShowDraftTab）
        // - 視欄位需求決定是否加 Include（若 Index 欄位有顯示關聯名稱則加）
        IQueryable<Xxx> query = context.XxxTable;
        // 若需要 Include，例如：
        // IQueryable<Xxx> query = context.XxxTable.Include(x => x.Supplier);

        if (filterFunc != null)
            query = filterFunc(query);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.排序欄位)           // 維持與原 GetAll 相同的排序
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
        return (new List<Xxx>(), 0);
    }
}
```

> **注意：不要加 Include**。Index 列表不需要導航屬性，Edit Modal 開啟時才由 `GetByIdAsync` 另外載入完整資料。

### 步驟 3：Index 頁面（`XxxIndex.razor`）

1. 移除 `DataLoader="@LoadDataAsync"` 和 `FilterApplier="@ApplyFilters"`
2. 加入 `ServerDataLoader="@ServerLoadDataAsync"`
3. 移除 `LoadDataAsync()` 和 `ApplyFilters()` 方法
4. 新增 `ServerLoadDataAsync()` 方法

```razor
<GenericIndexPageComponent ...
                          ServerDataLoader="@ServerLoadDataAsync"
                          ... />
```

```csharp
private Task<(List<Xxx> Items, int TotalCount)> ServerLoadDataAsync(
    SearchFilterModel filter, int page, int pageSize)
{
    return XxxService.GetPagedWithFiltersAsync(
        q => {
            // 統一 IsDraft 規則：無 ShowDraftTab 時 ShowDrafts=null → ?? false → 只看正式資料
            q = q.Where(x => x.IsDraft == (filter.ShowDrafts ?? false));
            // ⚠️ 必須做 null 檢查：ServerDataLoader 在 OnInitializedAsync 完成前就可能被呼叫
            if (fieldConfiguration != null)
                q = fieldConfiguration.ApplyFilters(filter, q, nameof(ServerLoadDataAsync), GetType());
            var remarks = filter.GetFilterValue("Remarks")?.ToString();
            if (!string.IsNullOrWhiteSpace(remarks))
                q = q.Where(x => x.Remarks != null && x.Remarks.Contains(remarks));
            return q;
        },
        page, pageSize);
}
```

> **⚠️ 重要：`fieldConfiguration` null 檢查是必要的。**
> Blazor Server 的 `GenericIndexPageComponent` 在 `OnAfterRenderAsync` 觸發初始載入，
> 時序上比父頁面 `OnInitializedAsync`（初始化 `fieldConfiguration`）更早一步，
> 若不加 null 檢查會拋出 `NullReferenceException`。
> `fieldConfiguration == null` 時跳過欄位篩選（IsDraft 仍正常過濾），
> 第二次載入（`OnInitializedAsync` 完成後）就有完整篩選。

---

## 各頁面狀態

### ✅ 已完成（不需再動）

| 頁面 | 模組 | 完成日期 | 備註 |
|------|------|----------|------|
| `AccountItemIndex.razor` | 會計 | 2026-03-10 | 首次實作，含 Debug Badge；已統一 IsDraft 至 filterFunc |
| `PurchaseOrderIndex.razor` | 採購 | 2026-03-10 | ShowDraftTab，Include Company+Supplier |
| `PurchaseReceivingIndex.razor` | 採購 | 2026-03-10 | ShowDraftTab，Include Supplier |
| `PurchaseReturnIndex.razor` | 採購 | 2026-03-10 | ShowDraftTab，Include Supplier |
| `PurchaseReturnReasonIndex.razor` | 採購 | 2026-03-10 | ShowDraftTab，無 Include |
| `JournalEntryIndex.razor` | 財務 | 2026-03-10 | 無 IsDraft，Include Company |
| `QuotationIndex.razor` | 銷售 | 2026-03-10 | ShowDraftTab，Include Customer |
| `SalesOrderIndex.razor` | 銷售 | 2026-03-10 | ShowDraftTab，Include Customer |
| `SalesDeliveryIndex.razor` | 銷售 | 2026-03-10 | ShowDraftTab，Include Customer |
| `SalesReturnIndex.razor` | 銷售 | 2026-03-10 | ShowDraftTab，Include Customer+ReturnReason；移除 InitializeBasicData |
| `CustomerVisitIndex.razor` | 客戶 | 2026-03-10 | 無 IsDraft，Include Customer |
| `SetoffDocumentIndex.razor` | 財務 | 2026-03-10 | 無 IsDraft，Include Company；SetoffType 由 closure 捕獲 |
| `InventoryTransactionIndex.razor` | 倉庫 | 2026-03-10 | 無 IsDraft，Include Warehouse |
| `WasteRecordIndex.razor` | 廢棄物 | 2026-03-10 | ShowDraftTab，Include WasteType |
| `StockTakingIndex.razor` | 倉庫 | 2026-03-10 | ShowDraftTab，Include Warehouse |
| `MaterialIssueIndex.razor` | 倉庫 | 2026-03-10 | ShowDraftTab，無 Include |
| `ErrorLogIndex.razor` | 系統 | 2026-03-10 | 無 IsDraft；currentFilter 由 closure 捕獲 |
| `CustomerIndex.razor` | 客戶 | 2026-03-10 | ShowDraftTab，無 Include |
| `SupplierIndex.razor` | 廠商 | 2026-03-10 | ShowDraftTab，無 Include |
| `ProductIndex.razor` | 商品 | 2026-03-10 | ShowDraftTab，Include ProductCategory+Unit+ProductionUnit+Size |
| `EmployeeIndex.razor` | 員工 | 2026-03-10 | ShowDraftTab，Include Dept+Position+Role；SuperAdmin closure 過濾 |
| `InventoryStockIndex.razor` | 倉庫 | 2026-03-10 | ShowDraftTab，Include Product.ProductCategory |
| `ProductCompositionIndex.razor` | 生產 | 2026-03-10 | ShowDraftTab，Include ParentProduct+CompositionCategory |
| `MaterialIndex.razor` | 生產 | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |
| `DocumentIndex.razor` | 文件 | 2026-03-10 | ShowDraftTab，Include DocumentCategory |
| `VehicleIndex.razor` | 車輛 | 2026-03-10 | ShowDraftTab，無 Include |
| `VehicleMaintenanceIndex.razor` | 車輛 | 2026-03-10 | ShowDraftTab，Include Vehicle |
| `EmployeeSalaryIndex.razor` | 薪資 | 2026-03-10 | 無 IsDraft（GetAllAsync），Include Employee |

---

### 🔴 高優先（已全部完成）

所有高優先頁面已於 2026-03-10 完成遷移。

---

### 🟡 中優先（已全部完成）

所有中優先頁面已於 2026-03-10 完成遷移。

備註：`PayrollIndex.razor` 為完全自訂頁面（非 GenericIndexPageComponent），無需遷移。

---

### 🟢 低優先（已全部完成）

所有低優先頁面已於 2026-03-10 完成遷移。

| 頁面 | Service Interface | 完成日期 | 備註 |
|------|------------------|----------|------|
| `DepartmentIndex.razor` | `IDepartmentService` | 2026-03-10 | ShowDraftTab，無 Include |
| `EmployeePositionIndex.razor` | `IEmployeePositionService` | 2026-03-10 | ShowDraftTab，無 Include |
| `RoleIndex.razor` | `IRoleService` | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |
| `PermissionIndex.razor` | `IPermissionManagementService` | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |
| `BankIndex.razor` | `IBankService` | 2026-03-10 | ShowDraftTab，無 Include |
| `CurrencyIndex.razor` | `ICurrencyService` | 2026-03-10 | ShowDraftTab，無 Include |
| `PaymentMethodIndex.razor` | `IPaymentMethodService` | 2026-03-10 | ShowDraftTab，無 Include |
| `ProductCategoryIndex.razor` | `IProductCategoryService` | 2026-03-10 | ShowDraftTab，無 Include |
| `UnitIndex.razor` | `IUnitService` | 2026-03-10 | ShowDraftTab，無 Include |
| `SizeIndex.razor` | `ISizeService` | 2026-03-10 | ShowDraftTab，無 Include |
| `CompositionCategoryIndex.razor` | `ICompositionCategoryService` | 2026-03-10 | ShowDraftTab，無 Include |
| `VehicleTypeIndex.razor` | `IVehicleTypeService` | 2026-03-10 | ShowDraftTab，無 Include |
| `WasteTypeIndex.razor` | `IWasteTypeService` | 2026-03-10 | ShowDraftTab，無 Include |
| `DocumentCategoryIndex.razor` | `IDocumentCategoryService` | 2026-03-10 | ShowDraftTab，無 Include |
| `SalesReturnReasonIndex.razor` | `ISalesReturnReasonService` | 2026-03-10 | ShowDraftTab，無 Include |
| `PurchaseReturnReasonIndex.razor` | `IPurchaseReturnReasonService` | 2026-03-10 | 已提前完成（先前版本）|
| `WarehouseIndex.razor` | `IWarehouseService` | 2026-03-10 | ShowDraftTab，無 Include |
| `WarehouseLocationIndex.razor` | `IWarehouseLocationService` | 2026-03-10 | ShowDraftTab，Include Warehouse（顯示名稱需要）|
| `PrinterConfigurationIndex.razor` | `IPrinterConfigurationService` | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |
| `PaperSettingIndex.razor` | `IPaperSettingService` | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |
| `ReportPrintConfigurationIndex.razor` | `IReportPrintConfigurationService` | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |
| `CompanyIndex.razor` | `ICompanyService` | 2026-03-10 | 無 IsDraft（GetAllAsync），無 Include |

---

## 特殊情況說明

### IsDraft 統一處理規則

**所有頁面一律採用相同的模式**，不論有無 `ShowDraftTab`：

1. **Service 基礎查詢**：不加任何 IsDraft 過濾
2. **filterFunc 內**：統一加入 `q = q.Where(x => x.IsDraft == (filter.ShowDrafts ?? false));`

```csharp
// 兩種頁面行為相同，只差 ShowDrafts 的值：
// 無 ShowDraftTab：ShowDrafts 永遠是 null → ?? false → 只顯示正式資料
// 有 ShowDraftTab：SwitchDraftTab() 設定 ShowDrafts = true/false → 動態切換
q = q.Where(x => x.IsDraft == (filter.ShowDrafts ?? false));
```

**好處**：未來任何頁面要加 ShowDraftTab，Service 不需修改，只要在 GenericIndexPageComponent 加 `ShowDraftTab="true"` 即可。

### 需要 Include 的欄位

若 Index 列表的欄位有顯示關聯資料（例如欄位為 `Supplier.CompanyName`），在 `GetPagedWithFiltersAsync` 基礎查詢加上 Include：

```csharp
IQueryable<PurchaseOrder> query = context.PurchaseOrders
    .Include(po => po.Company)
    .Include(po => po.Supplier);
```

Index 列表顯示的關聯欄位通常只有 1-2 個，Include 影響有限，比 Projection 簡單易維護。

---

## 不需修改的項目

下列已在基礎架構層完成，**所有頁面共用，不需個別修改**：

| 項目 | 檔案 | 說明 |
|------|------|------|
| `ServerDataLoader` 參數 | `GenericIndexPageComponent.razor.cs` | ✅ 已完成 |
| 伺服器端分頁路徑邏輯 | `GenericIndexPageComponent.DataLoader.cs` | ✅ 已完成 |
| 換頁/搜尋/換頁大小的分支邏輯 | `GenericIndexPageComponent.DataLoader.cs` | ✅ 已完成 |
| BatchDelete Modal 相容處理 | `GenericIndexPageComponent.razor` | ✅ 已完成 |
| Debug Badge 量測顯示 | `GenericIndexPageComponent.razor` + `.razor.cs` | ✅ 已完成 |
| `SearchFilterModel.ShowDrafts` 屬性 | `SearchFilterDefinition.cs` | ✅ 已完成（2026-03-10）|
| `SwitchDraftTab` 伺服器端支援 | `GenericIndexPageComponent.DataLoader.cs` | ✅ 已完成（2026-03-10）|
