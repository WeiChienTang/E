# 採購訂單庫存邏輯修正記錄

## 更新日期
2025年9月8日

## 修正概述
重新設計並實現採購訂單與庫存系統的整合邏輯，確保採購服務只負責記錄預計到貨數量，不干涉實際庫存管理。

## 主要修正內容

### 1. 核心邏輯調整

#### 修正前的問題
- 採購訂單核准時會檢查倉庫位置
- 錯誤訊息：`核准訂單成功，但記錄庫存位置失敗：採購訂單未指定倉庫，無法記錄庫存位置`
- 採購服務過度干涉庫存管理

#### 修正後的邏輯
- **採購確認時**：只增加 `InventoryStock.InTransitStock`（在途庫存）
- **採購駁回時**：減少 `InventoryStock.InTransitStock`（如果之前已核准）
- **不檢查倉庫和庫位**：`WarehouseId` 和 `WarehouseLocationId` 設為 `null`
- **自動創建庫存記錄**：如果商品沒有庫存記錄，自動創建基本記錄

### 2. 修改的文件

#### `Services/Purchase/PurchaseOrderService.cs`
- 新增 `#region 採購在途庫存管理` 區塊
- 實現 `UpdateInTransitStockAsync()` 方法
- 實現 `UpdateProductInTransitStockAsync()` 方法
- 實現 `LogInventoryTransactionAsync()` 方法（暫時不記錄，因為需要 WarehouseId）
- 修改 `ApproveOrderAsync()` 方法：增加在途庫存更新
- 修改 `RejectOrderAsync()` 方法：減少在途庫存更新

### 3. 庫存記錄創建邏輯

```csharp
// 如果沒有庫存記錄，創建一個基本記錄
stock = new InventoryStock
{
    ProductId = productId,
    WarehouseId = null,  // 採購階段不指定倉庫
    WarehouseLocationId = null,  // 採購階段不指定庫位
    CurrentStock = 0,  // 不影響實際庫存
    ReservedStock = 0,
    InTransitStock = isIncrease ? quantity : 0,  // 只記錄在途數量
    Status = EntityStatus.Active,
    CreatedAt = DateTime.Now,
    UpdatedAt = DateTime.Now
};
```

### 4. 事務處理邏輯

```csharp
// 採購確認時，增加在途庫存
var inventoryUpdateResult = await UpdateInTransitStockAsync(orderId, true);
if (!inventoryUpdateResult.IsSuccess)
{
    await transaction.RollbackAsync();
    return ServiceResult.Failure($"核准訂單成功，但更新在途庫存失敗：{inventoryUpdateResult.ErrorMessage}");
}
```

## 業務流程

### 採購訂單核准流程
1. 驗證採購訂單是否存在且未核准
2. 檢查是否有有效的商品明細
3. 更新採購訂單狀態（`IsApproved = true`）
4. 針對每個商品：
   - 檢查是否有庫存記錄，沒有則創建
   - 增加 `InTransitStock` 數量
   - 不影響 `CurrentStock`
5. 提交事務

### 採購訂單駁回流程
1. 驗證採購訂單是否存在
2. 檢查是否已有進貨記錄（有則無法駁回）
3. 記錄之前是否已核准
4. 重置審核狀態（`IsApproved = false`）
5. 如果之前已核准：
   - 減少對應商品的 `InTransitStock` 數量
6. 提交事務

## 技術細節

### 依賴注入
```csharp
private readonly IInventoryStockService? _inventoryStockService;
```
- 可選注入，向下兼容現有代碼
- 目前採購服務直接操作 `InventoryStock`，未使用庫存服務

### 錯誤處理
- 使用資料庫事務確保數據一致性
- 庫存更新失敗會回滾採購訂單狀態更新
- 詳細的錯誤日誌記錄

### InventoryTransaction 記錄
目前暫時跳過記錄 `InventoryTransaction`，因為：
- `InventoryTransaction` 需要 `WarehouseId`（必填）
- 採購階段沒有指定倉庫
- 實際的庫存異動記錄應該在入庫時由庫存服務負責

## 未完成的部分

### 1. InventoryTransaction 記錄
- **問題**：採購階段無法記錄 `InventoryTransaction`，因為缺少 `WarehouseId`
- **待解決**：
  - 選項 1：修改 `InventoryTransaction` 結構，讓 `WarehouseId` 可為 null
  - 選項 2：在採購階段使用預設倉庫
  - 選項 3：延遲到入庫時才記錄交易

### 2. 庫存服務整合
- **現狀**：直接操作 `InventoryStock` 實體
- **改進方向**：使用 `IInventoryStockService` 進行庫存操作
- **好處**：更好的封裝和統一的庫存管理邏輯

### 3. 入庫流程整合
- **待實現**：收貨時將 `InTransitStock` 轉為 `CurrentStock`
- **需要考慮**：部分收貨、退貨、庫存位置分配

### 4. 庫存預警機制
- **基於 InTransitStock 的預警**：總庫存 = CurrentStock + InTransitStock
- **報表整合**：在途庫存報表和分析功能

## 測試建議

### 測試案例 1：新採購訂單核准
1. 創建新採購訂單（包含明細）
2. 核准採購訂單
3. 驗證：
   - 訂單狀態變為已核准
   - 對應商品的 `InTransitStock` 增加
   - `CurrentStock` 不變

### 測試案例 2：已核准訂單駁回
1. 使用已核准的採購訂單
2. 駁回採購訂單
3. 驗證：
   - 訂單狀態變為未核准
   - 對應商品的 `InTransitStock` 減少
   - `CurrentStock` 不變

### 測試案例 3：沒有庫存記錄的商品
1. 使用全新商品創建採購訂單
2. 核准採購訂單
3. 驗證：
   - 自動創建 `InventoryStock` 記錄
   - `WarehouseId` 和 `WarehouseLocationId` 為 null
   - `InTransitStock` 正確設置

### 測試案例 4：事務回滾
1. 模擬庫存更新失敗
2. 驗證採購訂單狀態沒有更新（事務回滾正常）

## SQL 查詢驗證

```sql
-- 查看特定商品的在途庫存
SELECT p.Code, p.Name, i.InTransitStock, i.CurrentStock, i.WarehouseId
FROM InventoryStocks i
JOIN Products p ON i.ProductId = p.Id
WHERE p.Id = [商品ID];

-- 查看採購訂單狀態
SELECT PurchaseOrderNumber, IsApproved, ApprovedAt, ApprovedBy
FROM PurchaseOrders
WHERE PurchaseOrderNumber = '[訂單號]';

-- 查看庫存記錄（包含 null 倉庫的記錄）
SELECT p.Code, p.Name, i.CurrentStock, i.InTransitStock, i.WarehouseId, i.WarehouseLocationId
FROM InventoryStocks i
JOIN Products p ON i.ProductId = p.Id
WHERE i.WarehouseId IS NULL;
```

## 版本兼容性

### 向下兼容
- 現有的 `PurchaseOrderService` 調用不會受影響
- 可選的 `IInventoryStockService` 依賴注入
- 不影響其他服務的運作

### 數據遷移需求
- **無需遷移**：現有的 `InventoryStock` 記錄保持不變
- **新記錄**：採購創建的記錄會有 `WarehouseId = null`

## 後續開發建議

### 優先級 1（高）
1. 解決 `InventoryTransaction` 記錄問題
2. 實現收貨流程的庫存轉換
3. 增加單元測試

### 優先級 2（中）
1. 整合 `IInventoryStockService`
2. 實現庫存預警機制
3. 添加在途庫存報表

### 優先級 3（低）
1. 性能優化
2. 批量處理支援
3. 庫存分析功能

## 相關文件

- [原始測試指引](./test_inventory_integration.md)
- [庫存實體定義](../Data/Entities/Warehouses/InventoryStock.cs)
- [採購訂單服務](../Services/Purchase/PurchaseOrderService.cs)

## 修改歷史

| 日期 | 修改者 | 修改內容 |
|------|--------|----------|
| 2025-09-08 | AI Assistant | 重新實現採購庫存邏輯，修正編譯錯誤 |

---

**注意**：此文檔記錄了當前的實現狀態和未完成的部分，建議在繼續開發前先閱讀此文檔了解現況。
