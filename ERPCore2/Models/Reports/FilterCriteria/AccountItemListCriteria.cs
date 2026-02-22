using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterAttributes;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 會計科目表篩選條件
/// 屬性上的 Filter*Attribute 供 DynamicFilterTemplate 自動產生篩選 UI
/// </summary>
public class AccountItemListCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定科目大類（空表示所有大類）
    /// </summary>
    [FilterEnum(typeof(AccountType),
        Group = FilterGroup.Basic,
        Label = "科目大類",
        Order = 1)]
    public List<AccountType> AccountTypes { get; set; } = new();

    /// <summary>
    /// 借貸方向篩選（空表示全部）
    /// </summary>
    [FilterEnum(typeof(AccountDirection),
        Group = FilterGroup.Basic,
        Label = "借貸方向",
        Order = 2)]
    public List<AccountDirection> AccountDirections { get; set; } = new();

    /// <summary>
    /// 層級篩選（空表示所有層級）
    /// </summary>
    [FilterEnum(typeof(AccountLevelFilter),
        Group = FilterGroup.Basic,
        Label = "層級",
        Order = 3)]
    public List<AccountLevelFilter> AccountLevels { get; set; } = new();

    /// <summary>
    /// 科目代碼搜尋
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "科目代碼", Placeholder = "搜尋科目代碼...", Order = 1)]
    public string? CodeKeyword { get; set; }

    /// <summary>
    /// 科目名稱搜尋
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "科目名稱", Placeholder = "搜尋科目名稱...", Order = 2)]
    public string? NameKeyword { get; set; }

    /// <summary>
    /// 是否僅顯示明細科目（預設 false）
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "科目類型", CheckboxLabel = "僅顯示明細科目", DefaultValue = false, Order = 3)]
    public bool DetailAccountOnly { get; set; } = false;

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
            ["accountTypes"] = AccountTypes.Any() ? AccountTypes : null,
            ["accountDirections"] = AccountDirections.Any() ? AccountDirections : null,
            ["accountLevels"] = AccountLevels.Any() ? AccountLevels : null,
            ["codeKeyword"] = string.IsNullOrWhiteSpace(CodeKeyword) ? null : CodeKeyword,
            ["nameKeyword"] = string.IsNullOrWhiteSpace(NameKeyword) ? null : NameKeyword,
            ["detailAccountOnly"] = DetailAccountOnly
        };
    }

    public string GetSummary()
    {
        var summary = new List<string>();

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
            summary.Add($"大類：{string.Join("、", typeNames)}");
        }

        if (AccountDirections.Any())
        {
            var dirNames = AccountDirections.Select(d => d switch
            {
                AccountDirection.Debit => "借方",
                AccountDirection.Credit => "貸方",
                _ => d.ToString()
            });
            summary.Add($"方向：{string.Join("、", dirNames)}");
        }

        if (AccountLevels.Any())
        {
            var levelNames = AccountLevels.OrderBy(l => (int)l).Select(l => $"第{(int)l}層");
            summary.Add($"層級：{string.Join("、", levelNames)}");
        }

        if (!string.IsNullOrEmpty(CodeKeyword))
            summary.Add($"代碼：{CodeKeyword}");

        if (!string.IsNullOrEmpty(NameKeyword))
            summary.Add($"名稱：{NameKeyword}");

        if (DetailAccountOnly)
            summary.Add("僅明細科目");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
