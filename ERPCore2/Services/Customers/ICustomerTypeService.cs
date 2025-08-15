using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶類型服務介面 - 管理客戶類型的 CRUD 操作
    /// </summary>
    public interface ICustomerTypeService : IGenericManagementService<CustomerType>
    {
        // 業務特定方法
        Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null);
        Task<bool> IsCustomerTypeCodeExistsAsync(string typeCode, int? excludeId = null);
        
        // 分頁查詢
        Task<(List<CustomerType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}

