using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 錯誤記錄實體
    /// </summary>
    public class ErrorLog : BaseEntity
    {
        /// <summary>
        /// 錯誤唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "錯誤ID為必填")]
        [MaxLength(50, ErrorMessage = "錯誤ID不可超過50個字元")]
        [Display(Name = "錯誤ID")]
        public string ErrorId { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        [Required(ErrorMessage = "錯誤訊息為必填")]
        [MaxLength(1000, ErrorMessage = "錯誤訊息不可超過1000個字元")]
        [Display(Name = "錯誤訊息")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 堆疊追蹤
        /// </summary>
        [Display(Name = "堆疊追蹤")]
        public string? StackTrace { get; set; }

        /// <summary>
        /// 錯誤來源
        /// </summary>
        [Required(ErrorMessage = "錯誤來源為必填")]
        [Display(Name = "錯誤來源")]
        public ErrorSource Source { get; set; }

        /// <summary>
        /// 錯誤等級
        /// </summary>
        [Required(ErrorMessage = "錯誤等級為必填")]
        [Display(Name = "錯誤等級")]
        public ErrorLevel Level { get; set; }

        /// <summary>
        /// 發生時間
        /// </summary>
        [Required(ErrorMessage = "發生時間為必填")]
        [Display(Name = "發生時間")]
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// 例外類型
        /// </summary>
        [MaxLength(200, ErrorMessage = "例外類型不可超過200個字元")]
        [Display(Name = "例外類型")]
        public string? ExceptionType { get; set; }

        /// <summary>
        /// 內部例外
        /// </summary>
        [Display(Name = "內部例外")]
        public string? InnerException { get; set; }

        /// <summary>
        /// 額外資料 (JSON格式)
        /// </summary>
        [Display(Name = "額外資料")]
        public string? AdditionalData { get; set; }

        /// <summary>
        /// 錯誤分類
        /// </summary>
        [MaxLength(100, ErrorMessage = "錯誤分類不可超過100個字元")]
        [Display(Name = "錯誤分類")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 所屬模組
        /// </summary>
        [MaxLength(100, ErrorMessage = "所屬模組不可超過100個字元")]
        [Display(Name = "所屬模組")]
        public string? Module { get; set; }

        /// <summary>
        /// 使用者ID
        /// </summary>
        [MaxLength(100, ErrorMessage = "使用者ID不可超過100個字元")]
        [Display(Name = "使用者ID")]
        public string? UserId { get; set; }

        /// <summary>
        /// 使用者代理字串
        /// </summary>
        [MaxLength(500, ErrorMessage = "使用者代理字串不可超過500個字元")]
        [Display(Name = "使用者代理")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 請求路徑
        /// </summary>
        [MaxLength(500, ErrorMessage = "請求路徑不可超過500個字元")]
        [Display(Name = "請求路徑")]
        public string? RequestPath { get; set; }

        /// <summary>
        /// 是否已解決
        /// </summary>
        [Display(Name = "已解決")]
        public bool IsResolved { get; set; } = false;

        /// <summary>
        /// 解決者
        /// </summary>
        [MaxLength(100, ErrorMessage = "解決者不可超過100個字元")]
        [Display(Name = "解決者")]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// 解決時間
        /// </summary>
        [Display(Name = "解決時間")]
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// 解決方案描述
        /// </summary>
        [MaxLength(1000, ErrorMessage = "解決方案描述不可超過1000個字元")]
        [Display(Name = "解決方案")]
        public string? Resolution { get; set; }
    }
}
