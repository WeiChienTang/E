using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 銷貨單報表服務介面
    /// </summary>
    public interface ISalesOrderReportService
    {
        /// <summary>
        /// 生成銷貨單報表
        /// </summary>
        /// <param name="salesOrderId">銷貨單ID</param>
        /// <param name="format">報表格式</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>報表內容</returns>
        Task<string> GenerateSalesOrderReportAsync(
            int salesOrderId, 
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null);

        /// <summary>
        /// 批次生成銷貨單報表（支援多條件篩選）
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
