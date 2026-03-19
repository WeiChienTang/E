using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 應收帳款帳齡分析篩選條件
/// 以出貨日 + 客戶付款天數計算到期日，再依到期日距截止日的天數分組
/// </summary>
public class ARAgingCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 帳齡截止日（預設今日）
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "帳齡截止日", Order = 1)]
    public DateTime? AsOfDate { get; set; } = DateTime.Today;

    /// <summary>
    /// 結束日期（FilterDateRange 需要，帳齡分析僅使用 AsOfDate，此欄位忽略）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 指定客戶 ID 清單（空表示所有客戶）
    /// </summary>
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "指定客戶",
        Placeholder = "搜尋客戶...",
        EmptyMessage = "未選擇客戶（查詢全部）",
        Order = 1)]
    public List<int> CustomerIds { get; set; } = new();

    /// <summary>
    /// 業務負責人 ID 清單（空表示全部業務）
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
    /// 是否顯示餘額為零的客戶（預設隱藏）
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示選項", CheckboxLabel = "顯示已結清（餘額為零）", DefaultValue = false, Order = 1)]
    public bool ShowZeroBalance { get; set; } = false;

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (!AsOfDate.HasValue)
        {
            errorMessage = "請指定帳齡截止日";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["asOfDate"] = AsOfDate,
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["employeeIds"] = EmployeeIds.Any() ? EmployeeIds : null,
            ["showZeroBalance"] = ShowZeroBalance
        };
    }

    public string GetSummary()
    {
        var summary = new List<string>();

        summary.Add($"截止日：{AsOfDate:yyyy/MM/dd}");

        if (CustomerIds.Any())
            summary.Add($"客戶：{CustomerIds.Count} 家");

        if (EmployeeIds.Any())
            summary.Add($"業務：{EmployeeIds.Count} 人");

        if (ShowZeroBalance)
            summary.Add("含已結清");

        return string.Join(" | ", summary);
    }
}
