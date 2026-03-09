using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface ILaborInsuranceGradeService
    {
        Task<List<LaborInsuranceGrade>> GetAllAsync();
        Task<List<LaborInsuranceGrade>> GetByEffectiveDateAsync(DateOnly effectiveDate);
        Task<List<DateOnly>> GetAvailableYearsAsync();
        Task<LaborInsuranceGrade?> GetByIdAsync(int id);
        Task<LaborInsuranceGrade?> GetEffectiveGradeAsync(decimal salary, DateOnly date);
        Task<ServiceResult<LaborInsuranceGrade>> CreateAsync(LaborInsuranceGrade entity);
        Task<ServiceResult<LaborInsuranceGrade>> UpdateAsync(LaborInsuranceGrade entity);
        Task<ServiceResult> DeleteAsync(int id);
        Task<ServiceResult> CopyYearAsync(DateOnly sourceDate, DateOnly targetDate);
    }
}
