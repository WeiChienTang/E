using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 印表機測試服務介面
    /// </summary>
    public interface IPrinterTestService
    {
        /// <summary>
        /// 測試印表機連接並列印測試頁
        /// </summary>
        /// <param name="printerConfiguration">印表機配置</param>
        /// <returns>測試結果</returns>
        Task<ServiceResult> TestPrintAsync(PrinterConfiguration printerConfiguration);

        /// <summary>
        /// 檢查印表機連接狀態
        /// </summary>
        /// <param name="printerConfiguration">印表機配置</param>
        /// <returns>連接狀態結果</returns>
        Task<ServiceResult> CheckConnectionAsync(PrinterConfiguration printerConfiguration);

        /// <summary>
        /// 產生測試頁內容
        /// </summary>
        /// <param name="printerConfiguration">印表機配置</param>
        /// <returns>測試頁內容</returns>
        string GenerateTestPageContent(PrinterConfiguration printerConfiguration);
    }
}
