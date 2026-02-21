using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 廢料記錄報表服務介面（WL001）
    /// 繼承自 IEntityReportService&lt;WasteRecord&gt;，提供統一的報表服務方法
    /// </summary>
    public interface IWasteRecordReportService : IEntityReportService<WasteRecord>
    {
        /// <summary>
        /// 以廢料記錄篩選條件批次渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(WasteRecordCriteria criteria);
    }
}
