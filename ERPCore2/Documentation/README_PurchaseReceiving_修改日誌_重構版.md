# 採購進貨系統修改日誌 - 重構版

## 問題1 - 編輯模式庫存重複累加

### 描述
編輯採購進貨單時，庫存數量會重複累加而非精確調整。第一次編輯商品A從20個改為21個庫存正確為21個，第二次編輯從21個改為22個時庫存卻變成23個，第三次編輯從30個改為20個時庫存變成36個，減量操作完全不生效。

### 原因
❌ 交易記錄查詢不完整
原始邏輯只查詢 `Purchase` 類型的交易記錄，但編輯時會產生多種交易編號：
```csharp
var existingTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber == currentReceiving.ReceiptNumber && 
           t.TransactionType == InventoryTransactionTypeEnum.Purchase)
```
實際編輯產生的交易編號包括：原始進貨 `R202501140001`、編輯調增 `R202501140001_ADJ`、編輯回退 `R202501140001_REVERT`

❌ 差異計算基準點錯誤
每次編輯都是拿當前明細與原始交易記錄比較，而不是與上一次編輯後的狀態比較，導致累加計算錯誤。

### 解決
✅ 完整統計所有相關交易記錄
```csharp
var existingTransactions = await context.InventoryTransactions
    .Where(t => (t.TransactionNumber == currentReceiving.ReceiptNumber ||
               t.TransactionNumber.StartsWith(currentReceiving.ReceiptNumber + "_")) &&
           !t.IsDeleted)
    .ToListAsync();
```

✅ 使用淨值計算方式
```csharp
int processedQuantity = hasProcessed ? processedInventory[key].NetProcessedQuantity : 0;
int targetQuantity = hasCurrent ? currentInventory[key].CurrentQuantity : 0;
int adjustmentNeeded = targetQuantity - processedQuantity;
```

✅ 防重複更新機制
實作 `UpdateInventoryByDifferenceAsync` 方法進行差異比較更新，避免重複累加問題。

### 修改檔
- `Services/Purchase/PurchaseReceivingService.cs`
- `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`

## 問題2 - 產品替換庫存錯誤

### 描述
將產品A 20個改為產品C 20個時，結果兩個產品庫存都是20個，產品A應該變成0個。或者產品A從20個改為40個時，庫存變成60個而非40個。

### 原因
❌ 缺乏編輯前後差異比較機制
`SavePurchaseReceivingDetailsAsync` 每次都調用 `ConfirmReceiptAsync` 進行全量庫存增加，沒有考慮編輯前的狀態。

❌ 沒有庫存回退邏輯
當明細變更或刪除時，系統沒有相應的庫存回退處理機制。

### 解決
✅ 實作智能差異比較系統
新增 `UpdateInventoryByDifferenceAsync` 方法，根據編輯前後的實際變化進行精確庫存調整：
- 明細刪除：回退對應庫存使用 `ReduceStockAsync`
- 明細新增：直接增加庫存使用 `AddStockAsync`
- 明細修改：計算差異量進行調整
- 產品替換：舊產品回退加新產品增加

✅ 智能模式選擇機制
```csharp
bool isEditMode = PurchaseReceivingId.HasValue && PurchaseReceivingId.Value > 0;
if (isEditMode)
{
    var differenceResult = await PurchaseReceivingService.UpdateInventoryByDifferenceAsync(purchaseReceiving.Id, 0);
}
else
{
    var confirmResult = await PurchaseReceivingService.ConfirmReceiptAsync(purchaseReceiving.Id, 0);
}
```

### 修改檔
- `Services/Purchase/PurchaseReceivingService.cs`
- `Services/Purchase/IPurchaseReceivingService.cs`
- `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`

## 問題3 - 編輯模式明細不顯示

### 描述
編輯模式中即便資料表有資料，採購入庫明細也不會顯示在畫面上。

### 原因
❌ 過於複雜的商品匹配邏輯
`LoadExistingDetailsAsync` 方法使用複雜的商品匹配邏輯，當無法在 `Products` 或 `AvailableProducts` 清單中找到對應商品時，整個明細項目就會被忽略：
```csharp
var productId = GetPropertyValue<int>(detail, "ProductId");
var product = Products.FirstOrDefault(p => p.Id == productId) ?? 
             AvailableProducts.FirstOrDefault(p => p.Id == productId);
if (product != null) // 找不到商品就跳過整個明細
```

### 解決
✅ 直接使用 Entity Framework Navigation Properties
```csharp
if (detail is PurchaseReceivingDetail purchaseDetail)
{
    var item = new ReceivingItem
    {
        SelectedProduct = purchaseDetail.Product,
        SelectedWarehouse = purchaseDetail.Warehouse,
        SelectedWarehouseLocation = purchaseDetail.WarehouseLocation,
        SelectedPurchaseDetail = purchaseDetail.PurchaseOrderDetail,
        ReceivedQuantity = purchaseDetail.ReceivedQuantity,
        UnitPrice = purchaseDetail.UnitPrice,
        Remarks = purchaseDetail.InspectionRemarks ?? string.Empty,
        ExistingDetailEntity = detail
    };
    
    item.ProductSearch = item.DisplayName;
    ReceivingItems.Add(item);
}
```

✅ 服務層載入完整關聯資料
```csharp
var purchaseReceivingQuery = _context.PurchaseReceivingDetails
    .Include(d => d.Product)
    .Include(d => d.Warehouse)
    .Include(d => d.WarehouseLocation)
    .Include(d => d.PurchaseOrderDetail)
    .ThenInclude(pod => pod.Product)
    .Where(d => d.PurchaseReceivingId == parentId);
```

### 修改檔
- `Components/Shared/SubCollections/PurchaseReceivingDetailManagerComponent.razor`
- `Services/Purchase/PurchaseReceivingDetailService.cs`
- `Services/Interfaces/IPurchaseReceivingDetailService.cs`

## 問題4 - MARS 交易警告與儲存失敗

### 描述
採購進貨明細儲存時出現 MARS (Multiple Active Result Sets) 交易警告，導致明細資料未正確儲存到資料表。

### 原因
❌ 巢狀交易衝突
`SaveWithDetailsAsync` 方法內調用 `UpdateDetailsAsync` 造成巢狀交易衝突，SQL Server 無法處理同時進行的多個交易。

❌ 驗證邏輯過於嚴格
驗證要求 `OrderQuantity`、`PurchaseOrderDetailId` 等非必要欄位，但用戶實際只需要選擇產品和倉庫。

### 解決
✅ 實作 Customer 模式二階段儲存
```csharp
<GenericEditModalComponent @ref="editModalComponent"
                          TEntity="PurchaseReceiving"
                          UseGenericSave="true"
                          AfterSave="@SavePurchaseReceivingDetailsAsync">
```

✅ 分離主檔和明細儲存邏輯
主檔透過 `UseGenericSave="true"` 先儲存，明細在 `AfterSave` 回調中使用獨立交易處理，避免巢狀交易衝突。

✅ 簡化驗證邏輯
```csharp
private bool IsValidDetail(ReceivingItem item)
{
    return item.SelectedProduct?.Id > 0 && item.SelectedWarehouse?.Id > 0;
}
```

### 修改檔
- `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`
- `Services/Purchase/PurchaseReceivingDetailService.cs`

## 問題5 - 採購明細選擇缺乏業務資訊

### 描述
原本的商品選擇模式顯示為 `[產品編號] 產品名稱`，用戶無法知道選擇的是哪張採購單的明細，容易造成混淆。

### 原因
❌ 缺乏採購單資訊顯示
商品選擇模式無法區分同商品但來自不同採購單的明細，用戶難以做出正確選擇。

❌ 沒有剩餘數量資訊
用戶無法得知該採購明細還有多少數量可以進貨。

### 解決
✅ 改為採購明細選擇模式
重構 `ReceivingItem` 類別，主要邏輯基於 `SelectedPurchaseDetail` 而非 `SelectedProduct`：
```csharp
public string DisplayName => 
    SelectedPurchaseDetail?.Product != null && !string.IsNullOrEmpty(SelectedPurchaseDetail.PurchaseOrder?.PurchaseOrderNumber)
        ? $"採購單 {SelectedPurchaseDetail.PurchaseOrder.PurchaseOrderNumber} [{SelectedPurchaseDetail.Product.Code}] {SelectedPurchaseDetail.Product.Name}"
        : SelectedProduct != null 
            ? $"[{SelectedProduct.Code}] {SelectedProduct.Name}" 
            : ProductSearch;
```

✅ 實作三層篩選系統
- 第一層：廠商篩選
- 第二層：採購單篩選
- 第三層：商品篩選

✅ 新增 PurchaseOrderDetailSelectHelper
專門處理採購明細選擇，包含採購單資訊顯示和剩餘數量計算。

### 修改檔
- `Helpers/PurchaseOrderDetailSelectHelper.cs`
- `Components/Shared/SubCollections/PurchaseReceivingDetailManagerComponent.razor`
- `Models/ReceivingItem.cs`

## 問題6 - 屬性對應錯誤

### 描述
採購進貨明細的備註欄位儲存時出現屬性對應錯誤，導致資料無法正確儲存。

### 原因
❌ 錯誤的屬性對應
```csharp
existingDetail.Remarks = updatedDetail.InspectionRemarks; // 錯誤對應
```

### 解決
✅ 修正屬性對應關係
```csharp
existingDetail.InspectionRemarks = updatedDetail.InspectionRemarks;
```

### 修改檔
- `Services/Purchase/PurchaseReceivingDetailService.cs`