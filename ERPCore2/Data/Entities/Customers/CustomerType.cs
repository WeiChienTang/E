using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 客戶類型實體 - 定義不同類型的客戶分類
    /// </summary>
    public class CustomerType : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "類型名稱為必填")]
        [MaxLength(50, ErrorMessage = "類型名稱不可超過50個字元")]
        [Display(Name = "類型名稱")]
        public string TypeName { get; set; } = string.Empty;
        
        // Navigation Properties
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
