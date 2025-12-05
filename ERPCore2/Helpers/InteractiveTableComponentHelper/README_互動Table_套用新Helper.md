# InteractiveTableComponent Helper 優化建議

---

## 📌 目標

針對使用 `InteractiveTableComponent` 的多個 Table 組件進行重複代碼分析，提供可抽取為 Helper 的建議方案，減少代碼重複並提高可維護性。

---

## 🔍 分析範圍

### 影響範圍統計

| 組件名稱 | 路徑 | 優先級 |
|---------|------|--------|
| SalesOrderTable | Components/Shared/BaseModal/Modals/Sales/ | 🔴 高 |
| PurchaseReceivingTable | Components/Shared/BaseModal/Modals/Purchase/ | 🔴 高 |
| PurchaseReturnTable | Components/Shared/BaseModal/Modals/Purchase/ | 🔴 高 |
| SalesReturnTable | Components/Shared/BaseModal/Modals/Sales/ | 🔴 高 |
| PurchaseOrderTable | Components/Shared/BaseModal/Modals/Purchase/ | 🟡 中 |
| QuotationTable | Components/Shared/BaseModal/Modals/Quotation/ | 🟡 中 |
| SalesDeliveryTable | Components/Shared/BaseModal/Modals/Sales/ | 🟡 中 |
| MaterialIssueTable | Components/Shared/BaseModal/Modals/MaterialIssue/ | 🟡 中 |
| 其他 Table 組件 | 多個路徑 | 🟢 低 |

---

## 🎯 建議創建的 Helper 清單

### 1. DetailLockHelper - 明細鎖定檢查輔助類

**優先級**: 🔴 高  
**預估工作量**: 3-4 小時  
**影響範圍**: 7+ 個組件

#### 功能說明

統一處理明細是否可刪除/修改的檢查邏輯，包括：
- 沖款記錄檢查 (TotalPaidAmount / TotalReceivedAmount)
- 退貨記錄檢查 (已退貨數量字典)
- 轉單記錄檢查 (ConvertedQuantity)

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// SalesOrderTable.razor
private bool HasPaymentRecord(SalesItem item)
{
    if (item.ExistingDetailEntity is SalesOrderDetail detail && detail.Id > 0)
    {
        return detail.TotalReceivedAmount > 0;
    }
    return false;
}

private bool HasReturnRecord(SalesItem item)
{
    if (item.ExistingDetailEntity is SalesOrderDetail detail && detail.Id > 0)
    {
        return _returnedQuantities.ContainsKey(detail.Id);
    }
    return false;
}

private decimal GetReturnedQuantity(SalesItem item)
{
    if (item.ExistingDetailEntity is SalesOrderDetail detail && detail.Id > 0)
    {
        return _returnedQuantities.TryGetValue(detail.Id, out var qty) ? qty : 0;
    }
    return 0;
}

private bool CanDeleteItem(SalesItem item, out string reason)
{
    if (HasReturnRecord(item))
    {
        var returnedQty = GetReturnedQuantity(item);
        reason = $"此商品已有退貨記錄（已退貨 {returnedQty} 個），無法刪除";
        return false;
    }
    
    if (HasPaymentRecord(item))
    {
        var receivedAmount = GetReceivedAmount(item);
        reason = $"此商品已有沖款記錄（已收款 {receivedAmount:N0} 元），無法刪除";
        return false;
    }
    
    reason = string.Empty;
    return true;
}
```

**類似的代碼也出現在**:
- PurchaseReturnTable.razor (檢查 TotalReceivedAmount)
- SalesReturnTable.razor (檢查 TotalPaidAmount)
- PurchaseReceivingTable.razor (檢查退貨記錄和付款記錄)
- QuotationTable.razor (檢查 ConvertedQuantity)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/DetailLockHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 明細鎖定檢查輔助類
    /// 用於檢查明細項目是否因為有相關記錄而無法刪除或修改
    /// </summary>
    public static class DetailLockHelper
    {
        /// <summary>
        /// 檢查實體是否有付款/收款記錄
        /// 支援的屬性名稱: TotalPaidAmount, TotalReceivedAmount
        /// </summary>
        public static bool HasPaymentRecord<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return false;
            
            var type = entity.GetType();
            
            // 檢查 TotalPaidAmount (應付款)
            var paidProperty = type.GetProperty("TotalPaidAmount");
            if (paidProperty != null)
            {
                var paidValue = (decimal?)paidProperty.GetValue(entity);
                if (paidValue > 0) return true;
            }
            
            // 檢查 TotalReceivedAmount (應收款)
            var receivedProperty = type.GetProperty("TotalReceivedAmount");
            if (receivedProperty != null)
            {
                var receivedValue = (decimal?)receivedProperty.GetValue(entity);
                if (receivedValue > 0) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得付款/收款金額
        /// </summary>
        public static decimal GetPaymentAmount<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return 0;
            
            var type = entity.GetType();
            var paidProperty = type.GetProperty("TotalPaidAmount");
            if (paidProperty != null)
            {
                return (decimal?)paidProperty.GetValue(entity) ?? 0;
            }
            
            var receivedProperty = type.GetProperty("TotalReceivedAmount");
            if (receivedProperty != null)
            {
                return (decimal?)receivedProperty.GetValue(entity) ?? 0;
            }
            
            return 0;
        }
        
        /// <summary>
        /// 檢查實體是否有退貨記錄 (透過外部字典)
        /// </summary>
        public static bool HasReturnRecord<TEntity>(
            TEntity entity, 
            Dictionary<int, decimal> returnedQuantities) where TEntity : class
        {
            if (entity == null || returnedQuantities == null) return false;
            
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var id = (int)idProperty.GetValue(entity)!;
                return returnedQuantities.ContainsKey(id);
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得退貨數量
        /// </summary>
        public static decimal GetReturnedQuantity<TEntity>(
            TEntity entity, 
            Dictionary<int, decimal> returnedQuantities) where TEntity : class
        {
            if (entity == null || returnedQuantities == null) return 0;
            
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var id = (int)idProperty.GetValue(entity)!;
                return returnedQuantities.TryGetValue(id, out var qty) ? qty : 0;
            }
            
            return 0;
        }
        
        /// <summary>
        /// 檢查是否有轉單記錄
        /// 支援的屬性名稱: ConvertedQuantity
        /// </summary>
        public static bool HasConversionRecord<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return false;
            
            var convertedProperty = entity.GetType().GetProperty("ConvertedQuantity");
            if (convertedProperty != null)
            {
                var convertedValue = (decimal?)convertedProperty.GetValue(entity);
                return convertedValue > 0;
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得已轉單數量
        /// </summary>
        public static decimal GetConvertedQuantity<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return 0;
            
            var convertedProperty = entity.GetType().GetProperty("ConvertedQuantity");
            if (convertedProperty != null)
            {
                return (decimal?)convertedProperty.GetValue(entity) ?? 0;
            }
            
            return 0;
        }
        
        /// <summary>
        /// 綜合檢查項目是否可以刪除
        /// </summary>
        public static bool CanDeleteItem<TEntity>(
            TEntity entity,
            out string reason,
            Dictionary<int, decimal>? returnedQuantities = null) where TEntity : class
        {
            reason = string.Empty;
            
            if (entity == null)
            {
                reason = "項目不存在";
                return false;
            }
            
            // 檢查退貨記錄
            if (returnedQuantities != null && HasReturnRecord(entity, returnedQuantities))
            {
                var returnedQty = GetReturnedQuantity(entity, returnedQuantities);
                reason = $"此項目已有退貨記錄（已退貨 {returnedQty} 個），無法刪除";
                return false;
            }
            
            // 檢查付款記錄
            if (HasPaymentRecord(entity))
            {
                var paidAmount = GetPaymentAmount(entity);
                reason = $"此項目已有沖款記錄（已沖款 {paidAmount:N0} 元），無法刪除";
                return false;
            }
            
            // 檢查轉單記錄
            if (HasConversionRecord(entity))
            {
                var convertedQty = GetConvertedQuantity(entity);
                reason = $"此項目已有轉單記錄（已轉單 {convertedQty} 個），無法刪除";
                return false;
            }
            
            return true;
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesOrderTable.razor**:
```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 簡化為一行調用
private bool HasReturnRecord(SalesItem item)
{
    return item.ExistingDetailEntity != null && 
           DetailLockHelper.HasReturnRecord(item.ExistingDetailEntity, _returnedQuantities);
}

private bool HasPaymentRecord(SalesItem item)
{
    return item.ExistingDetailEntity != null && 
           DetailLockHelper.HasPaymentRecord(item.ExistingDetailEntity);
}

private decimal GetReturnedQuantity(SalesItem item)
{
    return item.ExistingDetailEntity != null 
        ? DetailLockHelper.GetReturnedQuantity(item.ExistingDetailEntity, _returnedQuantities)
        : 0;
}

private bool CanDeleteItem(SalesItem item, out string reason)
{
    if (item.ExistingDetailEntity == null)
    {
        reason = string.Empty;
        return true;
    }
    
    // 所有檢查邏輯都封裝在 Helper 中
    return DetailLockHelper.CanDeleteItem(
        item.ExistingDetailEntity, 
        out reason, 
        _returnedQuantities);
}
```

**優點**:
- ✅ 減少 30-50 行重複代碼
- ✅ 統一錯誤訊息格式
- ✅ 使用反射自動偵測屬性，支援不同實體類型
- ✅ 更容易測試和維護

#### 套用進度

- [ ] SalesOrderTable.razor
- [ ] PurchaseReceivingTable.razor
- [ ] PurchaseReturnTable.razor
- [ ] SalesReturnTable.razor
- [ ] QuotationTable.razor
- [ ] SalesDeliveryTable.razor
- [ ] PurchaseOrderTable.razor

---

### 2. RelatedDocumentsViewHelper - 相關單據查看輔助類

**優先級**: 🔴 高  
**預估工作量**: 3-4 小時  
**影響範圍**: 10+ 個組件

#### 功能說明

統一處理相關單據查看的 Modal 顯示邏輯，減少每個組件重複維護 Modal 狀態、載入邏輯和錯誤處理。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// SalesOrderTable.razor
// ===== 相關單據查看 =====
private bool showRelatedDocumentsModal = false;
private string selectedProductName = string.Empty;
private List<RelatedDocument>? relatedDocuments = null;
private bool isLoadingRelatedDocuments = false;

private async Task ShowRelatedDocuments(SalesItem item)
{
    if (item.ExistingDetailEntity is not SalesOrderDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";
    
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        relatedDocuments = await RelatedDocumentsHelper.GetRelatedDocumentsForSalesOrderDetailAsync(detail.Id);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入相關單據失敗：{ex.Message}");
    }
    finally
    {
        isLoadingRelatedDocuments = false;
        StateHasChanged();
    }
}

// Razor 標記
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

**類似的代碼也出現在**:
- PurchaseOrderTable.razor (載入進貨單相關單據)
- PurchaseReturnTable.razor (載入退貨相關單據)
- QuotationTable.razor (載入報價轉銷貨單據)
- MaterialIssueTable.razor (載入領貨相關單據)
- 其他多個檔案...

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/RelatedDocumentsViewHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 相關單據查看輔助類
    /// 用於統一管理相關單據 Modal 的顯示狀態和資料載入
    /// </summary>
    public class RelatedDocumentsViewHelper
    {
        public bool IsVisible { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<RelatedDocument>? Documents { get; set; }
        public bool IsLoading { get; set; }
        
        /// <summary>
        /// 顯示相關單據 Modal
        /// </summary>
        /// <typeparam name="TDetail">明細實體類型</typeparam>
        /// <param name="detail">明細實體</param>
        /// <param name="productName">商品名稱</param>
        /// <param name="loadDocumentsFunc">載入單據的委派函數</param>
        /// <param name="notificationService">通知服務</param>
        /// <param name="stateHasChangedAction">狀態變更回調</param>
        public async Task ShowAsync<TDetail>(
            TDetail? detail,
            string productName,
            Func<int, Task<List<RelatedDocument>>> loadDocumentsFunc,
            INotificationService notificationService,
            Action stateHasChangedAction) where TDetail : class
        {
            // 檢查明細是否有效
            if (detail == null)
            {
                await notificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
                return;
            }
            
            var idProperty = detail.GetType().GetProperty("Id");
            if (idProperty == null)
            {
                await notificationService.ShowWarningAsync("無法取得項目ID", "錯誤");
                return;
            }
            
            var detailId = (int)idProperty.GetValue(detail)!;
            if (detailId <= 0)
            {
                await notificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
                return;
            }
            
            // 設定狀態並開始載入
            ProductName = productName;
            IsVisible = true;
            IsLoading = true;
            Documents = null;
            stateHasChangedAction();
            
            try
            {
                Documents = await loadDocumentsFunc(detailId);
            }
            catch (Exception ex)
            {
                await notificationService.ShowErrorAsync($"載入相關單據失敗：{ex.Message}", "錯誤");
            }
            finally
            {
                IsLoading = false;
                stateHasChangedAction();
            }
        }
        
        /// <summary>
        /// 關閉 Modal
        /// </summary>
        public void Hide()
        {
            IsVisible = false;
            Documents = null;
            ProductName = string.Empty;
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesOrderTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 只需要一個 Helper 實例，不需要 4 個狀態變數
private RelatedDocumentsViewHelper _relatedDocsHelper = new();

// 顯示相關單據的方法大幅簡化
private async Task ShowRelatedDocuments(SalesItem item)
{
    await _relatedDocsHelper.ShowAsync(
        detail: item.ExistingDetailEntity as SalesOrderDetail,
        productName: item.SelectedProduct?.Name ?? "未知商品",
        loadDocumentsFunc: async (detailId) => 
            await RelatedDocumentsHelper.GetRelatedDocumentsForSalesOrderDetailAsync(detailId),
        notificationService: NotificationService,
        stateHasChangedAction: StateHasChanged
    );
}

// Razor 標記 - 綁定到 Helper 的屬性
<RelatedDocumentsModalComponent IsVisible="@_relatedDocsHelper.IsVisible"
                               IsVisibleChanged="@((bool visible) => { if (!visible) _relatedDocsHelper.Hide(); })"
                               ProductName="@_relatedDocsHelper.ProductName"
                               RelatedDocuments="@_relatedDocsHelper.Documents"
                               IsLoading="@_relatedDocsHelper.IsLoading"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

**優點**:
- ✅ 減少 40-60 行重複代碼（每個使用的組件）
- ✅ 統一錯誤處理邏輯
- ✅ 狀態管理更清晰（封裝在 Helper 中）
- ✅ 避免忘記設定 StateHasChanged
- ✅ 使用泛型支援不同的明細實體類型

#### 套用進度

- [ ] SalesOrderTable.razor
- [ ] PurchaseReceivingTable.razor
- [ ] PurchaseReturnTable.razor
- [ ] SalesReturnTable.razor
- [ ] PurchaseOrderTable.razor
- [ ] QuotationTable.razor
- [ ] MaterialIssueTable.razor
- [ ] ProductSupplierTable.razor
- [ ] SupplierProductTable.razor
- [ ] ProductCompositionTable.razor

---

### 3. BatchOperationHelper - 批次操作輔助類

**優先級**: 🟡 中  
**預估工作量**: 2-3 小時  
**影響範圍**: 5+ 個組件

#### 功能說明

統一處理批次操作（全填、清空、刪除等）的邏輯，包括鎖定項目的檢查和訊息提示。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// PurchaseReceivingTable.razor
private async Task FillAllQuantities()
{
    var nonEmptyItems = ReceivingItems.Where(item => !IsEmptyRow(item)).ToList();
    
    if (!nonEmptyItems.Any())
    {
        await NotificationService.ShowWarningAsync("沒有可更新的明細項目", "提示");
        return;
    }
    
    var unlocked = nonEmptyItems.Where(item => CanDeleteItem(item, out _)).ToList();
    var locked = nonEmptyItems.Where(item => !CanDeleteItem(item, out _)).ToList();
    
    if (!unlocked.Any())
    {
        await NotificationService.ShowWarningAsync(
            "所有明細都已有退貨或沖款記錄，無法批次填入數量", 
            "操作限制");
        return;
    }
    
    foreach (var item in unlocked)
    {
        if (item.SelectedPurchaseDetail != null)
        {
            item.ReceivedQuantity = item.OrderQuantity;
        }
    }
    
    var message = $"已填入 {unlocked.Count} 項明細的進貨數量";
    if (locked.Any())
    {
        message += $"\n（已跳過 {locked.Count} 項已鎖定的明細）";
    }
    await NotificationService.ShowSuccessAsync(message);
    
    await NotifyDetailsChanged();
}

private async Task ClearAllQuantities()
{
    var nonEmptyItems = ReceivingItems.Where(item => !IsEmptyRow(item)).ToList();
    
    if (!nonEmptyItems.Any())
    {
        await NotificationService.ShowWarningAsync("沒有可更新的明細項目", "提示");
        return;
    }
    
    var unlocked = nonEmptyItems.Where(item => CanDeleteItem(item, out _)).ToList();
    var locked = nonEmptyItems.Where(item => !CanDeleteItem(item, out _)).ToList();
    
    if (!unlocked.Any())
    {
        await NotificationService.ShowWarningAsync(
            "所有明細都已被鎖定，無法批次操作", 
            "操作限制");
        return;
    }
    
    foreach (var item in unlocked)
    {
        item.ReceivedQuantity = 0;
    }
    
    var message = $"已清空 {unlocked.Count} 項明細的進貨數量";
    if (locked.Any())
    {
        message += $"\n（已跳過 {locked.Count} 項已鎖定的明細）";
    }
    await NotificationService.ShowSuccessAsync(message);
    
    await NotifyDetailsChanged();
}
```

**類似的代碼也出現在**:
- PurchaseReturnTable.razor (批次填入/清空)
- SalesReturnTable.razor (批次操作退貨數量)
- InventoryStockTable.razor (批次套用倉庫)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/BatchOperationHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 批次操作結果
    /// </summary>
    public class BatchOperationResult
    {
        public int ProcessedCount { get; set; }
        public int SkippedCount { get; set; }
        public bool HasItems => ProcessedCount > 0 || SkippedCount > 0;
        public bool HasProcessedItems => ProcessedCount > 0;
        public bool HasSkippedItems => SkippedCount > 0;
    }
    
    /// <summary>
    /// 批次操作輔助類
    /// 用於統一處理批次填入、清空、刪除等操作
    /// </summary>
    public static class BatchOperationHelper
    {
        /// <summary>
        /// 批次填入數量或值
        /// </summary>
        public static async Task<BatchOperationResult> FillAsync<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool> canModify,
            Action<TItem> fillAction,
            INotificationService notificationService,
            string operationName = "數量") where TItem : class
        {
            var result = new BatchOperationResult();
            
            var nonEmptyItems = items.Where(item => !isEmptyRow(item)).ToList();
            
            if (!nonEmptyItems.Any())
            {
                await notificationService.ShowWarningAsync("沒有可更新的明細項目", "提示");
                return result;
            }
            
            var unlocked = nonEmptyItems.Where(item => canModify(item)).ToList();
            var locked = nonEmptyItems.Where(item => !canModify(item)).ToList();
            
            result.SkippedCount = locked.Count;
            
            if (!unlocked.Any())
            {
                await notificationService.ShowWarningAsync(
                    "所有明細都已被鎖定，無法批次操作", 
                    "操作限制");
                return result;
            }
            
            foreach (var item in unlocked)
            {
                fillAction(item);
            }
            
            result.ProcessedCount = unlocked.Count;
            
            var message = $"已填入 {result.ProcessedCount} 項明細的{operationName}";
            if (result.HasSkippedItems)
            {
                message += $"\n（已跳過 {result.SkippedCount} 項已鎖定的明細）";
            }
            await notificationService.ShowSuccessAsync(message);
            
            return result;
        }
        
        /// <summary>
        /// 批次清空數量或值
        /// </summary>
        public static async Task<BatchOperationResult> ClearAsync<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool> canModify,
            Action<TItem> clearAction,
            INotificationService notificationService,
            string operationName = "數量") where TItem : class
        {
            var result = new BatchOperationResult();
            
            var nonEmptyItems = items.Where(item => !isEmptyRow(item)).ToList();
            
            if (!nonEmptyItems.Any())
            {
                await notificationService.ShowWarningAsync("沒有可更新的明細項目", "提示");
                return result;
            }
            
            var unlocked = nonEmptyItems.Where(item => canModify(item)).ToList();
            var locked = nonEmptyItems.Where(item => !canModify(item)).ToList();
            
            result.SkippedCount = locked.Count;
            
            if (!unlocked.Any())
            {
                await notificationService.ShowWarningAsync(
                    "所有明細都已被鎖定，無法批次操作", 
                    "操作限制");
                return result;
            }
            
            foreach (var item in unlocked)
            {
                clearAction(item);
            }
            
            result.ProcessedCount = unlocked.Count;
            
            var message = $"已清空 {result.ProcessedCount} 項明細的{operationName}";
            if (result.HasSkippedItems)
            {
                message += $"\n（已跳過 {result.SkippedCount} 項已鎖定的明細）";
            }
            await notificationService.ShowSuccessAsync(message);
            
            return result;
        }
        
        /// <summary>
        /// 批次刪除明細
        /// </summary>
        public static async Task<BatchOperationResult> RemoveAsync<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool> canDelete,
            List<int> deletedIds,
            Func<TItem, int> getEntityId,
            EventCallback<TItem>? onItemRemoved,
            INotificationService notificationService) where TItem : class
        {
            var result = new BatchOperationResult();
            
            var nonEmptyItems = items.Where(item => !isEmptyRow(item)).ToList();
            
            if (!nonEmptyItems.Any())
            {
                await notificationService.ShowWarningAsync("沒有可移除的明細項目", "提示");
                return result;
            }
            
            var unlocked = nonEmptyItems.Where(item => canDelete(item)).ToList();
            var locked = nonEmptyItems.Where(item => !canDelete(item)).ToList();
            
            result.SkippedCount = locked.Count;
            
            if (!unlocked.Any())
            {
                await notificationService.ShowWarningAsync(
                    "所有明細都已被鎖定，無法移除", 
                    "操作限制");
                return result;
            }
            
            // 通知父組件項目即將被移除
            if (onItemRemoved.HasValue)
            {
                foreach (var item in unlocked)
                {
                    await onItemRemoved.Value.InvokeAsync(item);
                }
            }
            
            // 記錄要刪除的實體 ID
            foreach (var item in unlocked)
            {
                var entityId = getEntityId(item);
                if (entityId > 0)
                {
                    deletedIds.Add(entityId);
                }
            }
            
            // 從列表中移除
            foreach (var item in unlocked)
            {
                items.Remove(item);
            }
            
            result.ProcessedCount = unlocked.Count;
            
            var message = $"已移除 {result.ProcessedCount} 項明細";
            if (result.HasSkippedItems)
            {
                message += $"\n（已保留 {result.SkippedCount} 項已鎖定的明細）";
            }
            await notificationService.ShowSuccessAsync(message);
            
            return result;
        }
    }
}
```

#### 套用之後的寫法

**修改後 - PurchaseReceivingTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 大幅簡化批次填入邏輯
private async Task FillAllQuantities()
{
    var result = await BatchOperationHelper.FillAsync(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        canModify: item => CanDeleteItem(item, out _),
        fillAction: item => item.ReceivedQuantity = item.OrderQuantity,
        notificationService: NotificationService,
        operationName: "進貨數量"
    );
    
    if (result.HasProcessedItems)
    {
        await NotifyDetailsChanged();
    }
}

// 大幅簡化批次清空邏輯
private async Task ClearAllQuantities()
{
    var result = await BatchOperationHelper.ClearAsync(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        canModify: item => CanDeleteItem(item, out _),
        clearAction: item => item.ReceivedQuantity = 0,
        notificationService: NotificationService,
        operationName: "進貨數量"
    );
    
    if (result.HasProcessedItems)
    {
        await NotifyDetailsChanged();
    }
}

// 批次刪除也變得很簡單
private async Task ClearAllDetails()
{
    var result = await BatchOperationHelper.RemoveAsync(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        canDelete: item => CanDeleteItem(item, out _),
        deletedIds: _deletedDetailIds,
        getEntityId: item => item.ExistingDetailEntity?.Id ?? 0,
        onItemRemoved: OnItemRemoved,
        notificationService: NotificationService
    );
    
    if (result.HasProcessedItems)
    {
        EnsureOneEmptyRow();
        await NotifyDetailsChanged();
    }
}
```

**優點**:
- ✅ 每個批次操作方法從 30-40 行減少到 10 行以下
- ✅ 統一的訊息格式和錯誤處理
- ✅ 自動處理鎖定項目的跳過邏輯
- ✅ 回傳結果物件，方便後續處理

#### 套用進度

- [ ] PurchaseReceivingTable.razor
- [ ] PurchaseReturnTable.razor
- [ ] SalesReturnTable.razor
- [ ] InventoryStockTable.razor
- [ ] MaterialIssueTable.razor

---

### 4. ApprovalCheckHelper - 審核檢查輔助類

**優先級**: 🟡 中  
**預估工作量**: 2-3 小時  
**影響範圍**: 3+ 個組件

#### 功能說明

統一處理審核相關的警告訊息和檢查邏輯，用於驗證來源單據的審核狀態。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// PurchaseReceivingTable.razor
private int GetUnapprovedItemsCount()
{
    if (!IsApprovalEnabled)
        return 0;
    
    return ReceivingItems
        .Where(item => !IsEmptyRow(item) && 
                      item.SelectedPurchaseDetail != null &&
                      !(item.SelectedPurchaseDetail.PurchaseOrder?.IsApproved ?? false))
        .Count();
}

// 在 Razor 標記中
@if (IsApprovalEnabled && GetUnapprovedItemsCount() > 0)
{
    <div class="alert alert-warning mb-3" role="alert">
        <div class="d-flex align-items-start">
            <i class="fas fa-exclamation-triangle me-2 mt-1"></i>
            <div>
                <strong>注意：</strong>目前有 <strong>@GetUnapprovedItemsCount()</strong> 項明細來自未審核的採購單。
                <br/>
                <small class="text-muted">這些明細將無法儲存，請確認相關採購單已完成審核後再進行入庫作業。</small>
            </div>
        </div>
    </div>
}

// 在驗證方法中
public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    // ... 其他驗證
    
    if (IsApprovalEnabled)
    {
        var unapprovedItems = ReceivingItems
            .Where(item => !IsEmptyRow(item) && 
                          item.SelectedPurchaseDetail != null &&
                          !(item.SelectedPurchaseDetail.PurchaseOrder?.IsApproved ?? false))
            .ToList();
        
        if (unapprovedItems.Any())
        {
            var itemNames = unapprovedItems
                .Select(item => $"{item.SelectedProduct?.Name} (採購單: {item.SelectedPurchaseDetail?.PurchaseOrder?.Code})")
                .ToList();
            
            errors.Add($"以下項目來自未審核的採購單，無法儲存：\n" +
                      string.Join("\n", itemNames.Select(name => $"• {name}")) +
                      $"\n\n請先完成相關採購單的審核作業。");
        }
    }
    
    // ...
}
```

**類似的代碼也出現在**:
- SalesOrderTable.razor (檢查報價單審核狀態)
- MaterialIssueTable.razor (檢查領料單審核)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/ApprovalCheckHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 審核警告資訊
    /// </summary>
    public class ApprovalWarningInfo
    {
        public int UnapprovedCount { get; set; }
        public bool HasUnapprovedItems => UnapprovedCount > 0;
        public string WarningMessage { get; set; } = string.Empty;
        public List<string> UnapprovedItemNames { get; set; } = new();
    }
    
    /// <summary>
    /// 審核檢查輔助類
    /// 用於統一處理審核相關的警告訊息
    /// </summary>
    public static class ApprovalCheckHelper
    {
        /// <summary>
        /// 取得未審核項目的警告資訊
        /// </summary>
        public static ApprovalWarningInfo GetWarningInfo<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool> isApproved,
            Func<TItem, string> getItemName,
            string documentTypeName = "單據") where TItem : class
        {
            var info = new ApprovalWarningInfo();
            
            var unapprovedItems = items
                .Where(item => !isEmptyRow(item) && !isApproved(item))
                .ToList();
            
            info.UnapprovedCount = unapprovedItems.Count;
            
            if (info.HasUnapprovedItems)
            {
                info.UnapprovedItemNames = unapprovedItems
                    .Select(getItemName)
                    .ToList();
                
                info.WarningMessage = $"目前有 {info.UnapprovedCount} 項明細來自未審核的{documentTypeName}。\n" +
                                     $"這些明細將無法儲存，請確認相關{documentTypeName}已完成審核後再進行作業。";
            }
            
            return info;
        }
        
        /// <summary>
        /// 驗證是否有未審核的項目（用於表單驗證）
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateApproval<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool> isApproved,
            Func<TItem, string> getItemName,
            string documentTypeName = "單據") where TItem : class
        {
            var errors = new List<string>();
            
            var unapprovedItems = items
                .Where(item => !isEmptyRow(item) && !isApproved(item))
                .ToList();
            
            if (unapprovedItems.Any())
            {
                var itemNames = unapprovedItems
                    .Select(getItemName)
                    .ToList();
                
                errors.Add($"以下項目來自未審核的{documentTypeName}，無法儲存：\n" +
                          string.Join("\n", itemNames.Select(name => $"• {name}")) +
                          $"\n\n請先完成相關{documentTypeName}的審核作業。");
            }
            
            return (!errors.Any(), errors);
        }
        
        /// <summary>
        /// 產生警告徽章 HTML
        /// </summary>
        public static string GetWarningBadgeHtml(int count)
        {
            if (count == 0) return string.Empty;
            return $"<span class='badge bg-warning text-dark ms-2'>{count} 項未審核</span>";
        }
    }
}
```

#### 套用之後的寫法

**修改後 - PurchaseReceivingTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 簡化為一個方法調用
private ApprovalWarningInfo GetApprovalWarning()
{
    if (!IsApprovalEnabled)
        return new ApprovalWarningInfo();
    
    return ApprovalCheckHelper.GetWarningInfo(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        isApproved: item => item.SelectedPurchaseDetail?.PurchaseOrder?.IsApproved ?? false,
        getItemName: item => item.SelectedProduct?.Name ?? "未知商品",
        documentTypeName: "採購單"
    );
}

// Razor 標記中使用
@{
    var approvalWarning = GetApprovalWarning();
}

@if (IsApprovalEnabled && approvalWarning.HasUnapprovedItems)
{
    <div class="alert alert-warning mb-3" role="alert">
        <div class="d-flex align-items-start">
            <i class="fas fa-exclamation-triangle me-2 mt-1"></i>
            <div>
                <strong>注意：</strong>@approvalWarning.WarningMessage
            </div>
        </div>
    </div>
}

// 驗證方法中使用
public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    // ... 其他驗證
    
    if (IsApprovalEnabled)
    {
        var (isValid, approvalErrors) = ApprovalCheckHelper.ValidateApproval(
            items: ReceivingItems,
            isEmptyRow: IsEmptyRow,
            isApproved: item => item.SelectedPurchaseDetail?.PurchaseOrder?.IsApproved ?? false,
            getItemName: item => $"{item.SelectedProduct?.Name} (採購單: {item.SelectedPurchaseDetail?.PurchaseOrder?.Code})",
            documentTypeName: "採購單"
        );
        
        if (!isValid)
        {
            errors.AddRange(approvalErrors);
        }
    }
    
    // ...
}
```

**優點**:
- ✅ 減少 20-30 行重複代碼
- ✅ 統一的警告訊息格式
- ✅ 驗證邏輯可重用
- ✅ 支援不同類型的單據審核檢查

#### 套用進度

- [ ] PurchaseReceivingTable.razor
- [ ] SalesOrderTable.razor
- [ ] MaterialIssueTable.razor

---

### 5. InventoryLocationHelper - 庫存倉位輔助類

**優先級**: 🟡 中  
**預估工作量**: 2-3 小時  
**影響範圍**: 3+ 個組件

#### 功能說明

統一處理倉庫庫位相關的操作，包括庫存數量載入、顯示格式化、庫位篩選等。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// SalesDeliveryTable.razor
private async Task LoadStockQuantityAsync(DeliveryItem item)
{
    if (!item.ProductId.HasValue || !item.WarehouseId.HasValue)
    {
        item.CurrentStockQuantity = null;
        return;
    }

    try
    {
        var stockQuantity = await InventoryStockService.GetStockQuantityAsync(
            item.ProductId.Value,
            item.WarehouseId.Value,
            item.WarehouseLocationId);

        item.CurrentStockQuantity = stockQuantity;
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入庫存數量失敗：{ex.Message}");
        item.CurrentStockQuantity = null;
    }
}

// 取得庫存數量的顯示樣式
private string GetStockQuantityBadgeClass(int? quantity)
{
    if (!quantity.HasValue) return "bg-secondary";
    
    if (quantity.Value == 0) return "bg-danger";
    if (quantity.Value < 10) return "bg-warning";
    return "bg-success";
}

// 格式化倉庫庫位顯示
private string FormatWarehouseLocationDisplay(DeliveryItem item)
{
    var warehouseName = item.WarehouseId.HasValue 
        ? Warehouses.FirstOrDefault(w => w.Id == item.WarehouseId)?.Name ?? "未知倉庫"
        : "-";
    
    var locationName = item.WarehouseLocationId.HasValue
        ? WarehouseLocations.FirstOrDefault(l => l.Id == item.WarehouseLocationId)?.Name ?? "預設位置"
        : "預設位置";
    
    var stockInfo = item.CurrentStockQuantity.HasValue
        ? $" (庫存: {item.CurrentStockQuantity.Value})"
        : "";
    
    return $"{warehouseName} - {locationName}{stockInfo}";
}

// 取得可用庫位
private List<WarehouseLocation> GetAvailableLocations(int? warehouseId)
{
    if (!warehouseId.HasValue || warehouseId.Value <= 0)
    {
        return new List<WarehouseLocation>();
    }
    
    return WarehouseLocations.Where(l => l.WarehouseId == warehouseId.Value).ToList();
}
```

**類似的代碼也出現在**:
- MaterialIssueTable.razor (倉庫庫位選擇和庫存顯示)
- PurchaseReceivingTable.razor (倉庫庫位管理)
- InventoryStockTable.razor (庫位篩選)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/InventoryLocationHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 庫存倉位輔助類
    /// 用於統一處理倉庫庫位相關的操作
    /// </summary>
    public static class InventoryLocationHelper
    {
        /// <summary>
        /// 載入庫存數量
        /// </summary>
        public static async Task<int?> LoadStockQuantityAsync(
            int? productId,
            int? warehouseId,
            int? warehouseLocationId,
            IInventoryStockService inventoryStockService,
            INotificationService? notificationService = null)
        {
            if (!productId.HasValue || !warehouseId.HasValue)
            {
                return null;
            }

            try
            {
                return await inventoryStockService.GetStockQuantityAsync(
                    productId.Value,
                    warehouseId.Value,
                    warehouseLocationId);
            }
            catch (Exception ex)
            {
                if (notificationService != null)
                {
                    await notificationService.ShowErrorAsync($"載入庫存數量失敗：{ex.Message}");
                }
                return null;
            }
        }
        
        /// <summary>
        /// 取得庫存數量徽章樣式類別
        /// </summary>
        public static string GetStockQuantityBadgeClass(int? quantity)
        {
            if (!quantity.HasValue) return "bg-secondary";
            
            if (quantity.Value == 0) return "bg-danger";
            if (quantity.Value < 10) return "bg-warning";
            return "bg-success";
        }
        
        /// <summary>
        /// 格式化倉庫庫位顯示文字
        /// </summary>
        public static string FormatWarehouseLocationDisplay<TWH, TLoc>(
            int? warehouseId,
            int? warehouseLocationId,
            List<TWH> warehouses,
            List<TLoc> locations,
            Func<TWH, int> getWarehouseId,
            Func<TWH, string> getWarehouseName,
            Func<TLoc, int> getLocationId,
            Func<TLoc, string> getLocationName,
            int? stockQuantity = null)
        {
            var warehouseName = warehouseId.HasValue 
                ? warehouses.FirstOrDefault(w => getWarehouseId(w) == warehouseId)?.Let(w => getWarehouseName(w)) ?? "未知倉庫"
                : "-";
            
            var locationName = warehouseLocationId.HasValue
                ? locations.FirstOrDefault(l => getLocationId(l) == warehouseLocationId)?.Let(l => getLocationName(l)) ?? "預設位置"
                : "預設位置";
            
            var stockInfo = stockQuantity.HasValue
                ? $" (庫存: {stockQuantity.Value})"
                : "";
            
            return $"{warehouseName} - {locationName}{stockInfo}";
        }
        
        /// <summary>
        /// 取得可用庫位清單
        /// </summary>
        public static List<TLocation> GetAvailableLocations<TLocation>(
            int? warehouseId,
            List<TLocation> allLocations,
            Func<TLocation, int> getWarehouseId)
        {
            if (!warehouseId.HasValue || warehouseId.Value <= 0)
            {
                return new List<TLocation>();
            }
            
            return allLocations.Where(l => getWarehouseId(l) == warehouseId.Value).ToList();
        }
        
        /// <summary>
        /// 產生庫存數量徽章 HTML
        /// </summary>
        public static string GetStockQuantityBadgeHtml(int? quantity)
        {
            if (!quantity.HasValue)
            {
                return "<span class='badge bg-secondary'>-</span>";
            }
            
            var badgeClass = GetStockQuantityBadgeClass(quantity);
            return $"<span class='badge {badgeClass}'>{quantity.Value}</span>";
        }
    }
    
    /// <summary>
    /// 擴展方法：Let (用於簡化 null 檢查)
    /// </summary>
    public static class ObjectExtensions
    {
        public static TResult? Let<T, TResult>(this T obj, Func<T, TResult> func)
            where T : class
            where TResult : class
        {
            return obj != null ? func(obj) : null;
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesDeliveryTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 大幅簡化庫存載入
private async Task LoadStockQuantityAsync(DeliveryItem item)
{
    item.CurrentStockQuantity = await InventoryLocationHelper.LoadStockQuantityAsync(
        productId: item.ProductId,
        warehouseId: item.WarehouseId,
        warehouseLocationId: item.WarehouseLocationId,
        inventoryStockService: InventoryStockService,
        notificationService: NotificationService
    );
}

// 簡化樣式取得
private string GetStockQuantityBadgeClass(int? quantity)
{
    return InventoryLocationHelper.GetStockQuantityBadgeClass(quantity);
}

// 簡化格式化顯示
private string FormatWarehouseLocationDisplay(DeliveryItem item)
{
    return InventoryLocationHelper.FormatWarehouseLocationDisplay(
        warehouseId: item.WarehouseId,
        warehouseLocationId: item.WarehouseLocationId,
        warehouses: Warehouses,
        locations: WarehouseLocations,
        getWarehouseId: w => w.Id,
        getWarehouseName: w => w.Name,
        getLocationId: l => l.Id,
        getLocationName: l => l.Name,
        stockQuantity: item.CurrentStockQuantity
    );
}

// 簡化庫位篩選
private List<WarehouseLocation> GetAvailableLocations(int? warehouseId)
{
    return InventoryLocationHelper.GetAvailableLocations(
        warehouseId: warehouseId,
        allLocations: WarehouseLocations,
        getWarehouseId: l => l.WarehouseId
    );
}

// 在 Razor 標記中使用
@{
    var badgeHtml = InventoryLocationHelper.GetStockQuantityBadgeHtml(item.CurrentStockQuantity);
}
@((MarkupString)badgeHtml)
```

**優點**:
- ✅ 減少 40-50 行重複代碼
- ✅ 統一的錯誤處理
- ✅ 一致的顯示格式
- ✅ 支援泛型，適用於不同的實體類型

#### 套用進度

- [ ] SalesDeliveryTable.razor
- [ ] MaterialIssueTable.razor
- [ ] PurchaseReceivingTable.razor
- [ ] InventoryStockTable.razor

---

### 6. PropertyAccessHelper - 屬性存取輔助類

**優先級**: 🟢 低  
**預估工作量**: 1-2 小時  
**影響範圍**: 3+ 個組件 (泛型組件)

#### 功能說明

統一處理動態屬性存取和型別轉換，主要用於泛型組件中的屬性操作。

#### 當前寫法

**重複出現在泛型組件中**:

```csharp
// InventoryStockTable.razor
private T? GetPropertyValue<T>(object obj, string propertyName)
{
    var property = obj.GetType().GetProperty(propertyName);
    if (property == null) return default(T);
    
    var value = property.GetValue(obj);
    if (value == null) return default(T);
    
    if (typeof(T) == typeof(object)) return (T)value;
    
    // 處理可空型別的轉換
    var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
    
    // 如果值的型別與目標型別相同或可直接轉換
    if (targetType.IsAssignableFrom(value.GetType()))
    {
        return (T)value;
    }
    
    try
    {
        return (T)Convert.ChangeType(value, targetType);
    }
    catch
    {
        return default(T);
    }
}

private void SetPropertyValue(object obj, string propertyName, object? value)
{
    var property = obj.GetType().GetProperty(propertyName);
    if (property != null && property.CanWrite)
    {
        if (value != null && property.PropertyType != value.GetType())
        {
            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            try
            {
                value = Convert.ChangeType(value, targetType);
            }
            catch
            {
                value = null;
            }
        }
        property.SetValue(obj, value);
    }
}
```

**類似的代碼也出現在**:
- SupplierProductTable.razor (透過 Getter/Setter 委派存取)
- ProductSupplierTable.razor (透過 Getter/Setter 委派存取)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/PropertyAccessHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 屬性存取輔助類
    /// 用於統一處理動態屬性存取和型別轉換
    /// </summary>
    public static class PropertyAccessHelper
    {
        /// <summary>
        /// 取得物件的屬性值 (支援型別轉換)
        /// </summary>
        public static T? GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return default(T);
            
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) return default(T);
            
            var value = property.GetValue(obj);
            if (value == null) return default(T);
            
            if (typeof(T) == typeof(object)) return (T)value;
            
            // 處理可空型別的轉換
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            
            // 如果值的型別與目標型別相同或可直接轉換
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return (T)value;
            }
            
            try
            {
                return (T)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default(T);
            }
        }
        
        /// <summary>
        /// 設定物件的屬性值 (支援型別轉換)
        /// </summary>
        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return;
            
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite)
                return;
            
            if (value != null && property.PropertyType != value.GetType())
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                try
                {
                    value = Convert.ChangeType(value, targetType);
                }
                catch
                {
                    value = null;
                }
            }
            
            property.SetValue(obj, value);
        }
        
        /// <summary>
        /// 檢查物件是否有指定的屬性
        /// </summary>
        public static bool HasProperty(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return false;
            
            return obj.GetType().GetProperty(propertyName) != null;
        }
        
        /// <summary>
        /// 取得屬性的型別
        /// </summary>
        public static Type? GetPropertyType(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return null;
            
            var property = obj.GetType().GetProperty(propertyName);
            return property?.PropertyType;
        }
        
        /// <summary>
        /// 批次取得多個屬性值
        /// </summary>
        public static Dictionary<string, object?> GetPropertyValues(object obj, params string[] propertyNames)
        {
            var result = new Dictionary<string, object?>();
            
            foreach (var propertyName in propertyNames)
            {
                result[propertyName] = GetPropertyValue<object>(obj, propertyName);
            }
            
            return result;
        }
        
        /// <summary>
        /// 批次設定多個屬性值
        /// </summary>
        public static void SetPropertyValues(object obj, Dictionary<string, object?> propertyValues)
        {
            foreach (var kvp in propertyValues)
            {
                SetPropertyValue(obj, kvp.Key, kvp.Value);
            }
        }
    }
}
```

#### 套用之後的寫法

**修改後 - InventoryStockTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 直接使用 Helper，不需要自己實作
private T? GetPropertyValue<T>(object obj, string propertyName)
{
    return PropertyAccessHelper.GetPropertyValue<T>(obj, propertyName);
}

private void SetPropertyValue(object obj, string propertyName, object? value)
{
    PropertyAccessHelper.SetPropertyValue(obj, propertyName, value);
}

// 新增的便利方法
private bool HasProperty(object obj, string propertyName)
{
    return PropertyAccessHelper.HasProperty(obj, propertyName);
}

// 批次操作範例
private void CopyPropertiesFromSource(TDetailEntity target, TDetailEntity source)
{
    var values = PropertyAccessHelper.GetPropertyValues(source, 
        WarehouseIdPropertyName, 
        WarehouseLocationIdPropertyName,
        CurrentStockPropertyName);
    
    PropertyAccessHelper.SetPropertyValues(target, values);
}
```

**優點**:
- ✅ 減少 30-40 行反射相關代碼
- ✅ 統一的錯誤處理和型別轉換邏輯
- ✅ 支援批次操作
- ✅ 更好的空值檢查

#### 套用進度

- [ ] InventoryStockTable.razor
- [ ] SupplierProductTable.razor (可選，已使用委派模式)
- [ ] ProductSupplierTable.razor (可選，已使用委派模式)

---

### 7. QuantityCalculationHelper - 數量計算輔助類

**優先級**: 🟡 中  
**預估工作量**: 2-3 小時  
**影響範圍**: 5+ 個組件

#### 功能說明

統一處理數量相關的計算邏輯，包括可退貨數量、剩餘數量、數量範圍驗證等。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// SalesReturnTable.razor
private int CalculateAvailableReturnQuantity(SalesItem item)
{
    if (item.SelectedOrderDetail == null) return 0;
    
    var originalQuantity = item.SelectedOrderDetail.Quantity;
    var alreadyReturned = _returnedQuantities.TryGetValue(item.SelectedOrderDetail.Id, out var returned) 
        ? returned 
        : 0;
    
    var availableQuantity = originalQuantity - alreadyReturned;
    return Math.Max(0, availableQuantity);
}

// 驗證數量範圍
private bool ValidateReturnQuantity(SalesItem item, out string errorMessage)
{
    var availableQty = CalculateAvailableReturnQuantity(item);
    
    if (item.ReturnQuantity <= 0)
    {
        errorMessage = "退貨數量必須大於 0";
        return false;
    }
    
    if (item.ReturnQuantity > availableQty)
    {
        errorMessage = $"退貨數量不可超過可退數量 ({availableQty})";
        return false;
    }
    
    errorMessage = string.Empty;
    return true;
}

// 載入已退數量
private async Task LoadReturnedQuantitiesAsync()
{
    _returnedQuantities.Clear();
    
    var detailIds = SelectedItems
        .Where(item => item.SelectedOrderDetail != null)
        .Select(item => item.SelectedOrderDetail!.Id)
        .Distinct()
        .ToList();
    
    if (!detailIds.Any()) return;
    
    foreach (var detailId in detailIds)
    {
        var returnedQty = await SalesReturnDetailService.GetTotalReturnedQuantityAsync(detailId);
        if (returnedQty > 0)
        {
            _returnedQuantities[detailId] = returnedQty;
        }
    }
}
```

**類似的代碼也出現在**:
- PurchaseReturnTable.razor (計算可退貨數量)
- SalesOrderTable.razor (計算已退數量)
- QuotationTable.razor (計算已轉單數量)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/QuantityCalculationHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 數量計算輔助類
    /// 用於統一處理數量相關的計算邏輯
    /// </summary>
    public static class QuantityCalculationHelper
    {
        /// <summary>
        /// 計算可退貨數量
        /// </summary>
        public static decimal CalculateAvailableReturnQuantity(decimal originalQty, decimal returnedQty)
        {
            var availableQty = originalQty - returnedQty;
            return Math.Max(0, availableQty);
        }
        
        /// <summary>
        /// 計算剩餘數量
        /// </summary>
        public static decimal CalculateRemainingQuantity(decimal totalQty, decimal usedQty)
        {
            var remainingQty = totalQty - usedQty;
            return Math.Max(0, remainingQty);
        }
        
        /// <summary>
        /// 驗證數量範圍
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateQuantityRange(
            decimal quantity,
            decimal minQty,
            decimal maxQty,
            string itemName = "數量")
        {
            if (quantity <= 0)
            {
                return (false, $"{itemName}必須大於 0");
            }
            
            if (quantity < minQty)
            {
                return (false, $"{itemName}不可小於 {minQty}");
            }
            
            if (quantity > maxQty)
            {
                return (false, $"{itemName}不可超過 {maxQty}");
            }
            
            return (true, string.Empty);
        }
        
        /// <summary>
        /// 從服務批次取得已退數量
        /// </summary>
        public static async Task<Dictionary<int, decimal>> GetReturnedQuantitiesAsync<TService>(
            List<int> detailIds,
            TService service,
            Func<TService, int, Task<decimal>> getReturnedQuantityFunc)
        {
            var result = new Dictionary<int, decimal>();
            
            if (!detailIds.Any()) return result;
            
            foreach (var detailId in detailIds)
            {
                var returnedQty = await getReturnedQuantityFunc(service, detailId);
                if (returnedQty > 0)
                {
                    result[detailId] = returnedQty;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 計算百分比 (避免除以零)
        /// </summary>
        public static decimal CalculatePercentage(decimal part, decimal total)
        {
            if (total == 0) return 0;
            return Math.Round((part / total) * 100, 2);
        }
        
        /// <summary>
        /// 驗證退貨數量 (整合可用數量檢查)
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateReturnQuantity(
            decimal returnQty,
            decimal originalQty,
            decimal alreadyReturnedQty,
            string itemName = "退貨數量")
        {
            var availableQty = CalculateAvailableReturnQuantity(originalQty, alreadyReturnedQty);
            
            if (returnQty <= 0)
            {
                return (false, $"{itemName}必須大於 0");
            }
            
            if (returnQty > availableQty)
            {
                return (false, $"{itemName}不可超過可退數量 ({availableQty})");
            }
            
            return (true, string.Empty);
        }
        
        /// <summary>
        /// 批次計算小計 (數量 × 單價)
        /// </summary>
        public static decimal CalculateSubtotal(decimal quantity, decimal unitPrice)
        {
            return quantity * unitPrice;
        }
        
        /// <summary>
        /// 批次計算小計 (數量 × 單價 × 折扣)
        /// </summary>
        public static decimal CalculateSubtotalWithDiscount(
            decimal quantity, 
            decimal unitPrice, 
            decimal discountPercentage)
        {
            var subtotal = quantity * unitPrice;
            var discount = subtotal * (discountPercentage / 100);
            return subtotal - discount;
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesReturnTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 簡化可退數量計算
private decimal CalculateAvailableReturnQuantity(SalesItem item)
{
    if (item.SelectedOrderDetail == null) return 0;
    
    var originalQty = item.SelectedOrderDetail.Quantity;
    var alreadyReturned = _returnedQuantities.TryGetValue(item.SelectedOrderDetail.Id, out var returned) 
        ? returned 
        : 0;
    
    return QuantityCalculationHelper.CalculateAvailableReturnQuantity(originalQty, alreadyReturned);
}

// 簡化驗證
private bool ValidateReturnQuantity(SalesItem item, out string errorMessage)
{
    if (item.SelectedOrderDetail == null)
    {
        errorMessage = "請選擇訂單明細";
        return false;
    }
    
    var originalQty = item.SelectedOrderDetail.Quantity;
    var alreadyReturned = _returnedQuantities.TryGetValue(item.SelectedOrderDetail.Id, out var returned) 
        ? returned 
        : 0;
    
    var (isValid, error) = QuantityCalculationHelper.ValidateReturnQuantity(
        returnQty: item.ReturnQuantity,
        originalQty: originalQty,
        alreadyReturnedQty: alreadyReturned,
        itemName: "退貨數量"
    );
    
    errorMessage = error;
    return isValid;
}

// 簡化載入已退數量
private async Task LoadReturnedQuantitiesAsync()
{
    var detailIds = SelectedItems
        .Where(item => item.SelectedOrderDetail != null)
        .Select(item => item.SelectedOrderDetail!.Id)
        .Distinct()
        .ToList();
    
    _returnedQuantities = await QuantityCalculationHelper.GetReturnedQuantitiesAsync(
        detailIds: detailIds,
        service: SalesReturnDetailService,
        getReturnedQuantityFunc: async (service, detailId) => 
            await service.GetTotalReturnedQuantityAsync(detailId)
    );
}
```

**優點**:
- ✅ 減少 20-30 行計算相關代碼
- ✅ 統一的計算邏輯和驗證
- ✅ 避免重複的數學運算
- ✅ 更容易單元測試

#### 套用進度

- [ ] SalesReturnTable.razor
- [ ] PurchaseReturnTable.razor
- [ ] SalesOrderTable.razor
- [ ] QuotationTable.razor
- [ ] PurchaseReceivingTable.razor

---

### 8. ValidationMessageHelper - 驗證訊息輔助類

**優先級**: 🟢 低  
**預估工作量**: 1-2 小時  
**影響範圍**: 所有 Table 組件

#### 功能說明

統一處理驗證錯誤訊息的建立和顯示，提供一致的驗證訊息格式。

#### 當前寫法

**重複出現在所有檔案中**:

```csharp
// 各個 Table 的 ValidateAsync 方法
public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    // 檢查是否有明細
    if (!AutoEmptyRowHelper.ForAny<SalesItem>.HasSufficientItems(SalesItems, IsEmptyRow, 1))
    {
        errors.Add("至少需要一筆銷貨明細");
    }
    
    // 檢查必填欄位
    var itemsWithoutProduct = SalesItems
        .Where(item => !IsEmptyRow(item) && item.ProductId == null)
        .ToList();
    
    if (itemsWithoutProduct.Any())
    {
        errors.Add("所有明細都必須選擇商品");
    }
    
    // 檢查數量範圍
    var invalidQuantities = SalesItems
        .Where(item => !IsEmptyRow(item) && item.Quantity <= 0)
        .ToList();
    
    if (invalidQuantities.Any())
    {
        errors.Add("所有明細的數量必須大於 0");
    }
    
    // 檢查重複
    var duplicateProducts = SalesItems
        .Where(item => !IsEmptyRow(item))
        .GroupBy(item => item.ProductId)
        .Where(g => g.Count() > 1)
        .ToList();
    
    if (duplicateProducts.Any())
    {
        errors.Add("存在重複的商品");
    }
    
    if (errors.Any())
    {
        var errorMessage = string.Join("\n", errors);
        await NotificationService.ShowErrorAsync(errorMessage);
        return false;
    }
    
    return true;
}
```

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/ValidationMessageHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 驗證訊息輔助類
    /// 用於統一處理驗證錯誤訊息的建立和顯示
    /// </summary>
    public static class ValidationMessageHelper
    {
        /// <summary>
        /// 建立驗證錯誤訊息
        /// </summary>
        public static string BuildValidationMessage(List<string> errors)
        {
            if (!errors.Any()) return string.Empty;
            
            return string.Join("\n", errors.Select((error, index) => $"{index + 1}. {error}"));
        }
        
        /// <summary>
        /// 新增必填欄位錯誤
        /// </summary>
        public static void AddRequiredFieldError(
            List<string> errors,
            string fieldName,
            bool hasError)
        {
            if (hasError)
            {
                errors.Add($"所有明細都必須填寫{fieldName}");
            }
        }
        
        /// <summary>
        /// 新增範圍錯誤
        /// </summary>
        public static void AddRangeError(
            List<string> errors,
            string fieldName,
            decimal? minValue,
            decimal? maxValue,
            bool hasError)
        {
            if (!hasError) return;
            
            if (minValue.HasValue && maxValue.HasValue)
            {
                errors.Add($"{fieldName}必須介於 {minValue} 到 {maxValue} 之間");
            }
            else if (minValue.HasValue)
            {
                errors.Add($"{fieldName}必須大於或等於 {minValue}");
            }
            else if (maxValue.HasValue)
            {
                errors.Add($"{fieldName}必須小於或等於 {maxValue}");
            }
        }
        
        /// <summary>
        /// 新增重複錯誤
        /// </summary>
        public static void AddDuplicateError(
            List<string> errors,
            string fieldName,
            bool hasError)
        {
            if (hasError)
            {
                errors.Add($"存在重複的{fieldName}");
            }
        }
        
        /// <summary>
        /// 新增最小項目數錯誤
        /// </summary>
        public static void AddMinimumItemsError(
            List<string> errors,
            string itemName,
            int minimumCount,
            bool hasError)
        {
            if (hasError)
            {
                errors.Add($"至少需要 {minimumCount} 筆{itemName}");
            }
        }
        
        /// <summary>
        /// 顯示驗證錯誤
        /// </summary>
        public static async Task<bool> ShowValidationErrorsAsync(
            List<string> errors,
            INotificationService notificationService)
        {
            if (!errors.Any()) return true;
            
            var errorMessage = BuildValidationMessage(errors);
            await notificationService.ShowErrorAsync(errorMessage, "驗證失敗");
            return false;
        }
        
        /// <summary>
        /// 驗證並顯示錯誤 (整合方法)
        /// </summary>
        public static async Task<bool> ValidateAndShowAsync(
            List<string> errors,
            INotificationService notificationService)
        {
            return await ShowValidationErrorsAsync(errors, notificationService);
        }
        
        /// <summary>
        /// 建立詳細的驗證訊息 (包含項目清單)
        /// </summary>
        public static string BuildDetailedValidationMessage(
            string errorTitle,
            List<string> itemNames)
        {
            if (!itemNames.Any()) return string.Empty;
            
            var items = string.Join("\n", itemNames.Select((name, index) => $"  • {name}"));
            return $"{errorTitle}：\n{items}";
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesOrderTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    // 使用 Helper 新增各種錯誤
    ValidationMessageHelper.AddMinimumItemsError(
        errors,
        itemName: "銷貨明細",
        minimumCount: 1,
        hasError: !AutoEmptyRowHelper.ForAny<SalesItem>.HasSufficientItems(SalesItems, IsEmptyRow, 1)
    );
    
    ValidationMessageHelper.AddRequiredFieldError(
        errors,
        fieldName: "商品",
        hasError: SalesItems.Any(item => !IsEmptyRow(item) && item.ProductId == null)
    );
    
    ValidationMessageHelper.AddRangeError(
        errors,
        fieldName: "數量",
        minValue: 1,
        maxValue: null,
        hasError: SalesItems.Any(item => !IsEmptyRow(item) && item.Quantity <= 0)
    );
    
    ValidationMessageHelper.AddDuplicateError(
        errors,
        fieldName: "商品",
        hasError: SalesItems
            .Where(item => !IsEmptyRow(item))
            .GroupBy(item => item.ProductId)
            .Any(g => g.Count() > 1)
    );
    
    // 統一顯示錯誤
    return await ValidationMessageHelper.ValidateAndShowAsync(errors, NotificationService);
}
```

**優點**:
- ✅ 減少 10-20 行訊息相關代碼
- ✅ 統一的錯誤訊息格式
- ✅ 更容易維護和國際化
- ✅ 程式碼更清晰易讀

#### 套用進度

- [ ] 所有 Table 組件 (34 個檔案)

---

### 9. DuplicateCheckHelper - 重複檢查輔助類

**優先級**: 🟢 低  
**預估工作量**: 1-2 小時  
**影響範圍**: 4+ 個組件

#### 功能說明

統一處理重複項目的檢查邏輯，支援泛型和自訂鍵值提取。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// ProductSupplierTable.razor
public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    if (!AutoEmptyRowHelper.For<TProductSupplierEntity>.HasSufficientItems(Items, IsEmptyRow, 1))
    {
        errors.Add("至少需要一個廠商");
    }
    else
    {
        // 取得非空的項目進行重複檢查
        var nonEmptyItems = AutoEmptyRowHelper.For<TProductSupplierEntity>.GetNonEmptyItems(Items, IsEmptyRow);
        var supplierIds = nonEmptyItems.Select(item => GetSupplierId(item)).Where(id => id.HasValue).ToList();
        if (supplierIds.Count != supplierIds.Distinct().Count())
        {
            errors.Add("存在重複的廠商");
        }
    }
      
    if (errors.Any())
    {
        var errorMessage = string.Join("\n", errors);
        await NotificationService.ShowErrorAsync(errorMessage);
        return false;
    }
    
    return true;
}
```

**類似的代碼也出現在**:
- SupplierProductTable.razor (檢查重複商品)
- ProductCompositionTable.razor (檢查重複材料)
- InventoryStockTable.razor (檢查重複倉庫庫位組合)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/DuplicateCheckHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 重複檢查結果
    /// </summary>
    public class DuplicateCheckResult<T>
    {
        public bool HasDuplicates { get; set; }
        public List<IGrouping<object, T>> DuplicateGroups { get; set; } = new();
        public List<string> DuplicateInfo { get; set; } = new();
    }
    
    /// <summary>
    /// 重複檢查輔助類
    /// 用於統一處理重複項目的檢查邏輯
    /// </summary>
    public static class DuplicateCheckHelper
    {
        /// <summary>
        /// 檢查是否有重複項目
        /// </summary>
        public static bool HasDuplicates<T, TKey>(
            List<T> items,
            Func<T, TKey> getKeyFunc)
        {
            var keys = items.Select(getKeyFunc).Where(k => k != null).ToList();
            return keys.Count != keys.Distinct().Count();
        }
        
        /// <summary>
        /// 取得重複群組
        /// </summary>
        public static List<IGrouping<TKey, T>> GetDuplicateGroups<T, TKey>(
            List<T> items,
            Func<T, TKey> getKeyFunc) where TKey : notnull
        {
            return items
                .Where(item => getKeyFunc(item) != null)
                .GroupBy(getKeyFunc)
                .Where(g => g.Count() > 1)
                .ToList();
        }
        
        /// <summary>
        /// 檢查重複並取得詳細資訊
        /// </summary>
        public static DuplicateCheckResult<T> CheckDuplicates<T, TKey>(
            List<T> items,
            Func<T, TKey> getKeyFunc,
            Func<T, string> getDisplayNameFunc) where TKey : notnull
        {
            var result = new DuplicateCheckResult<T>();
            
            var duplicateGroups = GetDuplicateGroups(items, getKeyFunc);
            
            result.HasDuplicates = duplicateGroups.Any();
            result.DuplicateGroups = duplicateGroups.Cast<IGrouping<object, T>>().ToList();
            
            foreach (var group in duplicateGroups)
            {
                var displayNames = group.Select(getDisplayNameFunc).ToList();
                result.DuplicateInfo.Add($"{string.Join(", ", displayNames)} (共 {group.Count()} 筆)");
            }
            
            return result;
        }
        
        /// <summary>
        /// 顯示重複警告
        /// </summary>
        public static async Task ShowDuplicateWarningAsync(
            List<string> duplicateInfo,
            string itemTypeName,
            INotificationService notificationService)
        {
            if (!duplicateInfo.Any()) return;
            
            var message = $"發現重複的{itemTypeName}：\n" + string.Join("\n", duplicateInfo.Select(info => $"• {info}"));
            await notificationService.ShowWarningAsync(message, "重複項目");
        }
        
        /// <summary>
        /// 檢查複合鍵重複 (例如：倉庫+庫位)
        /// </summary>
        public static bool HasDuplicatesWithCompositeKey<T>(
            List<T> items,
            params Func<T, object>[] getKeyFuncs)
        {
            var keys = items.Select(item => 
                string.Join("|", getKeyFuncs.Select(func => func(item)?.ToString() ?? ""))
            ).ToList();
            
            return keys.Count != keys.Distinct().Count();
        }
        
        /// <summary>
        /// 取得重複項目的索引
        /// </summary>
        public static List<int> GetDuplicateIndices<T, TKey>(
            List<T> items,
            Func<T, TKey> getKeyFunc) where TKey : notnull
        {
            var duplicateKeys = items
                .Where(item => getKeyFunc(item) != null)
                .GroupBy(getKeyFunc)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();
            
            var indices = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                var key = getKeyFunc(items[i]);
                if (key != null && duplicateKeys.Contains(key))
                {
                    indices.Add(i);
                }
            }
            
            return indices;
        }
    }
}
```

#### 套用之後的寫法

**修改後 - ProductSupplierTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    ValidationMessageHelper.AddMinimumItemsError(
        errors,
        itemName: "廠商",
        minimumCount: 1,
        hasError: !AutoEmptyRowHelper.For<TProductSupplierEntity>.HasSufficientItems(Items, IsEmptyRow, 1)
    );
    
    // 使用 Helper 檢查重複
    var nonEmptyItems = AutoEmptyRowHelper.For<TProductSupplierEntity>.GetNonEmptyItems(Items, IsEmptyRow);
    
    ValidationMessageHelper.AddDuplicateError(
        errors,
        fieldName: "廠商",
        hasError: DuplicateCheckHelper.HasDuplicates(
            nonEmptyItems,
            item => GetSupplierId(item)
        )
    );
    
    return await ValidationMessageHelper.ValidateAndShowAsync(errors, NotificationService);
}

// 或者取得詳細的重複資訊
public async Task ShowDuplicateWarningAsync()
{
    var nonEmptyItems = AutoEmptyRowHelper.For<TProductSupplierEntity>.GetNonEmptyItems(Items, IsEmptyRow);
    
    var duplicateCheck = DuplicateCheckHelper.CheckDuplicates(
        items: nonEmptyItems,
        getKeyFunc: item => GetSupplierId(item),
        getDisplayNameFunc: item => GetSupplierDisplayText(item)
    );
    
    if (duplicateCheck.HasDuplicates)
    {
        await DuplicateCheckHelper.ShowDuplicateWarningAsync(
            duplicateCheck.DuplicateInfo,
            itemTypeName: "廠商",
            notificationService: NotificationService
        );
    }
}
```

**修改後 - InventoryStockTable.razor (複合鍵範例)**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    var nonEmptyItems = StockDetailItems.Where(item => !IsEmptyRow(item)).ToList();
    
    // 檢查倉庫+庫位的複合鍵重複
    ValidationMessageHelper.AddDuplicateError(
        errors,
        fieldName: "倉庫與庫位組合",
        hasError: DuplicateCheckHelper.HasDuplicatesWithCompositeKey(
            nonEmptyItems,
            item => item.SelectedWarehouseId,
            item => item.SelectedWarehouseLocationId
        )
    );
    
    return await ValidationMessageHelper.ValidateAndShowAsync(errors, NotificationService);
}
```

**優點**:
- ✅ 減少 10-20 行重複檢查代碼
- ✅ 支援單一鍵和複合鍵
- ✅ 提供詳細的重複資訊
- ✅ 統一的檢查邏輯

#### 套用進度

- [ ] ProductSupplierTable.razor
- [ ] SupplierProductTable.razor
- [ ] ProductCompositionTable.razor
- [ ] InventoryStockTable.razor

---

### 10. SmartLoadingHelper - 智能載入輔助類

**優先級**: 🟡 中  
**預估工作量**: 2-3 小時  
**影響範圍**: 2 個組件

#### 功能說明

統一處理智能載入邏輯，如載入最後完整訂單、從來源單據載入等。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// PurchaseOrderTable.razor
private async Task LoadLastPurchaseDetails()
{
    if (!SelectedSupplierId.HasValue || SelectedSupplierId.Value <= 0)
    {
        await NotificationService.ShowWarningAsync("請先選擇廠商", "提示");
        return;
    }

    try
    {
        var lastPurchaseOrder = await PurchaseOrderService.GetLastCompletePurchaseOrderAsync(SelectedSupplierId.Value);
        
        if (lastPurchaseOrder == null || !lastPurchaseOrder.PurchaseOrderDetails.Any())
        {
            await NotificationService.ShowInfoAsync("此廠商沒有完整的採購記錄", "提示");
            return;
        }

        var confirmed = await JSRuntime.InvokeAsync<bool>(
            "confirm",
            $"是否載入此廠商的最後一次完整採購單？\n單號：{lastPurchaseOrder.Code}\n日期：{lastPurchaseOrder.OrderDate:yyyy-MM-dd}\n明細數：{lastPurchaseOrder.PurchaseOrderDetails.Count} 項");

        if (!confirmed) return;

        // 清空現有明細
        ProductItems.Clear();

        // 載入明細
        foreach (var detail in lastPurchaseOrder.PurchaseOrderDetails)
        {
            var product = await ProductService.GetByIdAsync(detail.ProductId);
            if (product == null) continue;

            var item = new ProductItem
            {
                ProductId = detail.ProductId,
                SelectedProduct = product,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                // ... 其他欄位
            };

            ProductItems.Add(item);
        }

        EnsureOneEmptyRow();
        await NotifyDetailsChanged();
        await NotificationService.ShowSuccessAsync($"已載入 {ProductItems.Count} 項商品");
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入失敗：{ex.Message}");
    }
}
```

**類似的代碼也出現在**:
- SalesOrderTable.razor (載入智能下單、載入報價單)

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/SmartLoadingHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 智能載入結果
    /// </summary>
    public class SmartLoadingResult<T>
    {
        public bool IsSuccess { get; set; }
        public List<T> LoadedItems { get; set; } = new();
        public string SourceDocumentCode { get; set; } = string.Empty;
        public DateTime? SourceDocumentDate { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// 智能載入輔助類
    /// 用於統一處理智能載入邏輯
    /// </summary>
    public static class SmartLoadingHelper
    {
        /// <summary>
        /// 載入最後完整訂單
        /// </summary>
        public static async Task<SmartLoadingResult<TItem>> LoadLastCompleteOrderAsync<TSource, TItem>(
            int? partyId,
            Func<int, Task<TSource?>> getLastOrderFunc,
            Func<TSource, List<TItem>> convertToItemsFunc,
            Func<TSource, string> getDocumentCodeFunc,
            Func<TSource, DateTime> getDocumentDateFunc,
            Func<TSource, int> getDetailCountFunc,
            IJSRuntime jsRuntime,
            INotificationService notificationService,
            string partyTypeName = "對象",
            string documentTypeName = "單據")
        {
            var result = new SmartLoadingResult<TItem>();
            
            if (!partyId.HasValue || partyId.Value <= 0)
            {
                result.ErrorMessage = $"請先選擇{partyTypeName}";
                await notificationService.ShowWarningAsync(result.ErrorMessage, "提示");
                return result;
            }

            try
            {
                var lastOrder = await getLastOrderFunc(partyId.Value);
                
                if (lastOrder == null || getDetailCountFunc(lastOrder) == 0)
                {
                    result.ErrorMessage = $"此{partyTypeName}沒有完整的{documentTypeName}記錄";
                    await notificationService.ShowInfoAsync(result.ErrorMessage, "提示");
                    return result;
                }

                var documentCode = getDocumentCodeFunc(lastOrder);
                var documentDate = getDocumentDateFunc(lastOrder);
                var detailCount = getDetailCountFunc(lastOrder);

                var confirmed = await jsRuntime.InvokeAsync<bool>(
                    "confirm",
                    $"是否載入此{partyTypeName}的最後一次完整{documentTypeName}？\n單號：{documentCode}\n日期：{documentDate:yyyy-MM-dd}\n明細數：{detailCount} 項");

                if (!confirmed)
                {
                    result.ErrorMessage = "使用者取消";
                    return result;
                }

                result.LoadedItems = convertToItemsFunc(lastOrder);
                result.SourceDocumentCode = documentCode;
                result.SourceDocumentDate = documentDate;
                result.IsSuccess = true;

                await notificationService.ShowSuccessAsync($"已載入 {result.LoadedItems.Count} 項明細");
                
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                await notificationService.ShowErrorAsync($"載入失敗：{ex.Message}");
                return result;
            }
        }
        
        /// <summary>
        /// 從來源單據載入
        /// </summary>
        public static async Task<SmartLoadingResult<TItem>> LoadFromSourceDocumentAsync<TSource, TItem>(
            int? sourceId,
            Func<int, Task<TSource?>> getSourceFunc,
            Func<TSource, List<TItem>> convertToItemsFunc,
            Func<TSource, string> getDocumentCodeFunc,
            Func<TSource, bool> validateSourceFunc,
            IJSRuntime jsRuntime,
            INotificationService notificationService,
            string documentTypeName = "單據",
            string? confirmMessage = null)
        {
            var result = new SmartLoadingResult<TItem>();
            
            if (!sourceId.HasValue || sourceId.Value <= 0)
            {
                result.ErrorMessage = $"請先選擇{documentTypeName}";
                await notificationService.ShowWarningAsync(result.ErrorMessage, "提示");
                return result;
            }

            try
            {
                var source = await getSourceFunc(sourceId.Value);
                
                if (source == null)
                {
                    result.ErrorMessage = $"{documentTypeName}不存在";
                    await notificationService.ShowErrorAsync(result.ErrorMessage);
                    return result;
                }
                
                if (!validateSourceFunc(source))
                {
                    result.ErrorMessage = $"{documentTypeName}狀態不符合載入條件";
                    await notificationService.ShowWarningAsync(result.ErrorMessage);
                    return result;
                }

                var documentCode = getDocumentCodeFunc(source);
                
                var message = confirmMessage ?? $"是否載入{documentTypeName} {documentCode} 的明細？";
                var confirmed = await jsRuntime.InvokeAsync<bool>("confirm", message);

                if (!confirmed)
                {
                    result.ErrorMessage = "使用者取消";
                    return result;
                }

                result.LoadedItems = convertToItemsFunc(source);
                result.SourceDocumentCode = documentCode;
                result.IsSuccess = true;

                await notificationService.ShowSuccessAsync($"已載入 {result.LoadedItems.Count} 項明細");
                
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                await notificationService.ShowErrorAsync($"載入失敗：{ex.Message}");
                return result;
            }
        }
    }
}
```

#### 套用之後的寫法

**修改後 - PurchaseOrderTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper
@inject IJSRuntime JSRuntime

private async Task LoadLastPurchaseDetails()
{
    var result = await SmartLoadingHelper.LoadLastCompleteOrderAsync(
        partyId: SelectedSupplierId,
        getLastOrderFunc: async (supplierId) => 
            await PurchaseOrderService.GetLastCompletePurchaseOrderAsync(supplierId),
        convertToItemsFunc: (order) => order.PurchaseOrderDetails.Select(detail => new ProductItem
        {
            ProductId = detail.ProductId,
            SelectedProduct = Products.FirstOrDefault(p => p.Id == detail.ProductId),
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            // ... 其他欄位
        }).ToList(),
        getDocumentCodeFunc: (order) => order.Code,
        getDocumentDateFunc: (order) => order.OrderDate,
        getDetailCountFunc: (order) => order.PurchaseOrderDetails.Count,
        jsRuntime: JSRuntime,
        notificationService: NotificationService,
        partyTypeName: "廠商",
        documentTypeName: "採購單"
    );

    if (result.IsSuccess)
    {
        ProductItems.Clear();
        ProductItems.AddRange(result.LoadedItems);
        EnsureOneEmptyRow();
        await NotifyDetailsChanged();
    }
}
```

**修改後 - SalesOrderTable.razor (從報價單載入)**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper
@inject IJSRuntime JSRuntime

private async Task LoadQuotationDetails()
{
    var result = await SmartLoadingHelper.LoadFromSourceDocumentAsync(
        sourceId: SelectedQuotationId,
        getSourceFunc: async (quotationId) => 
            await QuotationService.GetByIdWithDetailsAsync(quotationId),
        convertToItemsFunc: (quotation) => quotation.QuotationDetails
            .Where(detail => detail.ConvertedQuantity < detail.Quantity) // 只載入未完全轉單的
            .Select(detail => new SalesItem
            {
                ProductId = detail.ProductId,
                SelectedProduct = Products.FirstOrDefault(p => p.Id == detail.ProductId),
                Quantity = detail.Quantity - detail.ConvertedQuantity, // 待轉數量
                UnitPrice = detail.UnitPrice,
                DiscountPercentage = detail.DiscountPercentage,
                // ... 其他欄位
            }).ToList(),
        getDocumentCodeFunc: (quotation) => quotation.Code,
        validateSourceFunc: (quotation) => quotation.IsApproved, // 必須已審核
        jsRuntime: JSRuntime,
        notificationService: NotificationService,
        documentTypeName: "報價單"
    );

    if (result.IsSuccess)
    {
        SalesItems.Clear();
        SalesItems.AddRange(result.LoadedItems);
        EnsureOneEmptyRow();
        await NotifyDetailsChanged();
    }
}
```

**優點**:
- ✅ 減少 50-70 行載入相關代碼
- ✅ 統一的確認對話框和錯誤處理
- ✅ 可重用的載入模式
- ✅ 更清晰的業務邏輯

#### 套用進度

- [ ] PurchaseOrderTable.razor
- [ ] SalesOrderTable.razor

---

### 11. SearchableSelectHelper - 可搜尋下拉選單輔助類

**優先級**: 🟢 低  
**預估工作量**: 1-2 小時  
**影響範圍**: 所有包含可搜尋下拉選單的組件

#### 功能說明

統一處理可搜尋下拉選單的過濾和鍵盤導航邏輯。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// 各個 Table 的商品/廠商/客戶下拉選單
private List<Product> FilteredProducts { get; set; } = new();
private string _productSearchText = string.Empty;

private void OnProductSearch(string searchText)
{
    _productSearchText = searchText;
    
    if (string.IsNullOrWhiteSpace(searchText))
    {
        FilteredProducts = Products;
        return;
    }
    
    var lowerSearchText = searchText.ToLower();
    FilteredProducts = Products
        .Where(p => 
            p.Code.ToLower().Contains(lowerSearchText) ||
            p.Name.ToLower().Contains(lowerSearchText) ||
            p.ChineseName.ToLower().Contains(lowerSearchText))
        .ToList();
}

private void OnProductKeyDown(KeyboardEventArgs e)
{
    if (e.Key == "Enter" && FilteredProducts.Count == 1)
    {
        // 自動選擇唯一結果
        CurrentItem.ProductId = FilteredProducts[0].Id;
        CurrentItem.SelectedProduct = FilteredProducts[0];
    }
}
```

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/SearchableSelectHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 可搜尋下拉選單輔助類
    /// 用於統一處理下拉選單的過濾和鍵盤導航邏輯
    /// </summary>
    public static class SearchableSelectHelper
    {
        /// <summary>
        /// 過濾項目 (支援多個屬性)
        /// </summary>
        public static List<T> FilterItems<T>(
            List<T> sourceItems,
            string searchText,
            params Func<T, string>[] getPropertiesFuncs)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return sourceItems;
            }
            
            var lowerSearchText = searchText.ToLower();
            
            return sourceItems
                .Where(item => getPropertiesFuncs.Any(func => 
                {
                    var value = func(item);
                    return !string.IsNullOrEmpty(value) && value.ToLower().Contains(lowerSearchText);
                }))
                .ToList();
        }
        
        /// <summary>
        /// 處理鍵盤事件
        /// </summary>
        public static bool HandleKeyboardNavigation<T>(
            KeyboardEventArgs e,
            List<T> filteredItems,
            out T? selectedItem)
        {
            selectedItem = default;
            
            if (e.Key == "Enter" && filteredItems.Count == 1)
            {
                selectedItem = filteredItems[0];
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 過濾項目 (支援自訂比對邏輯)
        /// </summary>
        public static List<T> FilterItems<T>(
            List<T> sourceItems,
            string searchText,
            Func<T, string, bool> matchFunc)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return sourceItems;
            }
            
            return sourceItems.Where(item => matchFunc(item, searchText)).ToList();
        }
        
        /// <summary>
        /// 建立標準的商品過濾邏輯
        /// </summary>
        public static List<T> FilterProducts<T>(
            List<T> products,
            string searchText,
            Func<T, string> getCodeFunc,
            Func<T, string> getNameFunc,
            Func<T, string> getChineseNameFunc)
        {
            return FilterItems(
                products,
                searchText,
                getCodeFunc,
                getNameFunc,
                getChineseNameFunc
            );
        }
        
        /// <summary>
        /// 建立標準的客戶/廠商過濾邏輯
        /// </summary>
        public static List<T> FilterParties<T>(
            List<T> parties,
            string searchText,
            Func<T, string> getCodeFunc,
            Func<T, string> getNameFunc)
        {
            return FilterItems(
                parties,
                searchText,
                getCodeFunc,
                getNameFunc
            );
        }
        
        /// <summary>
        /// 高亮顯示搜尋文字
        /// </summary>
        public static string HighlightSearchText(string text, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
            
            var index = text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return text;
            }
            
            var before = text.Substring(0, index);
            var match = text.Substring(index, searchText.Length);
            var after = text.Substring(index + searchText.Length);
            
            return $"{before}<mark>{match}</mark>{after}";
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesOrderTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private List<Product> FilteredProducts { get; set; } = new();
private string _productSearchText = string.Empty;

private void OnProductSearch(string searchText)
{
    _productSearchText = searchText;
    
    FilteredProducts = SearchableSelectHelper.FilterProducts(
        products: Products,
        searchText: searchText,
        getCodeFunc: p => p.Code,
        getNameFunc: p => p.Name,
        getChineseNameFunc: p => p.ChineseName
    );
}

private void OnProductKeyDown(KeyboardEventArgs e)
{
    if (SearchableSelectHelper.HandleKeyboardNavigation(e, FilteredProducts, out Product? selectedProduct) 
        && selectedProduct != null)
    {
        CurrentItem.ProductId = selectedProduct.Id;
        CurrentItem.SelectedProduct = selectedProduct;
    }
}

// 可選：在顯示時高亮搜尋文字
private string GetProductDisplayText(Product product)
{
    var displayText = $"{product.Code} - {product.Name}";
    return SearchableSelectHelper.HighlightSearchText(displayText, _productSearchText);
}
```

**修改後 - PurchaseOrderTable.razor (廠商過濾)**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private List<Supplier> FilteredSuppliers { get; set; } = new();

private void OnSupplierSearch(string searchText)
{
    FilteredSuppliers = SearchableSelectHelper.FilterParties(
        parties: Suppliers,
        searchText: searchText,
        getCodeFunc: s => s.Code,
        getNameFunc: s => s.Name
    );
}

private void OnSupplierKeyDown(KeyboardEventArgs e)
{
    if (SearchableSelectHelper.HandleKeyboardNavigation(e, FilteredSuppliers, out Supplier? selectedSupplier)
        && selectedSupplier != null)
    {
        SelectedSupplierId = selectedSupplier.Id;
        SelectedSupplier = selectedSupplier;
    }
}
```

**優點**:
- ✅ 減少 10-15 行過濾代碼
- ✅ 統一的搜尋邏輯
- ✅ 支援鍵盤導航
- ✅ 可選的高亮顯示功能

#### 套用進度

- [ ] 所有包含可搜尋下拉選單的 Table 組件

---

### 12. DiscountHelper - 折扣計算輔助類

**優先級**: 🟡 中  
**預估工作量**: 1-2 小時  
**影響範圍**: 5+ 個組件

#### 功能說明

統一處理折扣相關的計算和驗證邏輯，包括折扣百分比驗證、折扣後金額計算等。

#### 當前寫法

**重複出現在多個檔案中**:

```csharp
// SalesOrderTable.razor, QuotationTable.razor 等
private bool ValidateDiscountPercentage(decimal discountPercentage, out string errorMessage)
{
    if (discountPercentage < 0)
    {
        errorMessage = "折扣不可小於 0%";
        return false;
    }
    
    if (discountPercentage > 100)
    {
        errorMessage = "折扣不可大於 100%";
        return false;
    }
    
    errorMessage = string.Empty;
    return true;
}

private decimal CalculateDiscountedPrice(decimal quantity, decimal unitPrice, decimal discountPercentage)
{
    var subtotal = quantity * unitPrice;
    var discountAmount = subtotal * (discountPercentage / 100);
    return subtotal - discountAmount;
}

private decimal CalculateDiscountAmount(decimal subtotal, decimal discountPercentage)
{
    return subtotal * (discountPercentage / 100);
}

// 計算明細總金額時
private void CalculateTotals()
{
    decimal total = 0;
    
    foreach (var item in SalesItems.Where(i => !IsEmptyRow(i)))
    {
        var subtotal = item.Quantity * item.UnitPrice;
        var discountAmount = subtotal * (item.DiscountPercentage / 100);
        var itemTotal = subtotal - discountAmount;
        total += itemTotal;
    }
    
    TotalAmount = total;
}
```

#### Helper 的寫法

**檔案位置**: `Helpers/InteractiveTableComponentHelper/DiscountHelper.cs`

```csharp
namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 折扣計算輔助類
    /// 用於統一處理折扣相關的計算和驗證邏輯
    /// </summary>
    public static class DiscountHelper
    {
        /// <summary>
        /// 驗證折扣百分比
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateDiscountPercentage(
            decimal discountPercentage,
            decimal minDiscount = 0,
            decimal maxDiscount = 100)
        {
            if (discountPercentage < minDiscount)
            {
                return (false, $"折扣不可小於 {minDiscount}%");
            }
            
            if (discountPercentage > maxDiscount)
            {
                return (false, $"折扣不可大於 {maxDiscount}%");
            }
            
            return (true, string.Empty);
        }
        
        /// <summary>
        /// 計算折扣金額
        /// </summary>
        public static decimal CalculateDiscountAmount(decimal subtotal, decimal discountPercentage)
        {
            return Math.Round(subtotal * (discountPercentage / 100), 2);
        }
        
        /// <summary>
        /// 計算折扣後金額
        /// </summary>
        public static decimal CalculateDiscountedPrice(decimal subtotal, decimal discountPercentage)
        {
            var discountAmount = CalculateDiscountAmount(subtotal, discountPercentage);
            return subtotal - discountAmount;
        }
        
        /// <summary>
        /// 計算折扣後金額 (從數量和單價)
        /// </summary>
        public static decimal CalculateDiscountedPrice(
            decimal quantity,
            decimal unitPrice,
            decimal discountPercentage)
        {
            var subtotal = quantity * unitPrice;
            return CalculateDiscountedPrice(subtotal, discountPercentage);
        }
        
        /// <summary>
        /// 計算實際折扣百分比 (從原價和折後價)
        /// </summary>
        public static decimal CalculateActualDiscountPercentage(decimal originalPrice, decimal discountedPrice)
        {
            if (originalPrice == 0) return 0;
            
            var discountAmount = originalPrice - discountedPrice;
            return Math.Round((discountAmount / originalPrice) * 100, 2);
        }
        
        /// <summary>
        /// 批次計算項目總金額 (包含折扣)
        /// </summary>
        public static decimal CalculateTotalAmount<T>(
            IEnumerable<T> items,
            Func<T, decimal> getQuantityFunc,
            Func<T, decimal> getUnitPriceFunc,
            Func<T, decimal> getDiscountPercentageFunc)
        {
            decimal total = 0;
            
            foreach (var item in items)
            {
                var quantity = getQuantityFunc(item);
                var unitPrice = getUnitPriceFunc(item);
                var discountPercentage = getDiscountPercentageFunc(item);
                
                var itemTotal = CalculateDiscountedPrice(quantity, unitPrice, discountPercentage);
                total += itemTotal;
            }
            
            return Math.Round(total, 2);
        }
        
        /// <summary>
        /// 計算折扣後單價 (用於顯示)
        /// </summary>
        public static decimal CalculateDiscountedUnitPrice(decimal unitPrice, decimal discountPercentage)
        {
            return CalculateDiscountedPrice(1, unitPrice, discountPercentage);
        }
        
        /// <summary>
        /// 格式化折扣顯示文字
        /// </summary>
        public static string FormatDiscountText(decimal discountPercentage)
        {
            if (discountPercentage == 0)
            {
                return "無折扣";
            }
            
            if (discountPercentage == 100)
            {
                return "免費";
            }
            
            return $"{discountPercentage:0.##}% OFF";
        }
        
        /// <summary>
        /// 計算多層折扣 (例如：先打9折，再打95折)
        /// </summary>
        public static decimal CalculateMultipleDiscounts(decimal originalPrice, params decimal[] discountPercentages)
        {
            var currentPrice = originalPrice;
            
            foreach (var discountPercentage in discountPercentages)
            {
                currentPrice = CalculateDiscountedPrice(currentPrice, discountPercentage);
            }
            
            return Math.Round(currentPrice, 2);
        }
    }
}
```

#### 套用之後的寫法

**修改後 - SalesOrderTable.razor**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private bool ValidateDiscountPercentage(decimal discountPercentage, out string errorMessage)
{
    var (isValid, error) = DiscountHelper.ValidateDiscountPercentage(discountPercentage);
    errorMessage = error;
    return isValid;
}

// 在資料行定義中計算折扣後金額
private decimal GetItemTotal(SalesItem item)
{
    return DiscountHelper.CalculateDiscountedPrice(
        quantity: item.Quantity,
        unitPrice: item.UnitPrice,
        discountPercentage: item.DiscountPercentage
    );
}

// 計算折扣金額 (用於顯示)
private decimal GetDiscountAmount(SalesItem item)
{
    var subtotal = item.Quantity * item.UnitPrice;
    return DiscountHelper.CalculateDiscountAmount(subtotal, item.DiscountPercentage);
}

// 簡化總金額計算
private void CalculateTotals()
{
    TotalAmount = DiscountHelper.CalculateTotalAmount(
        items: SalesItems.Where(i => !IsEmptyRow(i)),
        getQuantityFunc: item => item.Quantity,
        getUnitPriceFunc: item => item.UnitPrice,
        getDiscountPercentageFunc: item => item.DiscountPercentage
    );
}

// 顯示折扣文字
private string GetDiscountDisplayText(SalesItem item)
{
    return DiscountHelper.FormatDiscountText(item.DiscountPercentage);
}
```

**修改後 - QuotationTable.razor (多層折扣範例)**:

```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

// 假設有商品折扣 + VIP 折扣
private decimal CalculateFinalPrice(QuotationItem item)
{
    var basePrice = item.Quantity * item.UnitPrice;
    
    // 先套用商品折扣，再套用 VIP 折扣
    return DiscountHelper.CalculateMultipleDiscounts(
        originalPrice: basePrice,
        discountPercentages: new[] { item.DiscountPercentage, VIPDiscountPercentage }
    );
}
```

**優點**:
- ✅ 減少 15-25 行折扣計算代碼
- ✅ 統一的折扣驗證和計算邏輯
- ✅ 支援多層折扣計算
- ✅ 一致的數值精度處理

#### 套用進度

- [ ] SalesOrderTable.razor
- [ ] QuotationTable.razor
- [ ] PurchaseOrderTable.razor
- [ ] SalesReturnTable.razor
- [ ] PurchaseReturnTable.razor

---

## 📊 預計效益

### 程式碼減少量估計

| Helper 名稱 | 每個組件減少行數 | 影響組件數 | 總減少行數 |
|------------|----------------|-----------|----------|
| DetailLockHelper | 30-50 行 | 7 | ~280 行 |
| RelatedDocumentsViewHelper | 40-60 行 | 10 | ~500 行 |

### 維護性提升

- ✅ **統一邏輯**: 所有檢查邏輯集中在 Helper，修改時只需改一處
- ✅ **更容易測試**: Helper 可以獨立進行單元測試
- ✅ **降低錯誤率**: 減少複製貼上導致的不一致問題
- ✅ **提高可讀性**: 組件代碼更簡潔，關注業務邏輯

---

## 🚀 實施計劃

### Phase 1: 高優先級 Helper（預計 1 週）
1. ✅ AutoEmptyRowHelper（已完成）
2. ⏳ DetailLockHelper
3. ⏳ RelatedDocumentsViewHelper

### Phase 2: 中優先級 Helper（預計 1 週）
4. BatchOperationHelper
5. ApprovalCheckHelper
6. InventoryLocationHelper

### Phase 3: 低優先級 Helper（預計 3-5 天）
7. PropertyAccessHelper
8. DuplicateCheckHelper
9. ValidationMessageHelper

---

## 📝 注意事項

1. **向後兼容**: 實施過程中不會破壞現有功能
2. **逐步遷移**: 可以一個組件一個組件地套用，不需要一次全部修改
3. **測試驗證**: 每個 Helper 實施後都需要完整測試
4. **文檔更新**: Helper 需要完整的 XML 註解和使用範例

---

## 🔗 相關文件

- [AutoEmptyRowHelper 說明文件](./readme_InteractiveTableComponentHelper_新Helpers套用紀錄.md)
- [InteractiveTableComponent 使用說明](./README_互動Table說明.md)

- [ ] SalesOrderTable.razor
- [ ] QuotationTable.razor
- [ ] PurchaseOrderTable.razor
- [ ] SalesReturnTable.razor
- [ ] PurchaseReturnTable.razor

---

## 📊 預計效益

| Helper 類別 | 優先級 | 預估工作量 | 影響檔案數 | 可減少代碼行數 | 節省維護成本 |
|------------|-------|----------|----------|--------------|------------|
| DetailLockHelper | 🔴 高 | 3-4 小時 | 7+ | ~280 行 | 40% |
| RelatedDocumentsViewHelper | 🔴 高 | 3-4 小時 | 10+ | ~500 行 | 50% |
| BatchOperationHelper | 🟡 中 | 2-3 小時 | 5+ | ~200 行 | 35% |
| ApprovalCheckHelper | 🟡 中 | 2-3 小時 | 3+ | ~120 行 | 30% |
| InventoryLocationHelper | 🟡 中 | 2-3 小時 | 3+ | ~150 行 | 35% |
| PropertyAccessHelper | 🟢 低 | 1-2 小時 | 3+ | ~100 行 | 25% |
| QuantityCalculationHelper | 🟡 中 | 2-3 小時 | 5+ | ~150 行 | 35% |
| ValidationMessageHelper | 🟢 低 | 1-2 小時 | 34+ | ~400 行 | 40% |
| DuplicateCheckHelper | 🟢 低 | 1-2 小時 | 4+ | ~80 行 | 30% |
| SmartLoadingHelper | 🟡 中 | 2-3 小時 | 2+ | ~140 行 | 40% |
| SearchableSelectHelper | 🟢 低 | 1-2 小時 | 所有 | ~300 行 | 30% |
| DiscountHelper | 🟡 中 | 1-2 小時 | 5+ | ~125 行 | 35% |
| **總計** | - | **22-30 小時** | **34+ 個檔案** | **~2,545 行** | **平均 35%** |

### 總體效益分析

1. **代碼減少**: 預計可減少約 **2,545 行**重複代碼
2. **維護成本**: 平均降低 **35%** 的維護成本
3. **開發效率**: 新增類似功能時，開發時間可節省 **50%**
4. **程式碼品質**: 
   - ✅ 統一的業務邏輯
   - ✅ 更容易進行單元測試
   - ✅ 降低 Bug 發生率
   - ✅ 提升代碼可讀性

## 🚀 實施計劃

### 第一階段：高優先級 Helper (預估 6-8 小時)

**目標**: 先實作影響範圍最大、效益最高的 Helper

1. **DetailLockHelper** (3-4 小時)
   - 建立 Helper 類別
   - 套用到 7+ 個檔案
   - 測試鎖定檢查邏輯

2. **RelatedDocumentsViewHelper** (3-4 小時)
   - 建立 Helper 類別
   - 套用到 10+ 個檔案
   - 測試相關單據顯示

### 第二階段：中優先級 Helper (預估 10-15 小時)

**目標**: 實作常用的業務邏輯 Helper

3. **BatchOperationHelper** (2-3 小時)
   - 建立 Helper 類別
   - 套用到 5+ 個檔案
   - 測試批次操作

4. **ApprovalCheckHelper** (2-3 小時)
   - 建立 Helper 類別
   - 套用到 3+ 個檔案
   - 測試審核檢查

5. **InventoryLocationHelper** (2-3 小時)
   - 建立 Helper 類別
   - 套用到 3+ 個檔案
   - 測試庫存載入

6. **QuantityCalculationHelper** (2-3 小時)
   - 建立 Helper 類別
   - 套用到 5+ 個檔案
   - 測試數量計算

7. **SmartLoadingHelper** (2-3 小時)
   - 建立 Helper 類別
   - 套用到 2+ 個檔案
   - 測試智能載入

8. **DiscountHelper** (1-2 小時)
   - 建立 Helper 類別
   - 套用到 5+ 個檔案
   - 測試折扣計算

### 第三階段：低優先級 Helper (預估 6-7 小時)

**目標**: 完成所有 Helper，提升整體代碼品質

9. **PropertyAccessHelper** (1-2 小時)
   - 建立 Helper 類別
   - 套用到 3+ 個檔案
   - 測試屬性存取

10. **ValidationMessageHelper** (1-2 小時)
    - 建立 Helper 類別
    - 套用到所有檔案
    - 測試驗證訊息

11. **DuplicateCheckHelper** (1-2 小時)
    - 建立 Helper 類別
    - 套用到 4+ 個檔案
    - 測試重複檢查

12. **SearchableSelectHelper** (1-2 小時)
    - 建立 Helper 類別
    - 套用到所有檔案
    - 測試搜尋過濾

## 📝 實施注意事項

### 開發規範

1. **命名規範**
   - Helper 類別名稱: `{功能}Helper`
   - 檔案位置: `Helpers/InteractiveTableComponentHelper/{功能}Helper.cs`
   - Namespace: `ERPCore2.Helpers.InteractiveTableComponentHelper`

2. **程式碼規範**
   - 所有 Helper 方法都使用 `public static`
   - 提供完整的 XML 註解
   - 使用泛型提高重用性
   - 避免依賴外部服務 (除非必要)

3. **測試規範**
   - 每個 Helper 都應有單元測試
   - 測試覆蓋率目標: 80%+
   - 測試檔案位置: `Tests/Helpers/InteractiveTableComponentHelper/{功能}HelperTests.cs`

### 套用流程

1. **建立 Helper 類別**
   ```csharp
   // 1. 建立 Helper 檔案
   // 2. 實作靜態方法
   // 3. 加入 XML 註解
   ```

2. **套用到 Table 組件**
   ```csharp
   // 1. 加入 using 語句
   @using ERPCore2.Helpers.InteractiveTableComponentHelper
   
   // 2. 替換原有代碼
   // 3. 測試功能正常
   ```

3. **驗證與測試**
   ```csharp
   // 1. 執行單元測試
   // 2. 手動測試 UI 功能
   // 3. 檢查是否有遺漏的邊界情況
   ```

### 風險控管

1. **向後相容性**
   - 保留原有方法作為過渡期
   - 標記為 `[Obsolete]` 並註明替代方案
   - 給予充足的遷移時間

2. **功能驗證**
   - 每套用一個檔案就進行測試
   - 確保 UI 行為一致
   - 檢查錯誤訊息是否正確

3. **效能監控**
   - 監控 Helper 方法的執行時間
   - 避免不必要的重複計算
   - 適時使用快取機制

## 📈 後續優化建議

### 1. 建立 Helper 基底類別

考慮建立一個 `InteractiveTableHelperBase` 基底類別，提供共用的功能：

```csharp
public abstract class InteractiveTableHelperBase
{
    protected static INotificationService? NotificationService { get; set; }
    protected static IJSRuntime? JSRuntime { get; set; }
    
    public static void Initialize(INotificationService notificationService, IJSRuntime jsRuntime)
    {
        NotificationService = notificationService;
        JSRuntime = jsRuntime;
    }
}
```

### 2. 建立 Helper 組合包

將相關的 Helper 組合成一個更高階的 Helper：

```csharp
public static class InteractiveTableHelpers
{
    public static DetailLockHelper Lock => new();
    public static ValidationMessageHelper Validation => new();
    public static QuantityCalculationHelper Quantity => new();
    // ... 其他 Helper
}

// 使用方式
InteractiveTableHelpers.Lock.HasPaymentRecord(...)
InteractiveTableHelpers.Validation.BuildValidationMessage(...)
```

### 3. 效能優化

- 使用 `Lazy<T>` 延遲載入
- 加入快取機制 (MemoryCache)
- 批次操作時使用 `Parallel` 提升效能

### 4. 擴展功能

- 支援多語言 (i18n)
- 加入日誌記錄
- 提供更詳細的錯誤資訊
- 支援自訂驗證規則

## 🎯 成功指標

完成所有 Helper 實作後，應達成以下目標：

- [ ] 減少 **2,500+ 行**重複代碼
- [ ] 所有 Helper 都有 **80%+** 的測試覆蓋率
- [ ] **34+ 個** Table 組件都已套用相關 Helper
- [ ] 維護成本降低 **35%**
- [ ] 新增類似功能的開發時間節省 **50%**
- [ ] 代碼審查時間減少 **40%**
- [ ] Bug 回報數量降低 **30%**

---

## 📚 相關文件

- [AutoEmptyRowHelper 使用說明](./README_互動Table說明.md)
- [InteractiveTableComponent 使用指南](./README_互動Table說明.md)
- [主檔鎖住設計](./README_主檔鎖住設計.md)
- [A單轉B單流程](./README_A單轉B單.md)

---

**建立日期**: 2025-01-11  
**最後更新**: 2025-01-11  
**文件版本**: 1.0
