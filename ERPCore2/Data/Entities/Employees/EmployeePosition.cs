using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工職位實體 - 組織架構職位管理
    /// 用於定義員工在組織中的職位階層（如：部門經理、資深專員、助理等）
    /// 注意：此為組織職位，與 Role（系統角色）不同
    /// </summary>
    public class EmployeePosition : BaseEntity
    {
        /// <summary>
        /// 職位名稱
        /// </summary>
        [Required(ErrorMessage = "職位名稱為必填")]
        [MaxLength(50, ErrorMessage = "職位名稱不可超過50個字元")]
        [Display(Name = "職位名稱")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 職位描述
        /// </summary>
        [MaxLength(200, ErrorMessage = "職位描述不可超過200個字元")]
        [Display(Name = "職位描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 職位代碼
        /// </summary>
        [MaxLength(20, ErrorMessage = "職位代碼不可超過20個字元")]
        [Display(Name = "職位代碼")]
        public string? Code { get; set; }

        // 導航屬性
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
