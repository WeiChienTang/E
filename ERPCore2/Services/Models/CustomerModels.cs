using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Models
{
    // Request/Response Models
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
        
        [MaxLength(50, ErrorMessage = "建立者不可超過50個字元")]
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }
    }

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
        
        [Display(Name = "狀態")]
        public EntityStatus Status { get; set; }
        
        [MaxLength(50, ErrorMessage = "修改者不可超過50個字元")]
        [Display(Name = "修改者")]
        public string? ModifiedBy { get; set; }
    }

    // Service Result Pattern
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();
        
        public static ServiceResult Success() => new() { IsSuccess = true };
        public static ServiceResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
        public static ServiceResult ValidationFailure(List<string> errors) => new() { IsSuccess = false, ValidationErrors = errors };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }
        
        public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static new ServiceResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
        public static new ServiceResult<T> ValidationFailure(List<string> errors) => new() { IsSuccess = false, ValidationErrors = errors };
    }

    // DTO for display
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? TaxNumber { get; set; }
        public string? CustomerTypeName { get; set; }
        public string? IndustryName { get; set; }
        public EntityStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
