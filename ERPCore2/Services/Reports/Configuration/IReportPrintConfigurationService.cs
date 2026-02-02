using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Reports.Configuration
{
    /// <summary>
    /// 報表列印配置服務介面
    /// </summary>
    public interface IReportPrintConfigurationService : IGenericManagementService<ReportPrintConfiguration>
    {
        /// <summary>
        /// 根據報表名稱取得列印配置
        /// </summary>
        /// <param name="reportName">報表名稱</param>
        /// <returns>報表列印配置</returns>
        Task<ReportPrintConfiguration?> GetByReportNameAsync(string reportName);

        /// <summary>
        /// 檢查報表名稱是否已存在
        /// </summary>
        /// <param name="reportName">報表名稱</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsReportNameExistsAsync(string reportName, int? excludeId = null);

        /// <summary>
        /// 取得所有啟用的報表列印配置
        /// </summary>
        /// <returns>啟用的報表列印配置清單</returns>
        Task<List<ReportPrintConfiguration>> GetActiveConfigurationsAsync();

        /// <summary>
        /// 取得報表名稱清單（用於下拉選單）
        /// </summary>
        /// <returns>報表名稱清單</returns>
        Task<List<string>> GetReportNamesAsync();

        /// <summary>
        /// 根據印表機設定ID取得相關的報表配置
        /// </summary>
        /// <param name="printerConfigurationId">印表機設定ID</param>
        /// <returns>相關的報表配置清單</returns>
        Task<List<ReportPrintConfiguration>> GetByPrinterConfigurationIdAsync(int printerConfigurationId);

        /// <summary>
        /// 根據紙張設定ID取得相關的報表配置
        /// </summary>
        /// <param name="paperSettingId">紙張設定ID</param>
        /// <returns>相關的報表配置清單</returns>
        Task<List<ReportPrintConfiguration>> GetByPaperSettingIdAsync(int paperSettingId);

        /// <summary>
        /// 取得完整的報表列印配置（包含印表機和紙張設定）
        /// </summary>
        /// <param name="reportName">報表名稱</param>
        /// <returns>完整的報表列印配置</returns>
        Task<ReportPrintConfiguration?> GetCompleteConfigurationAsync(string reportName);

        /// <summary>
        /// 批量更新報表列印配置
        /// </summary>
        /// <param name="configurations">報表列印配置清單</param>
        /// <returns>執行結果</returns>
        Task<bool> BatchUpdateAsync(List<ReportPrintConfiguration> configurations);

        /// <summary>
        /// 複製報表列印配置
        /// </summary>
        /// <param name="sourceReportName">來源報表名稱</param>
        /// <param name="targetReportName">目標報表名稱</param>
        /// <returns>執行結果</returns>
        Task<bool> CopyConfigurationAsync(string sourceReportName, string targetReportName);
    }
}
