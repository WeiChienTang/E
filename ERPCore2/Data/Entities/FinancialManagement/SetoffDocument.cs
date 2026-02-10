using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 沖款單實體 - 統一處理應收/應付帳款沖款作業
    /// </summary>
    [Index(nameof(Code))]
    [Index(nameof(SetoffType))]
    [Index(nameof(RelatedPartyId))]
    [Index(nameof(CompanyId))]
    [Index(nameof(SetoffDate))]
    public class SetoffDocument : BaseEntity
    {
        /// <summary>
        /// 沖款類型（應收/應付）
        /// </summary>
        [Required(ErrorMessage = "沖款類型為必填")]
        [Display(Name = "沖款類型")]
        public SetoffType SetoffType { get; set; }

        /// <summary>
        /// 沖款日期
        /// </summary>
        [Required(ErrorMessage = "沖款日期為必填")]
        [Display(Name = "沖款日期")]
        public DateTime SetoffDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 關聯方ID (客戶或供應商)
        /// </summary>
        [Required(ErrorMessage = "關聯方為必填")]
        [Display(Name = "關聯方")]
        public int RelatedPartyId { get; set; }

        /// <summary>
        /// 關聯方類型 (Customer/Supplier)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string RelatedPartyType { get; set; } = string.Empty;

        /// <summary>
        /// 公司ID
        /// </summary>
        [Required(ErrorMessage = "公司為必填")]
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        /// <summary>
        /// 本期應收
        /// </summary>
        [Display(Name = "本期總應收/應付")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSetoffAmount { get; set; } = 0;

        /// <summary>
        /// 本期沖銷（本次沖款單所沖銷的金額）
        /// </summary>
        [Display(Name = "本期沖銷")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentSetoffAmount { get; set; } = 0;

        /// <summary>
        /// 本期總收款（實際收到的款項總額）
        /// </summary>
        [Display(Name = "本期總收款")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCollectionAmount { get; set; } = 0;

        /// <summary>
        /// 本期總折讓（折讓抵銷的總額）
        /// </summary>
        [Display(Name = "本期總折讓")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAllowanceAmount { get; set; } = 0;

        /// <summary>
        /// 本期預收/付（當使用者有預收的時候會顯示在此欄位）
        /// </summary>
        [Display(Name = "本期預收/付")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrepaymentAmount { get; set; } = 0;

        /// <summary>
        /// 預收/付沖款（使用預收/付的金額進行沖款）
        /// </summary>
        [Display(Name = "預收/付沖款")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrepaymentSetoffAmount { get; set; } = 0;

        // Navigation Properties
        /// <summary>
        /// 公司導航屬性
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// 關聯的財務交易記錄
        /// </summary>
        public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

        /// <summary>
        /// 關聯的收款記錄
        /// </summary>
        public ICollection<SetoffPayment> SetoffPayments { get; set; } = new List<SetoffPayment>();

        /// <summary>
        /// 關聯的商品明細
        /// </summary>
        public ICollection<SetoffProductDetail> SetoffProductDetails { get; set; } = new List<SetoffProductDetail>();

        /// <summary>
        /// 關聯的預收付款項記錄（建立的預收付款項）
        /// </summary>
        public ICollection<SetoffPrepayment> Prepayments { get; set; } = new List<SetoffPrepayment>();
        
        /// <summary>
        /// 關聯的預收付款項使用記錄（使用既有預收付款項的記錄）
        /// </summary>
        public ICollection<SetoffPrepaymentUsage> PrepaymentUsages { get; set; } = new List<SetoffPrepaymentUsage>();

        #region 計算屬性

        /// <summary>
        /// 是否為應收帳款沖款
        /// </summary>
        [NotMapped]
        public bool IsAccountsReceivable => SetoffType == SetoffType.AccountsReceivable;

        /// <summary>
        /// 是否為應付帳款沖款
        /// </summary>
        [NotMapped]
        public bool IsAccountsPayable => SetoffType == SetoffType.AccountsPayable;

        /// <summary>
        /// 關聯方名稱(需要在查詢時載入)
        /// </summary>
        [NotMapped]
        [Display(Name = "關聯方名稱")]
        public string RelatedPartyName { get; set; } = string.Empty;

        /// <summary>
        /// 客戶ID (虛擬屬性,用於表單綁定)
        /// </summary>
        [NotMapped]
        public int? CustomerId
        {
            get => RelatedPartyType == "Customer" ? (RelatedPartyId > 0 ? RelatedPartyId : null) : null;
            set
            {
                if (value.HasValue && value.Value > 0)
                {
                    RelatedPartyId = value.Value;
                    RelatedPartyType = "Customer";
                }
            }
        }

        /// <summary>
        /// 廠商ID (虛擬屬性,用於表單綁定)
        /// </summary>
        [NotMapped]
        public int? SupplierId
        {
            get => RelatedPartyType == "Supplier" ? (RelatedPartyId > 0 ? RelatedPartyId : null) : null;
            set
            {
                if (value.HasValue && value.Value > 0)
                {
                    RelatedPartyId = value.Value;
                    RelatedPartyType = "Supplier";
                }
            }
        }

        #endregion
    }
}
