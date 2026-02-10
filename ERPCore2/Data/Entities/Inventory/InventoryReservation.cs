using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存預留實體 - 管理銷售或生產訂單的庫存預留
    /// </summary>
    [Index(nameof(ProductId), nameof(ReservationDate))]
    [Index(nameof(ReservationType), nameof(ReservationStatus))]
    [Index(nameof(ReferenceNumber))]
    public class InventoryReservation : BaseEntity
    {
        [Required(ErrorMessage = "預留單號為必填")]
        [MaxLength(30, ErrorMessage = "預留單號不可超過30個字元")]
        [Display(Name = "預留單號")]
        public string ReservationNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "預留類型為必填")]
        [Display(Name = "預留類型")]
        public InventoryReservationType ReservationType { get; set; }
        
        [Required(ErrorMessage = "預留狀態為必填")]
        [Display(Name = "預留狀態")]
        public InventoryReservationStatus ReservationStatus { get; set; } = InventoryReservationStatus.Reserved;
        
        [Required(ErrorMessage = "預留日期為必填")]
        [Display(Name = "預留日期")]
        public DateTime ReservationDate { get; set; } = DateTime.Now;
        
        [Display(Name = "到期日期")]
        public DateTime? ExpiryDate { get; set; }
        
        [Required(ErrorMessage = "預留數量為必填")]
        [Display(Name = "預留數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ReservedQuantity { get; set; }
        
        [Display(Name = "已釋放數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ReleasedQuantity { get; set; } = 0;
        
        [Display(Name = "剩餘預留數量")]
        public decimal RemainingQuantity => ReservedQuantity - ReleasedQuantity;
        
        [MaxLength(50, ErrorMessage = "參考單號不可超過50個字元")]
        [Display(Name = "參考單號")]
        public string? ReferenceNumber { get; set; }
        
        [MaxLength(200, ErrorMessage = "備註不可超過200個字元")]
        [Display(Name = "備註")]
        public string? ReservationRemarks { get; set; }
        
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
