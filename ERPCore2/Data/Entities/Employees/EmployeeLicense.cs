using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工證照紀錄
    /// </summary>
    public class EmployeeLicense : BaseEntity
    {
        /// <summary>所屬員工 ID</summary>
        [Required(ErrorMessage = "請選擇員工")]
        [Display(Name = "員工")]
        public int EmployeeId { get; set; }

        /// <summary>所屬員工</summary>
        public Employee? Employee { get; set; }

        /// <summary>證照名稱</summary>
        [Required(ErrorMessage = "請輸入證照名稱")]
        [MaxLength(100, ErrorMessage = "證照名稱不可超過100個字元")]
        [Display(Name = "證照名稱")]
        public string LicenseName { get; set; } = string.Empty;

        /// <summary>證照字號</summary>
        [MaxLength(50, ErrorMessage = "證照字號不可超過50個字元")]
        [Display(Name = "證照字號")]
        public string? LicenseNumber { get; set; }

        /// <summary>發照機關</summary>
        [MaxLength(100, ErrorMessage = "發照機關不可超過100個字元")]
        [Display(Name = "發照機關")]
        public string? IssuingAuthority { get; set; }

        /// <summary>取得日期</summary>
        [Required(ErrorMessage = "請輸入取得日期")]
        [Display(Name = "取得日期")]
        public DateTime IssuedDate { get; set; } = DateTime.Today;

        /// <summary>到期日（永久有效則為空）</summary>
        [Display(Name = "到期日")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>到期前幾天標示警告</summary>
        [Display(Name = "提醒天數")]
        public int AlertDays { get; set; } = 30;
    }
}
