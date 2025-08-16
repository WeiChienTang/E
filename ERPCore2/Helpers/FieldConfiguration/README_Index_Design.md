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
```csharp
private async Task InitializeBreadcrumbsAsync()
{
    try
    {
        breadcrumbItems = new List<GenericHeaderComponent.BreadcrumbItem>
        {
            new("首頁", "/"),
            new("客戶管理")
        };
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(InitializeBreadcrumbsAsync), GetType(), additionalData: "初始化麵包屑導航失敗");

        // 設定安全的預設值
        breadcrumbItems = new List<GenericHeaderComponent.BreadcrumbItem>();
    }
}
```

#### LoadXXXAsync() - 資料載入方法
```csharp
private async Task<List<Customer>> LoadCustomersAsync()
{
    try
    {
        return await CustomerService.GetAllAsync();
    }
    catch (Exception ex)
    {
        // 記錄錯誤到資料庫並通知使用者
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadCustomersAsync), GetType(), additionalData: "載入客戶資料失敗");
        await NotificationService.ShowErrorAsync("載入客戶資料失敗");
        // 設定安全的預設值
        return new List<Customer>();
    }
}

private async Task LoadCustomerTypesAsync()
{
    try
    {
        customerTypes = await CustomerService.GetCustomerTypesAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadCustomerTypesAsync), GetType(), additionalData: "載入客戶類型資料失敗");
        await NotificationService.ShowErrorAsync("載入客戶類型資料失敗");
        // 設定安全的預設值
        customerTypes = new List<CustomerType>();
    }
}
```

#### ApplyXXXFilters() - 篩選方法 (唯一非 Async)
```csharp
private IQueryable<Customer> ApplyCustomerFilters(SearchFilterModel searchModel, IQueryable<Customer> query)
{
    try
    {
        return fieldConfiguration.ApplyFilters(searchModel, query, nameof(ApplyCustomerFilters), GetType());
    }
    catch (Exception ex)
    {
        // 記錄錯誤並回傳安全的預設查詢
        _ = ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(ApplyCustomerFilters), GetType(), additionalData: "客戶篩選器應用失敗");
        _ = NotificationService.ShowErrorAsync("篩選條件應用失敗，已顯示全部資料");
        
        // 回傳基本排序的查詢，確保頁面仍能正常運作
        return query.OrderBy(c => c.Code);
    }
}
```

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
breadcrumbItems = new List<GenericHeaderComponent.BreadcrumbItem>();

// 查詢類型
return query.OrderBy(c => c.Code);

// 配置類型
filterDefinitions = new List<SearchFilterDefinition>();
columnDefinitions = new List<TableColumnDefinition>();
```

## 使用範例：CustomerIndex 重構

### 修改前的問題
```csharp
// 原本需要三個分離的方法，代碼重複
private async Task InitializeFiltersAsync() { ... }      // 30+ 行
private async Task InitializeTableColumnsAsync() { ... } // 15+ 行  
private IQueryable<Customer> ApplyCustomerFilters() { ... } // 35+ 行
```

### 修改步驟

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
        try
        {
            return fieldConfiguration.ApplyFilters(searchModel, query, nameof(ApplyCustomerFilters), GetType());
        }
        catch (Exception ex)
        {
            // 錯誤處理
            return query.OrderBy(c => c.Code);
        }
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

## 代碼減少統計

### 修改前
```
InitializeFiltersAsync()     : ~30 行
InitializeTableColumnsAsync(): ~15 行
ApplyCustomerFilters()       : ~35 行
總計                         : ~80 行
```

### 修改後
```
欄位配置類別                : ~60 行 (可重用)
OnInitializedAsync()       : ~20 行 (簡化)
ApplyCustomerFilters()     : ~8 行 (大幅簡化)
總計                      : ~28 行 (頁面內)
```

**代碼減少**: ~65% (52 行)

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
private List<GenericHeaderComponent.BreadcrumbItem> breadcrumbItems = new();

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

## 總結

通過使用欄位配置系統並遵循標準的設計規範，可以達到以下目標：

### 技術優勢
- **集中管理** - 將分散的欄位定義邏輯集中管理
- **大幅減少重複代碼** - 提高代碼重用性
- **提高可維護性** - 修改欄位只需要更新一個地方
- **確保一致性** - 所有 Index 頁面遵循相同的設計模式

### 穩定性保證
- **完整的錯誤處理** - 每個方法都有適當的錯誤處理機制
- **使用者友善** - 錯誤發生時有清楚的通知訊息
- **系統穩定** - 提供後備機制，避免頁面崩潰
- **除錯支援** - 詳細的錯誤記錄協助問題診斷

### 開發效率
- **標準化流程** - 有明確的開發檢查清單
- **可擴展性** - 容易應用到新的 Index 頁面
- **可測試性** - 配置邏輯可以獨立測試
- **團隊協作** - 統一的代碼風格和錯誤處理模式

這是一個可擴展且穩健的解決方案，適合應用到整個系統的 Index 頁面開發。
