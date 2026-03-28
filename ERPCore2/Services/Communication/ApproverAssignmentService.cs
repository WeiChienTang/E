using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Communication
{
    /// <summary>
    /// 審核人員指派服務實作
    /// </summary>
    public class ApproverAssignmentService : IApproverAssignmentService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<ApproverAssignmentService> _logger;

        public ApproverAssignmentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<ApproverAssignmentService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<int>> GetApproverEmployeeIdsAsync(string moduleName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ApproverAssignments
                    .Where(a => a.ModuleName == moduleName && a.Status == EntityStatus.Active)
                    .OrderByDescending(a => a.IsPrimary)
                    .Select(a => a.ApproverEmployeeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetApproverEmployeeIdsAsync), GetType(), _logger, new
                {
                    ModuleName = moduleName
                });
                return new List<int>();
            }
        }

        /// <inheritdoc />
        public async Task<List<ApproverAssignment>> GetByModuleAsync(string moduleName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ApproverAssignments
                    .Include(a => a.Approver)
                    .Where(a => a.ModuleName == moduleName && a.Status == EntityStatus.Active)
                    .OrderByDescending(a => a.IsPrimary)
                    .ThenBy(a => a.ApproverEmployeeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByModuleAsync), GetType(), _logger, new
                {
                    ModuleName = moduleName
                });
                return new List<ApproverAssignment>();
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> SaveAssignmentsAsync(string moduleName, List<ApproverAssignment> assignments)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                // 移除舊指派
                var existing = await context.ApproverAssignments
                    .Where(a => a.ModuleName == moduleName)
                    .ToListAsync();
                context.ApproverAssignments.RemoveRange(existing);

                // 新增新指派
                var now = DateTime.UtcNow;
                foreach (var assignment in assignments)
                {
                    assignment.ModuleName = moduleName;
                    assignment.Status = EntityStatus.Active;
                    assignment.CreatedAt = now;
                }
                context.ApproverAssignments.AddRange(assignments);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SaveAssignmentsAsync), GetType(), _logger, new
                {
                    ModuleName = moduleName,
                    Count = assignments.Count
                });
                return ServiceResult.Failure("儲存審核人員指派失敗");
            }
        }
    }
}
