using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Communication
{
    /// <summary>
    /// 系統通知服務實作
    /// 使用 IDbContextFactory 模式，與專案其他 Service 一致
    /// </summary>
    public class SystemNotificationService : ISystemNotificationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<SystemNotificationService> _logger;

        public SystemNotificationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<SystemNotificationService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<SystemNotification>> CreateNotificationAsync(SystemNotification notification)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                notification.CreatedAt = DateTime.UtcNow;
                notification.IsRead = false;
                notification.Status = EntityStatus.Active;

                context.SystemNotifications.Add(notification);
                await context.SaveChangesAsync();

                return ServiceResult<SystemNotification>.Success(notification);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateNotificationAsync), GetType(), _logger, new
                {
                    RecipientEmployeeId = notification.RecipientEmployeeId,
                    NotificationType = notification.NotificationType
                });
                return ServiceResult<SystemNotification>.Failure("建立通知失敗");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> CreateNotificationsAsync(IEnumerable<SystemNotification> notifications)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var now = DateTime.UtcNow;
                foreach (var notification in notifications)
                {
                    notification.CreatedAt = now;
                    notification.IsRead = false;
                    notification.Status = EntityStatus.Active;
                }

                context.SystemNotifications.AddRange(notifications);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateNotificationsAsync), GetType(), _logger);
                return ServiceResult.Failure("批次建立通知失敗");
            }
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SystemNotifications
                    .Where(n => n.RecipientEmployeeId == employeeId
                             && !n.IsRead
                             && n.Status == EntityStatus.Active)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnreadCountAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<(List<SystemNotification> Items, int TotalCount)> GetByEmployeeIdAsync(
            int employeeId, int pageNumber = 1, int pageSize = 20, bool unreadOnly = false,
            NotificationType? typeFilter = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                IQueryable<SystemNotification> query = context.SystemNotifications
                    .Include(n => n.Sender)
                    .Where(n => n.RecipientEmployeeId == employeeId
                             && n.Status == EntityStatus.Active);

                if (unreadOnly)
                    query = query.Where(n => !n.IsRead);

                if (typeFilter.HasValue)
                    query = query.Where(n => n.NotificationType == typeFilter.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeIdAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    PageNumber = pageNumber
                });
                return (new List<SystemNotification>(), 0);
            }
        }

        /// <inheritdoc />
        public async Task<SystemNotification?> GetByIdAsync(int notificationId, int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SystemNotifications
                    .Include(n => n.Sender)
                    .FirstOrDefaultAsync(n => n.Id == notificationId
                                           && n.RecipientEmployeeId == employeeId
                                           && n.Status == EntityStatus.Active);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    NotificationId = notificationId,
                    EmployeeId = employeeId
                });
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> MarkAsReadAsync(int notificationId, int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var notification = await context.SystemNotifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId
                                           && n.RecipientEmployeeId == employeeId
                                           && n.Status == EntityStatus.Active);

                if (notification == null)
                    return ServiceResult.Failure("找不到通知");

                if (notification.IsRead)
                    return ServiceResult.Success();

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(MarkAsReadAsync), GetType(), _logger, new
                {
                    NotificationId = notificationId,
                    EmployeeId = employeeId
                });
                return ServiceResult.Failure("標記已讀失敗");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> MarkAllAsReadAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var now = DateTime.UtcNow;
                await context.SystemNotifications
                    .Where(n => n.RecipientEmployeeId == employeeId
                             && !n.IsRead
                             && n.Status == EntityStatus.Active)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(n => n.IsRead, true)
                        .SetProperty(n => n.ReadAt, now));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(MarkAllAsReadAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return ServiceResult.Failure("全部標記已讀���敗");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(int notificationId, int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var notification = await context.SystemNotifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId
                                           && n.RecipientEmployeeId == employeeId);

                if (notification == null)
                    return ServiceResult.Failure("找不到通知");

                context.SystemNotifications.Remove(notification);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new
                {
                    NotificationId = notificationId,
                    EmployeeId = employeeId
                });
                return ServiceResult.Failure("刪除���知失敗");
            }
        }

        /// <inheritdoc />
        public async Task<int> PurgeOldNotificationsAsync(int daysToKeep = 90)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
                var deleted = await context.SystemNotifications
                    .Where(n => n.IsRead && n.CreatedAt < cutoff)
                    .ExecuteDeleteAsync();

                _logger.LogInformation("已清除 {Count} 筆超過 {Days} 天的已讀通知", deleted, daysToKeep);
                return deleted;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PurgeOldNotificationsAsync), GetType(), _logger, new
                {
                    DaysToKeep = daysToKeep
                });
                return 0;
            }
        }
    }
}
