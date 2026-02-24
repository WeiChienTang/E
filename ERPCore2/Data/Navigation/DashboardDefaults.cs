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
        },
        new DefaultPanelDefinition
        {
            Title = "會計",
            SortOrder = 2,
            IconClass = "bi bi-calculator",
            ItemKeys = new List<string>
            {
                "/account-items",                   // 會計科目
                "/journal-entries",                  // 傳票管理
                "/journal-entry-batch",              // 批次轉傳票
                "QuickAction:NewAccountItem",        // 新增會計科目
                "QuickAction:NewJournalEntry",       // 新增傳票
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
        new IconOption("bi bi-grid-3x3-gap-fill", "九宮格", "常用"),
        new IconOption("bi bi-lightning-fill", "閃電", "常用"),
        new IconOption("bi bi-star-fill", "星號", "常用"),
        new IconOption("bi bi-heart-fill", "愛心", "常用"),
        new IconOption("bi bi-bookmark-fill", "書籤", "常用"),
        new IconOption("bi bi-pin-fill", "圖釘", "常用"),
        new IconOption("bi bi-flag-fill", "旗幟", "常用"),
        new IconOption("bi bi-bell-fill", "鈴鐺", "常用"),
        new IconOption("bi bi-check-circle-fill", "勾選", "常用"),
        new IconOption("bi bi-x-circle-fill", "叉叉", "常用"),
        new IconOption("bi bi-exclamation-circle-fill", "驚嘆號", "常用"),
        new IconOption("bi bi-info-circle-fill", "資訊", "常用"),
        new IconOption("bi bi-question-circle-fill", "問號", "常用"),
        new IconOption("bi bi-plus-circle-fill", "加號", "常用"),
        new IconOption("bi bi-dash-circle-fill", "減號", "常用"),
        new IconOption("bi bi-fire", "火焰", "常用"),
        new IconOption("bi bi-rocket-takeoff-fill", "火箭", "常用"),
        new IconOption("bi bi-trophy-fill", "獎盃", "常用"),
        new IconOption("bi bi-award-fill", "獎章", "常用"),

        // 商業
        new IconOption("bi bi-building", "大樓", "商業"),
        new IconOption("bi bi-building-fill", "大樓(實)", "商業"),
        new IconOption("bi bi-shop", "商店", "商業"),
        new IconOption("bi bi-shop-window", "店面", "商業"),
        new IconOption("bi bi-cart-fill", "購物車", "商業"),
        new IconOption("bi bi-cart-plus-fill", "加入購物車", "商業"),
        new IconOption("bi bi-bag-fill", "購物袋", "商業"),
        new IconOption("bi bi-basket-fill", "購物籃", "商業"),
        new IconOption("bi bi-cash-stack", "現金", "商業"),
        new IconOption("bi bi-cash-coin", "硬幣", "商業"),
        new IconOption("bi bi-currency-dollar", "美元", "商業"),
        new IconOption("bi bi-currency-exchange", "換匯", "商業"),
        new IconOption("bi bi-credit-card-fill", "信用卡", "商業"),
        new IconOption("bi bi-wallet-fill", "錢包", "商業"),
        new IconOption("bi bi-receipt", "收據", "商業"),
        new IconOption("bi bi-receipt-cutoff", "發票", "商業"),
        new IconOption("bi bi-safe-fill", "保險箱", "商業"),
        new IconOption("bi bi-bank", "銀行", "商業"),
        new IconOption("bi bi-piggy-bank-fill", "存錢筒", "商業"),
        new IconOption("bi bi-percent", "百分比", "商業"),
        new IconOption("bi bi-tags-fill", "標籤", "商業"),
        new IconOption("bi bi-gift-fill", "禮物", "商業"),

        // 文件
        new IconOption("bi bi-file-earmark-text-fill", "文件", "文件"),
        new IconOption("bi bi-file-earmark-fill", "檔案", "文件"),
        new IconOption("bi bi-file-earmark-plus-fill", "新增檔案", "文件"),
        new IconOption("bi bi-file-earmark-check-fill", "核准檔案", "文件"),
        new IconOption("bi bi-folder-fill", "資料夾", "文件"),
        new IconOption("bi bi-folder-plus", "新增資料夾", "文件"),
        new IconOption("bi bi-clipboard-fill", "剪貼板", "文件"),
        new IconOption("bi bi-clipboard-check-fill", "檢查表", "文件"),
        new IconOption("bi bi-clipboard-data-fill", "數據板", "文件"),
        new IconOption("bi bi-journal-text", "日誌", "文件"),
        new IconOption("bi bi-journal-check", "核對日誌", "文件"),
        new IconOption("bi bi-file-earmark-pdf-fill", "PDF", "文件"),
        new IconOption("bi bi-file-earmark-spreadsheet-fill", "表格", "文件"),
        new IconOption("bi bi-file-earmark-word-fill", "Word", "文件"),
        new IconOption("bi bi-file-earmark-image-fill", "圖檔", "文件"),
        new IconOption("bi bi-file-earmark-zip-fill", "壓縮檔", "文件"),
        new IconOption("bi bi-paperclip", "附件", "文件"),
        new IconOption("bi bi-sticky-fill", "便利貼", "文件"),

        // 系統
        new IconOption("bi bi-house-fill", "首頁", "系統"),
        new IconOption("bi bi-house-door-fill", "房屋", "系統"),
        new IconOption("bi bi-gear-fill", "設定", "系統"),
        new IconOption("bi bi-gear-wide-connected", "系統設定", "系統"),
        new IconOption("bi bi-tools", "工具", "系統"),
        new IconOption("bi bi-wrench-adjustable", "扳手", "系統"),
        new IconOption("bi bi-sliders", "調整", "系統"),
        new IconOption("bi bi-sliders2", "滑桿", "系統"),
        new IconOption("bi bi-speedometer2", "儀表板", "系統"),
        new IconOption("bi bi-pie-chart-fill", "圓餅圖", "系統"),
        new IconOption("bi bi-shield-fill", "防護", "系統"),
        new IconOption("bi bi-shield-lock-fill", "安全鎖", "系統"),
        new IconOption("bi bi-key-fill", "鑰匙", "系統"),
        new IconOption("bi bi-lock-fill", "鎖定", "系統"),
        new IconOption("bi bi-unlock-fill", "解鎖", "系統"),
        new IconOption("bi bi-power", "電源", "系統"),
        new IconOption("bi bi-plug-fill", "插頭", "系統"),
        new IconOption("bi bi-cpu-fill", "處理器", "系統"),
        new IconOption("bi bi-hdd-fill", "硬碟", "系統"),
        new IconOption("bi bi-terminal-fill", "終端機", "系統"),

        // 資料/庫存
        new IconOption("bi bi-box-fill", "箱子", "資料"),
        new IconOption("bi bi-box-seam-fill", "包裹", "資料"),
        new IconOption("bi bi-boxes", "多箱", "資料"),
        new IconOption("bi bi-archive-fill", "封存", "資料"),
        new IconOption("bi bi-database-fill", "資料庫", "資料"),
        new IconOption("bi bi-database-fill-gear", "資料庫設定", "資料"),
        new IconOption("bi bi-server", "伺服器", "資料"),
        new IconOption("bi bi-graph-up", "上升圖", "資料"),
        new IconOption("bi bi-graph-down", "下降圖", "資料"),
        new IconOption("bi bi-graph-up-arrow", "趨勢上升", "資料"),
        new IconOption("bi bi-bar-chart-fill", "長條圖", "資料"),
        new IconOption("bi bi-bar-chart-line-fill", "線條圖", "資料"),
        new IconOption("bi bi-diagram-3-fill", "流程圖", "資料"),
        new IconOption("bi bi-activity", "活動", "資料"),
        new IconOption("bi bi-list-ul", "列表", "資料"),
        new IconOption("bi bi-list-check", "待辦清單", "資料"),
        new IconOption("bi bi-table", "表格", "資料"),
        new IconOption("bi bi-kanban-fill", "看板", "資料"),

        // 人員
        new IconOption("bi bi-person-fill", "單人", "人員"),
        new IconOption("bi bi-person-circle", "頭像", "人員"),
        new IconOption("bi bi-people-fill", "多人", "人員"),
        new IconOption("bi bi-person-badge-fill", "員工證", "人員"),
        new IconOption("bi bi-person-gear", "帳號設定", "人員"),
        new IconOption("bi bi-person-plus-fill", "新增人員", "人員"),
        new IconOption("bi bi-person-check-fill", "驗證人員", "人員"),
        new IconOption("bi bi-person-workspace", "工作空間", "人員"),
        new IconOption("bi bi-person-video3", "視訊會議", "人員"),
        new IconOption("bi bi-person-vcard-fill", "名片", "人員"),
        new IconOption("bi bi-person-lines-fill", "通訊錄", "人員"),
        new IconOption("bi bi-mortarboard-fill", "學歷", "人員"),

        // 物流
        new IconOption("bi bi-truck", "卡車", "物流"),
        new IconOption("bi bi-truck-front-fill", "貨車", "物流"),
        new IconOption("bi bi-send-fill", "寄送", "物流"),
        new IconOption("bi bi-send-check-fill", "已寄送", "物流"),
        new IconOption("bi bi-geo-alt-fill", "地點", "物流"),
        new IconOption("bi bi-geo-fill", "定位", "物流"),
        new IconOption("bi bi-map-fill", "地圖", "物流"),
        new IconOption("bi bi-compass-fill", "指南針", "物流"),
        new IconOption("bi bi-signpost-fill", "路標", "物流"),
        new IconOption("bi bi-calendar-check-fill", "排程", "物流"),
        new IconOption("bi bi-calendar-event-fill", "事件", "物流"),
        new IconOption("bi bi-calendar-week-fill", "週曆", "物流"),
        new IconOption("bi bi-clock-fill", "時鐘", "物流"),
        new IconOption("bi bi-stopwatch-fill", "碼錶", "物流"),
        new IconOption("bi bi-hourglass-split", "沙漏", "物流"),

        // 通訊
        new IconOption("bi bi-chat-fill", "對話", "通訊"),
        new IconOption("bi bi-chat-dots-fill", "聊天", "通訊"),
        new IconOption("bi bi-chat-left-text-fill", "訊息", "通訊"),
        new IconOption("bi bi-chat-square-text-fill", "留言", "通訊"),
        new IconOption("bi bi-envelope-fill", "信封", "通訊"),
        new IconOption("bi bi-envelope-open-fill", "已讀信", "通訊"),
        new IconOption("bi bi-telephone-fill", "電話", "通訊"),
        new IconOption("bi bi-phone-fill", "手機", "通訊"),
        new IconOption("bi bi-megaphone-fill", "擴音器", "通訊"),
        new IconOption("bi bi-broadcast-pin", "廣播", "通訊"),
        new IconOption("bi bi-wifi", "無線網路", "通訊"),
        new IconOption("bi bi-globe", "全球", "通訊"),

        // 媒體
        new IconOption("bi bi-image-fill", "圖片", "媒體"),
        new IconOption("bi bi-images", "相簿", "媒體"),
        new IconOption("bi bi-camera-fill", "相機", "媒體"),
        new IconOption("bi bi-camera-video-fill", "攝影機", "媒體"),
        new IconOption("bi bi-film", "影片", "媒體"),
        new IconOption("bi bi-play-circle-fill", "播放", "媒體"),
        new IconOption("bi bi-music-note-beamed", "音樂", "媒體"),
        new IconOption("bi bi-mic-fill", "麥克風", "媒體"),
        new IconOption("bi bi-volume-up-fill", "音量", "媒體"),
        new IconOption("bi bi-palette-fill", "調色盤", "媒體"),
        new IconOption("bi bi-brush-fill", "畫筆", "媒體"),
        new IconOption("bi bi-eyedropper", "滴管", "媒體"),

        // 工具/編輯
        new IconOption("bi bi-pencil-fill", "鉛筆", "編輯"),
        new IconOption("bi bi-pen-fill", "筆", "編輯"),
        new IconOption("bi bi-eraser-fill", "橡皮擦", "編輯"),
        new IconOption("bi bi-scissors", "剪刀", "編輯"),
        new IconOption("bi bi-trash-fill", "垃圾桶", "編輯"),
        new IconOption("bi bi-recycle", "回收", "編輯"),
        new IconOption("bi bi-arrow-repeat", "重新整理", "編輯"),
        new IconOption("bi bi-arrow-clockwise", "順時針", "編輯"),
        new IconOption("bi bi-arrow-counterclockwise", "逆時針", "編輯"),
        new IconOption("bi bi-search", "搜尋", "編輯"),
        new IconOption("bi bi-zoom-in", "放大", "編輯"),
        new IconOption("bi bi-zoom-out", "縮小", "編輯"),
        new IconOption("bi bi-filter", "篩選", "編輯"),
        new IconOption("bi bi-funnel-fill", "漏斗", "編輯"),
        new IconOption("bi bi-sort-alpha-down", "排序", "編輯"),

        // 天氣/自然
        new IconOption("bi bi-sun-fill", "太陽", "自然"),
        new IconOption("bi bi-moon-fill", "月亮", "自然"),
        new IconOption("bi bi-cloud-fill", "雲", "自然"),
        new IconOption("bi bi-cloud-sun-fill", "多雲", "自然"),
        new IconOption("bi bi-cloud-rain-fill", "雨天", "自然"),
        new IconOption("bi bi-snow", "雪花", "自然"),
        new IconOption("bi bi-lightning-charge-fill", "雷電", "自然"),
        new IconOption("bi bi-thermometer-half", "溫度計", "自然"),
        new IconOption("bi bi-droplet-fill", "水滴", "自然"),
        new IconOption("bi bi-tree-fill", "樹木", "自然"),
        new IconOption("bi bi-flower1", "花朵", "自然"),
        new IconOption("bi bi-bug-fill", "蟲子", "自然"),

        // 形狀/符號
        new IconOption("bi bi-circle-fill", "圓形", "形狀"),
        new IconOption("bi bi-square-fill", "方形", "形狀"),
        new IconOption("bi bi-triangle-fill", "三角形", "形狀"),
        new IconOption("bi bi-diamond-fill", "菱形", "形狀"),
        new IconOption("bi bi-hexagon-fill", "六角形", "形狀"),
        new IconOption("bi bi-octagon-fill", "八角形", "形狀"),
        new IconOption("bi bi-pentagon-fill", "五角形", "形狀"),
        new IconOption("bi bi-suit-heart-fill", "紅心", "形狀"),
        new IconOption("bi bi-suit-diamond-fill", "方塊", "形狀"),
        new IconOption("bi bi-suit-club-fill", "梅花", "形狀"),
        new IconOption("bi bi-suit-spade-fill", "黑桃", "形狀"),
        new IconOption("bi bi-asterisk", "星號", "形狀"),
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
