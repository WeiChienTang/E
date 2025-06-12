using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廠商類型實體 - 定義不同類型的廠商分類
    /// </summary>
    public class SupplierType : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "類型名稱為必填")]
        [MaxLength(50, ErrorMessage = "類型名稱不可超過50個字元")]
        [Display(Name = "類型名稱")]
        public string TypeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
        
        // Navigation Properties
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}