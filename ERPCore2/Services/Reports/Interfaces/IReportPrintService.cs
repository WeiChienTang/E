using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 報表列印服務介面 - 支援伺服器端直接列印
    /// 用於實現自動選擇預設印表機和紙張設定的列印功能
    /// </summary>
    public interface IReportPrintService
    {
        /// <summary>
        /// 直接列印報表（使用指定的印表機和紙張設定）
        /// </summary>
        /// <param name="htmlContent">HTML 報表內容</param>
        /// <param name="printConfig">列印配置（包含印表機和紙張設定）</param>
        /// <param name="documentName">文件名稱（顯示在列印佇列中）</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> PrintReportAsync(string htmlContent, ReportPrintConfiguration printConfig, string documentName);

        /// <summary>
        /// 直接列印報表（使用報表識別碼自動載入配置）
        /// </summary>
        /// <param name="htmlContent">HTML 報表內容</param>
        /// <param name="reportId">報表識別碼（如 PO001）</param>
        /// <param name="documentName">文件名稱</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> PrintReportByIdAsync(string htmlContent, string reportId, string documentName);

        /// <summary>
        /// 檢查印表機是否可用
        /// </summary>
        /// <param name="printerConfigurationId">印表機配置 ID</param>
        /// <returns>是否可用</returns>
        Task<ServiceResult> CheckPrinterAvailableAsync(int printerConfigurationId);

        /// <summary>
        /// 取得報表的預設列印配置
        /// </summary>
        /// <param name="reportId">報表識別碼</param>
        /// <returns>列印配置（含印表機和紙張資訊）</returns>
        Task<ReportPrintConfiguration?> GetDefaultPrintConfigAsync(string reportId);

        /// <summary>
        /// 列印報表並返回結果（含詳細資訊）
        /// </summary>
        /// <param name="htmlContent">HTML 報表內容</param>
        /// <param name="printerName">印表機名稱（系統印表機名稱）</param>
        /// <param name="paperSetting">紙張設定</param>
        /// <param name="documentName">文件名稱</param>
        /// <returns>列印結果</returns>
        Task<ReportPrintResult> PrintWithDetailsAsync(
            string htmlContent, 
            string printerName, 
            PaperSetting? paperSetting, 
            string documentName);

        /// <summary>
        /// 將 HTML 渲染為圖片（用於預覽）
        /// </summary>
        /// <param name="htmlContent">HTML 內容</param>
        /// <param name="paperSetting">紙張設定（可選，用於決定頁面尺寸）</param>
        /// <returns>圖片資料清單（PNG 格式）</returns>
        Task<List<byte[]>> RenderHtmlToImagesAsync(string htmlContent, PaperSetting? paperSetting = null);
    }

    /// <summary>
    /// 報表列印結果
    /// </summary>
    public class ReportPrintResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 使用的印表機名稱
        /// </summary>
        public string? PrinterName { get; set; }

        /// <summary>
        /// 使用的紙張名稱
        /// </summary>
        public string? PaperName { get; set; }

        /// <summary>
        /// 列印時間
        /// </summary>
        public DateTime PrintTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 頁數
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        public static ReportPrintResult Success(string printerName, string? paperName = null, int pageCount = 1)
        {
            return new ReportPrintResult
            {
                IsSuccess = true,
                PrinterName = printerName,
                PaperName = paperName,
                PageCount = pageCount
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public static ReportPrintResult Failure(string errorMessage)
        {
            return new ReportPrintResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
