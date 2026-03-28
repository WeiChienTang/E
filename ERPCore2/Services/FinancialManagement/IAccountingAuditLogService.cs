using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 會計稽核日誌服務介面
    /// </summary>
    public interface IAccountingAuditLogService
    {
        /// <summary>
        /// 記錄會計操作稽核日誌
        /// </summary>
        Task LogAsync(string actionType, string entityType, int entityId,
            string? entityCode, string? description,
            string? previousValue, string? newValue,
            int companyId, string? performedBy);

        /// <summary>
        /// 查詢指定實體的稽核日誌
        /// </summary>
        Task<List<AccountingAuditLog>> GetByEntityAsync(string entityType, int entityId);

        /// <summary>
        /// 查詢指定期間內的稽核日誌
        /// </summary>
        Task<List<AccountingAuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, int companyId);
    }
}
