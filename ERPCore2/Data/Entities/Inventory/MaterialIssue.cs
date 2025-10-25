using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 領貨主檔實體 - 記錄領貨單基本資訊
    /// 使用 BaseEntity.Code 作為領貨單號
    /// 使用 BaseEntity.Remarks 作為備註/用途說明
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(IssueDate))]
    [Index(nameof(EmployeeId), nameof(IssueDate))]
    [Index(nameof(DepartmentId), nameof(IssueDate))]
    public class MaterialIssue : BaseEntity
    {
        [Required(ErrorMessage = "領貨日期為必填")]
        [Display(Name = "領貨日期")]
        public DateTime IssueDate { get; set; } = DateTime.Now;
        
        [Display(Name = "領料人員")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }
        
        [Display(Name = "領料部門")]
        [ForeignKey(nameof(Department))]
        public int? DepartmentId { get; set; }
        
        [Display(Name = "總數量")]
        [NotMapped]
        public int TotalQuantity => MaterialIssueDetails?.Sum(d => d.IssueQuantity) ?? 0;
        
        [Display(Name = "明細筆數")]
        [NotMapped]
        public int DetailCount => MaterialIssueDetails?.Count ?? 0;
        
        // Navigation Properties
        public Employee? Employee { get; set; }
        public Department? Department { get; set; }
        public ICollection<MaterialIssueDetail> MaterialIssueDetails { get; set; } = new List<MaterialIssueDetail>();
    }
}
