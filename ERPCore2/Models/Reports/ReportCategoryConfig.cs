namespace ERPCore2.Models.Reports;

/// <summary>
/// 報表分類設定 - 定義每個分類的顯示資訊
/// </summary>
public class ReportCategoryConfig
{
    /// <summary>
    /// 分類識別碼（對應 ReportCategory 常數）
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 顯示標題
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Bootstrap Icons 圖示類別
    /// </summary>
    public string Icon { get; set; } = "bi-printer";
    
    /// <summary>
    /// 取得所有報表分類設定
    /// </summary>
    public static Dictionary<string, ReportCategoryConfig> GetAllConfigs()
    {
        return new Dictionary<string, ReportCategoryConfig>
        {
            [ReportCategory.Customer] = new ReportCategoryConfig
            {
                Category = ReportCategory.Customer,
                Title = "客戶報表集",
                Icon = "bi-people"
            },
            [ReportCategory.Supplier] = new ReportCategoryConfig
            {
                Category = ReportCategory.Supplier,
                Title = "廠商報表集",
                Icon = "bi-building"
            },
            [ReportCategory.Product] = new ReportCategoryConfig
            {
                Category = ReportCategory.Product,
                Title = "產品報表集",
                Icon = "bi-box"
            },
            [ReportCategory.Inventory] = new ReportCategoryConfig
            {
                Category = ReportCategory.Inventory,
                Title = "庫存報表集",
                Icon = "bi-box-seam"
            },
            [ReportCategory.Sales] = new ReportCategoryConfig
            {
                Category = ReportCategory.Sales,
                Title = "銷售報表集",
                Icon = "bi-cart"
            },
            [ReportCategory.Purchase] = new ReportCategoryConfig
            {
                Category = ReportCategory.Purchase,
                Title = "採購報表集",
                Icon = "bi-cart-plus"
            },
            [ReportCategory.Financial] = new ReportCategoryConfig
            {
                Category = ReportCategory.Financial,
                Title = "財務報表集",
                Icon = "bi-currency-dollar"
            },
            [ReportCategory.HR] = new ReportCategoryConfig
            {
                Category = ReportCategory.HR,
                Title = "人力報表集",
                Icon = "bi-person-badge"
            }
        };
    }
    
    /// <summary>
    /// 根據分類取得設定
    /// </summary>
    /// <param name="category">分類識別碼</param>
    /// <returns>分類設定，若找不到則返回預設值</returns>
    public static ReportCategoryConfig GetConfig(string category)
    {
        var configs = GetAllConfigs();
        if (configs.TryGetValue(category, out var config))
        {
            return config;
        }
        
        // 返回預設設定
        return new ReportCategoryConfig
        {
            Category = category,
            Title = "報表集",
            Icon = "bi-printer"
        };
    }
}
