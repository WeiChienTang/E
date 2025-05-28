# Services 資料夾說明文檔

## 概述
Services 資料夾包含了 ERPCore2 系統的業務邏輯層實作，負責處理業務規則、資料驗證、商業邏輯封裝，並直接使用 EF Core DbContext 進行資料操作。

**核心設計理念：簡化架構，移除 Repository 層，Service 直接使用 EF Core，不使用 DTO，直接操作 Entity 模型。**

## 命名規範

### 命名空間
所有 Services 資料夾下的檔案統一使用：`ERPCore2.Services`

### 檔案命名
- **服務實作**：`[業務領域]Service.cs`（如：CustomerService.cs）
- **服務介面**：`I[業務領域]Service.cs`（如：ICustomerService.cs）
- **結果類別**：`ServiceResult.cs`

---

## 架構設計原則

### 為什麼簡化架構？
- **EF Core 本身就是完整的資料存取層**：DbContext 和 DbSet 已提供完整功能
- **避免過度設計**：移除不必要的 Repository 和 DTO 層
- **更直接的操作**：充分利用 EF Core 的 LINQ、Include 等功能
- **更好的效能**：減少資料轉換和抽象層開銷

### Service 層職責
- **直接的 EF Core 資料操作**：使用 DbContext 進行 CRUD
- **業務規則驗證**：檢查業務邏輯和資料完整性
- **Entity 驗證**：使用 DataAnnotations 驗證
- **稽核欄位管理**：設定 CreatedDate、ModifiedBy 等
- **關聯資料載入**：使用 Include() 載入相關實體
- **錯誤處理**：使用 ServiceResult 封裝結果
- **交易管理**：處理複雜的跨實體操作

---

## Service 標準結構

### 1. 服務介面定義
```csharp
public interface ICustomerService
{
    // 基本 CRUD 操作
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<ServiceResult<Customer>> CreateAsync(Customer customer);
    Task<ServiceResult<Customer>> UpdateAsync(Customer customer);
    Task<ServiceResult> DeleteAsync(int id);
    
    // 業務特定方法
    Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null);
    Task<List<Customer>> SearchAsync(string searchTerm);
}
```

### 2. 服務實作結構
```csharp
public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomerService> _logger;
    
    public CustomerService(AppDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // 實作方法...
}
```

### 3. ServiceResult 使用模式
```csharp
// 成功結果
return ServiceResult<Customer>.Success(customer);

// 失敗結果
return ServiceResult<Customer>.Failure("錯誤訊息");

// 驗證失敗
return ServiceResult.ValidationFailure(validationErrors);
```

---

## Service 方法設計規範

### 基本 CRUD 模式

#### GetAll 方法
```csharp
public async Task<List<Customer>> GetAllAsync()
{
    return await _context.Customers
        .Where(c => c.Status != EntityStatus.Deleted)
        .Include(c => c.CustomerType)
        .Include(c => c.Industry)
        .OrderBy(c => c.CompanyName)
        .ToListAsync();
}
```

#### Create 方法
```csharp
public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
{
    try
    {
        // 1. 業務驗證
        var validationResult = await ValidateCustomerAsync(customer);
        if (!validationResult.IsSuccess)
            return ServiceResult<Customer>.Failure(validationResult.ErrorMessage);
        
        // 2. 業務規則檢查
        if (await IsCustomerCodeExistsAsync(customer.CustomerCode))
            return ServiceResult<Customer>.Failure("客戶代碼已存在");
        
        // 3. 設定系統欄位
        customer.CreatedDate = DateTime.Now;
        customer.CreatedBy = "系統管理員";
        customer.Status = EntityStatus.Active;
        
        // 4. 儲存資料
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        return ServiceResult<Customer>.Success(customer);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "建立客戶時發生錯誤");
        return ServiceResult<Customer>.Failure("建立客戶時發生錯誤");
    }
}
```

#### Update 方法
```csharp
public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
{
    try
    {
        // 1. 檢查實體存在
        var existingCustomer = await GetByIdAsync(customer.CustomerId);
        if (existingCustomer == null)
            return ServiceResult<Customer>.Failure("客戶不存在");
        
        // 2. 業務規則檢查
        if (await IsCustomerCodeExistsAsync(customer.CustomerCode, customer.CustomerId))
            return ServiceResult<Customer>.Failure("客戶代碼已存在");
        
        // 3. 設定稽核欄位
        customer.ModifiedDate = DateTime.Now;
        customer.ModifiedBy = "系統管理員";
        
        // 4. 更新資料
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        
        return ServiceResult<Customer>.Success(customer);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "更新客戶時發生錯誤 ID: {CustomerId}", customer.CustomerId);
        return ServiceResult<Customer>.Failure("更新客戶時發生錯誤");
    }
}
```

#### Delete 方法（軟刪除）
```csharp
public async Task<ServiceResult> DeleteAsync(int id)
{
    try
    {
        var customer = await GetByIdAsync(id);
        if (customer == null)
            return ServiceResult.Failure("客戶不存在");
        
        // 業務規則檢查（如檢查是否有關聯訂單等）
        var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
        if (hasOrders)
            return ServiceResult.Failure("客戶有關聯訂單，無法刪除");
        
        // 軟刪除
        customer.Status = EntityStatus.Deleted;
        customer.ModifiedDate = DateTime.Now;
        customer.ModifiedBy = "系統管理員";
        
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "刪除客戶時發生錯誤 ID: {CustomerId}", id);
        return ServiceResult.Failure("刪除客戶時發生錯誤");
    }
}
```

---

## 驗證和業務規則

### Entity 驗證
```csharp
private async Task<ServiceResult> ValidateCustomerAsync(Customer customer)
{
    // 使用 DataAnnotations 驗證
    var context = new ValidationContext(customer);
    var results = new List<ValidationResult>();
    
    if (!Validator.TryValidateObject(customer, context, results, true))
    {
        var errors = results.Select(r => r.ErrorMessage ?? "驗證錯誤").ToList();
        return ServiceResult.ValidationFailure(errors);
    }
    
    // 額外的業務驗證
    if (string.IsNullOrWhiteSpace(customer.CustomerCode))
        return ServiceResult.Failure("客戶代碼為必填");
    
    return ServiceResult.Success();
}
```

### 重複檢查
```csharp
public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
{
    var query = _context.Customers
        .Where(c => c.CustomerCode == customerCode && c.Status != EntityStatus.Deleted);
    
    if (excludeId.HasValue)
        query = query.Where(c => c.CustomerId != excludeId.Value);
    
    return await query.AnyAsync();
}
```

---

## 複雜業務操作

### 使用交易
```csharp
public async Task<ServiceResult> ComplexBusinessOperationAsync(int customerId, SomeRequest request)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 步驟 1：業務邏輯處理
        var customer = await GetByIdAsync(customerId);
        if (customer == null)
            throw new ArgumentException("客戶不存在");
        
        // 步驟 2：更新多個相關實體
        var addresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
        
        foreach (var address in addresses)
        {
            // 業務邏輯處理
            address.ModifiedDate = DateTime.Now;
        }
        
        // 步驟 3：儲存變更
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "複雜業務操作失敗 CustomerId: {CustomerId}", customerId);
        return ServiceResult.Failure("操作失敗");
    }
}
```

---

## 效能最佳化

### 查詢最佳化
```csharp
// 使用 AsNoTracking 提升查詢效能（只讀情況）
public async Task<List<Customer>> GetCustomersForDisplayAsync()
{
    return await _context.Customers
        .AsNoTracking()
        .Where(c => c.Status == EntityStatus.Active)
        .Include(c => c.CustomerType)
        .ToListAsync();
}

// 分頁查詢
public async Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
{
    var query = _context.Customers
        .Where(c => c.Status != EntityStatus.Deleted);
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Include(c => c.CustomerType)
        .OrderBy(c => c.CompanyName)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return (items, totalCount);
}
```

---

## 依賴注入註冊

### Program.cs 設定
```csharp
// 資料庫註冊
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 服務註冊
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAddressService, AddressService>();
```

---

## 開發檢查清單

### 建立新服務時
- [ ] 建立服務介面和實作類別
- [ ] 使用正確的命名空間 `ERPCore2.Services`
- [ ] 注入 DbContext 和 ILogger
- [ ] 實作基本 CRUD 方法
- [ ] 使用 ServiceResult 封裝回傳結果
- [ ] 包含適當的業務驗證
- [ ] 實作軟刪除邏輯
- [ ] 加入錯誤處理和日誌記錄
- [ ] 在 Program.cs 中註冊服務

### 方法設計檢查
- [ ] 所有異步方法使用 async/await
- [ ] 適當使用 Include() 載入關聯資料
- [ ] 實作業務規則驗證
- [ ] 設定稽核欄位（CreatedDate、ModifiedBy 等）
- [ ] 使用軟刪除而非硬刪除
- [ ] 複雜操作使用交易管理
- [ ] 包含適當的例外處理

### 效能考量
- [ ] 只讀查詢使用 AsNoTracking()
- [ ] 大量資料查詢實作分頁
- [ ] 避免 N+1 查詢問題
- [ ] 適當使用 Select 投影減少資料傳輸

---

## 常見模式摘要

- **錯誤處理**：統一使用 ServiceResult 模式
- **驗證**：結合 DataAnnotations 和業務驗證
- **軟刪除**：使用 EntityStatus.Deleted 標記
- **稽核欄位**：自動設定 CreatedDate、ModifiedDate 等
- **關聯載入**：適當使用 Include() 避免延遲載入問題
- **交易管理**：複雜操作使用 EF Core 交易
- **日誌記錄**：記錄重要操作和錯誤資訊

---

*本指南提供 Service 層的標準設計模式和最佳實踐。通過遵循這些原則，可以建構出一致、可維護且高效能的業務邏輯層。*