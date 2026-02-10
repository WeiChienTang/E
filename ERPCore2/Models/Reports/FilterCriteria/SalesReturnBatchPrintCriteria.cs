using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 銷貨退回單批次列印篩選條件
/// 包裝 BatchPrintCriteria 並實作 IReportFilterCriteria 介面
/// </summary>
public class SalesReturnBatchPrintCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 客戶 ID 清單（空表示所有客戶）
    /// </summary>
    public List<int> CustomerIds { get; set; } = new();
    
    /// <summary>
    /// 起始日期（退回日期）
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 結束日期（退回日期）
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 單據狀態清單（空表示所有狀態）
    /// </summary>
    public List<string> Statuses { get; set; } = new();
    
    /// <summary>
    /// 單據編號關鍵字（模糊搜尋）
    /// </summary>
    public string? DocumentNumberKeyword { get; set; }
    
    /// <summary>
    /// 是否包含已取消的單據
    /// </summary>
    public bool IncludeCancelled { get; set; } = false;
    
    /// <summary>
    /// 排序欄位
    /// </summary>
    public string SortBy { get; set; } = "ReturnDate";
    
    /// <summary>
    /// 排序方向（true = 降序）
    /// </summary>
    public bool SortDescending { get; set; } = true;

    public bool Validate(out string? errorMessage)
    {
        // 日期範圍驗證
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不能大於結束日期";
            return false;
        }
        
        // 日期範圍不能過大（超過1年）
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
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["statuses"] = Statuses.Any() ? Statuses : null,
            ["documentNumberKeyword"] = string.IsNullOrWhiteSpace(DocumentNumberKeyword) ? null : DocumentNumberKeyword,
            ["includeCancelled"] = IncludeCancelled,
            ["sortBy"] = SortBy,
            ["sortDescending"] = SortDescending
        };
    }
    
    /// <summary>
    /// 轉換為 BatchPrintCriteria（用於呼叫現有的報表服務）
    /// </summary>
    public BatchPrintCriteria ToBatchPrintCriteria()
    {
        return new BatchPrintCriteria
        {
            StartDate = StartDate,
            EndDate = EndDate,
            RelatedEntityIds = CustomerIds,
            Statuses = Statuses,
            DocumentNumberKeyword = DocumentNumberKeyword,
            IncludeCancelled = IncludeCancelled,
            MaxResults = null,
            SortBy = SortBy,
            SortDirection = SortDescending ? SortDirection.Descending : SortDirection.Ascending,
            ReportType = "SalesReturn"
        };
    }
    
    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (StartDate.HasValue || EndDate.HasValue)
        {
            var dateRange = $"{StartDate?.ToString("yyyy/MM/dd") ?? "不限"} ~ {EndDate?.ToString("yyyy/MM/dd") ?? "不限"}";
            summary.Add($"日期：{dateRange}");
        }

        if (CustomerIds.Any())
        {
            summary.Add($"客戶：{CustomerIds.Count} 家");
        }

        if (Statuses.Any())
        {
            summary.Add($"狀態：{string.Join(", ", Statuses)}");
        }

        if (!string.IsNullOrEmpty(DocumentNumberKeyword))
        {
            summary.Add($"單號含：{DocumentNumberKeyword}");
        }

        return string.Join(" | ", summary);
    }
}
