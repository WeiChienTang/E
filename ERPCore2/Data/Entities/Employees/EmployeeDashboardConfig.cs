using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工儀表板配置 - 每位員工的個人首頁配置
    /// 使用 NavigationItemKey 對應 NavigationConfig 中的導航項目
    /// </summary>
    [Index(nameof(EmployeeId), nameof(NavigationItemKey), IsUnique = true)]
    [Index(nameof(EmployeeId), nameof(SortOrder))]
    public class EmployeeDashboardConfig : BaseEntity
    {
        /// <summary>
        /// 所屬員工
        /// </summary>
        [Required]
        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        /// <summary>
        /// 導航項目識別鍵（對應 NavigationConfig 中的 Route 或 ActionId）
        /// 格式：Route 類型使用路由路徑（如 "/employees"）；Action 類型使用 "Action:{ActionId}"
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "導航項目識別鍵")]
        public string NavigationItemKey { get; set; } = string.Empty;

        /// <summary>
        /// 顯示排序（數字越小越前面）
        /// </summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否顯示
        /// </summary>
        [Display(Name = "是否顯示")]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 個人化參數（JSON 格式，預留給未來進階功能）
        /// </summary>
        [Display(Name = "個人化設定")]
        public string? WidgetSettings { get; set; }

        // 導航屬性
        public Employee Employee { get; set; } = null!;
    }
}
