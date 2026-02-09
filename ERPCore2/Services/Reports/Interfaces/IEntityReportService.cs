using ERPCore2.Data;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 通用實體報表服務介面
    /// 適用於所有需要列印功能的單據實體
    /// 設計理念：統一報表服務方法簽名，讓 GenericEditModalComponent 可以直接整合列印功能
    /// </summary>
    /// <typeparam name="TEntity">實體類型，須繼承自 BaseEntity</typeparam>
    public interface IEntityReportService<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// 生成格式化報表文件
        /// 支援表格框線、圖片嵌入、精確排版
        /// </summary>
        /// <param name="entityId">實體 ID</param>
        /// <returns>格式化報表文件</returns>
        Task<FormattedDocument> GenerateReportAsync(int entityId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        /// <param name="entityId">實體 ID</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int entityId);

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        /// <param name="entityId">實體 ID</param>
        /// <param name="paperSetting">紙張設定</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        Task<List<byte[]>> RenderToImagesAsync(int entityId, PaperSetting paperSetting);

        /// <summary>
        /// 直接列印（使用報表列印配置）
        /// </summary>
        /// <param name="entityId">實體 ID</param>
        /// <param name="reportId">報表識別碼（用於載入列印配置）</param>
        /// <param name="copies">列印份數</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintAsync(int entityId, string reportId, int copies = 1);

        /// <summary>
        /// 批次列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);

        /// <summary>
        /// 批次渲染報表為圖片（用於批次預覽）
        /// 根據篩選條件查詢符合的單據，逐一產生預覽圖片
        /// </summary>
        /// <param name="criteria">批次篩選條件</param>
        /// <returns>批次預覽結果，包含所有圖片和合併的 FormattedDocument</returns>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria);
    }

    /// <summary>
    /// 批次預覽結果
    /// </summary>
    public class BatchPreviewResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息（失敗時）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 所有頁面的預覽圖片（PNG 格式）
        /// </summary>
        public List<byte[]> PreviewImages { get; set; } = new();

        /// <summary>
        /// 合併的格式化文件（用於列印）
        /// </summary>
        public FormattedDocument? MergedDocument { get; set; }

        /// <summary>
        /// 符合條件的單據數量
        /// </summary>
        public int DocumentCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        public static BatchPreviewResult Success(List<byte[]> images, FormattedDocument? document, int documentCount)
        {
            return new BatchPreviewResult
            {
                IsSuccess = true,
                PreviewImages = images,
                MergedDocument = document,
                DocumentCount = documentCount,
                TotalPages = images.Count
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public static BatchPreviewResult Failure(string errorMessage)
        {
            return new BatchPreviewResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
