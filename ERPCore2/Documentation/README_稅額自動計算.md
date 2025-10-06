# 採購單稅額自動計算功能實作說明

## 📋 功能概述

實作採購單編輯時的稅額自動計算功能，當使用者輸入單價和數量時，系統會自動計算：
1. **採購總金額**（未稅）
2. **採購稅額**（根據系統稅率）
3. **含稅總金額**（總金額 + 稅額）

## 🎯 核心設計理念

### ⚡ 效能優化：稅率快取機制

**問題**：如果每次輸入都去查詢資料庫取得稅率，會造成嚴重的效能問題。

**解決方案**：在組件初始化時**一次性載入稅率**並快取在記憶體中，後續計算直接使用快取值。

```csharp
// 稅率快取變數
private decimal currentTaxRate = 5.0m; // 預設 5%，實際值會在初始化時從資料庫載入
```

## 🔧 實作細節

### 1️⃣ 稅率載入（初始化時執行一次）

在 `LoadAdditionalDataAsync()` 方法中載入稅率：

```csharp
// 🔑 一次性載入系統稅率（避免每次計算都查詢資料庫）
try
{
    currentTaxRate = await SystemParameterService.GetTaxRateAsync();
}
catch (Exception)
{
    // 如果載入失敗，使用預設值 5%
    currentTaxRate = 5.0m;
}
```

**執行時機**：
- 組件初始化時 (`OnParametersSetAsync`)
- Modal 開啟時

**優點**：
- ✅ 只查詢一次資料庫
- ✅ 後續所有計算都使用快取值
- ✅ 有容錯處理（失敗時使用預設值）

### 2️⃣ 自動計算稅額（每次明細變更時執行）

在 `HandleDetailsChanged()` 方法中計算：

```csharp
if (editModalComponent?.Entity != null)
{
    // 1. 計算總金額（未稅）
    editModalComponent.Entity.TotalAmount = purchaseOrderDetails.Sum(d => d.SubtotalAmount);
    
    // 2. 計算稅額 = 總金額 × 稅率（使用快取的稅率，避免每次都查詢資料庫）
    editModalComponent.Entity.PurchaseTaxAmount = Math.Round(
        editModalComponent.Entity.TotalAmount * (currentTaxRate / 100), 
        2  // 四捨五入到小數點後2位
    );
    
    // 3. 含稅總金額會自動計算（PurchaseTotalAmountIncludingTax 是計算屬性）
    //    = TotalAmount + PurchaseTaxAmount
    
    StateHasChanged();
}
```

**觸發時機**：
- 使用者輸入或修改商品單價
- 使用者輸入或修改商品數量
- 新增或刪除商品明細

**計算公式**：
```
採購總金額 = Σ(商品單價 × 數量)
採購稅額 = 採購總金額 × (稅率 ÷ 100)
含稅總金額 = 採購總金額 + 採購稅額
```

### 3️⃣ 儲存前再次計算（確保資料一致性）

在 `SavePurchaseOrderWithDetails()` 方法中：

```csharp
// 更新總金額和稅額
purchaseOrder.TotalAmount = purchaseOrderDetails.Sum(d => d.SubtotalAmount);
purchaseOrder.PurchaseTaxAmount = Math.Round(
    purchaseOrder.TotalAmount * (currentTaxRate / 100), 
    2
);
```

**目的**：確保儲存到資料庫的資料是最新且正確的。

### 4️⃣ 動態顯示稅率（UI 提示）

在表單欄位定義中：

```csharp
new FormFieldDefinition()
{
    PropertyName = nameof(PurchaseOrder.PurchaseTaxAmount),
    Label = $"採購稅額({currentTaxRate:F2}%)",  // 動態顯示稅率
    FieldType = FormFieldType.Number,
    IsRequired = false,
    HelpText = $"採購單的稅額，根據明細自動計算（稅率：{currentTaxRate:F2}%）",
    IsReadOnly = true
}
```

**效果**：
- 標籤會顯示「採購稅額(5.00%)」
- 提示文字會顯示當前稅率
- 如果系統稅率變更，UI 會自動更新

## 📊 完整資料流程

```
使用者輸入單價/數量
    ↓
OnPriceInput / OnQuantityInput (PurchaseOrderDetailManagerComponent)
    ↓
NotifyDetailsChanged
    ↓
HandleDetailsChanged (PurchaseOrderEditModalComponent)
    ↓
計算：
  1. TotalAmount = Σ(SubtotalAmount)
  2. PurchaseTaxAmount = TotalAmount × (currentTaxRate / 100)
  3. PurchaseTotalAmountIncludingTax = TotalAmount + PurchaseTaxAmount (自動計算屬性)
    ↓
StateHasChanged() → UI 更新
```

## 🗂️ 相關實體類別

### PurchaseOrder.cs

```csharp
[Display(Name = "訂單總金額")]
[Column(TypeName = "decimal(18,2)")]
public decimal TotalAmount { get; set; } = 0;

[Display(Name = "採購稅額")]
[Column(TypeName = "decimal(18,2)")]
public decimal PurchaseTaxAmount { get; set; } = 0;

[Display(Name = "採購含稅總金額")]
[NotMapped]  // 不儲存在資料庫，計算屬性
public decimal PurchaseTotalAmountIncludingTax => PurchaseTaxAmount + TotalAmount;
```

### PurchaseOrderDetail.cs

```csharp
[Display(Name = "小計金額")]
[Column(TypeName = "decimal(18,2)")]
public decimal SubtotalAmount => OrderQuantity * UnitPrice;  // 自動計算屬性
```

### SystemParameter.cs

```csharp
/// <summary>
/// 稅率 (%)
/// </summary>
[Display(Name = "稅率 (%)")]
[Range(0.00, 100.00, ErrorMessage = "稅率範圍必須在 0% 到 100% 之間")]
public decimal TaxRate { get; set; } = 5.00m; // 預設 5% 稅率
```

## ⚙️ 服務層支援

### ISystemParameterService

```csharp
/// <summary>
/// 取得系統稅率
/// </summary>
/// <returns>當前系統稅率</returns>
Task<decimal> GetTaxRateAsync();
```

## ✅ 優點總結

1. **效能優化**：
   - ✅ 稅率只在初始化時載入一次
   - ✅ 避免重複查詢資料庫
   - ✅ 即時計算不會造成效能負擔

2. **使用者體驗**：
   - ✅ 即時自動計算，無需手動操作
   - ✅ UI 即時更新
   - ✅ 動態顯示當前稅率

3. **資料一致性**：
   - ✅ 計算邏輯集中管理
   - ✅ 儲存前再次驗證
   - ✅ 使用 `Math.Round` 確保精度

4. **維護性**：
   - ✅ 稅率來自系統參數表，易於調整
   - ✅ 程式碼清晰易懂
   - ✅ 有完整的錯誤處理

## 🔮 未來擴展建議

### 1. 稅率變更通知機制

如果系統稅率在使用中被修改，可以考慮實作通知機制：

```csharp
// 方案一：定期檢查（適用於長時間開啟 Modal 的情況）
// 方案二：SignalR 即時通知（更複雜但更即時）
```

### 2. 支援不同稅率

如果未來需要支援不同商品使用不同稅率：

```csharp
// 在 Product 實體中新增 TaxRate 屬性
// 明細計算時使用商品的稅率而非系統統一稅率
```

### 3. 稅額調整功能

如果需要手動微調稅額（例如尾數調整）：

```csharp
// 新增 ManualTaxAdjustment 屬性
// 最終稅額 = 計算稅額 + 手動調整額
```

## 📝 相關文件

- [採購單編輯組件](../Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor)
- [採購明細管理組件](../Components/Shared/SubCollections/PurchaseOrderDetailManagerComponent.razor)
- [系統參數實體](../Data/Entities/Systems/SystemParameter.cs)
- [系統參數服務](../Services/Systems/SystemParameterService.cs)

## 🎓 學習要點

1. **快取策略**：對於不常變動的資料（如系統參數），在組件生命週期內快取可大幅提升效能
2. **計算屬性**：善用 C# 的計算屬性（`get => ...`）可簡化程式碼並確保資料一致性
3. **精度處理**：金額計算務必使用 `decimal` 類型並適當使用 `Math.Round`
4. **事件驅動**：透過 `EventCallback` 實現子組件到父組件的資料流動

---

**最後更新**：2025/10/06  
**作者**：ERP 系統開發團隊
