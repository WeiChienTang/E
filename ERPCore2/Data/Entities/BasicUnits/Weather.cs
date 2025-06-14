using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 天氣類型 - 基礎單位資料
    /// </summary>
    public class Weather : BaseEntity
    {
        [Required(ErrorMessage = "天氣名稱為必填")]
        [MaxLength(50, ErrorMessage = "天氣名稱不可超過50個字元")]
        [Display(Name = "天氣名稱")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "天氣代碼為必填")]
        [MaxLength(20, ErrorMessage = "天氣代碼不可超過20個字元")]
        [Display(Name = "天氣代碼")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "圖示不可超過50個字元")]
        [Display(Name = "圖示")]
        public string? Icon { get; set; }

        [Range(-50, 50, ErrorMessage = "溫度範圍應在-50到50度之間")]
        [Display(Name = "參考溫度")]
        public decimal? ReferenceTemperature { get; set; }
    }
}
