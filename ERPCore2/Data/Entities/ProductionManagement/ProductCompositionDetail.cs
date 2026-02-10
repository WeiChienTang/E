using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品合成明細檔（BOM 明細）- 記錄每個成品所需的原料/半成品及其數量
    /// </summary>
    [Index(nameof(ComponentProductId))]
    [Index(nameof(ProductCompositionId), nameof(ComponentProductId), IsUnique = true)]
    public class ProductCompositionDetail : BaseEntity
    {
        // 關聯資訊
        [Required(ErrorMessage = "商品合成主檔為必填")]
        [Display(Name = "商品合成主檔")]
        [ForeignKey(nameof(ProductComposition))]
        public int ProductCompositionId { get; set; }

        [Required(ErrorMessage = "組件商品為必填")]
        [Display(Name = "組件商品")]
        [ForeignKey(nameof(ComponentProduct))]
        public int ComponentProductId { get; set; }

        // 基本資訊
        [Required(ErrorMessage = "所需數量為必填")]
        [Display(Name = "所需數量")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; } = 1;

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        // 進階設定
        [Display(Name = "組件成本")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ComponentCost { get; set; }

        // Navigation Properties
        /// <summary>
        /// 商品合成主檔
        /// </summary>
        public ProductComposition ProductComposition { get; set; } = null!;

        /// <summary>
        /// 組件商品
        /// </summary>
        public Product ComponentProduct { get; set; } = null!;

        /// <summary>
        /// 單位
        /// </summary>
        public Unit? Unit { get; set; }
    }
}
