using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 訂單照片實體 - 儲存訂單的多張附件照片
    /// </summary>
    public class SalesOrderPhoto : BaseEntity
    {
        [Required]
        public int SalesOrderId { get; set; }

        [Required]
        [MaxLength(500)]
        public string PhotoPath { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        [MaxLength(100)]
        public string? Caption { get; set; }

        public int SortOrder { get; set; } = 0;

        // 導航屬性
        [ForeignKey(nameof(SalesOrderId))]
        public SalesOrder? SalesOrder { get; set; }
    }
}
