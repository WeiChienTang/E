using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Controllers.Reports;

namespace ERPCore2.Controllers.Reports
{
    /// <summary>
    /// 採購報表控制器 - 處理所有採購相關的報表生成
    /// 包含：採購單、進貨單、採購退貨單、沖款單等報表
    /// 支援兩種列印模式：
    /// 1. 瀏覽器列印（window.print）- 使用者手動選擇印表機
    /// 2. 伺服器端直接列印 - 使用預設印表機和紙張設定
    /// </summary>
    [Route("api/purchase-report")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "採購報表")]
    public class PurchaseReportController : BaseReportController
    {
        private readonly IPurchaseOrderReportService _purchaseOrderReportService;
        private readonly IPurchaseReceivingReportService _purchaseReceivingReportService;
        private readonly IPurchaseReturnReportService _purchaseReturnReportService;

        public PurchaseReportController(
            IPurchaseOrderReportService purchaseOrderReportService,
            IPurchaseReceivingReportService purchaseReceivingReportService,
            IPurchaseReturnReportService purchaseReturnReportService,
            IReportPrintConfigurationService reportPrintConfigurationService,
            IReportPrintService reportPrintService,
            ILogger<PurchaseReportController> logger)
            : base(reportPrintConfigurationService, reportPrintService, logger)
        {
            _purchaseOrderReportService = purchaseOrderReportService;
            _purchaseReceivingReportService = purchaseReceivingReportService;
            _purchaseReturnReportService = purchaseReturnReportService;
        }

        #region 採購單報表

        /// <summary>
        /// 取得採購單純文字報表（預覽用）
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <returns>純文字報表（HTML 包裝）</returns>
        [HttpGet("order/{id}")]
        public async Task<IActionResult> GetPurchaseOrderReport(int id)
        {
            try
            {
                var plainText = await _purchaseOrderReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"採購單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成採購單報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 採購單直接列印（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="reportId">報表識別碼（預設 PO001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("order/{id}/direct")]
        public async Task<IActionResult> DirectPrintPurchaseOrder(
            int id,
            [FromQuery] string reportId = "PO001")
        {
            try
            {
                var result = await _purchaseOrderReportService.DirectPrintByReportIdAsync(id, reportId);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "列印成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "直接列印採購單時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得採購單純文字報表（自動列印）
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <returns>純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpGet("order/{id}/print")]
        public async Task<IActionResult> PrintPurchaseOrderReport(int id)
        {
            try
            {
                var plainText = await _purchaseOrderReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"採購單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成採購單列印報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽採購單報表（同 GetPurchaseOrderReport）
        /// </summary>
        [HttpGet("order/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseOrderReport(int id)
        {
            return await GetPurchaseOrderReport(id);
        }

        /// <summary>
        /// 批次生成採購單報表
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝）</returns>
        [HttpPost("order/batch")]
        public async Task<IActionResult> BatchPrintPurchaseOrders([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _purchaseOrderReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "採購單批次報表");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次採購單報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成採購單報表並自動列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpPost("order/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseOrdersWithAuto([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _purchaseOrderReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "採購單批次報表", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次採購單列印報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次直接列印採購單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼（預設 PO001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("order/batch/direct")]
        public async Task<IActionResult> DirectPrintBatchPurchaseOrders(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] string reportId = "PO001")
        {
            try
            {
                var result = await _purchaseOrderReportService.DirectPrintBatchAsync(criteria, reportId);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "批次列印成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次直接列印採購單時發生錯誤");
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 進貨單報表

        /// <summary>
        /// 取得進貨單純文字報表（預覽用）
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <returns>純文字報表（HTML 包裝）</returns>
        [HttpGet("receiving/{id}")]
        public async Task<IActionResult> GetPurchaseReceivingReport(int id)
        {
            try
            {
                var plainText = await _purchaseReceivingReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"進貨單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成進貨單報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 進貨單直接列印（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="reportId">報表識別碼（預設 PO002）</param>
        /// <returns>列印結果</returns>
        [HttpPost("receiving/{id}/direct")]
        public async Task<IActionResult> DirectPrintPurchaseReceiving(
            int id,
            [FromQuery] string reportId = "PO002")
        {
            try
            {
                var result = await _purchaseReceivingReportService.DirectPrintByReportIdAsync(id, reportId);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "列印成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "直接列印進貨單時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得進貨單純文字報表（自動列印）
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <returns>純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpGet("receiving/{id}/print")]
        public async Task<IActionResult> PrintPurchaseReceivingReport(int id)
        {
            try
            {
                var plainText = await _purchaseReceivingReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"進貨單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成進貨單列印報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽進貨單報表（同 GetPurchaseReceivingReport）
        /// </summary>
        [HttpGet("receiving/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseReceivingReport(int id)
        {
            return await GetPurchaseReceivingReport(id);
        }

        /// <summary>
        /// 批次生成進貨單報表
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝）</returns>
        [HttpPost("receiving/batch")]
        public async Task<IActionResult> BatchPrintPurchaseReceivings([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _purchaseReceivingReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "進貨單批次報表");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次進貨單報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成進貨單報表並自動列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpPost("receiving/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseReceivingsWithAuto([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _purchaseReceivingReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "進貨單批次報表", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次進貨單列印報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次直接列印進貨單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼（預設 PO002）</param>
        /// <returns>列印結果</returns>
        [HttpPost("receiving/batch/direct")]
        public async Task<IActionResult> DirectPrintBatchPurchaseReceivings(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] string reportId = "PO002")
        {
            try
            {
                var result = await _purchaseReceivingReportService.DirectPrintBatchAsync(criteria, reportId);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "批次列印成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次直接列印進貨單時發生錯誤");
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 進貨退出單報表

        /// <summary>
        /// 取得進貨退出單純文字報表（預覽用）
        /// </summary>
        /// <param name="id">進貨退出單ID</param>
        /// <returns>純文字報表（HTML 包裝）</returns>
        [HttpGet("return/{id}")]
        public async Task<IActionResult> GetPurchaseReturnReport(int id)
        {
            try
            {
                var plainText = await _purchaseReturnReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"進貨退出單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成進貨退出單報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 進貨退出單直接列印（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="id">進貨退出單ID</param>
        /// <param name="reportId">報表識別碼（預設 PO003）</param>
        /// <returns>列印結果</returns>
        [HttpPost("return/{id}/direct")]
        public async Task<IActionResult> DirectPrintPurchaseReturn(
            int id,
            [FromQuery] string reportId = "PO003")
        {
            try
            {
                var result = await _purchaseReturnReportService.DirectPrintByReportIdAsync(id, reportId);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "列印成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "直接列印進貨退出單時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得進貨退出單純文字報表（自動列印）
        /// </summary>
        /// <param name="id">進貨退出單ID</param>
        /// <returns>純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpGet("return/{id}/print")]
        public async Task<IActionResult> PrintPurchaseReturnReport(int id)
        {
            try
            {
                var plainText = await _purchaseReturnReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"進貨退出單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成進貨退出單列印報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽進貨退出單報表（同 GetPurchaseReturnReport）
        /// </summary>
        [HttpGet("return/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseReturnReport(int id)
        {
            return await GetPurchaseReturnReport(id);
        }

        /// <summary>
        /// 批次生成進貨退出單報表
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝）</returns>
        [HttpPost("return/batch")]
        public async Task<IActionResult> BatchPrintPurchaseReturns([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _purchaseReturnReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "進貨退出單批次報表");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次進貨退出單報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成進貨退出單報表並自動列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpPost("return/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseReturnsWithAuto([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _purchaseReturnReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "進貨退出單批次報表", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次進貨退出單列印報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次直接列印進貨退出單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼（預設 PO003）</param>
        /// <returns>列印結果</returns>
        [HttpPost("return/batch/direct")]
        public async Task<IActionResult> DirectPrintBatchPurchaseReturns(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] string reportId = "PO003")
        {
            try
            {
                var result = await _purchaseReturnReportService.DirectPrintBatchAsync(criteria, reportId);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "批次列印成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次直接列印進貨退出單時發生錯誤");
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 純文字報表輔助方法

        /// <summary>
        /// 將純文字包裝成 HTML 格式（用於瀏覽器預覽）
        /// </summary>
        /// <param name="plainText">純文字內容</param>
        /// <param name="title">報表標題</param>
        /// <param name="autoPrint">是否自動列印</param>
        /// <returns>HTML 內容</returns>
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
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Courier New', Consolas, 'Microsoft JhengHei', monospace;
            font-size: 12px;
            line-height: 1.4;
            margin: 20px;
            background-color: #fff;
        }}
        pre {{
            white-space: pre-wrap;
            word-wrap: break-word;
            margin: 0;
            padding: 0;
        }}
        @media print {{
            body {{ margin: 0; }}
            pre {{ page-break-inside: avoid; }}
        }}
    </style>
</head>
<body>
<pre>{escapedText}</pre>
<script>{autoPrintScript}</script>
</body>
</html>";
        }

        #endregion
    }
}
