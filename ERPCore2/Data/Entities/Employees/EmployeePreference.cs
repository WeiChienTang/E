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

        // 導航屬性
        public Employee? Employee { get; set; }
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
