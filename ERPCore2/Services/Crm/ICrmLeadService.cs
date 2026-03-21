using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Crm
{
    /// <summary>
    /// 潛在客戶服務介面
    /// </summary>
    public interface ICrmLeadService : IGenericManagementService<CrmLead>
    {
        /// <summary>
        /// 伺服器端分頁查詢（含篩選）
        /// </summary>
        Task<(List<CrmLead> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<CrmLead>, IQueryable<CrmLead>>? filterFunc,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// 取得指定業務員的所有潛在客戶
        /// </summary>
        Task<List<CrmLead>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// 將潛在客戶轉換為正式客戶
        /// 建立 Customer 並更新 CrmLead.ConvertedCustomerId / ConvertedAt / LeadStage = Won
        /// </summary>
        Task<ServiceResult<int>> ConvertToCustomerAsync(int leadId, string? createdBy = null);

        /// <summary>
        /// 判斷指定公司名稱是否已存在
        /// </summary>
        Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeId = null);
    }
}
