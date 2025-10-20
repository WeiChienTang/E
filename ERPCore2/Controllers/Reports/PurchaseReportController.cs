using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services;
using ERPCore2.Models;
using ERPCore2.Controllers.Reports;

namespace ERPCore2.Controllers.Reports
{
    /// <summary>
    /// 採購報表控制器 - 處理所有採購相關的報表生成
    /// 包含：採購單、進貨單、採購退貨單、沖款單等報表
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
            ILogger<PurchaseReportController> logger)
            : base(reportPrintConfigurationService, logger)
        {
            _purchaseOrderReportService = purchaseOrderReportService;
            _purchaseReceivingReportService = purchaseReceivingReportService;
            _purchaseReturnReportService = purchaseReturnReportService;
        }

        #region 採購單報表

        /// <summary>
        /// 生成採購單報表
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("order/{id}")]
        public async Task<IActionResult> GetPurchaseOrderReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GenerateReportAsync(
                _purchaseOrderReportService,
                (svc, id, fmt, cfg) => svc.GeneratePurchaseOrderReportAsync(id, fmt, cfg),
                id, format, configId, reportType,
                "採購單");
        }

        /// <summary>
        /// 生成採購單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("order/{id}/print")]
        public async Task<IActionResult> PrintPurchaseOrderReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await PrintReportAsync(
                _purchaseOrderReportService,
                (svc, id, fmt, cfg) => svc.GeneratePurchaseOrderReportAsync(id, fmt, cfg),
                id, configId, reportType,
                "採購單");
        }

        /// <summary>
        /// 預覽採購單報表
        /// </summary>
        /// <param name="id">採購單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("order/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseOrderReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GetPurchaseOrderReport(id, "html", configId, reportType);
        }

        /// <summary>
        /// 批次生成採購單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("order/batch")]
        public async Task<IActionResult> BatchPrintPurchaseOrders(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchReportAsync(
                _purchaseOrderReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "採購單");
        }

        /// <summary>
        /// 批次生成採購單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("order/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseOrdersWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchPrintReportAsync(
                _purchaseOrderReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "採購單");
        }

        #endregion

        #region 進貨單報表

        /// <summary>
        /// 生成進貨單報表
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("receiving/{id}")]
        public async Task<IActionResult> GetPurchaseReceivingReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GenerateReportAsync(
                _purchaseReceivingReportService,
                (svc, id, fmt, cfg) => svc.GeneratePurchaseReceivingReportAsync(id, fmt, cfg),
                id, format, configId, reportType,
                "進貨單");
        }

        /// <summary>
        /// 生成進貨單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("receiving/{id}/print")]
        public async Task<IActionResult> PrintPurchaseReceivingReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await PrintReportAsync(
                _purchaseReceivingReportService,
                (svc, id, fmt, cfg) => svc.GeneratePurchaseReceivingReportAsync(id, fmt, cfg),
                id, configId, reportType,
                "進貨單");
        }

        /// <summary>
        /// 預覽進貨單報表
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("receiving/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseReceivingReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GetPurchaseReceivingReport(id, "html", configId, reportType);
        }

        /// <summary>
        /// 批次生成進貨單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("receiving/batch")]
        public async Task<IActionResult> BatchPrintPurchaseReceivings(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchReportAsync(
                _purchaseReceivingReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "進貨單");
        }

        /// <summary>
        /// 批次生成進貨單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("receiving/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseReceivingsWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchPrintReportAsync(
                _purchaseReceivingReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "進貨單");
        }

        #endregion

        #region 進貨退出單報表

        /// <summary>
        /// 生成進貨退出單報表
        /// </summary>
        /// <param name="id">進貨退出單ID</param>
        /// <param name="format">報表格式 (html/pdf)</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表內容</returns>
        [HttpGet("return/{id}")]
        public async Task<IActionResult> GetPurchaseReturnReport(
            int id,
            [FromQuery] string format = "html",
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GenerateReportAsync(
                _purchaseReturnReportService,
                (svc, id, fmt, cfg) => svc.GeneratePurchaseReturnReportAsync(id, fmt, cfg),
                id, format, configId, reportType,
                "進貨退出單");
        }

        /// <summary>
        /// 生成進貨退出單列印報表（自動觸發列印對話框）
        /// </summary>
        /// <param name="id">進貨退出單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpGet("return/{id}/print")]
        public async Task<IActionResult> PrintPurchaseReturnReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await PrintReportAsync(
                _purchaseReturnReportService,
                (svc, id, fmt, cfg) => svc.GeneratePurchaseReturnReportAsync(id, fmt, cfg),
                id, configId, reportType,
                "進貨退出單");
        }

        /// <summary>
        /// 預覽進貨退出單報表
        /// </summary>
        /// <param name="id">進貨退出單ID</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>報表預覽內容</returns>
        [HttpGet("return/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseReturnReport(
            int id,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await GetPurchaseReturnReport(id, "html", configId, reportType);
        }

        /// <summary>
        /// 批次生成進貨退出單報表（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>合併後的報表內容</returns>
        [HttpPost("return/batch")]
        public async Task<IActionResult> BatchPrintPurchaseReturns(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchReportAsync(
                _purchaseReturnReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "進貨退出單");
        }

        /// <summary>
        /// 批次生成進貨退出單報表並自動列印（支援多條件篩選）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="configId">列印配置ID（可選）</param>
        /// <param name="reportType">報表類型（可選）</param>
        /// <returns>可列印的報表內容</returns>
        [HttpPost("return/batch/print")]
        public async Task<IActionResult> BatchPrintPurchaseReturnsWithAuto(
            [FromBody] BatchPrintCriteria criteria,
            [FromQuery] int? configId = null,
            [FromQuery] string? reportType = null)
        {
            return await BatchPrintReportAsync(
                _purchaseReturnReportService,
                (svc, cri, fmt, cfg) => svc.GenerateBatchReportAsync(cri, fmt, cfg),
                criteria, configId, reportType,
                "進貨退出單");
        }

        #endregion

        // 未來在此處新增其他採購相關報表
        // 例如：沖款單等
    }
}
