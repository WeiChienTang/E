using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 庫存盤點差異表篩選條件
/// </summary>
public class StockTakingDifferenceCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定倉庫 ID 清單（空表示所有倉庫）
    /// </summary>
    [FilterFK(typeof(IWarehouseService),
        Group = FilterGroup.Basic,
        Label = "指定倉庫",
        Placeholder = "搜尋倉庫...",
        EmptyMessage = "未選擇倉庫（查詢全部倉庫）",
        Order = 1)]
    public List<int> WarehouseIds { get; set; } = new();

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
    /// 是否僅顯示有差異的項目
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示條件", CheckboxLabel = "僅差異項目", DefaultValue = false, Order = 2)]
    public bool OnlyDifferenceItems { get; set; } = false;

    /// <summary>
    /// 關鍵字搜尋（盤點單號、品號、品名）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋盤點單號、品號、品名...", Order = 1)]
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
            ["warehouseIds"] = WarehouseIds.Any() ? WarehouseIds : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["onlyDifferenceItems"] = OnlyDifferenceItems,
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

        if (WarehouseIds.Any())
            summary.Add($"倉庫：{WarehouseIds.Count} 個");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (OnlyDifferenceItems)
            summary.Add("僅差異項目");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
