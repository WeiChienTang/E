using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 會計稽核日誌服務 — 記錄敏感會計操作（過帳、沖銷、關帳、年底結帳等）
    /// </summary>
    public class AccountingAuditLogService : IAccountingAuditLogService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<AccountingAuditLogService> _logger;

        public AccountingAuditLogService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<AccountingAuditLogService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task LogAsync(string actionType, string entityType, int entityId,
            string? entityCode, string? description,
            string? previousValue, string? newValue,
            int companyId, string? performedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 嘗試解析 performedBy 為員工 ID
                int? employeeId = null;
                if (int.TryParse(performedBy, out var empId))
                    employeeId = empId;

                var log = new AccountingAuditLog
                {
                    ActionType = actionType,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityCode,
                    Description = description,
                    PreviousValue = previousValue,
                    NewValue = newValue,
                    CompanyId = companyId,
                    PerformedAt = DateTime.UtcNow,
                    PerformedByEmployeeId = employeeId,
                    PerformedByName = performedBy
                };

                context.AccountingAuditLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // 稽核日誌寫入失敗不應阻擋主流程，僅記錄警告
                _logger.LogWarning(ex, "寫入會計稽核日誌失敗：{ActionType} {EntityType}#{EntityId}",
                    actionType, entityType, entityId);
            }
        }

        public async Task<List<AccountingAuditLog>> GetByEntityAsync(string entityType, int entityId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountingAuditLogs
                .Where(l => l.EntityType == entityType && l.EntityId == entityId)
                .OrderByDescending(l => l.PerformedAt)
                .ToListAsync();
        }

        public async Task<List<AccountingAuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, int companyId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountingAuditLogs
                .Where(l => l.CompanyId == companyId && l.PerformedAt >= from && l.PerformedAt <= to)
                .OrderByDescending(l => l.PerformedAt)
                .ToListAsync();
        }
    }
}
