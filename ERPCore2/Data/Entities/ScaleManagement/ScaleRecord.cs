using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 磅秤紀錄單據 - 記錄客戶送來磅秤紀錄的每一筆事件
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(VehicleId), nameof(RecordDate))]
    [Index(nameof(CustomerId), nameof(RecordDate))]
    [CodeGenerationStrategy(
        CodeGenerationStrategy.TimestampWithSequence,
        Prefix = "WR",
        DateFieldName = nameof(RecordDate),
        SequenceDigits = 4
    )]
    public class ScaleRecord : BaseEntity
    {
        // ===== 基本資訊 =====

        [Required(ErrorMessage = "記錄日期為必填")]
        [Display(Name = "記錄日期")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        // ===== 磅秤重量欄位 =====

        [Display(Name = "進場重量(公斤)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EntryWeight { get; set; }

        [Display(Name = "進場時間")]
        public DateTime? EntryTime { get; set; }

        [Display(Name = "出場重量(公斤)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExitWeight { get; set; }

        [Display(Name = "出場時間")]
        public DateTime? ExitTime { get; set; }

        [Display(Name = "淨重(公斤)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NetWeight { get; set; }

        // ===== 費用欄位 =====

        /// <summary>我們向客戶/廠商收取的磅秤紀錄處理服務費</summary>
        [Display(Name = "處理費")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DisposalFee { get; set; }

        /// <summary>客戶/廠商向我們收取的磅秤紀錄採購費用</summary>
        [Display(Name = "採購費")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchaseFee { get; set; }

        /// <summary>淨額 = 處理費 - 採購費</summary>
        [Display(Name = "淨額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NetAmount { get; set; }

        // ===== 外鍵 =====

        [Display(Name = "品項")]
        [ForeignKey(nameof(Item))]
        public int? ItemId { get; set; }
        public Item? Item { get; set; }

        [Display(Name = "車輛")]
        [ForeignKey(nameof(Vehicle))]
        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Display(Name = "入庫倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        [Display(Name = "入庫庫位")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
