using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶拜訪紀錄服務介面
    /// </summary>
    public interface ICustomerVisitService : IGenericManagementService<CustomerVisit>
    {
        /// <summary>
        /// 取得指定客戶的所有拜訪紀錄（依拜訪日期降序）
        /// </summary>
        Task<List<CustomerVisit>> GetByCustomerAsync(int customerId);
    }
}
