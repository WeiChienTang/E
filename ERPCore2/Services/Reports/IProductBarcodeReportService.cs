using ERPCore2.Models;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 產品條碼報表服務介面
    /// </summary>
    public interface IProductBarcodeReportService
    {
        /// <summary>
        /// 生成條碼批次列印報表
        /// </summary>
        /// <param name="criteria">條碼列印條件</param>
        /// <returns>可列印的 HTML 內容</returns>
        Task<string> GenerateBarcodeReportAsync(ProductBarcodePrintCriteria criteria);
    }
}
