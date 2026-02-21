using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 客戶交易明細篩選條件
/// </summary>
public class CustomerTransactionCriteria : IReportFilterCriteria
{
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
    /// 起始日期
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否包含出貨單
    /// </summary>
    [FilterToggle(Group = FilterGroup.Basic, Label = "選項", CheckboxLabel = "出貨單", DefaultValue = true, Order = 2)]
    public bool IncludeDeliveries { get; set; } = true;

    /// <summary>
    /// 是否包含退貨單
    /// </summary>
    [FilterToggle(Group = FilterGroup.Basic, Label = "選項", CheckboxLabel = "退貨單", DefaultValue = true, Order = 3)]
    public bool IncludeReturns { get; set; } = true;

    /// <summary>
    /// 是否排除已取消
    /// </summary>
    [FilterToggle(Group = FilterGroup.Basic, Label = "選項", CheckboxLabel = "排除已取消", DefaultValue = true, Order = 4)]
    public bool ExcludeCancelled { get; set; } = true;

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

        if (!StartDate.HasValue && !EndDate.HasValue)
        {
            errorMessage = "請指定查詢日期範圍";
            return false;
        }

        if (!IncludeDeliveries && !IncludeReturns)
        {
            errorMessage = "至少須選擇一種交易類型（出貨或退貨）";
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
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["includeDeliveries"] = IncludeDeliveries,
            ["includeReturns"] = IncludeReturns,
            ["excludeCancelled"] = ExcludeCancelled
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
            summary.Add($"客戶：{CustomerIds.Count} 個");

        var types = new List<string>();
        if (IncludeDeliveries) types.Add("出貨");
        if (IncludeReturns) types.Add("退貨");
        summary.Add($"類型：{string.Join("、", types)}");

        if (ExcludeCancelled)
            summary.Add("排除已取消");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
