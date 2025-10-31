using ERPCore2.Models;

namespace ERPCore2.Data.Navigation;

/// <summary>
/// 導航選單配置 - 系統唯一選單資料來源
/// 此配置同時用於 NavMenu 渲染和搜尋功能
/// </summary>
public static class NavigationConfig
{
    /// <summary>
    /// 取得所有導航項目（包含父級和子級）
    /// </summary>
    public static List<NavigationItem> GetAllNavigationItems()
    {
        return new List<NavigationItem>
        {
            // ==================== 首頁 ====================
            new NavigationItem
            {
                Name = "首頁",
                Description = "系統首頁和總覽",
                Route = "/",
                IconClass = "bi-house-door-fill",
                Category = "基礎功能",
                SearchKeywords = new List<string> { "首頁", "主頁", "主畫面", "home", "dashboard", "總覽" }
            },

            // ==================== 人力資源管理 ====================
            new NavigationItem
            {
                Name = "人力資源管理",
                Description = "員工相關功能管理",
                Route = "#",
                IconClass = "bi bi-person-badge-fill",
                Category = "人力資源管理",
                IsParent = true,
                MenuKey = "employee_management",
                SearchKeywords = new List<string> { "員工", "人員", "人事", "HR", "employee", "staff" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "員工維護",
                        Description = "管理員工資料和人事資訊",
                        Route = "/employees",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "人力資源管理",
                        RequiredPermission = "Employee.Read",
                        SearchKeywords = new List<string> { "員工維護", "員工資料", "人員管理" }
                    },
                    new NavigationItem
                    {
                        Name = "部門",
                        Description = "管理公司部門組織架構",
                        Route = "/departments",
                        IconClass = "",
                        Category = "人力資源管理",
                        RequiredPermission = "Department.Read",
                        SearchKeywords = new List<string> { "部門", "組織", "department" }
                    },
                    new NavigationItem
                    {
                        Name = "職位",
                        Description = "管理員工職位設定",
                        Route = "/employee-positions",
                        IconClass = "",
                        Category = "人力資源管理",
                        RequiredPermission = "EmployeePosition.Read",
                        SearchKeywords = new List<string> { "職位", "職稱", "position" }
                    },
                    new NavigationItem
                    {
                        Name = "身分權限",
                        Description = "管理角色與權限關係",
                        Route = "/employees/role-permission-management",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "人力資源管理",
                        RequiredPermission = "Role.Read",
                        SearchKeywords = new List<string> { "身分權限", "角色權限", "role permission" }
                    },
                    new NavigationItem
                    {
                        Name = "身分",
                        Description = "管理使用者角色身分",
                        Route = "/roles",
                        IconClass = "",
                        Category = "人力資源管理",
                        RequiredPermission = "Role.Read",
                        SearchKeywords = new List<string> { "身分", "角色", "role" }
                    },
                    new NavigationItem
                    {
                        Name = "權限",
                        Description = "管理系統權限設定",
                        Route = "/permissions",
                        IconClass = "",
                        Category = "人力資源管理",
                        RequiredPermission = "Permission.Read",
                        SearchKeywords = new List<string> { "權限", "permission", "授權" }
                    }
                }
            },

            // ==================== 廠商管理 ====================
            new NavigationItem
            {
                Name = "廠商管理",
                Description = "廠商相關功能管理",
                Route = "#",
                IconClass = "bi bi-building-gear",
                Category = "供應鏈管理",
                IsParent = true,
                MenuKey = "supplier_management",
                SearchKeywords = new List<string> { "廠商", "供應商", "supplier", "vendor" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "廠商維護",
                        Description = "管理供應商和廠商資料",
                        Route = "/suppliers",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "供應鏈管理",
                        RequiredPermission = "Supplier.Read",
                        SearchKeywords = new List<string> { "廠商維護", "供應商資料", "廠商管理" }
                    },
                    new NavigationItem
                    {
                        Name = "類型",
                        Description = "管理廠商類型分類",
                        Route = "/supplier-types",
                        IconClass = "",
                        Category = "供應鏈管理",
                        RequiredPermission = "SupplierType.Read",
                        SearchKeywords = new List<string> { "廠商類型", "supplier type" }
                    }
                }
            },

            // ==================== 客戶管理 ====================
            new NavigationItem
            {
                Name = "客戶管理",
                Description = "客戶相關功能管理",
                Route = "#",
                IconClass = "bi bi-people-fill",
                Category = "客戶關係管理",
                IsParent = true,
                MenuKey = "customer_management",
                SearchKeywords = new List<string> { "客戶", "顧客", "customer", "client", "CRM" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "客戶維護",
                        Description = "管理客戶資料、聯絡資訊和客戶關係",
                        Route = "/customers",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "客戶關係管理",
                        RequiredPermission = "Customer.Read",
                        SearchKeywords = new List<string> { "客戶維護", "客戶資料", "客戶管理", "聯絡人" }
                    },
                }
            },

            // ==================== 產品管理 ====================
            new NavigationItem
            {
                Name = "產品管理",
                Description = "產品相關功能管理",
                Route = "#",
                IconClass = "bi bi-box-seam-fill",
                Category = "產品管理",
                IsParent = true,
                MenuKey = "product_management",
                SearchKeywords = new List<string> { "產品", "商品", "product", "item" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "產品維護",
                        Description = "管理產品資料和產品目錄",
                        Route = "/products",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "產品管理",
                        RequiredPermission = "Product.Read",
                        SearchKeywords = new List<string> { "產品維護", "產品資料", "商品目錄", "品項","條碼" }
                    },
                    new NavigationItem
                    {
                        Name = "類型",
                        Description = "管理產品類型分類",
                        Route = "/product-categories",
                        IconClass = "",
                        Category = "產品管理",
                        RequiredPermission = "ProductCategory.Read",
                        SearchKeywords = new List<string> { "產品類型", "產品分類", "category" }
                    },
                    new NavigationItem
                    {
                        Name = "單位",
                        Description = "管理產品計量單位",
                        Route = "/units",
                        IconClass = "",
                        Category = "產品管理",
                        RequiredPermission = "Unit.Read",
                        SearchKeywords = new List<string> { "單位", "計量單位", "unit" }
                    },
                    new NavigationItem
                    {
                        Name = "尺寸",
                        Description = "管理產品尺寸規格",
                        Route = "/sizes",
                        IconClass = "",
                        Category = "產品管理",
                        RequiredPermission = "Size.Read",
                        SearchKeywords = new List<string> { "尺寸", "規格", "size" }
                    },
                    new NavigationItem
                    {
                        Name = "物料清單",
                        Description = "管理產品的配方和組件結構",
                        Route = "/product-compositions",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "產品管理",
                        RequiredPermission = "ProductComposition.Read",
                        SearchKeywords = new List<string> { "物料清單", "BOM", "物料清單", "Bill of Materials" }
                    },
                    new NavigationItem
                    {
                        Name = "生產排程管理",
                        Description = "管理生產排程的詳細資料",
                        Route = "/production-schedules",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "產品管理",
                        RequiredPermission = "ProductionSchedule.Read",
                        SearchKeywords = new List<string> { "生產排程", "排程管理", "production schedule" }
                    }
                }
            },

            // ==================== 庫存管理 ====================
            new NavigationItem
            {
                Name = "庫存管理",
                Description = "庫存相關功能管理",
                Route = "#",
                IconClass = "bi-boxes nav-menu-bi",
                Category = "庫存管理",
                IsParent = true,
                MenuKey = "inventory_management",
                SearchKeywords = new List<string> { "庫存", "倉庫", "inventory", "warehouse", "stock" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "庫存查詢",
                        Description = "查詢和管理庫存資料",
                        Route = "/inventoryStocks",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = "InventoryStock.Read",
                        SearchKeywords = new List<string> { "庫存查詢", "庫存資料", "stock inquiry" }
                    },
                    new NavigationItem
                    {
                        Name = "倉庫",
                        Description = "管理倉庫資料和設定",
                        Route = "/warehouses",
                        IconClass = "",
                        Category = "庫存管理",
                        RequiredPermission = "Warehouse.Read",
                        SearchKeywords = new List<string> { "倉庫", "倉庫管理", "warehouse" }
                    },
                    new NavigationItem
                    {
                        Name = "倉庫內位置",
                        Description = "管理倉庫內的儲位配置",
                        Route = "/warehouseLocations",
                        IconClass = "",
                        Category = "庫存管理",
                        RequiredPermission = "WarehouseLocation.Read",
                        SearchKeywords = new List<string> { "倉庫位置", "庫位", "儲位", "location" }
                    },
                    new NavigationItem
                    {
                        Name = "庫存異動記錄",
                        Description = "查看庫存異動歷史記錄",
                        Route = "/inventoryTransactions",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = "InventoryTransaction.Read",
                        SearchKeywords = new List<string> { "庫存異動", "異動記錄", "transaction" }
                    },
                    new NavigationItem
                    {
                        Name = "領料",
                        Description = "管理物料領用作業",
                        Route = "/materialIssues",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = "MaterialIssue.Read",
                        SearchKeywords = new List<string> { "領料", "物料領用", "material issue" }
                    }
                }
            },

            // ==================== 採購管理 ====================
            new NavigationItem
            {
                Name = "採購管理",
                Description = "採購相關功能管理",
                Route = "#",
                IconClass = "bi bi-truck",
                Category = "採購管理",
                IsParent = true,
                MenuKey = "purchase_management",
                SearchKeywords = new List<string> { "採購", "進貨", "purchase", "procurement" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "採購單維護",
                        Description = "管理採購訂單",
                        Route = "/purchase/orders",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = "PurchaseOrder.Read",
                        SearchKeywords = new List<string> { "採購單", "訂購單", "purchase order", "PO" }
                    },
                    new NavigationItem
                    {
                        Name = "進貨",
                        Description = "管理進貨作業",
                        Route = "/purchase/receiving",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = "PurchaseReceiving.Read",
                        SearchKeywords = new List<string> { "進貨", "收貨", "receiving", "GR" }
                    },
                    new NavigationItem
                    {
                        Name = "進貨退出",
                        Description = "管理進貨退回作業",
                        Route = "/purchase/returns",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = "PurchaseReturn.Read",
                        SearchKeywords = new List<string> { "進貨退出", "退貨", "return", "退回" }
                    }
                }
            },

            // ==================== 銷貨管理 ====================
            new NavigationItem
            {
                Name = "銷貨管理",
                Description = "銷貨相關功能管理",
                Route = "#",
                IconClass = "bi bi-cart-fill",
                Category = "銷售管理",
                IsParent = true,
                MenuKey = "sales_management",
                SearchKeywords = new List<string> { "銷貨", "銷售", "sales", "order" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "報價單維護",
                        Description = "管理銷售報價單",
                        Route = "/quotations",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "Quotation.Read",
                        SearchKeywords = new List<string> { "報價單", "銷售報價", "quotation", "quote" }
                    },
                    new NavigationItem
                    {
                        Name = "銷貨",
                        Description = "管理銷貨訂單",
                        Route = "/salesOrders",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "SalesOrder.Read",
                        SearchKeywords = new List<string> { "銷貨單", "銷售單", "sales order", "SO" }
                    },
                    new NavigationItem
                    {
                        Name = "銷貨退回",
                        Description = "管理銷貨退回作業",
                        Route = "/salesReturns",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "SalesReturn.Read",
                        SearchKeywords = new List<string> { "銷貨退回", "退貨", "sales return" }
                    },
                    new NavigationItem
                    {
                        Name = "銷貨退回原因",
                        Description = "管理退貨原因分類",
                        Route = "/salesReturnReasons",
                        IconClass = "",
                        Category = "銷售管理",
                        RequiredPermission = "SalesReturnReason.Read",
                        SearchKeywords = new List<string> { "退回原因", "退貨原因", "return reason" }
                    }
                }
            },

            // ==================== 財務管理 ====================
            new NavigationItem
            {
                Name = "財務管理",
                Description = "財務相關功能管理",
                Route = "#",
                IconClass = "bi bi-journal-text",
                Category = "財務管理",
                IsParent = true,
                MenuKey = "financial_management",
                SearchKeywords = new List<string> { "財務", "會計", "finance", "accounting" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "應收帳款",
                        Description = "管理應收帳款沖銷",
                        Route = "/accountsReceivableSetoff",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "財務管理",
                        RequiredPermission = "SetoffDocument.Read",
                        SearchKeywords = new List<string> { "應收帳款", "AR", "receivable", "收款" }
                    },
                    new NavigationItem
                    {
                        Name = "付款方式",
                        Description = "管理付款方式設定",
                        Route = "/paymentMethods",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = "PaymentMethod.Read",
                        SearchKeywords = new List<string> { "付款方式", "payment method", "支付" }
                    },
                    new NavigationItem
                    {
                        Name = "銀行",
                        Description = "管理銀行帳戶資料",
                        Route = "/banks",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = "Bank.Read",
                        SearchKeywords = new List<string> { "銀行", "帳戶", "bank" }
                    },
                    new NavigationItem
                    {
                        Name = "貨幣",
                        Description = "管理貨幣和匯率設定",
                        Route = "/currencies",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = "Currency.Read",
                        SearchKeywords = new List<string> { "貨幣", "幣別", "currency", "匯率" }
                    },
                    new NavigationItem
                    {
                        Name = "應付帳款",
                        Description = "管理應付帳款沖銷",
                        Route = "/accountsPayableSetoff",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "財務管理",
                        RequiredPermission = "SetoffDocument.Read",
                        SearchKeywords = new List<string> { "應付帳款", "AP", "payable", "付款" }
                    }
                }
            },

            // ==================== 系統管理 ====================
            new NavigationItem
            {
                Name = "系統管理",
                Description = "系統管理和維護功能",
                Route = "#",
                IconClass = "bi bi-gear-fill",
                Category = "系統管理",
                IsParent = true,
                MenuKey = "system_management",
                SearchKeywords = new List<string> { "系統管理", "管理", "system", "admin", "administration" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "公司維護",
                        Description = "管理公司基本資料",
                        Route = "/companies",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "系統管理",
                        RequiredPermission = "Company.Read",
                        SearchKeywords = new List<string> { "公司資料", "公司設定", "company" }
                    },
                    new NavigationItem
                    {
                        Name = "系統參數",
                        Description = "管理系統參數設定",
                        Route = "/system-parameters",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "系統管理",
                        RequiredPermission = "SystemParameter.Read",
                        SearchKeywords = new List<string> { "系統參數", "參數設定", "parameter", "config" }
                    },
                    new NavigationItem
                    {
                        Name = "錯誤記錄",
                        Description = "檢視和管理系統錯誤記錄",
                        Route = "/error-logs",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "系統管理",
                        RequiredPermission = "SystemControl.Read",
                        SearchKeywords = new List<string> { "錯誤記錄", "錯誤", "log", "error", "system error" }
                    },
                    new NavigationItem
                    {
                        Name = "紙張設定",
                        Description = "管理列印紙張設定",
                        Route = "/paper-settings",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "系統管理",
                        RequiredPermission = "PaperSetting.Read",
                        SearchKeywords = new List<string> { "紙張設定", "列印設定", "paper setting" }
                    },
                    new NavigationItem
                    {
                        Name = "印表機設定",
                        Description = "管理印表機配置",
                        Route = "/printerCconfigurations",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "系統管理",
                        RequiredPermission = "PrinterSetting.Read",
                        SearchKeywords = new List<string> { "印表機", "列印設定", "printer" }
                    },
                    new NavigationItem
                    {
                        Name = "報表設定",
                        Description = "管理報表列印配置",
                        Route = "/reportPrintConfigurations",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "系統管理",
                        RequiredPermission = "ReportPrintConfiguration.Read",
                        SearchKeywords = new List<string> { "報表設定", "報表配置", "report configuration" }
                    }
                }
            }
        };
    }

    /// <summary>
    /// 取得所有選單群組（僅父級項目）
    /// 用於 NavMenu 渲染
    /// </summary>
    public static List<NavigationItem> GetMenuGroups()
    {
        return GetAllNavigationItems();
    }

    /// <summary>
    /// 取得扁平化的所有導航項目（包含子項目）
    /// 用於搜尋功能
    /// </summary>
    public static List<NavigationItem> GetFlattenedNavigationItems()
    {
        var result = new List<NavigationItem>();
        var allItems = GetAllNavigationItems();

        foreach (var item in allItems)
        {
            // 加入父級項目（如果不是純選單容器）
            if (!item.IsParent || !string.IsNullOrEmpty(item.Route) && item.Route != "#")
            {
                result.Add(item);
            }

            // 加入所有子項目
            if (item.Children.Any())
            {
                foreach (var child in item.Children)
                {
                    // 確保子項目繼承父項目的分類（如果子項目沒有設定）
                    if (string.IsNullOrEmpty(child.Category) && !string.IsNullOrEmpty(item.Category))
                    {
                        child.Category = item.Category;
                    }
                    result.Add(child);
                }
            }
        }

        return result;
    }
}
