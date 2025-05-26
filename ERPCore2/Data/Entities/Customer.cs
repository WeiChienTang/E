using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    public class Customer
    {
        // Primary Key
        public int CustomerId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "客戶代碼為必填")]
        [MaxLength(20, ErrorMessage = "客戶代碼不可超過20個字元")]
        [Display(Name = "客戶代碼")]
        public string CustomerCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "公司名稱為必填")]
        [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(20, ErrorMessage = "統一編號不可超過20個字元")]
        [Display(Name = "統一編號")]
        public string? TaxNumber { get; set; }
        
        // Foreign Keys
        [Display(Name = "客戶類型")]
        public int? CustomerTypeId { get; set; }
        
        [Display(Name = "行業別")]
        public int? IndustryId { get; set; }
        
        // Audit Fields
        [Display(Name = "建立日期")]
        public DateTime CreatedDate { get; set; }
        
        [MaxLength(50, ErrorMessage = "建立者不可超過50個字元")]
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }
        
        [Display(Name = "修改日期")]
        public DateTime? ModifiedDate { get; set; }
        
        [MaxLength(50, ErrorMessage = "修改者不可超過50個字元")]
        [Display(Name = "修改者")]
        public string? ModifiedBy { get; set; }
          // Status
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Default;
        
        // Navigation Properties
        public CustomerType? CustomerType { get; set; }
        public Industry? Industry { get; set; }
        public ICollection<CustomerContact> CustomerContacts { get; set; } = new List<CustomerContact>();
        public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
    }
}
