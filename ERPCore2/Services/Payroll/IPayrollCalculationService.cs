using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    /// <summary>薪資計算引擎服務介面</summary>
    public interface IPayrollCalculationService
    {
        /// <summary>計算指定員工在指定薪資週期的薪資（若已存在則更新，若為 Confirmed 或周期 Closed 則拒絕）</summary>
        Task<ServiceResult<PayrollRecord>> CalculateEmployeeAsync(int employeeId, int periodId, string? calculatedBy = null);

        /// <summary>批次計算指定薪資週期內所有在職員工</summary>
        Task<ServiceResult> CalculateAllAsync(int periodId, string? calculatedBy = null);

        /// <summary>重新計算指定薪資單（重置為 Draft 並重算）</summary>
        Task<ServiceResult<PayrollRecord>> RecalculateAsync(int recordId, string? calculatedBy = null);

        /// <summary>取得指定週期所有薪資單（含員工、明細）</summary>
        Task<List<PayrollRecord>> GetByPeriodAsync(int periodId);

        /// <summary>取得指定員工在指定週期的薪資單</summary>
        Task<PayrollRecord?> GetRecordAsync(int employeeId, int periodId);

        /// <summary>依 Id 取得薪資單（含員工、週期、明細）</summary>
        Task<PayrollRecord?> GetByIdAsync(int recordId);

        /// <summary>取得指定員工的所有薪資單（依週期倒序）</summary>
        Task<List<PayrollRecord>> GetByEmployeeAsync(int employeeId);

        /// <summary>確認薪資單（Draft → Confirmed）</summary>
        Task<ServiceResult> ConfirmRecordAsync(int recordId, string? confirmedBy = null);

        /// <summary>取消確認（Confirmed → Draft，若週期未關帳）</summary>
        Task<ServiceResult> UnconfirmRecordAsync(int recordId, string? operatedBy = null);
    }
}
