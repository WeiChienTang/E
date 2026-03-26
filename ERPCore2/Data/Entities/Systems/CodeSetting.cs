using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 代碼自動產生設定 - 每個模組一筆設定，控制自動編號的格式與流水號
    /// </summary>
    public class CodeSetting : BaseEntity
    {
        /// <summary>
        /// 模組識別鍵，例如 "Customer"、"PurchaseOrder"
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "模組識別鍵")]
        public string ModuleKey { get; set; } = string.Empty;

        /// <summary>
        /// UI 顯示名稱，例如 "客戶"（僅顯示用，不影響邏輯）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "模組顯示名稱")]
        public string ModuleDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用自動產生代碼
        /// </summary>
        [Display(Name = "啟用自動編號")]
        public bool IsAutoCode { get; set; } = true;

        /// <summary>
        /// 前綴字串，例如 "CUS"、"PO"
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Display(Name = "前綴")]
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// 格式樣板字串，例如 "{Prefix}-{Year:yy}{Seq:4}"
        /// 支援的 Token: {Prefix}, {Year:yy}, {Year:yyyy}, {Month:MM}, {Day:dd}, {Seq:N}
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "格式樣板")]
        public string FormatTemplate { get; set; } = string.Empty;

        /// <summary>
        /// 目前流水號（不包含日期部分）
        /// </summary>
        [Display(Name = "目前流水號")]
        public int CurrentSeq { get; set; } = 0;

        /// <summary>
        /// 目前已使用的年份（用於判斷是否需要年度重置）
        /// </summary>
        [Display(Name = "目前年份")]
        public int? CurrentYear { get; set; }

        /// <summary>
        /// 目前已使用的月份（用於判斷是否需要月度重置）
        /// </summary>
        [Display(Name = "目前月份")]
        public int? CurrentMonth { get; set; }

        /// <summary>
        /// 樂觀鎖版本戳記（EF Core concurrency token）
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
