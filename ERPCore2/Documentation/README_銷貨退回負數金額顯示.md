# 銷貨退回負數金額顯示功能

## 問題描述

在應收沖款功能中，銷貨退回（公司應付給客戶的退款）的金額處理存在以下問題：

1. **資料庫儲存**：`ReturnSubtotalAmount` 本身是負數（表示應付給客戶）
2. **Service 層處理**：使用 `Math.Abs()` 將負數轉為正數顯示
3. **UI 顯示問題**：待沖款金額顯示為 0，使用者無法清楚看到實際應退款金額
4. **業務邏輯混淆**：正數和負數都顯示為正數，無法區分「應收」和「應付」

## 解決方案

採用**保持負數顯示**的方案，讓金額的正負號清楚表達業務意義：

### 業務邏輯定義

- **正數**：客戶欠公司錢（應收帳款，例如銷貨訂單）
- **負數**：公司欠客戶錢（應付給客戶，例如銷貨退回）

### 修改內容

#### 1. Service 層修改

**檔案**：`AccountsReceivableSetoffDetailService.cs`

- 移除對 `ReturnSubtotalAmount` 的 `Math.Abs()` 處理
- 保留原始負數值，讓金額的正負號表達業務意義
- 調整待沖款過濾條件從 `>= 0` 改為 `!= 0`

```csharp
// 修改前
var pendingAmount = detail.ReturnSubtotalAmount - detail.TotalPaidAmount - discountedAmount;
if (pendingAmount >= 0) { ... }

// 修改後
// 銷貨退回的 ReturnSubtotalAmount 為負數（表示應付給客戶）
var pendingAmount = detail.ReturnSubtotalAmount - detail.TotalPaidAmount - discountedAmount;
// 保留所有未結清的退款項目（pendingAmount 為負數表示尚未完全退款給客戶）
if (pendingAmount != 0) { ... }
```

#### 2. UI 顯示修改

**檔案**：`SetoffDetailManagerComponent.razor`

##### 欄位顯示調整

所有金額欄位（總金額、已沖款、已折讓、待沖款）增加負數處理：

```csharp
// 負數使用括號顯示，例如：(1,000) 表示 -1000
var displayValue = amount < 0 
    ? $"({Math.Abs(amount):N0})" 
    : amount.ToString("N0");
```

##### 顏色標示

- **待沖款欄位**：
  - 正數（應收）：紅色 `text-danger`
  - 負數（應付）：藍色加粗 `text-primary fw-bold`
  
- **總金額欄位**：
  - 負數：紅色 `text-danger`

##### 輸入欄位

支援負數輸入，使用括號格式：

```csharp
// 使用者可以輸入：
// - 正數：直接輸入 "1000"
// - 負數：使用括號 "(1000)" 或減號 "-1000"

// 顯示：
// - 正數：1,000
// - 負數：(1,000)
```

#### 3. 輸入處理修改

**檔案**：`SetoffDetailManagerComponent.razor`

##### HandleAmountChanged 和 HandleDiscountAmountChanged

支援負數輸入和驗證：

```csharp
// 處理括號格式的負數輸入，例如 "(1000)" -> -1000
var cleanValue = args.value.Trim();
bool isNegative = false;

if (cleanValue.StartsWith("(") && cleanValue.EndsWith(")"))
{
    isNegative = true;
    cleanValue = cleanValue.Substring(1, cleanValue.Length - 2);
}
else if (cleanValue.StartsWith("-"))
{
    isNegative = true;
    cleanValue = cleanValue.Substring(1);
}

// 移除千分位符號
cleanValue = cleanValue.Replace(",", "");

if (decimal.TryParse(cleanValue, out var amount))
{
    if (isNegative) amount = -amount;
    
    // 根據待沖款的正負號來限制輸入
    if (maxAmount >= 0)
    {
        // 正常應收：限制在 0 到 maxAmount 之間
        args.detail.ThisTimeAmount = Math.Max(0, Math.Min(amount, maxAmount));
    }
    else
    {
        // 銷貨退回（應付給客戶）：限制在 maxAmount 到 0 之間
        args.detail.ThisTimeAmount = Math.Min(0, Math.Max(amount, maxAmount));
    }
}
```

#### 4. 驗證邏輯修改

**檔案**：`SetoffDetailDto.cs`

更新驗證方法 `ValidateThisTimeAmount`、`ValidateThisTimeDiscountAmount` 和 `ValidateTotalThisTimeAmount`：

```csharp
public (bool IsValid, string? ErrorMessage) ValidateThisTimeAmount()
{
    var pendingForValidation = PendingAmountForValidation;
    
    // 根據待沖款的正負號進行驗證
    if (pendingForValidation >= 0)
    {
        // 正常應收（例如銷貨訂單）：沖款金額必須為正數且不超過待沖款
        if (ThisTimeAmount < 0)
        {
            return (false, "沖款金額不能為負數");
        }
        if (ThisTimeAmount > pendingForValidation)
        {
            return (false, $"沖款金額不能超過待沖款金額 {pendingForValidation:N2}");
        }
    }
    else
    {
        // 銷貨退回（應付給客戶）：沖款金額必須為負數且不小於待沖款
        if (ThisTimeAmount > 0)
        {
            return (false, "退款金額必須為負數（使用括號輸入，例如 (1000)）");
        }
        if (ThisTimeAmount < pendingForValidation)
        {
            return (false, $"退款金額不能超過待退款金額 ({Math.Abs(pendingForValidation):N2})");
        }
    }

    return (true, null);
}
```

## 使用說明

### 銷貨訂單（應收帳款）

1. **顯示**：
   - 待沖款：5,000（紅色）
   - 本次沖款：輸入正數，例如 "3000"

2. **結果**：
   - 扣款 3,000 元
   - 剩餘待沖款：2,000

### 銷貨退回（應付給客戶）

1. **顯示**：
   - 待沖款：(5,000)（藍色加粗，表示應付給客戶）
   - 本次沖款：輸入負數，例如 "(3000)" 或 "-3000"

2. **結果**：
   - 退款 3,000 元給客戶
   - 剩餘待退款：(2,000)

## 優點

1. **清晰的業務語義**：
   - 正數 = 應收
   - 負數 = 應付
   
2. **自動抵銷**：
   - 在計算總沖款金額時，正數和負數會自動抵銷
   - 例如：銷貨 10,000 + 退貨 -3,000 = 實際應收 7,000

3. **符合會計慣例**：
   - 會計分錄中負數表示反向交易
   - 財務報表中退款顯示為負數

4. **直觀的視覺提示**：
   - 不同顏色標示正負金額
   - 括號表示負數，清楚易懂

## 測試項目

- [ ] 新增模式：載入銷貨退回，待沖款顯示負數並用括號和藍色顯示
- [ ] 新增模式：輸入負數沖款金額（使用括號或減號）
- [ ] 新增模式：驗證負數金額不能超過待退款金額
- [ ] 編輯模式：載入已有退款記錄，正確顯示負數
- [ ] 編輯模式：修改退款金額，驗證正確性
- [ ] 混合場景：同時有銷貨訂單（正數）和銷貨退回（負數），總金額計算正確
- [ ] 驗證：正數訂單不能輸入負數，負數退款不能輸入正數

## 相關文件

- [應收沖款明細管理組件](README_應收沖款明細管理組件.md)
- [應收帳款折讓與財務表](README_應收帳款_折讓與財務表.md)
- [沖款明細服務](README_Services.md)

## 變更歷史

- 2025-10-02：初始版本，實作銷貨退回負數金額顯示功能
