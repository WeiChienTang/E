using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 部門實體 - 組織架構管理
    /// </summary>
    public class Department : BaseEntity
    {
        /// <summary>
        /// 部門代碼
        /// </summary>
        [Display(Name = "部門代碼")]
        [Required(ErrorMessage = "請輸入部門代碼")]
        [MaxLength(20, ErrorMessage = "部門代碼不可超過20個字元")]
        public string DepartmentCode { get; set; } = string.Empty;

        /// <summary>
        /// 部門名稱
        /// </summary>
        [Display(Name = "部門名稱")]
        [Required(ErrorMessage = "請輸入部門名稱")]
        [MaxLength(50, ErrorMessage = "部門名稱不可超過50個字元")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 部門描述
        /// </summary>
        [Display(Name = "部門描述")]
        [MaxLength(200, ErrorMessage = "部門描述不可超過200個字元")]
        public string? Description { get; set; }

        /// <summary>
        /// 上級部門ID
        /// </summary>
        [Display(Name = "上級部門")]
        public int? ParentDepartmentId { get; set; }

        /// <summary>
        /// 部門主管ID
        /// </summary>
        [Display(Name = "部門主管")]
        public int? ManagerId { get; set; }

        /// <summary>
        /// 排序順序
        /// </summary>
        [Display(Name = "排序順序")]
        public int SortOrder { get; set; } = 0;

        // 導航屬性
        /// <summary>
        /// 上級部門
        /// </summary>
        public Department? ParentDepartment { get; set; }

        /// <summary>
        /// 下級部門
        /// </summary>
        public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

        /// <summary>
        /// 部門員工
        /// </summary>
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
