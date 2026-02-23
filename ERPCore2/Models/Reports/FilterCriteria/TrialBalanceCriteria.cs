using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterAttributes;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 試算表篩選條件
/// 顯示指定期間內各科目的本期發生額（借/貸）及期末累計餘額（借/貸）
/// </summary>
public class TrialBalanceCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 日期範圍（本期發生額的起訖日）
    /// 累計餘額固定從最早傳票到 EndDate
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "傳票日期", Order = 1)]
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 科目大類篩選（空表示全部大類）
    /// </summary>
    [FilterEnum(typeof(AccountType),
        Group = FilterGroup.Basic,
        Label = "科目大類",
        Order = 1)]
    public List<AccountType> AccountTypes { get; set; } = new();

    /// <summary>
    /// 是否顯示零餘額科目（預設 false：隱藏零餘額）
    /// </summary>
    [FilterToggle(Group = FilterGroup.Basic, Label = "顯示選項", CheckboxLabel = "顯示零餘額科目", DefaultValue = false, Order = 2)]
    public bool ShowZeroBalance { get; set; } = false;

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
            ["endDate"] = EndDate,
            ["accountTypes"] = AccountTypes.Any() ? AccountTypes : null,
            ["showZeroBalance"] = ShowZeroBalance
        };
    }

    public string GetSummary()
    {
        var parts = new List<string>();

        if (StartDate.HasValue && EndDate.HasValue)
            parts.Add($"{StartDate:yyyy/MM/dd} ~ {EndDate:yyyy/MM/dd}");
        else if (StartDate.HasValue)
            parts.Add($"{StartDate:yyyy/MM/dd} 起");
        else if (EndDate.HasValue)
            parts.Add($"截至 {EndDate:yyyy/MM/dd}");

        if (AccountTypes.Any())
        {
            var typeNames = AccountTypes.Select(t => t switch
            {
                AccountType.Asset => "資產",
                AccountType.Liability => "負債",
                AccountType.Equity => "權益",
                AccountType.Revenue => "營業收入",
                AccountType.Cost => "營業成本",
                AccountType.Expense => "營業費用",
                AccountType.NonOperatingIncomeAndExpense => "營業外",
                AccountType.ComprehensiveIncome => "綜合損益",
                _ => t.ToString()
            });
            parts.Add($"大類：{string.Join("、", typeNames)}");
        }

        if (ShowZeroBalance)
            parts.Add("含零餘額");

        return parts.Any() ? string.Join(" | ", parts) : "全部";
    }
}
