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
    }
}
