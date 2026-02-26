using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 客戶拜訪紀錄
    /// </summary>
    public class CustomerVisit : BaseEntity
    {
        /// <summary>所屬客戶 ID</summary>
        [Required(ErrorMessage = "請選擇客戶")]
        [Display(Name = "客戶")]
        public int CustomerId { get; set; }

        /// <summary>所屬客戶</summary>
        public Customer? Customer { get; set; }

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

        /// <summary>結果摘要</summary>
        [MaxLength(200, ErrorMessage = "結果摘要不可超過200個字元")]
        [Display(Name = "結果摘要")]
        public string? Result { get; set; }

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
