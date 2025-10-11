# 進貨明細鎖定主檔欄位功能

## 功能說明

當進貨單的明細中有不可刪除的記錄（已退貨或已沖款）時，主檔的某些關鍵欄位會自動變為唯讀，防止數據不一致。

## 鎖定規則

### 觸發條件
明細符合以下任一條件時，視為「不可刪除」：
1. **已有退貨記錄**：`TotalReturnedQuantity > 0`
2. **已有沖款記錄**：`TotalPaidAmount > 0`

### 被鎖定的欄位
當存在不可刪除的明細時，以下主檔欄位會被設為唯讀：
- **廠商** (`SupplierId`) - 同時移除「新增」和「編輯」操作按鈕
- **採購單** (`PurchaseOrderId`)
- **產品篩選** (`FilterProductId`)
- **進貨日** (`ReceiptDate`)
- **備註** (`Remarks`)

> **重要**：廠商欄位不僅會被設為唯讀，連同其 ActionButtons（新增/編輯按鈕）也會被移除，防止使用者透過按鈕間接修改被鎖定的資料。

### 不被鎖定的欄位（系統自動計算/生成）
以下欄位本身就是唯讀，不受鎖定狀態影響：
- **進貨單號** (`ReceiptNumber`) - 系統自動生成
- **入庫總金額** (`TotalAmount`) - 根據明細自動計算
- **採購稅額** (`PurchaseReceivingTaxAmount`) - 根據明細自動計算
- **含稅總金額** (`PurchaseReceivingTotalAmountIncludingTax`) - 根據明細自動計算

### 解除鎖定
當所有不可刪除的明細都被處理（退貨沖回或沖款取消）後，欄位會自動解除鎖定。

## 實現架構

### 1. 子組件 (PurchaseReceivingDetailManagerComponent)

#### 新增參數
```csharp
/// <summary>
/// 狀態通知參數 - 通知父組件是否有不可刪除的明細
/// </summary>
[Parameter] public EventCallback<bool> OnHasUndeletableDetailsChanged { get; set; }
```

#### 核心方法

**檢查是否有不可刪除的明細**
```csharp
/// <summary>
/// 檢查是否有不可刪除的明細（已有退貨或沖款記錄）
/// </summary>
private bool HasUndeletableDetails()
{
    return ReceivingItems.Any(item => 
        !IsEmptyRow(item) && !CanDeleteItem(item, out _));
}
```

**通知父組件**
```csharp
/// <summary>
/// 通知父組件是否有不可刪除的明細
/// </summary>
private async Task NotifyHasUndeletableDetailsChanged()
{
    if (OnHasUndeletableDetailsChanged.HasDelegate)
    {
        var hasUndeletableDetails = HasUndeletableDetails();
        await OnHasUndeletableDetailsChanged.InvokeAsync(hasUndeletableDetails);
    }
}
```

#### 觸發時機
1. **載入退貨數量後**：`LoadReturnedQuantitiesAsync()` 完成時
2. **明細變更時**：`NotifyDetailsChanged()` 中
3. **明細新增/刪除時**：透過 `NotifyDetailsChanged()` 間接觸發

### 2. 父組件 (PurchaseReceivingEditModalComponent)

#### 新增狀態
```csharp
// ===== 鎖定狀態 =====
private bool hasUndeletableDetails = false;
```

#### 核心方法

**處理狀態變更**
```csharp
/// <summary>
/// 處理有不可刪除明細的狀態變更
/// 當有不可刪除的明細時(已退貨或已沖款),鎖定主檔的廠商和採購單欄位
/// </summary>
private async Task HandleHasUndeletableDetailsChanged(bool hasUndeletable)
{
    try
    {
        hasUndeletableDetails = hasUndeletable;
        
        // 更新欄位的唯讀狀態
        UpdateFieldsReadOnlyState();
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理明細鎖定狀態時發生錯誤：{ex.Message}");
    }
}
```

**更新欄位唯讀狀態**
```csharp
/// <summary>
/// 更新欄位的唯讀狀態
/// 當有不可刪除的明細時,所有使用者可輸入的欄位都應設為唯讀:
/// - 廠商 (SupplierId) - 同時禁用新增/編輯按鈕
/// - 採購單 (PurchaseOrderId)
/// - 產品篩選 (FilterProductId)
/// - 進貨日 (ReceiptDate)
/// - 備註 (Remarks)
/// </summary>
private void UpdateFieldsReadOnlyState()
{
    // 廠商欄位 - 設為唯讀並清空 ActionButtons
    var supplierField = formFields.FirstOrDefault(f => f.PropertyName == nameof(PurchaseReceiving.SupplierId));
    if (supplierField != null)
    {
        supplierField.IsReadOnly = hasUndeletableDetails;
        
        if (hasUndeletableDetails)
        {
            supplierField.ActionButtons = new List<FieldActionButton>();
        }
        else
        {
            supplierField.ActionButtons = GetSupplierActionButtonsAsync();
        }
    }
    
    // 採購單欄位
    var purchaseOrderField = formFields.FirstOrDefault(f => f.PropertyName == nameof(PurchaseReceiving.PurchaseOrderId));
    if (purchaseOrderField != null)
    {
        purchaseOrderField.IsReadOnly = hasUndeletableDetails;
    }
    
    // 產品篩選欄位
    var filterProductField = formFields.FirstOrDefault(f => f.PropertyName == "FilterProductId");
    if (filterProductField != null)
    {
        filterProductField.IsReadOnly = hasUndeletableDetails;
    }
    
    // 進貨日欄位
    var receiptDateField = formFields.FirstOrDefault(f => f.PropertyName == nameof(PurchaseReceiving.ReceiptDate));
    if (receiptDateField != null)
    {
        receiptDateField.IsReadOnly = hasUndeletableDetails;
    }
    
    // 備註欄位
    var remarksField = formFields.FirstOrDefault(f => f.PropertyName == nameof(BaseEntity.Remarks));
    if (remarksField != null)
    {
        remarksField.IsReadOnly = hasUndeletableDetails;
    }
}
```

**防止欄位變更時重新添加按鈕**
```csharp
private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    try
    {
        // 當廠商變更時，更新採購單選項並清空採購單選擇
        if (fieldChange.PropertyName == nameof(PurchaseReceiving.SupplierId))
        {
            // ... 其他邏輯 ...
            
            // 🔑 關鍵：只有在沒有不可刪除明細時才更新 ActionButtons
            if (!hasUndeletableDetails)
            {
                await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                    supplierModalManager, 
                    formFields, 
                    nameof(PurchaseReceiving.SupplierId), 
                    fieldChange.Value
                );
            }
            
            StateHasChanged();
        }
        // ... 其他欄位處理 ...
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理欄位變更時發生錯誤：{ex.Message}");
    }
}
```

#### 綁定事件
```razor
<PurchaseReceivingDetailManagerComponent 
    ...
    OnHasUndeletableDetailsChanged="@HandleHasUndeletableDetailsChanged"
    ... />
```

## 使用流程

### 場景 1：編輯已有退貨記錄的進貨單
1. 用戶打開進貨單編輯頁面
2. 組件載入現有明細
3. `LoadReturnedQuantitiesAsync()` 檢測到某些明細已退貨
4. 觸發 `NotifyHasUndeletableDetailsChanged()` → 傳遞 `true`
5. 父組件收到通知，調用 `UpdateFieldsReadOnlyState()`
6. 廠商、採購單、產品篩選、進貨日、備註欄位變為唯讀
7. 用戶無法修改這些關鍵欄位

### 場景 2：新增明細後發現有不可刪除項
1. 用戶正在編輯進貨單
2. 某個明細已被用於退貨
3. 用戶嘗試新增其他明細
4. `NotifyDetailsChanged()` 檢查所有明細狀態
5. 發現有不可刪除的明細 → 鎖定主檔欄位

### 場景 3：刪除所有不可刪除明細後解鎖
1. 進貨單目前有已退貨的明細，主檔欄位被鎖定
2. 用戶透過其他流程處理退貨（沖回）
3. 明細的 `TotalReturnedQuantity` 變為 0
4. 重新載入明細時，`HasUndeletableDetails()` 返回 `false`
5. 通知父組件 → 主檔欄位解鎖

## 設計優點

### ✅ 數據一致性
- 防止修改已退貨或已沖款明細的關鍵資訊
- 避免廠商/產品不一致導致的財務錯誤
- **防止透過操作按鈕繞過唯讀限制**

### ✅ 自動化
- 無需手動檢查，系統自動偵測並鎖定
- 狀態變更時即時響應
- **自動移除/恢復操作按鈕**

### ✅ 用戶友好
- 唯讀欄位仍可查看，不會隱藏資訊
- 清楚告知用戶哪些欄位不可修改

### ✅ 解耦設計
- 子組件負責檢測，父組件負責響應
- 通過 EventCallback 解耦，易於維護

## 測試要點

### 功能測試
1. **初始載入**
   - 有不可刪除明細時，主檔欄位應為唯讀
   - 無不可刪除明細時，主檔欄位應可編輯

2. **動態變更**
   - 新增明細後，檢查鎖定狀態是否正確更新
   - 刪除明細後，檢查鎖定狀態是否正確更新

3. **退貨沖回**
   - 退貨沖回後，欄位應解鎖

4. **沖款處理**
   - 有沖款記錄時，欄位應鎖定
   - 沖款取消後，欄位應解鎖

### 邊界測試
1. 空明細列表
2. 所有明細都可刪除
3. 所有明細都不可刪除
4. 混合狀態（部分可刪除，部分不可刪除）

## 注意事項

### ⚠️ 欄位鎖定範圍
**被鎖定的欄位（使用者可輸入）：**
- 廠商（同時移除操作按鈕）
- 採購單
- 產品篩選
- **進貨日**
- **備註**

**不被鎖定的欄位（系統自動計算/生成）：**
- 進貨單號（已經是唯讀）
- 入庫總金額（已經是唯讀）
- 採購稅額（已經是唯讀）
- 含稅總金額（已經是唯讀）

> **設計原則**：凡是使用者可以輸入/修改的欄位都應該被鎖定，系統自動計算的欄位本身就是唯讀，不受鎖定狀態影響。

### ⚠️ ActionButtons 處理
- 廠商欄位被鎖定時，會清空其 `ActionButtons` 列表
- 解鎖時會呼叫 `GetSupplierActionButtonsAsync()` 恢復按鈕
- **重要**：在 `OnFieldValueChanged` 事件中，需檢查 `hasUndeletableDetails` 狀態，避免在欄位獲得焦點時重新添加按鈕
- 其他有 ActionButtons 的欄位需要鎖定時，也應採用相同邏輯

### ⚠️ 不影響明細編輯
- 主檔欄位鎖定不影響明細的數量、價格等欄位編輯
- 不可刪除的明細仍可修改（需根據業務規則決定是否限制）

### ⚠️ 性能考量
- `HasUndeletableDetails()` 會遍歷所有明細
- 對於大量明細的情況，考慮快取結果

## 未來擴展

### 可能的增強
1. **欄位級提示**：在每個被鎖定的欄位旁顯示鎖頭圖標
2. **部分鎖定**：根據不可刪除明細的類型，鎖定不同的欄位
3. **鎖定記錄**：記錄欄位何時被鎖定，誰嘗試修改
4. **強制解鎖**：管理員權限可強制解鎖（需謹慎使用）
5. **詳細原因**：點擊提示訊息時，顯示哪些明細導致鎖定（明細編號、退貨/沖款金額等）

## 相關文件
- `README_刪除限制設計.md` - 進貨明細刪除限制的整體設計
- `README_PurchaseReceiving_刪除限制增強.md` - 進貨單刪除限制增強

## 修改歷史
- 2025-01-11：初始版本，實現基於退貨和沖款記錄的主檔欄位鎖定功能
- 2025-01-11：修正 ActionButtons 繞過限制問題（第一版）- 鎖定欄位時同時移除操作按鈕
- 2025-01-11：修正 ActionButtons 繞過限制問題（完整版）- 在 OnFieldValueChanged 中也檢查鎖定狀態，防止欄位獲得焦點時重新添加按鈕
- 2025-01-11：擴展鎖定範圍 - 將所有使用者可輸入的欄位（進貨日、備註）都加入鎖定，只保留系統自動計算的欄位為唯讀但不受鎖定影響
