# 採購退貨明細編輯時庫存回滾問題分析與解決方案

## 問題描述
當商品透過編輯模式修改其中的明細商品成另一項商品時，庫存 `InventoryStock` 並不會做任何的更動，不會回滾原本的商品然後對新的商品進行編輯。

## 問題深入分析

### 發現的真正問題

經過進一步檢查，發現了更深層的問題：

#### 問題 1：EF Core 實體追蹤問題
在原始修正中，當我們將 `existingDetail` 加入 `quantityChanges` 用於庫存回滾時，由於 EF Core 的實體追蹤機制：

1. `existingDetail` 是從資料庫載入的實體
2. 當我們執行 `context.Entry(existing).CurrentValues.SetValues(updated)` 時
3. `existingDetail` 的 `ProductId` 已經被更新為新的商品ID
4. 因此兩個 `quantityChanges` 記錄都指向新商品，原商品庫存沒有被回滾

#### 問題 2：庫存變更記錄的商品ID錯誤
```csharp
// 錯誤的做法：existingDetail 的 ProductId 已經被 EF 更新
quantityChanges.Add((existingDetail, -existingDetail.ReturnQuantity)); // ❌ ProductId 已是新商品
quantityChanges.Add((detail, detail.ReturnQuantity)); // ✅ ProductId 是新商品
```

結果：兩個操作都作用在新商品上，原商品庫存沒有變化

### 根本解決方案

#### 創建獨立的原商品記錄
為了正確處理商品變更，我們需要創建一個獨立的 `PurchaseReturnDetail` 物件來保存原始商品資訊：

```csharp
if (productChanged)
{
    // 1. 創建原商品的庫存回滾記錄（使用原始資料）
    if (existingDetail.ReturnQuantity > 0)
    {
        var originalProductDetail = new PurchaseReturnDetail
        {
            Id = existingDetail.Id,
            ProductId = existingDetail.ProductId, // 保持原商品ID
            PurchaseReceivingDetailId = existingDetail.PurchaseReceivingDetailId, // 保持原進貨明細ID
            WarehouseLocationId = existingDetail.WarehouseLocationId,
            ReturnQuantity = existingDetail.ReturnQuantity,
            OriginalUnitPrice = existingDetail.OriginalUnitPrice
        };
        
        quantityChanges.Add((originalProductDetail, -existingDetail.ReturnQuantity));
    }
    
    // 2. 扣減新商品的庫存
    if (detail.ReturnQuantity > 0)
    {
        quantityChanges.Add((detail, detail.ReturnQuantity));
    }
}
```

#### 關鍵技術點

1. **保存原始商品資訊**：在 EF 更新實體之前，先保存原始的商品ID和相關資訊
2. **獨立物件創建**：創建新的 `PurchaseReturnDetail` 物件，避免 EF 追蹤干擾
3. **正確的庫存操作**：
   - 原商品：回滾庫存（增加數量）
   - 新商品：扣減庫存（減少數量）

## 解決方案實作

### 已完成的修正

#### 1. 修正 PurchaseReturnService.UpdateDetailsInContext 方法

已加入商品變更檢測和完整的庫存回滾邏輯：

```csharp
// 檢查是否有商品變更（關鍵修正點）
bool productChanged = existingDetail.ProductId != detail.ProductId || 
                     existingDetail.PurchaseReceivingDetailId != detail.PurchaseReceivingDetailId;

if (productChanged)
{
    // 商品變更：需要完整的庫存回滾和重新扣減
    // 1. 回滾原商品的庫存（增加原來退回的數量）
    if (existingDetail.ReturnQuantity > 0)
    {
        quantityChanges.Add((existingDetail, -existingDetail.ReturnQuantity));
        _logger?.LogInformation("檢測到退貨明細商品變更 - 明細ID: {DetailId}, 原商品: {OldProductId}, 新商品: {NewProductId}, 回滾數量: {Quantity}", 
                              detail.Id, existingDetail.ProductId, detail.ProductId, existingDetail.ReturnQuantity);
    }
    
    // 2. 扣減新商品的庫存（減少新的退回數量）
    if (detail.ReturnQuantity > 0)
    {
        quantityChanges.Add((detail, detail.ReturnQuantity));
        _logger?.LogInformation("商品變更後新增庫存扣減 - 明細ID: {DetailId}, 新商品: {ProductId}, 扣減數量: {Quantity}", 
                              detail.Id, detail.ProductId, detail.ReturnQuantity);
    }
}
else
{
    // 只有數量變更：計算差異
    var quantityDiff = detail.ReturnQuantity - existingDetail.ReturnQuantity;
    if (quantityDiff != 0)
    {
        quantityChanges.Add((detail, quantityDiff));
        _logger?.LogInformation("退貨明細數量變更 - 明細ID: {DetailId}, 商品: {ProductId}, 數量差異: {QuantityDiff}", 
                              detail.Id, detail.ProductId, quantityDiff);
    }
}
```

#### 2. 增強庫存更新日誌

加入更詳細的日誌記錄，方便追蹤庫存變更：

```csharp
if (quantityDiff > 0)
{
    // 退貨數量增加，需要減少庫存
    operationDescription = $"採購退貨增量 - {savedEntity.PurchaseReturnNumber} (商品ID: {detail.ProductId})";
    _logger?.LogInformation("執行庫存扣減 - 商品ID: {ProductId}, 倉庫ID: {WarehouseId}, 數量: {Quantity}", 
                          detail.ProductId, warehouseId.Value, quantityDiff);
}
else
{
    // 退貨數量減少，需要增加庫存 (撤銷部分退貨)
    operationDescription = $"採購退貨撤銷 - {savedEntity.PurchaseReturnNumber} (商品ID: {detail.ProductId})";
    _logger?.LogInformation("執行庫存回復 - 商品ID: {ProductId}, 倉庫ID: {WarehouseId}, 數量: {Quantity}", 
                          detail.ProductId, warehouseId.Value, Math.Abs(quantityDiff));
}
```

### 修正效果

1. **商品變更檢測**：系統現在能正確檢測 `ProductId` 和 `PurchaseReceivingDetailId` 的變更
2. **完整庫存回滾**：當檢測到商品變更時，會先回滾原商品的庫存，再對新商品進行庫存扣減
3. **詳細日誌記錄**：提供完整的操作追蹤，方便問題診斷
4. **錯誤處理**：增強了錯誤處理和回滾機制

### 測試場景

建議測試以下情況：
- ✅ 商品 A → 商品 B 的變更
- ✅ 數量變更 + 商品變更的組合  
- ✅ 進貨明細變更（可能涉及倉庫變更）
- ✅ 庫存不足的錯誤處理

## 結論

這個問題的根本原因是庫存更新邏輯過於簡化，只考慮了數量變更而忽略了商品變更。通過加入商品變更檢測和完整的庫存回滾邏輯，可以確保庫存資料的準確性。

建議參考採購進貨服務的做法，實作完整的差異比較邏輯，以支援更複雜的編輯情況。