using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 產品合成主檔（BOM 主檔）- 定義成品與其組成零件的關係
    /// </summary>
    public class ProductComposition : BaseEntity
    {
        // 基本資訊
        [Required(ErrorMessage = "成品為必填")]
        [Display(Name = "成品")]
        [ForeignKey(nameof(ParentProduct))]
        public int ParentProductId { get; set; }

        [Required(ErrorMessage = "合成表名稱為必填")]
        [MaxLength(100, ErrorMessage = "合成表名稱不可超過100個字元")]
        [Display(Name = "合成表名稱")]
        public string Name { get; set; } = string.Empty;

        // 設定
        [Required(ErrorMessage = "合成類型為必填")]
        [Display(Name = "合成類型")]
        public CompositionType CompositionType { get; set; } = CompositionType.Standard;

        // Navigation Properties
        /// <summary>
        /// 成品（父產品）
        /// </summary>
        public Product ParentProduct { get; set; } = null!;

        /// <summary>
        /// 合成明細列表
        /// </summary>
        public ICollection<ProductCompositionDetail> CompositionDetails { get; set; } = new List<ProductCompositionDetail>();
    }
}
