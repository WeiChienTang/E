using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IHealthInsuranceGradeService
    {
        Task<List<HealthInsuranceGrade>> GetAllAsync();
        Task<List<HealthInsuranceGrade>> GetByEffectiveDateAsync(DateOnly effectiveDate);
        Task<List<DateOnly>> GetAvailableYearsAsync();
        Task<HealthInsuranceGrade?> GetByIdAsync(int id);
        Task<HealthInsuranceGrade?> GetEffectiveGradeAsync(decimal salary, DateOnly date);
        Task<ServiceResult<HealthInsuranceGrade>> CreateAsync(HealthInsuranceGrade entity);
        Task<ServiceResult<HealthInsuranceGrade>> UpdateAsync(HealthInsuranceGrade entity);
        Task<ServiceResult> DeleteAsync(int id);
        Task<ServiceResult> CopyYearAsync(DateOnly sourceDate, DateOnly targetDate);
    }
}
