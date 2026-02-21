using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 廠商詳細資料報表服務介面（AP005）
    /// 每位廠商各佔一區塊，顯示完整聯絡與付款資訊
    /// 繼承自 IEntityReportService&lt;Supplier&gt;，提供統一的報表服務方法
    /// </summary>
    public interface ISupplierDetailReportService : IEntityReportService<Supplier>
    {
        /// <summary>
        /// 以廠商名冊篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(SupplierRosterCriteria criteria);
    }
}
