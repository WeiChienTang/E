# 欄位配置系統使用指南

## 概述

這個欄位配置系統用於簡化和統一 Index 頁面的欄位管理，包括篩選器、表格欄位和篩選邏輯。通過使用配置類別，可以大幅減少重複代碼並提高可維護性。

## 核心類別

### 1. BaseFieldConfiguration<TEntity>
抽象基礎類別，提供共同的配置邏輯。

### 2. FieldDefinition<TEntity>
定義單一欄位的所有屬性，包括顯示名稱、篩選類型、排序等。

### 3. 具體配置類別 (如 CustomerFieldConfiguration)
繼承 BaseFieldConfiguration，定義特定實體的欄位配置。

## Index 頁面設計規範

### 必須遵循的設計原則

#### 1. 錯誤處理規範
所有方法都必須包含 try-catch 錯誤處理，確保系統穩定性。

#### 2. 通知機制
使用 `INotificationService` 通知使用者錯誤發生。

#### 3. 錯誤記錄
使用 `ErrorHandlingHelper` 記錄詳細的錯誤資訊到系統日誌。

#### 4. 安全的後備機制
當錯誤發生時，提供安全的預設值，確保頁面仍能正常運作。

### 標準方法實作模式

#### OnInitializedAsync() - 頁面初始化
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        // 初始化 Modal 處理器
        modalHandler = ModalHelper.CreateModalHandler<Customer, GenericIndexPageComponent<Customer, ICustomerService>>(
            id => editingCustomerId = id,
            visible => showEditModal = visible,
            () => indexComponent,
            StateHasChanged,
            GetType());
        
        await InitializeBreadcrumbsAsync();
        
        // 載入相關資料
        await LoadCustomerTypesAsync();
        
        // 建立欄位配置
        fieldConfiguration = new CustomerFieldConfiguration(customerTypes, NotificationService);
        filterDefinitions = fieldConfiguration.BuildFilters();
        columnDefinitions = fieldConfiguration.BuildColumns();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(OnInitializedAsync), GetType(), additionalData: "初始化客戶頁面失敗");
        await NotificationService.ShowErrorAsync("初始化客戶頁面失敗");
    }
}
```

#### InitializeBreadcrumbsAsync() - 麵包屑導航初始化

**方式一：簡單的兩層麵包屑（首頁 > 頁面名稱）**
```csharp
private async Task InitializeBreadcrumbsAsync()
{
    breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync(
        "客戶管理",
        NotificationService,
        GetType());
}
```

**方式二：三層麵包屑（首頁 > 模組 > 頁面名稱）**
```csharp
private async Task InitializeBreadcrumbsAsync()
{
    breadcrumbItems = await BreadcrumbHelper.CreateThreeLevelAsync(
        "庫存管理",      // 模組名稱
        "倉庫維護",      // 頁面名稱
        NotificationService,
        GetType());
}
```

**方式三：自訂麵包屑（完全自訂）**
```csharp
private async Task InitializeBreadcrumbsAsync()
{
    breadcrumbItems = await BreadcrumbHelper.InitializeAsync(
        new[]
        {
            new BreadcrumbItem("採購管理", "#"),
            new BreadcrumbItem("進貨退出管理")
        },
        NotificationService,
        GetType());
}
```

**注意**: BreadcrumbHelper 已內建完整的錯誤處理機制，包含錯誤記錄、通知和安全的後備值。

#### LoadXXXAsync() - 資料載入方法

**使用 DataLoaderHelper（推薦）**
```csharp
private Task<List<Customer>> LoadCustomersAsync()
{
    return DataLoaderHelper.LoadAsync(
        () => CustomerService.GetAllAsync(),
        "客戶",
        NotificationService,
        GetType());
}
```

**自訂方法名稱的版本**
```csharp
private Task<List<Customer>> LoadCustomersAsync()
{
    return DataLoaderHelper.LoadAsync(
        () => CustomerService.GetAllAsync(),
        "客戶",
        NotificationService,
        GetType(),
        nameof(LoadCustomersAsync));  // 明確指定方法名稱
}
```

**注意**: DataLoaderHelper 已內建完整的錯誤處理機制，包含錯誤記錄、通知和安全的後備值。

#### ApplyXXXFilters() - 篩選方法 (唯一非 Async)

**方式一：簡化處理（推薦）**
```csharp
private IQueryable<Employee> ApplyEmployeeFilters(SearchFilterModel searchModel, IQueryable<Employee> query)
{
    // 確保 fieldConfiguration 已初始化（避免與 GenericIndexPageComponent 初始化的競爭條件）
    if (fieldConfiguration == null)
    {
        // 如果配置未初始化，回傳基本排序的查詢
        return query.OrderBy(e => e.Name);
    }

    return fieldConfiguration.ApplyFilters(searchModel, query, nameof(ApplyEmployeeFilters), GetType());
}
```

**注意**: 由於 `fieldConfiguration.ApplyFilters()` 內部已包含完整的錯誤處理機制，因此**方式一**的簡化處理已足夠。只需要處理配置物件可能為 null 的情況即可。

### 錯誤處理最佳實踐

#### 1. ErrorHandlingHelper 使用
```csharp
await ErrorHandlingHelper.HandlePageErrorAsync(
    ex,                           // 例外物件
    nameof(MethodName),          // 方法名稱
    GetType(),                   // 類別類型
    additionalData: "額外說明"    // 額外的錯誤資訊
);
```

#### 2. NotificationService 使用
```csharp
// 錯誤通知
await NotificationService.ShowErrorAsync("使用者友善的錯誤訊息");

// 成功通知
await NotificationService.ShowSuccessAsync("操作成功完成");

// 警告通知
await NotificationService.ShowWarningAsync("注意事項");
```

#### 3. 安全的後備值
```csharp
// 集合類型
return new List<Customer>();
breadcrumbItems = new List<BreadcrumbItem>();

// 查詢類型
return query.OrderBy(c => c.Code);

// 配置類型
filterDefinitions = new List<SearchFilterDefinition>();
columnDefinitions = new List<TableColumnDefinition>();
```

## 使用範例：CustomerIndex 實作

#### 1. 創建欄位配置類別

```csharp
// Helpers/FieldConfiguration/CustomerFieldConfiguration.cs
public class CustomerFieldConfiguration : BaseFieldConfiguration<Customer>
{
    private readonly List<CustomerType> _customerTypes;
    private readonly INotificationService? _notificationService;
    
    public CustomerFieldConfiguration(List<CustomerType> customerTypes, INotificationService? notificationService = null)
    {
        _customerTypes = customerTypes;
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
                        DisplayName = "客戶代碼",
                        FilterPlaceholder = "輸入客戶代碼搜尋",
                        TableOrder = 1,
                        FilterOrder = 1,
                        HeaderStyle = "width: 180px;",
                        FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                            model, query, nameof(Customer.Code), c => c.Code)
                    }
                },
                {
                    nameof(Customer.CustomerTypeId),
                    new FieldDefinition<Customer>
                    {
                        PropertyName = "CustomerType.TypeName", // 表格顯示用
                        FilterPropertyName = nameof(Customer.CustomerTypeId), // 篩選器用
                        DisplayName = "客戶類型",
                        FilterType = SearchFilterType.Select,
                        TableOrder = 4,
                        FilterOrder = 4,
                        Options = _customerTypes.Select(ct => new SelectOption 
                        { 
                            Text = ct.TypeName, 
                            Value = ct.Id.ToString() 
                        }).ToList(),
                        FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                            model, query, nameof(Customer.CustomerTypeId), c => c.CustomerTypeId)
                    }
                }
                // ... 其他欄位
            };
        }
        catch (Exception ex)
        {
            // 錯誤處理邏輯
            return new Dictionary<string, FieldDefinition<Customer>>();
        }
    }
}
```

#### 2. 修改 Index 頁面

```csharp
// CustomerIndex.razor
@page "/customers"
@inject ICustomerService CustomerService
@rendermode InteractiveServer
@inject INotificationService NotificationService

@code {
    // 原本的變數保持不變
    private List<SearchFilterDefinition> filterDefinitions = new();
    private List<TableColumnDefinition> columnDefinitions = new();
    private List<CustomerType> customerTypes = new();
    
    // 👇 新增欄位配置變數
    private CustomerFieldConfiguration fieldConfiguration = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 原本的 Modal 處理器初始化保持不變
            modalHandler = ModalHelper.CreateModalHandler<Customer, GenericIndexPageComponent<Customer, ICustomerService>>(...);
            
            await InitializeBreadcrumbsAsync();
            
            // 載入相關資料
            await LoadCustomerTypesAsync();
            
            // 👇 使用欄位配置替代原本的三個方法調用
            fieldConfiguration = new CustomerFieldConfiguration(customerTypes, NotificationService);
            filterDefinitions = fieldConfiguration.BuildFilters();
            columnDefinitions = fieldConfiguration.BuildColumns();
            
            // ❌ 移除這些調用
            // await InitializeFiltersAsync();
            // await InitializeTableColumnsAsync();
        }
        catch (Exception ex)
        {
            // 錯誤處理保持不變
        }
    }

    // 👇 大幅簡化篩選方法
    private IQueryable<Customer> ApplyCustomerFilters(SearchFilterModel searchModel, IQueryable<Customer> query)
    {
        // 確保 fieldConfiguration 已初始化
        if (fieldConfiguration == null)
        {
            return query.OrderBy(c => c.Code);
        }

        return fieldConfiguration.ApplyFilters(searchModel, query, nameof(ApplyCustomerFilters), GetType());
    }

    // ❌ 刪除這些方法
    // private async Task InitializeFiltersAsync() { ... }
    // private async Task InitializeTableColumnsAsync() { ... }
}
```

## 重要概念

### FilterPropertyName vs PropertyName
```csharp
{
    nameof(Customer.CustomerTypeId),
    new FieldDefinition<Customer>
    {
        PropertyName = "CustomerType.TypeName",      // 表格顯示用（關聯屬性）
        FilterPropertyName = nameof(Customer.CustomerTypeId), // 篩選器用（外鍵）
        // ...
    }
}
```

### 錯誤處理
- 配置類別內建錯誤處理
- 自動記錄錯誤到系統日誌
- 通知使用者發生錯誤
- 提供安全的後備機制

## DataLoaderHelper 輔助類別

### 功能說明
DataLoaderHelper 提供統一的資料載入與錯誤處理機制，大幅簡化資料載入方法的實作。

### 可用方法

#### LoadAsync - 標準資料載入
```csharp
Task<List<TEntity>> LoadAsync<TEntity>(
    Func<Task<List<TEntity>>> loadFunc,           // 資料載入函數
    string entityName,                             // 實體名稱（用於錯誤訊息）
    INotificationService notificationService,      // 通知服務
    Type callerType)                               // 呼叫者類型
where TEntity : class
```

#### LoadAsync (完整版) - 自訂方法名稱
```csharp
Task<List<TEntity>> LoadAsync<TEntity>(
    Func<Task<List<TEntity>>> loadFunc,
    string entityName,
    INotificationService notificationService,
    Type callerType,
    string methodName)                             // 方法名稱（用於錯誤記錄）
where TEntity : class
```

### 使用範例

#### 範例 1: 基本使用
```csharp
private Task<List<Customer>> LoadCustomersAsync()
{
    return DataLoaderHelper.LoadAsync(
        () => CustomerService.GetAllAsync(),
        "客戶",
        NotificationService,
        GetType());
}
```

#### 範例 2: 從不同的服務載入
```csharp
private Task<List<Warehouse>> LoadWarehousesAsync()
{
    return DataLoaderHelper.LoadAsync(
        () => WarehouseService.GetAllAsync(),
        "倉庫",
        NotificationService,
        GetType());
}
```

#### 範例 3: 載入下拉選單資料（變數賦值）
```csharp
private List<CustomerType> customerTypes = new();

private async Task LoadCustomerTypesAsync()
{
    customerTypes = await DataLoaderHelper.LoadAsync(
        () => CustomerService.GetCustomerTypesAsync(),
        "客戶類型",
        NotificationService,
        GetType(),
        nameof(LoadCustomerTypesAsync));
}
```

#### 範例 4: 帶參數的載入
```csharp
private Task<List<Product>> LoadProductsByCategoryAsync(int categoryId)
{
    return DataLoaderHelper.LoadAsync(
        () => ProductService.GetByCategoryAsync(categoryId),
        "產品",
        NotificationService,
        GetType(),
        nameof(LoadProductsByCategoryAsync));
}
```

### 內建功能
- ✅ 完整的 try-catch 錯誤處理
- ✅ 自動使用 ErrorHandlingHelper 記錄錯誤
- ✅ 自動通知使用者錯誤
- ✅ 安全的後備值（空列表）
- ✅ 自動產生方法名稱（或可自訂）

### 程式碼簡化對比

**使用前（15 行）**:
```csharp
private async Task<List<Customer>> LoadCustomersAsync()
{
    try
    {
        return await CustomerService.GetAllAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadCustomersAsync), GetType(), additionalData: "載入客戶資料失敗");
        await NotificationService.ShowErrorAsync("載入客戶資料失敗");
        return new List<Customer>();
    }
}
```

**使用後（7 行，減少 53%）**:
```csharp
private Task<List<Customer>> LoadCustomersAsync()
{
    return DataLoaderHelper.LoadAsync(
        () => CustomerService.GetAllAsync(),
        "客戶",
        NotificationService,
        GetType());
}
```

## BreadcrumbHelper 輔助類別

### 功能說明
BreadcrumbHelper 提供三種便捷方法來建立麵包屑導航，已內建完整的錯誤處理機制。

### 可用方法

#### 1. CreateSimpleAsync - 兩層麵包屑
```csharp
Task<List<BreadcrumbItem>> CreateSimpleAsync(
    string pageName,                      // 頁面名稱
    INotificationService? notificationService = null,
    Type? callerType = null)
```

**使用範例**:
```csharp
breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync(
    "客戶管理",
    NotificationService,
    GetType());
// 結果: 首頁 > 客戶管理
```

#### 2. CreateThreeLevelAsync - 三層麵包屑
```csharp
Task<List<BreadcrumbItem>> CreateThreeLevelAsync(
    string moduleName,                    // 模組名稱
    string pageName,                      // 頁面名稱
    string? moduleUrl = null,             // 模組連結（可選）
    INotificationService? notificationService = null,
    Type? callerType = null)
```

**使用範例**:
```csharp
breadcrumbItems = await BreadcrumbHelper.CreateThreeLevelAsync(
    "庫存管理",
    "倉庫維護",
    NotificationService,
    GetType());
// 結果: 首頁 > 庫存管理 > 倉庫維護

// 帶連結的版本
breadcrumbItems = await BreadcrumbHelper.CreateThreeLevelAsync(
    "採購管理",
    "進貨退出管理",
    "#",  // 模組連結
    NotificationService,
    GetType());
// 結果: 首頁 > 採購管理（可點擊） > 進貨退出管理
```

#### 3. InitializeAsync - 完全自訂
```csharp
Task<List<BreadcrumbItem>> InitializeAsync(
    IEnumerable<BreadcrumbItem> items,    // 自訂項目（不含首頁）
    INotificationService? notificationService = null,
    Type? callerType = null)
```

**使用範例**:
```csharp
breadcrumbItems = await BreadcrumbHelper.InitializeAsync(
    new[]
    {
        new BreadcrumbItem("系統管理", "/systems"),
        new BreadcrumbItem("參數設定", "/systems/parameters"),
        new BreadcrumbItem("編輯")
    },
    NotificationService,
    GetType());
// 結果: 首頁 > 系統管理 > 參數設定 > 編輯
```

### 內建功能
- ✅ 自動添加「首頁」項目
- ✅ 完整的錯誤處理和記錄
- ✅ 自動通知使用者錯誤
- ✅ 安全的後備值（確保至少有首頁）
- ✅ 支援可選的 NotificationService 和錯誤記錄

## 應用到其他 Index 頁面

### 1. 創建對應的配置類別
```csharp
// DepartmentFieldConfiguration.cs
public class DepartmentFieldConfiguration : BaseFieldConfiguration<Department>
{
    public override Dictionary<string, FieldDefinition<Department>> GetFieldDefinitions()
    {
        return new Dictionary<string, FieldDefinition<Department>>
        {
            {
                nameof(Department.Code),
                new FieldDefinition<Department>
                {
                    PropertyName = nameof(Department.Code),
                    DisplayName = "部門代碼",
                    // ...
                }
            },
            // ... 其他欄位
        };
    }
}
```

### 2. 套用到 Index 頁面
```csharp
// DepartmentIndex.razor
private DepartmentFieldConfiguration fieldConfiguration = default!;

protected override async Task OnInitializedAsync()
{
    // ...
    fieldConfiguration = new DepartmentFieldConfiguration();
    filterDefinitions = fieldConfiguration.BuildFilters();
    columnDefinitions = fieldConfiguration.BuildColumns();
}
```

## 優點

1. **代碼重用**: 配置可用於其他相關頁面
2. **維護性**: 欄位修改只需要更新一個地方
3. **一致性**: 篩選器、表格欄位、篩選邏輯完全同步
4. **擴展性**: 容易添加新欄位或修改現有欄位行為
5. **錯誤處理**: 內建完整的錯誤處理機制
6. **可測試性**: 配置邏輯可以獨立測試

## 適用的 Index 頁面

- CustomerIndex, CustomerTypeIndex
- DepartmentIndex, EmployeeIndex  
- SupplierIndex, SupplierTypeIndex
- ProductIndex, UnitIndex, SizeIndex
- WarehouseIndex
- 其他所有使用 GenericIndexPageComponent 的頁面

## 注意事項

1. 需要 `@using ERPCore2.Helpers` 引用
2. 配置類別建議放在 `Helpers/FieldConfiguration/` 目錄下
3. 複雜的自訂模板仍可在 FieldDefinition 中定義
4. 如果不需要自訂排序，可省略 `GetDefaultSort()` 方法

## Index 頁面開發檢查清單

### 必要的依賴注入
```csharp
@inject INotificationService NotificationService
@inject IXXXService XXXService  // 對應的服務
```

### 必要的變數聲明
```csharp
// 欄位配置
private XXXFieldConfiguration fieldConfiguration = default!;

// 配置相關
private List<SearchFilterDefinition> filterDefinitions = new();
private List<TableColumnDefinition> columnDefinitions = new();
private List<BreadcrumbItem> breadcrumbItems = new();

// Modal 相關 (如果需要)
private ModalHandler<XXX, GenericIndexPageComponent<XXX, IXXXService>> modalHandler = default!;
```

### 必須實作的方法
- ✅ `OnInitializedAsync()` - 包含完整的 try-catch
- ✅ `InitializeBreadcrumbsAsync()` - 包含錯誤處理
- ✅ `LoadXXXAsync()` - 主要資料載入，包含錯誤處理
- ✅ `LoadRelatedDataAsync()` - 相關資料載入 (如下拉選單資料)
- ✅ `ApplyXXXFilters()` - 篩選邏輯，包含錯誤處理

### 錯誤處理檢查項目
- ✅ 每個方法都有 try-catch
- ✅ 使用 `ErrorHandlingHelper.HandlePageErrorAsync()` 記錄錯誤
- ✅ 使用 `NotificationService.ShowErrorAsync()` 通知使用者
- ✅ 提供安全的後備值
- ✅ 非同步錯誤處理使用 `_ = Task.Run(async () => { ... });`

### 代碼品質檢查
- ✅ 方法命名遵循 `XxxAsync` 或 `ApplyXxxFilters` 模式
- ✅ 錯誤訊息具有描述性且使用者友善
- ✅ additionalData 包含有用的除錯資訊
- ✅ 後備值確保頁面不會崩潰