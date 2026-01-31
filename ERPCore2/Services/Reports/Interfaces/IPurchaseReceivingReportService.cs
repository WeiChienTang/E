using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 進貨單（入庫單）報表服務介面
    /// </summary>
    public interface IPurchaseReceivingReportService
    {
        /// <summary>
        /// 生成進貨單報表
        /// </summary>
        /// <param name="purchaseReceivingId">進貨單 ID</param>
        /// <param name="format">輸出格式</param>
        /// <returns>報表內容</returns>
        Task<string> GeneratePurchaseReceivingReportAsync(int purchaseReceivingId, ReportFormat format = ReportFormat.Html);
        
        /// <summary>
        /// 生成進貨單報表（支援列印配置）
        /// </summary>
        /// <param name="purchaseReceivingId">進貨單 ID</param>
        /// <param name="format">輸出格式</param>
        /// <param name="reportPrintConfig">報表列印配置</param>
        /// <returns>報表內容</returns>
        Task<string> GeneratePurchaseReceivingReportAsync(
            int purchaseReceivingId, 
            ReportFormat format, 
            ReportPrintConfiguration? reportPrintConfig);
        
        /// <summary>
        /// 批次生成進貨單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">輸出格式（預設 HTML）</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>合併後的報表內容（所有進貨單在同一個 HTML，自動分頁）</returns>
        Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null);
    }
}
