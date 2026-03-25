using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 客戶拜訪報告服務介面
/// 列印客戶拜訪記錄報告，含拜訪日期、客戶、拜訪人員、拜訪目的及結果摘要
/// </summary>
public interface ICustomerVisitReportService
{
    /// <summary>
    /// 以客戶拜訪篩選條件渲染報表為圖片
    /// </summary>
    Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerVisitReportCriteria criteria);
}
