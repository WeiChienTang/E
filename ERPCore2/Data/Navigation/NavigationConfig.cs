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
                Description = "系統首頁和總覽",
                Route = "/",
                IconClass = "bi-house-door-fill",
                Category = "基礎功能",
                SearchKeywords = new List<string> { "首頁", "主頁", "主畫面", "home", "dashboard", "總覽" }
            },

            // ==================== 人力資源管理 ====================
            new NavigationItem
            {
                Name = "人力資源",
                Description = "員工相關功能管理",
                Route = "#",
                IconClass = "bi bi-person-badge-fill",
                Category = "人力管理",
                IsParent = true,
                MenuKey = "employee_management",
                SearchKeywords = new List<string> { "員工", "人員", "人事", "HR", "employee", "staff" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "員工",
                        Description = "管理員工資料和人事資訊",
                        Route = "/employees",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "人力管理",
                        RequiredPermission = "Employee.Read",
                        SearchKeywords = new List<string> { "員工管理", "員工資料", "人力管理" },
                        QuickActionId = "NewEmployee",
                        QuickActionName = "新增員工"
                    },
                    new NavigationItem
                    {
                        Name = "部門",
                        Description = "管理公司部門組織架構",
                        Route = "/departments",
                        IconClass = "",
                        Category = "人力管理",
                        RequiredPermission = "Department.Read",
                        SearchKeywords = new List<string> { "部門", "組織", "department" },
                        QuickActionId = "NewDepartment",
                        QuickActionName = "新增部門"
                    },
                    new NavigationItem
                    {
                        Name = "職位",
                        Description = "管理員工職位設定",
                        Route = "/employee-positions",
                        IconClass = "",
                        Category = "人力管理",
                        RequiredPermission = "EmployeePosition.Read",
                        SearchKeywords = new List<string> { "職位", "職稱", "position" },
                        QuickActionId = "NewEmployeePosition",
                        QuickActionName = "新增職位"
                    },
                    NavigationActionHelper.CreateActionItem(
                        name: "權限分配",
                        description: "管理權限組與權限關係",
                        iconClass: "bi bi-caret-right-fill",
                        actionId: "OpenRolePermissionManagement",
                        category: "人力管理",
                        requiredPermission: "Role.Read",
                        searchKeywords: new List<string> { "權限分配", "角色權限", "role permission", "權限設定" }
                    ),
                    new NavigationItem
                    {
                        Name = "權限組",
                        Description = "管理使用者權限組",
                        Route = "/roles",
                        IconClass = "",
                        Category = "人力管理",
                        RequiredPermission = "Role.Read",
                        SearchKeywords = new List<string> { "權限組", "角色", "role" },
                        QuickActionId = "NewRole",
                        QuickActionName = "新增權限組"
                    },
                    new NavigationItem
                    {
                        Name = "權限",
                        Description = "管理系統權限設定",
                        Route = "/permissions",
                        IconClass = "",
                        Category = "人力管理",
                        RequiredPermission = "Permission.Read",
                        SearchKeywords = new List<string> { "權限", "permission", "授權" },
                        QuickActionId = "NewPermission",
                        QuickActionName = "新增權限"
                    },

                    // 分隔線 - 區分資料維護與報表
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
                        searchKeywords: new List<string> { "人力報表", "人力報表集", "HR report", "員工名冊" }
                    ),
                }
            },

            // ==================== 廠商管理 ====================
            new NavigationItem
            {
                Name = "廠商",
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
                        Name = "廠商",
                        Description = "管理供應商和廠商資料",
                        Route = "/suppliers",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "供應鏈管理",
                        RequiredPermission = "Supplier.Read",
                        SearchKeywords = new List<string> { "廠商管理", "供應商資料", "廠商管理" },
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
                        searchKeywords: new List<string> { "廠商報表", "廠商報表集", "supplier report", "應付帳款" }
                    ),
                }
            },

            // ==================== 客戶管理 ====================
            new NavigationItem
            {
                Name = "客戶",
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
                        Name = "客戶",
                        Description = "管理客戶資料、聯絡資訊和客戶關係",
                        Route = "/customers",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "客戶關係管理",
                        RequiredPermission = "Customer.Read",
                        SearchKeywords = new List<string> { "客戶管理", "客戶資料", "客戶管理", "聯絡人" },
                        QuickActionId = "NewCustomer",
                        QuickActionName = "新增客戶"
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
                        searchKeywords: new List<string> { "客戶報表", "客戶報表集", "customer report", "應收帳款" }
                    ),
                }
            },

            // ==================== 商品管理 ====================
            new NavigationItem
            {
                Name = "商品",
                Description = "商品相關功能管理",
                Route = "#",
                IconClass = "bi bi-box-seam-fill",
                Category = "商品管理",
                IsParent = true,
                MenuKey = "product_management",
                SearchKeywords = new List<string> { "商品", "商品", "product", "item" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "商品",
                        Description = "管理商品資料和商品目錄",
                        Route = "/products",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "商品管理",
                        RequiredPermission = "Product.Read",
                        SearchKeywords = new List<string> { "商品管理", "商品資料", "商品目錄", "品項","條碼" },
                        QuickActionId = "NewProduct",
                        QuickActionName = "新增商品"
                    },
                    new NavigationItem
                    {
                        Name = "類型",
                        Description = "管理商品類型分類",
                        Route = "/product-categories",
                        IconClass = "",
                        Category = "商品管理",
                        RequiredPermission = "ProductCategory.Read",
                        SearchKeywords = new List<string> { "商品類型", "商品分類", "category" },
                        QuickActionId = "NewProductCategory",
                        QuickActionName = "新增商品類型"
                    },
                    new NavigationItem
                    {
                        Name = "單位",
                        Description = "管理商品計量單位",
                        Route = "/units",
                        IconClass = "",
                        Category = "商品管理",
                        RequiredPermission = "Unit.Read",
                        SearchKeywords = new List<string> { "單位", "計量單位", "unit" },
                        QuickActionId = "NewUnit",
                        QuickActionName = "新增單位"
                    },

                    new NavigationItem
                    {
                        Name = "尺寸",
                        Description = "管理商品尺寸規格",
                        Route = "/sizes",
                        IconClass = "",
                        Category = "商品管理",
                        RequiredPermission = "Size.Read",
                        SearchKeywords = new List<string> { "尺寸", "規格", "size" },
                        QuickActionId = "NewSize",
                        QuickActionName = "新增尺寸"
                    },
                    new NavigationItem
                    {
                        Name = "物料清單",
                        Description = "管理商品的配方和組件結構",
                        Route = "/product-compositions",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "商品管理",
                        RequiredPermission = "ProductComposition.Read",
                        SearchKeywords = new List<string> { "商品合成", "BOM", "物料清單", "Bill of Materials" },
                        QuickActionId = "NewProductComposition",
                        QuickActionName = "新增物料清單"
                    },
                    new NavigationItem
                    {
                        Name = "物料類型",
                        Description = "管理物料清單的類型分類",
                        Route = "/composition-categories",
                        IconClass = "",
                        Category = "商品管理",
                        RequiredPermission = "CompositionCategory.Read",
                        SearchKeywords = new List<string> { "物料清單類型", "BOM類型", "category" },
                        QuickActionId = "NewCompositionCategory",
                        QuickActionName = "新增物料清單類型"
                    },
                    new NavigationItem
                    {
                        Name = "生產排程",
                        Description = "管理生產排程的詳細資料",
                        Route = "/production-schedules",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "商品管理",
                        RequiredPermission = "ProductionSchedule.Read",
                        SearchKeywords = new List<string> { "生產排程", "排程管理", "production schedule" },
                        QuickActionId = "NewProductionSchedule",
                        QuickActionName = "新增生產排程"
                    },
                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },
                    
                    NavigationActionHelper.CreateActionItem(
                        name: "商品報表集",
                        description: "查看和列印所有商品相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenProductReportIndex",
                        category: "商品管理",
                        requiredPermission: "Product.Read",
                        searchKeywords: new List<string> { "商品報表", "商品報表集", "product report", "產品報表" }
                    ),
                }
            },

            // ==================== 庫存管理 ====================
            new NavigationItem
            {
                Name = "庫存",
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
                        SearchKeywords = new List<string> { "庫存查詢", "庫存資料", "stock inquiry" },
                        QuickActionId = "NewInventoryStock",
                        QuickActionName = "新增庫存查詢"
                    },
                    new NavigationItem
                    {
                        Name = "倉庫",
                        Description = "管理倉庫資料和設定",
                        Route = "/warehouses",
                        IconClass = "",
                        Category = "庫存管理",
                        RequiredPermission = "Warehouse.Read",
                        SearchKeywords = new List<string> { "倉庫", "倉庫管理", "warehouse" },
                        QuickActionId = "NewWarehouse",
                        QuickActionName = "新增倉庫"
                    },
                    new NavigationItem
                    {
                        Name = "庫位",
                        Description = "管理倉庫內的儲位配置",
                        Route = "/warehouseLocations",
                        IconClass = "",
                        Category = "庫存管理",
                        RequiredPermission = "WarehouseLocation.Read",
                        SearchKeywords = new List<string> { "倉庫位置", "庫位", "儲位", "location" },
                        QuickActionId = "NewWarehouseLocation",
                        QuickActionName = "新增庫位"
                    },
                    new NavigationItem
                    {
                        Name = "庫存盤點",
                        Description = "管理庫存盤點作業",
                        Route = "/stockTakings",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = "StockTaking.Read",
                        SearchKeywords = new List<string> { "庫存盤點", "盤點作業", "stock taking", "inventory audit" },
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
                    //     RequiredPermission = "InventoryTransaction.Read",
                    //     SearchKeywords = new List<string> { "庫存異動", "異動記錄", "transaction" }
                    // },
                    new NavigationItem
                    {
                        Name = "領料",
                        Description = "管理物料領用作業",
                        Route = "/materialIssues",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "庫存管理",
                        RequiredPermission = "MaterialIssue.Read",
                        SearchKeywords = new List<string> { "領料", "物料領用", "material issue" },
                        QuickActionId = "NewMaterialIssue",
                        QuickActionName = "新增領料"
                    },

                    // 分隔線 - 區分資料維護與報表
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
                        searchKeywords: new List<string> { "倉庫報表", "庫存報表", "倉庫報表集", "inventory report", "庫存現況" }
                    ),
                }
            },

            // ==================== 採購管理 ====================
            new NavigationItem
            {
                Name = "採購",
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
                        Name = "採購",
                        Description = "管理採購訂單",
                        Route = "/purchase/orders",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = "PurchaseOrder.Read",
                        SearchKeywords = new List<string> { "採購單", "訂購單", "purchase order", "PO" },
                        // QuickAction: 在首頁快速開啟採購單新增 Modal
                        QuickActionId = "NewPurchaseOrder",
                        QuickActionName = "新增採購單",
                    },
                    new NavigationItem
                    {
                        Name = "進貨",
                        Description = "管理進貨作業",
                        Route = "/purchase/receiving",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = "PurchaseReceiving.Read",
                        SearchKeywords = new List<string> { "進貨", "收貨", "receiving", "GR" },
                        QuickActionId = "NewPurchaseReceiving",
                        QuickActionName = "新增進貨單"
                    },
                    new NavigationItem
                    {
                        Name = "進貨退出",
                        Description = "管理進貨退回作業",
                        Route = "/purchase/returns",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "採購管理",
                        RequiredPermission = "PurchaseReturn.Read",
                        SearchKeywords = new List<string> { "進貨退出", "退貨", "return", "退回" },
                        QuickActionId = "NewPurchaseReturn",
                        QuickActionName = "新增進貨退出單"
                    },
                    
                    // 分隔線 - 區分資料維護與報表
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
                        searchKeywords: new List<string> { "採購報表", "採購報表集", "purchase report", "進貨報表" }
                    ),
                }
            },

            // ==================== 銷貨管理 ====================
            new NavigationItem
            {
                Name = "銷貨",
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
                        Name = "報價",
                        Description = "管理銷售報價單",
                        Route = "/quotations",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "Quotation.Read",
                        SearchKeywords = new List<string> { "報價單", "銷售報價", "quotation", "quote" },
                        QuickActionId = "NewQuotation",
                        QuickActionName = "新增報價單"
                    },
                    new NavigationItem
                    {
                        Name = "訂單",
                        Description = "管理訂單",
                        Route = "/salesOrders",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "SalesOrder.Read",
                        SearchKeywords = new List<string> { "訂單", "sales order", "SO" },
                        // QuickAction: 在首頁快速開啟訂單新增 Modal
                        QuickActionId = "NewSalesOrder",
                        QuickActionName = "新增訂單",
                    },
                    new NavigationItem
                    {
                        Name = "銷貨",
                        Description = "管理銷貨出貨作業",
                        Route = "/salesDeliveries",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "SalesDelivery.Read",
                        SearchKeywords = new List<string> { "出貨","銷貨單", "銷貨單", "delivery", "銷貨" },
                        QuickActionId = "NewSalesDelivery",
                        QuickActionName = "新增銷貨單"
                    },
                    new NavigationItem
                    {
                        Name = "銷貨退回",
                        Description = "管理銷貨退回作業",
                        Route = "/salesReturns",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "銷售管理",
                        RequiredPermission = "SalesReturn.Read",
                        SearchKeywords = new List<string> { "銷貨退回", "退貨", "sales return" },
                        QuickActionId = "NewSalesReturn",
                        QuickActionName = "新增銷貨退回單"
                    },
                    new NavigationItem
                    {
                        Name = "銷退原因",
                        Description = "管理退貨原因分類",
                        Route = "/salesReturnReasons",
                        IconClass = "",
                        Category = "銷售管理",
                        RequiredPermission = "SalesReturnReason.Read",
                        SearchKeywords = new List<string> { "退回原因", "退貨原因", "return reason" },
                        QuickActionId = "NewSalesReturnReason",
                        QuickActionName = "新增銷貨退回原因"
                    },
                    
                    // 分隔線 - 區分資料維護與報表
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
                        searchKeywords: new List<string> { "銷貨報表", "銷貨報表集", "sales report", "銷售報表" }
                    ),
                }
            },

            // ==================== 車輛管理 ====================
            new NavigationItem
            {
                Name = "車輛",
                Description = "車輛相關功能管理",
                Route = "#",
                IconClass = "bi bi-truck-front-fill",
                Category = "車輛管理",
                IsParent = true,
                MenuKey = "vehicle_management",
                SearchKeywords = new List<string> { "車輛", "車輛管理", "vehicle", "fleet", "車隊" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "車輛",
                        Description = "管理車輛基本資料",
                        Route = "/vehicles",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "車輛管理",
                        RequiredPermission = "Vehicle.Read",
                        SearchKeywords = new List<string> { "車輛管理", "車輛資料", "vehicle management" },
                        QuickActionId = "NewVehicle",
                        QuickActionName = "新增車輛"
                    },
                    new NavigationItem
                    {
                        Name = "車型",
                        Description = "管理車輛類型設定",
                        Route = "/vehicle-types",
                        IconClass = "",
                        Category = "車輛管理",
                        RequiredPermission = "VehicleType.Read",
                        SearchKeywords = new List<string> { "車型", "車輛類型", "vehicle type" },
                        QuickActionId = "NewVehicleType",
                        QuickActionName = "新增車型"
                    },
                    new NavigationItem
                    {
                        Name = "保養紀錄",
                        Description = "管理車輛保養與維修紀錄",
                        Route = "/vehicle-maintenances",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "車輛管理",
                        RequiredPermission = "VehicleMaintenance.Read",
                        SearchKeywords = new List<string> { "保養紀錄", "維修紀錄", "vehicle maintenance" },
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
                        searchKeywords: new List<string> { "車輛報表", "車輛報表集", "vehicle report", "保養報表" }
                    ),
                }
            },

            // ==================== 廢料管理 ====================
            new NavigationItem
            {
                Name = "廢料",
                Description = "廢料收料記錄與類型管理",
                Route = "#",
                IconClass = "bi bi-recycle",
                Category = "廢料管理",
                IsParent = true,
                MenuKey = "waste_management",
                SearchKeywords = new List<string> { "廢料", "廢棄物", "廢料管理", "waste", "scrap" },
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Name = "廢料記錄",
                        Description = "管理廢料收料記錄",
                        Route = "/waste-records",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "廢料管理",
                        RequiredPermission = "WasteRecord.Read",
                        SearchKeywords = new List<string> { "廢料記錄", "收料記錄", "廢料單", "waste record" },
                        QuickActionId = "NewWasteRecord",
                        QuickActionName = "新增廢料記錄"
                    },
                    new NavigationItem
                    {
                        Name = "廢料類型",
                        Description = "管理廢料類型分類設定",
                        Route = "/waste-types",
                        IconClass = "",
                        Category = "廢料管理",
                        RequiredPermission = "WasteType.Read",
                        SearchKeywords = new List<string> { "廢料類型", "廢料分類", "waste type" },
                        QuickActionId = "NewWasteType",
                        QuickActionName = "新增廢料類型"
                    },

                    // 分隔線 - 區分資料維護與報表
                    new NavigationItem
                    {
                        IsDivider = true
                    },

                    NavigationActionHelper.CreateActionItem(
                        name: "廢料報表集",
                        description: "查看和列印所有廢料相關報表",
                        iconClass: "bi bi-printer-fill",
                        actionId: "OpenWasteReportIndex",
                        category: "廢料管理",
                        requiredPermission: "WasteRecord.Read",
                        searchKeywords: new List<string> { "廢料報表", "廢料報表集", "waste report", "廢料記錄表" }
                    ),
                }
            },

            // ==================== 財務管理 ====================
            new NavigationItem
            {
                Name = "財務",
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
                        Name = "應收沖款",
                        Description = "管理應收帳款沖款",
                        Route = "/accountsReceivableSetoff",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "財務管理",
                        RequiredPermission = "SetoffDocument.Read",
                        SearchKeywords = new List<string> { "應收帳款", "AR", "receivable", "收款","沖款" },
                        QuickActionId = "NewARSetoff",
                        QuickActionName = "新增應收沖款"
                    },
                    new NavigationItem
                    {
                        Name = "應付沖款",
                        Description = "管理應付帳款沖款",
                        Route = "/accountsPayableSetoff",
                        IconClass = "bi bi-caret-right-fill",
                        Category = "財務管理",
                        RequiredPermission = "SetoffDocument.Read",
                        SearchKeywords = new List<string> { "應付帳款", "AP", "payable", "付款","沖款" },
                        QuickActionId = "NewAPSetoff",
                        QuickActionName = "新增應付沖款"
                    },
                    new NavigationItem
                    {
                        Name = "付款方式",
                        Description = "管理付款方式設定",
                        Route = "/paymentMethods",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = "PaymentMethod.Read",
                        SearchKeywords = new List<string> { "付款方式", "payment method", "支付" },
                        QuickActionId = "NewPaymentMethod",
                        QuickActionName = "新增付款方式"
                    },
                    new NavigationItem
                    {
                        Name = "銀行",
                        Description = "管理銀行帳戶資料",
                        Route = "/banks",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = "Bank.Read",
                        SearchKeywords = new List<string> { "銀行", "帳戶", "bank" },
                        QuickActionId = "NewBank",
                        QuickActionName = "新增銀行"
                    },
                    new NavigationItem
                    {
                        Name = "貨幣",
                        Description = "管理貨幣和匯率設定",
                        Route = "/currencies",
                        IconClass = "",
                        Category = "財務管理",
                        RequiredPermission = "Currency.Read",
                        SearchKeywords = new List<string> { "貨幣", "幣別", "currency", "匯率" },
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
                        searchKeywords: new List<string> { "財務報表", "財務報表集", "financial report", "沖款單報表", "應收沖款", "應付沖款" }
                    ),

                }
            },

            // ==================== 系統管理 ====================
            new NavigationItem
            {
                Name = "系統",
                Description = "系統管理和管理功能",
                Route = "#",
                IconClass = "bi bi-gear-fill",
                Category = "系統管理",
                IsParent = true,
                MenuKey = "system_management",
                SearchKeywords = new List<string> { "系統管理", "管理", "system", "admin", "administration" },
                Children = new List<NavigationItem>
                {
                    NavigationActionHelper.CreateActionItem(
                        name: "系統參數",
                        description: "管理系統參數設定",
                        iconClass: "bi bi-caret-right-fill",
                        actionId: "OpenSystemParameterSettings",
                        category: "系統管理",
                        requiredPermission: "SystemParameter.Read",
                        searchKeywords: new List<string> { "系統參數", "參數設定", "parameter", "config", "設定" }
                    ),
                    new NavigationItem
                    {
                        Name = "公司",
                        Description = "管理公司基本資料",
                        Route = "/companies",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = "Company.Read",
                        SearchKeywords = new List<string> { "公司資料", "公司設定", "company" },
                        QuickActionId = "NewCompany",
                        QuickActionName = "新增公司"
                    },
                    new NavigationItem
                    {
                        Name = "錯誤記錄",
                        Description = "檢視和管理系統錯誤記錄",
                        Route = "/error-logs",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = "SystemControl.Read",
                        SearchKeywords = new List<string> { "錯誤記錄", "錯誤", "log", "error", "system error" }
                    },
                    new NavigationItem
                    {
                        Name = "印表機",
                        Description = "管理印表機配置",
                        Route = "/printerCconfigurations",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = "PrinterSetting.Read",
                        SearchKeywords = new List<string> { "印表機", "列印設定", "printer" },
                        QuickActionId = "NewPrinterConfiguration",
                        QuickActionName = "新增印表機設定"
                    },
                    new NavigationItem
                    {
                        Name = "紙張",
                        Description = "管理列印紙張設定",
                        Route = "/paper-settings",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = "PaperSetting.Read",
                        SearchKeywords = new List<string> { "紙張設定", "列印設定", "paper setting" },
                        QuickActionId = "NewPaperSetting",
                        QuickActionName = "新增紙張設定"
                    },
                    new NavigationItem
                    {
                        Name = "報表",
                        Description = "管理報表列印配置",
                        Route = "/reportPrintConfigurations",
                        IconClass = "",
                        Category = "系統管理",
                        RequiredPermission = "ReportPrintConfiguration.Read",
                        SearchKeywords = new List<string> { "報表設定", "報表配置", "report configuration" },
                        QuickActionId = "NewReportPrintConfiguration",
                        QuickActionName = "新增報表設定"
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
    /// 取得可作為頁面連結捷徑的導航項目（Route + Action 類型）
    /// </summary>
    public static List<NavigationItem> GetShortcutWidgetItems()
    {
        return GetFlattenedNavigationItems()
            .Where(item => 
                !item.IsDivider &&
                !item.IsParent &&
                !string.IsNullOrEmpty(item.Name) &&
                (item.ItemType == NavigationItemType.Action || 
                 (!string.IsNullOrEmpty(item.Route) && item.Route != "/" && item.Route != "#")))
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
            "商品管理" => "bi bi-box-seam-fill",
            "庫存管理" => "bi bi-boxes",
            "採購管理" => "bi bi-truck",
            "銷售管理" => "bi bi-cart-fill",
            "財務管理" => "bi bi-journal-text",
            "車輛管理" => "bi bi-truck-front-fill",
            "廢料管理" => "bi bi-recycle",
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

        var allItems = GetDashboardWidgetItems();

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