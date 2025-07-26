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
    [Index(nameof(ReturnStatus), nameof(ReturnDate))]
    [Index(nameof(PurchaseOrderId), nameof(ReturnDate))]
    public class PurchaseReturn : BaseEntity
    {
        [Required(ErrorMessage = "退回單號為必填")]
        [MaxLength(30, ErrorMessage = "退回單號不可超過30個字元")]
        [Display(Name = "退回單號")]
        public string PurchaseReturnNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "退回日期為必填")]
        [Display(Name = "退回日期")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [Display(Name = "預計處理日期")]
        public DateTime? ExpectedProcessDate { get; set; }

        [Display(Name = "實際處理日期")]
        public DateTime? ActualProcessDate { get; set; }

        [Required(ErrorMessage = "退回狀態為必填")]
        [Display(Name = "退回狀態")]
        public PurchaseReturnStatus ReturnStatus { get; set; } = PurchaseReturnStatus.Draft;

        [Required(ErrorMessage = "退回原因為必填")]
        [Display(Name = "退回原因")]
        public PurchaseReturnReason ReturnReason { get; set; } = PurchaseReturnReason.QualityIssue;

        [MaxLength(100, ErrorMessage = "處理人員不可超過100個字元")]
        [Display(Name = "處理人員")]
        public string? ProcessPersonnel { get; set; }

        [MaxLength(500, ErrorMessage = "退回說明不可超過500個字元")]
        [Display(Name = "退回說明")]
        public string? ReturnDescription { get; set; }

        [MaxLength(500, ErrorMessage = "處理備註不可超過500個字元")]
        [Display(Name = "處理備註")]
        public string? ProcessRemarks { get; set; }

        [Display(Name = "退回總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmount { get; set; } = 0;

        [Display(Name = "退回稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnTaxAmount { get; set; } = 0;

        [Display(Name = "退回含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmountWithTax { get; set; } = 0;

        [Display(Name = "是否已收到退款")]
        public bool IsRefunded { get; set; } = false;

        [Display(Name = "退款日期")]
        public DateTime? RefundDate { get; set; }

        [Display(Name = "退款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0;

        [MaxLength(200, ErrorMessage = "退款備註不可超過200個字元")]
        [Display(Name = "退款備註")]
        public string? RefundRemarks { get; set; }

        [Display(Name = "確認時間")]
        public DateTime? ConfirmedAt { get; set; }

        [Display(Name = "處理完成時間")]
        public DateTime? ProcessCompletedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Display(Name = "原始採購訂單")]
        [ForeignKey(nameof(PurchaseOrder))]
        public int? PurchaseOrderId { get; set; }

        [Display(Name = "原始採購進貨單")]
        [ForeignKey(nameof(PurchaseReceiving))]
        public int? PurchaseReceivingId { get; set; }

        [Display(Name = "處理員工")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        [Display(Name = "退貨倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        [Display(Name = "確認人員")]
        [ForeignKey(nameof(ConfirmedByUser))]
        public int? ConfirmedBy { get; set; }

        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public PurchaseOrder? PurchaseOrder { get; set; }
        public PurchaseReceiving? PurchaseReceiving { get; set; }
        public Employee? Employee { get; set; }
        public Warehouse? Warehouse { get; set; }
        public Employee? ConfirmedByUser { get; set; }
        public ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();
    }
}
