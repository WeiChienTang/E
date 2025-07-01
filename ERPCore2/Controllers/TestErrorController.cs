using Microsoft.AspNetCore.Mvc;

namespace ERPCore2.Controllers
{
    /// <summary>
    /// 錯誤測試控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestErrorController : ControllerBase
    {
        private readonly ILogger<TestErrorController> _logger;

        public TestErrorController(ILogger<TestErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 測試拋出例外
        /// </summary>
        [HttpGet("throw-exception")]
        public IActionResult ThrowException()
        {
            _logger.LogInformation("即將透過 API 拋出例外");
            throw new InvalidOperationException("這是一個測試用的 API 例外");
        }

        /// <summary>
        /// 測試拋出空引用例外
        /// </summary>
        [HttpGet("throw-null-reference")]
        public IActionResult ThrowNullReference()
        {
            _logger.LogInformation("即將拋出空引用例外");
            string? nullString = null;
            return Ok(nullString!.Length);
        }

        /// <summary>
        /// 測試拋出參數例外
        /// </summary>
        [HttpGet("throw-argument-exception")]
        public IActionResult ThrowArgumentException()
        {
            _logger.LogInformation("即將拋出參數例外");
            throw new ArgumentException("無效的參數", nameof(ThrowArgumentException));
        }
    }
}
