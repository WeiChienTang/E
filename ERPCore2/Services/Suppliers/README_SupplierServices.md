# Suppliers Services 功能說明

## 🏗️ 架構概述

所有廠商服務都繼承自 `GenericManagementService<T>`，提供統一的基礎 CRUD 操作，並根據業務需求擴展特定功能。

### 服務繼承結構
```
GenericManagementService<T>
├── SupplierService
├── SupplierTypeService
├── SupplierContactService
└── SupplierAddressService
```

## 📁 檔案結構
```
Services/
└── Suppliers/
    ├── Interfaces/
    │   ├── ISupplierService.cs
    │   ├── ISupplierTypeService.cs
    │   ├── ISupplierContactService.cs
    │   └── ISupplierAddressService.cs
    ├── SupplierService.cs
    ├── SupplierTypeService.cs
    ├── SupplierContactService.cs
    └── SupplierAddressService.cs
```

## 🔧 核心服務功能

### 1. SupplierService - 廠商主要服務
**繼承**：`GenericManagementService<Supplier>` → `ISupplierService`

**主要功能**：
- ✅ 基本 CRUD 操作（繼承自基底類別）
- ✅ 廠商代碼唯一性驗證
- ✅ 統一編號格式驗證
- ✅ 信用額度管理
- ✅ 廠商狀態管理
- ✅ 關聯資料查詢（廠商類型、行業類型）
- ✅ 聯絡資料整合管理
- ✅ 地址資料整合管理

**特殊查詢方法**：
- `GetBySupplierCodeAsync()` - 根據廠商代碼查詢
- `GetBySupplierTypeAsync()` - 根據廠商類型查詢
- `GetByIndustryTypeAsync()` - 根據行業類型查詢

### 2. SupplierTypeService - 廠商類型服務
**繼承**：`GenericManagementService<SupplierType>` → `ISupplierTypeService`

**主要功能**：
- ✅ 廠商類型名稱唯一性驗證
- ✅ 刪除前檢查（是否有廠商使用此類型）
- ✅ 類型名稱查詢

### 3. SupplierContactService - 廠商聯絡方式服務
**繼承**：`GenericManagementService<SupplierContact>` → `ISupplierContactService`

**主要功能**：
- ✅ 廠商聯絡方式管理
- ✅ 主要聯絡方式設定
- ✅ 聯絡方式複製功能
- ✅ 預設聯絡方式初始化
- ✅ 記憶體操作方法（用於UI編輯）

**UI 支援功能**：
- `CreateNewContact()` - 建立新聯絡方式
- `InitializeDefaultContacts()` - 初始化預設聯絡方式
- `GetCompletedContactCount()` - 取得完成聯絡方式數量

### 4. SupplierAddressService - 廠商地址服務
**繼承**：`GenericManagementService<SupplierAddress>` → `ISupplierAddressService`

**主要功能**：
- ✅ 廠商地址管理
- ✅ 主要地址設定
- ✅ 地址複製功能
- ✅ 預設地址初始化
- ✅ 記憶體操作方法（用於UI編輯）

**UI 支援功能**：
- `CreateNewAddress()` - 建立新地址
- `InitializeDefaultAddresses()` - 初始化預設地址
- `GetCompletedAddressCount()` - 取得完成地址數量

## 🎯 設計模式與最佳實踐

### 通用服務模式
```csharp
// 所有服務都遵循相同的模式
public class SupplierService : GenericManagementService<Supplier>, ISupplierService
{
    // 1. 覆寫基底方法（如需要）
    public override async Task<List<Supplier>> GetAllAsync() { }
    
    // 2. 實作業務特定方法
    public async Task<bool> IsSupplierCodeExistsAsync(string code) { }
    
    // 3. 輔助方法
    public void InitializeNewSupplier(Supplier supplier) { }
}
```

### 依賴注入模式
所有服務都已註冊在 `ServiceRegistration.cs` 中：
```csharp
services.AddScoped<ISupplierService, SupplierService>();
services.AddScoped<ISupplierContactService, SupplierContactService>();
services.AddScoped<ISupplierAddressService, SupplierAddressService>();
services.AddScoped<ISupplierTypeService, SupplierTypeService>();
```

### 錯誤處理模式
```csharp
// 統一使用 ServiceResult 封裝結果
public async Task<ServiceResult> UpdateSupplierAsync(Supplier supplier)
{
    try
    {
        // 業務邏輯
        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "錯誤訊息");
        return ServiceResult.Failure($"操作失敗: {ex.Message}");
    }
}
```

## 🔄 與其他服務的關聯

### 共用服務依賴
- **ContactTypeService** - 聯絡類型管理
- **AddressTypeService** - 地址類型管理  
- **IndustryTypeService** - 行業類型管理

### 資料流向
```
SupplierService (主服務)
├── SupplierContactService (聯絡資料)
├── SupplierAddressService (地址資料)
├── SupplierTypeService (廠商類型)
└── IndustryTypeService (行業類型)
```

## 📊 重要特性

### 🔒 資料驗證
- **必填欄位檢查**：廠商代碼、公司名稱
- **唯一性檢查**：廠商代碼、廠商類型名稱
- **格式驗證**：統一編號、信用額度
- **關聯性檢查**：主要聯絡方式、主要地址

### 🏃‍♂️ 效能優化
- **Include() 關聯載入**：避免 N+1 查詢問題
- **AsNoTracking()**：只讀查詢效能優化
- **批次操作**：支援批次建立、更新、刪除

### 🛡️ 軟刪除機制
- 所有刪除操作都是軟刪除（設定 `IsDeleted = true`）
- 查詢時自動過濾已刪除資料
- 支援資料復原機制

### 📋 稽核欄位
- **CreatedAt/UpdatedAt**：自動設定建立/更新時間
- **CreatedBy/UpdatedBy**：自動設定建立/更新者
- **Status**：實體狀態管理

## 🎮 使用範例

### 基本使用
```csharp
// 注入服務
private readonly ISupplierService _supplierService;

// 取得所有廠商（包含關聯資料）
var suppliers = await _supplierService.GetAllAsync();

// 根據廠商代碼查詢
var supplier = await _supplierService.GetBySupplierCodeAsync("SUP001");

// 建立新廠商
var newSupplier = new Supplier();
_supplierService.InitializeNewSupplier(newSupplier);
var result = await _supplierService.CreateAsync(newSupplier);
```

### 聯絡資料管理
```csharp
// 取得廠商聯絡資料
var contacts = await _supplierContactService.GetBySupplierIdAsync(supplierId);

// 設定主要聯絡方式
await _supplierContactService.SetPrimaryContactAsync(contactId);

// 批次更新聯絡資料
await _supplierContactService.UpdateSupplierContactsAsync(supplierId, contacts);
```

---

*此架構遵循 ERPCore2 系統的統一設計原則，確保代碼一致性和可維護性。*
