using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 地址類型實體 - 定義不同類型的地址分類
    /// </summary>
    public class AddressType : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "地址類型名稱為必填")]
        [MaxLength(20, ErrorMessage = "地址類型名稱不可超過20個字元")]
        [Display(Name = "地址類型")]
        public string TypeName { get; set; } = string.Empty;

        // Optional Properties
        [MaxLength(100, ErrorMessage = "描述不可超過100個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
        
        // Navigation Properties
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
