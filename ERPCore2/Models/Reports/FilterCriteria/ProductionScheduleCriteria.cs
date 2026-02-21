using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 生產排程表篩選條件
/// </summary>
public class ProductionScheduleCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 排程日期起始
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 排程日期結束
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 指定排程單 ID 清單（空表示不限定，用於從編輯畫面列印單一排程）
    /// </summary>
    public List<int> ScheduleIds { get; set; } = new();

    /// <summary>
    /// 指定客戶 ID 清單（空表示所有客戶）
    /// </summary>
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "指定客戶",
        Placeholder = "搜尋客戶...",
        EmptyMessage = "未選擇客戶（查詢全部客戶）",
        Order = 1)]
    public List<int> CustomerIds { get; set; } = new();

    /// <summary>
    /// 生產狀態篩選（空表示所有狀態）
    /// </summary>
    [FilterEnum(typeof(ProductionItemStatus), Group = FilterGroup.Quick, Label = "生產狀態", Order = 1)]
    public List<ProductionItemStatus> StatusFilters { get; set; } = new();

    /// <summary>
    /// 是否包含已結案項目
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "生產狀態", CheckboxLabel = "包含已結案", DefaultValue = false, Order = 2)]
    public bool IncludeClosed { get; set; } = false;

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

        // 有指定排程單 ID 時不需要日期範圍
        if (!ScheduleIds.Any() && !StartDate.HasValue && !EndDate.HasValue)
        {
            errorMessage = "請指定查詢日期範圍";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["statusFilters"] = StatusFilters.Any() ? StatusFilters : null,
            ["includeClosed"] = IncludeClosed
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

        if (StatusFilters.Any())
        {
            var statusNames = StatusFilters.Select(s => s switch
            {
                ProductionItemStatus.Pending => "待生產",
                ProductionItemStatus.InProgress => "生產中",
                ProductionItemStatus.Completed => "已完成",
                ProductionItemStatus.Discontinued => "已停產",
                _ => s.ToString()
            });
            summary.Add($"狀態：{string.Join("、", statusNames)}");
        }

        if (IncludeClosed)
            summary.Add("含已結案");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
