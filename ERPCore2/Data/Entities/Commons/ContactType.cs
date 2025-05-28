using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{    public class ContactType
    {
        // Primary Key
        public int ContactTypeId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "聯絡類型名稱為必填")]
        [MaxLength(20, ErrorMessage = "聯絡類型名稱不可超過20個字元")]
        [Display(Name = "聯絡類型")]
        public string TypeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(100, ErrorMessage = "描述不可超過100個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
        
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
        public ICollection<CustomerContact> CustomerContacts { get; set; } = new List<CustomerContact>();
    }
}
