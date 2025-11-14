using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 通用管理服務介面 - 提供基本的 CRUD 操作和常用查詢
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface IGenericManagementService<T> where T : class
    {
        #region 基本 CRUD 操作
        
        /// <summary>
        /// 取得所有資料（不含已刪除）
        /// </summary>
        Task<List<T>> GetAllAsync();
        
        /// <summary>
        /// 取得所有啟用的資料
        /// </summary>
        Task<List<T>> GetActiveAsync();
        
        /// <summary>
        /// 根據 ID 取得單一資料
        /// </summary>
        Task<T?> GetByIdAsync(int id);
        
        /// <summary>
        /// 建立新資料
        /// </summary>
        Task<ServiceResult<T>> CreateAsync(T entity);
        
        /// <summary>
        /// 更新資料
        /// </summary>
        Task<ServiceResult<T>> UpdateAsync(T entity);
        
        /// <summary>
        /// 刪除資料（硬刪除）
        /// 不要再調用此方法，已經不再使用軟除 2025/09/24
        /// </summary>
        Task<ServiceResult> DeleteAsync(int id);
        
        /// <summary>
        /// 永久刪除資料（硬刪除）
        /// </summary>
        Task<ServiceResult> PermanentDeleteAsync(int id);
        
        #endregion

        #region 批次操作
        
        /// <summary>
        /// 批次建立
        /// </summary>
        Task<ServiceResult<List<T>>> CreateBatchAsync(List<T> entities);
        
        /// <summary>
        /// 批次更新
        /// </summary>
        Task<ServiceResult<List<T>>> UpdateBatchAsync(List<T> entities);
        
        /// <summary>
        /// 批次刪除
        /// </summary>
        Task<ServiceResult> DeleteBatchAsync(List<int> ids);
        
        #endregion

        #region 查詢操作
        
        /// <summary>
        /// 分頁查詢
        /// </summary>
        Task<(List<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        
        /// <summary>
        /// 根據條件查詢（需要子類別實作具體邏輯）
        /// </summary>
        Task<List<T>> SearchAsync(string searchTerm);
        
        /// <summary>
        /// 檢查資料是否存在
        /// </summary>
        Task<bool> ExistsAsync(int id);
        
        /// <summary>
        /// 取得資料總數
        /// </summary>
        Task<int> GetCountAsync();
        
        #endregion

        #region 狀態管理
        
        /// <summary>
        /// 設定特定狀態
        /// </summary>
        Task<ServiceResult> SetStatusAsync(int id, EntityStatus status);
        
        /// <summary>
        /// 切換狀態（Active <-> Inactive）
        /// </summary>
        Task<ServiceResult> ToggleStatusAsync(int id);
        
        /// <summary>
        /// 批次設定狀態
        /// </summary>
        Task<ServiceResult> SetStatusBatchAsync(List<int> ids, EntityStatus status);
        
        #endregion

        #region 驗證
        
        /// <summary>
        /// 驗證實體資料
        /// </summary>
        Task<ServiceResult> ValidateAsync(T entity);
        
        /// <summary>
        /// 檢查名稱是否存在（適用於有名稱欄位的實體）
        /// </summary>
        Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
        
        #endregion

        #region 記錄導航
        
        /// <summary>
        /// 取得上一筆記錄的 ID（按 ID 排序）
        /// </summary>
        /// <param name="currentId">當前記錄的 ID</param>
        /// <returns>上一筆記錄的 ID，如果沒有則返回 null</returns>
        Task<int?> GetPreviousIdAsync(int currentId);
        
        /// <summary>
        /// 取得下一筆記錄的 ID（按 ID 排序）
        /// </summary>
        /// <param name="currentId">當前記錄的 ID</param>
        /// <returns>下一筆記錄的 ID，如果沒有則返回 null</returns>
        Task<int?> GetNextIdAsync(int currentId);
        
        #endregion
    }
}
