using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Data.Entities;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Controllers.Reports
{
    /// <summary>
    /// 報表控制器基底類別 - 提供所有報表控制器共用的邏輯
    /// 設計目的：
    /// 1. 統一報表生成、列印、預覽、批次列印的邏輯
    /// 2. 減少子類別的重複程式碼
    /// 3. 集中管理錯誤處理、日誌記錄、權限驗證等橫切關注點
    /// 4. 支援兩種列印模式：瀏覽器列印（window.print）和伺服器端直接列印
    /// </summary>
    [ApiController]
    public abstract class BaseReportController : ControllerBase
    {
        protected readonly IReportPrintConfigurationService _reportPrintConfigurationService;
        protected readonly IReportPrintService? _reportPrintService;
        protected readonly ILogger _logger;

        protected BaseReportController(
            IReportPrintConfigurationService reportPrintConfigurationService,
            ILogger logger)
        {
            _reportPrintConfigurationService = reportPrintConfigurationService;
            _logger = logger;
        }

        /// <summary>
        /// 建構函式 - 支援直接列印功能
        /// </summary>
        protected BaseReportController(
            IReportPrintConfigurationService reportPrintConfigurationService,
            IReportPrintService reportPrintService,
            ILogger logger)
        {
            _reportPrintConfigurationService = reportPrintConfigurationService;
            _reportPrintService = reportPrintService;
            _logger = logger;
        }

        #region 通用報表生成方法

        /// <summary>
        /// 通用報表生成邏輯 - 支援單筆報表
        /// </summary>
        /// <typeparam name="TService">報表服務類型</typeparam>
        /// <param name="service">報表服務實例</param>
        /// <param name="generateFunc">報表生成函式</param>
        /// <param name="id">單據ID</param>
        /// <param name="format">報表格式</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <param name="reportName">報表名稱（用於日誌）</param>
        /// <returns>報表內容</returns>
        protected async Task<IActionResult> GenerateReportAsync<TService>(
            TService service,
            Func<TService, int, ReportFormat, ReportPrintConfiguration?, Task<string>> generateFunc,
            int id,
            string format,
            int? configId,
            string? reportType,
            string reportName)
            where TService : class
        {
            try
            {

                // 載入列印配置
                var printConfig = await LoadPrintConfigurationAsync(configId, reportType);

                // 解析報表格式
                var reportFormat = ParseReportFormat(format);

                // 生成報表
                var reportHtml = await generateFunc(service, id, reportFormat, printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"生成{reportName}報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 通用列印報表邏輯 - 自動觸發列印對話框
        /// </summary>
        protected async Task<IActionResult> PrintReportAsync<TService>(
            TService service,
            Func<TService, int, ReportFormat, ReportPrintConfiguration?, Task<string>> generateFunc,
            int id,
            int? configId,
            string? reportType,
            string reportName)
            where TService : class
        {
            try
            {

                // 載入列印配置
                var printConfig = await LoadPrintConfigurationAsync(configId, reportType);

                // 生成報表 HTML
                var reportHtml = await generateFunc(service, id, ReportFormat.Html, printConfig);

                // 添加自動列印的 JavaScript
                reportHtml = AddAutoPrintScript(reportHtml);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"生成{reportName}列印報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 通用批次報表生成邏輯
        /// </summary>
        protected async Task<IActionResult> BatchReportAsync<TService>(
            TService service,
            Func<TService, BatchPrintCriteria, ReportFormat, ReportPrintConfiguration?, Task<string>> generateFunc,
            BatchPrintCriteria criteria,
            int? configId,
            string? reportType,
            string reportName)
            where TService : class
        {
            try
            {

                // 驗證篩選條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
                    return BadRequest(new { message = "篩選條件驗證失敗", errors = validation.Errors });
                }

                // 載入列印配置
                var printConfig = await LoadPrintConfigurationAsync(configId, reportType, criteria.ReportType);

                // 生成批次報表
                var reportHtml = await generateFunc(service, criteria, ReportFormat.Html, printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"批次生成{reportName}報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 通用批次列印報表邏輯 - 自動觸發列印
        /// </summary>
        protected async Task<IActionResult> BatchPrintReportAsync<TService>(
            TService service,
            Func<TService, BatchPrintCriteria, ReportFormat, ReportPrintConfiguration?, Task<string>> generateFunc,
            BatchPrintCriteria criteria,
            int? configId,
            string? reportType,
            string reportName)
            where TService : class
        {
            try
            {

                // 先生成報表
                var response = await BatchReportAsync(service, generateFunc, criteria, configId, reportType, reportName);

                if (response is ContentResult contentResult)
                {
                    var reportHtml = contentResult.Content;

                    // 添加自動列印的 JavaScript
                    reportHtml = AddAutoPrintScript(reportHtml!);

                    return Content(reportHtml, "text/html; charset=utf-8");
                }

                return response;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"批次列印{reportName}報表時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 載入列印配置
        /// </summary>
        /// <param name="configId">列印配置ID</param>
        /// <param name="reportType">報表類型</param>
        /// <param name="fallbackReportType">備用報表類型（用於批次列印）</param>
        /// <returns>列印配置</returns>
        protected async Task<ReportPrintConfiguration?> LoadPrintConfigurationAsync(
            int? configId = null,
            string? reportType = null,
            string? fallbackReportType = null)
        {
            try
            {
                if (configId.HasValue)
                {
                    return await _reportPrintConfigurationService.GetByIdAsync(configId.Value);
                }
                
                if (!string.IsNullOrEmpty(reportType))
                {
                    return await _reportPrintConfigurationService.GetCompleteConfigurationAsync(reportType);
                }
                
                if (!string.IsNullOrEmpty(fallbackReportType))
                {
                    return await _reportPrintConfigurationService.GetCompleteConfigurationAsync(fallbackReportType);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析報表格式
        /// </summary>
        /// <param name="format">格式字串</param>
        /// <returns>報表格式列舉</returns>
        protected ReportFormat ParseReportFormat(string format)
        {
            return format.ToLower() switch
            {
                "html" => ReportFormat.Html,
                "pdf" => throw new NotImplementedException("PDF 格式尚未實作"),
                "excel" => ReportFormat.Excel,
                _ => ReportFormat.Html
            };
        }

        /// <summary>
        /// 添加自動列印腳本
        /// </summary>
        /// <param name="html">原始 HTML</param>
        /// <returns>加入列印腳本的 HTML</returns>
        protected string AddAutoPrintScript(string html)
        {
            const string printScript = @"
<script>
    window.addEventListener('load', function() {
        setTimeout(function() {
            window.print();
        }, 500);
    });
</script>
";
            return html.Replace("</body>", printScript + "</body>");
        }

        #endregion

        #region 伺服器端直接列印方法

        /// <summary>
        /// 使用伺服器端直接列印（使用預設印表機和紙張設定）
        /// </summary>
        /// <typeparam name="TService">報表服務類型</typeparam>
        /// <param name="service">報表服務實例</param>
        /// <param name="generateFunc">報表生成函式</param>
        /// <param name="id">單據ID</param>
        /// <param name="reportId">報表識別碼（如 PO001，用於載入預設配置）</param>
        /// <param name="documentName">文件名稱（顯示在列印佇列中）</param>
        /// <returns>列印結果</returns>
        protected async Task<IActionResult> DirectPrintReportAsync<TService>(
            TService service,
            Func<TService, int, ReportFormat, ReportPrintConfiguration?, Task<string>> generateFunc,
            int id,
            string reportId,
            string documentName)
            where TService : class
        {
            try
            {
                if (_reportPrintService == null)
                {
                    return BadRequest(new { 
                        message = "直接列印服務未配置，請使用預覽列印功能",
                        fallbackUrl = $"?format=html&printMode=browser"
                    });
                }

                // 載入報表列印配置
                var printConfig = await _reportPrintService.GetDefaultPrintConfigAsync(reportId);
                if (printConfig == null)
                {
                    return BadRequest(new { 
                        message = $"找不到報表 '{reportId}' 的列印配置，請先在「報表列印配置」中設定印表機和紙張",
                        configUrl = "/reportPrintConfigurations"
                    });
                }

                // 檢查印表機是否可用
                if (printConfig.PrinterConfigurationId.HasValue)
                {
                    var printerCheck = await _reportPrintService.CheckPrinterAvailableAsync(printConfig.PrinterConfigurationId.Value);
                    if (!printerCheck.IsSuccess)
                    {
                        return BadRequest(new { 
                            message = $"印表機不可用: {printerCheck.ErrorMessage}",
                            fallbackUrl = $"?format=html&printMode=browser"
                        });
                    }
                }

                // 生成報表 HTML
                var reportHtml = await generateFunc(service, id, ReportFormat.Html, printConfig);

                // 執行直接列印
                var printResult = await _reportPrintService.PrintReportAsync(reportHtml, printConfig, documentName);

                if (printResult.IsSuccess)
                {
                    return Ok(new { 
                        message = "列印任務已送出",
                        success = true,
                        documentName = documentName
                    });
                }
                else
                {
                    return BadRequest(new { 
                        message = printResult.ErrorMessage,
                        success = false,
                        fallbackUrl = $"?format=html&printMode=browser"
                    });
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "直接列印報表時發生錯誤");
                return StatusCode(500, new { 
                    message = $"直接列印時發生錯誤: {ex.Message}",
                    fallbackUrl = $"?format=html&printMode=browser"
                });
            }
        }

        /// <summary>
        /// 預覽報表並提供列印選項（返回 HTML 並包含列印模式資訊）
        /// </summary>
        /// <typeparam name="TService">報表服務類型</typeparam>
        /// <param name="service">報表服務實例</param>
        /// <param name="generateFunc">報表生成函式</param>
        /// <param name="id">單據ID</param>
        /// <param name="reportId">報表識別碼</param>
        /// <param name="printMode">列印模式：browser=瀏覽器列印, direct=直接列印, preview=僅預覽</param>
        /// <param name="reportName">報表名稱</param>
        /// <returns>報表 HTML（含列印控制）</returns>
        protected async Task<IActionResult> PreviewAndPrintReportAsync<TService>(
            TService service,
            Func<TService, int, ReportFormat, ReportPrintConfiguration?, Task<string>> generateFunc,
            int id,
            string reportId,
            string printMode,
            string reportName)
            where TService : class
        {
            try
            {
                // 載入列印配置
                var printConfig = await LoadPrintConfigurationAsync(null, reportId);

                // 生成報表
                var reportHtml = await generateFunc(service, id, ReportFormat.Html, printConfig);

                switch (printMode?.ToLower())
                {
                    case "direct":
                        // 直接列印模式
                        if (_reportPrintService != null && printConfig != null)
                        {
                            var printResult = await _reportPrintService.PrintReportAsync(
                                reportHtml, 
                                printConfig, 
                                $"{reportName}-{id}");

                            if (printResult.IsSuccess)
                            {
                                // 返回列印成功訊息頁面
                                return Content(GeneratePrintSuccessHtml(reportName, id), "text/html; charset=utf-8");
                            }
                            else
                            {
                                // 列印失敗，回退到瀏覽器列印
                                reportHtml = AddPrintErrorBanner(reportHtml, printResult.ErrorMessage ?? "列印失敗");
                                reportHtml = AddAutoPrintScript(reportHtml);
                            }
                        }
                        else
                        {
                            // 無法直接列印，回退到瀏覽器列印
                            reportHtml = AddAutoPrintScript(reportHtml);
                        }
                        break;

                    case "browser":
                        // 瀏覽器列印模式（自動觸發 window.print）
                        reportHtml = AddAutoPrintScript(reportHtml);
                        break;

                    case "preview":
                    default:
                        // 僅預覽模式，添加列印按鈕工具列
                        reportHtml = AddPrintToolbar(reportHtml, id, reportId, printConfig != null);
                        break;
                }

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "預覽報表時發生錯誤");
                return StatusCode(500, new { message = $"生成{reportName}報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 添加列印工具列到報表 HTML
        /// </summary>
        private string AddPrintToolbar(string html, int id, string reportId, bool hasDirectPrintConfig)
        {
            var directPrintButton = hasDirectPrintConfig
                ? $@"<button onclick=""directPrint()"" class=""btn btn-success"">
                        <i class=""bi bi-printer-fill""></i> 直接列印（使用預設印表機）
                    </button>"
                : @"<button class=""btn btn-secondary"" disabled title=""請先在報表列印配置中設定印表機"">
                        <i class=""bi bi-printer-fill""></i> 直接列印（未設定）
                    </button>";

            var toolbar = $@"
<style>
    .print-toolbar {{
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        background: #f8f9fa;
        padding: 10px 20px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        z-index: 9999;
        display: flex;
        gap: 10px;
        align-items: center;
    }}
    .print-toolbar .btn {{
        padding: 8px 16px;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 14px;
        display: inline-flex;
        align-items: center;
        gap: 6px;
    }}
    .print-toolbar .btn-primary {{
        background: #0d6efd;
        color: white;
    }}
    .print-toolbar .btn-success {{
        background: #198754;
        color: white;
    }}
    .print-toolbar .btn-secondary {{
        background: #6c757d;
        color: white;
    }}
    .print-toolbar .btn:hover:not(:disabled) {{
        opacity: 0.9;
    }}
    .print-toolbar .spacer {{
        flex: 1;
    }}
    body {{
        padding-top: 60px;
    }}
    @media print {{
        .print-toolbar {{
            display: none !important;
        }}
        body {{
            padding-top: 0;
        }}
    }}
</style>
<div class=""print-toolbar"">
    <button onclick=""browserPrint()"" class=""btn btn-primary"">
        <i class=""bi bi-printer""></i> 瀏覽器列印
    </button>
    {directPrintButton}
    <div class=""spacer""></div>
    <button onclick=""window.close()"" class=""btn btn-secondary"">
        <i class=""bi bi-x-lg""></i> 關閉
    </button>
</div>
<script>
    function browserPrint() {{
        window.print();
    }}
    async function directPrint() {{
        try {{
            const response = await fetch(window.location.pathname + '/direct?reportId={reportId}', {{
                method: 'POST'
            }});
            const result = await response.json();
            if (result.success) {{
                alert('列印任務已送出: ' + result.documentName);
            }} else {{
                alert('列印失敗: ' + result.message + '\\n\\n將使用瀏覽器列印...');
                window.print();
            }}
        }} catch (e) {{
            alert('列印時發生錯誤，將使用瀏覽器列印...');
            window.print();
        }}
    }}
</script>
";
            return html.Replace("<body>", "<body>" + toolbar);
        }

        /// <summary>
        /// 生成列印成功訊息 HTML
        /// </summary>
        private string GeneratePrintSuccessHtml(string reportName, int id)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>列印成功</title>
    <style>
        body {{
            font-family: 'Microsoft JhengHei', sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: #f0f0f0;
        }}
        .success-box {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            text-align: center;
        }}
        .success-icon {{
            font-size: 48px;
            color: #198754;
            margin-bottom: 20px;
        }}
        .btn {{
            padding: 10px 20px;
            background: #0d6efd;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class=""success-box"">
        <div class=""success-icon"">✓</div>
        <h2>列印任務已送出</h2>
        <p>{reportName} (ID: {id}) 已送至印表機列印佇列</p>
        <button class=""btn"" onclick=""window.close()"">關閉視窗</button>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// 添加列印錯誤橫幅
        /// </summary>
        private string AddPrintErrorBanner(string html, string errorMessage)
        {
            var banner = $@"
<div style=""background: #fff3cd; padding: 10px 20px; border-bottom: 1px solid #ffc107; font-family: sans-serif;"">
    <strong>⚠ 直接列印失敗:</strong> {System.Net.WebUtility.HtmlEncode(errorMessage)}。改為使用瀏覽器列印...
</div>";
            return html.Replace("<body>", "<body>" + banner);
        }

        #endregion
    }
}
