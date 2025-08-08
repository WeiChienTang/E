using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 刪除記錄服務介面
    /// </summary>
    public interface IDeletedRecordService : IGenericManagementService<DeletedRecord>
    {
        /// <summary>
        /// 記錄刪除操作
        /// </summary>
        /// <param name="tableName">被刪除的資料表名稱</param>
        /// <param name="recordId">被刪除的記錄ID</param>
        /// <param name="recordDisplayName">記錄顯示名稱</param>
        /// <param name="deletedBy">執行刪除的用戶</param>
        /// <param name="deleteReason">刪除原因</param>
        /// <returns>刪除記錄</returns>
        Task<DeletedRecord> LogDeleteAsync(string tableName, int recordId, string? recordDisplayName = null, string? deletedBy = null, string? deleteReason = null);

        /// <summary>
        /// 根據資料表名稱和記錄ID取得刪除記錄
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="recordId">記錄ID</param>
        /// <returns>刪除記錄</returns>
        Task<DeletedRecord?> GetDeletedRecordAsync(string tableName, int recordId);

        /// <summary>
        /// 根據資料表名稱取得所有刪除記錄
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <returns>刪除記錄列表</returns>
        Task<List<DeletedRecord>> GetDeletedRecordsByTableAsync(string tableName);

        /// <summary>
        /// 根據刪除用戶取得刪除記錄
        /// </summary>
        /// <param name="deletedBy">刪除用戶</param>
        /// <returns>刪除記錄列表</returns>
        Task<List<DeletedRecord>> GetDeletedRecordsByUserAsync(string deletedBy);
    }
}
