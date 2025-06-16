# Employee Services 說明文檔

## 概述
Employee Services 資料夾包含了員工管理相關的業務邏輯層實作，負責處理員工、員工聯絡資料和員工地址的 CRUD 操作及業務邏輯。

## 服務結構

### 核心服務
- **IEmployeeService / EmployeeService** - 員工主檔管理
- **IEmployeeContactService / EmployeeContactService** - 員工聯絡資料管理
- **IEmployeeAddressService / EmployeeAddressService** - 員工地址管理

### 認證相關服務（現有）
- **IAuthenticationService / AuthenticationService** - 認證服務
- **IRoleService / RoleService** - 角色管理
- **IPermissionService / PermissionService** - 權限管理

---

## EmployeeService

### 主要功能
- 員工基本資料 CRUD 操作
- 使用者認證相關功能
- 員工代碼自動產生
- 角色管理整合

### 重要變更
**Email 欄位已移除**：原本的 Email 欄位已從 Employee 實體中移除，改為使用 EmployeeContact 來管理員工的聯絡資訊（包括電子郵件）。

### 核心方法
```csharp
// 基本 CRUD（繼承自 GenericManagementService）
Task<List<Employee>> GetAllAsync()
Task<Employee?> GetByIdAsync(int id)
Task<ServiceResult> CreateAsync(Employee entity)
Task<ServiceResult> UpdateAsync(Employee entity)
Task<ServiceResult> DeleteAsync(int id)

// 員工特定功能
Task<ServiceResult<Employee>> GetByUsernameAsync(string username)
Task<ServiceResult<Employee>> GetByEmployeeCodeAsync(string employeeCode)
Task<ServiceResult<bool>> IsUsernameExistsAsync(string username, int? excludeEmployeeId = null)
Task<ServiceResult<bool>> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null)
Task<ServiceResult<List<Employee>>> SearchEmployeesAsync(string searchTerm)
Task<ServiceResult<List<Employee>>> GetEmployeesByRoleAsync(int roleId)
Task<ServiceResult<List<Employee>>> GetEmployeesByDepartmentAsync(string department)
Task<ServiceResult<string>> GenerateNextEmployeeCodeAsync(string prefix = "EMP")
```

### 導航屬性載入
現在的 GetAllAsync() 和 GetByIdAsync() 方法會自動載入：
- Role（角色）
- EmployeeContacts（員工聯絡資料）
- EmployeeAddresses（員工地址）

---

## EmployeeContactService

### 主要功能
- 員工聯絡資料管理（電話、傳真、電子郵件等）
- 主要聯絡方式設定
- 聯絡資料驗證
- 重複檢查

### 核心方法
```csharp
// 基本 CRUD（繼承自 GenericManagementService）
Task<List<EmployeeContact>> GetAllAsync()
Task<EmployeeContact?> GetByIdAsync(int id)
Task<ServiceResult> CreateAsync(EmployeeContact entity)
Task<ServiceResult> UpdateAsync(EmployeeContact entity)
Task<ServiceResult> DeleteAsync(int id)

// 員工聯絡資料特定功能
Task<ServiceResult<List<EmployeeContact>>> GetByEmployeeIdAsync(int employeeId)
Task<ServiceResult<List<EmployeeContact>>> GetByContactTypeAsync(int contactTypeId)
Task<ServiceResult> SetAsPrimaryAsync(int employeeContactId)

// 輔助方法
string GetContactValue(int employeeId, string contactTypeName, 
    List<ContactType> contactTypes, List<EmployeeContact> employeeContacts)
ServiceResult UpdateContactValue(int employeeId, string contactTypeName, string value,
    List<ContactType> contactTypes, List<EmployeeContact> employeeContacts)
int GetContactCompletedFieldsCount(List<EmployeeContact> employeeContacts)
ServiceResult ValidateEmployeeContacts(List<EmployeeContact> employeeContacts)
ServiceResult EnsureUniquePrimaryContacts(List<EmployeeContact> employeeContacts)
```

### 驗證規則
- 員工ID 必須有效
- 聯絡內容不能為空
- 同一員工、同一聯絡類型不能有重複的聯絡內容
- 每種聯絡類型只能有一個主要聯絡方式

---

## EmployeeAddressService

### 主要功能
- 員工地址資料管理
- 主要地址設定
- 地址格式化
- 地址統計分析

### 核心方法
```csharp
// 基本 CRUD（繼承自 GenericManagementService）
Task<List<EmployeeAddress>> GetAllAsync()
Task<EmployeeAddress?> GetByIdAsync(int id)
Task<ServiceResult> CreateAsync(EmployeeAddress entity)
Task<ServiceResult> UpdateAsync(EmployeeAddress entity)
Task<ServiceResult> DeleteAsync(int id)

// 員工地址特定功能
Task<List<EmployeeAddress>> GetByEmployeeIdAsync(int employeeId)
Task<EmployeeAddress?> GetPrimaryAddressAsync(int employeeId)
Task<List<EmployeeAddress>> GetByAddressTypeAsync(int addressTypeId)
Task<ServiceResult> SetPrimaryAddressAsync(int addressId)
Task<ServiceResult<EmployeeAddress>> CopyAddressToEmployeeAsync(EmployeeAddress sourceAddress, int targetEmployeeId, int? targetAddressTypeId = null)
Task<ServiceResult> EnsureEmployeeHasPrimaryAddressAsync(int employeeId)

// 統計功能
Task<ServiceResult<Dictionary<string, int>>> GetAddressCompletionStatsAsync(int employeeId)
Task<List<EmployeeAddress>> GetAddressesByCityAsync(string city)
Task<List<EmployeeAddress>> GetAddressesByDistrictAsync(string district)
Task<List<EmployeeAddress>> GetAddressesByPostalCodeAsync(string postalCode)

// 輔助方法
string FormatFullAddress(EmployeeAddress address)
Task<bool> IsDuplicateAddressAsync(EmployeeAddress address)
ServiceResult ValidateAddress(EmployeeAddress address)
```

### 地址欄位
- **PostalCode** - 郵遞區號
- **City** - 城市
- **District** - 行政區
- **Address** - 詳細地址
- **IsPrimary** - 是否為主要地址

### 驗證規則
- 員工ID 必須有效
- 地址資訊不能全部為空
- 同一員工不能有完全相同的地址
- 每種地址類型只能有一個主要地址

---

## 使用範例

### 取得員工完整資訊（包含聯絡資料和地址）
```csharp
public async Task<Employee?> GetEmployeeWithDetailsAsync(int employeeId)
{
    var employee = await _employeeService.GetByIdAsync(employeeId);
    // employee.EmployeeContacts 和 employee.EmployeeAddresses 會自動載入
    return employee;
}
```

### 管理員工聯絡資料
```csharp
// 新增員工電子郵件
var contact = new EmployeeContact
{
    EmployeeId = employeeId,
    ContactTypeId = emailTypeId,
    ContactValue = "employee@company.com",
    IsPrimary = true
};
await _employeeContactService.CreateAsync(contact);

// 設定為主要聯絡方式
await _employeeContactService.SetAsPrimaryAsync(contact.Id);
```

### 管理員工地址
```csharp
// 新增員工地址
var address = new EmployeeAddress
{
    EmployeeId = employeeId,
    AddressTypeId = homeTypeId,
    PostalCode = "100",
    City = "台北市",
    District = "中正區",
    Address = "重慶南路一段122號",
    IsPrimary = true
};
await _employeeAddressService.CreateAsync(address);

// 取得格式化地址
var fullAddress = _employeeAddressService.FormatFullAddress(address);
// 結果: "100 台北市 中正區 重慶南路一段122號"
```

---

## 依賴注入設定

在 Program.cs 中註冊這些服務：

```csharp
// 員工相關服務
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeContactService, EmployeeContactService>();
builder.Services.AddScoped<IEmployeeAddressService, EmployeeAddressService>();

// 現有的員工認證相關服務
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
```

---

## 資料庫關聯

### Employee 實體
- **Role** - 多對一關聯（每個員工有一個角色）
- **EmployeeContacts** - 一對多關聯（一個員工可有多個聯絡方式）
- **EmployeeAddresses** - 一對多關聯（一個員工可有多個地址）

### EmployeeContact 實體
- **Employee** - 多對一關聯
- **ContactType** - 多對一關聯（電話、傳真、電子郵件等）

### EmployeeAddress 實體
- **Employee** - 多對一關聯
- **AddressType** - 多對一關聯（住家、公司、通訊地址等）

---

## 注意事項

1. **Email 欄位移除**：員工的電子郵件現在透過 EmployeeContact 管理
2. **主要聯絡方式/地址**：系統會自動確保每種類型只有一個主要項目
3. **軟刪除**：所有刪除操作都是軟刪除（設定 IsDeleted = true）
4. **稽核欄位**：自動設定 CreatedAt、UpdatedAt、CreatedBy、UpdatedBy
5. **導航屬性**：主要查詢方法會自動載入相關實體以提供完整資訊

---

*本文檔說明了 Employee 相關 Services 的架構和使用方式。這些服務遵循 README_Services.md 中定義的通用模式，提供一致的開發體驗。*
