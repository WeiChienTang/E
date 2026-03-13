using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶投訴服務介面
    /// </summary>
    public interface ICustomerComplaintService : IGenericManagementService<CustomerComplaint>
    {
        /// <summary>
        /// 取得指定客戶的所有投訴紀錄（依投訴日期降序）
        /// </summary>
        Task<List<CustomerComplaint>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// 伺服器端分頁查詢（僅取列表所需欄位）。
        /// </summary>
        Task<(List<CustomerComplaint> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<CustomerComplaint>, IQueryable<CustomerComplaint>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
