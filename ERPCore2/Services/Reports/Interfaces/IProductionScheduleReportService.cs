using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 生產排程表報表服務介面
    /// 繼承自 IEntityReportService&lt;ProductionSchedule&gt;，提供統一的報表服務方法
    /// 額外提供以 ProductionScheduleCriteria 為條件的批次報表方法
    /// </summary>
    public interface IProductionScheduleReportService : IEntityReportService<ProductionSchedule>
    {
        // 繼承自 IEntityReportService<ProductionSchedule> 的所有方法：
        // - GenerateReportAsync(int entityId)
        // - RenderToImagesAsync(int entityId)
        // - RenderToImagesAsync(int entityId, PaperSetting paperSetting)
        // - DirectPrintAsync(int entityId, string reportId, int copies = 1)
        // - DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        // - RenderBatchToImagesAsync(BatchPrintCriteria criteria)

        /// <summary>
        /// 以生產排程專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductionScheduleCriteria criteria);
    }
}
