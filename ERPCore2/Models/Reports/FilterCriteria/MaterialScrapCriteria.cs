using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 用料損耗退料記錄報表篩選條件
/// 只查詢有損耗量或退料量的明細記錄
/// </summary>
public class MaterialScrapCriteria : IReportFilterCriteria
{
    /// <summary>結算日期起始（以明細最後更新日期為準）</summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "結算日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>結算日期結束</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>指定成品（生產的主產品），空表示全部</summary>
    [FilterFK(typeof(IItemService),
        Group = FilterGroup.Basic,
        Label = "成品",
        Placeholder = "搜尋成品...",
        EmptyMessage = "未選擇（查詢全部成品）",
        Order = 1)]
    public List<int> ItemIds { get; set; } = new();

    /// <summary>指定組件，空表示全部</summary>
    [FilterFK(typeof(IItemService),
        Group = FilterGroup.Basic,
        Label = "組件",
        Placeholder = "搜尋組件...",
        EmptyMessage = "未選擇（查詢全部組件）",
        Order = 2)]
    public List<int> ComponentItemIds { get; set; } = new();

    /// <summary>只顯示有損耗量的記錄（ScrapQty > 0）</summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "損耗篩選", CheckboxLabel = "只顯示有損耗量", DefaultValue = false, Order = 1)]
    public bool OnlyWithScrap { get; set; } = false;

    /// <summary>只顯示有退料量的記錄（ReturnQty > 0）</summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "退料篩選", CheckboxLabel = "只顯示有退料量", DefaultValue = false, Order = 2)]
    public bool OnlyWithReturn { get; set; } = false;

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
        ["componentItemIds"] = ComponentItemIds.Any() ? ComponentItemIds : null,
        ["onlyWithScrap"] = OnlyWithScrap,
        ["onlyWithReturn"] = OnlyWithReturn
    };

    public string GetSummary()
    {
        var parts = new List<string>();
        if (StartDate.HasValue) parts.Add($"起：{StartDate:yyyy/MM/dd}");
        if (EndDate.HasValue) parts.Add($"迄：{EndDate:yyyy/MM/dd}");
        if (ItemIds.Any()) parts.Add($"成品：{ItemIds.Count} 項");
        if (ComponentItemIds.Any()) parts.Add($"組件：{ComponentItemIds.Count} 項");
        if (OnlyWithScrap) parts.Add("含損耗");
        if (OnlyWithReturn) parts.Add("含退料");
        return parts.Any() ? string.Join(" | ", parts) : "全部";
    }
}
