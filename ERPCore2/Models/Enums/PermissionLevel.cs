using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 權限級別
    /// </summary>
    public enum PermissionLevel
    {
        /// <summary>
        /// 一般權限 - 可授予一般員工
        /// </summary>
        [Display(Name = "一般權限")]
        Normal = 1,

        /// <summary>
        /// 敏感權限 - 需要更高權限者授予，影響系統安全性
        /// </summary>
        [Display(Name = "敏感權限")]
        Sensitive = 2
    }
}
