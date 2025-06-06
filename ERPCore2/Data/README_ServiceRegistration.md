# ServiceRegistration.cs 說明文檔

## 概述
`ServiceRegistration.cs` 是一個靜態類別，專門用於管理整個應用程式的依賴注入服務註冊。這個檔案的目的是：

1. **保持 Program.cs 整潔**：將所有服務註冊邏輯從 Program.cs 移出
2. **集中管理服務註冊**：統一管理所有依賴注入註冊
3. **按領域分組**：按業務領域組織服務註冊
4. **易於維護**：新增服務時有明確的位置添加

## 核心方法

### `AddApplicationServices()`
主要的服務註冊方法，會註冊所有應用程式需要的服務：
- 資料庫服務
- 業務邏輯服務

### `AddDatabaseServices()`
註冊資料庫相關服務：
- DbContextFactory（解決並發問題）

### `AddServices()`
註冊所有業務邏輯服務，在方法內部使用註解分類：

#### 客戶相關服務
- `ICustomerService` / `CustomerService`
- `ICustomerContactService` / `CustomerContactService`
- `ICustomerAddressService` / `CustomerAddressService`
- `ICustomerTypeService` / `CustomerTypeService`

#### 共用資料服務
- `IContactTypeService` / `ContactTypeService`
- `IAddressTypeService` / `AddressTypeService`

#### 行業類型服務
- `IIndustryTypeService` / `IndustryTypeService`

## 使用方式

### 在 Program.cs 中使用
```csharp
// 註冊所有應用程式服務
builder.Services.AddApplicationServices(connectionString);
```

### 只註冊特定領域服務
```csharp
// 只註冊資料庫服務
builder.Services.AddDatabaseServices(connectionString);

// 只註冊業務服務
builder.Services.AddServices();
```

## 新增服務的步驟

1. **確定服務類型**：客戶相關、共用資料或行業類型
2. **在 `AddServices()` 方法中添加**：找到對應的註解分類區域
3. **添加服務註冊**：使用適當的生命週期（通常是 `AddScoped`）
4. **保持分類整潔**：按照現有的註解分組模式

## 服務生命週期指南

- **AddScoped**：用於業務邏輯服務（推薦）
- **AddSingleton**：用於配置或快取服務
- **AddTransient**：用於輕量級、無狀態的服務

## 範例：添加新服務

```csharp
public static void AddServices(this IServiceCollection services)
{
    // 客戶相關服務
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<ICustomerContactService, CustomerContactService>();
    services.AddScoped<ICustomerAddressService, CustomerAddressService>();
    services.AddScoped<ICustomerTypeService, CustomerTypeService>();
    // 新增的客戶服務
    services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();

    // 共用資料服務
    services.AddScoped<IContactTypeService, ContactTypeService>();
    services.AddScoped<IAddressTypeService, AddressTypeService>();
    // 新增的共用服務
    services.AddScoped<ICurrencyTypeService, CurrencyTypeService>();

    // 行業類型服務
    services.AddScoped<IIndustryTypeService, IndustryTypeService>();
}
```

## 優點

1. **可維護性**：所有註冊集中在 `AddServices()` 方法中
2. **可讀性**：使用註解分類，容易理解服務分組
3. **簡潔性**：不需要多個方法，統一在一個地方管理
4. **擴展性**：新增服務時只需在對應分類下添加
5. **測試友好**：可以獨立註冊特定服務進行測試
6. **Program.cs 簡潔**：主程式檔案專注於應用程式配置

## 注意事項

- 保持服務註冊的一致性
- 遵循命名慣例：I[服務名]Service, [服務名]Service
- 確保服務介面和實作類別都已正確引用
- 新增服務時記得放在對應的註解分類區域下
- 維持註解分類的清晰度和一致性