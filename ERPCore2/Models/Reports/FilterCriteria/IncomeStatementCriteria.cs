using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 損益表篩選條件
/// 彙總指定期間的營業收入、成本、費用，計算毛利、營業損益、稅前損益
/// 固定只查 Revenue/Cost/Expense/NonOperating 科目大類
/// </summary>
public class IncomeStatementCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 公司篩選（多公司環境下必填；留空時僅查詢主要公司）
    /// </summary>
    [FilterFK(typeof(ICompanyService),
        Group = FilterGroup.Basic,
        Label = "公司",
        EmptyMessage = "全部公司",
        Order = 0)]
    public int? CompanyId { get; set; }

    /// <summary>
    /// 日期範圍（傳票日期）
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "會計期間", Order = 1)]
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

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
            ["startDate"] = StartDate,
            ["endDate"] = EndDate
        };
    }

    public string GetSummary()
    {
        if (StartDate.HasValue && EndDate.HasValue)
            return $"{StartDate:yyyy/MM/dd} ~ {EndDate:yyyy/MM/dd}";
        if (StartDate.HasValue)
            return $"{StartDate:yyyy/MM/dd} 起";
        if (EndDate.HasValue)
            return $"截至 {EndDate:yyyy/MM/dd}";
        return "全部期間";
    }
}
