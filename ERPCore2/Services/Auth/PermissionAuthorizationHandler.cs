using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.Services.Auth
{
    /// <summary>
    /// 權限授權處理器
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(IPermissionService permissionService, ILogger<PermissionAuthorizationHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            try
            {
                // 取得使用者的員工ID
                var employeeIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
                {
                    _logger.LogWarning("無法解析員工ID，權限檢查失敗");
                    context.Fail();
                    return;
                }

                // 檢查權限
                var hasPermission = await _permissionService.HasPermissionAsync(employeeId, requirement.Permission);
                
                if (hasPermission.IsSuccess && hasPermission.Data)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning($"員工 {employeeId} 沒有權限 {requirement.Permission}");
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(HandleRequirementAsync), GetType(), _logger, new { 
                    Method = nameof(HandleRequirementAsync),
                    ServiceType = GetType().Name,
                    Permission = requirement.Permission,
                    EmployeeId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                });
                context.Fail();
            }
        }
    }

    /// <summary>
    /// 權限需求
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
