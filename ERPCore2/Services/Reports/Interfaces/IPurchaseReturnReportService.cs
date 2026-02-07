using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 進貨退出單報表服務介面
    /// 統一使用格式化報表模式（FormattedDocument），支援表格線、圖片嵌入
    /// </summary>
    public interface IPurchaseReturnReportService
    {
        /// <summary>
        /// 生成格式化進貨退出單報表文件
        /// 支援表格框線、圖片嵌入、精確排版
        /// </summary>
        /// <param name="purchaseReturnId">進貨退出單 ID</param>
        /// <returns>格式化報表文件</returns>
        Task<FormattedDocument> GenerateReportAsync(int purchaseReturnId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        /// <param name="purchaseReturnId">進貨退出單 ID</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int purchaseReturnId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        /// <param name="purchaseReturnId">進貨退出單 ID</param>
        /// <param name="paperSetting">紙張設定</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int purchaseReturnId, PaperSetting paperSetting);

        /// <summary>
        /// 直接列印進貨退出單（使用報表列印配置）
        /// </summary>
        /// <param name="purchaseReturnId">進貨退出單 ID</param>
        /// <param name="reportId">報表識別碼</param>
        /// <param name="copies">列印份數</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintAsync(int purchaseReturnId, string reportId, int copies = 1);

        /// <summary>
        /// 批次列印進貨退出單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);
    }
}
