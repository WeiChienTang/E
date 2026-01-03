using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品合成主檔（BOM 主檔）- 定義成品與其組成零件的關係
    /// </summary>
    public class ProductComposition : BaseEntity
    {
        // 基本資訊
        [Required(ErrorMessage = "成品為必填")]
        [Display(Name = "成品")]
        [ForeignKey(nameof(ParentProduct))]
        public int ParentProductId { get; set; }

        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }

        [MaxLength(200, ErrorMessage = "規格不可超過200個字元")]
        [Display(Name = "規格")]
        public string? Specification { get; set; }

        [Display(Name = "製單人員")]
        [ForeignKey(nameof(CreatedByEmployee))]
        public int? CreatedByEmployeeId { get; set; }

        // 設定
        [Display(Name = "物料清單類型")]
        [ForeignKey(nameof(CompositionCategory))]
        public int? CompositionCategoryId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 成品（父商品）
        /// </summary>
        public Product ParentProduct { get; set; } = null!;

        /// <summary>
        /// 客戶
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// 製單人員
        /// </summary>
        public Employee? CreatedByEmployee { get; set; }

        /// <summary>
        /// 物料清單類型
        /// </summary>
        public CompositionCategory? CompositionCategory { get; set; }

        /// <summary>
        /// 合成明細列表
        /// </summary>
        public ICollection<ProductCompositionDetail> CompositionDetails { get; set; } = new List<ProductCompositionDetail>();
    }
}
