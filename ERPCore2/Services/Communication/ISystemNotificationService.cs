using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services.Communication
{
    /// <summary>
    /// 系統通知服務介面 - 管理持久化的使用者通知
    /// </summary>
    public interface ISystemNotificationService
    {
        /// <summary>
        /// 建立通知（寫入 DB）
        /// </summary>
        Task<ServiceResult<SystemNotification>> CreateNotificationAsync(SystemNotification notification);

        /// <summary>
        /// 批次建立通知（發送給多人）
        /// </summary>
        Task<ServiceResult> CreateNotificationsAsync(IEnumerable<SystemNotification> notifications);

        /// <summary>
        /// 取得使用者未讀通知數量
        /// </summary>
        Task<int> GetUnreadCountAsync(int employeeId);

        /// <summary>
        /// 取得使用者通知列表（分頁）
        /// </summary>
        Task<(List<SystemNotification> Items, int TotalCount)> GetByEmployeeIdAsync(
            int employeeId, int pageNumber = 1, int pageSize = 20, bool unreadOnly = false,
            NotificationType? typeFilter = null);

        /// <summary>
        /// 依 ID 取得通知
        /// </summary>
        Task<SystemNotification?> GetByIdAsync(int notificationId, int employeeId);

        /// <summary>
        /// 標記為已讀
        /// </summary>
        Task<ServiceResult> MarkAsReadAsync(int notificationId, int employeeId);

        /// <summary>
        /// 標記全部已讀
        /// </summary>
        Task<ServiceResult> MarkAllAsReadAsync(int employeeId);

        /// <summary>
        /// 刪除通知
        /// </summary>
        Task<ServiceResult> DeleteAsync(int notificationId, int employeeId);

        /// <summary>
        /// 清除已讀的舊通知（排程清理用）
        /// </summary>
        Task<int> PurgeOldNotificationsAsync(int daysToKeep = 90);
    }
}
