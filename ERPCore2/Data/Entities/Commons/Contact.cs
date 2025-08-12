using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 統一聯絡方式實體 - 用於所有實體的聯絡資訊管理
    /// </summary>
    public class Contact : BaseEntity
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
        public ContactType? ContactType { get; set; }
        
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
    /// 聯絡方式擁有者類型常數
    /// </summary>
    public static class ContactOwnerTypes
    {
        public const string Customer = "Customer";
        public const string Supplier = "Supplier";
        public const string Employee = "Employee";
    }
}
