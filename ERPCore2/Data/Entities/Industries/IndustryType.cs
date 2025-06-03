using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 行業類型實體 - 定義不同的行業分類
    /// </summary>
    public class IndustryType : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "行業類型名稱為必填")]
        [MaxLength(100, ErrorMessage = "行業類型名稱不可超過100個字元")]
        [Display(Name = "行業類型名稱")]
        public string IndustryTypeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(10, ErrorMessage = "行業類型代碼不可超過10個字元")]
        [Display(Name = "行業類型代碼")]
        public string? IndustryTypeCode { get; set; }
        
        // Navigation Properties
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}