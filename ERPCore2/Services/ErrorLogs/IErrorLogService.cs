using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 錯誤記錄服務介面
    /// </summary>
    public interface IErrorLogService : IGenericManagementService<ErrorLog>
    {
        /// <summary>
        /// 記錄錯誤並返回錯誤ID
        /// </summary>
        /// <param name="exception">例外物件</param>
        /// <param name="additionalData">額外資料</param>
        /// <returns>錯誤唯一識別碼</returns>
        Task<string> LogErrorAsync(Exception exception, object? additionalData = null);

        /// <summary>
        /// 根據錯誤ID取得錯誤記錄
        /// </summary>
        /// <param name="errorId">錯誤ID</param>
        /// <returns>錯誤記錄</returns>
        Task<ErrorLog?> GetByErrorIdAsync(string errorId);

        /// <summary>
        /// 根據錯誤等級取得錯誤記錄
        /// </summary>
        /// <param name="level">錯誤等級</param>
        /// <returns>錯誤記錄列表</returns>
        Task<List<ErrorLog>> GetByLevelAsync(ErrorLevel level);

        /// <summary>
        /// 根據錯誤來源取得錯誤記錄
        /// </summary>
        /// <param name="source">錯誤來源</param>
        /// <returns>錯誤記錄列表</returns>
        Task<List<ErrorLog>> GetBySourceAsync(ErrorSource source);

        /// <summary>
        /// 根據時間範圍取得錯誤記錄
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>錯誤記錄列表</returns>
        Task<List<ErrorLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得未解決的錯誤記錄
        /// </summary>
        /// <returns>未解決的錯誤記錄列表</returns>
        Task<List<ErrorLog>> GetUnresolvedAsync();

        /// <summary>
        /// 標記錯誤為已解決
        /// </summary>
        /// <param name="errorId">錯誤ID</param>
        /// <param name="resolvedBy">解決者</param>
        /// <param name="resolution">解決方案</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> MarkAsResolvedAsync(string errorId, string resolvedBy, string resolution);

        /// <summary>
        /// 批次標記錯誤為已解決
        /// </summary>
        /// <param name="errorIds">錯誤ID列表</param>
        /// <param name="resolvedBy">解決者</param>
        /// <param name="resolution">解決方案</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> MarkBatchAsResolvedAsync(List<string> errorIds, string resolvedBy, string resolution);

        /// <summary>
        /// 清理舊的錯誤記錄
        /// </summary>
        /// <param name="daysToKeep">保留天數</param>
        /// <returns>刪除的記錄數量</returns>
        Task<int> CleanupOldErrorsAsync(int daysToKeep = 30);
    }
}
