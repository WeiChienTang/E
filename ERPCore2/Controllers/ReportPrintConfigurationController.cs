using Microsoft.AspNetCore.Mvc;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Controllers
{
    /// <summary>
    /// 報表列印配置控制器 - 提供報表列印配置管理的 Web API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReportPrintConfigurationController : ControllerBase
    {
        private readonly IReportPrintConfigurationService _reportPrintConfigurationService;

        public ReportPrintConfigurationController(IReportPrintConfigurationService reportPrintConfigurationService)
        {
            _reportPrintConfigurationService = reportPrintConfigurationService;
        }

        /// <summary>
        /// 根據報表類型取得列印配置
        /// </summary>
        /// <param name="reportType">報表類型</param>
        /// <returns>報表列印配置</returns>
        [HttpGet("by-report-type/{reportType}")]
        public async Task<IActionResult> GetByReportType(string reportType)
        {
            try
            {
                var configuration = await _reportPrintConfigurationService.GetByReportTypeAsync(reportType);
                if (configuration == null)
                {
                    return NotFound(new { message = $"找不到報表類型 '{reportType}' 的列印配置" });
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得報表列印配置時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得完整的報表列印配置（包含印表機和紙張設定）
        /// </summary>
        /// <param name="reportType">報表類型</param>
        /// <returns>完整的報表列印配置</returns>
        [HttpGet("complete/{reportType}")]
        public async Task<IActionResult> GetCompleteConfiguration(string reportType)
        {
            try
            {
                var configuration = await _reportPrintConfigurationService.GetCompleteConfigurationAsync(reportType);
                if (configuration == null)
                {
                    return NotFound(new { message = $"找不到報表類型 '{reportType}' 的列印配置" });
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得完整報表列印配置時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 取得所有啟用的報表列印配置
        /// </summary>
        /// <returns>所有啟用的報表列印配置清單</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveConfigurations()
        {
            try
            {
                var configurations = await _reportPrintConfigurationService.GetActiveConfigurationsAsync();
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得啟用的報表列印配置時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 檢查報表類型是否已存在
        /// </summary>
        /// <param name="reportType">報表類型</param>
        /// <param name="excludeId">排除的 ID（用於更新時檢查）</param>
        /// <returns>是否存在</returns>
        [HttpGet("exists/{reportType}")]
        public async Task<IActionResult> CheckReportTypeExists(string reportType, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _reportPrintConfigurationService.IsReportTypeExistsAsync(reportType, excludeId);
                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "檢查報表類型時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 複製報表列印配置
        /// </summary>
        /// <param name="request">複製請求</param>
        /// <returns>複製結果</returns>
        [HttpPost("copy")]
        public async Task<IActionResult> CopyConfiguration([FromBody] CopyConfigurationRequest request)
        {
            try
            {
                var result = await _reportPrintConfigurationService.CopyConfigurationAsync(
                    request.SourceReportType, 
                    request.TargetReportType, 
                    request.TargetReportName);

                if (result)
                {
                    return Ok(new { message = "報表列印配置複製成功" });
                }
                else
                {
                    return BadRequest(new { message = "報表列印配置複製失敗" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "複製報表列印配置時發生內部錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 批量更新報表列印配置
        /// </summary>
        /// <param name="configurations">配置清單</param>
        /// <returns>更新結果</returns>
        [HttpPut("batch")]
        public async Task<IActionResult> BatchUpdate([FromBody] List<ReportPrintConfiguration> configurations)
        {
            try
            {
                var result = await _reportPrintConfigurationService.BatchUpdateAsync(configurations);
                if (result)
                {
                    return Ok(new { message = "批量更新報表列印配置成功" });
                }
                else
                {
                    return BadRequest(new { message = "批量更新報表列印配置失敗" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "批量更新報表列印配置時發生內部錯誤", detail = ex.Message });
            }
        }
    }

    /// <summary>
    /// 複製配置請求模型
    /// </summary>
    public class CopyConfigurationRequest
    {
        /// <summary>
        /// 來源報表類型
        /// </summary>
        public string SourceReportType { get; set; } = string.Empty;

        /// <summary>
        /// 目標報表類型
        /// </summary>
        public string TargetReportType { get; set; } = string.Empty;

        /// <summary>
        /// 目標報表名稱
        /// </summary>
        public string TargetReportName { get; set; } = string.Empty;
    }
}
