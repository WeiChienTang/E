using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品分類實體 - 定義商品的分類資訊，支援階層結構
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class ProductCategory : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "分類名稱為必填")]
        [MaxLength(50, ErrorMessage = "分類名稱不可超過50個字元")]
        [Display(Name = "分類名稱")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "可販售")]
        public bool IsSaleable { get; set; } = true;  // 預設為可販售（大部分分類都可販售）
        
        // Navigation Properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
