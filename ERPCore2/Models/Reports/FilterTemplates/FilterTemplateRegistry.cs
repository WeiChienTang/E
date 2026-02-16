using System.Collections.Concurrent;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterTemplates;

/// <summary>
/// 報表篩選模板註冊表 - 集中管理報表 ID 與篩選模板的對應
/// 新增報表篩選時，只需在此檔案新增配置即可
/// </summary>
public static class FilterTemplateRegistry
{
    private static readonly ConcurrentDictionary<string, ReportFilterConfig> _configs = new();
    private static readonly object _initLock = new();
    private static volatile bool _isInitialized = false;
    
    /// <summary>
    /// 初始化註冊表（直接在此定義所有模板配置）
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        lock (_initLock)
        {
            if (_isInitialized) return;
        
        // ==================== 客戶報表 ====================

        // 客戶對帳單（含期初餘額、出貨/退貨/收款明細、期末餘額）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.CustomerStatement,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.CustomerStatementBatchFilterTemplate",
            CriteriaType = typeof(CustomerStatementCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ICustomerStatementReportService),
            PreviewTitle = "客戶對帳單預覽",
            FilterTitle = "客戶對帳單篩選條件",
            IconClass = "bi-file-earmark-ruled",
            GetDocumentName = criteria =>
            {
                var c = (CustomerStatementCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"客戶對帳單-{dateRange}";
            }
        });

        // 應收帳款報表
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.AccountsReceivable,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.AccountsReceivableFilterTemplate",
            CriteriaType = typeof(AccountsReceivableCriteria),
            ReportServiceType = null,  // TODO: 待實作 IAccountsReceivableReportService
            PreviewTitle = "應收帳款報表預覽",
            FilterTitle = "應收帳款報表篩選條件",
            IconClass = "bi-cash-stack",
            GetDocumentName = criteria =>
            {
                var c = (AccountsReceivableCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"應收帳款報表-{dateRange}";
            }
        });

        // 客戶銷售分析（按銷售額由高至低排列）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.CustomerSalesAnalysis,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.CustomerSalesAnalysisBatchFilterTemplate",
            CriteriaType = typeof(CustomerSalesAnalysisCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ICustomerSalesAnalysisReportService),
            PreviewTitle = "客戶銷售分析預覽",
            FilterTitle = "客戶銷售分析篩選條件",
            IconClass = "bi-graph-up",
            GetDocumentName = criteria =>
            {
                var c = (CustomerSalesAnalysisCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"客戶銷售分析-{dateRange}";
            }
        });

        // 客戶交易明細（依客戶分組，列出出貨與退貨明細）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.CustomerTransaction,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.CustomerTransactionBatchFilterTemplate",
            CriteriaType = typeof(CustomerTransactionCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ICustomerTransactionReportService),
            PreviewTitle = "客戶交易明細預覽",
            FilterTitle = "客戶交易明細篩選條件",
            IconClass = "bi-list-check",
            GetDocumentName = criteria =>
            {
                var c = (CustomerTransactionCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"客戶交易明細-{dateRange}";
            }
        });

        // ==================== 廠商報表 ====================

        // 廠商對帳單（含期初餘額、進貨/退貨/付款明細、期末餘額）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.SupplierStatement,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.SupplierStatementBatchFilterTemplate",
            CriteriaType = typeof(SupplierStatementCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ISupplierStatementReportService),
            PreviewTitle = "廠商對帳單預覽",
            FilterTitle = "廠商對帳單篩選條件",
            IconClass = "bi-file-earmark-ruled",
            GetDocumentName = criteria =>
            {
                var c = (SupplierStatementCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"廠商對帳單-{dateRange}";
            }
        });

        // ==================== 採購報表 ====================

        // 採購單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.PurchaseOrder,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.PurchaseOrderBatchFilterTemplate",
            CriteriaType = typeof(PurchaseOrderBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IPurchaseOrderReportService),
            PreviewTitle = "採購單列印預覽",
            FilterTitle = "採購單列印篩選條件",
            IconClass = "bi-cart-plus",
            GetDocumentName = criteria => 
            {
                var c = (PurchaseOrderBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"採購單-{dateRange}";
            }
        });
        
        // 進貨單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.PurchaseReceiving,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.PurchaseReceivingBatchFilterTemplate",
            CriteriaType = typeof(PurchaseReceivingBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IPurchaseReceivingReportService),
            PreviewTitle = "進貨單列印預覽",
            FilterTitle = "進貨單列印篩選條件",
            IconClass = "bi-box-arrow-in-down",
            GetDocumentName = criteria => 
            {
                var c = (PurchaseReceivingBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"進貨單-{dateRange}";
            }
        });
        
        // 進貨退出單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.PurchaseReturn,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.PurchaseReturnBatchFilterTemplate",
            CriteriaType = typeof(PurchaseReturnBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IPurchaseReturnReportService),
            PreviewTitle = "進貨退出單列印預覽",
            FilterTitle = "進貨退出單列印篩選條件",
            IconClass = "bi-box-arrow-up",
            GetDocumentName = criteria => 
            {
                var c = (PurchaseReturnBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"進貨退出單-{dateRange}";
            }
        });
        
        // ==================== 銷售報表 ====================
        
        // 報價單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.Quotation,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.QuotationBatchFilterTemplate",
            CriteriaType = typeof(QuotationBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IQuotationReportService),
            PreviewTitle = "報價單列印預覽",
            FilterTitle = "報價單列印篩選條件",
            IconClass = "bi-file-earmark-text",
            GetDocumentName = criteria => 
            {
                var c = (QuotationBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"報價單-{dateRange}";
            }
        });
        
        // 訂單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.SalesOrder,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.SalesOrderBatchFilterTemplate",
            CriteriaType = typeof(SalesOrderBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ISalesOrderReportService),
            PreviewTitle = "訂單列印預覽",
            FilterTitle = "訂單列印篩選條件",
            IconClass = "bi-file-earmark-text",
            GetDocumentName = criteria => 
            {
                var c = (SalesOrderBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"訂單-{dateRange}";
            }
        });
        
        // 銷貨單（出貨單）（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.SalesDelivery,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.SalesDeliveryBatchFilterTemplate",
            CriteriaType = typeof(SalesDeliveryBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ISalesDeliveryReportService),
            PreviewTitle = "銷貨單列印預覽",
            FilterTitle = "銷貨單列印篩選條件",
            IconClass = "bi-file-earmark-text",
            GetDocumentName = criteria => 
            {
                var c = (SalesDeliveryBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"銷貨單-{dateRange}";
            }
        });
        
        // 銷貨退回單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.SalesReturn,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.SalesReturnBatchFilterTemplate",
            CriteriaType = typeof(SalesReturnBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ISalesReturnReportService),
            PreviewTitle = "銷貨退回單列印預覽",
            FilterTitle = "銷貨退回單列印篩選條件",
            IconClass = "bi-file-earmark-text",
            GetDocumentName = criteria => 
            {
                var c = (SalesReturnBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue 
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}" 
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"銷貨退回單-{dateRange}";
            }
        });
        
        // ==================== 商品報表 ====================
        
        // 商品條碼標籤
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.ProductBarcode,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.ProductBarcodeBatchFilterTemplate",
            CriteriaType = typeof(ProductBarcodeBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IProductBarcodeReportService),
            PreviewTitle = "商品條碼標籤預覽",
            FilterTitle = "商品條碼列印篩選條件",
            IconClass = "bi-upc-scan",
            GetDocumentName = criteria => 
            {
                var c = (ProductBarcodeBatchPrintCriteria)criteria;
                var count = c.ProductIds.Count;
                var total = c.PrintQuantities.Values.Sum();
                return $"商品條碼-{count}品項-{total}張-{DateTime.Now:yyyyMMddHHmm}";
            }
        });
        
        // ==================== 商品報表（清單式）====================

        // 商品資料表（報表集進入時顯示篩選，清單式報表）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.ProductList,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.ProductListBatchFilterTemplate",
            CriteriaType = typeof(ProductListBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IProductListReportService),
            PreviewTitle = "商品資料表預覽",
            FilterTitle = "商品資料表篩選條件",
            IconClass = "bi-box-seam",
            GetDocumentName = criteria =>
            {
                return $"商品資料表-{DateTime.Now:yyyyMMddHHmm}";
            }
        });

        // 生產排程表（依排程單分組顯示排程項目明細）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.ProductionSchedule,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.ProductionScheduleBatchFilterTemplate",
            CriteriaType = typeof(ProductionScheduleCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.IProductionScheduleReportService),
            PreviewTitle = "生產排程表預覽",
            FilterTitle = "生產排程表篩選條件",
            IconClass = "bi-calendar-check",
            GetDocumentName = criteria =>
            {
                var c = (ProductionScheduleCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"生產排程表-{dateRange}";
            }
        });

        // ==================== 財務報表 ====================

        // 應收沖款單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.AccountsReceivableSetoff,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.AccountsReceivableSetoffBatchFilterTemplate",
            CriteriaType = typeof(SetoffDocumentBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ISetoffDocumentReportService),
            PreviewTitle = "應收沖款單列印預覽",
            FilterTitle = "應收沖款單列印篩選條件",
            IconClass = "bi-receipt-cutoff",
            GetDocumentName = criteria =>
            {
                var c = (SetoffDocumentBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"應收沖款單-{dateRange}";
            }
        });

        // 應付沖款單（報表集進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = ReportIds.AccountsPayableSetoff,
            FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.AccountsPayableSetoffBatchFilterTemplate",
            CriteriaType = typeof(SetoffDocumentBatchPrintCriteria),
            ReportServiceType = typeof(ERPCore2.Services.Reports.Interfaces.ISetoffDocumentReportService),
            PreviewTitle = "應付沖款單列印預覽",
            FilterTitle = "應付沖款單列印篩選條件",
            IconClass = "bi-receipt-cutoff",
            GetDocumentName = criteria =>
            {
                var c = (SetoffDocumentBatchPrintCriteria)criteria;
                var dateRange = c.StartDate.HasValue && c.EndDate.HasValue
                    ? $"{c.StartDate:yyyyMMdd}-{c.EndDate:yyyyMMdd}"
                    : DateTime.Now.ToString("yyyyMMdd");
                return $"應付沖款單-{dateRange}";
            }
        });

            _isInitialized = true;
        }
    }
    
    /// <summary>
    /// 確保已初始化（延遲初始化）
    /// </summary>
    public static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }
    
    /// <summary>
    /// 註冊篩選配置
    /// </summary>
    public static void RegisterConfig(ReportFilterConfig config)
    {
        _configs.TryAdd(config.ReportId, config);
    }
    
    /// <summary>
    /// 根據報表 ID 取得篩選配置
    /// </summary>
    /// <param name="reportId">報表識別碼</param>
    /// <returns>篩選配置，找不到時返回 null</returns>
    public static ReportFilterConfig? GetConfig(string reportId)
    {
        return _configs.GetValueOrDefault(reportId);
    }
    
    /// <summary>
    /// 檢查報表是否有篩選配置
    /// </summary>
    public static bool HasConfig(string reportId)
    {
        return _configs.ContainsKey(reportId);
    }
    
    /// <summary>
    /// 取得所有已註冊的配置
    /// </summary>
    public static IReadOnlyDictionary<string, ReportFilterConfig> GetAllConfigs()
    {
        return _configs;
    }
}