using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Purchase;

public interface IPurchaseChartService
{
    /// <summary>依品項統計進貨金額排行 Top N（含稅）</summary>
    Task<List<ChartDataItem>> GetTopItemsByReceivingAmountAsync(int top = 10);

    /// <summary>取得近 N 個月每月進貨金額趨勢（含稅）</summary>
    Task<List<ChartDataItem>> GetMonthlyReceivingTrendAsync(int months = 12);

    /// <summary>採購訂單核准狀態分布（待審核 / 已核准 / 已拒絕）</summary>
    Task<List<ChartDataItem>> GetOrderApprovalStatusAsync();

    /// <summary>依退回原因統計採購退回次數</summary>
    Task<List<ChartDataItem>> GetReturnsByReasonAsync();

    /// <summary>取得近 N 個月每月採購退回金額趨勢（含稅）</summary>
    Task<List<ChartDataItem>> GetMonthlyReturnTrendAsync(int months = 12);

    /// <summary>取得採購統計摘要</summary>
    Task<PurchaseChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====
    Task<List<ChartDetailItem>> GetReceivingDetailsByItemAsync(string productLabel);
    Task<List<ChartDetailItem>> GetOrderDetailsByApprovalStatusAsync(string label);
    Task<List<ChartDetailItem>> GetReturnDetailsByReasonAsync(string reasonLabel);
}
