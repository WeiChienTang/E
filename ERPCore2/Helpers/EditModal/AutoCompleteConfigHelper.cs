namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// AutoComplete 配置輔助工具
/// 提供統一的 AutoComplete 配置生成邏輯，避免在各個編輯 Modal 中重複相同的程式碼
/// 
/// 使用場景：
/// - 配置關聯實體的 AutoComplete 欄位（客戶、廠商、員工等）
/// - 統一管理 Prefillers、Collections、DisplayProperties、ValueProperties
/// - 簡化 AutoComplete 的配置程式碼，從 4 個方法簡化到 Builder 模式
/// 
/// 影響範圍：30+ 個 EditModal
/// 重複度：⭐⭐⭐⭐ (80%)
/// </summary>
public class AutoCompleteConfig
{
    /// <summary>
    /// 預填器字典：定義搜尋詞如何轉換為預填值
    /// </summary>
    public Dictionary<string, Func<string, Dictionary<string, object?>>> Prefillers { get; set; } = new();

    /// <summary>
    /// 資料集合字典：提供 AutoComplete 的資料來源
    /// </summary>
    public Dictionary<string, IEnumerable<object>> Collections { get; set; } = new();

    /// <summary>
    /// 顯示屬性字典：指定 AutoComplete 顯示的欄位名稱
    /// </summary>
    public Dictionary<string, string> DisplayProperties { get; set; } = new();

    /// <summary>
    /// 值屬性字典：指定 AutoComplete 實際儲存的欄位名稱
    /// </summary>
    public Dictionary<string, string> ValueProperties { get; set; } = new();
}

/// <summary>
/// AutoComplete 配置建構器
/// 使用 Fluent API 方式簡化配置程式碼
/// </summary>
/// <typeparam name="TEntity">實體類型（如 SalesOrder、PurchaseOrder）</typeparam>
public class AutoCompleteConfigBuilder<TEntity>
{
    private readonly AutoCompleteConfig _config = new();

    /// <summary>
    /// 新增單一欄位的 AutoComplete 配置
    /// </summary>
    /// <typeparam name="TRelated">關聯實體類型（如 Customer、Employee）</typeparam>
    /// <param name="propertyName">欄位屬性名稱（如 nameof(SalesOrder.CustomerId)）</param>
    /// <param name="displayProperty">顯示屬性名稱（如 "CompanyName", "Name"）</param>
    /// <param name="collection">資料集合（如 availableCustomers）</param>
    /// <param name="valueProperty">值屬性名稱（預設為 "Id"）</param>
    /// <param name="customPrefiller">自訂預填器（如果預設的不符合需求）</param>
    /// <returns>建構器實例（支援鏈式呼叫）</returns>
    /// <example>
    /// <code>
    /// var config = new AutoCompleteConfigBuilder&lt;SalesOrder&gt;()
    ///     .AddField&lt;Customer&gt;(
    ///         nameof(SalesOrder.CustomerId), 
    ///         "CompanyName", 
    ///         availableCustomers)
    ///     .AddField&lt;Employee&gt;(
    ///         nameof(SalesOrder.EmployeeId), 
    ///         "Name", 
    ///         availableEmployees)
    ///     .Build();
    /// </code>
    /// </example>
    public AutoCompleteConfigBuilder<TEntity> AddField<TRelated>(
        string propertyName,
        string displayProperty,
        IEnumerable<TRelated> collection,
        string valueProperty = "Id",
        Func<string, Dictionary<string, object?>>? customPrefiller = null)
    {
        // 預設 prefiller：使用 displayProperty 進行搜尋
        // 例如：searchTerm => new Dictionary<string, object?> { ["CompanyName"] = searchTerm }
        var prefiller = customPrefiller ??
            (searchTerm => new Dictionary<string, object?> { [displayProperty] = searchTerm });

        _config.Prefillers[propertyName] = prefiller;
        _config.Collections[propertyName] = collection.Cast<object>();
        _config.DisplayProperties[propertyName] = displayProperty;
        _config.ValueProperties[propertyName] = valueProperty;

        return this;
    }

    /// <summary>
    /// 新增多個欄位的 AutoComplete 配置（使用相同的設定）
    /// </summary>
    /// <typeparam name="TRelated">關聯實體類型</typeparam>
    /// <param name="displayProperty">顯示屬性名稱</param>
    /// <param name="fieldsConfig">欄位配置陣列（propertyName, collection）</param>
    /// <param name="valueProperty">值屬性名稱（預設為 "Id"）</param>
    /// <returns>建構器實例（支援鏈式呼叫）</returns>
    /// <example>
    /// <code>
    /// var config = new AutoCompleteConfigBuilder&lt;SalesOrder&gt;()
    ///     .AddMultipleFields&lt;Employee&gt;("Name", 
    ///         (nameof(SalesOrder.EmployeeId), availableEmployees),
    ///         (nameof(SalesOrder.ApprovedById), availableEmployees))
    ///     .Build();
    /// </code>
    /// </example>
    public AutoCompleteConfigBuilder<TEntity> AddMultipleFields<TRelated>(
        string displayProperty,
        params (string propertyName, IEnumerable<TRelated> collection)[] fieldsConfig)
    {
        foreach (var (propertyName, collection) in fieldsConfig)
        {
            AddField(propertyName, displayProperty, collection);
        }
        return this;
    }

    /// <summary>
    /// 新增具有複合搜尋條件的欄位
    /// 例如：同時搜尋公司名稱和統一編號
    /// </summary>
    /// <typeparam name="TRelated">關聯實體類型</typeparam>
    /// <param name="propertyName">欄位屬性名稱</param>
    /// <param name="displayProperty">顯示屬性名稱</param>
    /// <param name="collection">資料集合</param>
    /// <param name="searchProperties">搜尋屬性陣列</param>
    /// <param name="valueProperty">值屬性名稱（預設為 "Id"）</param>
    /// <returns>建構器實例（支援鏈式呼叫）</returns>
    /// <example>
    /// <code>
    /// var config = new AutoCompleteConfigBuilder&lt;SalesOrder&gt;()
    ///     .AddFieldWithMultipleSearchProperties&lt;Customer&gt;(
    ///         nameof(SalesOrder.CustomerId),
    ///         "CompanyName",
    ///         availableCustomers,
    ///         new[] { "CompanyName", "TaxNumber" })
    ///     .Build();
    /// </code>
    /// </example>
    public AutoCompleteConfigBuilder<TEntity> AddFieldWithMultipleSearchProperties<TRelated>(
        string propertyName,
        string displayProperty,
        IEnumerable<TRelated> collection,
        string[] searchProperties,
        string valueProperty = "Id")
    {
        var prefiller = (string searchTerm) =>
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in searchProperties)
            {
                dict[prop] = searchTerm;
            }
            return dict;
        };

        return AddField(propertyName, displayProperty, collection, valueProperty, prefiller);
    }

    /// <summary>
    /// 新增具有條件式配置的欄位
    /// </summary>
    /// <typeparam name="TRelated">關聯實體類型</typeparam>
    /// <param name="condition">條件判斷（true 時才新增）</param>
    /// <param name="propertyName">欄位屬性名稱</param>
    /// <param name="displayProperty">顯示屬性名稱</param>
    /// <param name="collection">資料集合</param>
    /// <param name="valueProperty">值屬性名稱（預設為 "Id"）</param>
    /// <returns>建構器實例（支援鏈式呼叫）</returns>
    /// <example>
    /// <code>
    /// var config = new AutoCompleteConfigBuilder&lt;SalesOrder&gt;()
    ///     .AddFieldIf(hasPermission,
    ///         nameof(SalesOrder.ApprovedById),
    ///         "Name",
    ///         availableEmployees)
    ///     .Build();
    /// </code>
    /// </example>
    public AutoCompleteConfigBuilder<TEntity> AddFieldIf<TRelated>(
        bool condition,
        string propertyName,
        string displayProperty,
        IEnumerable<TRelated> collection,
        string valueProperty = "Id",
        Func<string, Dictionary<string, object?>>? customPrefiller = null)
    {
        if (condition)
        {
            AddField(propertyName, displayProperty, collection, valueProperty, customPrefiller);
        }
        return this;
    }

    /// <summary>
    /// 建立 AutoComplete 配置
    /// </summary>
    /// <returns>完整的 AutoComplete 配置物件</returns>
    public AutoCompleteConfig Build() => _config;
}

/// <summary>
/// AutoComplete 配置輔助工具的靜態擴充方法
/// </summary>
public static class AutoCompleteConfigHelper
{
    /// <summary>
    /// 建立標準的 AutoComplete 配置建構器
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <returns>配置建構器實例</returns>
    public static AutoCompleteConfigBuilder<TEntity> CreateBuilder<TEntity>()
    {
        return new AutoCompleteConfigBuilder<TEntity>();
    }

    /// <summary>
    /// 從現有配置複製並建立新的建構器
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="existingConfig">現有配置</param>
    /// <returns>包含現有配置的建構器</returns>
    public static AutoCompleteConfigBuilder<TEntity> CreateBuilderFrom<TEntity>(AutoCompleteConfig existingConfig)
    {
        var builder = new AutoCompleteConfigBuilder<TEntity>();
        
        // 複製現有配置
        foreach (var kvp in existingConfig.Prefillers)
        {
            builder.Build().Prefillers[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in existingConfig.Collections)
        {
            builder.Build().Collections[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in existingConfig.DisplayProperties)
        {
            builder.Build().DisplayProperties[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in existingConfig.ValueProperties)
        {
            builder.Build().ValueProperties[kvp.Key] = kvp.Value;
        }

        return builder;
    }

    /// <summary>
    /// 驗證 AutoComplete 配置的完整性
    /// </summary>
    /// <param name="config">要驗證的配置</param>
    /// <returns>驗證結果（propertyName, errorMessage）</returns>
    public static List<(string PropertyName, string ErrorMessage)> ValidateConfig(AutoCompleteConfig config)
    {
        var errors = new List<(string, string)>();

        // 檢查所有欄位是否都有完整配置
        var allPropertyNames = config.Prefillers.Keys
            .Union(config.Collections.Keys)
            .Union(config.DisplayProperties.Keys)
            .Union(config.ValueProperties.Keys)
            .Distinct();

        foreach (var propertyName in allPropertyNames)
        {
            if (!config.Prefillers.ContainsKey(propertyName))
                errors.Add((propertyName, "缺少 Prefiller 配置"));
            
            if (!config.Collections.ContainsKey(propertyName))
                errors.Add((propertyName, "缺少 Collection 配置"));
            
            if (!config.DisplayProperties.ContainsKey(propertyName))
                errors.Add((propertyName, "缺少 DisplayProperty 配置"));
            
            if (!config.ValueProperties.ContainsKey(propertyName))
                errors.Add((propertyName, "缺少 ValueProperty 配置"));

            // 檢查 Collection 是否為空
            if (config.Collections.ContainsKey(propertyName))
            {
                var collection = config.Collections[propertyName];
                if (collection == null)
                {
                    errors.Add((propertyName, "Collection 為 null"));
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// 合併多個 AutoComplete 配置
    /// </summary>
    /// <param name="configs">要合併的配置陣列</param>
    /// <returns>合併後的配置</returns>
    public static AutoCompleteConfig MergeConfigs(params AutoCompleteConfig[] configs)
    {
        var merged = new AutoCompleteConfig();

        foreach (var config in configs)
        {
            foreach (var kvp in config.Prefillers)
                merged.Prefillers[kvp.Key] = kvp.Value;
            
            foreach (var kvp in config.Collections)
                merged.Collections[kvp.Key] = kvp.Value;
            
            foreach (var kvp in config.DisplayProperties)
                merged.DisplayProperties[kvp.Key] = kvp.Value;
            
            foreach (var kvp in config.ValueProperties)
                merged.ValueProperties[kvp.Key] = kvp.Value;
        }

        return merged;
    }

    /// <summary>
    /// 建立簡單的單一欄位配置（快速建立方法）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TRelated">關聯實體類型</typeparam>
    /// <param name="propertyName">欄位屬性名稱</param>
    /// <param name="displayProperty">顯示屬性名稱</param>
    /// <param name="collection">資料集合</param>
    /// <param name="valueProperty">值屬性名稱（預設為 "Id"）</param>
    /// <returns>AutoComplete 配置</returns>
    public static AutoCompleteConfig CreateSimpleConfig<TEntity, TRelated>(
        string propertyName,
        string displayProperty,
        IEnumerable<TRelated> collection,
        string valueProperty = "Id")
    {
        return new AutoCompleteConfigBuilder<TEntity>()
            .AddField(propertyName, displayProperty, collection, valueProperty)
            .Build();
    }
}
