using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廠商聯絡方式實體 - 定義廠商的聯絡資訊
    /// </summary>
    public class SupplierContact : BaseEntity
    {
        // Foreign Keys
        [Required(ErrorMessage = "廠商為必填")]
        [Display(Name = "廠商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Display(Name = "聯絡類型")]
        [ForeignKey(nameof(ContactType))]
        public int? ContactTypeId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "聯絡內容為必填")]
        [MaxLength(100, ErrorMessage = "聯絡內容不可超過100個字元")]
        [Display(Name = "聯絡內容")]
        public string ContactValue { get; set; } = string.Empty;
        
        // Optional Properties
        [Display(Name = "是否為主要聯絡方式")]
        public bool IsPrimary { get; set; } = false;
        
        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public ContactType? ContactType { get; set; }
    }
}