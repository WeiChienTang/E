using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工實體 - 系統用戶和權限管理
    /// </summary>
    [Index(nameof(Account), IsUnique = true)]
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
        /// 是否為系統使用者
        /// </summary>
        [Display(Name = "系統使用者")]
        public bool IsSystemUser { get; set; } = false;

        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [MaxLength(50, ErrorMessage = "帳號不可超過50個字元")]
        public string? Account { get; set; }
        
        /// <summary>
        /// 密碼
        /// </summary>
        [Display(Name = "密碼")]
        [MaxLength(255, ErrorMessage = "密碼不可超過255個字元")]
        public string? Password { get; set; }

        /// <summary>
        /// 部門ID
        /// </summary>
        [Display(Name = "部門")]
        [ForeignKey(nameof(Department))]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// 職位ID
        /// </summary>
        [Display(Name = "職位")]
        [ForeignKey(nameof(EmployeePosition))]
        public int? EmployeePositionId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Display(Name = "角色")]
        [ForeignKey(nameof(Role))]
        public int? RoleId { get; set; }

        /// <summary>
        /// 最後登入時間
        /// </summary>
        [Display(Name = "最後登入時間")]
        public DateTime? LastLoginAt { get; set; }

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
        public Role? Role { get; set; }
        public Department? Department { get; set; }
        public EmployeePosition? EmployeePosition { get; set; }
        
        // 聯絡資訊請使用 IContactService 取得 (OwnerType = "Employee", OwnerId = this.Id)
        // 地址資訊請使用 IAddressService 取得
    }
}
