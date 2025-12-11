# 庫存異動正確撰寫方式

## 目錄
- [1. 系統架構總覽](#1-系統架構總覽)
- [2. 核心服務與資料表結構](#2-核心服務與資料表結構)
- [3. 庫存異動流程原理](#3-庫存異動流程原理)
- [4. 完整流程範例](#4-完整流程範例)
- [5. 庫存異動規則與約定](#5-庫存異動規則與約定)
- [6. 實際案例分析](#6-實際案例分析)
- [7. 常見錯誤與解決方案](#7-常見錯誤與解決方案)
- [8. 開發新功能指南](#8-開發新功能指南)

---

## 1. 系統架構總覽

### 1.1 庫存管理的三層架構

```
┌─────────────────────────────────────────────────────────┐
│                    UI 層 (Blazor Components)             │
│  例如: PurchaseReceivingEditModalComponent.razor        │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  業務邏輯層 (Services)                    │
│  - PurchaseReceivingService (進貨單服務)                 │
│  - PurchaseReturnService (退貨單服務)                    │
│  - SalesDeliveryService (銷貨單服務)                     │
│  └──► 呼叫 InventoryStockService (庫存服務)             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  資料存取層 (Data Layer)                  │
│  - InventoryStock (庫存主檔)                             │
│  - InventoryStockDetail (庫存明細)                       │
│  - InventoryTransaction (庫存異動記錄)                   │
└─────────────────────────────────────────────────────────┘
```

### 1.2 核心設計理念

**關注點分離 (Separation of Concerns)**
- ✅ **業務服務**只負責業務邏輯，不直接操作庫存表
- ✅ **InventoryStockService** 是唯一允許操作庫存的服務
- ✅ 所有庫存變動必須透過標準 API：`AddStockAsync`、`ReduceStockAsync`、`TransferStockAsync`

---

## 2. 核心服務與資料表結構

### 2.1 InventoryStock (庫存主檔)

**用途**: 每個商品一筆主檔記錄

```csharp
public class InventoryStock : BaseEntity
{
    public int ProductId { get; set; }  // 商品ID (唯一)
    
    // 導航屬性
    public Product Product { get; set; }
    public ICollection<InventoryStockDetail> InventoryStockDetails { get; set; }
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
}
```

### 2.2 InventoryStockDetail (庫存明細)

**用途**: 記錄商品在各倉庫/庫位的庫存數量

```csharp
public class InventoryStockDetail : BaseEntity
{
    public int InventoryStockId { get; set; }       // 關聯主檔
    public int WarehouseId { get; set; }            // 倉庫ID
    public int? WarehouseLocationId { get; set; }   // 庫位ID (可選)
    
    // 庫存數量
    public int CurrentStock { get; set; }           // 現有庫存
    public int ReservedStock { get; set; }          // 預留庫存
    public int InTransitStock { get; set; }         // 在途庫存
    public int AvailableStock => CurrentStock - ReservedStock;  // 可用庫存
    
    // 批次資訊
    public string? BatchNumber { get; set; }        // 批號
    public DateTime BatchDate { get; set; }         // 批次日期
    public DateTime? ExpiryDate { get; set; }       // 到期日
    
    // 成本資訊
    public decimal? AverageCost { get; set; }       // 平均成本
    public DateTime LastTransactionDate { get; set; }
}
```

**重要概念**: 組合鍵 = `ProductId + WarehouseId + WarehouseLocationId`

### 2.3 InventoryTransaction (庫存異動記錄)

**用途**: 記錄所有庫存變動的歷史軌跡（不可刪除、只能新增）

```csharp
public class InventoryTransaction : BaseEntity
{
    public string TransactionNumber { get; set; }           // 交易單號 (來源單號)
    public InventoryTransactionTypeEnum TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    
    public int Quantity { get; set; }                       // 異動數量 (入庫為正，出庫為負)
    public decimal? UnitCost { get; set; }                  // 單位成本
    
    public int StockBefore { get; set; }                    // 異動前庫存
    public int StockAfter { get; set; }                     // 異動後庫存
    
    // 批號追蹤欄位
    public string? TransactionBatchNumber { get; set; }     // 交易批號
    public DateTime? TransactionBatchDate { get; set; }     // 交易批次進貨日期
    public DateTime? TransactionExpiryDate { get; set; }    // 交易批次到期日期
    
    // 關聯
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int? WarehouseLocationId { get; set; }
    public int? InventoryStockId { get; set; }
    public int? InventoryStockDetailId { get; set; }
}
```

**重要**: `InventoryTransaction` 是唯讀的歷史記錄，永遠不應該被更新或刪除

### 2.4 InventoryTransactionTypeEnum

```csharp
public enum InventoryTransactionTypeEnum
{
    Purchase = 1,          // 採購入庫
    Sales = 2,             // 銷貨出庫
    Return = 3,            // 退貨 (採購退出/銷貨退回)
    Transfer = 4,          // 庫存調撥
    Adjustment = 5,        // 庫存調整
    Production = 6,        // 生產入庫
    MaterialIssue = 7      // 領料出庫
}
```

---

## 3. 庫存異動流程原理

### 3.1 InventoryStockService 核心 API

#### **AddStockAsync** - 增加庫存

```csharp
public async Task<ServiceResult> AddStockAsync(
    int productId,                          // 商品ID
    int warehouseId,                        // 倉庫ID
    int quantity,                           // 增加數量 (必須 > 0)
    InventoryTransactionTypeEnum transactionType,  // 交易類型
    string transactionNumber,               // 來源單號
    decimal? unitCost = null,               // 單位成本 (用於計算平均成本)
    int? locationId = null,                 // 庫位ID (可選)
    string? remarks = null,                 // 備註
    string? batchNumber = null,             // 批號
    DateTime? batchDate = null,             // 批次日期
    DateTime? expiryDate = null             // 到期日
)
```

**處理流程**:
1. 取得或建立 `InventoryStock` (依 ProductId)
2. 取得或建立 `InventoryStockDetail` (依 WarehouseId + LocationId)
3. 更新 `CurrentStock += quantity`
4. 更新平均成本 (加權平均法)
5. 建立 `InventoryTransaction` 記錄 (Quantity = 正數)

#### **ReduceStockAsync** - 減少庫存

```csharp
public async Task<ServiceResult> ReduceStockAsync(
    int productId,                          // 商品ID
    int warehouseId,                        // 倉庫ID
    int quantity,                           // 減少數量 (必須 > 0)
    InventoryTransactionTypeEnum transactionType,  // 交易類型
    string transactionNumber,               // 來源單號
    int? locationId = null,                 // 庫位ID (可選)
    string? remarks = null                  // 備註
)
```

**處理流程**:
1. 取得 `InventoryStock` 和 `InventoryStockDetail`
2. **檢查可用庫存**: `AvailableStock >= quantity`
3. 更新 `CurrentStock -= quantity`
4. 建立 `InventoryTransaction` 記錄 (Quantity = **負數**)

#### **TransferStockAsync** - 調撥庫存

```csharp
public async Task<ServiceResult> TransferStockAsync(
    int productId,
    int fromWarehouseId,       // 來源倉庫
    int toWarehouseId,         // 目標倉庫
    int quantity,
    string transactionNumber,
    int? fromLocationId = null,
    int? toLocationId = null,
    string? remarks = null
)
```

**處理流程**:
1. 呼叫 `ReduceStockAsync` 扣減來源倉庫
2. 呼叫 `AddStockAsync` 增加目標倉庫
3. 建立兩筆 `InventoryTransaction` 記錄

---

## 4. 完整流程範例

### 4.1 採購進貨流程 (PurchaseReceiving)

#### 使用者操作

```
1. 開啟進貨單編輯視窗 (PurchaseReceivingEditModalComponent)
2. 選擇廠商、倉庫
3. 新增進貨明細 (商品、數量、單價)
4. 點擊「儲存」
```

#### 背後處理流程

```csharp
// 位於: PurchaseReceivingEditModalComponent.razor
// GenericEditModalComponent 的 AfterSave 事件處理器

private async Task SavePurchaseReceivingDetailsAsync()
{
    // 1. 儲存主檔 (由 GenericEditModalComponent 自動完成)
    
    // 2. 儲存明細 (使用 PurchaseReceivingDetailService)
    var detailResult = await PurchaseReceivingDetailService
        .SaveDetailsAsync(savedId, purchaseReceivingDetails);
    
    // 3. 更新庫存 (重點在這裡！)
    var updateResult = await PurchaseReceivingService
        .UpdateInventoryByDifferenceAsync(savedId);
    
    // 4. 更新採購訂單的已進貨數量
    await UpdateReceivedQuantitiesAsync();
}
```

#### 庫存異動詳細步驟 (UpdateInventoryByDifferenceAsync)

```csharp
// 位於: PurchaseReceivingService.cs

public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0)
{
    // 步驟1: 查詢所有相關的庫存交易記錄
    var existingTransactions = await context.InventoryTransactions
        .Where(t => t.TransactionNumber == currentReceiving.Code ||
                   t.TransactionNumber.StartsWith(currentReceiving.Code + "_"))
        .ToListAsync();
    
    // 步驟2: 計算已處理的庫存淨值
    // 格式: ProductId_WarehouseId_LocationId -> 已處理數量
    var processedInventory = new Dictionary<string, int>();
    foreach (var trans in existingTransactions)
    {
        var key = $"{trans.ProductId}_{trans.WarehouseId}_{trans.WarehouseLocationId}";
        processedInventory[key] = processedInventory.GetValueOrDefault(key) + trans.Quantity;
    }
    
    // 步驟3: 計算當前明細的目標數量
    var currentInventory = new Dictionary<string, int>();
    foreach (var detail in currentReceiving.PurchaseReceivingDetails)
    {
        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId}";
        currentInventory[key] = currentInventory.GetValueOrDefault(key) + detail.ReceivedQuantity;
    }
    
    // 步驟4: 計算差異並執行庫存調整
    foreach (var key in allKeys)
    {
        int targetQuantity = currentInventory.GetValueOrDefault(key);      // 目標數量
        int processedQuantity = processedInventory.GetValueOrDefault(key); // 已處理數量
        int adjustmentNeeded = targetQuantity - processedQuantity;         // 需調整數量
        
        if (adjustmentNeeded > 0)
        {
            // 需要增加庫存
            await _inventoryStockService.AddStockAsync(
                productId, warehouseId, adjustmentNeeded,
                InventoryTransactionTypeEnum.Purchase,
                $"{code}_ADJ",  // 交易單號加上 _ADJ 後綴
                unitPrice, locationId,
                $"採購進貨編輯調增 - {code}"
            );
        }
        else if (adjustmentNeeded < 0)
        {
            // 需要減少庫存
            await _inventoryStockService.ReduceStockAsync(
                productId, warehouseId, Math.Abs(adjustmentNeeded),
                InventoryTransactionTypeEnum.Return,
                $"{code}_ADJ",
                locationId,
                $"採購進貨編輯調減 - {code}"
            );
        }
        // adjustmentNeeded == 0: 無需調整
    }
}
```

#### 範例說明

**場景1: 新增進貨單**

```
操作: 新增進貨單 PR0001
明細: 商品A x 100個，倉庫W1

處理過程:
1. existingTransactions = [] (沒有歷史記錄)
2. processedInventory = {}
3. currentInventory = { "A_W1_null": 100 }
4. adjustmentNeeded = 100 - 0 = 100
5. 執行: AddStockAsync(商品A, W1, 100, Purchase, "PR0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 100
- InventoryTransaction: PR0001_ADJ, Quantity = 100
```

**場景2: 修改進貨單 - 增加數量**

```
操作: 編輯進貨單 PR0001，將數量從 100 改為 150

處理過程:
1. existingTransactions = [PR0001_ADJ, Qty=100]
2. processedInventory = { "A_W1_null": 100 }
3. currentInventory = { "A_W1_null": 150 }
4. adjustmentNeeded = 150 - 100 = 50
5. 執行: AddStockAsync(商品A, W1, 50, Purchase, "PR0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 150
- InventoryTransaction: 新增一筆 PR0001_ADJ, Quantity = 50
```

**場景3: 修改進貨單 - 減少數量**

```
操作: 編輯進貨單 PR0001，將數量從 150 改為 80

處理過程:
1. existingTransactions = [PR0001_ADJ(100), PR0001_ADJ(50)]
2. processedInventory = { "A_W1_null": 150 }
3. currentInventory = { "A_W1_null": 80 }
4. adjustmentNeeded = 80 - 150 = -70
5. 執行: ReduceStockAsync(商品A, W1, 70, Return, "PR0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 80
- InventoryTransaction: 新增一筆 PR0001_ADJ, Quantity = -70
```

**場景4: 修改進貨單 - 更換商品**

```
操作: 編輯進貨單 PR0001，將商品A(80個) 改為商品B(80個)

處理過程:
1. existingTransactions = [商品A的記錄，總計150, 然後-70]
2. processedInventory = { "A_W1_null": 80 }
3. currentInventory = { "B_W1_null": 80 }
4. 商品A: adjustmentNeeded = 0 - 80 = -80 → ReduceStockAsync(商品A, 80)
5. 商品B: adjustmentNeeded = 80 - 0 = 80 → AddStockAsync(商品B, 80)

結果:
- 商品A庫存減少 80
- 商品B庫存增加 80
- 建立兩筆異動記錄
```

### 4.2 銷貨出貨流程 (SalesDelivery)

#### 使用者操作

```
1. 開啟出貨單編輯視窗 (SalesDeliveryEditModalComponent)
2. 選擇客戶、倉庫
3. 新增出貨明細 (商品、數量、單價)
4. 點擊「儲存」
```

#### 背後處理流程

```csharp
// 位於: SalesDeliveryEditModalComponent.razor
// GenericEditModalComponent 的 AfterSave 事件處理器

private async Task SaveSalesDeliveryDetailsAsync()
{
    // 1. 儲存主檔 (由 GenericEditModalComponent 自動完成)
    
    // 2. 儲存明細
    foreach (var detail in validDetails)
    {
        if (detail.Id == 0)
            await SalesDeliveryDetailService.CreateAsync(detail);
        else
            await SalesDeliveryDetailService.UpdateAsync(detail);
    }
    
    // 3. 更新庫存 (重點在這裡！)
    var inventoryUpdateResult = await SalesDeliveryService
        .UpdateInventoryByDifferenceAsync(savedId);
    
    // 4. 更新銷貨訂單的已出貨數量
    await UpdateDeliveredQuantitiesAsync();
}
```

#### 庫存異動詳細步驟 (UpdateInventoryByDifferenceAsync)

**關鍵差異**: 銷貨出貨是「出庫」操作，與採購進貨相反

```csharp
// 位於: SalesDeliveryService.cs

public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0)
{
    // 步驟1: 查詢所有相關的庫存交易記錄
    var existingTransactions = await context.InventoryTransactions
        .Where(t => t.TransactionNumber == currentDelivery.Code ||
                   t.TransactionNumber.StartsWith(currentDelivery.Code + "_"))
        .ToListAsync();
    
    // 步驟2: 計算已處理的庫存淨值（出庫記錄為負數）
    var processedInventory = new Dictionary<string, int>();
    foreach (var trans in existingTransactions)
    {
        var key = $"{trans.ProductId}_{trans.WarehouseId}_{trans.WarehouseLocationId}";
        processedInventory[key] = processedInventory.GetValueOrDefault(key) + trans.Quantity;
        // 注意：出庫的 Quantity 已經是負數
    }
    
    // 步驟3: 計算當前明細的目標數量（以負數表示出庫）
    var currentInventory = new Dictionary<string, int>();
    foreach (var detail in currentDelivery.DeliveryDetails)
    {
        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId}";
        currentInventory[key] = currentInventory.GetValueOrDefault(key) + detail.DeliveryQuantity;
    }
    
    // 步驟4: 計算差異並執行庫存調整
    foreach (var key in allKeys)
    {
        int targetQuantity = -currentInventory.GetValueOrDefault(key);  // 轉為負數（出庫）
        int processedQuantity = processedInventory.GetValueOrDefault(key);
        int adjustmentNeeded = targetQuantity - processedQuantity;
        
        if (adjustmentNeeded < 0)
        {
            // 需要扣減更多庫存（出貨數量增加）
            await _inventoryStockService.ReduceStockAsync(
                productId, warehouseId, Math.Abs(adjustmentNeeded),
                InventoryTransactionTypeEnum.Sales,
                $"{code}_ADJ",
                locationId,
                $"銷貨出貨編輯調增 - {code}"
            );
        }
        else if (adjustmentNeeded > 0)
        {
            // 需要回補庫存（出貨數量減少）
            await _inventoryStockService.AddStockAsync(
                productId, warehouseId, adjustmentNeeded,
                InventoryTransactionTypeEnum.Sales,
                $"{code}_ADJ",
                null,  // 銷貨回補不需要成本
                locationId,
                $"銷貨出貨編輯調減 - {code}"
            );
        }
    }
}
```

#### 範例說明

**場景1: 新增出貨單**

```
操作: 新增出貨單 SD0001
明細: 商品A x 100個，倉庫W1

處理過程:
1. existingTransactions = [] (沒有歷史記錄)
2. processedInventory = {}
3. currentInventory = { "A_W1_null": 100 } → 目標: -100 (出庫)
4. adjustmentNeeded = -100 - 0 = -100
5. 執行: ReduceStockAsync(商品A, W1, 100, Sales, "SD0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 原庫存 - 100
- InventoryTransaction: SD0001_ADJ, Quantity = -100 (負數表示出庫)
```

**場景2: 修改出貨單 - 增加數量**

```
操作: 編輯出貨單 SD0001，將數量從 100 改為 150

處理過程:
1. existingTransactions = [SD0001_ADJ, Qty=-100]
2. processedInventory = { "A_W1_null": -100 }
3. currentInventory = { "A_W1_null": 150 } → 目標: -150
4. adjustmentNeeded = -150 - (-100) = -50
5. 執行: ReduceStockAsync(商品A, W1, 50, Sales, "SD0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 原庫存 - 150
- InventoryTransaction: 新增一筆 SD0001_ADJ, Quantity = -50
```

**場景3: 修改出貨單 - 減少數量**

```
操作: 編輯出貨單 SD0001，將數量從 150 改為 80

處理過程:
1. existingTransactions = [SD0001_ADJ(-100), SD0001_ADJ(-50)]
2. processedInventory = { "A_W1_null": -150 }
3. currentInventory = { "A_W1_null": 80 } → 目標: -80
4. adjustmentNeeded = -80 - (-150) = 70
5. 執行: AddStockAsync(商品A, W1, 70, Sales, "SD0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 原庫存 - 80 (回補了70)
- InventoryTransaction: 新增一筆 SD0001_ADJ, Quantity = 70 (正數表示回補)
```

**場景4: 刪除出貨單**

```
操作: 刪除出貨單 SD0001

處理過程:
1. 遍歷所有明細
2. 對每個明細執行: AddStockAsync(商品A, W1, 80, Sales, "SD0001_DEL")

結果:
- InventoryStockDetail: CurrentStock = 原庫存（完全回補）
- InventoryTransaction: SD0001_DEL, Quantity = 80
```

### 4.3 採購退貨流程 (PurchaseReturn)

#### 使用者操作

```
1. 從進貨單點擊「轉退貨」或新增退貨單
2. 選擇要退貨的進貨明細
3. 輸入退貨數量
4. 點擊「儲存」
```

#### 庫存異動處理

```csharp
// 位於: PurchaseReturnService.cs - SaveWithDetailsAsync

// 退貨會減少庫存 (因為商品退還給廠商)
foreach (var (detail, quantityDiff) in stockChanges.Where(sc => sc.Item2 != 0))
{
    // 取得倉庫ID (從關聯的進貨明細取得)
    var receivingDetail = await context.PurchaseReceivingDetails
        .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseReceivingDetailId);
    var warehouseId = receivingDetail.WarehouseId;
    
    if (quantityDiff > 0)
    {
        // 退貨數量增加 → 減少庫存
        await _inventoryStockService.ReduceStockAsync(
            detail.ProductId,
            warehouseId,
            quantityDiff,
            InventoryTransactionTypeEnum.Return,
            savedEntity.Code,
            detail.WarehouseLocationId,
            $"採購退貨增量 - {savedEntity.Code}"
        );
    }
    else
    {
        // 退貨數量減少 → 增加庫存 (撤銷部分退貨)
        await _inventoryStockService.AddStockAsync(
            detail.ProductId,
            warehouseId,
            Math.Abs(quantityDiff),
            InventoryTransactionTypeEnum.Return,
            savedEntity.Code,
            detail.OriginalUnitPrice,
            detail.WarehouseLocationId,
            $"採購退貨撤銷 - {savedEntity.Code}"
        );
    }
}
```

#### 範例說明

```
前提: 進貨單 PR0001 已入庫商品A x 100個

操作1: 新增退貨單 PRR0001，退貨 30個
→ ReduceStockAsync(商品A, W1, 30, Return, "PRR0001")
→ InventoryTransaction: PRR0001, Quantity = -30
→ CurrentStock: 100 → 70

操作2: 修改退貨單 PRR0001，改為退貨 50個
→ quantityDiff = 50 - 30 = 20
→ ReduceStockAsync(商品A, W1, 20, Return, "PRR0001")
→ InventoryTransaction: 新增 PRR0001, Quantity = -20
→ CurrentStock: 70 → 50

操作3: 修改退貨單 PRR0001，改為退貨 20個
→ quantityDiff = 20 - 50 = -30
→ AddStockAsync(商品A, W1, 30, Return, "PRR0001")
→ InventoryTransaction: 新增 PRR0001, Quantity = 30
→ CurrentStock: 50 → 80
```

### 4.4 銷貨退回流程 (SalesReturn)

#### 使用者操作

```
1. 從出貨單點擊「轉退貨」或新增退貨單
2. 選擇要退貨的出貨明細
3. 輸入退貨數量
4. 點擊「儲存」
```

#### 背後處理流程

```csharp
// 位於: SalesReturnEditModalComponent.razor
// SaveHandler 自訂儲存處理器

private async Task<bool> SaveSalesReturnWithDetails(SalesReturn salesReturn)
{
    // 1. 儲存主檔和明細 (使用 SaveWithDetailsAsync)
    var result = await SalesReturnService.SaveWithDetailsAsync(salesReturn, salesReturnDetails);
    
    if (!result.IsSuccess)
        return false;
    
    // 2. 使用差異計算更新庫存 (重點在這裡！)
    await UpdateInventoryByDifferenceAsync(result.Data.Id);
    
    return true;
}

private async Task UpdateInventoryByDifferenceAsync(int salesReturnId)
{
    var inventoryUpdateResult = await SalesReturnService
        .UpdateInventoryByDifferenceAsync(salesReturnId);
    
    if (!inventoryUpdateResult.IsSuccess)
    {
        throw new Exception($"庫存更新失敗：{inventoryUpdateResult.ErrorMessage}");
    }
}
```

#### 庫存異動詳細步驟 (UpdateInventoryByDifferenceAsync)

**關鍵差異**: 銷貨退回是「入庫」操作，與銷貨出貨相反

```csharp
// 位於: SalesReturnService.cs

public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0)
{
    // 步驟1: 查詢所有相關的庫存交易記錄
    var existingTransactions = await context.InventoryTransactions
        .Where(t => (t.TransactionNumber == currentReturn.Code ||
                   t.TransactionNumber.StartsWith(currentReturn.Code + "_ADJ"))
                   && !t.TransactionNumber.EndsWith("_DEL"))
        .ToListAsync();
    
    // 步驟2: 計算已處理的庫存淨值（退貨記錄為正數）
    var processedInventory = new Dictionary<string, int>();
    foreach (var trans in existingTransactions)
    {
        var key = $"{trans.ProductId}_{trans.WarehouseId}_{trans.WarehouseLocationId}";
        processedInventory[key] = processedInventory.GetValueOrDefault(key) + trans.Quantity;
        // 注意：退貨的 Quantity 是正數（增加庫存）
    }
    
    // 步驟3: 計算當前明細的目標數量（以正數表示入庫）
    var currentInventory = new Dictionary<string, int>();
    foreach (var detail in currentReturn.SalesReturnDetails)
    {
        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId}";
        currentInventory[key] = currentInventory.GetValueOrDefault(key) + detail.ReturnQuantity;
        // 退貨數量保持正數
    }
    
    // 步驟4: 計算差異並執行庫存調整
    foreach (var key in allKeys)
    {
        int targetQuantity = currentInventory.GetValueOrDefault(key);  // 正數（入庫）
        int processedQuantity = processedInventory.GetValueOrDefault(key);
        int adjustmentNeeded = targetQuantity - processedQuantity;
        
        if (adjustmentNeeded > 0)
        {
            // 退貨數量增加，需要增加更多庫存
            await _inventoryStockService.AddStockAsync(
                productId, warehouseId, adjustmentNeeded,
                InventoryTransactionTypeEnum.Return,
                $"{code}_ADJ",
                null,  // 退貨不需要成本
                locationId,
                $"銷貨退回編輯調增 - {code}"
            );
        }
        else if (adjustmentNeeded < 0)
        {
            // 退貨數量減少，需要扣減庫存（撤銷部分退貨）
            await _inventoryStockService.ReduceStockAsync(
                productId, warehouseId, Math.Abs(adjustmentNeeded),
                InventoryTransactionTypeEnum.Return,
                $"{code}_ADJ",
                locationId,
                $"銷貨退回編輯調減 - {code}"
            );
        }
    }
}
```

#### 範例說明

**場景1: 新增銷貨退回單**

```
前提: 出貨單 SD0001 已出貨商品A x 100個（庫存已扣除100）

操作: 新增退貨單 SR0001，退貨 30個

處理過程:
1. existingTransactions = [] (沒有歷史記錄)
2. processedInventory = {}
3. currentInventory = { "A_W1_null": 30 } → 目標: 30 (入庫)
4. adjustmentNeeded = 30 - 0 = 30
5. 執行: AddStockAsync(商品A, W1, 30, Return, "SR0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 原庫存 + 30
- InventoryTransaction: SR0001_ADJ, Quantity = 30 (正數表示入庫)
```

**場景2: 修改銷貨退回單 - 增加退貨數量**

```
操作: 編輯退貨單 SR0001，將數量從 30 改為 50

處理過程:
1. existingTransactions = [SR0001_ADJ, Qty=30]
2. processedInventory = { "A_W1_null": 30 }
3. currentInventory = { "A_W1_null": 50 } → 目標: 50
4. adjustmentNeeded = 50 - 30 = 20
5. 執行: AddStockAsync(商品A, W1, 20, Return, "SR0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 原庫存 + 50
- InventoryTransaction: 新增一筆 SR0001_ADJ, Quantity = 20
```

**場景3: 修改銷貨退回單 - 減少退貨數量**

```
操作: 編輯退貨單 SR0001，將數量從 50 改為 20

處理過程:
1. existingTransactions = [SR0001_ADJ(30), SR0001_ADJ(20)]
2. processedInventory = { "A_W1_null": 50 }
3. currentInventory = { "A_W1_null": 20 } → 目標: 20
4. adjustmentNeeded = 20 - 50 = -30
5. 執行: ReduceStockAsync(商品A, W1, 30, Return, "SR0001_ADJ")

結果:
- InventoryStockDetail: CurrentStock = 原庫存 + 20 (扣減了30)
- InventoryTransaction: 新增一筆 SR0001_ADJ, Quantity = -30 (負數表示撤銷退貨)
```

### 4.5 刪除進貨單流程

```csharp
// 位於: PurchaseReceivingService.cs - DeleteAsync / PermanentDeleteAsync

public override async Task<ServiceResult> DeleteAsync(int id)
{
    // 1. 取得進貨單及明細
    var purchaseReceiving = await GetByIdAsync(id);
    
    // 2. 對每個已入庫的明細進行庫存回退
    foreach (var detail in purchaseReceiving.PurchaseReceivingDetails)
    {
        if (detail.ReceivedQuantity > 0)
        {
            await _inventoryStockService.ReduceStockAsync(
                detail.ProductId,
                detail.WarehouseId,
                detail.ReceivedQuantity,
                InventoryTransactionTypeEnum.Return,
                $"{purchaseReceiving.Code}_DEL",  // 使用 _DEL 後綴
                detail.WarehouseLocationId,
                $"刪除採購進貨單 - {purchaseReceiving.Code}"
            );
        }
    }
    
    // 3. 執行軟刪除 (設定 IsDeleted = true)
    // EF 會自動級聯刪除明細
}
```

**重要**: 刪除使用 `_DEL` 後綴，與編輯時的 `_ADJ` 後綴區分

---

## 5. 庫存異動規則與約定

### 5.1 交易單號命名規範

| 場景 | 交易單號格式 | 範例 | 說明 |
|------|-------------|------|------|
| **首次新增** | `{Code}` | `PR0001` | **原始單號**，不帶任何後綴 |
| **編輯調整** | `{Code}_ADJ` | `PR0001_ADJ` | Adjustment（調整），可重複使用，系統會自動計算淨值 |
| **刪除回退** | `{Code}_DEL` | `PR0001_DEL` | Delete（刪除），刪除時同時清除所有 _ADJ 記錄 |
| **退貨** | `{Code}` | `PRR0001` | 直接使用退貨單號（首次新增） |
| **調撥** | `{Code}_OUT` / `{Code}_IN` | `TF0001_OUT` | Transfer |

**🔑 關鍵設計原則**：
- ✅ **新增階段**：使用 `ConfirmXxxAsync`，TransactionNumber = **原始Code**
- ✅ **編輯階段**：使用 `UpdateInventoryByDifferenceAsync`，TransactionNumber = **Code_ADJ**
- ✅ **刪除階段**：TransactionNumber = **Code_DEL** + **清除所有 _ADJ 記錄**

### 5.2 Quantity 正負號約定

| 交易類型 | Quantity 符號 | 說明 | API 方法 |
|---------|--------------|------|---------|
| Purchase (採購入庫) | **正數** (+) | 增加庫存 | `AddStockAsync` |
| Sales (銷貨出庫) | **負數** (-) | 減少庫存 | `ReduceStockAsync` |
| Return (退貨) | 視情況 | 採購退出為負，銷貨退回為正 | 採購用 `ReduceStockAsync`，銷貨用 `AddStockAsync` |
| Transfer (調撥) | 出庫為負，入庫為正 | 會產生兩筆記錄 | `TransferStockAsync` |
| Adjustment (調整) | 視調整方向 | 增加為正，減少為負 | 視方向選擇 API |
| Production (生產入庫) | **正數** (+) | 增加成品庫存 | `AddStockAsync` |
| MaterialIssue (領料出庫) | **負數** (-) | 減少原料庫存 | `ReduceStockAsync` |

### 5.3 資料庫交易 (Transaction) 使用規範

**必須使用交易的場景**:
1. ✅ 主檔 + 明細同時儲存
2. ✅ 庫存變動 + 單據更新
3. ✅ 批次庫存調整
4. ✅ 刪除含庫存回退

**範例**:
```csharp
using var context = await _contextFactory.CreateDbContextAsync();
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    // 執行多個操作
    await SaveMainRecord();
    await UpdateInventory();
    await SaveDetails();
    
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 5.4 錯誤處理規範

**庫存不足錯誤**:
```csharp
if (detail.AvailableStock < quantity)
{
    return ServiceResult.Failure(
        $"商品 {product.Name} 庫存不足，" +
        $"可用庫存：{detail.AvailableStock}，" +
        $"需求數量：{quantity}"
    );
}
```

**交易失敗回滾**:
- 任何步驟失敗，必須 `RollbackAsync()`
- 返回明確的錯誤訊息給使用者
- 記錄錯誤日誌 (使用 `ErrorHandlingHelper`)

---

## 6. 實際案例分析

### 案例1: 進貨單數量調整導致重複累加 (已修復)

**問題描述**:
```
PR0001 首次儲存: 商品A x 100
→ 庫存: 100 ✅

編輯為 150:
→ 錯誤: 直接 AddStockAsync(150) → 庫存變成 250 ❌
→ 正確: 計算差異 50，AddStockAsync(50) → 庫存變成 150 ✅
```

**解決方案**: 使用 `UpdateInventoryByDifferenceAsync` 淨值計算法

### 案例2: 商品替換未正確處理庫存 (已修復)

**問題描述**:
```
PR0001 原商品: 商品A x 100
編輯為: 商品B x 100

錯誤做法:
- 只增加商品B的庫存，忘記減少商品A ❌

正確做法:
- 商品A: 目標0，已處理100 → ReduceStockAsync(100)
- 商品B: 目標100，已處理0 → AddStockAsync(100)
```

### 案例3: 刪除單據後庫存未回退

**問題原因**: 刪除方法只做軟刪除，未處理庫存

**正確做法**:
```csharp
public override async Task<ServiceResult> DeleteAsync(int id)
{
    // 1. 先回退庫存
    foreach (var detail in details)
    {
        await ReduceStockAsync(..., $"{code}_DEL");
    }
    
    // 2. 再執行軟刪除
    entity.IsDeleted = true;
}
```

### 案例4: 錯誤的設計模式 - 新增時就使用 _ADJ 後綴 (已修復)

**早期錯誤設計**（2025/12/11 前）:
```
SalesDeliveryService, SalesReturnService, PurchaseReturnService 的舊設計：
├─ 新增：直接調用 UpdateInventoryByDifferenceAsync → "SD001_ADJ" ❌
├─ 編輯：UpdateInventoryByDifferenceAsync → "SD001_ADJ"
└─ 刪除：PermanentDeleteAsync → "SD001_DEL" + 清除所有 _ADJ

問題：
1. 所有記錄都是 _ADJ，失去了「原始新增」的區分
2. 審計追蹤不完整，無法看出哪筆是首次新增
3. 刪除後重新新增仍用 _ADJ，與編輯無法區分
```

**正確的設計模式**（參考 PurchaseReceivingService）:
```
完整的三階段設計：
├─ 新增：ConfirmXxxAsync → "PR001" (原始Code，不帶後綴) ✅
├─ 編輯：UpdateInventoryByDifferenceAsync → "PR001_ADJ" ✅
└─ 刪除：PermanentDeleteAsync → "PR001_DEL" + 清除所有 _ADJ ✅

優點：
1. ✅ 完整保留審計追蹤
2. ✅ 清楚區分「新增」、「編輯」、「刪除」三個階段
3. ✅ 刪除後重新新增沒有問題（因為 _ADJ 已清除）
```

**實際測試記錄對比**:

**正確做法（PurchaseReceivingService）**:
```
操作: 新增入庫 > 刪除入庫 > 新增入庫 > 新增退貨

記錄:
1. PR001 +40        (新增，原始Code) ✅
2. PR001_DEL -40    (刪除)
3. PR001 +40        (重新新增，原始Code) ✅
4. PRR001 -20       (退貨)

✅ 可以清楚看出「新增」vs「編輯」的差異
✅ 完整的審計追蹤
```

**錯誤做法（舊版其他服務）**:
```
操作: 新增出貨 > 刪除出貨 > 新增出貨 > 新增退貨

記錄:
1. SD001_ADJ -40    (新增就用_ADJ❌)
2. SD001_DEL +40    (刪除)
3. SD001_ADJ -40    (重新新增仍用_ADJ❌)
4. SR001_ADJ +20    (退貨也用_ADJ❌)

❌ 所有記錄都是 _ADJ，無法區分首次新增
❌ 失去審計追蹤的意義
```

**修復方案**（2025/12/11 實施）:

所有服務新增獨立的 `ConfirmXxxAsync` 方法：

```csharp
// SalesDeliveryService - 新增
public async Task<ServiceResult> ConfirmDeliveryAsync(int id, int confirmedBy = 0)
{
    // 使用原始單號，不帶 _ADJ
    await _inventoryStockService.ReduceStockAsync(..., 
        salesDelivery.Code,  // ← 原始Code
        ...);
}

// SalesReturnService - 新增  
public async Task<ServiceResult> ConfirmReturnAsync(int id, int confirmedBy = 0)
{
    // 使用原始單號，不帶 _ADJ
    await _inventoryStockService.AddStockAsync(...,
        salesReturn.Code,  // ← 原始Code
        ...);
}

// PurchaseReturnService - 新增
public async Task<ServiceResult> ConfirmReturnAsync(int id, int confirmedBy = 0)
{
    // 使用原始單號，不帶 _ADJ
    await _inventoryStockService.ReduceStockAsync(...,
        purchaseReturn.Code,  // ← 原始Code
        ...);
}
```

**UI 層調整**（需配合實施）:

```csharp
// 編輯視窗的 AfterSave 處理
private async Task SaveDetailsAsync()
{
    // 判斷是新增還是編輯模式
    bool isEditMode = EntityId.HasValue && EntityId.Value > 0;
    
    if (isEditMode)
    {
        // 編輯模式：使用差異比較更新庫存
        await Service.UpdateInventoryByDifferenceAsync(savedId);
    }
    else
    {
        // 新增模式：使用確認流程，創建原始記錄
        // 檢查是否已經有庫存交易記錄，避免重複確認
        var hasExistingTransactions = await HasExistingInventoryTransactions(code);
        
        if (!hasExistingTransactions)
        {
            await Service.ConfirmXxxAsync(savedId);
        }
    }
}
```

**修復後的服務**:
- ✅ `PurchaseReceivingService.cs` - ConfirmReceiptAsync（原本就正確）
- ✅ `SalesDeliveryService.cs` - ConfirmDeliveryAsync（新增）
- ✅ `SalesReturnService.cs` - ConfirmReturnAsync（新增）
- ✅ `PurchaseReturnService.cs` - ConfirmReturnAsync（新增）

**關鍵設計原則**:
```
三階段完整流程：

階段1 - 新增:
TransactionNumber: {Code}（原始單號）
方法: ConfirmXxxAsync
目的: 記錄首次新增的原始資料

階段2 - 編輯:
TransactionNumber: {Code}_ADJ
方法: UpdateInventoryByDifferenceAsync  
目的: 記錄編輯產生的調整

階段3 - 刪除:
TransactionNumber: {Code}_DEL
方法: PermanentDeleteAsync
目的: 記錄刪除操作 + 清除所有 _ADJ

優勢:
✅ 完整的審計追蹤（可追溯每筆記錄的來源）
✅ 清楚區分新增/編輯/刪除三個階段
✅ 支援每日序號編碼（刪除後可重新使用）
✅ 防止誤判（新增不會被當成編輯）
```

---

## 7. 常見錯誤與解決方案

### 錯誤1: 直接操作 InventoryStock 表

❌ **錯誤做法**:
```csharp
var stock = await context.InventoryStocks.FirstAsync(...);
stock.CurrentStock += quantity;
await context.SaveChangesAsync();
```

✅ **正確做法**:
```csharp
await _inventoryStockService.AddStockAsync(
    productId, warehouseId, quantity,
    InventoryTransactionTypeEnum.Purchase,
    transactionNumber
);
```

### 錯誤2: 未使用資料庫交易

❌ **錯誤做法**:
```csharp
await SaveMainRecord();
await UpdateInventory();  // 如果這裡失敗，主檔已儲存 ❌
```

✅ **正確做法**:
```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    await SaveMainRecord();
    await UpdateInventory();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 錯誤3: 重複計算庫存

❌ **錯誤做法** (每次編輯都加全部數量):
```csharp
foreach (var detail in details)
{
    await AddStockAsync(detail.Quantity);  // 會重複累加 ❌
}
```

✅ **正確做法** (計算差異):
```csharp
int targetQuantity = currentDetails.Sum(d => d.Quantity);
int processedQuantity = GetProcessedQuantityFromTransactions();
int adjustmentNeeded = targetQuantity - processedQuantity;

if (adjustmentNeeded > 0)
    await AddStockAsync(adjustmentNeeded);
else if (adjustmentNeeded < 0)
    await ReduceStockAsync(Math.Abs(adjustmentNeeded));
```

### 錯誤4: 未檢查庫存是否足夠

❌ **錯誤做法**:
```csharp
await _inventoryStockService.ReduceStockAsync(...);
// 沒有檢查返回結果 ❌
```

✅ **正確做法**:
```csharp
var result = await _inventoryStockService.ReduceStockAsync(...);
if (!result.IsSuccess)
{
    await transaction.RollbackAsync();
    return ServiceResult.Failure($"庫存扣減失敗：{result.ErrorMessage}");
}
```

---

## 8. 開發新功能指南

### 8.1 需要操作庫存的新功能開發流程

**步驟1: 分析業務邏輯**
- 這個功能是入庫還是出庫？
- 需要使用哪個 `InventoryTransactionTypeEnum`？
- 是否涉及多個倉庫？

**步驟2: 注入 InventoryStockService**
```csharp
public class YourNewService : GenericManagementService<YourEntity>
{
    private readonly IInventoryStockService _inventoryStockService;
    
    public YourNewService(
        IDbContextFactory<AppDbContext> contextFactory,
        IInventoryStockService inventoryStockService)
        : base(contextFactory)
    {
        _inventoryStockService = inventoryStockService;
    }
}
```

**步驟3: 在適當時機呼叫庫存 API**

**新增時**:
```csharp
public override async Task<ServiceResult<YourEntity>> CreateAsync(YourEntity entity)
{
    using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        // 1. 儲存主檔
        var result = await base.CreateAsync(entity);
        
        // 2. 更新庫存
        foreach (var detail in entity.Details)
        {
            var stockResult = await _inventoryStockService.AddStockAsync(
                detail.ProductId,
                detail.WarehouseId,
                detail.Quantity,
                InventoryTransactionTypeEnum.YourType,
                entity.Code,
                detail.UnitPrice
            );
            
            if (!stockResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return ServiceResult<YourEntity>.Failure(stockResult.ErrorMessage);
            }
        }
        
        await transaction.CommitAsync();
        return result;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

**編輯時** (參考 `PurchaseReceivingService.UpdateInventoryByDifferenceAsync`):
```csharp
public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id)
{
    // 1. 查詢現有交易記錄
    var existingTransactions = await context.InventoryTransactions
        .Where(t => t.TransactionNumber.StartsWith(entity.Code))
        .ToListAsync();
    
    // 2. 計算已處理數量
    var processedQty = existingTransactions.Sum(t => t.Quantity);
    
    // 3. 計算目標數量
    var targetQty = entity.Details.Sum(d => d.Quantity);
    
    // 4. 調整差異
    var diff = targetQty - processedQty;
    if (diff > 0)
        await _inventoryStockService.AddStockAsync(...);
    else if (diff < 0)
        await _inventoryStockService.ReduceStockAsync(...);
}
```

**刪除時**:
```csharp
public override async Task<ServiceResult> DeleteAsync(int id)
{
    using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        // 1. 取得實體
        var entity = await GetByIdAsync(id);
        
        // 2. 回退庫存
        foreach (var detail in entity.Details)
        {
            await _inventoryStockService.ReduceStockAsync(
                detail.ProductId,
                detail.WarehouseId,
                detail.Quantity,
                InventoryTransactionTypeEnum.YourType,
                $"{entity.Code}_DEL"
            );
        }
        
        // 3. 執行刪除
        await base.DeleteAsync(id);
        
        await transaction.CommitAsync();
        return ServiceResult.Success();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### 8.2 新增自訂 InventoryTransactionTypeEnum

如果現有的交易類型不符合需求：

1. 在 `InventoryTransactionTypeEnum` 新增項目
2. 更新相關顯示名稱字典
3. 在報表和統計查詢中處理新類型

```csharp
public enum InventoryTransactionTypeEnum
{
    // ...現有類型
    YourNewType = 8  // 新增類型
}
```

### 8.3 測試檢查清單

開發完成後，務必測試以下場景：

**基本功能測試**：
- [ ] 新增功能: 
  - [ ] 庫存正確增加/減少
  - [ ] TransactionNumber 使用**原始Code**（不帶 _ADJ）
  - [ ] 使用 `ConfirmXxxAsync` 方法
- [ ] 編輯功能: 
  - [ ] 增加數量 → 庫存相應增加
  - [ ] 減少數量 → 庫存相應減少
  - [ ] 更換商品 → 舊商品減、新商品增
  - [ ] 更換倉庫 → 舊倉庫減、新倉庫增
  - [ ] TransactionNumber 使用 **Code_ADJ**
  - [ ] 使用 `UpdateInventoryByDifferenceAsync` 方法
- [ ] 刪除功能: 
  - [ ] 庫存正確回退
  - [ ] 創建 **Code_DEL** 記錄
  - [ ] **清除所有 _ADJ 記錄**
  - [ ] 使用 `PermanentDeleteAsync` 方法

**審計追蹤測試**（關鍵）：
- [ ] 新增 → 編輯 → 刪除完整流程:
  - [ ] 第1筆: {Code} (新增)
  - [ ] 第2筆: {Code}_ADJ (編輯調整)
  - [ ] 第3筆: {Code}_DEL (刪除回退)
- [ ] 刪除後重新新增:
  - [ ] 刪除後 _ADJ 記錄被清除
  - [ ] 重新新增使用原始 {Code}，不是 _ADJ
  - [ ] 不會被誤判為編輯

**進階測試**：
- [ ] 庫存不足: 顯示正確錯誤訊息
- [ ] 交易記錄: `InventoryTransaction` 正確記錄所有異動
- [ ] 批號追蹤: 批號資訊正確傳遞和記錄 (如適用)
- [ ] 每日序號: 刪除後可重新使用相同序號

---

## 9. 總結

### 核心原則

1. **單一職責**: InventoryStockService 是庫存操作的唯一入口
2. **淨值計算**: 編輯時計算差異，避免重複累加
3. **完整記錄**: 所有異動必須記錄在 InventoryTransaction
4. **交易安全**: 使用資料庫交易確保資料一致性
5. **清晰命名**: 使用 `_ADJ`、`_DEL` 等後綴區分操作類型

### 快速參考表

#### 主要業務操作對比

| 業務場景 | 庫存效果 | 目標數量符號 | 使用方法 | Transaction Type | Quantity 符號 |
|---------|---------|------------|----------|-----------------|--------------|
| **採購入庫** (Purchase) | 增加庫存 | 正數 (+100) | `AddStockAsync` | Purchase | 正數 (+100) |
| **銷貨出貨** (Sales) | 減少庫存 | 負數 (-100) | `ReduceStockAsync` | Sale | 負數 (-100) |
| **採購退貨** (PurchaseReturn) | 減少庫存 | 負數 (-30) | `ReduceStockAsync` | Return | 負數 (-30) |
| **銷貨退回** (SalesReturn) | 增加庫存 | 正數 (+30) | `AddStockAsync` | Return | 正數 (+30) |

**記憶口訣**：
- 採購 & 銷貨退回 → 貨物進來 → **增加庫存** → AddStockAsync → 正數
- 銷貨 & 採購退貨 → 貨物出去 → **減少庫存** → ReduceStockAsync → 負數

#### 所有業務場景

| 業務場景 | 操作 | 使用方法 | 範例 |
|---------|------|---------|------|
| 採購入庫 | 增加庫存 | `AddStockAsync` | 進貨單儲存 |
| 銷貨出庫 | 減少庫存 | `ReduceStockAsync` | 出貨單儲存 |
| 採購退貨 | 減少庫存 | `ReduceStockAsync` | 退還給廠商 |
| 銷貨退回 | 增加庫存 | `AddStockAsync` | 客戶退回商品 |
| 刪除進貨單 | 減少庫存 | `ReduceStockAsync` | 回退入庫數量 |
| 刪除出貨單 | 增加庫存 | `AddStockAsync` | 回補出庫數量 |
| 庫存調撥 | 一減一增 | `TransferStockAsync` | 倉庫間移動 |
| 編輯單據 | 差異調整 | `UpdateInventoryByDifferenceAsync` | 修改數量/商品 |

### 開發建議

1. 複製現有類似功能的程式碼作為範本
2. 重點關注交易單號的命名規則
3. 務必使用資料庫交易
4. 充分測試各種編輯場景
5. 檢查 InventoryTransaction 記錄是否正確

---

**文檔版本**: 1.0  
**最後更新**: 2025-12-11  
**維護者**: ERPCore2 開發團隊
