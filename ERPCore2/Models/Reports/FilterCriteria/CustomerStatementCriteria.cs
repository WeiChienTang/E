using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 客戶對帳單篩選條件
/// </summary>
public class CustomerStatementCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定客戶 ID 清單（空表示所有客戶）
    /// </summary>
    public List<int> CustomerIds { get; set; } = new();

    /// <summary>
    /// 起始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否包含出貨單
    /// </summary>
    public bool IncludeDeliveries { get; set; } = true;

    /// <summary>
    /// 是否包含退貨單
    /// </summary>
    public bool IncludeReturns { get; set; } = true;

    /// <summary>
    /// 是否包含收款（沖款單）
    /// </summary>
    public bool IncludePayments { get; set; } = true;

    /// <summary>
    /// 是否排除已取消
    /// </summary>
    public bool ExcludeCancelled { get; set; } = true;

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

        if (!StartDate.HasValue && !EndDate.HasValue)
        {
            errorMessage = "請指定查詢日期範圍";
            return false;
        }

        if (!IncludeDeliveries && !IncludeReturns && !IncludePayments)
        {
            errorMessage = "至少須選擇一種交易類型";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["includeDeliveries"] = IncludeDeliveries,
            ["includeReturns"] = IncludeReturns,
            ["includePayments"] = IncludePayments,
            ["excludeCancelled"] = ExcludeCancelled
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

        if (CustomerIds.Any())
            summary.Add($"客戶：{CustomerIds.Count} 個");

        var types = new List<string>();
        if (IncludeDeliveries) types.Add("出貨");
        if (IncludeReturns) types.Add("退貨");
        if (IncludePayments) types.Add("收款");
        summary.Add($"類型：{string.Join("、", types)}");

        if (ExcludeCancelled)
            summary.Add("排除已取消");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
