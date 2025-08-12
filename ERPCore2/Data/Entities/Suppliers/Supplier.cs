using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廠商實體 - 定義廠商基本資訊
    /// </summary>
    [Index(nameof(SupplierCode), IsUnique = true)]
    public class Supplier : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "廠商代碼為必填")]
        [MaxLength(20, ErrorMessage = "廠商代碼不可超過20個字元")]
        [Display(Name = "廠商代碼")]
        public string SupplierCode { get; set; } = string.Empty;
        
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
        [Display(Name = "廠商類型")]
        [ForeignKey(nameof(SupplierType))]
        public int? SupplierTypeId { get; set; }
        
        // Navigation Properties
        public SupplierType? SupplierType { get; set; }
        
        // 聯絡資訊請使用 IContactService 取得 (OwnerType = "Supplier", OwnerId = this.Id)
        // 地址資訊請使用 IAddressService 取得
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
        public ICollection<Product> PrimaryProducts { get; set; } = new List<Product>();
    }
}