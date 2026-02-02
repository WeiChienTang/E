using System.ComponentModel.DataAnnotations;
using ERPCore2.Data;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 報表列印配置實體 - 管理不同報表的列印設定
    /// </summary>
    public class ReportPrintConfiguration : BaseEntity
    {
        /// <summary>
        /// 報表識別碼（對應 ReportRegistry 中的 Id，如 AR001、AP001）
        /// </summary>
        [Required(ErrorMessage = "報表識別碼為必填")]
        [MaxLength(50, ErrorMessage = "報表識別碼不可超過50個字元")]
        [Display(Name = "報表識別碼")]
        public string ReportId { get; set; } = string.Empty;

        /// <summary>
        /// 報表顯示名稱
        /// </summary>
        [Required(ErrorMessage = "報表名稱為必填")]
        [MaxLength(100, ErrorMessage = "報表名稱不可超過100個字元")]
        [Display(Name = "報表名稱")]
        public string ReportName { get; set; } = string.Empty;

        /// <summary>
        /// 印表機設定ID
        /// </summary>
        [Display(Name = "印表機設定")]
        public int? PrinterConfigurationId { get; set; }

        /// <summary>
        /// 紙張設定ID
        /// </summary>
        [Display(Name = "紙張設定")]
        public int? PaperSettingId { get; set; }

        // 導航屬性
        /// <summary>
        /// 印表機設定
        /// </summary>
        public PrinterConfiguration? PrinterConfiguration { get; set; }

        /// <summary>
        /// 紙張設定
        /// </summary>
        public PaperSetting? PaperSetting { get; set; }
    }
}
