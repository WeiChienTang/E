# 主檔鎖住設計模式 - 統一渲染方案

## 📋 文件資訊

- **建立日期**: 2025/10/15
- **設計模式**: 延遲渲染 + 狀態同步
- **適用範圍**: 所有具有主從式資料結構的編輯頁面
- **參考實作**: `PurchaseReceivingEditModalComponent`（進貨單編輯）

---

## 🎯 設計目標

解決主從式資料編輯時，**明細相關資料（如退貨數量、沖款記錄）影響主檔欄位狀態**的時序同步問題。

### 核心問題

在傳統的設計中，會遇到以下時序問題：

```
❌ 傳統流程（有問題）：
1. Modal 開啟 → LoadData() 載入主檔
2. LoadStatusMessage() 讀取狀態 → hasUndeletableDetails = false（預設值）
3. 狀態訊息快取 = null（沒有要顯示的訊息）
4. 明細管理器開始渲染
5. LoadReturnedQuantitiesAsync() 非同步查詢退貨數量
6. 發現有退貨記錄 → hasUndeletableDetails = true ✅
7. 但狀態訊息已經快取了，不會重新顯示 ❌
```

**結果**：
- ❌ 第一次開啟 Modal：狀態訊息不顯示（因為載入時 hasUndeletableDetails 還是 false）
- ✅ 第二次開啟 Modal：狀態訊息顯示（因為 hasUndeletableDetails 保留了上次的 true 值）

---

## ✅ 解決方案：統一渲染設計模式

### 設計原則

> **所有影響主檔狀態的明細資料，必須在明細管理器渲染前完全載入完成**

### 核心機制

1. **延遲渲染**：使用 `isDetailDataReady` 旗標控制明細管理器的渲染時機
2. **同步載入**：在主檔 `DataLoader` 中同步載入所有明細相關資料
3. **狀態前置**：確保 `hasUndeletableDetails` 等狀態在 `GetStatusMessage` 調用前就正確設定

### 新的流程

```
✅ 統一渲染流程（正確）：
1. Modal 開啟 → LoadData() 載入主檔
2. 載入明細資料 → LoadPurchaseReceivingDetails()
3. 🔑 同步載入明細相關資料 → LoadDetailRelatedDataAsync()
   ├─ 查詢退貨數量
   ├─ 檢查沖款記錄
   ├─ 更新 hasUndeletableDetails = true ✅
   └─ 更新欄位唯讀狀態
4. 🔑 標記資料準備完成 → isDetailDataReady = true
5. LoadStatusMessage() 讀取狀態 → hasUndeletableDetails = true ✅
6. 狀態訊息快取 = "部分明細已有退貨..." ✅
7. 明細管理器開始渲染（因為 isDetailDataReady = true）
8. 顯示鎖定警告訊息 ✅
```

**結果**：
- ✅ 第一次開啟 Modal：所有狀態正確顯示
- ✅ 第二次開啟 Modal：所有狀態正確顯示
- ✅ 狀態訊息和欄位鎖定完全同步

---

## 📖 實作步驟

### 步驟 1：新增狀態旗標

```csharp
// ===== 鎖定狀態 =====
private bool hasUndeletableDetails = false;

// ===== 明細載入狀態 =====
private bool isDetailDataReady = false;  // 🔑 標記明細相關資料是否已完整載入
```

### 步驟 2：注入必要服務

確保可以查詢明細相關資料：

```razor
@inject IPurchaseReturnDetailService PurchaseReturnDetailService
@inject ISetoffDocumentService SetoffDocumentService
```

### 步驟 3：在 DataLoader 中同步載入明細相關資料

```csharp
private async Task<PurchaseReceiving?> LoadPurchaseReceivingData()
{
    try
    {
        // 🔑 重置明細資料準備狀態
        isDetailDataReady = false;
        hasUndeletableDetails = false;
        
        if (!PurchaseReceivingId.HasValue)
        {
            // 新增模式：直接標記為準備就緒
            isDetailDataReady = true;
            return new PurchaseReceiving { ... };
        }

        // 編輯模式
        var purchaseReceiving = await PurchaseReceivingService.GetByIdAsync(PurchaseReceivingId.Value);
        
        if (purchaseReceiving != null)
        {
            // 1. 載入進貨明細
            await LoadPurchaseReceivingDetails(PurchaseReceivingId.Value);
            
            // 2. 🔑 關鍵：載入退貨數量和沖款資料，並更新 hasUndeletableDetails
            await LoadDetailRelatedDataAsync();
            
            // 3. 🔑 標記明細資料已準備就緒（包括退貨數量等資訊）
            isDetailDataReady = true;
            
            // 4. 其他資料載入...
            await UpdatePurchaseOrderOptions(purchaseReceiving.SupplierId);
            
            StateHasChanged();
        }
        
        return purchaseReceiving;
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入資料時發生錯誤：{ex.Message}");
        isDetailDataReady = true; // 即使錯誤也要允許渲染，避免卡住
        return null;
    }
}
```

### 步驟 4：實作明細相關資料載入方法

```csharp
/// <summary>
/// 載入明細相關資料（退貨數量、沖款記錄等）
/// 這個方法在主檔載入時就同步執行，確保 hasUndeletableDetails 狀態在渲染前就正確
/// </summary>
private async Task LoadDetailRelatedDataAsync()
{
    try
    {
        if (!purchaseReceivingDetails.Any())
        {
            hasUndeletableDetails = false;
            return;
        }

        // 檢查是否有不可刪除的明細
        bool hasUndeletable = false;
        
        foreach (var detail in purchaseReceivingDetails.Where(d => d.Id > 0))
        {
            // 檢查 1：退貨記錄
            var returnedQty = await PurchaseReturnDetailService
                .GetReturnedQuantityByReceivingDetailAsync(detail.Id);
            if (returnedQty > 0)
            {
                hasUndeletable = true;
                break;
            }
            
            // 檢查 2：沖款記錄（直接讀取 TotalPaidAmount 欄位）
            if (detail.TotalPaidAmount > 0)
            {
                hasUndeletable = true;
                break;
            }
        }
        
        hasUndeletableDetails = hasUndeletable;
        
        // 如果有不可刪除的明細，立即更新欄位狀態
        if (hasUndeletableDetails)
        {
            UpdateFieldsReadOnlyState();
        }
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(
            ex, nameof(LoadDetailRelatedDataAsync), GetType());
        hasUndeletableDetails = false; // 錯誤時保守處理，不鎖定
    }
}
```

### 步驟 5：實作狀態訊息方法（Modal 頂部徽章）

```csharp
/// <summary>
/// 取得狀態訊息（顯示在 Modal 頂部的徽章）
/// </summary>
private async Task<(string Message, GenericEditModalComponent<TMainEntity, TService>.BadgeVariant Variant, string IconClass)?> 
    GetStatusMessage()
{
    try
    {
        // 如果資料還沒準備好，不顯示訊息
        if (!isDetailDataReady || editModalComponent?.Entity == null)
            return null;
        
        // 只有在有不可刪除的明細時才顯示鎖定訊息
        if (hasUndeletableDetails)
        {
            return (
                "明細有其他動作，主檔欄位已鎖定",  // 簡短訊息
                GenericEditModalComponent<TMainEntity, TService>.BadgeVariant.Warning,  // 黃色警告
                "fas fa-lock"  // 鎖定圖示
            );
        }
        
        // 正常狀態不顯示訊息
        return null;
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetStatusMessage), GetType());
        return null;
    }
}
```

### 步驟 6：實作警告訊息方法（表單頂部提示）

```csharp
/// <summary>
/// 取得警告訊息（顯示在表單最上方）
/// </summary>
private RenderFragment? GetWarningMessage() => __builder =>
{
    @if (isDetailDataReady && hasUndeletableDetails)
    {
        <div class="alert alert-warning mb-2 py-2" role="alert">
            <i class="fas fa-lock me-2"></i>因部分明細有其他動作，為保護資料完整性主檔欄位已設唯讀。
        </div>
    }
};
```

**樣式說明**：
- `alert alert-warning`：Bootstrap 警告樣式（黃色背景）
- `mb-2`：底部間距 0.5rem（與下方欄位保持適當間距）
- `py-2`：上下內距 0.5rem（縮小警告框高度，更緊湊）

**文字原則**：
- ✅ **簡潔**：一句話說明原因和結果
- ✅ **一行顯示**：避免換行，方便手機閱讀
- ✅ **語意清晰**：「因...，為...已...」的結構

### 步驟 7：在 GenericEditModalComponent 中綁定

```razor
<GenericEditModalComponent TEntity="TMainEntity" 
                          TService="TService"
                          ...
                          GetStatusMessage="@GetStatusMessage"
                          FormHeaderContent="@GetWarningMessage()"
                          ... />
```

**參數說明**：
- `GetStatusMessage`：Modal 頂部徽章（簡短狀態）
- `FormHeaderContent`：表單最上方警告（詳細說明）

> **重要**：`FormHeaderContent` 會在**所有表單欄位之前**渲染，確保使用者第一眼就看到警告。

### 步驟 7：延遲渲染明細管理器

```razor
private RenderFragment CreateDetailManagerContent() => __builder =>
{
    @if (editModalComponent?.Entity != null)
    {
        // 🔑 關鍵：等待明細資料完全準備好（包括退貨數量等）後才渲染
        @if (!isDetailDataReady)
        {
            <div class="d-flex justify-content-center align-items-center py-4">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                <span class="text-muted">載入明細資料中...</span>
            </div>
        }
        else if (editModalComponent.Entity.SupplierId > 0)  // 根據實際主鍵條件調整
        {
            // ⚠️ 警告訊息已在 FormHeaderContent 顯示，這裡不需要重複
            
            // 明細管理器渲染
            <DetailManagerComponent 
                @ref="detailManager"
                ...
                OnHasUndeletableDetailsChanged="@HandleHasUndeletableDetailsChanged"
                ... />
        }
        else
        {
            <div class="alert alert-info text-center" role="alert">
                <i class="fas fa-info-circle me-2"></i>
                請先選擇必要欄位後再管理明細
            </div>
        }
    }
};
```

### 步驟 8：處理動態變更（保留原有機制）

```csharp
/// <summary>
/// 處理有不可刪除明細的狀態變更
/// 當明細動態變化時（新增退貨、取消沖款等），這個方法會被調用
/// </summary>
private async Task HandleHasUndeletableDetailsChanged(bool hasUndeletable)
{
    try
    {
        hasUndeletableDetails = hasUndeletable;
        
        // 更新欄位的唯讀狀態
        UpdateFieldsReadOnlyState();
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理明細鎖定狀態時發生錯誤：{ex.Message}");
    }
}
```

---

## 🎨 視覺效果

### 1. Modal 頂部狀態徽章（GetStatusMessage）

顯示位置：Modal 標題右側，按鈕區域左側

```
┌────────────────────────────────────────────────────┐
│ � 編輯單據  [�🔒 明細有其他動作，主檔欄位已鎖定]   │ ← 頂部徽章
├────────────────────────────────────────────────────┤
```

- **顏色**：🟡 Warning（黃色徽章）
- **圖示**：🔒 `fas fa-lock`
- **文字**：簡短說明（建議 15 字內）
- **位置**：Modal 頂部，與按鈕同列

### 2. 表單頂部警告訊息（FormHeaderContent）

顯示位置：Modal Body 最上方，所有欄位之前

```
┌────────────────────────────────────────────────────┐
│ 🔒 因部分明細有其他動作，為保護資料完整性         │ ← 表單頂部警告
│    主檔欄位已設唯讀。                              │
└────────────────────────────────────────────────────┘
         ↓ (0.5rem 間距)
┌────────────────────────────────────────────────────┐
│ 欄位區域開始                                        │
```

- **顏色**：🟡 Alert Warning（淺黃色背景）
- **圖示**：🔒 `fas fa-lock`（標準大小）
- **樣式**：`alert alert-warning mb-2 py-2`
- **文字**：完整說明（建議單行，30 字內）
- **內距**：上下 `0.5rem`（縮小 50%）
- **外距**：下方 `0.5rem`（與欄位適當間距）

### 3. 載入中狀態

```
┌────────────────────────────────────────────────────┐
│ ⏳ 載入明細資料中...                                │
└────────────────────────────────────────────────────┘
```

### 4. 完整視覺流程

```
┌─────────────────────────────────────────────────────┐
│ 📝 編輯進貨  [🔒 明細有其他動作，主檔欄位已鎖定]    │ ← ⭐ 頂部徽章
├─────────────────────────────────────────────────────┤
│ [取消] [儲存] [列印]                                 │
├─────────────────────────────────────────────────────┤
│ Modal Body 開始                                     │
│                                                     │
│ ┌─────────────────────────────────────────────┐   │
│ │🔒 因部分明細有其他動作，為保護資料完整性    │   │ ← ⭐ 警告（最上方）
│ │   主檔欄位已設唯讀。                        │   │
│ └─────────────────────────────────────────────┘   │
│           ↓ (0.5rem 間距)                          │
│ ┌─────────────────────────────────────────────┐   │
│ │ 基本資訊                                     │   │
│ │ ────────────────────────────────────────    │   │
│ │ 單號: [RCV202510111502401]                  │   │
│ │ 廠商: [邪] [唯讀] ← 鎖定，無新增/編輯按鈕   │   │
│ │ 採購單: [請選擇...] [唯讀] ← 鎖定           │   │
│ │ 進貨日: [2025/10/11] [唯讀] ← 鎖定          │   │
│ └─────────────────────────────────────────────┘   │
│                                                     │
│ ┌─────────────────────────────────────────────┐   │
│ │ 明細管理器                                   │   │
│ │ [商品列表...]                                │   │
│ └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

## 🔍 關鍵設計細節

### 1. 為什麼需要 `isDetailDataReady` 旗標？

**問題**：如果沒有這個旗標，明細管理器會立即渲染，然後它的 `LoadReturnedQuantitiesAsync` 會非同步執行，導致狀態更新時序不同步。

**解決**：延遲明細管理器的渲染，直到所有影響主檔狀態的資料都載入完成。

### 2. 為什麼要在 `DataLoader` 中載入明細相關資料？

**問題**：`GenericEditModalComponent` 的 `LoadStatusMessageData()` 在 `LoadData()` 之後立即執行，此時必須確保所有狀態已正確。

**解決**：在 `DataLoader`（即 `LoadPurchaseReceivingData`）中同步載入所有影響狀態的資料。

### 3. 為什麼新增模式要立即設為 `isDetailDataReady = true`？

**問題**：新增模式沒有明細，不需要載入退貨數量等資料，如果不標記為準備就緒，會一直顯示載入中。

**解決**：新增模式下，沒有明細需要檢查，直接標記為準備就緒。

### 4. 為什麼保留 `HandleHasUndeletableDetailsChanged`？

**問題**：使用者在編輯過程中可能會動態變更明細（新增退貨、取消沖款等），狀態可能動態改變。

**解決**：保留這個事件處理器，讓明細管理器可以即時通知父組件狀態變更。

### 5. 錯誤處理策略

```csharp
catch (Exception ex)
{
    await ErrorHandlingHelper.HandlePageErrorAsync(ex, ...);
    isDetailDataReady = true; // 🔑 即使錯誤也要允許渲染，避免卡住
    hasUndeletableDetails = false; // 🔑 錯誤時保守處理，不鎖定
    return null;
}
```

**原因**：如果載入過程中發生錯誤，不應該讓 UI 永久卡在載入狀態，而是應該允許使用者看到錯誤訊息並繼續操作。

---

## 📊 狀態流轉圖

```
                          ┌─────────────┐
                          │ Modal 開啟  │
                          └──────┬──────┘
                                 │
                    ┌────────────▼────────────┐
                    │ isDetailDataReady=false │
                    │ hasUndeletableDetails=  │
                    │        false            │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  LoadPurchaseReceiving  │
                    │        Data()           │
                    └────────────┬────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
    ┌────▼─────┐         ┌──────▼──────┐        ┌──────▼──────┐
    │ 新增模式 │         │  編輯模式   │        │  載入錯誤   │
    └────┬─────┘         └──────┬──────┘        └──────┬──────┘
         │                      │                       │
    ┌────▼─────┐         ┌──────▼──────────────┐  ┌────▼─────┐
    │isReady=  │         │LoadPurchaseReceiving│  │isReady=  │
    │  true    │         │     Details()       │  │  true    │
    └────┬─────┘         └──────┬──────────────┘  │hasUnde-  │
         │                      │                  │letable=  │
         │              ┌───────▼──────────┐       │  false   │
         │              │LoadDetailRelated │       └────┬─────┘
         │              │    DataAsync()   │            │
         │              │                  │            │
         │              │ • 查詢退貨數量   │            │
         │              │ • 檢查沖款記錄   │            │
         │              │ • 更新狀態       │            │
         │              └───────┬──────────┘            │
         │                      │                       │
         │              ┌───────▼──────────┐            │
         │              │hasUndeletable-   │            │
         │              │Details = ?       │            │
         │              │                  │            │
         │              │isDetailDataReady │            │
         │              │    = true        │            │
         │              └───────┬──────────┘            │
         │                      │                       │
         └──────────────────────┼───────────────────────┘
                                │
                    ┌───────────▼───────────┐
                    │ LoadStatusMessage()   │
                    │                       │
                    │ • 讀取hasUndeletable- │
                    │   Details狀態         │
                    │ • 快取狀態訊息        │
                    └───────────┬───────────┘
                                │
                    ┌───────────▼───────────┐
                    │   明細管理器渲染      │
                    │                       │
                    │ • 顯示警告訊息（如需）│
                    │ • 欄位已鎖定（如需）  │
                    │ • 狀態訊息已顯示      │
                    └───────────────────────┘
```

---

## 📝 快速套用檢查清單

### 必要步驟（按順序執行）

- [ ] **1. 新增狀態變數**
  ```csharp
  private bool hasUndeletableDetails = false;
  private bool isDetailDataReady = false;
  ```

- [ ] **2. 注入必要服務**（根據業務需求調整）
  ```razor
  @inject IReturnDetailService ReturnDetailService
  @inject IPaymentService PaymentService
  ```

- [ ] **3. 修改 DataLoader**
  - 新增模式：設定 `isDetailDataReady = true`
  - 編輯模式：
    - 載入明細後調用 `LoadDetailRelatedDataAsync()`
    - 設定 `isDetailDataReady = true`
    - 錯誤處理：確保 `isDetailDataReady = true`

- [ ] **4. 實作 `LoadDetailRelatedDataAsync()`**
  - 檢查退貨/沖款等限制條件
  - 更新 `hasUndeletableDetails`
  - 調用 `UpdateFieldsReadOnlyState()`

- [ ] **5. 實作狀態訊息方法**
  ```csharp
  private async Task<(...)> GetStatusMessage()
  {
      if (!isDetailDataReady || ...) return null;
      if (hasUndeletableDetails) return (...);
      return null;
  }
  ```

- [ ] **6. 實作警告訊息方法**
  ```csharp
  private RenderFragment? GetWarningMessage() => __builder =>
  {
      @if (isDetailDataReady && hasUndeletableDetails)
      {
          <div class="alert alert-warning mb-2 py-2" role="alert">
              <i class="fas fa-lock me-2"></i>警告文字（單行，30字內）
          </div>
      }
  };
  ```

- [ ] **7. 綁定到 GenericEditModalComponent**
  ```razor
  GetStatusMessage="@GetStatusMessage"
  FormHeaderContent="@GetWarningMessage()"
  ```

- [ ] **8. 修改明細管理器渲染**
  - 加入 `@if (!isDetailDataReady)` 載入中邏輯
  - 移除明細區域的重複警告訊息

- [ ] **9. 保留動態更新機制**
  ```csharp
  private async Task HandleHasUndeletableDetailsChanged(bool hasUndeletable)
  {
      hasUndeletableDetails = hasUndeletable;
      UpdateFieldsReadOnlyState();
      StateHasChanged();
  }
  ```

### 樣式規範

**頂部徽章**：
- 文字：15 字內
- 格式：`"明細有其他動作，主檔欄位已鎖定"`

**表單警告**：
- 樣式：`class="alert alert-warning mb-2 py-2"`
- 文字：30 字內，單行顯示
- 格式：`"因[原因]，為[目的][結果]。"`
- 範例：`"因部分明細有其他動作，為保護資料完整性主檔欄位已設唯讀。"`

### 測試驗證

- [ ] 新增模式：立即顯示，無延遲
- [ ] 編輯模式（無限制）：無警告，欄位可編輯
- [ ] 編輯模式（有限制）：
  - [ ] 頂部徽章顯示
  - [ ] 表單頂部警告顯示（在欄位之前）
  - [ ] 相關欄位鎖定
  - [ ] 操作按鈕移除
- [ ] 第一次開啟正確顯示
- [ ] 載入錯誤不會卡住

---

## 🎓 設計模式優點

### ✅ 優點

1. **時序可控**：所有狀態在渲染前就確定，避免非同步競爭
2. **狀態一致**：頂部狀態訊息、明細警告、欄位鎖定完全同步
3. **第一次正確**：第一次開啟就能正確顯示，不需要關閉重開
4. **性能優化**：明細相關資料只查詢一次（在 DataLoader 中）
5. **錯誤穩健**：載入失敗也不會卡住 UI
6. **易於維護**：邏輯清晰，狀態流轉明確

### ⚠️ 注意事項

1. **載入時間**：如果明細很多，可能會增加載入時間（但換來正確性）
2. **記憶體使用**：會在主檔載入時就載入所有明細相關資料
3. **服務依賴**：需要注入額外的服務（如 `IPurchaseReturnDetailService`）

---

## 📝 應用場景

這個設計模式適用於以下場景：

### ✅ 適用

- **主從式資料編輯**：主檔欄位是否可編輯取決於明細狀態
- **狀態聯動**：明細的某些屬性影響主檔的顯示或行為
- **資料保護**：需要根據明細狀態鎖定主檔欄位
- **審核流程**：明細的審核狀態影響主檔的可編輯性

### ❌ 不適用

- **簡單表單**：沒有主從關係的獨立表單
- **唯讀頁面**：僅用於查看資料的頁面
- **即時互動**：需要立即渲染並動態載入的場景（如無限滾動）

---

## 🔗 相關文件

- [README_狀態訊息顯示.md](./README_狀態訊息顯示.md) - 狀態訊息顯示系統
- [README_進貨明細鎖定主檔欄位.md](./README_進貨明細鎖定主檔欄位.md) - 欄位鎖定功能
- [README_InteractiveTableComponent.md](./README_InteractiveTableComponent.md) - 互動式表格元件

---

## 🔧 完整實作範例

### 範例 1：進貨單（PurchaseReceivingEditModalComponent）

**業務邏輯**：
- 明細有退貨記錄 → 不能刪除 → 鎖定主檔
- 明細有沖款記錄 → 不能刪除 → 鎖定主檔

**檢查方法**：
```csharp
private async Task LoadDetailRelatedDataAsync()
{
    if (!purchaseReceivingDetails.Any())
    {
        hasUndeletableDetails = false;
        return;
    }

    bool hasUndeletable = false;
    
    foreach (var detail in purchaseReceivingDetails.Where(d => d.Id > 0))
    {
        // 檢查退貨記錄
        var returnedQty = await PurchaseReturnDetailService
            .GetReturnedQuantityByReceivingDetailAsync(detail.Id);
        if (returnedQty > 0)
        {
            hasUndeletable = true;
            break;
        }
        
        // 檢查沖款記錄
        if (detail.TotalPaidAmount > 0)
        {
            hasUndeletable = true;
            break;
        }
    }
    
    hasUndeletableDetails = hasUndeletable;
    
    if (hasUndeletableDetails)
    {
        UpdateFieldsReadOnlyState();
    }
}
```

**鎖定欄位**：
- 廠商（SupplierId）+ 移除新增/編輯按鈕
- 採購單（PurchaseOrderId）
- 商品篩選（FilterProductId）
- 進貨日（ReceiptDate）
- 備註（Remarks）

**完整檔案**：
- `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`
- `Components/Shared/SubCollections/PurchaseReceivingDetailManagerComponent.razor`

---

### 範例 2：銷售單（可套用的模板）

**業務邏輯**：
- 明細有出貨記錄 → 不能刪除 → 鎖定主檔
- 明細有收款記錄 → 不能刪除 → 鎖定主檔

**檢查方法**：
```csharp
private async Task LoadDetailRelatedDataAsync()
{
    if (!salesOrderDetails.Any())
    {
        hasUndeletableDetails = false;
        return;
    }

    bool hasUndeletable = false;
    
    foreach (var detail in salesOrderDetails.Where(d => d.Id > 0))
    {
        // 檢查出貨記錄
        var shippedQty = await ShipmentDetailService
            .GetShippedQuantityBySalesDetailAsync(detail.Id);
        if (shippedQty > 0)
        {
            hasUndeletable = true;
            break;
        }
        
        // 檢查收款記錄
        if (detail.TotalReceivedAmount > 0)
        {
            hasUndeletable = true;
            break;
        }
    }
    
    hasUndeletableDetails = hasUndeletable;
    
    if (hasUndeletableDetails)
    {
        UpdateFieldsReadOnlyState();
    }
}
```

---

### 通用模板（複製使用）

```csharp
// ===== 1. 狀態變數 =====
private bool hasUndeletableDetails = false;
private bool isDetailDataReady = false;

// ===== 2. DataLoader 修改 =====
private async Task<TMainEntity?> LoadMainEntityData()
{
    try
    {
        isDetailDataReady = false;
        hasUndeletableDetails = false;
        
        if (!EntityId.HasValue)
        {
            isDetailDataReady = true;
            return new TMainEntity { ... };
        }

        var entity = await Service.GetByIdAsync(EntityId.Value);
        
        if (entity != null)
        {
            await LoadDetails(EntityId.Value);
            await LoadDetailRelatedDataAsync();  // 🔑 關鍵
            isDetailDataReady = true;  // 🔑 關鍵
            StateHasChanged();
        }
        
        return entity;
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入資料時發生錯誤：{ex.Message}");
        isDetailDataReady = true;  // 錯誤也要允許渲染
        return null;
    }
}

// ===== 3. 明細相關資料載入 =====
private async Task LoadDetailRelatedDataAsync()
{
    try
    {
        if (!details.Any())
        {
            hasUndeletableDetails = false;
            return;
        }

        bool hasUndeletable = false;
        
        foreach (var detail in details.Where(d => d.Id > 0))
        {
            // 🔑 根據業務需求檢查限制條件
            // if (await CheckCondition1(detail.Id)) { hasUndeletable = true; break; }
            // if (detail.SomeAmount > 0) { hasUndeletable = true; break; }
        }
        
        hasUndeletableDetails = hasUndeletable;
        
        if (hasUndeletableDetails)
        {
            UpdateFieldsReadOnlyState();
        }
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadDetailRelatedDataAsync), GetType());
        hasUndeletableDetails = false;
    }
}

// ===== 4. 狀態訊息（頂部徽章）=====
private async Task<(string Message, GenericEditModalComponent<TEntity, TService>.BadgeVariant Variant, string IconClass)?> 
    GetStatusMessage()
{
    try
    {
        if (!isDetailDataReady || editModalComponent?.Entity == null)
            return null;
        
        if (hasUndeletableDetails)
        {
            return (
                "明細有其他動作，主檔欄位已鎖定",
                GenericEditModalComponent<TEntity, TService>.BadgeVariant.Warning,
                "fas fa-lock"
            );
        }
        
        return null;
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetStatusMessage), GetType());
        return null;
    }
}

// ===== 5. 警告訊息（表單頂部）=====
private RenderFragment? GetWarningMessage() => __builder =>
{
    @if (isDetailDataReady && hasUndeletableDetails)
    {
        <div class="alert alert-warning mb-2 py-2" role="alert">
            <i class="fas fa-lock me-2"></i>因部分明細有其他動作，為保護資料完整性主檔欄位已設唯讀。
        </div>
    }
};

// ===== 6. 動態更新處理 =====
private async Task HandleHasUndeletableDetailsChanged(bool hasUndeletable)
{
    try
    {
        hasUndeletableDetails = hasUndeletable;
        UpdateFieldsReadOnlyState();
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理明細鎖定狀態時發生錯誤：{ex.Message}");
    }
}

// ===== 7. 明細管理器渲染 =====
private RenderFragment CreateDetailManagerContent() => __builder =>
{
    @if (editModalComponent?.Entity != null)
    {
        @if (!isDetailDataReady)
        {
            <div class="d-flex justify-content-center align-items-center py-4">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                <span class="text-muted">載入明細資料中...</span>
            </div>
        }
        else if (/* 必要條件判斷 */)
        {
            <DetailManagerComponent 
                ...
                OnHasUndeletableDetailsChanged="@HandleHasUndeletableDetailsChanged"
                ... />
        }
    }
};
```

---

### Razor 綁定（複製使用）

```razor
<GenericEditModalComponent TEntity="TMainEntity" 
                          TService="TService"
                          @ref="editModalComponent"
                          ...
                          DataLoader="@LoadMainEntityData"
                          GetStatusMessage="@GetStatusMessage"
                          FormHeaderContent="@GetWarningMessage()"
                          CustomModules="@GetCustomModules()"
                          ... />
```

---

## 📅 版本歷史

| 版本 | 日期 | 修改內容 |
|------|------|---------|
| v1.0 | 2025/10/15 | 初始版本，建立統一渲染設計模式 |

---

## 👨‍💻 維護資訊

- **設計者**: 開發團隊
- **最後更新**: 2025/10/15
- **測試狀態**: ✅ 已驗證可解決第一次開啟不顯示的問題
- **應用狀態**: 🚀 已應用於進貨單編輯功能

---

**📌 核心思想**: 
> 與其在非同步執行完成後追趕狀態更新，不如在渲染前就確保所有狀態準備就緒。
> 延遲是可接受的代價，換來的是狀態的一致性和可預測性。

---

**🎯 記住**：
1. 使用 `isDetailDataReady` 旗標控制渲染時機
2. 在 `DataLoader` 中同步載入影響狀態的資料
3. 確保 `GetStatusMessage` 調用時狀態已正確
4. 即使錯誤也要允許渲染，避免卡住

---

如有任何問題或需要進一步協助，請參考相關文件或聯繫開發團隊。
