using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工工具配給紀錄
    /// </summary>
    public class EmployeeTool : BaseEntity
    {
        /// <summary>所屬員工 ID</summary>
        [Required(ErrorMessage = "請選擇員工")]
        [Display(Name = "員工")]
        public int EmployeeId { get; set; }

        /// <summary>所屬員工</summary>
        public Employee? Employee { get; set; }

        /// <summary>工具名稱</summary>
        [Required(ErrorMessage = "請輸入工具名稱")]
        [MaxLength(50, ErrorMessage = "工具名稱不可超過50個字元")]
        [Display(Name = "工具名稱")]
        public string ToolName { get; set; } = string.Empty;

        /// <summary>工具序號/型號</summary>
        [MaxLength(50, ErrorMessage = "序號/型號不可超過50個字元")]
        [Display(Name = "序號/型號")]
        public string? ToolCode { get; set; }

        /// <summary>配給日期</summary>
        [Required(ErrorMessage = "請輸入配給日期")]
        [Display(Name = "配給日期")]
        public DateTime AssignedDate { get; set; } = DateTime.Today;

        /// <summary>歸還日期（未歸還則為空）</summary>
        [Display(Name = "歸還日期")]
        public DateTime? ReturnedDate { get; set; }

    }
}
