# CurrentUserHelper 使用指南

## 概述

`CurrentUserHelper` 是一個靜態輔助類別，用於在 Blazor 組件中取得當前登入使用者的資訊。這個類別封裝了從 `AuthenticationStateProvider` 取得使用者 Claims 的邏輯，提供一致且簡單的 API。

## 檔案位置

```
ERPCore2/
└── Helpers/
    ├── CurrentUserHelper.cs
    └── README_CurrentUserHelper.md (本文件)
```

## 主要功能

### 1. GetCurrentUserFullNameAsync()
取得使用者的完整姓名，優先使用 FirstName + LastName，如果沒有則回退到 Username。

### 2. GetCurrentUserNameAsync()
取得使用者的登入名稱 (Username)。

### 3. GetCurrentEmployeeCodeAsync()
取得使用者的員工代碼。

### 4. GetCurrentDepartmentAsync()
取得使用者所屬的部門名稱。

## Claims 資料來源

這些方法從 `AuthController.cs` 設定的 Claims 中取得資料：

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
    new Claim(ClaimTypes.Name, employee.Account),           // Account
    new Claim(ClaimTypes.GivenName, employee.FirstName ?? ""), // FirstName
    new Claim(ClaimTypes.Surname, employee.LastName ?? ""),    // LastName
    new Claim(ClaimTypes.Role, employee.Role?.RoleName ?? "User"),
    new Claim("EmployeeCode", employee.EmployeeCode),          // 員工代碼
    new Claim("Department", employee.Department?.Name ?? ""),   // 部門
    new Claim("Position", employee.EmployeePosition?.Name ?? "")
};
```

## 使用方式

### 基本設定

在需要使用的 Blazor 組件中添加以下設定：

```csharp
@using Microsoft.AspNetCore.Components.Authorization
@using ERPCore2.Helpers
@inject AuthenticationStateProvider AuthenticationStateProvider
```

### 使用範例

#### 1. 在採購訂單頁面自動設定採購人員

```csharp
private async Task SetCurrentUserAsPurchasePersonnel(PurchaseOrder? order = null)
{
    try
    {
        var targetOrder = order ?? purchaseOrder;
        var personnelName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
        targetOrder.PurchasePersonnel = personnelName;
    }
    catch (Exception ex)
    {
        // 錯誤處理
        var targetOrder = order ?? purchaseOrder;
        targetOrder.PurchasePersonnel = "系統使用者";
    }
}
```

#### 2. 在銷貨訂單頁面自動設定銷售人員

```csharp
private async Task InitializeDefaultValues()
{
    if (!Id.HasValue)
    {
        salesOrder.OrderDate = DateTime.Today;
        salesOrder.SalesPersonnel = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
        salesOrder.Department = await CurrentUserHelper.GetCurrentDepartmentAsync(AuthenticationStateProvider);
    }
}
```

#### 3. 在庫存異動頁面記錄操作者

```csharp
private async Task InitializeDefaultValues()
{
    if (!Id.HasValue)
    {
        inventoryTransaction.TransactionDate = DateTime.Today;
        inventoryTransaction.OperatorName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
        inventoryTransaction.OperatorCode = await CurrentUserHelper.GetCurrentEmployeeCodeAsync(AuthenticationStateProvider);
    }
}
```

## 標準化模式

### 1. 頁面初始化模式

```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        InitializeBreadcrumbs();
        await InitializeDefaultValues();
        InitializeBasicFormFields();
    }
    catch (Exception ex)
    {
        // 錯誤處理
    }
}

private async Task InitializeDefaultValues()
{
    if (!Id.HasValue) // 只在新增模式下設定
    {
        // 基本預設值
        entity.Date = DateTime.Today;
        entity.Status = DefaultStatus;
        
        // 設定當前使用者資訊
        entity.OperatorName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
        entity.Department = await CurrentUserHelper.GetCurrentDepartmentAsync(AuthenticationStateProvider);
    }
}
```

### 2. 表單欄位設定模式

```csharp
new()
{
    PropertyName = nameof(Entity.OperatorName),
    Label = "操作人員",
    FieldType = FormFieldType.Text,
    IsReadOnly = true,  // 通常設為唯讀
    ContainerCssClass = "col-md-6"
},
```

### 3. 資料載入模式（適用於通用組件）

```csharp
private async Task<Entity?> LoadEntityData()
{
    try
    {
        if (Id.HasValue)
        {
            // 編輯模式：載入現有資料
            var entity = await Service.GetByIdAsync(Id.Value);
            return entity;
        }
        else
        {
            // 新增模式：建立新實體並設定預設值
            var newEntity = new Entity
            {
                // 基本預設值
                Date = DateTime.Today,
                Status = EntityStatus.Active
            };
            
            // 設定當前使用者資訊
            await SetCurrentUserInfo(newEntity);
            
            return newEntity;
        }
    }
    catch (Exception ex)
    {
        // 錯誤處理
        return new Entity();
    }
}

private async Task SetCurrentUserInfo(Entity entity)
{
    entity.OperatorName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
    entity.OperatorCode = await CurrentUserHelper.GetCurrentEmployeeCodeAsync(AuthenticationStateProvider);
    entity.Department = await CurrentUserHelper.GetCurrentDepartmentAsync(AuthenticationStateProvider);
}
```

## 適用場景

### 必須使用的場景
- 採購訂單的採購人員欄位
- 銷貨訂單的銷售人員欄位
- 庫存異動的操作人員欄位
- 盤點作業的盤點人員欄位
- 任何需要記錄操作者的表單

### 建議使用的場景
- 系統日誌記錄
- 審核流程的處理人員
- 資料建立/修改的責任歸屬

## 錯誤處理

所有方法都包含 try-catch 錯誤處理：

```csharp
try
{
    // 正常邏輯
    var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
    // ... 處理邏輯
    return result;
}
catch
{
    // 發生錯誤時返回安全的預設值
    return "系統使用者";
}
```

## 返回值說明

| 方法 | 正常情況 | 未登入 | 錯誤情況 |
|------|----------|--------|----------|
| `GetCurrentUserFullNameAsync()` | "系統管理員" 或 "admin" | "未登入使用者" | "系統使用者" |
| `GetCurrentUserNameAsync()` | "admin" | "未知使用者" | "系統使用者" |
| `GetCurrentEmployeeCodeAsync()` | "ADMIN001" | "" | "" |
| `GetCurrentDepartmentAsync()` | "資訊部" | "" | "" |

## 注意事項

1. **時序問題**：確保在正確的生命週期方法中呼叫（如 `OnInitializedAsync`）
2. **編輯模式**：通常只在新增模式下自動設定使用者資訊，編輯模式保留原有值
3. **唯讀欄位**：使用者資訊欄位通常應設為 `IsReadOnly = true`
4. **錯誤處理**：始終包含適當的錯誤處理邏輯
5. **一致性**：在整個專案中保持一致的使用模式

## 未來擴展

如需添加更多使用者資訊，可以在 `CurrentUserHelper` 中添加新方法：

```csharp
/// <summary>
/// 取得當前使用者的職位
/// </summary>
public static async Task<string> GetCurrentPositionAsync(AuthenticationStateProvider authenticationStateProvider)
{
    try
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        return authState?.User?.FindFirst("Position")?.Value ?? "";
    }
    catch
    {
        return "";
    }
}
```

對應的 Claims 需要在 `AuthController.cs` 中設定：

```csharp
new Claim("Position", employee.EmployeePosition?.Name ?? "")
```
