using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using AuthService = ERPCore2.Services.IAuthenticationService;

namespace ERPCore2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("🔐 API 登入請求: Username={Username}", request.Username);

                // 驗證輸入
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { error = "請輸入使用者名稱和密碼" });
                }

                // 調用驗證服務
                var result = await _authService.LoginAsync(request.Username.Trim(), request.Password.Trim());
                
                if (!result.IsSuccess || result.Data == null)
                {
                    _logger.LogWarning("登入失敗: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new { error = result.ErrorMessage ?? "登入失敗，請檢查您的使用者名稱和密碼" });
                }

                var employee = result.Data;
                _logger.LogInformation("✅ 員工驗證成功: Id={Id}, Username={Username}", employee.Id, employee.Username);                // 建立聲明
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                    new Claim(ClaimTypes.Name, employee.Username),
                    new Claim(ClaimTypes.GivenName, employee.FirstName ?? ""),
                    new Claim(ClaimTypes.Surname, employee.LastName ?? ""),
                    new Claim(ClaimTypes.Email, employee.Email ?? ""),
                    new Claim("EmployeeCode", employee.EmployeeCode ?? ""),
                    new Claim("Department", employee.Department ?? ""),
                    new Claim("Position", employee.Position ?? "")
                };

                _logger.LogInformation("🔍 建立的基本 Claims:");
                foreach (var claim in claims)
                {
                    _logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
                }

                // 加入角色聲明
                if (employee.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, employee.Role.RoleName));
                    _logger.LogInformation("✅ 添加角色聲明: {RoleName}", employee.Role.RoleName);
                }
                else
                {
                    _logger.LogWarning("⚠️ 員工沒有角色資料");
                }                // 建立身份
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = request.RememberMe,
                    ExpiresUtc = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                _logger.LogInformation("🍪 準備設定 Cookie 認證, IsPersistent: {IsPersistent}", request.RememberMe);

                // 執行登入
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("✅ Cookie 認證設定完成，登入成功");

                // 返回成功響應
                return Ok(new { 
                    success = true, 
                    redirectUrl = !string.IsNullOrEmpty(request.ReturnUrl) ? request.ReturnUrl : "/" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入過程中發生錯誤");
                return StatusCode(500, new { error = "登入時發生錯誤" });
            }        }

        [HttpGet("signin")]
        public async Task<IActionResult> SignIn([FromQuery] string claims, [FromQuery] string remember, [FromQuery] string returnUrl)
        {
            try
            {
                _logger.LogInformation("🔐 執行登入Cookie設定");

                // 解析Claims資料
                var claimsData = Uri.UnescapeDataString(claims);
                var claimsList = new List<Claim>();
                
                foreach (var claimData in claimsData.Split(';'))
                {
                    if (string.IsNullOrEmpty(claimData)) continue;
                    
                    var parts = claimData.Split('|', 2);
                    if (parts.Length == 2)
                    {
                        claimsList.Add(new Claim(parts[0], parts[1]));
                    }
                }

                // 建立身份
                var claimsIdentity = new ClaimsIdentity(claimsList, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = remember == "true",
                    ExpiresUtc = remember == "true" ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                // 執行登入
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("✅ Cookie 認證設定完成");

                // 重定向到目標頁面
                var decodedReturnUrl = Uri.UnescapeDataString(returnUrl ?? "/");
                return Redirect(decodedReturnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定認證Cookie時發生錯誤");
                return Redirect("/auth/login?error=登入時發生錯誤");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出過程中發生錯誤");
                return StatusCode(500, new { error = "登出時發生錯誤" });
            }
        }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}
