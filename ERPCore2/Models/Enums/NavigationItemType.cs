namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 導航項目類型
    /// </summary>
    public enum NavigationItemType
    {
        /// <summary>
        /// 路由導航（預設）
        /// </summary>
        Route,
        
        /// <summary>
        /// 觸發動作（用於開啟 Modal 等，由 MainLayout 處理）
        /// </summary>
        Action,
        
        /// <summary>
        /// 快速功能（用於在首頁直接開啟業務 EditModal，由 Home.razor 處理）
        /// </summary>
        QuickAction
    }
}
