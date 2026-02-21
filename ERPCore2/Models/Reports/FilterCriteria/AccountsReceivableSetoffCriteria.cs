using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 應收沖款單篩選條件（客戶沖款單，對應 FN003）
/// </summary>
public class AccountsReceivableSetoffCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 客戶 ID 清單（空表示全部）
    /// </summary>
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "指定客戶",
        Placeholder = "搜尋客戶...",
        EmptyMessage = "未選擇客戶（查詢全部）",
        Order = 1)]
    public List<int> RelatedPartyIds { get; set; } = new();

    /// <summary>
    /// 起始日期
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 單據編號關鍵字（模糊搜尋）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Basic, Label = "單號", Placeholder = "模糊搜尋...", Order = 2)]
    public string? DocumentNumberKeyword { get; set; }

    /// <summary>
    /// 排序欄位
    /// </summary>
    public string SortBy { get; set; } = "SetoffDate";

    /// <summary>
    /// 排序方向（true = 降序）
    /// </summary>
    public bool SortDescending { get; set; } = true;

    public bool Validate(out string? errorMessage)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不能大於結束日期";
            return false;
        }

        if (StartDate.HasValue && EndDate.HasValue)
        {
            var daysDiff = (EndDate.Value - StartDate.Value).TotalDays;
            if (daysDiff > 365)
            {
                errorMessage = "日期範圍不能超過一年";
                return false;
            }
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["setoffType"] = SetoffType.AccountsReceivable,
            ["relatedPartyIds"] = RelatedPartyIds.Any() ? RelatedPartyIds : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["documentNumberKeyword"] = string.IsNullOrWhiteSpace(DocumentNumberKeyword) ? null : DocumentNumberKeyword,
            ["sortBy"] = SortBy,
            ["sortDescending"] = SortDescending
        };
    }

    /// <summary>
    /// 轉換為 BatchPrintCriteria（用於呼叫報表服務）
    /// </summary>
    public BatchPrintCriteria ToBatchPrintCriteria()
    {
        return new BatchPrintCriteria
        {
            StartDate = StartDate,
            EndDate = EndDate,
            RelatedEntityIds = RelatedPartyIds,
            DocumentNumberKeyword = DocumentNumberKeyword,
            SortBy = SortBy,
            SortDirection = SortDescending ? SortDirection.Descending : SortDirection.Ascending,
            ReportType = ReportIds.AccountsReceivableSetoff
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        summary.Add("應收沖款單");

        if (StartDate.HasValue || EndDate.HasValue)
        {
            var dateRange = $"{StartDate?.ToString("yyyy/MM/dd") ?? "不限"} ~ {EndDate?.ToString("yyyy/MM/dd") ?? "不限"}";
            summary.Add($"日期：{dateRange}");
        }

        if (RelatedPartyIds.Any())
        {
            summary.Add($"客戶：{RelatedPartyIds.Count} 家");
        }

        if (!string.IsNullOrEmpty(DocumentNumberKeyword))
        {
            summary.Add($"單號含：{DocumentNumberKeyword}");
        }

        return string.Join(" | ", summary);
    }
}
