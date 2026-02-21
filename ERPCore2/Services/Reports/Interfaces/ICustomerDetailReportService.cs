using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 客戶詳細資料報表服務介面（AR006）
    /// 每位客戶各佔一區塊，顯示完整聯絡與付款資訊
    /// 繼承自 IEntityReportService&lt;Customer&gt;，提供統一的報表服務方法
    /// </summary>
    public interface ICustomerDetailReportService : IEntityReportService<Customer>
    {
        /// <summary>
        /// 以客戶名冊篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerRosterCriteria criteria);
    }
}
