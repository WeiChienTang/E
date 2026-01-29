using ERPCore2.Models;

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
                Id = "AR001",
                Name = "應收帳款報表",
                Description = "查詢和列印客戶應收帳款明細資料",
                IconClass = "bi bi-cash-stack",
                Category = ReportCategory.Customer,
                RequiredPermission = "SetoffDocument.Read",
                ActionId = "OpenAccountsReceivableReport",
                SortOrder = 1
            },
            new ReportDefinition
            {
                Id = "AR002",
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
                Id = "AR003",
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
                Id = "AR004",
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
                Id = "AP001",
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
                Id = "AP002",
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
                Id = "AP003",
                Name = "廠商進貨分析",
                Description = "分析廠商進貨金額與趨勢",
                IconClass = "bi bi-graph-up",
                Category = ReportCategory.Supplier,
                RequiredPermission = "Supplier.Read",
                ActionId = "OpenSupplierPurchaseAnalysisReport",
                SortOrder = 3,
                IsEnabled = false  // 尚未實作
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
}
