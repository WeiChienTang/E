using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 客戶銷售分析報表服務介面
    /// 分析客戶銷售金額排名，按銷售額由高至低排列
    /// </summary>
    public interface ICustomerSalesAnalysisReportService
    {
        /// <summary>
        /// 渲染客戶銷售分析報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerSalesAnalysisCriteria criteria);
    }
}
