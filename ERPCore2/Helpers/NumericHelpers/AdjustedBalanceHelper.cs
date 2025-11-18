namespace ERPCore2.Helpers.NumericHelpers;

/// <summary>
/// 動態餘額計算輔助類 - 處理編輯模式下的餘額調整邏輯
/// 適用於需要「從總額中扣除使用量，編輯時動態調整可用餘額」的業務場景
/// </summary>
/// <remarks>
/// 核心概念：
/// - 資料庫中的餘額是固定的（已扣除原有的使用金額）
/// - 編輯時，需要把「原有的使用金額」加回去，再扣除「新的使用金額」
/// - 這樣使用者看到的「可編輯空間」就是正確的
/// 
/// 使用場景：
/// - 沖款明細編輯（本次沖款、本次折讓）
/// - 預收付款項使用編輯
/// - 庫存分配調整
/// - 其他類似的「從總額中扣除使用量」的業務場景
/// </remarks>
public static class AdjustedBalanceHelper
{
    /// <summary>
    /// 計算調整後的可用餘額（編輯模式）
    /// </summary>
    /// <param name="baseBalance">資料庫中的原始餘額（已扣除原有使用金額）</param>
    /// <param name="originalUsedAmount">原有的已使用金額（資料庫中保存的值）</param>
    /// <param name="currentUsedAmount">目前輸入的使用金額（使用者當前編輯的值）</param>
    /// <param name="isEditMode">是否為編輯模式</param>
    /// <returns>調整後的可用餘額</returns>
    /// <example>
    /// 範例：
    /// - 原始總額：1000
    /// - 原有使用金額：20
    /// - 資料庫餘額：980 (1000 - 20)
    /// - 目前輸入：0
    /// 
    /// 計算：980 + 20 - 0 = 1000（使用者可以重新分配的最大金額）
    /// </example>
    public static decimal CalculateAdjustedBalance(
        decimal baseBalance,
        decimal originalUsedAmount,
        decimal currentUsedAmount,
        bool isEditMode)
    {
        if (!isEditMode)
        {
            // 新增模式：可用餘額 = 原始餘額 - 目前使用金額
            return baseBalance - currentUsedAmount;
        }
        
        // 編輯模式：可用餘額 = 原始餘額 + 原有使用金額 - 目前使用金額
        return baseBalance + originalUsedAmount - currentUsedAmount;
    }
    
    /// <summary>
    /// 驗證輸入金額是否超過可用餘額（考慮編輯模式的動態調整）
    /// </summary>
    /// <param name="inputAmount">使用者輸入的金額</param>
    /// <param name="baseBalance">資料庫中的原始餘額</param>
    /// <param name="originalUsedAmount">原有的已使用金額</param>
    /// <param name="isEditMode">是否為編輯模式</param>
    /// <returns>true 表示金額有效（不超過可用餘額），false 表示超過</returns>
    public static bool ValidateAmount(
        decimal inputAmount,
        decimal baseBalance,
        decimal originalUsedAmount,
        bool isEditMode)
    {
        var maxAvailable = CalculateAdjustedBalance(
            baseBalance, 
            originalUsedAmount, 
            0, // 當前使用金額設為0，計算最大可用金額
            isEditMode);
        
        return inputAmount <= maxAvailable;
    }
    
    /// <summary>
    /// 取得調整後的可用餘額（用於顯示）
    /// 此方法考慮了其他欄位的已使用金額
    /// </summary>
    /// <param name="baseBalance">資料庫中的原始餘額</param>
    /// <param name="originalTotalUsedAmount">原有的總使用金額（包含所有欄位）</param>
    /// <param name="otherFieldsUsedAmount">其他欄位目前的使用金額（不包含當前欄位）</param>
    /// <param name="isEditMode">是否為編輯模式</param>
    /// <returns>調整後的可用餘額（扣除其他欄位使用金額後的餘額）</returns>
    /// <example>
    /// 範例（沖款明細有「本次沖款」和「本次折讓」兩個欄位）：
    /// - 資料庫餘額：980
    /// - 原有本次沖款：15
    /// - 原有本次折讓：5
    /// - 當前本次折讓：3
    /// 
    /// 計算「本次沖款」的可用餘額：
    /// baseBalance = 980
    /// originalTotalUsedAmount = 15 + 5 = 20
    /// otherFieldsUsedAmount = 3（當前本次折讓）
    /// 
    /// 結果：980 + 20 - 3 = 997（「本次沖款」可以填入的最大金額）
    /// </example>
    public static decimal GetDisplayBalance(
        decimal baseBalance,
        decimal originalTotalUsedAmount,
        decimal otherFieldsUsedAmount,
        bool isEditMode)
    {
        if (!isEditMode)
            return baseBalance - otherFieldsUsedAmount;
        
        // 編輯模式：餘額 = 原始餘額 + 原有總使用金額 - 其他欄位的使用金額
        return baseBalance + originalTotalUsedAmount - otherFieldsUsedAmount;
    }
    
    /// <summary>
    /// 取得最大可填入金額（用於雙擊快速填入）
    /// </summary>
    /// <param name="baseBalance">資料庫中的原始餘額</param>
    /// <param name="originalTotalUsedAmount">原有的總使用金額（包含所有欄位）</param>
    /// <param name="otherFieldsUsedAmount">其他欄位目前的使用金額（不包含當前欄位）</param>
    /// <param name="isEditMode">是否為編輯模式</param>
    /// <returns>當前欄位可以填入的最大金額</returns>
    public static decimal GetMaxFillableAmount(
        decimal baseBalance,
        decimal originalTotalUsedAmount,
        decimal otherFieldsUsedAmount,
        bool isEditMode)
    {
        var available = GetDisplayBalance(baseBalance, originalTotalUsedAmount, otherFieldsUsedAmount, isEditMode);
        return Math.Max(0, available); // 確保不會返回負數
    }
}
