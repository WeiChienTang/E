using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IMinimumWageService
    {
        Task<List<MinimumWage>> GetAllAsync();
        Task<MinimumWage?> GetByIdAsync(int id);
        Task<MinimumWage?> GetEffectiveAsync(DateOnly date);
        Task<ServiceResult<MinimumWage>> CreateAsync(MinimumWage entity);
        Task<ServiceResult<MinimumWage>> UpdateAsync(MinimumWage entity);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
