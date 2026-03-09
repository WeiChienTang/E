using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IWithholdingTaxTableService
    {
        Task<List<WithholdingTaxTable>> GetAllAsync();
        Task<List<WithholdingTaxTable>> GetByEffectiveDateAsync(DateOnly effectiveDate);
        Task<List<DateOnly>> GetAvailableYearsAsync();
        Task<WithholdingTaxTable?> GetByIdAsync(int id);
        Task<ServiceResult<WithholdingTaxTable>> CreateAsync(WithholdingTaxTable entity);
        Task<ServiceResult<WithholdingTaxTable>> UpdateAsync(WithholdingTaxTable entity);
        Task<ServiceResult> DeleteAsync(int id);
        Task<ServiceResult> CopyYearAsync(DateOnly sourceDate, DateOnly targetDate);
    }
}
