using ERPCore2.Models;
using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 進貨退出單報表服務介面
    /// </summary>
    public interface IPurchaseReturnReportService
    {
        /// <summary>
        /// 生成進貨退出單報表
        /// </summary>
        /// <param name="purchaseReturnId">進貨退出單 ID</param>
        /// <param name="format">輸出格式</param>
        /// <returns>報表內容</returns>
        Task<string> GeneratePurchaseReturnReportAsync(int purchaseReturnId, ReportFormat format = ReportFormat.Html);
        
        /// <summary>
        /// 生成進貨退出單報表（支援列印配置）
        /// </summary>
        /// <param name="purchaseReturnId">進貨退出單 ID</param>
        /// <param name="format">輸出格式</param>
        /// <param name="reportPrintConfig">報表列印配置</param>
        /// <returns>報表內容</returns>
        Task<string> GeneratePurchaseReturnReportAsync(
            int purchaseReturnId, 
            ReportFormat format, 
            Data.Entities.ReportPrintConfiguration? reportPrintConfig);
        
        /// <summary>
        /// 批次生成進貨退出單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">輸出格式（預設 HTML）</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>合併後的報表內容（所有進貨退出單在同一個 HTML，自動分頁）</returns>
        Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            Data.Entities.ReportPrintConfiguration? reportPrintConfig = null);
    }
}
