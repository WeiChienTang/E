using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterTemplates;

/// <summary>
/// 報表篩選配置 - 將報表 ID 與篩選模板、服務綁定
/// </summary>
public class ReportFilterConfig
{
    /// <summary>
    /// 報表識別碼（對應 ReportRegistry 中的 Id）
    /// </summary>
    public string ReportId { get; set; } = string.Empty;
    
    /// <summary>
    /// 篩選模板組件類型名稱（完整類別名稱，用於延遲解析避免循環參考）
    /// 格式：ERPCore2.Components.Shared.Report.FilterTemplates.XxxFilterTemplate
    /// </summary>
    public string FilterTemplateTypeName { get; set; } = string.Empty;
    
    // 快取解析後的類型
    private Type? _filterTemplateType;
    
    /// <summary>
    /// 取得篩選模板組件類型（延遲解析）
    /// </summary>
    public Type GetFilterTemplateType()
    {
        if (_filterTemplateType == null && !string.IsNullOrEmpty(FilterTemplateTypeName))
        {
            _filterTemplateType = Type.GetType(FilterTemplateTypeName) 
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == FilterTemplateTypeName);
        }
        return _filterTemplateType ?? typeof(object);
    }
    
    /// <summary>
    /// 篩選條件 DTO 類型（實作 IReportFilterCriteria）
    /// </summary>
    public Type CriteriaType { get; set; } = typeof(EmptyFilterCriteria);
    
    /// <summary>
    /// 報表服務介面類型（用於動態解析服務）
    /// 可為 null，表示此報表尚未實作服務
    /// </summary>
    public Type? ReportServiceType { get; set; }
    
    /// <summary>
    /// 預覽 Modal 標題
    /// </summary>
    public string PreviewTitle { get; set; } = "報表預覽";
    
    /// <summary>
    /// 篩選 Modal 標題
    /// </summary>
    public string FilterTitle { get; set; } = "報表篩選條件";
    
    /// <summary>
    /// 報表圖示（Bootstrap Icons 類別）
    /// </summary>
    public string IconClass { get; set; } = "bi-printer";
    
    /// <summary>
    /// 產生報表文件名稱的委派（可選）
    /// 參數為篩選條件，返回文件名稱
    /// </summary>
    public Func<IReportFilterCriteria, string>? GetDocumentName { get; set; }
    
    /// <summary>
    /// 建立篩選條件的預設值
    /// </summary>
    public IReportFilterCriteria CreateDefaultCriteria()
    {
        return (IReportFilterCriteria)Activator.CreateInstance(CriteriaType)!;
    }
}

/// <summary>
/// 篩選模板組件必須實作的介面
/// </summary>
public interface IFilterTemplateComponent
{
    /// <summary>
    /// 取得目前的篩選條件
    /// </summary>
    IReportFilterCriteria GetCriteria();
    
    /// <summary>
    /// 重置篩選條件為預設值
    /// </summary>
    void Reset();
}
