using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品分類實體 - 定義商品的分類資訊，支援階層結構
    /// </summary>
    [Index(nameof(CategoryCode), IsUnique = true)]
    public class ProductCategory : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "分類名稱為必填")]
        [MaxLength(50, ErrorMessage = "分類名稱不可超過50個字元")]
        [Display(Name = "分類名稱")]
        public string CategoryName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(20, ErrorMessage = "分類代碼不可超過20個字元")]
        [Display(Name = "分類代碼")]
        public string? CategoryCode { get; set; }
        
        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
        
        // Foreign Keys (Self-referencing for hierarchy)
        [Display(Name = "父分類")]
        [ForeignKey(nameof(ParentCategory))]
        public int? ParentCategoryId { get; set; }
        
        // Navigation Properties
        public ProductCategory? ParentCategory { get; set; }
        public ICollection<ProductCategory> ChildCategories { get; set; } = new List<ProductCategory>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}