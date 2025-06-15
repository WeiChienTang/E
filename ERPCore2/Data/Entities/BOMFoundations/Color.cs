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

        [Required(ErrorMessage = "顏色代碼為必填")]
        [MaxLength(20, ErrorMessage = "顏色代碼不可超過20個字元")]
        [Display(Name = "顏色代碼")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [MaxLength(7, ErrorMessage = "十六進位色碼不可超過7個字元")]
        [Display(Name = "十六進位色碼")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "十六進位色碼格式不正確，應為 #RRGGBB 格式")]
        public string? HexCode { get; set; }

        [Range(0, 255, ErrorMessage = "紅色值應在0到255之間")]
        [Display(Name = "紅色值")]
        public int? RedValue { get; set; }

        [Range(0, 255, ErrorMessage = "綠色值應在0到255之間")]
        [Display(Name = "綠色值")]
        public int? GreenValue { get; set; }

        [Range(0, 255, ErrorMessage = "藍色值應在0到255之間")]
        [Display(Name = "藍色值")]
        public int? BlueValue { get; set; }
    }
}