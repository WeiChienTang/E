using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 統一地址實體 - 用於所有實體的地址資訊管理
    /// </summary>
    public class Address : BaseEntity
    {
        // Owner Information
        [Required(ErrorMessage = "擁有者類型為必填")]
        [MaxLength(20, ErrorMessage = "擁有者類型不可超過20個字元")]
        [Display(Name = "擁有者類型")]
        public string OwnerType { get; set; } = string.Empty; // "Customer", "Supplier", "Employee"
        
        [Required(ErrorMessage = "擁有者ID為必填")]
        [Display(Name = "擁有者ID")]
        public int OwnerId { get; set; }
        
        // Foreign Keys
        [Display(Name = "地址類型")]
        [ForeignKey(nameof(AddressType))]
        public int? AddressTypeId { get; set; }
        
        // Address Properties
        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? AddressLine { get; set; }
        
        // Navigation Properties
        public AddressType? AddressType { get; set; }
        
        // Helper methods for type-safe access
        public Customer? Customer => OwnerType == "Customer" ? GetOwner<Customer>() : null;
        public Supplier? Supplier => OwnerType == "Supplier" ? GetOwner<Supplier>() : null;
        public Employee? Employee => OwnerType == "Employee" ? GetOwner<Employee>() : null;
        
        private T? GetOwner<T>() where T : BaseEntity
        {
            // This would be resolved through the context when needed
            // Implementation would be handled by the service layer
            return default(T);
        }
    }
    
    /// <summary>
    /// 地址擁有者類型常數
    /// </summary>
    public static class AddressOwnerTypes
    {
        public const string Customer = "Customer";
        public const string Supplier = "Supplier";
        public const string Employee = "Employee";
    }
}
