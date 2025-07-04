GenericManagementService<T>：提供標準 CRUD 操作，所有服務都應繼承
專用服務介面：每個業務領域建立專屬的 `I[業務領域]Service` 介面
統一錯誤處理：使用 ServiceResult 封裝操作結果
命名空間：統一使用 `ERPCore2.Services`
檔案位置：`Service/[類型名稱]/` 底下放 `I[功能]Service` 和 `[功能]Service`
實體屬性：使用 Entity 中定義的屬性名稱，外鍵格式為 `[表名稱]Id`

## 統一的 Service 架構模式

### 建構子注入標準模式

所有 Service 必須遵循統一的建構子注入模式，包含以下必要依賴：

```csharp
public class [業務領域]Service : GenericManagementService<[實體]>, I[業務領域]Service
{
    private readonly ILogger<[業務領域]Service> _logger;
    private readonly IErrorLogService _errorLogService;

    public [業務領域]Service(AppDbContext context, ILogger<[業務領域]Service> logger, IErrorLogService errorLogService) 
        : base(context)
    {
        _logger = logger;
        _errorLogService = errorLogService;
    }
}
```

**必要依賴說明：**
- `AppDbContext context`：資料庫上下文，透過基底類別處理
- `ILogger<T> logger`：日誌記錄服務，用於記錄操作日誌
- `IErrorLogService errorLogService`：錯誤記錄服務，用於記錄錯誤至 ErrorLog 資料表

### 完整的錯誤處理機制

所有 Service 的公開方法都必須實作統一的錯誤處理模式：

#### 1. 異步方法錯誤處理
```csharp
public async Task<List<[實體]>> GetAllAsync()
{
    try
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        await _errorLogService.LogErrorAsync(ex);
        _logger.LogError(ex, "Error in GetAllAsync");
        return new List<[實體]>();
    }
}
```

#### 2. 同步方法錯誤處理
```csharp
public bool IsValidMethod(string parameter)
{
    try
    {
        // 業務邏輯
        return true;
    }
    catch (Exception ex)
    {
        _errorLogService.LogErrorAsync(ex).Wait();
        _logger.LogError(ex, "Error in IsValidMethod");
        return false;
    }
}
```

#### 3. 回傳 ServiceResult 的方法
```csharp
public async Task<ServiceResult> ValidateAsync([實體] entity)
{
    try
    {
        var errors = new List<string>();
        
        // 驗證邏輯
        if (string.IsNullOrWhiteSpace(entity.Name))
            errors.Add("名稱為必填欄位");
            
        if (errors.Any())
            return ServiceResult.Failure(string.Join("; ", errors));
            
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        await _errorLogService.LogErrorAsync(ex);
        _logger.LogError(ex, "Error in ValidateAsync");
        return ServiceResult.Failure("驗證過程發生錯誤");
    }
}
```

### 標準範例：SizeService

以下是完全符合標準的 SizeService 實作範例：

```csharp
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class SizeService : GenericManagementService<Size>, ISizeService
    {
        private readonly ILogger<SizeService> _logger;
        private readonly IErrorLogService _errorLogService;

        public SizeService(AppDbContext context, ILogger<SizeService> logger, IErrorLogService errorLogService) 
            : base(context)
        {
            _logger = logger;
            _errorLogService = errorLogService;
        }

        public override async Task<List<Size>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "Error in GetAllAsync");
                return new List<Size>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Size entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("尺寸名稱為必填欄位");

                if (!string.IsNullOrWhiteSpace(entity.Code) && await IsCodeExistsAsync(entity.Code, entity.Id))
                    errors.Add("尺寸代碼已存在");

                if (await IsNameExistsAsync(entity.Name, entity.Id))
                    errors.Add("尺寸名稱已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "Error in ValidateAsync");
                return ServiceResult.Failure("驗證尺寸時發生錯誤");
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(s => s.Code == code && !s.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "Error in IsCodeExistsAsync");
                return false;
            }
        }
    }
}
```

### 錯誤處理要點

1. **必須包裝所有公開方法**：每個 public 方法都要有 try-catch
2. **雙重錯誤記錄**：
   - `_errorLogService.LogErrorAsync(ex)` - 記錄至 ErrorLog 資料表
   - `_logger.LogError(ex, "錯誤描述")` - 記錄至應用程式日誌
3. **安全回傳值**：
   - List 回傳空列表
   - 單一實體回傳 null
   - bool 回傳 false
   - ServiceResult 回傳包含錯誤訊息的 Failure
4. **同步方法處理**：使用 `.Wait()` 調用異步錯誤記錄方法

1. 建立服務介面
```csharp
// Services/Customers/ICustomerService.cs
namespace ERPCore2.Services;

public interface ICustomerService : IGenericManagementService<Customer>
{
    Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null);
    Task<List<Customer>> GetByIndustryTypeAsync(int industryTypeId);
    Task<ServiceResult> UpdateCustomerStatusAsync(int customerId, EntityStatus status);
}
```

2. 實作服務類別（遵循統一模式）
```csharp
// Services/Customers/CustomerService.cs
using ERPCore2.Helpers;

namespace ERPCore2.Services;

public class CustomerService : GenericManagementService<Customer>, ICustomerService
{
    private readonly ILogger<CustomerService> _logger;
    private readonly IErrorLogService _errorLogService;

    public CustomerService(AppDbContext context, ILogger<CustomerService> logger, IErrorLogService errorLogService) 
        : base(context) 
    { 
        _logger = logger;
        _errorLogService = errorLogService;
    }
    
    // 覆寫基底方法（載入關聯資料）
    public override async Task<List<Customer>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .Include(c => c.CustomerType)
                .Include(c => c.IndustryType)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CustomerCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await _errorLogService.LogErrorAsync(ex);
            _logger.LogError(ex, "Error in GetAllAsync");
            return new List<Customer>();
        }
    }
    
    // 實作業務特定方法
    public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
    {
        try
        {
            var query = _dbSet.Where(c => c.CustomerCode == customerCode && !c.IsDeleted);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            await _errorLogService.LogErrorAsync(ex);
            _logger.LogError(ex, "Error in IsCustomerCodeExistsAsync");
            return false; // 安全回傳預設值
        }
    }
    
    // 覆寫驗證方法
    public override async Task<ServiceResult> ValidateAsync(Customer entity)
    {
        try
        {
            var errors = new List<string>();
            
            // 基本驗證
            if (string.IsNullOrWhiteSpace(entity.CustomerCode))
                errors.Add("客戶代碼不能為空");
            
            if (string.IsNullOrWhiteSpace(entity.CompanyName))
                errors.Add("公司名稱不能為空");
            
            // 業務邏輯驗證
            if (!string.IsNullOrWhiteSpace(entity.CustomerCode))
            {
                bool codeExists = await IsCustomerCodeExistsAsync(entity.CustomerCode, entity.Id == 0 ? null : entity.Id);
                if (codeExists)
                    errors.Add("客戶代碼已存在");
            }
            
            if (errors.Any())
                return ServiceResult.Failure(string.Join("; ", errors));
                
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            await _errorLogService.LogErrorAsync(ex);
            _logger.LogError(ex, "Error in ValidateAsync");
            return ServiceResult.Failure("驗證過程發生錯誤");
        }
    }
}
```

GenericManagementService 功能
CRUD：`GetAllAsync()`, `GetByIdAsync()`, `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`
批次操作：`CreateBatchAsync()`, `UpdateBatchAsync()`, `DeleteBatchAsync()`
查詢：`GetPagedAsync()`, `SearchAsync()`, `ExistsAsync()`, `GetCountAsync()`
狀態管理：`SetStatusAsync()`, `ToggleStatusAsync()`, `SetStatusBatchAsync()`
驗證：`ValidateAsync()`, `IsNameExistsAsync()`

自動處理功能
- 軟刪除（IsDeleted 標記）
- 稽核欄位（CreatedAt、UpdatedAt、CreatedBy、UpdatedBy）
- 狀態管理（EntityStatus 枚舉）

## 依賴注入設定

專案使用統一的服務註冊機制，將所有依賴注入設定集中管理在 `ServiceRegistration` 類別中。

### Program.cs 設定
```csharp
// Program.cs - 統一註冊所有應用程式服務
builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection")!);
```

### ServiceRegistration.cs 實作
```csharp
// Data/ServiceRegistration.cs
public static void AddApplicationServices(this IServiceCollection services, string connectionString)
{
    // 註冊資料庫服務（使用 DbContextFactory 解決並發問題）
    services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    // 註冊業務邏輯服務
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IAddressTypeService, AddressTypeService>();
    // ... 其他服務註冊
}
```

## 錯誤處理最佳實踐

專案採用統一的錯誤處理策略，結合 `ErrorHandlingHelper` 和 `IErrorLogService`，確保錯誤處理的一致性和可維護性。

### Service 層錯誤處理

#### 建構子注入模式
```csharp
public class [業務領域]Service : GenericManagementService<[實體]>, I[業務領域]Service
{
    private readonly ILogger<[業務領域]Service> _logger;
    private readonly IErrorLogService _errorLogService;

    public [業務領域]Service(
        AppDbContext context, 
        ILogger<[業務領域]Service> logger, 
        IErrorLogService errorLogService) : base(context)
    {
        _logger = logger;
        _errorLogService = errorLogService;
    }
}
```

#### 錯誤處理策略
1. **查詢方法**：記錄錯誤並重新拋出，讓上層處理
2. **驗證方法**：記錄錯誤並回傳包含錯誤訊息的 ServiceResult
3. **業務邏輯方法**：記錄錯誤並回傳安全的預設值

### Razor 頁面錯誤處理

#### 頁面層級注入
```csharp
@page "/[路由]"
@using ERPCore2.Helpers
@inject I[業務領域]Service [業務領域]Service
@inject INotificationService NotificationService
```

#### 資料載入錯誤處理
```csharp
// 簡單的資料載入
private async Task<List<[實體]>> Load[實體]sAsync()
{
    return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
        async () => await [業務領域]Service.GetAllAsync(),
        new List<[實體]>(),
        NotificationService,
        "載入[實體]資料失敗"
    );
}

// 複雜的資料載入（含額外處理）
private async Task LoadBasicDataAsync()
{
    try
    {
        // 載入基礎資料
        var data = await [業務領域]Service.GetSomeDataAsync();
        // 處理資料...
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleErrorSimplyAsync(
            ex, 
            nameof(LoadBasicDataAsync), 
            NotificationService,
            "載入基礎資料失敗"
        );
        // 設定預設值或錯誤狀態
        StateHasChanged();
    }
}
```

#### Service 結果錯誤處理
```csharp
private async Task SaveAsync()
{
    var result = await [業務領域]Service.CreateAsync(entity);
    
    if (result.Success)
    {
        await NotificationService.ShowSuccessAsync("儲存成功");
        Navigation.NavigateTo("/[路由]");
    }
    else
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(
            result,
            nameof(SaveAsync),
            NotificationService,
            "儲存失敗"
        );
    }
}
```

### ErrorHandlingHelper 方法說明

#### ExecuteWithErrorHandlingAsync<T>
- **用途**：包裝異步操作，自動處理錯誤並回傳預設值
- **適用**：簡單的資料載入操作
- **特點**：發生錯誤時顯示通知並回傳預設值

#### HandleErrorSimplyAsync
- **用途**：處理捕獲的例外，顯示使用者友善訊息
- **適用**：複雜的業務邏輯處理
- **特點**：需要手動 try-catch，但提供統一的錯誤通知

#### HandleServiceErrorAsync
- **用途**：處理 ServiceResult 的錯誤狀態
- **適用**：Service 層回傳的操作結果
- **特點**：自動處理驗證錯誤和業務邏輯錯誤

### 錯誤記錄最佳實踐

1. **Service 層**：使用 `IErrorLogService.LogErrorAsync()` 記錄詳細錯誤資訊
2. **Razor 頁面**：使用 `ErrorHandlingHelper` 處理錯誤並顯示使用者友善訊息
3. **記錄內容**：包含方法名稱、參數值、業務上下文等資訊
4. **錯誤分類**：根據錯誤性質選擇適當的處理策略

### 依賴注入更新

確保在 `ServiceRegistration.cs` 中註冊 `IErrorLogService`：

```csharp
public static void AddApplicationServices(this IServiceCollection services, string connectionString)
{
    // ... 現有註冊

    // 錯誤記錄服務
    services.AddScoped<IErrorLogService, ErrorLogService>();
    
    // 業務邏輯服務（更新建構子參數）
    services.AddScoped<ICustomerService, CustomerService>();
    // ... 其他服務
}
```

## Service 實作檢查清單

在實作或更新 Service 時，請確認以下項目：

### ✅ 建構子檢查
- [ ] 包含 `ILogger<T> logger` 參數
- [ ] 包含 `IErrorLogService errorLogService` 參數
- [ ] 正確儲存到私有欄位

### ✅ 錯誤處理檢查
- [ ] 所有公開方法都有 try-catch 包裝
- [ ] catch 區塊中調用 `_errorLogService.LogErrorAsync(ex)`
- [ ] catch 區塊中調用 `_logger.LogError(ex, "錯誤描述")`
- [ ] 所有方法都回傳安全的預設值

### ✅ 回傳值檢查
- [ ] `Task<List<T>>` 方法：回傳空列表 `new List<T>()`
- [ ] `Task<T?>` 方法：回傳 `null`
- [ ] `Task<bool>` 方法：回傳 `false`
- [ ] `Task<ServiceResult>` 方法：回傳 `ServiceResult.Failure("錯誤訊息")`
- [ ] 同步方法：使用 `.Wait()` 調用錯誤記錄

### ✅ 程式碼品質檢查
- [ ] 遵循命名規範
- [ ] 添加適當的 XML 註解
- [ ] 通過編譯檢查
- [ ] 符合專案的程式碼風格