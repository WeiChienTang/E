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
        [Required(ErrorMessage = "合成類型為必填")]
        [Display(Name = "合成類型")]
        public CompositionType CompositionType { get; set; } = CompositionType.Standard;

        // Navigation Properties
        /// <summary>
        /// 成品（父產品）
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
        /// 合成明細列表
        /// </summary>
        public ICollection<ProductCompositionDetail> CompositionDetails { get; set; } = new List<ProductCompositionDetail>();
    }
}
