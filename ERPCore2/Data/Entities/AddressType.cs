using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    public class AddressType
    {
        // Primary Key
        public int AddressTypeId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "地址類型名稱為必填")]
        [MaxLength(20, ErrorMessage = "地址類型名稱不可超過20個字元")]
        [Display(Name = "地址類型")]
        public string TypeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(100, ErrorMessage = "描述不可超過100個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
          // Status
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Default;
        
        // Navigation Properties
        public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
    }
}
