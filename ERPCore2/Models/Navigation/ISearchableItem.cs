namespace ERPCore2.Models.Navigation
{
    /// <summary>
    /// 可搜尋項目介面 - 用於通用搜尋 Modal 組件
    /// </summary>
    public interface ISearchableItem
    {
        /// <summary>
        /// 項目名稱（主要顯示文字）
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 項目描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 圖示 CSS 類別
        /// </summary>
        string IconClass { get; }
        
        /// <summary>
        /// 分類
        /// </summary>
        string Category { get; }
        
        /// <summary>
        /// 動作識別碼（用於執行對應的操作）
        /// </summary>
        string? ActionId { get; }
        
        /// <summary>
        /// 路由路徑（可選，用於導航）
        /// </summary>
        string? Route { get; }
    }
}
