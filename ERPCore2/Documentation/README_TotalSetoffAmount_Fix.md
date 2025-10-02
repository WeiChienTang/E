# 應收帳款沖款總金額與付款明細儲存問題修正

## 問題描述

在修改 `AccountsReceivableSetoffEditModalComponent.razor` 和新增 `SetoffPaymentDetailManagerComponent.razor` 元件之後，發現兩個關鍵問題：

1. **主表 `TotalSetoffAmount` 不會儲存**：明細資料正確儲存，但主表的總金額欄位始終為 0
2. **付款明細資料不會儲存**：`SetoffPaymentDetailManagerComponent` 的資料無法成功儲存到資料庫

## 根本原因分析

### 問題 1：`TotalSetoffAmount` 不儲存

**原因鏈**：

1. `AccountsReceivableSetoffDetailManagerComponent` 計算總金額後，透過 `OnTotalAmountChanged` 事件通知父組件
2. 父組件的 `HandleTotalSetoffAmountChanged` 方法**只更新了本地變數 `totalSetoffAmount`**
3. **沒有同步更新 `editModalComponent.Entity.TotalSetoffAmount`**
4. 因此在主檔第一次儲存時，`Entity.TotalSetoffAmount` 仍然是初始值（0 或舊值）

**相關代碼位置**：
- `AccountsReceivableSetoffEditModalComponent.razor` 第 1089-1103 行

### 問題 2：付款明細不儲存

**原因鏈**：

1. 在 `SaveSetoffDetailsAsync` 方法中調用 `paymentDetailManagerComponent.SaveAsync()`
2. `SaveAsync` 內部使用組件的 `SetoffId` 參數
3. 在**新增模式**下，組件初始化時 `SetoffId` 為 `null`
4. 主檔儲存後雖然有了 `setoff.Id`，但組件的 `SetoffId` 參數**沒有更新**
5. 因此付款明細嘗試用 `null` 或 0 的 SetoffId 儲存，導致失敗

**相關代碼位置**：
- `AccountsReceivableSetoffEditModalComponent.razor` 第 920 行（修正前）
- `SetoffPaymentDetailManagerComponent.razor` 第 838-856 行

## 修正方案

### 修正 1：確保 `TotalSetoffAmount` 正確同步到實體

**位置**：`AccountsReceivableSetoffEditModalComponent.razor` - `HandleTotalSetoffAmountChanged` 方法

```csharp
private async Task HandleTotalSetoffAmountChanged(decimal amount)
{
    try
    {
        totalSetoffAmount = amount;
        
        // 關鍵修正：同時更新表單實體的 TotalSetoffAmount
        // 這樣在主檔儲存時才會包含正確的金額
        if (editModalComponent?.Entity != null)
        {
            editModalComponent.Entity.TotalSetoffAmount = amount;
        }
        
        // 需要 StateHasChanged() 來更新 SetoffPaymentDetailManagerComponent 的 TotalSetoffAmount 參數
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleTotalSetoffAmountChanged), GetType());
    }
}
```

### 修正 2：在載入資料時初始化 `totalSetoffAmount`

**位置**：`AccountsReceivableSetoffEditModalComponent.razor` - `LoadSetoffData` 方法

```csharp
private async Task<AccountsReceivableSetoff?> LoadSetoffData()
{
    try
    {
        if (SetoffId.HasValue && SetoffId.Value > 0)
        {
            var setoff = await AccountsReceivableSetoffService.GetByIdAsync(SetoffId.Value);
            
            // 編輯模式：初始化本地的 totalSetoffAmount 變數
            // 這確保在明細組件載入之前，totalSetoffAmount 就有正確的初始值
            if (setoff != null)
            {
                totalSetoffAmount = setoff.TotalSetoffAmount;
            }
            
            return setoff;
        }
        else
        {
            // 新增模式：生成沖款單號並設置預設公司
            var primaryCompany = await CompanyService.GetPrimaryCompanyAsync();
            var newSetoff = new AccountsReceivableSetoff
            {
                SetoffNumber = await GenerateSetoffNumberAsync(),
                SetoffDate = DateTime.Today,
                CompanyId = primaryCompany?.Id ?? 0,
                TotalSetoffAmount = 0
            };
            
            // 新增模式：初始化 totalSetoffAmount
            totalSetoffAmount = 0;
            
            return newSetoff;
        }
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadSetoffData), GetType(), 
            additionalData: $"載入應收帳款沖款資料失敗 - SetoffId: {SetoffId}");
        return null;
    }
}
```

### 修正 3：在 `AfterSave` 中強制更新主檔並正確儲存付款明細

**位置**：`AccountsReceivableSetoffEditModalComponent.razor` - `SaveSetoffDetailsAsync` 方法

```csharp
// 步驟 1：先確保主檔總金額正確並更新到資料庫
// 這是關鍵步驟，必須在儲存付款明細之前完成
var calculatedTotal = selectedDetails.Sum(d => d.ThisTimeAmount + d.ThisTimeDiscountAmount);
if (Math.Abs(setoff.TotalSetoffAmount - calculatedTotal) > 0.01m)
{
    Console.WriteLine($"[修正] 更新主檔總金額：{setoff.TotalSetoffAmount} -> {calculatedTotal}");
    setoff.TotalSetoffAmount = calculatedTotal;
}

// 強制更新主檔以確保 TotalSetoffAmount 正確儲存
var updateResult = await AccountsReceivableSetoffService.UpdateAsync(setoff);
if (!updateResult.IsSuccess)
{
    await NotificationService.ShowErrorAsync("更新沖款總額失敗", updateResult.ErrorMessage ?? "未知錯誤");
    return;
}

// 步驟 2：創建財務交易記錄
await CreateFinancialTransactionRecordsAsync(setoff, selectedDetails);

// 步驟 3：儲存付款明細（使用正確的 setoff.Id）
if (paymentDetailManagerComponent != null)
{
    // 關鍵修正：確保付款明細使用正確的 SetoffId
    var paymentDetails = paymentDetailManagerComponent.GetPaymentDetails();
    var deletedIds = paymentDetailManagerComponent.GetDeletedDetailIds();
    
    // 直接調用服務層方法，傳入正確的 setoff.Id
    var paymentResult = await SetoffPaymentDetailService.SavePaymentDetailsAsync(
        setoff.Id,  // 使用 setoff.Id 而不是組件的 SetoffId 參數
        paymentDetails,
        deletedIds
    );
    
    if (!paymentResult.Success)
    {
        await NotificationService.ShowErrorAsync("儲存付款明細失敗", paymentResult.Message);
        return;
    }
}
```

## 修正要點總結

### 資料流向修正

**修正前**：
1. 明細組件計算總額 → 通知父組件 → 只更新本地變數
2. 主檔儲存 → Entity.TotalSetoffAmount = 0（未同步）
3. AfterSave → 嘗試更新主檔（但已經太晚）
4. 付款明細儲存 → 使用組件的 SetoffId（新增模式下為 null）

**修正後**：
1. 明細組件計算總額 → 通知父組件 → **同時更新本地變數和 Entity.TotalSetoffAmount**
2. 主檔儲存 → Entity.TotalSetoffAmount 已有正確值
3. AfterSave → **強制再次更新主檔以確保資料一致**
4. 付款明細儲存 → **直接使用 setoff.Id（已儲存的主檔 ID）**

### 關鍵改進

1. **雙重保險機制**：
   - 在 `HandleTotalSetoffAmountChanged` 中立即更新 Entity
   - 在 `SaveSetoffDetailsAsync` 中再次驗證並更新

2. **正確的 ID 傳遞**：
   - 不依賴組件參數的 `SetoffId`
   - 直接使用已儲存實體的 `setoff.Id`

3. **明確的儲存順序**：
   - 步驟 1：更新主檔總金額
   - 步驟 2：創建財務交易記錄
   - 步驟 3：儲存付款明細

## 測試驗證

### 測試案例 1：新增沖款單

1. 選擇客戶
2. 選擇沖款明細並輸入金額
3. 輸入付款明細
4. 儲存
5. **驗證**：
   - 主表 `TotalSetoffAmount` 應等於明細總額
   - 付款明細應正確儲存到資料庫

### 測試案例 2：編輯現有沖款單

1. 開啟現有沖款單
2. 修改明細金額
3. 修改付款明細
4. 儲存
5. **驗證**：
   - 主表 `TotalSetoffAmount` 應反映新的總額
   - 付款明細應正確更新

### 測試案例 3：刪除明細

1. 開啟現有沖款單
2. 刪除部分明細
3. 刪除部分付款明細
4. 儲存
5. **驗證**：
   - 主表 `TotalSetoffAmount` 應減少
   - 已刪除的付款明細應從資料庫移除

## 相關文件

- [應收帳款沖款明細管理組件](../Components/Shared/SubCollections/AccountsReceivableSetoffDetailManagerComponent.razor)
- [應收帳款沖款付款明細管理組件](../Components/Shared/SubCollections/SetoffPaymentDetailManagerComponent.razor)
- [應收帳款沖款編輯組件](../Components/Pages/FinancialManagement/AccountsReceivableSetoffEditModalComponent.razor)

## 修正日期

2025年10月2日

## 修正影響範圍

- ✅ 主表總金額儲存
- ✅ 付款明細儲存
- ✅ 新增模式
- ✅ 編輯模式
- ✅ 資料一致性
