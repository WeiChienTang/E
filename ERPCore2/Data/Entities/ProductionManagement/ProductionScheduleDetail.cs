using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 生產排程明細檔 - 記錄生產所需的組件物料資訊
    /// </summary>
    [Index(nameof(ProductionScheduleId), nameof(ComponentProductId))]
    [Index(nameof(ComponentProductId))]
    public class ProductionScheduleDetail : BaseEntity
    {
        // 關聯資訊
        [Required(ErrorMessage = "生產排程主檔為必填")]
        [Display(Name = "生產排程主檔")]
        [ForeignKey(nameof(ProductionSchedule))]
        public int ProductionScheduleId { get; set; }

        [Required(ErrorMessage = "組件產品為必填")]
        [Display(Name = "組件產品")]
        [ForeignKey(nameof(ComponentProduct))]
        public int ComponentProductId { get; set; }

        [Display(Name = "BOM明細")]
        [ForeignKey(nameof(ProductCompositionDetail))]
        public int? ProductCompositionDetailId { get; set; }

        // 數量資訊
        [Required(ErrorMessage = "需求數量為必填")]
        [Display(Name = "需求數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal RequiredQuantity { get; set; } = 0;

        // 成本資訊
        [Display(Name = "預估單位成本")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedUnitCost { get; set; }

        [Display(Name = "實際單位成本")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualUnitCost { get; set; }

        [Display(Name = "總成本")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }

        // 庫存與領料
        [Display(Name = "領料倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 生產排程主檔
        /// </summary>
        public ProductionSchedule ProductionSchedule { get; set; } = null!;

        /// <summary>
        /// 組件產品
        /// </summary>
        public Product ComponentProduct { get; set; } = null!;

        /// <summary>
        /// BOM明細
        /// </summary>
        public ProductCompositionDetail? ProductCompositionDetail { get; set; }

        /// <summary>
        /// 領料倉庫
        /// </summary>
        public Warehouse? Warehouse { get; set; }
    }
}
