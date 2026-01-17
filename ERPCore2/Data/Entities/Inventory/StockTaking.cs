using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存盤點實體 - 記錄盤點主檔
    /// </summary>
    [Index(nameof(TakingNumber), IsUnique = true)]
    [Index(nameof(WarehouseId), nameof(TakingDate))]
    [Index(nameof(TakingStatus), nameof(TakingDate))]
    public class StockTaking : BaseEntity
    {
        [Required(ErrorMessage = "盤點單號為必填")]
        [MaxLength(30, ErrorMessage = "盤點單號不可超過30個字元")]
        [Display(Name = "盤點單號")]
        public string TakingNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "盤點日期為必填")]
        [Display(Name = "盤點日期")]
        public DateTime TakingDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }

        [Required(ErrorMessage = "盤點類型為必填")]
        [Display(Name = "盤點類型")]
        public StockTakingTypeEnum TakingType { get; set; } = StockTakingTypeEnum.Full;

        [Required(ErrorMessage = "盤點狀態為必填")]
        [Display(Name = "盤點狀態")]
        public StockTakingStatusEnum TakingStatus { get; set; } = StockTakingStatusEnum.Draft;

        [Display(Name = "開始時間")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "結束時間")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "盤點人員")]
        public string? TakingPersonnel { get; set; }

        [Display(Name = "監盤人員")]
        public string? SupervisingPersonnel { get; set; }

        [Display(Name = "審核人員")]
        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedBy { get; set; }

        [Display(Name = "審核時間")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "是否已產生調整單")]
        public bool IsAdjustmentGenerated { get; set; } = false;

        [Display(Name = "盤點總項目數")]
        public int TotalItems { get; set; } = 0;

        [Display(Name = "已盤點項目數")]
        public int CompletedItems { get; set; } = 0;

        [Display(Name = "差異項目數")]
        public int DifferenceItems { get; set; } = 0;

        [Display(Name = "差異金額")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal DifferenceAmount { get; set; } = 0;

        /// <summary>
        /// 從明細計算差異金額（用於更新 DifferenceAmount 欄位）
        /// </summary>
        [NotMapped]
        public decimal CalculatedDifferenceAmount => StockTakingDetails?.Where(d => d.DifferenceAmount.HasValue).Sum(d => d.DifferenceAmount!.Value) ?? 0;

        [Display(Name = "盤點完成率")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal CompletionRate => TotalItems > 0 ? (decimal)CompletedItems / TotalItems * 100 : 0;

        // Navigation Properties
        public Warehouse Warehouse { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public ICollection<StockTakingDetail> StockTakingDetails { get; set; } = new List<StockTakingDetail>();
    }
}
