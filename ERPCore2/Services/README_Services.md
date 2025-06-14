# Services 資料夾說明文檔

## 概述
Services 資料夾包含了 ERPCore2 系統的業務邏輯層實作，負責處理業務規則、資料驗證、商業邏輯封裝，並直接使用 EF Core DbContext 進行資料操作。

**核心設計理念：使用通用服務基底類別 `GenericManagementService<T>`，避免重複的 CRUD 操作代碼，提供統一的服務模式。**

## 命名規範

### 命名空間
所有 Services 資料夾下的檔案統一使用：`ERPCore2.Services`

### 檔案組織結構
- **檔案建置位置**：Service/[類型名稱]/ 底下放 I[功能]Service 和 [功能]Service
- **服務實作**：`[業務領域]Service.cs`（如：CustomerService.cs）
- **服務介面**：`I[業務領域]Service.cs`（如：ICustomerService.cs）
- **通用服務基底**：`GenericManagementService<T>.cs`
- **結果類別**：`ServiceResult.cs`

### 資料夾結構範例
```
Services/
├── Customers/
│   ├── ICustomerService.cs
│   ├── CustomerService.cs
│   ├── ICustomerTypeService.cs
│   ├── CustomerTypeService.cs
│   ├── ICustomerAddressService.cs
│   ├── CustomerAddressService.cs
│   └── Readme_CustomerService.md
├── Products/
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IProductCategoryService.cs
│   ├── ProductCategoryService.cs
│   └── README_ProductServices.md
└── Industries/
    ├── IIndustryTypeService.cs
    ├── IndustryTypeService.cs
    └── Readme_IndustryType.md
```

---

## 架構設計原則

### 通用服務模式
- **GenericManagementService<T>**：提供標準 CRUD 操作，所有服務都應繼承此基底類別
- **專用服務介面**：每個業務領域建立專屬的 I[業務領域]Service 介面
- **業務特定功能**：將專門的業務邏輯實作在子類別中
- **統一錯誤處理**：使用 ServiceResult 封裝操作結果

### 實體屬性命名規範
- **Entity 屬性名稱**：統一採用 Entity 中定義的屬性名稱，例如 `Id`
- **外鍵屬性命名**：外鍵屬性使用全名格式 `[表名稱]Id`，例如 `ContactTypeId`、`CustomerTypeId`
- **避免映射屬性**：不使用 `[NotMapped]` 或其他映射技術，直接使用實體中定義的屬性
- **一致性原則**：確保資料庫、實體模型、服務層和 UI 層使用相同的屬性名稱

### 為什麼使用通用服務？
- **避免重複代碼**：基本 CRUD 操作在 `GenericManagementService<T>` 中統一實作
- **一致性操作**：所有實體都有相同的基本操作模式
- **更容易維護**：修改基本功能時只需更新基底類別
- **標準化開發**：新增服務時遵循相同的開發模式

### Service 層職責
- **繼承通用功能**：從 `GenericManagementService<T>` 繼承基本 CRUD 操作
- **業務特定邏輯**：實作專門的業務規則和驗證
- **關聯資料載入**：使用 Include() 載入相關實體
- **錯誤處理**：使用 ServiceResult 封裝結果
- **稽核欄位管理**：自動設定 CreatedAt、UpdatedAt、CreatedBy、UpdatedBy
- **軟刪除管理**：使用 IsDeleted 標記進行軟刪除

---

## Service 開發模式

### 1. 建立專用服務介面（在適當的類型資料夾下）
```csharp
// 檔案位置：Services/Customers/ICustomerService.cs
namespace ERPCore2.Services;

public interface ICustomerService : IGenericManagementService<Customer>
{
    // 業務特定方法
    Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null);
    Task<List<Customer>> GetByIndustryTypeAsync(int industryTypeId);
    Task<ServiceResult> UpdateCustomerStatusAsync(int customerId, EntityStatus status);
}
```

### 2. 實作服務類別（在同一資料夾下）
```csharp
// 檔案位置：Services/Customers/CustomerService.cs
namespace ERPCore2.Services;

public class CustomerService : GenericManagementService<Customer>, ICustomerService
{
    public CustomerService(AppDbContext context) : base(context)
    {
    }
    
    // 覆寫基底方法（如需要）
    public override async Task<List<Customer>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.CustomerType)
            .Include(c => c.IndustryType)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }
    
    // 實作業務特定方法
    public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.CustomerCode == customerCode && !c.IsDeleted);
        
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }
    
    // 覆寫驗證方法
    public override async Task<ServiceResult> ValidateAsync(Customer entity)
    {
        // 先執行基底驗證
        var baseResult = await base.ValidateAsync(entity);
        if (!baseResult.IsSuccess)
            return baseResult;
            
        // 額外的業務驗證
        if (await IsCustomerCodeExistsAsync(entity.CustomerCode, entity.Id))
            return ServiceResult.Failure("客戶代碼已存在");
            
        return ServiceResult.Success();
    }
}
```

---

## GenericManagementService 提供的功能

### 基本 CRUD 操作
- `GetAllAsync()` - 取得所有資料（不含已刪除）
- `GetActiveAsync()` - 取得所有啟用的資料
- `GetByIdAsync(int id)` - 根據 ID 取得單一資料
- `CreateAsync(T entity)` - 建立新資料
- `UpdateAsync(T entity)` - 更新資料
- `DeleteAsync(int id)` - 軟刪除資料

### 批次操作
- `CreateBatchAsync(List<T> entities)` - 批次建立
- `UpdateBatchAsync(List<T> entities)` - 批次更新
- `DeleteBatchAsync(List<int> ids)` - 批次刪除

### 查詢操作
- `GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)` - 分頁查詢
- `SearchAsync(string searchTerm)` - 條件查詢（需子類別實作）
- `ExistsAsync(int id)` - 檢查資料是否存在
- `GetCountAsync()` - 取得資料總數

### 狀態管理
- `SetStatusAsync(int id, EntityStatus status)` - 設定特定狀態
- `ToggleStatusAsync(int id)` - 切換狀態
- `SetStatusBatchAsync(List<int> ids, EntityStatus status)` - 批次設定狀態

### 驗證功能
- `ValidateAsync(T entity)` - 驗證實體資料（可覆寫）
- `IsNameExistsAsync(string name, int? excludeId)` - 檢查名稱是否存在

---

## 覆寫基底方法的時機

### 何時需要覆寫？
1. **GetAllAsync()** - 需要載入關聯資料或特殊排序
2. **SearchAsync()** - 實作特定的搜尋邏輯
3. **ValidateAsync()** - 添加額外的業務驗證規則
4. **IsNameExistsAsync()** - 實體沒有標準名稱欄位時

### 覆寫範例
```csharp
public override async Task<List<Customer>> GetAllAsync()
{
    return await _dbSet
        .Include(c => c.CustomerType)
        .Include(c => c.IndustryType)
        .Where(c => !c.IsDeleted)
        .OrderBy(c => c.CompanyName)
        .ToListAsync();
}

public override async Task<List<Customer>> SearchAsync(string searchTerm)
{
    if (string.IsNullOrWhiteSpace(searchTerm))
        return await GetAllAsync();
        
    return await _dbSet
        .Include(c => c.CustomerType)
        .Where(c => !c.IsDeleted && 
                   (c.CompanyName.Contains(searchTerm) || 
                    c.CustomerCode.Contains(searchTerm)))
        .ToListAsync();
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
builder.Services.AddScoped<IAddressTypeService, AddressTypeService>();
builder.Services.AddScoped<IContactTypeService, ContactTypeService>();
```

---

## 開發檢查清單

### 建立新服務時
- [ ] 在適當的類型資料夾下建立專用服務介面，繼承 `IGenericManagementService<T>`
- [ ] 在同一資料夾下建立服務實作類別，繼承 `GenericManagementService<T>`
- [ ] 使用正確的命名空間 `ERPCore2.Services`
- [ ] 確保介面和實作檔案放在 Service/[類型名稱]/ 資料夾內
- [ ] 實作業務特定方法（如果需要）
- [ ] 覆寫基底方法（如果需要特殊邏輯）
- [ ] 在 Program.cs 中註冊服務

### 覆寫方法檢查
- [ ] 需要關聯資料時覆寫 `GetAllAsync()`
- [ ] 實作特定搜尋邏輯時覆寫 `SearchAsync()`
- [ ] 有額外驗證需求時覆寫 `ValidateAsync()`
- [ ] 實體無標準名稱欄位時覆寫 `IsNameExistsAsync()`

### 效能考量
- [ ] 只讀查詢使用 AsNoTracking()
- [ ] 適當使用 Include() 載入關聯資料
- [ ] 避免 N+1 查詢問題

---

## 常見模式摘要

- **檔案組織**：Interface 和 Service 檔案統一放在 Service/[類型名稱]/ 資料夾內
- **繼承模式**：所有服務繼承 `GenericManagementService<T>`
- **介面設計**：專用介面繼承 `IGenericManagementService<T>`
- **命名空間**：所有服務統一使用 `ERPCore2.Services` 命名空間
- **錯誤處理**：統一使用 ServiceResult 模式
- **軟刪除**：使用 IsDeleted 標記
- **稽核欄位**：自動設定 CreatedAt、UpdatedAt、CreatedBy、UpdatedBy
- **狀態管理**：使用 EntityStatus 枚舉

---

*本指南提供基於 GenericManagementService 的 Service 層開發模式。通過繼承通用服務基底類別，可以大幅減少重複代碼，提高開發效率和代碼一致性。*