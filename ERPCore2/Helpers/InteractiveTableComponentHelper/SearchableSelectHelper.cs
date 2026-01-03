using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers;

/// <summary>
/// SearchableSelect Helper - 統一商品搜尋和選擇邏輯
/// 用於 InteractiveTableComponent 的 SearchableSelect 欄位處理
/// </summary>
public static class SearchableSelectHelper
{
    /// <summary>
    /// 處理商品搜尋輸入（通用版本）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">當前項目</param>
    /// <param name="searchValue">搜尋值</param>
    /// <param name="availableProducts">可選擇的商品清單</param>
    /// <param name="setSearchValue">設定搜尋值的動作</param>
    /// <param name="setFilteredProducts">設定過濾後商品的動作</param>
    /// <param name="setShowDropdown">設定是否顯示下拉選單的動作</param>
    /// <param name="setSelectedIndex">設定選中索引的動作</param>
    /// <param name="onStateChanged">狀態變更回呼（可選，通常是 StateHasChanged）</param>
    /// <param name="maxDisplayItems">最大顯示項目數（預設 20）</param>
    public static void HandleProductSearch<TItem>(
        TItem item,
        string? searchValue,
        List<Product> availableProducts,
        Action<TItem, string> setSearchValue,
        Action<TItem, List<Product>> setFilteredProducts,
        Action<TItem, bool> setShowDropdown,
        Action<TItem, int> setSelectedIndex,
        Action? onStateChanged = null,
        int maxDisplayItems = 20)
    {
        // 更新搜尋值
        setSearchValue(item, searchValue ?? string.Empty);
        
        // 過濾商品清單
        var filtered = string.IsNullOrWhiteSpace(searchValue)
            ? availableProducts.Take(maxDisplayItems).ToList()
            : availableProducts
                .Where(p => 
                    (p.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(maxDisplayItems)
                .ToList();
        
        setFilteredProducts(item, filtered);
        setShowDropdown(item, true);
        setSelectedIndex(item, -1);
        onStateChanged?.Invoke();
    }
    
    /// <summary>
    /// 處理商品選擇（含稅率自動帶入）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">當前項目</param>
    /// <param name="selectedProduct">選中的商品</param>
    /// <param name="setSelectedProduct">設定選中商品的動作</param>
    /// <param name="setSearchValue">設定搜尋值的動作</param>
    /// <param name="setTaxRate">設定稅率的動作</param>
    /// <param name="setShowDropdown">設定是否顯示下拉選單的動作</param>
    /// <param name="systemParameterService">系統參數服務（用於取得預設稅率）</param>
    /// <param name="onChanged">變更後的回呼函式（可選）</param>
    /// <returns>是否成功選擇</returns>
    public static async Task<bool> HandleProductSelectionAsync<TItem>(
        TItem item,
        Product? selectedProduct,
        Action<TItem, Product?> setSelectedProduct,
        Action<TItem, string> setSearchValue,
        Action<TItem, decimal> setTaxRate,
        Action<TItem, bool> setShowDropdown,
        ISystemParameterService systemParameterService,
        Func<Task>? onChanged = null)
    {
        if (selectedProduct != null)
        {
            setSelectedProduct(item, selectedProduct);
            setSearchValue(item, FormatProductDisplayText(selectedProduct));
            
            // 自動帶入稅率
            var taxRate = selectedProduct.TaxRate ?? await systemParameterService.GetTaxRateAsync();
            setTaxRate(item, taxRate);
        }
        else
        {
            setSelectedProduct(item, null);
            setSearchValue(item, string.Empty);
        }
        
        setShowDropdown(item, false);
        
        if (onChanged != null)
        {
            await onChanged();
        }
        
        return true;
    }
    
    /// <summary>
    /// 處理商品選擇（簡化版，不帶入稅率）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">當前項目</param>
    /// <param name="selectedProduct">選中的商品</param>
    /// <param name="setSelectedProduct">設定選中商品的動作</param>
    /// <param name="setSearchValue">設定搜尋值的動作</param>
    /// <param name="setShowDropdown">設定是否顯示下拉選單的動作</param>
    /// <param name="onChanged">變更後的回呼函式（可選）</param>
    /// <returns>是否成功選擇</returns>
    public static async Task<bool> HandleProductSelectionSimpleAsync<TItem>(
        TItem item,
        Product? selectedProduct,
        Action<TItem, Product?> setSelectedProduct,
        Action<TItem, string> setSearchValue,
        Action<TItem, bool> setShowDropdown,
        Func<Task>? onChanged = null)
    {
        if (selectedProduct != null)
        {
            setSelectedProduct(item, selectedProduct);
            setSearchValue(item, FormatProductDisplayText(selectedProduct));
        }
        else
        {
            setSelectedProduct(item, null);
            setSearchValue(item, string.Empty);
        }
        
        setShowDropdown(item, false);
        
        if (onChanged != null)
        {
            await onChanged();
        }
        
        return true;
    }
    
    /// <summary>
    /// 格式化商品顯示文字
    /// </summary>
    /// <param name="product">商品</param>
    /// <returns>格式化後的文字（格式：[編號] 名稱）</returns>
    public static string FormatProductDisplayText(Product? product)
    {
        if (product == null) return string.Empty;
        
        // 同時有編號和名稱：[編號] 名稱
        if (!string.IsNullOrEmpty(product.Code) && !string.IsNullOrEmpty(product.Name))
        {
            return $"[{product.Code}] {product.Name}";
        }
        
        // 只有編號：[編號]
        if (!string.IsNullOrEmpty(product.Code))
        {
            return $"[{product.Code}]";
        }
        
        // 只有名稱：名稱
        return product.Name ?? string.Empty;
    }
    
    /// <summary>
    /// 處理焦點事件（顯示下拉選單）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">當前項目</param>
    /// <param name="searchValue">當前搜尋值</param>
    /// <param name="availableProducts">可選擇的商品清單</param>
    /// <param name="setFilteredProducts">設定過濾後商品的動作</param>
    /// <param name="setShowDropdown">設定是否顯示下拉選單的動作</param>
    /// <param name="maxDisplayItems">最大顯示項目數（預設 20）</param>
    public static void HandleProductFocus<TItem>(
        TItem item,
        string? searchValue,
        List<Product> availableProducts,
        Action<TItem, List<Product>> setFilteredProducts,
        Action<TItem, bool> setShowDropdown,
        int maxDisplayItems = 20)
    {
        var filtered = string.IsNullOrWhiteSpace(searchValue)
            ? availableProducts.Take(maxDisplayItems).ToList()
            : availableProducts
                .Where(p => 
                    (p.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(maxDisplayItems)
                .ToList();
        
        setFilteredProducts(item, filtered);
        setShowDropdown(item, true);
    }
    
    /// <summary>
    /// 處理失焦事件（延遲隱藏下拉選單）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">當前項目</param>
    /// <param name="setShowDropdown">設定是否顯示下拉選單的動作</param>
    /// <param name="delayMs">延遲時間（毫秒，預設 200ms，避免點擊項目時下拉選單先關閉）</param>
    public static async Task HandleProductBlurAsync<TItem>(
        TItem item,
        Action<TItem, bool> setShowDropdown,
        int delayMs = 200)
    {
        // 延遲關閉，避免點擊下拉選單項目時選單先關閉
        await Task.Delay(delayMs);
        setShowDropdown(item, false);
    }
    
    /// <summary>
    /// 鍵盤導航處理（上下鍵選擇項目）
    /// </summary>
    /// <typeparam name="TItem">項目類型</typeparam>
    /// <param name="item">當前項目</param>
    /// <param name="key">按下的按鍵</param>
    /// <param name="currentIndex">當前選中索引</param>
    /// <param name="filteredProducts">過濾後的商品清單</param>
    /// <param name="setSelectedIndex">設定選中索引的動作</param>
    /// <param name="onEnter">按下 Enter 時的回呼函式（可選）</param>
    /// <returns>是否處理了按鍵事件</returns>
    public static bool HandleKeyboardNavigation<TItem>(
        TItem item,
        string key,
        int currentIndex,
        List<Product> filteredProducts,
        Action<TItem, int> setSelectedIndex,
        Func<Task>? onEnter = null)
    {
        switch (key)
        {
            case "ArrowDown":
                if (currentIndex < filteredProducts.Count - 1)
                {
                    setSelectedIndex(item, currentIndex + 1);
                }
                return true;
            
            case "ArrowUp":
                if (currentIndex > 0)
                {
                    setSelectedIndex(item, currentIndex - 1);
                }
                return true;
            
            case "Enter":
                if (currentIndex >= 0 && currentIndex < filteredProducts.Count)
                {
                    onEnter?.Invoke();
                }
                return true;
            
            default:
                return false;
        }
    }
}
