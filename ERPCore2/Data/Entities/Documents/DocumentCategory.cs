using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 檔案分類 - 管理文件的來源分類與預設存取層級
    /// </summary>
    public class DocumentCategory : BaseEntity
    {
        [Required(ErrorMessage = "分類名稱為必填")]
        [MaxLength(100, ErrorMessage = "分類名稱不可超過100個字元")]
        [Display(Name = "分類名稱")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "來源類型")]
        public DocumentSource Source { get; set; } = DocumentSource.Internal;

        [Display(Name = "預設存取層級")]
        public DocumentAccessLevel DefaultAccessLevel { get; set; } = DocumentAccessLevel.Normal;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
