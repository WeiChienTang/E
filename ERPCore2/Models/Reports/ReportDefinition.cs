namespace ERPCore2.Models.Reports
{
    /// <summary>
    /// 報表定義 - 用於報表索引顯示
    /// </summary>
    public class ReportDefinition
    {
        /// <summary>
        /// 報表識別碼（唯一）
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 報表名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
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
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
