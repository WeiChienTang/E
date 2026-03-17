using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 磅秤紀錄報表服務介面（WL001）
    /// 繼承自 IEntityReportService&lt;ScaleRecord&gt;，提供統一的報表服務方法
    /// </summary>
    public interface IScaleRecordReportService : IEntityReportService<ScaleRecord>
    {
        /// <summary>
        /// 以磅秤紀錄篩選條件批次渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ScaleRecordCriteria criteria);
    }
}
