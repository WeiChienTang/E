using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存交易記錄實體 - 記錄所有庫存異動歷史
    /// </summary>
    [Index(nameof(ProductId), nameof(TransactionDate))]
    [Index(nameof(WarehouseId), nameof(TransactionDate))]
    [Index(nameof(TransactionType), nameof(TransactionDate))]
    public class InventoryTransaction : BaseEntity
    {
        [Required(ErrorMessage = "交易單號為必填")]
        [MaxLength(30, ErrorMessage = "交易單號不可超過30個字元")]
        [Display(Name = "交易單號")]
        public string TransactionNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "交易類型為必填")]
        [Display(Name = "交易類型")]
        public InventoryTransactionTypeEnum TransactionType { get; set; }
        
        [Required(ErrorMessage = "交易日期為必填")]
        [Display(Name = "交易日期")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "交易數量為必填")]
        [Display(Name = "交易數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }
        
        [Display(Name = "單位成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }
        
        [Display(Name = "交易前庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal StockBefore { get; set; }
        
        [Display(Name = "交易後庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal StockAfter { get; set; }
        
        // === 批號追蹤欄位 ===
        [Display(Name = "交易批號")]
        [MaxLength(50, ErrorMessage = "交易批號不可超過50個字元")]
        public string? TransactionBatchNumber { get; set; }
        
        [Display(Name = "交易批次進貨日期")]
        public DateTime? TransactionBatchDate { get; set; }
        
        [Display(Name = "交易批次到期日期")]
        public DateTime? TransactionExpiryDate { get; set; }
        
        // Foreign Keys
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        
        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        [Display(Name = "庫存主檔")]
        [ForeignKey(nameof(InventoryStock))]
        public int? InventoryStockId { get; set; }
        
        [Display(Name = "庫存明細")]
        [ForeignKey(nameof(InventoryStockDetail))]
        public int? InventoryStockDetailId { get; set; }
        
        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
        public InventoryStock? InventoryStock { get; set; }
        public InventoryStockDetail? InventoryStockDetail { get; set; }
    }
}
