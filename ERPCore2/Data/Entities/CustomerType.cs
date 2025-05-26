using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    public class CustomerType
    {
        // Primary Key
        public int CustomerTypeId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "類型名稱為必填")]
        [MaxLength(50, ErrorMessage = "類型名稱不可超過50個字元")]
        [Display(Name = "類型名稱")]
        public string TypeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
          // Status
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Default;
        
        // Navigation Properties
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
