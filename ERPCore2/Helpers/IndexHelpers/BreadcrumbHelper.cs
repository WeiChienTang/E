using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Helpers;

/// <summary>
/// 麵包屑導航輔助類，提供統一的麵包屑初始化方法
/// </summary>
public static class BreadcrumbHelper
{
    /// <summary>
    /// 初始化麵包屑導航項目
    /// </summary>
    /// <param name="items">要設定的麵包屑項目（不包含首頁，首頁會自動加入）</param>
    /// <param name="notificationService">通知服務（可選）</param>
    /// <param name="callerType">呼叫者類型，用於錯誤記錄</param>
    /// <returns>完整的麵包屑項目列表，包含首頁</returns>
    public static async Task<List<BreadcrumbItem>> InitializeAsync(
        IEnumerable<BreadcrumbItem> items,
        INotificationService? notificationService = null,
        Type? callerType = null)
    {
        try
        {
            var breadcrumbItems = new List<BreadcrumbItem>
            {
                new("首頁", "/")
            };
            
            breadcrumbItems.AddRange(items);
            
            return breadcrumbItems;
        }
        catch (Exception ex)
        {
            // 記錄錯誤
            await ErrorHandlingHelper.HandlePageErrorAsync(
                ex, 
                nameof(InitializeAsync), 
                callerType ?? typeof(BreadcrumbHelper), 
                additionalData: "初始化麵包屑導航失敗");
            
            // 顯示錯誤通知
            if (notificationService != null)
            {
                await notificationService.ShowErrorAsync("初始化麵包屑導航失敗");
            }
            
            // 回傳安全的預設值（至少包含首頁）
            return new List<BreadcrumbItem>
            {
                new("首頁", "/")
            };
        }
    }

    /// <summary>
    /// 建立簡單的麵包屑導航（兩層：首頁 > 頁面名稱）
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="notificationService">通知服務（可選）</param>
    /// <param name="callerType">呼叫者類型，用於錯誤記錄</param>
    /// <returns>麵包屑項目列表</returns>
    public static Task<List<BreadcrumbItem>> CreateSimpleAsync(
        string pageName,
        INotificationService? notificationService = null,
        Type? callerType = null)
    {
        return InitializeAsync(
            new[] { new BreadcrumbItem(pageName) },
            notificationService,
            callerType);
    }

    /// <summary>
    /// 建立三層麵包屑導航（首頁 > 模組名稱 > 頁面名稱）
    /// </summary>
    /// <param name="moduleName">模組名稱</param>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="moduleUrl">模組連結（可選，預設為 null）</param>
    /// <param name="notificationService">通知服務（可選）</param>
    /// <param name="callerType">呼叫者類型，用於錯誤記錄</param>
    /// <returns>麵包屑項目列表</returns>
    public static Task<List<BreadcrumbItem>> CreateThreeLevelAsync(
        string moduleName,
        string pageName,
        string? moduleUrl = null,
        INotificationService? notificationService = null,
        Type? callerType = null)
    {
        var items = moduleUrl != null
            ? new[] 
            { 
                new BreadcrumbItem(moduleName, moduleUrl),
                new BreadcrumbItem(pageName)
            }
            : new[] 
            { 
                new BreadcrumbItem(moduleName),
                new BreadcrumbItem(pageName)
            };

        return InitializeAsync(items, notificationService, callerType);
    }
}
