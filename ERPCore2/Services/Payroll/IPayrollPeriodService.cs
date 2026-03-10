using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services.Payroll
{
    public interface IPayrollPeriodService : IGenericManagementService<PayrollPeriod>
    {
        /// <summary>開立新的薪資週期（自動帶入民國年月，防重複）</summary>
        Task<ServiceResult<PayrollPeriod>> OpenPeriodAsync(int year, int month, string? createdBy = null);

        /// <summary>關帳：將 PeriodStatus 設為 Closed，記錄關帳時間與人員</summary>
        Task<ServiceResult> ClosePeriodAsync(int periodId, string? closedBy = null);

        /// <summary>取得目前開立中（Draft 或 Processing）的薪資週期</summary>
        Task<PayrollPeriod?> GetCurrentOpenPeriodAsync();

        /// <summary>檢查指定年月的週期是否已存在</summary>
        Task<bool> PeriodExistsAsync(int year, int month);

        /// <summary>取得週期的薪資記錄數量（用於判斷能否刪除）</summary>
        Task<int> GetRecordCountAsync(int periodId);

        /// <summary>伺服器端分頁查詢</summary>
        Task<(List<PayrollPeriod> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<PayrollPeriod>, IQueryable<PayrollPeriod>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
