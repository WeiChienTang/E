# 倉庫特定庫存驗證與 FIFO 批次管理系統

## 📝 修改概述

本次修改實現了銷貨訂單的倉庫特定庫存驗證以及基於批次號的 FIFO（先進先出）庫存管理系統。

**修改日期**: 2025年9月18日  
**修改原因**: 優化銷貨訂單庫存管理，從檢查所有倉庫改為特定倉庫檢查，並實現 FIFO 批次庫存扣減機制。

---

## 🎯 核心需求

1. **倉庫選擇驗證**: 儲存銷貨訂單時必須選擇倉庫，無選擇則無法儲存
2. **特定倉庫庫存檢查**: 只檢查選定倉庫的商品庫存，不再檢查所有倉庫
3. **FIFO 批次管理**: 透過 `PurchaseReceivingDetail.BatchNumber` 實現先進先出的庫存扣減

---

## 🔧 技術實現

### 1. 資料庫結構修改

#### InventoryStock 實體擴展
```csharp
// 新增欄位
[MaxLength(50)]
[Display(Name = "批次號碼")]
public string? BatchNumber { get; set; }

[Display(Name = "批次日期")]
public DateTime? BatchDate { get; set; }

[Display(Name = "到期日期")]  
public DateTime? ExpiryDate { get; set; }
```

#### 資料庫遷移
- **遷移檔案**: `20250918031336_AddBatchFieldsToInventoryStock.cs`
- **新增索引**: `IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId_BatchNumber`

### 2. 服務層修改

#### SalesOrderService 庫存驗證邏輯
```csharp
// 原方法: ValidateInventoryStockAsync (檢查所有倉庫)
// 新方法: ValidateWarehouseInventoryStockAsync (檢查特定倉庫)

public async Task<ServiceResult> ValidateWarehouseInventoryStockAsync(List<SalesOrderDetail> salesOrderDetails)
{
    // 1. 檢查倉庫選擇
    var itemsWithoutWarehouse = salesOrderDetails
        .Where(d => d.ProductId > 0 && d.OrderQuantity > 0 && !d.WarehouseId.HasValue)
        .ToList();
    
    if (itemsWithoutWarehouse.Any())
    {
        return ServiceResult.Failure("請為所有產品選擇倉庫");
    }
    
    // 2. 按倉庫分組檢查庫存
    var warehouseGroups = salesOrderDetails
        .Where(d => d.ProductId > 0 && d.OrderQuantity > 0 && d.WarehouseId.HasValue)
        .GroupBy(d => d.WarehouseId!.Value);
        
    // 3. 特定倉庫庫存檢查邏輯...
}
```

#### InventoryStockService FIFO 實現
```csharp
// 新增 FIFO 扣減方法
public async Task<ServiceResult> ReduceStockWithFIFOAsync(
    int productId,
    int warehouseId, 
    int quantity,
    InventoryTransactionTypeEnum transactionType,
    string referenceNumber,
    int? warehouseLocationId = null,
    string? remark = null)
{
    // 1. 按批次日期排序 (FIFO)
    var availableStocks = await _context.InventoryStocks
        .Where(s => s.ProductId == productId 
                 && s.WarehouseId == warehouseId 
                 && s.CurrentStock > 0)
        .OrderBy(s => s.BatchDate ?? DateTime.MinValue)  // FIFO 排序
        .ToListAsync();
    
    // 2. 批次扣減邏輯
    var reductionDetails = new List<BatchReductionDetail>();
    int remainingQuantity = quantity;
    
    foreach (var stock in availableStocks)
    {
        if (remainingQuantity <= 0) break;
        
        int reduceFromThisBatch = Math.Min(remainingQuantity, stock.CurrentStock);
        // 扣減處理...
    }
}

// 擴展 AddStockAsync 支援批次資訊  
public async Task<ServiceResult> AddStockAsync(
    int productId,
    int warehouseId,
    int quantity,
    InventoryTransactionTypeEnum transactionType,
    string referenceNumber,
    int? warehouseLocationId = null,
    string? remark = null,
    string? batchNumber = null,      // 新增
    DateTime? batchDate = null,      // 新增
    DateTime? expiryDate = null)     // 新增
```

#### PurchaseReceivingService 批次整合
```csharp
// ConfirmReceiptAsync 修改為傳遞批次資訊
await _inventoryStockService.AddStockAsync(
    detail.ProductId,
    detail.WarehouseId ?? purchaseReceiving.WarehouseId!.Value,
    receivedQuantity,
    InventoryTransactionTypeEnum.PurchaseReceive,
    purchaseReceiving.ReceivingNumber ?? $"PR-{purchaseReceiving.Id}",
    detail.WarehouseLocationId,
    $"採購入庫 - {purchaseReceiving.ReceivingNumber}",
    detail.BatchNumber,           // 傳遞批次號
    purchaseReceiving.BatchDate,  // 傳遞批次日期  
    detail.ExpiryDate            // 傳遞到期日期
);
```

### 3. 前端元件修改

#### SalesOrderEditModalComponent 流程整合
```csharp
// SaveSalesOrderWithDetails 方法流程
private async Task<bool> SaveSalesOrderWithDetails()
{
    // 1. 驗證基本資料
    // 2. 倉庫特定庫存驗證 (新)
    var validationResult = await SalesOrderService.ValidateWarehouseInventoryStockAsync(validDetails);
    if (!validationResult.IsSuccess)
    {
        await NotificationService.ShowErrorAsync(validationResult.ErrorMessage);
        return false;
    }
    
    // 3. 儲存訂單
    var result = await SalesOrderService.SaveAsync(salesOrder);
    
    // 4. 儲存明細  
    await SaveSalesOrderDetails(result.Data.Id);
    
    // 5. FIFO 庫存扣減 (新)
    await ReduceInventoryWithFIFOAsync(result.Data.Id);
}

// 新增 FIFO 庫存扣減方法
private async Task ReduceInventoryWithFIFOAsync(int salesOrderId)
{
    var validDetails = salesOrderDetails
        .Where(d => d.ProductId > 0 && d.OrderQuantity > 0 && d.WarehouseId.HasValue)
        .ToList();

    foreach (var detail in validDetails)
    {
        var result = await InventoryStockService.ReduceStockWithFIFOAsync(
            detail.ProductId,
            detail.WarehouseId.Value,
            (int)Math.Ceiling(detail.OrderQuantity),
            InventoryTransactionTypeEnum.Sale,
            $"SO-{salesOrderId}",
            null,
            $"銷貨訂單出貨 - SO-{salesOrderId}"
        );
        // 錯誤處理...
    }
}
```

---

## 📋 測試檢查清單

### ✅ 基礎功能測試

1. **資料庫結構確認**
   ```sql
   -- 檢查 InventoryStock 表是否有新欄位
   SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
   FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'InventoryStocks' 
   AND COLUMN_NAME IN ('BatchNumber', 'BatchDate', 'ExpiryDate');
   
   -- 檢查新索引是否存在
   SELECT name FROM sys.indexes 
   WHERE object_id = OBJECT_ID('InventoryStocks') 
   AND name = 'IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId_BatchNumber';
   ```

2. **編譯檢查**
   ```bash
   cd ERPCore2
   dotnet build
   # 應該要成功編譯，無錯誤
   ```

### ✅ 採購流程測試

1. **建立採購單**
   - 選擇供應商和商品
   - 填入批次號碼 (BatchNumber)
   - 設定批次日期

2. **確認採購入庫**
   - 執行 "確認收貨"
   - 檢查 InventoryStock 是否正確記錄批次資訊
   ```sql
   SELECT ProductId, WarehouseId, BatchNumber, BatchDate, CurrentStock
   FROM InventoryStocks 
   WHERE BatchNumber IS NOT NULL
   ORDER BY BatchDate;
   ```

### ✅ 銷貨訂單測試

1. **倉庫選擇驗證測試**
   - 建立銷貨訂單，加入商品但不選倉庫
   - 嘗試儲存 → 應顯示錯誤訊息：「請為所有產品選擇倉庫」

2. **特定倉庫庫存檢查測試**
   ```text
   測試情境：
   - 倉庫A：商品X 有庫存 100
   - 倉庫B：商品X 無庫存 
   - 建立銷貨訂單：商品X 數量 50，選擇倉庫B
   - 預期：顯示庫存不足錯誤（只檢查倉庫B，不考慮倉庫A的庫存）
   ```

3. **FIFO 扣減測試**
   ```text
   準備數據：
   - 商品A，倉庫1
   - 批次1：數量 30，批次日期 2025-01-01  
   - 批次2：數量 50，批次日期 2025-01-15
   - 批次3：數量 20，批次日期 2025-01-10
   
   測試銷貨：數量 60
   預期扣減順序：
   1. 批次1 (2025-01-01) 扣減 30 → 剩餘 0
   2. 批次3 (2025-01-10) 扣減 20 → 剩餘 0  
   3. 批次2 (2025-01-15) 扣減 10 → 剩餘 40
   ```

### ✅ 資料驗證

4. **庫存交易記錄檢查**
   ```sql
   -- 檢查 FIFO 扣減是否正確記錄
   SELECT it.*, s.BatchNumber, s.BatchDate
   FROM InventoryTransactions it
   LEFT JOIN InventoryStocks s ON it.ProductId = s.ProductId 
       AND it.WarehouseId = s.WarehouseId
   WHERE it.TransactionType = 2  -- Sale
   ORDER BY it.TransactionDate DESC;
   ```

5. **庫存餘額驗證**
   ```sql
   -- 驗證庫存餘額正確性
   SELECT ProductId, WarehouseId, BatchNumber, BatchDate, 
          CurrentStock, ExpiryDate
   FROM InventoryStocks 
   WHERE CurrentStock > 0
   ORDER BY ProductId, WarehouseId, BatchDate;
   ```

---

## 🐛 常見問題排除

### 問題 1: 編譯錯誤 - InventoryStockService 找不到
**原因**: 未注入 `IInventoryStockService`  
**解決**: 確認 `SalesOrderEditModalComponent.razor` 頂部有 `@inject IInventoryStockService InventoryStockService`

### 問題 2: 資料庫遷移失敗
**原因**: 現有資料與新索引衝突  
**解決**: 
```sql
-- 清理重複資料後重新執行遷移
DELETE FROM InventoryStocks 
WHERE Id NOT IN (
    SELECT MIN(Id) 
    FROM InventoryStocks 
    GROUP BY ProductId, WarehouseId, WarehouseLocationId
);
```

### 問題 3: FIFO 順序不正確
**原因**: BatchDate 為 NULL 的處理  
**解決**: 檢查 `OrderBy(s => s.BatchDate ?? DateTime.MinValue)` 邏輯

### 問題 4: 倉庫驗證不生效
**原因**: 前端元件使用舊方法名稱  
**解決**: 確認使用 `ValidateWarehouseInventoryStockAsync` 而非 `ValidateInventoryStockAsync`

---

## 📊 效能考量

1. **索引優化**: 新增的複合索引支援 FIFO 查詢效能
2. **批次查詢**: 一次查詢所有可用批次，避免 N+1 問題  
3. **交易管理**: 庫存扣減使用資料庫交易確保一致性

---

## 🔄 後續優化建議

1. **批次過期提醒**: 實現即將到期批次的警告機制
2. **庫存報表**: 新增按批次的庫存明細報表
3. **批次追蹤**: 實現完整的批次流向追蹤功能
4. **自動批次號**: 實現自動批次號產生規則

---

## 📁 相關檔案清單

### 實體層
- `Data/Entities/InventoryStock.cs` - 庫存實體，新增批次欄位
- `Migrations/20250918031336_AddBatchFieldsToInventoryStock.cs` - 資料庫遷移

### 服務層  
- `Services/Inventory/IInventoryStockService.cs` - 庫存服務介面
- `Services/Inventory/InventoryStockService.cs` - 庫存服務實現
- `Services/Sales/ISalesOrderService.cs` - 銷貨服務介面
- `Services/Sales/SalesOrderService.cs` - 銷貨服務實現  
- `Services/Purchase/PurchaseReceivingService.cs` - 採購入庫服務

### 前端元件
- `Components/Pages/Sales/SalesOrderEditModalComponent.razor` - 銷貨訂單編輯
- `Components/Shared/SubCollections/SalesOrderDetailManagerComponent.razor` - 銷貨明細管理

---

## ✅ 驗收標準

- [ ] 所有測試案例通過
- [ ] 編譯無錯誤無警告  
- [ ] 資料庫遷移成功執行
- [ ] FIFO 庫存扣減正確運作
- [ ] 倉庫驗證邏輯正確實施
- [ ] 無資料遺失或異常

**測試完成後，請在此打勾確認各項功能正常運作。**