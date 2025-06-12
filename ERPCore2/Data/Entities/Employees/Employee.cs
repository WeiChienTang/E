using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工實體 - 系統用戶和權限管理
    /// </summary>
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(EmployeeCode), IsUnique = true)]
    public class Employee : BaseEntity
    {
        /// <summary>
        /// 員工代碼
        /// </summary>
        [Display(Name = "員工代碼")]
        [Required(ErrorMessage = "請輸入員工代碼")]
        [MaxLength(20, ErrorMessage = "員工代碼不可超過20個字元")]
        public string EmployeeCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 名字
        /// </summary>
        [Display(Name = "名字")]
        [MaxLength(25, ErrorMessage = "名字不可超過25個字元")]
        public string? FirstName { get; set; }

        /// <summary>
        /// 姓氏
        /// </summary>
        [Display(Name = "姓氏")]
        [MaxLength(25, ErrorMessage = "姓氏不可超過25個字元")]
        public string? LastName { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required(ErrorMessage = "請輸入帳號")]
        [MaxLength(50, ErrorMessage = "帳號不可超過50個字元")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密碼雜湊
        /// </summary>
        [Display(Name = "密碼雜湊")]
        [Required(ErrorMessage = "請設定密碼")]
        [MaxLength(255, ErrorMessage = "密碼雜湊不可超過255個字元")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件")]
        [MaxLength(100, ErrorMessage = "電子郵件不可超過100個字元")]
        public string? Email { get; set; }

        /// <summary>
        /// 部門
        /// </summary>
        [Display(Name = "部門")]
        [MaxLength(50, ErrorMessage = "部門不可超過50個字元")]
        public string? Department { get; set; }

        /// <summary>
        /// 職位
        /// </summary>
        [Display(Name = "職位")]
        [MaxLength(50, ErrorMessage = "職位不可超過50個字元")]
        public string? Position { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Display(Name = "角色")]
        [Required(ErrorMessage = "請選擇角色")]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }        /// <summary>
        /// 最後登入時間
        /// </summary>
        [Display(Name = "最後登入時間")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 帳號是否鎖定
        /// </summary>
        [Display(Name = "帳號鎖定")]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// 登入失敗次數
        /// </summary>
        [Display(Name = "登入失敗次數")]
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// 帳號鎖定時間
        /// </summary>
        [Display(Name = "鎖定時間")]
        public DateTime? LockedAt { get; set; }

        // 導航屬性
        public Role Role { get; set; } = null!;
    }
}
