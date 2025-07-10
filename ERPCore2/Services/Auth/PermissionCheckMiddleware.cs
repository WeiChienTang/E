using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.Services.Auth
{
    /// <summary>
    /// 權限檢查中間件
    /// </summary>
    public class PermissionCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionCheckMiddleware> _logger;

        public PermissionCheckMiddleware(RequestDelegate next, ILogger<PermissionCheckMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 檢查是否為 Blazor 頁面路由
                if (context.Request.Path.StartsWithSegments("/_blazor") || 
                    context.Request.Path.StartsWithSegments("/api") ||
                    context.Request.Path.StartsWithSegments("/auth"))
                {
                    await _next(context);
                    return;
                }

                // 檢查是否需要權限驗證的路由
                var routePermissions = GetRoutePermissions(context.Request.Path);
                if (routePermissions.Any())
                {
                    var user = context.User;
                    if (!user.Identity?.IsAuthenticated ?? true)
                    {
                        context.Response.Redirect("/auth/login");
                        return;
                    }

                    // 檢查權限
                    var employeeIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(employeeIdClaim, out int employeeId))
                    {
                        var serviceProvider = context.RequestServices;
                        var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

                        bool hasPermission = false;
                        foreach (var permission in routePermissions)
                        {
                            var result = await permissionService.HasPermissionAsync(employeeId, permission);
                            if (result.IsSuccess && result.Data)
                            {
                                hasPermission = true;
                                break;
                            }
                        }

                        if (!hasPermission)
                        {
                            context.Response.Redirect("/access-denied");
                            return;
                        }
                    }
                    else
                    {
                        context.Response.Redirect("/auth/login");
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(InvokeAsync), GetType(), _logger, new { 
                    Method = nameof(InvokeAsync),
                    ServiceType = GetType().Name,
                    RequestPath = context.Request.Path.ToString(),
                    EmployeeId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                });
                // 發生錯誤時，重定向到登入頁面
                context.Response.Redirect("/auth/login");
            }
        }

        private List<string> GetRoutePermissions(PathString path)
        {
            var permissions = new List<string>();
            
            // 定義路由權限對應表
            var routePermissionMap = new Dictionary<string, string[]>
            {
                { "/customers", new[] { "Customer.Read" } },
                { "/customers/create", new[] { "Customer.Create" } },
                { "/customers/edit/", new[] { "Customer.Update" } },
                { "/customers/detail/", new[] { "Customer.Read" } },
                
                { "/suppliers", new[] { "Supplier.Read" } },
                { "/suppliers/create", new[] { "Supplier.Create" } },
                { "/suppliers/edit/", new[] { "Supplier.Update" } },
                { "/suppliers/detail/", new[] { "Supplier.Read" } },
                
                { "/employees", new[] { "Employee.Read" } },
                { "/employees/create", new[] { "Employee.Create" } },
                { "/employees/edit/", new[] { "Employee.Update" } },
                { "/employees/detail/", new[] { "Employee.Read" } },
                
                { "/products", new[] { "Product.Read" } },
                { "/products/create", new[] { "Product.Create" } },
                { "/products/edit/", new[] { "Product.Update" } },
                { "/products/detail/", new[] { "Product.Read" } },
                
                { "/permissions", new[] { "System.Admin" } },
                { "/roles", new[] { "System.Admin" } },
                { "/error-logs", new[] { "System.Admin" } }
            };

            var pathString = path.ToString().ToLower();
            
            // 檢查完全匹配
            if (routePermissionMap.ContainsKey(pathString))
            {
                permissions.AddRange(routePermissionMap[pathString]);
            }
            else
            {
                // 檢查部分匹配（例如 /customers/edit/123）
                foreach (var kvp in routePermissionMap)
                {
                    if (kvp.Key.EndsWith("/") && pathString.StartsWith(kvp.Key))
                    {
                        permissions.AddRange(kvp.Value);
                        break;
                    }
                }
            }

            return permissions;
        }
    }
}
