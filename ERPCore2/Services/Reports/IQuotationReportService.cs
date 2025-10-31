using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 報價單報表服務介面
    /// </summary>
    public interface IQuotationReportService
    {
        /// <summary>
        /// 生成報價單報表
        /// </summary>
        /// <param name="quotationId">報價單ID</param>
        /// <param name="format">報表格式</param>
        /// <returns>報表內容</returns>
        Task<string> GenerateQuotationReportAsync(int quotationId, ReportFormat format = ReportFormat.Html);

        /// <summary>
        /// 生成報價單報表（帶列印配置）
        /// </summary>
        /// <param name="quotationId">報價單ID</param>
        /// <param name="format">報表格式</param>
        /// <param name="reportPrintConfig">列印配置</param>
        /// <returns>報表內容</returns>
        Task<string> GenerateQuotationReportAsync(
            int quotationId,
            ReportFormat format,
            ReportPrintConfiguration? reportPrintConfig);

        /// <summary>
        /// 批次生成報價單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">報表格式</param>
        /// <param name="reportPrintConfig">列印配置</param>
        /// <returns>合併後的報表內容</returns>
        Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null);
    }
}
