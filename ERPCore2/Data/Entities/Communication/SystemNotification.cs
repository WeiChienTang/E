using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 系統通知 - 持久化的使用者通知記錄
    /// 支援審核通知、系統通知、使用者訊息等多種類型
    /// </summary>
    [Index(nameof(RecipientEmployeeId), nameof(IsRead), nameof(CreatedAt))]
    [Index(nameof(RecipientEmployeeId), nameof(CreatedAt))]
    public class SystemNotification : BaseEntity
    {
        /// <summary>
        /// 收件人（EmployeeId）
        /// </summary>
        [Required]
        [Display(Name = "收件人")]
        [ForeignKey(nameof(Recipient))]
        public int RecipientEmployeeId { get; set; }

        /// <summary>
        /// 發送人（EmployeeId，null = 系統自動產生）
        /// </summary>
        [Display(Name = "發送人")]
        [ForeignKey(nameof(Sender))]
        public int? SenderEmployeeId { get; set; }

        /// <summary>
        /// 通知類型
        /// </summary>
        [Required]
        [Display(Name = "通知類型")]
        public NotificationType NotificationType { get; set; }

        /// <summary>
        /// 通知優先程度
        /// </summary>
        [Display(Name = "優先程度")]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// 通知標題
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "標題")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知內容
        /// </summary>
        [MaxLength(2000)]
        [Display(Name = "內容")]
        public string? Content { get; set; }

        /// <summary>
        /// 是否已讀
        /// </summary>
        [Display(Name = "已讀")]
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// 已讀時間
        /// </summary>
        [Display(Name = "已讀時間")]
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// 來源模組（如 "SalesOrder"、"PurchaseOrder"）
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "來源模組")]
        public string? SourceModule { get; set; }

        /// <summary>
        /// 來源記錄 ID（對應模組記錄的 PK）
        /// </summary>
        [Display(Name = "來源 ID")]
        public int? SourceId { get; set; }

        /// <summary>
        /// 通知關聯的導航 URL（點擊通知後跳轉的路徑）
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "連結")]
        public string? NavigationUrl { get; set; }

        // ===== 導航屬性 =====

        public Employee? Recipient { get; set; }
        public Employee? Sender { get; set; }

        // ===== 計算屬性 =====

        /// <summary>
        /// 通知類型顯示文字
        /// </summary>
        [NotMapped]
        public string NotificationTypeText => NotificationType switch
        {
            NotificationType.ApprovalRequest => "審核請求",
            NotificationType.ApprovalResult => "審核結果",
            NotificationType.SystemAlert => "系統通知",
            NotificationType.UserMessage => "使用者訊息",
            NotificationType.Reminder => "提醒",
            _ => "未知"
        };

        /// <summary>
        /// 優先程度顯示文字
        /// </summary>
        [NotMapped]
        public string PriorityText => Priority switch
        {
            NotificationPriority.Low => "低",
            NotificationPriority.Normal => "一般",
            NotificationPriority.High => "高",
            NotificationPriority.Urgent => "緊急",
            _ => "一般"
        };

        /// <summary>
        /// 發送人顯示名稱
        /// </summary>
        [NotMapped]
        public string SenderDisplayName =>
            Sender != null ? Sender.Name ?? "未知" : "系統";

        /// <summary>
        /// 通知類型對應的 Bootstrap Icon CSS class
        /// </summary>
        [NotMapped]
        public string TypeIconClass => NotificationType switch
        {
            NotificationType.ApprovalRequest => "bi-clipboard-check",
            NotificationType.ApprovalResult => "bi-check-circle",
            NotificationType.SystemAlert => "bi-exclamation-triangle",
            NotificationType.UserMessage => "bi-envelope",
            NotificationType.Reminder => "bi-alarm",
            _ => "bi-bell"
        };

        /// <summary>
        /// 通知類型對應的 CSS class（用於顏色區分）
        /// </summary>
        [NotMapped]
        public string TypeBadgeClass => NotificationType switch
        {
            NotificationType.ApprovalRequest => "badge-warning",
            NotificationType.ApprovalResult => "badge-info",
            NotificationType.SystemAlert => "badge-danger",
            NotificationType.UserMessage => "badge-primary",
            NotificationType.Reminder => "badge-secondary",
            _ => "badge-secondary"
        };
    }
}
