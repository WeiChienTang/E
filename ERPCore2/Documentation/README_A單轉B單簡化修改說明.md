# A單轉B單簡化修改說明

## 修改日期
- 2025年11月14日：初版（轉進貨單功能）
- 2025年11月15日：新增列印功能套用相同模式

## 修改範圍
`PurchaseOrderEditModalComponent.razor` - 採購單轉進貨單功能 + 列印功能

---

## 問題背景

### 原有設計的問題
1. **過度複雜的驗證邏輯**：轉進貨單時執行 6 步驗證（60+ 行程式碼）
2. **責任不清**：前端重複驗證已在儲存時檢查過的業務邏輯
3. **維護困難**：大量冗餘檢查導致程式碼難以維護
4. **容易出錯**：複雜的參數傳遞和 nullable 處理容易產生運行時錯誤

### 原有驗證步驟（已移除）
```csharp
// 第一步：檢查採購單是否已儲存到資料庫
if (!PurchaseOrderId.HasValue || PurchaseOrderId.Value <= 0)
    
// 第二步：檢查實體資料是否存在
if (editModalComponent?.Entity == null)
    
// 第三步：檢查採購單是否真的存在於資料庫中
var existingOrder = await PurchaseOrderService.GetByIdAsync(...)
    
// 第四步：檢查是否有廠商資料
if (editModalComponent.Entity.SupplierId <= 0)
    
// 第五步：檢查是否有明細資料
if (!purchaseOrderDetails.Any())
    
// 第六步：檢查是否所有明細都已完成進貨
var hasIncompleteDetails = ...
```

---

## 解決方案

### 核心思想
**使用簡單的狀態變數控制轉單流程，所有業務驗證交由目標單據（進貨單）Modal 處理**

### 設計原則
1. **單一職責**：前端只負責控制「是否可轉單」，業務驗證交給服務層
2. **狀態驅動**：使用布林變數 `canCreateReceiving` 控制整個流程
3. **資料可靠**：使用 `Entity.Id` 而非參數傳遞 ID
4. **簡化邏輯**：從 6 步驗證簡化為 1 步狀態檢查

---

## 具體修改

### 1. 新增狀態變數

**位置**：內部狀態區域

```csharp
// ===== 轉單狀態 =====
private bool canCreateReceiving = false; 
// 是否可以轉進貨單（新增模式需先儲存，編輯模式直接為 true）
```

**用途**：
- 控制是否允許執行轉單操作
- 避免新增模式下未儲存就轉單

---

### 2. 初始化狀態（LoadPurchaseOrderData）

**位置**：`LoadPurchaseOrderData()` 方法

**新增模式**：
```csharp
if (!PurchaseOrderId.HasValue)
{
    // ... 其他邏輯 ...
    
    // 新增模式：必須先儲存才能轉單
    canCreateReceiving = false;
    
    return newOrder;
}
```

**編輯模式**：
```csharp
// 編輯模式 - 載入採購單和明細
canCreateReceiving = true; // 編輯模式：可以直接轉單
var purchaseOrder = await PurchaseOrderService.GetByIdAsync(PurchaseOrderId.Value);
```

**邏輯說明**：
- 新增時：`canCreateReceiving = false`（未儲存，不可轉）
- 編輯時：`canCreateReceiving = true`（已存在資料庫，可轉）

---

### 3. 儲存成功後啟用轉單（SavePurchaseOrderWrapper）

**位置**：`SavePurchaseOrderWrapper()` 方法

**修改前**：
```csharp
private async Task<bool> SavePurchaseOrderWrapper(PurchaseOrder purchaseOrder)
{
    if (!await ValidatePurchaseOrderDetailsAsync(purchaseOrder))
    {
        return false;
    }
    
    return await SavePurchaseOrderWithDetails(purchaseOrder, isPreApprovalSave: false);
}
```

**修改後**：
```csharp
private async Task<bool> SavePurchaseOrderWrapper(PurchaseOrder purchaseOrder)
{
    if (!await ValidatePurchaseOrderDetailsAsync(purchaseOrder))
    {
        return false;
    }
    
    var result = await SavePurchaseOrderWithDetails(purchaseOrder, isPreApprovalSave: false);
    
    // 儲存成功後允許轉進貨
    if (result)
    {
        canCreateReceiving = true;
    }
    
    return result;
}
```

**關鍵邏輯**：
- 儲存成功 → `canCreateReceiving = true`
- 新增模式第一次儲存後，即可轉單

---

### 4. 大幅簡化轉單邏輯（HandleCreateReceivingFromOrder）

**位置**：`HandleCreateReceivingFromOrder()` 方法

**修改前（60+ 行）**：
```csharp
private async Task HandleCreateReceivingFromOrder()
{
    // 第一步：檢查 PurchaseOrderId
    // 第二步：檢查 Entity
    // 第三步：資料庫查詢驗證
    // 第四步：檢查廠商
    // 第五步：檢查明細
    // 第六步：檢查完成進貨狀態
    // ... 60+ 行程式碼
}
```

**修改後（15 行）**：
```csharp
/// <summary>
/// 從採購單轉進貨單 - 簡化版本，所有業務驗證交給進貨單 Modal 處理
/// </summary>
private async Task HandleCreateReceivingFromOrder()
{
    try
    {
        // 唯一檢查：確認是否已儲存（新增模式需先儲存，編輯模式直接通過）
        if (!canCreateReceiving)
        {
            await NotificationService.ShowWarningAsync("請先儲存採購單後，才能轉進貨單");
            return;
        }
        
        // 安全檢查：確保 Entity 已載入（canCreateReceiving = true 時理論上一定有值）
        if (editModalComponent?.Entity == null)
        {
            await NotificationService.ShowWarningAsync("採購單資料載入失敗，請重新整理頁面");
            return;
        }
        
        // 直接開啟進貨單 Modal，使用 Entity.Id 作為採購單 ID（最可靠的來源）
        // 所有業務驗證（廠商、明細、完成進貨等）交給 Modal 內部處理
        await purchaseReceivingEditModal!.ShowAddModalWithPrefilledOrder(
            editModalComponent.Entity.SupplierId,
            editModalComponent.Entity.Id  // 使用 Entity.Id 而非 PurchaseOrderId 參數
        );
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleCreateReceivingFromOrder), GetType(), 
            additionalData: $"轉進貨單失敗 - OrderId: {editModalComponent?.Entity?.Id}");
        await NotificationService.ShowErrorAsync($"轉進貨單時發生錯誤: {ex.Message}");
    }
}
```

**關鍵改進**：
1. **移除 6 步驗證** → 只保留 1 個狀態檢查
2. **責任分離** → 業務驗證交給進貨單 Modal
3. **資料來源** → 使用 `Entity.Id` 而非 `PurchaseOrderId` 參數（更可靠）
4. **程式碼量** → 從 60+ 行減少到 15 行

---

### 5. 更新按鈕註解

**位置**：`CustomActionButtons` RenderFragment

**修改前**：
```csharp
@* 轉進貨單按鈕 - 編輯模式下始終顯示，驗證邏輯在點擊時執行 *@
```

**修改後**：
```csharp
@* 轉進貨單按鈕 - 新增模式需先儲存，編輯模式直接可用 *@
```

---

## 使用情境測試

### 情況 1：新增 → 儲存 → 直接轉進貨

**流程**：
1. 開啟新增 Modal → `canCreateReceiving = false`
2. 填寫資料並按下儲存 → 儲存成功 → `canCreateReceiving = true`
3. 點擊「轉進貨」→ 檢查通過 → 開啟進貨單 Modal

**結果**：✅ 正常運作

---

### 情況 2：新增 → 儲存 → 關閉 → 編輯 → 轉進貨

**流程**：
1. 開啟新增 Modal → `canCreateReceiving = false`
2. 填寫資料並儲存 → `canCreateReceiving = true`
3. 關閉 Modal
4. 重新開啟編輯 Modal → `LoadPurchaseOrderData()` → `canCreateReceiving = true`
5. 點擊「轉進貨」→ 檢查通過 → 開啟進貨單 Modal

**結果**：✅ 正常運作

---

### 情況 3：新增 → 未儲存 → 轉進貨

**流程**：
1. 開啟新增 Modal → `canCreateReceiving = false`
2. 填寫資料（未儲存）
3. 點擊「轉進貨」→ 檢查失敗 → 顯示提示「請先儲存採購單後，才能轉進貨單」

**結果**：✅ 正確阻擋

---

## 技術要點

### 為什麼使用 `Entity.Id` 而非 `PurchaseOrderId` 參數？

**問題**：
```csharp
[Parameter] public int? PurchaseOrderId { get; set; }
```
- `[Parameter]` 是父子組件通訊用，直接賦值可能不會立即同步
- 在 `SavePurchaseOrderWithDetails` 中有 `PurchaseOrderId = result.Data.Id;`
- 但這個賦值可能不會觸發父組件更新

**解決方案**：
```csharp
// 使用 Entity.Id（最可靠的資料來源）
editModalComponent.Entity.Id
```

**優勢**：
- ✅ 直接從資料庫返回的實體取得 ID
- ✅ 儲存成功後一定有正確的值
- ✅ 不依賴參數同步機制
- ✅ 避免 nullable 錯誤

---

### 為什麼移除業務驗證？

**原因**：
1. **廠商檢查** → 儲存時已驗證（必填欄位）
2. **明細檢查** → `ValidatePurchaseOrderDetailsAsync` 已驗證
3. **資料庫存在性** → `canCreateReceiving = true` 即代表已儲存
4. **完成進貨檢查** → 應由進貨單 Modal 處理（業務邏輯）

**責任分離**：
- 前端：控制「是否可轉單」（已儲存？）
- 服務層/Modal：驗證業務邏輯（廠商、明細、完成狀態等）

---

## 效益總結

### 轉單功能改進
| 指標 | 修改前 | 修改後 | 改進 |
|------|--------|--------|------|
| 程式碼行數 | 60+ 行 | 15 行 | **75% 減少** |
| 驗證步驟 | 6 步 | 1 步 | **83% 簡化** |
| 複雜度 | 高 | 低 | **大幅降低** |
| 可維護性 | 差 | 優 | **顯著提升** |

### 列印功能改進（2025-11-15 新增）
| 指標 | 修改前 | 修改後 | 改進 |
|------|--------|--------|------|
| 資料來源 | `PurchaseOrderId` 參數 | `Entity.Id` | **更可靠** |
| 驗證方式 | Helper 複雜驗證 | 狀態檢查 | **更簡潔** |
| 使用體驗 | 需關閉 Modal 再開啟 | 儲存後立即可用 | **更流暢** |
| 設計一致性 | 獨立邏輯 | 與轉單相同模式 | **更統一** |

### 技術優勢
✅ **單一職責**：前端只管狀態，業務驗證交給服務層  
✅ **資料可靠**：使用 `Entity.Id` 避免參數同步問題  
✅ **易於維護**：邏輯清晰，程式碼簡潔  
✅ **錯誤更少**：減少 nullable 處理，降低運行時錯誤風險  
✅ **擴展性強**：可套用至所有「需儲存後執行」的功能  
✅ **設計統一**：轉單、列印等功能使用相同模式  

### 使用者體驗
✅ **清晰提示**：新增模式未儲存時明確提示  
✅ **操作流暢**：編輯模式直接可用，儲存後立即啟用  
✅ **無需重開**：儲存後不需關閉 Modal 即可執行功能  
✅ **錯誤處理**：完善的異常捕捉和使用者友善的錯誤訊息  

---

## 套用到其他功能場景

此設計模式不僅適用於「A單轉B單」，也適用於任何需要「儲存後才能執行」的功能。

### 範例 1：報價單轉銷貨單

```csharp
// 1. 新增狀態變數
private bool canCreateSalesOrder = false;

// 2. LoadQuotationData 中初始化
if (!QuotationId.HasValue)
    canCreateSalesOrder = false;  // 新增模式
else
    canCreateSalesOrder = true;   // 編輯模式

// 3. SaveQuotationWrapper 中啟用
if (result)
    canCreateSalesOrder = true;

// 4. HandleCreateSalesOrderFromQuotation 簡化
private async Task HandleCreateSalesOrderFromQuotation()
{
    if (!canCreateSalesOrder)
    {
        await NotificationService.ShowWarningAsync("請先儲存報價單後，才能轉銷貨單");
        return;
    }
    
    if (editModalComponent?.Entity == null)
    {
        await NotificationService.ShowWarningAsync("報價單資料載入失敗，請重新整理頁面");
        return;
    }
    
    await salesOrderEditModal!.ShowAddModalWithPrefilledQuotation(
        editModalComponent.Entity.CustomerId,
        editModalComponent.Entity.Id
    );
}
```

### 範例 2：列印功能（2025-11-15 新增）

**問題背景**：
- 原本使用 `PurchaseOrderId` 參數進行驗證
- 儲存後未關閉 Modal 時，列印按鈕顯示「請先儲存採購單後再進行列印」
- 需要完全關閉 Modal 再重新開啟才能列印

**解決方案**：套用相同的狀態管理模式

```csharp
// 1. 新增狀態變數（與轉單狀態並列）
private bool canPrint = false;

// 2. LoadPurchaseOrderData 中初始化
if (!PurchaseOrderId.HasValue)
{
    canCreateReceiving = false;
    canPrint = false;  // 新增模式：必須先儲存才能列印
    return newOrder;
}
else
{
    canCreateReceiving = true;
    canPrint = true;  // 編輯模式：可以直接列印
}

// 3. SavePurchaseOrderWrapper 中啟用
if (result)
{
    canCreateReceiving = true;
    canPrint = true;  // 儲存成功後允許列印
}

// 4. HandlePrint 簡化（從 ~15 行簡化為 ~12 行）
private async Task HandlePrint()
{
    try
    {
        // 第一步：確認是否已儲存
        if (!canPrint)
        {
            await NotificationService.ShowWarningAsync("請先儲存採購單後再進行列印");
            return;
        }
        
        // 第二步：安全檢查 - 確保 Entity 已載入
        if (editModalComponent?.Entity == null)
        {
            await NotificationService.ShowWarningAsync("採購單資料載入失敗，請重新整理頁面");
            return;
        }
        
        // 第三步：根據系統參數檢查是否需要審核才能列印
        if (isApprovalEnabled && !editModalComponent.Entity.IsApproved)
        {
            await NotificationService.ShowWarningAsync("採購單需審核通過後才能列印");
            return;
        }
        
        // 直接執行列印，使用 Entity.Id（最可靠的來源）
        await HandleDirectPrint(null);
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandlePrint), GetType(), 
            additionalData: $"採購單列印處理失敗 - ID: {editModalComponent?.Entity?.Id}");
        await NotificationService.ShowErrorAsync("列印處理時發生錯誤");
    }
}

// 5. HandleDirectPrint 改用 Entity.Id
private async Task HandleDirectPrint(ReportPrintConfiguration? printConfig)
{
    // 使用 Entity.Id 而非 PurchaseOrderId 參數（最可靠的來源）
    var printUrl = ReportPrintHelper.BuildPrintUrl(
        baseUrl: NavigationManager.BaseUri,
        reportType: "purchase-report/order",
        documentId: editModalComponent.Entity.Id,  // 改用 Entity.Id
        configuration: printConfig,
        autoprint: true
    );
    // ... 其餘邏輯
}
```

**關鍵改進**：
1. **移除複雜驗證** → 使用 `canPrint` 狀態變數
2. **資料來源** → 使用 `Entity.Id` 而非 `PurchaseOrderId` 參數
3. **立即可用** → 儲存成功後不需關閉 Modal 即可列印
4. **一致性** → 與轉單功能使用相同的設計模式

### 通用模式總結

**適用場景**：
- ✅ A單轉B單（採購單轉進貨單、報價單轉銷貨單等）
- ✅ 列印功能（需要儲存後才能列印）
- ✅ 任何「新增模式需先儲存，編輯模式直接可用」的功能

**實作步驟**：
```csharp
// 1. 新增狀態變數
private bool canExecuteAction = false;

// 2. 初始化（新增 = false，編輯 = true）
if (!EntityId.HasValue)
    canExecuteAction = false;  // 新增模式：需先儲存
else
    canExecuteAction = true;   // 編輯模式：直接可用

// 3. 儲存成功啟用
if (saveResult)
    canExecuteAction = true;

// 4. 執行動作前檢查
if (!canExecuteAction)
{
    await NotificationService.ShowWarningAsync("請先儲存後再執行此操作");
    return;
}

// 5. 使用 Entity.Id 而非參數 ID
使用目標單據/功能(Entity.Id);
```

**核心原則**：
1. **狀態驅動**：用布林變數控制功能是否可用
2. **資料可靠**：使用 `Entity.Id` 而非參數 ID
3. **簡化驗證**：只檢查「是否已儲存」，其他交給服務層
4. **一致體驗**：儲存後立即可用，無需關閉 Modal

---

## 注意事項

1. **不要過度驗證**：已在儲存時驗證的邏輯不要重複檢查
2. **使用 Entity.Id**：優先使用實體 ID 而非參數 ID（避免同步問題）
3. **責任分離**：業務驗證交給服務層或目標 Modal
4. **狀態管理**：確保狀態變數在正確時機更新
5. **異常處理**：保留完善的 try-catch 和使用者友善的錯誤訊息
6. **一致性原則**：同類型功能使用相同的設計模式（如轉單、列印等）

---

## 常見問題 (FAQ)

### Q1: 為什麼不在 SavePurchaseOrderWithDetails 中更新 PurchaseOrderId 參數？
**A**: `[Parameter]` 屬性主要用於父子組件通訊，直接在組件內部賦值可能不會觸發參數更新事件。使用 `Entity.Id` 更可靠，因為它是資料庫返回的實際值。

### Q2: 如果儲存失敗，狀態變數會怎樣？
**A**: 只有在 `result = true` 時才會設定 `canPrint = true` 或 `canCreateReceiving = true`，儲存失敗時狀態不變，確保安全性。

### Q3: 編輯模式為什麼可以直接設定為 true？
**A**: 編輯模式意味著資料已存在資料庫中（有 ID），因此可以直接執行需要 ID 的操作。

### Q4: 這個模式可以套用到所有功能嗎？
**A**: 適用於「新增模式需先儲存，編輯模式直接可用」的功能。如果功能不需要資料庫 ID，則不需要此模式。

### Q5: 如果有多個功能需要不同的驗證條件怎麼辦？
**A**: 可以為每個功能設定獨立的狀態變數（如 `canPrint`、`canCreateReceiving`），並在各自的 handler 中加入特定的業務邏輯檢查。

---

## 參考文件

- [README_A單轉B單.md](./README_A單轉B單.md) - 原有設計說明
- [README_主檔鎖住設計.md](./README_主檔鎖住設計.md) - 相關鎖定邏輯
- [README_更新明細元件在Action編輯之後說明.md](./README_更新明細元件在Action編輯之後說明.md) - 明細更新機制

---

## 修改歷史

| 日期 | 修改者 | 說明 |
|------|--------|------|
| 2025-11-14 | 系統 | 初版：採購單轉進貨單簡化修改 |
| 2025-11-15 | 系統 | 新增列印功能套用相同模式，擴充為通用設計模式文件 |

---

## 相關問題與解決記錄

### 問題：儲存後列印仍顯示「請先儲存」警告（2025-11-15）

**症狀**：
- 儲存已修改為不關閉 Modal
- 資料已正確儲存到資料庫
- 但按下列印按鈕仍出現「請先儲存採購單後再進行列印」警告
- 必須完全關閉 Modal 再重新開啟才能列印

**原因分析**：
1. `HandlePrint` 使用 `PurchaseOrderId` 參數進行驗證
2. 新增模式儲存時，雖然更新了 `PurchaseOrderId = result.Data.Id`（第 724 行）
3. 但 `[Parameter]` 屬性不會立即同步，導致驗證失敗
4. 與「轉進貨」按鈕之前遇到的問題完全相同

**解決方案**：
套用「A單轉B單簡化修改」的設計模式：
1. 新增 `canPrint` 狀態變數
2. 在 `LoadPurchaseOrderData` 中初始化（新增=false，編輯=true）
3. 在 `SavePurchaseOrderWrapper` 中啟用（儲存成功後設為 true）
4. 簡化 `HandlePrint` 驗證邏輯（只檢查狀態變數）
5. 改用 `Entity.Id` 而非 `PurchaseOrderId` 參數

**效果**：
- ✅ 儲存後立即可列印，無需關閉 Modal
- ✅ 與轉單功能使用相同設計模式，提高一致性
- ✅ 程式碼更簡潔可靠
