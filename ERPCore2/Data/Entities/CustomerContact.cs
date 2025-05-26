using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    public class CustomerContact
    {
        // Primary Key
        public int ContactId { get; set; }
        
        // Foreign Keys
        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        public int CustomerId { get; set; }
        
        [Display(Name = "聯絡類型")]
        public int? ContactTypeId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "聯絡內容為必填")]
        [MaxLength(100, ErrorMessage = "聯絡內容不可超過100個字元")]
        [Display(Name = "聯絡內容")]
        public string ContactValue { get; set; } = string.Empty;
        
        // Optional Properties
        [Display(Name = "是否為主要聯絡方式")]
        public bool IsPrimary { get; set; } = false;
          // Status
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Default;
        
        // Navigation Properties
        public Customer Customer { get; set; } = null!;
        public ContactType? ContactType { get; set; }
    }
}
