using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 應收帳款沖款明細實體 - 記錄每筆沖款對應的銷貨或退回明細
    /// </summary>
    [Index(nameof(SetoffId))]
    [Index(nameof(SalesOrderDetailId))]
    [Index(nameof(SalesReturnDetailId))]
    public class AccountsReceivableSetoffDetail : BaseEntity
    {
        /// <summary>
        /// 沖款單ID
        /// </summary>
        [Required(ErrorMessage = "沖款單為必填")]
        [Display(Name = "沖款單")]
        [ForeignKey(nameof(Setoff))]
        public int SetoffId { get; set; }

        /// <summary>
        /// 銷貨訂單明細ID (二選一)
        /// </summary>
        [Display(Name = "銷貨訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int? SalesOrderDetailId { get; set; }

        /// <summary>
        /// 銷貨退回明細ID (二選一)
        /// </summary>
        [Display(Name = "銷貨退回明細")]
        [ForeignKey(nameof(SalesReturnDetail))]
        public int? SalesReturnDetailId { get; set; }

        /// <summary>
        /// 應收金額 (明細的應收總額)
        /// </summary>
        [Display(Name = "應收金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivableAmount { get; set; } = 0;

        /// <summary>
        /// 本次沖款金額
        /// </summary>
        [Required(ErrorMessage = "沖款金額為必填")]
        [Display(Name = "本次沖款金額")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "沖款金額必須大於 0")]
        public decimal SetoffAmount { get; set; } = 0;

        /// <summary>
        /// 沖款前累計收款金額
        /// </summary>
        [Display(Name = "沖款前累計收款")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousReceivedAmount { get; set; } = 0;

        /// <summary>
        /// 沖款後累計收款金額
        /// </summary>
        [Display(Name = "沖款後累計收款")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AfterReceivedAmount { get; set; } = 0;

        /// <summary>
        /// 剩餘應收金額
        /// </summary>
        [Display(Name = "剩餘應收金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; } = 0;

        /// <summary>
        /// 是否完全收款
        /// </summary>
        [Display(Name = "是否完全收款")]
        public bool IsFullyReceived { get; set; } = false;

        /// <summary>
        /// 單據類型 (SalesOrder/SalesReturn)
        /// </summary>
        [Required(ErrorMessage = "單據類型為必填")]
        [MaxLength(20, ErrorMessage = "單據類型不可超過20個字元")]
        [Display(Name = "單據類型")]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>
        /// 單據編號 (冗余欄位，便於查詢顯示)
        /// </summary>
        [MaxLength(50, ErrorMessage = "單據編號不可超過50個字元")]
        [Display(Name = "單據編號")]
        public string? DocumentNumber { get; set; }

        /// <summary>
        /// 商品名稱 (冗余欄位，便於顯示)
        /// </summary>
        [MaxLength(200, ErrorMessage = "商品名稱不可超過200個字元")]
        [Display(Name = "商品名稱")]
        public string? ProductName { get; set; }

        /// <summary>
        /// 數量 (冗余欄位，便於顯示)
        /// </summary>
        [Display(Name = "數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; } = 0;

        /// <summary>
        /// 單位名稱 (冗余欄位，便於顯示)
        /// </summary>
        [MaxLength(20, ErrorMessage = "單位名稱不可超過20個字元")]
        [Display(Name = "單位")]
        public string? UnitName { get; set; }

        // Navigation Properties
        /// <summary>
        /// 沖款單導航屬性
        /// </summary>
        public AccountsReceivableSetoff Setoff { get; set; } = null!;

        /// <summary>
        /// 銷貨訂單明細導航屬性
        /// </summary>
        public SalesOrderDetail? SalesOrderDetail { get; set; }

        /// <summary>
        /// 銷貨退回明細導航屬性
        /// </summary>
        public SalesReturnDetail? SalesReturnDetail { get; set; }

        /// <summary>
        /// 取得關聯的實體 ID (SalesOrderDetailId 或 SalesReturnDetailId)
        /// </summary>
        [NotMapped]
        public int RelatedDetailId => SalesOrderDetailId ?? SalesReturnDetailId ?? 0;

        /// <summary>
        /// 驗證明細資料的完整性
        /// </summary>
        public bool IsValid()
        {
            // 必須指定銷貨訂單明細或銷貨退回明細其中之一
            if (!SalesOrderDetailId.HasValue && !SalesReturnDetailId.HasValue)
                return false;

            // 不能同時指定兩者
            if (SalesOrderDetailId.HasValue && SalesReturnDetailId.HasValue)
                return false;

            // 沖款金額必須大於 0 且小於等於剩餘應收金額
            if (SetoffAmount <= 0 || SetoffAmount > (ReceivableAmount - PreviousReceivedAmount))
                return false;

            return true;
        }
    }
}