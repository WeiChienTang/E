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

    /// <summary>依啟用/停用狀態統計客戶數量</summary>
    Task<List<ChartDataItem>> GetCustomersByActiveStatusAsync();

    /// <summary>依信用額度分段統計客戶數量</summary>
    Task<List<ChartDataItem>> GetCustomersByCreditLimitRangeAsync();

    /// <summary>取得客戶基本統計摘要</summary>
    Task<CustomerChartSummary> GetSummaryAsync();
}
