using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 客戶名冊表篩選條件
/// </summary>
public class CustomerRosterCriteria : IReportFilterCriteria
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
    /// 指定業務負責人 ID 清單（空表示所有）
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
    /// 關鍵字搜尋（客戶編號、公司名稱、聯絡人、統編）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋客戶編號、公司名稱、聯絡人、統編...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用客戶（預設 true）
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示條件", CheckboxLabel = "僅啟用", DefaultValue = true, Order = 2)]
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
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["employeeIds"] = EmployeeIds.Any() ? EmployeeIds : null,
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

        if (CustomerIds.Any())
            summary.Add($"客戶：{CustomerIds.Count} 家");

        if (EmployeeIds.Any())
            summary.Add($"業務負責人：{EmployeeIds.Count} 位");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
