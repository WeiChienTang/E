using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 部門實體 - 組織架構管理
    /// </summary>
    public class Department : BaseEntity
    {
        /// <summary>
        /// 部門名稱
        /// </summary>
        [Display(Name = "部門名稱")]
        [MaxLength(50, ErrorMessage = "部門名稱不可超過50個字元")]
        public string? Name { get; set; }

        /// <summary>
        /// 上級部門ID
        /// </summary>
        [Display(Name = "上級部門")]
        [ForeignKey(nameof(ParentDepartment))]
        public int? ParentDepartmentId { get; set; }

        /// <summary>
        /// 部門主管ID
        /// </summary>
        [Display(Name = "部門主管")]
        [ForeignKey(nameof(Manager))]
        public int? ManagerId { get; set; }

        /// <summary>
        /// 代理主管ID
        /// </summary>
        [Display(Name = "代理主管")]
        [ForeignKey(nameof(DeputyManager))]
        public int? DeputyManagerId { get; set; }

        /// <summary>
        /// 部門電話
        /// </summary>
        [Display(Name = "部門電話")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// 辦公地點
        /// </summary>
        [Display(Name = "辦公地點")]
        [MaxLength(100)]
        public string? Location { get; set; }

        // 導航屬性
        /// <summary>
        /// 上級部門
        /// </summary>
        public Department? ParentDepartment { get; set; }

        /// <summary>
        /// 子部門
        /// </summary>
        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();

        /// <summary>
        /// 部門主管
        /// </summary>
        public Employee? Manager { get; set; }

        /// <summary>
        /// 代理主管
        /// </summary>
        public Employee? DeputyManager { get; set; }

        /// <summary>
        /// 部門員工
        /// </summary>
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
