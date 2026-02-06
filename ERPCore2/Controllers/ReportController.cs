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
                // 使用格式化報表渲染為圖片
                var images = await _purchaseOrderReportService.RenderToImagesAsync(id);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = $"無法生成採購單報表：找不到 ID {id}" });
                }
                
                var html = WrapImagesAsHtml(images, $"採購單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
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
                // 使用格式化報表渲染為圖片，並自動列印
                var images = await _purchaseOrderReportService.RenderToImagesAsync(id);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = $"無法生成採購單報表：找不到 ID {id}" });
                }
                
                var html = WrapImagesAsHtml(images, $"採購單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
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
                // 使用格式化報表渲染為圖片
                var images = await _purchaseOrderReportService.RenderToImagesAsync(id);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = $"無法生成採購單報表：找不到 ID {id}" });
                }
                
                var html = WrapImagesAsHtml(images, $"採購單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
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
        /// 批次生成採購單報表（已棄用）
        /// </summary>
        /// <returns>錯誤訊息：此端點已棄用</returns>
        [HttpPost("purchase-order/batch")]
        public IActionResult BatchPrintPurchaseOrders(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return StatusCode(410, new { 
                message = "此端點已棄用", 
                detail = "批次列印功能請使用 UI 介面操作" 
            });
        }

        /// <summary>
        /// 批次生成採購單報表並自動列印（已棄用）
        /// </summary>
        /// <returns>錯誤訊息：此端點已棄用</returns>
        [HttpPost("purchase-order/batch/print")]
        public IActionResult BatchPrintPurchaseOrdersWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return StatusCode(410, new { 
                message = "此端點已棄用", 
                detail = "批次列印功能請使用 UI 介面操作" 
            });
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
                // 使用格式化報表渲染為圖片
                var images = await _purchaseReceivingReportService.RenderToImagesAsync(id);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = $"無法生成進貨單報表：找不到 ID {id}" });
                }
                
                var html = WrapImagesAsHtml(images, $"進貨單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
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
                // 使用格式化報表渲染為圖片，並自動列印
                var images = await _purchaseReceivingReportService.RenderToImagesAsync(id);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = $"無法生成進貨單報表：找不到 ID {id}" });
                }
                
                var html = WrapImagesAsHtml(images, $"進貨單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
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
                // 使用格式化報表渲染為圖片
                var images = await _purchaseReceivingReportService.RenderToImagesAsync(id);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = $"無法生成進貨單報表：找不到 ID {id}" });
                }
                
                var html = WrapImagesAsHtml(images, $"進貨單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
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
        /// 注意：批次列印建議使用 DirectPrintBatchAsync 方法直接發送到印表機
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

                // 批次報表目前不支援透過 HTTP 預覽，請使用 DirectPrintBatchAsync 方法直接列印
                return BadRequest(new { message = "批次報表請使用 DirectPrintBatchAsync 方法直接列印" });
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

        #region 純文字報表輔助方法

        /// <summary>
        /// 將純文字包裝成 HTML 格式（用於瀏覽器預覽）
        /// </summary>
        private static string WrapPlainTextAsHtml(string plainText, string title, bool autoPrint = false)
        {
            var escapedText = System.Net.WebUtility.HtmlEncode(plainText);
            var autoPrintScript = autoPrint 
                ? @"
    window.addEventListener('load', function() {
        setTimeout(function() { window.print(); }, 500);
    });" 
                : "";
            
            return $@"<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>{title}</title>
    <style>
        body {{ font-family: 'Courier New', Consolas, monospace; font-size: 12px; margin: 20px; }}
        pre {{ white-space: pre-wrap; margin: 0; }}
        @media print {{ body {{ margin: 0; }} }}
    </style>
</head>
<body>
<pre>{escapedText}</pre>
<script>{autoPrintScript}</script>
</body>
</html>";
        }

        /// <summary>
        /// 將圖片列表包裝成 HTML 頁面
        /// </summary>
        private static string WrapImagesAsHtml(List<byte[]> images, string title, bool autoPrint = false)
        {
            var autoPrintScript = autoPrint 
                ? @"
    window.addEventListener('load', function() {
        setTimeout(function() { window.print(); }, 500);
    });" 
                : "";
            
            var imagesHtml = string.Join("\n", images.Select((img, i) => 
                $@"<div class='page'>
                    <div class='page-number'>第 {i + 1} 頁</div>
                    <img src='data:image/png;base64,{Convert.ToBase64String(img)}' alt='第 {i + 1} 頁' />
                </div>"));

            return $@"<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>{title}</title>
    <style>
        body {{ font-family: sans-serif; margin: 20px; background: #f0f0f0; }}
        .page {{ background: white; margin: 20px auto; padding: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.2); max-width: 100%; }}
        .page img {{ width: 100%; height: auto; display: block; }}
        .page-number {{ text-align: center; color: #666; font-size: 12px; margin-bottom: 10px; }}
        @media print {{ 
            body {{ margin: 0; background: white; }} 
            .page {{ box-shadow: none; margin: 0; page-break-after: always; }}
            .page-number {{ display: none; }}
        }}
    </style>
</head>
<body>
{imagesHtml}
<script>{autoPrintScript}</script>
</body>
</html>";
        }

        #endregion
    }
}
