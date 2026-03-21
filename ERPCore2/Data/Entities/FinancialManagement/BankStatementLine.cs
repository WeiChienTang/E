using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銀行對帳單明細行 - 對帳單上的每一筆交易記錄
    /// 可與傳票分錄（JournalEntryLine）手動配對，標記為已對帳。
    /// </summary>
    [Index(nameof(BankStatementId))]
    [Index(nameof(TransactionDate))]
    [Index(nameof(IsMatched))]
    public class BankStatementLine : BaseEntity
    {
        /// <summary>所屬對帳單 ID</summary>
        [Required]
        [Display(Name = "對帳單")]
        public int BankStatementId { get; set; }

        /// <summary>所屬對帳單導航屬性</summary>
        public BankStatement BankStatement { get; set; } = null!;

        /// <summary>交易日期</summary>
        [Required(ErrorMessage = "請輸入交易日期")]
        [Display(Name = "交易日期")]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        /// <summary>交易說明</summary>
        [Required(ErrorMessage = "請輸入交易說明")]
        [MaxLength(200, ErrorMessage = "交易說明不可超過200個字元")]
        [Display(Name = "交易說明")]
        public string Description { get; set; } = string.Empty;

        /// <summary>支出金額（扣款，從帳戶流出）</summary>
        [Display(Name = "支出")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; } = 0;

        /// <summary>收入金額（存入，流入帳戶）</summary>
        [Display(Name = "收入")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; } = 0;

        /// <summary>是否已配對傳票分錄</summary>
        [Display(Name = "已配對")]
        public bool IsMatched { get; set; } = false;

        /// <summary>配對的傳票分錄 ID（手動指定）</summary>
        [Display(Name = "配對分錄")]
        public int? MatchedJournalEntryLineId { get; set; }

        /// <summary>配對的傳票分錄導航屬性</summary>
        public JournalEntryLine? MatchedJournalEntryLine { get; set; }

        /// <summary>
        /// 銀行流水號（從 CSV 匯入時帶入，供未來自動配對使用）
        /// 各銀行格式不同，如 TXN001、20260321-0001 等
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "銀行流水號")]
        public string? BankReference { get; set; }

        /// <summary>排序序號</summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;

        // ===== 計算屬性（NotMapped）=====

        /// <summary>淨金額（收入 - 支出，正數 = 淨收入）</summary>
        [NotMapped]
        public decimal NetAmount => CreditAmount - DebitAmount;
    }
}
