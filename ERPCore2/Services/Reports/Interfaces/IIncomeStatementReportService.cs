using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 損益表報表服務介面
    /// 彙總指定期間的營業收入、成本、費用，計算毛利、營業損益、稅前損益
    /// </summary>
    public interface IIncomeStatementReportService
    {
        /// <summary>
        /// 以損益表篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(IncomeStatementCriteria criteria);
    }
}
