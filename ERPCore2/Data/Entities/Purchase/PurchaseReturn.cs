using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers.EditModal;

// 使用別名來避免命名衝突
using EntityPurchaseReturnReason = ERPCore2.Data.Entities.PurchaseReturnReason;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購退回主檔實體 - 記錄採購退回基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(SupplierId), nameof(ReturnDate))]
    [CodeGenerationStrategy(
        CodeGenerationStrategy.TimestampWithSequence,
        Prefix = "PRT",
        DateFieldName = nameof(ReturnDate),
        SequenceDigits = 4
    )]
    public class PurchaseReturn : BaseEntity
    {
        [Required(ErrorMessage = "退回日期為必填")]
        [Display(Name = "退回日期")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "稅率算法為必填")]
        [Display(Name = "稅率算法")]
        public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;

        [Display(Name = "退回總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmount { get; set; } = 0;

        [Display(Name = "退回稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnTaxAmount { get; set; } = 0;

        [Display(Name = "退回含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmountWithTax => TotalReturnAmount + ReturnTaxAmount;

        [Display(Name = "已轉傳票")]
        public bool IsJournalized { get; set; } = false;

        [Display(Name = "轉傳票時間")]
        public DateTime? JournalizedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Display(Name = "退出原因")]
        [ForeignKey(nameof(ReturnReason))]
        public int? ReturnReasonId { get; set; }

        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public EntityPurchaseReturnReason? ReturnReason { get; set; }
        public ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();
    }
}
