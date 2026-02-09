using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterTemplates;

/// <summary>
/// 報表篩選模板註冊表 - 集中管理報表 ID 與篩選模板的對應
/// </summary>
public static class FilterTemplateRegistry
{
    private static readonly Dictionary<string, ReportFilterConfig> _configs = new();
    private static bool _isInitialized = false;
    
    // 模板類型註冊表（由 Blazor 組件層註冊）
    private static readonly Dictionary<string, Type> _templateTypes = new();
    
    /// <summary>
    /// 註冊模板類型（需在應用程式啟動時呼叫，由 Blazor 組件層負責）
    /// </summary>
    public static void RegisterTemplateType(string reportId, Type templateType)
    {
        _templateTypes[reportId] = templateType;
    }
    
    /// <summary>
    /// 初始化註冊表（自動使用已註冊的模板類型）
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        // 客戶報表 - 應收帳款
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = "AR001",
            FilterTemplateType = _templateTypes.GetValueOrDefault("AR001") ?? typeof(object),
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
        
        // 可在此繼續註冊其他報表...
        
        // 採購報表 - 採購單（報表中心進入時顯示篩選，EditModal 直接單筆列印）
        RegisterConfig(new ReportFilterConfig
        {
            ReportId = "PO001",
            FilterTemplateType = _templateTypes.GetValueOrDefault("PO001") ?? typeof(object),
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
