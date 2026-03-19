using ERPCore2.Models.Charts;

namespace ERPCore2.Services.ScaleManagement;

public interface IScaleChartService
{
    /// <summary>依品項過磅淨重排行 Top N</summary>
    Task<List<ChartDataItem>> GetNetWeightByItemAsync(int top = 10);

    /// <summary>每月過磅淨重趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlyWeightTrendAsync(int months = 12);

    /// <summary>依客戶淨收益排行 Top N</summary>
    Task<List<ChartDataItem>> GetNetAmountByCustomerAsync(int top = 10);

    /// <summary>每月淨收益趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlyRevenueTrendAsync(int months = 12);

    /// <summary>每月過磅筆數趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlyRecordCountAsync(int months = 12);

    /// <summary>取得磅秤管理統計摘要</summary>
    Task<ScaleChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依品項 Drill-down：顯示該品項最近過磅記錄</summary>
    Task<List<ChartDetailItem>> GetRecordDetailsByItemAsync(string itemLabel);

    /// <summary>依客戶 Drill-down：顯示該客戶最近過磅記錄</summary>
    Task<List<ChartDetailItem>> GetRecordDetailsByCustomerAsync(string customerLabel);
}
