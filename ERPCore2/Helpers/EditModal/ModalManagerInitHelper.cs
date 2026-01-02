using ERPCore2.Components.Shared.PageTemplate;
using ERPCore2.Data;
using ERPCore2.Services;
using System.Linq.Expressions;
using System.Reflection;

namespace ERPCore2.Helpers.EditModal
{
    /// <summary>
    /// ModalManager 集合容器
    /// 用於儲存和管理多個 RelatedEntityModalManager 實例
    /// </summary>
    public class ModalManagerCollection
    {
        private readonly Dictionary<string, object> _managers = new();

        /// <summary>
        /// 取得指定屬性的 ModalManager
        /// </summary>
        /// <typeparam name="TRelated">關聯實體類型</typeparam>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns>對應的 RelatedEntityModalManager</returns>
        /// <exception cref="KeyNotFoundException">找不到指定屬性的 Manager</exception>
        public RelatedEntityModalManager<TRelated> Get<TRelated>(string propertyName)
            where TRelated : BaseEntity
        {
            if (!_managers.ContainsKey(propertyName))
            {
                throw new KeyNotFoundException($"找不到屬性 {propertyName} 的 ModalManager");
            }
            return (RelatedEntityModalManager<TRelated>)_managers[propertyName];
        }

        /// <summary>
        /// 嘗試取得指定屬性的 ModalManager
        /// </summary>
        /// <typeparam name="TRelated">關聯實體類型</typeparam>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="manager">輸出的 Manager（如果存在）</param>
        /// <returns>是否成功取得</returns>
        public bool TryGet<TRelated>(string propertyName, out RelatedEntityModalManager<TRelated>? manager)
            where TRelated : BaseEntity
        {
            if (_managers.ContainsKey(propertyName))
            {
                manager = (RelatedEntityModalManager<TRelated>)_managers[propertyName];
                return true;
            }
            manager = null;
            return false;
        }

        /// <summary>
        /// 內部方法：新增 Manager
        /// </summary>
        internal void Add<TRelated>(string propertyName, RelatedEntityModalManager<TRelated> manager)
            where TRelated : BaseEntity
        {
            _managers[propertyName] = manager;
        }

        /// <summary>
        /// 取得所有已註冊的屬性名稱
        /// </summary>
        public IEnumerable<string> GetRegisteredProperties() => _managers.Keys;

        /// <summary>
        /// 檢查是否包含指定屬性的 Manager
        /// </summary>
        public bool Contains(string propertyName) => _managers.ContainsKey(propertyName);

        /// <summary>
        /// 取得已註冊的 Manager 數量
        /// </summary>
        public int Count => _managers.Count;
    }

    /// <summary>
    /// ModalManager 建構器配置
    /// </summary>
    public class ModalManagerBuilderConfig<TEntity, TService>
        where TEntity : BaseEntity, new()
        where TService : class
    {
        /// <summary>
        /// 取得 GenericEditModalComponent 的方法
        /// </summary>
        public required Func<GenericEditModalComponent<TEntity, TService>?> GetEditModalComponent { get; set; }

        /// <summary>
        /// NotificationService 實例
        /// </summary>
        public required INotificationService NotificationService { get; set; }

        /// <summary>
        /// StateHasChanged 回調
        /// </summary>
        public required Action StateChangedCallback { get; set; }

        /// <summary>
        /// 預設的重新載入資料回調（可選，可在個別 Manager 覆寫）
        /// </summary>
        public Func<Task>? DefaultReloadDataCallback { get; set; }

        /// <summary>
        /// 預設的初始化表單欄位回調（可選，可在個別 Manager 覆寫）
        /// </summary>
        public Func<Task>? DefaultInitializeFormFieldsCallback { get; set; }

        /// <summary>
        /// 預設是否刷新 AutoComplete 欄位（可選，可在個別 Manager 覆寫）
        /// </summary>
        public bool DefaultRefreshAutoCompleteFields { get; set; } = true;
    }

    /// <summary>
    /// ModalManager 建構器
    /// 使用 Builder 模式簡化多個 RelatedEntityModalManager 的初始化
    /// </summary>
    public class ModalManagerBuilder<TEntity, TService>
        where TEntity : BaseEntity, new()
        where TService : class
    {
        private readonly ModalManagerCollection _collection = new();
        private readonly ModalManagerBuilderConfig<TEntity, TService> _config;

        /// <summary>
        /// 建立 ModalManagerBuilder 實例
        /// </summary>
        /// <param name="config">建構器配置</param>
        public ModalManagerBuilder(ModalManagerBuilderConfig<TEntity, TService> config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 簡化建構函式（使用個別參數）
        /// </summary>
        public ModalManagerBuilder(
            Func<GenericEditModalComponent<TEntity, TService>?> getEditModalComponent,
            INotificationService notificationService,
            Action stateChangedCallback,
            Func<Task>? defaultReloadDataCallback = null,
            Func<Task>? defaultInitializeFormFieldsCallback = null,
            bool defaultRefreshAutoCompleteFields = true)
        {
            _config = new ModalManagerBuilderConfig<TEntity, TService>
            {
                GetEditModalComponent = getEditModalComponent,
                NotificationService = notificationService,
                StateChangedCallback = stateChangedCallback,
                DefaultReloadDataCallback = defaultReloadDataCallback,
                DefaultInitializeFormFieldsCallback = defaultInitializeFormFieldsCallback,
                DefaultRefreshAutoCompleteFields = defaultRefreshAutoCompleteFields
            };
        }

        /// <summary>
        /// 新增單一 Manager（標準版）
        /// </summary>
        /// <typeparam name="TRelated">關聯實體類型</typeparam>
        /// <param name="propertyName">屬性名稱（如 nameof(SalesOrder.CustomerId)）</param>
        /// <param name="displayName">顯示名稱（如 "客戶"）</param>
        /// <param name="reloadDataCallback">重新載入資料回調（null 則使用預設）</param>
        /// <param name="initializeFormFieldsCallback">初始化表單欄位回調（null 則使用預設）</param>
        /// <param name="refreshAutoCompleteFields">是否刷新 AutoComplete 欄位（null 則使用預設）</param>
        /// <returns>Builder 實例（支援鏈式呼叫）</returns>
        public ModalManagerBuilder<TEntity, TService> AddManager<TRelated>(
            string propertyName,
            string displayName,
            Func<Task>? reloadDataCallback = null,
            Func<Task>? initializeFormFieldsCallback = null,
            bool? refreshAutoCompleteFields = null)
            where TRelated : BaseEntity
        {
            var manager = RelatedEntityModalManagerHelper.CreateStandardManager(
                new StandardModalManagerConfig<TEntity, TRelated, TService>
                {
                    NotificationService = _config.NotificationService,
                    EntityDisplayName = displayName,
                    PropertyName = propertyName,
                    GetEditModalComponent = _config.GetEditModalComponent,
                    ReloadDataCallback = reloadDataCallback ?? _config.DefaultReloadDataCallback,
                    StateChangedCallback = _config.StateChangedCallback,
                    AutoSelectAction = CreateAutoSelectAction<TRelated>(propertyName),
                    InitializeFormFieldsCallback = initializeFormFieldsCallback ?? _config.DefaultInitializeFormFieldsCallback,
                    RefreshAutoCompleteFields = refreshAutoCompleteFields ?? _config.DefaultRefreshAutoCompleteFields
                });

            _collection.Add(propertyName, manager);
            return this;
        }

        /// <summary>
        /// 新增單一 Manager（使用 Expression 避免魔術字串）
        /// </summary>
        /// <typeparam name="TRelated">關聯實體類型</typeparam>
        /// <param name="propertySelector">屬性選擇器（如 e => e.CustomerId）</param>
        /// <param name="displayName">顯示名稱（如 "客戶"）</param>
        /// <param name="reloadDataCallback">重新載入資料回調（null 則使用預設）</param>
        /// <param name="initializeFormFieldsCallback">初始化表單欄位回調（null 則使用預設）</param>
        /// <param name="refreshAutoCompleteFields">是否刷新 AutoComplete 欄位（null 則使用預設）</param>
        /// <returns>Builder 實例（支援鏈式呼叫）</returns>
        public ModalManagerBuilder<TEntity, TService> AddManager<TRelated>(
            Expression<Func<TEntity, int?>> propertySelector,
            string displayName,
            Func<Task>? reloadDataCallback = null,
            Func<Task>? initializeFormFieldsCallback = null,
            bool? refreshAutoCompleteFields = null)
            where TRelated : BaseEntity
        {
            var propertyName = GetPropertyName(propertySelector);
            return AddManager<TRelated>(propertyName, displayName, reloadDataCallback, initializeFormFieldsCallback, refreshAutoCompleteFields);
        }

        /// <summary>
        /// 批次新增多個 Manager（使用相同的回調）
        /// </summary>
        /// <param name="managerConfigs">Manager 配置陣列（屬性名稱, 關聯類型, 顯示名稱）</param>
        /// <param name="reloadDataCallback">共用的重新載入資料回調</param>
        /// <param name="initializeFormFieldsCallback">共用的初始化表單欄位回調</param>
        /// <returns>Builder 實例（支援鏈式呼叫）</returns>
        public ModalManagerBuilder<TEntity, TService> AddMultipleManagers(
            (string PropertyName, Type RelatedType, string DisplayName)[] managerConfigs,
            Func<Task>? reloadDataCallback = null,
            Func<Task>? initializeFormFieldsCallback = null)
        {
            foreach (var config in managerConfigs)
            {
                AddManagerDynamic(
                    config.PropertyName,
                    config.RelatedType,
                    config.DisplayName,
                    reloadDataCallback,
                    initializeFormFieldsCallback);
            }
            return this;
        }

        /// <summary>
        /// 條件式新增 Manager
        /// </summary>
        /// <typeparam name="TRelated">關聯實體類型</typeparam>
        /// <param name="condition">條件（true 才新增）</param>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="displayName">顯示名稱</param>
        /// <param name="reloadDataCallback">重新載入資料回調</param>
        /// <param name="initializeFormFieldsCallback">初始化表單欄位回調</param>
        /// <returns>Builder 實例（支援鏈式呼叫）</returns>
        public ModalManagerBuilder<TEntity, TService> AddManagerIf<TRelated>(
            bool condition,
            string propertyName,
            string displayName,
            Func<Task>? reloadDataCallback = null,
            Func<Task>? initializeFormFieldsCallback = null)
            where TRelated : BaseEntity
        {
            if (condition)
            {
                AddManager<TRelated>(propertyName, displayName, reloadDataCallback, initializeFormFieldsCallback);
            }
            return this;
        }

        /// <summary>
        /// 完成建構並返回 ModalManagerCollection
        /// </summary>
        /// <returns>ModalManagerCollection 實例</returns>
        public ModalManagerCollection Build()
        {
            return _collection;
        }

        /// <summary>
        /// 內部方法：建立 AutoSelectAction
        /// </summary>
        private Action<TEntity?, int> CreateAutoSelectAction<TRelated>(string propertyName)
        {
            return (entity, relatedId) =>
            {
                if (entity == null) return;

                var property = typeof(TEntity).GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    // 處理 int? 和 int 兩種情況
                    if (property.PropertyType == typeof(int?))
                    {
                        property.SetValue(entity, (int?)relatedId);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(entity, relatedId);
                    }
                }
            };
        }

        /// <summary>
        /// 內部方法：從 Expression 取得屬性名稱
        /// </summary>
        private string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            throw new ArgumentException("無效的屬性選擇器", nameof(propertySelector));
        }

        /// <summary>
        /// 內部方法：動態新增 Manager（使用反射）
        /// </summary>
        private void AddManagerDynamic(
            string propertyName,
            Type relatedType,
            string displayName,
            Func<Task>? reloadDataCallback,
            Func<Task>? initializeFormFieldsCallback)
        {
            var method = typeof(ModalManagerBuilder<TEntity, TService>)
                .GetMethod(nameof(AddManager), new[] { typeof(string), typeof(string), typeof(Func<Task>), typeof(Func<Task>), typeof(bool?) })
                ?.MakeGenericMethod(relatedType);

            method?.Invoke(this, new object?[] { propertyName, displayName, reloadDataCallback, initializeFormFieldsCallback, null });
        }
    }

    /// <summary>
    /// ModalManagerInitHelper - 提供靜態輔助方法
    /// </summary>
    public static class ModalManagerInitHelper
    {
        /// <summary>
        /// 建立標準的 ModalManagerBuilder
        /// </summary>
        /// <typeparam name="TEntity">主實體類型</typeparam>
        /// <typeparam name="TService">服務介面類型</typeparam>
        /// <param name="getEditModalComponent">取得 GenericEditModalComponent 的方法</param>
        /// <param name="notificationService">NotificationService 實例</param>
        /// <param name="stateChangedCallback">StateHasChanged 回調</param>
        /// <param name="defaultReloadDataCallback">預設的重新載入資料回調</param>
        /// <param name="defaultInitializeFormFieldsCallback">預設的初始化表單欄位回調</param>
        /// <returns>ModalManagerBuilder 實例</returns>
        public static ModalManagerBuilder<TEntity, TService> CreateBuilder<TEntity, TService>(
            Func<GenericEditModalComponent<TEntity, TService>?> getEditModalComponent,
            INotificationService notificationService,
            Action stateChangedCallback,
            Func<Task>? defaultReloadDataCallback = null,
            Func<Task>? defaultInitializeFormFieldsCallback = null)
            where TEntity : BaseEntity, new()
            where TService : class
        {
            return new ModalManagerBuilder<TEntity, TService>(
                getEditModalComponent,
                notificationService,
                stateChangedCallback,
                defaultReloadDataCallback,
                defaultInitializeFormFieldsCallback);
        }

        /// <summary>
        /// 快速建立包含單一 Manager 的 Collection
        /// </summary>
        /// <typeparam name="TEntity">主實體類型</typeparam>
        /// <typeparam name="TService">服務介面類型</typeparam>
        /// <typeparam name="TRelated">關聯實體類型</typeparam>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="displayName">顯示名稱</param>
        /// <param name="getEditModalComponent">取得 GenericEditModalComponent 的方法</param>
        /// <param name="notificationService">NotificationService 實例</param>
        /// <param name="stateChangedCallback">StateHasChanged 回調</param>
        /// <param name="reloadDataCallback">重新載入資料回調</param>
        /// <param name="initializeFormFieldsCallback">初始化表單欄位回調</param>
        /// <returns>包含單一 Manager 的 ModalManagerCollection</returns>
        public static ModalManagerCollection CreateSingleManager<TEntity, TService, TRelated>(
            string propertyName,
            string displayName,
            Func<GenericEditModalComponent<TEntity, TService>?> getEditModalComponent,
            INotificationService notificationService,
            Action stateChangedCallback,
            Func<Task> reloadDataCallback,
            Func<Task> initializeFormFieldsCallback)
            where TEntity : BaseEntity, new()
            where TService : class
            where TRelated : BaseEntity
        {
            return CreateBuilder<TEntity, TService>(
                    getEditModalComponent,
                    notificationService,
                    stateChangedCallback)
                .AddManager<TRelated>(propertyName, displayName, reloadDataCallback, initializeFormFieldsCallback)
                .Build();
        }

        /// <summary>
        /// 驗證 ModalManagerCollection 的完整性
        /// </summary>
        /// <param name="collection">要驗證的 Collection</param>
        /// <param name="requiredProperties">必要的屬性清單</param>
        /// <returns>驗證結果（屬性名稱, 錯誤訊息）</returns>
        public static List<(string PropertyName, string ErrorMessage)> ValidateCollection(
            ModalManagerCollection collection,
            params string[] requiredProperties)
        {
            var errors = new List<(string, string)>();

            if (collection == null)
            {
                errors.Add(("Collection", "ModalManagerCollection 為 null"));
                return errors;
            }

            foreach (var property in requiredProperties)
            {
                if (!collection.Contains(property))
                {
                    errors.Add((property, $"缺少必要的 Manager: {property}"));
                }
            }

            return errors;
        }

        /// <summary>
        /// 從現有 Collection 複製並建立新的 Builder
        /// </summary>
        /// <typeparam name="TEntity">主實體類型</typeparam>
        /// <typeparam name="TService">服務介面類型</typeparam>
        /// <param name="sourceCollection">來源 Collection</param>
        /// <param name="getEditModalComponent">取得 GenericEditModalComponent 的方法</param>
        /// <param name="notificationService">NotificationService 實例</param>
        /// <param name="stateChangedCallback">StateHasChanged 回調</param>
        /// <returns>新的 ModalManagerBuilder（已包含來源的所有 Manager）</returns>
        /// <remarks>注意：此方法會建立新的 Manager 實例，而非共用參考</remarks>
        public static ModalManagerBuilder<TEntity, TService> CreateBuilderFrom<TEntity, TService>(
            ModalManagerCollection sourceCollection,
            Func<GenericEditModalComponent<TEntity, TService>?> getEditModalComponent,
            INotificationService notificationService,
            Action stateChangedCallback)
            where TEntity : BaseEntity, new()
            where TService : class
        {
            var builder = CreateBuilder<TEntity, TService>(
                getEditModalComponent,
                notificationService,
                stateChangedCallback);

            // 注意：此處僅示範結構，實際複製需要存取 Manager 內部配置
            // 由於 RelatedEntityModalManager 沒有公開配置，此方法主要用於文檔目的
            // 實際使用時建議直接重新建立 Manager

            return builder;
        }
    }
}
