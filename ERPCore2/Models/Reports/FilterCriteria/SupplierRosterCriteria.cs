using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 廠商名冊表篩選條件
/// </summary>
public class SupplierRosterCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定廠商 ID 清單（空表示所有廠商）
    /// </summary>
    public List<int> SupplierIds { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（廠商編號、公司名稱、聯絡人、統編）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用廠商（預設 true）
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
            ["supplierIds"] = SupplierIds.Any() ? SupplierIds : null,
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

        if (SupplierIds.Any())
            summary.Add($"廠商：{SupplierIds.Count} 家");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
