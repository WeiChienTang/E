using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廢料類型主檔
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class WasteType : BaseEntity
    {
        [Required(ErrorMessage = "廢料類型名稱為必填")]
        [MaxLength(100, ErrorMessage = "廢料類型名稱不可超過100個字元")]
        [Display(Name = "廢料類型名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [MaxLength(20, ErrorMessage = "單位不可超過20個字元")]
        [Display(Name = "計量單位")]
        public string? Unit { get; set; }

        // ===== 外鍵關聯 =====

        /// <summary>關聯的商品（用於入庫追蹤，可選）</summary>
        [Display(Name = "關聯商品")]
        [ForeignKey(nameof(Product))]
        public int? ProductId { get; set; }

        public Product? Product { get; set; }

        // Navigation Properties
        public ICollection<WasteRecord> WasteRecords { get; set; } = new List<WasteRecord>();
    }
}
