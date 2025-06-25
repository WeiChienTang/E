using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 材質實體 - 用於定義產品的材質類型
    /// </summary>
    public class Material : BaseEntity
    {
        /// <summary>
        /// 材質代碼
        /// </summary>
        [Required(ErrorMessage = "材質代碼為必填")]
        [MaxLength(20, ErrorMessage = "材質代碼不可超過20個字元")]
        [Display(Name = "材質代碼")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 材質名稱
        /// </summary>
        [Required(ErrorMessage = "材質名稱為必填")]
        [MaxLength(50, ErrorMessage = "材質名稱不可超過50個字元")]
        [Display(Name = "材質名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 材質描述
        /// </summary>
        [MaxLength(200, ErrorMessage = "材質描述不可超過200個字元")]
        [Display(Name = "材質描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 材質類別
        /// </summary>
        [MaxLength(50, ErrorMessage = "材質類別不可超過50個字元")]
        [Display(Name = "材質類別")]
        public string? Category { get; set; }

        /// <summary>
        /// 密度 (g/cm³)
        /// </summary>
        [Display(Name = "密度 (g/cm³)")]
        [Column(TypeName = "decimal(10,4)")]
        public decimal? Density { get; set; }

        /// <summary>
        /// 熔點 (°C)
        /// </summary>
        [Display(Name = "熔點 (°C)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? MeltingPoint { get; set; }

        /// <summary>
        /// 是否環保材質
        /// </summary>
        [Display(Name = "環保材質")]
        public bool IsEcoFriendly { get; set; }

        /// <summary>
        /// 供應商ID (選擇性關聯)
        /// </summary>
        [Display(Name = "主要供應商")]
        public int? SupplierId { get; set; }

        // 導航屬性
        /// <summary>
        /// 主要供應商
        /// </summary>
        public Supplier? Supplier { get; set; }
    }
}
