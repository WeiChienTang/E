# 應付帳款沖款付款明細服務實作

## 概述
本次實作為 `AccountsPayableSetoffPaymentDetail` 實體建立了完整的服務層，參考了 `AccountsReceivableSetoffPaymentDetail` (應收帳款沖款付款明細) 的服務邏輯。

## 建立的檔案

### 1. 服務介面
**檔案位置：** `Services/FinancialManagement/IAccountsPayableSetoffPaymentDetailService.cs`

定義了以下介面方法：
- `GetBySetoffIdAsync(int setoffId)` - 依據沖款單ID取得付款明細列表
- `SavePaymentDetailsAsync(int setoffId, List<SetoffPaymentDetailDto> details, List<int> deletedIds)` - 批次儲存付款明細
- `ValidatePaymentDetailsAsync(int setoffId, List<SetoffPaymentDetailDto> details, decimal totalSetoffAmount)` - 驗證付款明細總額是否符合沖款總額
- `CalculateTotalPaymentAmountAsync(int setoffId)` - 計算付款明細總額
- `DeleteBySetoffIdAsync(int setoffId)` - 刪除指定沖款單的所有付款明細

### 2. 服務實作
**檔案位置：** `Services/FinancialManagement/AccountsPayableSetoffPaymentDetailService.cs`

實作內容包括：

#### 繼承與建構子
- 繼承 `GenericManagementService<AccountsPayableSetoffPaymentDetail>`
- 實作 `IAccountsPayableSetoffPaymentDetailService` 介面
- 提供兩個建構子：簡易版（僅 contextFactory）和完整版（含 logger）

#### 覆寫的基底方法
- `GetAllAsync()` - 取得所有付款明細（包含相關導航屬性）
- `GetByIdAsync(int id)` - 依ID取得單筆付款明細
- `SearchAsync(string searchTerm)` - 搜尋付款明細
- `ValidateAsync(AccountsPayableSetoffPaymentDetail entity)` - 驗證付款明細

#### 核心功能實作
1. **批次儲存付款明細** (`SavePaymentDetailsAsync`)
   - 使用資料庫交易確保資料一致性
   - 處理刪除、新增、更新操作
   - 過濾有效明細（必須有付款方式且金額 > 0）

2. **驗證付款明細** (`ValidatePaymentDetailsAsync`)
   - 驗證至少有一筆付款明細
   - 驗證付款總額是否符合沖款總額
   - 驗證每筆明細的有效性

3. **輔助功能**
   - 計算付款總額
   - 依沖款單ID取得明細
   - 刪除沖款單的所有明細
   - 判斷付款方式是否需要銀行資訊

## 資料庫上下文更新

### 修改的檔案
**檔案位置：** `Data/Context/AppDbContext.cs`

新增 DbSet：
```csharp
public DbSet<AccountsPayableSetoffPaymentDetail> AccountsPayableSetoffPaymentDetails { get; set; }
```

## 依賴注入設定

### 修改的檔案
**檔案位置：** `Data/ServiceRegistration.cs`

在財務管理服務區塊新增服務註冊：
```csharp
services.AddScoped<IAccountsPayableSetoffPaymentDetailService, AccountsPayableSetoffPaymentDetailService>();
```

## 符合的開發規範

### ✅ 遵循 README_Services.md 規範
1. **繼承結構**
   - 正確繼承 `GenericManagementService<T>`
   - 未重複宣告基底類別已提供的欄位

2. **建構子設計**
   - 提供簡易版和完整版建構子
   - 正確調用基底建構子 `base(contextFactory, logger)`

3. **錯誤處理**
   - 所有公開方法都有 try-catch
   - 使用 `ErrorHandlingHelper.HandleServiceErrorAsync()` 統一處理錯誤
   - 傳入詳細的錯誤上下文資訊

4. **安全回傳值**
   - `List<T>` 回傳空列表 `new List<T>()`
   - `T?` 回傳 `null`
   - `bool` 回傳 `false`
   - `decimal` 回傳 `0`

5. **資料庫操作**
   - 使用 `using var context = await _contextFactory.CreateDbContextAsync();`
   - 在交易中處理批次操作
   - 正確使用 Include 載入導航屬性

6. **驗證邏輯**
   - 實作必要的 `ValidateAsync()` 方法
   - 實作必要的 `SearchAsync()` 方法
   - 提供業務邏輯驗證（金額、必填欄位、總額檢查）

## 與應收帳款服務的差異

雖然邏輯相似，但兩個服務處理不同的實體：
- **應收帳款沖款付款明細** (`AccountsReceivableSetoffPaymentDetail`) - 處理客戶付款給公司
- **應付帳款沖款付款明細** (`AccountsPayableSetoffPaymentDetail`) - 處理公司付款給廠商

主要差異在於：
- 實體類型不同
- DbContext 中的 DbSet 名稱不同
- 服務類別名稱不同

業務邏輯完全一致，確保兩種付款明細的處理方式一致。

## 測試建議

1. **單元測試**
   - 測試付款明細的新增、更新、刪除
   - 測試金額驗證邏輯
   - 測試付款總額與沖款總額的驗證

2. **整合測試**
   - 測試與沖款單的關聯
   - 測試交易的回滾機制
   - 測試批次儲存的完整流程

3. **邊界測試**
   - 測試空明細列表
   - 測試金額為 0 的情況
   - 測試付款總額不符的情況

## 後續使用

在需要處理應付帳款沖款付款明細的地方，可以注入 `IAccountsPayableSetoffPaymentDetailService` 服務：

```csharp
@inject IAccountsPayableSetoffPaymentDetailService PaymentDetailService

// 使用範例
var details = await PaymentDetailService.GetBySetoffIdAsync(setoffId);
var (success, message) = await PaymentDetailService.SavePaymentDetailsAsync(setoffId, details, deletedIds);
var (isValid, errorMessage) = await PaymentDetailService.ValidatePaymentDetailsAsync(setoffId, details, totalAmount);
```

## 總結

本次實作完全遵循專案的服務層開發規範，提供了完整的應付帳款沖款付款明細管理功能，確保與應收帳款的處理邏輯一致，便於維護和擴展。
