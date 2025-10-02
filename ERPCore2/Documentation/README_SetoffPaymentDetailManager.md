# 付款明細管理組件使用說明

## 📋 功能概述

`SetoffPaymentDetailManagerComponent` 是用於管理應收帳款沖款單的付款明細的組件，支援多種付款方式的混合使用。

## 🎯 主要特性

- ✅ 支援多筆付款方式明細
- ✅ 自動驗證付款總額是否符合沖款總額
- ✅ 條件顯示銀行欄位（根據付款方式判斷）
- ✅ 自動空行管理（AutoEmptyRow）
- ✅ 即時金額計算與驗證
- ✅ 使用 InteractiveTableComponent 統一 UI

## 📦 資料結構

### 新增的資料表

#### `AccountsReceivableSetoffPaymentDetail` (付款明細表)

| 欄位名稱 | 類型 | 說明 | 必填 |
|---------|------|------|------|
| Id | int | 主鍵 | ✅ |
| SetoffId | int | 沖款單ID (FK) | ✅ |
| PaymentMethodId | int | 付款方式ID (FK) | ✅ |
| BankId | int? | 銀行ID (FK) | ❌ |
| Amount | decimal(18,2) | 付款金額 | ✅ |
| AccountNumber | nvarchar(100) | 帳號/票號 | ❌ |
| TransactionReference | nvarchar(100) | 交易參考號 | ❌ |
| PaymentDate | datetime2 | 付款日期 | ❌ |
| Remarks | nvarchar(500) | 備註 | ❌ |

### 修改的資料表

#### `AccountsReceivableSetoff` (沖款單主檔)

新增導航屬性:
```csharp
public ICollection<AccountsReceivableSetoffPaymentDetail> PaymentDetails { get; set; }
```

## 🔧 使用方式

### 1. 在頁面中引用組件

```razor
@using ERPCore2.Components.Shared.SubCollections
@using ERPCore2.Models

<SetoffPaymentDetailManagerComponent @ref="paymentDetailManager"
                                     SetoffId="@Model.Id"
                                     TotalSetoffAmount="@totalSetoffAmount"
                                     OnPaymentDetailsChanged="@HandlePaymentDetailsChanged"
                                     OnTotalPaymentAmountChanged="@HandleTotalPaymentAmountChanged"
                                     IsReadOnly="@isReadOnly"
                                     IsEditMode="@isEditMode" />

@code {
    private SetoffPaymentDetailManagerComponent? paymentDetailManager;
    private decimal totalSetoffAmount = 50000m;
    private bool isReadOnly = false;
    private bool isEditMode = false;
    
    private async Task HandlePaymentDetailsChanged(List<SetoffPaymentDetailDto> details)
    {
        // 處理付款明細變更
        Console.WriteLine($"付款明細數量: {details.Count}");
    }
    
    private async Task HandleTotalPaymentAmountChanged(decimal totalPayment)
    {
        // 處理付款總額變更
        Console.WriteLine($"付款總額: {totalPayment:N2}");
    }
}
```

### 2. 儲存付款明細

```csharp
private async Task SavePaymentDetails()
{
    if (paymentDetailManager != null)
    {
        // 方法 1: 使用組件內建的儲存方法
        var (success, message) = await paymentDetailManager.SaveAsync();
        
        if (success)
        {
            await NotificationService.ShowSuccessAsync("付款明細儲存成功");
        }
        else
        {
            await NotificationService.ShowErrorAsync(message);
        }
        
        // 方法 2: 手動取得明細並儲存
        var details = paymentDetailManager.GetPaymentDetails();
        var deletedIds = paymentDetailManager.GetDeletedDetailIds();
        
        var result = await SetoffPaymentDetailService.SavePaymentDetailsAsync(
            setoffId, 
            details, 
            deletedIds
        );
    }
}
```

### 3. 驗證付款明細

```csharp
private async Task ValidatePaymentDetails()
{
    if (paymentDetailManager != null)
    {
        var (isValid, errors) = paymentDetailManager.ValidatePaymentDetails();
        
        if (!isValid)
        {
            foreach (var error in errors)
            {
                await NotificationService.ShowErrorAsync(error);
            }
        }
    }
}
```

### 4. 刷新資料

```csharp
private async Task RefreshPaymentDetails()
{
    if (paymentDetailManager != null)
    {
        await paymentDetailManager.RefreshAsync();
    }
}
```

## 📊 業務邏輯

### 付款方式判斷是否需要銀行

系統會自動判斷付款方式名稱中是否包含以下關鍵字:
- 匯款
- 轉帳
- 支票
- ATM
- 銀行

如果包含任一關鍵字，則會顯示銀行選擇欄位。

### 金額驗證規則

1. **個別金額驗證**:
   - 付款金額必須 > 0
   - 付款金額不可超過沖款總額

2. **總額驗證**:
   - 所有付款明細的金額總和必須等於沖款總額
   - 系統會即時顯示差額提示

### 自動空行管理

- 當最後一行被填寫資料時，自動新增一個空行
- 確保始終有一個空行可供新增
- 刪除多餘的空行（保留一個）

## 💡 使用範例

### 範例 1: 多種付款方式混合

```
沖款總額: 50,000 元

付款明細:
┌──────────┬────────────┬──────────┬──────────────┬──────────────┐
│ 付款方式 │    銀行    │ 付款金額 │   帳號/票號  │ 交易參考號   │
├──────────┼────────────┼──────────┼──────────────┼──────────────┤
│ 現金     │     -      │  15,000  │      -       │      -       │
│ 匯款     │  台灣銀行  │  25,000  │ 123-456-789  │  TXN2025001  │
│ 支票     │  玉山銀行  │  10,000  │ CK20250101   │      -       │
└──────────┴────────────┴──────────┴──────────────┴──────────────┘

付款總額: 50,000 元 ✅ 符合沖款總額
```

### 範例 2: 單一付款方式

```
沖款總額: 30,000 元

付款明細:
┌──────────┬────────────┬──────────┐
│ 付款方式 │    銀行    │ 付款金額 │
├──────────┼────────────┼──────────┤
│ 現金     │     -      │  30,000  │
└──────────┴────────────┴──────────┘

付款總額: 30,000 元 ✅ 符合沖款總額
```

## ⚠️ 注意事項

1. **付款總額驗證**:
   - 儲存前必須確保付款總額 = 沖款總額
   - 系統會自動顯示差額警告

2. **銀行選擇**:
   - 選擇需要銀行的付款方式時，必須同時選擇銀行
   - 系統會自動驗證

3. **刪除功能**:
   - 刪除已儲存的明細時，ID 會被記錄到 DeletedDetailIds
   - 儲存時會實際從資料庫刪除

4. **編輯模式**:
   - IsEditMode = true 時，會載入現有的付款明細
   - IsEditMode = false 時，為新增模式

## 🔌 相關 Service

### ISetoffPaymentDetailService

```csharp
public interface ISetoffPaymentDetailService
{
    // 取得付款明細
    Task<List<SetoffPaymentDetailDto>> GetBySetoffIdAsync(int setoffId);
    
    // 批次儲存
    Task<(bool Success, string Message)> SavePaymentDetailsAsync(
        int setoffId, 
        List<SetoffPaymentDetailDto> details, 
        List<int> deletedIds);
    
    // 驗證
    Task<(bool IsValid, string? ErrorMessage)> ValidatePaymentDetailsAsync(
        int setoffId,
        List<SetoffPaymentDetailDto> details, 
        decimal totalSetoffAmount);
    
    // 計算總額
    Task<decimal> CalculateTotalPaymentAmountAsync(int setoffId);
    
    // 刪除
    Task<bool> DeleteBySetoffIdAsync(int setoffId);
}
```

## 📝 Migration 資訊

Migration 名稱: `AddSetoffPaymentDetail`

執行指令:
```bash
dotnet ef migrations add AddSetoffPaymentDetail
dotnet ef database update
```

建立的內容:
- 新增 `AccountsReceivableSetoffPaymentDetails` 資料表
- 新增 3 個索引 (SetoffId, PaymentMethodId, BankId)
- 新增外鍵約束到 AccountsReceivableSetoffs, PaymentMethods, Banks

## 🎉 完成項目

- ✅ Entity 定義
- ✅ DTO 定義與驗證
- ✅ Service 介面與實作
- ✅ 組件開發
- ✅ Migration 執行
- ✅ 資料庫更新成功

---

**建立日期**: 2025年10月1日  
**版本**: 1.0.0  
**作者**: GitHub Copilot
