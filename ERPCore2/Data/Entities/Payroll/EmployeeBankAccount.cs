using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>員工銀行帳號（薪資轉帳用）</summary>
    public class EmployeeBankAccount : BaseEntity
    {
        [Required] public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        /// <summary>所屬銀行（銀行主檔）</summary>
        [Required]
        [Display(Name = "銀行")]
        public int BankId { get; set; }
        public Bank Bank { get; set; } = null!;

        /// <summary>分行代號</summary>
        [MaxLength(10)]
        [Display(Name = "分行代號")]
        public string? BranchCode { get; set; }

        /// <summary>分行名稱</summary>
        [MaxLength(50)]
        [Display(Name = "分行名稱")]
        public string? BranchName { get; set; }

        /// <summary>帳號</summary>
        [Required]
        [MaxLength(30)]
        [Display(Name = "帳號")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>戶名</summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "戶名")]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>主要轉帳帳號（同一員工只能有一個主帳號）</summary>
        [Display(Name = "主要帳號")]
        public bool IsPrimary { get; set; } = true;
    }
}
