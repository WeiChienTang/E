using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data;

namespace ERPCore2.Models
{
    /// <summary>
    /// 應收沖款視圖模型 - 統一處理銷貨訂單和採購退回的應收款項
    /// </summary>
    public class ReceivableViewModel : BaseEntity
    {
        /// <summary>
        /// 單據類型 (SalesOrder/PurchaseReturn)
        /// </summary>
        [Display(Name = "單據類型")]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>
        /// 單據編號
        /// </summary>
        [Display(Name = "單據編號")]
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// 單據日期
        /// </summary>
        [Display(Name = "單據日期")]
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// 客戶或供應商名稱
        /// </summary>
        [Display(Name = "往來對象")]
        public string CustomerOrSupplier { get; set; } = string.Empty;

        /// <summary>
        /// 商品名稱
        /// </summary>
        [Display(Name = "商品名稱")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 數量
        /// </summary>
        [Display(Name = "數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; } = 0;

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位")]
        public string? UnitName { get; set; }

        /// <summary>
        /// 應收總額 (小計)
        /// </summary>
        [Display(Name = "應收總額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        /// <summary>
        /// 本次收款金額
        /// </summary>
        [Display(Name = "本次收款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivedAmount { get; set; } = 0;

        /// <summary>
        /// 累計收款金額
        /// </summary>
        [Display(Name = "累計收款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceivedAmount { get; set; } = 0;

        /// <summary>
        /// 餘額 (應收總額 - 累計收款金額)
        /// </summary>
        [Display(Name = "餘額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAmount => TotalAmount - TotalReceivedAmount;

        /// <summary>
        /// 是否結清
        /// </summary>
        [Display(Name = "是否結清")]
        public bool IsSettled { get; set; } = false;

        /// <summary>
        /// 收款進度百分比
        /// </summary>
        [Display(Name = "收款進度")]
        public decimal ProgressPercentage => TotalAmount > 0 ? (TotalReceivedAmount / TotalAmount) * 100 : 0;

        /// <summary>
        /// 最後收款日期
        /// </summary>
        [Display(Name = "最後收款日期")]
        public DateTime? LastReceivedDate { get; set; }

        /// <summary>
        /// 預計收款日期
        /// </summary>
        [Display(Name = "預計收款日期")]
        public DateTime? ExpectedReceiveDate { get; set; }

        /// <summary>
        /// 逾期天數 (如果有預計收款日期且超過今日)
        /// </summary>
        [Display(Name = "逾期天數")]
        public int OverdueDays 
        { 
            get
            {
                if (IsSettled || !ExpectedReceiveDate.HasValue)
                    return 0;

                var days = (DateTime.Now.Date - ExpectedReceiveDate.Value.Date).Days;
                return days > 0 ? days : 0;
            }
        }

        /// <summary>
        /// 是否逾期
        /// </summary>
        [Display(Name = "是否逾期")]
        public bool IsOverdue => OverdueDays > 0;

        /// <summary>
        /// 狀態顯示文字
        /// </summary>
        [Display(Name = "狀態")]
        public string StatusText 
        { 
            get
            {
                if (IsSettled) return "已結清";
                if (IsOverdue) return "逾期";
                if (TotalReceivedAmount > 0) return "部分收款";
                return "未收款";
            }
        }

        /// <summary>
        /// 倉庫名稱
        /// </summary>
        [Display(Name = "倉庫")]
        public string? WarehouseName { get; set; }
    }
}