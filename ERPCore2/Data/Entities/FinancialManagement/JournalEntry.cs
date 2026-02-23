using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 傳票主檔 - 記錄每一筆會計分錄的主要資訊
    /// 來源可為業務單據自動產生（如進貨單、銷貨單、沖款單），或由會計人員手動輸入
    /// 傳票必須借貸平衡（TotalDebitAmount == TotalCreditAmount）才能過帳
    /// </summary>
    [Index(nameof(EntryDate))]
    [Index(nameof(FiscalYear), nameof(FiscalPeriod))]
    [Index(nameof(EntryType))]
    [Index(nameof(JournalEntryStatus))]
    [Index(nameof(CompanyId))]
    [Index(nameof(SourceDocumentType), nameof(SourceDocumentId))]
    public class JournalEntry : BaseEntity
    {
        /// <summary>
        /// 傳票日期
        /// </summary>
        [Required(ErrorMessage = "傳票日期為必填")]
        [Display(Name = "傳票日期")]
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// 傳票類型（自動產生/手動輸入/調整分錄/結帳分錄/沖銷分錄）
        /// </summary>
        [Required(ErrorMessage = "傳票類型為必填")]
        [Display(Name = "傳票類型")]
        public JournalEntryType EntryType { get; set; } = JournalEntryType.Manual;

        /// <summary>
        /// 傳票狀態（草稿/已過帳/已取消/已沖銷）
        /// 只有「已過帳」的傳票才會影響科目餘額
        /// </summary>
        [Required(ErrorMessage = "傳票狀態為必填")]
        [Display(Name = "傳票狀態")]
        public JournalEntryStatus JournalEntryStatus { get; set; } = JournalEntryStatus.Draft;

        /// <summary>
        /// 傳票說明
        /// </summary>
        [MaxLength(500, ErrorMessage = "傳票說明不可超過500個字元")]
        [Display(Name = "傳票說明")]
        public string? Description { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        [Required(ErrorMessage = "公司為必填")]
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        /// <summary>
        /// 會計年度（如 2026）
        /// </summary>
        [Required(ErrorMessage = "會計年度為必填")]
        [Display(Name = "會計年度")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// 會計期間（月份 1~12）
        /// </summary>
        [Required(ErrorMessage = "會計期間為必填")]
        [Range(1, 12, ErrorMessage = "會計期間必須介於 1 到 12 之間")]
        [Display(Name = "會計期間")]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// 來源單據類型（如 "SetoffDocument", "PurchaseReceiving", "SalesDelivery"）
        /// 手動傳票此欄位為 null
        /// </summary>
        [MaxLength(100, ErrorMessage = "來源單據類型不可超過100個字元")]
        [Display(Name = "來源單據類型")]
        public string? SourceDocumentType { get; set; }

        /// <summary>
        /// 來源單據ID - 對應 SourceDocumentType 所指資料表的主鍵
        /// </summary>
        [Display(Name = "來源單據ID")]
        public int? SourceDocumentId { get; set; }

        /// <summary>
        /// 來源單號 - 方便查詢顯示，不需 JOIN 即可知道來源單據號碼
        /// </summary>
        [MaxLength(50, ErrorMessage = "來源單號不可超過50個字元")]
        [Display(Name = "來源單號")]
        public string? SourceDocumentCode { get; set; }

        /// <summary>
        /// 借方合計金額
        /// </summary>
        [Display(Name = "借方合計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebitAmount { get; set; } = 0;

        /// <summary>
        /// 貸方合計金額
        /// </summary>
        [Display(Name = "貸方合計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCreditAmount { get; set; } = 0;

        /// <summary>
        /// 是否已沖銷
        /// </summary>
        [Display(Name = "已沖銷")]
        public bool IsReversed { get; set; } = false;

        /// <summary>
        /// 沖銷傳票ID - 若此傳票已被沖銷，指向沖銷用的傳票
        /// </summary>
        [Display(Name = "沖銷傳票")]
        [ForeignKey(nameof(ReversalEntry))]
        public int? ReversalEntryId { get; set; }

        // Navigation Properties

        /// <summary>
        /// 公司導航屬性
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// 沖銷傳票導航屬性
        /// </summary>
        public JournalEntry? ReversalEntry { get; set; }

        /// <summary>
        /// 傳票分錄集合
        /// </summary>
        public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();

        // Computed Properties (NotMapped)

        /// <summary>
        /// 借貸是否平衡
        /// </summary>
        [NotMapped]
        public bool IsBalanced => TotalDebitAmount == TotalCreditAmount && TotalDebitAmount > 0;

        /// <summary>
        /// 差額（借方 - 貸方，平衡時為 0）
        /// </summary>
        [NotMapped]
        public decimal Difference => TotalDebitAmount - TotalCreditAmount;
    }
}
