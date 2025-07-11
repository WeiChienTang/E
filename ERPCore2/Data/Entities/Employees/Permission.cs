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
        /// 權限代碼 (如: Customer.View, Customer.Create)
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

        /// <summary>
        /// 模組名稱 (如: Customer, Order, Product)
        /// </summary>
        [Display(Name = "模組")]
        [Required(ErrorMessage = "請輸入模組")]
        [MaxLength(50, ErrorMessage = "模組不可超過50個字元")]
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// 動作類型 (如: Read, Create, Update, Delete)
        /// </summary>
        [Display(Name = "動作")]
        [Required(ErrorMessage = "請輸入動作")]
        [MaxLength(50, ErrorMessage = "動作不可超過50個字元")]
        public string Action { get; set; } = string.Empty;
        /// <summary>
        /// 權限群組 (用於UI分組顯示)
        /// </summary>
        [Display(Name = "權限群組")]
        [MaxLength(50, ErrorMessage = "權限群組不可超過50個字元")]
        public string? PermissionGroup { get; set; }

        // 導航屬性
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
