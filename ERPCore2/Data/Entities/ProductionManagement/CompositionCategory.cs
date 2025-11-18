using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 合成表類型主檔
    /// </summary>
    public class CompositionCategory : BaseEntity
    {
        [Required(ErrorMessage = "名稱為必填")]
        [MaxLength(50, ErrorMessage = "名稱不可超過50個字元")]
        [Display(Name = "名稱")]
        public string Name { get; set; } = string.Empty;

        // Navigation Properties
        /// <summary>
        /// 使用此類型的產品合成記錄
        /// </summary>
        public ICollection<ProductComposition> ProductCompositions { get; set; } = new List<ProductComposition>();
    }
}
