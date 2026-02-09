using ERPCore2.Models.Reports.FilterTemplates;
using ERPCore2.Components.Shared.Report.FilterTemplates;

namespace ERPCore2.Components.Shared.Report;

/// <summary>
/// 報表篩選模板初始化器
/// 負責註冊所有篩選模板組件到 FilterTemplateRegistry
/// </summary>
public static class FilterTemplateInitializer
{
    private static bool _isInitialized = false;
    
    /// <summary>
    /// 初始化所有篩選模板
    /// 應在應用程式啟動時呼叫
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        // 註冊所有模板類型
        FilterTemplateRegistry.RegisterTemplateType("AR001", typeof(AccountsReceivableFilterTemplate));
        FilterTemplateRegistry.RegisterTemplateType("PO001", typeof(PurchaseOrderBatchFilterTemplate)); // 報表中心進入時用
        
        // 初始化 FilterTemplateRegistry
        FilterTemplateRegistry.Initialize();
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// 確保已初始化（可在任何地方呼叫）
    /// </summary>
    public static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }
}
