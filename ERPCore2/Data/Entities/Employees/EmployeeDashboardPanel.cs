using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工儀表板面板 - 每位員工可建立多個自訂面板
    /// 每個面板可包含多個 EmployeeDashboardConfig（頁面連結或快速功能）
    /// </summary>
    [Index(nameof(EmployeeId), nameof(SortOrder))]
    public class EmployeeDashboardPanel : BaseEntity
    {
        /// <summary>
        /// 所屬員工
        /// </summary>
        [Required]
        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        /// <summary>
        /// 面板標題（使用者自訂）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "面板標題")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 面板排序（數字越小越前面）
        /// </summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 面板圖示（選填，預設 bi-grid-fill）
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "圖示")]
        public string? IconClass { get; set; }

        // 導航屬性
        public Employee Employee { get; set; } = null!;
        
        /// <summary>
        /// 此面板包含的儀表板配置項目
        /// </summary>
        public ICollection<EmployeeDashboardConfig> DashboardConfigs { get; set; } = new List<EmployeeDashboardConfig>();
    }
}
