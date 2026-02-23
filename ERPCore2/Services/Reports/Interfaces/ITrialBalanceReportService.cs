using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 試算表報表服務介面
    /// 顯示指定期間各科目的本期發生額（借/貸）及期末累計餘額（借/貸）
    /// </summary>
    public interface ITrialBalanceReportService
    {
        /// <summary>
        /// 以試算表篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(TrialBalanceCriteria criteria);
    }
}
