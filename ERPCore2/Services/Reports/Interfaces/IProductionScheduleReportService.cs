using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 生產排程表報表服務介面
    /// 產生指定期間的生產排程表，包含排程項目、數量、狀態、預計日期等明細
    /// </summary>
    public interface IProductionScheduleReportService
    {
        /// <summary>
        /// 渲染生產排程表報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductionScheduleCriteria criteria);
    }
}
