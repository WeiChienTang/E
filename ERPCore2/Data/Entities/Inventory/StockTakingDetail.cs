using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存盤點明細實體 - 記錄盤點明細資料
    /// </summary>
    [Index(nameof(StockTakingId), nameof(ProductId), nameof(WarehouseLocationId), IsUnique = true)]
    [Index(nameof(ProductId))]
    public class StockTakingDetail : BaseEntity
    {
        [Required(ErrorMessage = "盤點主檔為必填")]
        [Display(Name = "盤點主檔")]
        [ForeignKey(nameof(StockTaking))]
        public int StockTakingId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }

        [Display(Name = "系統庫存")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal SystemStock { get; set; } = 0;

        [Display(Name = "實盤數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ActualStock { get; set; }

        [Display(Name = "差異數量")]
        public decimal? DifferenceQuantity => ActualStock.HasValue ? ActualStock.Value - SystemStock : null;

        [Display(Name = "單位成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }

        [Display(Name = "差異金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DifferenceAmount => DifferenceQuantity.HasValue && UnitCost.HasValue 
            ? DifferenceQuantity.Value * UnitCost.Value : null;

        [Display(Name = "盤點狀態")]
        public StockTakingDetailStatusEnum DetailStatus { get; set; } = StockTakingDetailStatusEnum.Pending;

        [Display(Name = "盤點時間")]
        public DateTime? TakingTime { get; set; }

        [Display(Name = "盤點人員")]
        [MaxLength(100, ErrorMessage = "盤點人員不可超過100個字元")]
        public string? TakingPersonnel { get; set; }

        [MaxLength(200, ErrorMessage = "備註不可超過200個字元")]
        [Display(Name = "備註")]
        public string? DetailRemarks { get; set; }

        [Display(Name = "是否有差異")]
        public bool HasDifference => DifferenceQuantity.HasValue && DifferenceQuantity.Value != 0;

        [Display(Name = "是否已調整")]
        public bool IsAdjusted { get; set; } = false;

        [Display(Name = "調整單號")]
        [MaxLength(30, ErrorMessage = "調整單號不可超過30個字元")]
        public string? AdjustmentNumber { get; set; }

        // Navigation Properties
        public StockTaking StockTaking { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
