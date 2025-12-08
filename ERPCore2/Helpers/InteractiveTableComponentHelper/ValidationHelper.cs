namespace ERPCore2.Helpers;

/// <summary>
/// 驗證 Helper - 統一驗證和檢查邏輯
/// 用於 InteractiveTableComponent 的各種驗證場景
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// 綜合檢查是否可刪除（支援多種檢查條件）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">要檢查的項目</param>
    /// <param name="reason">無法刪除的原因（輸出參數）</param>
    /// <param name="checkDelivery">檢查出貨記錄的函式（可選）</param>
    /// <param name="checkReturn">檢查退貨記錄的函式（可選）</param>
    /// <param name="checkPayment">檢查沖款記錄的函式（可選）</param>
    /// <param name="checkReceiving">檢查進貨記錄的函式（可選）</param>
    /// <param name="additionalChecks">額外的檢查函式清單（可選）</param>
    /// <returns>是否可以刪除</returns>
    public static bool CanDeleteItem<TItem>(
        TItem item,
        out string reason,
        Func<TItem, bool>? checkDelivery = null,
        Func<TItem, bool>? checkReturn = null,
        Func<TItem, bool>? checkPayment = null,
        Func<TItem, bool>? checkReceiving = null,
        List<Func<TItem, (bool hasIssue, string reason)>>? additionalChecks = null)
    {
        // 檢查出貨記錄
        if (checkDelivery != null && checkDelivery(item))
        {
            reason = "此項目已有出貨記錄，無法刪除";
            return false;
        }
        
        // 檢查退貨記錄
        if (checkReturn != null && checkReturn(item))
        {
            reason = "此項目已有退貨記錄，無法刪除";
            return false;
        }
        
        // 檢查沖款記錄
        if (checkPayment != null && checkPayment(item))
        {
            reason = "此項目已有沖款記錄，無法刪除";
            return false;
        }
        
        // 檢查進貨記錄
        if (checkReceiving != null && checkReceiving(item))
        {
            reason = "此項目已有進貨記錄，無法刪除";
            return false;
        }
        
        // 額外檢查
        if (additionalChecks != null)
        {
            foreach (var check in additionalChecks)
            {
                var (hasIssue, issueReason) = check(item);
                if (hasIssue)
                {
                    reason = issueReason;
                    return false;
                }
            }
        }
        
        reason = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 檢查重複項目
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <typeparam name="TKey">鍵值類型</typeparam>
    /// <param name="items">所有項目集合</param>
    /// <param name="currentItem">當前項目</param>
    /// <param name="keySelector">鍵值選擇器（用於判斷重複的欄位）</param>
    /// <param name="duplicateItem">重複的項目（輸出參數）</param>
    /// <returns>是否有重複</returns>
    public static bool HasDuplicate<TItem, TKey>(
        List<TItem> items,
        TItem currentItem,
        Func<TItem, TKey> keySelector,
        out TItem? duplicateItem)
    {
        var currentKey = keySelector(currentItem);
        
        duplicateItem = items.FirstOrDefault(i => 
            !EqualityComparer<TItem>.Default.Equals(i, currentItem) &&
            EqualityComparer<TKey>.Default.Equals(keySelector(i), currentKey));
        
        return duplicateItem != null;
    }
    
    /// <summary>
    /// 檢查重複項目（多個鍵值）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="items">所有項目集合</param>
    /// <param name="currentItem">當前項目</param>
    /// <param name="keySelector">鍵值選擇器（返回複合鍵值）</param>
    /// <param name="duplicateItem">重複的項目（輸出參數）</param>
    /// <returns>是否有重複</returns>
    public static bool HasDuplicateCompositeKey<TItem>(
        List<TItem> items,
        TItem currentItem,
        Func<TItem, object> keySelector,
        out TItem? duplicateItem)
    {
        var currentKey = keySelector(currentItem);
        
        duplicateItem = items.FirstOrDefault(i => 
            !EqualityComparer<TItem>.Default.Equals(i, currentItem) &&
            Equals(keySelector(i), currentKey));
        
        return duplicateItem != null;
    }
    
    /// <summary>
    /// 數量驗證（不可超過最大數量）
    /// </summary>
    /// <param name="quantity">要驗證的數量</param>
    /// <param name="maxQuantity">最大數量（可選）</param>
    /// <param name="error">錯誤訊息（輸出參數）</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="allowZero">是否允許 0（預設 false）</param>
    /// <returns>是否驗證通過</returns>
    public static bool ValidateQuantity(
        decimal quantity,
        decimal? maxQuantity,
        out string error,
        string fieldName = "數量",
        bool allowZero = false)
    {
        if (!allowZero && quantity <= 0)
        {
            error = $"{fieldName}必須大於 0";
            return false;
        }
        
        if (allowZero && quantity < 0)
        {
            error = $"{fieldName}不可小於 0";
            return false;
        }
        
        if (maxQuantity.HasValue && quantity > maxQuantity.Value)
        {
            error = $"{fieldName}不可超過 {maxQuantity.Value}";
            return false;
        }
        
        error = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 價格驗證（必須大於等於 0）
    /// </summary>
    /// <param name="price">要驗證的價格</param>
    /// <param name="error">錯誤訊息（輸出參數）</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="allowZero">是否允許 0（預設 true）</param>
    /// <returns>是否驗證通過</returns>
    public static bool ValidatePrice(
        decimal price,
        out string error,
        string fieldName = "價格",
        bool allowZero = true)
    {
        if (!allowZero && price <= 0)
        {
            error = $"{fieldName}必須大於 0";
            return false;
        }
        
        if (allowZero && price < 0)
        {
            error = $"{fieldName}不可小於 0";
            return false;
        }
        
        error = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 百分比驗證（範圍 0-100）
    /// </summary>
    /// <param name="percentage">要驗證的百分比</param>
    /// <param name="error">錯誤訊息（輸出參數）</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="min">最小值（預設 0）</param>
    /// <param name="max">最大值（預設 100）</param>
    /// <returns>是否驗證通過</returns>
    public static bool ValidatePercentage(
        decimal percentage,
        out string error,
        string fieldName = "百分比",
        decimal min = 0,
        decimal max = 100)
    {
        if (percentage < min)
        {
            error = $"{fieldName}不可小於 {min}";
            return false;
        }
        
        if (percentage > max)
        {
            error = $"{fieldName}不可大於 {max}";
            return false;
        }
        
        error = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 必填欄位驗證
    /// </summary>
    /// <param name="value">要驗證的值</param>
    /// <param name="error">錯誤訊息（輸出參數）</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <returns>是否驗證通過</returns>
    public static bool ValidateRequired(
        string? value,
        out string error,
        string fieldName = "此欄位")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            error = $"{fieldName}為必填";
            return false;
        }
        
        error = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 必選欄位驗證（物件不可為 null）
    /// </summary>
    /// <typeparam name="T">物件類型</typeparam>
    /// <param name="value">要驗證的值</param>
    /// <param name="error">錯誤訊息（輸出參數）</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <returns>是否驗證通過</returns>
    public static bool ValidateRequiredObject<T>(
        T? value,
        out string error,
        string fieldName = "此欄位")
        where T : class
    {
        if (value == null)
        {
            error = $"{fieldName}為必選";
            return false;
        }
        
        error = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 日期範圍驗證
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <param name="error">錯誤訊息（輸出參數）</param>
    /// <param name="allowSameDate">是否允許同一天（預設 true）</param>
    /// <returns>是否驗證通過</returns>
    public static bool ValidateDateRange(
        DateTime? startDate,
        DateTime? endDate,
        out string error,
        bool allowSameDate = true)
    {
        if (startDate == null || endDate == null)
        {
            error = "開始日期和結束日期皆為必填";
            return false;
        }
        
        if (allowSameDate)
        {
            if (startDate.Value.Date > endDate.Value.Date)
            {
                error = "開始日期不可晚於結束日期";
                return false;
            }
        }
        else
        {
            if (startDate.Value.Date >= endDate.Value.Date)
            {
                error = "開始日期必須早於結束日期";
                return false;
            }
        }
        
        error = string.Empty;
        return true;
    }
    
    /// <summary>
    /// 批次驗證（驗證多個項目）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="items">要驗證的項目集合</param>
    /// <param name="validator">驗證函式（返回是否通過和錯誤訊息）</param>
    /// <param name="errors">錯誤訊息集合（輸出參數）</param>
    /// <returns>是否全部驗證通過</returns>
    public static bool ValidateBatch<TItem>(
        List<TItem> items,
        Func<TItem, (bool isValid, string error)> validator,
        out List<string> errors)
    {
        errors = new List<string>();
        
        for (int i = 0; i < items.Count; i++)
        {
            var (isValid, error) = validator(items[i]);
            if (!isValid)
            {
                errors.Add($"第 {i + 1} 項：{error}");
            }
        }
        
        return !errors.Any();
    }
}
