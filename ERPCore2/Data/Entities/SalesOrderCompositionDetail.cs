using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨訂單組成明細（BOM）
    /// 用於儲存銷貨訂單專屬的 BOM 組成資料
    /// </summary>
    public class SalesOrderCompositionDetail : BaseEntity
    {
        /// <summary>
        /// 銷貨訂單明細 ID
        /// </summary>
        [Display(Name = "銷貨訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int SalesOrderDetailId { get; set; }
        
        /// <summary>
        /// 組成商品 ID
        /// </summary>
        [Display(Name = "組成商品")]
        [ForeignKey(nameof(ComponentProduct))]
        public int ComponentProductId { get; set; }
        
        /// <summary>
        /// 組成數量
        /// </summary>
        [Display(Name = "數量")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// 單位 ID
        /// </summary>
        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }
        
        /// <summary>
        /// 組成成本
        /// </summary>
        [Display(Name = "組成成本")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ComponentCost { get; set; }
        
        // Navigation Properties
        public virtual SalesOrderDetail SalesOrderDetail { get; set; } = null!;
        public virtual Product ComponentProduct { get; set; } = null!;
        public virtual Unit? Unit { get; set; }
    }
}
