using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 應付帳款帳齡分析篩選條件
/// 以收貨日 + 廠商付款天數計算到期日，再依到期日距截止日的天數分組
/// </summary>
public class APAgingCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 帳齡截止日（預設今日）
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "帳齡截止日", Order = 1)]
    public DateTime? AsOfDate { get; set; } = DateTime.Today;

    /// <summary>
    /// 結束日期（FilterDateRange 需要，帳齡分析僅使用 AsOfDate，此欄位忽略）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 指定廠商 ID 清單（空表示所有廠商）
    /// </summary>
    [FilterFK(typeof(ISupplierService),
        Group = FilterGroup.Basic,
        Label = "指定廠商",
        Placeholder = "搜尋廠商...",
        EmptyMessage = "未選擇廠商（查詢全部）",
        Order = 1)]
    public List<int> SupplierIds { get; set; } = new();

    /// <summary>
    /// 是否顯示餘額為零的廠商（預設隱藏）
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示選項", CheckboxLabel = "顯示已結清（餘額為零）", DefaultValue = false, Order = 1)]
    public bool ShowZeroBalance { get; set; } = false;

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (!AsOfDate.HasValue)
        {
            errorMessage = "請指定帳齡截止日";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["asOfDate"] = AsOfDate,
            ["supplierIds"] = SupplierIds.Any() ? SupplierIds : null,
            ["showZeroBalance"] = ShowZeroBalance
        };
    }

    public string GetSummary()
    {
        var summary = new List<string>();

        summary.Add($"截止日：{AsOfDate:yyyy/MM/dd}");

        if (SupplierIds.Any())
            summary.Add($"廠商：{SupplierIds.Count} 家");

        if (ShowZeroBalance)
            summary.Add("含已結清");

        return string.Join(" | ", summary);
    }
}
