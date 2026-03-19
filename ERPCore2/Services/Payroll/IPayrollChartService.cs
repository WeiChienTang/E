using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Payroll;

public interface IPayrollChartService
{
    /// <summary>每月薪資總支出趨勢（近 N 個月，以實發薪資計）</summary>
    Task<List<ChartDataItem>> GetMonthlyPayrollTrendAsync(int months = 12);

    /// <summary>依部門應發薪資分布</summary>
    Task<List<ChartDataItem>> GetGrossIncomeByDepartmentAsync();

    /// <summary>員工實發薪資排行 Top N</summary>
    Task<List<ChartDataItem>> GetTopEarnersAsync(int top = 10);

    /// <summary>薪資單狀態分布（試算中/已確認）</summary>
    Task<List<ChartDataItem>> GetRecordStatusDistributionAsync();

    /// <summary>員工加班時數排行 Top N</summary>
    Task<List<ChartDataItem>> GetTopOvertimeByEmployeeAsync(int top = 10);

    /// <summary>取得薪資管理統計摘要</summary>
    Task<PayrollChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依部門 Drill-down：顯示該部門員工薪資清單</summary>
    Task<List<ChartDetailItem>> GetEmployeeDetailsByDepartmentAsync(string departmentLabel);

    /// <summary>薪資排行 Drill-down：顯示該員工的薪資期間記錄</summary>
    Task<List<ChartDetailItem>> GetPayrollDetailsByEmployeeAsync(string employeeLabel);

    /// <summary>依薪資單狀態 Drill-down</summary>
    Task<List<ChartDetailItem>> GetRecordDetailsByStatusAsync(string statusLabel);
}
