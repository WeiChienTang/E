using ERPCore2.Data.Enums;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 通用管理服務介面 - 提供基本的 CRUD 操作
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface IGenericManagementService<T> where T : class
    {
        // 基本 CRUD 操作
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetActiveAsync();
        Task<T?> GetByIdAsync(int id);
        Task<ServiceResult<T>> CreateAsync(T entity);
        Task<ServiceResult<T>> UpdateAsync(T entity);
        Task<ServiceResult> DeleteAsync(int id);

        // 狀態管理
        Task<ServiceResult> ToggleStatusAsync(int id, EntityStatus newStatus);
        Task<ServiceResult> ToggleStatusAsync(int id); // 切換 Active/Inactive
        
        // 驗證
        Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
    }
}
