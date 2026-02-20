using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廢料記錄單據 - 記錄客戶送來廢料的每一筆事件
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
    public class WasteRecord : BaseEntity
    {
        // ===== 基本資訊 =====

        [Required(ErrorMessage = "記錄日期為必填")]
        [Display(Name = "記錄日期")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        // ===== 廢料數量 =====

        [Display(Name = "數量")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Quantity { get; set; }

        [Display(Name = "總重量(公斤)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalWeight { get; set; }

        // ===== 費用欄位 =====

        /// <summary>我們向客戶/廠商收取的廢料處理服務費</summary>
        [Display(Name = "處理費")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DisposalFee { get; set; }

        /// <summary>客戶/廠商向我們收取的廢料採購費用</summary>
        [Display(Name = "採購費")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchaseFee { get; set; }

        /// <summary>淨額 = 處理費 - 採購費</summary>
        [Display(Name = "淨額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NetAmount { get; set; }

        // ===== 外鍵 =====

        [Required(ErrorMessage = "車輛為必填")]
        [Display(Name = "車輛")]
        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required(ErrorMessage = "廢料類型為必填")]
        [Display(Name = "廢料類型")]
        [ForeignKey(nameof(WasteType))]
        public int WasteTypeId { get; set; }
        public WasteType WasteType { get; set; } = null!;

        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required(ErrorMessage = "入庫倉庫為必填")]
        [Display(Name = "入庫倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        [Display(Name = "入庫庫位")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
