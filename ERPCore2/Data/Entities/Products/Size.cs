using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 尺寸實體 - 定義商品尺寸規格
    /// </summary>
    [Index(nameof(SizeCode), IsUnique = true)]
    public class Size : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "尺寸代碼為必填")]
        [MaxLength(20, ErrorMessage = "尺寸代碼不可超過20個字元")]
        [Display(Name = "尺寸代碼")]
        public string SizeCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "尺寸名稱為必填")]
        [MaxLength(50, ErrorMessage = "尺寸名稱不可超過50個字元")]
        [Display(Name = "尺寸名稱")]
        public string SizeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(100, ErrorMessage = "尺寸描述不可超過100個字元")]
        [Display(Name = "尺寸描述")]
        public string? Description { get; set; }
        
        // Navigation Properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
