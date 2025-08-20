using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 刪除記錄實體 - 記錄被軟刪除的資料詳細資訊
    /// </summary>
    [Index(nameof(TableName), nameof(RecordId))]
    [Index(nameof(DeletedAt))]
    public class DeletedRecord : BaseEntity
    {
        /// <summary>
        /// 被刪除的資料表名稱
        /// </summary>
        [Required(ErrorMessage = "資料表名稱為必填")]
        [MaxLength(100, ErrorMessage = "資料表名稱不可超過100個字元")]
        [Display(Name = "資料表名稱")]
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 被刪除的記錄ID
        /// </summary>
        [Required(ErrorMessage = "記錄ID為必填")]
        [Display(Name = "記錄ID")]
        public int RecordId { get; set; }

        /// <summary>
        /// 被刪除記錄的顯示名稱（如：部門名稱、員工姓名等）
        /// </summary>
        [MaxLength(200, ErrorMessage = "顯示名稱不可超過200個字元")]
        [Display(Name = "記錄顯示名稱")]
        public string? RecordDisplayName { get; set; }

        /// <summary>
        /// 刪除時間
        /// </summary>
        [Display(Name = "刪除時間")]
        public DateTime DeletedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 執行刪除的用戶
        /// </summary>
        [MaxLength(50, ErrorMessage = "刪除用戶不可超過50個字元")]
        [Display(Name = "刪除用戶")]
        public string? DeletedBy { get; set; }

        /// <summary>
        /// 刪除原因或備註
        /// 詳細的刪除資訊可以記錄在此欄位或基底的 Remarks 欄位
        /// </summary>
        [MaxLength(1000, ErrorMessage = "刪除原因不可超過1000個字元")]
        [Display(Name = "刪除原因")]
        public string? DeleteReason { get; set; }
    }
}
