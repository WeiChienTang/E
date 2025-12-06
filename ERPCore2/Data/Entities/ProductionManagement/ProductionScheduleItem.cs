using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 生產排程項目 - 記錄要生產的商品項目（以產品為單位）
    /// 每個銷售訂單明細轉排程會產生一筆 ProductionScheduleItem
    /// </summary>
    [Index(nameof(ProductionScheduleId), nameof(ProductId))]
    [Index(nameof(SalesOrderDetailId))]
    [Index(nameof(ProductionItemStatus))]
    public class ProductionScheduleItem : BaseEntity
    {
        // === 生產資訊 ===
        
        /// <summary>
        /// 排程數量 - 本次要生產的數量
        /// </summary>
        [Required(ErrorMessage = "排程數量為必填")]
        [Display(Name = "排程數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal ScheduledQuantity { get; set; } = 0;
        
        /// <summary>
        /// 已完成數量 - 已入庫的完成品數量
        /// </summary>
        [Display(Name = "已完成數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal CompletedQuantity { get; set; } = 0;
        
        /// <summary>
        /// 待完成數量 - 計算屬性
        /// </summary>
        [Display(Name = "待完成數量")]
        [NotMapped]
        public decimal PendingQuantity => ScheduledQuantity - CompletedQuantity;
        
        /// <summary>
        /// 生產項目狀態
        /// </summary>
        [Required(ErrorMessage = "生產狀態為必填")]
        [Display(Name = "生產狀態")]
        public ProductionItemStatus ProductionItemStatus { get; set; } = ProductionItemStatus.Pending;
        
        /// <summary>
        /// 預計開始日期
        /// </summary>
        [Display(Name = "預計開始日期")]
        public DateTime? PlannedStartDate { get; set; }
        
        /// <summary>
        /// 預計完成日期
        /// </summary>
        [Display(Name = "預計完成日期")]
        public DateTime? PlannedEndDate { get; set; }
        
        /// <summary>
        /// 實際開始日期
        /// </summary>
        [Display(Name = "實際開始日期")]
        public DateTime? ActualStartDate { get; set; }
        
        /// <summary>
        /// 實際完成日期
        /// </summary>
        [Display(Name = "實際完成日期")]
        public DateTime? ActualEndDate { get; set; }
        
        /// <summary>
        /// 優先順序
        /// </summary>
        [Display(Name = "優先順序")]
        public int Priority { get; set; } = 0;        
        
        // === Foreign Keys ===
        
        /// <summary>
        /// 生產排程主檔 ID
        /// </summary>
        [Required(ErrorMessage = "生產排程主檔為必填")]
        [Display(Name = "生產排程主檔")]
        [ForeignKey(nameof(ProductionSchedule))]
        public int ProductionScheduleId { get; set; }
        
        /// <summary>
        /// 商品 ID - 要生產的商品
        /// </summary>
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        /// <summary>
        /// 來源銷售訂單明細 ID - 追蹤來源
        /// </summary>
        [Display(Name = "來源銷售訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int? SalesOrderDetailId { get; set; }
        
        /// <summary>
        /// 入庫倉庫 ID - 生產完成後入庫的倉庫
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
        
        // === Navigation Properties ===
        
        /// <summary>
        /// 生產排程主檔
        /// </summary>
        public ProductionSchedule ProductionSchedule { get; set; } = null!;
        
        /// <summary>
        /// 商品
        /// </summary>
        public Product Product { get; set; } = null!;
        
        /// <summary>
        /// 來源銷售訂單明細
        /// </summary>
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        
        /// <summary>
        /// 入庫倉庫
        /// </summary>
        public Warehouse? Warehouse { get; set; }
        
        /// <summary>
        /// 入庫倉庫位置
        /// </summary>
        public WarehouseLocation? WarehouseLocation { get; set; }
        
        /// <summary>
        /// 此項目的組件明細（需要領料的物料）
        /// </summary>
        public ICollection<ProductionScheduleDetail> ScheduleDetails { get; set; } = new List<ProductionScheduleDetail>();
        
        /// <summary>
        /// 此項目的完成入庫紀錄
        /// </summary>
        public ICollection<ProductionScheduleCompletion> Completions { get; set; } = new List<ProductionScheduleCompletion>();
    }
}
