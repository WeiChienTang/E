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
        /// 沖款後累計收款金額
        /// </summary>
        [Display(Name = "沖款後累計收款")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AfterReceivedAmount { get; set; } = 0;

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

        #region 計算屬性

        /// <summary>
        /// 沖款前累計收款金額 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "沖款前累計收款")]
        public decimal PreviousReceivedAmount => AfterReceivedAmount - SetoffAmount;

        /// <summary>
        /// 剩餘應收金額 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "剩餘應收金額")]
        public decimal RemainingAmount => ReceivableAmount - AfterReceivedAmount;

        /// <summary>
        /// 是否完全收款 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "是否完全收款")]
        public bool IsFullyReceived => RemainingAmount <= 0;

        /// <summary>
        /// 取得關聯的實體 ID (計算屬性)
        /// </summary>
        [NotMapped]
        public int RelatedDetailId => SalesOrderDetailId ?? SalesReturnDetailId ?? 0;

        /// <summary>
        /// 取得單據類型 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "單據類型")]
        public string DocumentType => SalesOrderDetailId.HasValue ? "SalesOrder" : 
                                     SalesReturnDetailId.HasValue ? "SalesReturn" : 
                                     string.Empty;

        /// <summary>
        /// 取得單據編號 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "單據編號")]
        public string DocumentNumber => SalesOrderDetail?.SalesOrder?.SalesOrderNumber ?? 
                                       SalesReturnDetail?.SalesReturn?.SalesReturnNumber ?? 
                                       string.Empty;

        /// <summary>
        /// 商品名稱 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "商品名稱")]
        public string ProductName => SalesOrderDetail?.Product?.Name ?? 
                                    SalesReturnDetail?.Product?.Name ?? 
                                    string.Empty;

        /// <summary>
        /// 數量 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "數量")]
        public decimal Quantity => SalesOrderDetailId.HasValue ? 
                                  (SalesOrderDetail?.OrderQuantity ?? 0) : 
                                  (SalesReturnDetail?.ReturnQuantity ?? 0);

        /// <summary>
        /// 單位名稱 (計算屬性)
        /// </summary>
        [NotMapped]
        [Display(Name = "單位")]
        public string UnitName => SalesOrderDetail?.Product?.Unit?.Name ?? 
                                 SalesReturnDetail?.Product?.Unit?.Name ?? 
                                 string.Empty;

        #endregion

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