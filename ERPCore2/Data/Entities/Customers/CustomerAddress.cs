using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    public class CustomerAddress
    {
        // Primary Key
        public int AddressId { get; set; }
        
        // Foreign Keys
        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        public int CustomerId { get; set; }
        
        [Display(Name = "地址類型")]
        public int? AddressTypeId { get; set; }
        
        // Optional Properties
        [MaxLength(10, ErrorMessage = "郵遞區號不可超過10個字元")]
        [Display(Name = "郵遞區號")]
        public string? PostalCode { get; set; }
        
        [MaxLength(50, ErrorMessage = "城市不可超過50個字元")]
        [Display(Name = "城市")]
        public string? City { get; set; }
        
        [MaxLength(50, ErrorMessage = "行政區不可超過50個字元")]
        [Display(Name = "行政區")]
        public string? District { get; set; }
        
        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? Address { get; set; }
        
        [Display(Name = "是否為主要地址")]
        public bool IsPrimary { get; set; } = false;
          // Status
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Default;
        
        // Navigation Properties
        public Customer Customer { get; set; } = null!;
        public AddressType? AddressType { get; set; }
    }
}
