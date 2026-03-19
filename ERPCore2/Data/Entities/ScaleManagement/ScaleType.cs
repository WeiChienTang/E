using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 磅秤類型主檔
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class ScaleType : BaseEntity
    {
        [Required(ErrorMessage = "磅秤類型名稱為必填")]
        [MaxLength(100, ErrorMessage = "磅秤類型名稱不可超過100個字元")]
        [Display(Name = "磅秤類型名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [MaxLength(20, ErrorMessage = "單位不可超過20個字元")]
        [Display(Name = "計量單位")]
        public string? Unit { get; set; }

        // ===== 外鍵關聯 =====

        /// <summary>關聯的品項（用於入庫追蹤，可選）</summary>
        [Display(Name = "關聯品項")]
        [ForeignKey(nameof(Item))]
        public int? ItemId { get; set; }

        public Item? Item { get; set; }

    }
}
