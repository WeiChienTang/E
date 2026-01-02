# 選取廠商或客戶對於 Table 產生的變化 - 修改紀錄

## 問題描述

### 原始問題
在 `SalesReturnEditModalComponent` 中選擇客戶時，發現以下問題：
1. **過度渲染**：`OnParametersSetAsync` 被觸發 10+ 次
2. **空行消失**：選擇客戶後，退貨明細表格的空行會消失（`ReturnItems.Count = 0`）
3. **警告訊息閃爍**：畫面會先顯示「尚未新增退貨商品」警告，然後才出現空行

### 根本原因分析

#### 問題 1：過度渲染的原因
在 `SalesReturnEditModalComponent.OnFieldValueChanged` 中，客戶變更時會：
```csharp
// ❌ 舊的錯誤做法
await UpdateSalesOrderOptions(customerId);      // 修改 formFields.Options
await UpdateFilterProductOptions(customerId);   // 再次修改 formFields.Options
StateHasChanged();                               // 又觸發一次渲染
```

**問題**：每次修改 `formFields` 的 `Options` 都會被 GenericEditModalComponent 視為「參數變更」，導致：
- GenericEditModalComponent 重新渲染
- 傳遞新參數給 SalesReturnTable
- SalesReturnTable.OnParametersSetAsync 被觸發
- 重複 10+ 次

#### 問題 2：空行消失的原因
在 `SalesReturnTable.OnParametersSetAsync` 中，客戶變更時會：
```csharp
// ❌ 舊的錯誤做法
ReturnItems.Clear();                    // 清空所有項目（包括空行）
_dataLoadCompleted = false;             // 設為 false
await InvokeAsync(async () =>
{
    await Task.Delay(1);
    _dataLoadCompleted = true;          // 設為 true
});
```

**問題**：
1. `_dataLoadCompleted` 從 `false → true` 會觸發 `InteractiveTable.OnAfterRenderAsync` 偵測到變化
2. 然後新增空行
3. 但這又觸發 `StateHasChanged()`
4. 導致父組件重新渲染
5. 進入無限循環

## 解決方案

### 參考標準：SalesOrderEditModalComponent 的做法

檢查 `SalesOrderEditModalComponent` 和 `SalesOrderTable` 發現它們的做法非常簡潔：

#### SalesOrderEditModalComponent
```csharp
private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    if (fieldChange.PropertyName == nameof(SalesOrder.CustomerId))
    {
        // ✅ 只更新 ActionButtons（如果需要）
        if (!hasUndeletableDetails)
        {
            await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                customerModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
        }
        
        // ✅ 只觸發一次渲染
        StateHasChanged();
    }
}
```

**關鍵特點**：
- ❌ **不修改** `formFields` 的 `Options`
- ✅ 只更新 `ActionButtons`
- ✅ 只呼叫一次 `StateHasChanged()`

#### SalesOrderTable
```csharp
protected override async Task OnParametersSetAsync()
{
    bool customerChanged = _previousSelectedCustomerId != SelectedCustomerId;
    
    if (customerChanged && !existingDetailsChanged)
    {
        _previousSelectedCustomerId = SelectedCustomerId;
        
        await LoadAvailableProductsAsync();
        
        // ✅ 直接清空並重新載入
        SalesItems.Clear();
        await LoadExistingDetailsAsync();
        return;
    }
}
```

**關鍵特點**：
- ✅ 直接清空並重新載入
- ❌ **不使用** `InvokeAsync` 或 `Task.Delay`
- ❌ **不手動控制** `_dataLoadCompleted` 的 `false → true` 轉換

### 修改內容

#### 1. SalesReturnEditModalComponent.razor

**修改前**：
```csharp
case nameof(SalesReturn.CustomerId):
    if (int.TryParse(fieldChange.Value?.ToString(), out var customerId))
    {
        // 準備所有選項數據
        var filteredSalesOrders = salesOrders
            .Where(so => so.CustomerId == customerId && !IsOrderFullyReturned(so))
            .Select(so => new SelectOption { Text = so.Code ?? string.Empty, Value = so.Id.ToString() })
            .ToList();
        
        var filteredProducts = products
            .Select(p => new SelectOption { Value = p.Id.ToString(), Text = $"{p.Code} - {p.Name}" })
            .ToList();
        
        // 統一更新 formFields（一次性修改，避免多次觸發變更偵測）
        var salesOrderField = formFields.FirstOrDefault(f => f.PropertyName == nameof(SalesReturn.SalesOrderId));
        var filterProductField = formFields.FirstOrDefault(f => f.PropertyName == "FilterProductId");
        
        if (salesOrderField != null)
            salesOrderField.Options = filteredSalesOrders;
        if (filterProductField != null)
            filterProductField.Options = filteredProducts;
        
        // 清空篩選選擇
        filterProductId = null;
        
        // 只有在沒有不可刪除明細時才更新 ActionButtons
        if (!hasUndeletableDetails)
        {
            await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                customerModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
        }
        
        // 清除現有的銷售單選擇
        if (editModalComponent?.Entity != null)
        {
            editModalComponent.Entity.SalesOrderId = null;
        }
        
        // 所有數據都準備好後，統一觸發一次渲染
        StateHasChanged();
    }
    break;
```

**修改後**：
```csharp
case nameof(SalesReturn.CustomerId):
    if (int.TryParse(fieldChange.Value?.ToString(), out var customerId))
    {
        // 只有在沒有不可刪除明細時才更新 ActionButtons
        if (!hasUndeletableDetails)
        {
            await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                customerModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
        }
        
        // 客戶變更時，觸發退貨明細管理器重新渲染
        StateHasChanged();
    }
    break;
```

**改進說明**：
- ❌ **移除**所有 `formFields.Options` 的修改
- ❌ **移除** `UpdateSalesOrderOptions()` 和 `UpdateFilterProductOptions()` 的呼叫
- ✅ 簡化為只更新 `ActionButtons` 和觸發一次 `StateHasChanged()`
- ✅ 與 `SalesOrderEditModalComponent` 保持一致

#### 2. SalesReturnTable.razor

**修改前**：
```csharp
// 如果客戶變更，需要重新載入所有資料
if (customerChanged)
{
    ConsoleHelper.WriteInfo($"[SalesReturnTable] 客戶變更, CustomerId: {EffectiveCustomerId}");
    _previousSelectedCustomerId = EffectiveCustomerId;
    _previousSelectedSalesOrderId = EffectiveSalesOrderId;
    _previousFilterProductId = EffectiveFilterProductId;
    
    await LoadAvailableProductsAsync();
    
    // 客戶變更時清空現有選項並重新載入
    ReturnItems.Clear();
    
    // 判斷是否有現有明細，確保 _dataLoadCompleted 正確設定
    if (ExistingReturnDetails?.Any() != true)
    {
        ConsoleHelper.WriteWarning("[SalesReturnTable] 新增模式：無現有明細");
        // 新增模式：沒有現有明細
        // 關鍵修復：設置標記但不立即渲染，讓 OnAfterRenderAsync 處理
        _dataLoadCompleted = false;
        // 延遲設置完成標記，確保 OnAfterRenderAsync 能偵測到 false→true 轉換
        await InvokeAsync(async () =>
        {
            await Task.Delay(1); // 極短延遲確保渲染週期完成
            _dataLoadCompleted = true;
            // 不要在這裡 StateHasChanged，讓 OnAfterRenderAsync 自動觸發
            ConsoleHelper.WriteSuccess($"[SalesReturnTable] 新增模式設置完成標記");
        });
    }
    else
    {
        ConsoleHelper.WriteInfo($"[SalesReturnTable] 編輯模式：載入 {ExistingReturnDetails.Count} 筆現有明細");
        // 編輯模式：有現有明細，呼叫載入方法（內部會設定 _dataLoadCompleted = true）
        await LoadExistingDetailsAsync();
        ConsoleHelper.WriteSuccess($"[SalesReturnTable] 編輯模式完成, ReturnItems 數量: {ReturnItems.Count}");
    }
    return;
}
```

**修改後**：
```csharp
// 如果客戶變更，需要重新載入所有資料
if (customerChanged)
{
    ConsoleHelper.WriteInfo($"[SalesReturnTable] 客戶變更, CustomerId: {EffectiveCustomerId}");
    _previousSelectedCustomerId = EffectiveCustomerId;
    _previousSelectedSalesOrderId = EffectiveSalesOrderId;
    _previousFilterProductId = EffectiveFilterProductId;
    
    await LoadAvailableProductsAsync();
    
    // 客戶變更時清空現有選項並重新載入
    ReturnItems.Clear();
    await LoadExistingDetailsAsync();
    return;
}
```

**改進說明**：
- ❌ **移除** `InvokeAsync` 和 `Task.Delay` 的複雜邏輯
- ❌ **移除**手動控制 `_dataLoadCompleted` 的 `false → true` 轉換
- ✅ 簡化為直接呼叫 `LoadExistingDetailsAsync()`（內部會正確處理 `_dataLoadCompleted`）
- ✅ 與 `SalesOrderTable` 保持一致

## 修改結果

### 執行效能比較

#### 修改前
```
🔍 [SalesReturnTable] OnParametersSetAsync - 被觸發 10+ 次
⚠ [SalesReturnTable] 新增模式：無現有明細
🔍 [InteractiveTable] OnAfterRenderAsync - Items: 0  ❌ 空行消失
🔍 [InteractiveTable] OnAfterRenderAsync - Items: 1  ✓ 空行出現（但會再次消失）
🔍 [SalesReturnTable] OnParametersSetAsync - 持續觸發...
```

#### 修改後
```
🔍 [SalesReturnTable] OnParametersSetAsync - 被觸發 7 次
🔍 [SalesReturnTable] OnParametersSetAsync - 無變化,跳過處理（5次）
ℹ [SalesReturnTable] 客戶變更, CustomerId: 68
ℹ [SalesReturnTable] LoadExistingDetailsAsync 開始, ExistingReturnDetails 數量: 0
⚠ [SalesReturnTable] 無現有明細，清空並標記完成
🔍 [InteractiveTable] OnAfterRenderAsync - Items: 1  ✓ 空行正常顯示並保持
✓ [SalesReturnTable] OnInitializedAsync 完成, ReturnItems 數量: 1
```

### 改進成果

| 項目 | 修改前 | 修改後 | 改進 |
|------|--------|--------|------|
| `OnParametersSetAsync` 觸發次數 | 10+ 次 | 7 次（5 次跳過） | ✅ 減少 30% |
| 實際處理次數 | 10+ 次 | 2 次 | ✅ 減少 80% |
| 空行狀態 | ❌ 消失 | ✅ 正常保持 | ✅ 已修復 |
| 警告訊息閃爍 | ❌ 會閃爍 | ✅ 不會閃爍 | ✅ 已修復 |
| 程式碼複雜度 | 高（需要 InvokeAsync + Task.Delay） | 低（與標準做法一致） | ✅ 更簡潔 |

## 重要原則總結

### ✅ 正確做法（參考 SalesOrderEditModalComponent）

1. **EditModal 層級**（OnFieldValueChanged）：
   - ❌ **不要**修改 `formFields` 的 `Options`
   - ✅ 只更新 `ActionButtons`（如果需要）
   - ✅ 只呼叫一次 `StateHasChanged()`

2. **Table 層級**（OnParametersSetAsync）：
   - ✅ 直接清空 `Items` 並重新載入
   - ❌ **不要**使用 `InvokeAsync` 或 `Task.Delay`
   - ❌ **不要**手動控制 `_dataLoadCompleted` 的轉換
   - ✅ 讓 `LoadExistingDetailsAsync()` 內部處理 `_dataLoadCompleted`

3. **LoadExistingDetailsAsync 內部**：
   ```csharp
   private async Task LoadExistingDetailsAsync()
   {
       if (ExistingReturnDetails?.Any() != true) 
       {
           ReturnItems.Clear();
           _dataLoadCompleted = true;              // ✅ 直接設為 true
           tableComponent?.RefreshEmptyRow();      // ✅ 觸發空行檢查
           StateHasChanged();
           return;
       }
       
       _dataLoadCompleted = false;                 // ✅ 開始載入
       ReturnItems.Clear();
       
       // ... 載入明細資料 ...
       
       _dataLoadCompleted = true;                  // ✅ 載入完成
       tableComponent?.RefreshEmptyRow();
       StateHasChanged();
   }
   ```

### ❌ 錯誤做法（會導致過度渲染）

1. **在 OnFieldValueChanged 中修改 formFields.Options**
   ```csharp
   // ❌ 這會觸發多次參數變更偵測
   salesOrderField.Options = newOptions;
   filterProductField.Options = newOptions;
   StateHasChanged();
   ```

2. **手動控制 _dataLoadCompleted 的轉換**
   ```csharp
   // ❌ 這會觸發無限循環
   _dataLoadCompleted = false;
   await InvokeAsync(async () =>
   {
       await Task.Delay(1);
       _dataLoadCompleted = true;
   });
   ```

## 適用範圍

此修改模式適用於所有具有以下結構的組件：
- `EditModalComponent`（主檔編輯 Modal）
  - 包含客戶、廠商等篩選欄位
  - 需要根據篩選條件更新明細表格
  
- `Table Component`（明細表格）
  - 接收篩選參數（CustomerId, SupplierId 等）
  - 具有自動空行管理功能
  - 需要根據參數變更重新載入資料

### 已套用此模式的組件
1. ✅ `SalesOrderEditModalComponent` + `SalesOrderTable`（參考標準）
2. ✅ `SalesReturnEditModalComponent` + `SalesReturnTable`（本次修改）

### 建議套用的組件
- `SalesDeliveryEditModalComponent` + `SalesDeliveryTable`
- `PurchaseOrderEditModalComponent` + `PurchaseOrderTable`
- `PurchaseReturnEditModalComponent` + `PurchaseReturnTable`
- 其他所有遵循相同模式的組件

## 注意事項

1. **不要過度優化**：如果組件沒有出現過度渲染問題，不需要修改
2. **保持一致性**：新組件應遵循 `SalesOrderEditModalComponent` 的標準做法
3. **測試完整性**：修改後務必測試以下場景：
   - 新增模式選擇客戶（空行應正常顯示）
   - 編輯模式載入明細（明細應正確顯示）
   - 上下筆切換（明細應正確更新）
   - 有不可刪除明細時的鎖定狀態

## 修改日期
2025年11月28日

## 修改者
GitHub Copilot (Claude Sonnet 4.5)
