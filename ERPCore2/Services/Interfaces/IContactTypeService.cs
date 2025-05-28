using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 聯絡類型服務介面 - 管理聯絡類型的 CRUD 操作
    /// </summary>
    public interface IContactTypeService : IGenericManagementService<ContactType>
    {
        // 業務特定方法
        Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null);
        
        // 分頁查詢
        Task<(List<ContactType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}
