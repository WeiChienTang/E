using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 通知類型
    /// </summary>
    public enum NotificationType
    {
        /// <summary>審核請求（文件提交待審核時通知審核者）</summary>
        [Display(Name = "審核請求")]
        ApprovalRequest = 1,

        /// <summary>審核結果（文件核准或駁回時通知建立者）</summary>
        [Display(Name = "審核結果")]
        ApprovalResult = 2,

        /// <summary>系統通知（系統自動產生的一般通知）</summary>
        [Display(Name = "系統通知")]
        SystemAlert = 3,

        /// <summary>使用者訊息（使用者間的直接通訊）</summary>
        [Display(Name = "使用者訊息")]
        UserMessage = 4,

        /// <summary>提醒（排程提醒或到期提醒）</summary>
        [Display(Name = "提醒")]
        Reminder = 5
    }

    /// <summary>
    /// 通知優先程度
    /// </summary>
    public enum NotificationPriority
    {
        [Display(Name = "低")]
        Low = 0,

        [Display(Name = "一般")]
        Normal = 1,

        [Display(Name = "高")]
        High = 2,

        [Display(Name = "緊急")]
        Urgent = 3
    }
}
