using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Data.Entities;

namespace ERPCore2.Controllers
{
    /// <summary>
    /// 報表 API 控制器 - 提供各種單據的列印報表
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IPurchaseOrderReportService _purchaseOrderReportService;
        private readonly IReportPrintConfigurationService _reportPrintConfigurationService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IPurchaseOrderReportService purchaseOrderReportService,
            IReportPrintConfigurationService reportPrintConfigurationService,
            ILogger<ReportController> logger)
        {
            _purchaseOrderReportService = purchaseOrderReportService;
            _reportPrintConfigurationService = reportPrintConfigurationService;
            _logger = logger;
        }

        /// <summary>
        /// 生成採購單報表
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("purchase-order/{id}")]
        public async Task<IActionResult> GetPurchaseOrderReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {
                _logger.LogInformation("開始生成採購單報表 - ID: {Id}, Format: {Format}", id, format);

                // 載入列印配置
                ReportPrintConfiguration? printConfig = null;
                if (configId.HasValue)
                {
                    printConfig = await _reportPrintConfigurationService.GetByIdAsync(configId.Value);
                }
                else if (!string.IsNullOrEmpty(reportType))
                {
                    printConfig = await _reportPrintConfigurationService.GetCompleteConfigurationAsync(reportType);
                }

                // 解析報表格式
                var reportFormat = format.ToLower() switch
                {
                    "html" => ReportFormat.Html,
                    "pdf" => throw new NotImplementedException("PDF 格式尚未實作"),
                    "excel" => ReportFormat.Excel,
                    _ => ReportFormat.Html
                };

                // 生成報表
                var reportHtml = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(
                    id,
                    reportFormat,
                    printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "找不到採購單 - ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                _logger.LogWarning(ex, "不支援的報表格式 - Format: {Format}", format);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成採購單報表時發生錯誤 - ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 生成採購單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("purchase-order/{id}/print")]
        public async Task<IActionResult> PrintPurchaseOrderReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {
                _logger.LogInformation("開始生成採購單列印報表 - ID: {Id}", id);

                // 載入列印配置
                ReportPrintConfiguration? printConfig = null;
                if (configId.HasValue)
                {
                    printConfig = await _reportPrintConfigurationService.GetByIdAsync(configId.Value);
                }
                else if (!string.IsNullOrEmpty(reportType))
                {
                    printConfig = await _reportPrintConfigurationService.GetCompleteConfigurationAsync(reportType);
                }

                // 生成報表 HTML
                var reportHtml = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(
                    id,
                    ReportFormat.Html,
                    printConfig);

                // 添加自動列印的 JavaScript
                var printScript = @"
<script>
    window.addEventListener('load', function() {
        setTimeout(function() {
            window.print();
        }, 500);
    });
</script>
";
                reportHtml = reportHtml.Replace("</body>", printScript + "</body>");

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "找不到採購單 - ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成採購單列印報表時發生錯誤 - ID: {Id}", id);
                return StatusCode(500, new { message = "生成列印報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽採購單報表
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("purchase-order/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseOrderReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {
                _logger.LogInformation("開始生成採購單預覽報表 - ID: {Id}", id);

                // 載入列印配置
                ReportPrintConfiguration? printConfig = null;
                if (configId.HasValue)
                {
                    printConfig = await _reportPrintConfigurationService.GetByIdAsync(configId.Value);
                }
                else if (!string.IsNullOrEmpty(reportType))
                {
                    printConfig = await _reportPrintConfigurationService.GetCompleteConfigurationAsync(reportType);
                }

                // 生成報表
                var reportHtml = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(
                    id,
                    ReportFormat.Html,
                    printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "找不到採購單 - ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成採購單預覽報表時發生錯誤 - ID: {Id}", id);
                return StatusCode(500, new { message = "生成預覽報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成採購單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("purchase-order/batch")]
        public async Task<IActionResult> BatchPrintPurchaseOrders(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {
                _logger.LogInformation("開始批次生成採購單報表 - 條件: {Criteria}", criteria.GetSummary());

                // 驗證篩選條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
                    _logger.LogWarning("批次列印條件驗證失敗 - 錯誤: {Errors}", validation.GetAllErrors());
                    return BadRequest(new { message = "篩選條件驗證失敗", errors = validation.Errors });
                }

                // 載入列印配置
                ReportPrintConfiguration? printConfig = null;
                if (configId.HasValue)
                {
                    printConfig = await _reportPrintConfigurationService.GetByIdAsync(configId.Value);
                }
                else if (!string.IsNullOrEmpty(reportType))
                {
                    printConfig = await _reportPrintConfigurationService.GetCompleteConfigurationAsync(reportType);
                }
                else if (!string.IsNullOrEmpty(criteria.ReportType))
                {
                    printConfig = await _reportPrintConfigurationService.GetCompleteConfigurationAsync(criteria.ReportType);
                }

                // 生成批次報表
                var reportHtml = await _purchaseOrderReportService.GenerateBatchReportAsync(
                    criteria,
                    ReportFormat.Html,
                    printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "批次列印條件錯誤");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次生成採購單報表時發生錯誤");
                return StatusCode(500, new { message = "批次生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成採購單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("purchase-order/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseOrdersWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {
                _logger.LogInformation("開始批次列印採購單（自動列印） - 條件: {Criteria}", criteria.GetSummary());

                // 先生成報表
                var response = await BatchPrintPurchaseOrders(criteria, configId, reportType);
                
                if (response is ContentResult contentResult)
                {
                    var reportHtml = contentResult.Content;
                    
                    // 添加自動列印的 JavaScript
                    var printScript = @"
<script>
    window.addEventListener('load', function() {
        setTimeout(function() {
            window.print();
        }, 500);
    });
</script>
";
                    reportHtml = reportHtml!.Replace("</body>", printScript + "</body>");

                    return Content(reportHtml, "text/html; charset=utf-8");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次列印採購單時發生錯誤");
                return StatusCode(500, new { message = "批次列印報表時發生錯誤", detail = ex.Message });
            }
        }
    }
}
