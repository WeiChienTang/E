namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 報表篩選條件基礎介面
/// 所有報表篩選條件 DTO 都應實作此介面
/// </summary>
public interface IReportFilterCriteria
{
    /// <summary>
    /// 驗證篩選條件是否有效
    /// </summary>
    /// <param name="errorMessage">驗證失敗時的錯誤訊息</param>
    /// <returns>驗證是否通過</returns>
    bool Validate(out string? errorMessage);
    
    /// <summary>
    /// 轉換為查詢參數字典（用於 API 或 Service 呼叫）
    /// </summary>
    /// <returns>參數字典</returns>
    Dictionary<string, object?> ToQueryParameters();
}

/// <summary>
/// 空篩選條件（用於不需要篩選的報表）
/// </summary>
public class EmptyFilterCriteria : IReportFilterCriteria
{
    public bool Validate(out string? errorMessage)
    {
        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>();
    }
}
