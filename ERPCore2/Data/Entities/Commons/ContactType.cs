using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 聯絡類型實體 - 定義不同類型的聯絡方式
    /// </summary>
    public class ContactType : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "聯絡類型名稱為必填")]
        [MaxLength(20, ErrorMessage = "聯絡類型名稱不可超過20個字元")]
        [Display(Name = "聯絡類型")]
        public string TypeName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(100, ErrorMessage = "描述不可超過100個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; } = string.Empty;
          // Navigation Properties
        public ICollection<CustomerContact> CustomerContacts { get; set; } = new List<CustomerContact>();
        public ICollection<SupplierContact> SupplierContacts { get; set; } = new List<SupplierContact>();
    }
}
