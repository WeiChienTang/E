# 資料重複讀取問題修正方案

> GenericEditModalComponent 導航機制優化與 Table 資料載入統一  
> 文件版本：v3.1 | 日期：2026-02-13  
> **狀態：所有模組已完成實作、編譯驗證通過**

---

## 1. 問題概述

在 ERPCore2 系統中，所有包含明細 Table 的 EditModalComponent 都存在兩個核心問題：

### 問題一：上/下一筆導航與 Table 控制邏輯重複

導航時 `NavigateToRecordAsync` 繞過 `DataLoader`，各 EditModal 必須在 `HandleEntityLoaded` 中重複寫「載入明細」邏輯。**8 個組件 × 2 處 = 16 處以上的重複程式碼**。

### 問題二：資料庫重複讀取

Table 組件的 `LoadExistingDetailsAsync` 對每筆明細逐筆查詢 `HasUsageRecord`（N+1 問題），`RefreshDetailsAsync` 造成二次處理，`LoadAdditionalDataAsync` 被多次呼叫。

### 1.1 受影響的組件

| 組件 | Table 數量 | Table 類型 | 完成狀態 |
|------|-----------|-----------|----------|
| PurchaseOrderEditModal | 1 | PurchaseOrderTable | ✅ 已完成 |
| PurchaseReceivingEditModal | 1 | PurchaseReceivingTable | ✅ 已完成 |
| PurchaseReturnEditModal | 1 | PurchaseReturnTable | ✅ 已完成 |
| QuotationEditModal | 1 | QuotationTable | ✅ 已完成 |
| SalesOrderEditModal | 1 | SalesOrderTable | ✅ 已完成 |
| SalesDeliveryEditModal | 1 | SalesDeliveryTable | ✅ 已完成 |
| SalesReturnEditModal | 1 | SalesReturnTable | ✅ 已完成 |
| SetoffDocumentEditModal | 3 | SetoffProductTable + SetoffPaymentTable + SetoffPrepaymentTable | ✅ 已完成 |
| ProductionScheduleEditModal | 2 | ProductionScheduleOrderTable + ProductionScheduleItemTable | ✅ 已完成（特殊模式） |

---

## 2. 問題根因分析

### 2.1 導航機制的架構缺陷

`GenericEditModalComponent` 的 `NavigateToRecordAsync` 方法在設計上繞過了 `DataLoader`，直接使用反射呼叫 `Service.GetByIdAsync` 載入主檔。這導致兩條載入路徑：

| 場景 | 資料來源 | 呼叫 DataLoader | 呼叫 OnEntityLoaded | 明細載入位置 |
|------|---------|----------------|---------------------|------------|
| 初次開啟 Modal | `DataLoader()` | ✅ 是 | ✖ 否 | 在 DataLoader 內 |
| 上/下一筆導航 | `Service.GetByIdAsync()` | ✖ 否 | ✅ 是 | 在 HandleEntityLoaded 內 |

每個 EditModal 都必須在兩個地方維護相同的「載入明細」邏輯：

- `DataLoader` 方法（編輯模式的分支）
- `HandleEntityLoaded` 事件處理器

#### 各組件 HandleEntityLoaded 的共同模式

```
HandleEntityLoaded(int loadedEntityId)
├── 1. 從 DB 重新載入明細 → LoadXxxDetails(id)
├── 2. StateHasChanged()
├── 3. (部分有) LoadDetailRelatedDataAsync()
├── 4. xxxDetailManager.RefreshDetailsAsync()
└── 5. StateHasChanged()  ← 第二次
```

### 2.2 資料庫重複讀取的四個來源

#### 來源 1：N+1 查詢問題（影響最大）

Table 組件的 `LoadExistingDetailsAsync` 對每筆 detail 逐筆呼叫 `HasUsageRecord`，實際上透過 `RelatedDocumentsHelper` 對 DB 做查詢。**10 筆明細就產生 10 次額外的 DB 查詢**。

```csharp
// 現狀：逐筆查詢
foreach (var detail in ExistingDetails)
{
    item.HasUsageRecordCache = await HasUsageRecord(item);  // 每筆一次 DB 查詢
}
```

#### 來源 2：DataLoader 與 HandleEntityLoaded 各自載入明細

雖然兩者不會在同一次生命週期內都執行，但共用的 `LoadDetailRelatedDataAsync` 內可能包含對每筆 detail 的逐筆檢查。

#### 來源 3：RefreshDetailsAsync 造成二次資料處理

`HandleEntityLoaded` 的流程：父組件更新 `xxxDetails` → `StateHasChanged()` → Table 收到新的 `ExistingDetails` 參數 → 又呼叫 `RefreshDetailsAsync()` → 再次執行 `LoadExistingDetailsAsync`（含逐筆 `HasUsageRecord`）。

#### 來源 4：LoadAdditionalDataAsync 被多次呼叫

此方法被註冊為 `ModalManagerInitHelper` 的回呼，同時在 `OnParametersSetAsync` 中也會呼叫。部分組件（如 SetoffDocument）即使已載入過資料，仍會再次呼叫。

---

## 3. 修正方案

> **方案總覽**  
> 共分三個階段執行：方案 A（統一導航路徑）、方案 C（批次查詢）、方案 D（自動參數偵測）。三者解決不同層面的問題，組合後可完整消除所有重複讀取。

### 方案對照表

| 問題 | 方案 A | 方案 C | 方案 D |
|------|-------|-------|-------|
| HandleEntityLoaded 與 DataLoader 重複 | ✅ 解決 | ✖ | ✖ |
| 8 個 EditModal 手寫雷同導航邏輯 | ✅ 解決 | ✖ | ✖ |
| N+1 逐筆查詢 HasUsageRecord | ✖ | ✅ 解決 | ✖ |
| RefreshDetailsAsync 二次處理 | ✖ | ✖ | ✅ 解決 |
| @key 造成元件重建 | ✖ | ✖ | ✅ 解決 |
| Task.Run/Delay 渲染風暴 | ✖ | ✖ | ✅ 解決 |
| IsLoading 狀態造成重複載入 | ✖ | ✖ | ✅ 解決 |

---

### 3.1 方案 A：讓 NavigateToRecordAsync 走 DataLoader 路徑

#### 核心思路

修改 `GenericEditModalComponent` 的 `NavigateToRecordAsync`，在載入主檔後也呼叫 `DataLoader`，而非只用反射呼叫 `Service.GetByIdAsync`。這樣導航和初次開啟都走同一條路徑，消除程式碼重複。

#### 具體修改內容

**Step 1：修改 GenericEditModalComponent.razor**

在 `NavigateToRecordAsync` 方法中，將原本透過反射呼叫 `Service.GetByIdAsync` 的邏輯，改為先更新 Id 參數，再呼叫 `DataLoader`。

```csharp
// 修改前（現行做法）
private async Task NavigateToRecordAsync(int targetId)
{
    _isNavigating = true;
    _currentId = targetId;
    // 使用反射呼叫 Service.GetByIdAsync
    var getByIdMethod = Service.GetType().GetMethod("GetByIdAsync");
    var loadedEntity = await getByIdTask;
    // ... 後續處理 ...
    if (OnEntityLoaded.HasDelegate)
        await OnEntityLoaded.InvokeAsync(targetId);
}
```

```csharp
// 修改後
private async Task NavigateToRecordAsync(int targetId)
{
    _isNavigating = true;
    _currentId = targetId;

    // 通知父組件更新 Id（確保 DataLoader 走編輯路徑）
    if (IdChanged.HasDelegate)
        await IdChanged.InvokeAsync(targetId);

    // 直接呼叫 DataLoader（包含載入主檔 + 明細）
    if (DataLoader != null)
    {
        var loadedEntity = await DataLoader();
        if (loadedEntity != null)
        {
            Entity = loadedEntity;
            editContext = new EditContext(Entity);
            UpdateAllActionButtons();
            await LoadStatusMessageData();
            await LoadNavigationStateAsync();
        }
    }

    // OnEntityLoaded 純粹用於 UI 刷新
    if (OnEntityLoaded.HasDelegate)
        await OnEntityLoaded.InvokeAsync(targetId);

    StateHasChanged();
    _isNavigating = false;
}
```

**Step 2：簡化所有 EditModal 的 HandleEntityLoaded**

修改後，所有包含 Table 的 EditModal 的 `HandleEntityLoaded` 都可以統一簡化為相同的模式：

```csharp
// 簡化後的 HandleEntityLoaded（所有組件統一）
private async Task HandleEntityLoaded(int loadedEntityId)
{
    // 明細已由 DataLoader 載入，只需刷新 Table UI
    StateHasChanged();
    if (xxxDetailManager != null)
        await xxxDetailManager.RefreshDetailsAsync();
    StateHasChanged();
}
```

#### 需要修改的檔案清單

| 檔案 | 修改內容 | 風險 |
|------|---------|------|
| GenericEditModalComponent.razor | 修改 NavigateToRecordAsync 方法 | **中** |
| PurchaseOrderEditModal | 簡化 HandleEntityLoaded | 低 |
| PurchaseReceivingEditModal | 簡化 HandleEntityLoaded | 低 |
| PurchaseReturnEditModal | 簡化 HandleEntityLoaded | 低 |
| QuotationEditModal | 簡化 HandleEntityLoaded | 低 |
| SalesOrderEditModal | 簡化 HandleEntityLoaded | 低 |
| SalesDeliveryEditModal | 簡化 HandleEntityLoaded | 低 |
| SalesReturnEditModal | 簡化 HandleEntityLoaded | 低 |
| SetoffDocumentEditModal | 簡化 HandleEntityLoaded（3 個 Table） | 低 |

> ⚠️ **注意事項**  
> 各 EditModal 的 DataLoader 已經包含 `if (!XxxId.HasValue)` 判斷新增/編輯模式。導航時透過 `IdChanged` 更新 Id 後，DataLoader 自然會走編輯路徑，不需要額外的標記或參數。但需要注意 DataLoader 內的 `Task.Run` 延遲呼叫是否會造成競爭條件。

---

### 3.2 方案 C：批次查詢取代逐筆查詢

#### 核心思路

在 Service 層新增批次查詢方法，將 N+1 查詢優化為單次查詢。此方案不改架構，只改查詢策略。

#### 具體修改內容

**Step 1：Service 層新增批次方法**

```csharp
// IRelatedDocumentsHelper 新增
Task<Dictionary<int, bool>> HasUsageRecordBatchAsync(
    List<int> detailIds);
```

```csharp
// 實作
public async Task<Dictionary<int, bool>>
    HasUsageRecordBatchAsync(List<int> detailIds)
{
    var result = new Dictionary<int, bool>();
    if (!detailIds.Any()) return result;

    // 單次 DB 查詢：找出所有有使用紀錄的 detailId
    var usedIds = await _context.XxxDetails
        .Where(d => detailIds.Contains(d.SourceDetailId))
        .Select(d => d.SourceDetailId)
        .Distinct()
        .ToListAsync();

    foreach (var id in detailIds)
        result[id] = usedIds.Contains(id);
    return result;
}
```

**Step 2：Table 組件改用批次方法**

```csharp
// 修改前（逐筆查詢）
foreach (var detail in ExistingDetails)
{
    item.HasUsageRecordCache = await HasUsageRecord(item);
}
```

```csharp
// 修改後（批次查詢）
var detailIds = ExistingDetails
    .Where(d => d.Id > 0)
    .Select(d => d.Id).ToList();
var usageMap = await RelatedDocumentsHelper
    .HasUsageRecordBatchAsync(detailIds);
foreach (var detail in ExistingDetails)
{
    item.HasUsageRecordCache =
        usageMap.GetValueOrDefault(detail.Id, false);
}
```

#### 效能對比

| 情境 | 修改前 DB 查詢次數 | 修改後 DB 查詢次數 |
|------|-------------------|-------------------|
| 10 筆明細 | **10 次** | **1 次** |
| 30 筆明細 | **30 次** | **1 次** |
| 50 筆明細 | **50 次** | **1 次** |

---

### 3.3 方案 D 改良版：DataVersion 追蹤（取代 ReferenceEquals）

#### 核心思路

原本方案 D 使用 `ReferenceEquals(ExistingDetails, _previousDetails)` 來偵測參數變化，但在 Blazor 渲染機制下會失效，因為父元件的 `StateHasChanged()` 或 Render 會創建新的 List 實例，即使內容相同。

**改良版使用整數 `DataVersion` 作為版本戳記**，每次父元件載入新資料時遞增，Table 組件偵測此值變化來判斷是否需重新載入。

#### 具體修改內容

**Step 1：EditModal 新增 DataVersion 計數器**

```csharp
// 在 EditModal 中宣告
private int _detailsDataVersion = 0;

// 在 LoadXxxDetails 方法中，每次載入後遞增
private async Task LoadPurchaseOrderDetails(int purchaseOrderId)
{
    purchaseOrderDetails = await XxxService.GetOrderDetailsAsync(purchaseOrderId);
    if (purchaseOrderDetails == null)
        purchaseOrderDetails = new List<XxxDetail>();
    
    // 🔥 方案 D 改良版：遞增版本號，通知 Table 重新載入
    _detailsDataVersion++;
}
```

**Step 2：傳遞 DataVersion 到 Table 組件**

```razor
<PurchaseOrderTable 
    ...
    ExistingDetails="@purchaseOrderDetails"
    DataVersion="@_detailsDataVersion"
    ...
/>
```

**Step 3：Table 組件偵測 DataVersion 變化**

```csharp
// 在 Table 組件中宣告
[Parameter] public int DataVersion { get; set; } = 0;
private int _previousDataVersion = 0;
private int? _previousSelectedSupplierId = null;

protected override async Task OnParametersSetAsync()
{
    base.OnParametersSet();
    
    // 🔥 防止重入
    if (_isLoadingDetails) return;
    
    // 🔥 方案 D 改良版：優先檢查 DataVersion
    bool versionChanged = DataVersion != _previousDataVersion;
    
    if (versionChanged)
    {
        // 同時更新所有追蹤變數，避免 supplierChanged 誤判
        _previousDataVersion = DataVersion;
        _previousSelectedSupplierId = SelectedSupplierId;
        
        _isLoadingDetails = true;
        try
        {
            await LoadExistingDetailsAsync();
            tableComponent?.RefreshEmptyRow();
        }
        finally
        {
            _isLoadingDetails = false;
        }
    }
    else
    {
        // 只有 DataVersion 沒變時，才檢查 supplierChanged
        // 用於使用者手動更換廠商的情況
        bool supplierChanged = _previousSelectedSupplierId != SelectedSupplierId;
        
        if (supplierChanged)
        {
            _previousSelectedSupplierId = SelectedSupplierId;
            _isLoadingDetails = true;
            try
            {
                ProductItems.Clear();
                await LoadExistingDetailsAsync();
                await CheckLastPurchaseRecordAsync();
            }
            finally
            {
                _isLoadingDetails = false;
            }
        }
    }
}
```

> ⚠️ **關鍵設計**  
> - `versionChanged` 優先於 `supplierChanged`，確保導航載入新資料時不會被廠商變更誤判
> - 同時更新 `_previousDataVersion` 和 `_previousSelectedSupplierId`，避免連鎖觸發
> - 使用 `_isLoadingDetails` 防止重入

---

### 3.4 額外必要修正：Blazor 生命週期問題

在實際測試 PurchaseOrder 時發現，方案 A+C+D 仍無法完全解決重複讀取，還需要處理以下 Blazor 生命週期問題：

#### 3.4.1 移除 @key 指令（避免元件全部重建）

**問題**：`@key="@editModalComponent.Entity.Id"` 會在導航時銷毀並重建整個 Table 元件，導致 `OnInitializedAsync` 重新執行。

**修正**：移除 Table 元件上的 `@key` 指令。

```razor
<!-- ❌ 錯誤：會導致元件重建 -->
<PurchaseOrderTable @key="@editModalComponent.Entity.Id" ... />

<!-- ✅ 正確：移除 @key -->
<PurchaseOrderTable @ref="purchaseOrderDetailManager" ... />
```

#### 3.4.2 移除 Task.Run + Task.Delay 延遲模式（避免渲染風暴）

**問題**：部分 DataLoader 使用 `Task.Run` 搭配 `Task.Delay(10)` 來延遲呼叫 `InitializeFormFieldsAsync`，這會造成 30+ 次 `OnParametersSetAsync` 觸發。

**修正**：直接 `await` 呼叫，不使用 Task.Run。

```csharp
// ❌ 錯誤：會造成渲染風暴
_ = Task.Run(async () =>
{
    await Task.Delay(10);
    await InvokeAsync(async () =>
    {
        await InitializeFormFieldsAsync();
        StateHasChanged();
    });
});

// ✅ 正確：直接 await
await InitializeFormFieldsAsync();
// NavigateToRecordAsync 會在 DataLoader 返回後呼叫 StateHasChanged()
```

#### 3.4.3 新增 IsParentLoading 參數（避免載入中的重複讀取）

**問題**：當 `GenericEditModalComponent` 的 `IsLoading` 從 `true` 變為 `false` 時，會觸發重新渲染。此時 Table 元件的 `OnInitializedAsync` 可能執行，但 `ExistingDetails` 還是舊資料。

**修正**：

1. **GenericEditModalComponent**：將 `IsLoading` 屬性改為 public（保留 private setter）

```csharp
// 修改前
private bool IsLoading { get; set; } = false;

// 修改後
public bool IsLoading { get; private set; } = false;
```

2. **Table 組件**：新增 `IsParentLoading` 參數

```csharp
// 在 Table 組件中新增
[Parameter] public bool IsParentLoading { get; set; } = false;

protected override async Task OnInitializedAsync()
{
    _previousSelectedSupplierId = SelectedSupplierId;
    
    // 🔥 修正：如果父元件正在載入中，跳過資料載入
    // ⚠️ 關鍵：此時不設定 _previousDataVersion，讓 OnParametersSetAsync 能夠偵測到變化
    if (IsParentLoading)
    {
        return;
    }
    
    // 只有在成功載入後才設定 _previousDataVersion
    _previousDataVersion = DataVersion;
    
    await LoadExistingDetailsAsync();
    await CheckLastPurchaseRecordAsync();
}
```

> ⚠️ **v3.1 重要修正**  
> `_previousDataVersion = DataVersion` 必須在 `IsParentLoading` 檢查**之後**執行。  
> 如果在檢查之前設定，當 `IsParentLoading` 為 `true` 時會跳過載入，但 `_previousDataVersion` 已被設定，  
> 導致後續 `OnParametersSetAsync` 無法偵測到 `DataVersion` 變化，造成商品無法正確顯示、無法自動增加下一行。

3. **EditModal 綁定 IsParentLoading**

```razor
<PurchaseOrderTable 
    ...
    IsParentLoading="@(editModalComponent?.IsLoading ?? false)"
    ...
/>
```

#### 3.4.4 使用 CSS 隱藏取代條件渲染（保持元件存活）

**問題**：`@if (entity.SupplierId > 0)` 條件渲染會在條件變化時銷毀/重建 Table 元件。

**修正**：改用 CSS `display:none` 隱藏，保持元件存在但不顯示。

```razor
<!-- ❌ 錯誤：條件渲染會銷毀元件 -->
@if (editModalComponent.Entity.SupplierId > 0)
{
    <PurchaseOrderTable ... />
}

<!-- ✅ 正確：CSS 隱藏 + 顯示提示 -->
@if (editModalComponent.Entity.SupplierId <= 0)
{
    <div class="alert alert-info text-center">
        請先選擇廠商後再進行明細管理
    </div>
}

<div style="@(editModalComponent.Entity.SupplierId > 0 ? "" : "display:none")">
    <PurchaseOrderTable ... />
</div>
```

---

## 4. 實施計畫

### 4.1 修改順序（已驗證）

以下順序已在 PurchaseOrder 模組驗證通過：

| 步驟 | 修改項目 | 涉及檔案 |
|------|---------|---------|
| 1 | 方案 C：新增批次查詢方法 | `RelatedDocumentsHelper.cs`、`IRelatedDocumentsHelper.cs` |
| 2 | 方案 A：已由架構完成 | `GenericEditModalComponent.razor` |
| 3 | 方案 D：Table 新增 DataVersion 參數 | `XxxTable.razor` |
| 4 | 方案 D：EditModal 新增 _detailsDataVersion | `XxxEditModalComponent.razor` |
| 5 | 修正：移除 @key | `XxxEditModalComponent.razor` |
| 6 | 修正：移除 Task.Run/Delay | `XxxEditModalComponent.razor` |
| 7 | 修正：IsParentLoading 參數 | `XxxTable.razor`、`XxxEditModalComponent.razor` |
| 8 | 修正：CSS 隱藏取代條件渲染 | `XxxEditModalComponent.razor` |

### 4.2 驗證測試項目

使用 ConsoleHelper 監控以下步驟，確保每步只有 1 次 DB 查詢：

1. **步驟 1**：點擊 Index 列表開啟記錄 → 檢查 `LoadExistingDetailsAsync` 執行次數
2. **步驟 2**：點擊「下一筆」導航 → 檢查 `LoadExistingDetailsAsync` 執行次數
3. **步驟 3**：點擊「上一筆」導航 → 檢查 `LoadExistingDetailsAsync` 執行次數  
4. **步驟 4**：修改欄位數值 → 確認不觸發 `LoadExistingDetailsAsync`
5. **步驟 5**：點擊「儲存」按鈕 → 檢查 `LoadExistingDetailsAsync` 只執行 1 次

**監控程式碼範例**（可在修改完成後移除）：

```csharp
// 在 Table 組件的 LoadExistingDetailsAsync 開頭加入
ConsoleHelper.WriteInfo($"[XxxTable] LoadExistingDetailsAsync - ExistingDetails.Count={ExistingDetails?.Count ?? 0}");

// 在 Table 組件的 OnParametersSetAsync 加入
ConsoleHelper.WriteDebug($"[XxxTable] OnParametersSetAsync - versionChanged={versionChanged} (v{_previousDataVersion}→{DataVersion})");

// 在 Table 組件的 OnInitializedAsync 加入
if (IsParentLoading)
{
    ConsoleHelper.WriteWarning($"[XxxTable] OnInitializedAsync - 跳過（父元件載入中）");
    return;
}
```

### 4.3 各模組實作檢查清單

每個模組完成後，確認以下項目：

- [ ] **Table 組件**
  - [ ] 新增 `[Parameter] public int DataVersion { get; set; } = 0;`
  - [ ] 新增 `private int _previousDataVersion = 0;`
  - [ ] 新增 `[Parameter] public bool IsParentLoading { get; set; } = false;`
  - [ ] 新增 `private bool _isLoadingDetails = false;` 防止重入
  - [ ] `OnInitializedAsync` 檢查 IsParentLoading
  - [ ] `OnParametersSetAsync` 使用 versionChanged 優先邏輯
  - [ ] `LoadExistingDetailsAsync` 使用批次查詢

- [ ] **EditModal 組件**
  - [ ] 新增 `private int _detailsDataVersion = 0;`
  - [ ] `LoadXxxDetails` 方法末尾遞增 `_detailsDataVersion++`
  - [ ] Table 綁定傳入 `DataVersion="@_detailsDataVersion"`
  - [ ] Table 綁定傳入 `IsParentLoading="@(editModalComponent?.IsLoading ?? false)"`
  - [ ] 移除 Table 上的 `@key` 指令
  - [ ] 移除 `Task.Run` + `Task.Delay` 延遲模式
  - [ ] 條件顯示改用 CSS `display:none`
  - [ ] `HandleEntityLoaded` 簡化為只呼叫 `StateHasChanged()`

---

## 5. 預期效果

| 指標 | 修改前 | 修改後 |
|------|--------|--------|
| 導航時明細載入邏輯重複處數 | **16+ 處** | **0 處** |
| HandleEntityLoaded 程式碼行數（單個組件） | **10~25 行** | **1~3 行** |
| 載入 10 筆明細的 DB 查詢次數 | **10+ 次** | **1 次** |
| RefreshDetailsAsync 的冗餘呼叫 | **每次導航 1 次** | **0 次** |
| 8 個 EditModal 的 HandleEntityLoaded 一致性 | **各自不同** | **完全統一** || OnParametersSetAsync 觸發次數（導航） | **30+ 次** | **1~2 次** |

---

## 6. 參考實作：PurchaseOrder（完整範例）

以下為已驗證通過的完整實作程式碼，可作為其他模組的參考：

### 6.1 PurchaseOrderTable.razor（關鍵程式碼）

```csharp
@code {
    // ===== 方案 D 改良版：新增參數 =====
    [Parameter] public int DataVersion { get; set; } = 0;
    private int _previousDataVersion = 0;
    
    // ===== 額外修正：新增參數 =====
    [Parameter] public bool IsParentLoading { get; set; } = false;
    
    // ===== 防止重入 =====
    private bool _isLoadingDetails = false;
    private int? _previousSelectedSupplierId = null;

    protected override async Task OnInitializedAsync()
    {
        // 初始化追蹤變數
        _previousSelectedSupplierId = SelectedSupplierId;
        
        // 🔥 修正：如果父元件正在載入中，跳過資料載入
        // ⚠️ 關鍵：此時不設定 _previousDataVersion，讓 OnParametersSetAsync 能偵測到變化
        if (IsParentLoading)
        {
            return;
        }
        
        // 只有在成功載入後才設定 _previousDataVersion
        _previousDataVersion = DataVersion;
        
        await LoadExistingDetailsAsync();
        await CheckLastPurchaseRecordAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        base.OnParametersSet();
        
        // 🔥 防止重入
        if (_isLoadingDetails)
        {
            ConsoleHelper.WriteWarning($"[PurchaseOrderTable] OnParametersSetAsync - 跳過（正在載入中）");
            return;
        }
        
        // 🔥 方案 D 改良版：優先檢查 DataVersion
        bool versionChanged = DataVersion != _previousDataVersion;
        
        ConsoleHelper.WriteDebug($"[PurchaseOrderTable] OnParametersSetAsync - versionChanged={versionChanged}");
        
        if (versionChanged)
        {
            // 同時更新所有追蹤變數
            _previousDataVersion = DataVersion;
            _previousSelectedSupplierId = SelectedSupplierId;
            
            _isLoadingDetails = true;
            try
            {
                await LoadExistingDetailsAsync();
                tableComponent?.RefreshEmptyRow();
            }
            finally
            {
                _isLoadingDetails = false;
            }
        }
        else
        {
            bool supplierChanged = _previousSelectedSupplierId != SelectedSupplierId;
            
            if (supplierChanged)
            {
                _previousSelectedSupplierId = SelectedSupplierId;
                
                _isLoadingDetails = true;
                try
                {
                    ProductItems.Clear();
                    await LoadExistingDetailsAsync();
                    await CheckLastPurchaseRecordAsync();
                }
                finally
                {
                    _isLoadingDetails = false;
                }
            }
        }
    }

    private async Task LoadExistingDetailsAsync()
    {
        // 🔍 監控
        ConsoleHelper.WriteInfo($"[PurchaseOrderTable] LoadExistingDetailsAsync - Count={ExistingDetails?.Count ?? 0}");
        
        if (ExistingDetails?.Any() != true) return;

        _dataLoadCompleted = false;
        ProductItems.Clear();
        
        // 🔥 方案 C：批次查詢
        var detailIds = ExistingDetails
            .Select(d => GetPropertyValue<int>(d, "Id"))
            .Where(id => id > 0)
            .ToList();
        
        var usageRecordMap = detailIds.Any() 
            ? await RelatedDocumentsHelper.HasUsageRecordBatchForPurchaseOrderDetailsAsync(detailIds)
            : new Dictionary<int, bool>();
        
        foreach (var detail in ExistingDetails)
        {
            // ... 建立 ProductItem ...
            var detailId = GetPropertyValue<int>(detail, "Id");
            item.HasUsageRecordCache = usageRecordMap.GetValueOrDefault(detailId, false);
            ProductItems.Add(item);
        }
        
        _dataLoadCompleted = true;
        StateHasChanged();
    }
}
```

### 6.2 PurchaseOrderEditModalComponent.razor（關鍵程式碼）

```csharp
@code {
    // ===== 方案 D 改良版：版本計數器 =====
    private int _detailsDataVersion = 0;

    private async Task LoadPurchaseOrderDetails(int purchaseOrderId)
    {
        try
        {
            purchaseOrderDetails = await PurchaseOrderService.GetOrderDetailsAsync(purchaseOrderId);
            
            if (purchaseOrderDetails == null)
                purchaseOrderDetails = new List<PurchaseOrderDetail>();
            
            // 🔥 方案 D 改良版：遞增版本號
            _detailsDataVersion++;
            ConsoleHelper.WriteStep(0, $"[EditModal] LoadDetails - 版本更新至 {_detailsDataVersion}");
            
            var hasReceiving = purchaseOrderDetails.Any(d => d.ReceivedQuantity > 0);
            await HandleHasUndeletableDetailsChanged(hasReceiving);
        }
        catch (Exception ex)
        {
            purchaseOrderDetails = new List<PurchaseOrderDetail>();
        }
    }

    private async Task<PurchaseOrder?> LoadPurchaseOrderData()
    {
        // ... 新增模式略 ...
        
        // 編輯模式
        var purchaseOrder = await PurchaseOrderService.GetByIdAsync(PurchaseOrderId.Value);
        
        if (purchaseOrder != null)
        {
            await LoadPurchaseOrderDetails(PurchaseOrderId.Value);
            
            // 🔥 修正：直接 await，不使用 Task.Run
            await InitializeFormFieldsAsync();
        }
        
        return purchaseOrder;
    }

    /// <summary>
    /// 🔥 方案 A + D：HandleEntityLoaded 簡化版
    /// </summary>
    private async Task HandleEntityLoaded(int loadedEntityId)
    {
        try
        {
            canCreateReceiving = true;
            canCopyMessage = true;
            
            // 明細已由 DataLoader 載入，Table 會自動偵測 DataVersion 變化
            StateHasChanged();
        }
        catch (Exception ex)
        {
            // 錯誤處理
        }
    }
}
```

```razor
@* 🔥 修正：移除 @key，使用 CSS 隱藏 *@
@if (editModalComponent.Entity.SupplierId <= 0)
{
    <div class="alert alert-info text-center">請先選擇廠商</div>
}

<div style="@(editModalComponent.Entity.SupplierId > 0 ? "" : "display:none")">
    <PurchaseOrderTable @ref="purchaseOrderDetailManager"
                       TMainEntity="PurchaseOrder" 
                       TDetailEntity="PurchaseOrderDetail"
                       Products="@availableProducts"
                       SelectedSupplierId="@editModalComponent.Entity.SupplierId"
                       MainEntity="@editModalComponent.Entity"
                       ExistingDetails="@purchaseOrderDetails"
                       DataVersion="@_detailsDataVersion"
                       IsParentLoading="@(editModalComponent?.IsLoading ?? false)"
                       OnDetailsChanged="@HandleDetailsChanged"
                       ... />
</div>
```

---

## 7. 批次查詢方法對應表

各模組需要使用的批次查詢方法（已在 RelatedDocumentsHelper 中實作）：

| 模組 | Table 元件 | 批次查詢方法 |
|------|-----------|-------------|
| PurchaseOrder | PurchaseOrderTable | `HasUsageRecordBatchForPurchaseOrderDetailsAsync` |
| PurchaseReceiving | PurchaseReceivingTable | `HasUsageRecordBatchForPurchaseReceivingDetailsAsync` |
| PurchaseReturn | PurchaseReturnTable | `HasUsageRecordBatchForPurchaseReturnDetailsAsync` |
| Quotation | QuotationTable | `HasUsageRecordBatchForQuotationDetailsAsync` |
| SalesOrder | SalesOrderTable | `HasUsageRecordBatchForSalesOrderDetailsAsync` |
| SalesDelivery | SalesDeliveryTable | `HasUsageRecordBatchForSalesDeliveryDetailsAsync` |
| SalesReturn | SalesReturnTable | `HasUsageRecordBatchForSalesReturnDetailsAsync` |
| SetoffDocument | SetoffProductTable | `HasUsageRecordBatchForSetoffProductDetailsAsync` |
| SetoffDocument | SetoffPaymentTable | `HasUsageRecordBatchForSetoffPaymentDetailsAsync` |
| SetoffDocument | SetoffPrepaymentTable | `HasUsageRecordBatchForSetoffPrepaymentDetailsAsync` |

---

## 8. 重要注意事項

### 8.1 GenericEditModalComponent.IsLoading 已改為 public

此修改已完成，所有模組可直接使用：

```csharp
// 位置：Components/Shared/Modal/GenericEditModalComponent.razor
public bool IsLoading { get; private set; } = false;
```

### 8.2 ConsoleHelper 監控程式碼

**已移除**：所有 ConsoleHelper 監控程式碼均已從生產程式碼中移除，以減少 Console 輸出並提高效能。

### 8.3 SetoffDocument 特殊處理

SetoffDocument 有 3 個 Table，使用 3 個獨立的 `_detailsDataVersion` 計數器：
- `_productDetailsDataVersion` - SetoffProductTable
- `_paymentDetailsDataVersion` - SetoffPaymentTable  
- `_prepaymentDetailsDataVersion` - SetoffPrepaymentTable

所有 3 個 Table 都綁定 `IsParentLoading`，並在 `LoadSetoffDocumentDetails` 結束時同時遞增所有計數器。

### 8.4 ProductionSchedule 特殊處理（不同模式）

ProductionScheduleEditModal 的兩個 Table（ProductionScheduleOrderTable、ProductionScheduleItemTable）**沒有自己的資料庫載入邏輯**，它們只透過 Parameter 接收父組件傳入的資料。因此不需要 DataVersion/IsParentLoading 參數。

取而代之，在 EditModal 中使用**載入防護機制**：

```csharp
// 防止重複載入的標記
private bool _isLoadingScheduleItems = false;
private bool _isLoadingPendingDetails = false;
private int? _lastLoadedScheduleId = null;

private async Task LoadScheduleItemsAsync(int scheduleId, bool forceReload = false)
{
    // 防止並發載入
    if (_isLoadingScheduleItems) return;
    
    // 防止對相同 ID 的重複載入
    if (!forceReload && _lastLoadedScheduleId == scheduleId && currentScheduleItems.Any())
        return;
    
    _isLoadingScheduleItems = true;
    _lastLoadedScheduleId = scheduleId;
    
    try {
        // 實際載入邏輯...
    }
    finally {
        _isLoadingScheduleItems = false;
    }
}
```

**重點**：
- 儲存後需使用 `forceReload: true` 強制重載
- Modal 關閉時重設所有狀態標記，確保下次開啟時乾淨狀態

---

## 9. 實作完成總結

### 9.1 已完成的模組

| 模組 | 模式 | 驗證狀態 |
|------|------|---------|
| PurchaseOrderEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| PurchaseReceivingEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| PurchaseReturnEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| QuotationEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| SalesOrderEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| SalesDeliveryEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| SalesReturnEditModal | DataVersion + IsParentLoading | ✅ 編譯通過 |
| SetoffDocumentEditModal | 3× DataVersion + IsParentLoading | ✅ 編譯通過 |
| ProductionScheduleEditModal | 載入防護機制 | ✅ 編譯通過 |

### 9.2 關鍵修改項目

1. **GenericEditModalComponent.IsLoading** - 改為 `public` with `private set`
2. **Table 組件** - 新增 `DataVersion`、`IsParentLoading` 參數，`OnInitializedAsync` 和 `OnParametersSetAsync` 使用版本追蹤
3. **EditModal 組件** - 新增 `_detailsDataVersion` 計數器，移除 `@key`，CSS 隱藏取代條件渲染
4. **批次查詢** - `RelatedDocumentsHelper` 已實作各模組的批次查詢方法
5. **ConsoleHelper** - 所有監控程式碼已移除

### 9.3 v3.1 重要修正（2026-02-13）

**問題描述**：Table 組件在新增模式下，選擇商品時無法正確顯示，且無法自動增加下一行。

**根本原因**：`OnInitializedAsync` 中的 `_previousDataVersion = DataVersion` 在 `IsParentLoading` 檢查**之前**執行。

當 `IsParentLoading` 為 `true` 時：
1. `_previousDataVersion` 被設為當前的 `DataVersion`（例如 0）
2. `LoadProductsAsync()` 沒有執行（因為 return）
3. 稍後 `IsParentLoading` 變為 `false`，但 `DataVersion` 沒有變（仍然是 0）
4. `OnParametersSetAsync` 被觸發，但 `versionChanged` 是 `false`
5. 結果：商品清單永遠沒有被載入

**修正**：將 `_previousDataVersion = DataVersion` 移到 `IsParentLoading` 檢查**之後**，只有在成功載入後才設定。

**受影響組件**（已全部修正）：
- PurchaseOrderTable
- PurchaseReceivingTable
- PurchaseReturnTable
- QuotationTable
- SalesOrderTable
- SalesDeliveryTable
- SalesReturnTable
- SetoffPaymentTable
- SetoffProductTable
- SetoffPrepaymentTable