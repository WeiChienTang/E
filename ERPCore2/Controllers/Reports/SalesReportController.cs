using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Controllers.Reports;

namespace ERPCore2.Controllers.Reports
{
    /// <summary>
    /// 銷售報表控制器 - 處理所有銷售相關的報表生成
    /// 包含：銷貨單等報表
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
            ILogger<SalesReportController> logger)
            : base(reportPrintConfigurationService, logger)
        {
            _salesOrderReportService = salesOrderReportService;
            _salesReturnReportService = salesReturnReportService;
            _quotationReportService = quotationReportService;
        }

        #region 銷貨單報表

        /// <summary>
        /// 生成銷貨單報表
        /// </summary>
        /// <param name="id">銷貨單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("order/{id}")]
        public async Task<IActionResult> GetSalesOrderReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GenerateReportAsync(
                _salesOrderReportService,
                (svc, id, fmt, cfg) => svc.GenerateSalesOrderReportAsync(id, fmt, cfg),
                id, format, configId, reportType,
                "銷貨單");
        }

        /// <summary>
        /// 生成銷貨單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">銷貨單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("order/{id}/print")]
        public async Task<IActionResult> PrintSalesOrderReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await PrintReportAsync(
                _salesOrderReportService,
                (svc, id, fmt, cfg) => svc.GenerateSalesOrderReportAsync(id, fmt, cfg),
                id, configId, reportType,
                "銷貨單");
        }

        /// <summary>
        /// 預覽銷貨單報表
        /// </summary>
        /// <param name="id">銷貨單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("order/{id}/preview")]
        public async Task<IActionResult> PreviewSalesOrderReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GetSalesOrderReport(id, "html", configId, reportType);
        }

        /// <summary>
        /// 批次生成銷貨單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("order/batch")]
        public async Task<IActionResult> BatchPrintSalesOrders(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchReportAsync(
                _salesOrderReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "銷貨單");
        }

        /// <summary>
        /// 批次生成銷貨單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("order/batch/print")]
        public async Task<IActionResult> BatchPrintSalesOrdersWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchPrintReportAsync(
                _salesOrderReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "銷貨單");
        }

        #endregion

        #region 銷貨退回單報表

        /// <summary>
        /// 生成銷貨退回單報表
        /// </summary>
        /// <param name="id">銷貨退回單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("return/{id}")]
        public async Task<IActionResult> GetSalesReturnReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GenerateReportAsync(
                _salesReturnReportService,
                (svc, id, fmt, cfg) => svc.GenerateSalesReturnReportAsync(id, fmt, cfg),
                id, format, configId, reportType,
                "銷貨退回單");
        }

        /// <summary>
        /// 生成銷貨退回單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">銷貨退回單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("return/{id}/print")]
        public async Task<IActionResult> PrintSalesReturnReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await PrintReportAsync(
                _salesReturnReportService,
                (svc, id, fmt, cfg) => svc.GenerateSalesReturnReportAsync(id, fmt, cfg),
                id, configId, reportType,
                "銷貨退回單");
        }

        /// <summary>
        /// 預覽銷貨退回單報表
        /// </summary>
        /// <param name="id">銷貨退回單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("return/{id}/preview")]
        public async Task<IActionResult> PreviewSalesReturnReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GetSalesReturnReport(id, "html", configId, reportType);
        }

        /// <summary>
        /// 批次生成銷貨退回單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("return/batch")]
        public async Task<IActionResult> BatchPrintSalesReturns(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchReportAsync(
                _salesReturnReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "銷貨退回單");
        }

        /// <summary>
        /// 批次生成銷貨退回單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("return/batch/print")]
        public async Task<IActionResult> BatchPrintSalesReturnsWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchPrintReportAsync(
                _salesReturnReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "銷貨退回單");
        }

        #endregion

        #region 報價單報表

        /// <summary>
        /// 生成報價單報表
        /// </summary>
        /// <param name="id">報價單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("quotation/{id}")]
        public async Task<IActionResult> GetQuotationReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GenerateReportAsync(
                _quotationReportService,
                (svc, id, fmt, cfg) => svc.GenerateQuotationReportAsync(id, fmt, cfg),
                id, format, configId, reportType,
                "報價單");
        }

        /// <summary>
        /// 生成報價單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">報價單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("quotation/{id}/print")]
        public async Task<IActionResult> PrintQuotationReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await PrintReportAsync(
                _quotationReportService,
                (svc, id, fmt, cfg) => svc.GenerateQuotationReportAsync(id, fmt, cfg),
                id, configId, reportType,
                "報價單");
        }

        /// <summary>
        /// 預覽報價單報表
        /// </summary>
        /// <param name="id">報價單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("quotation/{id}/preview")]
        public async Task<IActionResult> PreviewQuotationReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GetQuotationReport(id, "html", configId, reportType);
        }

        /// <summary>
        /// 批次生成報價單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("quotation/batch")]
        public async Task<IActionResult> BatchPrintQuotations(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchReportAsync(
                _quotationReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "報價單");
        }

        /// <summary>
        /// 批次生成報價單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("quotation/batch/print")]
        public async Task<IActionResult> BatchPrintQuotationsWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchPrintReportAsync(
                _quotationReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "報價單");
        }

        #endregion

        // 未來在此處新增其他銷售相關報表
        // 例如：出貨單等
    }
}
