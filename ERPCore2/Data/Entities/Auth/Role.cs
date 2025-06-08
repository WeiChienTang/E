using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 角色實體 - 權限群組管理
    /// </summary>
    public class Role : BaseEntity
    {
        /// <summary>
        /// 角色名稱
        /// </summary>
        [Display(Name = "角色名稱")]
        [Required(ErrorMessage = "請輸入角色名稱")]
        [MaxLength(50, ErrorMessage = "角色名稱不可超過50個字元")]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [Display(Name = "角色描述")]
        [MaxLength(200, ErrorMessage = "角色描述不可超過200個字元")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否為系統預設角色
        /// </summary>
        [Display(Name = "系統預設角色")]
        public bool IsSystemRole { get; set; } = false;

        // 導航屬性
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
