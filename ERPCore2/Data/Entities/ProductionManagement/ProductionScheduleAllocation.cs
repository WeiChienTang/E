using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 生產排程分配 - 記錄生產數量分配到哪些銷售訂單
    /// 當同一個產品被多個訂單共用生產時，追蹤每個訂單分配到的數量
    /// </summary>
    [Index(nameof(ProductionScheduleItemId), nameof(SalesOrderDetailId))]
    public class ProductionScheduleAllocation : BaseEntity
    {
        /// <summary>
        /// 分配數量 - 分配給該訂單的生產數量
        /// </summary>
        [Required(ErrorMessage = "分配數量為必填")]
        [Display(Name = "分配數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal AllocatedQuantity { get; set; } = 0;
        
        // === Foreign Keys ===
        
        /// <summary>
        /// 生產排程項目 ID
        /// </summary>
        [Required(ErrorMessage = "生產排程項目為必填")]
        [Display(Name = "生產排程項目")]
        [ForeignKey(nameof(ProductionScheduleItem))]
        public int ProductionScheduleItemId { get; set; }
        
        /// <summary>
        /// 銷售訂單明細 ID - 分配到的訂單明細
        /// </summary>
        [Required(ErrorMessage = "銷售訂單明細為必填")]
        [Display(Name = "銷售訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int SalesOrderDetailId { get; set; }
        
        // === Navigation Properties ===
        
        /// <summary>
        /// 生產排程項目
        /// </summary>
        public ProductionScheduleItem ProductionScheduleItem { get; set; } = null!;
        
        /// <summary>
        /// 銷售訂單明細
        /// </summary>
        public SalesOrderDetail SalesOrderDetail { get; set; } = null!;
    }
}
