using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Controllers.Reports;

namespace ERPCore2.Controllers.Reports
{
    /// <summary>
    /// 銷售報表控制器 - 處理所有銷售相關的報表生成
    /// 包含：銷貨單、銷貨退回單、報價單等報表
    /// 支援兩種列印模式：
    /// 1. 瀏覽器列印（window.print）- 使用者手動選擇印表機
    /// 2. 伺服器端直接列印 - 使用預設印表機和紙張設定
    /// </summary>
    [Route("api/sales-report")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "銷售報表")]
    public class SalesReportController : BaseReportController
    {
        private readonly ISalesOrderReportService _salesOrderReportService;
        private readonly ISalesReturnReportService _salesReturnReportService;
        private readonly IQuotationReportService _quotationReportService;

        public SalesReportController(
            ISalesOrderReportService salesOrderReportService,
            ISalesReturnReportService salesReturnReportService,
            IQuotationReportService quotationReportService,
            IReportPrintConfigurationService reportPrintConfigurationService,
            IReportPrintService reportPrintService,
            ILogger<SalesReportController> logger)
            : base(reportPrintConfigurationService, reportPrintService, logger)
        {
            _salesOrderReportService = salesOrderReportService;
            _salesReturnReportService = salesReturnReportService;
            _quotationReportService = quotationReportService;
        }

        #region 銷貨單報表

        /// <summary>
        /// 取得銷貨單純文字報表（預覽用）
        /// </summary>
        /// <param name="id">銷貨單ID</param>
        /// <returns>純文字報表（HTML 包裝）</returns>
        [HttpGet("order/{id}")]
        public async Task<IActionResult> GetSalesOrderReport(int id)
        {
            try
            {
                var plainText = await _salesOrderReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"銷貨單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成銷貨單報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 銷貨單直接列印（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="id">銷貨單ID</param>
        /// <param name="reportId">報表識別碼（預設 SO001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("order/{id}/direct")]
        public async Task<IActionResult> DirectPrintSalesOrder(
            int id,
            [FromQuery] string reportId = "SO001")
        {
            try
            {
                var result = await _salesOrderReportService.DirectPrintByReportIdAsync(id, reportId);
                
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
                _logger.LogError(ex, "直接列印銷貨單時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得銷貨單純文字報表（自動列印）
        /// </summary>
        /// <param name="id">銷貨單ID</param>
        /// <returns>純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpGet("order/{id}/print")]
        public async Task<IActionResult> PrintSalesOrderReport(int id)
        {
            try
            {
                var plainText = await _salesOrderReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"銷貨單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成銷貨單列印報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽銷貨單報表（同 GetSalesOrderReport）
        /// </summary>
        [HttpGet("order/{id}/preview")]
        public async Task<IActionResult> PreviewSalesOrderReport(int id)
        {
            return await GetSalesOrderReport(id);
        }

        /// <summary>
        /// 批次生成銷貨單報表
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝）</returns>
        [HttpPost("order/batch")]
        public async Task<IActionResult> BatchPrintSalesOrders([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _salesOrderReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "銷貨單批次報表");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次銷貨單報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成銷貨單報表並自動列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpPost("order/batch/print")]
        public async Task<IActionResult> BatchPrintSalesOrdersWithAuto([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _salesOrderReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "銷貨單批次報表", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次銷貨單列印報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次直接列印銷貨單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼（預設 SO001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("order/batch/direct")]
        public async Task<IActionResult> DirectPrintBatchSalesOrders(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] string reportId = "SO001")
        {
            try
            {
                var result = await _salesOrderReportService.DirectPrintBatchAsync(criteria, reportId);
                
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
                _logger.LogError(ex, "批次直接列印銷貨單時發生錯誤");
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 銷貨退回單報表

        /// <summary>
        /// 取得銷貨退回單純文字報表（預覽用）
        /// </summary>
        /// <param name="id">銷貨退回單ID</param>
        /// <returns>純文字報表（HTML 包裝）</returns>
        [HttpGet("return/{id}")]
        public async Task<IActionResult> GetSalesReturnReport(int id)
        {
            try
            {
                var plainText = await _salesReturnReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"銷貨退回單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成銷貨退回單報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 銷貨退回單直接列印（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="id">銷貨退回單ID</param>
        /// <param name="reportId">報表識別碼（預設 SR001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("return/{id}/direct")]
        public async Task<IActionResult> DirectPrintSalesReturn(
            int id,
            [FromQuery] string reportId = "SR001")
        {
            try
            {
                var result = await _salesReturnReportService.DirectPrintByReportIdAsync(id, reportId);
                
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
                _logger.LogError(ex, "直接列印銷貨退回單時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得銷貨退回單純文字報表（自動列印）
        /// </summary>
        /// <param name="id">銷貨退回單ID</param>
        /// <returns>純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpGet("return/{id}/print")]
        public async Task<IActionResult> PrintSalesReturnReport(int id)
        {
            try
            {
                var plainText = await _salesReturnReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"銷貨退回單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成銷貨退回單列印報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽銷貨退回單報表（同 GetSalesReturnReport）
        /// </summary>
        [HttpGet("return/{id}/preview")]
        public async Task<IActionResult> PreviewSalesReturnReport(int id)
        {
            return await GetSalesReturnReport(id);
        }

        /// <summary>
        /// 批次生成銷貨退回單報表
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝）</returns>
        [HttpPost("return/batch")]
        public async Task<IActionResult> BatchPrintSalesReturns([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _salesReturnReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "銷貨退回單批次報表");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次銷貨退回單報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成銷貨退回單報表並自動列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpPost("return/batch/print")]
        public async Task<IActionResult> BatchPrintSalesReturnsWithAuto([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _salesReturnReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "銷貨退回單批次報表", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次銷貨退回單列印報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次直接列印銷貨退回單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼（預設 SR001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("return/batch/direct")]
        public async Task<IActionResult> DirectPrintBatchSalesReturns(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] string reportId = "SR001")
        {
            try
            {
                var result = await _salesReturnReportService.DirectPrintBatchAsync(criteria, reportId);
                
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
                _logger.LogError(ex, "批次直接列印銷貨退回單時發生錯誤");
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        #endregion

        #region 報價單報表

        /// <summary>
        /// 取得報價單純文字報表（預覽用）
        /// </summary>
        /// <param name="id">報價單ID</param>
        /// <returns>純文字報表（HTML 包裝）</returns>
        [HttpGet("quotation/{id}")]
        public async Task<IActionResult> GetQuotationReport(int id)
        {
            try
            {
                var plainText = await _quotationReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"報價單報表 - {id}");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成報價單報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 報價單直接列印（使用 System.Drawing.Printing）
        /// </summary>
        /// <param name="id">報價單ID</param>
        /// <param name="reportId">報表識別碼（預設 QT001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("quotation/{id}/direct")]
        public async Task<IActionResult> DirectPrintQuotation(
            int id,
            [FromQuery] string reportId = "QT001")
        {
            try
            {
                var result = await _quotationReportService.DirectPrintByReportIdAsync(id, reportId);
                
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
                _logger.LogError(ex, "直接列印報價單時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得報價單純文字報表（自動列印）
        /// </summary>
        /// <param name="id">報價單ID</param>
        /// <returns>純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpGet("quotation/{id}/print")]
        public async Task<IActionResult> PrintQuotationReport(int id)
        {
            try
            {
                var plainText = await _quotationReportService.GeneratePlainTextReportAsync(id);
                var html = WrapPlainTextAsHtml(plainText, $"報價單報表 - {id}", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成報價單列印報表時發生錯誤，ID: {Id}", id);
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽報價單報表（同 GetQuotationReport）
        /// </summary>
        [HttpGet("quotation/{id}/preview")]
        public async Task<IActionResult> PreviewQuotationReport(int id)
        {
            return await GetQuotationReport(id);
        }

        /// <summary>
        /// 批次生成報價單報表
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝）</returns>
        [HttpPost("quotation/batch")]
        public async Task<IActionResult> BatchPrintQuotations([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _quotationReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "報價單批次報表");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次報價單報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次生成報價單報表並自動列印
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>合併後的純文字報表（HTML 包裝，含自動列印腳本）</returns>
        [HttpPost("quotation/batch/print")]
        public async Task<IActionResult> BatchPrintQuotationsWithAuto([FromBody] BatchPrintCriteria criteria)
        {
            try
            {
                var plainText = await _quotationReportService.GenerateBatchPlainTextReportAsync(criteria);
                var html = WrapPlainTextAsHtml(plainText, "報價單批次報表", autoPrint: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成批次報價單列印報表時發生錯誤");
                return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批次直接列印報價單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="reportId">報表識別碼（預設 QT001）</param>
        /// <returns>列印結果</returns>
        [HttpPost("quotation/batch/direct")]
        public async Task<IActionResult> DirectPrintBatchQuotations(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] string reportId = "QT001")
        {
            try
            {
                var result = await _quotationReportService.DirectPrintBatchAsync(criteria, reportId);
                
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
                _logger.LogError(ex, "批次直接列印報價單時發生錯誤");
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

        // 未來在此處新增其他銷售相關報表
        // 例如：出貨單等
    }
}
