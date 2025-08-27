using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購訂單明細實體 - 記錄採購訂單商品明細
    /// </summary>
    [Index(nameof(PurchaseOrderId), nameof(ProductId))]
    [Index(nameof(ProductId))]
    public class PurchaseOrderDetail : BaseEntity
    {
        [Required(ErrorMessage = "採購訂單為必填")]
        [Display(Name = "採購訂單")]
        [ForeignKey(nameof(PurchaseOrder))]
        public int PurchaseOrderId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "訂購數量為必填")]
        [Display(Name = "訂購數量")]
        public int OrderQuantity { get; set; }

        [Display(Name = "已進貨數量")]
        public int ReceivedQuantity { get; set; } = 0;

        [Display(Name = "待進貨數量")]
        public int PendingQuantity => OrderQuantity - ReceivedQuantity;

        [Required(ErrorMessage = "單價為必填")]
        [Display(Name = "單價")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "小計金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount => OrderQuantity * UnitPrice;

        [Display(Name = "已進貨金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivedAmount { get; set; } = 0;

        [Display(Name = "預計到貨日期")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        // Navigation Properties
        public PurchaseOrder PurchaseOrder { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ICollection<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; } = new List<PurchaseReceivingDetail>();
    }
}
