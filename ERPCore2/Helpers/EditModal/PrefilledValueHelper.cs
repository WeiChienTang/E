using System.Reflection;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 預填值處理輔助工具
/// 提供統一的預填值處理邏輯，避免在各個編輯 Modal 中重複相同的程式碼
/// 
/// 使用場景：
/// - 從父組件傳入預填值到新增的實體
/// - 轉單時預填來源單據資訊
/// - 快速新增時預設特定欄位值
/// </summary>
public static class PrefilledValueHelper
{
    /// <summary>
    /// 將預填值字典套用到實體物件
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="entity">目標實體物件</param>
    /// <param name="prefilledValues">預填值字典（欄位名稱 → 值）</param>
    /// <param name="ignoreErrors">是否忽略轉換錯誤（預設 true）</param>
    /// <returns>成功套用的欄位數量</returns>
    public static int ApplyPrefilledValues<TEntity>(
        TEntity entity,
        Dictionary<string, object?>? prefilledValues,
        bool ignoreErrors = true)
        where TEntity : class
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (prefilledValues == null || !prefilledValues.Any())
            return 0;

        int successCount = 0;

        foreach (var kvp in prefilledValues)
        {
            try
            {
                if (SetPropertyValue(entity, kvp.Key, kvp.Value))
                {
                    successCount++;
                }
            }
            catch (Exception)
            {
                if (!ignoreErrors)
                    throw;
                // 忽略轉換失敗的值
            }
        }

        return successCount;
    }

    /// <summary>
    /// 設定單一屬性值（帶類型轉換）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="entity">目標實體物件</param>
    /// <param name="propertyName">屬性名稱</param>
    /// <param name="value">要設定的值</param>
    /// <returns>是否成功設定</returns>
    public static bool SetPropertyValue<TEntity>(
        TEntity entity,
        string propertyName,
        object? value)
        where TEntity : class
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        var property = typeof(TEntity).GetProperty(propertyName);
        if (property == null || !property.CanWrite)
            return false;

        // 如果值為 null，直接設定 null
        if (value == null)
        {
            // 檢查屬性是否可為 null
            var propertyType = property.PropertyType;
            if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
            {
                // 不可為 null 的值類型，跳過
                return false;
            }

            property.SetValue(entity, null);
            return true;
        }

        // 取得目標類型（處理 Nullable<T>）
        var targetType = Nullable.GetUnderlyingType(property.PropertyType)
            ?? property.PropertyType;

        // 如果類型已經相符，直接設定
        if (value.GetType() == targetType || targetType.IsAssignableFrom(value.GetType()))
        {
            property.SetValue(entity, value);
            return true;
        }

        // 嘗試類型轉換
        try
        {
            var convertedValue = Convert.ChangeType(value, targetType);
            property.SetValue(entity, convertedValue);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 建立預填值字典（Builder 模式）
    /// </summary>
    public class PrefilledValueBuilder
    {
        private readonly Dictionary<string, object?> _values = new();

        /// <summary>
        /// 新增一個預填值
        /// </summary>
        public PrefilledValueBuilder Add(string propertyName, object? value)
        {
            _values[propertyName] = value;
            return this;
        }

        /// <summary>
        /// 條件式新增（只在條件為 true 時新增）
        /// </summary>
        public PrefilledValueBuilder AddIf(bool condition, string propertyName, object? value)
        {
            if (condition)
            {
                _values[propertyName] = value;
            }
            return this;
        }

        /// <summary>
        /// 只在值不為 null 時新增
        /// </summary>
        public PrefilledValueBuilder AddIfNotNull(string propertyName, object? value)
        {
            if (value != null)
            {
                _values[propertyName] = value;
            }
            return this;
        }

        /// <summary>
        /// 合併另一個預填值字典
        /// </summary>
        public PrefilledValueBuilder Merge(Dictionary<string, object?>? other)
        {
            if (other != null)
            {
                foreach (var kvp in other)
                {
                    _values[kvp.Key] = kvp.Value;
                }
            }
            return this;
        }

        /// <summary>
        /// 建立預填值字典
        /// </summary>
        public Dictionary<string, object?> Build() => _values;
    }

    /// <summary>
    /// 驗證預填值是否可套用到指定實體類型
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="prefilledValues">預填值字典</param>
    /// <returns>驗證結果（屬性名稱 → 是否可套用）</returns>
    public static Dictionary<string, bool> ValidatePrefilledValues<TEntity>(
        Dictionary<string, object?>? prefilledValues)
        where TEntity : class
    {
        var results = new Dictionary<string, bool>();

        if (prefilledValues == null || !prefilledValues.Any())
            return results;

        foreach (var kvp in prefilledValues)
        {
            var property = typeof(TEntity).GetProperty(kvp.Key);
            results[kvp.Key] = property != null && property.CanWrite;
        }

        return results;
    }

    /// <summary>
    /// 取得實體中所有可以被預填的屬性名稱
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="excludeReadOnly">是否排除唯讀屬性（預設 true）</param>
    /// <returns>可預填的屬性名稱清單</returns>
    public static List<string> GetPrefillabledProperties<TEntity>(bool excludeReadOnly = true)
        where TEntity : class
    {
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (excludeReadOnly)
        {
            return properties
                .Where(p => p.CanWrite)
                .Select(p => p.Name)
                .ToList();
        }

        return properties.Select(p => p.Name).ToList();
    }

    /// <summary>
    /// 從實體中提取指定屬性的值，建立預填值字典
    /// 適用於複製/轉單等場景
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="sourceEntity">來源實體</param>
    /// <param name="propertyNames">要提取的屬性名稱清單</param>
    /// <returns>預填值字典</returns>
    public static Dictionary<string, object?> ExtractValues<TEntity>(
        TEntity sourceEntity,
        params string[] propertyNames)
        where TEntity : class
    {
        if (sourceEntity == null)
            throw new ArgumentNullException(nameof(sourceEntity));

        var values = new Dictionary<string, object?>();

        foreach (var propertyName in propertyNames)
        {
            var property = typeof(TEntity).GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                values[propertyName] = property.GetValue(sourceEntity);
            }
        }

        return values;
    }

    /// <summary>
    /// 從實體中提取所有公開屬性的值
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="sourceEntity">來源實體</param>
    /// <param name="excludeProperties">要排除的屬性名稱</param>
    /// <returns>預填值字典</returns>
    public static Dictionary<string, object?> ExtractAllValues<TEntity>(
        TEntity sourceEntity,
        params string[] excludeProperties)
        where TEntity : class
    {
        if (sourceEntity == null)
            throw new ArgumentNullException(nameof(sourceEntity));

        var values = new Dictionary<string, object?>();
        var excludeSet = new HashSet<string>(excludeProperties);

        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.CanRead && !excludeSet.Contains(property.Name))
            {
                values[property.Name] = property.GetValue(sourceEntity);
            }
        }

        return values;
    }

    /// <summary>
    /// 複製實體（淺複製），可選擇性覆寫部分屬性
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="sourceEntity">來源實體</param>
    /// <param name="overrideValues">要覆寫的屬性值</param>
    /// <param name="excludeProperties">複製時要排除的屬性</param>
    /// <returns>新的實體實例</returns>
    public static TEntity? CloneWithOverride<TEntity>(
        TEntity sourceEntity,
        Dictionary<string, object?>? overrideValues = null,
        params string[] excludeProperties)
        where TEntity : class, new()
    {
        if (sourceEntity == null)
            return null;

        var newEntity = new TEntity();
        var excludeSet = new HashSet<string>(excludeProperties);

        // 複製所有屬性
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite && !excludeSet.Contains(property.Name))
            {
                var value = property.GetValue(sourceEntity);
                property.SetValue(newEntity, value);
            }
        }

        // 套用覆寫值
        if (overrideValues != null && overrideValues.Any())
        {
            ApplyPrefilledValues(newEntity, overrideValues);
        }

        return newEntity;
    }

    /// <summary>
    /// 比較兩個預填值字典的差異
    /// </summary>
    /// <param name="current">當前預填值</param>
    /// <param name="previous">先前預填值</param>
    /// <returns>差異報告（屬性名稱 → (舊值, 新值)）</returns>
    public static Dictionary<string, (object? OldValue, object? NewValue)> ComparePrefilledValues(
        Dictionary<string, object?>? current,
        Dictionary<string, object?>? previous)
    {
        var differences = new Dictionary<string, (object?, object?)>();

        if (current == null && previous == null)
            return differences;

        var allKeys = new HashSet<string>();
        if (current != null) allKeys.UnionWith(current.Keys);
        if (previous != null) allKeys.UnionWith(previous.Keys);

        foreach (var key in allKeys)
        {
            var currentValue = current?.GetValueOrDefault(key);
            var previousValue = previous?.GetValueOrDefault(key);

            if (!Equals(currentValue, previousValue))
            {
                differences[key] = (previousValue, currentValue);
            }
        }

        return differences;
    }
}
