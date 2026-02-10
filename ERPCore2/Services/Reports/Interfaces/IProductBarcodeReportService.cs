using ERPCore2.Models;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 商品條碼報表服務介面
    /// </summary>
    public interface IProductBarcodeReportService
    {
        /// <summary>
        /// 生成條碼批次列印報表（舊版 API，保留向後相容）
        /// </summary>
        /// <param name="criteria">條碼列印條件</param>
        /// <returns>可列印的 HTML 內容</returns>
        Task<string> GenerateBarcodeReportAsync(ProductBarcodePrintCriteria criteria);
        
        /// <summary>
        /// 批次渲染條碼報表為圖片（統一報表架構）
        /// </summary>
        /// <param name="criteria">條碼列印條件</param>
        /// <returns>批次預覽結果</returns>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductBarcodeBatchPrintCriteria criteria);
    }
}
