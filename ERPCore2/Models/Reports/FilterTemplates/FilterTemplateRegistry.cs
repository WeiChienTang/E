using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterTemplates;

/// <summary>
/// 報表篩選模板註冊表 - 集中管理報表 ID 與篩選模板的對應
/// 新增報表篩選時，只需在此檔案新增配置即可
/// </summary>
public static class FilterTemplateRegistry
{
    private static readonly Dictionary<string, ReportFilterConfig> _configs = new();
    private static bool _isInitialized = false;
    
    /// <summary>
    /// 初始化註冊表（直接在此定義所有模板配置）
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        // ==================== 客戶報表 ====================
        
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
        
        // ==================== 採購報表 ====================
        
        // 採購單（報表中心進入時顯示篩選，EditModal 直接單筆列印）
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
        
        // 可在此繼續註冊其他報表...
        
        _isInitialized = true;
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
        _configs[config.ReportId] = config;
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
