using ERPCore2.Data;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers;

/// <summary>
/// 明細同步 Helper - 統一資料載入和同步邏輯
/// 用於 InteractiveTableComponent 的明細資料管理
/// </summary>
/// <typeparam name="TMainEntity">主檔實體類型</typeparam>
/// <typeparam name="TDetailEntity">明細實體類型</typeparam>
public static class DetailSyncHelper<TMainEntity, TDetailEntity> 
    where TMainEntity : BaseEntity
    where TDetailEntity : BaseEntity, new()
{
    /// <summary>
    /// 載入現有明細（同步版本）
    /// </summary>
    /// <typeparam name="TItem">顯示用的 Item 類型</typeparam>
    /// <param name="existingDetails">現有的明細實體集合</param>
    /// <param name="converter">轉換函式（將 TDetailEntity 轉換為 TItem）</param>
    /// <returns>轉換後的 Item 集合</returns>
    public static List<TItem> LoadExistingDetails<TItem>(
        List<TDetailEntity>? existingDetails,
        Func<TDetailEntity, TItem?> converter)
    {
        if (existingDetails?.Any() != true)
        {
            return new List<TItem>();
        }
        
        return existingDetails
            .Select(converter)
            .Where(item => item != null)
            .Select(item => item!)
            .ToList();
    }
    
    /// <summary>
    /// 載入現有明細（非同步版本，支援額外的非同步資料載入）
    /// </summary>
    /// <typeparam name="TItem">顯示用的 Item 類型</typeparam>
    /// <param name="existingDetails">現有的明細實體集合</param>
    /// <param name="asyncConverter">非同步轉換函式（將 TDetailEntity 轉換為 TItem，支援額外資料載入）</param>
    /// <returns>轉換後的 Item 集合</returns>
    public static async Task<List<TItem>> LoadExistingDetailsAsync<TItem>(
        List<TDetailEntity>? existingDetails,
        Func<TDetailEntity, Task<TItem?>> asyncConverter)
    {
        if (existingDetails?.Any() != true)
        {
            return new List<TItem>();
        }
        
        var tasks = existingDetails.Select(asyncConverter);
        var results = await Task.WhenAll(tasks);
        
        return results
            .Where(item => item != null)
            .Select(item => item!)
            .ToList();
    }
    
    /// <summary>
    /// 同步明細到父組件
    /// </summary>
    /// <typeparam name="TItem">顯示用的 Item 類型</typeparam>
    /// <param name="items">要同步的 Item 集合</param>
    /// <param name="onItemsChanged">父組件的 EventCallback（接收 Item 集合）</param>
    /// <param name="onStateChanged">狀態變更回呼（可選，通常是 StateHasChanged）</param>
    public static async Task SyncToParentAsync<TItem>(
        List<TItem> items,
        EventCallback<List<TItem>> onItemsChanged,
        Action? onStateChanged = null)
    {
        if (onItemsChanged.HasDelegate)
        {
            await onItemsChanged.InvokeAsync(items);
        }
        
        onStateChanged?.Invoke();
    }
    
    /// <summary>
    /// 同步明細實體到父組件（直接傳遞實體）
    /// </summary>
    /// <typeparam name="TItem">顯示用的 Item 類型</typeparam>
    /// <param name="items">要同步的 Item 集合</param>
    /// <param name="converter">轉換函式（將 TItem 轉換為 TDetailEntity）</param>
    /// <param name="onDetailsChanged">父組件的 EventCallback（接收實體集合）</param>
    /// <param name="onStateChanged">狀態變更回呼（可選）</param>
    /// <param name="excludeEmpty">是否排除空項目（預設 true）</param>
    public static async Task SyncEntitiesToParentAsync<TItem>(
        List<TItem> items,
        Func<TItem, TDetailEntity?> converter,
        EventCallback<List<TDetailEntity>> onDetailsChanged,
        Action? onStateChanged = null,
        bool excludeEmpty = true)
    {
        var entities = ConvertToEntities(items, converter, excludeEmpty);
        
        if (onDetailsChanged.HasDelegate)
        {
            await onDetailsChanged.InvokeAsync(entities);
        }
        
        onStateChanged?.Invoke();
    }
    
    /// <summary>
    /// 轉換為實體物件（供儲存使用）
    /// </summary>
    /// <typeparam name="TItem">顯示用的 Item 類型</typeparam>
    /// <param name="items">要轉換的 Item 集合</param>
    /// <param name="converter">轉換函式（將 TItem 轉換為 TDetailEntity）</param>
    /// <param name="excludeEmpty">是否排除空項目（預設 true，排除 null 值）</param>
    /// <returns>實體集合</returns>
    public static List<TDetailEntity> ConvertToEntities<TItem>(
        List<TItem> items,
        Func<TItem, TDetailEntity?> converter,
        bool excludeEmpty = true)
    {
        var query = items.Select(converter);
        
        if (excludeEmpty)
        {
            query = query.Where(e => e != null);
        }
        
        return query
            .Select(e => e!)
            .ToList();
    }
    
    /// <summary>
    /// 通用的通知變更方法（簡化版，只處理狀態變更）
    /// </summary>
    /// <param name="onStateChanged">狀態變更回呼（通常是 StateHasChanged）</param>
    public static void NotifyStateChanged(Action? onStateChanged)
    {
        onStateChanged?.Invoke();
    }
}

/// <summary>
/// 無泛型約束版本的 DetailSyncHelper（用於不需要指定主檔和明細實體的場景）
/// </summary>
public static class DetailSyncHelper
{
    /// <summary>
    /// 同步集合到父組件
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="items">要同步的項目集合</param>
    /// <param name="onItemsChanged">父組件的 EventCallback</param>
    /// <param name="onStateChanged">狀態變更回呼（可選）</param>
    public static async Task SyncToParentAsync<TItem>(
        List<TItem> items,
        EventCallback<List<TItem>> onItemsChanged,
        Action? onStateChanged = null)
    {
        if (onItemsChanged.HasDelegate)
        {
            await onItemsChanged.InvokeAsync(items);
        }
        
        onStateChanged?.Invoke();
    }
    
    /// <summary>
    /// 通用的通知變更方法
    /// </summary>
    /// <param name="onStateChanged">狀態變更回呼</param>
    public static void NotifyStateChanged(Action? onStateChanged)
    {
        onStateChanged?.Invoke();
    }
}
