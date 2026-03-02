using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 廠商拜訪紀錄
    /// </summary>
    public class SupplierVisit : BaseEntity
    {
        /// <summary>所屬廠商 ID</summary>
        [Required(ErrorMessage = "請選擇廠商")]
        [Display(Name = "廠商")]
        public int SupplierId { get; set; }

        /// <summary>所屬廠商</summary>
        public Supplier? Supplier { get; set; }

        /// <summary>拜訪日期</summary>
        [Required(ErrorMessage = "請輸入拜訪日期")]
        [Display(Name = "拜訪日期")]
        public DateTime VisitDate { get; set; } = DateTime.Today;

        /// <summary>拜訪方式</summary>
        [Required(ErrorMessage = "請選擇拜訪方式")]
        [Display(Name = "拜訪方式")]
        public VisitType VisitType { get; set; } = VisitType.Phone;

        /// <summary>拜訪目的</summary>
        [MaxLength(100, ErrorMessage = "拜訪目的不可超過100個字元")]
        [Display(Name = "拜訪目的")]
        public string? Purpose { get; set; }

        /// <summary>拜訪內容</summary>
        [MaxLength(1000, ErrorMessage = "拜訪內容不可超過1000個字元")]
        [Display(Name = "拜訪內容")]
        public string? Content { get; set; }

        /// <summary>下次追蹤日期</summary>
        [Display(Name = "下次追蹤日期")]
        public DateTime? NextFollowUpDate { get; set; }

        /// <summary>拜訪業務人員 ID</summary>
        [Display(Name = "業務人員")]
        public int? EmployeeId { get; set; }

        /// <summary>拜訪業務人員</summary>
        public Employee? Employee { get; set; }
    }
}
