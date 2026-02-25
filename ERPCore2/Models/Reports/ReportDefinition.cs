namespace ERPCore2.Models.Reports
{
    /// <summary>
    /// 報表定義 - 用於報表索引顯示
    /// </summary>
    public class ReportDefinition : ISearchableItem
    {
        /// <summary>
        /// 報表識別碼（唯一）
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 報表名稱（繁體中文，用於搜尋 fallback）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 多語系資源鍵（設定後 UI 會使用 IStringLocalizer 取得當前語言名稱）
        /// 格式：Report.Xxx，對應 SharedResource.resx 中的 key
        /// </summary>
        public string? NameKey { get; set; }

        /// <summary>
        /// 報表說明
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 報表圖示 CSS 類別
        /// </summary>
        public string IconClass { get; set; } = "bi bi-file-earmark-text";
        
        /// <summary>
        /// 報表分類（Customer/Supplier/Financial/Inventory 等）
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// 所需權限
        /// </summary>
        public string? RequiredPermission { get; set; }
        
        /// <summary>
        /// 開啟報表的 Action 識別碼
        /// </summary>
        public string ActionId { get; set; } = string.Empty;
        
        // 顯式實作 ISearchableItem.ActionId（轉型為 string?）
        string? ISearchableItem.ActionId => string.IsNullOrEmpty(ActionId) ? null : ActionId;
        
        // 顯式實作 ISearchableItem.Route（報表沒有路由，返回 null）
        string? ISearchableItem.Route => null;
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
    
    /// <summary>
    /// 報表分類常數
    /// </summary>
    public static class ReportCategory
    {
        public const string Customer = "Customer";
        public const string Supplier = "Supplier";
        public const string Product = "Product";
        public const string Financial = "Financial";
        public const string Inventory = "Inventory";
        public const string Sales = "Sales";
        public const string Purchase = "Purchase";
        public const string HR = "HR";
        public const string Vehicle = "Vehicle";
        public const string Waste = "Waste";
    public const string Accounting = "Accounting";
    }
}
