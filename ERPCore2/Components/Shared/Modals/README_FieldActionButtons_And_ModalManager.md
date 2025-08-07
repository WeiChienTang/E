# 表單欄位操作按鈕與泛型 Modal 管理器實作指南

## 概述

本指南說明如何在 `GenericEditModalComponent` 中的表單欄位旁邊添加操作按鈕，並使用泛型的 `RelatedEntityModalManager` 來簡化實作。這套系統支援在任何表單欄位旁添加新增/編輯按鈕，大幅減少重複代碼。

## 功能特點

- **泛型設計**：支援任何繼承自 `BaseEntity` 的實體類型
- **自動狀態管理**：自動處理 Modal 的開啟、關閉狀態
- **動態按鈕更新**：根據選擇值自動更新按鈕文字（新增/編輯）
- **標準化事件處理**：統一的事件處理模式
- **Builder 模式**：靈活的配置方式
- **自定義後處理**：支援實體特定的業務邏輯

## 架構說明

### 相關檔案
- `FormFieldDefinition.cs` - 表單欄位定義類別
- `RelatedEntityModalManager.cs` - 泛型 Modal 管理器
- `GenericFormComponent.razor` - 通用表單組件
- `EmployeeEditModalComponent.razor` - 完整實作範例
- `DepartmentEditModalComponent.razor` - 被管理的 Modal 範例

### 核心類別

#### FieldActionButton - 按鈕定義
```csharp
public class FieldActionButton
{
    public string Text { get; set; } = string.Empty;
    public string Variant { get; set; } = "Primary";
    public string Size { get; set; } = "Medium";
    public string? IconClass { get; set; }
    public string? Title { get; set; }
    public bool IsDisabled { get; set; } = false;
    public Func<Task>? OnClick { get; set; }
}
```

#### RelatedEntityModalManager - 泛型管理器
```csharp
public class RelatedEntityModalManager<TEntity> where TEntity : BaseEntity
{
    public bool IsModalVisible { get; private set; }
    public int? SelectedEntityId { get; private set; }
    
    // 核心方法
    public async Task OpenModalAsync(int? entityId);
    public List<FieldActionButton> GenerateActionButtons(int? currentSelectedId);
    public void UpdateFieldActionButtons(List<FormFieldDefinition>? formFields, string propertyName, int? newValue);
    public async Task HandleEntitySavedAsync(TEntity savedEntity, bool shouldAutoSelect = true);
    // ... 其他方法
}
```

## 實作步驟

### 步驟 1：在組件中聲明泛型管理器

**舊方式（繁瑣）：**
```csharp
// 需要為每個相關實體維護多個變數
private bool isDepartmentModalVisible = false;
private int? selectedDepartmentId = null;
private DepartmentEditModalComponent? departmentEditModal;
```

**新方式（簡潔）：**
```csharp
// 只需要一個管理器實例
private DepartmentEditModalComponent? departmentEditModal;
private RelatedEntityModalManager<Department> departmentModalManager = default!;
```

### 步驟 2：初始化管理器（使用 Builder 模式）

```csharp
private void InitializeDepartmentModalManager()
{
    departmentModalManager = new RelatedEntityManagerBuilder<Department>(NotificationService, "部門")
        .WithPropertyName(nameof(Employee.DepartmentId))
        .WithReloadCallback(LoadAdditionalDataAsync)
        .WithStateChangedCallback(StateHasChanged)
        .WithAutoSelectCallback(departmentId => 
        {
            if (editModalComponent?.Entity != null)
            {
                editModalComponent.Entity.DepartmentId = departmentId;
            }
        })
        .WithCustomPostProcess(department => 
        {
            // 實體特定的後處理邏輯（如重新初始化表單）
            InitializeFormFields(editModalComponent?.Entity?.IsSystemUser ?? false);
            return Task.CompletedTask;
        })
        .Build();
}

// 在 OnInitializedAsync 中調用
protected override async Task OnInitializedAsync()
{
    try
    {
        InitializeDepartmentModalManager(); // 初始化管理器
        await LoadAdditionalDataAsync();
        InitializeFormFields(false);
    }
    catch (Exception)
    {
        _ = NotificationService.ShowErrorAsync("初始化組件時發生錯誤");
    }
}
```

### 步驟 3：在表單欄位中設定操作按鈕

```csharp
private void InitializeFormFields()
{
    formFields = new List<FormFieldDefinition>
    {
        // 其他欄位...
        
        new()
        {
            PropertyName = nameof(Employee.DepartmentId),
            Label = "部門",
            FieldType = FormFieldType.Select,
            Placeholder = "請選擇部門",
            Options = departmentOptions,
            HelpText = "選擇員工的部門",
            ActionButtons = GetDepartmentActionButtons() // 使用泛型管理器產生
        }
    };
}

/// <summary>
/// 使用泛型管理器產生部門操作按鈕
/// </summary>
private List<FieldActionButton> GetDepartmentActionButtons()
{
    var currentDepartmentId = editModalComponent?.Entity?.DepartmentId;
    return departmentModalManager.GenerateActionButtons(currentDepartmentId);
}
```

### 步驟 4：修改 Modal 組件綁定

**舊方式：**
```razor
<DepartmentEditModalComponent @ref="departmentEditModal"
                             IsVisible="@isDepartmentModalVisible"
                             IsVisibleChanged="@OnDepartmentModalVisibilityChanged"
                             DepartmentId="@selectedDepartmentId"
                             OnDepartmentSaved="@OnDepartmentSaved"
                             OnCancel="@OnDepartmentModalCancel" />
```

**新方式：**
```razor
<DepartmentEditModalComponent @ref="departmentEditModal"
                             IsVisible="@departmentModalManager.IsModalVisible"
                             IsVisibleChanged="@departmentModalManager.HandleModalVisibilityChangedAsync"
                             DepartmentId="@departmentModalManager.SelectedEntityId"
                             OnDepartmentSaved="@OnDepartmentSavedWrapper"
                             OnCancel="@departmentModalManager.HandleModalCancelAsync" />
```

### 步驟 5：添加包裝器方法

```csharp
/// <summary>
/// 包裝部門儲存事件以符合原有介面
/// </summary>
private async Task OnDepartmentSavedWrapper(Department savedDepartment)
{
    await departmentModalManager.HandleEntitySavedAsync(savedDepartment, shouldAutoSelect: true);
}

/// <summary>
/// 開啟部門編輯 Modal - 使用泛型管理器
/// </summary>
private async Task OpenDepartmentModal(int? departmentId)
{
    await departmentModalManager.OpenModalAsync(departmentId);
}
```

### 步驟 6：修改欄位變更處理

**舊方式（手動處理）：**
```csharp
private Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    if (fieldChange.PropertyName == nameof(Employee.DepartmentId))
    {
        // 手動更新按鈕狀態（約 20+ 行代碼）
        var departmentField = formFields?.FirstOrDefault(f => f.PropertyName == nameof(Employee.DepartmentId));
        if (departmentField != null)
        {
            int? newDepartmentId = null;
            if (fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int deptId))
            {
                newDepartmentId = deptId;
            }
            
            var buttonText = newDepartmentId.HasValue ? "編輯" : "新增";
            var buttonTitle = newDepartmentId.HasValue ? "編輯目前選擇的部門" : "新增新的部門";
            
            departmentField.ActionButtons = new List<FieldActionButton>
            {
                new FieldActionButton
                {
                    Text = buttonText,
                    Variant = "OutlinePrimary",
                    Size = "Small",
                    Title = buttonTitle,
                    OnClick = () => OpenDepartmentModal(newDepartmentId)
                }
            };
            
            StateHasChanged();
        }
    }
    
    return Task.CompletedTask;
}
```

**新方式（自動處理）：**
```csharp
private Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    try
    {
        // 使用泛型管理器自動處理（只需 3 行代碼）
        if (fieldChange.PropertyName == nameof(Employee.DepartmentId))
        {
            departmentModalManager.UpdateFieldActionButtons(formFields, fieldChange.PropertyName, 
                fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int deptId) ? deptId : null);
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

## Builder 模式配置選項

### WithPropertyName(string propertyName)
設定相關的屬性名稱，用於標識欄位。

### WithReloadCallback(Func<Task> reloadCallback)
設定重新載入資料的回調函式，通常指向 `LoadAdditionalDataAsync`。

### WithStateChangedCallback(Action stateChangedCallback)
設定狀態變更的回調函式，通常指向 `StateHasChanged`。

### WithAutoSelectCallback(Action<int> autoSelectCallback)
設定自動選擇新實體的回調函式，用於在新增後自動選擇該實體。

### WithCustomPostProcess(Func<TEntity, Task> customPostProcessCallback)
設定自定義後處理邏輯，用於處理實體特定的業務邏輯。

## 重構前後對比

### 重構前的問題
- **代碼重複**：每個實體需要 3 個狀態變數 + 6-8 個方法
- **維護困難**：相同邏輯散布在多個地方
- **容易出錯**：手動狀態管理容易遺漏邊界情況
- **擴展困難**：新增實體需要大量重複代碼

### 重構後的優勢
- **代碼簡潔**：只需要 1 個管理器實例 + 2-3 個包裝器方法
- **維護容易**：核心邏輯集中在管理器中
- **錯誤減少**：自動化的狀態管理
- **擴展簡單**：新實體只需幾行代碼

### 數據對比

| 項目 | 重構前 | 重構後 | 改善 |
|------|--------|--------|------|
| 狀態變數 | 3個/實體 | 1個/實體 | -67% |
| 方法數量 | 6-8個/實體 | 2-3個/實體 | -70% |
| 代碼行數 | ~80行/實體 | ~15行/實體 | -81% |
| 維護複雜度 | 高 | 低 | 大幅簡化 |

## 擴展其他實體

要為其他實體（如 EmployeePosition、Role 等）添加相同功能：

```csharp
// 1. 聲明管理器
private RelatedEntityModalManager<EmployeePosition> positionModalManager = default!;

// 2. 初始化
private void InitializePositionModalManager()
{
    positionModalManager = new RelatedEntityManagerBuilder<EmployeePosition>(NotificationService, "職位")
        .WithPropertyName(nameof(Employee.EmployeePositionId))
        .WithReloadCallback(LoadAdditionalDataAsync)
        .WithStateChangedCallback(StateHasChanged)
        .WithAutoSelectCallback(positionId => 
        {
            if (editModalComponent?.Entity != null)
            {
                editModalComponent.Entity.EmployeePositionId = positionId;
            }
        })
        .Build();
}

// 3. 在欄位中使用
new FormFieldDefinition
{
    PropertyName = nameof(Employee.EmployeePositionId),
    Label = "職位",
    FieldType = FormFieldType.Select,
    Options = positionOptions,
    ActionButtons = GetPositionActionButtons() // 類似的方法
}

// 4. 在 OnFieldValueChanged 中添加處理
if (fieldChange.PropertyName == nameof(Employee.EmployeePositionId))
{
    positionModalManager.UpdateFieldActionButtons(formFields, fieldChange.PropertyName, 
        fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int posId) ? posId : null);
}
```

## 完整範例

參考 `EmployeeEditModalComponent.razor` 中的部門編輯功能，這是一個完整的工作範例：

1. **主組件**：使用 `RelatedEntityModalManager<Department>` 管理部門相關功能
2. **被管理組件**：`DepartmentEditModalComponent` 作為標準的編輯 Modal
3. **表單整合**：在員工編輯表單的部門欄位旁自動顯示新增/編輯按鈕

## 按鈕樣式配置

### 支援的 Variant 值
- `Primary`, `Secondary`, `Success`, `Danger`, `Warning`, `Info`
- `Light`, `Dark`
- `OutlinePrimary`, `OutlineSecondary` 等

### 支援的 Size 值
- `Small`, `Medium`, `Large`

## 注意事項

### 1. 實體顯示名稱
管理器會自動嘗試使用實體的 `Name` 屬性作為顯示名稱，如果沒有則使用 ID。

### 2. 錯誤處理
所有操作都包含完整的錯誤處理，會通過 `INotificationService` 顯示錯誤訊息。

### 3. 狀態同步
管理器會自動同步 Modal 狀態和表單狀態。

### 4. 性能考量
- 避免在 Builder 配置中執行重量級操作
- 只在必要時更新 ActionButtons
- 合理使用 StateHasChanged()

## 常見問題

### Q: 按鈕不會動態更新？
A: 確保在 `OnFieldValueChanged` 中正確調用 `UpdateFieldActionButtons` 方法。

### Q: Modal 狀態混亂？
A: 使用泛型管理器後，所有狀態都由管理器自動處理，不需要手動管理。

### Q: 如何自定義後處理邏輯？
A: 使用 `WithCustomPostProcess` 配置項添加實體特定的處理邏輯。

### Q: 編譯錯誤？
A: 檢查是否正確引用了 `RelatedEntityModalManager` 和相關命名空間。

## 最佳實踐

1. **命名一致性**：使用一致的命名規則，如 `xxxModalManager`
2. **錯誤處理**：確保所有回調函式都有適當的錯誤處理
3. **代碼組織**：將管理器初始化放在組件初始化階段
4. **配置集中**：使用 Builder 模式集中配置所有選項
5. **測試覆蓋**：為泛型管理器編寫單元測試

## 遷移指南

### 從舊方式遷移到新方式

1. **識別現有的 Modal 管理代碼**
2. **用管理器聲明替換狀態變數**
3. **用 Builder 模式替換初始化邏輯**
4. **用管理器方法替換事件處理方法**
5. **測試功能是否正常**

---

最後更新：2025年8月7日  
作者：ERPCore2 開發團隊
