using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銀行別 - 管理銀行基本資料
    /// </summary>
    public class Bank : BaseEntity
    {
        [Required(ErrorMessage = "銀行名稱為必填")]
        [MaxLength(100, ErrorMessage = "銀行名稱不可超過100個字元")]
        [Display(Name = "銀行名稱")]
        public string BankName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "英文名稱不可超過100個字元")]
        [Display(Name = "英文名稱")]
        public string? BankNameEn { get; set; }

        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "電話不可超過50個字元")]
        [Display(Name = "電話")]
        public string? Phone { get; set; }

        [MaxLength(50, ErrorMessage = "傳真不可超過50個字元")]
        [Display(Name = "傳真")]
        public string? Fax { get; set; }

        [MaxLength(20, ErrorMessage = "SWIFT編號不可超過20個字元")]
        [Display(Name = "SWIFT編號")]
        public string? SwiftCode { get; set; }
    }
}
