using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 檔案存留 - 管理各類文件的儲存、分類與存取權限
    /// </summary>
    public class Document : BaseEntity
    {
        [Required(ErrorMessage = "文件標題為必填")]
        [MaxLength(200, ErrorMessage = "文件標題不可超過200個字元")]
        [Display(Name = "文件標題")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "檔案分類")]
        public int DocumentCategoryId { get; set; }
        public DocumentCategory? DocumentCategory { get; set; }

        [Required(ErrorMessage = "檔案路徑為必填")]
        [MaxLength(500, ErrorMessage = "檔案路徑不可超過500個字元")]
        [Display(Name = "檔案路徑")]
        public string FilePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "檔案名稱為必填")]
        [MaxLength(255, ErrorMessage = "檔案名稱不可超過255個字元")]
        [Display(Name = "原始檔案名稱")]
        public string FileName { get; set; } = string.Empty;

        [Display(Name = "檔案大小（Bytes）")]
        public long FileSize { get; set; }

        [MaxLength(100, ErrorMessage = "MIME類型不可超過100個字元")]
        [Display(Name = "MIME類型")]
        public string? MimeType { get; set; }

        [MaxLength(200, ErrorMessage = "發文機關不可超過200個字元")]
        [Display(Name = "發文機關/來源")]
        public string? IssuedBy { get; set; }

        [Display(Name = "發文日期")]
        public DateTime? IssuedDate { get; set; }

        [Display(Name = "有效期限")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// 個別存取層級，null 表示使用所屬分類的 DefaultAccessLevel
        /// </summary>
        [Display(Name = "存取層級")]
        public DocumentAccessLevel? AccessLevel { get; set; }

        // 預留多型關聯欄位（供未來擴充使用）
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }
}
