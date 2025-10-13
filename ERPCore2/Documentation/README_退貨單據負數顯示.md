# 退貨單據負數顯示功能

## 📋 功能概述

在沖款明細管理中，退貨單據（銷貨退回、進貨退回）的金額現在會以**負數**顯示，讓使用者更清楚地看出退貨對沖款的影響。

## 🎯 修改目標

1. **應收/付金額欄位**：退貨單據顯示為負數（紅色）
2. **未沖款餘額欄位**：退貨單據顯示為負數（黃色/灰色）
3. **本次沖款欄位**：退貨單據顯示為負數，輸入時自動轉換
4. **本次折讓欄位**：退貨單據顯示為負數，輸入時自動轉換
5. **計算邏輯**：正確處理正負數抵銷

## 💡 範例說明

### 情境：同時有銷貨和銷貨退回
```
銷貨訂單：     應收金額 600 元（綠色） → 未沖款餘額 600 元
銷貨退回：     應收金額 -300 元（紅色）→ 未沖款餘額 -300 元
----------------------------------------------------------------
本期應收總額：  300 元
```

### 沖款計算
```
銷貨訂單沖款：  100 元  → 顯示為 100  → 未沖款餘額變為 500
銷貨退回沖款：   50 元  → 顯示為 -50  → 未沖款餘額變為 -250
----------------------------------------------------------------
本期沖銷總額：   50 元  （100 - 50）
```

## 🔧 技術實現

### 1. 前端顯示邏輯 (SetoffProductDetailManagerComponent.razor)

#### 應收/付金額欄位（第 288-302 行）
```csharp
var cssClass = detail?.IsReturn == true ? "text-danger" : "text-success";
var amount = detail?.TotalAmount ?? 0;
var displayAmount = detail?.IsReturn == true ? -amount : amount;
return @<div class="text-end fw-bold @cssClass">@displayAmount.ToString("N2")</div>;
```

#### 未沖款餘額欄位（第 303-318 行）
```csharp
var remainingAmount = detail?.RemainingAmount ?? 0;
var displayAmount = detail?.IsReturn == true ? -remainingAmount : remainingAmount;
var cssClass = remainingAmount > 0 ? "text-warning fw-bold" : "text-muted";
return @<div class="text-end @cssClass">@displayAmount.ToString("N2")</div>;
```

#### 本次沖款欄位（第 319-338 行）
```csharp
var isReturn = setoffItem.UnsettledDetail?.IsReturn == true;
var displayValue = setoffItem.CurrentSetoffAmount == 0 ? "" : 
    (isReturn ? -setoffItem.CurrentSetoffAmount : setoffItem.CurrentSetoffAmount).ToString();
```

#### 本次折讓欄位（第 345-367 行）
```csharp
var isReturn = setoffItem.UnsettledDetail?.IsReturn == true;
var displayValue = setoffItem.CurrentAllowanceAmount == 0 ? "" : 
    (isReturn ? -setoffItem.CurrentAllowanceAmount : setoffItem.CurrentAllowanceAmount).ToString();
```

### 2. 輸入處理（第 387-421 行）

使用者無論輸入正數或負數，系統都會將金額轉為**絕對值**儲存：

```csharp
private async Task OnSetoffAmountInput(SetoffProductDetailItem item, string? value)
{
    if (decimal.TryParse(value, out var amount))
    {
        // 將輸入的金額轉為絕對值儲存（無論用戶輸入正數或負數）
        item.CurrentSetoffAmount = Math.Abs(amount);
        await NotifyDetailsChanged();
    }
    ...
}
```

### 3. 計算方法

#### GetSourceTotalAmount() - 本期應收計算
```csharp
public decimal GetSourceTotalAmount()
{
    return SetoffItems
        .Where(item => item.CurrentSetoffAmount > 0 || item.CurrentAllowanceAmount > 0)
        .Sum(item => 
        {
            var amount = item.SourceTotalAmount;
            // 如果是退貨單據，金額取負值
            return item.UnsettledDetail?.IsReturn == true ? -amount : amount;
        });
}
```

#### GetCurrentSetoffTotal() - 本期沖銷計算
```csharp
public decimal GetCurrentSetoffTotal()
{
    return SetoffItems
        .Where(item => item.CurrentSetoffAmount > 0 || item.CurrentAllowanceAmount > 0)
        .Sum(item =>
        {
            var total = item.CurrentSetoffAmount + item.CurrentAllowanceAmount;
            // 如果是退貨單據，金額取負值
            var isReturn = item.SourceDetailType == SetoffDetailType.SalesReturnDetail || 
                          item.SourceDetailType == SetoffDetailType.PurchaseReturnDetail;
            return isReturn ? -total : total;
        });
}
```

#### GetCurrentAllowanceTotal() - 本期折讓計算
```csharp
public decimal GetCurrentAllowanceTotal()
{
    return SetoffItems
        .Where(item => item.CurrentSetoffAmount > 0 || item.CurrentAllowanceAmount > 0)
        .Sum(item =>
        {
            var total = item.CurrentAllowanceAmount;
            // 如果是退貨單據，金額取負值
            var isReturn = item.SourceDetailType == SetoffDetailType.SalesReturnDetail || 
                          item.SourceDetailType == SetoffDetailType.PurchaseReturnDetail;
            return isReturn ? -total : total;
        });
}
```

### 4. 父組件更新 (SetoffDocumentEditModalComponent.razor)

使用新的計算方法來更新總額：

```csharp
private void HandleProductDetailsChanged(List<SetoffProductDetail> details)
{
    setoffProductDetails = details;
    
    if (editModalComponent?.Entity != null)
    {
        // 本期應收（考慮退貨為負數）
        if (setoffProductDetailManager != null)
        {
            editModalComponent.Entity.TotalSetoffAmount = 
                setoffProductDetailManager.GetSourceTotalAmount();
        }
        
        // 本期沖銷（考慮退貨為負數）
        if (setoffProductDetailManager != null)
        {
            editModalComponent.Entity.CurrentSetoffAmount = 
                setoffProductDetailManager.GetCurrentSetoffTotal();
        }
        
        // 本期折讓（考慮退貨為負數）
        if (setoffProductDetailManager != null)
        {
            editModalComponent.Entity.TotalAllowanceAmount = 
                setoffProductDetailManager.GetCurrentAllowanceTotal();
        }
    }
    
    StateHasChanged();
}
```

### 5. 儲存驗證邏輯 (SetoffDocumentEditModalComponent.razor)

儲存前驗證也需要考慮退貨為負數：

#### 本期沖銷驗證（第 298-310 行）
```csharp
// 驗證本期沖銷 = 商品明細的本次沖款 + 本次折讓總和（考慮退貨為負數）
var expectedCurrentSetoff = setoffProductDetails?.Sum(d => 
{
    var total = d.CurrentSetoffAmount + d.CurrentAllowanceAmount;
    // 如果是退貨單據，金額取負值
    var isReturn = d.SourceDetailType == SetoffDetailType.SalesReturnDetail || 
                  d.SourceDetailType == SetoffDetailType.PurchaseReturnDetail;
    return isReturn ? -total : total;
}) ?? 0;
```

#### 商品明細本次沖款驗證（第 307-318 行）
```csharp
// 考慮退貨單據為負數
var expectedReceivedFromProduct = setoffProductDetails?.Sum(d => 
{
    var isReturn = d.SourceDetailType == SetoffDetailType.SalesReturnDetail || 
                  d.SourceDetailType == SetoffDetailType.PurchaseReturnDetail;
    return isReturn ? -d.CurrentSetoffAmount : d.CurrentSetoffAmount;
}) ?? 0;

var expectedAllowanceFromProduct = setoffProductDetails?.Sum(d => 
{
    var isReturn = d.SourceDetailType == SetoffDetailType.SalesReturnDetail || 
                  d.SourceDetailType == SetoffDetailType.PurchaseReturnDetail;
    return isReturn ? -d.CurrentAllowanceAmount : d.CurrentAllowanceAmount;
}) ?? 0;
```

#### 本期折讓驗證（第 353-365 行）
```csharp
// 驗證本期折讓 = 商品明細的本次折讓總和（考慮退貨為負數）
var expectedAllowance = setoffProductDetails?.Sum(d => 
{
    var isReturn = d.SourceDetailType == SetoffDetailType.SalesReturnDetail || 
                  d.SourceDetailType == SetoffDetailType.PurchaseReturnDetail;
    return isReturn ? -d.CurrentAllowanceAmount : d.CurrentAllowanceAmount;
}) ?? 0;
```

## 📊 資料儲存

重要：所有金額在**資料庫中仍以正數儲存**，負號只用於：
1. 前端顯示
2. 計算總額時的正負抵銷

這樣的設計確保：
- 資料的一致性（所有金額都是正數）
- 計算的正確性（透過 `SourceDetailType` 判斷是否為退貨）
- 顯示的直覺性（退貨直接顯示負數）

## 🔍 判斷退貨的依據

使用 `SetoffDetailType` 枚舉來判斷：
- `SalesReturnDetail` (2) = 銷貨退回明細
- `PurchaseReturnDetail` (4) = 進貨退回明細

## ✅ 測試要點

1. **單一退貨單據**：
   - 應收/付金額顯示為負數（紅色）
   - 未沖款餘額顯示為負數（黃色/灰色）
   - 本次沖款顯示為負數
   
2. **混合單據**：
   - 銷貨 600 + 銷貨退回 -300 = 本期應收 300
   - 未沖款餘額：銷貨 600 + 銷貨退回 -300
   - 銷貨沖款 100 + 銷貨退回沖款 -50 = 本期沖銷 50
   
3. **輸入驗證**：
   - 輸入正數：正常儲存，退貨自動顯示負號
   - 輸入負數：轉為絕對值儲存，退貨自動顯示負號
   
4. **餘額計算**：
   - 銷貨未沖款餘額 600，沖款 100 後剩餘 500（顯示為 500）
   - 銷貨退回未沖款餘額 -300，沖款 50 後剩餘 -250（顯示為 -250）

## 📅 修改日期

2025年10月13日

## � 修改記錄

### 2025年10月13日 - 修正小計顯示邏輯
- **問題**：小計欄位沒有考慮退貨單據，導致負數沖款顯示為正數
- **修正**：在小計計算時根據 `IsReturn` 判斷是否需要轉為負數
- **位置**：`SetoffProductDetailManagerComponent.razor` 第 371-388 行

### 2025年10月13日 - 修正儲存驗證邏輯
- **問題**：儲存時的驗證邏輯直接累加 `CurrentSetoffAmount` 和 `CurrentAllowanceAmount`，沒有考慮退貨為負數
- **修正**：在驗證計算時根據 `SourceDetailType` 判斷是否為退貨，並將金額轉為負數
- **位置**：`SetoffDocumentEditModalComponent.razor`
  - 本期沖銷驗證（第 298-310 行）
  - 商品明細本次沖款驗證（第 307-318 行）
  - 本期折讓驗證（第 353-365 行）

## �👤 修改人員

GitHub Copilot
