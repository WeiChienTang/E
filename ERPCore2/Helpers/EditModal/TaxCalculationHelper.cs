using ERPCore2.Models.Enums;
using ERPCore2.Services;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 稅額計算輔助工具
/// 提供統一的稅額計算邏輯，避免在各個編輯 Modal 中重複相同的程式碼
/// 
/// 使用場景：
/// - 採購單、進貨單、進貨退出的稅額計算
/// - 報價單、銷貨訂單、銷貨退回的稅額計算
/// - 其他需要計算稅額的單據
/// 
/// 標準流程：
/// 1. 在 LoadAdditionalDataAsync 中使用 LoadTaxRateAsync 載入稅率
/// 2. 在 HandleDetailsChanged 中使用 CalculateTax 計算稅額
/// 3. 含稅總額通常由實體的計算屬性自動處理
/// </summary>
public static class TaxCalculationHelper
{
    /// <summary>
    /// 預設稅率（當無法從系統參數載入時使用）
    /// </summary>
    public const decimal DefaultTaxRate = 5.0m;

    #region 稅率載入方法

    /// <summary>
    /// 從系統參數服務載入當前稅率
    /// </summary>
    /// <param name="systemParameterService">系統參數服務</param>
    /// <param name="defaultRate">載入失敗時使用的預設稅率（預設為 5%）</param>
    /// <returns>稅率（百分比，例如 5.0 代表 5%）</returns>
    /// <remarks>
    /// 使用範例：
    /// <code>
    /// private decimal currentTaxRate = 5.0m;
    /// 
    /// protected override async Task LoadAdditionalDataAsync()
    /// {
    ///     currentTaxRate = await TaxCalculationHelper.LoadTaxRateAsync(SystemParameterService);
    /// }
    /// </code>
    /// </remarks>
    public static async Task<decimal> LoadTaxRateAsync(
        ISystemParameterService systemParameterService,
        decimal defaultRate = DefaultTaxRate)
    {
        try
        {
            return await systemParameterService.GetTaxRateAsync();
        }
        catch (Exception)
        {
            // 如果載入失敗，使用預設值
            return defaultRate;
        }
    }

    /// <summary>
    /// 同步載入稅率（適用於已有快取的場景）
    /// </summary>
    /// <param name="systemParameterService">系統參數服務</param>
    /// <param name="defaultRate">載入失敗時使用的預設稅率</param>
    /// <returns>稅率（百分比）</returns>
    public static decimal LoadTaxRate(
        ISystemParameterService systemParameterService,
        decimal defaultRate = DefaultTaxRate)
    {
        try
        {
            return systemParameterService.GetTaxRateAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return defaultRate;
        }
    }

    #endregion

    #region 稅額計算方法

    /// <summary>
    /// 根據總金額和稅率計算稅額
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxRate">稅率（百分比，例如 5.0 代表 5%）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>計算後的稅額（四捨五入到指定小數位數）</returns>
    /// <remarks>
    /// 計算公式：稅額 = 總金額 × (稅率 / 100)
    /// 
    /// 使用範例：
    /// <code>
    /// entity.TaxAmount = TaxCalculationHelper.CalculateTax(entity.TotalAmount, currentTaxRate);
    /// </code>
    /// </remarks>
    public static decimal CalculateTax(decimal totalAmount, decimal taxRate, int decimals = 2)
    {
        return Math.Round(totalAmount * (taxRate / 100), decimals);
    }

    /// <summary>
    /// 計算含稅總額
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxRate">稅率（百分比）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>含稅總額</returns>
    /// <remarks>
    /// 計算公式：含稅總額 = 總金額 + 稅額 = 總金額 × (1 + 稅率/100)
    /// 
    /// 使用範例：
    /// <code>
    /// decimal totalWithTax = TaxCalculationHelper.CalculateTotalWithTax(entity.TotalAmount, currentTaxRate);
    /// </code>
    /// </remarks>
    public static decimal CalculateTotalWithTax(decimal totalAmount, decimal taxRate, int decimals = 2)
    {
        var taxAmount = CalculateTax(totalAmount, taxRate, decimals);
        return Math.Round(totalAmount + taxAmount, decimals);
    }

    /// <summary>
    /// 直接從總金額計算含稅總額（使用乘法一次計算）
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxRate">稅率（百分比）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>含稅總額</returns>
    /// <remarks>
    /// 與 CalculateTotalWithTax 不同，此方法使用單一公式計算，可能會有微小的精度差異
    /// </remarks>
    public static decimal CalculateTotalWithTaxDirect(decimal totalAmount, decimal taxRate, int decimals = 2)
    {
        return Math.Round(totalAmount * (1 + taxRate / 100), decimals);
    }

    #endregion

    #region 反向計算方法（從含稅金額推算未稅金額）

    /// <summary>
    /// 從含稅總額反推未稅金額
    /// </summary>
    /// <param name="totalWithTax">含稅總額</param>
    /// <param name="taxRate">稅率（百分比）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>未稅金額</returns>
    /// <remarks>
    /// 計算公式：未稅金額 = 含稅總額 / (1 + 稅率/100)
    /// </remarks>
    public static decimal CalculateAmountBeforeTax(decimal totalWithTax, decimal taxRate, int decimals = 2)
    {
        return Math.Round(totalWithTax / (1 + taxRate / 100), decimals);
    }

    /// <summary>
    /// 從含稅總額反推稅額
    /// </summary>
    /// <param name="totalWithTax">含稅總額</param>
    /// <param name="taxRate">稅率（百分比）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>稅額</returns>
    /// <remarks>
    /// 計算公式：稅額 = 含稅總額 - (含稅總額 / (1 + 稅率/100))
    /// </remarks>
    public static decimal CalculateTaxFromTotalWithTax(decimal totalWithTax, decimal taxRate, int decimals = 2)
    {
        var amountBeforeTax = CalculateAmountBeforeTax(totalWithTax, taxRate, decimals);
        return Math.Round(totalWithTax - amountBeforeTax, decimals);
    }

    #endregion

    #region 批次計算方法

    /// <summary>
    /// 更新實體的稅額和含稅總額（使用反射）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="entity">實體物件</param>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxRate">稅率（百分比）</param>
    /// <param name="taxAmountPropertyName">稅額屬性名稱（預設為 "TaxAmount"）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <remarks>
    /// 使用範例：
    /// <code>
    /// TaxCalculationHelper.UpdateEntityTaxAmount(
    ///     purchaseOrder, 
    ///     purchaseOrder.TotalAmount, 
    ///     currentTaxRate,
    ///     "PurchaseTaxAmount"
    /// );
    /// </code>
    /// 
    /// 注意：含稅總額通常由實體的計算屬性自動處理，此方法不會設定含稅總額
    /// </remarks>
    public static void UpdateEntityTaxAmount<TEntity>(
        TEntity entity,
        decimal totalAmount,
        decimal taxRate,
        string taxAmountPropertyName = "TaxAmount",
        int decimals = 2) where TEntity : class
    {
        var taxAmount = CalculateTax(totalAmount, taxRate, decimals);

        var property = typeof(TEntity).GetProperty(taxAmountPropertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, taxAmount);
        }
        else
        {
            throw new InvalidOperationException(
                $"實體 {typeof(TEntity).Name} 沒有可寫入的屬性 {taxAmountPropertyName}");
        }
    }

    /// <summary>
    /// 計算多個明細的總金額並更新主檔稅額
    /// </summary>
    /// <typeparam name="TEntity">主檔實體類型</typeparam>
    /// <typeparam name="TDetail">明細實體類型</typeparam>
    /// <param name="entity">主檔實體</param>
    /// <param name="details">明細清單</param>
    /// <param name="taxRate">稅率（百分比）</param>
    /// <param name="detailAmountPropertyName">明細金額屬性名稱（預設為 "SubtotalAmount"）</param>
    /// <param name="entityTotalAmountPropertyName">主檔總金額屬性名稱（預設為 "TotalAmount"）</param>
    /// <param name="entityTaxAmountPropertyName">主檔稅額屬性名稱（預設為 "TaxAmount"）</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <remarks>
    /// 此方法會：
    /// 1. 計算所有明細的總金額
    /// 2. 更新主檔的 TotalAmount
    /// 3. 計算並更新主檔的 TaxAmount
    /// 
    /// 使用範例：
    /// <code>
    /// TaxCalculationHelper.CalculateAndUpdateFromDetails(
    ///     purchaseOrder,
    ///     purchaseOrderDetails,
    ///     currentTaxRate,
    ///     entityTaxAmountPropertyName: "PurchaseTaxAmount"
    /// );
    /// </code>
    /// </remarks>
    public static void CalculateAndUpdateFromDetails<TEntity, TDetail>(
        TEntity entity,
        IEnumerable<TDetail> details,
        decimal taxRate,
        string detailAmountPropertyName = "SubtotalAmount",
        string entityTotalAmountPropertyName = "TotalAmount",
        string entityTaxAmountPropertyName = "TaxAmount",
        int decimals = 2)
        where TEntity : class
        where TDetail : class
    {
        // 1. 計算總金額
        var detailProperty = typeof(TDetail).GetProperty(detailAmountPropertyName);
        if (detailProperty == null || !detailProperty.CanRead)
        {
            throw new InvalidOperationException(
                $"明細 {typeof(TDetail).Name} 沒有可讀取的屬性 {detailAmountPropertyName}");
        }

        decimal totalAmount = 0m;
        foreach (var detail in details)
        {
            var value = detailProperty.GetValue(detail);
            if (value is decimal amount)
            {
                totalAmount += amount;
            }
        }

        // 2. 更新主檔總金額
        var entityTotalProperty = typeof(TEntity).GetProperty(entityTotalAmountPropertyName);
        if (entityTotalProperty != null && entityTotalProperty.CanWrite)
        {
            entityTotalProperty.SetValue(entity, totalAmount);
        }

        // 3. 計算並更新稅額
        var taxAmount = CalculateTax(totalAmount, taxRate, decimals);
        var entityTaxProperty = typeof(TEntity).GetProperty(entityTaxAmountPropertyName);
        if (entityTaxProperty != null && entityTaxProperty.CanWrite)
        {
            entityTaxProperty.SetValue(entity, taxAmount);
        }
    }

    #endregion

    #region 格式化方法

    /// <summary>
    /// 格式化稅率顯示文字（例如：5.00%）
    /// </summary>
    /// <param name="taxRate">稅率</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>格式化的稅率文字</returns>
    public static string FormatTaxRate(decimal taxRate, int decimals = 2)
    {
        return $"{taxRate.ToString($"F{decimals}")}%";
    }

    /// <summary>
    /// 產生稅額欄位的 Label 文字（例如：採購稅額(5.00%)）
    /// </summary>
    /// <param name="prefix">前綴文字（例如：採購稅額、銷貨稅額）</param>
    /// <param name="taxRate">稅率</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>完整的 Label 文字</returns>
    public static string GenerateTaxAmountLabel(string prefix, decimal taxRate, int decimals = 2)
    {
        return $"{prefix}({FormatTaxRate(taxRate, decimals)})";
    }

    /// <summary>
    /// 產生稅額欄位的 HelpText（例如：採購單的稅額，根據明細自動計算（稅率：5.00%））
    /// </summary>
    /// <param name="documentType">單據類型（例如：採購單、銷貨單）</param>
    /// <param name="taxRate">稅率</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>HelpText 文字</returns>
    public static string GenerateTaxAmountHelpText(string documentType, decimal taxRate, int decimals = 2)
    {
        return $"{documentType}的稅額，根據明細自動計算（稅率：{FormatTaxRate(taxRate, decimals)}）";
    }

    #endregion

    #region 驗證方法

    /// <summary>
    /// 驗證稅額計算是否正確
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxAmount">稅額</param>
    /// <param name="taxRate">稅率</param>
    /// <param name="tolerance">容差（預設為 0.01，即 1 分錢）</param>
    /// <returns>稅額是否正確</returns>
    public static bool ValidateTaxAmount(
        decimal totalAmount,
        decimal taxAmount,
        decimal taxRate,
        decimal tolerance = 0.01m)
    {
        var expectedTaxAmount = CalculateTax(totalAmount, taxRate);
        return Math.Abs(taxAmount - expectedTaxAmount) <= tolerance;
    }

    /// <summary>
    /// 驗證含稅總額計算是否正確
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxAmount">稅額</param>
    /// <param name="totalWithTax">含稅總額</param>
    /// <param name="tolerance">容差（預設為 0.01）</param>
    /// <returns>含稅總額是否正確</returns>
    public static bool ValidateTotalWithTax(
        decimal totalAmount,
        decimal taxAmount,
        decimal totalWithTax,
        decimal tolerance = 0.01m)
    {
        var expectedTotal = totalAmount + taxAmount;
        return Math.Abs(totalWithTax - expectedTotal) <= tolerance;
    }

    #endregion

    #region 統計與分析方法

    /// <summary>
    /// 計算稅額統計資訊
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxRate">稅率</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>包含未稅金額、稅額、含稅總額的元組</returns>
    public static (decimal TotalAmount, decimal TaxAmount, decimal TotalWithTax) CalculateTaxSummary(
        decimal totalAmount,
        decimal taxRate,
        int decimals = 2)
    {
        var taxAmount = CalculateTax(totalAmount, taxRate, decimals);
        var totalWithTax = Math.Round(totalAmount + taxAmount, decimals);

        return (totalAmount, taxAmount, totalWithTax);
    }

    /// <summary>
    /// 計算有效稅率（實際稅額 ÷ 未稅金額）
    /// </summary>
    /// <param name="totalAmount">總金額（未稅）</param>
    /// <param name="taxAmount">實際稅額</param>
    /// <param name="decimals">小數位數（預設為 2）</param>
    /// <returns>有效稅率（百分比）</returns>
    public static decimal CalculateEffectiveTaxRate(decimal totalAmount, decimal taxAmount, int decimals = 2)
    {
        if (totalAmount == 0)
            return 0m;

        return Math.Round((taxAmount / totalAmount) * 100, decimals);
    }

    #endregion

    #region 依稅別計算方法（從明細列表）

    /// <summary>
    /// 根據稅別（TaxCalculationMethod）從明細列表計算總金額與稅額
    /// 支援三種稅別：外加稅、內含稅、免稅
    ///
    /// 此方法統一了各 EditModal 中重複的 switch(taxMethod) 區塊，
    /// 適用於採購單、進貨單、銷貨單、出貨單等所有具有明細稅率的單據。
    /// </summary>
    /// <typeparam name="TDetail">明細型別</typeparam>
    /// <param name="details">明細列表</param>
    /// <param name="taxMethod">稅別計算方式</param>
    /// <param name="defaultTaxRate">預設稅率（當明細無獨立稅率時使用）</param>
    /// <param name="getSubtotal">取得明細小計金額的委派</param>
    /// <param name="getTaxRate">取得明細獨立稅率的委派（回傳 null 表示使用預設稅率）</param>
    /// <returns>(TotalAmount 未稅金額, TaxAmount 稅額)</returns>
    /// <remarks>
    /// 使用範例：
    /// <code>
    /// var (total, tax) = TaxCalculationHelper.CalculateFromDetails(
    ///     purchaseOrderDetails,
    ///     editModalComponent.Entity.TaxCalculationMethod,
    ///     currentTaxRate,
    ///     d => d.SubtotalAmount,
    ///     d => d.TaxRate);
    /// editModalComponent.Entity.TotalAmount = total;
    /// editModalComponent.Entity.PurchaseTaxAmount = tax;
    /// </code>
    /// </remarks>
    public static (decimal TotalAmount, decimal TaxAmount) CalculateFromDetails<TDetail>(
        IEnumerable<TDetail> details,
        TaxCalculationMethod taxMethod,
        decimal defaultTaxRate,
        Func<TDetail, decimal> getSubtotal,
        Func<TDetail, decimal?> getTaxRate)
    {
        var detailList = details as IList<TDetail> ?? details.ToList();

        switch (taxMethod)
        {
            case TaxCalculationMethod.TaxExclusive:
            default:
            {
                // 外加稅：金額 = 明細小計合計，稅額 = 各明細分別算稅後加總（四捨五入到整數）
                var totalAmount = Math.Round(
                    detailList.Sum(d => getSubtotal(d)), 0, MidpointRounding.AwayFromZero);
                var taxAmount = detailList.Sum(d =>
                {
                    var rate = getTaxRate(d) ?? defaultTaxRate;
                    return Math.Round(getSubtotal(d) * (rate / 100m), 0, MidpointRounding.AwayFromZero);
                });
                return (totalAmount, taxAmount);
            }

            case TaxCalculationMethod.TaxInclusive:
            {
                // 內含稅：總額 = 明細小計，稅額 = 各明細反推稅額後加總，金額 = 總額 - 稅額
                var totalWithTax = detailList.Sum(d => getSubtotal(d));
                var taxAmount = detailList.Sum(d =>
                {
                    var rate = getTaxRate(d) ?? defaultTaxRate;
                    return Math.Round(
                        getSubtotal(d) / (1 + rate / 100m) * (rate / 100m),
                        0, MidpointRounding.AwayFromZero);
                });
                var totalAmount = Math.Round(totalWithTax - taxAmount, 0, MidpointRounding.AwayFromZero);
                return (totalAmount, taxAmount);
            }

            case TaxCalculationMethod.NoTax:
            {
                // 免稅：金額 = 明細小計合計，稅額 = 0
                var totalAmount = Math.Round(
                    detailList.Sum(d => getSubtotal(d)), 0, MidpointRounding.AwayFromZero);
                return (totalAmount, 0m);
            }
        }
    }

    #endregion
}
