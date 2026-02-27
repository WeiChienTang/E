using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// PDF 匯出服務介面
    /// 將已渲染的報表頁面圖片組合為 PDF 檔案
    /// </summary>
    public interface IPdfExportService
    {
        /// <summary>
        /// 將頁面圖片列表匯出為 PDF 檔案
        /// </summary>
        /// <param name="pageImages">各頁面的 PNG 圖片資料</param>
        /// <param name="pageWidthCm">頁面寬度（公分），預設 A4 21cm</param>
        /// <param name="pageHeightCm">頁面高度（公分），預設 A4 29.7cm</param>
        /// <returns>PDF 檔案的 byte[]</returns>
        Task<byte[]> ExportToPdfAsync(List<byte[]> pageImages, double pageWidthCm = 21.0, double pageHeightCm = 29.7);

        /// <summary>
        /// 檢查服務是否可用
        /// </summary>
        bool IsSupported();
    }
}
