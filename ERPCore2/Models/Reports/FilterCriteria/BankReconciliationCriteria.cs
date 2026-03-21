using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 銀行存款餘額調節表篩選條件
/// 依公司與對帳期間查詢已建立的銀行對帳單，逐單產生餘額調節報表
/// </summary>
public class BankReconciliationCriteria : IReportFilterCriteria
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
    /// 對帳期間（查詢 BankStatement.PeriodStart 落於此範圍的對帳單）
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "對帳期間", Order = 1)]
    public DateTime? PeriodStart { get; set; }

    public DateTime? PeriodEnd { get; set; }

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (!PeriodStart.HasValue || !PeriodEnd.HasValue)
        {
            errorMessage = "請選擇對帳期間";
            return false;
        }
        if (PeriodEnd < PeriodStart)
        {
            errorMessage = "結束日期不可早於起始日期";
            return false;
        }
        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["CompanyId"] = CompanyId,
            ["PeriodStart"] = PeriodStart,
            ["PeriodEnd"] = PeriodEnd
        };
    }

    public string GetSummary()
    {
        var parts = new List<string>();
        if (PeriodStart.HasValue && PeriodEnd.HasValue)
            parts.Add($"{PeriodStart:yyyy/MM/dd} ~ {PeriodEnd:yyyy/MM/dd}");
        return parts.Any() ? string.Join("，", parts) : "全部期間";
    }
}
