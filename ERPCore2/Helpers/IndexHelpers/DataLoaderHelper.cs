using ERPCore2.Services;

namespace ERPCore2.Helpers;

/// <summary>
/// 資料載入輔助類別，提供統一的資料載入與錯誤處理方法
/// </summary>
public static class DataLoaderHelper
{
    /// <summary>
    /// 執行資料載入操作，包含完整的錯誤處理機制
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="loadFunc">資料載入函數</param>
    /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="callerType">呼叫者類型，用於錯誤記錄</param>
    /// <param name="methodName">方法名稱，用於錯誤記錄</param>
    /// <returns>載入的實體列表，失敗時回傳空列表</returns>
    public static async Task<List<TEntity>> LoadAsync<TEntity>(
        Func<Task<List<TEntity>>> loadFunc,
        string entityName,
        INotificationService notificationService,
        Type callerType,
        string methodName) where TEntity : class
    {
        try
        {
            return await loadFunc();
        }
        catch (Exception ex)
        {
            // 記錄錯誤到資料庫
            await ErrorHandlingHelper.HandlePageErrorAsync(
                ex,
                methodName,
                callerType,
                additionalData: $"載入{entityName}資料失敗");

            // 通知使用者
            await notificationService.ShowErrorAsync($"載入{entityName}資料失敗");

            // 回傳安全的預設值
            return new List<TEntity>();
        }
    }

    /// <summary>
    /// 執行資料載入操作（簡化版本，自動使用預設方法名稱）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="loadFunc">資料載入函數</param>
    /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="callerType">呼叫者類型，用於錯誤記錄</param>
    /// <returns>載入的實體列表，失敗時回傳空列表</returns>
    public static Task<List<TEntity>> LoadAsync<TEntity>(
        Func<Task<List<TEntity>>> loadFunc,
        string entityName,
        INotificationService notificationService,
        Type callerType) where TEntity : class
    {
        return LoadAsync(
            loadFunc,
            entityName,
            notificationService,
            callerType,
            $"Load{typeof(TEntity).Name}Async");
    }
}
