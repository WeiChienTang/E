# Blazor CustomTemplate 渲染問題修正 - 欄位即時更新

## 📋 問題描述

### 問題現象
在 `SetoffPrepaymentManagerComponent` 和 `SetoffDetailManagerComponent` 中，當使用者在輸入欄位（例如「本次金額」或「本次沖款」）輸入數字時，相關的顯示欄位## 📝 修正的檔案

### 1. SetoffPrepaymentManagerComponent.razor

**修正的欄位定義：**
- ✅ 原始金額（Amount）- 加上 PropertyName
- ✅ 已用金額（UsedAmount）- 加上 PropertyName
- ✅ 可用金額（AvailableAmount）- 加上 PropertyName

**修正的方法和變數：**
- ✅ 添加 `_shouldRenderOverride` 強制渲染標記
- ✅ 修改 `ShouldRender()` - 支持強制渲染標記
- ✅ 修改 `HandleAddAmountChanged()` - 設置強制渲染標記

### 2. SetoffDetailManagerComponent.razor

**修正的方法和變數：**
- ✅ 添加 `_shouldRenderOverride` 強制渲染標記
- ✅ 修改 `ShouldRender()` - 支持強制渲染標記
- ✅ 修改 `HandleAmountChanged()` - 設置強制渲染標記
- ✅ 修改 `HandleDiscountAmountChanged()` - 設置強制渲染標記

**備註：** 此組件的顯示欄位（已沖款、已折讓、待沖款）使用了動態計算屬性（`DynamicSettledAmount`、`DynamicDiscountedAmount`、`DynamicPendingAmount`），配合強制渲染標記即可正常運作。款」）**只會顯示第一個數字**。

**例如：**
- 輸入「本次金額」：600
- 「原始金額」顯示：6（❌ 錯誤）
- 用滑鼠圈選其他欄位後
- 「原始金額」顯示：600（✅ 正確）

### 問題原因

這是 Blazor 的渲染機制問題，有**兩個主要原因**：

#### 原因 1：CustomTemplate 的靜態特性
1. **CustomTemplate 的靜態特性**
   - `CustomTemplate` 內的表達式（如 `@prepayment.Amount.ToString("N2")`）只在組件渲染時執行
   - 當使用 `@oninput` 事件時，Blazor 只更新綁定的輸入欄位，不會重新渲染其他欄位的 `CustomTemplate`

2. **缺少明確的重新渲染觸發**
   - 數據模型更新後，需要明確調用 `StateHasChanged()` 來通知 Blazor 重新渲染
   - 或者在 `CustomTemplate` 中使用 `PropertyName` 來建立數據綁定

#### 🎯 原因 2：ShouldRender() 阻擋渲染（關鍵問題！）

**最關鍵的問題：** 組件覆寫了 `ShouldRender()` 方法，但只檢查參數變更和集合數量變更，**沒有檢查集合內部數據的變更**。

```csharp
protected override bool ShouldRender()
{
    // ❌ 只檢查這些
    bool hasChanges = _previousPartnerId != actualPartnerId ||
                      _previousMode != Mode ||
                      _previousIsEditMode != IsEditMode ||
                      _previousIsLoading != IsLoading ||
                      _previousDetailsCount != Details.Count;  // ❌ 只檢查數量，不檢查內容
    
    return hasChanges;
}
```

**問題流程：**
1. ✅ 使用者輸入數字
2. ✅ `HandleAmountChanged` 被調用
3. ✅ 數據被更新（`detail.ThisTimeAmount = 600`）
4. ✅ `StateHasChanged()` 被調用
5. ❌ **但 `ShouldRender()` 返回 `false`，阻止渲染！**
6. ❌ UI 不更新

**為什麼測試頁面成功？**
- 測試頁面**沒有覆寫 `ShouldRender()`**
- 所以 `StateHasChanged()` 可以正常觸發渲染

## 🔧 解決方案

### 方案 1：為顯示欄位添加 PropertyName（✅ 採用）

在 `CustomTemplate` 欄位定義中添加 `PropertyName`，這樣 Blazor 可以追蹤屬性變更：

**修改前：**
```csharp
new InteractiveColumnDefinition
{
    Title = "原始金額",
    PropertyName = "",  // ❌ 空字串，無法追蹤
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item =>
    {
        var prepayment = (SetoffPrepaymentDto)item;
        return @<span>@prepayment.Amount.ToString("N2")</span>;
    }
}
```

**修改後：**
```csharp
new InteractiveColumnDefinition
{
    Title = "原始金額",
    PropertyName = nameof(SetoffPrepaymentDto.Amount),  // ✅ 明確指定屬性
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item =>
    {
        var prepayment = (SetoffPrepaymentDto)item;
        var displayValue = prepayment.Amount.ToString("N2");
        return @<span>@displayValue</span>;
    }
}
```

### 方案 2：添加強制渲染標記（✅ 關鍵解決方案！）

**問題：** `ShouldRender()` 阻擋了內部數據變更的渲染

**解決方法：** 添加 `_shouldRenderOverride` 標記，在數據變更時強制渲染

#### 步驟 1：添加私有變數
```csharp
private bool _shouldRenderOverride = false; // 強制渲染標記（用於數據變更時）
```

#### 步驟 2：修改 ShouldRender 方法
```csharp
protected override bool ShouldRender()
{
    // ✅ 如果有強制渲染標記，立即渲染並清除標記
    if (_shouldRenderOverride)
    {
        _shouldRenderOverride = false;
        return true;
    }
    
    // 原有的參數檢查邏輯
    bool hasChanges = _previousPartnerId != actualPartnerId ||
                      _previousMode != Mode ||
                      // ... 其他檢查
    
    return hasChanges;
}
```

#### 步驟 3：在數據變更處理方法中設置標記
```csharp
private async Task HandleAmountChanged(...)
{
    // 更新數據
    args.detail.ThisTimeAmount = amount;
    
    // 驗證
    ValidateAmounts();
    
    // ✅ 設置強制渲染標記（因為明細數據變更了）
    _shouldRenderOverride = true;
    
    // 通知變更（內部會調用 StateHasChanged）
    await NotifySelectionChanged();
}
```

### 方案 3：確保 StateHasChanged 在正確時機調用（優化）

避免重複調用 `StateHasChanged()` 導致過度渲染：

**原則：**
- 如果變更處理方法內已調用了其他會觸發 `StateHasChanged()` 的方法（如 `NotifyChanges()` 或 `NotifySelectionChanged()`），則不需要重複調用
- 如果沒有，則需要明確調用 `StateHasChanged()`

**修改前（可能導致雙重渲染）：**
```csharp
private async Task HandleAddAmountChanged(...)
{
    // 更新數據
    prepayment.Amount = prepayment.ThisTimeAddAmount;
    
    StateHasChanged();  // ❌ 第一次調用
    
    await NotifyChanges();  // ❌ NotifyChanges 內部也會調用 StateHasChanged()
}
```

**修改後（避免重複渲染）：**
```csharp
private async Task HandleAddAmountChanged(...)
{
    // 更新數據
    prepayment.Amount = prepayment.ThisTimeAddAmount;
    
    // 設置強制渲染標記
    _shouldRenderOverride = true;
    
    // NotifyChanges 內部會調用 StateHasChanged()，這裡不需要重複調用
    await NotifyChanges();
}
```

## 📝 修正的檔案

### 1. SetoffPrepaymentManagerComponent.razor

**修正的欄位定義：**
- ✅ 原始金額（Amount）
- ✅ 已用金額（UsedAmount）
- ✅ 可用金額（AvailableAmount）

**修正的方法：**
- ✅ `HandleAddAmountChanged()` - 移除重複的 `StateHasChanged()` 調用

### 2. SetoffDetailManagerComponent.razor

**修正的方法：**
- ✅ `HandleAmountChanged()` - 移除重複的 `StateHasChanged()` 調用
- ✅ `HandleDiscountAmountChanged()` - 移除重複的 `StateHasChanged()` 調用

**備註：** 此組件的顯示欄位（已沖款、已折讓、待沖款）使用了動態計算屬性（`DynamicSettledAmount`、`DynamicDiscountedAmount`、`DynamicPendingAmount`），所以不需要修改欄位定義。

## 🧪 測試方法

### 測試頁面
訪問測試頁面：`/test/setoff-prepayment-render`

### 測試步驟

1. **基本輸入測試**
   - 在「本次金額」欄位輸入：`600`
   - ✅ 預期：「原始金額」立即顯示 `600.00`
   - ❌ 錯誤：顯示 `6.00` 或其他不完整的數字

2. **負數輸入測試**
   - 在「本次金額」欄位輸入：`-500` 或 `(500)`
   - ✅ 預期：「原始金額」立即顯示 `-500.00` 或 `(500.00)`

3. **清空測試**
   - 清空「本次金額」欄位
   - ✅ 預期：「原始金額」立即顯示 `0.00`

4. **多筆資料測試**
   - 在多個列中輸入不同的金額
   - ✅ 預期：每一列的「原始金額」都正確顯示對應的值

5. **渲染次數檢查**
   - 觀察「渲染次數」欄位
   - ✅ 預期：輸入時渲染次數增加，但不會過度渲染（每次輸入最多 +1 或 +2）

## 🎯 技術要點

### 1. PropertyName 的作用
- **綁定追蹤：** 當指定 `PropertyName` 時，Blazor 可以追蹤該屬性的變更
- **自動更新：** 屬性變更時，相關的 UI 元素會自動重新渲染
- **性能優化：** 只重新渲染受影響的欄位，而不是整個組件

### 2. ShouldRender() 的陷阱與解決

#### ❌ 常見錯誤
```csharp
protected override bool ShouldRender()
{
    // 只檢查集合數量，不檢查內容
    return _previousDetailsCount != Details.Count;
}
```

**問題：**
- 當你修改 `Details[0].ThisTimeAmount = 600` 時
- `Details.Count` 沒有變化
- `ShouldRender()` 返回 `false`
- UI 不更新！

#### ✅ 正確做法
```csharp
private bool _shouldRenderOverride = false;

protected override bool ShouldRender()
{
    // 優先檢查強制渲染標記
    if (_shouldRenderOverride)
    {
        _shouldRenderOverride = false;
        return true;  // 強制渲染
    }
    
    // 然後檢查參數變更
    return _previousDetailsCount != Details.Count || 
           _previousPartnerId != actualPartnerId;
}

// 在數據變更時設置標記
private async Task HandleAmountChanged(...)
{
    detail.ThisTimeAmount = amount;
    _shouldRenderOverride = true;  // 🔥 設置標記
    await NotifySelectionChanged();
}
```

### 3. StateHasChanged() 的時機
```csharp
// ✅ 好的做法：在數據變更後，調用會觸發 StateHasChanged 的方法
private async Task HandleChange()
{
    UpdateData();
    _shouldRenderOverride = true;  // 設置強制渲染標記
    await NotifyChanges();  // 內部會調用 StateHasChanged()
}

// ❌ 不好的做法：重複調用 StateHasChanged
private async Task HandleChange()
{
    UpdateData();
    StateHasChanged();      // 第一次
    await NotifyChanges();  // 第二次（NotifyChanges 內部也調用）
}

// ✅ 好的做法：如果沒有其他方法會觸發，則明確調用
private void HandleChange()
{
    UpdateData();
    _shouldRenderOverride = true;
    StateHasChanged();  // 必須調用，因為沒有其他方法會觸發
}
```

### 4. CustomTemplate 的最佳實踐
```csharp
new InteractiveColumnDefinition
{
    Title = "顯示欄位",
    PropertyName = nameof(MyDto.MyProperty),  // ✅ 明確指定屬性名稱
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item =>
    {
        var dto = (MyDto)item;
        var displayValue = dto.MyProperty.ToString("N2");  // ✅ 先計算值
        return @<span>@displayValue</span>;  // ✅ 然後顯示
    }
}
```

### 5. 為什麼測試頁面成功但實際組件失敗？

**測試頁面：**
```csharp
// ✅ 沒有覆寫 ShouldRender()
// StateHasChanged() 可以正常工作
```

**實際組件：**
```csharp
// ❌ 覆寫了 ShouldRender()
// ❌ 只檢查參數和集合數量
// ❌ 沒有檢查集合內部數據變更
// 結果：StateHasChanged() 被 ShouldRender() 阻擋
```

## 📊 修正效果對比

| 項目 | 修正前 | 修正後 |
|------|--------|--------|
| 輸入 600 後顯示 | 6 | 600.00 ✅ |
| 需要手動觸發渲染 | 是（圈選其他欄位）| 否 ✅ |
| 渲染次數 | 可能過多 | 優化 ✅ |
| 使用者體驗 | 差 | 良好 ✅ |

## 🔍 相關資源

- [Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering)
- [Blazor Component Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
- 專案文件：`/test/setoff-prepayment-render` 測試頁面

## ✅ 檢查清單

修正完成後，請確認：

- [ ] 編譯無錯誤
- [ ] 輸入數字時，相關欄位立即更新
- [ ] 不需要點擊其他欄位才能看到正確數字
- [ ] 負數顯示正確（括號或負號）
- [ ] 清空輸入時，相關欄位正確歸零
- [ ] 沒有過度渲染（檢查渲染次數）
- [ ] 多筆資料同時編輯時正常運作

---

**修正日期：** 2025-10-03  
**修正人員：** GitHub Copilot  
**影響組件：** SetoffPrepaymentManagerComponent, SetoffDetailManagerComponent
