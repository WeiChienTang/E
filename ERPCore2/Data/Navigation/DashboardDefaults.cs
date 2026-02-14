using ERPCore2.Models.Navigation;

namespace ERPCore2.Data.Navigation;

/// <summary>
/// 儀表板預設配置 - 定義預設面板與項目
/// </summary>
public static class DashboardDefaults
{
    /// <summary>
    /// 面板最大數量限制
    /// </summary>
    public const int MaxPanelCount = 6;

    /// <summary>
    /// 面板標題最大長度
    /// </summary>
    public const int MaxPanelTitleLength = 20;

    /// <summary>
    /// 預設面板定義
    /// </summary>
    public static readonly List<DefaultPanelDefinition> DefaultPanelDefinitions = new()
    {
        new DefaultPanelDefinition
        {
            Title = "頁面捷徑",
            SortOrder = 0,
            IconClass = "bi bi-grid-fill",
            ItemKeys = new List<string>
            {
                "/employees",           // 員工管理
                "/customers",           // 客戶管理
                "/suppliers",           // 廠商管理
                "/products",            // 商品管理
                "/inventoryStocks",     // 庫存查詢
                "/purchase/orders",     // 採購單管理
                "/salesOrders",         // 訂單管理
            }
        },
        new DefaultPanelDefinition
        {
            Title = "快速功能",
            SortOrder = 1,
            IconClass = "bi bi-lightning-fill",
            ItemKeys = new List<string>
            {
                "QuickAction:NewPurchaseOrder",  // 新增採購單
                "QuickAction:NewSalesOrder",      // 新增訂單
            }
        }
    };

    /// <summary>
    /// 從 NavigationItem 生成識別鍵
    /// Route 類型：使用 Route（如 "/employees"）
    /// Action 類型：使用 "Action:{ActionId}"
    /// QuickAction 類型：使用 "QuickAction:{ActionId}"
    /// </summary>
    public static string GetNavigationItemKey(NavigationItem item)
    {
        if (item.ItemType == NavigationItemType.QuickAction && !string.IsNullOrEmpty(item.ActionId))
        {
            return $"QuickAction:{item.ActionId}";
        }
        if (item.ItemType == NavigationItemType.Action && !string.IsNullOrEmpty(item.ActionId))
        {
            return $"Action:{item.ActionId}";
        }
        return item.Route;
    }

    /// <summary>
    /// 判斷 Key 是否為 QuickAction 類型
    /// </summary>
    public static bool IsQuickActionKey(string navigationItemKey)
    {
        return navigationItemKey?.StartsWith("QuickAction:") == true;
    }

    /// <summary>
    /// 取得預設面板中項目的排序順序
    /// </summary>
    public static int GetDefaultItemSortOrder(string panelTitle, string navigationItemKey)
    {
        var panel = DefaultPanelDefinitions.FirstOrDefault(p => p.Title == panelTitle);
        if (panel == null) return 1000;

        var index = panel.ItemKeys.IndexOf(navigationItemKey);
        return index >= 0 ? (index + 1) * 10 : 1000;
    }

    /// <summary>
    /// 可選用的面板圖示清單（精選 Bootstrap Icons）
    /// </summary>
    public static readonly List<IconOption> AvailableIcons = new()
    {
        // 常用
        new IconOption("bi bi-grid-fill", "格狀", "常用"),
        new IconOption("bi bi-lightning-fill", "閃電", "常用"),
        new IconOption("bi bi-star-fill", "星號", "常用"),
        new IconOption("bi bi-heart-fill", "愛心", "常用"),
        new IconOption("bi bi-bookmark-fill", "書籤", "常用"),
        new IconOption("bi bi-pin-fill", "圖釘", "常用"),
        new IconOption("bi bi-flag-fill", "旗幟", "常用"),
        new IconOption("bi bi-bell-fill", "鈴鐺", "常用"),

        // 商業
        new IconOption("bi bi-building", "大樓", "商業"),
        new IconOption("bi bi-shop", "商店", "商業"),
        new IconOption("bi bi-cart-fill", "購物車", "商業"),
        new IconOption("bi bi-bag-fill", "購物袋", "商業"),
        new IconOption("bi bi-cash-stack", "現金", "商業"),
        new IconOption("bi bi-currency-dollar", "美元", "商業"),
        new IconOption("bi bi-credit-card-fill", "信用卡", "商業"),
        new IconOption("bi bi-receipt", "收據", "商業"),

        // 文件
        new IconOption("bi bi-file-earmark-text-fill", "文件", "文件"),
        new IconOption("bi bi-folder-fill", "資料夾", "文件"),
        new IconOption("bi bi-clipboard-fill", "剪貼板", "文件"),
        new IconOption("bi bi-journal-text", "日誌", "文件"),
        new IconOption("bi bi-file-earmark-pdf-fill", "PDF", "文件"),
        new IconOption("bi bi-file-earmark-spreadsheet-fill", "表格", "文件"),

        // 導航/系統
        new IconOption("bi bi-house-fill", "首頁", "系統"),
        new IconOption("bi bi-gear-fill", "設定", "系統"),
        new IconOption("bi bi-tools", "工具", "系統"),
        new IconOption("bi bi-sliders", "調整", "系統"),
        new IconOption("bi bi-speedometer2", "儀表板", "系統"),
        new IconOption("bi bi-pie-chart-fill", "圓餅圖", "系統"),

        // 資料/庫存
        new IconOption("bi bi-box-fill", "箱子", "資料"),
        new IconOption("bi bi-box-seam-fill", "包裹", "資料"),
        new IconOption("bi bi-archive-fill", "封存", "資料"),
        new IconOption("bi bi-database-fill", "資料庫", "資料"),
        new IconOption("bi bi-graph-up", "上升圖", "資料"),
        new IconOption("bi bi-bar-chart-fill", "長條圖", "資料"),

        // 人員
        new IconOption("bi bi-person-fill", "單人", "人員"),
        new IconOption("bi bi-people-fill", "多人", "人員"),
        new IconOption("bi bi-person-badge-fill", "員工證", "人員"),
        new IconOption("bi bi-person-gear", "帳號設定", "人員"),

        // 物流
        new IconOption("bi bi-truck", "卡車", "物流"),
        new IconOption("bi bi-send-fill", "寄送", "物流"),
        new IconOption("bi bi-geo-alt-fill", "地點", "物流"),
        new IconOption("bi bi-calendar-check-fill", "排程", "物流"),
    };
}

/// <summary>
/// 圖示選項定義
/// </summary>
public class IconOption
{
    /// <summary>
    /// Bootstrap Icon CSS class
    /// </summary>
    public string IconClass { get; set; }

    /// <summary>
    /// 圖示顯示名稱
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 圖示分類
    /// </summary>
    public string Category { get; set; }

    public IconOption(string iconClass, string displayName, string category)
    {
        IconClass = iconClass;
        DisplayName = displayName;
        Category = category;
    }
}

/// <summary>
/// 預設面板定義
/// </summary>
public class DefaultPanelDefinition
{
    /// <summary>
    /// 面板標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 面板排序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 面板圖示
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// 面板內的項目識別鍵清單（按排序順序）
    /// </summary>
    public List<string> ItemKeys { get; set; } = new();
}
