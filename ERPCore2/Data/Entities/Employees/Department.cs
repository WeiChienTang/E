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
        [Required(ErrorMessage = "請輸入部門名稱")]
        [MaxLength(50, ErrorMessage = "部門名稱不可超過50個字元")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 部門主管ID
        /// </summary>
        [Display(Name = "部門主管")]
        [ForeignKey(nameof(Manager))]
        public int? ManagerId { get; set; }

        // 導航屬性
        /// <summary>
        /// 部門主管
        /// </summary>
        public Employee? Manager { get; set; }

        /// <summary>
        /// 部門員工
        /// </summary>
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
