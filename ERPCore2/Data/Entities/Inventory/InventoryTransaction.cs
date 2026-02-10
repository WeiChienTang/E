using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存異動主檔實體 - 記錄庫存異動的主要資訊
    /// 一筆主檔對應多筆明細（InventoryTransactionDetail）
    /// </summary>
    [Index(nameof(TransactionNumber))]
    [Index(nameof(WarehouseId), nameof(TransactionDate))]
    [Index(nameof(TransactionType), nameof(TransactionDate))]
    [Index(nameof(SourceDocumentType), nameof(SourceDocumentId))]
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
        
        // === 來源單據關聯（新增）===
        
        /// <summary>
        /// 來源單據類型（PurchaseReceiving、SalesDelivery、StockTaking 等）
        /// </summary>
        [Display(Name = "來源單據類型")]
        [MaxLength(50)]
        public string? SourceDocumentType { get; set; }
        
        /// <summary>
        /// 來源單據 ID
        /// </summary>
        [Display(Name = "來源單據ID")]
        public int? SourceDocumentId { get; set; }
        
        // === 彙總欄位（新增）===
        
        /// <summary>
        /// 總數量（所有明細的數量加總）
        /// </summary>
        [Display(Name = "總數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalQuantity { get; set; }
        
        /// <summary>
        /// 總金額（所有明細的金額加總）
        /// </summary>
        [Display(Name = "總金額")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; }
        
        // === Foreign Keys ===
        
        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        
        /// <summary>
        /// 經辦人員
        /// </summary>
        [Display(Name = "經辦人員")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }
        
        // === Navigation Properties ===
        
        public Warehouse Warehouse { get; set; } = null!;
        public Employee? Employee { get; set; }
        
        /// <summary>
        /// 異動明細集合
        /// </summary>
        public ICollection<InventoryTransactionDetail> Details { get; set; } = new List<InventoryTransactionDetail>();
    }
}
