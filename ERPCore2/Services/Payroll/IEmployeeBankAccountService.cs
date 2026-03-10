using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IEmployeeBankAccountService : IGenericManagementService<EmployeeBankAccount>
    {
        /// <summary>取得指定員工的所有銀行帳戶</summary>
        Task<List<EmployeeBankAccount>> GetByEmployeeIdAsync(int employeeId);

        /// <summary>取得指定員工的主要轉帳帳戶</summary>
        Task<EmployeeBankAccount?> GetPrimaryAccountAsync(int employeeId);

        /// <summary>設定指定帳戶為主要帳戶（同時取消同員工其他帳戶的主要標記）</summary>
        Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int employeeId);

        /// <summary>伺服器端分頁查詢</summary>
        Task<(List<EmployeeBankAccount> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EmployeeBankAccount>, IQueryable<EmployeeBankAccount>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
