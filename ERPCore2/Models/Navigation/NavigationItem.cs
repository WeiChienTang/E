namespace ERPCore2.Models.Navigation
{
    /// <summary>
    /// 導航項目模型
    /// </summary>
    public class NavigationItem : ISearchableItem
    {
        /// <summary>
        /// 功能名稱（繁體中文，用於搜尋 fallback）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 多語系資源鍵（設定後 NavMenu 會使用 IStringLocalizer 取得當前語言名稱）
        /// 格式：Nav.Xxx，對應 SharedResource.resx 中的 key
        /// </summary>
        public string? NameKey { get; set; }
        
        /// <summary>
        /// 功能描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 路由路徑（當 ItemType 為 Route 時使用）
        /// </summary>
        public string Route { get; set; } = string.Empty;
        
        // 顯式實作 ISearchableItem.Route（轉型為 string?）
        string? ISearchableItem.Route => string.IsNullOrEmpty(Route) ? null : Route;
        
        /// <summary>
        /// 導航項目類型
        /// </summary>
        public NavigationItemType ItemType { get; set; } = NavigationItemType.Route;
        
        /// <summary>
        /// 動作識別碼（當 ItemType 為 Action 時使用）
        /// </summary>
        public string? ActionId { get; set; }
        
        /// <summary>
        /// 圖示 CSS 類別
        /// </summary>
        public string IconClass { get; set; } = string.Empty;
        
        /// <summary>
        /// 分類
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// 權限要求
        /// </summary>
        public string? RequiredPermission { get; set; }
        
        /// <summary>
        /// 選單識別鍵（用於權限收集）
        /// </summary>
        public string? MenuKey { get; set; }
        
        /// <summary>
        /// 對應的公司模組識別鍵（僅父級選單使用）
        /// 設定後 CompanyModuleSeeder 會自動建立對應的 CompanyModule 記錄
        /// 未設定表示此選單群組不受模組啟用控制（如系統管理）
        /// </summary>
        public string? ModuleKey { get; set; }
        
        /// <summary>
        /// 搜尋關鍵字（包含同義詞等）
        /// </summary>
        public List<string> SearchKeywords { get; set; } = new();
        
        /// <summary>
        /// 是否為父級選單項目
        /// </summary>
        public bool IsParent { get; set; } = false;
        
        /// <summary>
        /// 子選單項目
        /// </summary>
        public List<NavigationItem> Children { get; set; } = new();
        
        /// <summary>
        /// 是否為分隔線（用於視覺分隔不同類型的選單項目）
        /// </summary>
        public bool IsDivider { get; set; } = false;

        // ===== 圖表介面支援（可選，設定後此項目會出現在首頁儀表板的「圖表介面」Tab） =====

        /// <summary>
        /// 是否為圖表介面項目（設定後會出現在首頁儀表板的「圖表介面」Tab，並從「頁面連結」Tab 移除）
        /// </summary>
        public bool IsChartWidget { get; set; } = false;

        // ===== QuickAction 支援（可選，設定後此項目可作為首頁快速功能） =====

        /// <summary>
        /// 快速功能識別碼（設定後表示此項目支援在首頁直接開啟 EditModal）
        /// Home.razor 透過此 Id 對應要開啟的 Modal 元件
        /// </summary>
        public string? QuickActionId { get; set; }

        /// <summary>
        /// 快速功能顯示名稱（選填，預設自動衍生為「新增」+ Name）
        /// </summary>
        public string? QuickActionName { get; set; }

        /// <summary>
        /// 快速功能描述（選填，預設自動衍生）
        /// </summary>
        public string? QuickActionDescription { get; set; }

        /// <summary>
        /// 快速功能圖示（選填，預設 "bi bi-plus-circle-fill"）
        /// </summary>
        public string? QuickActionIconClass { get; set; }
    }
}
