using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;
using ERPCore2.Helpers;

namespace ERPCore2.Services.Auth
{    /// <summary>
    /// 自定義認證狀態提供者，確保 Blazor Server 組件能正確讀取 Cookie 認證狀態
    /// </summary>
    public class CustomRevalidatingServerAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IServiceScopeFactory? _scopeFactory;

        // 完整建構子
        public CustomRevalidatingServerAuthenticationStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory)
            : base(loggerFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // 簡易建構子
        public CustomRevalidatingServerAuthenticationStateProvider(
            ILogger<CustomRevalidatingServerAuthenticationStateProvider> logger)
            : base(LoggerFactory.Create(builder => builder.AddConsole()))
        {
            _scopeFactory = null;
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            try
            {
                // 如果用戶未認證，返回 false
                if (!authenticationState.User.Identity?.IsAuthenticated ?? true)
                    return false;

                // 獲取用戶 ID
                var userIdClaim = authenticationState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return false;

                // 如果 _scopeFactory 為 null，無法進行驗證
                if (_scopeFactory == null)
                    return false;

                using var scope = _scopeFactory.CreateScope();
                var employeeService = scope.ServiceProvider.GetRequiredService<ERPCore2.Services.IEmployeeService>();
                
                // 驗證員工是否仍然存在且未被刪除或鎖定
                var employee = await employeeService.GetByIdAsync(userId);
                if (employee == null)
                    return false;

                return !employee.IsDeleted && !employee.IsLocked;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAuthenticationStateAsync), typeof(CustomRevalidatingServerAuthenticationStateProvider));
                return false;
            }
        }
    }
}
