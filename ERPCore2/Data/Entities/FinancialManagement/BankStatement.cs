using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Entities.Systems;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銀行對帳單主檔 - 記錄銀行存款帳戶的對帳資訊（P4-C 銀行對帳）
    /// 將銀行存款科目的帳面餘額與銀行寄送的對帳單進行核對，找出未入帳項目和差異。
    /// </summary>
    [Index(nameof(CompanyId))]
    [Index(nameof(CompanyBankAccountId))]
    [Index(nameof(StatementDate))]
    [Index(nameof(Status))]
    public class BankStatement : BaseEntity
    {
        /// <summary>公司 ID</summary>
        [Required(ErrorMessage = "請選擇公司")]
        [Display(Name = "公司")]
        public int CompanyId { get; set; }

        /// <summary>公司導航屬性</summary>
        public Company Company { get; set; } = null!;

        /// <summary>公司銀行帳號 ID（對應 CompanyBankAccount）</summary>
        [Required(ErrorMessage = "請選擇銀行帳號")]
        [Display(Name = "銀行帳號")]
        public int CompanyBankAccountId { get; set; }

        /// <summary>公司銀行帳號導航屬性</summary>
        public CompanyBankAccount CompanyBankAccount { get; set; } = null!;

        /// <summary>對帳單日期（通常為月底）</summary>
        [Required(ErrorMessage = "請輸入對帳單日期")]
        [Display(Name = "對帳單日期")]
        public DateTime StatementDate { get; set; } = DateTime.Today;

        /// <summary>對帳期間起始日</summary>
        [Required(ErrorMessage = "請輸入對帳期間起始日")]
        [Display(Name = "期間起始")]
        public DateTime PeriodStart { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        /// <summary>對帳期間結束日</summary>
        [Required(ErrorMessage = "請輸入對帳期間結束日")]
        [Display(Name = "期間結束")]
        public DateTime PeriodEnd { get; set; } = DateTime.Today;

        /// <summary>銀行對帳單期初餘額</summary>
        [Display(Name = "期初餘額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0;

        /// <summary>銀行對帳單期末餘額</summary>
        [Display(Name = "期末餘額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ClosingBalance { get; set; } = 0;

        /// <summary>對帳單狀態（草稿 / 對帳中 / 已完成）</summary>
        [Required]
        [Display(Name = "對帳狀態")]
        public BankStatementStatus StatementStatus { get; set; } = BankStatementStatus.Draft;

        // ===== 導航屬性 =====

        /// <summary>對帳單明細行集合</summary>
        public ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();

        // ===== 計算屬性（NotMapped）=====

        /// <summary>明細行總數</summary>
        [NotMapped]
        public int TotalLines => Lines?.Count ?? 0;

        /// <summary>已配對行數</summary>
        [NotMapped]
        public int MatchedLines => Lines?.Count(l => l.IsMatched) ?? 0;

        /// <summary>對帳差異（期末餘額 - 銀行流水合計）</summary>
        [NotMapped]
        public decimal Difference =>
            ClosingBalance - OpeningBalance - (Lines?.Sum(l => l.CreditAmount - l.DebitAmount) ?? 0);
    }
}
