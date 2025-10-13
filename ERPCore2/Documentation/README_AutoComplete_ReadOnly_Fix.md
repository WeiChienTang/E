# AutoComplete 欄位唯讀狀態修正

## 問題描述

當表單欄位設定為 `IsReadOnly = true` 時,如果該欄位的類型是 `FormFieldType.AutoComplete`,雖然輸入框會變為唯讀,但下拉選單仍然可以透過以下方式觸發和使用:

1. **點擊欄位獲得焦點** → `onfocus` 事件觸發 → 顯示下拉選單
2. **輸入文字** → `oninput` 事件觸發 → 搜尋並顯示下拉選單
3. **鍵盤操作** → `onkeydown` 事件觸發 → 可以用方向鍵選擇選項

這導致使用者可以繞過唯讀限制,仍然能夠修改欄位值,違反了鎖定邏輯的初衷。

## 問題場景

### 進貨單/銷貨單明細鎖定
根據 `README_進貨明細鎖定主檔欄位.md` 的設計,當單據中有不可刪除的明細時(已退貨或已沖款),主檔的關鍵欄位應該被鎖定:

- 廠商 / 客戶欄位 (AutoComplete)
- 採購單欄位 (AutoComplete)  
- 業務員欄位 (AutoComplete)
- 其他相關欄位

雖然這些欄位設定為 `IsReadOnly = true`,但下拉選單仍然可以使用,讓使用者可以選擇其他選項,破壞了資料一致性。

## 解決方案

### ✅ 方案:修改 GenericFormComponent 的 RenderAutoComplete 方法

**核心思路**:在 `IsReadOnly` 或 `IsDisabled` 狀態下,不綁定任何互動事件,也不顯示下拉選單和載入指示器。

### 修改位置
**檔案**: `Components/Shared/Forms/GenericFormComponent.razor`  
**方法**: `RenderAutoComplete()`

### 修改內容

#### 1. 條件式綁定互動事件

```csharp
// 🔑 只有在非唯讀和非停用狀態下才綁定互動事件
if (!field.IsReadOnly && !field.IsDisabled)
{
    // 輸入事件處理
    builder.AddAttribute(sequence + 12, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, args =>
    {
        var inputValue = args.Value?.ToString() ?? "";
        autoCompleteDisplayValues[fieldId] = inputValue;
        _ = HandleAutoCompleteInput(field, inputValue);
    }));
    
    // 焦點事件處理
    builder.AddAttribute(sequence + 13, "onfocus", EventCallback.Factory.Create(this, () =>
    {
        // ... 焦點處理邏輯
    }));
    
    // 失去焦點事件處理
    builder.AddAttribute(sequence + 14, "onblur", EventCallback.Factory.Create(this, async () =>
    {
        // ... 失去焦點處理邏輯
    }));
    
    // 鍵盤事件處理
    builder.AddAttribute(sequence + 15, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, args =>
    {
        _ = HandleKeyDown(field, args);
    }));
}
```

#### 2. 條件式顯示下拉元素

```csharp
// 🔑 只有在非唯讀和非停用狀態下才顯示載入指示器和下拉選項
if (!field.IsReadOnly && !field.IsDisabled)
{
    // 載入指示器
    if (autoCompleteLoading.GetValueOrDefault(fieldId, false))
    {
        // ... 載入指示器渲染邏輯
    }
    
    // 下拉選項
    if (autoCompleteVisible.GetValueOrDefault(fieldId, false) && autoCompleteOptions.ContainsKey(fieldId))
    {
        // ... 下拉選項渲染邏輯
    }
}
```

## 效果

### ✅ 唯讀狀態 (IsReadOnly = true)
- ✅ 輸入框顯示為唯讀,游標仍可聚焦但無法修改
- ✅ 點擊欄位時**不會**觸發下拉選單
- ✅ 無法輸入文字搜尋
- ✅ 鍵盤操作無效(方向鍵、Enter、Tab等)
- ✅ 不顯示載入指示器
- ✅ 完全無法修改欄位值

### ✅ 正常狀態 (IsReadOnly = false, IsDisabled = false)
- ✅ 所有 AutoComplete 功能正常運作
- ✅ 可以輸入文字搜尋
- ✅ 可以點擊顯示下拉選單
- ✅ 鍵盤導航正常
- ✅ Tab 鍵智能匹配正常

### ✅ 停用狀態 (IsDisabled = true)
- ✅ 輸入框完全停用,無法獲得焦點
- ✅ 不綁定任何事件
- ✅ 不顯示下拉選單和載入指示器

## 優點

### 🎯 一致性
- 唯讀行為與一般 Text Input 一致
- 符合使用者對唯讀欄位的預期

### 🔒 安全性
- 完全阻止使用者修改被鎖定的欄位
- 防止透過下拉選單繞過唯讀限制

### 🚀 效能
- 唯讀狀態下不執行搜尋邏輯
- 減少不必要的事件處理
- 不渲染下拉選單,減少 DOM 元素

### 🛠️ 維護性
- 修改集中在一個方法內
- 不需要在每個使用 AutoComplete 的組件中額外處理
- 所有使用 `GenericFormComponent` 的地方都自動獲得此修正

## 影響範圍

此修改會影響**所有使用 `GenericFormComponent` 渲染的 AutoComplete 欄位**,包括但不限於:

- 進貨單編輯頁面 (廠商、採購單等)
- 銷貨單編輯頁面 (客戶、業務員等)
- 其他所有使用 AutoComplete 的表單

**向下相容性**: ✅ 完全相容
- 原本沒有設定 `IsReadOnly` 的欄位不受影響
- 原本設定 `IsReadOnly = false` 的欄位行為不變
- 只有明確設定 `IsReadOnly = true` 或 `IsDisabled = true` 的欄位才會套用新邏輯

## 測試要點

### 功能測試
1. **唯讀 AutoComplete 欄位**
   - [ ] 點擊欄位不會顯示下拉選單
   - [ ] 輸入文字無效(欄位已經是 readonly,無法輸入)
   - [ ] 鍵盤操作無效
   - [ ] 不顯示載入指示器

2. **正常 AutoComplete 欄位**
   - [ ] 所有搜尋功能正常
   - [ ] 下拉選單正常顯示
   - [ ] 鍵盤導航正常
   - [ ] Tab 鍵智能匹配正常

3. **停用 AutoComplete 欄位**
   - [ ] 欄位完全無法互動
   - [ ] 不顯示任何下拉元素

### 整合測試
1. **進貨單明細鎖定**
   - [ ] 有不可刪除明細時,廠商欄位完全無法修改
   - [ ] 無不可刪除明細時,廠商欄位可正常使用

2. **銷貨單明細鎖定**
   - [ ] 有不可刪除明細時,客戶和業務員欄位完全無法修改
   - [ ] 無不可刪除明細時,客戶和業務員欄位可正常使用

## 相關文件
- `README_進貨明細鎖定主檔欄位.md` - 進貨明細鎖定主檔欄位的整體設計
- `README_刪除限制設計.md` - 進貨明細刪除限制的整體設計

## 修改歷史
- 2025-01-13:初始版本 - 修正 AutoComplete 欄位在唯讀狀態下仍可使用下拉選單的問題
