using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 生產完成入庫紀錄 - 記錄每次生產完成的入庫資訊
    /// 支援分批入庫，可多次完成同一個生產排程項目
    /// </summary>
    [Index(nameof(ProductionScheduleItemId))]
    [Index(nameof(CompletionDate))]
    public class ProductionScheduleCompletion : BaseEntity
    {
        /// <summary>
        /// 完成數量 - 本次入庫的完成品數量
        /// </summary>
        [Required(ErrorMessage = "完成數量為必填")]
        [Display(Name = "完成數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal CompletedQuantity { get; set; } = 0;
        
        /// <summary>
        /// 完成日期
        /// </summary>
        [Required(ErrorMessage = "完成日期為必填")]
        [Display(Name = "完成日期")]
        public DateTime CompletionDate { get; set; } = DateTime.Today;
        
        /// <summary>
        /// 實際單位成本 - 入庫時的成本
        /// </summary>
        [Display(Name = "實際單位成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ActualUnitCost { get; set; }
        
        /// <summary>
        /// 批號 - 入庫的批號
        /// </summary>
        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }
        
        /// <summary>
        /// 到期日期
        /// </summary>
        [Display(Name = "到期日期")]
        public DateTime? ExpiryDate { get; set; }
        
        /// <summary>
        /// 品質檢驗結果
        /// </summary>
        [Display(Name = "品質檢驗結果")]
        [MaxLength(200, ErrorMessage = "品質檢驗結果不可超過200個字元")]
        public string? QualityCheckResult { get; set; }        
        
        /// <summary>
        /// 操作人員 ID
        /// </summary>
        [Display(Name = "操作人員")]
        [ForeignKey(nameof(CompletedByEmployee))]
        public int? CompletedByEmployeeId { get; set; }
        
        // === Foreign Keys ===
        
        /// <summary>
        /// 生產排程項目 ID
        /// </summary>
        [Required(ErrorMessage = "生產排程項目為必填")]
        [Display(Name = "生產排程項目")]
        [ForeignKey(nameof(ProductionScheduleItem))]
        public int ProductionScheduleItemId { get; set; }
        
        /// <summary>
        /// 入庫倉庫 ID
        /// </summary>
        [Display(Name = "入庫倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }
        
        /// <summary>
        /// 入庫倉庫位置 ID
        /// </summary>
        [Display(Name = "入庫倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        /// <summary>
        /// 庫存異動紀錄 ID - 關聯到庫存異動
        /// </summary>
        [Display(Name = "庫存異動紀錄")]
        [ForeignKey(nameof(InventoryTransaction))]
        public int? InventoryTransactionId { get; set; }
        
        // === Navigation Properties ===
        
        /// <summary>
        /// 生產排程項目
        /// </summary>
        public ProductionScheduleItem ProductionScheduleItem { get; set; } = null!;
        
        /// <summary>
        /// 入庫倉庫
        /// </summary>
        public Warehouse? Warehouse { get; set; }
        
        /// <summary>
        /// 入庫倉庫位置
        /// </summary>
        public WarehouseLocation? WarehouseLocation { get; set; }
        
        /// <summary>
        /// 操作人員
        /// </summary>
        public Employee? CompletedByEmployee { get; set; }
        
        /// <summary>
        /// 庫存異動紀錄
        /// </summary>
        public InventoryTransaction? InventoryTransaction { get; set; }
    }
}
