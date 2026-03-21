using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 潛在客戶（CRM Lead）
    /// </summary>
    public class CrmLead : BaseEntity
    {
        // ===== 公司基本資料 =====

        /// <summary>公司名稱</summary>
        [Required(ErrorMessage = "請輸入公司名稱")]
        [MaxLength(200, ErrorMessage = "公司名稱不可超過200個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>聯絡人姓名</summary>
        [MaxLength(100, ErrorMessage = "聯絡人不可超過100個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }

        /// <summary>聯絡電話</summary>
        [MaxLength(50, ErrorMessage = "聯絡電話不可超過50個字元")]
        [Display(Name = "聯絡電話")]
        public string? ContactPhone { get; set; }

        /// <summary>電子郵件</summary>
        [MaxLength(200, ErrorMessage = "Email 不可超過200個字元")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>行業 / 產業別</summary>
        [MaxLength(100, ErrorMessage = "行業不可超過100個字元")]
        [Display(Name = "行業")]
        public string? Industry { get; set; }

        // ===== 開發資訊 =====

        /// <summary>來源</summary>
        [Required(ErrorMessage = "請選擇來源")]
        [Display(Name = "來源")]
        public LeadSource LeadSource { get; set; } = LeadSource.BusinessDevelopment;

        /// <summary>開發階段</summary>
        [Required(ErrorMessage = "請選擇開發階段")]
        [Display(Name = "開發階段")]
        public LeadStage LeadStage { get; set; } = LeadStage.Cold;

        /// <summary>預估商機金額</summary>
        [Display(Name = "預估商機金額")]
        [Range(0, 999999999999, ErrorMessage = "金額必須為非負數")]
        public decimal? EstimatedValue { get; set; }

        // ===== 負責業務 =====

        /// <summary>負責業務員 ID</summary>
        [Display(Name = "負責業務員")]
        public int? AssignedEmployeeId { get; set; }

        /// <summary>負責業務員</summary>
        public Employee? AssignedEmployee { get; set; }

        // ===== 轉換資訊（成交後） =====

        /// <summary>已轉換的正式客戶 ID（null 表示尚未成交）</summary>
        [Display(Name = "轉換客戶")]
        public int? ConvertedCustomerId { get; set; }

        /// <summary>已轉換的正式客戶</summary>
        public Customer? ConvertedCustomer { get; set; }

        /// <summary>成交轉換時間</summary>
        [Display(Name = "成交時間")]
        public DateTime? ConvertedAt { get; set; }

        // ===== 導航屬性 =====

        /// <summary>跟進紀錄</summary>
        public ICollection<CrmLeadFollowUp> FollowUps { get; set; } = new List<CrmLeadFollowUp>();
    }
}
