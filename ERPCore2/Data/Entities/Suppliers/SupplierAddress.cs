using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廠商地址實體 - 定義廠商的地址資訊
    /// </summary>
    public class SupplierAddress : BaseEntity
    {
        // Foreign Keys
        [Display(Name = "廠商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }
        
        [Display(Name = "地址類型")]
        [ForeignKey(nameof(AddressType))]
        public int? AddressTypeId { get; set; }
        
        // Optional Properties
        [MaxLength(10, ErrorMessage = "郵遞區號不可超過10個字元")]
        [Display(Name = "郵遞區號")]
        public string? PostalCode { get; set; }
        
        [MaxLength(50, ErrorMessage = "城市不可超過50個字元")]
        [Display(Name = "城市")]
        public string? City { get; set; }
        
        // 已停用 - 不再使用行政區欄位，保留以維持資料庫結構相容性
        // 預設為空值，避免影響現有資料和業務邏輯
        [MaxLength(50, ErrorMessage = "行政區不可超過50個字元")]
        [Display(Name = "行政區")]
        public string? District { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? Address { get; set; }
        
        [Display(Name = "是否為主要地址")]
        public bool IsPrimary { get; set; } = false;
        
        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public AddressType? AddressType { get; set; }
    }
}