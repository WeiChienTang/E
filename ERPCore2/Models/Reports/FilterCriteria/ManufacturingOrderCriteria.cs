using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 製令單篩選條件
/// </summary>
public class ManufacturingOrderCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 計劃開始日期範圍
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "計劃開始日期", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 計劃開始日期結束
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 指定製令單 ID 清單（從編輯畫面或右鍵列印單筆時使用）
    /// </summary>
    public List<int> ManufacturingOrderIds { get; set; } = new();

    /// <summary>
    /// 品項篩選
    /// </summary>
    [FilterFK(typeof(IItemService),
        Group = FilterGroup.Basic,
        Label = "品項",
        Placeholder = "搜尋品項...",
        EmptyMessage = "未選擇品項（查詢全部）",
        Order = 1)]
    public List<int> ItemIds { get; set; } = new();

    /// <summary>
    /// 負責人員篩選
    /// </summary>
    [FilterFK(typeof(IEmployeeService),
        Group = FilterGroup.Basic,
        Label = "負責人員",
        Placeholder = "搜尋員工...",
        EmptyMessage = "未選擇（查詢全部）",
        Order = 2)]
    public List<int> ResponsibleEmployeeIds { get; set; } = new();

    /// <summary>
    /// 生產狀態篩選
    /// </summary>
    [FilterEnum(typeof(ProductionItemStatus), Group = FilterGroup.Quick, Label = "生產狀態", Order = 1)]
    public List<ProductionItemStatus> StatusFilters { get; set; } = new();

    /// <summary>
    /// 是否包含已結案
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示選項", CheckboxLabel = "包含已結案", DefaultValue = false, Order = 2)]
    public bool IncludeClosed { get; set; } = false;

    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (ManufacturingOrderIds.Any())
        {
            errorMessage = null;
            return true;
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不可大於結束日期";
            return false;
        }

        if (!StartDate.HasValue && !EndDate.HasValue && !ItemIds.Any())
        {
            errorMessage = "請指定查詢日期範圍或選擇品項";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["productIds"] = ItemIds.Any() ? ItemIds : null,
            ["responsibleEmployeeIds"] = ResponsibleEmployeeIds.Any() ? ResponsibleEmployeeIds : null,
            ["statusFilters"] = StatusFilters.Any() ? StatusFilters : null,
            ["includeClosed"] = IncludeClosed
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

        if (ItemIds.Any())
            parts.Add($"品項：{ItemIds.Count} 個");

        if (ResponsibleEmployeeIds.Any())
            parts.Add($"負責人員：{ResponsibleEmployeeIds.Count} 位");

        if (StatusFilters.Any())
        {
            var names = StatusFilters.Select(s => s switch
            {
                ProductionItemStatus.Pending => "待生產",
                ProductionItemStatus.WaitingMaterial => "等待領料",
                ProductionItemStatus.InProgress => "生產中",
                ProductionItemStatus.Paused => "已暫停",
                ProductionItemStatus.Completed => "已完成",
                ProductionItemStatus.Aborted => "已終止",
                _ => s.ToString()
            });
            parts.Add($"狀態：{string.Join("、", names)}");
        }

        if (IncludeClosed)
            parts.Add("含已結案");

        return parts.Any() ? string.Join(" | ", parts) : "全部";
    }
}
