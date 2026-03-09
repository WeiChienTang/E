using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IInsuranceRateService
    {
        Task<List<InsuranceRate>> GetAllAsync();
        Task<InsuranceRate?> GetByIdAsync(int id);
        Task<InsuranceRate?> GetEffectiveAsync(DateOnly date);
        Task<ServiceResult<InsuranceRate>> CreateAsync(InsuranceRate entity);
        Task<ServiceResult<InsuranceRate>> UpdateAsync(InsuranceRate entity);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
