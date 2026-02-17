# EditModal 設計

## 更新日期
2026-02-17

---

## 概述

EditModal 是每個業務實體的新增/編輯表單，使用 `GenericEditModalComponent<TEntity, TService>` 提供完整的 CRUD 操作介面，包括資料載入、表單渲染、驗證、儲存、上下筆導航等功能。

---

## 檔案結構

| 檔案 | 路徑 | 說明 |
|------|------|------|
| EditModal | `Components/Pages/{Module}/{Entity}EditModalComponent.razor` | 編輯 Modal 元件 |

---

## 完整範例

```razor
@inject IYourEntityService YourEntityService
@inject IRelatedService RelatedService
@inject INotificationService NotificationService
@inject ActionButtonHelper ActionButtonHelper

<GenericEditModalComponent TEntity="YourEntity"
                          TService="IYourEntityService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@YourEntityId"
                          Service="@YourEntityService"
                          EntityName="實體"
                          ModalTitle="@(YourEntityId.HasValue ? "編輯實體" : "新增實體")"
                          Size="GenericEditModalComponent<YourEntity, IYourEntityService>.ModalSize.Desktop"
                          UseGenericForm="true"
                          FormFields="@GetFormFields()"
                          FormSections="@formSections"
                          TabDefinitions="@tabDefinitions"
                          AutoCompletePrefillers="@autoCompleteConfig?.Prefillers"
                          AutoCompleteCollections="@autoCompleteConfig?.Collections"
                          AutoCompleteDisplayProperties="@autoCompleteConfig?.DisplayProperties"
                          AutoCompleteValueProperties="@autoCompleteConfig?.ValueProperties"
                          ModalManagers="@GetModalManagers()"
                          DataLoader="@LoadEntityData"
                          UseGenericSave="true"
                          SaveSuccessMessage="@(YourEntityId.HasValue ? "更新成功" : "新增成功")"
                          SaveFailureMessage="儲存失敗"
                          RequiredPermission="YourEntity.Read"
                          OnSaveSuccess="@HandleSaveSuccess"
                          OnCancel="@HandleCancel"
                          OnFieldChanged="@OnFieldValueChanged">
</GenericEditModalComponent>

@* 關聯實體編輯 Modal *@
<RelatedEditModalComponent @ref="relatedEditModal"
                           IsVisible="@relatedModalManager.IsModalVisible"
                           IsVisibleChanged="@relatedModalManager.HandleModalVisibilityChangedAsync"
                           RelatedId="@relatedModalManager.SelectedEntityId"
                           OnRelatedSaved="@OnRelatedSavedWrapper"
                           OnCancel="@relatedModalManager.HandleModalCancelAsync" />

@code {
    // ===== 參數 =====
    [Parameter] public bool IsVisible { get; set; } = false;
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public int? YourEntityId { get; set; }
    [Parameter] public EventCallback<YourEntity> OnYourEntitySaved { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    // ===== 內部狀態 =====
    private GenericEditModalComponent<YourEntity, IYourEntityService>? editModalComponent;
    private List<FormFieldDefinition> formFields = new();
    private Dictionary<string, string> formSections = new();
    private List<FormTabDefinition>? tabDefinitions;

    // AutoComplete 配置
    private AutoCompleteConfig? autoCompleteConfig;

    // 選項資料
    private List<RelatedEntity> availableRelatedEntities = new();

    // Modal Manager
    private ModalManagerCollection? modalManagers;
    private RelatedEditModalComponent? relatedEditModal;
    private RelatedEntityModalManager<RelatedEntity> relatedModalManager = default!;

    // Lazy Loading 旗標
    private bool isDataLoaded = false;

    // ===== 初始化 =====
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 使用 ModalManagerInitHelper 初始化
            modalManagers = ModalManagerInitHelper
                .CreateBuilder<YourEntity, IYourEntityService>(
                    () => editModalComponent,
                    NotificationService,
                    StateHasChanged,
                    LoadAdditionalDataAsync,
                    InitializeFormFieldsAsync)
                .AddManager<RelatedEntity>(nameof(YourEntity.RelatedEntityId), "關聯實體")
                .Build();

            relatedModalManager = modalManagers.Get<RelatedEntity>(
                nameof(YourEntity.RelatedEntityId));
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("初始化組件時發生錯誤");
        }
    }

    // ===== Lazy Loading（重要！）=====
    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && !isDataLoaded)
        {
            await LoadAdditionalDataAsync();
            await InitializeFormFieldsAsync();
            isDataLoaded = true;
        }
        else if (!IsVisible)
        {
            isDataLoaded = false;
        }
    }

    // ===== 表單欄位初始化 =====
    private async Task InitializeFormFieldsAsync()
    {
        try
        {
            formFields = new List<FormFieldDefinition>
            {
                new()
                {
                    PropertyName = nameof(YourEntity.Code),
                    Label = "編號",
                    FieldType = FormFieldType.Text,
                    Placeholder = "請輸入編號",
                    IsRequired = true,
                    MaxLength = 20
                },
                new()
                {
                    PropertyName = nameof(YourEntity.Name),
                    Label = "名稱",
                    FieldType = FormFieldType.Text,
                    Placeholder = "請輸入名稱",
                    IsRequired = true,
                    MaxLength = 50
                },
                new()
                {
                    PropertyName = nameof(YourEntity.RelatedEntityId),
                    Label = "關聯實體",
                    FieldType = FormFieldType.AutoComplete,
                    Placeholder = "請輸入或選擇",
                    MinSearchLength = 0,
                    ActionButtons = await GetRelatedActionButtonsAsync()
                },
                // 使用 Helper 建立常用欄位
                FormFieldConfigurationHelper.CreateRemarksField<YourEntity>()
            };

            // 使用 FormSectionHelper 定義區段與 Tab
            var layout = FormSectionHelper<YourEntity>.Create()
                .AddToSection(FormSectionNames.BasicInfo,
                    e => e.Code,
                    e => e.Name,
                    e => e.RelatedEntityId)
                .AddToSection(FormSectionNames.AdditionalData,
                    e => e.Remarks)
                .GroupIntoTab("基本資料", "bi-info-circle",
                    FormSectionNames.BasicInfo, FormSectionNames.AdditionalData)
                .BuildAll();

            formSections = layout.FieldSections;
            tabDefinitions = layout.TabDefinitions;
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("初始化表單欄位時發生錯誤");
        }
    }

    // ===== 資料載入 =====
    private async Task<YourEntity?> LoadEntityData()
    {
        try
        {
            if (!YourEntityId.HasValue)
            {
                // 新增模式
                return new YourEntity
                {
                    Code = await EntityCodeGenerationHelper
                        .GenerateForEntity<YourEntity, IYourEntityService>(
                            YourEntityService, "ENT"),
                    Status = EntityStatus.Active
                };
            }
            // 編輯模式
            return await YourEntityService.GetByIdAsync(YourEntityId.Value);
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"載入資料時發生錯誤：{ex.Message}");
            return null;
        }
    }

    // ===== 額外資料載入 =====
    private async Task LoadAdditionalDataAsync()
    {
        try
        {
            availableRelatedEntities = await RelatedService.GetAllAsync();

            autoCompleteConfig = new AutoCompleteConfigBuilder<YourEntity>()
                .AddField(nameof(YourEntity.RelatedEntityId), "Name",
                    availableRelatedEntities)
                .Build();
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("載入相關資料時發生錯誤");
        }
    }

    // ===== Modal Manager 配置 =====
    private Dictionary<string, object> GetModalManagers()
    {
        return new Dictionary<string, object>
        {
            { nameof(YourEntity.RelatedEntityId), relatedModalManager }
        };
    }

    // ===== ActionButton =====
    private async Task<List<FieldActionButton>> GetRelatedActionButtonsAsync()
    {
        return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
            editModalComponent, relatedModalManager,
            nameof(YourEntity.RelatedEntityId));
    }

    // ===== 欄位變更處理 =====
    private async Task OnFieldValueChanged(
        (string PropertyName, object? Value) fieldChange)
    {
        try
        {
            if (fieldChange.PropertyName == nameof(YourEntity.RelatedEntityId))
            {
                await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                    relatedModalManager, formFields,
                    fieldChange.PropertyName, fieldChange.Value);
            }
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("欄位變更處理時發生錯誤");
        }
    }

    // ===== 儲存與取消 =====
    private async Task HandleSaveSuccess()
    {
        if (editModalComponent?.Entity != null)
            await OnYourEntitySaved.InvokeAsync(editModalComponent.Entity);
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsVisibleChanged.InvokeAsync(false);
    }

    // ===== 關聯實體儲存包裝 =====
    private async Task OnRelatedSavedWrapper(RelatedEntity saved)
    {
        await relatedModalManager.HandleEntitySavedAsync(saved, shouldAutoSelect: true);
    }
}
```

---

## 重要設計模式

### 1. Lazy Loading 模式（必須遵守）

```csharp
private bool isDataLoaded = false;

// OnInitializedAsync 中只初始化 Manager，不載入資料
protected override async Task OnInitializedAsync()
{
    modalManagers = ModalManagerInitHelper.CreateBuilder...;
    // ❌ 不要在這裡呼叫 LoadAdditionalDataAsync
    // ❌ 不要在這裡呼叫 InitializeFormFieldsAsync
}

// OnParametersSetAsync 中實作 Lazy Loading
protected override async Task OnParametersSetAsync()
{
    if (IsVisible && !isDataLoaded)
    {
        await LoadAdditionalDataAsync();
        await InitializeFormFieldsAsync();
        isDataLoaded = true;
    }
    else if (!IsVisible)
    {
        isDataLoaded = false; // 關閉時重置
    }
}
```

> **注意**：不要在 `GenericEditModalComponent` 上設定 `AdditionalDataLoader` 參數，否則會導致重複載入。

### 2. ModalManagerInitHelper - 關聯實體管理

```csharp
modalManagers = ModalManagerInitHelper
    .CreateBuilder<MainEntity, IMainService>(
        () => editModalComponent,        // Modal 組件參考
        NotificationService,             // 通知服務
        StateHasChanged,                 // 狀態變更回呼
        LoadAdditionalDataAsync,         // 資料重新載入
        InitializeFormFieldsAsync)       // 表單重新初始化
    .AddManager<Employee>(nameof(MainEntity.EmployeeId), "員工")
    .AddManager<PaymentMethod>(nameof(MainEntity.PaymentMethodId), "付款方式")
    .Build();

// 取得個別 Manager
employeeModalManager = modalManagers.Get<Employee>(nameof(MainEntity.EmployeeId));
```

### 3. AutoCompleteConfigBuilder - AutoComplete 配置

```csharp
// 基本用法
autoCompleteConfig = new AutoCompleteConfigBuilder<MainEntity>()
    .AddField(nameof(MainEntity.EmployeeId), "Name", availableEmployees)
    .AddField(nameof(MainEntity.PaymentMethodId), "Name", availablePaymentMethods)
    .Build();

// 複合搜尋（搜尋多個欄位）
autoCompleteConfig = new AutoCompleteConfigBuilder<MainEntity>()
    .AddFieldWithMultipleSearchProperties<Customer>(
        nameof(MainEntity.CustomerId), "CompanyName", availableCustomers,
        new[] { "CompanyName", "TaxNumber" })
    .Build();

// 條件式配置
autoCompleteConfig = new AutoCompleteConfigBuilder<MainEntity>()
    .AddFieldIf(hasPermission,
        nameof(MainEntity.ApprovedById), "Name", availableEmployees)
    .Build();
```

### 4. EntityCodeGenerationHelper - 自動產生編號

```csharp
var newEntity = new YourEntity
{
    Code = await EntityCodeGenerationHelper
        .GenerateForEntity<YourEntity, IYourEntityService>(
            YourEntityService, "ENT"),  // 前綴
    Status = EntityStatus.Active
};
```

### 5. ActionButtonHelper - 欄位操作按鈕

```csharp
// 產生按鈕（新增/編輯關聯實體）
private async Task<List<FieldActionButton>> GetRelatedActionButtonsAsync()
{
    return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
        editModalComponent, relatedModalManager,
        nameof(MainEntity.RelatedEntityId));
}

// 欄位值變更時更新按鈕狀態
private async Task OnFieldValueChanged(
    (string PropertyName, object? Value) fieldChange)
{
    if (fieldChange.PropertyName == nameof(MainEntity.RelatedEntityId))
    {
        await ActionButtonHelper.UpdateFieldActionButtonsAsync(
            relatedModalManager, formFields,
            fieldChange.PropertyName, fieldChange.Value);
    }
}
```

---

## FormSectionHelper - 表單區段與 Tab 佈局

### 無 Tab（水平並排區段）

```csharp
formSections = FormSectionHelper<YourEntity>.Create()
    .AddToSection(FormSectionNames.BasicInfo,
        e => e.Code, e => e.Name)
    .AddToSection(FormSectionNames.ContactInfo,
        e => e.Phone, e => e.Email)
    .Build(); // 回傳 Dictionary<string, string>
```

### 有 Tab（Tab → Section(s) → Field(s)）

```csharp
var layout = FormSectionHelper<YourEntity>.Create()
    .AddToSection(FormSectionNames.BasicInfo,
        e => e.Code, e => e.Name)
    .AddToSection(FormSectionNames.ContactInfo,
        e => e.Phone, e => e.Email)
    .AddToSection(FormSectionNames.AdditionalData,
        e => e.Remarks)
    // 一個 Tab 可以包含多個 Section
    .GroupIntoTab("基本資料", "bi-person-fill",
        FormSectionNames.BasicInfo, FormSectionNames.ContactInfo)
    .GroupIntoTab("其他", "bi-file-text",
        FormSectionNames.AdditionalData)
    .BuildAll(); // 回傳 FormLayoutResult

formSections = layout.FieldSections;       // Dictionary<string, string>
tabDefinitions = layout.TabDefinitions;    // List<FormTabDefinition>
```

### 條件式區段

```csharp
var layout = FormSectionHelper<Employee>.Create()
    .AddToSection(FormSectionNames.BasicInfo, e => e.Code, e => e.Name)
    .AddIf(hasPermission, FormSectionNames.AccountInfo,
        e => e.Account, e => e.Password, e => e.RoleId)
    .AddCustomFields("篩選區", "FilterProductId", "FilterCategory")
    .BuildAll();
```

---

## 常用表單欄位 Helper

```csharp
// 備註欄位
FormFieldConfigurationHelper.CreateRemarksField<YourEntity>()

// 編號欄位
FormFieldConfigurationHelper.CreateCodeField<YourEntity>("客戶編號", "CUST")

// 狀態欄位
FormFieldConfigurationHelper.CreateStatusField<YourEntity>()
```

---

## 開發檢查清單

- [ ] 使用 `GenericEditModalComponent<TEntity, TService>`
- [ ] 實作 Lazy Loading（`OnParametersSetAsync` + `isDataLoaded`）
- [ ] 不在 `OnInitializedAsync` 中載入資料
- [ ] 不設定 `AdditionalDataLoader` 參數
- [ ] 使用 `ModalManagerInitHelper` 管理關聯實體
- [ ] 使用 `AutoCompleteConfigBuilder` 建立 AutoComplete 配置
- [ ] 使用 `FormSectionHelper` 定義區段（需要 Tab 時用 `.BuildAll()`）
- [ ] 使用 `ActionButtonHelper` 產生欄位按鈕
- [ ] 使用 `EntityCodeGenerationHelper` 自動產生編號

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md) - 總綱
- [README_FormField表單欄位設計.md](README_FormField表單欄位設計.md) - 表單欄位詳細設計
- [README_Index頁面設計.md](README_Index頁面設計.md) - Index 頁面設計
