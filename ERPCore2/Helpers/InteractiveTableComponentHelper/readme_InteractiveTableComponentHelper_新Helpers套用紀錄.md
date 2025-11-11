# InteractiveTableComponent Helper 優化建議與套用紀錄

---

## 📌 目標

針對使用 `InteractiveTableComponent` 的多個 Table 組件進行重複代碼分析，並提供可抽取為 Helper 的建議方案。

---

## 🔍 重複功能分析結果

### 影響範圍統計

| 組件名稱 | 路徑 | 重複功能數量 | 優先級 |
|---------|------|-------------|--------|
| SalesOrderTable | Components/Shared/BaseModal/Modals/Sales/ | 5 | 🔴 高 |
| PurchaseReceivingTable | Components/Shared/BaseModal/Modals/Purchase/ | 6 | 🔴 高 |
| PurchaseReturnTable | Components/Shared/BaseModal/Modals/Purchase/ | 5 | 🔴 高 |
| SalesReturnTable | Components/Shared/BaseModal/Modals/Sales/ | 5 | 🔴 高 |
| PurchaseOrderTable | Components/Shared/BaseModal/Modals/Purchase/ | 3 | 🟡 中 |
| QuotationTable | Components/Shared/BaseModal/Modals/Quotation/ | 2 | 🟡 中 |
| ProductSupplierTable | Components/Shared/BaseModal/Modals/Product/ | 1 | 🟢 低 |
| MaterialIssueTable | Components/Shared/BaseModal/Modals/MaterialIssue/ | 2 | 🟡 中 |

---

## 🎯 建議創建的 Helper 清單

### 1. DetailLockHelper - 明細鎖定檢查輔助類

**優先級**: 🔴 高  
**預估工作量**: 2-3 小時  
**影響範圍**: 5 個組件

#### 功能說明

統一處理明細是否可刪除/修改的檢查邏輯，包括：
- 沖款記錄檢查
- 退貨記錄檢查
- 轉單記錄檢查

#### 重複代碼範例

**當前狀況** (每個組件都重複):
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

#### 建議實作

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
            
            // 優先檢查 TotalPaidAmount
            var paidProperty = type.GetProperty("TotalPaidAmount");
            if (paidProperty != null)
            {
                return (decimal?)paidProperty.GetValue(entity) ?? 0;
            }
            
            // 其次檢查 TotalReceivedAmount
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
    }
}
```

#### 使用範例

**修改前**:
```csharp
// SalesOrderTable.razor
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

**修改後**:
```csharp
// SalesOrderTable.razor
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private bool CanDeleteItem(SalesItem item, out string reason)
{
    if (item.ExistingDetailEntity == null)
    {
        reason = string.Empty;
        return true;
    }
    
    return DetailLockHelper.CanDeleteItem(
        item.ExistingDetailEntity, 
        out reason, 
        _returnedQuantities);
}

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
```

#### 套用進度

- [ ] SalesOrderTable.razor
- [ ] PurchaseReceivingTable.razor
- [ ] PurchaseReturnTable.razor
- [ ] SalesReturnTable.razor
- [ ] QuotationTable.razor

---

### 2. RelatedDocumentsViewHelper - 相關單據查看輔助類

**優先級**: 🔴 高  
**預估工作量**: 3-4 小時  
**影響範圍**: 10+ 個組件

#### 功能說明

統一處理相關單據查看的 Modal 顯示邏輯，減少每個組件重複維護 Modal 狀態。

#### 重複代碼範例

**當前狀況** (每個組件都重複):
```csharp
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
```

#### 建議實作

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

#### 使用範例

**修改前**:
```csharp
// SalesOrderTable.razor
// ===== 相關單據查看 =====
private bool showRelatedDocumentsModal = false;
private string selectedProductName = string.Empty;
private List<RelatedDocument>? relatedDocuments = null;
private bool isLoadingRelatedDocuments = false;

private async Task ShowRelatedDocuments(SalesItem item)
{
    // ... 50+ 行重複代碼
}

// Razor 標記
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

**修改後**:
```csharp
// SalesOrderTable.razor
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private RelatedDocumentsViewHelper _relatedDocsHelper = new();

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

// Razor 標記 - 更簡潔
<RelatedDocumentsModalComponent IsVisible="@_relatedDocsHelper.IsVisible"
                               IsVisibleChanged="@((bool visible) => { if (!visible) _relatedDocsHelper.Hide(); })"
                               ProductName="@_relatedDocsHelper.ProductName"
                               RelatedDocuments="@_relatedDocsHelper.Documents"
                               IsLoading="@_relatedDocsHelper.IsLoading"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

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
**影響範圍**: 3 個組件

#### 功能說明

統一處理批次操作（全填、清空、清除等）的邏輯，包括鎖定項目的檢查和訊息提示。

#### 重複代碼範例

**當前狀況**:
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
```

#### 建議實作

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
        /// 批次填入數量
        /// </summary>
        public static async Task<BatchOperationResult> FillQuantitiesAsync<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool, string, bool> canModify,
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
            
            var unlocked = nonEmptyItems.Where(item => canModify(item, out _)).ToList();
            var locked = nonEmptyItems.Where(item => !canModify(item, out _)).ToList();
            
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
        /// 批次清空數量
        /// </summary>
        public static async Task<BatchOperationResult> ClearQuantitiesAsync<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool, string, bool> canModify,
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
            
            var unlocked = nonEmptyItems.Where(item => canModify(item, out _)).ToList();
            var locked = nonEmptyItems.Where(item => !canModify(item, out _)).ToList();
            
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
        public static async Task<BatchOperationResult> RemoveItemsAsync<TItem>(
            List<TItem> items,
            Func<TItem, bool> isEmptyRow,
            Func<TItem, bool, string, bool> canDelete,
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
            
            var unlocked = nonEmptyItems.Where(item => canDelete(item, out _)).ToList();
            var locked = nonEmptyItems.Where(item => !canDelete(item, out _)).ToList();
            
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

#### 使用範例

**修改前**:
```csharp
private async Task FillAllQuantities()
{
    // ... 30+ 行重複代碼
}

private async Task ClearAllQuantities()
{
    // ... 30+ 行重複代碼
}

private async Task ClearAllDetails()
{
    // ... 40+ 行重複代碼
}
```

**修改後**:
```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private async Task FillAllQuantities()
{
    var result = await BatchOperationHelper.FillQuantitiesAsync(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        canModify: CanDeleteItem,
        fillAction: item => item.ReceivedQuantity = item.OrderQuantity,
        notificationService: NotificationService,
        operationName: "進貨數量"
    );
    
    if (result.HasProcessedItems)
    {
        await NotifyDetailsChanged();
    }
}

private async Task ClearAllQuantities()
{
    var result = await BatchOperationHelper.ClearQuantitiesAsync(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        canModify: CanDeleteItem,
        clearAction: item => item.ReceivedQuantity = 0,
        notificationService: NotificationService,
        operationName: "進貨數量"
    );
    
    if (result.HasProcessedItems)
    {
        await NotifyDetailsChanged();
    }
}

private async Task ClearAllDetails()
{
    var result = await BatchOperationHelper.RemoveItemsAsync(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        canDelete: CanDeleteItem,
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

#### 套用進度

- [ ] PurchaseReceivingTable.razor
- [ ] PurchaseReturnTable.razor
- [ ] SalesReturnTable.razor

---

### 4. ApprovalWarningHelper - 審核警告輔助類

**優先級**: 🟡 中  
**預估工作量**: 1-2 小時  
**影響範圍**: 2 個組件

#### 功能說明

統一處理審核相關的警告訊息和檢查邏輯。

#### 建議實作

**檔案位置**: `Helpers/InteractiveTableComponentHelper/ApprovalWarningHelper.cs`

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
    /// 審核警告輔助類
    /// 用於統一處理審核相關的警告訊息
    /// </summary>
    public static class ApprovalWarningHelper
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
                                     "這些明細將無法儲存，請確認相關{documentTypeName}已完成審核後再進行作業。";
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
    }
}
```

#### 使用範例

**修改前**:
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
```

**修改後**:
```csharp
@using ERPCore2.Helpers.InteractiveTableComponentHelper

private ApprovalWarningInfo GetApprovalWarning()
{
    if (!IsApprovalEnabled)
        return new ApprovalWarningInfo();
    
    return ApprovalWarningHelper.GetWarningInfo(
        items: ReceivingItems,
        isEmptyRow: IsEmptyRow,
        isApproved: item => item.SelectedPurchaseDetail?.PurchaseOrder?.IsApproved ?? false,
        getItemName: item => item.SelectedProduct?.Name ?? "未知商品",
        documentTypeName: "採購單"
    );
}

// 在 Razor 標記中
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

// 在驗證方法中
public async Task<bool> ValidateAsync()
{
    var errors = new List<string>();
    
    // ... 其他驗證
    
    if (IsApprovalEnabled)
    {
        var (isValid, approvalErrors) = ApprovalWarningHelper.ValidateApproval(
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