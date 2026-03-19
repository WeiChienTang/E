using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 客戶投訴紀錄
    /// </summary>
    public class CustomerComplaint : BaseEntity
    {
        /// <summary>所屬客戶 ID</summary>
        [Required(ErrorMessage = "請選擇客戶")]
        [Display(Name = "客戶")]
        public int CustomerId { get; set; }

        /// <summary>所屬客戶</summary>
        public Customer? Customer { get; set; }

        /// <summary>投訴日期</summary>
        [Required(ErrorMessage = "請輸入投訴日期")]
        [Display(Name = "投訴日期")]
        public DateTime ComplaintDate { get; set; } = DateTime.Today;

        /// <summary>投訴標題</summary>
        [Required(ErrorMessage = "請輸入投訴標題")]
        [MaxLength(200, ErrorMessage = "投訴標題不可超過200個字元")]
        [Display(Name = "投訴標題")]
        public string Title { get; set; } = string.Empty;

        /// <summary>投訴類別</summary>
        [Required(ErrorMessage = "請選擇投訴類別")]
        [Display(Name = "投訴類別")]
        public ComplaintCategory Category { get; set; } = ComplaintCategory.ItemQuality;

        /// <summary>投訴描述</summary>
        [MaxLength(2000, ErrorMessage = "投訴描述不可超過2000個字元")]
        [Display(Name = "投訴描述")]
        public string? Description { get; set; }

        /// <summary>處理狀態</summary>
        [Required(ErrorMessage = "請選擇處理狀態")]
        [Display(Name = "處理狀態")]
        public ComplaintStatus ComplaintStatus { get; set; } = ComplaintStatus.Open;

        /// <summary>負責處理人員 ID</summary>
        [Display(Name = "負責人員")]
        public int? EmployeeId { get; set; }

        /// <summary>負責處理人員</summary>
        public Employee? Employee { get; set; }

        /// <summary>處理方式說明</summary>
        [MaxLength(2000, ErrorMessage = "處理方式不可超過2000個字元")]
        [Display(Name = "處理方式")]
        public string? Resolution { get; set; }

        /// <summary>解決日期</summary>
        [Display(Name = "解決日期")]
        public DateTime? ResolvedDate { get; set; }

        /// <summary>預計追蹤日期</summary>
        [Display(Name = "追蹤日期")]
        public DateTime? FollowUpDate { get; set; }
    }
}
