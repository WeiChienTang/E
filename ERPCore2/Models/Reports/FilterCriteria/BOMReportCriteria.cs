using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 物料清單報表篩選條件
/// </summary>
public class BOMReportCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 成品（父商品）ID 清單（空表示所有成品）
    /// </summary>
    [FilterFK(typeof(IProductCompositionService),
        Group = FilterGroup.Basic,
        Label = "指定成品",
        Placeholder = "搜尋成品品號或品名...",
        EmptyMessage = "未選擇成品（查詢全部）",
        DisplayFormat = FilterDisplayFormat.CodeDashName,
        Order = 1)]
    public List<int> ParentProductIds { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（配方編號、成品品號、成品品名）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋配方編號、成品品號、成品品名...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用（預設 true）
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
            ["parentProductIds"] = ParentProductIds.Any() ? ParentProductIds : null,
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

        if (ParentProductIds.Any())
            summary.Add($"成品：{ParentProductIds.Count} 個");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
