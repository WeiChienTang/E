using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 採購單報表服務介面
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public interface IPurchaseOrderReportService
    {
        #region 純文字報表
        
        /// <summary>
        /// 生成純文字格式的採購單報表（適合直接列印和預覽）
        /// 直接生成格式化的純文字，不需要經過 HTML
        /// 使用預設版面配置（80 字元寬）
        /// </summary>
        /// <param name="purchaseOrderId">採購單 ID</param>
        /// <returns>格式化的純文字報表內容</returns>
        Task<string> GeneratePlainTextReportAsync(int purchaseOrderId);

        /// <summary>
        /// 生成純文字格式的採購單報表（根據紙張版面配置）
        /// 根據指定的紙張設定動態調整報表寬度和欄位配置
        /// </summary>
        /// <param name="purchaseOrderId">採購單 ID</param>
        /// <param name="layout">紙張版面配置</param>
        /// <returns>格式化的純文字報表內容</returns>
        Task<string> GeneratePlainTextReportAsync(int purchaseOrderId, PaperLayout layout);
        
        /// <summary>
        /// 批次生成純文字報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表內容（每張單據以分頁符號分隔）</returns>
        Task<string> GenerateBatchPlainTextReportAsync(BatchPrintCriteria criteria);
        
        #endregion
        
        #region 直接列印
        
        /// <summary>
        /// 直接列印採購單（使用 System.Drawing.Printing）
        /// 適合 Blazor Server 直接呼叫，不需要經過 API
        /// </summary>
        /// <param name="purchaseOrderId">採購單 ID</param>
        /// <param name="printerName">印表機名稱</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string printerName);
        
        /// <summary>
        /// 直接列印採購單（使用報表列印配置）
        /// </summary>
        /// <param name="purchaseOrderId">採購單 ID</param>
        /// <param name="reportId">報表識別碼（用於載入列印配置）</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> DirectPrintByReportIdAsync(int purchaseOrderId, string reportId);
        
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
