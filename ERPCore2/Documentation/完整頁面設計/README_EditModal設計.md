# EditModal 設計

## 更新日期
2026-02-17

---

## 概述

EditModal 是每個業務實體的新增/編輯表單，使用 `GenericEditModalComponent<TEntity, TService>` 提供完整的 CRUD 操作介面，包括資料載入、表單渲染、驗證、儲存、上下筆導航、列印、自訂內容 Tab 等功能。

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
@inject IYourEntityRosterReportService YourEntityReportService
@inject IVehicleService VehicleService
@inject ActionButtonHelper ActionButtonHelper

<GenericEditModalComponent TEntity="YourEntity"
                          TService="IYourEntityService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          @bind-Id="@YourEntityId"
                          Service="@YourEntityService"
                          EntityName="實體"
                          EntityNamePlural="實體"
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
                          OnFieldChanged="@OnFieldValueChanged"
                          OnEntityLoaded="@HandleEntityLoaded"
                          ShowPrintButton="true"
                          PrintButtonText="列印"
                          ReportService="@YourEntityReportService"
                          ReportId="@ReportIds.YourEntityRoster"
                          ReportPreviewTitle="實體資料預覽"
                          GetReportDocumentName="@(e => $"實體-{e.Code}")" />

@* 關聯實體編輯 Modal *@
<RelatedEditModalComponent @ref="relatedEditModal"
                           IsVisible="@relatedModalManager.IsModalVisible"
                           IsVisibleChanged="@relatedModalManager.HandleModalVisibilityChangedAsync"
                           RelatedId="@relatedModalManager.SelectedEntityId"
                           OnRelatedSaved="@OnRelatedSavedWrapper"
                           OnCancel="@relatedModalManager.HandleModalCancelAsync" />

@* 巢狀 Modal - 條件式渲染避免循環元件實例化 *@
@if (isNestedModalVisible)
{
    <NestedEditModalComponent @ref="nestedEditModal"
                              IsVisible="@isNestedModalVisible"
                              IsVisibleChanged="@HandleNestedModalVisibilityChanged"
                              NestedId="@editingNestedId"
                              PrefilledValues="@GetNestedPrefilledValues()"
                              OnNestedSaved="@OnNestedSavedWrapper"
                              OnCancel="@HandleNestedModalCancel" />
}

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

    // 自訂 Tab 資料
    private List<Vehicle> entityVehicles = new();

    // ===== 實體載入事件（上下筆切換時觸發） =====
    private async Task HandleEntityLoaded(int loadedEntityId)
    {
        try
        {
            if (loadedEntityId > 0)
            {
                entityVehicles = await VehicleService.GetByOwnerAsync(loadedEntityId);
            }
            else
            {
                entityVehicles = new();
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"載入資料時發生錯誤：{ex.Message}");
        }
    }

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
                // 自訂內容 Tab（嵌入子表格）
                .GroupIntoCustomTab("配給車輛", "bi-truck", CreateVehicleTabContent())
                .BuildAll();

            formSections = layout.FieldSections;
            tabDefinitions = layout.TabDefinitions;
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("初始化表單欄位時發生錯誤");
        }
    }

    // ===== 自訂 Tab 內容 =====
    private RenderFragment CreateVehicleTabContent() => __builder =>
    {
        <VehicleTable Vehicles="@entityVehicles"
                     OnAddVehicle="@HandleAddVehicle"
                     OnEditVehicle="@HandleEditVehicle"
                     OnUnlinkVehicle="@HandleUnlinkVehicle" />
    };

    // ===== 資料載入 =====
    private async Task<YourEntity?> LoadEntityData()
    {
        try
        {
            if (!YourEntityId.HasValue)
            {
                return new YourEntity
                {
                    Code = await EntityCodeGenerationHelper
                        .GenerateForEntity<YourEntity, IYourEntityService>(
                            YourEntityService, "ENT"),
                    Status = EntityStatus.Active
                };
            }
            var entity = await YourEntityService.GetByIdAsync(YourEntityId.Value);
            // 載入自訂 Tab 所需的資料
            entityVehicles = await VehicleService.GetByOwnerAsync(YourEntityId.Value);
            return entity;
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

    // ===== 其他方法（Modal Manager、ActionButton、儲存、取消等）=====
    private List<FormFieldDefinition> GetFormFields() => formFields;

    private Dictionary<string, object> GetModalManagers() => new()
    {
        { nameof(YourEntity.RelatedEntityId), relatedModalManager }
    };

    private async Task<List<FieldActionButton>> GetRelatedActionButtonsAsync()
    {
        return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
            editModalComponent, relatedModalManager,
            nameof(YourEntity.RelatedEntityId));
    }

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

    private async Task HandleSaveSuccess()
    {
        if (editModalComponent?.Entity != null)
            await OnYourEntitySaved.InvokeAsync(editModalComponent.Entity);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task OnRelatedSavedWrapper(RelatedEntity saved)
    {
        await relatedModalManager.HandleEntitySavedAsync(saved, shouldAutoSelect: true);
    }
}
```

---

## GenericEditModalComponent 參數

### 核心參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `@bind-Id` | int? | 實體 ID（支援雙向綁定，上下筆導航時自動更新） |
| `Service` | TService | 實體服務 |
| `EntityName` | string | 實體名稱（用於訊息顯示） |
| `EntityNamePlural` | string | 實體複數名稱 |
| `ModalTitle` | string | Modal 標題 |
| `Size` | ModalSize | Modal 尺寸（Desktop / Large / Medium） |
| `RequiredPermission` | string | 必要權限 |

### 表單參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `UseGenericForm` | bool | 使用 GenericFormComponent 渲染表單 |
| `FormFields` | `List<FormFieldDefinition>` | 表單欄位定義 |
| `FormSections` | `Dictionary<string, string>` | 欄位區段映射 |
| `TabDefinitions` | `List<FormTabDefinition>?` | Tab 頁籤定義 |
| `OnFieldChanged` | EventCallback | 欄位變更事件 |

### AutoComplete 參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `AutoCompletePrefillers` | Dictionary | 預填函數 |
| `AutoCompleteCollections` | Dictionary | 選項集合 |
| `AutoCompleteDisplayProperties` | Dictionary | 顯示屬性名稱 |
| `AutoCompleteValueProperties` | Dictionary | 值屬性名稱 |

### 儲存參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `DataLoader` | `Func<Task<TEntity?>>` | 資料載入函數 |
| `UseGenericSave` | bool | 使用內建儲存流程 |
| `SaveHandler` | `Func<Task<ServiceResult>>?` | 自訂儲存邏輯（覆蓋內建儲存） |
| `SaveSuccessMessage` | string | 儲存成功訊息 |
| `SaveFailureMessage` | string | 儲存失敗訊息 |

### 列印參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `ShowPrintButton` | bool | 是否顯示列印按鈕 |
| `PrintButtonText` | string | 列印按鈕文字 |
| `ReportService` | IEntityReportService | 報表服務 |
| `ReportId` | string | 報表 ID |
| `ReportPreviewTitle` | string | 預覽標題 |
| `GetReportDocumentName` | `Func<TEntity, string>` | 報表文件名稱產生函數 |

### 事件回呼

| 參數 | 類型 | 說明 |
|------|------|------|
| `OnSaveSuccess` | EventCallback | 儲存成功事件 |
| `OnCancel` | EventCallback | 取消事件 |
| `OnEntityLoaded` | `EventCallback<int>` | 實體載入完成事件（上下筆切換時觸發） |

### 其他參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `ModalManagers` | `Dictionary<string, object>` | 關聯實體 Modal Manager 集合 |
| `PrefilledValues` | `Dictionary<string, object?>?` | 預填值（從父元件新增時自動帶入 FK） |

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

### 2. OnEntityLoaded - 上下筆切換事件

當使用者點擊上一筆/下一筆時，`GenericEditModalComponent` 會觸發 `OnEntityLoaded` 事件，用於更新自訂 Tab 的資料。

```csharp
// GenericEditModalComponent 參數
OnEntityLoaded="@HandleEntityLoaded"

// 事件處理
private async Task HandleEntityLoaded(int loadedEntityId)
{
    // 重新載入自訂 Tab 的資料（如配給車輛）
    if (loadedEntityId > 0)
        entityVehicles = await VehicleService.GetByOwnerAsync(loadedEntityId);
    else
        entityVehicles = new();
    StateHasChanged();
}
```

### 3. @bind-Id - 雙向綁定 ID

使用 `@bind-Id` 而非 `Id=`，讓上下筆導航時能自動更新父元件的 ID：

```razor
<GenericEditModalComponent TEntity="YourEntity"
                          TService="IYourEntityService"
                          @bind-Id="@YourEntityId"  // 雙向綁定
                          ... />
```

### 4. SaveHandler - 自訂儲存邏輯

當需要在儲存前執行額外邏輯（如自訂驗證）時使用：

```csharp
<GenericEditModalComponent ...
                          SaveHandler="@CustomSaveLogic" />

private async Task<ServiceResult> CustomSaveLogic()
{
    // 自訂驗證
    if (string.IsNullOrWhiteSpace(editModalComponent?.Entity?.Name))
        return ServiceResult.Failure("名稱為必填");

    // 呼叫服務儲存
    return await YourEntityService.CreateOrUpdateAsync(editModalComponent.Entity);
}
```

### 5. PrefilledValues - 從父元件預填值

從父元件開啟巢狀 Modal 時，自動帶入外鍵值：

```csharp
// 父元件中
<NestedEditModalComponent PrefilledValues="@GetPrefilledValues()" ... />

private Dictionary<string, object?>? GetPrefilledValues()
{
    if (!editingNestedId.HasValue && ParentEntityId.HasValue)
    {
        return new Dictionary<string, object?>
        {
            { nameof(Nested.ParentEntityId), ParentEntityId.Value }
        };
    }
    return null;
}
```

### 6. ModalManagerInitHelper - 關聯實體管理

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

### 7. AutoCompleteConfigBuilder - AutoComplete 配置

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

### 8. EntityCodeGenerationHelper - 自動產生編號

```csharp
var newEntity = new YourEntity
{
    Code = await EntityCodeGenerationHelper
        .GenerateForEntity<YourEntity, IYourEntityService>(
            YourEntityService, "ENT"),  // 前綴
    Status = EntityStatus.Active
};
```

### 9. ActionButtonHelper - 欄位操作按鈕

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

### 10. 巢狀 Modal 條件式渲染

避免循環元件實例化（如 Customer → Vehicle → Customer）：

```razor
@if (isVehicleModalVisible)
{
    <VehicleEditModalComponent @ref="vehicleEditModal"
                              IsVisible="@isVehicleModalVisible"
                              IsVisibleChanged="@HandleVehicleModalVisibilityChanged"
                              VehicleId="@editingVehicleId"
                              PrefilledValues="@GetVehiclePrefilledValues()"
                              OnVehicleSaved="@OnVehicleSavedWrapper"
                              OnCancel="@HandleVehicleModalCancel" />
}
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
- [ ] 使用 `@bind-Id` 綁定 ID
- [ ] 實作 Lazy Loading（`OnParametersSetAsync` + `isDataLoaded`）
- [ ] 不在 `OnInitializedAsync` 中載入資料
- [ ] 不設定 `AdditionalDataLoader` 參數
- [ ] 使用 `ModalManagerInitHelper` 管理關聯實體
- [ ] 使用 `AutoCompleteConfigBuilder` 建立 AutoComplete 配置
- [ ] 使用 `FormSectionHelper` 定義區段（需要 Tab 時用 `.BuildAll()`）
- [ ] 使用 `ActionButtonHelper` 產生欄位按鈕
- [ ] 使用 `EntityCodeGenerationHelper` 自動產生編號
- [ ] 如有自訂 Tab，設定 `OnEntityLoaded` 處理上下筆切換

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md) - 總綱
- [README_FormField表單欄位設計.md](README_FormField表單欄位設計.md) - 表單欄位詳細設計
- [README_Index頁面設計.md](README_Index頁面設計.md) - Index 頁面設計
