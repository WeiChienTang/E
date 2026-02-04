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
                    return null;
                }

                // 如果找到配置且狀態為啟用，則使用該配置
                if (configuration.Status == Data.Enums.EntityStatus.Active)
                {
                    return configuration;
                }

                // 配置存在但已停用
                return null;
            }
            catch (Exception)
            {
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
        [Obsolete("請使用 BuildPrintUrl 方法，並傳入正確的 reportType 參數。例如：BuildPrintUrl(baseUrl, \"purchase-report/order\", reportId, configuration)")]
        public static string BuildReportUrl(
            string baseUrl, 
            int reportId, 
            ReportPrintConfiguration? configuration = null, 
            string format = "html")
        {
            // ⚠️ 此方法已棄用，請使用新的路由格式
            var url = $"{baseUrl}/api/purchase-report/order/{reportId}?format={format}";
            
            if (configuration != null)
            {
                url += $"&configId={configuration.Id}&reportName={Uri.EscapeDataString(configuration.ReportName)}";
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

        /// <summary>
        /// 使用隱藏 iframe 執行列印 - 通用方法
        /// </summary>
        /// <param name="printUrl">列印 URL (需包含 autoprint=true 參數)</param>
        /// <param name="jsRuntime">JavaScript 運行時</param>
        /// <param name="iframeId">iframe 的 ID，預設為 'printFrame'</param>
        /// <returns>執行結果</returns>
        public static async Task<bool> ExecutePrintWithHiddenIframeAsync(
            string printUrl,
            IJSRuntime jsRuntime,
            string iframeId = "printFrame")
        {
            try
            {
                // 確保 URL 包含 autoprint 參數
                if (!printUrl.Contains("autoprint=true", StringComparison.OrdinalIgnoreCase))
                {
                    // 自動添加 autoprint 參數
                    var separator = printUrl.Contains("?") ? "&" : "?";
                    printUrl = $"{printUrl}{separator}autoprint=true";
                }

                // 使用完全隱藏的 iframe 載入報表，報表會自動觸發列印
                await jsRuntime.InvokeVoidAsync("eval", $@"
                    (function() {{
                        // 移除舊的列印 iframe（如果存在）
                        const oldFrame = document.getElementById('{iframeId}');
                        if (oldFrame) {{
                            oldFrame.remove();
                        }}
                        
                        // 建立新的完全隱藏 iframe
                        const iframe = document.createElement('iframe');
                        iframe.id = '{iframeId}';
                        iframe.style.display = 'none';
                        iframe.style.position = 'absolute';
                        iframe.style.width = '0';
                        iframe.style.height = '0';
                        iframe.style.border = 'none';
                        iframe.style.visibility = 'hidden';
                        
                        // 設定 iframe 來源並加入頁面（不需要 onload，報表自己會處理）
                        document.body.appendChild(iframe);
                        iframe.src = '{printUrl}';
                    }})();
                ");
                
                return true;
            }
            catch (Exception ex)
            {
                await jsRuntime.InvokeVoidAsync("console.error", $"執行列印時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 建立通用的報表列印 URL（支援不同單據類型）
        /// </summary>
        /// <param name="baseUrl">基礎 URL（通常是 NavigationManager.BaseUri）</param>
        /// <param name="reportType">報表類型（如 "purchase-order", "purchase-receiving", "sales-order" 等）</param>
        /// <param name="documentId">單據 ID</param>
        /// <param name="configuration">列印配置（可選）</param>
        /// <param name="autoprint">是否自動列印，預設為 true</param>
        /// <returns>完整的報表 URL</returns>
        public static string BuildPrintUrl(
            string baseUrl,
            string reportType,
            int documentId,
            ReportPrintConfiguration? configuration = null,
            bool autoprint = true)
        {
            // 移除尾部的斜線
            baseUrl = baseUrl.TrimEnd('/');
            
            // 建立基礎 URL - 使用新的路由格式
            // 新格式：/api/{domain}-report/{type}/{id}
            // 範例：/api/purchase-report/order/123
            var url = $"{baseUrl}/api/{reportType}/{documentId}";
            
            // 添加查詢參數
            var parameters = new List<string>();
            
            if (autoprint)
            {
                parameters.Add("autoprint=true");
            }
            
            if (configuration != null)
            {
                parameters.Add($"configId={configuration.Id}");
                parameters.Add($"reportName={Uri.EscapeDataString(configuration.ReportName)}");
            }
            
            // 組合 URL
            if (parameters.Any())
            {
                url += "?" + string.Join("&", parameters);
            }
            
            return url;
        }

        /// <summary>
        /// 驗證實體是否已核准（適用於需要核准後才能列印的單據）
        /// </summary>
        /// <param name="isApproved">實體的核准狀態</param>
        /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
        /// <returns>驗證結果和錯誤訊息</returns>
        public static (bool IsValid, string ErrorMessage) ValidateApprovalStatus(
            bool isApproved,
            string entityName = "單據")
        {
            if (!isApproved)
            {
                return (false, $"請先核准{entityName}後再進行列印");
            }
            
            return (true, "");
        }

        /// <summary>
        /// 驗證實體和ID是否有效（用於列印前檢查）
        /// </summary>
        /// <param name="entity">實體物件</param>
        /// <param name="entityId">實體 ID</param>
        /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
        /// <returns>驗證結果和錯誤訊息</returns>
        public static (bool IsValid, string ErrorMessage) ValidateEntityForPrint(
            object? entity,
            int? entityId,
            string entityName = "單據")
        {
            if (entity == null)
            {
                return (false, $"請先儲存{entityName}資料後再進行列印");
            }
            
            if (!entityId.HasValue || entityId.Value <= 0)
            {
                return (false, $"請先儲存{entityName}後再進行列印");
            }
            
            return (true, "");
        }

        /// <summary>
        /// 完整的列印驗證（包含實體、ID、核准狀態）
        /// </summary>
        /// <param name="entity">實體物件</param>
        /// <param name="entityId">實體 ID</param>
        /// <param name="isApproved">核准狀態</param>
        /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
        /// <param name="requireApproval">是否需要核准才能列印，預設為 true</param>
        /// <returns>驗證結果和錯誤訊息</returns>
        public static (bool IsValid, string ErrorMessage) ValidateForPrint(
            object? entity,
            int? entityId,
            bool isApproved,
            string entityName = "單據",
            bool requireApproval = true)
        {
            // 檢查實體和ID
            var (entityValid, entityError) = ValidateEntityForPrint(entity, entityId, entityName);
            if (!entityValid)
            {
                return (false, entityError);
            }
            
            // 檢查核准狀態（如果需要）
            if (requireApproval)
            {
                var (approvalValid, approvalError) = ValidateApprovalStatus(isApproved, entityName);
                if (!approvalValid)
                {
                    return (false, approvalError);
                }
            }
            
            return (true, "");
        }

        /// <summary>
        /// 執行伺服器端直接列印（呼叫 /direct API 端點）
        /// 此方法使用伺服器端的 System.Drawing.Printing 直接列印到預設印表機，
        /// 不會顯示 Windows 列印對話框
        /// </summary>
        /// <param name="baseUrl">基礎 URL</param>
        /// <param name="reportType">報表類型（如 "purchase-report/order"）</param>
        /// <param name="documentId">單據 ID</param>
        /// <param name="httpClient">HttpClient 實例</param>
        /// <returns>列印結果（成功/失敗及訊息）</returns>
        public static async Task<(bool IsSuccess, string Message)> ExecuteServerDirectPrintAsync(
            string baseUrl,
            string reportType,
            int documentId,
            HttpClient httpClient)
        {
            try
            {
                // 移除尾部的斜線
                baseUrl = baseUrl.TrimEnd('/');
                
                // 建立直接列印 API URL
                // 格式：POST /api/{reportType}/{documentId}/direct
                var directPrintUrl = $"{baseUrl}/api/{reportType}/{documentId}/direct";
                
                // 發送 POST 請求到伺服器端直接列印
                var response = await httpClient.PostAsync(directPrintUrl, null);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // 嘗試解析回應內容以取得印表機名稱
                    if (content.Contains("成功"))
                    {
                        return (true, content);
                    }
                    return (true, "列印任務已成功送出至印表機");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"列印失敗：{response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"網路連線錯誤：{ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"列印時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 執行伺服器端直接列印（使用 IJSRuntime 透過 fetch API）
        /// 此方法適合在 Blazor 組件中使用，使用 JavaScript fetch 呼叫伺服器 API
        /// </summary>
        /// <remarks>
        /// ⚠️ 此方法已棄用，請改用報表服務的 DirectPrintAsync 方法
        /// 例如：await PurchaseOrderReportService.DirectPrintAsync(orderId, printerName)
        /// 直接列印模式不需要經過 API，更簡單可靠
        /// </remarks>
        /// <param name="baseUrl">基礎 URL</param>
        /// <param name="reportType">報表類型（如 "purchase-report/order"）</param>
        /// <param name="documentId">單據 ID</param>
        /// <param name="jsRuntime">IJSRuntime 實例</param>
        /// <returns>列印結果（成功/失敗及訊息）</returns>
        [Obsolete("此方法已棄用，請改用報表服務的 DirectPrintAsync 方法，例如 PurchaseOrderReportService.DirectPrintAsync()")]
        public static async Task<(bool IsSuccess, string Message)> ExecuteServerDirectPrintWithJsAsync(
            string baseUrl,
            string reportType,
            int documentId,
            IJSRuntime jsRuntime)
        {
            try
            {
                // 移除尾部的斜線
                baseUrl = baseUrl.TrimEnd('/');
                
                // 建立直接列印 API URL（僅保留圖片模式 /direct）
                var directPrintUrl = $"{baseUrl}/api/{reportType}/{documentId}/direct";
                
                // 使用 JavaScript fetch API 發送 POST 請求
                var result = await jsRuntime.InvokeAsync<string>("eval", $@"
                    (async function() {{
                        try {{
                            const response = await fetch('{directPrintUrl}', {{
                                method: 'POST',
                                headers: {{
                                    'Content-Type': 'application/json'
                                }}
                            }});
                            
                            if (response.ok) {{
                                const text = await response.text();
                                return JSON.stringify({{ success: true, message: text || '列印任務已送出' }});
                            }} else {{
                                const errorText = await response.text();
                                return JSON.stringify({{ success: false, message: '列印失敗: ' + response.status + ' - ' + errorText }});
                            }}
                        }} catch (error) {{
                            return JSON.stringify({{ success: false, message: '網路錯誤: ' + error.message }});
                        }}
                    }})();
                ");
                
                // 解析 JSON 結果
                var jsonResult = System.Text.Json.JsonDocument.Parse(result);
                var success = jsonResult.RootElement.GetProperty("success").GetBoolean();
                var message = jsonResult.RootElement.GetProperty("message").GetString() ?? "";
                
                return (success, message);
            }
            catch (Exception ex)
            {
                return (false, $"執行列印時發生錯誤：{ex.Message}");
            }
        }
    }
}
