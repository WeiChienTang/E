using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 品項照片實體 - 儲存品項的多張照片資訊
    /// </summary>
    public class ItemPhoto : BaseEntity
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(500)]
        public string PhotoPath { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        [MaxLength(100)]
        public string? Caption { get; set; }

        public int SortOrder { get; set; } = 0;

        // 導航屬性
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }
    }
}
