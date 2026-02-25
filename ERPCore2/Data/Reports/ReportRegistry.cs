using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Data;

/// <summary>
/// 報表註冊表 - 集中管理所有報表定義
/// </summary>
public static class ReportRegistry
{
    /// <summary>
    /// 取得所有報表定義
    /// </summary>
    public static List<ReportDefinition> GetAllReports()
    {
        return new List<ReportDefinition>
        {
            // ==================== 客戶報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.CustomerStatement,
                Name = "客戶對帳單",
                NameKey = "Report.CustomerStatement",
                Description = "產生指定期間客戶對帳單，含出貨、退貨、收款明細及期初期末餘額",
                IconClass = "bi bi-file-earmark-ruled",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerStatementReport",
                SortOrder = 2,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.CustomerSalesAnalysis,
                Name = "客戶銷售分析",
                NameKey = "Report.CustomerSalesAnalysis",
                Description = "按銷售額由高至低排列，分析客戶銷售金額佔比與排名",
                IconClass = "bi bi-graph-up",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerSalesAnalysisReport",
                SortOrder = 3,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.CustomerTransaction,
                Name = "客戶交易明細",
                NameKey = "Report.CustomerTransaction",
                Description = "查詢客戶出貨與退貨交易記錄明細，依客戶分組顯示",
                IconClass = "bi bi-list-check",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerTransactionReport",
                SortOrder = 4,
                IsEnabled = true
            },
            
            new ReportDefinition
            {
                Id = ReportIds.CustomerRoster,
                Name = "客戶名冊表",
                NameKey = "Report.CustomerRoster",
                Description = "列印客戶基本資料清單，含客戶編號、公司名稱、聯絡人、統編、聯絡電話、地址等",
                IconClass = "bi bi-person-vcard",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerRosterReport",
                SortOrder = 5,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.CustomerDetail,
                Name = "客戶詳細資料",
                NameKey = "Report.CustomerDetail",
                Description = "列印客戶完整詳細資料，含聯絡資訊、付款條件、業務負責人等，每位客戶各佔一區塊",
                IconClass = "bi bi-person-vcard-fill",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerDetailReport",
                SortOrder = 6,
                IsEnabled = true
            },

            // ==================== 廠商報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.SupplierStatement,
                Name = "廠商對帳單",
                NameKey = "Report.SupplierStatement",
                Description = "產生指定期間廠商對帳單，含進貨、退貨、付款明細及期初期末餘額",
                IconClass = "bi bi-file-earmark-ruled",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierStatementReport",
                SortOrder = 2,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.SupplierPurchaseAnalysis,
                Name = "廠商進貨分析",
                NameKey = "Report.SupplierPurchaseAnalysis",
                Description = "分析廠商進貨金額與趨勢",
                IconClass = "bi bi-graph-up",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierPurchaseAnalysisReport",
                SortOrder = 3,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.SupplierRoster,
                Name = "廠商名冊表",
                NameKey = "Report.SupplierRoster",
                Description = "列印廠商基本資料清單，含廠商編號、廠商名稱、聯絡人、統編、聯絡電話、地址等",
                IconClass = "bi bi-building",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierRosterReport",
                SortOrder = 4,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.SupplierDetail,
                Name = "廠商詳細資料",
                NameKey = "Report.SupplierDetail",
                Description = "列印廠商完整詳細資料，含聯絡資訊、付款條件、地址等，每位廠商各佔一區塊",
                IconClass = "bi bi-building-fill",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierDetailReport",
                SortOrder = 5,
                IsEnabled = true
            },

            // ==================== 採購報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.PurchaseOrder,
                Name = "採購單",
                NameKey = "Report.PurchaseOrder",
                Description = "列印採購訂單（含廠商資訊、商品明細、金額統計）",
                IconClass = "bi bi-cart-plus",
                Category = ReportCategory.Purchase,
                RequiredPermission = "PurchaseOrder.Read",
                ActionId = "PrintPurchaseOrder",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.PurchaseReceiving,
                Name = "進貨單",
                NameKey = "Report.PurchaseReceiving",
                Description = "列印進貨單（含驗收資訊、入庫明細）",
                IconClass = "bi bi-box-seam",
                Category = ReportCategory.Purchase,
                RequiredPermission = "PurchaseReceiving.Read",
                ActionId = "PrintPurchaseReceiving",
                SortOrder = 2,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.PurchaseReturn,
                Name = "進貨退出單",
                NameKey = "Report.PurchaseReturn",
                Description = "列印進貨退出單（含退貨原因、退貨明細）",
                IconClass = "bi bi-arrow-return-left",
                Category = ReportCategory.Purchase,
                RequiredPermission = "PurchaseReturn.Read",
                ActionId = "PrintPurchaseReturn",
                SortOrder = 3,
                IsEnabled = true
            },
            
            // ==================== 銷售報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.Quotation,
                Name = "報價單",
                NameKey = "Report.Quotation",
                Description = "列印報價單（含客戶資訊、商品明細、金額統計）",
                IconClass = "bi bi-file-earmark-text",
                Category = ReportCategory.Sales,
                RequiredPermission = "Quotation.Read",
                ActionId = "PrintQuotation",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.SalesOrder,
                Name = "銷貨訂單",
                NameKey = "Report.SalesOrder",
                Description = "列印銷貨訂單（含客戶資訊、商品明細、金額統計）",
                IconClass = "bi bi-receipt",
                Category = ReportCategory.Sales,
                RequiredPermission = "SalesOrder.Read",
                ActionId = "PrintSalesOrder",
                SortOrder = 2,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.SalesDelivery,
                Name = "出貨單",
                NameKey = "Report.SalesDelivery",
                Description = "列印出貨單（含客戶資訊、商品明細、金額統計）",
                IconClass = "bi bi-truck",
                Category = ReportCategory.Sales,
                RequiredPermission = "SalesDelivery.Read",
                ActionId = "PrintSalesDelivery",
                SortOrder = 3,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.SalesReturn,
                Name = "銷貨退回單",
                NameKey = "Report.SalesReturn",
                Description = "列印銷貨退回單（含退貨原因、退貨明細）",
                IconClass = "bi bi-arrow-return-right",
                Category = ReportCategory.Sales,
                RequiredPermission = "SalesReturn.Read",
                ActionId = "PrintSalesReturn",
                SortOrder = 4,
                IsEnabled = true
            },
            
            // ==================== 商品報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.ProductList,
                Name = "商品清單表",
                NameKey = "Report.ProductList",
                Description = "列印商品基本資料清單（含品號、品名、規格、條碼、分類、單位）",
                IconClass = "bi bi-box-seam",
                Category = ReportCategory.Product,
                RequiredPermission = "Product.Read",
                ActionId = "PrintProductList",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.ProductDetail,
                Name = "商品詳細資料",
                NameKey = "Report.ProductDetail",
                Description = "列印商品完整詳細資料，含規格、條碼、分類、單位、採購類型、成本定價等，每項商品各佔一區塊",
                IconClass = "bi bi-box-fill",
                Category = ReportCategory.Product,
                RequiredPermission = "Product.Read",
                ActionId = "OpenProductDetailReport",
                SortOrder = 2,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.ProductBarcode,
                Name = "商品條碼",
                NameKey = "Report.ProductBarcode",
                Description = "列印商品條碼",
                IconClass = "bi bi-upc-scan",
                Category = ReportCategory.Product,
                RequiredPermission = "Product.Read",
                ActionId = "PrintProductBarcode",
                SortOrder = 3,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.BOMReport,
                Name = "物料清單報表",
                NameKey = "Report.BOMReport",
                Description = "列印商品BOM物料清單，依配方分組顯示組件品號、品名、數量、單位、成本",
                IconClass = "bi bi-diagram-3",
                Category = ReportCategory.Product,
                RequiredPermission = "ProductComposition.Read",
                ActionId = "OpenBOMReport",
                SortOrder = 4,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.ProductionSchedule,
                Name = "生產排程表",
                NameKey = "Report.ProductionSchedule",
                Description = "查詢生產排程，含排程項目、數量、狀態、預計日期等明細",
                IconClass = "bi bi-calendar-check",
                Category = ReportCategory.Product,
                RequiredPermission = "ProductionSchedule.Read",
                ActionId = "OpenProductionScheduleReport",
                SortOrder = 5,
                IsEnabled = true
            },


            // ==================== 庫存報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.InventoryStatus,
                Name = "庫存現況表",
                NameKey = "Report.InventoryStatus",
                Description = "查詢各倉庫商品庫存現況，含現有庫存、預留庫存、可用庫存及庫存金額",
                IconClass = "bi bi-clipboard-data",
                Category = ReportCategory.Inventory,
                RequiredPermission = "InventoryStock.Read",
                ActionId = "OpenInventoryStatusReport",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.InventoryTransaction,
                Name = "庫存異動明細",
                NameKey = "Report.InventoryTransaction",
                Description = "查詢庫存進出異動記錄明細",
                IconClass = "bi bi-arrow-left-right",
                Category = ReportCategory.Inventory,
                RequiredPermission = "InventoryStock.Read",
                ActionId = "OpenInventoryTransactionReport",
                SortOrder = 2,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.InventoryCount,
                Name = "盤點差異表",
                NameKey = "Report.InventoryCount",
                Description = "依盤點單分組顯示各商品系統庫存、實盤數量及差異金額，支援僅差異項目篩選",
                IconClass = "bi bi-card-checklist",
                Category = ReportCategory.Inventory,
                RequiredPermission = "StockTaking.Read",
                ActionId = "OpenInventoryCountReport",
                SortOrder = 3,
                IsEnabled = true
            },

            // ==================== 車輛報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.VehicleList,
                Name = "車輛管理表",
                NameKey = "Report.VehicleList",
                Description = "列印車輛基本資料清單，含車牌號碼、車輛名稱、車型、廠牌、負責人、保險到期日等",
                IconClass = "bi bi-truck-front-fill",
                Category = ReportCategory.Vehicle,
                RequiredPermission = "Vehicle.Read",
                ActionId = "OpenVehicleListReport",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.VehicleMaintenance,
                Name = "車輛保養表",
                NameKey = "Report.VehicleMaintenance",
                Description = "列印車輛保養紀錄，含保養類型、保養日期、里程數、費用、維修廠等明細",
                IconClass = "bi bi-wrench-adjustable",
                Category = ReportCategory.Vehicle,
                RequiredPermission = "VehicleMaintenance.Read",
                ActionId = "OpenVehicleMaintenanceReport",
                SortOrder = 2,
                IsEnabled = true
            },

            // ==================== 人力報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.EmployeeRoster,
                Name = "員工名冊表",
                NameKey = "Report.EmployeeRoster",
                Description = "列印員工基本資料清單，含員工編號、姓名、部門、職位、到職日期、在職狀態等",
                IconClass = "bi bi-person-lines-fill",
                Category = ReportCategory.HR,
                RequiredPermission = "Employee.Read",
                ActionId = "OpenEmployeeRosterReport",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.EmployeeDetail,
                Name = "員工詳細資料",
                NameKey = "Report.EmployeeDetail",
                Description = "列印員工完整詳細資料，含聯絡資訊、緊急聯絡人、任職資訊等，每位員工各佔一區塊",
                IconClass = "bi bi-person-vcard-fill",
                Category = ReportCategory.HR,
                RequiredPermission = "Employee.Read",
                ActionId = "OpenEmployeeDetailReport",
                SortOrder = 2,
                IsEnabled = true
            },

            // ==================== 廢料報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.WasteRecord,
                Name = "廢料記錄表",
                NameKey = "Report.WasteRecord",
                Description = "列印廢料記錄清單，依廢料類型分組，含車牌號碼、客戶、入庫倉庫、重量及費用明細",
                IconClass = "bi bi-recycle",
                Category = ReportCategory.Waste,
                RequiredPermission = "WasteRecord.Read",
                ActionId = "OpenWasteRecordReport",
                SortOrder = 1,
                IsEnabled = true
            },

            // ==================== 財務報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.AccountsReceivableSetoff,
                Name = "應收沖款單",
                NameKey = "Report.AccountsReceivableSetoff",
                Description = "列印應收帳款沖款單（含客戶資訊、沖銷明細、收款記錄）",
                IconClass = "bi bi-receipt-cutoff",
                Category = ReportCategory.Financial,
                RequiredPermission = "SetoffDocument.Read",
                ActionId = "PrintAccountsReceivableSetoff",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.AccountsPayableSetoff,
                Name = "應付沖款單",
                NameKey = "Report.AccountsPayableSetoff",
                Description = "列印應付帳款沖款單（含廠商資訊、沖銷明細、付款記錄）",
                IconClass = "bi bi-receipt-cutoff",
                Category = ReportCategory.Financial,
                RequiredPermission = "SetoffDocument.Read",
                ActionId = "PrintAccountsPayableSetoff",
                SortOrder = 2,
                IsEnabled = true
            },
            // ==================== 會計報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.AccountItemList,
                Name = "會計科目表",
                NameKey = "Report.AccountItemList",
                Description = "列印標準會計科目表（Chart of Accounts），依科目大類分組顯示科目代碼、名稱、層級、借貸方向等",
                IconClass = "bi bi-list-columns",
                Category = ReportCategory.Accounting,
                RequiredPermission = "AccountItem.Read",
                ActionId = "OpenAccountItemListReport",
                SortOrder = 1,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.TrialBalance,
                Name = "試算表",
                NameKey = "Report.TrialBalance",
                Description = "依科目匯總已過帳傳票的本期借貸發生額與期末累計餘額，驗證借貸平衡",
                IconClass = "bi bi-calculator",
                Category = ReportCategory.Accounting,
                RequiredPermission = "JournalEntry.Read",
                ActionId = "OpenTrialBalanceReport",
                SortOrder = 2,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.IncomeStatement,
                Name = "損益表",
                NameKey = "Report.IncomeStatement",
                Description = "彙總指定期間的營業收入、成本、費用，計算毛利潤、營業損益及稅前損益",
                IconClass = "bi bi-graph-up",
                Category = ReportCategory.Accounting,
                RequiredPermission = "JournalEntry.Read",
                ActionId = "OpenIncomeStatementReport",
                SortOrder = 3,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.BalanceSheet,
                Name = "資產負債表",
                NameKey = "Report.BalanceSheet",
                Description = "彙總截止日當天的資產、負債、權益累計餘額（資產 = 負債 + 權益）",
                IconClass = "bi bi-bank",
                Category = ReportCategory.Accounting,
                RequiredPermission = "JournalEntry.Read",
                ActionId = "OpenBalanceSheetReport",
                SortOrder = 4,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.GeneralLedger,
                Name = "總分類帳",
                NameKey = "Report.GeneralLedger",
                Description = "顯示所有科目的帳戶卡片，依科目大類分組，每個科目列出所有已過帳傳票明細（含期初餘額、逐筆流水餘額、期末餘額）",
                IconClass = "bi bi-journal-text",
                Category = ReportCategory.Accounting,
                RequiredPermission = "JournalEntry.Read",
                ActionId = "OpenGeneralLedgerReport",
                SortOrder = 5,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.SubsidiaryLedger,
                Name = "明細分類帳",
                NameKey = "Report.SubsidiaryLedger",
                Description = "依科目代碼/名稱關鍵字查詢特定科目的帳戶卡片，顯示已過帳傳票明細與流水餘額，適合查看應收帳款按客戶、應付帳款按廠商等明細",
                IconClass = "bi bi-journal-bookmark",
                Category = ReportCategory.Accounting,
                RequiredPermission = "JournalEntry.Read",
                ActionId = "OpenSubsidiaryLedgerReport",
                SortOrder = 6,
                IsEnabled = true
            },
            new ReportDefinition
            {
                Id = ReportIds.DetailAccountBalance,
                Name = "明細科目餘額表",
                NameKey = "Report.DetailAccountBalance",
                Description = "彙總各科目的期初餘額、本期借方發生額、本期貸方發生額及期末餘額，無逐筆明細，適合快速掌握各科目餘額變動",
                IconClass = "bi bi-table",
                Category = ReportCategory.Accounting,
                RequiredPermission = "JournalEntry.Read",
                ActionId = "OpenDetailAccountBalanceReport",
                SortOrder = 7,
                IsEnabled = true
            },
        };
    }
    
    /// <summary>
    /// 依分類取得報表
    /// </summary>
    public static List<ReportDefinition> GetReportsByCategory(string category)
    {
        return GetAllReports()
            .Where(r => r.Category == category)
            .OrderBy(r => r.SortOrder)
            .ToList();
    }
    
    /// <summary>
    /// 取得客戶相關報表
    /// </summary>
    public static List<ReportDefinition> GetCustomerReports()
    {
        return GetReportsByCategory(ReportCategory.Customer);
    }
    
    /// <summary>
    /// 取得廠商相關報表
    /// </summary>
    public static List<ReportDefinition> GetSupplierReports()
    {
        return GetReportsByCategory(ReportCategory.Supplier);
    }
    
    /// <summary>
    /// 取得財務相關報表
    /// </summary>
    public static List<ReportDefinition> GetFinancialReports()
    {
        return GetReportsByCategory(ReportCategory.Financial);
    }

    /// <summary>
    /// 取得會計相關報表
    /// </summary>
    public static List<ReportDefinition> GetAccountingReports()
    {
        return GetReportsByCategory(ReportCategory.Accounting);
    }
    
    /// <summary>
    /// 取得採購相關報表
    /// </summary>
    public static List<ReportDefinition> GetPurchaseReports()
    {
        return GetReportsByCategory(ReportCategory.Purchase);
    }
    
    /// <summary>
    /// 取得銷售相關報表
    /// </summary>
    public static List<ReportDefinition> GetSalesReports()
    {
        return GetReportsByCategory(ReportCategory.Sales);
    }
    
    /// <summary>
    /// 取得庫存相關報表
    /// </summary>
    public static List<ReportDefinition> GetInventoryReports()
    {
        return GetReportsByCategory(ReportCategory.Inventory);
    }
    
    /// <summary>
    /// 取得商品相關報表
    /// </summary>
    public static List<ReportDefinition> GetProductReports()
    {
        return GetReportsByCategory(ReportCategory.Product);
    }

    /// <summary>
    /// 取得人力相關報表
    /// </summary>
    public static List<ReportDefinition> GetHRReports()
    {
        return GetReportsByCategory(ReportCategory.HR);
    }

    /// <summary>
    /// 取得車輛相關報表
    /// </summary>
    public static List<ReportDefinition> GetVehicleReports()
    {
        return GetReportsByCategory(ReportCategory.Vehicle);
    }

    /// <summary>
    /// 取得廢料相關報表
    /// </summary>
    public static List<ReportDefinition> GetWasteReports()
    {
        return GetReportsByCategory(ReportCategory.Waste);
    }

    /// <summary>
    /// 根據報表識別碼取得報表定義
    /// </summary>
    /// <param name="reportId">報表識別碼（如 PO001、AR001）</param>
    /// <returns>報表定義，若找不到則返回 null</returns>
    public static ReportDefinition? GetReportById(string reportId)
    {
        return GetAllReports().FirstOrDefault(r => r.Id == reportId);
    }
}
