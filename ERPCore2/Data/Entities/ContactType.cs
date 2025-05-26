using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    public class ContactType
    {
        // Primary Key
        public int ContactTypeId { get; set; }
        
        // Required Properties
        [Required(ErrorMessage = "聯絡類型名稱為必填")]
        [MaxLength(20, ErrorMessage = "聯絡類型名稱不可超過20個字元")]
        [Display(Name = "聯絡類型")]
        public string TypeName { get; set; } = string.Empty;
        
        // Navigation Properties
        public ICollection<CustomerContact> CustomerContacts { get; set; } = new List<CustomerContact>();
    }
}
