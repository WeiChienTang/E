using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 導航權限收集服務實作
    /// </summary>
    public class NavigationPermissionCollector : INavigationPermissionCollector
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _menuPermissions = new();
        private readonly ILogger<NavigationPermissionCollector> _logger;

        public NavigationPermissionCollector(ILogger<NavigationPermissionCollector> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 註冊一個菜單項目的權限
        /// </summary>
        public void RegisterPermission(string menuKey, string permission)
        {
            if (string.IsNullOrEmpty(menuKey) || string.IsNullOrEmpty(permission))
            {
                _logger.LogWarning("RegisterPermission 被調用但參數無效: menuKey={MenuKey}, permission={Permission}", menuKey, permission);
                return;
            }

            _logger.LogInformation("註冊權限: menuKey={MenuKey}, permission={Permission}", menuKey, permission);

            _menuPermissions.AddOrUpdate(
                menuKey,
                new HashSet<string> { permission },
                (key, existing) =>
                {
                    existing.Add(permission);
                    return existing;
                }
            );

            _logger.LogInformation("權限註冊完成，menuKey={MenuKey} 目前有 {Count} 個權限", menuKey, _menuPermissions[menuKey].Count);
        }

        /// <summary>
        /// 取得指定菜單的所有權限
        /// </summary>
        public string[] GetPermissions(string menuKey)
        {
            if (string.IsNullOrEmpty(menuKey))
            {
                _logger.LogWarning("GetPermissions 被調用但 menuKey 為空");
                return new string[0];
            }

            var result = _menuPermissions.TryGetValue(menuKey, out var permissions)
                ? permissions.ToArray()
                : new string[0];

            _logger.LogInformation("取得權限: menuKey={MenuKey}, 找到 {Count} 個權限: [{Permissions}]", 
                menuKey, result.Length, string.Join(", ", result));

            return result;
        }

        /// <summary>
        /// 清除指定菜單的權限記錄
        /// </summary>
        public void ClearPermissions(string menuKey)
        {
            if (!string.IsNullOrEmpty(menuKey))
            {
                var removed = _menuPermissions.TryRemove(menuKey, out var removedPermissions);
                _logger.LogInformation("清除權限: menuKey={MenuKey}, 是否成功={Success}, 清除了 {Count} 個權限", 
                    menuKey, removed, removedPermissions?.Count ?? 0);
            }
        }
    }
}
