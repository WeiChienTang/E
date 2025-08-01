using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 天氣類型 - 產品基礎元素
    /// </summary>
    public class Weather : BaseEntity
    {
        [Required(ErrorMessage = "天氣名稱為必填")]
        [MaxLength(50, ErrorMessage = "天氣名稱不可超過50個字元")]
        [Display(Name = "天氣名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "天氣代碼不可超過20個字元")]
        [Display(Name = "天氣代碼")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
    }
}