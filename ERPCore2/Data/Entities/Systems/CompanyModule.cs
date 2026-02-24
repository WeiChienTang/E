using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 公司模組實體 - 控制此公司可使用的功能模組
    /// 僅 SuperAdmin 可管理，預設全部啟用
    /// </summary>
    [Index(nameof(ModuleKey), IsUnique = true)]
    public class CompanyModule : BaseEntity
    {
        /// <summary>
        /// 模組識別鍵（對應頁面目錄名稱，如 "FinancialManagement"）
        /// </summary>
        [Required(ErrorMessage = "模組識別鍵為必填")]
        [MaxLength(100, ErrorMessage = "模組識別鍵不可超過100個字元")]
        [Display(Name = "模組識別鍵")]
        public string ModuleKey { get; set; } = string.Empty;

        /// <summary>
        /// 模組顯示名稱（如 "財務管理"）
        /// </summary>
        [Required(ErrorMessage = "模組名稱為必填")]
        [MaxLength(100, ErrorMessage = "模組名稱不可超過100個字元")]
        [Display(Name = "模組名稱")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 模組說明
        /// </summary>
        [MaxLength(500, ErrorMessage = "模組說明不可超過500個字元")]
        [Display(Name = "說明")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用此模組（預設啟用）
        /// </summary>
        [Display(Name = "啟用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排序順序（數字越小越靠前）
        /// </summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;
    }
}
