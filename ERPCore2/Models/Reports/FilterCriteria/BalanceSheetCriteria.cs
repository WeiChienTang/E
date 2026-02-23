using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 資產負債表篩選條件
/// 資產負債表為累積至截止日（EndDate）的快照，固定只查 Asset/Liability/Equity 科目大類
/// StartDate 在 UI 上顯示但通常留空，EndDate 作為「截止日期」使用
/// </summary>
public class BalanceSheetCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 截止日期（EndDate 為資產負債表快照日）
    /// StartDate 通常不填；若填入，則限制只看 StartDate 後的傳票（非標準用法，保留彈性）
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "截止日期", Order = 1)]
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    /// <summary>
    /// 實際使用的截止日（若 EndDate 未填，預設為今天）
    /// </summary>
    public DateTime AsOfDate => EndDate ?? DateTime.Today;

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
            ["endDate"] = EndDate ?? DateTime.Today
        };
    }

    public string GetSummary()
    {
        var asOf = EndDate?.ToString("yyyy/MM/dd") ?? DateTime.Today.ToString("yyyy/MM/dd");
        return $"截止日：{asOf}";
    }
}
