using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Services.Customers.Models
{
    /// <summary>
    /// 創建客戶的請求模型
    /// </summary>
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "客戶代碼為必填")]
        [MaxLength(20, ErrorMessage = "客戶代碼不可超過20個字元")]
        [Display(Name = "客戶代碼")]
        public string CustomerCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "公司名稱為必填")]
        [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;
        
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(20, ErrorMessage = "統一編號不可超過20個字元")]
        [Display(Name = "統一編號")]
        public string? TaxNumber { get; set; }
        
        [Display(Name = "客戶類型")]
        public int? CustomerTypeId { get; set; }
        
        [Display(Name = "行業別")]
        public int? IndustryId { get; set; }
    }

    /// <summary>
    /// 更新客戶的請求模型
    /// </summary>
    public class UpdateCustomerRequest
    {
        public int CustomerId { get; set; }
        
        [Required(ErrorMessage = "客戶代碼為必填")]
        [MaxLength(20, ErrorMessage = "客戶代碼不可超過20個字元")]
        [Display(Name = "客戶代碼")]
        public string CustomerCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "公司名稱為必填")]
        [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = string.Empty;
        
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(20, ErrorMessage = "統一編號不可超過20個字元")]
        [Display(Name = "統一編號")]
        public string? TaxNumber { get; set; }
        
        [Display(Name = "客戶類型")]
        public int? CustomerTypeId { get; set; }
        
        [Display(Name = "行業別")]
        public int? IndustryId { get; set; }
    }
}
