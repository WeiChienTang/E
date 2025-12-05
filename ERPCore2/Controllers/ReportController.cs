using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Data.Entities;

namespace ERPCore2.Controllers
{
    /// <summary>
    /// 報表 API 控制器（舊版，保留相容性）
    /// ⚠️ 已棄用：此控制器僅保留作為向後相容
    /// 新的報表端點已遷移至 Controllers/Reports/ 目錄下的專門控制器
    /// - 採購報表：PurchaseReportController (api/purchase-report)
    /// - 銷售報表：SalesReportController (api/sales-report)
    /// - 庫存報表：InventoryReportController (api/inventory-report)
    /// 
    /// 建議前端逐步遷移至新路由，舊路由將在未來版本移除
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Obsolete("此控制器已棄用，請使用新的專門報表控制器 (Controllers/Reports/)")]
    public class ReportController : ControllerBase
    {
        private readonly IPurchaseOrderReportService _purchaseOrderReportService;
        private readonly IPurchaseReceivingReportService _purchaseReceivingReportService;
        private readonly IProductBarcodeReportService _productBarcodeReportService;
        private readonly IReportPrintConfigurationService _reportPrintConfigurationService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IPurchaseOrderReportService purchaseOrderReportService,
            IPurchaseReceivingReportService purchaseReceivingReportService,
            IProductBarcodeReportService productBarcodeReportService,
            IReportPrintConfigurationService reportPrintConfigurationService,
            ILogger<ReportController> logger)
        {
            _purchaseOrderReportService = purchaseOrderReportService;
            _purchaseReceivingReportService = purchaseReceivingReportService;
            _productBarcodeReportService = productBarcodeReportService;
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
                return NotFound(new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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

                // 驗證篩選條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
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
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
                return StatusCode(500, new { message = "批次列印報表時發生錯誤", detail = ex.Message });
            }
        }

        #region 進貨單報表

        /// <summary>
        /// 生成進貨單報表
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("purchase-receiving/{id}")]
        public async Task<IActionResult> GetPurchaseReceivingReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {

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
                var reportHtml = await _purchaseReceivingReportService.GeneratePurchaseReceivingReportAsync(
                    id,
                    reportFormat,
                    printConfig);

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
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 生成進貨單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("purchase-receiving/{id}/print")]
        public async Task<IActionResult> PrintPurchaseReceivingReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {

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
                var reportHtml = await _purchaseReceivingReportService.GeneratePurchaseReceivingReportAsync(
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
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "生成列印報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽進貨單報表
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("purchase-receiving/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseReceivingReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {

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
                var reportHtml = await _purchaseReceivingReportService.GeneratePurchaseReceivingReportAsync(
                    id,
                    ReportFormat.Html,
                    printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "生成預覽報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成進貨單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("purchase-receiving/batch")]
        public async Task<IActionResult> BatchPrintPurchaseReceivings(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
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
                var reportHtml = await _purchaseReceivingReportService.GenerateBatchReportAsync(
                    criteria,
                    ReportFormat.Html,
                    printConfig);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "批次生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成進貨單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("purchase-receiving/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseReceivingsWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            try
            {

                // 先生成報表
                var response = await BatchPrintPurchaseReceivings(criteria, configId, reportType);
                
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
                return StatusCode(500, new { message = "批次列印報表時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 商品條碼列印

        /// <summary>
        /// 批次生成商品條碼列印報表
        /// </summary>
        /// <param name="criteria">條碼列印條件</param>
        /// <returns>可列印的條碼 HTML</returns>
        [HttpPost("products/barcode/batch")]
        public async Task<IActionResult> BatchPrintProductBarcodes(
            [FromBody] ProductBarcodePrintCriteria criteria)
        {
            try
            {

                // 生成條碼報表
                var reportHtml = await _productBarcodeReportService.GenerateBarcodeReportAsync(criteria);

                return Content(reportHtml, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "批次列印條碼時發生錯誤", detail = ex.Message });
            }
        }

        #endregion
    }
}
