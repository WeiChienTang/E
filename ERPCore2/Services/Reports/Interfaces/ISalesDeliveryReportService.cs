using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 出貨單報表服務介面
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// </summary>
    public interface ISalesDeliveryReportService
    {
        #region 報表生成

        /// <summary>
        /// 生成出貨單報表文件
        /// </summary>
        /// <param name="salesDeliveryId">出貨單 ID</param>
        /// <returns>格式化文件</returns>
        Task<FormattedDocument> GenerateReportAsync(int salesDeliveryId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        /// <param name="salesDeliveryId">出貨單 ID</param>
        /// <returns>PNG 圖片位元組陣列列表（每頁一張圖片）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int salesDeliveryId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        /// <param name="salesDeliveryId">出貨單 ID</param>
        /// <param name="paperSetting">紙張設定</param>
        /// <returns>PNG 圖片位元組陣列列表（每頁一張圖片）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int salesDeliveryId, PaperSetting paperSetting);

        #endregion

        #region 直接列印

        /// <summary>
        /// 直接列印出貨單
        /// </summary>
        /// <param name="salesDeliveryId">出貨單 ID</param>
        /// <param name="reportId">報表識別碼（用於載入列印配置）</param>
        /// <param name="copies">列印份數</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintAsync(int salesDeliveryId, string reportId, int copies = 1);

        /// <summary>
        /// 批次直接列印（使用報表列印配置）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);

        #endregion
    }
}
