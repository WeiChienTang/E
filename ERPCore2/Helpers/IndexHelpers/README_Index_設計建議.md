# Index 頁面設計完整指南

> **重要原則：優先使用 Helper！**  
> 系統中大多數功能都已經封裝成 Helper，分別存放在 `Helpers/IndexHelpers` 和 `Helpers/EditModal` 目錄下。  
> 創建新功能時，請先查找是否有對應的 Helper。若沒有，請創建新的 Helper 或在現有 Helper 中新增泛型方法。

---

## 📋 目錄

1. [設計流程概覽](#設計流程概覽)
2. [檔案結構](#檔案結構)
3. [Index 頁面設計](#index-頁面設計)
4. [FieldConfiguration 設計](#fieldconfiguration-設計)
5. [EditModal 設計](#editmodal-設計)
6. [可用的 Helper 清單](#可用的-helper-清單)
7. [常見問題與解決方案](#常見問題與解決方案)

---

## 🎯 設計流程概覽

創建新的 Index 頁面時，需要按照以下順序建立三個檔案：

```
1. FieldConfiguration (欄位配置)
   ↓
2. EditModal (編輯 Modal)
   ↓
3. Index (主頁面)
```

**核心原則：**
1. **優先使用 Helper** - 避免重複造輪子
2. **保持一致性** - 所有頁面使用相同的模式
3. **類型安全** - 盡可能使用 Lambda Expression 而非字串
4. **錯誤處理** - 統一使用 ErrorHandlingHelper
5. **可維護性** - 將複雜邏輯抽取成 Helper

**設計流程：**
```
1. 分析需求 → 2. 尋找 Helper → 3. 建立 FieldConfiguration 
→ 4. 建立 EditModal → 5. 建立 Index → 6. 測試與優化
```
---

## 📁 檔案結構

### 1. Index 頁面
**路徑：** `Components/Pages/{ModuleName}/{EntityName}Index.razor`  
**範例：** `Components/Pages/Customers/CustomerIndex.razor`

### 2. FieldConfiguration
**路徑：** `Components/FieldConfiguration/{EntityName}FieldConfiguration.cs`  
**範例：** `Components/FieldConfiguration/CustomerFieldConfiguration.cs`

### 3. EditModal
**路徑：** `Components/Pages/{ModuleName}/{EntityName}EditModalComponent.razor`  
**範例：** `Components/Pages/Customers/CustomerEditModalComponent.razor`

---

## 🔧 Index 頁面設計

### 完整範例參考：`CustomerIndex.razor`

```razor
@page "/customers"
@inject ICustomerService CustomerService
@rendermode InteractiveServer
@inject INotificationService NotificationService

<GenericIndexPageComponent TEntity="Customer" 
                      TService="ICustomerService"
                      Service="@CustomerService"
                      EntityBasePath="/customers"
                      PageTitle="客戶維護"
                      PageSubtitle="管理所有客戶資料與聯絡資訊"
                      EntityName="客戶"
                      BreadcrumbItems="@breadcrumbItems"
                      FilterDefinitions="@filterDefinitions"
                      ColumnDefinitions="@columnDefinitions"
                      DataLoader="@LoadCustomersAsync"
                      FilterApplier="@ApplyCustomerFilters"
                      GetEntityDisplayName="@(customer => customer.CompanyName)"
                      RequiredPermission="Customer.Read"
                      OnAddClick="@modalHandler.ShowAddModalAsync"
                      OnRowClick="@modalHandler.ShowEditModalAsync"
                      @ref="indexComponent" />

<CustomerEditModalComponent IsVisible="@showEditModal"
                           IsVisibleChanged="@((bool visible) => showEditModal = visible)"
                           CustomerId="@editingCustomerId"
                           OnCustomerSaved="@modalHandler.OnEntitySavedAsync"
                           OnCancel="@modalHandler.OnModalCancelAsync" />

@code {
    // 組件參考
    private GenericIndexPageComponent<Customer, ICustomerService> indexComponent = default!;
    
    // Modal 相關狀態
    private bool showEditModal = false;
    private int? editingCustomerId = null;
    
    // Modal 處理器 - 使用 ModalHelper
    private ModalHandler<Customer, GenericIndexPageComponent<Customer, ICustomerService>> modalHandler = default!;
    
    // 欄位配置
    private CustomerFieldConfiguration fieldConfiguration = default!;
    
    // 配置相關
    private List<SearchFilterDefinition> filterDefinitions = new();
    private List<TableColumnDefinition> columnDefinitions = new();
    private List<BreadcrumbItem> breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 1. 初始化 Modal 處理器 - 使用 ModalHelper.CreateModalHandler
            modalHandler = ModalHelper.CreateModalHandler<Customer, GenericIndexPageComponent<Customer, ICustomerService>>(
                id => editingCustomerId = id,
                visible => showEditModal = visible,
                () => indexComponent,
                StateHasChanged,
                GetType());
            
            // 2. 初始化麵包屑 - 使用 BreadcrumbHelper
            await InitializeBreadcrumbsAsync();
            
            // 3. 建立欄位配置（並傳遞 NotificationService）
            fieldConfiguration = new CustomerFieldConfiguration(NotificationService);
            
            // 4. 使用欄位配置建立篩選器和表格欄位
            filterDefinitions = fieldConfiguration.BuildFilters();
            columnDefinitions = fieldConfiguration.BuildColumns();
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(OnInitializedAsync), GetType(), 
                additionalData: "初始化客戶頁面失敗");
            await NotificationService.ShowErrorAsync("初始化客戶頁面失敗");
        }
    }

    // 使用 BreadcrumbHelper 初始化麵包屑
    private async Task InitializeBreadcrumbsAsync() => 
        breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync("客戶管理", NotificationService, GetType());
    
    // 使用 DataLoaderHelper 載入資料
    private Task<List<Customer>> LoadCustomersAsync() => 
        DataLoaderHelper.LoadAsync(
            () => CustomerService.GetAllAsync(),
            "客戶",
            NotificationService,
            GetType());

    // 套用篩選器 - 委派給 FieldConfiguration
    private IQueryable<Customer> ApplyCustomerFilters(SearchFilterModel searchModel, IQueryable<Customer> query)
    {
        return fieldConfiguration.ApplyFilters(searchModel, query, nameof(ApplyCustomerFilters), GetType());
    }
}
```

### 🎯 設計要點

#### 1. 使用 `ModalHelper.CreateModalHandler`
**替代原本的手動處理：**
```csharp
// ❌ 舊做法：手動處理 Modal 狀態
private async Task HandleAddClick()
{
    editingCustomerId = null;
    showEditModal = true;
}

// ✅ 新做法：使用 ModalHelper
modalHandler = ModalHelper.CreateModalHandler<Customer, GenericIndexPageComponent<Customer, ICustomerService>>(
    id => editingCustomerId = id,
    visible => showEditModal = visible,
    () => indexComponent,
    StateHasChanged,
    GetType());
```

#### 2. 使用 `BreadcrumbHelper`
**支援多種麵包屑模式：**
```csharp
// 簡單兩層（首頁 > 頁面名稱）
breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync("客戶管理", NotificationService, GetType());

// 三層（首頁 > 模組 > 頁面）
breadcrumbItems = await BreadcrumbHelper.CreateThreeLevelAsync("基本資料", "客戶管理", 
    "/master-data", NotificationService, GetType());

// 自訂層級
breadcrumbItems = await BreadcrumbHelper.InitializeAsync(
    new[] {
        new BreadcrumbItem("基本資料", "/master-data"),
        new BreadcrumbItem("客戶管理", "/customers"),
        new BreadcrumbItem("客戶詳細資料")
    },
    NotificationService,
    GetType());
```

#### 3. 使用 `DataLoaderHelper`
**統一的資料載入與錯誤處理：**
```csharp
private Task<List<Customer>> LoadCustomersAsync() => 
    DataLoaderHelper.LoadAsync(
        () => CustomerService.GetAllAsync(),  // 載入函數
        "客戶",                                // 實體名稱（用於錯誤訊息）
        NotificationService,                   // 通知服務
        GetType());                           // 呼叫者類型（用於錯誤記錄）
```

#### 4. FieldConfiguration 的整合
```csharp
// 建立欄位配置實例（傳入 NotificationService 以支援錯誤通知）
fieldConfiguration = new CustomerFieldConfiguration(NotificationService);

// 建立篩選器定義
filterDefinitions = fieldConfiguration.BuildFilters();

// 建立表格欄位定義
columnDefinitions = fieldConfiguration.BuildColumns();

// 套用篩選邏輯（委派給 FieldConfiguration）
private IQueryable<Customer> ApplyCustomerFilters(SearchFilterModel searchModel, IQueryable<Customer> query)
{
    return fieldConfiguration.ApplyFilters(searchModel, query, nameof(ApplyCustomerFilters), GetType());
}
```

---

## 🗂️ FieldConfiguration 設計

### 完整範例參考：`CustomerFieldConfiguration.cs`

```csharp
using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 客戶欄位配置
    /// </summary>
    public class CustomerFieldConfiguration : BaseFieldConfiguration<Customer>
    {
        private readonly INotificationService? _notificationService;
        
        public CustomerFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Customer>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Customer>>
                {
                    {
                        nameof(Customer.Code),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.Code),
                            DisplayName = "客戶編號",
                            FilterPlaceholder = "輸入客戶編號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.Code), c => c.Code)
                        }
                    },
                    {
                        nameof(Customer.CompanyName),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.CompanyName),
                            DisplayName = "公司名稱",
                            FilterPlaceholder = "輸入公司名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.CompanyName), c => c.CompanyName)
                        }
                    },
                    {
                        nameof(Customer.ContactPerson),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.ContactPerson),
                            DisplayName = "聯絡人",
                            FilterPlaceholder = "輸入聯絡人姓名搜尋",
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.ContactPerson), c => c.ContactPerson)
                        }
                    },
                    {
                        nameof(Customer.TaxNumber),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.TaxNumber),
                            DisplayName = "統一編號",
                            FilterPlaceholder = "輸入統一編號搜尋",
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.TaxNumber), c => c.TaxNumber, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化客戶欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Customer>>();
            }
        }
    }
}
```

### 🎯 設計要點

#### 1. 繼承 `BaseFieldConfiguration<TEntity>`
- 提供 `BuildFilters()` 和 `BuildColumns()` 方法
- 自動處理篩選邏輯

#### 2. 使用 `FilterHelper` 處理篩選
```csharp
// 文字包含篩選
FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
    model, query, nameof(Customer.Code), c => c.Code)

// 允許 null 值的文字篩選
FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
    model, query, nameof(Customer.TaxNumber), c => c.TaxNumber, allowNull: true)

// 數值篩選
FilterFunction = (model, query) => FilterHelper.ApplyNumericFilter(
    model, query, nameof(Product.Price), p => p.Price)

// 日期篩選
FilterFunction = (model, query) => FilterHelper.ApplyDateFilter(
    model, query, nameof(SalesOrder.OrderDate), s => s.OrderDate)

// 外鍵關聯篩選
FilterFunction = (model, query) => FilterHelper.ApplyForeignKeyFilter(
    model, query, nameof(SalesOrder.CustomerId), s => s.CustomerId)
```

#### 3. 錯誤處理
- 使用 `ErrorHandlingHelper` 記錄錯誤
- 透過 `INotificationService` 通知使用者
- 失敗時回傳安全的預設值

#### 4. 欄位屬性設定
```csharp
new FieldDefinition<Customer>
{
    PropertyName = nameof(Customer.Code),       // 屬性名稱
    DisplayName = "客戶編號",                    // 顯示名稱
    FilterPlaceholder = "輸入客戶編號搜尋",      // 篩選欄位提示文字
    TableOrder = 1,                             // 表格欄位順序
    HeaderStyle = "width: 180px;",              // 表頭樣式（可選）
    FilterFunction = ...                        // 篩選函數
}
```

---

## ✏️ EditModal 設計

### 完整範例參考：`CustomerEditModalComponent.razor`

```razor
@inject ICustomerService CustomerService
@inject IEmployeeService EmployeeService
@inject IPaymentMethodService PaymentMethodService
@inject INotificationService NotificationService
@inject ActionButtonHelper ActionButtonHelper

<GenericEditModalComponent TEntity="Customer" 
                          TService="ICustomerService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@CustomerId"
                          Service="@CustomerService"
                          EntityName="客戶"
                          ModalTitle="@(CustomerId.HasValue ? "編輯客戶" : "新增客戶")"
                          Size="GenericEditModalComponent<Customer, ICustomerService>.ModalSize.Desktop"
                          UseGenericForm="true"
                          FormFields="@GetFormFields()"
                          FormSections="@formSections"
                          AutoCompletePrefillers="@autoCompleteConfig?.Prefillers"
                          AutoCompleteCollections="@autoCompleteConfig?.Collections"
                          AutoCompleteDisplayProperties="@autoCompleteConfig?.DisplayProperties"
                          AutoCompleteValueProperties="@autoCompleteConfig?.ValueProperties"
                          ModalManagers="@GetModalManagers()"
                          DataLoader="@LoadCustomerData"
                          UseGenericSave="true"
                          SaveSuccessMessage="@(CustomerId.HasValue ? "客戶更新成功" : "客戶新增成功")"
                          SaveFailureMessage="客戶儲存失敗"
                          RequiredPermission="Customer.Read"
                          OnSaveSuccess="@HandleSaveSuccess"
                          OnCancel="@HandleCancel"
                          OnFieldChanged="@OnFieldValueChanged">
</GenericEditModalComponent>

@* 關聯實體編輯 Modal *@
<EmployeeEditModalComponent @ref="employeeEditModal"
                           IsVisible="@employeeModalManager.IsModalVisible"
                           IsVisibleChanged="@employeeModalManager.HandleModalVisibilityChangedAsync"
                           EmployeeId="@employeeModalManager.SelectedEntityId"
                           OnEmployeeSaved="@OnEmployeeSavedWrapper"
                           OnCancel="@employeeModalManager.HandleModalCancelAsync" />

<PaymentMethodEditModalComponent @ref="paymentMethodEditModal"
                                IsVisible="@paymentMethodModalManager.IsModalVisible"
                                IsVisibleChanged="@paymentMethodModalManager.HandleModalVisibilityChangedAsync"
                                PaymentMethodId="@paymentMethodModalManager.SelectedEntityId"
                                OnPaymentMethodSaved="@OnPaymentMethodSavedWrapper"
                                OnCancel="@paymentMethodModalManager.HandleModalCancelAsync" />

@code {
    // ===== 必要參數 =====
    [Parameter] public bool IsVisible { get; set; } = false;
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public int? CustomerId { get; set; }
    [Parameter] public EventCallback<Customer> OnCustomerSaved { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    // ===== 內部狀態 =====
    private GenericEditModalComponent<Customer, ICustomerService>? editModalComponent;
    private List<FormFieldDefinition> formFields = new();
    private Dictionary<string, string> formSections = new();
    
    // AutoComplete 配置 - 使用 AutoCompleteConfigHelper
    private AutoCompleteConfig? autoCompleteConfig;
    
    // 選項資料
    private List<Employee> availableEmployees = new();
    private List<PaymentMethod> availablePaymentMethods = new();
    
    // Modal Manager 集合 - 使用 ModalManagerInitHelper
    private ModalManagerCollection? modalManagers;
    
    // 個別 Modal Manager
    private EmployeeEditModalComponent? employeeEditModal;
    private RelatedEntityModalManager<Employee> employeeModalManager = default!;
    
    private PaymentMethodEditModalComponent? paymentMethodEditModal;
    private RelatedEntityModalManager<PaymentMethod> paymentMethodModalManager = default!;

    // ===== 資料載入狀態 =====
    private bool isDataLoaded = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 使用 ModalManagerInitHelper 初始化所有 Manager
            modalManagers = ModalManagerInitHelper.CreateBuilder<Customer, ICustomerService>(
                    () => editModalComponent,
                    NotificationService,
                    StateHasChanged,
                    LoadAdditionalDataAsync,
                    InitializeFormFieldsAsync)
                .AddManager<Employee>(nameof(Customer.EmployeeId), "員工")
                .AddManager<PaymentMethod>(nameof(Customer.PaymentMethodId), "付款方式")
                .Build();
            
            // 取得個別 Manager 供組件使用
            employeeModalManager = modalManagers.Get<Employee>(nameof(Customer.EmployeeId));
            paymentMethodModalManager = modalManagers.Get<PaymentMethod>(nameof(Customer.PaymentMethodId));
            
            // ⚠️ 注意：不在此載入資料，改用 Lazy Loading 模式
            // 資料會在 OnParametersSetAsync 中當 IsVisible = true 時才載入
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("初始化客戶編輯組件時發生錯誤");
        }
    }
    
    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && !isDataLoaded)
        {
            // Modal 打開時才載入資料（Lazy Loading）
            await LoadAdditionalDataAsync();
            await InitializeFormFieldsAsync();
            isDataLoaded = true;
        }
        else if (!IsVisible)
        {
            // Modal 關閉時重置狀態
            isDataLoaded = false;
        }
    }
    
    private async Task InitializeFormFieldsAsync()
    {
        try
        {
            formFields = new List<FormFieldDefinition>
            {
                new()
                {
                    PropertyName = nameof(Customer.Code),
                    Label = "客戶編號",
                    FieldType = FormFieldType.Text,
                    Placeholder = "請輸入客戶編號",
                    IsRequired = true,
                    MaxLength = 20,
                    HelpText = "客戶的唯一識別編號"
                },
                new()
                {
                    PropertyName = nameof(Customer.CompanyName),
                    Label = "公司名稱",
                    FieldType = FormFieldType.Text,
                    Placeholder = "請輸入公司名稱",
                    IsRequired = false,
                    MaxLength = 20
                },
                new()
                {
                    PropertyName = nameof(Customer.EmployeeId),
                    Label = "業務負責人",
                    FieldType = FormFieldType.AutoComplete,
                    Placeholder = "請輸入或選擇業務負責人",
                    IsRequired = false,
                    MinSearchLength = 0,
                    ActionButtons = await GetEmployeeActionButtonsAsync()  // 使用 ActionButtonHelper
                },
                // ... 更多欄位
                FormFieldConfigurationHelper.CreateRemarksField<Customer>()  // 使用預設備註欄位
            };

            // 使用 FormSectionHelper 建立區段配置
            formSections = FormSectionHelper<Customer>.Create()
                .AddToSection(FormSectionNames.BasicInfo,
                    c => c.Code,
                    c => c.CompanyName,
                    c => c.ResponsiblePerson)
                .AddToSection(FormSectionNames.ContactPersonInfo,
                    c => c.ContactPerson,
                    c => c.JobTitle)
                .AddToSection(FormSectionNames.SalesInfo,
                    c => c.EmployeeId)
                .AddToSection(FormSectionNames.OtherInfo,
                    c => c.Remarks)
                .Build();
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("初始化表單欄位時發生錯誤");
        }
    }

    private async Task<Customer?> LoadCustomerData()
    {
        try
        {
            if (!CustomerId.HasValue)
            {
                // 新增模式 - 使用 EntityCodeGenerationHelper
                var newCustomer = new Customer
                {
                    Code = await EntityCodeGenerationHelper.GenerateForEntity<Customer, ICustomerService>(
                        CustomerService, "CUST"),
                    Status = EntityStatus.Active
                };
                return newCustomer;
            }

            // 編輯模式
            return await CustomerService.GetByIdAsync(CustomerId.Value);
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"載入客戶資料時發生錯誤：{ex.Message}");
            return null;
        }
    }
    
    private async Task LoadAdditionalDataAsync()
    {
        try
        {
            availableEmployees = await EmployeeService.GetAllAsync();
            availablePaymentMethods = await PaymentMethodService.GetAllAsync();
            
            // 重新建立 AutoComplete 配置
            autoCompleteConfig = new AutoCompleteConfigBuilder<Customer>()
                .AddField(nameof(Customer.EmployeeId), "Name", availableEmployees)
                .AddField(nameof(Customer.PaymentMethodId), "Name", availablePaymentMethods)
                .Build();
        }
        catch (Exception)
        {
            await NotificationService.ShowErrorAsync("載入客戶編輯相關資料時發生錯誤");
        }
    }
    
    /// <summary>
    /// 配置 Modal 管理器
    /// </summary>
    private Dictionary<string, object> GetModalManagers()
    {
        return new Dictionary<string, object>
        {
            { nameof(Customer.EmployeeId), employeeModalManager },
            { nameof(Customer.PaymentMethodId), paymentMethodModalManager }
        };
    }
    
    /// <summary>
    /// 使用 ActionButtonHelper 產生操作按鈕
    /// </summary>
    private async Task<List<FieldActionButton>> GetEmployeeActionButtonsAsync()
    {
        return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
            editModalComponent, 
            employeeModalManager, 
            nameof(Customer.EmployeeId)
        );
    }
    
    /// <summary>
    /// 處理欄位值變更事件
    /// </summary>
    private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
    {
        try
        {
            if (fieldChange.PropertyName == nameof(Customer.EmployeeId))
            {
                await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                    employeeModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
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
        {
            await OnCustomerSaved.InvokeAsync(editModalComponent.Entity);
        }
        await CloseModal();
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await CloseModal();
    }

    private async Task CloseModal()
    {
        await IsVisibleChanged.InvokeAsync(false);
    }
    
    /// <summary>
    /// 包裝實體儲存事件
    /// </summary>
    private async Task OnEmployeeSavedWrapper(Employee savedEmployee)
    {
        await employeeModalManager.HandleEntitySavedAsync(savedEmployee, shouldAutoSelect: true);
    }
    
    private async Task OnPaymentMethodSavedWrapper(PaymentMethod savedPaymentMethod)
    {
        await paymentMethodModalManager.HandleEntitySavedAsync(savedPaymentMethod, shouldAutoSelect: true);
    }
}
```

### 🎯 設計要點

#### 1. **⚠️ 重要：Lazy Loading 模式（避免重複載入）**

**核心原則：**
- ❌ **不要**在 `OnInitializedAsync` 中呼叫 `LoadAdditionalDataAsync` 和 `InitializeFormFieldsAsync`
- ❌ **不要**在 `GenericEditModalComponent` 上設定 `AdditionalDataLoader` 參數
- ✅ **必須**實作 `OnParametersSetAsync`，使用 `isDataLoaded` 旗標控制載入時機
- ✅ 資料只在 Modal **打開時**（`IsVisible = true`）才載入
- ✅ Modal **關閉時**重置 `isDataLoaded` 狀態

**錯誤範例（會導致重複載入）：**
```csharp
// ❌ 錯誤：在 GenericEditModalComponent 上設定 AdditionalDataLoader
<GenericEditModalComponent ...
                          DataLoader="@LoadCustomerData"
                          AdditionalDataLoader="@LoadAdditionalDataAsync"  // ❌ 移除此行
                          ... />

// ❌ 錯誤：在 OnInitializedAsync 中載入資料
protected override async Task OnInitializedAsync()
{
    modalManagers = ModalManagerInitHelper.CreateBuilder...;
    await LoadAdditionalDataAsync();  // ❌ 移除此行
    await InitializeFormFieldsAsync(); // ❌ 移除此行
}
```

**正確範例（Lazy Loading）：**
```csharp
// ✅ 正確：移除 AdditionalDataLoader
<GenericEditModalComponent ...
                          DataLoader="@LoadCustomerData"
                          UseGenericSave="true"
                          ... />

// ✅ 正確：只在 OnInitializedAsync 初始化 Manager
private bool isDataLoaded = false;

protected override async Task OnInitializedAsync()
{
    modalManagers = ModalManagerInitHelper.CreateBuilder...;
    // 不載入資料，等待 OnParametersSetAsync
}

// ✅ 正確：在 OnParametersSetAsync 中實作 Lazy Loading
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
```

---

#### 2. 使用 `ModalManagerInitHelper` 初始化 Modal 管理器
```csharp
// ✅ 使用 Builder 模式建立多個 Manager
modalManagers = ModalManagerInitHelper.CreateBuilder<Customer, ICustomerService>(
        () => editModalComponent,
        NotificationService,
        StateHasChanged,
        LoadAdditionalDataAsync,           // 資料重新載入回調
        InitializeFormFieldsAsync)          // 表單欄位重新初始化回調
    .AddManager<Employee>(nameof(Customer.EmployeeId), "員工")
    .AddManager<PaymentMethod>(nameof(Customer.PaymentMethodId), "付款方式")
    .Build();

// 取得個別 Manager
employeeModalManager = modalManagers.Get<Employee>(nameof(Customer.EmployeeId));
```

#### 2. 使用 `AutoCompleteConfigHelper` 建立 AutoComplete 配置
```csharp
// ✅ 使用 Builder 模式簡化配置
autoCompleteConfig = new AutoCompleteConfigBuilder<Customer>()
    .AddField(nameof(Customer.EmployeeId), "Name", availableEmployees)
    .AddField(nameof(Customer.PaymentMethodId), "Name", availablePaymentMethods)
    .Build();

// 也支援更進階的配置
autoCompleteConfig = new AutoCompleteConfigBuilder<Customer>()
    // 複合搜尋（同時搜尋多個欄位）
    .AddFieldWithMultipleSearchProperties<Customer>(
        nameof(Customer.CustomerId),
        "CompanyName",
        availableCustomers,
        new[] { "CompanyName", "TaxNumber" })
    // 條件式配置
    .AddFieldIf(hasPermission,
        nameof(Customer.ApprovedById),
        "Name",
        availableEmployees)
    .Build();
```

#### 3. 使用 `FormSectionHelper` 定義表單區段
```csharp
// ✅ 使用 Lambda Expression（類型安全）
formSections = FormSectionHelper<Customer>.Create()
    .AddToSection(FormSectionNames.BasicInfo,
        c => c.Code,
        c => c.CompanyName)
    .AddToSection(FormSectionNames.ContactPersonInfo,
        c => c.ContactPerson)
    .Build();

// 也支援條件式配置
formSections = FormSectionHelper<Customer>.Create()
    .AddIf(showAdvancedFields, FormSectionNames.AdditionalInfo,
        c => c.CreditLimit,
        c => c.CurrentBalance)
    .Build();
```

#### 4. 使用 `ActionButtonHelper` 產生欄位操作按鈕
```csharp
// ✅ 標準用法
private async Task<List<FieldActionButton>> GetEmployeeActionButtonsAsync()
{
    return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
        editModalComponent, 
        employeeModalManager, 
        nameof(Customer.EmployeeId)
    );
}

// 處理欄位變更時更新按鈕
private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    if (fieldChange.PropertyName == nameof(Customer.EmployeeId))
    {
        await ActionButtonHelper.UpdateFieldActionButtonsAsync(
            employeeModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
    }
}
```

#### 5. 使用 `EntityCodeGenerationHelper` 產生編號
```csharp
// ✅ 新增模式時自動產生編號
var newCustomer = new Customer
{
    Code = await EntityCodeGenerationHelper.GenerateForEntity<Customer, ICustomerService>(
        CustomerService, "CUST"),
    Status = EntityStatus.Active
};
```

#### 6. 使用 `FormFieldConfigurationHelper` 建立常用欄位
```csharp
// ✅ 使用預設的備註欄位
formFields.Add(FormFieldConfigurationHelper.CreateRemarksField<Customer>());

// 也有其他預設欄位
formFields.Add(FormFieldConfigurationHelper.CreateCodeField<Customer>("客戶編號", "CUST"));
formFields.Add(FormFieldConfigurationHelper.CreateStatusField<Customer>());
```

---

## 🛠️ 可用的 Helper 清單

### IndexHelpers（位於 `Helpers/IndexHelpers/`）

| Helper | 功能 | 使用時機 |
|--------|------|---------|
| **BreadcrumbHelper** | 麵包屑導航初始化 | 所有 Index 頁面 |
| **DataLoaderHelper** | 統一資料載入與錯誤處理 | 所有需要載入資料的頁面 |

### EditModal Helpers（位於 `Helpers/EditModal/`）

| Helper | 功能 | 使用時機 |
|--------|------|---------|
| **ActionButtonHelper** | 欄位操作按鈕產生與更新 | 有關聯實體的 AutoComplete 欄位 |
| **ApprovalConfigHelper** | 審核流程配置 | 需要審核機制的單據 |
| **AutoCompleteConfigHelper** | AutoComplete 配置建立 | 所有 AutoComplete 欄位 |
| **ChildDocumentRefreshHelper** | 子文件刷新處理 | 有明細資料的主檔單據 |
| **CodeGenerationHelper** | 編號生成邏輯 | 需要自動產生編號的實體 |
| **EntityCodeGenerationHelper** | 實體編號生成（泛型） | 新增模式時自動產生編號 |
| **FormFieldLockHelper** | 表單欄位鎖定控制 | 需要根據狀態鎖定欄位 |
| **FormSectionHelper** | 表單區段定義 | 所有 EditModal |
| **ModalManagerInitHelper** | Modal 管理器初始化 | 有關聯實體編輯的 Modal |
| **PrefilledValueHelper** | 預填值處理 | 需要從其他實體複製值 |
| **TaxCalculationHelper** | 稅額計算 | 有稅額計算的單據 |
| **DocumentConversionHelper** | 單據轉換 | A 單轉 B 單功能 |

### 其他 Helpers（位於 `Helpers/`）

| Helper | 功能 | 使用時機 |
|--------|------|---------|
| **ErrorHandlingHelper** | 統一錯誤處理與記錄 | 所有需要錯誤處理的地方 |
| **FilterHelper** | 篩選邏輯處理 | FieldConfiguration 中定義篩選 |
| **ModalHelper** | Modal 狀態管理 | Index 頁面的 Modal 處理 |
| **CurrentUserHelper** | 當前使用者資訊 | 需要取得當前使用者 |
| **DependencyCheckHelper** | 依賴關係檢查 | 刪除前檢查是否有關聯資料 |
| **EntityStatusHelper** | 實體狀態管理 | 需要處理啟用/停用狀態 |
| **NavigationActionHelper** | 導航動作處理 | 需要頁面導航功能 |
| **NumberFormatHelper** | 數字格式化 | 顯示金額、數量等數值 |
| **ReportPrintHelper** | 報表列印 | 需要列印報表 |

---

## ❓ 常見問題與解決方案

### Q1: 如何決定是否需要建立新的 Helper？

**判斷標準：**
1. **重複度超過 3 次** → 應建立 Helper
2. **邏輯複雜度高** → 應建立 Helper
3. **跨多個頁面使用** → 應建立 Helper

**範例：**
```csharp
// ❌ 在多個 EditModal 中重複的編號
private async Task<List<FieldActionButton>> GetEmployeeActionButtonsAsync()
{
    var currentId = editModalComponent?.Entity?.EmployeeId;
    return employeeModalManager.GenerateActionButtons(currentId);
}

// ✅ 抽取成 Helper（ActionButtonHelper）
private async Task<List<FieldActionButton>> GetEmployeeActionButtonsAsync()
{
    return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
        editModalComponent, employeeModalManager, nameof(Customer.EmployeeId));
}
```

### Q2: 新 Helper 應該放在哪裡？

**目錄結構規則：**
- **IndexHelpers/** - Index 頁面專用（資料載入、麵包屑等）
- **EditModal/** - EditModal 專用（表單、AutoComplete、Modal 管理等）
- **NumericHelpers/** - 數字處理相關
- **根目錄** - 通用 Helper（錯誤處理、篩選、依賴檢查等）

### Q3: Helper 應該是靜態類別還是實例類別？

**選擇指南：**
```csharp
// ✅ 靜態類別 - 無狀態、純功能
public static class BreadcrumbHelper
{
    public static async Task<List<BreadcrumbItem>> CreateSimpleAsync(...)
}

// ✅ 實例類別 - 需要注入服務、有狀態
public class ActionButtonHelper
{
    private readonly INotificationService _notificationService;
    
    public ActionButtonHelper(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
}
```

### Q4: 如何處理 Helper 中的錯誤？

**統一使用 ErrorHandlingHelper：**
```csharp
try
{
    // 業務邏輯
}
catch (Exception ex)
{
    // 記錄錯誤到資料庫
    await ErrorHandlingHelper.HandlePageErrorAsync(
        ex, 
        nameof(MethodName), 
        GetType(),
        additionalData: "錯誤描述");
    
    // 通知使用者
    await _notificationService.ShowErrorAsync("使用者友善的錯誤訊息");
    
    // 回傳安全的預設值
    return new List<T>();
}
```

### Q5: 如何在 FieldConfiguration 中處理關聯實體篩選？

**使用 FilterHelper.ApplyForeignKeyFilter：**
```csharp
{
    nameof(SalesOrder.CustomerId),
    new FieldDefinition<SalesOrder>
    {
        PropertyName = nameof(SalesOrder.CustomerId),
        DisplayName = "客戶",
        FilterPlaceholder = "選擇客戶",
        TableOrder = 2,
        // 使用 ForeignKeyFilter 處理關聯實體
        FilterFunction = (model, query) => FilterHelper.ApplyForeignKeyFilter(
            model, query, nameof(SalesOrder.CustomerId), s => s.CustomerId),
        // 自訂顯示邏輯
        ValueGetter = s => s.Customer?.CompanyName ?? "未設定"
    }
}
```

### Q6: 如何處理需要特殊邏輯的 ActionButtons？

**使用 ActionButtonHelper.GenerateFieldActionButtonsWithCustomLogicAsync：**
```csharp
private async Task<List<FieldActionButton>> GetRoleActionButtonsAsync()
{
    return await ActionButtonHelper.GenerateFieldActionButtonsWithCustomLogicAsync(
        editModalComponent,
        roleModalManager,
        nameof(Employee.RoleId),
        (buttons, employee) =>
        {
            // 自訂邏輯：系統使用者不允許編輯角色
            if (employee?.IsSystemUser == true)
            {
                foreach (var button in buttons)
                {
                    button.IsDisabled = true;
                    button.Title = "系統使用者不可修改角色";
                }
            }
        });
}
```

### Q7: 如何處理複雜的表單區段配置？

**使用 FormSectionHelper 的進階功能：**
```csharp
formSections = FormSectionHelper<SalesOrder>.Create()
    // 基本欄位
    .AddToSection(FormSectionNames.BasicInfo,
        s => s.Code,
        s => s.OrderDate)
    // 條件式欄位（只有管理員可見）
    .AddIf(isAdmin, FormSectionNames.AdditionalInfo,
        s => s.InternalNotes,
        s => s.CostPrice)
    // 自訂欄位名稱（非實體屬性）
    .AddCustomFields(FormSectionNames.FilterInfo,
        "FilterProductId",
        "FilterCategory")
    .Build();
```

### Q8: 新增模式和編輯模式有什麼差異？

**DataLoader 的典型處理：**
```csharp
private async Task<Customer?> LoadCustomerData()
{
    if (!CustomerId.HasValue)
    {
        // ===== 新增模式 =====
        return new Customer
        {
            // 使用 Helper 自動產生編號
            Code = await EntityCodeGenerationHelper.GenerateForEntity<Customer, ICustomerService>(
                CustomerService, "CUST"),
            // 設定預設值
            Status = EntityStatus.Active,
            PaymentDate = 0,  // 預設月底收款
            CreditLimit = 0
        };
    }

    // ===== 編輯模式 =====
    return await CustomerService.GetByIdAsync(CustomerId.Value);
}
```

---