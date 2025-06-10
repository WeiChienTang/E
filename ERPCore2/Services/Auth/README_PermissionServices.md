# Permission Services 職責說明

## 概述
Auth 資料夾中有兩個 Permission 相關的服務，各自負責不同的職責範圍。

## 服務職責分工

### 🔍 IPermissionService / PermissionService
**主要職責：權限檢查和快取管理**

#### 核心功能
- **權限檢查**：`HasPermissionAsync()` - 檢查員工是否具有特定權限
- **批次權限檢查**：`HasAllPermissionsAsync()` / `HasAnyPermissionAsync()`
- **員工權限查詢**：`GetEmployeePermissionsAsync()` / `GetEmployeePermissionCodesAsync()`
- **角色權限查詢**：`GetRolePermissionsAsync()`
- **快取管理**：`RefreshEmployeePermissionCacheAsync()` / `ClearAllPermissionCacheAsync()`
- **模組存取檢查**：`CanAccessModuleAsync()`

#### 特點
- **快取優化**：使用 IMemoryCache 提升權限檢查效能
- **運行時檢查**：專注於系統運行時的權限驗證
- **只讀操作**：主要進行查詢和檢查，不修改權限資料

### 🛠️ IPermissionManagementService / PermissionManagementService
**主要職責：權限資料管理**

#### 核心功能
- **權限 CRUD**：繼承 `GenericManagementService<Permission>` 提供完整的增刪改查
- **權限代碼管理**：`GetByCodeAsync()` / `IsPermissionCodeExistsAsync()`
- **模組權限管理**：`GetPermissionsByModuleAsync()` / `GetAllModulesAsync()`
- **批次權限操作**：`CreatePermissionsBatchAsync()`
- **系統初始化**：`InitializeDefaultPermissionsAsync()`

#### 特點
- **資料管理**：專注於權限資料的 CRUD 操作
- **驗證功能**：包含資料驗證和重複檢查
- **管理功能**：提供系統管理員使用的權限管理功能

## 使用場景

### 🔍 使用 PermissionService 的場景
```csharp
// 在組件中檢查權限
var hasPermission = await permissionService.HasPermissionAsync(employeeId, "Customer.View");

// 檢查員工是否可以存取某個模組
var canAccess = await permissionService.CanAccessModuleAsync(employeeId, "Customer");

// 取得員工的所有權限
var permissions = await permissionService.GetEmployeePermissionsAsync(employeeId);
```

### 🛠️ 使用 PermissionManagementService 的場景
```csharp
// 在管理介面中新增權限
var newPermission = new Permission { PermissionCode = "Product.View", PermissionName = "檢視產品" };
await permissionManagementService.CreateAsync(newPermission);

// 檢查權限代碼是否重複（編輯時排除自己）
var exists = await permissionManagementService.IsPermissionCodeExistsAsync("Customer.Edit", excludeId);

// 初始化系統預設權限
await permissionManagementService.InitializeDefaultPermissionsAsync();
```

## 重複方法說明

### GetModulePermissionsAsync vs GetPermissionsByModuleAsync
- **功能相同**：都是查詢指定模組的權限
- **建議**：使用 `PermissionManagementService.GetPermissionsByModuleAsync()`
- **原因**：ManagementService 是專門負責權限資料管理的服務

### PermissionExistsAsync vs IsPermissionCodeExistsAsync
- **PermissionExistsAsync**：簡單檢查權限是否存在
- **IsPermissionCodeExistsAsync**：支援排除特定ID的檢查（編輯時使用）
- **建議**：根據需求選擇適當的方法

## 依賴注入

兩個服務都已註冊在 `ServiceRegistration.cs` 中：

```csharp
// 權限檢查服務（含快取）
services.AddScoped<IPermissionService, PermissionService>();

// 權限管理服務（含 CRUD）
services.AddScoped<IPermissionManagementService, PermissionManagementService>();
```

## 建議的使用原則

1. **權限檢查**：使用 `IPermissionService`
2. **權限管理**：使用 `IPermissionManagementService`
3. **避免混用**：同一個功能只使用一個服務
4. **性能考量**：頻繁的權限檢查使用有快取的 `PermissionService`

---

*此文檔說明了兩個 Permission 服務的設計目的和使用場景，確保開發者能正確選擇適當的服務。*
