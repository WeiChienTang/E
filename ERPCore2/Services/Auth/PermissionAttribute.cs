using Microsoft.AspNetCore.Authorization;

namespace ERPCore2.Services.Auth
{
    /// <summary>
    /// 權限授權屬性
    /// </summary>
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission) : base(permission)
        {
        }
    }

    /// <summary>
    /// 模組權限授權屬性
    /// </summary>
    public class RequireModuleAccessAttribute : AuthorizeAttribute
    {
        public RequireModuleAccessAttribute(string module) : base($"Module.{module}")
        {
        }
    }
}
