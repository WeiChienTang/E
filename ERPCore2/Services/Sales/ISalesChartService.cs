using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Sales;

public interface ISalesChartService
{
    /// <summary>依品項統計銷售金額排行 Top N（含稅）</summary>
    Task<List<ChartDataItem>> GetTopItemsBySalesAmountAsync(int top = 10);

    /// <summary>依業務員統計銷售業績排行 Top N（含稅）</summary>
    Task<List<ChartDataItem>> GetTopEmployeesBySalesAmountAsync(int top = 10);

    /// <summary>取得近 N 個月每月出貨金額趨勢（含稅）</summary>
    Task<List<ChartDataItem>> GetMonthlyDeliveryTrendAsync(int months = 12);

    /// <summary>依退回原因統計銷貨退回次數</summary>
    Task<List<ChartDataItem>> GetReturnsByReasonAsync();

    /// <summary>取得近 N 個月每月銷貨退回金額趨勢（含稅）</summary>
    Task<List<ChartDataItem>> GetMonthlyReturnTrendAsync(int months = 12);

    /// <summary>取得銷貨統計摘要</summary>
    Task<SalesChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====
    Task<List<ChartDetailItem>> GetDeliveryDetailsByItemAsync(string productLabel);
    Task<List<ChartDetailItem>> GetDeliveryDetailsByEmployeeAsync(string employeeLabel);
    Task<List<ChartDetailItem>> GetReturnDetailsByReasonAsync(string reasonLabel);

    // ===== 業績目標圖表 =====
    /// <summary>本月業績達成率（%）按業務員</summary>
    Task<List<ChartDataItem>> GetMonthlyAchievementRateAsync();

    /// <summary>本年度目標金額排行（按業務員）</summary>
    Task<List<ChartDataItem>> GetAnnualTargetByPersonAsync();
}
