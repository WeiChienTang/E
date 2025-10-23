using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存主檔實體 - 記錄商品的總庫存資訊（主檔）
    /// </summary>
    [Index(nameof(ProductId), IsUnique = true)]
    public class InventoryStock : BaseEntity
    {
        // === 計算屬性：從明細加總 ===
        [Display(Name = "總現有庫存")]
        [NotMapped]
        public int TotalCurrentStock => InventoryStockDetails?.Sum(d => d.CurrentStock) ?? 0;
        
        [Display(Name = "總預留庫存")]
        [NotMapped]
        public int TotalReservedStock => InventoryStockDetails?.Sum(d => d.ReservedStock) ?? 0;
        
        [Display(Name = "總可用庫存")]
        [NotMapped]
        public int TotalAvailableStock => TotalCurrentStock - TotalReservedStock;
        
        [Display(Name = "總在途庫存")]
        [NotMapped]
        public int TotalInTransitStock => InventoryStockDetails?.Sum(d => d.InTransitStock) ?? 0;
        
        [Display(Name = "加權平均成本")]
        [Column(TypeName = "decimal(18,4)")]
        [NotMapped]
        public decimal? WeightedAverageCost
        {
            get
            {
                if (InventoryStockDetails == null || !InventoryStockDetails.Any())
                    return null;
                
                var totalQuantity = InventoryStockDetails.Sum(d => d.CurrentStock);
                if (totalQuantity == 0)
                    return null;
                
                var totalValue = InventoryStockDetails
                    .Where(d => d.AverageCost.HasValue)
                    .Sum(d => d.CurrentStock * d.AverageCost!.Value);
                
                return totalValue / totalQuantity;
            }
        }
        
        [Display(Name = "最後交易日期")]
        [NotMapped]
        public DateTime? LastTransactionDate => InventoryStockDetails?
            .Where(d => d.LastTransactionDate.HasValue)
            .Max(d => d.LastTransactionDate);
        
        // Foreign Keys
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        // Navigation Properties
        public Product Product { get; set; } = null!;
        public ICollection<InventoryStockDetail> InventoryStockDetails { get; set; } = new List<InventoryStockDetail>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
    }
}
