using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 應收帳款報表篩選條件
/// </summary>
public class AccountsReceivableCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 客戶 ID 清單（空表示所有客戶）
    /// </summary>
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "指定客戶",
        Placeholder = "搜尋客戶...",
        EmptyMessage = "未選擇客戶（查詢全部）",
        Order = 1)]
    public List<int> CustomerIds { get; set; } = new();

    /// <summary>
    /// 業務負責人 ID 清單（空表示所有業務）
    /// </summary>
    [FilterFK(typeof(IEmployeeService),
        Group = FilterGroup.Basic,
        Label = "業務負責人",
        Placeholder = "搜尋業務負責人...",
        EmptyMessage = "未選擇業務負責人（查詢全部）",
        DisplayFormat = FilterDisplayFormat.CodeDashName,
        Order = 2)]
    public List<int> EmployeeIds { get; set; } = new();

    /// <summary>
    /// 起始日期
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否包含已結清帳款
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示選項", CheckboxLabel = "包含已結清帳款", DefaultValue = false, Order = 1)]
    public bool IncludeSettled { get; set; } = false;

    /// <summary>
    /// 是否僅顯示逾期帳款
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示選項", CheckboxLabel = "僅顯示逾期帳款", DefaultValue = false, Order = 2)]
    public bool OnlyOverdue { get; set; } = false;

    public bool Validate(out string? errorMessage)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不能大於結束日期";
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
            ["includeSettled"] = IncludeSettled,
            ["onlyOverdue"] = OnlyOverdue
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (StartDate.HasValue || EndDate.HasValue)
        {
            var dateRange = $"{StartDate?.ToString("yyyy/MM/dd") ?? "不限"} ~ {EndDate?.ToString("yyyy/MM/dd") ?? "不限"}";
            summary.Add($"日期：{dateRange}");
        }

        if (CustomerIds.Any())
            summary.Add($"客戶：{CustomerIds.Count} 家");

        if (EmployeeIds.Any())
            summary.Add($"業務：{EmployeeIds.Count} 人");

        if (IncludeSettled)
            summary.Add("含已結清");

        if (OnlyOverdue)
            summary.Add("僅逾期");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
