using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 資產負債表報表服務介面
    /// 彙總截止日當天的資產、負債、權益累計餘額
    /// </summary>
    public interface IBalanceSheetReportService
    {
        /// <summary>
        /// 以資產負債表篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(BalanceSheetCriteria criteria);
    }
}
