using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Employees;

public interface IEmployeeChartService
{
    /// <summary>依部門統計員工人數</summary>
    Task<List<ChartDataItem>> GetEmployeesByDepartmentAsync();

    /// <summary>依員工類別統計員工人數（正職/兼職/約聘/實習/派遣）</summary>
    Task<List<ChartDataItem>> GetEmployeesByTypeAsync();

    /// <summary>依在職狀態統計員工人數</summary>
    Task<List<ChartDataItem>> GetEmployeesByStatusAsync();

    /// <summary>依性別統計員工人數</summary>
    Task<List<ChartDataItem>> GetEmployeesByGenderAsync();

    /// <summary>取得近 N 個月每月入職趨勢</summary>
    Task<List<ChartDataItem>> GetEmployeeMonthlyHireTrendAsync(int months = 12);

    /// <summary>依年資分段統計員工人數</summary>
    Task<List<ChartDataItem>> GetEmployeesBySeniorityAsync();

    /// <summary>訓練時數排行 Top N</summary>
    Task<List<ChartDataItem>> GetTopEmployeesByTrainingHoursAsync(int top = 10);

    /// <summary>取得員工基本統計摘要</summary>
    Task<EmployeeChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====
    Task<List<ChartDetailItem>> GetEmployeeDetailsByDepartmentAsync(string label);
    Task<List<ChartDetailItem>> GetEmployeeDetailsByTypeAsync(string label);
    Task<List<ChartDetailItem>> GetEmployeeDetailsByStatusAsync(string label);
    Task<List<ChartDetailItem>> GetEmployeeDetailsByGenderAsync(string label);
    Task<List<ChartDetailItem>> GetEmployeeDetailsBySeniorityAsync(string label);
    Task<List<ChartDetailItem>> GetTopEmployeeTrainingDetailsAsync(string employeeLabel);
}
