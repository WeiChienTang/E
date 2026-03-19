using ERPCore2.Helpers;
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
                NameKey = "Nav.Home",
                Description = "系統首頁和總覽",
                Route = "/",
                IconClass = "bi-house-door-fill",
                Category = "基礎功能",
                SearchKeywords = new List<string> { "首頁", "主頁", "主畫面", "home", "dashboard", "總覽", "主页", "ホーム", "トップ", "概要" }
            },
            
            // ==================== 檔案留存 ====================
            new NavigationItem
            {
                Name = "檔案留存",
                NameKey = "Nav.DocumentGroup",
                Description = "管理與存取各類保存文件",
                Route = "#",
                IconClass = "bi bi-folder-fill",
                Category = "檔案管理",
                IsParent = true,
                MenuKey = "document_management",
                SearchKeywords = new List<string> { "檔案留存", "檔案管理", "文件管理", "document", "file", "档案管理", "ファイル管理", "書類管理" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "檔案列表",
                        NameKey = "Nav.Documents",
                        Description = "瀏覽與管理保存的各類文件",
                        Route = "/documents",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "檔案管理",
                        RequiredPermission = PermissionRegistry.Document.Read,
                        SearchKeywords = new List<string> { "檔案列表", "文件列表", "文件管理", "document", "档案列表", "ファイル一覧" },
                        QuickActionId = "NewDocument",
                        QuickActionName = "新增文件"
                    },
                    new NavigationItem
                    {
                        Name = "檔案分類",
                        NameKey = "Nav.DocumentCategories",
                        Description = "管理文件分類與來源設定",
                        Route = "/document-categories",
                        IconClass = "",
                        Category = "檔案管理",
                        RequiredPermission = PermissionRegistry.Document.Manage,
                        SearchKeywords = new List<string> { "檔案分類", "文件分類", "document category", "档案分类", "ファイル分類" },
                        QuickActionId = "NewDocumentCategory",
                        QuickActionName = "新增分類"
                    }
                }
            },

            // ==================== 人力資源管理 ====================
            new NavigationItem
            {
                Name = "人力資源",
                NameKey = "Nav.HumanResources",
                Description = "員工相關功能管理",
                Route = "#",
                IconClass = "bi bi-person-badge-fill",
                Category = "人力管理",
                IsParent = true,
                MenuKey = "employee_management",
                ModuleKey = "Employees",
                SearchKeywords = new List<string> { "員工", "人員", "人事", "HR", "employee", "staff", "人力资源", "雇员", "従業員", "人事管理" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "員工",
                        NameKey = "Nav.Employees",
                        Description = "管理員工資料和人事資訊",
                        Route = "/employees",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "人力管理",
                        RequiredPermission = PermissionRegistry.Employee.Read,
                        SearchKeywords = new List<string> { "員工管理", "員工資料", "人力管理", "员工管理", "雇员管理", "従業員管理", "スタッフ" },
                        QuickActionId = "NewEmployee",
                        QuickActionName = "新增員工"
                    },
                    new NavigationItem
                    {
                        Name = "部門",
                        NameKey = "Nav.Departments",
                        Description = "管理公司部門組織架構",
                        Route = "/departments",
                        IconClass = "",
                        Category = "人力管理",
                        RequiredPermission = PermissionRegistry.Department.Read,
                        SearchKeywords = new List<string> { "部門", "組織", "department", "部门", "组织架构", "部門管理", "部署" },
                        QuickActionId = "NewDepartment",
                        QuickActionName = "新增部門"
                    },
                    new NavigationItem
                    {
                        Name = "職位",
                        NameKey = "Nav.Positions",
                        Description = "管理員工職位設定",
                        Route = "/employee-positions",
                        IconClass = "",
                        Category = "人力管理",
                        RequiredPermission = PermissionRegistry.EmployeePosition.Read,
                        SearchKeywords = new List<string> { "職位", "職稱", "position", "职位", "职称", "役職", "ポジション", "job title" },
                        QuickActionId = "NewEmployeePosition",
                        QuickActionName = "新增職位"
                    },
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "人力報表集",
                        description: "查看和列印所有人力相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenHRReportIndex",
                        category: "人力管理",
                        requiredPermission: "Employee.Read",
                        searchKeywords: new List<string> { "人力報表", "人力報表集", "HR report", "員工名冊", "人力资源报表", "従業員レポート", "人事报表" },
                        nameKey: "Nav.HRReportIndex"
                    ),

                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    new NavigationItem
                    {
                        Name = "員工圖表",
                        NameKey = "Nav.EmployeeCharts",
                        Description = "依多維度查看員工統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenEmployeeCharts",
                        Category = "人力管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Employee.ChartRead,
                        SearchKeywords = new List<string> { "員工圖表", "員工分析", "統計分析", "人力分析", "employee chart", "HR analytics", "员工图表", "従業員分析" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 廠商管理 ====================
            new NavigationItem
            {
                Name = "廠商",
                NameKey = "Nav.SupplierGroup",
                Description = "廠商相關功能管理",
                Route = "#",
                IconClass = "bi bi-building-gear",
                Category = "供應鏈管理",
                IsParent = true,
                MenuKey = "supplier_management",
                ModuleKey = "Suppliers",
                SearchKeywords = new List<string> { "廠商", "供應商", "supplier", "vendor", "厂商", "供应商", "仕入先", "サプライヤー", "取引先" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "廠商",
                        NameKey = "Nav.Suppliers",
                        Description = "管理供應商和廠商資料",
                        Route = "/suppliers",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "供應鏈管理",
                        RequiredPermission = PermissionRegistry.Supplier.Read,
                        SearchKeywords = new List<string> { "廠商管理", "供應商資料", "廠商資料", "协力厂商", "厂商管理", "仕入先管理", "サプライヤー管理" },
                        QuickActionId = "NewSupplier",
                        QuickActionName = "新增廠商"
                    },
                    
                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    
                    NavigationActionHelper.CreateActionItem(
                        name: "廠商報表集",
                        description: "查看和列印所有廠商相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenSupplierReportIndex",
                        category: "供應鏈管理",
                        requiredPermission: "Supplier.Read",
                        searchKeywords: new List<string> { "廠商報表", "廠商報表集", "supplier report", "應付帳款", "厂商报表", "仕入先レポート", "買掛金" },
                        nameKey: "Nav.SupplierReportIndex"
                    ),
                    // 分隔線 - 區分報表與圖表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    new NavigationItem
                    {
                        Name = "廠商圖表",
                        NameKey = "Nav.SupplierCharts",
                        Description = "依多維度查看廠商統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenSupplierCharts",
                        Category = "供應鏈管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Supplier.ChartRead,
                        SearchKeywords = new List<string> { "廠商圖表", "廠商分析", "統計分析", "supplier chart", "analytics", "應付帳款", "厂商图表", "仕入先分析", "買掛分析" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 客戶管理 ====================
            new NavigationItem
            {
                Name = "客戶",
                NameKey = "Nav.CustomerGroup",
                Description = "客戶相關功能管理",
                Route = "#",
                IconClass = "bi bi-people-fill",
                Category = "客戶關係管理",
                IsParent = true,
                MenuKey = "customer_management",
                ModuleKey = "Customers",
                SearchKeywords = new List<string> { "客戶", "顧客", "customer", "client", "CRM", "客户", "顾客", "得意先", "顧客管理", "お客様" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "客戶",
                        NameKey = "Nav.Customers",
                        Description = "管理客戶資料、聯絡資訊和客戶關係",
                        Route = "/customers",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "客戶關係管理",
                        RequiredPermission = PermissionRegistry.Customer.Read,
                        SearchKeywords = new List<string> { "客戶管理", "客戶資料", "聯絡人", "客户管理", "客户资料", "得意先管理", "顧客情報", "取引先" },
                        QuickActionId = "NewCustomer",
                        QuickActionName = "新增客戶"
                    },
                    new NavigationItem
                    {
                        Name = "客訴記錄",
                        NameKey = "Nav.CustomerComplaints",
                        Description = "記錄與追蹤客戶投訴事件及處理狀態",
                        Route = "/customer-complaints",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "客戶關係管理",
                        RequiredPermission = PermissionRegistry.CustomerComplaint.Read,
                        SearchKeywords = new List<string> { "客訴", "投訴", "客戶投訴", "complaint", "客诉", "クレーム", "苦情" }
                    },
                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    NavigationActionHelper.CreateActionItem(
                        name: "客戶報表集",
                        description: "查看和列印所有客戶相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenCustomerReportIndex",
                        category: "客戶關係管理",
                        requiredPermission: "Customer.Read",
                        searchKeywords: new List<string> { "客戶報表", "客戶報表集", "customer report", "應收帳款", "客户报表", "得意先レポート", "売掛金" },
                        nameKey: "Nav.CustomerReportIndex"
                    ),
                    // 分隔線 - 區分報表與圖表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    new NavigationItem
                    {
                        Name = "客戶圖表",
                        NameKey = "Nav.CustomerCharts",
                        Description = "依多維度查看客戶統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenCustomerCharts",
                        Category = "客戶關係管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Customer.ChartRead,
                        SearchKeywords = new List<string> { "客戶圖表", "客戶分析", "統計分析", "customer chart", "analytics", "客户图表", "得意先分析", "売掛分析" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 品項管理 ====================
            new NavigationItem
            {
                Name = "品項",
                NameKey = "Nav.ItemGroup",
                Description = "品項相關功能管理",
                Route = "#",
                IconClass = "bi bi-box-seam-fill",
                Category = "品項管理",
                IsParent = true,
                MenuKey = "product_management",
                ModuleKey = "Items",
                SearchKeywords = new List<string> { "品項", "product", "item", "产品", "物品", "品項管理", "品目", "アイテム" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "品項",
                        NameKey = "Nav.Items",
                        Description = "管理品項資料和品項目錄",
                        Route = "/items",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.Item.Read,
                        SearchKeywords = new List<string> { "品項管理", "品項資料", "品項目錄", "品項", "條碼", "SKU", "品项编码", "産品管理", "品番", "barcode" },
                        QuickActionId = "NewItem",
                        QuickActionName = "新增品項"
                    },
                    new NavigationItem
                    {
                        Name = "類型",
                        NameKey = "Nav.ItemCategories",
                        Description = "管理品項類型分類",
                        Route = "/item-categories",
                        IconClass = "",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.ItemCategory.Read,
                        SearchKeywords = new List<string> { "品項類型", "品項分類", "category", "品项类型", "産品分類", "品目カテゴリ" },
                        QuickActionId = "NewItemCategory",
                        QuickActionName = "新增品項類型"
                    },
                    new NavigationItem
                    {
                        Name = "單位",
                        NameKey = "Nav.Units",
                        Description = "管理品項計量單位",
                        Route = "/units",
                        IconClass = "",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.Unit.Read,
                        SearchKeywords = new List<string> { "單位", "計量單位", "unit", "单位", "计量单位", "UOM", "単位", "数量単位" },
                        QuickActionId = "NewUnit",
                        QuickActionName = "新增單位"
                    },

                    new NavigationItem
                    {
                        Name = "尺寸",
                        NameKey = "Nav.Sizes",
                        Description = "管理品項尺寸規格",
                        Route = "/sizes",
                        IconClass = "",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.Size.Read,
                        SearchKeywords = new List<string> { "尺寸", "規格", "size", "尺码", "规格", "サイズ", "寸法", "spec" },
                        QuickActionId = "NewSize",
                        QuickActionName = "新增尺寸"
                    },
                    new NavigationItem
                    {
                        Name = "物料清單",
                        NameKey = "Nav.ItemCompositions",
                        Description = "管理品項的配方和組件結構",
                        Route = "/item-compositions",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.ItemComposition.Read,
                        SearchKeywords = new List<string> { "品項合成", "BOM", "物料清單", "Bill of Materials", "物料清单", "配方", "部品表", "構成部品", "配合表" },
                        QuickActionId = "NewItemComposition",
                        QuickActionName = "新增物料清單"
                    },
                    new NavigationItem
                    {
                        Name = "物料類型",
                        NameKey = "Nav.CompositionCategories",
                        Description = "管理物料清單的類型分類",
                        Route = "/composition-categories",
                        IconClass = "",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.CompositionCategory.Read,
                        SearchKeywords = new List<string> { "物料清單類型", "BOM類型", "category", "物料类型", "部品表分類", "配方分類" },
                        QuickActionId = "NewCompositionCategory",
                        QuickActionName = "新增物料清單類型"
                    },

                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "生產排程",
                        description: "管理生產排程的詳細資料",
                        iconClass: "bi bi-kanban",
                        actionId: "OpenProductionScheduleBoard",
                        category: "品項管理",
                        requiredPermission: PermissionRegistry.ProductionSchedule.Read,
                        searchKeywords: new List<string> { "生產排程", "排程管理", "production schedule", "生产排程", "生産計画", "製造スケジュール", "工単", "工作排程" },
                        nameKey: "Nav.ProductionSchedules"
                    ),
                    new NavigationItem
                    {
                        Name = "製令單",
                        NameKey = "Nav.ManufacturingOrders",
                        Description = "查詢與列印製令單",
                        IconClass = "bi bi-file-earmark-text",
                        Route = "/production/manufacturing-orders",
                        Category = "品項管理",
                        RequiredPermission = PermissionRegistry.ProductionSchedule.Read,
                        SearchKeywords = new List<string> { "製令單", "manufacturing order", "工单", "製造命令", "製造指示" }
                    },

                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    
                    NavigationActionHelper.CreateActionItem(
                        name: "品項報表集",
                        description: "查看和列印所有品項相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenItemReportIndex",
                        category: "品項管理",
                        requiredPermission: "Item.Read",
                        searchKeywords: new List<string> { "品項報表", "品項報表集", "product report", "產品報表", "品项报表", "産品レポート" },
                        nameKey: "Nav.ItemReportIndex"
                    ),

                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    new NavigationItem
                    {
                        Name = "品項圖表",
                        NameKey = "Nav.ItemCharts",
                        Description = "依多維度查看品項銷售與成本統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenItemCharts",
                        Category = "品項管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Item.ChartRead,
                        SearchKeywords = new List<string> { "品項圖表", "品項分析", "統計分析", "product chart", "item analytics", "品项图表", "品目分析" },
                        IsChartWidget = true
                    },
                    new NavigationItem
                    {
                        Name = "生產圖表",
                        NameKey = "Nav.ProductionCharts",
                        Description = "依多維度查看生產排程與製令統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenProductionCharts",
                        Category = "品項管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.ProductionSchedule.ChartRead,
                        SearchKeywords = new List<string> { "生產圖表", "製令分析", "生產統計", "production chart", "manufacturing analytics", "生产图表", "生産チャート" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 庫存管理 ====================
            new NavigationItem
            {
                Name = "庫存",
                NameKey = "Nav.InventoryGroup",
                Description = "庫存相關功能管理",
                Route = "#",
                IconClass = "bi-boxes nav-menu-bi",
                Category = "庫存管理",
                IsParent = true,
                MenuKey = "inventory_management",
                ModuleKey = "Warehouse",
                SearchKeywords = new List<string> { "庫存", "倉庫", "inventory", "warehouse", "stock", "库存", "仓库", "在庫", "倉庫管理", "存货" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "庫存查詢",
                        NameKey = "Nav.InventoryStocks",
                        Description = "查詢和管理庫存資料",
                        Route = "/inventoryStocks",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = PermissionRegistry.InventoryStock.Read,
                        SearchKeywords = new List<string> { "庫存查詢", "庫存資料", "stock inquiry", "库存查询", "庫存一覧", "在庫照会", "現有庫存", "即時庫存" },
                        QuickActionId = "NewInventoryStock",
                        QuickActionName = "新增庫存查詢"
                    },
                    new NavigationItem
                    {
                        Name = "倉庫",
                        NameKey = "Nav.Warehouses",
                        Description = "管理倉庫資料和設定",
                        Route = "/warehouses",
                        IconClass = "",
                        Category = "庫存管理",
                        RequiredPermission = PermissionRegistry.Warehouse.Read,
                        SearchKeywords = new List<string> { "倉庫", "倉庫管理", "warehouse", "仓库", "仓库管理", "倉庫設定", "物流倉庫" },
                        QuickActionId = "NewWarehouse",
                        QuickActionName = "新增倉庫"
                    },
                    new NavigationItem
                    {
                        Name = "庫位",
                        NameKey = "Nav.WarehouseLocations",
                        Description = "管理倉庫內的儲位配置",
                        Route = "/warehouseLocations",
                        IconClass = "",
                        Category = "庫存管理",
                        RequiredPermission = PermissionRegistry.WarehouseLocation.Read,
                        SearchKeywords = new List<string> { "倉庫位置", "庫位", "儲位", "location", "仓位", "储位", "棚番", "ロケーション", "棚管理" },
                        QuickActionId = "NewWarehouseLocation",
                        QuickActionName = "新增庫位"
                    },
                    new NavigationItem
                    {
                        Name = "庫存盤點",
                        NameKey = "Nav.StockTakings",
                        Description = "管理庫存盤點作業",
                        Route = "/stockTakings",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = PermissionRegistry.StockTaking.Read,
                        SearchKeywords = new List<string> { "庫存盤點", "盤點作業", "stock taking", "inventory audit", "库存盘点", "盘点", "棚卸", "実地棚卸", "盤點清冊" },
                        QuickActionId = "NewStockTaking",
                        QuickActionName = "新增庫存盤點"
                    },
                    // 暫時關閉，因為目前意義不明，尚無法明確表示任何有意義的資訊
                    // new NavigationItem
                    // {
                    //     Name = "庫存異動記錄",
                    //     Description = "查看庫存異動歷史記錄",
                    //     Route = "/inventoryTransactions",
                    //     IconClass = "bi bi-caret-right-fill",
                    //     Category = "庫存管理",
                    //     RequiredPermission = PermissionRegistry.InventoryTransaction.Read,
                    //     SearchKeywords = new List<string> { "庫存異動", "異動記錄", "transaction" }
                    // },
                    new NavigationItem
                    {
                        Name = "領料",
                        NameKey = "Nav.MaterialIssues",
                        Description = "管理物料領用作業",
                        Route = "/materialIssues",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = PermissionRegistry.MaterialIssue.Read,
                        SearchKeywords = new List<string> { "領料", "物料領用", "material issue", "领料", "出库", "材料领用", "出庫", "払出", "材料出庫", "用料" },
                        QuickActionId = "NewMaterialIssue",
                        QuickActionName = "新增領料"
                    },

                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "倉庫報表集",
                        description: "查看和列印所有倉庫庫存相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenInventoryReportIndex",
                        category: "庫存管理",
                        requiredPermission: "InventoryStock.Read",
                        searchKeywords: new List<string> { "倉庫報表", "庫存報表", "倉庫報表集", "inventory report", "庫存現況", "库存报表", "在庫レポート", "庫存盤點報表" },
                        nameKey: "Nav.InventoryReportIndex"
                    ),

                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    new NavigationItem
                    {
                        Name = "庫存圖表",
                        NameKey = "Nav.InventoryCharts",
                        Description = "依多維度查看庫存統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenInventoryCharts",
                        Category = "庫存管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Inventory.ChartRead,
                        SearchKeywords = new List<string> { "庫存圖表", "庫存分析", "統計分析", "inventory chart", "stock analytics", "库存图表", "在庫分析" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 採購管理 ====================
            new NavigationItem
            {
                Name = "採購",
                NameKey = "Nav.PurchaseGroup",
                Description = "採購相關功能管理",
                Route = "#",
                IconClass = "bi bi-truck",
                Category = "採購管理",
                IsParent = true,
                MenuKey = "purchase_management",
                ModuleKey = "Purchase",
                SearchKeywords = new List<string> { "採購", "進貨", "purchase", "procurement", "采购", "进货", "仕入", "購買管理", "調達" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "採購",
                        NameKey = "Nav.PurchaseOrders",
                        Description = "管理採購訂單",
                        Route = "/purchase/orders",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = PermissionRegistry.PurchaseOrder.Read,
                        SearchKeywords = new List<string> { "採購單", "訂購單", "purchase order", "PO", "采购单", "订购单", "発注", "発注書", "仕入注文" },
                        // QuickAction: 在首頁快速開啟採購單新增 Modal
                        QuickActionId = "NewPurchaseOrder",
                        QuickActionName = "新增採購單",
                    },
                    new NavigationItem
                    {
                        Name = "進貨",
                        NameKey = "Nav.PurchaseReceiving",
                        Description = "管理進貨作業",
                        Route = "/purchase/receiving",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = PermissionRegistry.PurchaseReceiving.Read,
                        SearchKeywords = new List<string> { "進貨", "入庫", "收貨", "receiving", "GR", "进货", "入库", "到貨", "驗收", "納品", "仕入入庫", "收料" },
                        QuickActionId = "NewPurchaseReceiving",
                        QuickActionName = "新增進貨單"
                    },
                    new NavigationItem
                    {
                        Name = "進貨退出",
                        NameKey = "Nav.PurchaseReturns",
                        Description = "管理進貨退回作業",
                        Route = "/purchase/returns",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = PermissionRegistry.PurchaseReturn.Read,
                        SearchKeywords = new List<string> { "進貨退出", "退貨", "return", "退回", "进货退出", "退货单", "仕入返品", "返品", "退料", "退貨單" },
                        QuickActionId = "NewPurchaseReturn",
                        QuickActionName = "新增進貨退出單"
                    },
                    new NavigationItem
                    {
                        Name = "退出原因",
                        NameKey = "Nav.PurchaseReturnReasons",
                        Description = "管理進貨退出原因分類",
                        Route = "/purchase/returnReasons",
                        IconClass = "",
                        Category = "採購管理",
                        RequiredPermission = PermissionRegistry.PurchaseReturnReason.Read,
                        SearchKeywords = new List<string> { "退出原因", "退貨原因", "進退原因", "purchase return reason", "退货原因", "返品理由", "退貨分類" },
                        QuickActionId = "NewPurchaseReturnReason",
                        QuickActionName = "新增進貨退出原因"
                    },

                    new NavigationItem
                    {
                        IsDivider = true
                    },                 

                    NavigationActionHelper.CreateActionItem(
                        name: "採購報表集",
                        description: "查看和列印所有採購相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenPurchaseReportIndex",
                        category: "採購管理",
                        requiredPermission: "PurchaseOrder.Read",
                        searchKeywords: new List<string> { "採購報表", "採購報表集", "purchase report", "進貨報表", "采购报表", "仕入レポート", "進貨統計" },
                        nameKey: "Nav.PurchaseReportIndex"
                    ),

                                        // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    
                    new NavigationItem
                    {
                        Name = "採購圖表",
                        NameKey = "Nav.PurchaseCharts",
                        Description = "依多維度查看採購統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenPurchaseCharts",
                        Category = "採購管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.PurchaseOrder.ChartRead,
                        SearchKeywords = new List<string> { "採購圖表", "採購分析", "統計分析", "purchase chart", "procurement analytics", "采购图表", "仕入分析" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 銷貨管理 ====================
            new NavigationItem
            {
                Name = "銷貨",
                NameKey = "Nav.SalesGroup",
                Description = "銷貨相關功能管理",
                Route = "#",
                IconClass = "bi bi-cart-fill",
                Category = "銷售管理",
                IsParent = true,
                MenuKey = "sales_management",
                ModuleKey = "Sales",
                SearchKeywords = new List<string> { "銷貨", "銷售", "sales", "order", "销货", "销售", "売上", "販売管理", "受注管理" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "報價",
                        NameKey = "Nav.Quotations",
                        Description = "管理銷售報價單",
                        Route = "/quotations",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = PermissionRegistry.Quotation.Read,
                        SearchKeywords = new List<string> { "報價單", "銷售報價", "quotation", "quote", "报价单", "见积", "見積", "見積書", "估价单" },
                        QuickActionId = "NewQuotation",
                        QuickActionName = "新增報價單"
                    },
                    new NavigationItem
                    {
                        Name = "訂單",
                        NameKey = "Nav.SalesOrders",
                        Description = "管理訂單",
                        Route = "/salesOrders",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = PermissionRegistry.SalesOrder.Read,
                        SearchKeywords = new List<string> { "訂單", "sales order", "SO", "订单", "受注", "受注書", "客戶訂單", "接單" },
                        // QuickAction: 在首頁快速開啟訂單新增 Modal
                        QuickActionId = "NewSalesOrder",
                        QuickActionName = "新增訂單",
                    },
                    new NavigationItem
                    {
                        Name = "銷貨",
                        NameKey = "Nav.SalesDeliveries",
                        Description = "管理銷貨出貨作業",
                        Route = "/salesDeliveries",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = PermissionRegistry.SalesDelivery.Read,
                        SearchKeywords = new List<string> { "出貨", "銷貨單", "delivery", "銷貨", "出货", "销货单", "出荷", "出荷指示", "發貨", "配送" },
                        QuickActionId = "NewSalesDelivery",
                        QuickActionName = "新增銷貨單"
                    },
                    new NavigationItem
                    {
                        Name = "銷貨退回",
                        NameKey = "Nav.SalesReturns",
                        Description = "管理銷貨退回作業",
                        Route = "/salesReturns",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = PermissionRegistry.SalesReturn.Read,
                        SearchKeywords = new List<string> { "銷貨退回", "退貨", "sales return", "销货退回", "退货", "売上返品", "返品", "退回品" },
                        QuickActionId = "NewSalesReturn",
                        QuickActionName = "新增銷貨退回單"
                    },
                    new NavigationItem
                    {
                        Name = "銷退原因",
                        NameKey = "Nav.SalesReturnReasons",
                        Description = "管理退貨原因分類",
                        Route = "/salesReturnReasons",
                        IconClass = "",
                        Category = "銷售管理",
                        RequiredPermission = PermissionRegistry.SalesReturnReason.Read,
                        SearchKeywords = new List<string> { "退回原因", "退貨原因", "return reason", "退货原因", "返品理由", "銷退分類" },
                        QuickActionId = "NewSalesReturnReason",
                        QuickActionName = "新增銷貨退回原因"
                    },

                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    new NavigationItem
                    {
                        Name = "業績目標",
                        NameKey = "Nav.SalesTargets",
                        Description = "設定與追蹤業務員銷售目標",
                        Route = "/sales-targets",
                        IconClass = "bi bi-trophy",
                        Category = "銷售管理",
                        RequiredPermission = PermissionRegistry.SalesTarget.Read,
                        SearchKeywords = new List<string> { "業績目標", "銷售目標", "KPI", "目標金額", "sales target" },
                        QuickActionId = "NewSalesTarget",
                        QuickActionName = "新增業績目標"
                    },

                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    
                    new NavigationItem
                    {
                        Name = "銷貨圖表",
                        NameKey = "Nav.SalesCharts",
                        Description = "依多維度查看銷貨統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenSalesCharts",
                        Category = "銷售管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Sales.ChartRead,
                        SearchKeywords = new List<string> { "銷貨圖表", "銷售分析", "業績分析", "sales chart", "sales analytics", "销货图表", "売上分析", "業績圖表" },
                        IsChartWidget = true
                    },

                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "銷貨報表集",
                        description: "查看和列印所有銷貨相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenSalesReportIndex",
                        category: "銷售管理",
                        requiredPermission: "SalesOrder.Read",
                        searchKeywords: new List<string> { "銷貨報表", "銷貨報表集", "sales report", "銷售報表", "销货报表", "売上レポート", "業績報表" },
                        nameKey: "Nav.SalesReportIndex"
                    ),
                }
            },

            // ==================== 車輛管理 ====================
            new NavigationItem
            {
                Name = "車輛",
                NameKey = "Nav.VehicleGroup",
                Description = "車輛相關功能管理",
                Route = "#",
                IconClass = "bi bi-truck-front-fill",
                Category = "車輛管理",
                IsParent = true,
                MenuKey = "vehicle_management",
                ModuleKey = "Vehicles",
                SearchKeywords = new List<string> { "車輛", "車輛管理", "vehicle", "fleet", "車隊", "车辆", "车队", "車両管理", "フリート" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "車輛",
                        NameKey = "Nav.Vehicles",
                        Description = "管理車輛基本資料",
                        Route = "/vehicles",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "車輛管理",
                        RequiredPermission = PermissionRegistry.Vehicle.Read,
                        SearchKeywords = new List<string> { "車輛管理", "車輛資料", "vehicle management", "车辆管理", "車両", "社用車", "配送車" },
                        QuickActionId = "NewVehicle",
                        QuickActionName = "新增車輛"
                    },
                    new NavigationItem
                    {
                        Name = "車型",
                        NameKey = "Nav.VehicleTypes",
                        Description = "管理車輛類型設定",
                        Route = "/vehicle-types",
                        IconClass = "",
                        Category = "車輛管理",
                        RequiredPermission = PermissionRegistry.VehicleType.Read,
                        SearchKeywords = new List<string> { "車型", "車輛類型", "vehicle type", "车型", "车辆类型", "車種", "車両種別" },
                        QuickActionId = "NewVehicleType",
                        QuickActionName = "新增車型"
                    },
                    new NavigationItem
                    {
                        Name = "保養紀錄",
                        NameKey = "Nav.VehicleMaintenances",
                        Description = "管理車輛保養與維修紀錄",
                        Route = "/vehicle-maintenances",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "車輛管理",
                        RequiredPermission = PermissionRegistry.VehicleMaintenance.Read,
                        SearchKeywords = new List<string> { "保養紀錄", "維修紀錄", "vehicle maintenance", "保养记录", "维修记录", "整備記録", "点検", "車両保養", "修車" },
                        QuickActionId = "NewVehicleMaintenance",
                        QuickActionName = "新增保養紀錄"
                    },

                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "車輛報表集",
                        description: "查看和列印所有車輛相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenVehicleReportIndex",
                        category: "車輛管理",
                        requiredPermission: "Vehicle.Read",
                        searchKeywords: new List<string> { "車輛報表", "車輛報表集", "vehicle report", "保養報表", "车辆报表", "車両レポート" },
                        nameKey: "Nav.VehicleReportIndex"
                    ),
                    new NavigationItem
                    {
                        Name = "車輛圖表",
                        NameKey = "Nav.VehicleCharts",
                        Description = "依多維度查看車輛保養費用與狀態統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenVehicleCharts",
                        Category = "車輛管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Vehicle.ChartRead,
                        SearchKeywords = new List<string> { "車輛圖表", "保養分析", "車輛統計", "vehicle chart", "vehicle analytics", "车辆图表", "車両チャート" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 磅秤管理 ====================
            new NavigationItem
            {
                Name = "磅秤紀錄",
                NameKey = "Nav.WasteGroup",
                Description = "磅秤收料紀錄與類型管理",
                Route = "#",
                IconClass = "bi bi-truck-flatbed",
                Category = "磅秤管理",
                IsParent = true,
                MenuKey = "scale_management",
                ModuleKey = "ScaleManagement",
                SearchKeywords = new List<string> { "磅秤紀錄", "磅秤管理", "磅秤", "waste", "scale", "weighing", "磅秤管理", "磅秤记录", "計量", "磅秤回收" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "磅秤紀錄",
                        NameKey = "Nav.WasteRecords",
                        Description = "管理磅秤收料紀錄",
                        Route = "/scale-records",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "磅秤管理",
                        RequiredPermission = PermissionRegistry.ScaleRecord.Read,
                        SearchKeywords = new List<string> { "磅秤紀錄", "磅秤記錄", "收料記錄", "waste record", "scale record", "計量記録", "收磅" },
                        QuickActionId = "NewScaleRecord",
                        QuickActionName = "新增磅秤紀錄"
                    },
                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "磅秤報表集",
                        description: "查看和列印所有磅秤相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenScaleReportIndex",
                        category: "磅秤管理",
                        requiredPermission: "ScaleRecord.Read",
                        searchKeywords: new List<string> { "磅秤報表", "磅秤報表集", "scale report", "磅秤报表", "計量レポート" },
                        nameKey: "Nav.WasteReportIndex"
                    ),

                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    new NavigationItem
                    {
                        Name = "磅秤圖表",
                        NameKey = "Nav.ScaleCharts",
                        Description = "依多維度查看過磅重量與收益統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenScaleCharts",
                        Category = "磅秤管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.ScaleRecord.ChartRead,
                        SearchKeywords = new List<string> { "磅秤圖表", "過磅分析", "磅秤統計", "scale chart", "weighing analytics", "磅秤图表", "計量チャート" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 財務管理 ====================
            new NavigationItem
            {
                Name = "財務",
                NameKey = "Nav.FinanceGroup",
                Description = "財務相關功能管理",
                Route = "#",
                IconClass = "bi bi-journal-text",
                Category = "財務管理",
                IsParent = true,
                MenuKey = "financial_management",
                ModuleKey = "FinancialManagement",
                SearchKeywords = new List<string> { "財務", "會計", "finance", "accounting", "财务", "财务管理", "財務管理", "ファイナンス", "経理" },
                Children = new List<NavigationItem>
                {

                    new NavigationItem
                    {
                        Name = "應收沖款",
                        NameKey = "Nav.ARSetoff",
                        Description = "管理應收帳款沖款",
                        Route = "/accountsReceivableSetoff",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "財務管理",
                        RequiredPermission = PermissionRegistry.SetoffDocument.Read,
                        SearchKeywords = new List<string> { "應收帳款", "AR", "receivable", "收款", "沖款", "应收账款", "売掛金", "売掛消込", "收款沖帳", "核銷" },
                        QuickActionId = "NewARSetoff",
                        QuickActionName = "新增應收沖款"
                    },
                    new NavigationItem
                    {
                        Name = "應付沖款",
                        NameKey = "Nav.APSetoff",
                        Description = "管理應付帳款沖款",
                        Route = "/accountsPayableSetoff",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "財務管理",
                        RequiredPermission = PermissionRegistry.SetoffDocument.Read,
                        SearchKeywords = new List<string> { "應付帳款", "AP", "payable", "付款", "沖款", "应付账款", "買掛金", "買掛消込", "付款沖帳", "核銷" },
                        QuickActionId = "NewAPSetoff",
                        QuickActionName = "新增應付沖款"
                    },
                    new NavigationItem
                    {
                        Name = "付款方式",
                        NameKey = "Nav.PaymentMethods",
                        Description = "管理付款方式設定",
                        Route = "/paymentMethods",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = PermissionRegistry.PaymentMethod.Read,
                        SearchKeywords = new List<string> { "付款方式", "payment method", "支付", "付款方式", "支払方法", "決済方法", "收款方式", "金流" },
                        QuickActionId = "NewPaymentMethod",
                        QuickActionName = "新增付款方式"
                    },
                    new NavigationItem
                    {
                        Name = "銀行",
                        NameKey = "Nav.Banks",
                        Description = "管理銀行帳戶資料",
                        Route = "/banks",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = PermissionRegistry.Bank.Read,
                        SearchKeywords = new List<string> { "銀行", "帳戶", "bank", "银行", "账户", "銀行口座", "バンク", "銀行帳號" },
                        QuickActionId = "NewBank",
                        QuickActionName = "新增銀行"
                    },
                    new NavigationItem
                    {
                        Name = "貨幣",
                        NameKey = "Nav.Currencies",
                        Description = "管理貨幣和匯率設定",
                        Route = "/currencies",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = PermissionRegistry.Currency.Read,
                        SearchKeywords = new List<string> { "貨幣", "幣別", "currency", "匯率", "货币", "币别", "通貨", "為替レート", "外幣", "外币" },
                        QuickActionId = "NewCurrency",
                        QuickActionName = "新增貨幣"
                    },
                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "財務報表集",
                        description: "查看和列印所有財務相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenFinancialReportIndex",
                        category: "財務管理",
                        requiredPermission: "SetoffDocument.Read",
                        searchKeywords: new List<string> { "財務報表", "財務報表集", "financial report", "沖款單報表", "應收沖款", "應付沖款", "财务报表", "経理レポート", "帳款報表" },
                        nameKey: "Nav.FinancialReportIndex"
                    ),
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    new NavigationItem
                    {
                        Name = "財務圖表",
                        NameKey = "Nav.FinancialCharts",
                        Description = "依多維度查看沖款與傳票統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenFinancialCharts",
                        Category = "財務管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.SetoffDocument.ChartRead,
                        SearchKeywords = new List<string> { "財務圖表", "沖款分析", "財務統計", "financial chart", "financial analytics", "财务图表", "財務チャート" },
                        IsChartWidget = true
                    },

                }
            },

            // ==================== 會計管理 ====================
            new NavigationItem
            {
                Name = "會計",
                NameKey = "Nav.AccountingGroup",
                Description = "會計相關功能管理",
                Route = "#",
                IconClass = "bi bi-calculator",
                Category = "會計管理",
                IsParent = true,
                MenuKey = "accounting_management",
                ModuleKey = "Accounting",
                SearchKeywords = new List<string> { "會計", "傳票", "科目", "accounting", "journal", "会计", "传票", "簿記", "経理", "記帳" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "傳票管理",
                        NameKey = "Nav.JournalEntries",
                        Description = "管理會計傳票（日記帳分錄）",
                        Route = "/journal-entries",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "會計管理",
                        RequiredPermission = PermissionRegistry.JournalEntry.Read,
                        SearchKeywords = new List<string> { "傳票", "日記帳", "分錄", "journal entry", "會計分錄", "传票", "日记账", "伝票", "仕訳", "仕訳帳", "複式記帳" },
                        QuickActionId = "NewJournalEntry",
                        QuickActionName = "新增傳票"
                    },                    
                    // 批次轉傳票已整合至傳票管理頁面 (/journal-entries) 的 Modal 中
                    new NavigationItem
                    {
                        Name = "期初餘額",
                        NameKey = "Nav.OpeningBalance",
                        Description = "輸入各科目的期初餘額（開帳設定），每個會計年度只需輸入一次",
                        Route = "/opening-balance",
                        IconClass = "",
                        Category = "會計管理",
                        RequiredPermission = PermissionRegistry.JournalEntry.Read,
                        SearchKeywords = new List<string> { "期初餘額", "開帳", "期初", "opening balance", "期首残高", "期初余额" }
                    },
                    new NavigationItem
                    {
                        Name = "會計期間管理",
                        NameKey = "Nav.FiscalPeriods",
                        Description = "管理會計期間（關帳、鎖定，控制可開立傳票的期間範圍）",
                        Route = "/fiscal-periods",
                        IconClass = "",
                        Category = "會計管理",
                        RequiredPermission = PermissionRegistry.FiscalPeriod.Read,
                        SearchKeywords = new List<string> { "會計期間", "關帳", "鎖定期間", "fiscal period", "会计期间", "会計期間", "決算", "期間管理" }
                    },
                    new NavigationItem
                    {
                        Name = "會計科目",
                        NameKey = "Nav.AccountItems",
                        Description = "管理標準會計科目表（Chart of Accounts）",
                        Route = "/account-items",
                        IconClass = "",
                        Category = "會計管理",
                        RequiredPermission = PermissionRegistry.AccountItem.Read,
                        SearchKeywords = new List<string> { "會計科目", "科目表", "chart of accounts", "會計", "会计科目", "科目管理", "勘定科目", "COA", "科目一覧" },
                        QuickActionId = "NewAccountItem",
                        QuickActionName = "新增會計科目"
                    },


                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "會計報表集",
                        description: "查看和列印所有會計相關報表（科目表、試算表、損益表、資產負債表、總分類帳、明細分類帳、明細科目餘額表）",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenAccountingReportIndex",
                        category: "會計管理",
                        requiredPermission: "JournalEntry.Read",
                        searchKeywords: new List<string> { "會計報表", "試算表", "損益表", "資產負債表", "科目表", "總分類帳", "明細分類帳", "明細科目餘額表", "accounting report", "会计报表", "试算表", "损益表", "资产负债表", "財務諸表", "P&L", "B/S" },
                        nameKey: "Nav.AccountingReportIndex"
                    ),
                }
            },

            // ==================== 薪資管理 ====================
            new NavigationItem
            {
                Name = "薪資管理",
                NameKey = "Nav.PayrollGroup",
                Description = "薪資相關功能管理",
                Route = "#",
                IconClass = "bi bi-cash-coin",
                Category = "薪資管理",
                IsParent = true,
                MenuKey = "payroll_management",
                ModuleKey = "Payroll",
                SearchKeywords = new List<string> { "薪資", "薪水", "payroll", "salary", "工資", "薪资", "工资", "給与", "給料", "給与計算" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "薪資計算作業",
                        NameKey = "Nav.PayrollCalculation",
                        Description = "每月薪資計算、確認、關帳作業",
                        Route = "/payroll",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "薪資管理",
                        RequiredPermission = PermissionRegistry.Payroll.Calculate,
                        SearchKeywords = new List<string> { "薪資計算", "發薪", "關帳", "salary calculation", "薪资计算", "发薪", "給与計算", "月結薪資", "薪資結算" }
                    },
                    new NavigationItem
                    {
                        Name = "員工薪資設定",
                        NameKey = "Nav.PayrollSalaryConfig",
                        Description = "設定員工本薪、津貼、勞健保等薪資參數",
                        Route = "/payroll/salary-config",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "薪資管理",
                        RequiredPermission = PermissionRegistry.Payroll.SalaryConfig,
                        SearchKeywords = new List<string> { "薪資設定", "員工薪資", "本薪", "salary config", "薪资设置", "给与設定", "底薪", "津貼", "薪酬" }
                    },
                    new NavigationItem
                    {
                        Name = "薪資項目設定",
                        NameKey = "Nav.PayrollItems",
                        Description = "維護薪資收入與扣除項目",
                        Route = "/payroll/items",
                        IconClass = "",
                        Category = "薪資管理",
                        RequiredPermission = PermissionRegistry.Payroll.RateTable,
                        SearchKeywords = new List<string> { "薪資項目", "薪資科目", "payroll item", "薪资项目", "給与項目", "加給", "扣除項目", "薪資明細" }
                    },
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    new NavigationItem
                    {
                        Name = "薪資圖表",
                        NameKey = "Nav.PayrollCharts",
                        Description = "依多維度查看薪資支出與部門分布統計分析圖表",
                        IconClass = "bi bi-bar-chart-fill",
                        ItemType = NavigationItemType.Action,
                        ActionId = "OpenPayrollCharts",
                        Category = "薪資管理",
                        ModuleKey = "Charts",
                        RequiredPermission = PermissionRegistry.Payroll.ChartRead,
                        SearchKeywords = new List<string> { "薪資圖表", "薪資分析", "薪資統計", "payroll chart", "salary analytics", "薪资图表", "給与チャート" },
                        IsChartWidget = true
                    },
                }
            },

            // ==================== 系統管理 ====================
            new NavigationItem
            {
                Name = "系統",
                NameKey = "Nav.SystemGroup",
                Description = "系統管理和管理功能",
                Route = "#",
                IconClass = "bi bi-gear-fill",
                Category = "系統管理",
                IsParent = true,
                MenuKey = "system_management",
                SearchKeywords = new List<string> { "系統管理", "管理", "system", "admin", "administration", "系统管理", "システム管理", "管理機能" },
                Children = new List<NavigationItem>
                {
                    NavigationActionHelper.CreateActionItem(
                        name: "系統參數",
                        description: "管理系統參數設定",
                        iconClass: "bi bi-caret-right-fill",
                        actionId: "OpenSystemParameterSettings",
                        category: "系統管理",
                        requiredPermission: "SystemParameter.Read",
                        searchKeywords: new List<string> { "系統參數", "參數設定", "parameter", "config", "設定", "系统参数", "システムパラメータ", "環境設定" },
                        nameKey: "Nav.SystemParameters"
                    ),

                    // ── 存取控制 ──────────────────────────────
                    new NavigationItem { IsDivider = true },

                    NavigationActionHelper.CreateActionItem(
                        name: "權限分配",
                        description: "管理權限組與權限關係",
                        iconClass: "bi bi-caret-right-fill",
                        actionId: "OpenRolePermissionManagement",
                        category: "系統管理",
                        requiredPermission: "Role.Read",
                        searchKeywords: new List<string> { "權限分配", "角色權限", "role permission", "權限設定", "权限分配", "権限設定", "アクセス権" },
                        nameKey: "Nav.RolePermission"
                    ),
                    new NavigationItem
                    {
                        Name = "權限組",
                        NameKey = "Nav.Roles",
                        Description = "管理使用者權限組",
                        Route = "/roles",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = PermissionRegistry.Role.Read,
                        SearchKeywords = new List<string> { "權限組", "角色", "role", "权限组", "角色管理", "ロール", "権限グループ" },
                        QuickActionId = "NewRole",
                        QuickActionName = "新增權限組"
                    },
                    new NavigationItem
                    {
                        Name = "權限",
                        NameKey = "Nav.Permissions",
                        Description = "管理系統權限設定",
                        Route = "/permissions",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = PermissionRegistry.Permission.Read,
                        SearchKeywords = new List<string> { "權限", "permission", "授權", "权限", "権限", "アクセス権限" }
                    },

                    // ── 基礎設定 ──────────────────────────────
                    new NavigationItem { IsDivider = true },

                    new NavigationItem
                    {
                        Name = "公司",
                        NameKey = "Nav.Companies",
                        Description = "管理公司基本資料",
                        Route = "/companies",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = PermissionRegistry.Company.Read,
                        SearchKeywords = new List<string> { "公司資料", "公司設定", "company", "公司信息", "会社情報", "企業設定", "公司基本資料" },
                        QuickActionId = "NewCompany",
                        QuickActionName = "新增公司"
                    },
                    new NavigationItem
                    {
                        Name = "錯誤記錄",
                        NameKey = "Nav.ErrorLogs",
                        Description = "檢視和管理系統錯誤記錄",
                        Route = "/error-logs",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = PermissionRegistry.SystemControl.Read,
                        SearchKeywords = new List<string> { "錯誤記錄", "錯誤", "log", "error", "system error", "错误记录", "エラーログ", "異常記錄", "系統錯誤" }
                    },
                    new NavigationItem
                    {
                        Name = "紙張",
                        NameKey = "Nav.PaperSettings",
                        Description = "管理列印紙張設定",
                        Route = "/paper-settings",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = PermissionRegistry.PaperSetting.Read,
                        SearchKeywords = new List<string> { "紙張設定", "列印設定", "paper setting", "纸张设置", "用紙設定", "A4", "B5" },
                        QuickActionId = "NewPaperSetting",
                        QuickActionName = "新增紙張設定"
                    },
                    new NavigationItem
                    {
                        Name = "報表",
                        NameKey = "Nav.ReportPrintConfigurations",
                        Description = "管理報表列印配置",
                        Route = "/reportPrintConfigurations",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = PermissionRegistry.ReportPrintConfiguration.Read,
                        SearchKeywords = new List<string> { "報表設定", "報表配置", "report configuration", "报表设置", "帳表設定", "レポート設定", "報表格式" },
                        QuickActionId = "NewReportPrintConfiguration",
                        QuickActionName = "新增報表設定"
                    }
                }
            },
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
                    // 確保子項目繼承父項目的模組鍵（報表、圖表等子項目屬於同一模組）
                    if (string.IsNullOrEmpty(child.ModuleKey) && !string.IsNullOrEmpty(item.ModuleKey))
                    {
                        child.ModuleKey = item.ModuleKey;
                    }
                    result.Add(child);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 取得可作為儀表板捷徑的導航項目（不含 QuickAction 衍生項目）
    /// 過濾掉：首頁、父級選單容器、分隔線、無路由的項目
    /// </summary>
    public static List<NavigationItem> GetDashboardWidgetItems()
    {
        var shortcutItems = GetFlattenedNavigationItems()
            .Where(item =>
                !item.IsDivider &&
                !item.IsParent &&
                !item.IsChartWidget &&
                !string.IsNullOrEmpty(item.Name) &&
                (item.ItemType == NavigationItemType.Action ||
                 (!string.IsNullOrEmpty(item.Route) && item.Route != "/" && item.Route != "#")))
            .ToList();

        // 加入衍生的 QuickAction 項目
        var quickActionItems = DeriveQuickActionItems();

        var result = new List<NavigationItem>(shortcutItems);
        result.AddRange(quickActionItems);
        return result;
    }

    /// <summary>
    /// 取得可作為頁面連結捷徑的導航項目（Route + Action 類型，不含圖表介面項目）
    /// </summary>
    public static List<NavigationItem> GetShortcutWidgetItems()
    {
        return GetFlattenedNavigationItems()
            .Where(item =>
                !item.IsDivider &&
                !item.IsParent &&
                !item.IsChartWidget &&
                !string.IsNullOrEmpty(item.Name) &&
                (item.ItemType == NavigationItemType.Action ||
                 (!string.IsNullOrEmpty(item.Route) && item.Route != "/" && item.Route != "#")))
            .ToList();
    }

    /// <summary>
    /// 取得可作為圖表介面捷徑的導航項目（IsChartWidget = true 的 Action 類型）
    /// </summary>
    public static List<NavigationItem> GetChartWidgetItems()
    {
        return GetFlattenedNavigationItems()
            .Where(item =>
                item.IsChartWidget &&
                item.ItemType == NavigationItemType.Action &&
                !string.IsNullOrEmpty(item.Name))
            .ToList();
    }

    /// <summary>
    /// 取得可作為快速功能的導航項目（從有設定 QuickActionId 的現有項目自動衍生）
    /// </summary>
    public static List<NavigationItem> GetQuickActionWidgetItems()
    {
        return DeriveQuickActionItems();
    }

    /// <summary>
    /// 從現有導航項目中衍生 QuickAction 項目
    /// 掃描所有設有 QuickActionId 的項目，自動產生對應的 QuickAction NavigationItem
    /// </summary>
    private static List<NavigationItem> DeriveQuickActionItems()
    {
        return GetFlattenedNavigationItems()
            .Where(item => !string.IsNullOrEmpty(item.QuickActionId))
            .Select(item => new NavigationItem
            {
                Name = item.QuickActionName ?? $"新增{item.Name}",
                Description = item.QuickActionDescription ?? $"快速開啟{item.Name}新增畫面",
                Route = "",
                ItemType = NavigationItemType.QuickAction,
                ActionId = item.QuickActionId,
                IconClass = item.QuickActionIconClass ?? "bi bi-plus-circle-fill",
                Category = item.Category,
                ModuleKey = item.ModuleKey,
                RequiredPermission = item.RequiredPermission,
                SearchKeywords = new List<string>
                {
                    item.QuickActionName ?? $"新增{item.Name}",
                    $"快速{item.Name}",
                    $"new {item.Name}"
                }
            })
            .ToList();
    }

    /// <summary>
    /// 根據分類取得對應的圖示
    /// 用於子項目無圖示時的備援顯示
    /// </summary>
    /// <param name="category">分類名稱</param>
    /// <returns>對應的 Bootstrap Icon class</returns>
    public static string GetCategoryIcon(string? category)
    {
        return category switch
        {
            "人力資源管理" => "bi bi-person-badge-fill",
            "供應鏈管理" => "bi bi-building-gear",
            "客戶關係管理" => "bi bi-people-fill",
            "品項管理" => "bi bi-box-seam-fill",
            "庫存管理" => "bi bi-boxes",
            "採購管理" => "bi bi-truck",
            "銷售管理" => "bi bi-cart-fill",
            "財務管理" => "bi bi-journal-text",
            "會計管理" => "bi bi-calculator",
            "車輛管理" => "bi bi-truck-front-fill",
            "磅秤管理" => "bi bi-recycle",
            "系統管理" => "bi bi-gear-fill",
            "基礎功能" => "bi bi-house-door-fill",
            _ => "bi bi-link-45deg" // 預設圖示
        };
    }

    /// <summary>
    /// 根據識別鍵取得導航項目
    /// </summary>
    /// <param name="key">識別鍵（Route 或 "Action:{ActionId}"）</param>
    /// <returns>對應的導航項目，找不到則回傳 null</returns>
    public static NavigationItem? GetNavigationItemByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        // 合併一般捷徑項目與圖表介面項目（圖表介面被 GetDashboardWidgetItems 排除，需額外加入）
        var allItems = GetDashboardWidgetItems().Concat(GetChartWidgetItems()).ToList();

        // QuickAction 類型
        if (key.StartsWith("QuickAction:"))
        {
            var actionId = key.Substring(12); // 移除 "QuickAction:" 前綴
            return allItems.FirstOrDefault(item => 
                item.ItemType == NavigationItemType.QuickAction && 
                item.ActionId == actionId);
        }

        // Action 類型
        if (key.StartsWith("Action:"))
        {
            var actionId = key.Substring(7); // 移除 "Action:" 前綴
            return allItems.FirstOrDefault(item => 
                item.ItemType == NavigationItemType.Action && 
                item.ActionId == actionId);
        }

        // Route 類型
        return allItems.FirstOrDefault(item => 
            item.ItemType == NavigationItemType.Route && 
            item.Route == key);
    }
}