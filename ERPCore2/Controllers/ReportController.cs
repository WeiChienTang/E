using Microsoft.AspNetCore.Mvc;
using ERPCore2.Services.Reports;
using ERPCore2.Services;
using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Controllers
{
    /// <summary>
    /// 報表控制器 - 提供報表生成的 Web API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IPurchaseOrderReportService _purchaseOrderReportService;
        private readonly IReportPrintConfigurationService _reportPrintConfigurationService;

        public ReportController(
            IPurchaseOrderReportService purchaseOrderReportService,
            IReportPrintConfigurationService reportPrintConfigurationService)
        {
            _purchaseOrderReportService = purchaseOrderReportService;
            _reportPrintConfigurationService = reportPrintConfigurationService;
        }

        /// <summary>
        /// 生成採購單報表
        /// </summary>
        /// <param name="id">採購單 ID</param>
        /// <param name="format">報表格式（html/excel）</param>
        /// <param name="reportType">報表類型（用於查找列印配置）</param>
        /// <param name="configId">指定的配置 ID</param>
        /// <returns>報表內容</returns>
        [HttpGet("purchase-order/{id}")]
        public async Task<IActionResult> GeneratePurchaseOrderReport(
            int id, 
            string format = "html", 
            string? reportType = null, 
            int? configId = null)
        {
            try
            {
                var reportFormat = format.ToLower() switch
                {
                    "excel" => ReportFormat.Excel,
                    _ => ReportFormat.Html
                };

                // 嘗試取得列印配置
                ReportPrintConfiguration? printConfig = null;
                
                if (configId.HasValue)
                {
                    // 使用指定的配置 ID
                    try
                    {
                        var configService = HttpContext.RequestServices.GetService<IReportPrintConfigurationService>();
                        if (configService != null)
                        {
                            printConfig = await configService.GetByIdAsync(configId.Value);
                        }
                    }
                    catch (Exception)
                    {
                        // 如果找不到配置，則使用預設設定
                    }
                }
                else if (!string.IsNullOrEmpty(reportType))
                {
                    // 使用報表類型查找配置
                    try
                    {
                        printConfig = await _reportPrintConfigurationService.GetByReportTypeAsync(reportType);
                    }
                    catch (Exception)
                    {
                        // 如果找不到配置，則使用預設設定
                    }
                }
                else
                {
                    // 使用預設的採購單配置
                    try
                    {
                        printConfig = await _reportPrintConfigurationService.GetByReportTypeAsync("PurchaseOrder");
                    }
                    catch (Exception)
                    {
                        // 如果找不到配置，則使用預設設定
                    }
                }

                // 根據是否有列印配置選擇對應的方法
                string reportContent;
                if (printConfig != null)
                {
                    reportContent = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(id, reportFormat, printConfig);
                }
                else
                {
                    reportContent = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(id, reportFormat);
                }

                return reportFormat switch
                {
                    ReportFormat.Html => Content(reportContent, "text/html", System.Text.Encoding.UTF8),
                    ReportFormat.Excel => File(Convert.FromBase64String(reportContent), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"採購單_{id}.xlsx"),
                    _ => Content(reportContent, "text/html", System.Text.Encoding.UTF8)
                };
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
                return StatusCode(500, new { message = "生成報表時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 生成可列印的採購單報表（會自動觸發列印對話框）
        /// </summary>
        /// <param name="id">採購單 ID</param>
        /// <returns>可列印的 HTML 報表</returns>
        [HttpGet("purchase-order/{id}/print")]
        public async Task<IActionResult> PrintPurchaseOrderReport(int id)
        {
            try
            {
                var reportService = HttpContext.RequestServices.GetService<IReportService>();
                if (reportService == null)
                {
                    return StatusCode(500, new { message = "無法取得報表服務" });
                }

                // 取得報表資料和配置
                var configuration = _purchaseOrderReportService.GetPurchaseOrderReportConfiguration();
                
                // 這裡需要手動載入資料，因為我們直接使用通用報表服務
                // 在實際應用中，可以考慮讓 PurchaseOrderReportService 提供資料載入方法
                var htmlContent = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(id, ReportFormat.Html);
                
                // 簡單的方式：在現有 HTML 中加入列印 JavaScript
                var printableHtml = htmlContent.Replace("</body>", @"
                    <script>
                        window.onload = function() {
                            window.print();
                        };
                    </script>
                </body>");

                return Content(printableHtml, "text/html", System.Text.Encoding.UTF8);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "生成可列印報表時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 預覽採購單報表
        /// </summary>
        /// <param name="id">採購單 ID</param>
        /// <returns>報表預覽頁面</returns>
        [HttpGet("purchase-order/{id}/preview")]
        public async Task<IActionResult> PreviewPurchaseOrderReport(int id)
        {
            try
            {
                var reportContent = await _purchaseOrderReportService.GeneratePurchaseOrderReportAsync(id, ReportFormat.Html);
                
                // 在預覽模式中加入一些額外的按鈕和樣式
                var previewHtml = reportContent.Replace("</body>", @"
                    <div style='position: fixed; top: 10px; right: 10px; z-index: 1000;'>
                        <button onclick='window.print()' style='background: #007bff; color: white; border: none; padding: 10px 20px; margin: 5px; border-radius: 4px; cursor: pointer;'>列印</button>
                        <button onclick='window.close()' style='background: #6c757d; color: white; border: none; padding: 10px 20px; margin: 5px; border-radius: 4px; cursor: pointer;'>關閉</button>
                    </div>
                    <style>
                        @media print {
                            div[style*='position: fixed'] { display: none !important; }
                        }
                    </style>
                </body>");

                return Content(previewHtml, "text/html", System.Text.Encoding.UTF8);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "預覽報表時發生內部錯誤", detail = ex.Message });
            }
        }
    }
}
