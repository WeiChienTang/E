using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 權限組實體 - 系統權限群組管理
    /// 用於控制員工在系統中的功能權限（如：管理員、財務人員、業務員等）
    /// 注意：此為系統權限組，與 EmployeePosition（組織職位）不同
    /// </summary>
    public class Role : BaseEntity
    {
        /// <summary>
        /// 權限組名稱
        /// </summary>
        [Display(Name = "權限組名稱")]
        [Required(ErrorMessage = "請輸入權限組名稱")]
        [MaxLength(50, ErrorMessage = "權限組名稱不可超過50個字元")]
        public string Name { get; set; } = string.Empty;

        // 導航屬性
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
