using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 廠商名冊表篩選條件
/// </summary>
public class SupplierRosterCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定廠商 ID 清單（空表示所有廠商）
    /// </summary>
    [FilterFK(typeof(ISupplierService),
        Group = FilterGroup.Basic,
        Label = "指定廠商",
        Placeholder = "搜尋廠商編號、名稱...",
        EmptyMessage = "未選擇廠商（查詢全部）",
        DisplayFormat = FilterDisplayFormat.CodeDashName,
        Order = 1)]
    public List<int> SupplierIds { get; set; } = new();

    /// <summary>
    /// 付款方式 ID 清單（空表示所有付款方式）
    /// </summary>
    [FilterFK(typeof(IPaymentMethodService),
        Group = FilterGroup.Basic,
        Label = "付款方式",
        Placeholder = "搜尋付款方式...",
        EmptyMessage = "未選擇付款方式（查詢全部）",
        Order = 2)]
    public List<int> PaymentMethodIds { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（廠商編號、公司名稱、聯絡人、統編）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋廠商編號、廠商名稱、聯絡人、統編...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用廠商（預設 true）
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
            ["supplierIds"] = SupplierIds.Any() ? SupplierIds : null,
            ["paymentMethodIds"] = PaymentMethodIds.Any() ? PaymentMethodIds : null,
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

        if (PaymentMethodIds.Any())
            summary.Add($"付款方式：{PaymentMethodIds.Count} 種");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
