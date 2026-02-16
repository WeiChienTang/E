using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 員工名冊表篩選條件
/// </summary>
public class EmployeeRosterCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定部門 ID 清單（空表示所有部門）
    /// </summary>
    public List<int> DepartmentIds { get; set; } = new();

    /// <summary>
    /// 指定在職狀態清單（空表示所有狀態）
    /// </summary>
    public List<EmployeeStatus> EmploymentStatuses { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（員工編號、姓名）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示在職員工（預設 true）
    /// </summary>
    public bool ActiveOnly { get; set; } = true;

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["departmentIds"] = DepartmentIds.Any() ? DepartmentIds : null,
            ["employmentStatuses"] = EmploymentStatuses.Any() ? EmploymentStatuses : null,
            ["keyword"] = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword,
            ["activeOnly"] = ActiveOnly
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (DepartmentIds.Any())
            summary.Add($"部門：{DepartmentIds.Count} 個");

        if (EmploymentStatuses.Any())
            summary.Add($"狀態：{EmploymentStatuses.Count} 種");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅在職");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
