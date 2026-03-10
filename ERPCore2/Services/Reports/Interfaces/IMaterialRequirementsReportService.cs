using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 用料需求報表服務介面
    /// 彙總排程物料需求，依組件品號統計需求量、已領量與待領量
    /// </summary>
    public interface IMaterialRequirementsReportService
    {
        /// <summary>
        /// 以具體篩選條件渲染報表為預覽圖片（由 GenericReportFilterModalComponent 優先呼叫）
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(MaterialRequirementsCriteria criteria);

        /// <summary>
        /// 批次列印回退方法（BatchPrintCriteria 相容）
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria);
    }
}
