using System.Collections.Generic;

namespace ERPCore2.Services
{
    /// <summary>
    /// 導航權限收集服務介面
    /// </summary>
    public interface INavigationPermissionCollector
    {
        /// <summary>
        /// 註冊一個菜單項目的權限
        /// </summary>
        /// <param name="menuKey">菜單識別鍵</param>
        /// <param name="permission">權限名稱</param>
        void RegisterPermission(string menuKey, string permission);

        /// <summary>
        /// 取得指定菜單的所有權限
        /// </summary>
        /// <param name="menuKey">菜單識別鍵</param>
        /// <returns>權限清單</returns>
        string[] GetPermissions(string menuKey);

        /// <summary>
        /// 清除指定菜單的權限記錄
        /// </summary>
        /// <param name="menuKey">菜單識別鍵</param>
        void ClearPermissions(string menuKey);
    }
}
