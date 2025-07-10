using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 角色權限關聯實體 - 多對多關聯表
    /// 用於建立系統角色(Role)與功能權限(Permission)之間的關聯
    /// 例如：「財務人員」角色擁有「Customer.View」和「Invoice.Create」權限
    /// </summary>
    public class RolePermission : BaseEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Display(Name = "角色")]
        [Required(ErrorMessage = "請選擇角色")]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        /// <summary>
        /// 權限ID
        /// </summary>
        [Display(Name = "權限")]
        [Required(ErrorMessage = "請選擇權限")]
        [ForeignKey(nameof(Permission))]
        public int PermissionId { get; set; }

        // 導航屬性
        public Role Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
