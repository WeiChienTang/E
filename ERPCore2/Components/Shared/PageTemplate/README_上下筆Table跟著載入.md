# 上下筆切換時 Table 明細自動載入功能

## 📋 問題描述

在使用 `GenericEditModalComponent` 的上下筆導航功能時，發現一個問題：

- ✅ **主檔資料**會正確切換到另一筆
- ❌ **明細資料（Table）**不會跟著更新，仍顯示上一筆的明細

### 問題根源分析

```
上下筆切換流程：
NavigateToRecordAsync (GenericEditModalComponent)
  ↓
使用 Service.GetByIdAsync 直接載入新的 Entity
  ↓
觸發 IdChanged.InvokeAsync(targetId) → 更新 PurchaseOrderId
  ↓
❌ 但 PurchaseOrderEditModalComponent 不會重新執行 LoadPurchaseOrderData()
  ↓
❌ purchaseOrderDetails 沒有被重新載入
  ↓
❌ PurchaseOrderTable 的 ExistingDetails 參數沒有變化
  ↓
❌ Table 不會重新渲染新的明細
```

**核心問題：** `NavigateToRecordAsync` 為了優化性能，直接設置了 `Entity`，不走 `DataLoader`，導致父組件（EditModal）沒有機會重新載入明細資料。

---

## 💡 解決方案：事件驅動架構（方案 A）

採用**事件通知機制**，讓 `GenericEditModalComponent` 在導航完成時通知父組件，由父組件決定是否需要重新載入相關資料。

### 設計優點

1. ✅ **通用性**：所有使用 `GenericEditModalComponent` 的頁面都可選擇性監聽此事件
2. ✅ **靈活性**：不同的業務邏輯可以有不同的處理方式
3. ✅ **解耦合**：不修改 Table 元件，保持元件職責單一
4. ✅ **向下相容**：不影響不需要此功能的現有頁面

---

## 🔧 實作步驟

### 步驟 1：在 `GenericEditModalComponent` 新增事件參數

**檔案：** `GenericEditModalComponent.razor`

**位置：** 約第 335 行，EventCallback 參數區域

```csharp
// 委派參數 - 事件處理
[Parameter] public EventCallback OnSaveSuccess { get; set; }
[Parameter] public EventCallback OnSaveFailure { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public EventCallback OnPrint { get; set; }
[Parameter] public Func<(string PropertyName, object? Value), Task>? OnFieldChanged { get; set; }

/// <summary>
/// 實體載入完成事件（導航切換時觸發）
/// 參數為已載入的實體 ID，用於通知父組件重新載入相關資料（如明細）
/// </summary>
[Parameter] public EventCallback<int> OnEntityLoaded { get; set; }
```

---

### 步驟 2：在 `NavigateToRecordAsync` 中觸發事件

**檔案：** `GenericEditModalComponent.razor`

**位置：** 約第 2035 行，`NavigateToRecordAsync` 方法內

```csharp
// 更新 ActionButtons（基於新的 Entity 資料）
UpdateAllActionButtons();

// 重新載入狀態訊息
await LoadStatusMessageData();

// 重新載入導航狀態（基於新的 Id）
await LoadNavigationStateAsync();

// 🆕 新增：觸發實體載入完成事件，通知父組件重新載入明細資料
if (OnEntityLoaded.HasDelegate)
{
    await OnEntityLoaded.InvokeAsync(targetId);
}

// 🔑 優化：所有資料更新完成後，只觸發一次 UI 重繪
StateHasChanged();
```

**說明：**
- 在所有主檔資料更新完成後觸發事件
- 傳遞 `targetId` 讓父組件知道要載入哪一筆明細
- 只有當事件有訂閱者時才觸發（避免不必要的執行）

---

### 步驟 3：在 `PurchaseOrderEditModalComponent` 綁定事件

**檔案：** `PurchaseOrderEditModalComponent.razor`

**位置：** 約第 20-53 行，`GenericEditModalComponent` 標籤

```razor
<GenericEditModalComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          @bind-Id="@PurchaseOrderId"
                          Service="@PurchaseOrderService"
                          EntityName="採購單"
                          EntityNamePlural="採購單"
                          <!-- ... 其他參數省略 ... -->
                          OnRejectWithReason="@HandlePurchaseOrderRejectWithReason"
                          FormHeaderContent="@WarningMessage"
                          CustomActionButtons="@CustomActionButtons"
                          OnEntityLoaded="@HandleEntityLoaded">  <!-- 🆕 新增此行 -->
</GenericEditModalComponent>
```

---

### 步驟 4：實作事件處理方法

**檔案：** `PurchaseOrderEditModalComponent.razor`

**位置：** 約第 850-900 行，採購明細管理方法區域（在 `HandleHasUndeletableDetailsChanged` 之前）

```csharp
// ===== 採購明細管理方法 =====

/// <summary>
/// 處理實體載入完成事件（由 GenericEditModalComponent 的導航觸發）
/// 當上下筆切換時，重新載入對應的明細資料
/// </summary>
private async Task HandleEntityLoaded(int loadedEntityId)
{
    try
    {
        // 重新載入明細資料
        await LoadPurchaseOrderDetails(loadedEntityId);
        
        // 觸發 Table 元件刷新
        if (purchaseOrderDetailManager != null)
        {
            await purchaseOrderDetailManager.RefreshDetailsAsync();
        }
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleEntityLoaded), GetType(), 
            additionalData: $"處理實體載入事件失敗 - EntityId: {loadedEntityId}");
        await NotificationService.ShowErrorAsync("載入明細資料時發生錯誤");
    }
}

/// <summary>
/// 處理有不可刪除明細的狀態變更
/// 當明細動態變化時（新增進貨、刪除進貨記錄等），這個方法會被調用
/// </summary>
private async Task HandleHasUndeletableDetailsChanged(bool hasUndeletable)
{
    // ... 原有程式碼 ...
}
```

**處理邏輯說明：**

1. **重新載入明細資料**
   - 呼叫 `LoadPurchaseOrderDetails(loadedEntityId)` 從資料庫載入新的明細
   - 此方法會更新 `purchaseOrderDetails` 變數
   - 同時會檢查是否有不可刪除的明細（有進貨記錄）

2. **刷新 Table 元件顯示**
   - 呼叫 `purchaseOrderDetailManager.RefreshDetailsAsync()`
   - 此方法會觸發 Table 元件重新渲染，顯示最新的明細內容

3. **通知 UI 更新**
   - `StateHasChanged()` 確保所有相關的計算欄位（總金額、稅額等）都正確更新

---

## 📊 資料流程圖

### 上下筆切換完整流程

```
使用者點擊「上一筆/下一筆」按鈕
  ↓
GenericEditModalComponent.HandlePrevious/HandleNext()
  ↓
GenericEditModalComponent.NavigateToRecordAsync(targetId)
  ↓
使用 Service.GetByIdAsync 載入新的 Entity
  ↓
觸發 IdChanged.InvokeAsync(targetId) → PurchaseOrderId 更新
  ↓
更新 ActionButtons、StatusMessage、NavigationState
  ↓
🆕 觸發 OnEntityLoaded.InvokeAsync(targetId)
  ↓
PurchaseOrderEditModalComponent.HandleEntityLoaded(targetId)
  ↓
LoadPurchaseOrderDetails(targetId) → 從資料庫載入明細
  ↓
purchaseOrderDetailManager.RefreshDetailsAsync() → 刷新 Table
  ↓
StateHasChanged() → 更新所有 UI
  ↓
✅ 主檔和明細都顯示正確的資料
```

---

## 🎯 適用場景

此解決方案適用於所有使用 `GenericEditModalComponent` 且包含**主檔-明細**結構的頁面：

### ✅ 已應用此方案的頁面

| 編號 | 頁面名稱 | 明細類型 | 實施日期 | 備註 |
|------|---------|---------|---------|------|
| 1 | `PurchaseOrderEditModalComponent` | 採購明細 | 2025-01-16 | 首次實施 |
| 2 | `PurchaseReceivingEditModalComponent` | 進貨明細 | 2025-01-16 | 需載入退貨數量、沖款記錄 |
| 3 | `PurchaseReturnEditModalComponent` | 進貨退出明細 | 2025-01-16 | 明細在 LoadData 中載入 |
| 4 | `SalesOrderEditModalComponent` | 銷貨訂單明細 | 2025-01-16 | - |
| 5 | `SalesDeliveryEditModalComponent` | 銷貨出貨明細 | 2025-01-16 | - |
| 6 | `SalesReturnEditModalComponent` | 銷售退貨明細 | 2025-01-16 | - |
| 7 | `QuotationEditModalComponent` | 報價明細 | 2025-01-16 | 需檢查轉單數量 |
| 8 | `MaterialIssueEditModalComponent` | 領料明細 | 2025-01-16 | 明細在 LoadData 中載入 |

**統計：** 共 8 個主檔-明細頁面已全部套用此功能 ✅

---

## 📝 套用步驟範本

如果您要在其他頁面套用此功能，請按照以下步驟：

### 情況 1：有獨立的 LoadDetails 方法

如果您的 EditModal 有獨立的載入明細方法（例如：`LoadPurchaseOrderDetails`），請使用以下模板：

#### 1. 在 EditModal 元件中綁定事件

```razor
<GenericEditModalComponent TEntity="YourEntity" 
                          TService="IYourService"
                          <!-- ... 其他參數 ... -->
                          OnEntityLoaded="@HandleEntityLoaded">
</GenericEditModalComponent>
```

#### 2. 實作事件處理方法

```csharp
/// <summary>
/// 處理實體載入完成事件（由 GenericEditModalComponent 的導航觸發）
/// 當上下筆切換時，重新載入對應的明細資料
/// </summary>
private async Task HandleEntityLoaded(int loadedEntityId)
{
    try
    {
        // 1. 重新載入明細資料（從資料庫）
        await LoadYourDetails(loadedEntityId);
        
        // 2. 如果有其他相關資料需要載入（例如：退貨數量、沖款記錄等）
        // await LoadDetailRelatedDataAsync();
        
        // 3. 觸發 Table 元件刷新（如果有）
        if (yourDetailManager != null)
        {
            await yourDetailManager.RefreshDetailsAsync();
        }
        
        // 4. 更新 UI
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleEntityLoaded), GetType(), 
            additionalData: $"處理實體載入事件失敗 - EntityId: {loadedEntityId}");
        await NotificationService.ShowErrorAsync("載入明細資料時發生錯誤");
    }
}
```

### 情況 2：明細在 LoadData 中直接載入

如果您的 EditModal 沒有獨立的載入明細方法，而是在 `LoadYourEntityData` 中直接載入明細（例如：`PurchaseReturnEditModalComponent`、`MaterialIssueEditModalComponent`），請使用以下模板：

#### 1. 在 EditModal 元件中綁定事件（同情況 1）

#### 2. 實作事件處理方法（直接從 Service 載入）

```csharp
/// <summary>
/// 處理實體載入完成事件（由 GenericEditModalComponent 的導航觸發）
/// 當上下筆切換時，重新載入對應的明細資料
/// </summary>
private async Task HandleEntityLoaded(int loadedEntityId)
{
    try
    {
        // 1. 從 Service 直接載入完整實體（含明細）
        var entity = await YourEntityService.GetWithDetailsAsync(loadedEntityId);
        if (entity?.YourEntityDetails != null)
        {
            yourEntityDetails = entity.YourEntityDetails.ToList();
        }
        else
        {
            yourEntityDetails = new List<YourEntityDetail>();
        }
        
        // 🔑 關鍵：立即觸發 UI 更新，確保 Table 元件收到新的參數
        // 這樣 RefreshDetailsAsync() 才能讀取到正確的明細資料
        StateHasChanged();
        
        // 2. 載入明細相關資料（例如：退貨數量、沖款記錄等）
        // await LoadDetailRelatedDataAsync();
        
        // 3. 觸發 Table 元件刷新（此時參數已經是新的資料）
        if (yourDetailManager != null)
        {
            await yourDetailManager.RefreshDetailsAsync();
        }
        
        // 4. 最後再次更新 UI（確保所有變更都反映在畫面上）
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleEntityLoaded), GetType(), 
            additionalData: $"處理實體載入事件失敗 - EntityId: {loadedEntityId}");
        await NotificationService.ShowErrorAsync("載入明細資料時發生錯誤");
    }
}
```

---

## ⚠️ 注意事項

### 1. Table 元件必須實作 RefreshDetailsAsync 方法

所有 Table 元件都需要實作公開的 `RefreshDetailsAsync()` 方法：

```csharp
/// <summary>
/// 公開的刷新方法，用於外部觸發明細刷新（例如：上下筆切換時）
/// </summary>
public async Task RefreshDetailsAsync()
{
    await LoadExistingDetailsAsync(); // 或其他載入邏輯
    tableComponent?.RefreshEmptyRow();
    StateHasChanged();
}
```

**已實作此方法的 Table 元件（2025-01-16 統一實作）：**
- ✅ PurchaseOrderTable
- ✅ PurchaseReceivingTable
- ✅ PurchaseReturnTable
- ✅ SalesOrderTable
- ✅ SalesDeliveryTable
- ✅ SalesReturnTable
- ✅ QuotationTable
- ✅ MaterialIssueTable

### 2. ⚠️ 關鍵：必須在載入明細後立即呼叫 StateHasChanged()

**這是最容易遺漏的重點！**

Blazor 的參數綁定機制需要 `StateHasChanged()` 才會將新參數傳遞給子元件。如果缺少這個步驟，會出現**明細延遲一次更新**的問題。

#### 錯誤範例（會出現時序問題）：

```csharp
private async Task HandleEntityLoaded(int loadedEntityId)
{
    // 1. 載入明細資料
    purchaseReturnDetails = await LoadDetails(loadedEntityId);
    
    // 2. 載入相關資料
    await LoadDetailRelatedDataAsync();
    
    // 3. 刷新 Table ❌ 此時 Table 還沒收到新的 ExistingReturnDetails
    await purchaseReturnDetailManager.RefreshDetailsAsync();
    
    // 4. 只在最後更新 UI
    StateHasChanged();
}
```

**問題現象：**
- 第一次按「上一筆」：主檔更新，明細仍是舊的
- 第二次按「上一筆」：主檔更新，明細變成上一次的（延遲一次）

#### 正確範例（時序正確）：

```csharp
private async Task HandleEntityLoaded(int loadedEntityId)
{
    // 1. 載入明細資料
    purchaseReturnDetails = await LoadDetails(loadedEntityId);
    
    // 🔑 關鍵：立即觸發 UI 更新，讓 Table 元件收到新的參數
    StateHasChanged();
    
    // 2. 載入相關資料
    await LoadDetailRelatedDataAsync();
    
    // 3. 刷新 Table ✅ 現在 Table 已經有正確的 ExistingReturnDetails
    await purchaseReturnDetailManager.RefreshDetailsAsync();
    
    // 4. 最後再次更新 UI
    StateHasChanged();
}
```

**執行順序說明：**

| 步驟 | 動作 | 目的 |
|------|------|------|
| 1 | 載入明細資料到變數 | 更新 `purchaseReturnDetails` 等變數 |
| 2 | **第一次 StateHasChanged()** | 🔑 **立即通知 Blazor 更新參數綁定**，讓 Table 收到新的 `ExistingDetails` 參數 |
| 3 | 載入相關資料 | 載入退貨數量、沖款記錄等附加資料 |
| 4 | 呼叫 RefreshDetailsAsync() | Table 內部重新載入並渲染（此時已有正確參數） |
| 5 | **第二次 StateHasChanged()** | 確保所有 UI 變更都反映在畫面上 |

#### 適用情境

這個雙重 `StateHasChanged()` 模式特別適用於：

1. **情況 1：有獨立 LoadDetails 方法**（如 PurchaseReceiving）
   - ✅ 需要在 `LoadDetails()` 之後立即呼叫
   - ✅ 需要在 `RefreshDetailsAsync()` 之後再次呼叫

2. **情況 2：明細在 LoadData 中載入**（如 PurchaseReturn）
   - ✅ 需要在載入明細後立即呼叫
   - ✅ 需要在 `RefreshDetailsAsync()` 之後再次呼叫

### 3. 避免重複載入

`HandleEntityLoaded` 只會在**上下筆切換時**觸發，不會在以下情況觸發：
- Modal 首次打開
- 儲存後
- 關閉 Modal

這些情況已由原有的 `LoadPurchaseOrderData()` 或 `DataLoader` 處理。

### 4. 異常處理

務必在 `HandleEntityLoaded` 中加入適當的異常處理，避免載入失敗時影響主檔顯示。

### 5. 性能考量

此方案會在每次上下筆切換時重新從資料庫載入明細，如果明細數量龐大，可能會有延遲。如有性能問題，可考慮：
- 加入載入指示器
- 實作明細快取機制
- 使用分頁載入明細

---

## 🔍 除錯提示

如果上下筆切換後明細仍未更新，請按照以下順序檢查：

### 常見問題 1：明細延遲一次更新（最常見！）

**症狀：**
- 第一次按「上一筆」：主檔更新，明細不變
- 第二次按「上一筆」：主檔更新，明細變成上一次的

**原因：** 缺少載入明細後的立即 `StateHasChanged()`

**解決方法：** 參考上方「注意事項 2」，在載入明細資料後立即呼叫 `StateHasChanged()`

### 常見問題 2：事件未正確綁定

**檢查清單：**
1. ✅ `OnEntityLoaded="@HandleEntityLoaded"` 是否正確綁定
2. ✅ `HandleEntityLoaded` 方法是否被正確呼叫（可加中斷點）
3. ✅ 確認方法簽名正確：`private async Task HandleEntityLoaded(int loadedEntityId)`

### 常見問題 3：資料載入失敗

**檢查清單：**
1. ✅ `LoadPurchaseOrderDetails` 或 `GetWithDetailsAsync` 是否成功載入資料
2. ✅ 檢查資料庫是否有該筆資料
3. ✅ 檢查 Service 方法是否正確實作 `Include()` 來載入明細

### 常見問題 4：Table 刷新失敗

**檢查清單：**
1. ✅ `purchaseOrderDetailManager.RefreshDetailsAsync()` 是否被呼叫
2. ✅ Table 元件的 `ExistingDetails` 參數是否正確綁定
3. ✅ Table 元件是否有實作 `RefreshDetailsAsync()` 方法

### 常見問題 5：其他錯誤

**檢查清單：**
1. ✅ 檢查瀏覽器開發者工具的 Console 是否有錯誤訊息
2. ✅ 檢查 Visual Studio 的輸出視窗是否有異常訊息
3. ✅ 確認 `try-catch` 區塊有正確處理異常

### 除錯步驟建議

如果問題仍未解決，建議按照以下步驟逐一檢查：

```csharp
private async Task HandleEntityLoaded(int loadedEntityId)
{
    try
    {
        Console.WriteLine($"[DEBUG] 開始載入明細 - EntityId: {loadedEntityId}");
        
        // 1. 載入明細
        purchaseReturnDetails = await LoadDetails(loadedEntityId);
        Console.WriteLine($"[DEBUG] 明細載入完成 - 數量: {purchaseReturnDetails.Count}");
        
        // 2. 第一次 StateHasChanged
        StateHasChanged();
        Console.WriteLine($"[DEBUG] 第一次 StateHasChanged 完成");
        
        // 3. 載入相關資料
        await LoadDetailRelatedDataAsync();
        Console.WriteLine($"[DEBUG] 相關資料載入完成");
        
        // 4. 刷新 Table
        if (purchaseReturnDetailManager != null)
        {
            await purchaseReturnDetailManager.RefreshDetailsAsync();
            Console.WriteLine($"[DEBUG] Table 刷新完成");
        }
        
        // 5. 第二次 StateHasChanged
        StateHasChanged();
        Console.WriteLine($"[DEBUG] 第二次 StateHasChanged 完成");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] 載入失敗: {ex.Message}");
        // ... 錯誤處理
    }
}
```

---

## 📚 相關文件

- `README_互動Table說明.md` - InteractiveTableComponent 使用說明
- `README_A單轉B單簡化修改說明.md` - 轉單流程說明
- `README_更新明細元件在Action編輯之後說明.md` - 明細刷新機制說明

---

## 🎓 設計模式

此解決方案採用的設計模式：

1. **觀察者模式（Observer Pattern）**
   - GenericEditModalComponent 是發布者
   - EditModal 元件是訂閱者
   - 通過 EventCallback 實現解耦

2. **單一職責原則（Single Responsibility Principle）**
   - GenericEditModalComponent 只負責通知
   - EditModal 負責具體的業務邏輯處理
   - Table 元件只負責顯示

3. **開放封閉原則（Open-Closed Principle）**
- 不修改既有的 Table 元件
- 通過事件擴展功能
- 向下相容，不影響現有頁面
- **統一性**：所有 Table 元件都實作相同的 `RefreshDetailsAsync` 方法

---## 📅 修改記錄

| 日期 | 版本 | 說明 | 修改者 |
|------|------|------|--------|
| 2025-01-16 | 1.0 | 初始版本：實作上下筆切換時 Table 自動載入功能 | System |
| 2025-01-16 | 2.0 | 批量套用：完成所有 8 個主檔-明細頁面的實施 | System |
| 2025-01-16 | 2.1 | **重要修正**：新增「立即 StateHasChanged()」的關鍵說明，解決明細延遲更新問題 | System |

---

## ✅ 總結

通過新增 `OnEntityLoaded` 事件參數，我們成功解決了上下筆切換時明細不跟著更新的問題。

此方案具有：
- ✅ **通用性**：適用於所有主檔-明細結構
- ✅ **可維護性**：程式碼清晰，職責分明
- ✅ **擴展性**：容易套用到其他頁面
- ✅ **向下相容**：不影響現有功能
- ✅ **完整覆蓋**：已套用到系統中所有 8 個主檔-明細頁面

**實施完成：** 所有需要上下筆導航的主檔-明細頁面都已套用此模式，確保使用者體驗一致。

---

## 🎯 核心要點總結

### 最關鍵的實作重點

1. **雙重 StateHasChanged() 模式**
   ```csharp
   載入明細 → StateHasChanged() → 載入相關資料 → RefreshDetailsAsync() → StateHasChanged()
   ```
   - 第一次：讓 Table 元件收到新參數
   - 第二次：確保所有 UI 變更生效

2. **時序很重要**
   - ❌ 錯誤：載入完所有資料後才呼叫一次 `StateHasChanged()`
   - ✅ 正確：載入明細後**立即**呼叫 `StateHasChanged()`，然後再載入相關資料

3. **完整的載入流程**
   - 載入明細資料（從資料庫）
   - 載入相關資料（退貨數量、沖款記錄等）
   - 刷新 Table 元件
   - 更新 UI 顯示

### 快速檢查清單

遇到明細不更新或延遲更新時，請檢查：
- [ ] 是否綁定 `OnEntityLoaded="@HandleEntityLoaded"`
- [ ] 是否在載入明細後**立即**呼叫 `StateHasChanged()`
- [ ] 是否使用 `GetWithDetailsAsync()` 而非 `GetByIdAsync()`
- [ ] 是否載入相關資料（如退貨數量、沖款記錄）
- [ ] 是否呼叫 `RefreshDetailsAsync()`
- [ ] 是否在最後再次呼叫 `StateHasChanged()`

只要遵循這些要點，就能確保上下筆切換時明細正確同步更新。
