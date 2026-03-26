using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 公家機關實體 - 定義公家機關基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class GovernmentAgency : BaseEntity
    {
        [MaxLength(100, ErrorMessage = "機關名稱不可超過100個字元")]
        [Display(Name = "機關名稱")]
        public string? AgencyName { get; set; }

        [MaxLength(20, ErrorMessage = "機關代碼不可超過20個字元")]
        [Display(Name = "機關代碼")]
        public string? AgencyCode { get; set; }

        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }

        [MaxLength(50, ErrorMessage = "職稱不可超過50個字元")]
        [Display(Name = "職稱")]
        public string? JobTitle { get; set; }

        [MaxLength(20, ErrorMessage = "聯絡電話不可超過20個字元")]
        [Display(Name = "聯絡電話")]
        public string? ContactPhone { get; set; }

        [MaxLength(20, ErrorMessage = "傳真不可超過20個字元")]
        [Display(Name = "傳真")]
        public string? Fax { get; set; }

        [MaxLength(100, ErrorMessage = "信箱不可超過100個字元")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        [Display(Name = "信箱")]
        public string? Email { get; set; }

        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "網站不可超過100個字元")]
        [Display(Name = "網站")]
        public string? Website { get; set; }

        [MaxLength(100, ErrorMessage = "管轄區域不可超過100個字元")]
        [Display(Name = "管轄區域")]
        public string? Jurisdiction { get; set; }

        [MaxLength(100, ErrorMessage = "上級機關不可超過100個字元")]
        [Display(Name = "上級機關")]
        public string? SupervisorAgency { get; set; }

        [Display(Name = "機關類型")]
        public GovernmentAgencyType? AgencyType { get; set; }

        [Display(Name = "機關狀態")]
        public GovernmentAgencyStatus AgencyStatus { get; set; } = GovernmentAgencyStatus.Active;
    }
}
