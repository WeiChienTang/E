using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 沖款單實體 - 統一處理應收/應付帳款沖款作業
    /// </summary>
    [Index(nameof(SetoffNumber))]
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
        /// 沖款單號
        /// </summary>
        [Required(ErrorMessage = "沖款單號為必填")]
        [MaxLength(50, ErrorMessage = "沖款單號不可超過50個字元")]
        [Display(Name = "沖款單號")]
        public string SetoffNumber { get; set; } = string.Empty;

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
        /// 總沖款金額
        /// </summary>
        [Display(Name = "總沖款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSetoffAmount { get; set; } = 0;

        /// <summary>
        /// 完成日期
        /// </summary>
        [Display(Name = "完成日期")]
        public DateTime? CompletedDate { get; set; }

        // Navigation Properties
        /// <summary>
        /// 公司導航屬性
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// 關聯的財務交易記錄
        /// </summary>
        public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

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
