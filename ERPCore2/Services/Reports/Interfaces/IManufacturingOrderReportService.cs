using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 製令單報表服務介面
    /// 繼承自 IEntityReportService&lt;ProductionScheduleItem&gt;，提供統一的報表服務方法
    /// 額外提供以 ManufacturingOrderCriteria 為條件的批次報表方法
    /// </summary>
    public interface IManufacturingOrderReportService : IEntityReportService<ProductionScheduleItem>
    {
        /// <summary>
        /// 以製令單專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ManufacturingOrderCriteria criteria);
    }
}
