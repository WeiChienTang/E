# 資料重複讀取問題修正方案

> GenericEditModalComponent 導航機制優化與 Table 資料載入統一  
> 文件版本：v1.0 | 日期：2026-02-13

---

## 1. 問題概述

在 ERPCore2 系統中，所有包含明細 Table 的 EditModalComponent 都存在兩個核心問題：

### 問題一：上/下一筆導航與 Table 控制邏輯重複

導航時 `NavigateToRecordAsync` 繞過 `DataLoader`，各 EditModal 必須在 `HandleEntityLoaded` 中重複寫「載入明細」邏輯。**8 個組件 × 2 處 = 16 處以上的重複程式碼**。

### 問題二：資料庫重複讀取

Table 組件的 `LoadExistingDetailsAsync` 對每筆明細逐筆查詢 `HasUsageRecord`（N+1 問題），`RefreshDetailsAsync` 造成二次處理，`LoadAdditionalDataAsync` 被多次呼叫。

### 1.1 受影響的組件

| 組件 | Table 數量 | Table 類型 | 影響程度 |
|------|-----------|-----------|----------|
| PurchaseOrderEditModal | 1 | PurchaseOrderTable | 中 |
| PurchaseReceivingEditModal | 1 | PurchaseReceivingTable | 中 |
| PurchaseReturnEditModal | 1 | PurchaseReturnTable | 中 |
| QuotationEditModal | 1 | QuotationTable | 中 |
| SalesOrderEditModal | 1 | SalesOrderTable | 中 |
| SalesDeliveryEditModal | 1 | SalesDeliveryTable | 中 |
| SalesReturnEditModal | 1 | SalesReturnTable | 中 |
| SetoffDocumentEditModal | 3 | SetoffProductTable + SetoffPaymentTable + SetoffPrepaymentTable | **高** |

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

### 3.3 方案 D：讓 Table 自動偵測參數變化

#### 核心思路

讓 Table 組件在 `OnParametersSet` 中偵測 `ExistingDetails` 參數的變化，自動觸發資料重新處理，不再需要父組件手動呼叫 `RefreshDetailsAsync`。

#### 具體修改內容

**Step 1：Table 組件新增參數變更偵測**

```csharp
// 在 Table 組件中新增
private List<TDetailEntity>? _previousDetails;

protected override async Task OnParametersSetAsync()
{
    // 偵測 ExistingDetails 參照是否變更
    if (!ReferenceEquals(ExistingDetails, _previousDetails))
    {
        _previousDetails = ExistingDetails;
        await LoadExistingDetailsAsync();
        tableComponent?.RefreshEmptyRow();
        StateHasChanged();
    }
}
```

**Step 2：父組件移除手動 RefreshDetailsAsync 呼叫**

```csharp
// HandleEntityLoaded 最終形態（結合方案 A + D）
private async Task HandleEntityLoaded(int loadedEntityId)
{
    // 明細已由 DataLoader 載入
    // Table 自動偵測參數變化並刷新
    StateHasChanged();  // 只需一次
}
```

> ✅ **效果**  
> 三個方案全部完成後，`HandleEntityLoaded` 從原本的 5+ 行程式碼簡化為僅呼叫 `StateHasChanged()`，所有 8 個組件完全統一。

---

## 4. 實施計畫

### 4.1 建議執行順序

| 階段 | 方案 | 修改範圍 | 風險 | 效益 |
|------|------|---------|------|------|
| 第一階段 | C - 批次查詢 | Service 層 + Table 組件 | **低** | **效能提升最大** |
| 第二階段 | A - 統一導航 | GenericEditModal + 8 個 EditModal | **中** | **消除程式碼重複** |
| 第三階段 | D - 自動參數偵測 | Table 組件 | **低** | **消除冗餘刷新** |

### 4.2 每階段驗證重點

#### 第一階段驗證（方案 C）

- 比對修改前後，開啟一筆有 10+ 明細的採購單，觀察 SQL 查詢次數是否從 N 次降為 1 次
- 確認 `HasUsageRecordCache` 的結果與逐筆查詢一致
- 測試新增明細（Id = 0）的情境是否正確跳過

#### 第二階段驗證（方案 A）

- 測試所有 8 個 EditModal 的上/下一筆導航
- 確認導航後明細 Table 顯示正確的資料
- 確認新增模式不受影響（DataLoader 的 `if (!Id.HasValue)` 分支仍正常運作）
- 特別注意 SetoffDocument 的 3 個 Table 是否都正確刷新
- 測試轉單功能（進貨轉沖款、銷貨轉沖款）是否正常

#### 第三階段驗證（方案 D）

- 確認移除 `RefreshDetailsAsync` 後，導航切換時 Table 仍能正確重新載入
- 確認空行自動管理仍然正常運作
- 確認儲存後的 Table 刷新不受影響

---

## 5. 預期效果

| 指標 | 修改前 | 修改後 |
|------|--------|--------|
| 導航時明細載入邏輯重複處數 | **16+ 處** | **0 處** |
| HandleEntityLoaded 程式碼行數（單個組件） | **10~25 行** | **1~3 行** |
| 載入 10 筆明細的 DB 查詢次數 | **10+ 次** | **1 次** |
| RefreshDetailsAsync 的冗餘呼叫 | **每次導航 1 次** | **0 次** |
| 8 個 EditModal 的 HandleEntityLoaded 一致性 | **各自不同** | **完全統一** |
