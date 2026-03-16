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
        /// <summary>從勞動部開放資料 API 匯入尚未存在的基本工資紀錄，回傳新增筆數</summary>
        Task<ServiceResult<int>> SyncFromGovernmentAsync();
    }
}
