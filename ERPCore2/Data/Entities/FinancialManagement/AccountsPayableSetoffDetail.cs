using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 應付帳款沖款明細實體 - 記錄每筆沖款對應的採購或退回明細
    /// </summary>
    [Index(nameof(SetoffId))]
    [Index(nameof(PurchaseOrderDetailId))]
    [Index(nameof(PurchaseReturnDetailId))]
    public class AccountsPayableSetoffDetail : BaseEntity
    {
        /// <summary>
        /// 沖款單ID
        /// </summary>
        [Required(ErrorMessage = "沖款單為必填")]
        [Display(Name = "沖款單")]
        [ForeignKey(nameof(Setoff))]
        public int SetoffId { get; set; }

        /// <summary>
        /// 採購訂單明細ID (二選一)
        /// </summary>
        [Display(Name = "採購訂單明細")]
        [ForeignKey(nameof(PurchaseOrderDetail))]
        public int? PurchaseOrderDetailId { get; set; }

        /// <summary>
        /// 採購退回明細ID (二選一)
        /// </summary>
        [Display(Name = "採購退回明細")]
        [ForeignKey(nameof(PurchaseReturnDetail))]
        public int? PurchaseReturnDetailId { get; set; }

        /// <summary>
        /// 應付金額 (明細的應付總額)
        /// </summary>
        [Display(Name = "應付金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PayableAmount { get; set; } = 0;

        /// <summary>
        /// 本次沖款金額
        /// </summary>
        [Required(ErrorMessage = "沖款金額為必填")]
        [Display(Name = "本次沖款金額")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "沖款金額必須大於 0")]
        public decimal SetoffAmount { get; set; } = 0;

        /// <summary>
        /// 沖款後累計付款金額
        /// </summary>
        [Display(Name = "沖款後累計付款")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AfterPaidAmount { get; set; } = 0;

        // Navigation Properties
        /// <summary>
        /// 沖款單導航屬性
        /// </summary>
        public AccountsPayableSetoff Setoff { get; set; } = null!;

        /// <summary>
        /// 採購訂單明細導航屬性
        /// </summary>
        public PurchaseOrderDetail? PurchaseOrderDetail { get; set; }

        /// <summary>
        /// 採購退回明細導航屬性
        /// </summary>
        public PurchaseReturnDetail? PurchaseReturnDetail { get; set; }

        #region 計算屬性

        /// <summary>
        /// 沖款前累計付款金額 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "沖款前累計付款")]
        public decimal PreviousPaidAmount => AfterPaidAmount - SetoffAmount;

        /// <summary>
        /// 剩餘應付金額 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "剩餘應付金額")]
        public decimal RemainingAmount => PayableAmount - AfterPaidAmount;

        /// <summary>
        /// 是否完全付款 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "是否完全付款")]
        public bool IsFullyPaid => RemainingAmount <= 0;

        /// <summary>
        /// 取得關聯的實體 ID (計算屬性)
        /// </summary>
        [NotMapped]
        public int RelatedDetailId => PurchaseOrderDetailId ?? PurchaseReturnDetailId ?? 0;

        /// <summary>
        /// 取得單據類型 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "單據類型")]
        public string DocumentType => PurchaseOrderDetailId.HasValue ? "PurchaseOrder" : 
                                     PurchaseReturnDetailId.HasValue ? "PurchaseReturn" : 
                                     string.Empty;

        /// <summary>
        /// 取得單據編號 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "單據編號")]
        public string DocumentNumber => PurchaseOrderDetail?.PurchaseOrder?.PurchaseOrderNumber ?? 
                                       PurchaseReturnDetail?.PurchaseReturn?.PurchaseReturnNumber ?? 
                                       string.Empty;

        /// <summary>
        /// 商品名稱 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "商品名稱")]
        public string ProductName => PurchaseOrderDetail?.Product?.Name ?? 
                                    PurchaseReturnDetail?.Product?.Name ?? 
                                    string.Empty;

        /// <summary>
        /// 數量 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "數量")]
        public decimal Quantity => PurchaseOrderDetailId.HasValue ? 
                                  (PurchaseOrderDetail?.OrderQuantity ?? 0) : 
                                  (PurchaseReturnDetail?.ReturnQuantity ?? 0);

        /// <summary>
        /// 單位名稱 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "單位")]
        public string UnitName => PurchaseOrderDetail?.Product?.Unit?.Name ?? 
                                 PurchaseReturnDetail?.Product?.Unit?.Name ?? 
                                 string.Empty;

        #endregion

        /// <summary>
        /// 驗證明細資料的完整性
        /// </summary>
        public bool IsValid()
        {
            // 必須指定採購訂單明細或採購退回明細其中之一
            if (!PurchaseOrderDetailId.HasValue && !PurchaseReturnDetailId.HasValue)
                return false;

            // 不能同時指定兩者
            if (PurchaseOrderDetailId.HasValue && PurchaseReturnDetailId.HasValue)
                return false;

            // 沖款金額必須大於 0 且小於等於剩餘應付金額
            if (SetoffAmount <= 0 || SetoffAmount > (PayableAmount - PreviousPaidAmount))
                return false;

            return true;
        }
    }
}
