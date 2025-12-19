using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 權限實體 - 系統功能權限定義
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Permission : BaseEntity
    {
        /// <summary>
        /// 權限名稱 (如: 客戶查看, 客戶新增)
        /// </summary>
        [Display(Name = "權限名稱")]
        [Required(ErrorMessage = "請輸入權限名稱")]
        [MaxLength(100, ErrorMessage = "權限名稱不可超過100個字元")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 權限級別 (一般/敏感)
        /// </summary>
        [Display(Name = "權限級別")]
        public PermissionLevel Level { get; set; } = PermissionLevel.Normal;

        // 導航屬性
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
