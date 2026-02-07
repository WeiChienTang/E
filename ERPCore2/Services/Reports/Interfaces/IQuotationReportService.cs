using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 報價單報表服務介面
    /// 統一使用格式化報表模式（FormattedDocument），支援表格線、圖片嵌入
    /// </summary>
    public interface IQuotationReportService
    {
        /// <summary>
        /// 生成格式化報價單報表文件
        /// 支援表格框線、圖片嵌入、精確排版
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <returns>格式化報表文件</returns>
        Task<FormattedDocument> GenerateReportAsync(int quotationId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int quotationId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <param name="paperSetting">紙張設定</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int quotationId, PaperSetting paperSetting);

        /// <summary>
        /// 直接列印報價單（使用報表列印配置）
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <param name="reportId">報表識別碼</param>
        /// <param name="copies">列印份數</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintAsync(int quotationId, string reportId, int copies = 1);

        /// <summary>
        /// 批次列印報價單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);
    }
}
