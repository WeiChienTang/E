using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterAttributes;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 明細分類帳篩選條件
/// 可依科目代碼/名稱關鍵字篩選特定科目，顯示帳戶卡片（含期初餘額、逐筆明細、期末餘額）
/// 適合查看應收帳款按客戶、應付帳款按廠商等子科目明細
/// </summary>
public class SubsidiaryLedgerCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 傳票日期範圍（本期明細起訖）
    /// 期初餘額自動計算為 StartDate 之前的累計餘額
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "傳票日期", Order = 1)]
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 科目代碼/名稱關鍵字（空白表示顯示全部）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Basic, Label = "科目代碼/名稱", Placeholder = "輸入科目代碼或名稱關鍵字", Order = 1)]
    public string? AccountKeyword { get; set; }

    /// <summary>
    /// 科目大類篩選（空表示全部大類）
    /// </summary>
    [FilterEnum(typeof(AccountType),
        Group = FilterGroup.Basic,
        Label = "科目大類",
        Order = 2)]
    public List<AccountType> AccountTypes { get; set; } = new();

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
            ["accountKeyword"] = AccountKeyword,
            ["accountTypes"] = AccountTypes.Any() ? AccountTypes : null
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

        if (!string.IsNullOrWhiteSpace(AccountKeyword))
            parts.Add($"科目：{AccountKeyword}");

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

        return parts.Any() ? string.Join(" | ", parts) : "全部";
    }
}
