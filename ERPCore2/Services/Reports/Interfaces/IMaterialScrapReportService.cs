using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 用料損耗退料記錄報表服務介面
    /// 批次查詢報表，無單一實體列印需求
    /// </summary>
    public interface IMaterialScrapReportService
    {
        /// <summary>
        /// 以具體篩選條件渲染報表為預覽圖片（由 GenericReportFilterModalComponent 優先呼叫）
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(MaterialScrapCriteria criteria);

        /// <summary>
        /// 批次列印回退方法（BatchPrintCriteria 相容）
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria);
    }
}
