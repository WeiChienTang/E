using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 聯絡類型服務介面
    /// </summary>
    public interface IContactTypeService : IGenericManagementService<ContactType>
    {
        // 業務特定方法
        Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null);
        
        // 分頁查詢
        Task<(List<ContactType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}

