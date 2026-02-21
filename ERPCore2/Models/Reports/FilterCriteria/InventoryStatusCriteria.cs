using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 庫存現況表篩選條件
/// </summary>
public class InventoryStatusCriteria : IReportFilterCriteria
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
    /// 商品分類 ID 清單（空表示所有分類）
    /// </summary>
    [FilterFK(typeof(IProductCategoryService),
        Group = FilterGroup.Basic,
        Label = "商品分類",
        Placeholder = "搜尋商品分類...",
        EmptyMessage = "未選擇分類（查詢全部分類）",
        Order = 2)]
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（品號、品名）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋品號、品名...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否包含零庫存商品
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示條件", CheckboxLabel = "包含零庫存", DefaultValue = false, Order = 2)]
    public bool IncludeZeroStock { get; set; } = false;

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
            ["warehouseIds"] = WarehouseIds.Any() ? WarehouseIds : null,
            ["categoryIds"] = CategoryIds.Any() ? CategoryIds : null,
            ["keyword"] = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword,
            ["includeZeroStock"] = IncludeZeroStock
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (WarehouseIds.Any())
            summary.Add($"倉庫：{WarehouseIds.Count} 個");

        if (CategoryIds.Any())
            summary.Add($"分類：{CategoryIds.Count} 個");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (IncludeZeroStock)
            summary.Add("含零庫存");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
