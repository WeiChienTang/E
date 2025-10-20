using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 銷貨退回單報表服務介面
    /// </summary>
    public interface ISalesReturnReportService
    {
        /// <summary>
        /// 生成銷貨退回單報表
        /// </summary>
        /// <param name="salesReturnId">銷貨退回單ID</param>
        /// <param name="format">報表格式</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>報表內容</returns>
        Task<string> GenerateSalesReturnReportAsync(
            int salesReturnId, 
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null);

        /// <summary>
        /// 批次生成銷貨退回單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">報表格式</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>合併後的報表內容</returns>
        Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null);
    }
}
