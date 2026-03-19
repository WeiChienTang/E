using ERPCore2.Models.Charts;

namespace ERPCore2.Services.ProductionManagement;

public interface IProductionChartService
{
    /// <summary>依生產狀態分布（待生產/生產中/已完成/已結案）</summary>
    Task<List<ChartDataItem>> GetOrdersByStatusAsync();

    /// <summary>品項排程數量排行 Top N</summary>
    Task<List<ChartDataItem>> GetTopItemsByScheduledQuantityAsync(int top = 10);

    /// <summary>每月製令開單趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlyOrderTrendAsync(int months = 12);

    /// <summary>依負責人員分布（製令數量）</summary>
    Task<List<ChartDataItem>> GetOrdersByEmployeeAsync();

    /// <summary>品項完成率排行 Top N（CompletedQty / ScheduledQty %）</summary>
    Task<List<ChartDataItem>> GetCompletionRateByItemAsync(int top = 10);

    /// <summary>組件用料排行 Top N（依實際消耗量）</summary>
    Task<List<ChartDataItem>> GetTopComponentsByUsageAsync(int top = 10);

    /// <summary>取得生產管理統計摘要</summary>
    Task<ProductionChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依生產狀態 Drill-down：顯示該狀態下的製令清單</summary>
    Task<List<ChartDetailItem>> GetOrderDetailsByStatusAsync(string statusLabel);

    /// <summary>品項排程數量 Drill-down：顯示該品項最近的製令清單</summary>
    Task<List<ChartDetailItem>> GetOrderDetailsByItemAsync(string itemLabel);

    /// <summary>依負責人員 Drill-down：顯示該人員負責的製令清單</summary>
    Task<List<ChartDetailItem>> GetOrderDetailsByEmployeeAsync(string employeeLabel);

    /// <summary>組件用料 Drill-down：顯示該組件的領用製令清單</summary>
    Task<List<ChartDetailItem>> GetComponentDetailsByItemAsync(string componentLabel);
}
