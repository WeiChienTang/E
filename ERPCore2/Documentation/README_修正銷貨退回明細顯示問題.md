# 修正銷貨退回明細無法顯示問題

## 問題描述

在應收沖款功能中，即使 `IsSettled = false` 的銷貨退回明細也無法顯示。

## 根本原因

Service 層在過濾明細時使用了額外的 `pendingAmount` 檢查：

```csharp
// 錯誤的邏輯
var pendingAmount = detail.ReturnSubtotalAmount - detail.TotalPaidAmount - discountedAmount;
if (pendingAmount != 0)  // 如果 pendingAmount = 0，資料會被過濾掉
{
    // 加入結果
}
```

這導致以下問題：
1. 如果 `ReturnSubtotalAmount` 為 0（可能是資料尚未正確計算），`pendingAmount` 就會是 0
2. 即使 `IsSettled = false`，資料也會被 `if (pendingAmount != 0)` 條件過濾掉
3. 使用者要求的邏輯是：**只要 IsSettled = false 就應該顯示，不管金額是多少**

## 修正內容

### 1. 前端組件 (SetoffDetailManagerComponent.razor)

**修改 `SelectedDetails` 屬性**：
```csharp
// 修改前
Details.Where(d => d.ThisTimeAmount > 0 || d.ThisTimeDiscountAmount > 0)

// 修改後（支援負數）
Details.Where(d => d.ThisTimeAmount != 0 || d.ThisTimeDiscountAmount != 0)
```

**修改 `GetDisplayDetails()` 方法**：
```csharp
// 修改前
return Details;  // 顯示所有資料

// 修改後（只顯示未結清）
return Details.Where(d => !d.IsSettled).ToList();
```

**修改編輯模式的過濾條件**：
```csharp
// 修改前
Details.Where(d => d.OriginalThisTimeAmount > 0 || d.OriginalThisTimeDiscountAmount > 0)

// 修改後（支援負數）
Details.Where(d => d.OriginalThisTimeAmount != 0 || d.OriginalThisTimeDiscountAmount != 0)
```

### 2. 應收帳款 Service (AccountsReceivableSetoffDetailService.cs)

**修改 `GetCustomerPendingDetailsAsync` 方法**：

#### 銷貨訂單明細
```csharp
// 修改前
var pendingAmount = detail.SubtotalAmount - detail.TotalReceivedAmount - discountedAmount;
if (pendingAmount > 0)
{
    result.Add(...);
}

// 修改後（移除 pendingAmount 檢查）
// 只要 IsSettled = false 就顯示，不管金額是多少
// 因為已經在 WHERE 條件中過濾了 !sod.IsSettled，所以這裡直接加入
result.Add(...);
```

#### 銷貨退回明細
```csharp
// 修改前
var pendingAmount = detail.ReturnSubtotalAmount - detail.TotalPaidAmount - discountedAmount;
if (pendingAmount != 0)
{
    result.Add(...);
}

// 修改後（移除 pendingAmount 檢查）
// 只要 IsSettled = false 就顯示，不管金額是多少
// 因為已經在 WHERE 條件中過濾了 !srd.IsSettled，所以這裡直接加入
result.Add(...);
```

### 3. 應付帳款 Service (AccountsPayableSetoffDetailService.cs)

**修改 `GetSupplierPendingDetailsAsync` 方法**：

#### 採購進貨明細
```csharp
// 修改前
var pendingAmount = totalAmount - settledAmount - discountedAmount;
if (pendingAmount > 0)
{
    result.Add(...);
}

// 修改後（移除 pendingAmount 檢查）
// 只要 IsSettled = false 就顯示，不管金額是多少
result.Add(...);
```

#### 採購退回明細
```csharp
// 修改前
var pendingAmount = totalAmount - settledAmount - discountedAmount;
if (pendingAmount > 0)
{
    result.Add(...);
}

// 修改後（移除 pendingAmount 檢查）
// 只要 IsSettled = false 就顯示，不管金額是多少
result.Add(...);
```

## 測試步驟

### 1. 執行診斷 SQL

執行 `check_salesreturn_data.sql` 來檢查資料狀態：

```sql
-- 檢查 ID=2 的銷貨退回明細
-- 查看 ReturnSubtotalAmount 的值
-- 確認 IsSettled = 0
```

### 2. 測試新增模式

1. 開啟應收沖款頁面
2. 選擇一個客戶（該客戶有未結清的銷貨退回記錄）
3. 確認銷貨退回明細顯示在列表中
4. 檢查以下欄位：
   - 單據編號
   - 產品名稱
   - 退回數量
   - 退回單價
   - 總金額（可能是負數或 0）
   - 是否結清（應該是 false）

### 3. 測試編輯模式

1. 開啟一個現有的沖款單
2. 確認原本有退款記錄的銷貨退回明細顯示出來
3. 可以修改退款金額

### 4. 測試資料過濾

確認以下項目：
- ✅ `IsSettled = false` 的記錄都會顯示
- ✅ `IsSettled = true` 的記錄不會顯示（除非在編輯模式且原本有沖款）
- ✅ 金額為 0 的記錄也會顯示（只要 `IsSettled = false`）
- ✅ 負數金額的記錄會顯示（銷貨退回）
- ✅ 正數金額的記錄會顯示（銷貨訂單）

## 可能的後續問題

### 問題 1: ReturnSubtotalAmount 為 0

如果發現 `ReturnSubtotalAmount` 欄位值為 0，需要：

1. **檢查計算邏輯**：確認 `SalesReturnDetailService.CalculateSubtotal` 是否正確
2. **更新現有資料**：執行 SQL 更新來重新計算小計金額

```sql
UPDATE SalesReturnDetails
SET ReturnSubtotalAmount = (ReturnQuantity * ReturnUnitPrice) - DiscountAmount
WHERE ReturnSubtotalAmount = 0 AND IsSettled = 0;
```

### 問題 2: 負數金額處理

如果需要支援負數金額（表示應付給客戶），確認：

1. `CalculateSubtotal` 方法不應使用 `Math.Max(0, subtotal)`
2. UI 顯示應該正確處理負數（使用括號或紅色標示）

## 修改檔案清單

- ✅ `SetoffDetailManagerComponent.razor` - 前端過濾邏輯
- ✅ `AccountsReceivableSetoffDetailService.cs` - 應收帳款 Service
- ✅ `AccountsPayableSetoffDetailService.cs` - 應付帳款 Service
- 📝 `check_salesreturn_data.sql` - 診斷查詢（新增）

## 相關文件

- [銷貨退回負數金額顯示](README_銷貨退回負數金額顯示.md)
- [應收沖款明細管理組件](README_應收沖款明細管理組件.md)

## 修改日期

2025-01-02
