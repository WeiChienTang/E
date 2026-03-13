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
        // 以 ConcurrentDictionary<string, byte> 模擬 concurrent set，避免 HashSet 在
        // ConcurrentDictionary.AddOrUpdate updateValueFactory 中被多執行緒同時寫入而損毀。
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _menuPermissions = new();
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
                return;

            var set = _menuPermissions.GetOrAdd(menuKey, _ => new ConcurrentDictionary<string, byte>());
            set.TryAdd(permission, 0);
        }

        /// <summary>
        /// 取得指定菜單的所有權限
        /// </summary>
        public string[] GetPermissions(string menuKey)
        {
            if (string.IsNullOrEmpty(menuKey))
                return Array.Empty<string>();

            return _menuPermissions.TryGetValue(menuKey, out var permissions)
                ? permissions.Keys.ToArray()
                : Array.Empty<string>();
        }

        /// <summary>
        /// 清除指定菜單的權限記錄
        /// </summary>
        public void ClearPermissions(string menuKey)
        {
            if (!string.IsNullOrEmpty(menuKey))
                _menuPermissions.TryRemove(menuKey, out _);
        }
    }
}
