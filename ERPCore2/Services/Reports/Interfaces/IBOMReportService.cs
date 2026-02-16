using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 物料清單報表服務介面
    /// 繼承自 IEntityReportService&lt;ProductComposition&gt;，提供統一的報表服務方法
    /// 額外提供以 BOMReportCriteria 為條件的批次報表方法
    /// </summary>
    public interface IBOMReportService : IEntityReportService<ProductComposition>
    {
        /// <summary>
        /// 以物料清單專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(BOMReportCriteria criteria);
    }
}
