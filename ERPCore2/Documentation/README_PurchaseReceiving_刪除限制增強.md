# 採購進貨單刪除限制增強

## 📋 功能說明

### 目標
如果進貨單的任何明細被鎖定（有退貨記錄或沖款記錄），則整個進貨單都不能刪除。

### 實作位置
- **Service 層**: `PurchaseReceivingService.cs` - 覆寫 `CanDeleteAsync` 方法
- **Component 層**: `PurchaseReceivingDetailManagerComponent.razor` - 明細項目的刪除限制

---

## 🔍 刪除檢查邏輯

### 主檔層級（PurchaseReceivingService）

```csharp
protected override async Task<ServiceResult> CanDeleteAsync(PurchaseReceiving entity)
{
    // 1. 基類檢查（外鍵關聯等）
    var baseResult = await base.CanDeleteAsync(entity);
    if (!baseResult.IsSuccess) return baseResult;
    
    // 2. 載入明細資料
    // 3. 檢查每個明細項目：
    //    - 退貨記錄檢查
    //    - 沖款記錄檢查
    // 4. 任一明細被鎖定則拒絕刪除
}
```

### 明細層級（PurchaseReceivingDetailManagerComponent）

```csharp
private bool CanDeleteItem(ReceivingItem item, out string reason)
{
    // 檢查 1：退貨記錄
    if (HasReturnRecord(item)) { ... }
    
    // 檢查 2：沖款記錄
    if (HasPaymentRecord(item)) { ... }
    
    return true;
}
```

---

## 🧪 測試案例

### 測試案例 1：正常刪除（無限制）
**條件**：
- 進貨單存在
- 所有明細都沒有退貨記錄
- 所有明細都沒有沖款記錄（TotalPaidAmount = 0）

**預期結果**：
✅ 可以成功刪除整個進貨單
✅ 庫存正確回退
✅ 採購單的已進貨數量正確回退

**測試步驟**：
1. 建立一筆測試進貨單（含 2-3 個明細）
2. 不進行退貨操作
3. 不進行沖款操作
4. 在進貨管理頁面點擊「刪除」按鈕
5. 確認刪除對話框
6. 檢查刪除是否成功

---

### 測試案例 2：有退貨記錄（限制刪除）
**條件**：
- 進貨單存在
- 至少一個明細有退貨記錄
- 其他明細可能正常

**預期結果**：
❌ 無法刪除進貨單
❌ 顯示錯誤訊息：「無法刪除此進貨單，因為商品「XXX」已有退貨記錄（已退貨 N 個）」

**測試步驟**：
1. 建立一筆測試進貨單（含 2-3 個明細）
2. 針對其中一個明細建立退貨單
3. 在進貨管理頁面點擊「刪除」按鈕
4. 確認刪除對話框
5. 檢查是否顯示正確的錯誤訊息
6. 確認進貨單未被刪除

---

### 測試案例 3：有沖款記錄（限制刪除）
**條件**：
- 進貨單存在
- 至少一個明細有沖款記錄（TotalPaidAmount > 0）
- 其他明細可能正常

**預期結果**：
❌ 無法刪除進貨單
❌ 顯示錯誤訊息：「無法刪除此進貨單，因為商品「XXX」已有沖款記錄（已沖款 N 元）」

**測試步驟**：
1. 建立一筆測試進貨單（含 2-3 個明細）
2. 針對進貨單建立沖款記錄（更新 PurchaseReceivingDetail.TotalPaidAmount）
3. 在進貨管理頁面點擊「刪除」按鈕
4. 確認刪除對話框
5. 檢查是否顯示正確的錯誤訊息
6. 確認進貨單未被刪除

---

### 測試案例 4：混合情況（部分限制）
**條件**：
- 進貨單有 3 個明細
- 明細 1：正常（無退貨、無沖款）
- 明細 2：有退貨記錄
- 明細 3：有沖款記錄

**預期結果**：
❌ 無法刪除進貨單
❌ 顯示第一個被鎖定的明細的錯誤訊息

**測試步驟**：
1. 建立測試進貨單（含 3 個明細）
2. 明細 2 建立退貨記錄
3. 明細 3 建立沖款記錄
4. 嘗試刪除進貨單
5. 檢查錯誤訊息（應該是明細 2 或明細 3，取決於檢查順序）

---

## 🔄 級聯刪除驗證

### 驗證點 1：庫存回退
刪除進貨單時，系統應該：
✅ 從庫存中減去該進貨的數量
✅ 記錄庫存異動（類型：退貨）
✅ 更新商品的總庫存數量

**驗證方法**：
```sql
-- 刪除前記錄庫存數量
SELECT ProductId, WarehouseId, Quantity 
FROM InventoryStocks 
WHERE ProductId = [進貨商品ID] AND WarehouseId = [倉庫ID]

-- 執行刪除

-- 刪除後驗證庫存已減少
SELECT ProductId, WarehouseId, Quantity 
FROM InventoryStocks 
WHERE ProductId = [進貨商品ID] AND WarehouseId = [倉庫ID]
```

### 驗證點 2：採購單已進貨數量回退
刪除進貨單時，系統應該：
✅ 更新對應採購單明細的 ReceivedQuantity（減去已進貨數量）
✅ 採購單狀態可能從「已完成」變回「部分進貨」或「待進貨」

**驗證方法**：
```sql
-- 刪除前記錄已進貨數量
SELECT Id, ProductId, OrderQuantity, ReceivedQuantity 
FROM PurchaseOrderDetails 
WHERE PurchaseOrderId = [採購單ID]

-- 執行刪除

-- 刪除後驗證 ReceivedQuantity 已減少
SELECT Id, ProductId, OrderQuantity, ReceivedQuantity 
FROM PurchaseOrderDetails 
WHERE PurchaseOrderId = [採購單ID]
```

### 驗證點 3：進貨明細刪除
刪除進貨單時，系統應該：
✅ 級聯刪除所有關聯的進貨明細（PurchaseReceivingDetails）

**驗證方法**：
```sql
-- 刪除後驗證明細已不存在
SELECT COUNT(*) 
FROM PurchaseReceivingDetails 
WHERE PurchaseReceivingId = [已刪除的進貨單ID]
-- 應該返回 0
```

---

## 📝 程式碼修改清單

### 1. PurchaseReceivingService.cs

#### 新增欄位
```csharp
/// <summary>
/// 採購退貨明細服務 - 用於檢查進貨明細是否有退貨記錄
/// </summary>
private readonly IPurchaseReturnDetailService? _purchaseReturnDetailService;
```

#### 修改建構子
```csharp
public PurchaseReceivingService(
    IDbContextFactory<AppDbContext> contextFactory,
    ILogger<GenericManagementService<PurchaseReceiving>> logger,
    IInventoryStockService inventoryStockService,
    IPurchaseReceivingDetailService detailService,
    IPurchaseOrderDetailService purchaseOrderDetailService,
    IPurchaseReturnDetailService purchaseReturnDetailService) // ← 新增參數
    : base(contextFactory, logger)
{
    _inventoryStockService = inventoryStockService;
    _detailService = detailService;
    _purchaseOrderDetailService = purchaseOrderDetailService;
    _purchaseReturnDetailService = purchaseReturnDetailService; // ← 新增注入
}
```

#### 新增方法
```csharp
/// <summary>
/// 檢查採購進貨單是否可以被刪除
/// </summary>
protected override async Task<ServiceResult> CanDeleteAsync(PurchaseReceiving entity)
{
    // 實作內容見上方邏輯說明
}
```

---

## 🎯 與現有邏輯的關係

### Component 層（明細項目控制）
- **位置**: `PurchaseReceivingDetailManagerComponent.razor`
- **功能**: 控制**單一明細項目**的刪除和編輯
- **UI 表現**: 
  - 有退貨/沖款記錄的明細會顯示🔒圖示
  - 該明細的所有欄位變為唯讀
  - 不顯示刪除按鈕

### Service 層（主檔整體控制）
- **位置**: `PurchaseReceivingService.CanDeleteAsync`
- **功能**: 控制**整個主檔**的刪除
- **UI 表現**: 
  - 在列表頁點擊刪除時進行檢查
  - 如果有任何明細被鎖定，顯示錯誤訊息
  - 無法刪除整個進貨單

### 兩者關係
```
使用者操作流程：
1. 編輯進貨單 → Component 檢查 → 個別明細的編輯/刪除限制
2. 列表頁刪除 → Service 檢查 → 整個主檔的刪除限制
```

**設計原則**：
- Component 層：細粒度控制，提供即時視覺回饋
- Service 層：粗粒度控制，最終把關資料完整性

---

## ⚠️ 注意事項

1. **服務注入**：確保 DI 容器中已註冊 `IPurchaseReturnDetailService`
2. **效能考量**：每次刪除都會查詢所有明細的退貨記錄，對於明細很多的進貨單可能影響效能
3. **錯誤訊息**：只顯示第一個被鎖定的明細資訊，不會列出所有被鎖定的明細
4. **向下相容**：如果 `_purchaseReturnDetailService` 為 null（舊版測試環境），退貨檢查會被跳過

---

## 🔧 故障排除

### 問題 1：刪除時沒有檢查退貨記錄
**可能原因**：
- `IPurchaseReturnDetailService` 沒有正確注入
- 建構子使用了簡化版本（不含退貨服務）

**解決方法**：
檢查 `ServiceRegistration.cs` 是否正確註冊服務

### 問題 2：有退貨記錄但仍可刪除
**可能原因**：
- `CanDeleteAsync` 方法沒有被正確覆寫
- 基類的檢查邏輯被破壞

**解決方法**：
使用偵錯器確認 `CanDeleteAsync` 是否被呼叫

### 問題 3：級聯刪除失效
**可能原因**：
- 資料庫的外鍵設定不正確
- `PermanentDeleteAsync` 方法的邏輯有誤

**解決方法**：
檢查資料庫外鍵設定和刪除邏輯

---

## 📅 變更歷史

| 日期 | 版本 | 變更內容 | 作者 |
|------|------|----------|------|
| 2025-10-11 | 1.0 | 初始版本 - 實作主檔刪除的明細鎖定檢查 | GitHub Copilot |

---

## 🔗 相關文件

- [刪除限制設計](./README_刪除限制設計.md)
- [進貨退貨明細刪除限制設計](./README_PurchaseReturnDetail_刪除限制設計.md)
- [沖款金額自動計算](./README_稅額自動計算.md)
