# 表單欄位操作按鈕實作指南

## 概述

本指南說明如何在 `GenericEditModalComponent` 中的表單欄位旁邊添加操作按鈕，例如在部門選擇欄位旁邊添加「新增/編輯」按鈕。

## 架構說明

### 相關檔案
- `FormFieldDefinition.cs` - 表單欄位定義類別
- `GenericFormComponent.razor` - 通用表單組件
- `EmployeeEditModalComponent.razor` - 範例實作

### 核心類別
```csharp
public class FieldActionButton
{
    public string Text { get; set; } = string.Empty;
    public string Variant { get; set; } = "Primary";
    public string Size { get; set; } = "Medium";
    public string? IconClass { get; set; }
    public string? Title { get; set; }
    public bool IsDisabled { get; set; } = false;
    public Action? OnClick { get; set; }
}
```

## 實作步驟

### 步驟 1：在 FormFieldDefinition 中添加 ActionButtons 屬性

確保 `FormFieldDefinition.cs` 包含以下屬性：

```csharp
/// <summary>
/// 欄位操作按鈕列表
/// </summary>
public List<FieldActionButton>? ActionButtons { get; set; }
```

### 步驟 2：在 GenericFormComponent 中支援按鈕渲染

在 `GenericFormComponent.razor` 的 `RenderField` 方法中添加按鈕渲染邏輯：

```csharp
// 渲染操作按鈕
if (field.ActionButtons != null && field.ActionButtons.Any())
{
    var buttonSequence = 11;
    foreach (var actionButton in field.ActionButtons)
    {
        builder.OpenComponent(buttonSequence, typeof(ERPCore2.Components.Shared.Buttons.GenericButtonComponent));
        builder.AddAttribute(buttonSequence + 1, "Text", actionButton.Text);
        builder.AddAttribute(buttonSequence + 2, "Variant", GetButtonVariant(actionButton.Variant));
        builder.AddAttribute(buttonSequence + 3, "Size", GetButtonSize(actionButton.Size));
        if (!string.IsNullOrEmpty(actionButton.IconClass))
        {
            builder.AddAttribute(buttonSequence + 4, "IconClass", actionButton.IconClass);
        }
        if (!string.IsNullOrEmpty(actionButton.Title))
        {
            builder.AddAttribute(buttonSequence + 5, "Title", actionButton.Title);
        }
        if (actionButton.OnClick != null)
        {
            builder.AddAttribute(buttonSequence + 6, "OnClick", EventCallback.Factory.Create(this, actionButton.OnClick));
        }
        builder.AddAttribute(buttonSequence + 7, "IsDisabled", actionButton.IsDisabled);
        builder.CloseComponent();
        buttonSequence += 8;
    }
}
```

### 步驟 3：在 Modal 組件中實作按鈕邏輯

#### 3.1 添加必要的變數

```csharp
// 相關 Modal 的狀態變數
private bool isRelatedModalVisible = false;
private int? selectedRelatedId = null;
private RelatedEditModalComponent? relatedEditModal;
```

#### 3.2 在 FormFieldDefinition 中設定 ActionButtons

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Employee.DepartmentId),
    Label = "部門",
    FieldType = FormFieldType.Select,
    Placeholder = "請選擇部門",
    Options = departmentOptions,
    HelpText = "選擇員工的部門",
    ActionButtons = GetDepartmentActionButtons() // 設定操作按鈕
}
```

#### 3.3 實作按鈕方法

```csharp
/// <summary>
/// 取得欄位的操作按鈕
/// </summary>
private List<FieldActionButton> GetFieldActionButtons()
{
    // 獲取當前選擇的值
    int? selectedId = GetCurrentSelectedId();
    
    var buttonText = selectedId.HasValue ? "編輯" : "新增";
    var buttonTitle = selectedId.HasValue ? "編輯目前選擇的項目" : "新增新的項目";

    return new List<FieldActionButton>
    {
        new FieldActionButton
        {
            Text = buttonText,
            Variant = "OutlinePrimary",
            Size = "Small",
            Title = buttonTitle,
            OnClick = () => OpenRelatedModal(selectedId)
        }
    };
}

/// <summary>
/// 開啟相關編輯 Modal
/// </summary>
private async Task OpenRelatedModal(int? relatedId)
{
    try
    {
        selectedRelatedId = relatedId;
        isRelatedModalVisible = true;
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"開啟編輯視窗時發生錯誤：{ex.Message}");
    }
}
```

### 步驟 4：實作動態按鈕更新

在 `OnFieldValueChanged` 方法中監聽欄位變更並更新按鈕：

```csharp
private Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    try
    {
        // 監聽目標欄位變更，更新按鈕狀態
        if (fieldChange.PropertyName == nameof(Employee.DepartmentId))
        {
            // 更新欄位的 ActionButtons
            var targetField = formFields?.FirstOrDefault(f => f.PropertyName == nameof(Employee.DepartmentId));
            if (targetField != null)
            {
                // 獲取新的ID值
                int? newId = null;
                if (fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int id))
                {
                    newId = id;
                }
                
                // 更新按鈕
                var buttonText = newId.HasValue ? "編輯" : "新增";
                var buttonTitle = newId.HasValue ? "編輯目前選擇的項目" : "新增新的項目";
                
                targetField.ActionButtons = new List<FieldActionButton>
                {
                    new FieldActionButton
                    {
                        Text = buttonText,
                        Variant = "OutlinePrimary",
                        Size = "Small",
                        Title = buttonTitle,
                        OnClick = () => OpenRelatedModal(newId)
                    }
                };
                
                StateHasChanged();
            }
        }
        
        return Task.CompletedTask;
    }
    catch (Exception)
    {
        _ = NotificationService.ShowErrorAsync("欄位變更處理時發生錯誤");
        return Task.CompletedTask;
    }
}
```

### 步驟 5：處理 Modal 事件

```csharp
/// <summary>
/// Modal 可見性變更事件
/// </summary>
private async Task OnRelatedModalVisibilityChanged(bool isVisible)
{
    try
    {
        isRelatedModalVisible = isVisible;
        if (!isVisible)
        {
            selectedRelatedId = null;
        }
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理視窗狀態變更時發生錯誤：{ex.Message}");
    }
}

/// <summary>
/// 儲存成功事件
/// </summary>
private async Task OnRelatedSaved(RelatedEntity savedEntity)
{
    try
    {
        // 重新載入選項
        await LoadAdditionalDataAsync();
        InitializeFormFields(editModalComponent?.Entity?.IsSystemUser ?? false);

        // 如果是新增，自動選擇新項目
        if (editModalComponent?.Entity != null && !editModalComponent.Entity.RelatedId.HasValue)
        {
            editModalComponent.Entity.RelatedId = savedEntity.Id;
            StateHasChanged();
        }

        // 關閉 Modal
        isRelatedModalVisible = false;
        selectedRelatedId = null;

        await NotificationService.ShowSuccessAsync($"項目「{savedEntity.Name}」已成功儲存");
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理儲存成功事件時發生錯誤：{ex.Message}");
    }
}
```

### 步驟 6：在 Razor 中添加相關 Modal

```razor
@* 相關編輯 Modal *@
<RelatedEditModalComponent @ref="relatedEditModal"
                          IsVisible="@isRelatedModalVisible"
                          IsVisibleChanged="@OnRelatedModalVisibilityChanged"
                          RelatedId="@selectedRelatedId"
                          OnRelatedSaved="@OnRelatedSaved"
                          OnCancel="@OnRelatedModalCancel" />
```

## 注意事項

### 1. 按鈕樣式變體
支援的 Variant 值：
- `Primary`
- `Secondary`
- `Success`
- `Danger`
- `Warning`
- `Info`
- `Light`
- `Dark`
- `OutlinePrimary`
- `OutlineSecondary`
- 等等...

### 2. 按鈕大小
支援的 Size 值：
- `Small`
- `Medium`
- `Large`

### 3. 動態更新
- 必須在 `OnFieldValueChanged` 中監聽欄位變更
- 更新按鈕後要調用 `StateHasChanged()`
- 按鈕的 `OnClick` 事件要重新設定

### 4. 錯誤處理
- 所有異步操作都要包含 try-catch
- 使用 NotificationService 顯示錯誤訊息
- 確保 Modal 狀態正確重置

### 5. 效能考量
- 避免在每次渲染時都重新創建按鈕
- 只在必要時更新 ActionButtons
- 合理使用 StateHasChanged()

## 常見問題

### Q: 按鈕不會動態更新？
A: 確保在 `OnFieldValueChanged` 中正確監聽欄位變更，並更新對應欄位的 ActionButtons 屬性。

### Q: 按鈕點擊沒有反應？
A: 檢查 OnClick 事件是否正確設定，確保方法是 async Task 類型。

### Q: Modal 狀態混亂？
A: 確保在 Modal 關閉時正確重置所有相關變數。

### Q: 編譯錯誤？
A: 檢查是否正確引用了必要的 using 語句和命名空間。

## 範例實作

參考 `EmployeeEditModalComponent.razor` 中的部門編輯按鈕實作，這是一個完整的工作範例。

## 最佳實踐

1. **命名一致性**：使用一致的命名規則
2. **錯誤處理**：所有操作都要有適當的錯誤處理
3. **狀態管理**：確保 Modal 狀態正確管理
4. **用戶體驗**：提供清楚的按鈕標籤和工具提示
5. **程式碼重用**：將通用邏輯抽取成可重用的方法

---

最後更新：2025年8月7日
作者：ERPCore2 開發團隊
