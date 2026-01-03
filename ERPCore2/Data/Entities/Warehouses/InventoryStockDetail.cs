using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存明細實體 - 記錄商品在各倉庫位置的具體庫存分佈
    /// </summary>
    [Index(nameof(InventoryStockId), nameof(WarehouseId), nameof(WarehouseLocationId), IsUnique = true)]
    public class InventoryStockDetail : BaseEntity
    {
        [Display(Name = "現有庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CurrentStock { get; set; } = 0;
        
        [Display(Name = "預留庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ReservedStock { get; set; } = 0;
        
        [Display(Name = "可用庫存")]
        public decimal AvailableStock => CurrentStock - ReservedStock;
        
        [Display(Name = "在途庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal InTransitStock { get; set; } = 0;
        
        /// <summary>
        /// 生產中庫存 - 已領料投入生產的組件數量
        /// </summary>
        [Display(Name = "生產中庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal InProductionStock { get; set; } = 0;
        
        [Display(Name = "平均成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? AverageCost { get; set; }
        
        [Display(Name = "最後交易日期")]
        public DateTime? LastTransactionDate { get; set; }
        
        // === 庫存警戒線設定 ===
        [Display(Name = "最低庫存警戒線")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MinStockLevel { get; set; }
        
        [Display(Name = "最高庫存警戒線")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MaxStockLevel { get; set; }
        
        // === 批號追蹤欄位 ===
        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }
        
        [Display(Name = "批次進貨日期")]
        public DateTime? BatchDate { get; set; }
        
        [Display(Name = "到期日期")]
        public DateTime? ExpiryDate { get; set; }
        
        // Foreign Keys
        [Required(ErrorMessage = "庫存主檔為必填")]
        [Display(Name = "庫存主檔")]
        [ForeignKey(nameof(InventoryStock))]
        public int InventoryStockId { get; set; }
        
        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        
        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        // Navigation Properties
        public InventoryStock InventoryStock { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
    }
}
