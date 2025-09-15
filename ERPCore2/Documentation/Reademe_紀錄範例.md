## 日誌修改範本

### 描述
- 用一到兩句話簡潔描述觀察到的錯誤或異常行為，目標是讓人一看就能理解問題（避免過多細節）。

### 原因
- 說明造成錯誤的程式碼或邏輯原因，包含相關檔案、方法或程式片段位置，必要時貼出片段或行號。
- 每一個小標題主題使用此符號❌作為開始

### 解決
- 提供可直接套用的修正程式碼或重寫方式，並簡短說明修正的關鍵點與風險（如需要 migrations 或資料重整也要註明）。
- 每一個小標題主題使用此符號✅作為開始

### 修改檔
- 紀錄修改的檔案位置

### 備註
- 圖示的使用 > 不需要過多的圖示來說明，僅在說明正確與錯誤可用✅❌此兩個符號，其餘地方不要使用任何圖示

## 以下為範例參考

### 描述 
第一次編輯: 商品A 20→21個 → 庫存 21個 ✅  
第二次編輯: 商品A 21→22個 → 庫存 23個 ❌ (應該22個)
第三次編輯: 商品A 30→20個 → 庫存 36個 ❌ (應該20個，減量時不生效)

### 原因
❌交易記錄查詢不完整
原始邏輯只查詢了 `Purchase` 類型的交易記錄：
```csharp
// 問題代碼
var existingTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber == currentReceiving.ReceiptNumber && 
           t.TransactionType == InventoryTransactionTypeEnum.Purchase)
```

但編輯時會產生多種交易號：
- 原始進貨：`"R202501140001"`
- 編輯調增：`"R202501140001_ADJ"`  
- 編輯回退：`"R202501140001_REVERT"`

❌差異計算基準點錯誤
每次編輯都是拿 **當前明細** 與 **原始交易記錄** 比較，而不是與 **上一次編輯後的狀態** 比較。

### 解決
✅完整統計所有相關交易記錄
```csharp
// 修正後 - 包含所有相關交易記錄
var existingTransactions = await context.InventoryTransactions
    .Where(t => (t.TransactionNumber == currentReceiving.ReceiptNumber ||
               t.TransactionNumber.StartsWith(currentReceiving.ReceiptNumber + "_")) &&
           !t.IsDeleted)
    .ToListAsync();
```

✅使用淨值計算方式
```csharp
// 計算已處理的庫存淨值（所有交易記錄 Quantity 的總和）
int processedQuantity = hasProcessed ? processedInventory[key].NetProcessedQuantity : 0;

// 計算目標庫存數量（當前明細應該有的數量）
int targetQuantity = hasCurrent ? currentInventory[key].CurrentQuantity : 0;

// 精確計算需要調整的數量
int adjustmentNeeded = targetQuantity - processedQuantity;
```

✅防重複更新機制
```csharp
// 編輯模式判斷
bool isEditMode = PurchaseReceivingId.HasValue && PurchaseReceivingId.Value > 0;

if (isEditMode)
{
    // 使用差異比較更新庫存
    var differenceResult = await PurchaseReceivingService.UpdateInventoryByDifferenceAsync(purchaseReceiving.Id, 0);
}
```

### 修改檔
- `Services/Purchase/PurchaseReceivingService.cs` - 修正 `UpdateInventoryByDifferenceAsync` 方法
- `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor` - 優化庫存更新邏輯