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
    [Index(nameof(Code), IsUnique = true)]
    public class Employee : BaseEntity
    {
        /// <summary>
        /// 名字
        /// </summary>
        [Display(Name = "名字")]
        [MaxLength(25, ErrorMessage = "名字不可超過25個字元")]
        public string? Name { get; set; }

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
        /// 權限組ID
        /// </summary>
        [Display(Name = "權限組")]
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
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        public Gender? Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        [Display(Name = "生日")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Display(Name = "身分證字號")]
        [MaxLength(10)]
        public string? IdNumber { get; set; }

        /// <summary>
        /// 手機號碼
        /// </summary>
        [Display(Name = "手機")]
        [MaxLength(20)]
        public string? Mobile { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "Email")]
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// 緊急聯絡人
        /// </summary>
        [Display(Name = "緊急聯絡人")]
        [MaxLength(50)]
        public string? EmergencyContact { get; set; }

        /// <summary>
        /// 緊急聯絡電話
        /// </summary>
        [Display(Name = "緊急聯絡電話")]
        [MaxLength(20)]
        public string? EmergencyPhone { get; set; }

        /// <summary>
        /// 到職日期
        /// </summary>
        [Display(Name = "到職日期")]
        public DateTime? HireDate { get; set; }

        /// <summary>
        /// 離職日期
        /// </summary>
        [Display(Name = "離職日期")]
        public DateTime? ResignationDate { get; set; }

        /// <summary>
        /// 在職狀態
        /// </summary>
        [Display(Name = "在職狀態")]
        public EmployeeStatus EmploymentStatus { get; set; } = EmployeeStatus.Active;
        
        // 導航屬性
        public Role? Role { get; set; }
        public Department? Department { get; set; }
        public EmployeePosition? EmployeePosition { get; set; }
        
        // 聯絡資訊請使用 IContactService 取得 (OwnerType = "Employee", OwnerId = this.Id)
        // 地址資訊請使用 IAddressService 取得
    }

    /// <summary>
    /// 性別
    /// </summary>
    public enum Gender
    {
        [Display(Name = "男性")]
        Male = 1,
        
        [Display(Name = "女性")]
        Female = 2,
        
        [Display(Name = "其他")]
        Other = 3
    }

    /// <summary>
    /// 員工狀態
    /// </summary>
    public enum EmployeeStatus
    {
        [Display(Name = "試用期")]
        Probation = 1,
        
        [Display(Name = "在職")]
        Active = 2,
        
        [Display(Name = "留職停薪")]
        LeaveOfAbsence = 3,
        
        [Display(Name = "已離職")]
        Resigned = 4,
        
        [Display(Name = "停用")]
        Inactive = 5
    }
}
