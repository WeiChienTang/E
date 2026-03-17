using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities.Systems
{
    /// <summary>公司銀行帳號 - 記錄公司在各銀行的帳戶資訊（收款、付款、薪資轉帳等）</summary>
    public class CompanyBankAccount : BaseEntity
    {
        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

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

        /// <summary>帳戶用途說明（如：薪資轉帳、一般收付款、營業稅）</summary>
        [MaxLength(100)]
        [Display(Name = "用途說明")]
        public string? Purpose { get; set; }

        /// <summary>主要帳號（同一公司只能有一個）</summary>
        [Display(Name = "主要帳號")]
        public bool IsPrimary { get; set; } = true;
    }
}
