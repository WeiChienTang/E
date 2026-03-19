using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 報價單組合明細檔 - 儲存報價單專屬的 BOM 組成資料
    /// 當報價單明細的品項有 BOM 組成時，可以複製並客製化其組成內容
    /// 這些資料不會影響原始的品項物料清單（ItemCompositionDetail）
    /// </summary>
    [Index(nameof(QuotationDetailId), nameof(ComponentItemId), IsUnique = true)]
    [Index(nameof(ComponentItemId))]
    public class QuotationCompositionDetail : BaseEntity
    {
        // 關聯資訊
        [Required(ErrorMessage = "報價單明細為必填")]
        [Display(Name = "報價單明細")]
        [ForeignKey(nameof(QuotationDetail))]
        public int QuotationDetailId { get; set; }

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
        /// 報價單明細
        /// </summary>
        public QuotationDetail QuotationDetail { get; set; } = null!;

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
