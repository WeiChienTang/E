using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 訂單照片服務介面
    /// </summary>
    public interface ISalesOrderPhotoService : IGenericManagementService<SalesOrderPhoto>
    {
        Task<List<SalesOrderPhoto>> GetBySalesOrderAsync(int salesOrderId);
    }
}
