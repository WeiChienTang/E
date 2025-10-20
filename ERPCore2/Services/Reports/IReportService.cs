using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 泛型報表服務介面
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// 生成 HTML 報表
        /// </summary>
        /// <typeparam name="TMainEntity">主要實體類型</typeparam>
        /// <typeparam name="TDetailEntity">明細實體類型</typeparam>
        /// <param name="configuration">報表配置</param>
        /// <param name="reportData">報表資料</param>
        /// <returns>HTML 字串</returns>
        Task<string> GenerateHtmlReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class;
        
        /// <summary>
        /// 列印報表（開啟瀏覽器列印對話框）
        /// </summary>
        Task<string> GeneratePrintableReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class;
    }
    
    /// <summary>
    /// 採購單報表服務介面
    /// </summary>
    public interface IPurchaseOrderReportService
    {
        /// <summary>
        /// 生成採購單報表
        /// </summary>
        /// <param name="purchaseOrderId">採購單 ID</param>
        /// <param name="format">輸出格式</param>
        /// <returns>報表內容</returns>
        Task<string> GeneratePurchaseOrderReportAsync(int purchaseOrderId, ReportFormat format = ReportFormat.Html);
        
        /// <summary>
        /// 生成採購單報表（支援列印配置）
        /// </summary>
        /// <param name="purchaseOrderId">採購單 ID</param>
        /// <param name="format">輸出格式</param>
        /// <param name="reportPrintConfig">報表列印配置</param>
        /// <returns>報表內容</returns>
        Task<string> GeneratePurchaseOrderReportAsync(
            int purchaseOrderId, 
            ReportFormat format, 
            ReportPrintConfiguration? reportPrintConfig);
        
        /// <summary>
        /// 批次生成採購單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">輸出格式（預設 HTML）</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>合併後的報表內容（所有採購單在同一個 HTML，自動分頁）</returns>
        Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null);
        
        /// <summary>
        /// 取得採購單報表配置
        /// </summary>
        /// <param name="company">公司資訊</param>
        /// <returns>採購單報表配置</returns>
        ReportConfiguration GetPurchaseOrderReportConfiguration(Company? company = null);
    }
}
