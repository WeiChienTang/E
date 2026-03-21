using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Crm
{
    /// <summary>
    /// 潛在客戶跟進紀錄服務介面
    /// </summary>
    public interface ICrmLeadFollowUpService : IGenericManagementService<CrmLeadFollowUp>
    {
        /// <summary>
        /// 取得指定潛在客戶的所有跟進紀錄（依日期降序）
        /// </summary>
        Task<List<CrmLeadFollowUp>> GetByLeadAsync(int leadId);

        /// <summary>
        /// 伺服器端分頁查詢（含篩選）
        /// </summary>
        Task<(List<CrmLeadFollowUp> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<CrmLeadFollowUp>, IQueryable<CrmLeadFollowUp>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
