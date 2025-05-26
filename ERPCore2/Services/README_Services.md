# Services 資料夾說明文檔

## 概述
Services 資料夾包含了 ERPCore2 系統的業務邏輯層實作，負責處理業務規則、資料驗證、商業邏輯封裝，並直接使用 EF Core DbContext 進行資料操作。

**重要：本系統已簡化架構，移除 Repository 層，Service 層直接使用 EF Core，並且不使用 DTO，直接操作 Entity 模型。**

## 資料夾結構

```
Services/
├── Customers/           # 客戶相關服務
│   ├── CustomerService.cs
│   └── Interfaces/
│       └── ICustomerService.cs
├── Results/             # 服務層回傳結果類別
│   └── ServiceResult.cs
└── README_Services.md   # 本說明文檔
```


## 命名規範

### 命名空間命名方式
所有 Services 資料夾下的檔案均使用統一的命名空間：

- ✅ **統一命名空間**：`ERPCore2.Services`

### 實際命名範例
- `Services/Customers/CustomerService.cs` → `namespace ERPCore2.Services.Customers`
- `Services/Results/ServiceResult.cs` → `namespace ERPCore2.Services`
- `Services/Customers/Interfaces/ICustomerService.cs` → `namespace ERPCore2.Services.Customers.Interfaces`

## 簡化架構設計原則

### 🏗️ 為什麼移除 Repository 層？
- **EF Core 本身就是 Repository 模式**：DbContext 和 DbSet 已經提供了完整的資料存取抽象
- **避免過度設計**：額外的 Repository 層只是增加了不必要的複雜性
- **更直接的資料操作**：可以直接使用 LINQ、Include() 等 EF Core 功能
- **更好的效能**：減少資料轉換和抽象層的開銷

### 🎯 為什麼不使用 DTO？
- **減少程式碼複雜度**：避免建立大量的 DTO 類別和對應的轉換邏輯
- **Entity 設計良好**：我們的 Entity 模型已經包含了適當的驗證屬性和結構
- **避免資料轉換開銷**：直接操作 Entity 避免了 Entity ↔ DTO 的轉換成本
- **更簡潔的 API**：Service 方法直接接受和回傳 Entity，使用更直觀
- **利用 EF Core 變更追蹤**：直接操作 Entity 可以充分利用 EF Core 的變更追蹤功能

### 🧠 Service 層新職責（直接使用 EF Core + Entity）
Service 層現在負責**完整的業務邏輯和資料存取**，主要包括：

- **直接的 EF Core 資料操作**：使用 DbContext 進行 CRUD 操作
- **業務規則驗證**：檢查業務邏輯限制和資料完整性
- **Entity 驗證**：使用 DataAnnotations 和自定義驗證邏輯
- **實體狀態管理**：設定稽核欄位（CreatedDate、ModifiedBy 等）
- **關聯資料載入**：使用 Include() 載入相關實體
- **錯誤處理**：使用 ServiceResult 包裝業務操作結果
- **事務管理**：處理跨多個實體的操作

```csharp
// ✅ Service 直接使用 EF Core 和 Entity
public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
{
    try
    {
        // 1. 業務驗證（直接驗證 Entity）
        var validationResult = ValidateCustomer(customer);
        if (!validationResult.IsSuccess)
            return ServiceResult<Customer>.Failure(validationResult.ErrorMessage);
        
        // 2. 業務規則檢查（直接使用 EF Core）
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerCode == customer.CustomerCode && c.Status != EntityStatus.Deleted);
        if (existingCustomer != null)
            return ServiceResult<Customer>.Failure("客戶代碼已存在");
        
        // 3. 設定業務相關欄位
        customer.CreatedDate = DateTime.Now;
        customer.CreatedBy = "系統管理員";
        customer.Status = EntityStatus.Active;
        
        // 4. 直接使用 EF Core 儲存
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        return ServiceResult<Customer>.Success(customer);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating customer");
        return ServiceResult<Customer>.Failure($"新增客戶時發生錯誤: {ex.Message}");
    }
}
```

## 實際使用場景

### 📊 UI 層直接使用 Service（推薦方式）
```csharp
@inject ICustomerService CustomerService

@code {
    private List<Customer> customers = new();
    private Customer newCustomer = new();
    
    protected override async Task OnInitializedAsync()
    {
        // ✅ 使用 Service 獲取資料（包含業務邏輯）
        customers = await CustomerService.GetAllAsync();
    }
    
    private async Task CreateCustomer()
    {
        // ✅ 使用 Service 進行業務操作（直接傳遞 Entity）
        var result = await CustomerService.CreateAsync(newCustomer);
        
        if (result.IsSuccess)
        {
            // 成功處理 - result.Data 就是 Customer Entity
            customers.Add(result.Data);
            newCustomer = new Customer(); // 重置表單
        }
        else
        {
            // 顯示業務錯誤訊息
            ShowError(result.ErrorMessage);
        }
    }
}
```

### 🔧 Service 方法範例（Entity-First 設計）
```csharp
public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    
    // ✅ 直接回傳 Entity List
    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Where(c => c.Status != EntityStatus.Deleted)
            .Include(c => c.CustomerType)    // 載入關聯資料
            .Include(c => c.Industry)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }
    
    // ✅ 直接接受 Entity 參數
    public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
    {
        // 業務邏輯處理...
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return ServiceResult<Customer>.Success(customer);
    }
    
    // ✅ 直接接受 Entity 參數進行更新
    public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
    {
        // 業務邏輯處理...
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return ServiceResult<Customer>.Success(customer);
    }
}
```

## Service 設計原則（Entity-First 方法）

### 1. 直接使用 Entity，不使用 DTO
所有 Service 方法都直接操作 Entity 模型：

```csharp
// ✅ 正確：直接接受和回傳 Entity
public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
public async Task<List<Customer>> GetAllAsync()
public async Task<Customer?> GetByIdAsync(int id)

// ❌ 避免：不需要建立 DTO
// public async Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto)
// public async Task<List<CustomerDto>> GetAllAsync()
```

### 2. 利用 Entity 的 DataAnnotations
直接使用 Entity 上的驗證屬性：

```csharp
public class Customer
{
    [Required(ErrorMessage = "客戶代碼為必填")]
    [MaxLength(20, ErrorMessage = "客戶代碼不可超過20個字元")]
    public string CustomerCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "公司名稱為必填")]
    [MaxLength(100, ErrorMessage = "公司名稱不可超過100個字元")]
    public string CompanyName { get; set; } = string.Empty;
}

// Service 中的驗證邏輯
private ServiceResult ValidateCustomer(Customer customer)
{
    var context = new ValidationContext(customer);
    var results = new List<ValidationResult>();
    
    if (!Validator.TryValidateObject(customer, context, results, true))
    {
        var errors = results.Select(r => r.ErrorMessage ?? "驗證錯誤").ToList();
        return ServiceResult.ValidationFailure(errors);
    }
    
    return ServiceResult.Success();
}
```

### 3. 依賴注入（直接使用 DbContext）
Service 直接注入 DbContext，不需要 Repository：

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
    
    // 直接使用 EF Core 功能
    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Where(c => c.Status != EntityStatus.Deleted)
            .Include(c => c.CustomerType)      // 載入關聯資料
            .Include(c => c.Industry)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }
}
```

### 4. 充分利用 EF Core 功能
直接使用 EF Core 的強大功能：

```csharp
// ✅ 使用 Include 載入關聯資料
public async Task<Customer?> GetByIdAsync(int id)
{
    return await _context.Customers
        .Include(c => c.CustomerType)
        .Include(c => c.Industry)
        .Include(c => c.CustomerContacts)
        .Include(c => c.CustomerAddresses)
        .FirstOrDefaultAsync(c => c.CustomerId == id && c.Status != EntityStatus.Deleted);
}

// ✅ 使用複雜的 LINQ 查詢
public async Task<List<Customer>> SearchAsync(string searchTerm, EntityStatus? status = null)
{
    var query = _context.Customers.AsQueryable();
    
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        query = query.Where(c => 
            c.CustomerCode.Contains(searchTerm) || 
            c.CompanyName.Contains(searchTerm) ||
            c.ContactPerson.Contains(searchTerm));
    }
    
    if (status.HasValue)
        query = query.Where(c => c.Status == status.Value);
    else
        query = query.Where(c => c.Status != EntityStatus.Deleted);
    
    return await query
        .Include(c => c.CustomerType)
        .OrderBy(c => c.CustomerCode)
        .ToListAsync();
}
```

## 檔案按資料夾分類

### 當前結構
```
Services/
├── Customers/                    # 客戶管理服務
│   ├── CustomerService.cs       # 客戶服務實作
│   └── Interfaces/
│       └── ICustomerService.cs  # 客戶服務介面
├── Results/                      # 通用結果類別
│   └── ServiceResult.cs         # 服務操作結果封裝
└── README_Services.md            # 本說明文檔
```

### 未來擴展建議
隨著系統成長，建議按業務模組分類：

```
Services/
├── Customers/          # 客戶管理服務
│   ├── CustomerService.cs
│   └── Interfaces/
├── Products/           # 產品管理服務（未來）
│   ├── ProductService.cs
│   └── Interfaces/
├── Orders/             # 訂單管理服務（未來）
│   ├── OrderService.cs
│   └── Interfaces/
├── Inventory/          # 庫存管理服務（未來）
├── Accounting/         # 會計管理服務（未來）
├── Results/            # 通用結果類別
└── Common/             # 共用服務（未來）
```


## 最佳實踐（Entity-First 架構）

1. **直接使用 Entity**：不建立 DTO，直接操作業務實體模型
2. **EF Core 優先**：充分利用 EF Core 的功能，如 Include、LINQ、變更追蹤等
3. **業務邏輯集中**：將所有業務規則、驗證邏輯集中在 Service 層
4. **錯誤處理統一**：使用 ServiceResult 處理所有業務操作結果
5. **介面設計**：為每個 Service 設計清楚的介面，便於測試和替換
6. **事務管理**：在 Service 層使用 DbContext 的事務功能處理複雜操作
7. **效能考量**：適當使用 AsNoTracking()、分頁查詢等最佳化技術

## 範例：完整的 Entity-First Service 方法

```csharp
public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
{
    try
    {
        // 1. 檢查實體是否存在
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && c.Status != EntityStatus.Deleted);
        if (existingCustomer == null)
            return ServiceResult<Customer>.Failure("客戶不存在");
        
        // 2. Entity 驗證（使用 DataAnnotations）
        var validationResult = ValidateCustomer(customer);
        if (!validationResult.IsSuccess)
            return ServiceResult<Customer>.Failure(validationResult.ErrorMessage);
        
        // 3. 業務規則檢查（避免重複代碼）
        var duplicateCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerCode == customer.CustomerCode && 
                                     c.CustomerId != customer.CustomerId && 
                                     c.Status != EntityStatus.Deleted);
        if (duplicateCustomer != null)
            return ServiceResult<Customer>.Failure("客戶代碼已存在");
        
        // 4. 設定稽核欄位
        customer.ModifiedDate = DateTime.Now;
        customer.ModifiedBy = "系統管理員"; // 可從認證系統獲取
        
        // 5. 直接使用 EF Core 更新
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        
        return ServiceResult<Customer>.Success(customer);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating customer with ID {CustomerId}", customer.CustomerId);
        return ServiceResult<Customer>.Failure($"更新客戶時發生錯誤: {ex.Message}");
    }
}
```

這樣的設計確保了：
- **簡潔性**：沒有不必要的抽象層和資料轉換
- **效能**：直接使用 EF Core，充分利用其最佳化功能
- **可維護性**：業務邏輯集中，程式碼結構清楚
- **可測試性**：透過介面注入，便於單元測試
- **擴展性**：可以輕鬆加入新的業務邏輯和驗證規則
