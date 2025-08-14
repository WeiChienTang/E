using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 顏色類型 - 產品基礎元素
    /// </summary>
    public class Color : BaseEntity
    {
        [Required(ErrorMessage = "顏色名稱為必填")]
        [MaxLength(50, ErrorMessage = "顏色名稱不可超過50個字元")]
        [Display(Name = "顏色名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
    }
}