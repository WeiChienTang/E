using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Entities.Customers;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Data.Entities.Suppliers;
using ERPCore2.Data.Entities.Systems;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銀行別 - 台灣金融機構代碼對照表（機構層級，非分行層級）
    /// Code 欄位存放金融機構代碼（3碼，如 004=台灣銀行）
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

        [MaxLength(20, ErrorMessage = "SWIFT編號不可超過20個字元")]
        [Display(Name = "SWIFT編號")]
        public string? SwiftCode { get; set; }

        // Navigation Properties
        public ICollection<CustomerBankAccount> CustomerBankAccounts { get; set; } = new List<CustomerBankAccount>();
        public ICollection<SupplierBankAccount> SupplierBankAccounts { get; set; } = new List<SupplierBankAccount>();
        public ICollection<CompanyBankAccount> CompanyBankAccounts { get; set; } = new List<CompanyBankAccount>();
        public ICollection<EmployeeBankAccount> EmployeeBankAccounts { get; set; } = new List<EmployeeBankAccount>();
    }
}
