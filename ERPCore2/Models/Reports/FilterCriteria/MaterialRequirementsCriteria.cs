using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 用料需求報表篩選條件
/// 彙總指定日期範圍內所有生產排程項目的物料需求，依組件品號統計
/// </summary>
public class MaterialRequirementsCriteria : IReportFilterCriteria
{
    /// <summary>排程日期起始（以 PlannedStartDate 為準）</summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "排程日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>排程日期結束</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>指定成品（生產的主產品），空表示全部</summary>
    [FilterFK(typeof(IItemService),
        Group = FilterGroup.Basic,
        Label = "成品篩選",
        Placeholder = "搜尋成品...",
        EmptyMessage = "未選擇（查詢全部成品）",
        Order = 1)]
    public List<int> ItemIds { get; set; } = new();

    /// <summary>排除已完成的生產項目（ProductionItemStatus == Completed）</summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "狀態篩選", CheckboxLabel = "排除已完成項目", DefaultValue = true, Order = 1)]
    public bool ExcludeCompleted { get; set; } = true;

    /// <summary>只顯示尚有待領量的組件（RequiredQty > IssuedQty）</summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "領料篩選", CheckboxLabel = "只顯示尚有待領量", DefaultValue = false, Order = 2)]
    public bool OnlyPendingIssue { get; set; } = false;

    /// <summary>紙張設定（由報表預覽框架注入）</summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (!StartDate.HasValue && !EndDate.HasValue)
        {
            errorMessage = "請指定查詢日期範圍";
            return false;
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不可大於結束日期";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters() => new()
    {
        ["startDate"] = StartDate,
        ["endDate"] = EndDate,
        ["productIds"] = ItemIds.Any() ? ItemIds : null,
        ["excludeCompleted"] = ExcludeCompleted,
        ["onlyPendingIssue"] = OnlyPendingIssue
    };

    public string GetSummary()
    {
        var parts = new List<string>();
        if (StartDate.HasValue) parts.Add($"起：{StartDate:yyyy/MM/dd}");
        if (EndDate.HasValue) parts.Add($"迄：{EndDate:yyyy/MM/dd}");
        if (ItemIds.Any()) parts.Add($"成品：{ItemIds.Count} 項");
        if (ExcludeCompleted) parts.Add("排除已完成");
        if (OnlyPendingIssue) parts.Add("只含待領");
        return parts.Any() ? string.Join(" | ", parts) : "全部";
    }
}
