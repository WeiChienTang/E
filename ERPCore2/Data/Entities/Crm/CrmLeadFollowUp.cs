using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 潛在客戶跟進紀錄
    /// </summary>
    public class CrmLeadFollowUp : BaseEntity
    {
        /// <summary>所屬潛在客戶 ID</summary>
        [Required(ErrorMessage = "請選擇潛在客戶")]
        [Display(Name = "潛在客戶")]
        public int CrmLeadId { get; set; }

        /// <summary>所屬潛在客戶</summary>
        public CrmLead? CrmLead { get; set; }

        /// <summary>跟進日期</summary>
        [Required(ErrorMessage = "請輸入跟進日期")]
        [Display(Name = "跟進日期")]
        public DateTime FollowUpDate { get; set; } = DateTime.Today;

        /// <summary>跟進方式（與客戶拜訪共用 VisitType 列舉）</summary>
        [Required(ErrorMessage = "請選擇跟進方式")]
        [Display(Name = "跟進方式")]
        public VisitType FollowUpType { get; set; } = VisitType.Phone;

        /// <summary>跟進內容</summary>
        [MaxLength(1000, ErrorMessage = "跟進內容不可超過1000個字元")]
        [Display(Name = "跟進內容")]
        public string? Content { get; set; }

        /// <summary>跟進後本次結果 / 結論</summary>
        [MaxLength(500, ErrorMessage = "本次結論不可超過500個字元")]
        [Display(Name = "本次結論")]
        public string? Conclusion { get; set; }

        /// <summary>下次跟進日期</summary>
        [Display(Name = "下次跟進日期")]
        public DateTime? NextFollowUpDate { get; set; }

        /// <summary>跟進人員 ID</summary>
        [Display(Name = "跟進人員")]
        public int? EmployeeId { get; set; }

        /// <summary>跟進人員</summary>
        public Employee? Employee { get; set; }
    }
}
