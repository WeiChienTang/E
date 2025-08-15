# Modal 組件轉換指南

本指南說明如何將現有的 Edit UI 轉換為使用 `GenericEditModalComponent` 的新模式。

## 完整範例參考
- **實際轉換案例**: `RoleIndex.razor` + `RoleEditModalComponent.razor`

## 重要注意事項

⚠️ **常見錯誤提醒**：
1. **GenericIndexPageComponent 參數**：不存在 `ShowAddButton` 和 `ShowEditButton` 參數
2. **正確的事件處理**：使用 `OnAddClick` 和 `OnRowClick` 參數
3. **using 語句**：由於 `_Imports.razor` 已包含所有必要的 using，組件中無需重複宣告

## 轉換步驟

### 1. 新增 Modal 組件

在 `Components/Shared/Modals/` 目錄下新增 `{Entity}EditModalComponent.razor` 檔案。

#### 必要結構：

```razor
@* 可重用的{實體}編輯組件 - 可在任何頁面中嵌入 *@
@* 注意：using 語句已在 _Imports.razor 中定義，無需重複宣告 *@
@inject I{Entity}Service {Entity}Service
@inject INotificationService NotificationService

<GenericEditModalComponent TEntity="{Entity}" 
                          TService="I{Entity}Service"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@{Entity}Id"
                          Service="@{Entity}Service"
                          EntityName="{實體中文名稱}"
                          EntityNamePlural="{實體中文名稱}"
                          ModalTitle="@({Entity}Id.HasValue ? "編輯{實體中文名稱}" : "新增{實體中文名稱}")"
                          Size="GenericEditModalComponent<{Entity}, I{Entity}Service>.ModalSize.Desktop"
                          UseGenericForm="true"
                          FormFields="@GetFormFields()"
                          FormSections="@formSections"
                          DataLoader="@Load{Entity}Data"
                          AdditionalDataLoader="@LoadAdditionalDataAsync"
                          UseGenericSave="true"
                          SaveSuccessMessage="@({Entity}Id.HasValue ? "{實體中文名稱}更新成功" : "{實體中文名稱}新增成功")"
                          SaveFailureMessage="{實體中文名稱}儲存失敗"
                          RequiredPermission="{Entity}.Read"
                          OnSaveSuccess="@HandleSaveSuccess"
                          OnCancel="@HandleCancel" />

@code {
    // ===== 必要參數 =====
    [Parameter] public bool IsVisible { get; set; } = false;
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public int? {Entity}Id { get; set; }
    [Parameter] public EventCallback<{Entity}> On{Entity}Saved { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    // ===== 內部狀態 =====
    private GenericEditModalComponent<{Entity}, I{Entity}Service>? editModalComponent;
    private List<FormFieldDefinition> formFields = new();
    private Dictionary<string, string> formSections = new();
    
    // 其他需要的變數...

    // ===== 必要方法 =====
    protected override async Task OnParametersSetAsync() { ... }
    private async Task HandleSaveSuccess() { ... }
    private async Task HandleCancel() { ... }
    private async Task CloseModal() { ... }
    private async Task Load{Entity}Data() { ... }
    private async Task LoadAdditionalDataAsync() { ... }
    private void InitializeFormFields() { ... }
    private List<FormFieldDefinition> GetFormFields() { ... }
}
```

#### 關鍵實作要點：

1. **表單欄位定義** (`InitializeFormFields`)：
   ```csharp
   formFields = new List<FormFieldDefinition>
   {
       new()
       {
           PropertyName = nameof({Entity}.PropertyName),
           Label = "欄位標籤",
           FieldType = FormFieldType.Text, // Text, Select, TextArea, Date 等
           Placeholder = "請輸入...",
           IsRequired = true,
           HelpText = "說明文字"
       }
   };
   ```

2. **表單區段定義** (`formSections`)：
   ```csharp
   formSections = new Dictionary<string, string>
   {
       { nameof({Entity}.Property1), "基本資訊" },
       { nameof({Entity}.Property2), "基本資訊" },
       { nameof({Entity}.Property3), "額外資料" }
   };
   ```

3. **資料載入方法**：
   ```csharp
   private async Task<{Entity}?> Load{Entity}Data()
   {
       if (!{Entity}Id.HasValue) 
       {
           // 新增模式
           return new {Entity}
           {
               // 預設值設定
               Status = EntityStatus.Active
           };
       }
       
       // 編輯模式
       return await {Entity}Service.GetByIdAsync({Entity}Id.Value);
   }
   ```

### 2. 修改 Index 頁面

#### GenericIndexPageComponent 正確參數配置：

**⚠️ 重要**：`GenericIndexPageComponent` 沒有 `ShowAddButton` 和 `ShowEditButton` 參數！

**正確的事件處理參數**：
```razor
<GenericIndexPageComponent TEntity="{Entity}" 
                          TService="I{Entity}Service"
                          Service="@{Entity}Service"
                          EntityBasePath="/{entities}"
                          PageTitle="{實體中文名稱}"
                          PageSubtitle="管理{實體中文名稱}相關設定"
                          EntityName="{實體中文名稱}"
                          BreadcrumbItems="@breadcrumbItems"
                          FilterDefinitions="@filterDefinitions"
                          ColumnDefinitions="@columnDefinitions"
                          DataLoader="@Load{Entity}sAsync"
                          FilterApplier="@Apply{Entity}Filters"
                          GetEntityDisplayName="@(entity => entity.Name)"
                          RequiredPermission="{Entity}.Read"
                          OnAddClick="@ShowAddModal"
                          OnRowClick="@ShowEditModal"
                          @ref="indexComponent" />
```

#### 需要修改的部分：

1. **Modal 組件引用**：
   ```razor
   @* 將舊的 Modal 替換為新的組件 *@
   <{Entity}EditModalComponent IsVisible="@showEditModal"
                              IsVisibleChanged="@((bool visible) => showEditModal = visible)"
                              {Entity}Id="@editing{Entity}Id"
                              On{Entity}Saved="@On{Entity}Saved"
                              OnCancel="@OnModalCancel" />
   ```

2. **Modal 狀態變數**：
   ```csharp
   // Modal 相關狀態
   private bool showEditModal = false;
   private int? editing{Entity}Id = null;
   ```

   3. **Modal 相關方法**：
   ```csharp
   // ===== Modal 相關方法 =====
   private Task ShowAddModal()
   {
       editing{Entity}Id = null;
       showEditModal = true;
       StateHasChanged();
       return Task.CompletedTask;
   }

   private Task ShowEditModal({Entity} entity)
   {
       if (entity?.Id != null)
       {
           editing{Entity}Id = entity.Id;
           showEditModal = true;
           StateHasChanged();
       }
       return Task.CompletedTask;
   }

   private async Task On{Entity}Saved({Entity} saved{Entity})
   {
       showEditModal = false;
       editing{Entity}Id = null;
       
       if (indexComponent != null)
       {
           await indexComponent.Refresh();
       }
       
       StateHasChanged();
   }

   private Task OnModalCancel()
   {
       showEditModal = false;
       editing{Entity}Id = null;
       StateHasChanged();
       return Task.CompletedTask;
   }
   ```

#### 完整的 Index 頁面 @code 區段範例：

```csharp
@code {
    // 組件參考
    private GenericIndexPageComponent<{Entity}, I{Entity}Service> indexComponent = default!;
    
    // 配置相關
    private List<SearchFilterDefinition> filterDefinitions = new();
    private List<TableColumnDefinition> columnDefinitions = new();
    private List<GenericHeaderComponent.BreadcrumbItem> breadcrumbItems = new();

    // Modal 相關狀態
    private bool showEditModal = false;
    private int? editing{Entity}Id = null;

    protected override void OnInitialized()
    {
        try
        {
            InitializeBreadcrumbs();
            InitializeFilters();
            InitializeTableColumns();
        }
        catch (Exception ex)
        {
            _ = ErrorHandlingHelper.HandlePageErrorAsync(
                ex,
                nameof(OnInitialized),
                GetType(),
                additionalData: new { PageName = "{Entity}Index" }
            );
        }
    }

    // 資料載入
    private async Task<List<{Entity}>> Load{Entity}sAsync()
    {
        try
        {
            return await {Entity}Service.GetAllAsync();
        }
        catch (Exception ex)
        {
            _ = ErrorHandlingHelper.HandlePageErrorAsync(
                ex,
                nameof(Load{Entity}sAsync),
                GetType(),
                additionalData: new { PageName = "{Entity}Index" }
            );
            return new List<{Entity}>();
        }
    }

    // Modal 相關方法
    private Task ShowAddModal()
    {
        editing{Entity}Id = null;
        showEditModal = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task ShowEditModal({Entity} entity)
    {
        if (entity?.Id != null)
        {
            editing{Entity}Id = entity.Id;
            showEditModal = true;
            StateHasChanged();
        }
        return Task.CompletedTask;
    }

    private async Task On{Entity}Saved({Entity} saved{Entity})
    {
        showEditModal = false;
        editing{Entity}Id = null;
        
        if (indexComponent != null)
        {
            await indexComponent.Refresh();
        }
        
        StateHasChanged();
    }

    private Task OnModalCancel()
    {
        showEditModal = false;
        editing{Entity}Id = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    // 其他必要方法：InitializeBreadcrumbs, InitializeFilters, InitializeTableColumns, Apply{Entity}Filters
}
```

### 3. 常見的表單欄位類型

```csharp
// 文字輸入
new() {
    PropertyName = nameof(Entity.Name),
    Label = "名稱",
    FieldType = FormFieldType.Text,
    Placeholder = "請輸入名稱",
    IsRequired = true,
    MaxLength = 50,
    HelpText = "說明文字"
}

// 下拉選單
new() {
    PropertyName = nameof(Entity.CategoryId),
    Label = "分類",
    FieldType = FormFieldType.Select,
    Options = categoryOptions, // List<SelectOption>
    IsRequired = true,
    HelpText = "請選擇分類"
}

// 文字區域
new() {
    PropertyName = nameof(Entity.Description),
    Label = "描述",
    FieldType = FormFieldType.TextArea,
    Placeholder = "請輸入描述",
    Rows = 3,
    MaxLength = 200,
    HelpText = "詳細描述"
}

// 日期選擇
new() {
    PropertyName = nameof(Entity.Date),
    Label = "日期",
    FieldType = FormFieldType.Date,
    IsRequired = true
}

// 數字輸入
new() {
    PropertyName = nameof(Entity.Amount),
    Label = "金額",
    FieldType = FormFieldType.Number,
    IsRequired = true,
    HelpText = "請輸入金額"
}

// 核取方塊
new() {
    PropertyName = nameof(Entity.IsActive),
    Label = "啟用狀態",
    FieldType = FormFieldType.Checkbox,
    HelpText = "是否啟用此項目"
}
```

### 4. 疑難排解

#### 常見編譯錯誤：

1. **錯誤**：`GenericIndexPageComponent does not contain a definition for 'ShowAddButton'`
   **解決**：移除 `ShowAddButton` 和 `ShowEditButton` 參數，使用 `OnAddClick` 和 `OnRowClick`

2. **錯誤**：Using 語句重複
   **解決**：移除組件中的 using 語句，因為 `_Imports.razor` 已包含

3. **錯誤**：Modal 不顯示或無法關閉
   **解決**：確認 `IsVisible` 和 `IsVisibleChanged` 參數正確綁定

4. **錯誤**：表單欄位不顯示
   **解決**：確認 `FormFieldDefinition` 的 `PropertyName` 與實體屬性名稱完全一致

#### 除錯提示：

- 使用瀏覽器開發者工具檢查是否有 JavaScript 錯誤
- 檢查 `GenericEditModalComponent` 的 `FormFields` 參數是否正確傳遞
- 確認 Service 注入是否正確
- 檢查權限設定是否正確

## 實際轉換案例：Role 實體

### RoleEditModalComponent.razor（完整範例）

```razor
@* 可重用的角色編輯組件 - 可在任何頁面中嵌入 *@
@inject IRoleService RoleService
@inject INotificationService NotificationService

<GenericEditModalComponent TEntity="Role" 
                          TService="IRoleService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@RoleId"
                          Service="@RoleService"
                          EntityName="角色"
                          EntityNamePlural="角色"
                          ModalTitle="@(RoleId.HasValue ? "編輯角色" : "新增角色")"
                          Size="GenericEditModalComponent<Role, IRoleService>.ModalSize.Desktop"
                          UseGenericForm="true"
                          FormFields="@GetFormFields()"
                          FormSections="@formSections"
                          DataLoader="@LoadRoleData"
                          AdditionalDataLoader="@LoadAdditionalDataAsync"
                          UseGenericSave="true"
                          SaveSuccessMessage="@(RoleId.HasValue ? "角色更新成功" : "角色新增成功")"
                          SaveFailureMessage="角色儲存失敗"
                          RequiredPermission="Role.Read"
                          OnSaveSuccess="@HandleSaveSuccess"
                          OnCancel="@HandleCancel" />

@code {
    [Parameter] public bool IsVisible { get; set; } = false;
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public int? RoleId { get; set; }
    [Parameter] public EventCallback<Role> OnRoleSaved { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private GenericEditModalComponent<Role, IRoleService>? editModalComponent;
    private List<FormFieldDefinition> formFields = new();
    private Dictionary<string, string> formSections = new();

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            await LoadAdditionalDataAsync();
            InitializeFormFields();
        }
    }

    private async Task<Role?> LoadRoleData()
    {
        try
        {
            if (!RoleId.HasValue)
            {
                return new Role
                {
                    Status = EntityStatus.Active,
                    IsSystemRole = false
                };
            }
            return await RoleService.GetByIdAsync(RoleId.Value);
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"載入角色資料時發生錯誤：{ex.Message}");
            return null;
        }
    }

    private void InitializeFormFields()
    {
        formFields = new List<FormFieldDefinition>
        {
            new()
            {
                PropertyName = nameof(Role.RoleName),
                Label = "角色名稱",
                FieldType = FormFieldType.Text,
                Placeholder = "請輸入角色名稱",
                IsRequired = true,
                MaxLength = 50,
                HelpText = "角色的唯一名稱，用於區別不同的系統權限群組"
            },
            new()
            {
                PropertyName = nameof(Role.IsSystemRole),
                Label = "系統角色",
                FieldType = FormFieldType.Checkbox,
                HelpText = "系統角色無法刪除且權限受限制"
            },
            new()
            {
                PropertyName = nameof(Role.Remarks),
                Label = "備註",
                FieldType = FormFieldType.TextArea,
                Placeholder = "請輸入備註",
                IsRequired = false,
                MaxLength = 500,
                Rows = 2,
                HelpText = "角色的額外說明或注意事項",
                ContainerCssClass = "col-12"
            }
        };

        formSections = new Dictionary<string, string>
        {
            { nameof(Role.RoleName), "基本資訊" },
            { nameof(Role.IsSystemRole), "基本資訊" },
            { nameof(Role.Description), "額外資訊" },
            { nameof(Role.Remarks), "額外資訊" }
        };
    }

    // 其他必要方法：HandleSaveSuccess, HandleCancel, CloseModal, LoadAdditionalDataAsync, GetFormFields
}
```

### RoleIndex.razor（關鍵部分）

```razor
@page "/roles"
@inject IRoleService RoleService
@rendermode InteractiveServer

<GenericIndexPageComponent TEntity="Role" 
                          TService="IRoleService"
                          Service="@RoleService"
                          EntityBasePath="/roles"
                          PageTitle="角色設定"
                          OnAddClick="@ShowAddModal"
                          OnRowClick="@ShowEditModal"
                          @ref="indexComponent" />

<RoleEditModalComponent IsVisible="@showEditModal"
                        IsVisibleChanged="@((bool visible) => showEditModal = visible)"
                        RoleId="@editingRoleId"
                        OnRoleSaved="@OnRoleSaved"
                        OnCancel="@OnModalCancel" />

@code {
    private GenericIndexPageComponent<Role, IRoleService> indexComponent = default!;
    private bool showEditModal = false;
    private int? editingRoleId = null;

    private Task ShowAddModal()
    {
        editingRoleId = null;
        showEditModal = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task ShowEditModal(Role role)
    {
        if (role?.Id != null)
        {
            editingRoleId = role.Id;
            showEditModal = true;
            StateHasChanged();
        }
        return Task.CompletedTask;
    }

    private async Task OnRoleSaved(Role savedRole)
    {
        showEditModal = false;
        editingRoleId = null;
        
        if (indexComponent != null)
        {
            await indexComponent.Refresh();
        }
        
        StateHasChanged();
    }

    private Task OnModalCancel()
    {
        showEditModal = false;
        editingRoleId = null;
        StateHasChanged();
        return Task.CompletedTask;
    }
}
```