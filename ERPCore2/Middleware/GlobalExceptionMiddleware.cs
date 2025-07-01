using ERPCore2.Services;
using System.Net;
using System.Text.Json;

namespace ERPCore2.Middleware
{
    /// <summary>
    /// 全域例外處理中間件
    /// 捕獲所有未處理的例外，記錄到錯誤日誌，並提供適當的回應
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發生未處理的例外");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string errorId = string.Empty;
            
            try
            {
                // 記錄錯誤到資料庫
                using var scope = _serviceScopeFactory.CreateScope();
                var errorLogService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();
                
                var additionalData = new
                {
                    RequestPath = context.Request.Path.Value,
                    QueryString = context.Request.QueryString.Value,
                    Method = context.Request.Method,
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserId = context.User?.Identity?.Name,
                    Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    RequestTime = DateTime.UtcNow
                };

                errorId = await errorLogService.LogErrorAsync(exception, additionalData);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "記錄錯誤時發生例外");
                errorId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            }

            // 避免重複設定回應
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("回應已開始，無法設定錯誤回應");
                return;
            }

            // 設定回應
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            // 根據請求類型決定回應格式
            if (IsAjaxRequest(context) || IsApiRequest(context))
            {
                // API 請求返回 JSON
                var response = new
                {
                    error = true,
                    errorId = errorId,
                    message = "系統發生錯誤，請稍後再試",
                    details = GetUserFriendlyMessage(exception)
                };

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                // 網頁請求重導向到錯誤頁面
                context.Response.StatusCode = (int)HttpStatusCode.Redirect;
                context.Response.Headers.Location = $"/error/{errorId}";
            }
        }

        private bool IsAjaxRequest(HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api") ||
                   context.Request.Headers["Accept"].ToString().Contains("application/json");
        }

        private string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                TimeoutException => "系統回應逾時，請稍後再試",
                UnauthorizedAccessException => "權限不足，請聯繫管理員",
                ArgumentException => "輸入參數有誤",
                InvalidOperationException => "操作無效，請檢查資料狀態",
                _ => "系統發生錯誤，請聯繫技術人員"
            };
        }
    }
}
