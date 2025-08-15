using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPCore2.Controllers
{
    public class AuthController : Controller
    {
        private readonly Services.IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(Services.IAuthenticationService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("/auth/signin")]
        public async Task<IActionResult> SignIn(string account, string password, bool rememberMe = false, string? returnUrl = null)
        {
            // 檢查 Request.Form 中的值來確認表單資料
            if (Request.Form.ContainsKey("rememberMe"))
            {
                var formValues = Request.Form["rememberMe"];
                
                // 如果表單中包含 "true" 值，強制設定 rememberMe 為 true
                if (formValues.Contains("true"))
                {
                    rememberMe = true;
                }
            }
            
            try
            {
                // 驗證用戶
                var result = await _authService.LoginAsync(account, password);
                
                if (!result.IsSuccess)
                {
                    // 登入失敗，重新導向回登入頁面並帶上錯誤訊息
                    var loginUrl = "/auth/login";
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
                    }
                    loginUrl += (loginUrl.Contains('?') ? "&" : "?") + "error=invalid";
                    return Redirect(loginUrl);
                }

                var employee = result.Data!;

                // 建立 Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                    new Claim(ClaimTypes.Name, employee.Account ?? ""),
                    new Claim(ClaimTypes.GivenName, employee.FirstName ?? ""),
                    new Claim(ClaimTypes.Surname, employee.LastName ?? ""),
                    new Claim(ClaimTypes.Role, employee.Role?.Name ?? "User"),
                    new Claim("EmployeeCode", employee.Code ?? ""),
                    new Claim("Department", employee.Department?.Name ?? ""),
                    new Claim("Position", employee.EmployeePosition?.Name ?? "")
                };

                // 建立認證身份
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe 
                        ? DateTimeOffset.UtcNow.AddDays(30) 
                        : DateTimeOffset.UtcNow.AddHours(8),
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                // 執行登入
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 更新最後登入時間
                await _authService.UpdateLastLoginAsync(employee.Id);

                // 導向目標頁面
                return LocalRedirect(returnUrl ?? "/");
            }
            catch (Exception)
            {
                // 發生錯誤，重新導向回登入頁面
                var loginUrl = "/auth/login";
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
                }
                loginUrl += (loginUrl.Contains('?') ? "&" : "?") + "error=system";
                return Redirect(loginUrl);
            }
        }

        [HttpPost("/auth/signout")]
        [HttpGet("/auth/signout")]
        public async Task<IActionResult> SignOut(string? returnUrl = null)
        {
            // 執行登出
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // 導向登入頁面
            return Redirect("/auth/login");
        }
    }
}