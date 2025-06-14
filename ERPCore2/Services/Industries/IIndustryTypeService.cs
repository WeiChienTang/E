using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 行業類型服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IIndustryTypeService : IGenericManagementService<IndustryType>
    {
        /// <summary>
        /// 檢查行業類型名稱是否存在
        /// </summary>
        Task<bool> IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null);
        
        /// <summary>
        /// 檢查行業類型代碼是否存在
        /// </summary>
        Task<bool> IsIndustryTypeCodeExistsAsync(string industryTypeCode, int? excludeId = null);
        
        /// <summary>
        /// 分頁查詢（無搜尋條件）
        /// </summary>
        Task<(List<IndustryType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}
