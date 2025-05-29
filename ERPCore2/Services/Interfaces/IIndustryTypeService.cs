using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 行業類型服務介面 - 管理行業類型的 CRUD 操作
    /// </summary>
    public interface IIndustryTypeService : IGenericManagementService<IndustryType>
    {
        // 業務特定方法
        Task<bool> IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null);
        Task<bool> IsIndustryTypeCodeExistsAsync(string industryTypeCode, int? excludeId = null);
        
        // 分頁查詢
        Task<(List<IndustryType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}
