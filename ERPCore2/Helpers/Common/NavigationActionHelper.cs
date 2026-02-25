using ERPCore2.Models;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Helpers;

/// <summary>
/// 導航動作輔助類別
/// 統一管理導航選單的 Action 類型項目建立、註冊和執行
/// </summary>
public static class NavigationActionHelper
{
    /// <summary>
    /// 建立 Action 類型的導航項目
    /// </summary>
    /// <param name="name">功能名稱</param>
    /// <param name="description">功能描述</param>
    /// <param name="iconClass">圖示 CSS 類別</param>
    /// <param name="actionId">動作識別碼（用於觸發對應的處理方法）</param>
    /// <param name="category">分類</param>
    /// <param name="requiredPermission">權限要求（可選）</param>
    /// <param name="searchKeywords">搜尋關鍵字（可選）</param>
    /// <param name="quickActionId">快速功能識別碼（可選，設定後此項目支援快速功能）</param>
    /// <param name="quickActionName">快速功能顯示名稱（可選）</param>
    /// <returns>設定好的 Action 類型導航項目</returns>
    public static NavigationItem CreateActionItem(
        string name,
        string description,
        string iconClass,
        string actionId,
        string category = "",
        string? requiredPermission = null,
        List<string>? searchKeywords = null,
        string? quickActionId = null,
        string? quickActionName = null,
        string? nameKey = null)
    {
        return new NavigationItem
        {
            Name = name,
            NameKey = nameKey,
            Description = description,
            IconClass = iconClass,
            ActionId = actionId,
            ItemType = NavigationItemType.Action,
            Route = "", // Action 類型不需要路由
            Category = category,
            RequiredPermission = requiredPermission,
            SearchKeywords = searchKeywords ?? new List<string>(),
            QuickActionId = quickActionId,
            QuickActionName = quickActionName
        };
    }

    /// <summary>
    /// 動作處理器容器
    /// 用於集中管理所有 Action 的處理方法
    /// </summary>
    public class ActionHandlerRegistry
    {
        private readonly Dictionary<string, Action> _handlers = new();
        private readonly ILogger? _logger;

        public ActionHandlerRegistry(ILogger? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// 註冊 Action 處理器
        /// </summary>
        /// <param name="actionId">動作識別碼</param>
        /// <param name="handler">處理方法</param>
        public void Register(string actionId, Action handler)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                throw new ArgumentException("ActionId 不能為空", nameof(actionId));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (_handlers.ContainsKey(actionId))
            {
            }

            _handlers[actionId] = handler;
        }

        /// <summary>
        /// 執行 Action（同步版本）
        /// </summary>
        /// <param name="actionId">動作識別碼</param>
        /// <returns>是否成功執行</returns>
        public bool Execute(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                return false;
            }

            if (!_handlers.TryGetValue(actionId, out var handler))
            {
                return false;
            }

            try
            {
                handler.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得已註冊的 ActionId 清單
        /// </summary>
        public IReadOnlyCollection<string> GetRegisteredActionIds()
        {
            return _handlers.Keys;
        }

        /// <summary>
        /// 檢查是否已註冊指定的 ActionId
        /// </summary>
        public bool IsRegistered(string actionId)
        {
            return _handlers.ContainsKey(actionId);
        }

        /// <summary>
        /// 清除所有註冊的處理器
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
        }
    }

    /// <summary>
    /// 建立 Action 處理器註冊表（簡化版 - 不需要 Logger）
    /// </summary>
    public static ActionHandlerRegistry CreateRegistry()
    {
        return new ActionHandlerRegistry();
    }

    /// <summary>
    /// 建立 Action 處理器註冊表（完整版 - 支援 Logger）
    /// </summary>
    public static ActionHandlerRegistry CreateRegistry(ILogger logger)
    {
        return new ActionHandlerRegistry(logger);
    }
}
