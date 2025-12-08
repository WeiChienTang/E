using ERPCore2.Helpers.InteractiveTableComponentHelper;
using ERPCore2.Services;
using Microsoft.JSInterop;

namespace ERPCore2.Helpers;

/// <summary>
/// 項目管理 Helper - 統一明細項目的增刪改查邏輯
/// 用於 InteractiveTableComponent 的項目管理操作
/// </summary>
public static class ItemManagementHelper
{
    /// <summary>
    /// 刪除項目（含驗證和通知）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">要刪除的項目</param>
    /// <param name="items">項目集合</param>
    /// <param name="canDeleteChecker">刪除檢查函式（返回是否可刪除和原因）</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="onDeleted">刪除後的回呼函式（可選）</param>
    /// <returns>是否成功刪除</returns>
    public static async Task<bool> HandleItemDeleteAsync<TItem>(
        TItem item,
        List<TItem> items,
        Func<TItem, (bool canDelete, string reason)> canDeleteChecker,
        INotificationService notificationService,
        Func<Task>? onDeleted = null)
    {
        var (canDelete, reason) = canDeleteChecker(item);
        
        if (!canDelete)
        {
            await notificationService.ShowWarningAsync(reason, "操作限制");
            return false;
        }
        
        items.Remove(item);
        
        if (onDeleted != null)
        {
            await onDeleted();
        }
        
        return true;
    }
    
    /// <summary>
    /// 刪除項目（簡化版，使用 DetailLockHelper 檢查）
    /// </summary>
    /// <typeparam name="TItem">項目類型（必須是 class）</typeparam>
    /// <param name="item">要刪除的項目</param>
    /// <param name="items">項目集合</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="onDeleted">刪除後的回呼函式（可選）</param>
    /// <param name="checkReceiving">是否檢查進貨記錄</param>
    /// <param name="checkDelivery">是否檢查出貨記錄</param>
    /// <returns>是否成功刪除</returns>
    public static async Task<bool> HandleItemDeleteWithLockCheckAsync<TItem>(
        TItem item,
        List<TItem> items,
        INotificationService notificationService,
        Func<Task>? onDeleted = null,
        bool checkReceiving = false,
        bool checkDelivery = false)
        where TItem : class
    {
        if (!DetailLockHelper.CanDeleteItem(item, out string reason, checkReceiving, checkDelivery))
        {
            await notificationService.ShowWarningAsync(reason, "操作限制");
            return false;
        }
        
        items.Remove(item);
        
        if (onDeleted != null)
        {
            await onDeleted();
        }
        
        return true;
    }
    
    /// <summary>
    /// 清除所有明細（含確認對話框）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="items">項目集合</param>
    /// <param name="jsRuntime">JS Runtime（用於顯示確認對話框）</param>
    /// <param name="onCleared">清除後的回呼函式（可選）</param>
    /// <param name="confirmMessage">確認訊息（可自訂）</param>
    /// <returns>是否成功清除</returns>
    public static async Task<bool> ClearAllDetailsAsync<TItem>(
        List<TItem> items,
        IJSRuntime jsRuntime,
        Func<Task>? onCleared = null,
        string confirmMessage = "確定要清除所有明細嗎？")
    {
        if (!items.Any()) 
        {
            return false;
        }
        
        var confirmed = await jsRuntime.InvokeAsync<bool>("confirm", confirmMessage);
        
        if (confirmed)
        {
            items.Clear();
            
            if (onCleared != null)
            {
                await onCleared();
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 清除所有明細（不需確認，直接清除）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="items">項目集合</param>
    /// <param name="onCleared">清除後的回呼函式（可選）</param>
    /// <returns>是否成功清除</returns>
    public static async Task<bool> ClearAllDetailsWithoutConfirmAsync<TItem>(
        List<TItem> items,
        Func<Task>? onCleared = null)
    {
        if (!items.Any())
        {
            return false;
        }
        
        items.Clear();
        
        if (onCleared != null)
        {
            await onCleared();
        }
        
        return true;
    }
    
    /// <summary>
    /// 批次刪除項目（含驗證）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="itemsToDelete">要刪除的項目集合</param>
    /// <param name="allItems">所有項目集合</param>
    /// <param name="canDeleteChecker">刪除檢查函式</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="onDeleted">刪除後的回呼函式（可選）</param>
    /// <returns>成功刪除的項目數量</returns>
    public static async Task<int> BatchDeleteItemsAsync<TItem>(
        List<TItem> itemsToDelete,
        List<TItem> allItems,
        Func<TItem, (bool canDelete, string reason)> canDeleteChecker,
        INotificationService notificationService,
        Func<Task>? onDeleted = null)
    {
        var deletedCount = 0;
        var failedItems = new List<(TItem item, string reason)>();
        
        foreach (var item in itemsToDelete)
        {
            var (canDelete, reason) = canDeleteChecker(item);
            
            if (canDelete)
            {
                allItems.Remove(item);
                deletedCount++;
            }
            else
            {
                failedItems.Add((item, reason));
            }
        }
        
        if (failedItems.Any())
        {
            var errorMessage = $"有 {failedItems.Count} 個項目無法刪除：\n" +
                             string.Join("\n", failedItems.Select((x, i) => $"{i + 1}. {x.reason}"));
            
            await notificationService.ShowWarningAsync(errorMessage, "部分刪除失敗");
        }
        
        if (deletedCount > 0 && onDeleted != null)
        {
            await onDeleted();
        }
        
        return deletedCount;
    }
}
