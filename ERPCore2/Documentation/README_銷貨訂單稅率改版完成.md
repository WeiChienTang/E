# 銷貨訂單稅率欄位改版完成報告

## 📋 改版概要

已完成銷貨訂單（SalesOrder）從「統一稅率」改為「明細獨立稅率」+ 「主檔稅別」的完整改版。

### 改版目的
- **舊設計**：整張單據使用系統統一稅率（5%）計算稅額
- **新設計（兩層架構）**：
  1. **主檔層**：增加「稅別」欄位（外加稅/內含稅/不含稅）
  2. **明細層**：每筆明細可設定獨立稅率，支援不同商品不同稅率的需求

---

## 🔧 完成的修改項目

### ✅ 步驟 0：主檔增加稅別欄位

#### 0.1 確認稅別 Enum
**檔案**：`Data/Enums/TaxCalculationMethod.cs` ✅ 已存在

```csharp
public enum TaxCalculationMethod
{
    [Display(Name = "外加稅")]
    TaxExclusive = 1,  // 稅額另外加上
    
    [Display(Name = "內含稅")]
    TaxInclusive = 2,  // 總價已包含稅
    
    [Display(Name = "不含稅")]
    NoTax = 3  // 完全免稅
}
```

#### 0.2 修改主檔實體
**檔案**：`Data/Entities/Sales/SalesOrder.cs` ✅ 已修改

新增欄位：
```csharp
[Required(ErrorMessage = "稅別為必填")]
[Display(Name = "稅別")]
public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;
```

#### 0.3 執行 Migration
✅ 已完成
- Migration 名稱：`20251127134220_AddTaxCalculationMethodToSalesOrder`
- 資料庫欄位：`SalesOrders.TaxCalculationMethod` (int, NOT NULL, DEFAULT 0)

#### 0.4 修改 EditModal 組件
**檔案**：`Components/Pages/Sales/SalesOrderEditModalComponent.razor` ✅ 已修改

- ✅ 增加 `taxCalculationMethodOptions` 變數
- ✅ 在 `LoadAdditionalDataAsync` 中初始化選項
- ✅ 在 `InitializeFormFieldsAsync` 中增加表單欄位
- ✅ 在 `formSections` 中加入「基本資訊」區段

---

### ✅ 步驟 1：資料表增加稅率欄位

#### 1.1 修改明細實體
**檔案**：`Data/Entities/Sales/SalesOrderDetail.cs` ✅ 已修改

新增欄位：
```csharp
[Display(Name = "稅率(%)")]
[Column(TypeName = "decimal(5,2)")]
[Range(0, 100, ErrorMessage = "稅率必須介於 0 到 100 之間")]
public decimal? TaxRate { get; set; }
```

#### 1.2 執行 Migration
✅ 已完成
- Migration 名稱：`20251127134249_AddTaxRateToSalesOrderDetail`
- 資料庫欄位：`SalesOrderDetails.TaxRate` (decimal(5,2), NULL)

---

### ✅ 步驟 2：Table 組件增加稅率欄位

**檔案**：`Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor` ✅ 已修改

#### 2.1 增加稅別參數
```csharp
// ===== 稅別參數（新增）=====
[Parameter] public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;

// ===== 輔助計算屬性 =====
private bool IsTaxCalculationMethodNoTax => TaxCalculationMethod == TaxCalculationMethod.NoTax;
```

#### 2.2 修改 SalesItem 類別
```csharp
public class SalesItem
{
    // ... 其他屬性 ...
    public decimal TaxRate { get; set; } = 5.0m;
    // ... 其他屬性 ...
}
```

#### 2.3 增加稅率欄位（在 GetColumnDefinitions 中）
- ✅ 位置：折扣欄位後面
- ✅ 類型：Number
- ✅ 可編輯：是（免稅時禁用）
- ✅ 動態提示：根據稅別和鎖定狀態

#### 2.4 增加稅率輸入處理方法
```csharp
private async Task OnTaxRateInput(SalesItem item, string? valueString)
{
    // 驗證並限制稅率範圍 0-100
    // 檢查是否可以修改（退貨/沖款記錄）
    // 通知父組件資料已變更
}
```

#### 2.5 增加小計計算方法（支援三種稅別）
```csharp
private decimal CalculateItemSubtotal(SalesItem item)
{
    switch (TaxCalculationMethod)
    {
        case TaxCalculationMethod.TaxExclusive:
            // 外加稅：小計 = 基礎金額 × (1 + 稅率%)
            return Math.Round(baseAmount * (1 + item.TaxRate / 100), 0);
            
        case TaxCalculationMethod.TaxInclusive:
            // 內含稅：小計 = 基礎金額（單價已含稅）
            return Math.Round(baseAmount, 0);
            
        case TaxCalculationMethod.NoTax:
            // 免稅：小計 = 基礎金額
            return Math.Round(baseAmount, 0);
    }
}
```

#### 2.6 商品選擇時自動帶入稅率
```csharp
private async Task OnProductSelectItem(SalesItem item, Product? selectedProduct)
{
    // 自動帶入商品稅率（優先使用商品稅率，沒有則使用預設 5%）
    item.TaxRate = selectedProduct.TaxRate ?? 5.0m;
}
```

#### 2.7 載入現有明細時設定稅率
```csharp
private async Task LoadExistingDetailsAsync()
{
    // 優先順序：明細 > 商品 > 系統預設
    TaxRate = salesDetail.TaxRate ?? salesDetail.Product?.TaxRate ?? 5.0m
}
```

#### 2.8 儲存時寫入稅率
```csharp
private List<TDetailEntity> ConvertToDetailEntities()
{
    // 更新稅率
    SetPropertyValue(detail, "TaxRate", item.TaxRate);
}
```

---

### ✅ 步驟 3：EditModal 傳遞稅別並改用明細稅率計算

**檔案**：`Components/Pages/Sales/SalesOrderEditModalComponent.razor` ✅ 已修改

#### 3.1 傳遞稅別給 Table 組件
```razor
<SalesOrderTable @ref="salesOrderDetailManager"
                 TaxCalculationMethod="@editModalComponent.Entity.TaxCalculationMethod"
                 ... />
```

#### 3.2 修改 HandleDetailsChanged 方法（支援三種稅別）
```csharp
private async Task HandleDetailsChanged(List<SalesOrderDetail> details)
{
    switch (editModalComponent.Entity.TaxCalculationMethod)
    {
        case TaxCalculationMethod.TaxExclusive:
            // 外加稅計算邏輯
            break;
            
        case TaxCalculationMethod.TaxInclusive:
            // 內含稅計算邏輯（反推）
            break;
            
        case TaxCalculationMethod.NoTax:
            // 免稅計算邏輯
            break;
    }
}
```

#### 3.3 增加稅別變更時的連動更新
```csharp
private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    if (fieldChange.PropertyName == nameof(SalesOrder.TaxCalculationMethod))
    {
        // 稅別變更時，重新計算金額和稅額
        await HandleDetailsChanged(salesOrderDetails);
        StateHasChanged();
    }
}
```

#### 3.4 鎖定稅別欄位（當有不可刪除的明細時）
```csharp
private void UpdateFieldsReadOnlyState()
{
    var fieldsToLock = new[]
    {
        nameof(SalesOrder.Code),
        nameof(SalesOrder.OrderDate),
        nameof(SalesOrder.TaxCalculationMethod),  // 新增
        // ... 其他欄位
    };
}
```

---

## 📊 計算範例

### 外加稅（TaxExclusive）
**主檔設定**：稅別 = 外加稅

| 商品 | 數量 | 單價 | 稅率 | 小計（未稅）| 稅額 | 小計（含稅）|
|------|------|------|------|------------|------|-------------|
| A商品 | 10 | 100 | 5% | 1,000 | 50 | 1,050 |
| B商品 | 5 | 200 | 10% | 1,000 | 100 | 1,100 |
| **合計** | | | | **2,000** | **150** | **2,150** |

### 內含稅（TaxInclusive）
**主檔設定**：稅別 = 內含稅

| 商品 | 數量 | 單價 | 稅率 | 小計（含稅）| 稅額 | 金額（未稅）|
|------|------|------|------|------------|------|-------------|
| A商品 | 10 | 100 | 5% | 1,000 | 48 | 952 |
| B商品 | 5 | 200 | 10% | 1,000 | 91 | 909 |
| **合計** | | | | **2,000** | **139** | **1,861** |

### 免稅（NoTax）
**主檔設定**：稅別 = 不含稅

| 商品 | 數量 | 單價 | 小計 | 稅額 | 總額 |
|------|------|------|------|------|------|
| A商品 | 10 | 100 | 1,000 | 0 | 1,000 |
| B商品 | 5 | 200 | 1,000 | 0 | 1,000 |
| **合計** | | | **2,000** | **0** | **2,000** |

---

## ✅ 測試檢查清單

### 基本功能
- [x] 新增單據時，稅別預設為「外加稅」
- [x] 稅別下拉選單有三個選項（外加稅/內含稅/不含稅）
- [x] 切換稅別時，金額、稅額、總額立即更新
- [x] 選擇免稅時，明細稅率欄位被禁用
- [x] 選擇免稅時，主檔稅額顯示為 0

### 稅額計算
- [x] 外加稅計算正確：小計 = 數量 × 單價 × (1 + 稅率%)
- [x] 內含稅計算正確：小計 = 數量 × 單價（反推稅額）
- [x] 免稅計算正確：小計 = 數量 × 單價，稅額 = 0
- [x] 混合不同稅率商品時，稅額計算正確

### 明細操作
- [x] 新增商品時，稅率自動帶入（商品稅率 > 系統預設值 5%）
- [x] 編輯單據時，稅率正確顯示
- [x] 載入現有明細時，稅率優先順序正確（明細 > 商品 > 系統）
- [x] 儲存後稅額計算正確（不會被覆蓋）
- [x] 舊資料（TaxRate = NULL）能正確顯示商品稅率

### 鎖定機制
- [x] 有退貨記錄的明細無法修改稅率
- [x] 有沖款記錄的明細無法修改稅率
- [x] 有不可刪除的明細時，主檔稅別欄位被鎖定

---

## 🎯 改版完成狀態

| 項目 | 狀態 |
|------|------|
| **步驟 0：主檔稅別** | ✅ 完成 |
| **步驟 1：明細稅率** | ✅ 完成 |
| **步驟 2：Table 組件** | ✅ 完成 |
| **步驟 3：EditModal 組件** | ✅ 完成 |
| **Migration 執行** | ✅ 完成 |
| **編譯測試** | ✅ 通過 |

---

## 📝 注意事項

1. **四捨五入規則**：所有金額和稅額都必須四捨五入到整數（0 位小數），符合台灣稅務規定
2. **稅率優先順序**：明細稅率 > 商品稅率 > 系統預設值（5%）
3. **鎖定機制**：有退貨或沖款記錄的明細無法修改任何欄位，包括稅率
4. **稅別變更**：切換稅別時會自動重新計算所有金額
5. **免稅處理**：選擇「不含稅」時，稅率欄位禁用，稅額顯示為 0

---

## 🔍 相關文件

- 改版指南：`Documentation/README_稅率欄位改版指南.md`
- 資料庫遷移：`Migrations/20251127134220_AddTaxCalculationMethodToSalesOrder.cs`
- 資料庫遷移：`Migrations/20251127134249_AddTaxRateToSalesOrderDetail.cs`

---

**改版日期**：2025年11月27日  
**改版狀態**：✅ 已完成並編譯通過
