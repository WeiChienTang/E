using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 傳票附件 - 傳票支援上傳掃描文件（發票、收據、合約等）
    /// 儲存位置：wwwroot/uploads/journal-attachments/{year}/{month}/
    /// ⚠️ 已過帳傳票的附件仍可刪除，因附件不影響帳務正確性。
    /// </summary>
    [Index(nameof(JournalEntryId))]
    [Index(nameof(UploadedByEmployeeId))]
    [Index(nameof(UploadedAt))]
    public class JournalEntryAttachment : BaseEntity
    {
        /// <summary>所屬傳票 ID</summary>
        [Required]
        [Display(Name = "傳票")]
        public int JournalEntryId { get; set; }

        /// <summary>所屬傳票導航屬性</summary>
        public JournalEntry JournalEntry { get; set; } = null!;

        /// <summary>原始檔名（使用者上傳時的檔案名稱，用於顯示）</summary>
        [Required(ErrorMessage = "請選擇檔案")]
        [MaxLength(255, ErrorMessage = "檔案名稱不可超過255個字元")]
        [Display(Name = "檔案名稱")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 儲存路徑（相對於 wwwroot，如 /uploads/journal-attachments/2026/03/20260321_abc12345.pdf）
        /// 儲存格式確保年月目錄組織，方便管理與清理
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Display(Name = "儲存路徑")]
        public string StoredFilePath { get; set; } = string.Empty;

        /// <summary>檔案大小（bytes）</summary>
        [Display(Name = "檔案大小")]
        public long FileSize { get; set; }

        /// <summary>MIME 類型（如 application/pdf、image/jpeg）</summary>
        [MaxLength(100)]
        [Display(Name = "檔案類型")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>上傳時間</summary>
        [Display(Name = "上傳時間")]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        /// <summary>上傳人員 ID</summary>
        [Display(Name = "上傳人員")]
        public int? UploadedByEmployeeId { get; set; }

        /// <summary>上傳人員導航屬性</summary>
        public Employee? UploadedByEmployee { get; set; }

        // ===== 計算屬性（NotMapped）=====

        /// <summary>副檔名（小寫，如 .pdf）</summary>
        [NotMapped]
        public string Extension => Path.GetExtension(FileName).ToLowerInvariant();

        /// <summary>是否為圖片（可 inline 預覽）</summary>
        [NotMapped]
        public bool IsImage => ContentType.StartsWith("image/");

        /// <summary>是否為 PDF</summary>
        [NotMapped]
        public bool IsPdf => ContentType == "application/pdf";
    }
}
