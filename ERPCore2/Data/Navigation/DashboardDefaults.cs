using ERPCore2.Models.Navigation;

namespace ERPCore2.Data.Navigation;

/// <summary>
/// 儀表板預設配置 - 定義預設捷徑清單和排序
/// </summary>
public static class DashboardDefaults
{
    /// <summary>
    /// 預設的首頁捷徑識別鍵清單（按排序順序）
    /// 新使用者會自動套用這些捷徑（需有對應權限）
    /// </summary>
    public static readonly List<string> DefaultWidgetKeys = new()
    {
        "/employees",           // 員工管理
        "/customers",           // 客戶管理
        "/suppliers",           // 廠商管理
        "/products",            // 商品管理
        "/inventoryStocks",     // 庫存查詢
        "/purchase/orders",     // 採購單管理
        "/salesOrders",         // 訂單管理
    };

    /// <summary>
    /// 從 NavigationItem 生成識別鍵
    /// Route 類型：使用 Route（如 "/employees"）
    /// Action 類型：使用 "Action:{ActionId}"
    /// </summary>
    public static string GetNavigationItemKey(NavigationItem item)
    {
        if (item.ItemType == NavigationItemType.Action && !string.IsNullOrEmpty(item.ActionId))
        {
            return $"Action:{item.ActionId}";
        }
        return item.Route;
    }

    /// <summary>
    /// 取得預設捷徑的排序順序
    /// </summary>
    public static int GetDefaultSortOrder(string navigationItemKey)
    {
        var index = DefaultWidgetKeys.IndexOf(navigationItemKey);
        return index >= 0 ? (index + 1) * 10 : 1000;
    }

    /// <summary>
    /// 判斷是否為預設捷徑
    /// </summary>
    public static bool IsDefaultWidget(string navigationItemKey)
    {
        return DefaultWidgetKeys.Contains(navigationItemKey);
    }
}
