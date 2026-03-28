using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 會計稽核日誌 — 記錄過帳、沖銷、關帳、年底結帳等敏感會計操作
    /// 供外部稽核人員查閱操作歷史
    /// </summary>
    [Index(nameof(ActionType))]
    [Index(nameof(PerformedAt))]
    [Index(nameof(PerformedByEmployeeId))]
    [Index(nameof(EntityType), nameof(EntityId))]
    public class AccountingAuditLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 操作類型（PostEntry / ReverseEntry / CancelEntry / ClosePeriod / LockPeriod / ReopenPeriod / YearEndClosing / SaveOpeningBalance）
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// 操作對象的實體類型（JournalEntry / FiscalPeriod）
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// 操作對象的 ID
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 操作對象的顯示代碼（如傳票編號 JE-2026-0001、期間 2026/01）
        /// </summary>
        [MaxLength(100)]
        public string? EntityCode { get; set; }

        /// <summary>
        /// 操作描述（簡要說明操作內容和結果）
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 操作前的關鍵狀態值（如 Draft / Open）
        /// </summary>
        [MaxLength(100)]
        public string? PreviousValue { get; set; }

        /// <summary>
        /// 操作後的關鍵狀態值（如 Posted / Closed）
        /// </summary>
        [MaxLength(100)]
        public string? NewValue { get; set; }

        /// <summary>
        /// 操作時間
        /// </summary>
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 操作者員工 ID
        /// </summary>
        public int? PerformedByEmployeeId { get; set; }

        /// <summary>
        /// 操作者名稱（冗餘存放，避免事後查詢 JOIN）
        /// </summary>
        [MaxLength(100)]
        public string? PerformedByName { get; set; }

        /// <summary>
        /// 公司 ID
        /// </summary>
        public int CompanyId { get; set; }
    }
}
