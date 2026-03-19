using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Vehicles;

public interface IVehicleChartService
{
    /// <summary>依保養類型費用分布</summary>
    Task<List<ChartDataItem>> GetMaintenanceByTypeAsync();

    /// <summary>每月保養費用趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlyCostTrendAsync(int months = 12);

    /// <summary>依車輛保養費用排行 Top N</summary>
    Task<List<ChartDataItem>> GetCostByVehicleAsync(int top = 10);

    /// <summary>依車輛歸屬類型分布（公司/客戶）</summary>
    Task<List<ChartDataItem>> GetVehiclesByOwnershipTypeAsync();

    /// <summary>保險到期分布（依距到期日分段）</summary>
    Task<List<ChartDataItem>> GetInsuranceExpiryDistributionAsync();

    /// <summary>取得車輛管理統計摘要</summary>
    Task<VehicleChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依保養類型 Drill-down：顯示該類型的保養記錄</summary>
    Task<List<ChartDetailItem>> GetMaintenanceDetailsByTypeAsync(string typeLabel);

    /// <summary>依車輛保養費用 Drill-down：顯示該車輛的保養記錄</summary>
    Task<List<ChartDetailItem>> GetMaintenanceDetailsByVehicleAsync(string vehicleLabel);
}
