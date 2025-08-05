using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 權限實體 - 系統功能權限定義
    /// </summary>
    [Index(nameof(PermissionCode), IsUnique = true)]
    public class Permission : BaseEntity
    {
        /// <summary>
        /// 權限代碼 (Customer.Create)
        /// </summary>
        [Display(Name = "權限代碼")]
        [Required(ErrorMessage = "請輸入權限代碼")]
        [MaxLength(100, ErrorMessage = "權限代碼不可超過100個字元")]
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// 權限名稱 (如: 客戶查看, 客戶新增)
        /// </summary>
        [Display(Name = "權限名稱")]
        [Required(ErrorMessage = "請輸入權限名稱")]
        [MaxLength(100, ErrorMessage = "權限名稱不可超過100個字元")]
        public string PermissionName { get; set; } = string.Empty;

        // 導航屬性
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
