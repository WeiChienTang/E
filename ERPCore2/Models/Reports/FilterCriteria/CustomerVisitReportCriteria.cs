using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 客戶拜訪報告篩選條件
/// </summary>
public class CustomerVisitReportCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定客戶 ID 清單（空表示所有客戶）
    /// </summary>
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "指定客戶",
        Placeholder = "搜尋客戶編號、名稱...",
        EmptyMessage = "未選擇客戶（查詢全部）",
        DisplayFormat = FilterDisplayFormat.CodeDashName,
        Order = 1)]
    public List<int> CustomerIds { get; set; } = new();

    /// <summary>
    /// 拜訪人員 ID 清單（空表示所有人員）
    /// </summary>
    [FilterFK(typeof(IEmployeeService),
        Group = FilterGroup.Basic,
        Label = "拜訪人員",
        Placeholder = "搜尋拜訪人員...",
        EmptyMessage = "未選擇拜訪人員（查詢全部）",
        DisplayFormat = FilterDisplayFormat.CodeDashName,
        Order = 2)]
    public List<int> EmployeeIds { get; set; } = new();

    /// <summary>
    /// 拜訪起始日期
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "拜訪日期", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 拜訪結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 關鍵字搜尋（客戶名稱、拜訪目的、結果摘要）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋客戶名稱、拜訪目的...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不可大於結束日期";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["employeeIds"] = EmployeeIds.Any() ? EmployeeIds : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["keyword"] = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (StartDate.HasValue)
            summary.Add($"起始：{StartDate:yyyy/MM/dd}");

        if (EndDate.HasValue)
            summary.Add($"結束：{EndDate:yyyy/MM/dd}");

        if (CustomerIds.Any())
            summary.Add($"客戶：{CustomerIds.Count} 家");

        if (EmployeeIds.Any())
            summary.Add($"拜訪人員：{EmployeeIds.Count} 位");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
