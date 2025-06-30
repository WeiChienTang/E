GenericManagementService<T>：提供標準 CRUD 操作，所有服務都應繼承
專用服務介面：每個業務領域建立專屬的 `I[業務領域]Service` 介面
統一錯誤處理：使用 ServiceResult 封裝操作結果
命名空間：統一使用 `ERPCore2.Services`
檔案位置：`Service/[類型名稱]/` 底下放 `I[功能]Service` 和 `[功能]Service`
實體屬性：使用 Entity 中定義的屬性名稱，外鍵格式為 `[表名稱]Id`

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
2. 實作服務類別
```csharp
// Services/Customers/CustomerService.cs
namespace ERPCore2.Services;

public class CustomerService : GenericManagementService<Customer>, ICustomerService
{
    public CustomerService(AppDbContext context) : base(context) { }
    
    // 覆寫基底方法（載入關聯資料）
    public override async Task<List<Customer>> GetAllAsync()
    {
        // 功能
    }
    
    // 實作業務特定方法
    public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
    {
        // 功能
    }
    
    // 覆寫驗證方法
    public override async Task<ServiceResult> ValidateAsync(Customer entity)
    {
        // 功能
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