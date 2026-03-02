using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 文件附件 - 一份文件可對應多個附件檔案
    /// </summary>
    [Index(nameof(DocumentId))]
    public class DocumentFile : BaseEntity
    {
        [Required(ErrorMessage = "文件為必填")]
        [Display(Name = "文件")]
        [ForeignKey(nameof(Document))]
        public int DocumentId { get; set; }

        /// <summary>
        /// 使用者自訂顯示名稱，預設帶入原始檔名，可自行修改
        /// </summary>
        [Required(ErrorMessage = "附件名稱為必填")]
        [MaxLength(255, ErrorMessage = "附件名稱不可超過255個字元")]
        [Display(Name = "附件名稱")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "檔案路徑為必填")]
        [MaxLength(500, ErrorMessage = "檔案路徑不可超過500個字元")]
        [Display(Name = "檔案路徑")]
        public string FilePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "原始檔名為必填")]
        [MaxLength(255, ErrorMessage = "原始檔名不可超過255個字元")]
        [Display(Name = "原始檔名")]
        public string FileName { get; set; } = string.Empty;

        [Display(Name = "檔案大小（Bytes）")]
        public long FileSize { get; set; }

        [MaxLength(100, ErrorMessage = "MIME類型不可超過100個字元")]
        [Display(Name = "MIME類型")]
        public string? MimeType { get; set; }

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        public Document Document { get; set; } = null!;
    }
}
