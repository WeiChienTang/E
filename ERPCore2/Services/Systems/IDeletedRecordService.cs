using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 刪除記錄服務介面
    /// </summary>
    public interface IDeletedRecordService : IGenericManagementService<DeletedRecord>
    {
        /// <summary>
        /// 記錄實體刪除
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="recordId">記錄ID</param>
        /// <param name="recordDisplayName">記錄顯示名稱</param>
        /// <param name="deleteReason">刪除原因</param>
        /// <param name="deletedBy">刪除用戶</param>
        /// <returns></returns>
        Task<ServiceResult> RecordDeletionAsync(string tableName, int recordId, string? recordDisplayName = null, string? deleteReason = null, string? deletedBy = null);

        /// <summary>
        /// 根據資料表名稱和記錄ID查詢刪除記錄
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="recordId">記錄ID</param>
        /// <returns></returns>
        Task<DeletedRecord?> GetByTableAndRecordAsync(string tableName, int recordId);

        /// <summary>
        /// 根據資料表名稱獲取所有刪除記錄
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <returns></returns>
        Task<List<DeletedRecord>> GetByTableNameAsync(string tableName);

        /// <summary>
        /// 根據刪除用戶獲取刪除記錄
        /// </summary>
        /// <param name="deletedBy">刪除用戶</param>
        /// <returns></returns>
        Task<List<DeletedRecord>> GetByDeletedByAsync(string deletedBy);

        /// <summary>
        /// 永久刪除記錄（真實刪除）
        /// 這會同時刪除：1. DeletedRecord 記錄本身  2. 原始資料表中對應的已軟刪除記錄
        /// </summary>
        /// <param name="deletedRecordId">刪除記錄的ID</param>
        /// <param name="tableName">原始資料表名稱</param>
        /// <param name="recordId">原始記錄ID</param>
        /// <returns></returns>
        Task<ServiceResult> PermanentlyDeleteAsync(int deletedRecordId, string tableName, int recordId);
    }
}
