using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 品項資料表批次列印篩選條件
/// </summary>
public class ProductListBatchPrintCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 品項分類 ID 清單（空表示所有分類）
    /// </summary>
    [FilterFK(typeof(IProductCategoryService),
        Group = FilterGroup.Basic,
        Label = "品項分類",
        Placeholder = "搜尋品項分類...",
        EmptyMessage = "未選擇分類（列印全部分類）",
        Order = 1)]
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（品號、品名、條碼、規格）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋品號、品名、條碼、規格...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用品項
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
            ["categoryIds"] = CategoryIds.Any() ? CategoryIds : null,
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

        if (CategoryIds.Any())
            summary.Add($"分類：{CategoryIds.Count} 個");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用");

        return summary.Any() ? string.Join(" | ", summary) : "全部品項";
    }
}
