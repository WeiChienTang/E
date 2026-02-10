using ERPCore2.Models.Enums;

namespace ERPCore2.Helpers;

/// <summary>
/// 計算相關 Helper - 統一所有金額計算邏輯
/// 用於 InteractiveTableComponent 的各種金額計算
/// </summary>
public static class CalculationHelper
{
    /// <summary>
    /// 計算小計（支援多種稅率算法和折扣）
    /// </summary>
    /// <param name="quantity">數量</param>
    /// <param name="unitPrice">單價</param>
    /// <param name="discountPercentage">折扣百分比 (0-100)</param>
    /// <param name="taxRate">稅率百分比 (0-100)</param>
    /// <param name="taxMethod">稅率計算方法</param>
    /// <returns>計算後的小計金額</returns>
    public static decimal CalculateSubtotal(
        decimal quantity, 
        decimal unitPrice, 
        decimal discountPercentage = 0,
        decimal taxRate = 0,
        TaxCalculationMethod taxMethod = TaxCalculationMethod.TaxExclusive)
    {
        // 先計算折扣後金額
        var afterDiscount = quantity * unitPrice * (1 - discountPercentage / 100);
        
        return taxMethod switch
        {
            // 稅外加：小計 = 折扣後金額 × (1 + 稅率/100)
            TaxCalculationMethod.TaxExclusive => afterDiscount * (1 + taxRate / 100),
            
            // 稅內含：小計 = 折扣後金額（稅已包含在單價中）
            TaxCalculationMethod.TaxInclusive => afterDiscount,
            
            // 免稅：小計 = 折扣後金額
            TaxCalculationMethod.NoTax => afterDiscount,
            
            _ => afterDiscount
        };
    }
    
    /// <summary>
    /// 計算稅額
    /// </summary>
    /// <param name="subtotal">小計金額</param>
    /// <param name="taxRate">稅率百分比 (0-100)</param>
    /// <param name="taxMethod">稅率計算方法</param>
    /// <returns>稅額</returns>
    public static decimal CalculateTaxAmount(
        decimal subtotal, 
        decimal taxRate,
        TaxCalculationMethod taxMethod)
    {
        return taxMethod switch
        {
            // 稅外加：稅額 = 小計 × 稅率 / 100
            TaxCalculationMethod.TaxExclusive => subtotal * taxRate / 100,
            
            // 稅內含：稅額 = 小計 × 稅率 / (100 + 稅率)
            TaxCalculationMethod.TaxInclusive => subtotal * taxRate / (100 + taxRate),
            
            // 免稅：稅額 = 0
            TaxCalculationMethod.NoTax => 0,
            
            _ => 0
        };
    }
    
    /// <summary>
    /// 計算總計（多筆明細）
    /// </summary>
    /// <typeparam name="TItem">明細項目類型</typeparam>
    /// <param name="items">明細項目集合</param>
    /// <param name="subtotalSelector">小計選擇器（從 item 取得小計金額的函式）</param>
    /// <returns>總計金額</returns>
    public static decimal CalculateTotal<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, decimal> subtotalSelector)
    {
        return items.Sum(subtotalSelector);
    }
    
    /// <summary>
    /// 單位換算（數量轉換）
    /// </summary>
    /// <param name="quantity">原始數量</param>
    /// <param name="conversionRate">換算率（例如：1箱 = 12個，則換算率為 12）</param>
    /// <returns>換算後的數量</returns>
    public static decimal ConvertQuantity(decimal quantity, decimal conversionRate)
    {
        return quantity * conversionRate;
    }
    
    /// <summary>
    /// 反向單位換算（從換算後數量計算原始數量）
    /// </summary>
    /// <param name="convertedQuantity">換算後的數量</param>
    /// <param name="conversionRate">換算率</param>
    /// <returns>原始數量</returns>
    public static decimal ReverseConvertQuantity(decimal convertedQuantity, decimal conversionRate)
    {
        if (conversionRate == 0) return 0;
        return convertedQuantity / conversionRate;
    }
    
    /// <summary>
    /// 計算折扣金額
    /// </summary>
    /// <param name="originalAmount">原始金額</param>
    /// <param name="discountPercentage">折扣百分比 (0-100)</param>
    /// <returns>折扣金額</returns>
    public static decimal CalculateDiscountAmount(decimal originalAmount, decimal discountPercentage)
    {
        return originalAmount * discountPercentage / 100;
    }
    
    /// <summary>
    /// 計算折扣後金額
    /// </summary>
    /// <param name="originalAmount">原始金額</param>
    /// <param name="discountPercentage">折扣百分比 (0-100)</param>
    /// <returns>折扣後金額</returns>
    public static decimal CalculateAfterDiscount(decimal originalAmount, decimal discountPercentage)
    {
        return originalAmount * (1 - discountPercentage / 100);
    }
    
    /// <summary>
    /// 四捨五入到指定小數位數
    /// </summary>
    /// <param name="value">原始值</param>
    /// <param name="decimals">小數位數（預設 2 位）</param>
    /// <returns>四捨五入後的值</returns>
    public static decimal Round(decimal value, int decimals = 2)
    {
        return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
    }
}
