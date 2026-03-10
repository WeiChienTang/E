using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IPayrollItemService : IGenericManagementService<PayrollItem>
    {
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>伺服器端分頁查詢</summary>
        Task<(List<PayrollItem> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<PayrollItem>, IQueryable<PayrollItem>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
