using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存異動明細實體 - 記錄單筆商品的異動詳情
    /// </summary>
    [Index(nameof(InventoryTransactionId))]
    [Index(nameof(ProductId))]
    public class InventoryTransactionDetail : BaseEntity
    {
        // === 關聯欄位 ===
        
        [Required(ErrorMessage = "庫存異動主檔為必填")]
        [Display(Name = "庫存異動主檔")]
        [ForeignKey(nameof(InventoryTransaction))]
        public int InventoryTransactionId { get; set; }
        
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        // === 數量與金額 ===
        
        [Required(ErrorMessage = "數量為必填")]
        [Display(Name = "數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }
        
        [Display(Name = "單位成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }
        
        [Display(Name = "金額")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }
        
        // === 庫存追蹤 ===
        
        [Display(Name = "異動前庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal StockBefore { get; set; }
        
        [Display(Name = "異動後庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal StockAfter { get; set; }
        
        // === 批號追蹤 ===
        
        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }
        
        [Display(Name = "批次日期")]
        public DateTime? BatchDate { get; set; }
        
        [Display(Name = "到期日期")]
        public DateTime? ExpiryDate { get; set; }
        
        // === 來源明細關聯（選填）===
        
        /// <summary>
        /// 來源單據明細 ID（例如：PurchaseReceivingDetailId、SalesDeliveryDetailId 等）
        /// </summary>
        [Display(Name = "來源明細ID")]
        public int? SourceDetailId { get; set; }
        
        // === 庫存關聯（追蹤用）===
        
        [Display(Name = "庫存主檔")]
        [ForeignKey(nameof(InventoryStock))]
        public int? InventoryStockId { get; set; }
        
        [Display(Name = "庫存明細")]
        [ForeignKey(nameof(InventoryStockDetail))]
        public int? InventoryStockDetailId { get; set; }
        
        // === 導航屬性 ===
        
        public InventoryTransaction InventoryTransaction { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
        public InventoryStock? InventoryStock { get; set; }
        public InventoryStockDetail? InventoryStockDetail { get; set; }
    }
}
