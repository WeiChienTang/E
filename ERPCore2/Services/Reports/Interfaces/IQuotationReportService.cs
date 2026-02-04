using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 報價單報表服務介面
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public interface IQuotationReportService
    {
        #region 純文字報表
        
        /// <summary>
        /// 生成純文字格式的報價單報表（適合直接列印和預覽）
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <returns>格式化的純文字報表內容</returns>
        Task<string> GeneratePlainTextReportAsync(int quotationId);
        
        /// <summary>
        /// 批次生成純文字報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表內容（每張單據以分頁符號分隔）</returns>
        Task<string> GenerateBatchPlainTextReportAsync(BatchPrintCriteria criteria);
        
        #endregion
        
        #region 直接列印
        
        /// <summary>
        /// 直接列印報價單（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <param name="printerName">印表機名稱</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintAsync(int quotationId, string printerName);
        
        /// <summary>
        /// 直接列印報價單（使用報表列印配置）
        /// </summary>
        /// <param name="quotationId">報價單 ID</param>
        /// <param name="reportId">報表識別碼（用於載入列印配置）</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintByReportIdAsync(int quotationId, string reportId);
        
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
