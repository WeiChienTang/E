using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 貨幣實體
    /// </summary>
    public class Currency : ERPCore2.Data.BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "貨幣名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        [Display(Name = "符號")]
        public string? Symbol { get; set; }

        [Display(Name = "是否為本位幣")]
        public bool IsBaseCurrency { get; set; } = false;

        // 以本位幣為基準的匯率（例如 1 USD = 30.5 TWD => ExchangeRate = 30.5 when base is TWD）
        [Display(Name = "匯率")]
        public decimal ExchangeRate { get; set; } = 1m;
    }
}
