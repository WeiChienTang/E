using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Suppliers;

public interface ISupplierChartService
{
    /// <summary>依廠商狀態統計廠商數量（正常往來/停用/暫停）</summary>
    Task<List<ChartDataItem>> GetSuppliersByStatusAsync();

    /// <summary>依廠商類型統計廠商數量（製造商/貿易商/代理商/服務商）</summary>
    Task<List<ChartDataItem>> GetSuppliersByTypeAsync();

    /// <summary>依付款方式統計廠商數量</summary>
    Task<List<ChartDataItem>> GetSuppliersByPaymentMethodAsync();

    /// <summary>取得近 N 個月每月新增廠商趨勢</summary>
    Task<List<ChartDataItem>> GetSuppliersByMonthAsync(int months = 12);

    /// <summary>廠商進貨金額排行 Top N（含稅，使用進貨單）</summary>
    Task<List<ChartDataItem>> GetTopSuppliersByPurchaseAmountAsync(int top = 10);

    /// <summary>每月進貨金額趨勢（含稅，近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlyPurchaseTrendAsync(int months = 12);

    /// <summary>依目前應付餘額分段統計廠商數量</summary>
    Task<List<ChartDataItem>> GetSuppliersByCurrentPayableRangeAsync();

    /// <summary>廠商退回金額排行 Top N（含稅，採購退出）</summary>
    Task<List<ChartDataItem>> GetTopSuppliersByReturnAmountAsync(int top = 10);

    /// <summary>取得廠商基本統計摘要</summary>
    Task<SupplierChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====
    Task<List<ChartDetailItem>> GetSupplierDetailsByStatusAsync(string label);
    Task<List<ChartDetailItem>> GetSupplierDetailsByTypeAsync(string label);
    Task<List<ChartDetailItem>> GetSupplierDetailsByPaymentMethodAsync(string label);
    Task<List<ChartDetailItem>> GetTopSupplierPurchaseReceivingDetailsAsync(string supplierLabel);
    Task<List<ChartDetailItem>> GetSuppliersByCurrentPayableRangeDetailsAsync(string label);
    Task<List<ChartDetailItem>> GetTopSupplierReturnDetailsAsync(string supplierLabel);
}
