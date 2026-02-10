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
                Id = ReportIds.AccountsReceivable,
                Name = "應收帳款報表",
                Description = "查詢和列印客戶應收帳款明細資料",
                IconClass = "bi bi-cash-stack",
                Category = ReportCategory.Customer,
                RequiredPermission = "SetoffDocument.Read",
                ActionId = "OpenAccountsReceivableReport",
                SortOrder = 1,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.CustomerStatement,
                Name = "客戶對帳單",
                Description = "產生指定期間的客戶對帳單",
                IconClass = "bi bi-file-earmark-ruled",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerStatementReport",
                SortOrder = 2,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.CustomerSalesAnalysis,
                Name = "客戶銷售分析",
                Description = "分析客戶銷售金額與趨勢",
                IconClass = "bi bi-graph-up",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerSalesAnalysisReport",
                SortOrder = 3,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.CustomerTransaction,
                Name = "客戶交易明細",
                Description = "查詢客戶所有交易記錄明細",
                IconClass = "bi bi-list-check",
                Category = ReportCategory.Customer,
                RequiredPermission = "Customer.Read",
                ActionId = "OpenCustomerTransactionReport",
                SortOrder = 4,
                IsEnabled = false  // 尚未實作
            },
            
            // ==================== 廠商報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.AccountsPayable,
                Name = "應付帳款報表",
                Description = "查詢和列印廠商應付帳款明細資料",
                IconClass = "bi bi-cash-stack",
                Category = ReportCategory.Supplier,
                RequiredPermission = "SetoffDocument.Read",
                ActionId = "OpenAccountsPayableReport",
                SortOrder = 1,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.SupplierStatement,
                Name = "廠商對帳單",
                Description = "產生指定期間的廠商對帳單",
                IconClass = "bi bi-file-earmark-ruled",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierStatementReport",
                SortOrder = 2,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = ReportIds.SupplierPurchaseAnalysis,
                Name = "廠商進貨分析",
                Description = "分析廠商進貨金額與趨勢",
                IconClass = "bi bi-graph-up",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierPurchaseAnalysisReport",
                SortOrder = 3,
                IsEnabled = false  // 尚未實作
            },
            
            // ==================== 採購報表 ====================
            new ReportDefinition
            {
                Id = ReportIds.PurchaseOrder,
                Name = "採購單",
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
                Id = "PD001",
                Name = "商品資料表",
                Description = "列印商品基本資料清單",
                IconClass = "bi bi-box-seam",
                Category = ReportCategory.Product,
                RequiredPermission = "Product.Read",
                ActionId = "PrintProductList",
                SortOrder = 1,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = "PD002",
                Name = "物料清單報表",
                Description = "列印商品BOM物料清單",
                IconClass = "bi bi-diagram-3",
                Category = ReportCategory.Product,
                RequiredPermission = "ProductComposition.Read",
                ActionId = "PrintBOMReport",
                SortOrder = 2,
                IsEnabled = false  // 尚未實作
            },
            new ReportDefinition
            {
                Id = "PD003",
                Name = "商品條碼標籤",
                Description = "列印商品條碼標籤",
                IconClass = "bi bi-upc-scan",
                Category = ReportCategory.Product,
                RequiredPermission = "Product.Read",
                ActionId = "PrintProductBarcode",
                SortOrder = 3,
                IsEnabled = true
            },
            
            // ==================== 庫存報表 ====================
            new ReportDefinition
            {
                Id = "IV001",
                Name = "商品條碼",
                Description = "列印商品條碼標籤",
                IconClass = "bi bi-upc-scan",
                Category = ReportCategory.Inventory,
                RequiredPermission = "Product.Read",
                ActionId = "PrintProductBarcode",
                SortOrder = 1,
                IsEnabled = true
            },
            
            // ==================== 財務報表 ====================
            // 可依需求繼續擴充...
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
    /// 根據報表識別碼取得報表定義
    /// </summary>
    /// <param name="reportId">報表識別碼（如 PO001、AR001）</param>
    /// <returns>報表定義，若找不到則返回 null</returns>
    public static ReportDefinition? GetReportById(string reportId)
    {
        return GetAllReports().FirstOrDefault(r => r.Id == reportId);
    }
}
