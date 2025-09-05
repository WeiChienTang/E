using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存主檔實體 - 記錄商品在各倉庫位置的庫存數量
    /// </summary>
    [Index(nameof(ProductId), nameof(WarehouseId), nameof(WarehouseLocationId), IsUnique = true)]
    public class InventoryStock : BaseEntity
    {
        [Display(Name = "現有庫存")]
        public int CurrentStock { get; set; } = 0;
        
        [Display(Name = "預留庫存")]
        public int ReservedStock { get; set; } = 0;
        
        [Display(Name = "可用庫存")]
        public int AvailableStock => CurrentStock - ReservedStock;
        
        [Display(Name = "在途庫存")]
        public int InTransitStock { get; set; } = 0;
        
        [Display(Name = "最低庫存警戒線")]
        public int? MinStockLevel { get; set; }
        
        [Display(Name = "最高庫存警戒線")]
        public int? MaxStockLevel { get; set; }
        
        [Display(Name = "平均成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? AverageCost { get; set; }
        
        [Display(Name = "最後交易日期")]
        public DateTime? LastTransactionDate { get; set; }
        
        // Foreign Keys
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }
        
        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Warehouse? Warehouse { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
    }
}
