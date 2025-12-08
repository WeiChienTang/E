using ERPCore2.Services;

namespace ERPCore2.Helpers;

/// <summary>
/// 歷史記錄檢查 Helper - 統一歷史記錄查詢和載入邏輯
/// 用於智能下單功能（從歷史記錄載入商品資訊）
/// </summary>
public static class HistoryCheckHelper
{
    /// <summary>
    /// 檢查並載入歷史記錄（通用版本）
    /// </summary>
    /// <typeparam name="THistoryRecord">歷史記錄類型</typeparam>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="entityId">實體 ID（例如：客戶 ID、廠商 ID）</param>
    /// <param name="historyLoader">歷史記錄載入函式（非同步）</param>
    /// <param name="converter">轉換函式（將歷史記錄轉換為 Item）</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
    /// <returns>轉換後的 Item 集合（如果沒有歷史記錄或發生錯誤，返回空集合）</returns>
    public static async Task<List<TItem>> CheckAndLoadHistoryAsync<THistoryRecord, TItem>(
        int? entityId,
        Func<int, Task<THistoryRecord?>> historyLoader,
        Func<THistoryRecord, List<TItem>> converter,
        INotificationService notificationService,
        string entityName = "此項目")
    {
        if (!entityId.HasValue || entityId.Value <= 0)
        {
            await notificationService.ShowWarningAsync($"請先選擇{entityName}", "提示");
            return new List<TItem>();
        }
        
        try
        {
            var historyRecord = await historyLoader(entityId.Value);
            
            if (historyRecord == null)
            {
                await notificationService.ShowWarningAsync($"查無{entityName}的歷史記錄", "提示");
                return new List<TItem>();
            }
            
            return converter(historyRecord);
        }
        catch (Exception ex)
        {
            await notificationService.ShowErrorAsync($"載入歷史記錄失敗：{ex.Message}");
            return new List<TItem>();
        }
    }
    
    /// <summary>
    /// 檢查並載入歷史記錄（含成功通知）
    /// </summary>
    /// <typeparam name="THistoryRecord">歷史記錄類型</typeparam>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="entityId">實體 ID</param>
    /// <param name="historyLoader">歷史記錄載入函式</param>
    /// <param name="converter">轉換函式</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="entityName">實體名稱</param>
    /// <param name="showSuccessMessage">是否顯示成功訊息</param>
    /// <returns>轉換後的 Item 集合</returns>
    public static async Task<List<TItem>> CheckAndLoadHistoryWithNotificationAsync<THistoryRecord, TItem>(
        int? entityId,
        Func<int, Task<THistoryRecord?>> historyLoader,
        Func<THistoryRecord, List<TItem>> converter,
        INotificationService notificationService,
        string entityName = "此項目",
        bool showSuccessMessage = true)
    {
        var items = await CheckAndLoadHistoryAsync(
            entityId,
            historyLoader,
            converter,
            notificationService,
            entityName
        );
        
        if (items.Any() && showSuccessMessage)
        {
            await notificationService.ShowSuccessAsync($"已載入 {items.Count} 筆歷史記錄", "載入成功");
        }
        
        return items;
    }
    
    /// <summary>
    /// 檢查並載入歷史記錄（含確認對話框）
    /// </summary>
    /// <typeparam name="THistoryRecord">歷史記錄類型</typeparam>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="entityId">實體 ID</param>
    /// <param name="currentItems">當前項目集合（用於檢查是否已有資料）</param>
    /// <param name="historyLoader">歷史記錄載入函式</param>
    /// <param name="converter">轉換函式</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="confirmIfNotEmpty">如果當前已有資料，是否顯示確認對話框</param>
    /// <param name="entityName">實體名稱</param>
    /// <returns>轉換後的 Item 集合</returns>
    public static async Task<List<TItem>> CheckAndLoadHistoryWithConfirmAsync<THistoryRecord, TItem>(
        int? entityId,
        List<TItem> currentItems,
        Func<int, Task<THistoryRecord?>> historyLoader,
        Func<THistoryRecord, List<TItem>> converter,
        INotificationService notificationService,
        bool confirmIfNotEmpty = true,
        string entityName = "此項目")
    {
        // 如果當前已有資料且需要確認
        if (currentItems.Any() && confirmIfNotEmpty)
        {
            var confirmed = await notificationService.ShowConfirmAsync(
                "載入歷史記錄將會覆蓋目前的明細，確定要繼續嗎？",
                "確認載入"
            );
            
            if (!confirmed)
            {
                return new List<TItem>();
            }
        }
        
        return await CheckAndLoadHistoryAsync(
            entityId,
            historyLoader,
            converter,
            notificationService,
            entityName
        );
    }
    
    /// <summary>
    /// 合併歷史記錄（不覆蓋現有項目，只新增不存在的項目）
    /// </summary>
    /// <typeparam name="THistoryRecord">歷史記錄類型</typeparam>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <typeparam name="TKey">鍵值類型（用於判斷重複）</typeparam>
    /// <param name="entityId">實體 ID</param>
    /// <param name="currentItems">當前項目集合</param>
    /// <param name="historyLoader">歷史記錄載入函式</param>
    /// <param name="converter">轉換函式</param>
    /// <param name="keySelector">鍵值選擇器（用於判斷重複的欄位）</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="entityName">實體名稱</param>
    /// <returns>新增的項目數量</returns>
    public static async Task<int> MergeHistoryAsync<THistoryRecord, TItem, TKey>(
        int? entityId,
        List<TItem> currentItems,
        Func<int, Task<THistoryRecord?>> historyLoader,
        Func<THistoryRecord, List<TItem>> converter,
        Func<TItem, TKey> keySelector,
        INotificationService notificationService,
        string entityName = "此項目")
    {
        var historyItems = await CheckAndLoadHistoryAsync(
            entityId,
            historyLoader,
            converter,
            notificationService,
            entityName
        );
        
        if (!historyItems.Any())
        {
            return 0;
        }
        
        // 取得當前項目的鍵值集合
        var existingKeys = new HashSet<TKey>(currentItems.Select(keySelector));
        
        // 只新增不存在的項目
        var newItems = historyItems
            .Where(item => !existingKeys.Contains(keySelector(item)))
            .ToList();
        
        currentItems.AddRange(newItems);
        
        if (newItems.Any())
        {
            await notificationService.ShowSuccessAsync(
                $"已新增 {newItems.Count} 筆歷史記錄（跳過 {historyItems.Count - newItems.Count} 筆重複項目）",
                "合併成功"
            );
        }
        else
        {
            await notificationService.ShowInfoAsync("沒有新的項目可新增（所有項目皆已存在）", "提示");
        }
        
        return newItems.Count;
    }
}
