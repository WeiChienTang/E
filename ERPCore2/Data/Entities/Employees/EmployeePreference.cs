using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工個人化設定 - 每位員工的偏好設定（1-to-1）
    /// 記錄不存在代表使用系統預設值
    /// </summary>
    [Index(nameof(EmployeeId), IsUnique = true)]
    public class EmployeePreference : BaseEntity
    {
        /// <summary>
        /// 關聯員工
        /// </summary>
        [Required]
        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        /// <summary>
        /// 介面語言
        /// </summary>
        [Display(Name = "介面語言")]
        public UILanguage Language { get; set; } = UILanguage.ZhTW;

        /// <summary>
        /// 字型縮放級別
        /// </summary>
        [Display(Name = "字型大小")]
        public ContentZoom Zoom { get; set; } = ContentZoom.Medium;

        /// <summary>
        /// 介面主題
        /// </summary>
        [Display(Name = "主題")]
        public AppTheme Theme { get; set; } = AppTheme.Light;

        // 導航屬性
        public Employee? Employee { get; set; }
    }

    /// <summary>
    /// 字型縮放級別列舉
    /// </summary>
    public enum ContentZoom
    {
        [Display(Name = "75%")]
        XSmall = 1,

        [Display(Name = "90%")]
        Small = 2,

        [Display(Name = "100%")]
        Medium = 3,

        [Display(Name = "110%")]
        Large = 4,

        [Display(Name = "125%")]
        XLarge = 5,

        [Display(Name = "150%")]
        XXLarge = 6
    }

    /// <summary>
    /// 介面主題列舉
    /// </summary>
    public enum AppTheme
    {
        [Display(Name = "淺色")]
        Light = 1,

        [Display(Name = "深色")]
        Dark = 2
    }

    /// <summary>
    /// 介面語言列舉
    /// </summary>
    public enum UILanguage
    {
        [Display(Name = "繁體中文")]
        ZhTW = 1,

        [Display(Name = "English")]
        EnUS = 2,

        [Display(Name = "日本語")]
        JaJP = 3,

        [Display(Name = "简体中文")]
        ZhCN = 4,

        [Display(Name = "Filipino")]
        FilPH = 5
    }
}
