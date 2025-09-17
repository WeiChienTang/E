using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 系統參數實體 - 儲存系統全域設定參數
    /// </summary>
    public class SystemParameter : BaseEntity
    {
        /// <summary>
        /// 稅率 (%)
        /// </summary>
        [Display(Name = "稅率 (%)")]
        [Range(0.00, 100.00, ErrorMessage = "稅率範圍必須在 0% 到 100% 之間")]
        public decimal TaxRate { get; set; } = 5.00m; // 預設 5% 稅率
    }
}