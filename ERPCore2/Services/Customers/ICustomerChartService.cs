using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Customers;

public interface ICustomerChartService
{
    /// <summary>依業務負責人統計客戶數量</summary>
    Task<List<ChartDataItem>> GetCustomersByAccountManagerAsync();

    /// <summary>依付款方式統計客戶數量</summary>
    Task<List<ChartDataItem>> GetCustomersByPaymentMethodAsync();

    /// <summary>取得近 N 個月每月新增客戶趨勢</summary>
    Task<List<ChartDataItem>> GetCustomersByMonthAsync(int months = 12);

    /// <summary>依客戶狀態統計客戶數量（正常往來/停用/黑名單）</summary>
    Task<List<ChartDataItem>> GetCustomersByCustomerStatusAsync();

    /// <summary>依信用額度分段統計客戶數量</summary>
    Task<List<ChartDataItem>> GetCustomersByCreditLimitRangeAsync();

    /// <summary>取得客戶基本統計摘要</summary>
    Task<CustomerChartSummary> GetSummaryAsync();

    // ===== 金錢數據圖表 =====

    /// <summary>客戶銷售金額排行 Top N（含稅）</summary>
    Task<List<ChartDataItem>> GetTopCustomersBySalesAmountAsync(int top = 10);

    /// <summary>每月銷售收入趨勢（含稅，近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlySalesTrendAsync(int months = 12);

    /// <summary>依目前應收餘額分段統計客戶數量</summary>
    Task<List<ChartDataItem>> GetCustomersByCurrentBalanceRangeAsync();

    /// <summary>客戶退貨金額排行 Top N（含稅）</summary>
    Task<List<ChartDataItem>> GetTopCustomersByReturnAmountAsync(int top = 10);

    // ===== Drill-down 明細查詢 =====
    Task<List<ChartDetailItem>> GetCustomerDetailsByPaymentMethodAsync(string label);
    Task<List<ChartDetailItem>> GetCustomerDetailsByAccountManagerAsync(string label);
    Task<List<ChartDetailItem>> GetCustomerDetailsByStatusAsync(string label);
    Task<List<ChartDetailItem>> GetCustomerDetailsByCreditLimitRangeAsync(string label);
    Task<List<ChartDetailItem>> GetTopCustomerSalesOrderDetailsAsync(string customerLabel);
    Task<List<ChartDetailItem>> GetCustomersByCurrentBalanceRangeDetailsAsync(string label);
    Task<List<ChartDetailItem>> GetTopCustomerReturnDetailsAsync(string customerLabel);
}
