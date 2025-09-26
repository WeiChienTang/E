# Service 層開發規範

## 核心原則
- **GenericManagementService<T>**：所有服務必須繼承，提供標準 CRUD 操作
- **統一錯誤處理**：使用 ServiceResult 封裝操作結果
- **命名規範**：`I[業務領域]Service` 介面、`[業務領域]Service` 實作
- **檔案位置**：`Service/[類型名稱]/` 目錄結構

## 標準實作模式

### 1. 建構子注入

#### 簡易版建構子（適用於簡單測試或最小依賴）
```csharp
public class [業務領域]Service : GenericManagementService<[實體]>, I[業務領域]Service
{
    public [業務領域]Service(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    {
    }
}
```

#### 完整版建構子（適用於生產環境的完整功能）
```csharp
public class [業務領域]Service : GenericManagementService<[實體]>, I[業務領域]Service
{
    public [業務領域]Service(
        IDbContextFactory<AppDbContext> contextFactory, 
        ILogger<GenericManagementService<[實體]>> logger) : base(contextFactory, logger)
    {
    }
}
```

```csharp
// 正確做法 - 直接使用基底類別提供的欄位
public class ColorService : GenericManagementService<Color>, IColorService
{
    public ColorService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    {
    }

    public ColorService(
        IDbContextFactory<AppDbContext> contextFactory, 
        ILogger<GenericManagementService<Color>> logger) : base(contextFactory, logger)
    {
    }
    
    // 可直接使用基底類別的 protected 欄位：
    // _contextFactory, _logger
}
```

### 2. 錯誤處理模式
所有公開方法必須包含 try-catch，使用 `ErrorHandlingHelper` 統一處理：

```csharp
// 異步方法範例
public async Task<List<[實體]>> GetAllAsync()
{
    try
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.[實體複數名稱]
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
            Method = nameof(GetAllAsync),
            ServiceType = GetType().Name 
        });
        return new List<[實體]>();  // 安全預設值
    }
}

// 同步方法範例（不建議使用，建議改用異步）
public List<[實體]> GetAllSync()
{
    try
    {
        using var context = _contextFactory.CreateDbContext();
        return context.[實體複數名稱]
            .OrderBy(e => e.Name)
            .ToList();
    }
    catch (Exception ex)
    {
        ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetAllSync), GetType(), _logger, new { 
            Method = nameof(GetAllSync),
            ServiceType = GetType().Name 
        });
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
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
            Method = nameof(ValidateAsync),
            ServiceType = GetType().Name,
            EntityId = entity.Id,
            EntityName = entity.Name 
        });
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
    }

    public class CustomerService : GenericManagementService<Customer>, ICustomerService
    {
        public CustomerService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Customer>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<Customer>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Customers
                    .Include(c => c.CustomerType)
                    .OrderBy(c => c.CustomerCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Customer>();
            }
        }

        public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Customers.Where(c => c.CustomerCode == customerCode);
                if (excludeId.HasValue)
                    query = query.Where(c => c.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCustomerCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsCustomerCodeExistsAsync),
                    ServiceType = GetType().Name,
                    CustomerCode = customerCode,
                    ExcludeId = excludeId 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.CompanyName 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
```

## GenericManagementService 提供的功能

### 基底類別提供的 Protected 成員
- **`_contextFactory`**：資料庫上下文工廠 (`IDbContextFactory<AppDbContext>`)
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
## 實作檢查清單

- [ ] 繼承 `GenericManagementService<T>`
- [ ] **不要** 重複宣告 `_logger`, `_errorLogService`, `_contextFactory` 欄位
- [ ] 建構子使用正確的泛型類型：`IDbContextFactory<AppDbContext>` 和 `ILogger<GenericManagementService<T>>`
- [ ] 建構子正確調用基底類別：`base(contextFactory, logger)`
- [ ] 方法中使用 `using var context = await _contextFactory.CreateDbContextAsync();`
- [ ] 查詢改為 `return await context.[實體複數名稱]...`
- [ ] 實作必要的抽象方法：`SearchAsync()` 和 `ValidateAsync()`
- [ ] 所有公開方法都有 try-catch 錯誤處理
- [ ] 異步方法使用 `ErrorHandlingHelper.HandleServiceErrorAsync()` 並傳入 `GetType()` 和詳細資訊
- [ ] 同步方法使用 `ErrorHandlingHelper.HandleServiceErrorSync()` 並傳入 `GetType()` 和詳細資訊
- [ ] 頁面層使用 `ErrorHandlingHelper.HandlePageErrorAsync()`
- [ ] 回傳安全預設值
- [ ] 在 `ServiceRegistration.cs` 註冊服務
- [ ] 在 `Program.cs` 中初始化 `ErrorHandlingHelper.Initialize(app.Services)`