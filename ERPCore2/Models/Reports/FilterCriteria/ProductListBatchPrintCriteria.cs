using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 商品資料表批次列印篩選條件
/// </summary>
public class ProductListBatchPrintCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 商品分類 ID 清單（空表示所有分類）
    /// </summary>
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>
    /// 採購類型篩選（null 表示全部）
    /// </summary>
    public ProcurementType? ProcurementType { get; set; }

    /// <summary>
    /// 關鍵字搜尋（品號、品名、條碼、規格）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用商品
    /// </summary>
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
            ["procurementType"] = ProcurementType,
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
        {
            summary.Add($"分類：{CategoryIds.Count} 個");
        }

        if (ProcurementType.HasValue)
        {
            var typeName = ProcurementType.Value switch
            {
                Enums.ProcurementType.Purchased => "外購",
                Enums.ProcurementType.Manufactured => "自製",
                Enums.ProcurementType.Outsourced => "委外",
                _ => ProcurementType.Value.ToString()
            };
            summary.Add($"採購類型：{typeName}");
        }

        if (!string.IsNullOrEmpty(Keyword))
        {
            summary.Add($"關鍵字：{Keyword}");
        }

        if (ActiveOnly)
        {
            summary.Add("僅啟用");
        }

        return summary.Any() ? string.Join(" | ", summary) : "全部商品";
    }
}
