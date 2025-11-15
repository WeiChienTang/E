# 上下筆功能失敗問題調查報告

## 問題描述

在 `GenericEditModalComponent` 中實現的上下筆（Previous/Next）導航功能，在某些 EditModal 組件中無法正常工作：

- **成功案例**: `UnitEditModalComponent`、`ProductCategoryEditModalComponent` - 沒有使用 ActionButtons 的組件
- **失敗案例**: `WarehouseLocationEditModalComponent` - 使用了 ActionButtons 的組件

### 症狀

點擊「下一筆」或「上一筆」按鈕後：
- 頁面重新刷新
- 載入動畫顯示
- 但表單資料保持原本的記錄，不會切換到下一筆或上一筆

## 調查過程

### 第一階段：ActionButtons 更新問題

**假設**: ActionButtons 沒有在導航時更新

**測試**:
1. 添加 `InvokeInitializeFormFieldsCallbacksAsync()` 機制
   - 在 `NavigateToRecordAsync` 中調用父組件的 `InitializeFormFieldsAsync`
   - 目的：重新生成 ActionButtons

2. 添加 `RegenerateFieldActionButtonsAsync()` 方法
   - 直接從 modalManager 重新生成按鈕
   - 直接更新 FormFields 中的 ActionButtons 屬性

3. 添加雙重 `_autoCompleteVersion` 遞增
   - 第一次遞增：觸發 GenericFormComponent 重新創建
   - StateHasChanged + Task.Delay(10)
   - 第二次遞增：確保完全重新渲染

4. 添加 `@key` 指令到按鈕元素
   - 使用 `@key="{field.PropertyName}_{actionButton.Text}_{actionButton.IsDisabled}"`
   - 確保按鈕元素在資料變化時被重新創建

**結果**: 日誌顯示 ActionButtons 正確更新（新增 → 編輯），但上下筆仍然失敗

### 第二階段：OnClick 閉包問題

**假設**: OnClick 事件處理器捕獲了舊的 entity ID

**測試**:
1. 在 `RelatedEntityModalManager.GenerateActionButtons` 中添加日誌
   - 記錄傳入的 `currentSelectedId`
   - 記錄按鈕類型（新增/編輯）

2. 在 OnClick lambda 內添加日誌
   - 記錄點擊時實際使用的 ID

**結果**: 
- 日誌顯示 ActionButtons 生成時使用正確的 ID
- 按鈕文字正確（編輯 vs 新增）
- 但仍未解決上下筆失敗問題

### 第三階段：FormFields 參數傳遞問題

**假設**: FormFields 參數沒有正確從父組件傳遞到子組件

**測試**:
1. 在父組件的 `GetFormFields()` 中添加 HashCode 日誌
   - 確認每次調用都返回新的列表實例

2. 在子組件的 `GetProcessedFormFields()` 中添加 HashCode 日誌
   - 確認子組件接收到的是新實例

3. 修改 `NavigateToRecordAsync` 流程
   ```csharp
   await InvokeInitializeFormFieldsCallbacksAsync(); // 調用父組件更新
   await InvokeAsync(StateHasChanged);  // 強制刷新渲染週期
   await InvokeAsync(() => { });  // 再次刷新
   _autoCompleteVersion++;  // 遞增版本號
   ```

4. 移除 `RegenerateFieldActionButtonsAsync` 調用
   - 因為父組件已經更新了 formFields
   - 避免重複修改導致時序問題

**結果**: 
- HashCode 日誌顯示每次都是新實例
- 初始: `65060509`
- 父組件更新後: `23779244`
- 子組件接收: `50712275`
- 第二次遞增: `17874811`
- **參數傳遞正常，但上下筆仍然失敗**

### 第四階段：AutoComplete 顯示值問題

**假設**: AutoComplete 輸入框的顯示值沒有更新

**發現**:
1. GenericFormComponent 使用內部字典 `autoCompleteDisplayValues` 存儲 AutoComplete 的顯示文字
2. 當組件被重新創建時（`@key` 變化），字典被清空
3. `OnParametersSet` 中的初始化邏輯是異步的，可能渲染後才完成

**測試**:
1. 在 `RenderAutoCompleteFieldWithButtons` 中添加初始化檢查
   ```csharp
   if (!autoCompleteDisplayValues.ContainsKey(fieldId) || 
       string.IsNullOrEmpty(autoCompleteDisplayValues[fieldId]))
   {
       InitializeAutoCompleteDisplayValue(field, currentValue);
   }
   ```

2. 添加 `InitializeAutoCompleteDisplayValue` 方法
   - 使用 `field.SearchFunction` 異步查找顯示文字
   - 從 SelectOption 列表中匹配 Value 找到對應的 Text
   - 調用 `InvokeAsync` 更新顯示值並觸發 StateHasChanged

3. 添加詳細日誌追蹤
   - 記錄何時初始化顯示值
   - 記錄找到的匹配項
   - 記錄找不到匹配項的情況

**結果**: 測試中...

## 日誌分析

### 典型的上下筆導航日誌

```
[WarehouseLocation] GetFormFields called but formFields not initialized yet, returning empty list  ×3
[WarehouseLocation] InitializeFormFieldsAsync called. Entity ID: 0
[WarehouseLocation] GetWarehouseActionButtonsAsync - Current WarehouseId: 0
[RelatedEntityModalManager] GenerateActionButtons called with currentSelectedId: 0
[RelatedEntityModalManager] Creating Add button
[WarehouseLocation] Generated buttons count: 1
  - Button: 新增, IsDisabled: False
[WarehouseLocation] InitializeFormFieldsAsync completed, calling StateHasChanged
[WarehouseLocation] GetFormFields called, returning 9 fields, HashCode: 45242186
[RelatedEntityModalManager] GenerateActionButtons called with currentSelectedId: 1
[RelatedEntityModalManager] Creating Edit button for ID: 1
[GenericEditModal] GetProcessedFormFields called, FormFields count: 9, HashCode: 65060509
[GenericEditModal] Calling InvokeInitializeFormFieldsCallbacksAsync
[GenericEditModal] InvokeInitializeFormFieldsCallbacksAsync - Entity ID: 7  // 切換到第7筆
[WarehouseLocation] InitializeFormFieldsAsync called. Entity ID: 7
[WarehouseLocation] GetWarehouseActionButtonsAsync - Current WarehouseId: 1
[RelatedEntityModalManager] GenerateActionButtons called with currentSelectedId: 1
[WarehouseLocation] Generated buttons count: 1
  - Button: 編輯, IsDisabled: False
[WarehouseLocation] InitializeFormFieldsAsync completed, calling StateHasChanged
[WarehouseLocation] GetFormFields called, returning 9 fields, HashCode: 23779244  // 新實例
[GenericEditModal] CustomPostProcessCallback completed for WarehouseId
[GenericEditModal] After InvokeAsync render cycle
[GenericEditModal] First _autoCompleteVersion increment: 1
[WarehouseLocation] GetFormFields called, returning 9 fields, HashCode: 50712275  // 又一個新實例
[GenericEditModal] GetProcessedFormFields called, FormFields count: 9, HashCode: 50712275
[GenericEditModal] Second _autoCompleteVersion increment: 2
[WarehouseLocation] GetFormFields called, returning 9 fields, HashCode: 17874811  // 再一個新實例
[GenericEditModal] GetProcessedFormFields called, FormFields count: 9, HashCode: 17874811
```

### 關鍵觀察

1. **Entity ID 正確更新**: 從 ID=5 切換到 ID=7
2. **ActionButtons 正確生成**: 根據 WarehouseId 正確顯示「編輯」按鈕
3. **FormFields 參數正確傳遞**: HashCode 持續變化，每次都是新實例
4. **父組件正確更新**: `InitializeFormFieldsAsync` 被調用並完成
5. **子組件正確接收**: `GetProcessedFormFields` 讀取到新的 FormFields

## 尚未解決的問題

儘管所有資料層面的更新都正確執行：
- Entity 更新 ✅
- ActionButtons 重新生成 ✅  
- FormFields 重新創建 ✅
- 參數正確傳遞 ✅
- 組件重新渲染 ✅ (透過 _autoCompleteVersion)

**但表單上的資料仍然顯示舊記錄**

## 可能的原因

### 1. EditContext 問題
`editContext = new EditContext(Entity)` 在 NavigateToRecordAsync 中已經創建，但可能：
- GenericFormComponent 沒有使用這個 EditContext
- 或者 EditContext 的變更沒有觸發表單重新綁定

### 2. 雙向綁定問題
GenericFormComponent 中的輸入框使用：
```razor
value="@GetPropertyValue(Model, field.PropertyName)?.ToString()"
@oninput="@(e => SetPropertyValue(Model, field.PropertyName, e.Value?.ToString()))"
```
可能存在：
- `Model` 參數沒有正確更新
- 或者 Blazor 的 diff 演算法認為元素沒變化

### 3. AutoComplete 顯示值快取
`autoCompleteDisplayValues` 字典可能：
- 保留了舊值
- 異步初始化未完成前顯示舊值
- 需要同步初始化機制

### 4. 組件重用問題
雖然使用了 `@key="@_autoCompleteVersion"`，但可能：
- GenericFormComponent 內部的某些子組件沒有被重新創建
- 輸入框元素被 Blazor 重用而非重新創建

## 下一步調查方向

1. **檢查 Model 參數綁定**
   - 確認 GenericFormComponent 的 Model 參數是否正確接收新 Entity
   - 添加日誌在 GenericFormComponent.OnParametersSet 中

2. **檢查輸入框的 value 綁定**
   - 確認 GetPropertyValue 是否返回正確的新值
   - 添加日誌在 RenderInputField 中

3. **測試純文字欄位**
   - 檢查 Code、Name 等 Text 欄位是否更新
   - 如果純文字欄位也不更新，問題在 Model 綁定
   - 如果只有 AutoComplete 不更新，問題在 autoCompleteDisplayValues

4. **強制重新創建輸入框**
   - 為所有輸入框添加 @key 指令
   - 使用 Entity.Id 或組合鍵確保每個記錄的輸入框都是新實例

5. **檢查 EditForm 和 EditContext**
   - 確認 GenericFormComponent 是否正確使用 EditContext
   - 測試是否需要在 EditForm 層級添加 @key

## 對比：成功的組件

成功的 `UnitEditModalComponent` 和 `ProductCategoryEditModalComponent` 的共同特徵：
- 不使用 ActionButtons
- 不使用 AutoComplete（或使用較簡單的 AutoComplete）
- FormFields 結構較簡單

這暗示問題可能與：
- ActionButtons 的存在導致額外的渲染邏輯
- AutoComplete 的 displayValues 快取機制
- 複雜的欄位處理邏輯

## 已實施的修改清單

### GenericEditModalComponent.razor
1. `NavigateToRecordAsync` 方法
   - 添加 `InvokeInitializeFormFieldsCallbacksAsync()` 調用
   - 添加雙重 InvokeAsync(StateHasChanged)
   - 雙重 _autoCompleteVersion 遞增
   - 詳細的 Console 日誌

2. `InvokeInitializeFormFieldsCallbacksAsync` 方法
   - 遍歷所有 ModalManagers
   - 調用 InitializeFormFieldsCallback
   - 移除 RegenerateFieldActionButtonsAsync 調用（避免時序問題）

3. `GetProcessedFormFields` 方法
   - 添加 HashCode 日誌
   - 詳細的 ActionButtons 日誌

### GenericFormComponent.razor
1. `RenderAutoCompleteFieldWithButtons` 方法
   - 添加 autoCompleteDisplayValues 初始化檢查
   - 調用 InitializeAutoCompleteDisplayValue 方法

2. 新增 `InitializeAutoCompleteDisplayValue` 方法
   - 使用 field.SearchFunction 異步查找顯示文字
   - InvokeAsync 更新 UI
   - 詳細的 Console 日誌

3. 按鈕元素
   - 添加 @key 指令確保重新創建

### WarehouseLocationEditModalComponent.razor
1. `GetFormFields` 方法
   - 添加初始化檢查，避免返回空列表
   - 添加 HashCode 日誌
   - 每次返回 formFields.ToList() 新實例

2. `InitializeFormFieldsAsync` 方法
   - 添加詳細的 Entity ID 日誌
   - 添加 ActionButtons 生成日誌

### RelatedEntityModalManager.cs
1. `GenerateActionButtons` 方法
   - 添加 currentSelectedId 日誌
   - 添加按鈕類型日誌（新增/編輯）

2. `OpenModalAsync` 方法
   - 添加 entityId 參數日誌

## 測試環境

- Framework: Blazor Server (.NET)
- 測試組件: WarehouseLocationEditModalComponent
- 測試實體: WarehouseLocation (ID: 5 → 7)
- 關聯欄位: WarehouseId (值: 1，保持不變)

## 結論

經過四個階段的深入調查和測試，已經確認：
- 資料層面的更新完全正常
- 組件參數傳遞機制正常
- ActionButtons 生成和更新機制正常

**但 UI 顯示仍然不更新**，問題很可能在 Blazor 的視圖綁定或組件渲染機制層面。

需要進一步調查 GenericFormComponent 如何綁定 Model 屬性到輸入框，以及為什麼即使 Model 改變了，輸入框的顯示值仍然保持不變。

---

**最後更新**: 2025年11月15日  
**狀態**: 🔴 問題尚未解決  
**下一步**: 檢查 GenericFormComponent 的 Model 參數綁定和輸入框 value 更新機制
