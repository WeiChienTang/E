using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 尺寸實體 - 定義商品尺寸規格
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Size : BaseEntity
    {
        
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
