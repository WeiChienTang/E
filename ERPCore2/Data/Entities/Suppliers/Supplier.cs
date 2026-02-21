using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廠商實體 - 定義廠商基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Supplier : BaseEntity
    {
        
        [Required(ErrorMessage = "公司名稱為必填")]
        [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;
        
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(8, ErrorMessage = "統一編號不可超過8個字元")]
        [Display(Name = "統一編號")]
        public string? TaxNumber { get; set; }
         [MaxLength(50, ErrorMessage = "負責人不可超過50個字元")]
        [Display(Name = "負責人")]
        public string? ResponsiblePerson { get; set; }

        [MaxLength(20, ErrorMessage = "公司聯絡電話不可超過20個字元")]
        [Display(Name = "公司聯絡電話")]
        public string? SupplierContactPhone { get; set; }
        
        [MaxLength(20, ErrorMessage = "聯絡電話不可超過20個字元")]
        [Display(Name = "聯絡電話")]
        public string? ContactPhone { get; set; }
        
        [MaxLength(20, ErrorMessage = "行動電話不可超過20個字元")]
        [Display(Name = "行動電話")]
        public string? MobilePhone { get; set; }
        
        [MaxLength(200, ErrorMessage = "聯絡地址不可超過200個字元")]
        [Display(Name = "聯絡地址")]
        public string? ContactAddress { get; set; }
        
        [MaxLength(200, ErrorMessage = "公司地址不可超過200個字元")]
        [Display(Name = "公司地址")]
        public string? SupplierAddress { get; set; }
        
        [MaxLength(100, ErrorMessage = "公司網址不可超過100個字元")]
        [Display(Name = "公司網址")]
        public string? Website { get; set; }
        
        [MaxLength(100, ErrorMessage = "信箱不可超過100個字元")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        [Display(Name = "信箱")]
        public string? Email { get; set; }
        
        [MaxLength(20, ErrorMessage = "傳真不可超過20個字元")]
        [Display(Name = "傳真")]
        public string? Fax { get; set; }
        
        [MaxLength(50, ErrorMessage = "職稱不可超過50個字元")]
        [Display(Name = "職稱")]
        public string? JobTitle { get; set; }
        
        [Display(Name = "付款方式")]
        public int? PaymentMethodId { get; set; }
        
        public PaymentMethod? PaymentMethod { get; set; }
        
        [MaxLength(100, ErrorMessage = "付款條件不可超過100個字元")]
        [Display(Name = "付款條件")]
        public string? PaymentTerms { get; set; }
        
        // 聯絡資訊請使用 IContactService 取得 (OwnerType = "Supplier", OwnerId = this.Id)
        // 地址資訊請使用 IAddressService 取得
        
        // Navigation Properties
        /// <summary>
        /// 供應商品列表（商品-供應商綁定）
        /// </summary>
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();

        /// <summary>
        /// 所屬車輛列表
        /// </summary>
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
