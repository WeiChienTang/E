using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 客戶實體 - 定義客戶基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Customer : BaseEntity
    {        
        [Required(ErrorMessage = "公司名稱為必填")]
        [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(8, ErrorMessage = "統一編號不可超過8個字元")]
        [Display(Name = "統一編號")]
        public string? TaxNumber { get; set; }
        
        // Foreign Keys
        [Display(Name = "客戶類型")]
        [ForeignKey(nameof(CustomerType))]
        public int? CustomerTypeId { get; set; }
        
        // Navigation Properties
        public CustomerType? CustomerType { get; set; }
        
        // 聯絡資訊請使用 IContactService 取得 (OwnerType = "Customer", OwnerId = this.Id)
        // 地址資訊請使用 IAddressService 取得
    }
}
