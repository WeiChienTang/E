using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 庫存盤點差異表篩選條件
/// </summary>
public class StockTakingDifferenceCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定倉庫 ID 清單（空表示所有倉庫）
    /// </summary>
    public List<int> WarehouseIds { get; set; } = new();

    /// <summary>
    /// 起始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否僅顯示有差異的項目
    /// </summary>
    public bool OnlyDifferenceItems { get; set; } = false;

    /// <summary>
    /// 關鍵字搜尋（盤點單號、品號、品名）
    /// </summary>
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
