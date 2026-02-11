using ERPCore2.Models.Reports.FilterTemplates;

namespace ERPCore2.Components.Shared.Report;

/// <summary>
/// 報表篩選模板初始化器
/// 負責在應用程式啟動時初始化 FilterTemplateRegistry
/// </summary>
public static class FilterTemplateInitializer
{
    private static readonly object _initLock = new();
    private static volatile bool _isInitialized = false;
    
    /// <summary>
    /// 初始化所有篩選模板配置
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        lock (_initLock)
        {
            if (_isInitialized) return;
            
            // 初始化 FilterTemplateRegistry（所有配置已集中定義在該類別中）
            FilterTemplateRegistry.Initialize();
            
            _isInitialized = true;
        }
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
