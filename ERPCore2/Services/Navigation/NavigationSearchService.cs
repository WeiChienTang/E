using ERPCore2.Models;

namespace ERPCore2.Services;

/// <summary>
/// 導航搜尋服務實作
/// 提供系統功能導航項目的搜尋和管理功能
/// </summary>
public class NavigationSearchService : INavigationSearchService
{
    private readonly List<NavigationItem> _navigationItems;

    public NavigationSearchService()
    {
        _navigationItems = InitializeNavigationItems();
    }

    /// <summary>
    /// 獲取所有導航項目
    /// </summary>
    public List<NavigationItem> GetAllNavigationItems()
    {
        return _navigationItems.ToList();
    }

    /// <summary>
    /// 搜尋導航項目
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <returns>符合搜尋條件的導航項目清單</returns>
    public List<NavigationItem> SearchNavigationItems(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return GetAllNavigationItems();
        }

        var results = new List<NavigationItem>();
        searchTerm = searchTerm.Trim().ToLower();

        foreach (var item in _navigationItems)
        {
            // 搜尋父級項目
            if (MatchesSearchTerm(item, searchTerm))
            {
                results.Add(item);
            }
            
            // 搜尋子級項目
            foreach (var child in item.Children)
            {
                if (MatchesSearchTerm(child, searchTerm))
                {
                    results.Add(child);
                }
            }
        }

        return results.Distinct().ToList();
    }

    /// <summary>
    /// 根據分類獲取導航項目
    /// </summary>
    /// <param name="category">分類名稱</param>
    /// <returns>指定分類的導航項目清單</returns>
    public List<NavigationItem> GetNavigationItemsByCategory(string category)
    {
        var results = new List<NavigationItem>();
        
        foreach (var item in _navigationItems)
        {
            if (item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(item);
            }
            
            // 檢查子項目
            foreach (var child in item.Children)
            {
                if (child.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(child);
                }
            }
        }
        
        return results;
    }

    /// <summary>
    /// 獲取所有分類
    /// </summary>
    /// <returns>所有分類清單</returns>
    public List<string> GetAllCategories()
    {
        var categories = new HashSet<string>();
        
        foreach (var item in _navigationItems)
        {
            if (!string.IsNullOrEmpty(item.Category))
            {
                categories.Add(item.Category);
            }
            
            foreach (var child in item.Children)
            {
                if (!string.IsNullOrEmpty(child.Category))
                {
                    categories.Add(child.Category);
                }
            }
        }
        
        return categories.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// 根據權限篩選導航項目
    /// </summary>
    /// <param name="userPermissions">使用者權限清單</param>
    /// <returns>使用者有權限的導航項目</returns>
    public List<NavigationItem> GetNavigationItemsByPermissions(List<string> userPermissions)
    {
        var results = new List<NavigationItem>();
        
        foreach (var item in _navigationItems)
        {
            // 如果項目不需要權限或使用者有相應權限
            if (string.IsNullOrEmpty(item.RequiredPermission) || 
                userPermissions.Contains(item.RequiredPermission))
            {
                // 創建項目副本並篩選子項目
                var filteredItem = new NavigationItem
                {
                    Name = item.Name,
                    Description = item.Description,
                    Route = item.Route,
                    IconClass = item.IconClass,
                    Category = item.Category,
                    RequiredPermission = item.RequiredPermission,
                    SearchKeywords = item.SearchKeywords.ToList(),
                    IsParent = item.IsParent,
                    Children = new List<NavigationItem>()
                };
                
                // 篩選子項目
                foreach (var child in item.Children)
                {
                    if (string.IsNullOrEmpty(child.RequiredPermission) || 
                        userPermissions.Contains(child.RequiredPermission))
                    {
                        filteredItem.Children.Add(child);
                    }
                }
                
                results.Add(filteredItem);
            }
        }
        
        return results;
    }

    #region 私有方法

    /// <summary>
    /// 檢查導航項目是否符合搜尋條件
    /// </summary>
    private bool MatchesSearchTerm(NavigationItem item, string searchTerm)
    {
        // 檢查名稱
        if (item.Name.ToLower().Contains(searchTerm))
            return true;
            
        // 檢查描述
        if (item.Description.ToLower().Contains(searchTerm))
            return true;
            
        // 檢查分類
        if (item.Category.ToLower().Contains(searchTerm))
            return true;
            
        // 檢查搜尋關鍵字
        if (item.SearchKeywords.Any(keyword => keyword.ToLower().Contains(searchTerm)))
            return true;

        return false;
    }

    /// <summary>
    /// 初始化系統導航項目
    /// 根據 NavMenu.razor 中的選單結構建立導航項目清單
    /// </summary>
    private List<NavigationItem> InitializeNavigationItems()
    {
        return new List<NavigationItem>
        {
            // 首頁
            new NavigationItem
            {
                Name = "首頁",
                Description = "系統首頁和總覽",
                Route = "/",
                IconClass = "bi bi-house-door-fill",
                Category = "基礎功能",
                SearchKeywords = new List<string> { "首頁", "主頁", "主畫面", "home", "dashboard", "總覽" }
            },

            // 客戶管理
            new NavigationItem
            {
                Name = "客戶管理",
                Description = "管理客戶資料、聯絡資訊和客戶關係",
                Route = "/customers",
                IconClass = "bi bi-people-fill",
                Category = "客戶關係管理",
                RequiredPermission = "Customer.Read",
                SearchKeywords = new List<string> { "客戶", "顧客", "客戶管理", "customer", "client", "CRM", "客戶資料", "聯絡人" }
            },

            // 廠商管理
            new NavigationItem
            {
                Name = "廠商管理",
                Description = "管理供應商和廠商資料",
                Route = "/suppliers",
                IconClass = "bi bi-building-gear",
                Category = "供應鏈管理",
                RequiredPermission = "Supplier.Read",
                SearchKeywords = new List<string> { "廠商", "供應商", "廠商管理", "supplier", "vendor", "採購", "進貨" }
            },

            // 員工管理
            new NavigationItem
            {
                Name = "員工管理",
                Description = "管理員工資料和人事資訊",
                Route = "/employees",
                IconClass = "bi bi-person-badge-fill",
                Category = "人力資源管理",
                RequiredPermission = "Employee.Read",
                SearchKeywords = new List<string> { "員工", "人員", "員工管理", "employee", "staff", "HR", "人事", "職員" }
            },

            // 產品管理
            new NavigationItem
            {
                Name = "產品管理",
                Description = "管理產品資料和產品目錄",
                Route = "/products",
                IconClass = "bi bi-box-seam-fill",
                Category = "產品管理",
                RequiredPermission = "Product.Read",
                SearchKeywords = new List<string> { "產品", "商品", "產品管理", "product", "item", "商品目錄", "品項" }
            },

            // 庫存管理（父級選單）
            new NavigationItem
            {
                Name = "庫存管理",
                Description = "庫存相關功能管理",
                Route = "#",
                IconClass = "bi bi-boxes",
                Category = "庫存管理",
                IsParent = true,
                SearchKeywords = new List<string> { "庫存", "倉庫", "inventory", "warehouse", "stock" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "倉庫管理",
                        Description = "管理倉庫資料和設定",
                        Route = "/inventory/warehouses",
                        IconClass = "bi bi-building",
                        Category = "庫存管理",
                        SearchKeywords = new List<string> { "倉庫", "倉庫管理", "warehouse", "storage" }
                    },
                    new NavigationItem
                    {
                        Name = "庫位管理",
                        Description = "管理倉庫內的儲位配置",
                        Route = "/inventory/warehouse-locations",
                        IconClass = "bi bi-diagram-3",
                        Category = "庫存管理",
                        SearchKeywords = new List<string> { "庫位", "儲位", "location", "storage location" }
                    },
                    new NavigationItem
                    {
                        Name = "單位管理",
                        Description = "管理計量單位設定",
                        Route = "/inventory/units",
                        IconClass = "bi bi-rulers",
                        Category = "庫存管理",
                        SearchKeywords = new List<string> { "單位", "計量單位", "unit", "measurement" }
                    },
                    new NavigationItem
                    {
                        Name = "單位轉換管理",
                        Description = "設定不同單位間的轉換關係",
                        Route = "/inventory/unit-conversions",
                        IconClass = "bi bi-arrow-left-right",
                        Category = "庫存管理",
                        SearchKeywords = new List<string> { "單位轉換", "轉換", "conversion", "unit conversion" }
                    },
                    new NavigationItem
                    {
                        Name = "異動類型管理",
                        Description = "管理庫存異動的類型分類",
                        Route = "/inventory/transaction-types",
                        IconClass = "bi bi-list-ul",
                        Category = "庫存管理",
                        SearchKeywords = new List<string> { "異動類型", "交易類型", "transaction type", "movement type" }
                    },
                    new NavigationItem
                    {
                        Name = "庫存報表",
                        Description = "檢視庫存相關統計報表",
                        Route = "/inventory/reports",
                        IconClass = "bi bi-graph-up",
                        Category = "庫存管理",
                        SearchKeywords = new List<string> { "庫存報表", "報表", "statistics", "report", "統計" }
                    }
                }
            },

            // BOM表單位管理（父級選單）
            new NavigationItem
            {
                Name = "BOM表單位管理",
                Description = "BOM表相關基礎單位管理",
                Route = "#",
                IconClass = "bi bi-gear-fill",
                Category = "BOM管理",
                IsParent = true,
                SearchKeywords = new List<string> { "BOM", "單位管理", "基礎管理", "foundation" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "天氣管理",
                        Description = "管理天氣相關設定",
                        Route = "/BOMFoundation/weather",
                        IconClass = "bi bi-cloud-fill",
                        Category = "BOM管理",
                        RequiredPermission = "Weather.Read",
                        SearchKeywords = new List<string> { "天氣", "氣候", "weather", "climate" }
                    },
                    new NavigationItem
                    {
                        Name = "顏色管理",
                        Description = "管理產品顏色分類",
                        Route = "/BOMFoundation/colors",
                        IconClass = "bi bi-palette-fill",
                        Category = "BOM管理",
                        RequiredPermission = "Color.Read",
                        SearchKeywords = new List<string> { "顏色", "色彩", "color", "colour" }
                    },
                    new NavigationItem
                    {
                        Name = "材質管理",
                        Description = "管理產品材質分類",
                        Route = "/BOMFoundation/materials",
                        IconClass = "bi bi-layers-fill",
                        Category = "BOM管理",
                        RequiredPermission = "Material.Read",
                        SearchKeywords = new List<string> { "材質", "材料", "material", "texture" }
                    }
                }
            },

            // 元件展示（父級選單）
            new NavigationItem
            {
                Name = "元件展示",
                Description = "系統元件的展示和測試頁面",
                Route = "#",
                IconClass = "bi bi-puzzle-fill",
                Category = "開發工具",
                IsParent = true,
                SearchKeywords = new List<string> { "元件", "展示", "demo", "component", "showcase" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "Actions 元件",
                        Description = "操作按鈕元件展示",
                        Route = "/sharedcomponentsdemo/actions",
                        IconClass = "bi bi-lightning-fill",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "actions", "操作", "按鈕" }
                    },
                    new NavigationItem
                    {
                        Name = "Alerts 元件",
                        Description = "警告提示元件展示",
                        Route = "/sharedcomponentsdemo/alerts",
                        IconClass = "bi bi-exclamation-triangle-fill",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "alerts", "警告", "提示" }
                    },
                    new NavigationItem
                    {
                        Name = "Badges 元件",
                        Description = "徽章標籤元件展示",
                        Route = "/sharedcomponentsdemo/badges",
                        IconClass = "bi bi-award-fill",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "badges", "徽章", "標籤" }
                    },
                    new NavigationItem
                    {
                        Name = "Buttons 元件",
                        Description = "按鈕元件展示",
                        Route = "/sharedcomponentsdemo/buttons",
                        IconClass = "bi bi-hand-index-fill",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "buttons", "按鈕" }
                    },
                    new NavigationItem
                    {
                        Name = "Details 元件",
                        Description = "詳細資訊元件展示",
                        Route = "/sharedcomponentsdemo/details/new",
                        IconClass = "bi bi-card-text",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "details", "詳細", "資訊" }
                    },
                    new NavigationItem
                    {
                        Name = "Forms 元件",
                        Description = "表單元件展示",
                        Route = "/demo/forms",
                        IconClass = "bi bi-ui-checks",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "forms", "表單", "輸入" }
                    },
                    new NavigationItem
                    {
                        Name = "通用表單元件",
                        Description = "通用表單元件展示",
                        Route = "/demo/forms/generic",
                        IconClass = "bi bi-ui-checks",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "generic forms", "通用表單" }
                    },
                    new NavigationItem
                    {
                        Name = "搜尋篩選元件",
                        Description = "搜尋篩選元件展示",
                        Route = "/demo/forms/search-filter",
                        IconClass = "bi bi-ui-checks",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "search filter", "搜尋篩選" }
                    },
                    new NavigationItem
                    {
                        Name = "Headers 元件",
                        Description = "頁面標題元件展示",
                        Route = "/demo/headers/generic",
                        IconClass = "bi bi-header",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "headers", "標題", "頁首" }
                    },
                    new NavigationItem
                    {
                        Name = "Loading 元件",
                        Description = "載入狀態元件展示",
                        Route = "/sharedcomponentsdemo/loading",
                        IconClass = "bi bi-arrow-repeat",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "loading", "載入", "等待" }
                    },
                    new NavigationItem
                    {
                        Name = "通用表格元件",
                        Description = "通用表格元件展示",
                        Route = "/demo/tables/generic",
                        IconClass = "bi bi-table",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "generic table", "通用表格" }
                    },
                    new NavigationItem
                    {
                        Name = "索引表格元件",
                        Description = "索引表格元件展示",
                        Route = "/demo/tables",
                        IconClass = "bi bi-table",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "index table", "索引表格" }
                    },
                    new NavigationItem
                    {
                        Name = "導航搜尋測試",
                        Description = "導航搜尋功能測試頁面",
                        Route = "/demo/navigation-search",
                        IconClass = "bi bi-search",
                        Category = "開發工具",
                        SearchKeywords = new List<string> { "navigation search", "導航搜尋", "功能搜尋" }
                    }
                }
            },

            // 系統管理（父級選單）
            new NavigationItem
            {
                Name = "系統管理",
                Description = "系統管理和維護功能",
                Route = "#",
                IconClass = "bi bi-gear-fill",
                Category = "系統管理",
                IsParent = true,
                SearchKeywords = new List<string> { "系統管理", "管理", "system", "admin", "administration" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "錯誤記錄",
                        Description = "檢視和管理系統錯誤記錄",
                        Route = "/error-logs",
                        IconClass = "bi bi-exclamation-circle",
                        Category = "系統管理",
                        SearchKeywords = new List<string> { "錯誤記錄", "錯誤", "log", "error", "system error" }
                    },
                    new NavigationItem
                    {
                        Name = "錯誤處理範例",
                        Description = "展示 ErrorHandlingHelper 的使用方法",
                        Route = "/error-helper-demo",
                        IconClass = "bi bi-tools",
                        Category = "系統管理",
                        SearchKeywords = new List<string> { "錯誤處理", "範例", "demo", "error handling", "helper", "ErrorHandlingHelper" }
                    }
                }
            }
        };
    }

    #endregion
}
