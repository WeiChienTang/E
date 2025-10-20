using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Data.Entities;

namespace ERPCore2.Controllers.Reports
{
    /// <summary>
    /// 報表控制器基底類別 - 提供所有報表控制器共用的邏輯
    /// 設計目的：
    /// 1. 統一報表生成、列印、預覽、批次列印的邏輯
    /// 2. 減少子類別的重複程式碼
    /// 3. 集中管理錯誤處理、日誌記錄、權限驗證等橫切關注點
    /// </summary>
    [ApiController]
    public abstract class BaseReportController : ControllerBase
    {
        protected readonly IReportPrintConfigurationService _reportPrintConfigurationService;
        protected readonly ILogger _logger;

        protected BaseReportController(
            IReportPrintConfigurationService reportPrintConfigurationService,
            ILogger logger)
        {
            _reportPrintConfigurationService = reportPrintConfigurationService;
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
                _logger.LogInformation("開始生成{ReportName}報表 - ID: {Id}, Format: {Format}", reportName, id, format);

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
                _logger.LogWarning(ex, "找不到{ReportName} - ID: {Id}", reportName, id);
                return NotFound(new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                _logger.LogWarning(ex, "不支援的報表格式 - Format: {Format}", format);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成{ReportName}報表時發生錯誤 - ID: {Id}", reportName, id);
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
                _logger.LogInformation("開始生成{ReportName}列印報表 - ID: {Id}", reportName, id);

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
                _logger.LogWarning(ex, "找不到{ReportName} - ID: {Id}", reportName, id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成{ReportName}列印報表時發生錯誤 - ID: {Id}", reportName, id);
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
                _logger.LogInformation("開始批次生成{ReportName}報表 - 條件: {Criteria}", reportName, criteria.GetSummary());

                // 驗證篩選條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
                    _logger.LogWarning("批次列印條件驗證失敗 - 錯誤: {Errors}", validation.GetAllErrors());
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
                _logger.LogWarning(ex, "批次列印{ReportName}條件錯誤", reportName);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次生成{ReportName}報表時發生錯誤", reportName);
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
                _logger.LogInformation("開始批次列印{ReportName}（自動列印） - 條件: {Criteria}", reportName, criteria.GetSummary());

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
                _logger.LogError(ex, "批次列印{ReportName}時發生錯誤", reportName);
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "載入列印配置失敗 - ConfigId: {ConfigId}, ReportType: {ReportType}", configId, reportType);
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
    }
}
