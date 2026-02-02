using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 印表機設定服務介面
    /// </summary>
    public interface IPrinterConfigurationService : IGenericManagementService<PrinterConfiguration>
    {
        /// <summary>
        /// 取得預設印表機設定
        /// </summary>
        /// <returns>預設印表機設定，若無則返回null</returns>
        Task<PrinterConfiguration?> GetDefaultPrinterAsync();

        /// <summary>
        /// 根據名稱取得印表機設定
        /// </summary>
        /// <param name="name">印表機名稱</param>
        /// <returns>印表機設定資料</returns>
        Task<PrinterConfiguration?> GetByNameAsync(string name);

        /// <summary>
        /// 根據IP位址取得印表機設定
        /// </summary>
        /// <param name="ipAddress">IP位址</param>
        /// <returns>印表機設定資料</returns>
        Task<PrinterConfiguration?> GetByIpAddressAsync(string ipAddress);

        /// <summary>
        /// 檢查IP位址是否已存在
        /// </summary>
        /// <param name="ipAddress">IP位址</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsIpAddressExistsAsync(string ipAddress, int? excludeId = null);

        /// <summary>
        /// 檢查印表機編號是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">印表機編號</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsPrinterConfigurationCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 設定為預設印表機（會取消其他印表機的預設狀態）
        /// </summary>
        /// <param name="id">印表機設定ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetAsDefaultAsync(int id);

        /// <summary>
        /// 取得所有啟用的印表機設定
        /// </summary>
        /// <returns>啟用的印表機設定清單</returns>
        Task<List<PrinterConfiguration>> GetActivePrintersAsync();

        /// <summary>
        /// 根據連接類型取得印表機設定（網路或本機）
        /// </summary>
        /// <param name="isNetworkPrinter">是否為網路印表機</param>
        /// <returns>對應的印表機設定清單</returns>
        Task<List<PrinterConfiguration>> GetByConnectionTypeAsync(bool isNetworkPrinter);

        /// <summary>
        /// 驗證IP位址格式是否正確
        /// </summary>
        /// <param name="ipAddress">IP位址</param>
        /// <returns>驗證結果</returns>
        ServiceResult ValidateIpAddress(string ipAddress);

        /// <summary>
        /// 測試印表機連接並列印測試頁
        /// </summary>
        /// <param name="printerConfiguration">印表機配置</param>
        /// <returns>測試結果</returns>
        Task<ServiceResult> TestPrintAsync(PrinterConfiguration printerConfiguration);

        /// <summary>
        /// 取得系統已安裝的印表機列表
        /// </summary>
        /// <returns>已安裝印表機資訊列表</returns>
        Task<List<InstalledPrinterInfo>> GetInstalledPrintersAsync();
    }
}
