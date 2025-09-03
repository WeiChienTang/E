using ERPCore2.Data.Entities;
using ERPCore2.Services;
using Microsoft.JSInterop;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 報表列印輔助類別
    /// </summary>
    public static class ReportPrintHelper
    {
        /// <summary>
        /// 取得可用的列印配置並處理使用者選擇（不使用 JavaScript 互操作）
        /// </summary>
        /// <param name="reportType">報表類型</param>
        /// <param name="configService">報表列印配置服務</param>
        /// <returns>選擇的列印配置</returns>
        public static async Task<ReportPrintConfiguration?> SelectPrintConfigurationWithoutJSAsync(
            string reportType,
            IReportPrintConfigurationService configService)
        {
            try
            {
                // 取得該報表類型的配置
                var configuration = await configService.GetCompleteConfigurationAsync(reportType);
                
                if (configuration == null)
                {
                    // 沒有找到配置，使用系統預設
                    Console.WriteLine($"沒有找到報表類型 '{reportType}' 的列印配置，將使用系統預設設定");
                    return null;
                }

                // 如果找到配置且狀態為啟用，則使用該配置
                if (configuration.Status == Data.Enums.EntityStatus.Active)
                {
                    Console.WriteLine($"使用報表類型 '{reportType}' 的列印配置：{configuration.ReportName}");
                    return configuration;
                }

                // 配置存在但已停用
                Console.WriteLine($"報表類型 '{reportType}' 的列印配置已停用，將使用系統預設設定");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得列印配置時發生錯誤: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 取得可用的列印配置並處理使用者選擇
        /// </summary>
        /// <param name="reportType">報表類型</param>
        /// <param name="configService">報表列印配置服務</param>
        /// <param name="jsRuntime">JavaScript 運行時</param>
        /// <returns>選擇的列印配置</returns>
        public static async Task<ReportPrintConfiguration?> SelectPrintConfigurationAsync(
            string reportType,
            IReportPrintConfigurationService configService,
            IJSRuntime jsRuntime)
        {
            try
            {
                // 取得該報表類型的配置
                var configuration = await configService.GetCompleteConfigurationAsync(reportType);
                
                if (configuration == null)
                {
                    // 沒有找到配置，使用系統預設
                    await jsRuntime.InvokeVoidAsync("console.log", $"沒有找到報表類型 '{reportType}' 的列印配置，將使用系統預設設定");
                    return null;
                }

                // 如果找到配置且狀態為啟用，則使用該配置
                if (configuration.Status == Data.Enums.EntityStatus.Active)
                {
                    await jsRuntime.InvokeVoidAsync("console.log", $"使用報表類型 '{reportType}' 的列印配置：{configuration.ReportName}");
                    return configuration;
                }

                // 配置存在但已停用
                await jsRuntime.InvokeVoidAsync("console.log", $"報表類型 '{reportType}' 的列印配置已停用，將使用系統預設設定");
                return null;
            }
            catch (Exception ex)
            {
                await jsRuntime.InvokeVoidAsync("console.error", $"取得列印配置時發生錯誤: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 取得多個列印配置供使用者選擇
        /// </summary>
        /// <param name="configService">報表列印配置服務</param>
        /// <param name="jsRuntime">JavaScript 運行時</param>
        /// <returns>所有啟用的列印配置</returns>
        public static async Task<List<ReportPrintConfiguration>> GetAvailableConfigurationsAsync(
            IReportPrintConfigurationService configService,
            IJSRuntime jsRuntime)
        {
            try
            {
                var configurations = await configService.GetActiveConfigurationsAsync();
                await jsRuntime.InvokeVoidAsync("console.log", $"找到 {configurations.Count} 個啟用的列印配置");
                return configurations;
            }
            catch (Exception ex)
            {
                await jsRuntime.InvokeVoidAsync("console.error", $"取得可用列印配置時發生錯誤: {ex.Message}");
                return new List<ReportPrintConfiguration>();
            }
        }

        /// <summary>
        /// 執行列印動作
        /// </summary>
        /// <param name="reportUrl">報表 URL</param>
        /// <param name="jsRuntime">JavaScript 運行時</param>
        /// <param name="windowName">新視窗名稱</param>
        /// <returns>執行結果</returns>
        public static async Task<bool> ExecutePrintAsync(
            string reportUrl,
            IJSRuntime jsRuntime,
            string windowName = "_blank")
        {
            try
            {
                // 開啟新視窗並自動列印
                await jsRuntime.InvokeVoidAsync("eval", $@"
                    const printWindow = window.open('{reportUrl}', '{windowName}');
                    if (printWindow) {{
                        printWindow.onload = function() {{
                            setTimeout(function() {{
                                printWindow.print();
                            }}, 500);
                        }};
                    }} else {{
                        alert('無法開啟列印視窗，請檢查瀏覽器的彈出視窗設定');
                    }}
                ");
                
                return true;
            }
            catch (Exception ex)
            {
                await jsRuntime.InvokeVoidAsync("console.error", $"執行列印時發生錯誤: {ex.Message}");
                await jsRuntime.InvokeVoidAsync("alert", $"列印失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 預覽報表
        /// </summary>
        /// <param name="reportUrl">報表 URL</param>
        /// <param name="jsRuntime">JavaScript 運行時</param>
        /// <param name="windowName">新視窗名稱</param>
        /// <returns>執行結果</returns>
        public static async Task<bool> PreviewReportAsync(
            string reportUrl,
            IJSRuntime jsRuntime,
            string windowName = "_blank")
        {
            try
            {
                // 開啟新視窗預覽報表
                await jsRuntime.InvokeVoidAsync("window.open", reportUrl, windowName);
                return true;
            }
            catch (Exception ex)
            {
                await jsRuntime.InvokeVoidAsync("console.error", $"預覽報表時發生錯誤: {ex.Message}");
                await jsRuntime.InvokeVoidAsync("alert", $"預覽失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 建立帶有列印配置的報表 URL
        /// </summary>
        /// <param name="baseUrl">基礎 URL</param>
        /// <param name="reportId">報表 ID</param>
        /// <param name="configuration">列印配置</param>
        /// <param name="format">報表格式</param>
        /// <returns>完整的報表 URL</returns>
        public static string BuildReportUrl(
            string baseUrl, 
            int reportId, 
            ReportPrintConfiguration? configuration = null, 
            string format = "html")
        {
            var url = $"{baseUrl}/api/report/purchase-order/{reportId}?format={format}";
            
            if (configuration != null)
            {
                url += $"&configId={configuration.Id}&reportType={configuration.ReportType}";
            }

            return url;
        }

        /// <summary>
        /// 驗證列印配置是否完整
        /// </summary>
        /// <param name="configuration">列印配置</param>
        /// <returns>驗證結果和錯誤訊息</returns>
        public static (bool IsValid, string ErrorMessage) ValidateConfiguration(ReportPrintConfiguration? configuration)
        {
            if (configuration == null)
            {
                return (true, ""); // null 配置被認為是有效的（使用系統預設）
            }

            if (configuration.Status != Data.Enums.EntityStatus.Active)
            {
                return (false, "列印配置已停用");
            }

            if (string.IsNullOrEmpty(configuration.ReportType))
            {
                return (false, "報表類型不能為空");
            }

            if (string.IsNullOrEmpty(configuration.ReportName))
            {
                return (false, "報表名稱不能為空");
            }

            // 檢查印表機配置是否存在且啟用
            if (configuration.PrinterConfiguration != null && 
                configuration.PrinterConfiguration.Status != Data.Enums.EntityStatus.Active)
            {
                return (false, "指定的印表機配置已停用");
            }

            // 檢查紙張設定是否存在且啟用
            if (configuration.PaperSetting != null && 
                configuration.PaperSetting.Status != Data.Enums.EntityStatus.Active)
            {
                return (false, "指定的紙張設定已停用");
            }

            return (true, "");
        }

        /// <summary>
        /// 取得配置的顯示名稱
        /// </summary>
        /// <param name="configuration">列印配置</param>
        /// <returns>顯示名稱</returns>
        public static string GetDisplayName(ReportPrintConfiguration? configuration)
        {
            if (configuration == null)
            {
                return "系統預設設定";
            }

            var displayName = configuration.ReportName;
            
            if (configuration.PrinterConfiguration != null)
            {
                displayName += $" ({configuration.PrinterConfiguration.Name})";
            }

            if (configuration.PaperSetting != null)
            {
                displayName += $" - {configuration.PaperSetting.Name}";
            }

            return displayName;
        }
    }
}
