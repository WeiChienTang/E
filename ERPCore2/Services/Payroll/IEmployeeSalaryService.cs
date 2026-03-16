using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IEmployeeSalaryService : IGenericManagementService<EmployeeSalary>
    {
        /// <summary>取得指定員工的所有薪資歷史記錄（依生效日期降冪）</summary>
        Task<List<EmployeeSalary>> GetByEmployeeIdAsync(int employeeId);

        /// <summary>取得指定員工目前有效的薪資設定（ExpiryDate = null）</summary>
        Task<EmployeeSalary?> GetCurrentSalaryAsync(int employeeId);

        /// <summary>新增薪資設定時，自動將前一筆有效記錄的 ExpiryDate 設為前一天</summary>
        Task<ServiceResult> AddWithExpiryAsync(EmployeeSalary newSalary);

        /// <summary>檢查薪資設定 Code 是否已存在（EditDataLoaderHelper 約定方法）</summary>
        Task<bool> IsEmployeeSalaryCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>更新薪資設定，同時取代所有固定津貼項目</summary>
        Task<ServiceResult> UpdateWithAllowanceItemsAsync(EmployeeSalary entity, List<EmployeeSalaryItem> allowanceItems);

        #region 伺服器端分頁

        /// <summary>
        /// 取得分頁資料（支援動態篩選）
        /// </summary>
        Task<(List<EmployeeSalary> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EmployeeSalary>, IQueryable<EmployeeSalary>>? filterFunc,
            int pageNumber,
            int pageSize);

        #endregion
    }
}
