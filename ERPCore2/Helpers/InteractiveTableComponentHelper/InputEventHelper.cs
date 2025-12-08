namespace ERPCore2.Helpers;

/// <summary>
/// 輸入事件處理 Helper - 統一輸入事件處理邏輯
/// 用於 InteractiveTableComponent 的各種輸入欄位處理
/// </summary>
public static class InputEventHelper
{
    /// <summary>
    /// 數量輸入處理
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="defaultValue">預設值（當輸入為空或無效時使用）</param>
    /// <returns>處理後的數量</returns>
    public static decimal HandleQuantityInput(string? value, decimal defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return decimal.TryParse(value, out var quantity) ? quantity : defaultValue;
    }
    
    /// <summary>
    /// 價格輸入處理
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="defaultValue">預設值（當輸入為空或無效時使用）</param>
    /// <returns>處理後的價格</returns>
    public static decimal HandlePriceInput(string? value, decimal defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return decimal.TryParse(value, out var price) ? price : defaultValue;
    }
    
    /// <summary>
    /// 百分比輸入處理（自動限制範圍 0-100）
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="min">最小值（預設 0）</param>
    /// <param name="max">最大值（預設 100）</param>
    /// <param name="defaultValue">預設值（當輸入為空或無效時使用）</param>
    /// <returns>處理後的百分比（自動限制在 min-max 範圍內）</returns>
    public static decimal HandlePercentageInput(
        string? value, 
        decimal min = 0, 
        decimal max = 100, 
        decimal defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        
        if (decimal.TryParse(value, out var percentage))
        {
            // 自動限制在指定範圍內
            return Math.Max(min, Math.Min(max, percentage));
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// 整數輸入處理
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="defaultValue">預設值（當輸入為空或無效時使用）</param>
    /// <returns>處理後的整數</returns>
    public static int HandleIntegerInput(string? value, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return int.TryParse(value, out var integer) ? integer : defaultValue;
    }
    
    /// <summary>
    /// 文字輸入處理
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="defaultValue">預設值（當輸入為 null 時使用）</param>
    /// <returns>處理後的文字</returns>
    public static string HandleTextInput(string? value, string defaultValue = "")
    {
        return value ?? defaultValue;
    }
    
    /// <summary>
    /// 整合版本：處理輸入並自動通知變更
    /// </summary>
    /// <typeparam name="T">輸入值的型別</typeparam>
    /// <param name="value">輸入值</param>
    /// <param name="parser">解析函式（將字串轉換為目標型別）</param>
    /// <param name="onChanged">變更後的回呼函式（可選）</param>
    /// <returns>處理後的值</returns>
    public static async Task<T> HandleInputWithNotificationAsync<T>(
        string? value,
        Func<string?, T> parser,
        Func<Task>? onChanged = null)
    {
        var result = parser(value);
        
        if (onChanged != null)
        {
            await onChanged();
        }
        
        return result;
    }
    
    /// <summary>
    /// 整合版本：處理數量輸入並自動通知變更
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="onChanged">變更後的回呼函式（可選）</param>
    /// <param name="defaultValue">預設值</param>
    /// <returns>處理後的數量</returns>
    public static async Task<decimal> HandleQuantityInputWithNotificationAsync(
        string? value,
        Func<Task>? onChanged = null,
        decimal defaultValue = 0)
    {
        return await HandleInputWithNotificationAsync(
            value,
            v => HandleQuantityInput(v, defaultValue),
            onChanged
        );
    }
    
    /// <summary>
    /// 整合版本：處理價格輸入並自動通知變更
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="onChanged">變更後的回呼函式（可選）</param>
    /// <param name="defaultValue">預設值</param>
    /// <returns>處理後的價格</returns>
    public static async Task<decimal> HandlePriceInputWithNotificationAsync(
        string? value,
        Func<Task>? onChanged = null,
        decimal defaultValue = 0)
    {
        return await HandleInputWithNotificationAsync(
            value,
            v => HandlePriceInput(v, defaultValue),
            onChanged
        );
    }
    
    /// <summary>
    /// 整合版本：處理百分比輸入並自動通知變更
    /// </summary>
    /// <param name="value">輸入值</param>
    /// <param name="onChanged">變更後的回呼函式（可選）</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    /// <param name="defaultValue">預設值</param>
    /// <returns>處理後的百分比</returns>
    public static async Task<decimal> HandlePercentageInputWithNotificationAsync(
        string? value,
        Func<Task>? onChanged = null,
        decimal min = 0,
        decimal max = 100,
        decimal defaultValue = 0)
    {
        return await HandleInputWithNotificationAsync(
            value,
            v => HandlePercentageInput(v, min, max, defaultValue),
            onChanged
        );
    }
}
