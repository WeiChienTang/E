using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 產品合成明細檔（BOM 明細）- 記錄每個成品所需的原料/半成品及其數量
    /// </summary>
    [Index(nameof(ProductCompositionId), nameof(Sequence))]
    [Index(nameof(ComponentProductId))]
    [Index(nameof(ProductCompositionId), nameof(ComponentProductId), IsUnique = true)]
    public class ProductCompositionDetail : BaseEntity
    {
        // 關聯資訊
        [Required(ErrorMessage = "產品合成主檔為必填")]
        [Display(Name = "產品合成主檔")]
        [ForeignKey(nameof(ProductComposition))]
        public int ProductCompositionId { get; set; }

        [Required(ErrorMessage = "組件產品為必填")]
        [Display(Name = "組件產品")]
        [ForeignKey(nameof(ComponentProduct))]
        public int ComponentProductId { get; set; }

        // 基本資訊
        [Required(ErrorMessage = "順序號為必填")]
        [Display(Name = "順序號")]
        public int Sequence { get; set; } = 1;

        [Required(ErrorMessage = "所需數量為必填")]
        [Display(Name = "所需數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; } = 1;

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        // 進階設定
        [Display(Name = "損耗率(%)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? LossRate { get; set; }

        [Display(Name = "是否為可選組件")]
        public bool IsOptional { get; set; } = false;

        [Display(Name = "是否為關鍵組件")]
        public bool IsKeyComponent { get; set; } = false;

        [Display(Name = "最小數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MinQuantity { get; set; }

        [Display(Name = "最大數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MaxQuantity { get; set; }

        [Display(Name = "組件成本")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ComponentCost { get; set; }

        [MaxLength(200, ErrorMessage = "位置說明不可超過200個字元")]
        [Display(Name = "位置說明")]
        public string? PositionDescription { get; set; }

        // Navigation Properties
        /// <summary>
        /// 產品合成主檔
        /// </summary>
        public ProductComposition ProductComposition { get; set; } = null!;

        /// <summary>
        /// 組件產品
        /// </summary>
        public Product ComponentProduct { get; set; } = null!;

        /// <summary>
        /// 單位
        /// </summary>
        public Unit? Unit { get; set; }
    }
}
