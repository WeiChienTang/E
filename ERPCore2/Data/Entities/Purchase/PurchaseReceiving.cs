using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購進貨單主檔實體 - 記錄採購進貨作業
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(SupplierId), nameof(ReceiptDate))]
    [CodeGenerationStrategy(
        CodeGenerationStrategy.TimestampWithSequence,
        Prefix = "PR",
        DateFieldName = nameof(ReceiptDate),
        SequenceDigits = 4
    )]
    public class PurchaseReceiving : BaseEntity
    {
        [Required(ErrorMessage = "進貨日期為必填")]
        [Display(Name = "進貨日期")]
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [Display(Name = "進貨總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "進貨稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseReceivingTaxAmount { get; set; } = 0;

        [Display(Name = "進貨含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseReceivingTotalAmountIncludingTax => TotalAmount + PurchaseReceivingTaxAmount;

        [Required(ErrorMessage = "稅率算法為必填")]
        [Display(Name = "稅率算法")]
        public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;

        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }

        [Display(Name = "已轉傳票")]
        public bool IsJournalized { get; set; } = false;

        [Display(Name = "轉傳票時間")]
        public DateTime? JournalizedAt { get; set; }

        // ===== 審核欄位 =====
        [Display(Name = "是否核准")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "核准人員")]
        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedBy { get; set; }

        [Display(Name = "核准時間")]
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(100, ErrorMessage = "駁回原因不可超過100個字元")]
        [Display(Name = "駁回原因")]
        public string? RejectReason { get; set; }

        [NotMapped]
        public string ApprovalStatusText =>
            IsApproved ? "已核准" :
            !string.IsNullOrEmpty(RejectReason) ? "已駁回" : "待審核";

        [NotMapped]
        public string? ApprovedAtText => ApprovedAt?.ToString("yyyy-MM-dd HH:mm");

        [NotMapped]
        public string ApprovedByDisplayName =>
            IsApproved ? (ApprovedByUser?.Name ?? "系統自動審核") :
            !string.IsNullOrEmpty(RejectReason) ? (ApprovedByUser?.Name ?? "") : "";

        // Foreign Keys
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int? SupplierId { get; set; }

        // Navigation Properties
        public Supplier? Supplier { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public ICollection<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; } = new List<PurchaseReceivingDetail>();
    }
}
