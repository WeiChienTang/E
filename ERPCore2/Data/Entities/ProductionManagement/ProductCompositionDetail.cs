using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 品項合成明細檔（BOM 明細）- 記錄每個成品所需的原料/半成品及其數量
    /// </summary>
    [Index(nameof(ComponentItemId))]
    [Index(nameof(ItemCompositionId), nameof(ComponentItemId), IsUnique = true)]
    public class ItemCompositionDetail : BaseEntity
    {
        // 關聯資訊
        [Required(ErrorMessage = "品項合成主檔為必填")]
        [Display(Name = "品項合成主檔")]
        [ForeignKey(nameof(ItemComposition))]
        public int ItemCompositionId { get; set; }

        [Required(ErrorMessage = "組件品項為必填")]
        [Display(Name = "組件品項")]
        [ForeignKey(nameof(ComponentItem))]
        public int ComponentItemId { get; set; }

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
        /// 品項合成主檔
        /// </summary>
        public ItemComposition ItemComposition { get; set; } = null!;

        /// <summary>
        /// 組件品項
        /// </summary>
        public Item ComponentItem { get; set; } = null!;

        /// <summary>
        /// 單位
        /// </summary>
        public Unit? Unit { get; set; }
    }
}
