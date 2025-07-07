# Service 層開發規範

## 核心原則
- **GenericManagementService<T>**：所有服務必須繼承，提供標準 CRUD 操作
- **統一錯誤處理**：使用 ServiceResult 封裝操作結果
- **命名規範**：`I[業務領域]Service` 介面、`[業務領域]Service` 實作
- **檔案位置**：`Service/[類型名稱]/` 目錄結構

## 標準實作模式

### 1. 建構子注入
```csharp
public class [業務領域]Service : GenericManagementService<[實體]>, I[業務領域]Service
{
    public [業務領域]Service(
        AppDbContext context, 
        ILogger<GenericManagementService<[實體]>> logger, 
        IErrorLogService errorLogService) : base(context, logger, errorLogService)
    {
    }
}
```

⚠️ **重要提醒：避免重複宣告基底類別欄位**
- **不要** 在子類別中重複宣告 `ILogger` 和 `IErrorLogService` 欄位
- **不要** 在子類別中重複宣告 `_context` 和 `_dbSet` 欄位
- 基底類別 `GenericManagementService<T>` 已提供這些欄位為 `protected`，可直接使用
- 建構子參數中的 `ILogger` 泛型類型應為 `ILogger<GenericManagementService<T>>`，不是 `ILogger<ServiceClassName>`

```csharp
// 正確做法 - 直接使用基底類別提供的欄位
public class CustomerService : GenericManagementService<Customer>, ICustomerService
{
    public CustomerService(
        AppDbContext context, 
        ILogger<GenericManagementService<Customer>> logger, 
        IErrorLogService errorLogService) : base(context, logger, errorLogService)
    {
    }
    
    // 可直接使用基底類別的 protected 欄位：
    // _context, _dbSet, _logger, _errorLogService
}
```

### 2. 錯誤處理模式
所有公開方法必須包含 try-catch：

```csharp
// 異步方法範例
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
        return new List<[實體]>();  // 安全預設值
    }
}

// ServiceResult 方法範例
public async Task<ServiceResult> ValidateAsync([實體] entity)
{
    try
    {
        var errors = new List<string>();
        
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

### 3. 安全回傳值規則
- `List<T>` → 空列表 `new List<T>()`
- `T?` → `null`
- `bool` → `false`
- `ServiceResult` → `ServiceResult.Failure("錯誤訊息")`

## 完整實作範例

```csharp
namespace ERPCore2.Services
{
    public interface ICustomerService : IGenericManagementService<Customer>
    {
        Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null);
        Task<List<Customer>> GetByIndustryTypeAsync(int industryTypeId);
    }

    public class CustomerService : GenericManagementService<Customer>, ICustomerService
    {
        public CustomerService(
            AppDbContext context, 
            ILogger<GenericManagementService<Customer>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

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
                return false;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Customer entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.CustomerCode))
                    errors.Add("客戶代碼不能為空");
                
                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                    errors.Add("公司名稱不能為空");
                
                if (!string.IsNullOrWhiteSpace(entity.CustomerCode) && 
                    await IsCustomerCodeExistsAsync(entity.CustomerCode, entity.Id == 0 ? null : entity.Id))
                    errors.Add("客戶代碼已存在");
                
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
}
```

## GenericManagementService 提供的功能

### 基底類別提供的 Protected 成員
- **`_context`**：資料庫上下文 (`AppDbContext`)
- **`_dbSet`**：當前實體的 DbSet (`DbSet<T>`)
- **`_logger`**：日誌記錄器 (`ILogger<GenericManagementService<T>>`)
- **`_errorLogService`**：錯誤記錄服務 (`IErrorLogService`)

### 標準 CRUD 方法
- **CRUD**：`GetAllAsync()`, `GetByIdAsync()`, `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`
- **批次操作**：`CreateBatchAsync()`, `UpdateBatchAsync()`, `DeleteBatchAsync()`
- **查詢**：`GetPagedAsync()`, `SearchAsync()`, `ExistsAsync()`, `GetCountAsync()`
- **狀態管理**：`SetStatusAsync()`, `ToggleStatusAsync()`, `SetStatusBatchAsync()`
- **驗證**：`ValidateAsync()`, `IsNameExistsAsync()`

### 必須實作的抽象方法
- **`SearchAsync(string searchTerm)`**：實作特定實體的搜尋邏輯
- **`ValidateAsync(T entity)`**：實作特定實體的驗證邏輯

## 依賴注入設定

在 `Data/ServiceRegistration.cs` 中註冊服務：

```csharp
public static void AddApplicationServices(this IServiceCollection services, string connectionString)
{
    // 資料庫上下文
    services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    // 錯誤記錄服務
    services.AddScoped<IErrorLogService, ErrorLogService>();
    
    // 業務邏輯服務
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IAddressTypeService, AddressTypeService>();
    // ... 其他服務
}
```

## Razor 頁面錯誤處理

使用 `ErrorHandlingHelper` 統一處理頁面層級的錯誤：

```csharp
@inject I[業務領域]Service [業務領域]Service
@inject INotificationService NotificationService

// 簡單資料載入
private async Task<List<[實體]>> Load[實體]sAsync()
{
    return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
        async () => await [業務領域]Service.GetAllAsync(),
        new List<[實體]>(),
        NotificationService,
        "載入資料失敗"
    );
}

// 處理 ServiceResult
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
        await ErrorHandlingHelper.HandleServiceResultErrorAsync(
            result, NotificationService, "儲存失敗"
        );
    }
}
```

## 實作檢查清單

- [ ] 繼承 `GenericManagementService<T>`
- [ ] **不要** 重複宣告 `_logger`, `_errorLogService`, `_context`, `_dbSet` 欄位
- [ ] 建構子使用正確的泛型類型：`ILogger<GenericManagementService<T>>`
- [ ] 建構子正確調用基底類別：`base(context, logger, errorLogService)`
- [ ] 實作必要的抽象方法：`SearchAsync()` 和 `ValidateAsync()`
- [ ] 所有公開方法都有 try-catch 錯誤處理
- [ ] 錯誤記錄：`_errorLogService.LogErrorAsync()` + `_logger.LogError()`
- [ ] 回傳安全預設值
- [ ] 在 `ServiceRegistration.cs` 註冊服務