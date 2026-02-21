using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 車輛管理表篩選條件
/// </summary>
public class VehicleListCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定車型 ID 清單（空表示所有車型）
    /// </summary>
    [FilterFK(typeof(IVehicleTypeService),
        Group = FilterGroup.Basic,
        Label = "指定車型",
        Placeholder = "搜尋車型...",
        EmptyMessage = "未選擇車型（查詢全部車型）",
        Order = 1)]
    public List<int> VehicleTypeIds { get; set; } = new();

    /// <summary>
    /// 負責人/駕駛人 ID 清單（空表示所有）
    /// </summary>
    [FilterFK(typeof(IEmployeeService),
        Group = FilterGroup.Basic,
        Label = "負責人/駕駛人",
        Placeholder = "搜尋負責人或駕駛人...",
        EmptyMessage = "未選擇負責人（查詢全部）",
        DisplayFormat = FilterDisplayFormat.CodeDashName,
        Order = 2)]
    public List<int> EmployeeIds { get; set; } = new();

    /// <summary>
    /// 歸屬類型篩選（空表示全部）
    /// </summary>
    [FilterEnum(typeof(VehicleOwnershipType),
        Group = FilterGroup.Basic,
        Label = "歸屬類型",
        Order = 3)]
    public List<VehicleOwnershipType> OwnershipTypes { get; set; } = new();

    /// <summary>
    /// 燃料類型篩選（空表示全部）
    /// </summary>
    [FilterEnum(typeof(FuelType),
        Group = FilterGroup.Basic,
        Label = "燃料類型",
        Order = 4)]
    public List<FuelType> FuelTypes { get; set; } = new();

    /// <summary>
    /// 關鍵字搜尋（車牌號碼、車輛名稱、廠牌）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋車牌號碼、車輛名稱、廠牌...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示啟用車輛（預設 true）
    /// </summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示條件", CheckboxLabel = "僅啟用", DefaultValue = true, Order = 2)]
    public bool ActiveOnly { get; set; } = true;

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
            ["vehicleTypeIds"] = VehicleTypeIds.Any() ? VehicleTypeIds : null,
            ["employeeIds"] = EmployeeIds.Any() ? EmployeeIds : null,
            ["ownershipTypes"] = OwnershipTypes.Any() ? OwnershipTypes : null,
            ["fuelTypes"] = FuelTypes.Any() ? FuelTypes : null,
            ["keyword"] = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword,
            ["activeOnly"] = ActiveOnly
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (VehicleTypeIds.Any())
            summary.Add($"車型：{VehicleTypeIds.Count} 個");

        if (EmployeeIds.Any())
            summary.Add($"負責人：{EmployeeIds.Count} 人");

        if (OwnershipTypes.Any())
        {
            var typeNames = OwnershipTypes.Select(t => t switch
            {
                VehicleOwnershipType.Company => "公司",
                VehicleOwnershipType.Customer => "客戶",
                _ => t.ToString()
            });
            summary.Add($"歸屬：{string.Join("、", typeNames)}");
        }

        if (FuelTypes.Any())
            summary.Add($"燃料：{FuelTypes.Count} 種");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
