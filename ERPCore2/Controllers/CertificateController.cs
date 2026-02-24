using ERPCore2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ERPCore2.Controllers
{
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ICertificateService _certificateService;

        public CertificateController(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ICertificateService certificateService)
        {
            _environment = environment;
            _configuration = configuration;
            _certificateService = certificateService;
        }

        /// <summary>
        /// 一鍵產生自簽憑證（限管理員使用）
        /// </summary>
        [HttpPost("/api/certificate/generate")]
        public async Task<IActionResult> GenerateCertificate()
        {
            var (success, message) = await _certificateService.GenerateSelfSignedCertificateAsync();
            if (success)
                return Ok(new { message });
            return StatusCode(500, new { message });
        }

        /// <summary>
        /// 下載公開憑證 (.cer)
        /// </summary>
        [HttpGet("/api/certificate/cer")]
        public IActionResult DownloadCertificate()
        {
            var certPath = Path.Combine(_environment.ContentRootPath, "Resources", "erpcore2.cer");

            if (!System.IO.File.Exists(certPath))
            {
                return NotFound("憑證檔案不存在，請聯絡系統管理員。");
            }

            var fileBytes = System.IO.File.ReadAllBytes(certPath);
            return File(fileBytes, "application/pkix-cert", "erpcore2.cer");
        }

        /// <summary>
        /// 下載一鍵安裝批次檔，會自動帶入伺服器位址
        /// </summary>
        [HttpGet("/api/certificate/installer")]
        public IActionResult DownloadInstaller()
        {
            // 取得伺服器 Host（純 IP 或 hostname，不含 port）
            var host = HttpContext.Request.Host.Host;

            // 從設定取得 HTTP port（預設 6011）
            var httpPort = GetHttpPort();
            var certDownloadUrl = $"http://{host}:{httpPort}/api/certificate/cer";

            // 使用純 ASCII 英文，避免 chcp 65001 在 cmd.exe 造成中文字元重複顯示的已知 bug
            var script = $@"@echo off
title ERPCore2 Certificate Installer

:: Request administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    powershell -Command ""Start-Process '%~f0' -Verb RunAs""
    exit /b
)

echo ============================================
echo   ERPCore2 Certificate Installer
echo ============================================
echo.
echo Downloading certificate from server...
echo Server: {certDownloadUrl}
echo.

powershell -Command ""try {{ Invoke-WebRequest -Uri '{certDownloadUrl}' -OutFile '%TEMP%\erpcore2_cert.cer' -TimeoutSec 30; Write-Host '[OK] Download complete' }} catch {{ Write-Host ('[Error] Download failed: ' + $_.Exception.Message); exit 1 }}""

if not exist ""%TEMP%\erpcore2_cert.cer"" (
    echo.
    echo [Error] Certificate download failed!
    echo Please check:
    echo   1. Server is running
    echo   2. Can connect to {certDownloadUrl}
    echo.
    pause
    exit /b 1
)

echo Installing certificate to Trusted Root Certification Authorities...
certutil -addstore -f ""Root"" ""%TEMP%\erpcore2_cert.cer"" >nul 2>&1
set INSTALL_RESULT=%errorLevel%

del ""%TEMP%\erpcore2_cert.cer"" >nul 2>&1

if %INSTALL_RESULT% equ 0 (
    echo.
    echo ============================================
    echo   Certificate installed successfully!
    echo ============================================
    echo.
    echo Next steps:
    echo   1. Close all browser windows
    echo   2. Reopen your browser
    echo   3. Connect to ERP - no more security warnings
    echo.
) else (
    echo.
    echo [Error] Installation failed. Please contact your administrator.
    echo.
)
pause";

            var bytes = Encoding.ASCII.GetBytes(script);
            return File(bytes, "application/octet-stream", "install-cert.bat");
        }

        private int GetHttpPort()
        {
            // 從 urls 設定解析 HTTP port
            var urls = _configuration["urls"] ?? string.Empty;
            foreach (var url in urls.Split(';'))
            {
                var trimmed = url.Trim();
                if (trimmed.StartsWith("http://") && !trimmed.StartsWith("https://"))
                {
                    if (Uri.TryCreate(trimmed.Replace("*", "localhost"), UriKind.Absolute, out var uri))
                        return uri.Port;
                }
            }
            return 6011; // 預設值
        }
    }
}
