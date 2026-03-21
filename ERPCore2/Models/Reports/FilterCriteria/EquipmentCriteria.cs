using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 設備清單篩選條件
/// </summary>
public class EquipmentCriteria : IReportFilterCriteria
{
    [FilterFK(typeof(IEquipmentCategoryService),
        Group = FilterGroup.Basic,
        Label = "設備類別",
        EmptyMessage = "全部類別",
        Order = 0)]
    public int? EquipmentCategoryId { get; set; }

    [FilterKeyword(Group = FilterGroup.Basic, Label = "設備名稱/編號", Order = 1)]
    public string? Keyword { get; set; }

    [FilterDateRange(Group = FilterGroup.Date, Label = "購入日期", Order = 2)]
    public DateTime? PurchaseDateStart { get; set; }

    public DateTime? PurchaseDateEnd { get; set; }

    [FilterDateRange(Group = FilterGroup.Date, Label = "下次保養日期", Order = 3)]
    public DateTime? NextMaintenanceDateStart { get; set; }

    public DateTime? NextMaintenanceDateEnd { get; set; }

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
            ["EquipmentCategoryId"] = EquipmentCategoryId,
            ["Keyword"] = Keyword,
            ["PurchaseDateStart"] = PurchaseDateStart,
            ["PurchaseDateEnd"] = PurchaseDateEnd,
            ["NextMaintenanceDateStart"] = NextMaintenanceDateStart,
            ["NextMaintenanceDateEnd"] = NextMaintenanceDateEnd
        };
    }

    public string GetSummary()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Keyword))
            parts.Add($"關鍵字：{Keyword}");
        if (PurchaseDateStart.HasValue && PurchaseDateEnd.HasValue)
            parts.Add($"購入日期：{PurchaseDateStart:yyyy/MM/dd} ~ {PurchaseDateEnd:yyyy/MM/dd}");
        return parts.Any() ? string.Join("，", parts) : "全部設備";
    }
}
