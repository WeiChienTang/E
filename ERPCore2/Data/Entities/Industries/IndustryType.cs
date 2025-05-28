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
          // Status
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Default;
        
        // Navigation Properties
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
