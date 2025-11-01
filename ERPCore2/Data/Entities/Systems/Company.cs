using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 公司實體 - 設定自家公司資訊，用於報表輸出
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Company : BaseEntity
    {
        /// <summary>
        /// 公司名稱
        /// </summary>
        [Required(ErrorMessage = "公司名稱為必填")]
        [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// 公司英文名稱
        /// </summary>
        [MaxLength(100, ErrorMessage = "公司英文名稱不可超過100個字元")]
        [Display(Name = "公司英文名稱")]
        public string? CompanyNameEn { get; set; }

        /// <summary>
        /// 統一編號
        /// </summary>
        [MaxLength(8, ErrorMessage = "統一編號不可超過8個字元")]
        [Display(Name = "統一編號")]
        public string? TaxId { get; set; }

        /// <summary>
        /// 負責人姓名
        /// </summary>
        [MaxLength(50, ErrorMessage = "負責人姓名不可超過50個字元")]
        [Display(Name = "負責人")]
        public string? Representative { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        [MaxLength(200, ErrorMessage = "公司地址不可超過200個字元")]
        [Display(Name = "公司地址")]
        public string? Address { get; set; }

        /// <summary>
        /// 公司電話
        /// </summary>
        [MaxLength(20, ErrorMessage = "公司電話不可超過20個字元")]
        [Display(Name = "公司電話")]
        public string? Phone { get; set; }

        /// <summary>
        /// 公司傳真
        /// </summary>
        [MaxLength(20, ErrorMessage = "公司傳真不可超過20個字元")]
        [Display(Name = "公司傳真")]
        public string? Fax { get; set; }

        /// <summary>
        /// 公司Email
        /// </summary>
        [MaxLength(100, ErrorMessage = "公司Email不可超過100個字元")]
        [EmailAddress(ErrorMessage = "請輸入正確的Email格式")]
        [Display(Name = "公司Email")]
        public string? Email { get; set; }

        /// <summary>
        /// 公司網站
        /// </summary>
        [MaxLength(100, ErrorMessage = "公司網站不可超過100個字元")]
        [Url(ErrorMessage = "請輸入正確的網站格式")]
        [Display(Name = "公司網站")]
        public string? Website { get; set; }

        /// <summary>
        /// 公司Logo路徑
        /// </summary>
        [MaxLength(500, ErrorMessage = "Logo路徑不可超過500個字元")]
        [Display(Name = "公司Logo")]
        public string? LogoPath { get; set; }

        /// <summary>
        /// 公司簡介
        /// </summary>
        [MaxLength(1000, ErrorMessage = "公司簡介不可超過1000個字元")]
        [Display(Name = "公司簡介")]
        public string? Description { get; set; }



        
        /// <summary>
        /// 公司簡稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "公司簡稱")]
        public string? ShortName { get; set; }

        /// <summary>
        /// 公司英文簡稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "公司英文簡稱")]
        public string? ShortNameEn { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        [Display(Name = "成立日期")]
        public DateTime? EstablishedDate { get; set; }
        /// <summary>
        /// 資本額
        /// </summary>
        [Display(Name = "資本額")]
        public decimal? CapitalAmount { get; set; }
        /// <summary>
        /// 發票抬頭（如與公司名稱不同）
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "發票抬頭")]
        public string? InvoiceTitle { get; set; }
    }
}
