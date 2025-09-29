using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 財務交易記錄實體 - 記錄所有財務交易的歷史
    /// </summary>
    [Index(nameof(TransactionNumber))]
    [Index(nameof(TransactionType), nameof(TransactionDate))]
    [Index(nameof(CustomerId), nameof(TransactionDate))]
    [Index(nameof(VendorId), nameof(TransactionDate))]
    [Index(nameof(CompanyId), nameof(TransactionDate))]
    [Index(nameof(SourceDocumentType), nameof(SourceDocumentId))]
    public class FinancialTransaction : BaseEntity
    {
        /// <summary>
        /// 交易單號 - 財務交易的唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "交易單號為必填")]
        [MaxLength(50, ErrorMessage = "交易單號不可超過50個字元")]
        [Display(Name = "交易單號")]
        public string TransactionNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易類型 - 定義財務交易的種類
        /// </summary>
        [Required(ErrorMessage = "交易類型為必填")]
        [Display(Name = "交易類型")]
        public FinancialTransactionTypeEnum TransactionType { get; set; }
        
        /// <summary>
        /// 交易日期 - 實際發生交易的日期
        /// </summary>
        [Required(ErrorMessage = "交易日期為必填")]
        [Display(Name = "交易日期")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 交易金額 - 本次交易的金額
        /// </summary>
        [Required(ErrorMessage = "交易金額為必填")]
        [Display(Name = "交易金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 交易描述 - 交易的詳細說明
        /// </summary>
        [MaxLength(500, ErrorMessage = "交易描述不可超過500個字元")]
        [Display(Name = "交易描述")]
        public string? Description { get; set; }
        
        // === 關聯方資訊 ===
        
        /// <summary>
        /// 客戶ID - 應收相關交易的客戶
        /// </summary>
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }
        
        /// <summary>
        /// 供應商ID - 應付相關交易的供應商 (未來實作)
        /// </summary>
        [Display(Name = "供應商")]
        public int? VendorId { get; set; }
        
        /// <summary>
        /// 公司ID - 交易所屬的公司
        /// </summary>
        [Required(ErrorMessage = "公司為必填")]
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }
        
        // === 來源單據資訊 ===
        
        /// <summary>
        /// 來源單據類型 - 產生此交易的單據類型
        /// </summary>
        [MaxLength(50, ErrorMessage = "來源單據類型不可超過50個字元")]
        [Display(Name = "來源單據類型")]
        public string? SourceDocumentType { get; set; }
        
        /// <summary>
        /// 來源單據ID - 產生此交易的單據ID
        /// </summary>
        [Display(Name = "來源單據ID")]
        public int? SourceDocumentId { get; set; }
        
        /// <summary>
        /// 來源單據號碼 - 產生此交易的單據號碼
        /// </summary>
        [MaxLength(50, ErrorMessage = "來源單據號碼不可超過50個字元")]
        [Display(Name = "來源單據號碼")]
        public string? SourceDocumentNumber { get; set; }
        
        /// <summary>
        /// 來源明細ID - 產生此交易的具體明細項目ID（可選，用於明細級別的追蹤）
        /// 例如：SalesOrderDetail.Id, AccountsReceivableSetoffDetail.Id
        /// </summary>
        [Display(Name = "來源明細ID")]
        public int? SourceDetailId { get; set; }
        
        // === 收付款資訊 ===
        
        /// <summary>
        /// 收付款方式ID - 此交易使用的收付款方式
        /// </summary>
        [Display(Name = "收付款方式")]
        [ForeignKey(nameof(PaymentMethod))]
        public int? PaymentMethodId { get; set; }
        
        /// <summary>
        /// 收付款帳戶 - 收付款的帳戶資訊
        /// </summary>
        [MaxLength(100, ErrorMessage = "收付款帳戶不可超過100個字元")]
        [Display(Name = "收付款帳戶")]
        public string? PaymentAccount { get; set; }
        
        /// <summary>
        /// 參考號碼 - 支票號碼、匯款單號等參考資訊
        /// </summary>
        [MaxLength(100, ErrorMessage = "參考號碼不可超過100個字元")]
        [Display(Name = "參考號碼")]
        public string? ReferenceNumber { get; set; }
        
        // === 餘額追蹤 ===
        
        /// <summary>
        /// 交易前餘額 - 執行此交易前的帳戶餘額
        /// </summary>
        [Display(Name = "交易前餘額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }
        
        /// <summary>
        /// 交易後餘額 - 執行此交易後的帳戶餘額
        /// </summary>
        [Display(Name = "交易後餘額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }
        
        // === 沖銷相關 ===
        
        /// <summary>
        /// 是否已沖銷 - 標記此交易是否已被沖銷
        /// </summary>
        [Display(Name = "是否已沖銷")]
        public bool IsReversed { get; set; } = false;
        
        /// <summary>
        /// 沖銷日期 - 交易被沖銷的日期
        /// </summary>
        [Display(Name = "沖銷日期")]
        public DateTime? ReversedDate { get; set; }
        
        /// <summary>
        /// 沖銷原因 - 交易被沖銷的原因說明
        /// </summary>
        [MaxLength(500, ErrorMessage = "沖銷原因不可超過500個字元")]
        [Display(Name = "沖銷原因")]
        public string? ReversalReason { get; set; }
        
        /// <summary>
        /// 沖銷交易ID - 指向沖銷此交易的另一筆交易
        /// </summary>
        [Display(Name = "沖銷交易ID")]
        [ForeignKey(nameof(ReversalTransaction))]
        public int? ReversalTransactionId { get; set; }
        
        // === 外幣相關 ===
        
        /// <summary>
        /// 原幣金額 - 外幣交易的原始金額
        /// </summary>
        [Display(Name = "原幣金額")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? OriginalAmount { get; set; }
        
        /// <summary>
        /// 貨幣代碼 - ISO 4217 貨幣代碼 (如 USD, EUR)
        /// </summary>
        [MaxLength(3, ErrorMessage = "貨幣代碼不可超過3個字元")]
        [Display(Name = "貨幣代碼")]
        public string? CurrencyCode { get; set; }
        
        /// <summary>
        /// 匯率 - 外幣兌換本幣的匯率
        /// </summary>
        [Display(Name = "匯率")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal? ExchangeRate { get; set; }
        
        // Navigation Properties
        
        /// <summary>
        /// 客戶導航屬性
        /// </summary>
        public Customer? Customer { get; set; }
        
        /// <summary>
        /// 公司導航屬性
        /// </summary>
        public Company Company { get; set; } = null!;
        
        /// <summary>
        /// 收付款方式導航屬性
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }
        
        /// <summary>
        /// 沖銷交易導航屬性
        /// </summary>
        public FinancialTransaction? ReversalTransaction { get; set; }
        
        /// <summary>
        /// 被此交易沖銷的交易集合
        /// </summary>
        public ICollection<FinancialTransaction> ReversedTransactions { get; set; } = new List<FinancialTransaction>();
        
        // === 計算屬性 ===
        
        /// <summary>
        /// 是否為收入類型交易
        /// </summary>
        [NotMapped]
        public bool IsIncomeTransaction => TransactionType switch
        {
            FinancialTransactionTypeEnum.AccountsReceivableSetoff => true,
            FinancialTransactionTypeEnum.AccountsReceivableDiscount => true,
            FinancialTransactionTypeEnum.CashReceipt => true,
            FinancialTransactionTypeEnum.BankReceipt => true,
            FinancialTransactionTypeEnum.CheckReceipt => true,
            FinancialTransactionTypeEnum.FinancialIncome => true,
            _ => false
        };
        
        /// <summary>
        /// 是否為支出類型交易
        /// </summary>
        [NotMapped]
        public bool IsExpenseTransaction => TransactionType switch
        {
            FinancialTransactionTypeEnum.AccountsPayableSetoff => true,
            FinancialTransactionTypeEnum.AccountsPayableAdvance => true,
            FinancialTransactionTypeEnum.CashPayment => true,
            FinancialTransactionTypeEnum.BankPayment => true,
            FinancialTransactionTypeEnum.CheckPayment => true,
            FinancialTransactionTypeEnum.FinancialExpense => true,
            _ => false
        };
    }
}