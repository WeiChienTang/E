using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 現金流量表報表服務介面（IAS 7 間接法）
/// </summary>
public interface ICashFlowReportService
{
    /// <summary>
    /// 以現金流量表篩選條件渲染報表為圖片
    /// </summary>
    Task<BatchPreviewResult> RenderBatchToImagesAsync(CashFlowCriteria criteria);
}
