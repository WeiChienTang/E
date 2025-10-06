using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購退回主檔實體 - 記錄採購退回基本資訊
    /// </summary>
    [Index(nameof(PurchaseReturnNumber), IsUnique = true)]
    [Index(nameof(SupplierId), nameof(ReturnDate))]
    public class PurchaseReturn : BaseEntity
    {
        [Required(ErrorMessage = "退回單號為必填")]
        [MaxLength(30, ErrorMessage = "退回單號不可超過30個字元")]
        [Display(Name = "退回單號")]
        public string PurchaseReturnNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "退回日期為必填")]
        [Display(Name = "退回日期")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [Display(Name = "退回總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmount { get; set; } = 0;

        [Display(Name = "退回稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnTaxAmount { get; set; } = 0;

        [Display(Name = "退回含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmountWithTax => TotalReturnAmount + ReturnTaxAmount;

        // Foreign Keys
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Display(Name = "原始採購進貨單")]
        [ForeignKey(nameof(PurchaseReceiving))]
        public int? PurchaseReceivingId { get; set; }

        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public PurchaseReceiving? PurchaseReceiving { get; set; }
        public ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();
    }
}
