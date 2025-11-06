using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 子單據儲存後刷新父單據明細的統一處理 Helper
/// 
/// 使用場景：
/// - 採購單 → 進貨單儲存後，刷新採購單明細的「入庫量」
/// - 進貨單 → 進貨退出儲存後，刷新進貨單明細的「退貨量」
/// - 銷貨訂單 → 銷貨退回儲存後，刷新銷貨訂單明細的「退貨量」
/// - 報價單 → 銷貨訂單儲存後，刷新報價單明細的「轉單數量」
/// 
/// 核心功能：
/// 1. 關閉子單據 Modal
/// 2. 重新載入父單據明細
/// 3. 刷新明細組件顯示
/// 4. 顯示成功通知
/// </summary>
public static class ChildDocumentRefreshHelper
{
    /// <summary>
    /// 處理子單據儲存後的標準刷新流程
    /// 
    /// 範例使用：
    /// <code>
    /// private async Task HandlePurchaseReceivingSaved(PurchaseReceiving savedReceiving)
    /// {
    ///     await ChildDocumentRefreshHelper.HandleChildDocumentSavedAsync(
    ///         closeModal: () => {
    ///             showPurchaseReceivingModal = false;
    ///             selectedPurchaseReceivingId = null;
    ///         },
    ///         reloadDetails: async () => {
    ///             if (PurchaseOrderId.HasValue)
    ///                 await LoadPurchaseOrderDetails(PurchaseOrderId.Value);
    ///         },
    ///         detailManager: purchaseOrderDetailManager,
    ///         notificationMessage: $"進貨單 {savedReceiving.ReceiptNumber} 已更新，採購明細已刷新",
    ///         stateHasChanged: StateHasChanged,
    ///         invokeAsync: InvokeAsync
    ///     );
    /// }
    /// </code>
    /// </summary>
    /// <param name="closeModal">關閉子單據 Modal 的動作（設定 visibility 和 id 為 null）</param>
    /// <param name="reloadDetails">重新載入父單據明細的動作</param>
    /// <param name="detailManager">明細管理組件的參考（用於強制 UI 刷新）</param>
    /// <param name="notificationMessage">成功通知訊息（選填）</param>
    /// <param name="stateHasChanged">StateHasChanged 方法</param>
    /// <param name="invokeAsync">InvokeAsync 方法（用於在正確的渲染上下文執行）</param>
    /// <param name="additionalActions">額外的自訂動作（選填）</param>
    public static async Task HandleChildDocumentSavedAsync(
        Action closeModal,
        Func<Task> reloadDetails,
        object? detailManager,
        string? notificationMessage,
        Action stateHasChanged,
        Func<Func<Task>, Task> invokeAsync,
        Func<Task>? additionalActions = null)
    {
        try
        {
            // 1. 關閉子單據 Modal
            closeModal();
            
            // 2. 重新載入父單據明細
            await reloadDetails();
            
            // 3. 刷新明細組件的顯示（強制 UI 更新）
            if (detailManager != null)
            {
                await invokeAsync(async () =>
                {
                    // 第一次刷新：觸發組件重新讀取資料
                    stateHasChanged();
                    
                    // 短暫延遲確保狀態更新完成
                    await Task.Delay(10);
                    
                    // 第二次刷新：確保 UI 完全更新
                    stateHasChanged();
                });
            }
            
            // 4. 執行額外的自訂動作（如果有）
            if (additionalActions != null)
            {
                await additionalActions();
            }
            
            // 5. 顯示成功通知（如果有提供訊息）
            // 注意：通知需要由調用者自行處理，因為 Helper 無法注入 INotificationService
            
            // 最後一次狀態刷新
            stateHasChanged();
        }
        catch
        {
            // 錯誤由調用者的 try-catch 處理
            throw;
        }
    }
    
    /// <summary>
    /// 簡化版：當不需要明細組件刷新時使用
    /// 
    /// 範例使用：
    /// <code>
    /// private async Task HandleSetoffDocumentSaved(SetoffDocument savedDocument)
    /// {
    ///     await ChildDocumentRefreshHelper.HandleChildDocumentSavedSimpleAsync(
    ///         closeModal: () => {
    ///             showSetoffDocumentModal = false;
    ///             selectedSetoffDocumentId = null;
    ///         },
    ///         stateHasChanged: StateHasChanged
    ///     );
    /// }
    /// </code>
    /// </summary>
    public static Task HandleChildDocumentSavedSimpleAsync(
        Action closeModal,
        Action stateHasChanged,
        Func<Task>? additionalActions = null)
    {
        try
        {
            // 關閉 Modal
            closeModal();
            
            // 執行額外動作（如果有）
            if (additionalActions != null)
            {
                additionalActions();
            }
            
            // 刷新狀態
            stateHasChanged();
            
            return Task.CompletedTask;
        }
        catch
        {
            throw;
        }
    }
    
    /// <summary>
    /// 進階版：支援明細組件的特定刷新方法調用
    /// 
    /// 範例使用：
    /// <code>
    /// private async Task HandlePurchaseReturnSaved(PurchaseReturn savedReturn)
    /// {
    ///     await ChildDocumentRefreshHelper.HandleChildDocumentSavedWithCustomRefreshAsync(
    ///         closeModal: () => {
    ///             showPurchaseReturnModal = false;
    ///             selectedPurchaseReturnId = null;
    ///         },
    ///         customRefresh: async () => {
    ///             if (purchaseReceivingDetailManager != null)
    ///                 await purchaseReceivingDetailManager.LoadReturnedQuantitiesAsync();
    ///         },
    ///         stateHasChanged: StateHasChanged,
    ///         invokeAsync: InvokeAsync
    ///     );
    /// }
    /// </code>
    /// </summary>
    public static async Task HandleChildDocumentSavedWithCustomRefreshAsync(
        Action closeModal,
        Func<Task> customRefresh,
        Action stateHasChanged,
        Func<Func<Task>, Task> invokeAsync)
    {
        try
        {
            // 關閉 Modal
            closeModal();
            
            // 執行自訂刷新動作
            await invokeAsync(async () =>
            {
                await customRefresh();
                stateHasChanged();
            });
            
            // 最後刷新
            stateHasChanged();
        }
        catch
        {
            throw;
        }
    }
    
    /// <summary>
    /// 特殊版：報價單轉銷貨訂單後的處理（需要更新轉單狀態和鎖定欄位）
    /// 
    /// 範例使用：
    /// <code>
    /// private async Task HandleSalesOrderSaved(SalesOrder savedSalesOrder)
    /// {
    ///     await ChildDocumentRefreshHelper.HandleQuotationConversionAsync(
    ///         closeModal: () => {
    ///             showSalesOrderModal = false;
    ///             selectedSalesOrderId = null;
    ///         },
    ///         quotationId: QuotationId,
    ///         savedSalesOrderId: savedSalesOrder.Id,
    ///         updateEntity: async () => {
    ///             if (editModalComponent?.Entity != null)
    ///             {
    ///                 editModalComponent.Entity.ConvertedToSalesOrderId = savedSalesOrder.Id;
    ///                 await QuotationService.UpdateAsync(editModalComponent.Entity);
    ///             }
    ///         },
    ///         reloadQuotation: async () => {
    ///             if (QuotationId.HasValue)
    ///             {
    ///                 await LoadQuotationDetails(QuotationId.Value);
    ///             }
    ///         },
    ///         checkUndeletable: () => quotationDetails.Any(d => d.ConvertedQuantity > 0),
    ///         updateHasUndeletableDetails: (hasUndeletable) => hasUndeletableDetails = hasUndeletable,
    ///         reinitializeFields: InitializeFormFieldsAsync,
    ///         stateHasChanged: StateHasChanged,
    ///         additionalActions: async () => {
    ///             if (quotationDetailManager != null)
    ///             {
    ///                 await quotationDetailManager.RefreshDetailsAsync();
    ///             }
    ///         }
    ///     );
    /// }
    /// </code>
    /// </summary>
    public static async Task HandleQuotationConversionAsync(
        Action closeModal,
        int? quotationId,
        int savedSalesOrderId,
        Func<Task> updateEntity,
        Func<Task> reloadQuotation,
        Func<bool> checkUndeletable,
        Action<bool> updateHasUndeletableDetails,
        Func<Task> reinitializeFields,
        Action stateHasChanged,
        Func<Task>? additionalActions = null)
    {
        try
        {
            // 關閉 Modal
            closeModal();
            
            if (!quotationId.HasValue)
                return;
            
            // 更新報價單實體的轉單狀態
            await updateEntity();
            
            // 重新載入報價單以取得完整資料
            await reloadQuotation();
            
            // 執行額外的動作（例如：呼叫 Table 組件的刷新方法）
            if (additionalActions != null)
            {
                await additionalActions();
            }
            
            // 更新鎖定狀態
            var hasUndeletable = checkUndeletable();
            updateHasUndeletableDetails(hasUndeletable);
            
            // 重新初始化表單欄位（更新唯讀狀態）
            await reinitializeFields();
            
            // 刷新 UI
            stateHasChanged();
        }
        catch
        {
            throw;
        }
    }
}
