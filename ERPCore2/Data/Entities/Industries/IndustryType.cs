using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    public class IndustryType
    {
        // Primary Key
        public int IndustryTypeId { get; set; }
          // Required Properties
        [Required(ErrorMessage = "行業類型名稱為必填")]
        [MaxLength(100, ErrorMessage = "行業類型名稱不可超過100個字元")]
        [Display(Name = "行業類型名稱")]
        public string IndustryTypeName { get; set; } = string.Empty;
          // Optional Properties
        [MaxLength(10, ErrorMessage = "行業類型代碼不可超過10個字元")]
        [Display(Name = "行業類型代碼")]
        public string? IndustryTypeCode { get; set; }
        
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
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}