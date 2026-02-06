using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 格式化列印服務介面
    /// 支援文字、表格、線條、圖片等元素的格式化列印
    /// 使用 System.Drawing.Printing 直接繪製到印表機
    /// </summary>
    public interface IFormattedPrintService
    {
        /// <summary>
        /// 列印格式化文件到指定印表機
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <param name="printerName">印表機名稱</param>
        /// <param name="copies">列印份數</param>
        /// <returns>列印結果</returns>
        ServiceResult Print(FormattedDocument document, string printerName, int copies = 1);

        /// <summary>
        /// 使用報表配置列印格式化文件
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <param name="reportId">報表識別碼（用於載入列印配置）</param>
        /// <param name="copies">列印份數</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> PrintByReportIdAsync(FormattedDocument document, string reportId, int copies = 1);

        /// <summary>
        /// 將格式化文件渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸（794x1123 像素 @ 96 DPI）
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <param name="pageWidth">頁面寬度（像素）</param>
        /// <param name="pageHeight">頁面高度（像素）</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        List<byte[]> RenderToImages(FormattedDocument document, int pageWidth = 794, int pageHeight = 1123, int dpi = 96);

        /// <summary>
        /// 將格式化文件渲染為圖片（用於預覽）
        /// 根據紙張設定計算頁面尺寸
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <param name="paperSetting">紙張設定（包含寬高、邊距等）</param>
        /// <param name="dpi">解析度（預設 96 DPI，用於螢幕顯示）</param>
        /// <returns>各頁面的圖片資料（PNG 格式）</returns>
        List<byte[]> RenderToImages(FormattedDocument document, PaperSetting paperSetting, int dpi = 96);

        /// <summary>
        /// 檢查是否支援格式化列印
        /// </summary>
        bool IsSupported();
    }
}
