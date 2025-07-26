using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨退回主檔實體 - 記錄銷貨退回基本資訊
    /// </summary>
    [Index(nameof(SalesReturnNumber), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(ReturnDate))]
    [Index(nameof(ReturnStatus), nameof(ReturnDate))]
    [Index(nameof(SalesOrderId), nameof(ReturnDate))]
    public class SalesReturn : BaseEntity
    {
        [Required(ErrorMessage = "退回單號為必填")]
        [MaxLength(30, ErrorMessage = "退回單號不可超過30個字元")]
        [Display(Name = "退回單號")]
        public string SalesReturnNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "退回日期為必填")]
        [Display(Name = "退回日期")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [Display(Name = "預計處理日期")]
        public DateTime? ExpectedProcessDate { get; set; }

        [Display(Name = "實際處理日期")]
        public DateTime? ActualProcessDate { get; set; }

        [Required(ErrorMessage = "退回狀態為必填")]
        [Display(Name = "退回狀態")]
        public SalesReturnStatus ReturnStatus { get; set; } = SalesReturnStatus.Draft;

        [Required(ErrorMessage = "退回原因為必填")]
        [Display(Name = "退回原因")]
        public SalesReturnReason ReturnReason { get; set; } = SalesReturnReason.CustomerRequest;

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

        [Display(Name = "是否已退款")]
        public bool IsRefunded { get; set; } = false;

        [Display(Name = "退款日期")]
        public DateTime? RefundDate { get; set; }

        [Display(Name = "退款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0;

        [MaxLength(200, ErrorMessage = "退款備註不可超過200個字元")]
        [Display(Name = "退款備註")]
        public string? RefundRemarks { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [Display(Name = "原始銷貨訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int? SalesOrderId { get; set; }

        [Display(Name = "原始銷貨出貨單")]
        [ForeignKey(nameof(SalesDelivery))]
        public int? SalesDeliveryId { get; set; }

        [Display(Name = "處理員工")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; } = null!;
        public SalesOrder? SalesOrder { get; set; }
        public SalesDelivery? SalesDelivery { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<SalesReturnDetail> SalesReturnDetails { get; set; } = new List<SalesReturnDetail>();
    }
}
